// C++�﷨
// https://github.com/antlr/grammars-v4/blob/master/cpp/CPP14Lexer.g4
// https://github.com/antlr/grammars-v4/blob/master/cpp/CPP14Parser.g4

// ANTLR�ĵ�
// https://decaf-lang.github.io/minidecaf-tutorial/
// https://wizardforcel.gitbooks.io/antlr4-short-course/content/

grammar Fa;



//
// keyword
//
Break:						'break';
CC__Cdecl:					'__cdecl';
CC__FastCall:				'__fastcall';
CC__StdCall:				'__stdcall';
Continue:					'continue';
Class:						'class';
Const:						'const';
Do:							'do';
Else:						'else';
Enum:						'enum';
For:						'for';
If:							'if';
Is:							'is';
Interface:					'interface';
Internal:					'internal';
Mut:						'mut';
Namespace:					'namespace';
New:						'new';
Operator:					'operator';
Params:						'params';
Public:						'public';
Protected:					'protected';
Private:					'private';
Return:						'return';
Signed:						'signed';
Static:						'static';
Step:						'step';
SwitchExpr:					'switchexpr';
Switch:						'switch';
Unsigned:					'unsigned';
Use:						'use';
Var:						'var';
When:						'when';
While:						'while';



//
// element
//

// ��ֵ����
Assign:						'=';
AddAssign:					AddOp Assign;
SubAssign:					SubOp Assign;
StarAssign:					StarOp Assign;
DivAssign:					DivOp Assign;
ModAssign:					ModOp Assign;
OrAssign:					OrOp Assign;
AndAssign:					AndOp Assign;
XorAssign:					XorOp Assign;
QusQusAssign:				QusQusOp Assign;
StarStarAssign:				StarStarOp Assign;
AndAndAssign:				AndAndOp Assign;
OrOrAssign:					OrOrOp Assign;
shiftLAssign:				shiftLOp Assign; // ������
shiftRAssign:				shiftROp Assign;
allAssign:					Assign | QusQusAssign | AddAssign | SubAssign | StarAssign | StarStarAssign | DivAssign | ModAssign | AndAssign | OrAssign | XorAssign | AndAndAssign | OrOrAssign | shiftLAssign | shiftRAssign;

// һԪ����
ReverseOp:					'~';
AddAddOp:					'++';
SubSubOp:					'--';
Exclam:						'!';

// ��Ԫ����
PointPoint:					'..';
PointOp:					'.';
AddOp:						'+';
SubOp:						'-';
StarOp:						'*';
DivOp:						'/';
ModOp:						'%';
OrOp:						'|';
AndOp:						'&';
XorOp:						'^';
QusQusOp:					Qus Qus;
StarStarOp:					StarOp StarOp;
AndAndOp:					AndOp AndOp;
OrOrOp:						OrOp OrOp;
shiftLOp:					qJianL qJianL;
shiftROp:					qJianR qJianR;

// ��Ԫ������
Qus:						'?';
Comma:						',';
ColonColon:					'::';
Colon:						':';
Lf:							'\n';
Semi:						';';
Underline:					'_';
endl:						(Lf | Semi)+;
endl2:						(Lf | Comma)+;

// ����
qFangL:						'[';
qFangR:						']';
qJianL:						'<';
qJianR:						'>';
qHuaL:						'{';
qHuaR:						'}';
qYuanL:						'(';
qYuanR:						')';
quotFangL:					qFangL endl?;
quotFangR:					endl? qFangR;
quotJianL:					qJianL endl?;
quotJianR:					endl? qJianR;
quotHuaL:					qHuaL endl?;
quotHuaR:					endl? qHuaR;
quotYuanL:					qYuanL endl?;
quotYuanR:					endl? qYuanR;

// �Ƚ�   TODO
ltOp:						qJianL;
ltEqualOp:					qJianL Assign;
gtOp:						qJianR;
gtEqualOp:					qJianR Assign;
equalOp:					Assign Assign;
notEqualOp:					Exclam Assign;
exprFuncDef:				Assign qJianR;


selfOp2:					AddOp | SubOp | StarOp | DivOp | StarStarOp | ModOp | AndOp | OrOp | XorOp | AndAndOp | OrOrOp | shiftLOp | shiftROp;
compareOp2:					ltOp | ltEqualOp | gtOp | gtEqualOp | equalOp | notEqualOp;
changeOp2:					QusQusOp | compareOp2;
allOp2:						selfOp2 | changeOp2;



