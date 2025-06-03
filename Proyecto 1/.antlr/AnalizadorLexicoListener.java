// Generated from c:/Users/Rodas/OneDrive/Documentos/USAC/11vo Semestre/OLC2/Lab/Proyecto 1/AnalizadorLexico.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.tree.ParseTreeListener;

/**
 * This interface defines a complete listener for a parse tree produced by
 * {@link AnalizadorLexicoParser}.
 */
public interface AnalizadorLexicoListener extends ParseTreeListener {
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#inicio}.
	 * @param ctx the parse tree
	 */
	void enterInicio(AnalizadorLexicoParser.InicioContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#inicio}.
	 * @param ctx the parse tree
	 */
	void exitInicio(AnalizadorLexicoParser.InicioContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#listaInstr}.
	 * @param ctx the parse tree
	 */
	void enterListaInstr(AnalizadorLexicoParser.ListaInstrContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#listaInstr}.
	 * @param ctx the parse tree
	 */
	void exitListaInstr(AnalizadorLexicoParser.ListaInstrContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion(AnalizadorLexicoParser.InstruccionContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion(AnalizadorLexicoParser.InstruccionContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#print}.
	 * @param ctx the parse tree
	 */
	void enterPrint(AnalizadorLexicoParser.PrintContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#print}.
	 * @param ctx the parse tree
	 */
	void exitPrint(AnalizadorLexicoParser.PrintContext ctx);
	/**
	 * Enter a parse tree produced by the {@code declaracionVar}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionVariable}.
	 * @param ctx the parse tree
	 */
	void enterDeclaracionVar(AnalizadorLexicoParser.DeclaracionVarContext ctx);
	/**
	 * Exit a parse tree produced by the {@code declaracionVar}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionVariable}.
	 * @param ctx the parse tree
	 */
	void exitDeclaracionVar(AnalizadorLexicoParser.DeclaracionVarContext ctx);
	/**
	 * Enter a parse tree produced by the {@code declaracionSliceVacio}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionSlice}.
	 * @param ctx the parse tree
	 */
	void enterDeclaracionSliceVacio(AnalizadorLexicoParser.DeclaracionSliceVacioContext ctx);
	/**
	 * Exit a parse tree produced by the {@code declaracionSliceVacio}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionSlice}.
	 * @param ctx the parse tree
	 */
	void exitDeclaracionSliceVacio(AnalizadorLexicoParser.DeclaracionSliceVacioContext ctx);
	/**
	 * Enter a parse tree produced by the {@code declaracionSliceLleno}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionSlice}.
	 * @param ctx the parse tree
	 */
	void enterDeclaracionSliceLleno(AnalizadorLexicoParser.DeclaracionSliceLlenoContext ctx);
	/**
	 * Exit a parse tree produced by the {@code declaracionSliceLleno}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionSlice}.
	 * @param ctx the parse tree
	 */
	void exitDeclaracionSliceLleno(AnalizadorLexicoParser.DeclaracionSliceLlenoContext ctx);
	/**
	 * Enter a parse tree produced by the {@code declaracionMatrizLleno}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionMatriz}.
	 * @param ctx the parse tree
	 */
	void enterDeclaracionMatrizLleno(AnalizadorLexicoParser.DeclaracionMatrizLlenoContext ctx);
	/**
	 * Exit a parse tree produced by the {@code declaracionMatrizLleno}
	 * labeled alternative in {@link AnalizadorLexicoParser#declaracionMatriz}.
	 * @param ctx the parse tree
	 */
	void exitDeclaracionMatrizLleno(AnalizadorLexicoParser.DeclaracionMatrizLlenoContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#listaFilas}.
	 * @param ctx the parse tree
	 */
	void enterListaFilas(AnalizadorLexicoParser.ListaFilasContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#listaFilas}.
	 * @param ctx the parse tree
	 */
	void exitListaFilas(AnalizadorLexicoParser.ListaFilasContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#fila}.
	 * @param ctx the parse tree
	 */
	void enterFila(AnalizadorLexicoParser.FilaContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#fila}.
	 * @param ctx the parse tree
	 */
	void exitFila(AnalizadorLexicoParser.FilaContext ctx);
	/**
	 * Enter a parse tree produced by the {@code asignacionVar}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void enterAsignacionVar(AnalizadorLexicoParser.AsignacionVarContext ctx);
	/**
	 * Exit a parse tree produced by the {@code asignacionVar}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void exitAsignacionVar(AnalizadorLexicoParser.AsignacionVarContext ctx);
	/**
	 * Enter a parse tree produced by the {@code asignacionIncremento}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void enterAsignacionIncremento(AnalizadorLexicoParser.AsignacionIncrementoContext ctx);
	/**
	 * Exit a parse tree produced by the {@code asignacionIncremento}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void exitAsignacionIncremento(AnalizadorLexicoParser.AsignacionIncrementoContext ctx);
	/**
	 * Enter a parse tree produced by the {@code asignacionSlice}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void enterAsignacionSlice(AnalizadorLexicoParser.AsignacionSliceContext ctx);
	/**
	 * Exit a parse tree produced by the {@code asignacionSlice}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void exitAsignacionSlice(AnalizadorLexicoParser.AsignacionSliceContext ctx);
	/**
	 * Enter a parse tree produced by the {@code append}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void enterAppend(AnalizadorLexicoParser.AppendContext ctx);
	/**
	 * Exit a parse tree produced by the {@code append}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void exitAppend(AnalizadorLexicoParser.AppendContext ctx);
	/**
	 * Enter a parse tree produced by the {@code sliceUpdate}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void enterSliceUpdate(AnalizadorLexicoParser.SliceUpdateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code sliceUpdate}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void exitSliceUpdate(AnalizadorLexicoParser.SliceUpdateContext ctx);
	/**
	 * Enter a parse tree produced by the {@code incrementoDecremento}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void enterIncrementoDecremento(AnalizadorLexicoParser.IncrementoDecrementoContext ctx);
	/**
	 * Exit a parse tree produced by the {@code incrementoDecremento}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void exitIncrementoDecremento(AnalizadorLexicoParser.IncrementoDecrementoContext ctx);
	/**
	 * Enter a parse tree produced by the {@code matrizUpdate}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void enterMatrizUpdate(AnalizadorLexicoParser.MatrizUpdateContext ctx);
	/**
	 * Exit a parse tree produced by the {@code matrizUpdate}
	 * labeled alternative in {@link AnalizadorLexicoParser#asignacion}.
	 * @param ctx the parse tree
	 */
	void exitMatrizUpdate(AnalizadorLexicoParser.MatrizUpdateContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#tipo}.
	 * @param ctx the parse tree
	 */
	void enterTipo(AnalizadorLexicoParser.TipoContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#tipo}.
	 * @param ctx the parse tree
	 */
	void exitTipo(AnalizadorLexicoParser.TipoContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion_if}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion_if(AnalizadorLexicoParser.Instruccion_ifContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion_if}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion_if(AnalizadorLexicoParser.Instruccion_ifContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion_ifelse}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion_ifelse(AnalizadorLexicoParser.Instruccion_ifelseContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion_ifelse}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion_ifelse(AnalizadorLexicoParser.Instruccion_ifelseContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion_else}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion_else(AnalizadorLexicoParser.Instruccion_elseContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion_else}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion_else(AnalizadorLexicoParser.Instruccion_elseContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion_switch}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion_switch(AnalizadorLexicoParser.Instruccion_switchContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion_switch}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion_switch(AnalizadorLexicoParser.Instruccion_switchContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#lista_case}.
	 * @param ctx the parse tree
	 */
	void enterLista_case(AnalizadorLexicoParser.Lista_caseContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#lista_case}.
	 * @param ctx the parse tree
	 */
	void exitLista_case(AnalizadorLexicoParser.Lista_caseContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion_case}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion_case(AnalizadorLexicoParser.Instruccion_caseContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion_case}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion_case(AnalizadorLexicoParser.Instruccion_caseContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion_default}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion_default(AnalizadorLexicoParser.Instruccion_defaultContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion_default}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion_default(AnalizadorLexicoParser.Instruccion_defaultContext ctx);
	/**
	 * Enter a parse tree produced by the {@code forNormal}
	 * labeled alternative in {@link AnalizadorLexicoParser#instruccion_for}.
	 * @param ctx the parse tree
	 */
	void enterForNormal(AnalizadorLexicoParser.ForNormalContext ctx);
	/**
	 * Exit a parse tree produced by the {@code forNormal}
	 * labeled alternative in {@link AnalizadorLexicoParser#instruccion_for}.
	 * @param ctx the parse tree
	 */
	void exitForNormal(AnalizadorLexicoParser.ForNormalContext ctx);
	/**
	 * Enter a parse tree produced by the {@code forIncremento}
	 * labeled alternative in {@link AnalizadorLexicoParser#instruccion_for}.
	 * @param ctx the parse tree
	 */
	void enterForIncremento(AnalizadorLexicoParser.ForIncrementoContext ctx);
	/**
	 * Exit a parse tree produced by the {@code forIncremento}
	 * labeled alternative in {@link AnalizadorLexicoParser#instruccion_for}.
	 * @param ctx the parse tree
	 */
	void exitForIncremento(AnalizadorLexicoParser.ForIncrementoContext ctx);
	/**
	 * Enter a parse tree produced by the {@code forRange}
	 * labeled alternative in {@link AnalizadorLexicoParser#instruccion_for}.
	 * @param ctx the parse tree
	 */
	void enterForRange(AnalizadorLexicoParser.ForRangeContext ctx);
	/**
	 * Exit a parse tree produced by the {@code forRange}
	 * labeled alternative in {@link AnalizadorLexicoParser#instruccion_for}.
	 * @param ctx the parse tree
	 */
	void exitForRange(AnalizadorLexicoParser.ForRangeContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#instruccion_funcion}.
	 * @param ctx the parse tree
	 */
	void enterInstruccion_funcion(AnalizadorLexicoParser.Instruccion_funcionContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#instruccion_funcion}.
	 * @param ctx the parse tree
	 */
	void exitInstruccion_funcion(AnalizadorLexicoParser.Instruccion_funcionContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#llamada_funcion}.
	 * @param ctx the parse tree
	 */
	void enterLlamada_funcion(AnalizadorLexicoParser.Llamada_funcionContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#llamada_funcion}.
	 * @param ctx the parse tree
	 */
	void exitLlamada_funcion(AnalizadorLexicoParser.Llamada_funcionContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#lista_parametros}.
	 * @param ctx the parse tree
	 */
	void enterLista_parametros(AnalizadorLexicoParser.Lista_parametrosContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#lista_parametros}.
	 * @param ctx the parse tree
	 */
	void exitLista_parametros(AnalizadorLexicoParser.Lista_parametrosContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#breakInstr}.
	 * @param ctx the parse tree
	 */
	void enterBreakInstr(AnalizadorLexicoParser.BreakInstrContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#breakInstr}.
	 * @param ctx the parse tree
	 */
	void exitBreakInstr(AnalizadorLexicoParser.BreakInstrContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#continueInstr}.
	 * @param ctx the parse tree
	 */
	void enterContinueInstr(AnalizadorLexicoParser.ContinueInstrContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#continueInstr}.
	 * @param ctx the parse tree
	 */
	void exitContinueInstr(AnalizadorLexicoParser.ContinueInstrContext ctx);
	/**
	 * Enter a parse tree produced by {@link AnalizadorLexicoParser#returnInstr}.
	 * @param ctx the parse tree
	 */
	void enterReturnInstr(AnalizadorLexicoParser.ReturnInstrContext ctx);
	/**
	 * Exit a parse tree produced by {@link AnalizadorLexicoParser#returnInstr}.
	 * @param ctx the parse tree
	 */
	void exitReturnInstr(AnalizadorLexicoParser.ReturnInstrContext ctx);
	/**
	 * Enter a parse tree produced by the {@code booleanExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterBooleanExpresion(AnalizadorLexicoParser.BooleanExpresionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code booleanExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitBooleanExpresion(AnalizadorLexicoParser.BooleanExpresionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code strconvParseFloat}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterStrconvParseFloat(AnalizadorLexicoParser.StrconvParseFloatContext ctx);
	/**
	 * Exit a parse tree produced by the {@code strconvParseFloat}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitStrconvParseFloat(AnalizadorLexicoParser.StrconvParseFloatContext ctx);
	/**
	 * Enter a parse tree produced by the {@code TypeOf}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterTypeOf(AnalizadorLexicoParser.TypeOfContext ctx);
	/**
	 * Exit a parse tree produced by the {@code TypeOf}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitTypeOf(AnalizadorLexicoParser.TypeOfContext ctx);
	/**
	 * Enter a parse tree produced by the {@code operadorLogico}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterOperadorLogico(AnalizadorLexicoParser.OperadorLogicoContext ctx);
	/**
	 * Exit a parse tree produced by the {@code operadorLogico}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitOperadorLogico(AnalizadorLexicoParser.OperadorLogicoContext ctx);
	/**
	 * Enter a parse tree produced by the {@code expreParentesis}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterExpreParentesis(AnalizadorLexicoParser.ExpreParentesisContext ctx);
	/**
	 * Exit a parse tree produced by the {@code expreParentesis}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitExpreParentesis(AnalizadorLexicoParser.ExpreParentesisContext ctx);
	/**
	 * Enter a parse tree produced by the {@code idExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterIdExpresion(AnalizadorLexicoParser.IdExpresionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code idExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitIdExpresion(AnalizadorLexicoParser.IdExpresionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code sliceAccess}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterSliceAccess(AnalizadorLexicoParser.SliceAccessContext ctx);
	/**
	 * Exit a parse tree produced by the {@code sliceAccess}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitSliceAccess(AnalizadorLexicoParser.SliceAccessContext ctx);
	/**
	 * Enter a parse tree produced by the {@code ejecutarFunciones}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterEjecutarFunciones(AnalizadorLexicoParser.EjecutarFuncionesContext ctx);
	/**
	 * Exit a parse tree produced by the {@code ejecutarFunciones}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitEjecutarFunciones(AnalizadorLexicoParser.EjecutarFuncionesContext ctx);
	/**
	 * Enter a parse tree produced by the {@code strconvAtoi}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterStrconvAtoi(AnalizadorLexicoParser.StrconvAtoiContext ctx);
	/**
	 * Exit a parse tree produced by the {@code strconvAtoi}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitStrconvAtoi(AnalizadorLexicoParser.StrconvAtoiContext ctx);
	/**
	 * Enter a parse tree produced by the {@code matrizAccess}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterMatrizAccess(AnalizadorLexicoParser.MatrizAccessContext ctx);
	/**
	 * Exit a parse tree produced by the {@code matrizAccess}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitMatrizAccess(AnalizadorLexicoParser.MatrizAccessContext ctx);
	/**
	 * Enter a parse tree produced by the {@code stringsJoin}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterStringsJoin(AnalizadorLexicoParser.StringsJoinContext ctx);
	/**
	 * Exit a parse tree produced by the {@code stringsJoin}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitStringsJoin(AnalizadorLexicoParser.StringsJoinContext ctx);
	/**
	 * Enter a parse tree produced by the {@code operadorNegacion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterOperadorNegacion(AnalizadorLexicoParser.OperadorNegacionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code operadorNegacion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitOperadorNegacion(AnalizadorLexicoParser.OperadorNegacionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code caracterExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterCaracterExpresion(AnalizadorLexicoParser.CaracterExpresionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code caracterExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitCaracterExpresion(AnalizadorLexicoParser.CaracterExpresionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code decimalExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterDecimalExpresion(AnalizadorLexicoParser.DecimalExpresionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code decimalExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitDecimalExpresion(AnalizadorLexicoParser.DecimalExpresionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code nilExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterNilExpresion(AnalizadorLexicoParser.NilExpresionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code nilExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitNilExpresion(AnalizadorLexicoParser.NilExpresionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code sliceIndex}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterSliceIndex(AnalizadorLexicoParser.SliceIndexContext ctx);
	/**
	 * Exit a parse tree produced by the {@code sliceIndex}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitSliceIndex(AnalizadorLexicoParser.SliceIndexContext ctx);
	/**
	 * Enter a parse tree produced by the {@code cadenaExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterCadenaExpresion(AnalizadorLexicoParser.CadenaExpresionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code cadenaExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitCadenaExpresion(AnalizadorLexicoParser.CadenaExpresionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code len}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterLen(AnalizadorLexicoParser.LenContext ctx);
	/**
	 * Exit a parse tree produced by the {@code len}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitLen(AnalizadorLexicoParser.LenContext ctx);
	/**
	 * Enter a parse tree produced by the {@code multiplicacionYdivision}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterMultiplicacionYdivision(AnalizadorLexicoParser.MultiplicacionYdivisionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code multiplicacionYdivision}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitMultiplicacionYdivision(AnalizadorLexicoParser.MultiplicacionYdivisionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code intExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterIntExpresion(AnalizadorLexicoParser.IntExpresionContext ctx);
	/**
	 * Exit a parse tree produced by the {@code intExpresion}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitIntExpresion(AnalizadorLexicoParser.IntExpresionContext ctx);
	/**
	 * Enter a parse tree produced by the {@code operadorRelacional}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterOperadorRelacional(AnalizadorLexicoParser.OperadorRelacionalContext ctx);
	/**
	 * Exit a parse tree produced by the {@code operadorRelacional}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitOperadorRelacional(AnalizadorLexicoParser.OperadorRelacionalContext ctx);
	/**
	 * Enter a parse tree produced by the {@code operadorNegativo}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterOperadorNegativo(AnalizadorLexicoParser.OperadorNegativoContext ctx);
	/**
	 * Exit a parse tree produced by the {@code operadorNegativo}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitOperadorNegativo(AnalizadorLexicoParser.OperadorNegativoContext ctx);
	/**
	 * Enter a parse tree produced by the {@code sumaYresta}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void enterSumaYresta(AnalizadorLexicoParser.SumaYrestaContext ctx);
	/**
	 * Exit a parse tree produced by the {@code sumaYresta}
	 * labeled alternative in {@link AnalizadorLexicoParser#expr}.
	 * @param ctx the parse tree
	 */
	void exitSumaYresta(AnalizadorLexicoParser.SumaYrestaContext ctx);
}