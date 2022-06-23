﻿using Antlr4.Runtime;
using fac.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fac.ASTs.Types {
	public class AstType_Integer: IAstType {
		public bool IsSign { get; set; } = true;
		public int BitWidth { get; set; } = 32;



		public override string ToString () => $"{(IsSign ? "" : "u")}int{BitWidth}";
		public static AstType_Integer FromType (string _type_str, IToken _token) {
			if (!sTypeNames.Contains (_type_str))
				return null;
			_type_str = sTypeMap.ContainsKey (_type_str) ? sTypeMap[_type_str] : _type_str;
			var _inttype = new AstType_Integer { Token = _token };
			_inttype.IsSign = _type_str[0] != 'u';
			_inttype.BitWidth = int.Parse (_type_str.Replace ("u", "").Replace ("int", ""));
			return _inttype;
		}

		public override string GenerateCSharp (int _indent) => (IsSign, BitWidth) switch {
			(true, 8) => "sbyte", (false, 8) => "byte",
			(true, 16) => "short", (false, 16) => "ushort",
			(true, 32) => "int", (false, 32) => "uint",
			(true, 64) => "long", (false, 64) => "ulong",
			_ => throw new UnimplException (Token)
		};

		public override string GenerateCpp (int _indent) => (IsSign, BitWidth) switch {
			(true, 8) => "int8_t",
			(false, 8) => "uint8_t",
			(true, 16) => "int16_t",
			(false, 16) => "uint16_t",
			(true, 32) => "int32_t",
			(false, 32) => "uint32_t",
			(true, 64) => "int64_t",
			(false, 64) => "uint64_t",
			_ => throw new UnimplException (Token)
		};

		private static HashSet<string> sTypeNames = new HashSet<string> { "int", "int8", "int16", "int32", "int64", "uint", "uint8", "uint16", "uint32", "uint64" };
		private static Dictionary<string, string> sTypeMap = new Dictionary<string, string> { ["int"] = "int32", ["uint"] = "uint32" };
	}
}
