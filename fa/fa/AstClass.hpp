#ifndef __AST_CLASS_HPP__
#define __AST_CLASS_HPP__



#include <format>
#include <functional>
#include <map>
#include <memory>
#include <optional>
#include <set>
#include <string>
#include <tuple>
#include <vector>

//#include "AstValue.hpp"

#include <llvm/IR/DerivedTypes.h>
#include <llvm/IR/Type.h>

#include <FaParser.h>



enum class PublicLevel { Unknown, Public, Internal, Protected, Private };



// ������� getter setter ����
class AstClassVarFunc {
public:
	PublicLevel						m_pv;					// ��������
	std::string						m_name;					// getter setter ����
	FaParser::ClassFuncBodyContext*	m_func;					// ������

	AstClassVarFunc (PublicLevel _pv, std::string _name, FaParser::ClassFuncBodyContext* _func)
		: m_pv (_pv), m_name (_name), m_func (_func) {}
};



// �����
class AstClassVar {
public:
	antlr4::Token*					m_t = nullptr;			//
	PublicLevel						m_pv;					// ��������
	bool							m_is_static;			// �Ƿ�̬
	std::string						m_type;					// ��������
	std::string						m_name;					// ��������
	FaParser::ExprContext*			m_init_value = nullptr;	// ��ʼֵ
	std::vector<AstClassVarFunc>	m_var_funcs;			// ���� getter setter ����

	AstClassVar (antlr4::Token* _t, PublicLevel _pv, bool _is_static, std::string _type, std::string _name)
		: m_t (_t), m_pv (_pv), m_is_static (_is_static), m_type (_type), m_name (_name) {}

	void SetInitValue (FaParser::ExprContext* _init_value) { m_init_value = _init_value; }

	std::tuple<bool, std::string> AddVarFunc (PublicLevel _pv, std::string _name, FaParser::ClassFuncBodyContext* _func) {
		static std::set<std::string> s_valid_names { "get", "set" };
		if (!s_valid_names.contains (_name))
			return { false, std::format ("�������֧�� {} ����", _name) };

		for (auto& _vfunc : m_var_funcs) {
			if (_vfunc.m_name == _name)
				return { false, std::format ("{} �����Ѵ���", _name) };
		}
		m_var_funcs.push_back (AstClassVarFunc { _pv, _name, _func });
		return { true, "" };
	}
};



// �෽��
class AstClassFunc {
public:
	PublicLevel								m_pv;						// ��������
	bool									m_is_static;				// �Ƿ�̬
	std::string								m_name;						// ��������
	std::string								m_name_abi;					// �ӿ�ʵ�ʷ�������
	std::string								m_ret_type;					// ��������
	antlr4::Token*							m_ret_type_t = nullptr;		//
	std::vector<std::string>				m_arg_types;				// ���������б�
	std::vector<antlr4::Token*>				m_arg_type_ts;				// ���������б�
	std::vector<std::string>				m_arg_names;				// ���������б�
	FaParser::ClassFuncBodyContext*			m_func = nullptr;			// ������

	AstClassFunc (PublicLevel _pv, bool _is_static, std::string _name)
		: m_pv (_pv), m_is_static (_is_static), m_name (_name) {}

	void SetReturnType (FaParser::TypeContext* _ret_type_raw) {
		m_ret_type = _ret_type_raw->getText ();
		m_ret_type_t = _ret_type_raw->start;
	}

	void SetArgumentTypeName (FaParser::TypeContext* _arg_type_raw, std::string _name) {
		SetArgumentTypeName (_arg_type_raw->getText (), _arg_type_raw->start, _name);
	}
	void SetArgumentTypeName (std::string _arg_type, antlr4::Token* _arg_type_t, std::string _name) {
		m_arg_types.push_back (_arg_type);
		m_arg_type_ts.push_back (_arg_type_t);
		m_arg_names.push_back (_name);
	}

	void SetFuncBody (FaParser::ClassFuncBodyContext* _func) { m_func = _func; }
};



class AstClass {
public:
	PublicLevel									m_level;
	std::string									m_name;
	std::vector<std::string>					m_parents;
	std::vector<std::shared_ptr<AstClassVar>>	m_vars;
	std::vector<std::shared_ptr<AstClassFunc>>	m_funcs;
	llvm::StructType*							m_type = nullptr;

	AstClass (PublicLevel _level, std::string _name): m_level (_level), m_name (_name) {}
	void AddParents (std::vector<std::string> &_parents) {
		m_parents.assign (_parents.cbegin (), _parents.cend ());
	}

	std::shared_ptr<AstClassVar> AddVar (antlr4::Token* _t, PublicLevel _pv, bool _is_static, std::string _type, std::string _name) {
		auto _var = std::make_shared<AstClassVar> (_t, _pv, _is_static, _type, _name);
		m_vars.push_back (_var);
		return _var;
	}

	std::shared_ptr<AstClassFunc> AddFunc (PublicLevel _pv, bool _is_static, std::string _name) {
		auto _func = std::make_shared<AstClassFunc> (_pv, _is_static, _name);
		m_funcs.push_back (_func);
		return _func;
	}

	std::optional<std::shared_ptr<AstClassVar>> GetVar (std::string _name) {
		for (auto _var : m_vars) {
			if (_var->m_name == _name)
				return _var;
		}
		return std::nullopt;
	}
	std::optional<size_t> GetVarIndex (std::string _name) {
		for (size_t i = 0; i < m_vars.size (); ++i) {
			if (m_vars [i]->m_name == _name)
				return i;
		}
		return std::nullopt;
	}

	std::optional<llvm::Type*> GetType (std::function<std::optional<llvm::Type*> (std::string, antlr4::Token*)> _cb) {
		if (!m_type) {
			std::vector<llvm::Type*> _v;
			for (auto _var : m_vars) {
				auto _otype = _cb (_var->m_type, _var->m_t);
				if (!_otype.has_value ())
					return std::nullopt;
				_v.push_back (_otype.value ());
			}
			m_type = llvm::StructType::create (_v);
		}
		return (llvm::Type*) m_type;
	}
};



class AstClasses {
public:
	std::optional<std::shared_ptr<AstClass>> GetClass (std::string _name, std::string _namesapce) {
		if (m_classes.contains (_name))
			return m_classes [_name];
		size_t _p = _namesapce.find ('.');
		std::string _tmp;
		while (_p != std::string::npos) {
			_tmp = std::format ("{}.{}", _namesapce.substr (0, _p), _name);
			if (m_classes.contains (_tmp))
				return m_classes [_tmp];
			_p = _namesapce.find ('.', _p + 1);
		};
		if (_namesapce != "") {
			_tmp = std::format ("{}.{}", _namesapce, _name);
			if (m_classes.contains (_tmp))
				return m_classes [_tmp];
		}
		return std::nullopt;
	}

	std::shared_ptr<AstClass> CreateNewClass (PublicLevel _level, std::string _name) {
		auto _cls = std::make_shared<AstClass> (_level, _name);
		m_classes [_name] = _cls;
		return _cls;
	}

	bool EnumClasses (std::function<bool (std::shared_ptr<AstClass>)> _cb) {
		for (auto &[_key, _val] : m_classes) {
			if (!_cb (_val))
				return false;
		}
		return true;
	}

private:
	std::map<std::string, std::shared_ptr<AstClass>> m_classes;
};



#endif //__AST_CLASS_HPP__