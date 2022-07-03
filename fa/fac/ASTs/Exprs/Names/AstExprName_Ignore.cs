﻿using fac.ASTs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs.Names {
	public class AstExprName_Ignore: IAstExprName {
		public override IAstExpr TraversalCalcType (IAstType? _expect_type) {
			ExpectType = new AstType_Any { Token = Token };
			return AstExprTypeCast.Make (this, _expect_type);
		}

		public override IAstType GuessType () => new AstType_Any { Token = Token };

		public override string GenerateCSharp (int _indent) => "_";

		public override string GenerateCpp (int _indent) => "_";

		public override bool AllowAssign () => true;
	}
}
