﻿using fac.ASTs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Exprs.Names {
	class AstExprName_LambdaArgument: IAstExprName {
		public AstExpr_Lambda Func { init; get; }
		public int ArgumentIndex { init; get; }



		public override IAstExpr TraversalCalcType (IAstType? _expect_type) {
			ExpectType = Func.Arguments[ArgumentIndex]._type;
			return AstExprTypeCast.Make (this, _expect_type);
		}

		public override IAstType GuessType () => Func.Arguments[ArgumentIndex]._type;

		public override string GenerateCSharp (int _indent) => Func.Arguments[ArgumentIndex]._name;

		public override string GenerateCpp (int _indent) => Func.Arguments [ArgumentIndex]._name;

		public override bool AllowAssign () => Func.Arguments[ArgumentIndex]._ext == ArgumentTypeExt.Mut;
	}
}
