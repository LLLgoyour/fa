﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Types {
	public class AstType_Any: IAstType {
		public override string ToString () => "any";
		public override string GenerateCSharp (int _indent) => "object";
		public override string GenerateCpp (int _indent) => "std::any";
	}
}
