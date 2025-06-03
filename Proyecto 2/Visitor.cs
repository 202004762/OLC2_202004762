using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Proyecto_1.AnalizadorLexico;
using Proyecto_1.Excepciones;
using System;
using System.Collections.Generic;


namespace Proyecto_1{
    class Visitor : AnalizadorLexicoBaseVisitor<Object>{
        public Stack<Entorno> pilaEntornos = new Stack<Entorno>();
        public Entorno entornoInicial;
        public List<Object> listaSalida = new List<Object>();
        public List<Errores> erroresSemanticos = new List<Errores>();

        public Visitor(Entorno entorno){
            this.entornoInicial = entorno;
            pilaEntornos.Push(entorno);

        }

        /* INICIO */
        public override Object VisitInicio([NotNull] AnalizadorLexicoParser.InicioContext context){
            if (context.listaInstr() != null) {
                // Primera pasada: registrar funciones
                foreach (var instruccion in context.listaInstr().instruccion()){
                    if (instruccion.instruccion_funcion() != null){
                        Visit(instruccion);

                    }

                }

                var entorno = pilaEntornos.Peek();
                if (entorno.variables.ContainsKey("main")){
                    var llamadaMain = new AnalizadorLexicoParser.Llamada_funcionContext(context, 0);
                    var fakeToken = new Antlr4.Runtime.CommonToken(AnalizadorLexicoParser.ID, "main");
                    llamadaMain.AddChild(new Antlr4.Runtime.Tree.TerminalNodeImpl(fakeToken));
                    VisitLlamada_funcion(llamadaMain);

                }else{
                    //erroresSemanticos.Add(new Errores("Semantico", $"Error: No se encontró la función main.", context.Start.Line, context.Start.Column));

                }

                foreach (var instruccion in context.listaInstr().instruccion()){
                    if (instruccion.instruccion_funcion() == null){
                        Visit(instruccion);

                    }

                }

            }

            return true;

        }

        public override Object VisitListaInstr([NotNull] AnalizadorLexicoParser.ListaInstrContext context){
            if (context.instruccion() == null){
                return true;

            }

            foreach (AnalizadorLexicoParser.InstruccionContext instruccion in context.instruccion()){
                if (instruccion != null){
                    var resultado = VisitInstruccion(instruccion);
                    if (resultado == Break.Instance){
                        return Break.Instance;

                    }else if (resultado == Continue.Instance){
                        return Continue.Instance;

                    }else if (resultado is Return){
                        return resultado;

                    }

                }

            }

            return true;

        }

        public override Object VisitInstruccion([NotNull] AnalizadorLexicoParser.InstruccionContext context){
            object resultado = null;
            if (context.print() != null){
                resultado = Visit(context.print());

            }else if (context.declaracionVariable() != null){
                resultado = Visit(context.declaracionVariable());

            }else if (context.declaracionSlice() != null){
                resultado = Visit(context.declaracionSlice());

            }else if (context.asignacion() != null){
                resultado = Visit(context.asignacion());

            }else if (context.instruccion_if() != null){
                resultado = Visit(context.instruccion_if());

            }else if (context.instruccion_switch() != null){
                resultado = Visit(context.instruccion_switch());

            }else if (context.instruccion_for() != null){
                resultado = Visit(context.instruccion_for());

            }else if (context.instruccion_funcion() != null){
                resultado = Visit(context.instruccion_funcion());

            }else if (context.llamada_funcion() != null){
                resultado = Visit(context.llamada_funcion());

            }else if (context.breakInstr() != null){
                resultado = Visit(context.breakInstr());

            }else if (context.continueInstr() != null){
                resultado = Visit(context.continueInstr());

            }else if (context.returnInstr() != null){
                resultado = Visit(context.returnInstr());

            }

            if (resultado is Break){
                return Break.Instance;

            }else if (resultado is Continue){
                return Continue.Instance;

            }else if (resultado is Return){
                return resultado;

            }

            return true;

        }

        /* IMPRIMIR */
        public override Object VisitPrint([NotNull] AnalizadorLexicoParser.PrintContext context){
            List<object> valores = new List<object>();
            foreach (var expr in context.expr()){
                Object valor = Visit(expr);
                if (valor == null){
                    valores.Add("nil");

                }else{
                    valores.Add(FormatearValor(valor));

                }

            }

            string resultado = string.Join(" ", valores);
            listaSalida.Add(resultado);
            Console.WriteLine(resultado);
            return true;

        }

        private string FormatearValor(object valor){
            if (valor is IEnumerable<object> enumerable){
                List<string> elementos = new List<string>();
                foreach (var item in enumerable){
                    elementos.Add(FormatearValor(item));

                }

                return "[" + string.Join(", ", elementos) + "]";

            }

            return valor?.ToString() ?? "nil";

        }

