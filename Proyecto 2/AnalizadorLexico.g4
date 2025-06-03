grammar AnalizadorLexico;

options { caseInsensitive = false; }


LINEA: '//' ~[\r\n]* -> skip;
MULTILINEA: '/*' .*? '*/' -> skip;
NEWLINE: [ \r\n\t]+ -> channel(HIDDEN);

INT: [0-9]+ ;
DECIMAL: [0-9]+'.'[0-9]+ ;
CADENA: '"' (~["\\] | '\\' . | '\n')* '"' ;
CARACTER: '\'' ( ~['\\\r\n] | '\\' [ntr\\'"] ) '\'';
BOOL : ('true'|'false') ;
NIL: 'nil';
ID: [A-Za-z][A-Za-z0-9_]* ;


IGUAL: '=' ;
DPIGUAL: ':=' ;
PARIZQ: '(' ;
PARDER: ')' ;
LLAVEIZQ: '{' ;
LLAVEDER: '}' ;
CORIZQ: '[' ;
CORDER: ']' ;
COMA: ',' ;
INCRE: '++';
DECRE: '--';

inicio: listaInstr
;

listaInstr : instruccion (instruccion)*
;

instruccion: print 
    | declaracionVariable 
    | declaracionSlice
    | asignacion
    | instruccion_if
    | instruccion_switch
    | instruccion_for
    | instruccion_funcion
    | llamada_funcion
    | breakInstr
    | continueInstr
    | returnInstr
;

print: 'fmt.Println' PARIZQ expr (COMA expr)* PARDER 
;

declaracionVariable: 'var' ID tipo (IGUAL expr)? #declaracionVar 
;

declaracionSlice: 'var' ID CORIZQ CORDER tipo #declaracionSliceVacio
    | ID signo=(IGUAL | DPIGUAL) CORIZQ CORDER tipo LLAVEIZQ expr (COMA expr)* LLAVEDER #declaracionSliceLleno
;

asignacion: ID signo=(IGUAL | DPIGUAL) expr #asignacionVar
    | ID operador=('+=' | '-=') expr #asignacionIncremento
    | ID IGUAL ID #asignacionSlice
    | ID IGUAL 'append' PARIZQ ID COMA expr (COMA expr)* PARDER #append
    | ID CORIZQ expr CORDER IGUAL expr #sliceUpdate
    | ID INCRE #incrementoDecremento
    | ID DECRE #incrementoDecremento
;

tipo: 'int'
    | 'float64'
    | 'string'
    | 'bool'
    | 'rune'
;

instruccion_if: 'if' expr LLAVEIZQ listaInstr LLAVEDER (instruccion_ifelse)* (instruccion_else)?
;

instruccion_ifelse: 'else' 'if' expr LLAVEIZQ listaInstr LLAVEDER
;

instruccion_else: 'else' LLAVEIZQ listaInstr LLAVEDER
;

instruccion_switch: 'switch' expr LLAVEIZQ lista_case? instruccion_default? LLAVEDER
;

lista_case: (instruccion_case)+
;

instruccion_case: 'case' expr ':' listaInstr
;

instruccion_default: 'default' ':' listaInstr
;

instruccion_for: 'for' expr LLAVEIZQ listaInstr LLAVEDER #forNormal
    | 'for' asignacion ';' expr ';' asignacion LLAVEIZQ listaInstr LLAVEDER #forIncremento
    | 'for' ID COMA ID DPIGUAL 'range' ID LLAVEIZQ listaInstr LLAVEDER #forRange
;

instruccion_funcion: 'func' ID PARIZQ lista_parametros? PARDER tipo? LLAVEIZQ listaInstr LLAVEDER
;

llamada_funcion: ID PARIZQ lista_argumentos? PARDER ';'?
;

lista_parametros: expr tipo (COMA expr tipo)*
;

lista_argumentos: expr (COMA expr)*
;

breakInstr: 'break'
;

continueInstr: 'continue'
;

returnInstr: 'return' expr? ';'?
;

expr: PARIZQ expr PARDER #expreParentesis
    | INT #intExpresion
    | DECIMAL #decimalExpresion
    | CARACTER #caracterExpresion
    | CADENA #cadenaExpresion
    | BOOL #booleanExpresion
    | ID #idExpresion
    | NIL #nilExpresion

    | '!' right=expr #operadorNegacion
    | '-' right=expr #operadorNegativo

    | expr ('*'|'/'|'%') expr #multiplicacionYdivision
    | expr ('+'|'-') expr #sumaYresta

    | left=expr operador='>=' right=expr #operadorRelacional
    | left=expr operador='>' right=expr #operadorRelacional
    | left=expr operador='<=' right=expr #operadorRelacional
    | left=expr operador='<' right=expr #operadorRelacional

    | left=expr operador='==' right=expr #operadorRelacional
    | left=expr operador='!=' right=expr #operadorRelacional

    | left=expr operador='&&' right=expr #operadorLogico
    | left=expr operador='||' right=expr #operadorLogico

    | 'slices.Index' PARIZQ expr COMA expr PARDER #sliceIndex
    | 'strings.Join' PARIZQ expr COMA CADENA PARDER #stringsJoin
    | 'len' PARIZQ expr PARDER #len
    | ID CORIZQ expr CORDER #sliceAccess

    | 'strconv.Atoi' PARIZQ expr PARDER #strconvAtoi
    | 'strconv.ParseFloat' PARIZQ expr PARDER #strconvParseFloat
    | 'reflect.TypeOf' PARIZQ expr PARDER #TypeOf

    | llamada_funcion #ejecutarFunciones
;