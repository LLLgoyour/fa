﻿using fac.ASTs.Exprs;
using fac.ASTs.Exprs.Names;
using fac.ASTs.Types;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Stmts {
	public abstract class IAstStmt: IAstExpr {
		public static IAstStmt FromExpr (FaParser.ExprContext _ctx, bool _return) {
			if (_return) {
				return AstStmt_Return.MakeFromExpr (FromContext (_ctx));
			} else {
				return AstStmt_ExprWrap.MakeFromExpr (FromContext (_ctx));
			}
		}

		public static List<IAstStmt> FromStmt (FaParser.StmtContext _ctx) {
			var _stmts = new List<IAstStmt> ();
			if (_ctx == null) {
				return new List<IAstStmt> ();
			} else if (_ctx.ifStmt () != null) {
				return AstStmt_If.FromContext (_ctx.ifStmt ());
			} else if (_ctx.whileStmt () != null) {
				var _whilestmt = new AstStmt_While { Token = _ctx.Start, IsDoWhile = false };
				_whilestmt.Condition = FromContext (_ctx.whileStmt ().expr ());
				_whilestmt.Contents = FromStmts (_ctx.whileStmt ().stmt ());
				_stmts.Add (_whilestmt);
			} else if (_ctx.whileStmt2 () != null) {
				var _whilestmt = new AstStmt_While { Token = _ctx.Start, IsDoWhile = true };
				_whilestmt.Condition = FromContext (_ctx.whileStmt2 ().expr ());
				_whilestmt.Contents = FromStmts (_ctx.whileStmt2 ().stmt ());
				_stmts.Add (_whilestmt);
			} else if (_ctx.forStmt () != null) {
				var _forstmt = new AstStmt_For { Token = _ctx.Start };
				_forstmt.Initializes = FromStmt (_ctx.forStmt ().stmt ()[0]);
				_forstmt.Condition = FromContext (_ctx.forStmt ().expr ()[0]);
				_forstmt.Increment = FromExprs (_ctx.forStmt ().expr ()[1..]);
				_forstmt.BodyCodes = FromStmts (_ctx.forStmt ().stmt ()[1..]);
				_stmts.Add (_forstmt);
			} else if (_ctx.forStmt2 () != null) {
				var _forstmt2 = new AstStmt_For2 { Token = _ctx.Start };
				_forstmt2.Iterator = new AstStmt_DefVariable { Token = _ctx.forStmt2 ().type ().Start, DataType = IAstType.FromContext (_ctx.forStmt2 ().type ()), VarName = _ctx.forStmt2 ().id ().GetText (), Expr = null };
				_forstmt2.ListContainer = FromContext (_ctx.forStmt2 ().expr ());
				_forstmt2.BodyCodes = FromStmts (_ctx.forStmt2 ().stmt ());
				_stmts.Add (_forstmt2);
			} else if (_ctx.quotStmtPart () != null) {
				_stmts.Add (new AstStmt_HuaQuotWrap { Token = _ctx.Start, Stmts = FromStmts (_ctx.quotStmtPart ().stmt ()) });
			} else if (_ctx.switchStmt2 () != null) {
				var _t = new AstStmt_Switch { Token = _ctx.Start, Condition = null };
				var _switch_items = _ctx.switchStmt2 ().switchStmtPart2 ();
				_t.CaseCond = null;
				_t.CaseWhen = (from p in _switch_items select FromContext (p.expr ())).ToList ();
				_t.CaseCodes = (from p in _switch_items select FromStmt (p.stmt ()).ToSingleStmt ()).ToList ();
				if (_ctx.switchStmt2 ().switchStmtPart2Last () != null) {
					_t.CaseWhen.Add (new AstExprName_Ignore { Token = _ctx.switchStmt2 ().switchStmtPart2Last ().Underline ().Symbol });
					_t.CaseCodes.Add (FromStmt (_ctx.switchStmt2 ().switchStmtPart2Last ().stmt ()).ToSingleStmt ());
				}
				_stmts.Add (_t);
			} else if (_ctx.switchStmt () != null) {
				var _t = new AstStmt_Switch { Token = _ctx.Start, Condition = FromContext (_ctx.switchStmt ().expr ()) };
				var _switch_items = _ctx.switchStmt ().switchStmtPart ();
				_t.CaseCond = (from p in _switch_items select FromContext (p.expr ()[0])).ToList ();
				_t.CaseCond.PreprocessCaseCond ();
				_t.CaseWhen = (from p in _switch_items select p.expr ().Length > 1 ? FromContext (p.expr ()[1]) : null).ToList ();
				_t.CaseCodes = (from p in _switch_items select FromStmt (p.stmt ()).ToSingleStmt ()).ToList ();
				_stmts.Add (_t);
			} else if (_ctx.defVarStmt () != null) {
				return AstStmt_DefVariable.FromContext (_ctx.defVarStmt ());
			} else if (_ctx.defVarStmt2 () != null) {
				return AstStmt_DefVariable.FromContext (_ctx.defVarStmt2 ());
			} else if (_ctx.normalStmt () != null) {
				if (_ctx.normalStmt ().Continue () != null) {
					_stmts.Add (AstStmt_ExprWrap.MakeContinue (_ctx.Start));
				} else if (_ctx.normalStmt ().Break () != null) {
					_stmts.Add (AstStmt_ExprWrap.MakeBreak (_ctx.Start));
				} else {
					var _expr_raw = _ctx.normalStmt ().expr ();
					if (_ctx.normalStmt ().Return () != null) {
						_stmts.Add (AstStmt_Return.MakeFromExpr (FromContext (_expr_raw)));
					} else if (_expr_raw != null) {
						_stmts.Add (AstStmt_ExprWrap.MakeFromExpr (FromContext (_expr_raw)));
					}
				}
			} else {
				throw new UnimplException (_ctx.Start);
			}
			return _stmts;
		}

		public static List<IAstExpr> FromExprs (FaParser.ExprContext[] _ctxs) => (from p in _ctxs select FromContext (p)).ToList ();
		public static List<IAstStmt> FromStmts (FaParser.StmtContext[] _ctxs) => (from p in _ctxs select FromStmt (p)).CombileStmts ();
		public override IAstType GuessType () => throw new Exception ("不应执行此处代码");
		public override bool AllowAssign () => throw new Exception ("不应执行此处代码");
		public override (List<IAstStmt>, IAstExpr) ExpandExpr ((IAstExprName _var, AstStmt_Label _pos)? _cache_err) => throw new Exception ("不应执行此处代码");

		/// <summary>
		/// 分解语句
		/// </summary>
		/// <param name="_cache_err">用于缓存错误的变量</param>
		/// <returns></returns>
		public abstract List<IAstStmt> ExpandStmt ((IAstExprName _var, AstStmt_Label _pos)? _cache_err);
	}
}
