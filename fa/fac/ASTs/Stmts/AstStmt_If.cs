﻿using fac.AntlrTools;
using fac.ASTs.Exprs;
using fac.ASTs.Exprs.Names;
using fac.ASTs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Stmts {
	public class AstStmt_If: IAstStmt {
		public IAstExpr Condition { get; set; }
		public List<IAstStmt> IfTrueCodes { get; set; }
		public List<IAstStmt> IfFalseCodes { get; set; } = null;



		public static List<IAstStmt> FromIfStmt (List<FaParser.ExprContext> _conditions, List<FaParser.StmtContext[]> _contents) {
			List<IAstStmt> _stmts = new List<IAstStmt> ();
			var _ifexpr = new AstStmt_If { Token = _conditions[0].Start };
			_ifexpr.Condition = FromContext (_conditions[0]);
			_ifexpr.IfTrueCodes = FromStmts (_contents[0]);
			if (_conditions.Count == 1) {
				if (_contents.Count > 1) {
					_ifexpr.IfFalseCodes = FromStmts (_contents[1]);
				} else {
					_ifexpr.IfFalseCodes = new List<IAstStmt> ();
				}
			} else {
				_ifexpr.IfFalseCodes = FromIfStmt (_conditions.Skip (1).ToList (), _contents.Skip (1).ToList ());
			}
			_stmts.Add (_ifexpr);
			return _stmts;
		}

		public static List<IAstStmt> FromContext (FaParser.IfStmtContext _ctx) {
			var _conditions = _ctx.expr ().ToList ();
			var _contents = (from p in _ctx.quotStmtPart () select p.stmt ()).ToList ();
			return FromIfStmt (_conditions, _contents);
		}

		public override void Traversal ((int _deep, int _group, int _loop, Func<IAstExpr, int, int, int, IAstExpr> _cb) _trav) {
			var _trav1 = (_deep: _trav._deep + 1, _group: Common.GetRandomInt (), _loop: _trav._loop, _cb: _trav._cb);
			Condition = Condition.TraversalWrap (_trav1);
			IfTrueCodes.TraversalWraps (_trav1);
			_trav1 = (_deep: _trav._deep + 1, _group: Common.GetRandomInt (), _loop: _trav._loop, _cb: _trav._cb);
			IfFalseCodes.TraversalWraps (_trav1);
		}

		public override IAstExpr TraversalCalcType (IAstType? _expect_type) {
			if (_expect_type != null)
				throw new Exception ("语句类型不可指定期望类型");
			bool _success = Condition.TraversalCalcTypeWrap (IAstType.FromName ("bool"), a => Condition = a);
			_success &= IfTrueCodes.TraversalCalcTypeWrap ();
			_success &= IfFalseCodes.TraversalCalcTypeWrap ();
			return _success ? this : null;
		}

		public override List<IAstStmt> ExpandStmt ((IAstExprName _var, AstStmt_Label _pos)? _cache_err) {
			List<IAstStmt> _pre_stmts = new List<IAstStmt> (), _true_stmts = new List<IAstStmt> ();
			IAstExpr _expr;
			if (Condition is AstExpr_Is _is_expr) {
				(_expr, _true_stmts) = _is_expr.ExpandExpr_If (_cache_err);
				Condition = _expr;
			} else {
				(_pre_stmts, _expr) = Condition.ExpandExpr (_cache_err);
				Condition = _expr;
			}
			_true_stmts.AddRange (IfTrueCodes.ExpandStmts (_cache_err));
			IfTrueCodes = _true_stmts;
			IfFalseCodes = IfFalseCodes?.ExpandStmts (_cache_err) ?? null;
			_pre_stmts.Add (this);
			return _pre_stmts;
		}

		public override string GenerateCSharp (int _indent) {
			var _sb = new StringBuilder ();
			_sb.AppendLine ($"{_indent.Indent ()}if ({Condition.GenerateCSharp (_indent)}) {{");
			_sb.AppendCSharpStmts (IfTrueCodes, _indent + 1);
			if (IfFalseCodes?.Any () ?? false) {
				_sb.AppendLine ($"{_indent.Indent ()}}} else {{");
				_sb.AppendCSharpStmts (IfFalseCodes, _indent + 1);
			}
			_sb.AppendLine ($"{_indent.Indent ()}}}");
			return _sb.ToString ();
		}

		public override string GenerateCpp (int _indent) {
			var _sb = new StringBuilder ();
			_sb.AppendLine ($"{_indent.Indent ()}if ({Condition.GenerateCpp (_indent)}) {{");
			_sb.AppendCppStmts (IfTrueCodes, _indent + 1);
			if (IfFalseCodes?.Any () ?? false) {
				_sb.AppendLine ($"{_indent.Indent ()}}} else {{");
				_sb.AppendCppStmts (IfFalseCodes, _indent + 1);
			}
			_sb.AppendLine ($"{_indent.Indent ()}}}");
			return _sb.ToString ();
		}
	}
}
