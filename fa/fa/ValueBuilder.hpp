#ifndef __VALUE_BUILDER_HPP__
#define __VALUE_BUILDER_HPP__



#include <format>
#include <functional>
#include <optional>
#include <string>
#include <tuple>

#include <llvm/ADT/APFloat.h>
#include <llvm/IR/Type.h>
#include <llvm/IR/Value.h>
#include <llvm/IR/Constant.h>
#include <llvm/IR/ConstantFolder.h>
#include <llvm/IR/ConstantRange.h>
#include <llvm/IR/Constants.h>

#include "FaVisitor.h"
#include "FaParser.h"
#include "TypeMap.hpp"
#include "StringProcessor.hpp"
#include "Log.hpp"



class ValueBuilder {
public:
	ValueBuilder (std::shared_ptr<TypeMap> _type_map, std::shared_ptr<llvm::LLVMContext> _ctx, std::shared_ptr<llvm::Module> _module)
		: m_type_map (_type_map), m_ctx (_ctx), m_module (_module) {
		m_builder = std::make_shared<llvm::IRBuilder<>> (*m_ctx);
	}

	std::optional<std::tuple<llvm::Value* , std::string>> Build (std::string _type, std::string _value, antlr4::Token* _t) {
		if (_type != "" && _type [0] == '$')
			_type = _type.substr (1);
		if (_type == "bool") {
			if (_value == "true") {
				return std::make_tuple<llvm::Value* , std::string> (llvm::ConstantInt::getTrue (*m_ctx), "bool");
			} else if (_value == "false") {
				return std::make_tuple<llvm::Value* , std::string> (llvm::ConstantInt::getFalse (*m_ctx), "bool");
			}
		} else if (_type == "cstr") {
			_value = _value.substr (1, _value.size () - 2);
			std::optional<std::string> _tmp_value = StringProcessor::TransformMean (_value, _t);
			if (_tmp_value.has_value ()) {
				return std::make_tuple<llvm::Value* , std::string> (m_builder->CreateGlobalStringPtr (_tmp_value.value (), "", 0, m_module.get ()), "cstr");
			}
		} else if (_type.find ("int") != std::string::npos) {
			if (_type == "int") {
				int64_t _i = std::stoll (_value);
				if (_i <= 2147483647L && _i >= -2147483648L) {
					_type = "int32";
				} else {
					_type = "int64";
				}
			}
			std::optional<llvm::Type*> _tp = m_type_map->GetType (_type, _t);
			if (!_tp.has_value ())
				return std::nullopt;
			std::optional<llvm::APInt> _int;
			if (_type [0] == 'u') {
				uint64_t _i = std::stoull (_value);
				if (_type == "uint8") {
					_int = llvm::APInt (8, _i, false);
				} else if (_type == "uint16") {
					_int = llvm::APInt (16, _i, false);
				} else if (_type == "uint32") {
					_int = llvm::APInt (32, _i, false);
				} else if (_type == "uint64") {
					_int = llvm::APInt (64, _i, false);
				}
			} else {
				int64_t _i = std::stoll (_value);
				if (_type == "int8") {
					_int = llvm::APInt (8, (uint64_t) _i, true);
				} else if (_type == "int16") {
					_int = llvm::APInt (16, (uint64_t) _i, true);
				} else if (_type == "int32") {
					_int = llvm::APInt (32, (uint64_t) _i, true);
				} else if (_type == "int64") {
					_int = llvm::APInt (64, (uint64_t) _i, true);
				}
			}
			if (_int.has_value ())
				return std::make_tuple ((llvm::Value* ) llvm::ConstantInt::get (_tp.value (), _int.value ()), _type);
		}

		LOG_ERROR (_t, std::format ("值 \"{}\" 无法转为 \"{}\" 类型。", _value, _type));
		return std::nullopt;
	}

	std::optional<AstValue> BuildArray (std::string _type, AstValue &_capacity, std::function<std::optional<AstValue> (size_t _index)> _cb) {
		std::vector<llvm::Type*> _v;
		_v.push_back (m_type_map->GetType ("int32").value ());
		_v.push_back (m_type_map->GetType ("int32").value ());
		_v.push_back (m_type_map->GetArrayType (_type, _capacity).value ());
		auto _stype = llvm::StructType::create (_v);
		llvm::AllocaInst* _inst = m_builder->CreateAlloca (_stype);
		//llvm::Value* _val_size = m_builder->CreateStructGEP (_inst, 0);
		//llvm::Value* _val_capacity = m_builder->CreateStructGEP (_inst, 1);
		AstValue _val_capacity { m_builder->CreateStructGEP (_inst, 1), _capacity.GetType () };
	}

private:
	std::shared_ptr<llvm::LLVMContext> m_ctx = nullptr;
	std::shared_ptr<TypeMap> m_type_map;
	std::shared_ptr<llvm::IRBuilder<>> m_builder;
	std::shared_ptr<llvm::Module> m_module;
};



#endif //__VALUE_BUILDER_HPP__
