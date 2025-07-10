grammar RateFormula;

// Rules

expression 
    : multiplyingExpression contExpression*
    ;

contExpression
    : operand=(PLUS|MINUS) contExp=multiplyingExpression
    ;

multiplyingExpression  
    : atom contMultiplyingExpression*
    ;

contMultiplyingExpression
    : TIMES contExp=atom # timesExp
    | DIV contExp=atom # divExp
    ;

atom 
    : number # atomNumber
    | LPAREN exp=expression RPAREN # atomExpression
    | minExpression # atomMIN
    | maxExpression # atomMAX
    | ifExpression # atomIF
    | roundExpression # atomRound
    | variable # atomVariable
    ;

roundExpression
    : ROUND LPAREN expression COMMA Integer RPAREN
    ;

number
    : numberBody=(Integer|DoubleNumber) PERCENT?
    ;

variable
    : uomPlaceHolder # varUOM
    | reservedVFD # varReservedKeyword
    | reservedCV # varReservedKeyword
    | reservedDOV # varDateOfValuation
    | code=CountrySpecificValue # varCountrySpecificKeyword
    | formulaSpecificValue # varAskAQuestion
    | meursingPlaceHolder # varMEURSING
    ;

reservedVFD : VFD ;
reservedCV : CV ;
reservedDOV : DOV ;

formulaSpecificValue
    : LCURLY formulaSpecificValueSpec? questionToAsk=STRING RCURLY
    ;

formulaSpecificValueSpec
    : DECIMAL LPAREN precision=Integer COMMA scale=Integer RPAREN COLUMN
    ;

ifExpression
    : IF LPAREN boolExpression COMMA trueExp=expression COMMA falseExp=expression RPAREN
    ;

hasExpression
	: HAS LPAREN opleft=STRING COMMA opright=STRING RPAREN
	;

boolExpression
    : basicExp=boolAndExpression (LOGICOR contExp+=boolAndExpression)*
    ;

boolAndExpression
    : basicExp=boolAtom (LOGICAND contExp+=boolAtom)*
    ;

boolAtom
    : op1=expression EQ op2=expression # boolEQ
    | op1=expression NOTEQ op2=expression # boolNE
    | op1=expression GT op2=expression # boolGT
    | op1=expression LT op2=expression # boolLT
    | op1=expression GTEQ op2=expression # boolGTEQ
    | op1=expression LTEQ op2=expression # boolLTEQ
    | LPAREN boolExpression RPAREN # boolWRAP
    | hasExpression # boolHAS
    ;

minExpression
    : MIN LPAREN opleft=expression COMMA opright=expression RPAREN
    ;

maxExpression
    : MAX LPAREN opleft=expression COMMA opright=expression RPAREN
    ;

uomPlaceHolder
    : UOMCode
    ;

meursingPlaceHolder
    : MEURSINGCode
    ;

// Lexars

VFD : V F D ;
CV : C V ;
DOV : D O V ;

PERCENT : '%' ; 
LCURLY : '{' ; 
RCURLY : '}' ; 
COLUMN : ':' ; 
LPAREN : '(' ; 
RPAREN : ')' ; 

DECIMAL : D E C I M A L ;
ROUND : R O U N D ;
MIN : M I N ;
MAX : M A X ;
IF : I F ;
HAS : H A S ;

Integer : DIGIT+ ;
DoubleNumber : Integer POINT Integer ;

LOGICAND : '&' ;
LOGICOR : '|' ;
PLUS : '+' ;
MINUS : '-' ;
TIMES : '*' ;
DIV : '/' ;
GT : '>' ;
LT : '<' ;
EQ : '=' ;
NOTEQ : '!=' ;
GTEQ : '>=' ;
LTEQ : '<=' ;

fragment POINT : '.' ;

COMMA : ',' ;

UOMCode : LSQUARE (LETTER | DIGIT) ((LETTER | DIGIT | ' ')* (LETTER | DIGIT))? RSQUARE;
MEURSINGCode : HASHTAG (LETTER | DIGIT)+LPAREN DIGIT RPAREN HASHTAG ;

LSQUARE : '[' ;
RSQUARE : ']' ;
HASHTAG : '#' ;

CountrySpecificValue : (LETTER | DIGIT)+ ;

STRING : '"' (LETTER | DIGIT) (~[:}])*? (~[ :}]) '"' ;

LETTER : LOWLETTER | UPLETTER ;

fragment A : 'A' | 'a' ;
fragment C : 'C' | 'c' ;
fragment D : 'D' | 'd' ;
fragment E : 'E' | 'e' ;
fragment F : 'F' | 'f' ;
fragment H : 'H' | 'h' ;
fragment I : 'I' | 'i' ;
fragment L : 'L' | 'l' ;
fragment M : 'M' | 'm' ;
fragment N : 'N' | 'n' ;
fragment O : 'O' | 'o' ;
fragment R : 'R' | 'r' ;
fragment S : 'S' | 's' ;
fragment U : 'U' | 'u' ;
fragment V : 'V' | 'v' ;
fragment X : 'X' | 'x' ;
fragment UPLETTER : ('A'..'Z') ;
fragment LOWLETTER : ('a'..'z') ;
fragment DIGIT : ZERODIGIT | NONZERODIGIT ;
fragment ZERODIGIT : '0' ;
fragment NONZERODIGIT : [1-9] ;

WS : [ \r\n\t]+ -> channel(HIDDEN) ;
