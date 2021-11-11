﻿using fac.ASTs.Exprs;
using fac.ASTs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Stmts {
	class AstStmt_DefVariable: IAstStmt {
		public IAstType DataType { get; set; }
		public string VarName { get; set; }
		public IAstExpr Expr { get; set; }



		public override void Traversal (int _deep, int _group, Func<IAstExpr, int, int, IAstExpr> _cb) {
			Expr = _cb (Expr, _deep, _group);
		}

		public override IAstExpr TraversalCalcType (string _expect_type) {
			if (_expect_type != "")
				throw new Exception ("语句类型不可指定期望类型");
			Expr = Expr.TraversalCalcType (DataType.TypeStr);
			return this;
		}

		public override (string, string) GenerateCSharp (int _indent) {
			var (_a, _b) = Expr.GenerateCSharp (_indent);
			return ("", $"{_a}{_indent.Indent ()}{DataType.GenerateCSharp (_indent)} {VarName} = {_b};\n");
		}
	}
}