        /* DECLARACION */
        public override Object VisitDeclaracionVar([NotNull] AnalizadorLexicoParser.DeclaracionVarContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreVariable = context.ID().GetText();
                string tipoVariable = context.tipo().GetText();
                if (entorno.variables.ContainsKey(nombreVariable)){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: La variable '{nombreVariable}' ya fue declarada en este ambito.", context.Start.Line, context.Start.Column));
                    return false;

                }

                object valor = context.expr() != null ? Visit(context.expr()) : ObtenerValorPorDefecto(tipoVariable);
                if (valor == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: No se pudo determinar el valor de la variable '{nombreVariable}'.", context.Start.Line, context.Start.Column));
                    return false;

                }

                ValidarTipo(nombreVariable, tipoVariable, valor);
                entorno.guardarVariable(nombreVariable, new Simbolos(nombreVariable, tipoVariable, valor));
                return true;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en declaracion de variable: {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

        }

        public override Object VisitDeclaracionSliceVacio([NotNull] AnalizadorLexicoParser.DeclaracionSliceVacioContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreVariable = context.ID().GetText();
                string tipoVariable = context.tipo().GetText();
                if (entorno.buscarVariable(nombreVariable) != null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreVariable}' ya fue declarado en este ambito.", context.Start.Line, context.Start.Column));
                    return false;

                }

                List<object> sliceVacio = new List<object>();
                entorno.guardarVariable(nombreVariable, new Simbolos(nombreVariable, $"slice<{tipoVariable}>", sliceVacio));
                Console.WriteLine($"Slice '{nombreVariable}' declarado como vacio.");
                return true;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en declaracion de slice vacio: {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

        }

        public override Object VisitDeclaracionSliceLleno([NotNull] AnalizadorLexicoParser.DeclaracionSliceLlenoContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreVariable = context.ID().GetText();
                string tipoVariable = context.tipo().GetText();
                string signo = context.signo.Text;
                List<object> valores = new List<object>();
                foreach (var expr in context.expr()){
                    object valor = Visit(expr);
                    if (valores.Count > 0 && DeterminarTipo(valor) != tipoVariable){
                        erroresSemanticos.Add(new Errores("Semantico", $"Error: Los valores del slice '{nombreVariable}' deben ser del tipo '{tipoVariable}', pero se encontró '{DeterminarTipo(valor)}'.", context.Start.Line, context.Start.Column));
                        return false;
                    }

                    valores.Add(valor);

                }

                if (signo == ":="){
                    if (entorno.buscarVariable(nombreVariable) != null){
                        erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreVariable}' ya fue declarado en este ambito.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                    entorno.guardarVariable(nombreVariable, new Simbolos(nombreVariable, $"slice<{tipoVariable}>", valores));
                    Console.WriteLine($"Slice '{nombreVariable}' declarado con valores: [{string.Join(", ", valores)}]");
                    return true;

                }else if (signo == "="){
                    if (entorno.buscarVariable(nombreVariable) == null){
                        erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreVariable}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                    var sliceExistente = entorno.variables[nombreVariable];
                    if (sliceExistente.tipo != $"slice<{tipoVariable}>"){
                        erroresSemanticos.Add(new Errores("Semantico", $"Error: No se puede asignar valores de tipo 'slice<{tipoVariable}>' a un slice de tipo '{sliceExistente.tipo}'.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                    entorno.actualizarValorSimbolo(nombreVariable, valores, DeterminarTipo, erroresSemanticos);
                    Console.WriteLine($"Slice '{nombreVariable}' actualizado con valores: [{string.Join(", ", valores)}]");
                    return true;

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en declaracion de slice con valores: {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

            return true;

        }

        private Object ObtenerValorPorDefecto(string tipo){
            return tipo switch{
                "int" => 0,
                "float64" => 0.0,
                "string" => "",
                "bool" => false,
                "rune" => '\0',
                _ => throw new Exception($"Tipo desconocido: {tipo}")
            };

        }

        /* ASIGNACION */
        public override Object VisitAsignacionVar([NotNull] AnalizadorLexicoParser.AsignacionVarContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreVariable = context.ID().GetText();
                string signo = context.GetChild(1).GetText();
                Object valor = Visit(context.expr());

                if (signo == ":="){
                    if (entorno.variables.ContainsKey(nombreVariable)){
                        erroresSemanticos.Add(new Errores("Semantico", $"Error: La variable '{nombreVariable}' ya ha sido declarada.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                    entorno.guardarVariable(nombreVariable, new Simbolos(nombreVariable, DeterminarTipo(valor), valor));

                }else if (signo == "="){
                    if (entorno.buscarVariable(nombreVariable) == null){
                        erroresSemanticos.Add(new Errores("Semantico", $"Error: La variable '{nombreVariable}' no ha sido declarada.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                    entorno.actualizarValorSimbolo(nombreVariable, valor, DeterminarTipo, erroresSemanticos);
                    return true;

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", e.Message, context.Start.Line, context.Start.Column));
                return false;

            }

            return true;

        }

        public override Object VisitAsignacionSlice([NotNull] AnalizadorLexicoParser.AsignacionSliceContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreDestino = context.ID(0).GetText();
                string nombreOrigen = context.ID(1).GetText();
                if (entorno.buscarVariable(nombreDestino) == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreDestino}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                    return false;

                }

                if (entorno.buscarVariable(nombreOrigen) == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreOrigen}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                    return false;

                }

                var sliceOrigen = entorno.variables[nombreOrigen];
                var sliceDestino = entorno.variables[nombreDestino];
                if (sliceOrigen.tipo != sliceDestino.tipo){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: No se puede asignar un slice de tipo '{sliceOrigen.tipo}' a un slice de tipo '{sliceDestino.tipo}'.", context.Start.Line, context.Start.Column));
                    return false;

                }

                List<object> copiaValores = new List<object>((List<object>)sliceOrigen.valor);
                entorno.actualizarValorSimbolo(nombreDestino, copiaValores, DeterminarTipo, erroresSemanticos);
                Console.WriteLine($"Slice '{nombreDestino}' ahora contiene los valores de '{nombreOrigen}': [{string.Join(", ", copiaValores)}]");
                return true;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en asignacion de slice: {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

        }

        public override Object VisitAsignacionIncremento([NotNull] AnalizadorLexicoParser.AsignacionIncrementoContext context){
            Entorno entorno = pilaEntornos.Peek();
            string nombre = context.ID().GetText();
            string operador = context.GetChild(1).GetText();
            object valor = Visit(context.expr());
            Simbolos? simbolo = entorno.buscarVariable(nombre);
            if (simbolo == null){
                erroresSemanticos.Add(new Errores("Semantico", $"La variable '{nombre}' no ha sido declarada.", context.Start.Line, context.Start.Column));
                return false;

            }

            if (valor == null){
                erroresSemanticos.Add(new Errores("Semantico", $"No se puede aplicar el operador '{operador}' con un valor nulo.", context.Start.Line, context.Start.Column));
                return false;

            }

            string tipoVar = simbolo.tipo;
            object valorActual = simbolo.valor;
            try{
                if (tipoVar == "int"){
                    if (valor is int val){
                        simbolo.valor = operador == "+=" ? (int)valorActual + val : (int)valorActual - val;

                    }else{
                        erroresSemanticos.Add(new Errores("Semantico", $"No se puede asignar un valor de tipo '{DeterminarTipo(valor)}' a una variable de tipo int con '{operador}'.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                }else if (tipoVar == "float64"){
                    double actual = valorActual is int i ? i : (double)valorActual;
                    double val = valor is int i2 ? i2 : (double)valor;
                    simbolo.valor = operador == "+=" ? actual + val : actual - val;

                }else if (tipoVar == "string"){
                    if (valor is string strVal && operador == "+="){
                        simbolo.valor = (string)valorActual + strVal;

                    }else{
                        erroresSemanticos.Add(new Errores("Semantico", $"El operador '{operador}' solo puede aplicarse a cadenas con concatenación.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                }else{
                    erroresSemanticos.Add(new Errores("Semantico", $"El operador '{operador}' no es válido para variables de tipo '{tipoVar}'.", context.Start.Line, context.Start.Column));
                    return false;

                }

                entorno.actualizarValorSimbolo(nombre, simbolo.valor, DeterminarTipo, erroresSemanticos);

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error al aplicar operador '{operador}': {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

            return true;

        }

        public override Object VisitSliceUpdate([NotNull] AnalizadorLexicoParser.SliceUpdateContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreSlice = context.ID().GetText();
                object indexObj = Visit(context.expr(0));
                object nuevoValor = Visit(context.expr(1));
                if (indexObj is not int index){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El indice debe ser un entero, pero se obtuvo '{indexObj.GetType().Name}'.", context.Start.Line, context.Start.Column));
                    return false;

                }

                Simbolos? sliceSimbolo = entorno.buscarVariable(nombreSlice);
                if (sliceSimbolo == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreSlice}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                    return false;

                }

                if (!sliceSimbolo.tipo.StartsWith("slice<")){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: '{nombreSlice}' no es un slice válido.", context.Start.Line, context.Start.Column));
                    return false;

                }

                string tipoSlice = sliceSimbolo.tipo.Substring(6, sliceSimbolo.tipo.Length - 7);
                List<object> listaValores = (List<object>)sliceSimbolo.valor;
                if (index < 0 || index >= listaValores.Count){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: Índice fuera de rango. El slice '{nombreSlice}' tiene {listaValores.Count} elementos.", context.Start.Line, context.Start.Column));
                    return false;

                }

                string tipoNuevoValor = DeterminarTipo(nuevoValor);
                if (tipoNuevoValor != tipoSlice){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: No se puede asignar un valor de tipo '{tipoNuevoValor}' a un slice de tipo '{tipoSlice}'.", context.Start.Line, context.Start.Column));
                    return false;

                }

                listaValores[index] = nuevoValor;
                Console.WriteLine($"Slice '{nombreSlice}' modificado en la posición {index}: [{string.Join(", ", listaValores)}]");
                return true;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en modificación de slice: {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

        }

        public override Object VisitAppend([NotNull] AnalizadorLexicoParser.AppendContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreDestino = context.ID(0).GetText();
                Simbolos? destino = entorno.buscarVariable(nombreDestino);
                if (destino == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: La variable '{nombreDestino}' no ha sido declarada.", context.Start.Line, context.Start.Column));
                    return false;

                }

                string tipoDestino = destino.tipo;
                if (tipoDestino.StartsWith("slice<")){
                    string tipoEsperado = tipoDestino.Substring(6, tipoDestino.Length - 7);
                    if (destino.valor is not List<object> slice){
                        erroresSemanticos.Add(new Errores("Semantico", $"Error interno: El valor de '{nombreDestino}' no es un slice valido.", context.Start.Line, context.Start.Column));
                        return false;

                    }

                    foreach (var expr in context.expr()){
                        object nuevoValor = Visit(expr);
                        string tipoValor = DeterminarTipo(nuevoValor);
                        if (tipoValor != tipoEsperado){
                            erroresSemanticos.Add(new Errores("Semantico", $"Error: No se puede agregar un valor de tipo '{tipoValor}' a un slice de tipo '{tipoEsperado}'.", context.Start.Line, context.Start.Column));
                            return false;

                        }

                        slice.Add(nuevoValor);

                    }

                    return true;

                }else{
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: append solo se puede usar con slices. '{nombreDestino}' es de tipo '{tipoDestino}'.", context.Start.Line, context.Start.Column));
                    return false;

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en append(): {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

        }

        public override Object VisitIncrementoDecremento([NotNull] AnalizadorLexicoParser.IncrementoDecrementoContext context){
            Entorno entorno = pilaEntornos.Peek();
            string nombre = context.ID().GetText();
            Simbolos? simbolo = entorno.buscarVariable(nombre);
            if (simbolo == null){
                erroresSemanticos.Add(new Errores("Semantico", $"La variable '{nombre}' no ha sido declarada.", context.Start.Line, context.Start.Column));
                return false;

            }

            if (simbolo.tipo != "int" && simbolo.tipo != "float64"){
                erroresSemanticos.Add(new Errores("Semantico", $"La operación de incremento/decremento solo es válida para variables de tipo int o float64.", context.Start.Line, context.Start.Column));
                return false;

            }

            if (simbolo.valor is int intVal){
                simbolo.valor = context.INCRE() != null ? intVal + 1 : intVal - 1;

            }else if (simbolo.valor is double floatVal){
                simbolo.valor = context.INCRE() != null ? floatVal + 1 : floatVal - 1;

            }

            entorno.actualizarValorSimbolo(nombre, simbolo.valor, DeterminarTipo, erroresSemanticos);
            return true;

        }

        private string DeterminarTipo(Object valor){
            try{
                if (valor is List<object> sliceValores){
                    if (sliceValores.Count > 0) {
                        return $"slice<{DeterminarTipo(sliceValores[0])}>";

                    }

                    return "slice<desconocido>";

                }

                return valor switch{
                    int => "int",
                    double => "float64",
                    string => "string",
                    bool => "bool",
                    char => "rune",
                    _ => throw new Exception($"Tipo desconocido para el valor: {valor}")
                };

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Tipo desconocido para el valor: {valor}", 0, 0));
                return null;
            
            }

        }

        private void ValidarTipo(string nombreVariable, string tipoEsperado, Object valor){
            try{
                string tipoValor = DeterminarTipo(valor);
                if (tipoEsperado == "float64" && tipoValor == "int") return;
                if (tipoEsperado != tipoValor){
                    erroresSemanticos.Add(new Errores("Semantico", $"Tipo incorrecto para {nombreVariable}. Se esperaba {tipoEsperado} pero se recibio {tipoValor}.", 0, 0));

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error al validar tipo: {e.Message}", 0, 0));

            }

        }

        /* INSTRUCCION IF */
        public override Object VisitInstruccion_if([NotNull] AnalizadorLexicoParser.Instruccion_ifContext context){
            try{
                object condicion = Visit(context.expr());
                if (condicion is not bool){
                    erroresSemanticos.Add(new Errores("Semantico", $"La condicion del if debe ser booleana, pero se recibio {condicion.GetType().Name}.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if ((bool)condicion){
                    Entorno entorno = new Entorno("If", pilaEntornos.Peek());
                    pilaEntornos.Peek().punteroASiguiente = entorno;
                    pilaEntornos.Push(entorno);
                    var resultado = VisitListaInstr(context.listaInstr());
                    pilaEntornos.Pop();
                    pilaEntornos.Peek().punteroASiguiente = null;  
                    return resultado;

                }

                foreach (var elseIf in context.instruccion_ifelse()){
                    object condicionElseIf = Visit(elseIf.expr());
                    if (condicionElseIf is not bool){
                        erroresSemanticos.Add(new Errores("Semantico", $"La condicion del else if debe ser booleana, pero se recibio {condicionElseIf.GetType().Name}.", elseIf.Start.Line, elseIf.Start.Column));
                        return null;

                    }

                    if ((bool)condicionElseIf){
                        Entorno entornoElseIf = new Entorno("ElseIf", pilaEntornos.Peek());
                        pilaEntornos.Peek().punteroASiguiente = entornoElseIf;
                        pilaEntornos.Push(entornoElseIf);
                        var resultado = VisitListaInstr(elseIf.listaInstr());                    
                        pilaEntornos.Pop();
                        pilaEntornos.Peek().punteroASiguiente = null;
                        return resultado;

                    }

                }

                if (context.instruccion_else() != null){
                    Entorno entornoElse = new Entorno("Else", pilaEntornos.Peek());
                    pilaEntornos.Peek().punteroASiguiente = entornoElse;
                    pilaEntornos.Push(entornoElse);
                    var resultado = VisitListaInstr(context.instruccion_else().listaInstr());                    
                    pilaEntornos.Pop();
                    pilaEntornos.Peek().punteroASiguiente = null;
                    return resultado;

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en la instruccion if: {e.Message}", context.Start.Line, context.Start.Column));

            }

            return null;

        }

        /* INSTRUCCION SWITCH */
        public override Object VisitInstruccion_switch([NotNull] AnalizadorLexicoParser.Instruccion_switchContext context){
            try {
                if (context.expr() == null){
                    erroresSemanticos.Add(new Errores("Semantico", "Error: El switch debe tener una expresion para evaluar.", context.Start.Line, context.Start.Column));
                    return null;

                }

                object valorSwitch = Visit(context.expr());
                bool casoEjecutado = false;
                var cases = context.lista_case()?.instruccion_case();
                if (cases != null){
                    foreach (var caseContext in cases){
                        object valorCase = Visit(caseContext.expr());
                        if (valorSwitch.Equals(valorCase)){
                            Entorno entornoCase = new Entorno("Case", pilaEntornos.Peek());
                            pilaEntornos.Peek().punteroASiguiente = entornoCase;
                            pilaEntornos.Push(entornoCase);
                            var resultado = VisitListaInstr(caseContext.listaInstr());
                            pilaEntornos.Pop();
                            pilaEntornos.Peek().punteroASiguiente = null;
                            if (resultado == Break.Instance){
                                return null;
                            
                            }else if (resultado == Continue.Instance){
                                erroresSemanticos.Add(new Errores("Semantico", "La sentencia 'continue' no puede usarse dentro de un switch.", context.Start.Line, context.Start.Column));
                                return null;

                            }

                            casoEjecutado = true;
                            break;

                        }

                    }

                }

                if (!casoEjecutado && context.instruccion_default() != null){
                    Entorno entornoDefault = new Entorno("Default", pilaEntornos.Peek());
                    pilaEntornos.Peek().punteroASiguiente = entornoDefault;
                    pilaEntornos.Push(entornoDefault);
                    var resultado = VisitListaInstr(context.instruccion_default().listaInstr());
                    pilaEntornos.Pop();
                    pilaEntornos.Peek().punteroASiguiente = null;
                    if (resultado == Break.Instance){
                        return null;

                    }else if (resultado == Continue.Instance){
                        erroresSemanticos.Add(new Errores("Semantico", "La sentencia 'continue' no puede usarse dentro de un switch.", context.Start.Line, context.Start.Column));
                        return null;

                    }

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en la instruccion switch: {e.Message}", context.Start.Line, context.Start.Column));

            }

            return null;

        }

        /* INSTRUCCION FOR */
        public override Object VisitForNormal([NotNull] AnalizadorLexicoParser.ForNormalContext context){
            try{
                Entorno entornoFor = new Entorno("For", pilaEntornos.Peek());
                pilaEntornos.Peek().punteroASiguiente = entornoFor;
                pilaEntornos.Push(entornoFor);
                while (true){
                    object condicion = Visit(context.expr());
                    if (condicion == null){
                        erroresSemanticos.Add(new Errores("Semantico", "La condición del bucle for es inválida (nil).", context.Start.Line, context.Start.Column));
                        break;

                    }

                    if (condicion is not bool){
                        erroresSemanticos.Add(new Errores("Semantico", $"La condición del bucle for debe ser booleana. Se recibió: {condicion.GetType().Name}.", context.Start.Line, context.Start.Column));
                        break;

                    }

                    if (!(bool)condicion){
                        break;

                    }

                    
                    var resultado = VisitListaInstr(context.listaInstr());
                    if (resultado is Break){
                        break;

                    }else if (resultado is Continue){
                        continue;

                    }

                }

                pilaEntornos.Pop();
                pilaEntornos.Peek().punteroASiguiente = null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error durante la ejecución del bucle for: {e.Message}", context.Start.Line, context.Start.Column));

            }

            return null;

        }

        public override Object VisitForIncremento([NotNull] AnalizadorLexicoParser.ForIncrementoContext context){
            try{
                Entorno entornoFor = new Entorno("ForIncremento", pilaEntornos.Peek());
                pilaEntornos.Peek().punteroASiguiente = entornoFor;
                pilaEntornos.Push(entornoFor);
                Visit(context.asignacion(0));
                while (true){
                    object condicion = Visit(context.expr());
                    if (condicion == null){
                        erroresSemanticos.Add(new Errores("Semantico", "La condicion del bucle for (incremento) es invalida (nil).", context.Start.Line, context.Start.Column));
                        break;

                    }

                    if (condicion is not bool){
                        erroresSemanticos.Add(new Errores("Semantico", $"La condicion del bucle for debe ser booleana. Se recibio: {condicion.GetType().Name}.", context.Start.Line, context.Start.Column));
                        break;

                    }

                    if (!(bool)condicion){
                        break;

                    }

                    var resultado = VisitListaInstr(context.listaInstr());
                    if (resultado is Break){
                        break;

                    }else if (resultado is Continue){
                        Visit(context.asignacion(1));
                        continue;

                    }

                    Visit(context.asignacion(1));

                }

                pilaEntornos.Pop();
                pilaEntornos.Peek().punteroASiguiente = null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error durante la ejecucion del bucle for con incremento: {e.Message}", context.Start.Line, context.Start.Column));
            
            }

            return null;

        }

        public override Object VisitForRange([NotNull] AnalizadorLexicoParser.ForRangeContext context){
            try{
                Entorno entornoActual = pilaEntornos.Peek();
                string idIndice = context.ID(0).GetText();
                string idValor = context.ID(1).GetText();
                string idSlice = context.ID(2).GetText();
                Simbolos? sliceSimbolo = entornoActual.buscarVariable(idSlice);
                if (sliceSimbolo == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"El slice '{idSlice}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (!sliceSimbolo.tipo.StartsWith("slice<")){
                    erroresSemanticos.Add(new Errores("Semantico", $"'{idSlice}' no es un slice valido.", context.Start.Line, context.Start.Column));
                    return null;

                }

                List<object> listaValores = (List<object>)sliceSimbolo.valor;
                string tipoElemento = sliceSimbolo.tipo.Substring(6, sliceSimbolo.tipo.Length - 7);
                Entorno entornoIteracion = new Entorno("ForRange", pilaEntornos.Peek());
                pilaEntornos.Peek().punteroASiguiente = entornoIteracion;
                pilaEntornos.Push(entornoIteracion);
                for (int i = 0; i < listaValores.Count; i++){
                    if (entornoIteracion.buscarVariable(idIndice) == null){
                        entornoIteracion.guardarVariable(idIndice, new Simbolos(idIndice, "int", i));

                    }else{
                        entornoIteracion.actualizarValorSimbolo(idIndice, i, DeterminarTipo, erroresSemanticos);

                    }

                    if (entornoIteracion.buscarVariable(idValor) == null){
                        entornoIteracion.guardarVariable(idValor, new Simbolos(idValor, tipoElemento, listaValores[i]));

                    }else{
                        entornoIteracion.actualizarValorSimbolo(idValor, listaValores[i], DeterminarTipo, erroresSemanticos);

                    }

                    var resultado = VisitListaInstr(context.listaInstr());
                    if (resultado is Break){
                        break;

                    }else if (resultado is Continue){
                        continue;

                    }

                }

                pilaEntornos.Pop();
                pilaEntornos.Peek().punteroASiguiente = null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error durante la ejecucion del bucle for-range: {e.Message}", context.Start.Line, context.Start.Column));

            }

            return null;

        }

        /* INSTRUCCION FUNCION */
        public override Object VisitInstruccion_funcion([NotNull] AnalizadorLexicoParser.Instruccion_funcionContext context){
            string nombreFuncion = context.ID().GetText();
            Entorno entorno = pilaEntornos.Peek();
            Entorno entornoGlobal = pilaEntornos.Last();
            if (entorno != entornoGlobal){
                erroresSemanticos.Add(new Errores("Semantico", $"Las funciones solo pueden declararse en el ámbito global.", context.Start.Line, context.Start.Column));
                return null;

            }

            if (entorno.buscarVariable(nombreFuncion) != null){
                erroresSemanticos.Add(new Errores("Semantico", $"El nombre '{nombreFuncion}' ya fue usado como variable o función.", context.Start.Line, context.Start.Column));
                return null;

            }

            Dictionary<string, Simbolos> parametros = new Dictionary<string, Simbolos>();
            List<string> tipoParametros = new List<string>();
            if (context.lista_parametros() != null){
                var exprs = context.lista_parametros().expr();
                var tipos = context.lista_parametros().tipo();

                for (int i = 0; i < exprs.Length; i++){
                    string nombreParametro = exprs[i].GetText();
                    string tipoParametro = tipos[i].GetText();
                    if (string.IsNullOrWhiteSpace(tipoParametro)){
                        erroresSemanticos.Add(new Errores("Semantico", $"El parámetro '{nombreParametro}' debe tener tipo explícito.", context.Start.Line, context.Start.Column));
                        return null;

                    }

                    if (parametros.ContainsKey(nombreParametro)){
                        erroresSemanticos.Add(new Errores("Semantico", $"El parámetro '{nombreParametro}' está duplicado en la función '{nombreFuncion}'.", context.Start.Line, context.Start.Column));
                        return null;

                    }

                    parametros.Add(nombreParametro, new Simbolos(nombreParametro, tipoParametro, null));
                    tipoParametros.Add(tipoParametro);

                }

            }

            string tipoRetorno = context.tipo()?.GetText() ?? "void";
            Simbolos simboloRetorno = new Simbolos("retorno_" + nombreFuncion, tipoRetorno, null);
            parametros.Add(simboloRetorno.identificador, simboloRetorno);
            Funcion funcion = new Funcion(nombreFuncion, parametros, context.listaInstr(), tipoParametros, simboloRetorno);
            entorno.guardarVariable(nombreFuncion, new Simbolos(nombreFuncion, "Funcion", funcion));
            Console.WriteLine($"Función '{nombreFuncion}' registrada correctamente.");
            return true;

        }

        public override Object VisitLlamada_funcion([NotNull] AnalizadorLexicoParser.Llamada_funcionContext context){
            string nombreFuncion = context.ID().GetText();
            Entorno entornoGlobal = pilaEntornos.Last();
            Simbolos? simbolo = entornoGlobal.variables.ContainsKey(nombreFuncion) ? entornoGlobal.variables[nombreFuncion] : null;

            if (simbolo != null && simbolo.valor is Funcion funcion){
                Entorno entornoFuncion = new Entorno("Funcion_" + nombreFuncion, entornoGlobal);
                int cantidadParametrosEsperada = funcion.parametros.Count - 1;
                int cantidadParametrosRecibida = 0;
                if (context.lista_argumentos() != null){
                    cantidadParametrosRecibida = context.lista_argumentos().expr().Length;

                }

                if (cantidadParametrosRecibida != cantidadParametrosEsperada){
                    erroresSemanticos.Add(new Errores("Semantico", $"Cantidad de parámetros incorrecta para la función '{nombreFuncion}'", context.Start.Line, context.Start.Column));
                    return null;

                }

                int index = 0;
                foreach (var kvp in funcion.parametros){
                    if (kvp.Key == funcion.retorno.identificador){
                        continue;

                    }

                    object valor = context.lista_argumentos() != null ? Visit(context.lista_argumentos().expr()[index]) : null;
                    string tipoValor = DeterminarTipo(valor);
                    string tipoEsperado = kvp.Value.tipo;
                    if (tipoValor != tipoEsperado){
                        erroresSemanticos.Add(new Errores("Semantico", $"El parámetro '{kvp.Key}' esperaba tipo '{tipoEsperado}' pero se recibió '{tipoValor}'", context.Start.Line, context.Start.Column));
                        return null;

                    }

                    object valorAGuardar = valor;
                    if (tipoEsperado.StartsWith("slice<")){
                        valorAGuardar = valor;

                    }else{
                        if (valor is int i) valorAGuardar = i;
                        else if (valor is double d) valorAGuardar = d;
                        else if (valor is string s) valorAGuardar = string.Copy(s);
                        else if (valor is bool b) valorAGuardar = b;
                        else valorAGuardar = valor;

                    }

                    entornoFuncion.guardarVariable(kvp.Key, new Simbolos(kvp.Key, tipoEsperado, valorAGuardar));
                    index++;

                }

                var nuevoRetorno = new Simbolos(funcion.retorno.identificador, funcion.retorno.tipo, null);
                entornoFuncion.guardarVariable(nuevoRetorno.identificador, nuevoRetorno);
                pilaEntornos.Push(entornoFuncion);
                var resultado = VisitListaInstr(funcion.instrucciones);
                pilaEntornos.Pop();
                pilaEntornos.Peek().punteroASiguiente = null;
                if (resultado is Return ret){
                    return ret.Valor;

                }

                if (funcion.retorno.tipo != "void"){
                    var simboloReturn = entornoFuncion.variables[nuevoRetorno.identificador];
                    if (simboloReturn.valor == null){
                        erroresSemanticos.Add(new Errores("Semantico", $"La función '{nombreFuncion}' debe retornar un valor de tipo '{funcion.retorno.tipo}'.", context.Start.Line, context.Start.Column));
                        return null;

                    }

                    return simboloReturn.valor;

                }

                return true;

            }else{
                erroresSemanticos.Add(new Errores("Semantico", $"La función '{nombreFuncion}' no existe en el entorno global.", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        /* SENTENCIAS DE TRANSFERENCIA */
        public override Object VisitBreakInstr([NotNull] AnalizadorLexicoParser.BreakInstrContext context){
            bool dentroDeBloqueValido = false;
            foreach (Entorno entorno in pilaEntornos){
                if (entorno.nombre.StartsWith("For") || entorno.nombre == "Switch" || entorno.nombre.StartsWith("Case") || entorno.nombre == "Default"){
                    dentroDeBloqueValido = true;
                    break;

                }

            }

            if (!dentroDeBloqueValido){
                erroresSemanticos.Add(new Errores("Semantico","La sentencia 'break' solo puede usarse dentro de un bucle for o un switch.", context.Start.Line,context.Start.Column));
                return null;

            }

            return Break.Instance;

        }

        public override Object VisitContinueInstr([NotNull] AnalizadorLexicoParser.ContinueInstrContext context){
            bool dentroDeFor = pilaEntornos.Any(e => e.nombre.StartsWith("For"));

            if (!dentroDeFor){
                erroresSemanticos.Add(new Errores(
                    "Semantico",
                    "La sentencia 'continue' solo puede usarse dentro de un bucle for.",
                    context.Start.Line,
                    context.Start.Column
                ));
                return null;

            }

            return Continue.Instance;

        }

        public override Object VisitReturnInstr([NotNull] AnalizadorLexicoParser.ReturnInstrContext context){
            object? valorRetorno = null;
            string tipoValor = "void";
            if (context.expr() != null){
                valorRetorno = Visit(context.expr());
                tipoValor = DeterminarTipo(valorRetorno);

            }

            for (Entorno? ent = pilaEntornos.Peek(); ent != null; ent = ent.punteroAPadre){
                foreach (var variable in ent.variables){
                    if (variable.Key.StartsWith("retorno_")){
                        Simbolos simboloRetorno = variable.Value;
                        string tipoEsperado = simboloRetorno.tipo;
                        if (tipoEsperado == "void" && valorRetorno != null){
                            erroresSemanticos.Add(new Errores("Semantico","No se puede retornar un valor en una función void.",context.Start.Line, context.Start.Column));
                            return null;

                        }

                        if (tipoEsperado != "void" && context.expr() == null){
                            erroresSemanticos.Add(new Errores("Semantico", $"La función debe retornar un valor de tipo '{tipoEsperado}', pero se usó 'return;' vacío.", context.Start.Line, context.Start.Column));
                            return null;

                        }

                        if (tipoEsperado != "void" && tipoValor != tipoEsperado){
                            erroresSemanticos.Add(new Errores("Semantico", $"El tipo de retorno es '{tipoValor}', pero se esperaba '{tipoEsperado}'.", context.Start.Line, context.Start.Column));
                            return null;

                        }

                        if (tipoEsperado != "void"){
                            simboloRetorno.valor = valorRetorno;
                            ent.variables[variable.Key] = simboloRetorno;
                            Console.WriteLine("DEBUG: Return enviado con valor " + valorRetorno);
                            return new Return(valorRetorno);

                        }else{
                            Console.WriteLine("DEBUG: Return vacío en función void");
                            return new Return(null);

                        }

                    }

                }

            }

            erroresSemanticos.Add(new Errores("Semantico", $"Instrucción 'return' usada fuera de una función.", context.Start.Line, context.Start.Column));
            return null;

        }

        /* EXPR */
        public override Object VisitExpreParentesis(AnalizadorLexicoParser.ExpreParentesisContext context){
            try{
                //Console.WriteLine("Encontro expresion en parentesis");
                return Visit(context.expr());

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en expresion entre parentesis: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitIntExpresion([NotNull] AnalizadorLexicoParser.IntExpresionContext context){

            try{
                //Console.WriteLine(context.GetText());
                return int.Parse(context.INT().GetText()); 

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error al interpretar numero entero: {context.GetText()} - {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitDecimalExpresion([NotNull] AnalizadorLexicoParser.DecimalExpresionContext context){
            try{
                //Console.WriteLine(context.GetText());
                return double.Parse(context.DECIMAL().GetText());

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error al interpretar numero decimal: {context.GetText()} - {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitCaracterExpresion([NotNull] AnalizadorLexicoParser.CaracterExpresionContext context){
            string texto = context.GetText();

            if (texto.Length < 3 || !texto.StartsWith("'") || !texto.EndsWith("'")){
                erroresSemanticos.Add(new Errores("Semantico", $"Caracter mal formado: {texto}", context.Start.Line, context.Start.Column));
                return null;

            }

            if (texto.Length == 3){
                return texto[1];

            }

            if (texto.Length == 4 && texto[1] == '\\'){
                return texto[2] switch{
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\'' => '\'',
                    '\\' => '\\',
                    _ => throw new Exception($"Error: Secuencia de escape desconocida {texto}")
                };

            }

            erroresSemanticos.Add(new Errores("Semantico", $"Caracter invalido: {texto}", context.Start.Line, context.Start.Column));
            return null;

        }

        public override Object VisitCadenaExpresion([NotNull] AnalizadorLexicoParser.CadenaExpresionContext context){
            string cadena = context.GetText();

            if (cadena.Length < 2 || (!cadena.StartsWith("\"") && !cadena.StartsWith("'")) || (!cadena.EndsWith("\"") && !cadena.EndsWith("'"))){
                erroresSemanticos.Add(new Errores("Semantico", "Cadena mal formada: " + cadena, context.Start.Line, context.Start.Column));
                return null;

            }

            string contenido = cadena.Substring(1, cadena.Length - 2);

            try{
                string procesada = contenido
                    .Replace("\\n", "\n")
                    .Replace("\\t", "\t")
                    .Replace("\\r", "\r")
                    .Replace("\\\"", "\"")
                    .Replace("\\\\", "\\");
                return procesada;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error procesando cadena: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitBooleanExpresion([NotNull] AnalizadorLexicoParser.BooleanExpresionContext context){
            if (context.BOOL().GetText() == "true"){
                return true;

            }else if (context.BOOL().GetText() == "false"){
                return false;

            }

            erroresSemanticos.Add(new Errores("Semantico", $"Error al procesar el valor booleano '{context.BOOL().GetText()}'.", context.Start.Line, context.Start.Column));
            return null;

        }

        public override Object VisitIdExpresion([NotNull] AnalizadorLexicoParser.IdExpresionContext context){
            Entorno entorno = pilaEntornos.Peek();
            Simbolos? simbolo = entorno.buscarVariable(context.ID().GetText());
            //Console.WriteLine("Variable: " + context.ID().GetText());
            if(simbolo != null){
                return simbolo.valor;

            }else{
                erroresSemanticos.Add(new Errores("Semantico", $"La variable '{context.ID().GetText()}' no existe.", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitNilExpresion([NotNull] AnalizadorLexicoParser.NilExpresionContext context){
            return null;

        }

        public override Object VisitOperadorNegacion([NotNull] AnalizadorLexicoParser.OperadorNegacionContext context){
            try{
                Object valor = Visit(context.right);

                if (valor == null){
                    erroresSemanticos.Add(new Errores("Semantico", "No se puede realizar la negacion a un nil.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (valor is bool){
                    return !(bool)valor;

                }

                erroresSemanticos.Add(new Errores("Semantico", $"El operador '!' solo se aplica a valores booleanos. Tipo recibido: {valor.GetType().Name}.", context.Start.Line, context.Start.Column));
                return null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en operador negacion: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitOperadorNegativo([NotNull] AnalizadorLexicoParser.OperadorNegativoContext context){
            try{
                Object valor = Visit(context.right);

                if (valor == null){
                    erroresSemanticos.Add(new Errores("Semantico", "No se puede realizar la negacion a un nil.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (valor is int){
                    return -(int)valor;

                }else if (valor is double){
                    return -(double)valor;

                }

                erroresSemanticos.Add(new Errores("Semantico", $"El operador negativo solo se aplica a tipos numericos. Tipo recibido: {valor.GetType().Name}.", context.Start.Line, context.Start.Column));
                return null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en operador negativo: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitMultiplicacionYdivision([NotNull] AnalizadorLexicoParser.MultiplicacionYdivisionContext context){
            try{
                string operador = context.GetChild(1).GetText();
                Object left = Visit(context.expr(0));
                Object right = Visit(context.expr(1));

                if (left == null || right == null){
                    erroresSemanticos.Add(new Errores("Semantico", "No se puede realizar operaciones aritmeticas con nil.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if ((operador == "/" || operador == "%") && (right is int && (int)right == 0)){
                    erroresSemanticos.Add(new Errores("Semantico", "Division o modulo por cero no permitido.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (left is int && right is int){
                    if (operador == "*") return (int)left * (int)right;
                    if (operador == "/") return (int)left / (int)right;
                    if (operador == "%") return (int)left % (int)right;

                }

                if ((left is int || left is double) && (right is int || right is double)){
                    double leftVal = left is int ? Convert.ToDouble(left) : (double)left;
                    double rightVal = right is int ? Convert.ToDouble(right) : (double)right;
                    if (operador == "*") return leftVal * rightVal;
                    if (operador == "/") return leftVal / rightVal;

                }

                erroresSemanticos.Add(new Errores("Semantico", $"Operador inesperado {operador}.", context.Start.Line, context.Start.Column));
                return null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en operacion de multiplicacion o division: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitSumaYresta([NotNull] AnalizadorLexicoParser.SumaYrestaContext context){
            try{
                string operador = context.GetChild(1).GetText();
                Object left = Visit(context.expr(0));
                Object right = Visit(context.expr(1));

                if (left == null || right == null){
                    erroresSemanticos.Add(new Errores("Semantico", "No se puede realizar operaciones aritmeticas con nil.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (left is string leftStr && right is string rightStr){
                    if (operador == "+"){
                        return leftStr + rightStr;

                    }else{
                        erroresSemanticos.Add(new Errores("Semantico", "No se puede restar strings.", context.Start.Line, context.Start.Column));
                        return null;

                    }

                }

                if (left is int && right is int){
                    return operador == "+" ? (int)left + (int)right : (int)left - (int)right;

                }

                if ((left is int || left is double) && (right is int || right is double)){
                    double leftVal = left is int ? Convert.ToDouble(left) : (double)left;
                    double rightVal = right is int ? Convert.ToDouble(right) : (double)right;
                    return operador == "+" ? leftVal + rightVal : leftVal - rightVal;

                }

                erroresSemanticos.Add(new Errores("Semantico", $"No se puede realizar la operacion {left.GetType().Name} {operador} {right.GetType().Name}.", context.Start.Line, context.Start.Column));
                return null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en operacion aritmetica: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitOperadorLogico([NotNull] AnalizadorLexicoParser.OperadorLogicoContext context){
            try{
                string operador = context.GetChild(1).GetText();
                dynamic left = Visit(context.left);
                dynamic right = Visit(context.right);

                if (left == null || right == null){
                    erroresSemanticos.Add(new Errores("Semantico", "No se puede operar con valores nil.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (left is not bool || right is not bool){
                    erroresSemanticos.Add(new Errores("Semantico", $"Operacion logica invalida: {left.GetType().Name} {operador} {right.GetType().Name}. Se esperaban valores booleanos.", context.Start.Line, context.Start.Column));
                    return false;

                }

                return operador switch{
                    "&&" => left && right,
                    "||" => left || right,
                    _ => throw new Exception($"Operador logico desconocido: {operador}")
                };

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en operacion logica: {e.Message}", context.Start.Line, context.Start.Column));
                return false;

            }

        }

        public override Object VisitOperadorRelacional([NotNull] AnalizadorLexicoParser.OperadorRelacionalContext context){
            try{
                string operador = context.GetChild(1).GetText();
                dynamic left = Visit(context.left);
                dynamic right = Visit(context.right);

                if (left == null || right == null) {
                    erroresSemanticos.Add(new Errores("Semantico", "No se puede operar con valores nil.", context.Start.Line, context.Start.Column));
                    return null;
                    
                }

                if ((left is int || left is double) && (right is int || right is double)){
                    double leftVal = left is int ? Convert.ToDouble(left) : (double)left;
                    double rightVal = right is int ? Convert.ToDouble(right) : (double)right;
                    return operador switch{
                        "==" => leftVal == rightVal,
                        "!=" => leftVal != rightVal,
                        ">"  => leftVal > rightVal,
                        ">=" => leftVal >= rightVal,
                        "<"  => leftVal < rightVal,
                        "<=" => leftVal <= rightVal,
                        _ => throw new Exception($"Operador relacional desconocido: {operador}")
                    };

                }

                if (left.GetType() != right.GetType()){
                    erroresSemanticos.Add(new Errores("Semantico", $"No se puede comparar tipos diferentes: {left.GetType().Name} {operador} {right.GetType().Name}.", context.Start.Line, context.Start.Column));
                    return false;

                }

                return operador switch{
                    "==" => left == right,
                    "!=" => left != right,
                    ">"  => left > right,
                    ">=" => left >= right,
                    "<"  => left < right,
                    "<=" => left <= right,
                    _ => throw new Exception($"Operador relacional desconocido: {operador}")
                };

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en operacion relacional: {e.Message}", context.Start.Line, context.Start.Column));
                return false; 

            }

        }

        public override Object VisitSliceIndex([NotNull] AnalizadorLexicoParser.SliceIndexContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreSlice = context.expr(0).GetText();
                object valorBuscado = Visit(context.expr(1));
                Simbolos? slice = entorno.buscarVariable(nombreSlice);
                if (slice == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreSlice}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                    return -1;

                }

                if (!slice.tipo.StartsWith("slice<")){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: '{nombreSlice}' no es un slice valido.", context.Start.Line, context.Start.Column));
                    return -1;

                }

                List<object> listaValores = (List<object>)slice.valor;
                int index = listaValores.IndexOf(valorBuscado);
                Console.WriteLine($"Index encontrado: {index}");
                return index;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en slices.Index: {e.Message}", context.Start.Line, context.Start.Column));
                return -1;

            }

        }

        public override Object VisitStringsJoin([NotNull] AnalizadorLexicoParser.StringsJoinContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreSlice = context.expr().GetText();
                string separador = context.CADENA().GetText().Trim('"');
                Simbolos? slice = entorno.buscarVariable(nombreSlice);
                if (slice == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreSlice}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (slice.tipo != "slice<string>"){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: strings.Join solo se puede aplicar a slices de tipo string.", context.Start.Line, context.Start.Column));
                    return null;

                }

                List<object> listaStrings = (List<object>)slice.valor;
                List<string> valoresStr = listaStrings.Select(x => x.ToString()).ToList();
                string resultado = string.Join(separador, valoresStr);
                Console.WriteLine($"Resultado de strings.Join: {resultado}");
                return resultado;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en strings.Join: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }
        
        }

        public override Object VisitLen([NotNull] AnalizadorLexicoParser.LenContext context){
            try{
                object valor = Visit(context.expr());
                if (valor is List<object> lista){
                    int length = lista.Count;
                    return length;

                }

                string tipoValor = DeterminarTipo(valor);
                erroresSemanticos.Add(new Errores("Semantico", $"Error: La función 'len' solo se puede aplicar a slices o matrices. Se recibió tipo: {tipoValor}.", context.Start.Line, context.Start.Column));
                return null;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en len: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitSliceAccess([NotNull] AnalizadorLexicoParser.SliceAccessContext context){
            try{
                Entorno entorno = pilaEntornos.Peek();
                string nombreSlice = context.ID().GetText();
                object indexObj = Visit(context.expr());
                if (indexObj is not int index){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El índice debe ser un entero, pero se obtuvo '{indexObj.GetType().Name}'.", context.Start.Line, context.Start.Column));
                    return null;

                }

                Simbolos? sliceSimbolo = entorno.buscarVariable(nombreSlice);
                if (sliceSimbolo == null){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: El slice '{nombreSlice}' no ha sido declarado.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (!sliceSimbolo.tipo.StartsWith("slice<")){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: '{nombreSlice}' no es un slice válido.", context.Start.Line, context.Start.Column));
                    return null;

                }

                List<object> listaValores = (List<object>)sliceSimbolo.valor;
                if (index < 0 || index >= listaValores.Count){
                    erroresSemanticos.Add(new Errores("Semantico", $"Error: Índice fuera de rango. El slice '{nombreSlice}' tiene {listaValores.Count} elementos.", context.Start.Line, context.Start.Column));
                    return null;

                }

                return listaValores[index];

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en acceso a slice: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitStrconvAtoi([NotNull] AnalizadorLexicoParser.StrconvAtoiContext context){
            try{
                object valor = Visit(context.expr());
                if (valor == null){
                    erroresSemanticos.Add(new Errores("Semantico", "strconv.Atoi: no se puede convertir un valor nil a int.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (valor is not string strValor){
                    erroresSemanticos.Add(new Errores("Semantico", $"strconv.Atoi: se esperaba una cadena, pero se recibio {valor.GetType().Name}.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (strValor.Contains(".")){
                    erroresSemanticos.Add(new Errores("Semantico", $"strconv.Atoi: el valor '{strValor}' es float64, solo se puede con enteros.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (int.TryParse(strValor, out int resultado)){
                    return resultado;

                }else{
                    erroresSemanticos.Add(new Errores("Semantico", $"strconv.Atoi: el valor '{strValor}' no es un entero valido.", context.Start.Line, context.Start.Column));
                    return null;

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en strconv.Atoi: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitStrconvParseFloat([NotNull] AnalizadorLexicoParser.StrconvParseFloatContext context){
            try{
                object valor = Visit(context.expr());

                if (valor == null){
                    erroresSemanticos.Add(new Errores("Semantico", "strconv.ParseFloat: no se puede convertir un valor nil a float64.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (valor is not string strValor){
                    erroresSemanticos.Add(new Errores("Semantico", $"strconv.ParseFloat: se esperaba una cadena, pero se recibio {valor.GetType().Name}.", context.Start.Line, context.Start.Column));
                    return null;

                }

                if (double.TryParse(strValor, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double resultado)){
                    return resultado;

                }else{
                    erroresSemanticos.Add(new Errores("Semantico", $"strconv.ParseFloat: el valor '{strValor}' no es un numero decimal valido.", context.Start.Line, context.Start.Column));
                    return null;

                }

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en strconv.ParseFloat: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override Object VisitTypeOf([NotNull] AnalizadorLexicoParser.TypeOfContext context){
            try{
                object valor = Visit(context.expr());
                if (valor == null){
                    return "nil";

                }

                string tipo = DeterminarTipo(valor);
                return tipo;

            }catch (Exception e){
                erroresSemanticos.Add(new Errores("Semantico", $"Error en reflect.TypeOf: {e.Message}", context.Start.Line, context.Start.Column));
                return null;

            }

        }

        public override object VisitEjecutarFunciones([NotNull] AnalizadorLexicoParser.EjecutarFuncionesContext context){
            return VisitLlamada_funcion(context.llamada_funcion());

        }

        public List<object> ObtenerTablaDeSimbolos(){
            List<object> simbolos = new List<object>();
            foreach (var entorno in pilaEntornos){
                foreach (var entry in entorno.variables){
                    simbolos.Add(new{
                        identificador = entry.Key,
                        tipo = entry.Value.tipo,
                        valor = entry.Value.valor?.ToString() ?? "null",
                        entorno = entorno.nombre
                    });

                }

            }

            return simbolos;

        }

    }


}