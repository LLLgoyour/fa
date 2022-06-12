﻿using Antlr4.Runtime;
using fac.ASTs.Exprs;
using fac.ASTs.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Structs {
	public class AstClassVar: IAst {
		public PublicLevel Level { init; get; }
		public bool Static { init; get; }
		public IAstType DataType { get; set; }
		public string Name { init; get; }
		public ParserRuleContext DefaultValueRaw { init; get; }
		public IAstExpr DefaultValue { get; set; } = null;



		public AstClassVar () { }
		public AstClassVar (FaParser.ClassItem2Context _ctx) {
			if (_ctx.classItemFuncExt2 () != null)
				throw new NotImplementedException ();
			Token = _ctx.Start;
			Level = Common.ParseEnum<PublicLevel> (_ctx.publicLevel ()?.GetText ()) ?? PublicLevel.Public;
			Static = _ctx.Static () != null;
			DataType = new AstType_TempType (_ctx.type ());
			Name = _ctx.classItemName ().GetText ();
			DefaultValueRaw = _ctx.middleExpr ();
		}
		public AstClassVar (FaParser.ClassItemVarContext _ctx) {
			Token = _ctx.Start;
			Level = Common.ParseEnum<PublicLevel> (_ctx.publicLevel ()?.GetText ()) ?? PublicLevel.Public;
			Static = _ctx.Static () != null;
			DataType = new AstType_TempType (_ctx.type ());
			Name = _ctx.classItemName ().GetText ();
			DefaultValueRaw = _ctx.middleExpr ();
		}

		public void ProcessType () {
			if (DataType is AstType_TempType _ttype)
				DataType = _ttype.GetRealType ();
		}

		public void ToAST () {
			if (DefaultValueRaw != null) {
				if (DefaultValueRaw is FaParser.ExprContext _expr_raw) {
					DefaultValue = IAstExpr.FromContext (_expr_raw);
				} else if (DefaultValueRaw is FaParser.MiddleExprContext _mexpr_raw) {
					DefaultValue = IAstExpr.FromContext (_mexpr_raw);
				} else {
					throw new NotImplementedException ();
				}
			}
		}

		public override string GenerateCSharp (int _indent) {
			//if (DefaultValue != null) {
			//	var (_a, _b) = DefaultValue.GenerateCSharp (_indent, "");
			//	return ("", $"{_a}{_indent.Indent ()}{Level.ToString ().ToLower ()}{(Static ? " static" : "")} {DataType} {Name} = {_b};");
			//} else {
			return $"{_indent.Indent ()}{Level.ToString ().ToLower ()}{(Static ? " static" : "")} {DataType.GenerateCSharp (_indent)} {Name};\r\n";
			//}
		}

		//public static int n0 = new Func<int> (() => 1).Invoke ();
	}
}