//
// Literal
//
fragment SimpleEscape:		'\\\'' | '\\"' | '\\\\' | '\\n' | '\\r' | ('\\' ('\r' '\n'? | '\n')) | '\\t';
fragment HexEscape:			'\\x' HEX HEX;
fragment UniEscape:			'\\u' HEX HEX HEX HEX;
fragment Schar:				SimpleEscape | HexEscape | UniEscape | ~["\\\r\n];
//
BoolLiteral:				'true' | 'false';
IntLiteral:					NUM+;
intNum:						SubOp? IntLiteral;
FloatLiteral:				NUM+ PointOp NUM+;
floatNum:					SubOp? FloatLiteral;
String1Literal:				'"' Schar* '"';
literal:					BoolLiteral | intNum | floatNum | String1Literal;

fragment NUM:				[0-9];
fragment HEX:				NUM | [a-fA-F];
fragment ID_BEGIN:			[a-zA-Z] | [\u0080-\u{10FFFF}];
fragment ID_AFTER:			NUM | [a-zA-Z_] | [\u0080-\u{10FFFF}];
PrepId:						'@' ID_AFTER+;
RawId:						(ID_BEGIN ID_AFTER*) | ('_' ID_AFTER+);
id:							Underline | PrepId | RawId;
ids:						id (PointOp id)*;



//
// type
//
typeAfter:					(quotFangL quotFangR) | Qus;
typeSingle:					ids (quotJianL typeWrap (Comma typeWrap)* quotJianR)?;
typeMulti:					quotYuanL typeVar (Comma typeVar)+ quotYuanR;
type:						(typeSingle | typeMulti) typeAfter*;
typeWrap:					(Mut | Params)? type;



//
// list
//
typeVar:					type id?;
typeVarList:				typeVar (Comma typeVar)*;
typeWrapVar1:				id Colon typeWrap;
typeWrapVarList1:			typeWrapVar1 (Comma typeWrapVar1)*;
typeWrapVar2:				typeWrap id?;
typeWrapVarList2:			typeWrapVar2 (Comma typeWrapVar2)*;
typeWrapVar3:				typeWrap? id;
typeWrapVarList3:			typeWrapVar3 (Comma typeWrapVar3)*;
//eTypeVar:					eType id?;
//eTypeVarList:				eTypeVar (Comma eTypeVar)*;



//
// if
//
quotStmtPart:				quotHuaL stmt* quotHuaR;
quotStmtExpr:				quotHuaL stmt* expr quotHuaR;
ifStmt:						If expr quotStmtPart (Else If expr quotStmtPart)* (Else quotStmtPart)?;
ifExpr:						If expr quotStmtExpr (Else If expr quotStmtExpr)* Else quotStmtExpr;



//
// loop
//
whileStmt:					While expr quotHuaL stmt* quotHuaR;
whileStmt2:					Do quotHuaL stmt* quotHuaR While expr;
forStmt:					For stmt expr Semi (expr (Comma expr)*)? quotHuaL stmt* quotHuaR;
forStmt2:					For type id Colon expr quotHuaL stmt* quotHuaR;



//
// switch
//
switchStmtPart2Last:		Underline exprFuncDef stmt;
quotStmtExprWrap:			quotStmtExpr | expr;
switchExprPartLast:			Underline exprFuncDef quotStmtExprWrap endl2?;
//
switchStmtPart:				expr (When expr)? exprFuncDef stmt;
switchStmt:					Switch expr quotHuaL switchStmtPart* quotHuaR;
switchStmtPart2:			When expr exprFuncDef stmt;
switchStmt2:				Switch quotHuaL switchStmtPart2* switchStmtPart2Last quotHuaR;
//
switchExprPart:				expr (When expr)? exprFuncDef quotStmtExprWrap endl2;
switchExpr:					SwitchExpr expr quotHuaL switchExprPart* switchExprPartLast quotHuaR;
switchExprPart2:			When expr exprFuncDef quotStmtExprWrap endl2;
switchExpr2:				SwitchExpr quotHuaL switchExprPart2* switchExprPartLast quotHuaR;



//
// expr
//
quotExpr:					quotYuanL expr quotYuanR;
exprOpt:					expr?;
newExprItem:				id (Assign middleExpr)?;
newExpr1:					New typeSingle? quotHuaL (newExprItem (Comma newExprItem)*)? quotHuaR;
newExpr2:					New typeSingle? quotYuanL (expr (Comma expr)*)? quotYuanR;
//newArray:					New typeSingle? quotFangL middleExpr quotFangR;
arrayExpr1:					quotFangL expr PointPoint expr (Step expr)? quotFangR;
arrayExpr2:					quotFangL expr (Comma expr)* quotFangR;
lambdaExpr:					quotYuanL typeWrapVarList3? quotYuanR exprFuncDef (expr | (quotHuaL stmt* quotHuaR));
strongExprBase:				(ColonColon? id) | literal | ifExpr | quotExpr | newExpr1 | newExpr2 | arrayExpr1 | arrayExpr2 | switchExpr2 | switchExpr | lambdaExpr;
strongExprPrefix:			SubOp | AddAddOp | SubSubOp | ReverseOp | Exclam;								// ǰ׺ - ++ -- ~ !
strongExprSuffix			: AddAddOp | SubSubOp															// ��׺ ++ --
							| (quotYuanL (expr (Comma expr)*)? quotYuanR)									//     Write ("")
							| (quotFangL (exprOpt (Colon exprOpt)*) quotFangR)								//     list [12]
							| (PointOp id)																	//     wnd.Name
							| (Is ids (quotYuanL id quotYuanR)?)											//     _a is EnumA (_val)
							;
strongExpr:					strongExprPrefix* strongExprBase strongExprSuffix*;
middleExpr:					strongExpr (allOp2 strongExpr)*;												//     a == 24    a + b - c
//expr:						middleExpr ((Qus strongExprBase Colon strongExprBase) | (allAssign middleExpr)*); // ������Ŀ�����?:
expr:						middleExpr (allAssign middleExpr)*;



//
// define variable
//
idAssignExpr:				id (Colon type)? Assign middleExpr;
defVarStmt:					Var idAssignExpr (Comma idAssignExpr)* endl;
idAssignExpr2:				id Assign middleExpr;
defVarStmt2:				type idAssignExpr2 (Comma idAssignExpr2)* endl;



//
// stmt
//
normalStmt:					((Return? expr?) | Break | Continue) endl;
stmt:						ifStmt | whileStmt | whileStmt2 | forStmt | forStmt2 | quotStmtPart | switchStmt2 | switchStmt | defVarStmt | defVarStmt2 | normalStmt;



//
// blocks base
//
publicLevel:				Public | Internal | Protected | Private;
blockTemplates:				quotJianL id (Comma id)* quotJianR;
itemName:					id | (Operator allOp2);
typeNameTuple:				(itemName Colon type) | (type itemName);
typeNameArgsTuple:			(itemName blockTemplates? quotYuanL typeWrapVarList1? quotYuanR Colon type) | (type itemName blockTemplates? quotYuanL typeWrapVarList2? quotYuanR);
nameArgsTuple:				id quotYuanL (typeWrapVarList1 | typeWrapVarList2)? quotYuanR;
funcBody:					(exprFuncDef expr) | (quotHuaL stmt* quotHuaR);
//classParent:				Colon ids (Comma ids)*;



//
// annotation
//
annoArg:					id Assign literal;
annoUsingPart:				quotFangL id (quotYuanL (annoArg endl2)* annoArg endl2? quotYuanR)? quotFangR endl;



//
// class
//
interfaceConstructFunc:		annoUsingPart* publicLevel? nameArgsTuple endl;
interfaceItemFunc:			annoUsingPart* publicLevel? Static? typeNameArgsTuple endl;
interfaceBlock:				annoUsingPart* publicLevel? Interface id blockTemplates? quotHuaL (classItemVar | interfaceItemFunc | interfaceConstructFunc)* quotHuaR endl;
//
classConstructFunc:			annoUsingPart* publicLevel? nameArgsTuple funcBody endl;
classItemVar:				annoUsingPart* publicLevel? Static? typeNameTuple (Assign middleExpr)? endl;
classItemFunc:				annoUsingPart* publicLevel? Static? typeNameArgsTuple funcBody endl;
classBlock:					annoUsingPart* publicLevel? Class id blockTemplates? quotHuaL (classItemVar | classItemFunc | classConstructFunc)* quotHuaR endl;
//
enumItem:					annoUsingPart* id (quotYuanL type quotYuanR)?;
enumBlock:					annoUsingPart* publicLevel? Enum id blockTemplates? quotHuaL
							(((enumItem endl2)* enumItem) | ((enumItem endl2)+ classItemFunc*))
							quotHuaR endl;



//
// file
//
useStmt:					Use (id Assign)? ids endl;
//callConvention:				CC__Cdecl | CC__FastCall | CC__StdCall;
//importStmt:					AImport type callConvention id quotYuanL typeVarList quotYuanR endl;
//libStmt:					ALib String1Literal endl;
namespaceStmt:				Namespace ids endl;
program:					endl* useStmt* namespaceStmt* (interfaceBlock | enumBlock | classBlock)*;



//
// entry
//
programEntry:				program EOF;
classItemFuncEntry:			classItemFunc EOF;
typeEntry:					type EOF;



//
// skips
//
Comment1:					'/*' .*? '*/' -> channel (HIDDEN);
Comment2:					'//' ~ [\n]* -> channel (HIDDEN);
WS:							[ \t\r]+ -> channel (HIDDEN);
