﻿using fac.ASTs.Exprs.Names;
using fac.ASTs.Stmts;
using fac.ASTs.Types;
using fac.ASTs.Types.Mappings;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs {
	public class AstExpr_NewObject: IAstExpr {
		public IAstType DataType { get; set; }
		public List<(string _name, IAstExpr _value)> InitialValues { get; set; } = null; // 结构体初值，当有值时ConstructorArguments为空
		public List<IAstExpr> ConstructorArguments { get; set; } = null;                 // 构造函数参数，当有值时InitialValues为空



		public static AstExpr_NewObject FromContext (FaParser.NewExpr1Context _ctx) {
			var _expr = new AstExpr_NewObject { Token = _ctx.Start };
			_expr.DataType = _ctx.typeSingle () != null ? IAstType.FromContext (_ctx.typeSingle ()) as AstType_Class : null;
			_expr.InitialValues = (from p in _ctx.newExprItem () select (_name: p.id ().GetText (), _value: FromContext (p.middleExpr ()))).ToList ();
			return _expr;
		}

		public static AstExpr_NewObject FromContext (FaParser.NewExpr2Context _ctx) {
			var _expr = new AstExpr_NewObject { Token = _ctx.Start };
			_expr.DataType = IAstType.FromContext (_ctx.typeSingle ()) as AstType_Class;
			_expr.ConstructorArguments = (from p in _ctx.expr () select FromContext (p)).ToList ();
			return _expr;
		}

		public override void Traversal ((int _deep, int _group, int _loop, Func<IAstExpr, int, int, int, IAstExpr> _cb) _trav) {
			if (InitialValues != null) {
				for (int i = 0; i < InitialValues.Count; ++i)
					InitialValues[i] = (_name: InitialValues[i]._name, _value: InitialValues[i]._value.TraversalWrap (_trav));
			}
			if (ConstructorArguments != null)
				ConstructorArguments.TraversalWraps (_trav);
		}

		public override IAstExpr TraversalCalcType (IAstType _expect_type) {
			if (DataType == null) {
				if (_expect_type == null)
					throw new CodeException (Token, "无法猜测对象类型");
				DataType = _expect_type;
			}
			if (InitialValues != null) {
				bool _success = true;
				var _default_init_vals = new List<(string _name, IAstExpr _value)> ();
				if (DataType is AstType_Class _cls_type) {
					foreach (var _vars in _cls_type.Class.ClassVars) {
						int i = 0;
						for (; i < InitialValues.Count; ++i) {
							if (InitialValues[i]._name != _vars.Name)
								continue;
							_success &= InitialValues[i]._value.TraversalCalcTypeWrap (_vars.DataType, a => InitialValues[i] = (_name: _vars.Name, _value: a));
							break;
						}
						if (i == InitialValues.Count) {
							if (_vars.DefaultValue == null)
								throw new CodeException (Token, $"未指定成员变量 {_vars.Name} 的初始值");
							_success &= _vars.DefaultValue.TraversalCalcTypeWrap (_vars.DataType, a => _default_init_vals.Add ((_name: _vars.Name, _value: a)));
						}
					}
				}
				if (!_success)
					return null;
				InitialValues.AddRange (_default_init_vals);
			} else if (ConstructorArguments != null) {
				// TODO 检查构造函数参数
				throw new UnimplException (Token);
			} else {
				// TODO 检查构造函数参数
				throw new UnimplException (Token);
			}
			ExpectType = DataType;
			return AstExprTypeCast.Make (this, _expect_type);
		}

		public override IAstType GuessType () => DataType;

		public override (List<IAstStmt>, IAstExpr) ExpandExpr ((IAstExprName _var, AstStmt_Label _pos)? _cache_err) {
			var _stmts = new List<IAstStmt> ();
			if (InitialValues != null) {
				for (int i = 0; i < InitialValues.Count; ++i) {
					var (_stmts1, _val1) = InitialValues[i]._value.ExpandExpr (_cache_err);
					_stmts.AddRange (_stmts1);
					InitialValues[i] = (InitialValues[i]._name, _val1);
				}
			}
			if (ConstructorArguments != null) {
				for (int i = 0; i < ConstructorArguments.Count; ++i) {
					var (_stmts1, _val1) = ConstructorArguments[i].ExpandExpr (_cache_err);
					_stmts.AddRange (_stmts1);
					ConstructorArguments[i] = _val1;
				}
			}
			return (_stmts, this);
		}

		public override string GenerateCSharp (int _indent) {
			StringBuilder _sb = new StringBuilder ();
			if (DataType is AstType_Class _cls_type) {
				_sb.Append ($"new {_cls_type.Class.CSharpFullName}");
			} else {
				_sb.Append ($"new {DataType.GenerateCSharp (_indent)}");
			}

			if (InitialValues != null) {
				_sb.Append ($" {{ ");
				foreach (var _init in InitialValues) {
					var _b = _init._value.GenerateCSharp (_indent);
					_sb.Append ($"{_init._name} = {_b}, ");
				}
				if (InitialValues.Any ())
					_sb.Remove (_sb.Length - 2, 2);
				_sb.Append ($" }}");
			} else {
				_sb.Append ($" (");
				foreach (var _arg in ConstructorArguments) {
					var _b = _arg.GenerateCSharp (_indent);
					_sb.Append ($"{_b}, ");
				}
				if (ConstructorArguments.Any ())
					_sb.Remove (_sb.Length - 2, 2);
				_sb.Append ($")");
			}
			return _sb.ToString ();
		}

		public override bool AllowAssign () => false;
	}
}
