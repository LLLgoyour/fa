﻿using Antlr4.Runtime;
using fac.AntlrTools;
using fac.ASTs.Exprs.Names;
using fac.ASTs.Stmts;
using fac.ASTs.Types;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs {
	public abstract class IAstExpr: IAst {
		public IAstType ExpectType { get; set; } = null;

		public abstract void Traversal ((int _deep, int _group, int _loop, Func<IAstExpr, int, int, int, IAstExpr> _cb) _trav);
		public IAstExpr TraversalWrap ((int _deep, int _group, int _loop, Func<IAstExpr, int, int, int, IAstExpr> _cb) _trav) {
			var _obj = this;
			if (Info.TraversalFirst)
				_obj = ExprTraversals.Traversal (_obj, _trav._deep, _trav._group, _trav._loop);
			Func<IAstExpr, int, int, int, IAstExpr> _cb = (_expr, _deep1, _group1, _loop1) => {
				if (_expr != null)
					_expr = _expr.TraversalWrap ((_deep: _deep1, _group: _group1, _loop: _loop1, _trav._cb));
				return _expr;
			};
			_obj.Traversal ((_deep: _trav._deep, _group: _trav._group, _loop: _trav._loop, _cb: _cb));
			if (Info.TraversalLast)
				_obj = ExprTraversals.Traversal (_obj, _trav._deep, _trav._group, _trav._loop);
			return _obj;
		}

		public abstract IAstExpr TraversalCalcType (IAstType _expect_type);
		public bool TraversalCalcTypeWrap (IAstType _expect_type, Action<IAstExpr> _cb) {
			var _expr1 = TraversalCalcType (_expect_type);
			bool _ret = _expr1 != null;
			if (_ret)
				_cb (_expr1);
			return _ret;
		}
		public abstract IAstType GuessType ();
		public abstract bool AllowAssign ();

		/// <summary>
		/// 赋值方式分解表达式
		/// </summary>
		/// <param name="_rval">待赋的值</param>
		/// <param name="_cache_err">用于缓存错误的变量，null代表不处理空判断，(null, null)代表当前方法返回类型不可空</param>
		/// <returns>执行此表达式前需执行的前置语句、简化后的表达式</returns>
		public virtual (List<IAstStmt>, IAstExpr) ExpandExprAssign (IAstExpr _rval, (IAstExprName _var, AstStmt_Label _pos)? _cache_err) => throw new NotImplementedException ();

		/// <summary>
		/// 分解表达式
		/// </summary>
		/// <param name="_cache_err">用于缓存错误的变量，null代表不处理空判断，(null, null)代表当前方法返回类型不可空</param>
		/// <returns>执行此表达式前需执行的前置语句、简化后的表达式</returns>
		public abstract (List<IAstStmt>, IAstExpr) ExpandExpr ((IAstExprName _var, AstStmt_Label _pos)? _cache_err);



		public bool IsSimpleExpr {
			get {
				if (this is IAstExprName _nameexpr) {
					return _nameexpr is not AstExprName_BuildIn;
				}
				return this is AstExpr_BaseId || this is AstExpr_BaseValue;
			}
		}

		public static IAstExpr FromContext (FaParser.ExprContext _ctx) {
			if (_ctx == null)
				return null;
			var _expr_ctxs = _ctx.middleExpr ();
			var _op2_ctxs = _ctx.allAssign ();
			if (_expr_ctxs.Length == 0)
				throw new UnimplException (_ctx);
			//
			var _expr = FromContext (_expr_ctxs[_expr_ctxs.Length - 1]);
			for (int i = _expr_ctxs.Length - 2; i >= 0; --i) {
				var _expr2 = new AstExpr_Op2 { Token = _ctx.Start };
				_expr2.Value1 = FromContext (_expr_ctxs[i]);
				_expr2.Value2 = _expr;
				_expr2.Operator = _op2_ctxs[i].GetText ();
				_expr = _expr2;
			}
			return _expr;
		}

		public static IAstExpr FromContext (FaParser.MiddleExprContext _ctx) {
			var _exprs = (from p in _ctx.strongExpr () select FromContext (p)).ToList ();

			// 处理优先级
			var _op2s = (from p in _ctx.allOp2 () select p.GetText ()).ToList ();
			var _op2l = (from p in _op2s select Operators.Op2Priority[p]).ToList ();
			for (int i = 0; i < Operators.MaxPriorityNum && _exprs.Count > 1; ++i) {
				// 比较运算符特殊处理
				if (i == Operators.ComparePriorityNum) {
					// 检查是否有连续比较运算符，如有，那么合并为单独的结构
					int _start = -1;
					Action<int> _combine_func = (int _count) => {
						var _expr = new AstExpr_Op2s { Token = _ctx.Start };
						_expr.Values = _exprs.Skip (_start).Take (_count + 1).ToList ();
						_expr.Operators = _op2s.Skip (_start).Take (_count).ToList ();
						_exprs.RemoveRange (_start, _count);
						_op2s.RemoveRange (_start, _count);
						_op2l.RemoveRange (_start, _count);
						_exprs [_start] = _expr;
						_start = -1;
					};
					int _op_num;
					for (int j = 0; j < _op2l.Count; ++j) {
						if (_op2l[j] != Operators.ComparePriorityNum) {
							_op_num = _start == -1 ? 0 : (j - _start);
							if (_op_num > 1) {
								_combine_func (_op_num);
								j -= _op_num;
							}
							continue;
						} else {
							_start = _start != -1 ? _start : j;
						}
					}
					_op_num = _start == -1 ? 0 : (_op2l.Count - _start);
					if (_op_num > 1) {
						_combine_func (_op_num);
					}
				}

				// 归并进AST
				for (int j = 0; j < _op2l.Count; ++j) {
					if (_op2l[j] != i)
						continue;

					// 合并
					var _expr = new AstExpr_Op2 { Token = _ctx.Start };
					_expr.Value1 = _exprs[j];
					_expr.Value2 = _exprs[j + 1];
					_expr.Operator = _op2s[j];
					_exprs[j] = _expr;
					_exprs.RemoveAt (j + 1);
					_op2s.RemoveAt (j);
					_op2l.RemoveAt (j);
					--j;
				}
			}
			return _exprs[0];
		}

		public static IAstExpr FromContext (FaParser.StrongExprContext _ctx) {
			var _expr = FromContext (_ctx.strongExprBase ());
			var _prefix_ctxs = _ctx.strongExprPrefix ();
			var _suffix_ctxs = _ctx.strongExprSuffix ();
			foreach (var _suffix_ctx in _suffix_ctxs) {
				if (_suffix_ctx.Is () != null) {
					_expr = AstExpr_Is_Temp.FromContext2 (_suffix_ctx.Is ().Symbol, _expr, _suffix_ctx.ids ().GetText (), _suffix_ctx.id ()?.GetText () ?? "");
				} else if (_suffix_ctx.AddAddOp () != null || _suffix_ctx.SubSubOp () != null || _suffix_ctx.id () != null) {
					var _tmp_expr = new AstExpr_Op1 { Token = _ctx.Start };
					_tmp_expr.Value = _expr;
					_tmp_expr.Operator = _suffix_ctx.GetText ();
					_tmp_expr.IsPrefix = false;
					_expr = _tmp_expr;
				} else if (_suffix_ctx.quotYuanL () != null) {
					var _tmp_expr = new AstExpr_OpN { Token = _ctx.Start };
					_tmp_expr.Value = _expr;
					_tmp_expr.Arguments = (from p in _suffix_ctx.expr () select FromContext (p)).ToList ();
					_expr = _tmp_expr;
				} else if (_suffix_ctx.quotFangL () != null) {
					//var _arr = _expr;
					var _args = (from p in _suffix_ctx.exprOpt () select p.expr () != null ? FromContext (p.expr ()) : null).ToList ();
					if (_args.Count != 1)
						throw new CodeException (_args.Count > 0 ? _args[0].Token : _expr.Token, "数组随机访问下标只能传一个整数");
					return AstExpr_ArrayAPI_Temp.Array_AccessItem (_expr, _args[0], true);
				} else {
					throw new UnimplException (_suffix_ctx);
				}
			}
			foreach (var _prefix_ctx in _prefix_ctxs.Reverse ()) {
				var _tmp_expr = new AstExpr_Op1 { Token = _ctx.Start };
				_tmp_expr.Value = _expr;
				_tmp_expr.Operator = _prefix_ctx.GetText ();
				_tmp_expr.IsPrefix = true;
				_expr = _tmp_expr;
			}
			return _expr;
		}

		public static IAstExpr FromContext (FaParser.StrongExprBaseContext _ctx) {
			if (_ctx.id () != null) {
				string _id = $"{(_ctx.ColonColon () != null ? "::" : "")}{_ctx.id ().GetText ()}";
				if (_id != "null") {
					return new AstExpr_BaseId { Token = _ctx.Start, Id = _id };
				} else {
					return AstExpr_OptError.MakeFromNull (_ctx.Start);
				}
			} else if (_ctx.literal () != null) {
				return FromContext (_ctx.literal ());
			} else if (_ctx.ifExpr () != null) {
				var _exprs = (from p in _ctx.ifExpr ().expr () select FromContext (p)).ToList ();
				var _branches = (from p in _ctx.ifExpr ().quotStmtExpr () select (IAstStmt.FromStmts (p.stmt ()), FromContext (p.expr ()))).ToList ();
				while (_exprs.Count > 0) {
					var _expr = new AstExpr_If { Token = _ctx.Start };
					_expr.Condition = _exprs[_exprs.Count - 1];
					(_expr.IfTrueCodes, _expr.IfTrue) = _branches[_branches.Count - 2];
					(_expr.IfFalseCodes, _expr.IfFalse) = _branches[_branches.Count - 1];
					_branches[_branches.Count - 2] = (new List<IAstStmt> (), _expr);
					_exprs.RemoveAt (_exprs.Count - 1);
					_branches.RemoveAt (_branches.Count - 1);
				}
				if (_branches[0].Item1.Count > 0)
					throw new Exception ("校验错误，此处不应该出现stmt代码");
				return _branches[0].Item2;
			} else if (_ctx.quotExpr () != null) {
				return FromContext (_ctx.quotExpr ().expr ());
			} else if (_ctx.newExpr1 () != null) {
				return AstExpr_NewObject.FromContext (_ctx.newExpr1 ());
			} else if (_ctx.newExpr2 () != null) {
				return AstExpr_NewObject.FromContext (_ctx.newExpr2 ());
				//} else if (_ctx.newArray () != null) {
				//	var _expr = new AstExpr_Array { Token = _ctx.Start };
				//	if (_ctx.newArray ().ids () != null)
				//		_expr.ItemDataType = IAstType.FromName (_ctx.newArray ().ids ().GetText ());
				//	_expr.InitValues = new List<IAstExpr> ();
				//	_expr.InitCount = FromContext (_ctx.newArray ().middleExpr ());
				//	return _expr;
			} else if (_ctx.arrayExpr1 () != null) {
				throw new UnimplException (_ctx);
			} else if (_ctx.arrayExpr2 () != null) {
				var _expr = new AstExpr_Array { Token = _ctx.Start };
				_expr.InitValues = (from p in _ctx.arrayExpr2 ().expr () select FromContext (p)).ToList ();
				_expr.InitCount = FromValue ("int", $"{_expr.InitValues.Count}");
				return _expr;
			} else if (_ctx.switchExpr2 () != null) {
				return AstExpr_Switch.FromContext (_ctx.switchExpr2 ());
			} else if (_ctx.switchExpr () != null) {
				return AstExpr_Switch.FromContext (_ctx.switchExpr ());
			} else if (_ctx.lambdaExpr () != null) {
				return new AstExpr_Lambda { Token = _ctx.Start, LambdaExprCtx = _ctx.lambdaExpr () };
			//} else if (_ctx.idExt () != null) {
			//	return AstExprName_ClassEnum_New._FindFromNameUncheckAttach (_ctx.Start, _ctx.idExt ().GetText ());
			} else {
				throw new UnimplException (_ctx);
			}
		}

		public static IAstExpr FromContext (FaParser.LiteralContext _ctx) {
			string _type, _value = _ctx.GetText ();
			if (_ctx.BoolLiteral () != null) {
				_type = "bool";
			} else if (_ctx.intNum () != null) {
				_type = "int";
			} else if (_ctx.floatNum () != null) {
				_type = "float";
			} else if (_ctx.String1Literal () != null) {
				//_type = "string";
				return AstExpr_BaseValue.FromCodeString (_ctx.Start, _value);
			} else {
				throw new UnimplException (_ctx);
			}
			return new AstExpr_BaseValue { Token = _ctx.Start, DataType = IAstType.FromName (_type), Value = _value };
		}

		public static IAstExpr FromValue (string _data_type, string _value) => FromValue (IAstType.FromName (_data_type), _value);
		public static IAstExpr FromValue (IAstType _data_type, string _value) => new AstExpr_BaseValue { Token = null, DataType = _data_type, Value = _value, ExpectType = _data_type };

		//// 生成一个可选类型中存储具体错误的对象
		//public static AstExprName_ClassEnum_New OptionalFromError (IToken _token, IAstType _type, fa_Error _err) {
		//	if (_type is AstType_Class _cls_type && _cls_type.Class.FullName == "fa.Error")
		//		return FromError (_token, _err);
		//	var _err_expr = AstExprName_ClassEnum_New.FindFromName (_token, AstType_OptionalWrap.ErrorClass, $"{_err}");
		//	var _expr = AstExprName_ClassEnum_New.FindFromName (_token, _type.AstClass, "Err", _err_expr);
		//	_expr.TraversalCalcTypeWrap (null, a => _expr = a as AstExprName_ClassEnum_New);
		//	return _expr;
		//}

		//// 生成一个错误类型的具体错误的对象
		//public static AstExprName_ClassEnum_New FromError (IToken _token, fa_Error _err) {
		//	var _expr = AstExprName_ClassEnum_New.FindFromName (_token, AstType_OptionalWrap.ErrorClass, $"{_err}");
		//	_expr.TraversalCalcTypeWrap (null, a => _expr = a as AstExprName_ClassEnum_New);
		//	return _expr;
		//}

		//// 判断一个可选类型是否有值，生成 bool 类型对象
		//public IAstExpr OptionalHasValue () {
		//	//var _expr = AstExpr_Is.FromContext2 (Token, this, "Val");
		//	//_expr.TraversalCalcTypeWrap (null, a => _expr = a as AstExpr_Is);
		//	//return _expr;
		//	return AstExpr_OptHasValue.Make (this);
		//}

		//// 将一个非可选类型值转为可选类型值
		//public IAstExpr OptionalFromValue () {
		//	if (ExpectType.IsOptional)
		//		return this;
		//	var _class = AstType_OptionalWrap.GetInstClass (ExpectType);
		//	var _expr1 = AstExprName_ClassEnum_New.FindFromName (Token, _class, "Val", this);
		//	_expr1.TraversalCalcTypeWrap (null, a => _expr1 = a as AstExprName_ClassEnum_New);
		//	return _expr1;
		//}

		//// 生成包含 Ok 值的 void? 类型对象
		//public static IAstExpr OptionalFromOk () {
		//	var _expr = AstExprName_ClassEnum_New.FindFromName (null, AstType_OptionalWrap.VoidClass, "Ok");
		//	_expr.TraversalCalcTypeWrap (null, a => _expr = a as AstExprName_ClassEnum_New);
		//	return _expr;
		//}

		//// 访问可选类型对象的值
		//public IAstExpr AccessValue () => AstExprName_ClassEnum_Access.FromAccess (this, "Val");

		//// 访问可选类型对象的错误信息
		//public IAstExpr AccessError () => AstExprName_ClassEnum_Access.FromAccess (this, "Err");
	}
}
