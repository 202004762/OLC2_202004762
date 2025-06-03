using Antlr4.Runtime.Misc;
using Proyecto_1.AnalizadorLexico;
using System.Text;
using Proyecto_1.ExcepcionesARM64;


namespace Proyecto_1{
    public class CodigoARM64{
        public string Texto { get; set; } = "";
        public string RegistroResultado { get; set; } = "";
        public string Tipo { get; set; } = ""; // "int", "float64", "string", "error"
        public string MensajeLiteral { get; set; } = ""; // Para string, bool y chars
        public double? ResultadoFloat { get; set; } = null; // Para float evaluado
        public string EtiquetaMensaje { get; set; } = "";
        public string? EtiquetaLongitud { get; set; } = null; // para slices
        public string? RegistroResultadoExtra { get; set; } = null; // para strings (longitud)

    }

    public class VisitorARM64 : AnalizadorLexicoBaseVisitor<object>{
        private StringBuilder textSection = new StringBuilder();
        private StringBuilder dataSection = new StringBuilder();
        private int tmpCounter = 0;
        private int tmpFloatCounter = 0;
        private int contadorMsg = 0;
        private int contadorPrint = 0;
        private int contadorVar = 0;
        private int contadorEtiqueta = 0;
        private Stack<EntornoARM64> pilaEntornosARM64 = new Stack<EntornoARM64>();
        private Stack<(string etqContinue, string etqBreak)> pilaBucles = new();
        private StringBuilder funcSection = new StringBuilder();

        // --- Constructor ---
        public VisitorARM64(){
            pilaEntornosARM64.Push(new EntornoARM64());

        }

        private string NuevoRegistroTmp(){
            if (9 + tmpCounter > 30){
                throw new Exception("[Error interno: Se agotaron los registros temporales disponibles (x9-x30)]");

            }

            return $"x{9 + (tmpCounter++)}";

        }

        private string NuevoRegistroTmpFloat(){
            if (9 + tmpFloatCounter > 30){
                throw new Exception("[Error interno: Se agotaron los registros temporales float disponibles (d9-d30)]");

            }

            return $"d{9 + (tmpFloatCounter++)}";

        }

        private void ResetearTemporales(){
            tmpCounter = 0;
            tmpFloatCounter = 0;

        }

        private string NuevaEtiqueta(string prefijo){
            return $"{prefijo}_{contadorEtiqueta++}";

        }

        private string ARegW(string reg) {
            return reg.StartsWith("x") ? reg.Replace("x", "w") : reg;

        }

        private bool EsRegistro(string nombre){
            return nombre.StartsWith("x") || nombre.StartsWith("w");

        }

        /* INICIO */
        private string VisitListaInstrTexto(AnalizadorLexicoParser.ListaInstrContext context){
            string temp = textSection.ToString();
            textSection.Clear();

            Visit(context);

            string instrucciones = textSection.ToString();
            textSection.Clear();
            textSection.Append(temp);

            return instrucciones;

        }

        /* IMPRIMIR */
        public override object VisitPrint(AnalizadorLexicoParser.PrintContext context){
            foreach (var expr in context.expr()){
                var cod = Visit(expr) as CodigoARM64;
                if (cod == null) continue;
                textSection.Append(cod.Texto);

                if (cod.Tipo == "int"){
                    textSection.AppendLine($"    mov x0, {cod.RegistroResultado}");
                    textSection.AppendLine($"    bl print_int");
                    textSection.AppendLine($"    bl print_newline");

                }else if (cod.Tipo == "float64"){
                    if (cod.RegistroResultado != "d0"){
                        textSection.AppendLine($"    fmov d0, {cod.RegistroResultado}");

                    }

                    textSection.AppendLine($"    bl print_float");
                    textSection.AppendLine($"    bl print_newline");

                }else if (cod.Tipo == "string"){
                    if (cod.MensajeLiteral != null && cod.RegistroResultadoExtra != null) {
                        textSection.AppendLine($"    mov x0, 1");
                        textSection.AppendLine($"    mov x1, {cod.RegistroResultado}");       
                        textSection.AppendLine($"    mov x2, {cod.RegistroResultadoExtra}"); 
                    } else {
                        textSection.AppendLine($"    ldr x1, ={cod.RegistroResultado}_ptr");
                        textSection.AppendLine($"    ldr x1, [x1]");
                        textSection.AppendLine($"    ldr x2, ={cod.RegistroResultado}_len");
                        textSection.AppendLine($"    ldr x2, [x2]");
                        textSection.AppendLine($"    mov x0, 1");
                    }

                    textSection.AppendLine($"    bl print_string");
                    textSection.AppendLine($"    bl print_newline");

                }else if (cod.Tipo == "rune"){
                    string regW = cod.RegistroResultado.Replace("x", "w");
                    textSection.AppendLine($"    mov w0, {regW}");
                    textSection.AppendLine($"    bl print_rune");
                    textSection.AppendLine($"    bl print_newline");

                }else if (cod.Tipo == "bool"){
                    textSection.AppendLine($"    mov x0, {cod.RegistroResultado}");
                    textSection.AppendLine($"    bl print_bool");
                    textSection.AppendLine($"    bl print_newline");

                }else if (cod.Tipo.StartsWith("slice<")){
                    string tipoContenido = cod.Tipo.Substring(6, cod.Tipo.Length - 7);
                    string baseEtiqueta = cod.RegistroResultado;
                    string lenEtiqueta = baseEtiqueta.Replace("ptr", "len");
                    string regPtr = NuevoRegistroTmp();
                    string regLen = NuevoRegistroTmp();
                    string regIdx = NuevoRegistroTmp();
                    string regTmp = NuevoRegistroTmp();
                    string regAux = NuevoRegistroTmp();
                    string etqLoop = NuevaEtiqueta("loop_slice");
                    string etqFin = NuevaEtiqueta("fin_slice");

                    string msgAbre = $"msg_abre_{contadorMsg++}";
                    dataSection.AppendLine($"{msgAbre}: .asciz \"[\"");
                    textSection.AppendLine($"    mov x0, 1");
                    textSection.AppendLine($"    ldr x1, ={msgAbre}");
                    textSection.AppendLine($"    mov x2, 1");
                    textSection.AppendLine($"    mov x8, 64");
                    textSection.AppendLine($"    svc 0");

                    string regAuxPtr = NuevoRegistroTmp();
                    textSection.AppendLine($"    ldr {regAuxPtr}, ={baseEtiqueta}");
                    textSection.AppendLine($"    ldr {regPtr}, [{regAuxPtr}]");

                    string regAuxLen = NuevoRegistroTmp();
                    textSection.AppendLine($"    ldr {regAuxLen}, ={lenEtiqueta}");
                    textSection.AppendLine($"    ldr {regLen}, [{regAuxLen}]");

                    textSection.AppendLine($"    mov {regIdx}, #0");
                    textSection.AppendLine($"{etqLoop}:");
                    textSection.AppendLine($"    cmp {regIdx}, {regLen}");
                    textSection.AppendLine($"    bge {etqFin}");

                    textSection.AppendLine($"    mov {regTmp}, {regIdx}");
                    if (tipoContenido == "string"){
                        textSection.AppendLine($"    lsl {regTmp}, {regTmp}, #4");

                    }else{
                        textSection.AppendLine($"    lsl {regTmp}, {regTmp}, #3");

                    }

                    textSection.AppendLine($"    add {regTmp}, {regPtr}, {regTmp}");
                    if (tipoContenido == "int"){
                        string regVal = NuevoRegistroTmp();
                        textSection.AppendLine($"    ldr {regVal}, [{regTmp}]");
                        textSection.AppendLine($"    mov x0, {regVal}");
                        textSection.AppendLine($"    bl print_int");

                    }else if (tipoContenido == "float64"){
                        textSection.AppendLine($"    ldr d0, [{regTmp}]");
                        textSection.AppendLine($"    bl print_float");

                    }else if (tipoContenido == "string"){
                        string regStrPtr = NuevoRegistroTmp();
                        string regLenVal = NuevoRegistroTmp();
                        textSection.AppendLine($"    ldr {regStrPtr}, [{regTmp}]");
                        textSection.AppendLine($"    ldr {regLenVal}, [{regTmp}, #8]");
                        textSection.AppendLine($"    mov x0, 1");
                        textSection.AppendLine($"    mov x1, {regStrPtr}");
                        textSection.AppendLine($"    mov x2, {regLenVal}");
                        textSection.AppendLine($"    bl print_string");

                    }else if (tipoContenido == "rune"){
                        string regRune = NuevoRegistroTmp();
                        string regRuneW = regRune.Replace("x", "w");
                        string etiquetaTmp = NuevaEtiqueta("rune_tmp_char");
                        dataSection.AppendLine($"{etiquetaTmp}: .asciz \" \"");
                        textSection.AppendLine($"    ldr {regRune}, [{regTmp}]");
                        textSection.AppendLine($"    ldr x1, ={etiquetaTmp}");
                        textSection.AppendLine($"    strb {regRuneW}, [x1]");
                        textSection.AppendLine($"    mov x0, 1");
                        textSection.AppendLine($"    ldr x1, ={etiquetaTmp}");
                        textSection.AppendLine($"    mov x2, 1");
                        textSection.AppendLine($"    mov x8, 64");
                        textSection.AppendLine($"    svc 0");

                    }else if (tipoContenido == "bool"){
                        string regVal = NuevoRegistroTmp();
                        string etqTrue = $"boolTrue_{contadorMsg++}";
                        string etqFalse = $"boolFalse_{contadorMsg++}";
                        string etqEnd = $"boolEnd_{contadorMsg++}";

                        if (!dataSection.ToString().Contains("msgTrue:")){
                            dataSection.AppendLine("msgTrue: .asciz \"True\"");
                        }

                        if (!dataSection.ToString().Contains("msgFalse:")){
                            dataSection.AppendLine("msgFalse: .asciz \"False\"");

                        }

                        textSection.AppendLine($"    ldr {regVal}, [{regTmp}]");
                        textSection.AppendLine($"    cmp {regVal}, #0");
                        textSection.AppendLine($"    bne {etqTrue}");
                        textSection.AppendLine($"    b {etqFalse}");
                        textSection.AppendLine($"{etqTrue}:");
                        textSection.AppendLine($"    mov x0, 1");
                        textSection.AppendLine($"    ldr x1, =msgTrue");
                        textSection.AppendLine($"    mov x2, 4");
                        textSection.AppendLine($"    mov x8, 64");
                        textSection.AppendLine($"    svc 0");
                        textSection.AppendLine($"    b {etqEnd}");
                        textSection.AppendLine($"{etqFalse}:");
                        textSection.AppendLine($"    mov x0, 1");
                        textSection.AppendLine($"    ldr x1, =msgFalse");
                        textSection.AppendLine($"    mov x2, 5");
                        textSection.AppendLine($"    mov x8, 64");
                        textSection.AppendLine($"    svc 0");
                        textSection.AppendLine($"{etqEnd}:");

                    }

                    textSection.AppendLine($"    add {regAux}, {regIdx}, #1");
                    textSection.AppendLine($"    cmp {regAux}, {regLen}");
                    string msgComa = $"msg_coma_{contadorMsg++}";
                    dataSection.AppendLine($"{msgComa}: .asciz \", \"");
                    textSection.AppendLine($"    bge no_coma_{contadorMsg}");
                    textSection.AppendLine($"    mov x0, 1");
                    textSection.AppendLine($"    ldr x1, ={msgComa}");
                    textSection.AppendLine($"    mov x2, 2");
                    textSection.AppendLine($"    mov x8, 64");
                    textSection.AppendLine($"    svc 0");
                    textSection.AppendLine($"no_coma_{contadorMsg}:");

                    textSection.AppendLine($"    add {regIdx}, {regIdx}, #1");
                    textSection.AppendLine($"    b {etqLoop}");

                    textSection.AppendLine($"{etqFin}:");
                    string msgCierra = $"msg_cierra_{contadorMsg++}";
                    dataSection.AppendLine($"{msgCierra}: .asciz \"]\\n\"");
                    textSection.AppendLine($"    mov x0, 1");
                    textSection.AppendLine($"    ldr x1, ={msgCierra}");
                    textSection.AppendLine($"    mov x2, 2");
                    textSection.AppendLine($"    mov x8, 64");
                    textSection.AppendLine($"    svc 0");

                }

            }

            ResetearTemporales();
            return null;

        }

        /* DECLARACION */
        public override object VisitDeclaracionVar(AnalizadorLexicoParser.DeclaracionVarContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string nombre = context.ID().GetText();
            string tipo = context.tipo().GetText();
            string etiqueta = $"var_{nombre}_{contadorVar++}";
            string valorStr = "";

            if (tipo == "int" || tipo == "bool" || tipo == "rune"){
                dataSection.AppendLine($"{etiqueta}: .quad 0");

            }else if (tipo == "float64"){
                dataSection.AppendLine($"{etiqueta}: .double 0.0");

            }else if (tipo == "string"){
                dataSection.AppendLine($"{etiqueta}_ptr: .quad 0");
                dataSection.AppendLine($"{etiqueta}_len: .quad 0");

            }

            if (context.expr() != null){
                var cod = Visit(context.expr()) as CodigoARM64;
                if (cod != null){
                    if (tipo == "int" || tipo == "float64" || tipo == "bool" || tipo == "rune"){
                        textSection.AppendLine($"    ldr x1, ={etiqueta}");
                        textSection.Append(cod.Texto);
                        textSection.AppendLine($"    str {cod.RegistroResultado}, [x1]");

                    }else if (tipo == "string"){
                        textSection.Append(cod.Texto);
                        textSection.AppendLine($"    ldr x1, ={etiqueta}_ptr");
                        textSection.AppendLine($"    mov x2, {cod.RegistroResultado}");
                        textSection.AppendLine($"    str x2, [x1]");

                        textSection.AppendLine($"    ldr x1, ={etiqueta}_len");
                        textSection.AppendLine($"    mov x2, {cod.RegistroResultadoExtra}");
                        textSection.AppendLine($"    str x2, [x1]");

                    }

                }

            }else{
                if (tipo == "string"){
                    // Asignar puntero a cadena vacía
                    string etiquetaVacio = "empty_string";
                    if (!dataSection.ToString().Contains($"{etiquetaVacio}:")){
                        dataSection.Insert(0, $"{etiquetaVacio}: .asciz \"\"\n");

                    }

                    textSection.AppendLine($"    ldr x1, ={etiqueta}_ptr");
                    textSection.AppendLine($"    ldr x2, ={etiquetaVacio}");
                    textSection.AppendLine($"    str x2, [x1]");

                    textSection.AppendLine($"    ldr x1, ={etiqueta}_len");
                    textSection.AppendLine($"    mov x2, #0");
                    textSection.AppendLine($"    str x2, [x1]");

                }

            }

            entornoActual.Declarar(nombre, new VariableARM64(nombre, tipo, etiqueta, valorStr));
            ResetearTemporales();
            return null;

        }

        public override object VisitDeclaracionSliceVacio(AnalizadorLexicoParser.DeclaracionSliceVacioContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string nombre = context.ID().GetText();
            string tipo = context.tipo().GetText();
            string tipoSlice = $"slice<{tipo}>";
            string sufijo = $"{nombre}_{contadorVar++}";
            string etiquetaPtr = $"slice_ptr_{sufijo}";
            string etiquetaLen = $"slice_len_{sufijo}";
            dataSection.AppendLine($"{etiquetaPtr}: .quad 0");
            dataSection.AppendLine($"{etiquetaLen}: .quad 0");
            entornoActual.Declarar(nombre, new VariableARM64(nombre, tipoSlice, etiquetaPtr){
                EtiquetaLongitud = etiquetaLen,
                CantidadElementos = 0
            });

            return null;

        }

        public override object VisitDeclaracionSliceLleno(AnalizadorLexicoParser.DeclaracionSliceLlenoContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string nombre = context.ID().GetText();
            string tipoBase = context.tipo().GetText();
            string tipoSlice = $"slice<{tipoBase}>";
            string signo = context.signo.Text;
            string sufijo = $"{nombre}_{contadorVar++}";
            string etiquetaData = $"slice_data_{sufijo}";
            string etiquetaPtr = $"slice_ptr_{sufijo}";
            string etiquetaLen = $"slice_len_{sufijo}";
            List<string> valoresAsm = new();
            int cantidad = 0;
            foreach (var expr in context.expr()){
                var cod = Visit(expr) as CodigoARM64;
                if (cod == null) continue;
                cantidad++;
                if (tipoBase == "int" || tipoBase == "rune"){
                    valoresAsm.Add($".quad {(int)(cod.ResultadoFloat ?? 0)}");

                }else if (tipoBase == "float64"){
                    valoresAsm.Add($".double {cod.ResultadoFloat?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "0.0"}");

                }else if (tipoBase == "string"){
                    int longitud = Encoding.UTF8.GetByteCount(cod.MensajeLiteral ?? "");
                    string contenidoEscapado = (cod.MensajeLiteral ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    string etiquetaStr = $"msg_slice_{contadorMsg++}";
                    dataSection.AppendLine($"{etiquetaStr}: .asciz \"{contenidoEscapado}\"");
                    valoresAsm.Add($".quad {etiquetaStr}");
                    valoresAsm.Add($".quad {longitud}");

                }else if (tipoBase == "bool"){
                    valoresAsm.Add($".quad {(int)(cod.ResultadoFloat ?? 0)}");

                }

            }

            dataSection.AppendLine($"{etiquetaData}:");
            foreach (var val in valoresAsm){
                dataSection.AppendLine($"    {val}");

            }

            dataSection.AppendLine($"{etiquetaPtr}: .quad {etiquetaData}");
            dataSection.AppendLine($"{etiquetaLen}: .quad {cantidad}");
            var variable = new VariableARM64(nombre, tipoSlice, etiquetaPtr){
                CantidadElementos = cantidad,
                EtiquetaLongitud = etiquetaLen
            };

            if (signo == ":="){
                entornoActual.Declarar(nombre, variable);

            }else if (signo == "="){
                var existente = entornoActual.ObtenerVariable(nombre);
                existente.EtiquetaMemoria = etiquetaPtr;
                existente.CantidadElementos = cantidad;
                existente.EtiquetaLongitud = etiquetaLen;

            }

            return null;

        }

        /* ASIGNACION */
        public override object VisitAsignacionVar(AnalizadorLexicoParser.AsignacionVarContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string nombreVariable = context.ID().GetText();
            string signo = context.GetChild(1).GetText();
            if (context.expr() is AnalizadorLexicoParser.IdExpresionContext idExpr){
                string origen = idExpr.GetText();
                var varDestino = entornoActual.ObtenerVariable(nombreVariable);
                var varOrigen = entornoActual.ObtenerVariable(origen);
                if (varDestino != null && varOrigen != null && varDestino.Tipo.StartsWith("slice<") && varOrigen.Tipo.StartsWith("slice<")){
                    textSection.AppendLine($"    ldr x0, ={varOrigen.EtiquetaMemoria}");
                    textSection.AppendLine($"    ldr x1, [x0]");
                    textSection.AppendLine($"    ldr x2, ={varDestino.EtiquetaMemoria}");
                    textSection.AppendLine($"    str x1, [x2]");
                    textSection.AppendLine($"    ldr x0, ={varOrigen.EtiquetaLongitud}");
                    textSection.AppendLine($"    ldr x1, [x0]");
                    textSection.AppendLine($"    ldr x2, ={varDestino.EtiquetaLongitud}");
                    textSection.AppendLine($"    str x1, [x2]");
                    return null;

                }

            }

            var cod = Visit(context.expr()) as CodigoARM64;
            if (cod == null) return null;
            string tipo = cod.Tipo;

            if (signo == ":="){
                string etiqueta = $"var_{nombreVariable}_{contadorVar++}";
                string etiquetaLen = $"{etiqueta}_len";
                if (tipo == "int" || tipo == "bool"){
                    dataSection.AppendLine($"{etiqueta}: .quad 0");
                    textSection.Append(cod.Texto);
                    textSection.AppendLine($"    ldr x1, ={etiqueta}");
                    textSection.AppendLine($"    str {cod.RegistroResultado}, [x1]");

                }else if (tipo == "float64"){
                    dataSection.AppendLine($"{etiqueta}: .double 0.0");
                    textSection.Append(cod.Texto);
                    textSection.AppendLine($"    ldr x1, ={etiqueta}");
                    textSection.AppendLine($"    str {cod.RegistroResultado}, [x1]");

                }else if (tipo == "rune"){
                    dataSection.AppendLine($"{etiqueta}: .word 0");
                    textSection.Append(cod.Texto);
                    textSection.AppendLine($"    ldr x1, ={etiqueta}");
                    textSection.AppendLine($"    str w{cod.RegistroResultado.Substring(1)}, [x1]");

                }else if (tipo == "string"){
                    dataSection.AppendLine($"{etiqueta}_ptr: .quad 0");
                    dataSection.AppendLine($"{etiqueta}_len: .quad 0");
                    textSection.Append(cod.Texto);
                    textSection.AppendLine($"    ldr x1, ={etiqueta}_ptr");
                    textSection.AppendLine($"    mov x2, {cod.RegistroResultado}");
                    textSection.AppendLine($"    str x2, [x1]");
                    if (!string.IsNullOrEmpty(cod.RegistroResultadoExtra)){
                        textSection.AppendLine($"    ldr x1, ={etiqueta}_len");
                        textSection.AppendLine($"    mov x2, {cod.RegistroResultadoExtra}");
                        textSection.AppendLine($"    str x2, [x1]");

                    }

                }

                var nuevaVar = new VariableARM64(nombreVariable, tipo, etiqueta, cod.MensajeLiteral ?? "");
                if (!string.IsNullOrEmpty(cod.RegistroResultadoExtra)){
                    nuevaVar.EtiquetaLongitud = etiquetaLen;

                }

                entornoActual.Declarar(nombreVariable, nuevaVar);

            }else if (signo == "="){
                var variable = entornoActual.ObtenerVariable(nombreVariable);
                if (variable != null){
                    textSection.Append(cod.Texto);
                    if (tipo == "int" || tipo == "bool" || tipo == "float64"){
                        textSection.AppendLine($"    ldr x1, ={variable.EtiquetaMemoria}");
                        textSection.AppendLine($"    str {cod.RegistroResultado}, [x1]");

                    }else if (tipo == "rune"){
                        textSection.AppendLine($"    ldr x1, ={variable.EtiquetaMemoria}");
                        textSection.AppendLine($"    str w{cod.RegistroResultado.Substring(1)}, [x1]");

                    }else if (tipo == "string"){
                        textSection.AppendLine($"    ldr x1, ={variable.EtiquetaMemoria}_ptr");
                        textSection.AppendLine($"    mov x2, {cod.RegistroResultado}");
                        textSection.AppendLine($"    str x2, [x1]");
                        if (!string.IsNullOrEmpty(cod.RegistroResultadoExtra)){
                            textSection.AppendLine($"    ldr x1, ={variable.EtiquetaMemoria}_len");
                            textSection.AppendLine($"    mov x2, {cod.RegistroResultadoExtra}");
                            textSection.AppendLine($"    str x2, [x1]");

                        }

                    }

                }

            }

            ResetearTemporales();
            return null;

        }

        public override object VisitSliceUpdate(AnalizadorLexicoParser.SliceUpdateContext context){
            var entorno = pilaEntornosARM64.Peek();
            string nombre = context.ID().GetText();
            var variable = entorno.ObtenerVariable(nombre);
            if (variable == null || !variable.Tipo.StartsWith("slice<")) return null;
            string tipoBase = variable.Tipo.Substring(6, variable.Tipo.Length - 7);
            int sizeTipo = tipoBase switch{
                "int" or "float64" => 8,
                "rune" => 4,
                "bool" => 1,
                "string" => 16,
                _ => 8
            };

            var codIndex = Visit(context.expr(0)) as CodigoARM64;
            var codValor = Visit(context.expr(1)) as CodigoARM64;

            string regIndex = codIndex.RegistroResultado;
            string regLenPtr = NuevoRegistroTmp();
            string regLen = NuevoRegistroTmp();
            string regBase = NuevoRegistroTmp();
            string regOffset = NuevoRegistroTmp();
            string regPtr = NuevoRegistroTmp();

            var asm = new StringBuilder();
            asm.Append(codIndex.Texto);
            asm.Append(codValor.Texto);

            asm.AppendLine($"    ldr {regLenPtr}, ={variable.EtiquetaLongitud}");
            asm.AppendLine($"    ldr {regLen}, [{regLenPtr}]");
            asm.AppendLine($"    cmp {regIndex}, {regLen}");
            asm.AppendLine($"    bge acceso_fuera_rango");
            asm.AppendLine($"    cmp {regIndex}, #0");
            asm.AppendLine($"    blt acceso_fuera_rango");

            asm.AppendLine($"    ldr {regBase}, ={variable.EtiquetaMemoria}");
            asm.AppendLine($"    ldr {regBase}, [{regBase}]");

            asm.AppendLine($"    lsl {regOffset}, {regIndex}, #{(int)Math.Log2(sizeTipo)}");
            asm.AppendLine($"    add {regPtr}, {regBase}, {regOffset}");

            if (tipoBase == "float64"){
                asm.AppendLine($"    str {codValor.RegistroResultado}, [{regPtr}]");

            }else if (tipoBase == "string"){
                asm.AppendLine($"    str {codValor.RegistroResultado}, [{regPtr}]");
                asm.AppendLine($"    str {codValor.RegistroResultadoExtra}, [{regPtr}, #8]");

            }else if (sizeTipo == 1){
                asm.AppendLine($"    strb {codValor.RegistroResultado.Replace("x", "w")}, [{regPtr}]");

            }else if (sizeTipo == 4){
                asm.AppendLine($"    str {codValor.RegistroResultado.Replace("x", "w")}, [{regPtr}]");

            }else{
                asm.AppendLine($"    str {codValor.RegistroResultado}, [{regPtr}]");

            }

            textSection.Append(asm.ToString());
            ResetearTemporales();
            return null;

        }

        public override object VisitAppend(AnalizadorLexicoParser.AppendContext context){
            var entorno = pilaEntornosARM64.Peek();
            string nombreSlice = context.ID(0).GetText();
            var variable = entorno.ObtenerVariable(nombreSlice);
            if (variable == null || !variable.Tipo.StartsWith("slice<")) return null;
            string tipoBase = variable.Tipo.Substring(6, variable.Tipo.Length - 7);
            int sizeTipo = tipoBase switch{
                "int" or "float64" => 8,
                "rune" => 4,
                "bool" => 1,
                "string" => 16,
                _ => 8
            };

            string etiquetaPtr = variable.EtiquetaMemoria;
            string etiquetaLen = variable.EtiquetaLongitud;

            string regOldPtr = NuevoRegistroTmp();
            string regOldLen = NuevoRegistroTmp();
            string regNewLen = NuevoRegistroTmp();
            string regNewPtr = NuevoRegistroTmp();
            string regOffset = NuevoRegistroTmp();
            string regIdx = NuevoRegistroTmp();

            int cantidadNuevos = context.expr().Length;
            var asm = new StringBuilder();

            asm.AppendLine($"    ldr {regOldPtr}, ={etiquetaPtr}");
            asm.AppendLine($"    ldr {regOldPtr}, [{regOldPtr}]");
            asm.AppendLine($"    ldr {regOldLen}, ={etiquetaLen}");
            asm.AppendLine($"    ldr {regOldLen}, [{regOldLen}]");

            asm.AppendLine($"    mov {regNewLen}, {regOldLen}");
            asm.AppendLine($"    add {regNewLen}, {regNewLen}, #{cantidadNuevos}");

            string etiquetaMemTmp = $"append_tmp_{contadorVar++}";
            dataSection.AppendLine($"{etiquetaMemTmp}: .skip {(sizeTipo * (cantidadNuevos + 100))}");
            asm.AppendLine($"    ldr {regNewPtr}, ={etiquetaMemTmp}");

            asm.AppendLine($"    mov {regIdx}, #0");
            string etqLoop = NuevaEtiqueta("copy_loop");
            string etqFin = NuevaEtiqueta("copy_fin");
            asm.AppendLine($"{etqLoop}:");
            asm.AppendLine($"    cmp {regIdx}, {regOldLen}");
            asm.AppendLine($"    bge {etqFin}");
            asm.AppendLine($"    mov {regOffset}, {regIdx}");
            asm.AppendLine($"    lsl {regOffset}, {regOffset}, #{(int)Math.Log2(sizeTipo)}");
            string regTmp = NuevoRegistroTmp();
            asm.AppendLine($"    add {regTmp}, {regOldPtr}, {regOffset}");
            string regVal = NuevoRegistroTmp();
            if (tipoBase == "float64"){
                asm.AppendLine($"    ldr d0, [{regTmp}]\n    str d0, [{regNewPtr}, {regOffset}]");

            }else if (tipoBase == "bool" || tipoBase == "rune"){
                asm.AppendLine($"    ldrb w0, [{regTmp}]\n    strb w0, [{regNewPtr}, {regOffset}]");

            }else if (tipoBase == "string"){
                asm.AppendLine($"    ldr x0, [{regTmp}]\n    ldr x1, [{regTmp}, #8]");
                asm.AppendLine($"    add {regTmp}, {regNewPtr}, {regOffset}");
                asm.AppendLine($"    str x0, [{regTmp}]\n    str x1, [{regTmp}, #8]");

            }else{
                asm.AppendLine($"    ldr {regVal}, [{regTmp}]\n    str {regVal}, [{regNewPtr}, {regOffset}]");

            }

            asm.AppendLine($"    add {regIdx}, {regIdx}, #1");
            asm.AppendLine($"    b {etqLoop}");
            asm.AppendLine($"{etqFin}:");
            for (int i = 0; i < context.expr().Length; i++){
                var cod = Visit(context.expr(i)) as CodigoARM64;
                if (cod == null) continue;
                asm.Append(cod.Texto);
                asm.AppendLine($"    mov {regOffset}, #{i}");
                asm.AppendLine($"    add {regOffset}, {regOffset}, {regOldLen}");
                asm.AppendLine($"    lsl {regOffset}, {regOffset}, #{(int)Math.Log2(sizeTipo)}");
                string regDst = NuevoRegistroTmp();
                asm.AppendLine($"    add {regDst}, {regNewPtr}, {regOffset}");
                if (tipoBase == "float64"){
                    asm.AppendLine($"    str {cod.RegistroResultado}, [{regDst}]");

                }else if (tipoBase == "bool" || tipoBase == "rune"){
                    asm.AppendLine($"    strb {cod.RegistroResultado.Replace("x", "w")}, [{regDst}]");

                }else if (tipoBase == "string"){
                    asm.AppendLine($"    str {cod.RegistroResultado}, [{regDst}]");
                    asm.AppendLine($"    str {cod.RegistroResultadoExtra}, [{regDst}, #8]");

                }else{
                    asm.AppendLine($"    str {cod.RegistroResultado}, [{regDst}]");

                }

            }

            asm.AppendLine($"    ldr x1, ={etiquetaPtr}");
            asm.AppendLine($"    str {regNewPtr}, [x1]");
            asm.AppendLine($"    ldr x1, ={etiquetaLen}");
            asm.AppendLine($"    str {regNewLen}, [x1]");

            textSection.Append(asm.ToString());
            ResetearTemporales();
            return null;

        }

        public override object VisitIncrementoDecremento(AnalizadorLexicoParser.IncrementoDecrementoContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string nombreVariable = context.ID().GetText();
            var variable = entornoActual.ObtenerVariable(nombreVariable);
            bool esIncremento = context.INCRE() != null;
            string reg = NuevoRegistroTmp();
            string texto = $"    ldr x1, ={variable.EtiquetaMemoria}\n";

            if (variable.Tipo == "int" || variable.Tipo == "bool" || variable.Tipo == "rune") {
                texto += $"    ldr {reg}, [x1]\n";
                texto += esIncremento
                    ? $"    add {reg}, {reg}, #1\n"
                    : $"    sub {reg}, {reg}, #1\n";
                texto += $"    str {reg}, [x1]\n";

            }else if (variable.Tipo == "float64") {
                if (!dataSection.ToString().Contains("flt_1:")){
                    dataSection.AppendLine("flt_1: .double 1.0");

                }

                string regFloat = NuevoRegistroTmpFloat();
                string regTmpFloat = NuevoRegistroTmpFloat();
                texto += $"    ldr {regFloat}, [x1]\n";
                texto += $"    ldr x2, =flt_1\n";
                texto += $"    ldr {regTmpFloat}, [x2]\n";
                texto += esIncremento
                    ? $"    fadd {regFloat}, {regFloat}, {regTmpFloat}\n"
                    : $"    fsub {regFloat}, {regFloat}, {regTmpFloat}\n";
                texto += $"    str {regFloat}, [x1]\n";

            }

            textSection.Append(texto);
            ResetearTemporales();
            return null;

        }

        /* INSTRUCCION IF */
        public override object VisitInstruccion_if(AnalizadorLexicoParser.Instruccion_ifContext context){
            string etiquetaFin = $"etq_if_fin_{contadorMsg++}";
            var condIf = Visit(context.expr()) as CodigoARM64;
            string etiquetaIfTrue = $"etq_if_true_{contadorMsg++}";
            textSection.Append(condIf.Texto);
            textSection.AppendLine($"    cmp {condIf.RegistroResultado}, #0");
            textSection.AppendLine($"    bne {etiquetaIfTrue}");
            List<(string etiquetaCond, string etiquetaExec)> etiquetasElseIf = new();
            foreach (var _ in context.instruccion_ifelse()){
                string etiquetaCond = $"etq_elseif_cond_{contadorMsg++}";
                string etiquetaExec = $"etq_elseif_exec_{contadorMsg++}";
                etiquetasElseIf.Add((etiquetaCond, etiquetaExec));

            }

            string etiquetaElse = context.instruccion_else() != null ? $"etq_else_{contadorMsg++}" : null;
            if (etiquetasElseIf.Count > 0){
                textSection.AppendLine($"    b {etiquetasElseIf[0].etiquetaCond}");

            }else if (etiquetaElse != null){
                textSection.AppendLine($"    b {etiquetaElse}");

            }else{
                textSection.AppendLine($"    b {etiquetaFin}");

            }

            textSection.AppendLine($"{etiquetaIfTrue}:");
            pilaEntornosARM64.Push(new EntornoARM64(pilaEntornosARM64.Peek()));
            Visit(context.listaInstr());
            pilaEntornosARM64.Pop();
            textSection.AppendLine($"    b {etiquetaFin}");

            for (int i = 0; i < context.instruccion_ifelse().Length; i++){
                var elseIf = context.instruccion_ifelse(i);
                var condElseIf = Visit(elseIf.expr()) as CodigoARM64;
                string etiquetaCond = etiquetasElseIf[i].etiquetaCond;
                string etiquetaExec = etiquetasElseIf[i].etiquetaExec;
                string etiquetaSiguiente = (i + 1 < etiquetasElseIf.Count)
                    ? etiquetasElseIf[i + 1].etiquetaCond
                    : (etiquetaElse ?? etiquetaFin);

                textSection.AppendLine($"{etiquetaCond}:");
                textSection.Append(condElseIf.Texto);
                textSection.AppendLine($"    cmp {condElseIf.RegistroResultado}, #0");
                textSection.AppendLine($"    bne {etiquetaExec}");
                textSection.AppendLine($"    b {etiquetaSiguiente}");

                textSection.AppendLine($"{etiquetaExec}:");
                pilaEntornosARM64.Push(new EntornoARM64(pilaEntornosARM64.Peek()));
                Visit(elseIf.listaInstr());
                pilaEntornosARM64.Pop();
                textSection.AppendLine($"    b {etiquetaFin}");

            }

            if (context.instruccion_else() != null){
                textSection.AppendLine($"{etiquetaElse}:");
                pilaEntornosARM64.Push(new EntornoARM64(pilaEntornosARM64.Peek()));
                Visit(context.instruccion_else().listaInstr());
                pilaEntornosARM64.Pop();
                textSection.AppendLine($"    b {etiquetaFin}");

            }

            textSection.AppendLine($"{etiquetaFin}:");
            ResetearTemporales();
            return null;

        }

        /* INSTRUCCION SWITCH */
        public override object VisitInstruccion_switch(AnalizadorLexicoParser.Instruccion_switchContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string etiquetaSwitchFin = NuevaEtiqueta("switch_fin");
            var exprSwitch = Visit(context.expr()) as CodigoARM64;
            if (exprSwitch == null){
                return null;

            }

            string texto = exprSwitch.Texto;
            string regSwitch = exprSwitch.RegistroResultado;
            var cases = context.lista_case()?.instruccion_case();
            List<(string etiqueta, AnalizadorLexicoParser.Instruccion_caseContext caseCtx)> listaCases = new();
            if (cases != null){
                foreach (var caseCtx in cases){
                    string etiquetaCase = NuevaEtiqueta("case");
                    listaCases.Add((etiquetaCase, caseCtx));
                    var exprCase = Visit(caseCtx.expr()) as CodigoARM64;
                    if (exprCase == null) continue;
                    texto += exprCase.Texto;
                    texto += $"    cmp {regSwitch}, {exprCase.RegistroResultado}\n";
                    texto += $"    b.eq {etiquetaCase}\n";

                }

            }

            string etiquetaDefault = null;
            if (context.instruccion_default() != null){
                etiquetaDefault = NuevaEtiqueta("default");
                texto += $"    b {etiquetaDefault}\n";

            }else{
                texto += $"    b {etiquetaSwitchFin}\n";

            }

            foreach (var (etiqueta, caseCtx) in listaCases){
                texto += $"{etiqueta}:\n";
                var entornoCase = new EntornoARM64(entornoActual);
                pilaEntornosARM64.Push(entornoCase);
                pilaBucles.Push((null, etiquetaSwitchFin));  
                texto += VisitListaInstrTexto(caseCtx.listaInstr());
                pilaBucles.Pop();
                pilaEntornosARM64.Pop();
                texto += $"    b {etiquetaSwitchFin}\n";

            }

            if (context.instruccion_default() != null){
                texto += $"{etiquetaDefault}:\n";
                var entornoDefault = new EntornoARM64(entornoActual);
                pilaEntornosARM64.Push(entornoDefault);
                pilaBucles.Push((null, etiquetaSwitchFin));

                texto += VisitListaInstrTexto(context.instruccion_default().listaInstr());

                pilaBucles.Pop();
                pilaEntornosARM64.Pop();
                texto += $"    b {etiquetaSwitchFin}\n";

            }

            texto += $"{etiquetaSwitchFin}:\n";
            textSection.Append(texto);
            ResetearTemporales();
            return null;

        }

        /* INSTRUCCION FOR */
        public override object VisitForNormal(AnalizadorLexicoParser.ForNormalContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string etiquetaInicio = NuevaEtiqueta("for_inicio");
            string etiquetaCondicion = NuevaEtiqueta("for_condicion");
            string etiquetaFin = NuevaEtiqueta("for_fin");

            string texto = $"    b {etiquetaCondicion}\n";
            texto += $"{etiquetaInicio}:\n";

            var entornoFor = new EntornoARM64(entornoActual);
            pilaEntornosARM64.Push(entornoFor);
            pilaBucles.Push((etiquetaCondicion, etiquetaFin));
            texto += VisitListaInstrTexto(context.listaInstr());
            pilaBucles.Pop();
            pilaEntornosARM64.Pop();
            texto += $"{etiquetaCondicion}:\n";

            var codCondicion = Visit(context.expr()) as CodigoARM64;
            if (codCondicion != null){
                texto += codCondicion.Texto;
                texto += $"    cmp {codCondicion.RegistroResultado}, #1\n";
                texto += $"    b.eq {etiquetaInicio}\n"; 

            }

            texto += $"{etiquetaFin}:\n";
            textSection.Append(texto);
            ResetearTemporales();
            return null;

        }

        public override object VisitForIncremento(AnalizadorLexicoParser.ForIncrementoContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string etqInicio     = NuevaEtiqueta("for_inc_inicio");
            string etqCond       = NuevaEtiqueta("for_inc_cond");
            string etqIncremento = NuevaEtiqueta("for_inc_incr");
            string etqFin        = NuevaEtiqueta("for_inc_fin");

            Visit(context.asignacion(0));
            textSection.AppendLine($"    b {etqCond}");
            textSection.AppendLine($"{etqInicio}:");
            var entornoFor = new EntornoARM64(entornoActual);
            pilaEntornosARM64.Push(entornoFor);
            pilaBucles.Push((etqIncremento, etqFin));
            textSection.Append(VisitListaInstrTexto(context.listaInstr()));
            pilaBucles.Pop();
            pilaEntornosARM64.Pop();

            textSection.AppendLine($"{etqIncremento}:");
            Visit(context.asignacion(1));

            textSection.AppendLine($"{etqCond}:");
            var codCond = Visit(context.expr()) as CodigoARM64;
            if (codCond != null){
                textSection.Append(codCond.Texto);
                textSection.AppendLine($"    cmp {codCond.RegistroResultado}, #1");
                textSection.AppendLine($"    b.eq {etqInicio}");

            }

            textSection.AppendLine($"{etqFin}:");
            ResetearTemporales();
            return null;

        }

        public override object VisitForRange(AnalizadorLexicoParser.ForRangeContext context){
            var entornoActual = pilaEntornosARM64.Peek();
            string idIndice = context.ID(0).GetText();
            string idValor = context.ID(1).GetText();
            string idSlice = context.ID(2).GetText();
            var slice = entornoActual.ObtenerVariable(idSlice);
            if (slice == null || !slice.Tipo.StartsWith("slice<")) return null;
            string tipoElem = slice.Tipo.Substring(6, slice.Tipo.Length - 7);
            int size = tipoElem switch{
                "int" or "float64" => 8,
                "rune" => 4,
                "bool" => 1,
                "string" => 16,
                _ => 8
            };

            string ptrEtiqueta = slice.EtiquetaMemoria;
            string lenEtiqueta = slice.EtiquetaLongitud;

            string uid = $"{contadorMsg++}";
            string etiquetaIndice = $"for_range_{idIndice}_{uid}";
            string etiquetaValor  = $"for_range_{idValor}_{uid}";

            string etqCond = $"for_range_cond_{uid}";
            string etqLoop = $"for_range_loop_{uid}";
            string etqContinue = $"for_range_continue_{uid}";
            string etqFin  = $"for_range_fin_{uid}";

            dataSection.AppendLine($"{etiquetaIndice}: .quad 0");
            dataSection.AppendLine($"{etiquetaValor}: .quad 0");

            string regIndice = "x19";
            string regPtrTmp = NuevoRegistroTmp();
            string regLenTmp = NuevoRegistroTmp();
            string regOffset = NuevoRegistroTmp();
            string regValTmp = tipoElem == "float64" ? NuevoRegistroTmpFloat() : NuevoRegistroTmp();

            textSection.AppendLine($"    mov {regIndice}, #0");
            textSection.AppendLine($"    b {etqCond}");

            textSection.AppendLine($"{etqLoop}:");
            textSection.AppendLine($"    ldr {regPtrTmp}, ={ptrEtiqueta}");
            textSection.AppendLine($"    ldr {regPtrTmp}, [{regPtrTmp}]");

            if (size > 1){
                textSection.AppendLine($"    lsl {regOffset}, {regIndice}, #{(int)Math.Log2(size)}");

            }else{
                textSection.AppendLine($"    mov {regOffset}, {regIndice}");

            }

            textSection.AppendLine($"    add {regOffset}, {regPtrTmp}, {regOffset}");
            if (tipoElem == "float64"){
                textSection.AppendLine($"    ldr {regValTmp}, [{regOffset}]");

            }else if (size == 1){
                textSection.AppendLine($"    ldrb {ARegW(regValTmp)}, [{regOffset}]");

            }else if (size == 4){
                textSection.AppendLine($"    ldr {ARegW(regValTmp)}, [{regOffset}]");

            }else{
                textSection.AppendLine($"    ldr {regValTmp}, [{regOffset}]");

            }

            textSection.AppendLine($"    ldr x0, ={etiquetaValor}");
            textSection.AppendLine($"    str {regValTmp}, [x0]");
            textSection.AppendLine($"    ldr x0, ={etiquetaIndice}");
            textSection.AppendLine($"    str {regIndice}, [x0]");

            var entornoFor = new EntornoARM64(entornoActual);
            entornoFor.Declarar(idIndice, new VariableARM64(idIndice, "int", etiquetaIndice));
            entornoFor.Declarar(idValor, new VariableARM64(idValor, tipoElem, etiquetaValor));
            pilaEntornosARM64.Push(entornoFor);
            pilaBucles.Push((etqContinue, etqFin));
            textSection.Append(VisitListaInstrTexto(context.listaInstr()));
            pilaBucles.Pop();
            pilaEntornosARM64.Pop();

            textSection.AppendLine($"{etqContinue}:");
            textSection.AppendLine($"    add {regIndice}, {regIndice}, #1");
            textSection.AppendLine($"    b {etqCond}");

            textSection.AppendLine($"{etqCond}:");
            textSection.AppendLine($"    ldr {regLenTmp}, ={lenEtiqueta}");
            textSection.AppendLine($"    ldr {regLenTmp}, [{regLenTmp}]");
            textSection.AppendLine($"    cmp {regIndice}, {regLenTmp}");
            textSection.AppendLine($"    blt {etqLoop}");

            textSection.AppendLine($"{etqFin}:");
            ResetearTemporales();
            return null;

        }

        public override object VisitInstruccion_funcion(AnalizadorLexicoParser.Instruccion_funcionContext context){
            string nombreFuncion = context.ID().GetText();
            string etiquetaFuncion = $"func_{nombreFuncion}";
            var entornoGlobal = pilaEntornosARM64.First();
            if (pilaEntornosARM64.Peek() != entornoGlobal){
                return null;

            }

            if (entornoGlobal.ContieneEnEsteAmbito(nombreFuncion)){
                return null;

            }

            var entornoFuncion = new EntornoARM64(entornoGlobal);
            pilaEntornosARM64.Push(entornoFuncion);

            if (context.lista_parametros() != null){
                var ids = context.lista_parametros().expr();
                var tipos = context.lista_parametros().tipo();

                for (int i = 0; i < ids.Length; i++){
                    string tipoParam = tipos[i].GetText();
                    string nombreParam = ids[i].GetText();
                    string baseEtiqueta = $"param_{nombreFuncion}_arg_{i}";

                    if (tipoParam == "string"){
                        dataSection.AppendLine($"{baseEtiqueta}_ptr: .quad 0");
                        dataSection.AppendLine($"{baseEtiqueta}_len: .quad 0");
                        entornoFuncion.Declarar(nombreParam, new VariableARM64(nombreParam, tipoParam, baseEtiqueta, "", $"{baseEtiqueta}_len"));

                    }else{
                        dataSection.AppendLine($"{baseEtiqueta}: .quad 0");
                        entornoFuncion.Declarar(nombreParam, new VariableARM64(nombreParam, tipoParam, baseEtiqueta));

                    }

                }

            }

            string tipoRetorno = context.tipo()?.GetText() ?? "void";
            if (tipoRetorno != "void"){
                entornoFuncion.Declarar("ret", new VariableARM64("ret", tipoRetorno, $"ret_{nombreFuncion}"));

            }

            funcSection.AppendLine($"{etiquetaFuncion}:");
            funcSection.AppendLine("    stp x29, x30, [sp, #-16]!");
            funcSection.AppendLine("    mov x29, sp");
            funcSection.Append(VisitListaInstrTexto(context.listaInstr()));
            funcSection.AppendLine("    ldp x29, x30, [sp], #16");
            funcSection.AppendLine("    ret");
            pilaEntornosARM64.Pop();
            entornoGlobal.Declarar(nombreFuncion, new VariableARM64(nombreFuncion, "func", etiquetaFuncion, tipoRetorno));
            return null;

        }

        public override object VisitLlamada_funcion(AnalizadorLexicoParser.Llamada_funcionContext context){
            string nombreFuncion = context.ID().GetText();
            var entornoGlobal = pilaEntornosARM64.First();
            if (!entornoGlobal.ContieneEnEsteAmbito(nombreFuncion)) return null;
            var simbolo = entornoGlobal.ObtenerVariable(nombreFuncion);
            if (simbolo == null || simbolo.Tipo != "func") return null;
            string etiquetaFuncion = simbolo.EtiquetaMemoria;
            string tipoRetorno = simbolo.ValorStr ?? "void";

            if (context.lista_argumentos() != null){
                var args = context.lista_argumentos().expr();
                for (int i = 0; i < args.Length; i++){
                    var codArg = Visit(args[i]) as CodigoARM64;
                    if (codArg == null) continue;
                    textSection.Append(codArg.Texto);
                    string etiquetaBase = $"param_{nombreFuncion}_arg_{i}";
                    if (codArg.Tipo == "string"){
                        if (!dataSection.ToString().Contains($"{etiquetaBase}_ptr:")){
                            dataSection.AppendLine($"{etiquetaBase}_ptr: .quad 0");

                        }

                        if (!dataSection.ToString().Contains($"{etiquetaBase}_len:")){
                            dataSection.AppendLine($"{etiquetaBase}_len: .quad 0");

                        }

                        textSection.AppendLine($"    ldr x0, ={etiquetaBase}_ptr");
                        textSection.AppendLine($"    str {codArg.RegistroResultado}, [x0]");
                        if (!string.IsNullOrEmpty(codArg.RegistroResultadoExtra)){
                            textSection.AppendLine($"    ldr x0, ={etiquetaBase}_len");
                            textSection.AppendLine($"    mov x1, {codArg.RegistroResultadoExtra}");
                            textSection.AppendLine($"    str x1, [x0]");

                        }

                    }else{
                        if (!dataSection.ToString().Contains($"{etiquetaBase}:")){
                            dataSection.AppendLine($"{etiquetaBase}: .quad 0");

                        }

                        textSection.AppendLine($"    ldr x0, ={etiquetaBase}");
                        textSection.AppendLine($"    str {codArg.RegistroResultado}, [x0]");

                    }

                }

            }

            textSection.AppendLine($"    bl {etiquetaFuncion}");

            return tipoRetorno switch{
                "string" => new CodigoARM64 { Texto = "", Tipo = "string", RegistroResultado = "x0", RegistroResultadoExtra = "x1" },
                "float64" => new CodigoARM64 { Texto = "", Tipo = "float64", RegistroResultado = "d0" },
                "int" or "bool" or "rune" => new CodigoARM64 { Texto = "", Tipo = tipoRetorno, RegistroResultado = "x0" },
                _ => null
            };

        }

        /* SENTENCIAS DE TRANSFERENCIA */
        public override object VisitBreakInstr(AnalizadorLexicoParser.BreakInstrContext context){
            if (pilaBucles.Count == 0) return null;
            string etiquetaBreak = pilaBucles.Peek().etqBreak;
            textSection.AppendLine($"    b {etiquetaBreak}");
            return null;

        }

        public override object VisitContinueInstr(AnalizadorLexicoParser.ContinueInstrContext context){
            if (pilaBucles.Count == 0) return null;
            string etiquetaContinue = pilaBucles.Peek().etqContinue;
            textSection.AppendLine($"    b {etiquetaContinue}");
            return null;

        }

        public override object VisitReturnInstr(AnalizadorLexicoParser.ReturnInstrContext context){
            var cod = Visit(context.expr()) as CodigoARM64;
            if (cod == null) return null;
            textSection.Append(cod.Texto);
            if (cod.Tipo == "string"){
                textSection.AppendLine($"    mov x0, {cod.RegistroResultado}");
                textSection.AppendLine($"    mov x1, {cod.RegistroResultadoExtra}");

            }else if (cod.Tipo == "float64"){
                if (cod.RegistroResultado != "d0"){
                    textSection.AppendLine($"    fmov d0, {cod.RegistroResultado}");

                }

            }else{
                if (cod.RegistroResultado != "x0"){
                    textSection.AppendLine($"    mov x0, {cod.RegistroResultado}");

                }

            }

            textSection.AppendLine("    ldp x29, x30, [sp], #16");
            textSection.AppendLine("    ret");
            return null;

        }

        /* EXPR */
        public override object VisitExpreParentesis(AnalizadorLexicoParser.ExpreParentesisContext context){
            return Visit(context.expr());

        }

        public override object VisitIntExpresion(AnalizadorLexicoParser.IntExpresionContext context){
            string valor = context.INT().GetText();
            string reg = NuevoRegistroTmp();
            string texto = $"    mov {reg}, {valor}\n";
            return new CodigoARM64{ 
                Texto = texto, 
                RegistroResultado = reg, 
                Tipo = "int",
                ResultadoFloat = double.Parse(valor)
            };

        }

        public override object VisitDecimalExpresion(AnalizadorLexicoParser.DecimalExpresionContext context){
            string valor = context.DECIMAL().GetText();
            string reg = NuevoRegistroTmpFloat();
            string etiqueta = $"flt{contadorMsg++}";
            dataSection.AppendLine($"{etiqueta}: .double {valor}");
            string texto = $"    ldr x0, ={etiqueta}\n    ldr {reg}, [x0]\n";
            return new CodigoARM64{
                Texto = texto,
                RegistroResultado = reg,
                Tipo = "float64",
                ResultadoFloat = double.TryParse(valor, out var d) ? d : (double?)null
            };

        }

        public override object VisitCaracterExpresion(AnalizadorLexicoParser.CaracterExpresionContext context){
            string texto = context.GetText();
            char ch;
            if (texto.Length == 3){
                ch = texto[1];

            }else if (texto.Length == 4 && texto[1] == '\\'){
                ch = texto[2] switch{
                    'n' => '\n',
                    't' => '\t',
                    'r' => '\r',
                    '\'' => '\'',
                    '\\' => '\\',
                    _ => '?'
                };

            }else ch = '?';

            int ascii = (int)ch;
            string reg = NuevoRegistroTmp();
            string asm = $"    mov {reg}, {ascii}\n";
            string mensaje = $"{ch}\n";
            string etiqueta = $"msg{contadorMsg++}";
            string mensajeEscapado = mensaje.Replace("\\", "\\\\").Replace("\"", "\\\"");
            dataSection.AppendLine($"{etiqueta}: .asciz \"{mensajeEscapado}\"");
            return new CodigoARM64{ 
                Texto = asm,
                RegistroResultado = reg,
                Tipo = "rune",
                MensajeLiteral = mensaje,
                EtiquetaMensaje = etiqueta,
                ResultadoFloat = ascii
            };

        }

        public override object VisitCadenaExpresion(AnalizadorLexicoParser.CadenaExpresionContext context){
            string cadena = context.GetText();
            string contenido = cadena.Substring(1, cadena.Length - 2)
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\r", "\r")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");

            string etiqueta = $"msg{contadorMsg++}";
            string contenidoEscapado = contenido.Replace("\\", "\\\\").Replace("\"", "\\\"");
            int longitud = Encoding.UTF8.GetByteCount(contenido);
            if (!dataSection.ToString().Contains($"{etiqueta}:")){
                dataSection.AppendLine($"{etiqueta}: .asciz \"{contenidoEscapado}\"");

            }

            string regPtr = NuevoRegistroTmp();
            string regLen = NuevoRegistroTmp();
            var sb = new StringBuilder();
            sb.AppendLine($"    ldr {regPtr}, ={etiqueta}");
            sb.AppendLine($"    mov {regLen}, #{longitud}");

            return new CodigoARM64{
                Texto = sb.ToString(),
                RegistroResultado = regPtr,
                RegistroResultadoExtra = regLen,
                Tipo = "string",
                MensajeLiteral = contenido
            };

        }

        public override object VisitBooleanExpresion(AnalizadorLexicoParser.BooleanExpresionContext context){
            string boolText = context.BOOL().GetText();
            int valor = (boolText == "true") ? 1 : 0;
            string reg = NuevoRegistroTmp();
            string asm = $"    mov {reg}, #{valor}\n";
            string etiqueta = $"msg{contadorMsg++}";
            string valStr = (valor == 1) ? "True" : "False";
            dataSection.AppendLine($"{etiqueta}: .asciz \"{valStr}\"");
            return new CodigoARM64{
                Texto = asm,           
                RegistroResultado = reg,      
                Tipo = "bool",               
                MensajeLiteral = valStr,      
                EtiquetaMensaje = etiqueta,
                ResultadoFloat = valor
            };

        }

        public override object VisitIdExpresion(AnalizadorLexicoParser.IdExpresionContext context){
            var entorno = pilaEntornosARM64.Peek();
            var variable = entorno.ObtenerVariable(context.ID().GetText());
            if (variable == null) return null;
            string etiqueta = variable.EtiquetaMemoria;
            if (variable.Tipo == "int" || variable.Tipo == "bool" || variable.Tipo == "rune"){
                string reg = NuevoRegistroTmp();
                string asm = $"    ldr x0, ={etiqueta}\n    ldr {reg}, [x0]\n";
                return new CodigoARM64{
                    Texto = asm,
                    RegistroResultado = reg,
                    Tipo = variable.Tipo,
                    MensajeLiteral = variable.ValorStr
                };

            }

            if (variable.Tipo == "float64"){
                string reg = NuevoRegistroTmpFloat();
                string asm = $"    ldr x0, ={etiqueta}\n    ldr {reg}, [x0]\n";
                return new CodigoARM64{
                    Texto = asm,
                    RegistroResultado = reg,
                    Tipo = "float64"
                };

            }

            if (variable.Tipo == "string"){
                if (variable.EtiquetaLongitud != null){
                    string regPtr = NuevoRegistroTmp();
                    string regLen = NuevoRegistroTmp();
                    string asm = $"    ldr {regPtr}, ={etiqueta}_ptr\n" + $"    ldr {regPtr}, [{regPtr}]\n" + $"    ldr {regLen}, ={etiqueta}_len\n" + $"    ldr {regLen}, [{regLen}]\n";
                    return new CodigoARM64{
                        Texto = asm,
                        RegistroResultado = regPtr,
                        RegistroResultadoExtra = regLen,
                        Tipo = "string"
                    };

                }else{
                    return new CodigoARM64{
                        Texto = "",
                        RegistroResultado = etiqueta,
                        RegistroResultadoExtra = $"#{variable.ValorStr?.Length ?? 0}",
                        Tipo = "string",
                        MensajeLiteral = variable.ValorStr ?? ""
                    };

                }

            }

            if (variable.Tipo.StartsWith("slice<")){
                return new CodigoARM64{
                    Texto = "",
                    RegistroResultado = variable.EtiquetaMemoria,
                    EtiquetaLongitud = variable.EtiquetaLongitud,
                    Tipo = variable.Tipo
                };

            }

            return null;

        }

        public override object VisitNilExpresion([NotNull] AnalizadorLexicoParser.NilExpresionContext context){
            return null;

        }

        public override object VisitOperadorNegacion(AnalizadorLexicoParser.OperadorNegacionContext context){
            var valor = Visit(context.right) as CodigoARM64;
            string regSrc = valor.RegistroResultado.Replace("x", "w");
            string regRes = NuevoRegistroTmp().Replace("x", "w");
            string texto = valor.Texto;
            texto += $"    eor {regRes}, {regSrc}, #1\n";
            return new CodigoARM64{
                Texto = texto,
                RegistroResultado = regRes.Replace("w", "x"),
                Tipo = "bool"
            };

        }

        public override object VisitOperadorNegativo(AnalizadorLexicoParser.OperadorNegativoContext context){
            var valor = Visit(context.right) as CodigoARM64;
            if (valor.Tipo == "int"){
                string regRes = NuevoRegistroTmp();
                string texto = valor.Texto;
                texto += $"    neg {regRes}, {valor.RegistroResultado}\n";
                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "int",
                    ResultadoFloat = valor.ResultadoFloat.HasValue ? -valor.ResultadoFloat : null
                };

            }

            if (valor.Tipo == "float64"){
                string regRes = NuevoRegistroTmpFloat();
                string texto = valor.Texto;
                texto += $"    fneg {regRes}, {valor.RegistroResultado}\n";
                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "float64",
                    ResultadoFloat = valor.ResultadoFloat.HasValue ? -valor.ResultadoFloat : null
                };

            }

            return null;

        }

        public override object VisitMultiplicacionYdivision(AnalizadorLexicoParser.MultiplicacionYdivisionContext context){
            string operador = context.GetChild(1).GetText();
            var left = Visit(context.expr(0)) as CodigoARM64;
            var right = Visit(context.expr(1)) as CodigoARM64;

            // INT * INT, INT / INT, INT % INT
            if (left.Tipo == "int" && right.Tipo == "int"){
                string regRes = NuevoRegistroTmp();
                string texto = left.Texto + right.Texto;
                double? resVal = null;
                if (operador == "*"){
                    texto += $"    mul {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n";
                    if (left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue){
                        resVal = left.ResultadoFloat * right.ResultadoFloat;

                    }

                }else if (operador == "/"){
                    texto += $"    udiv {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n";
                    if (left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue && right.ResultadoFloat.Value != 0){
                        resVal = left.ResultadoFloat / right.ResultadoFloat;

                    }

                }else if (operador == "%"){
                    string regDiv = NuevoRegistroTmp();
                    texto += $"    udiv {regDiv}, {left.RegistroResultado}, {right.RegistroResultado}\n";
                    texto += $"    msub {regRes}, {regDiv}, {right.RegistroResultado}, {left.RegistroResultado}\n";
                    if (left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue && right.ResultadoFloat.Value != 0){
                        resVal = left.ResultadoFloat % right.ResultadoFloat;

                    }

                }

                return new CodigoARM64{ 
                    Texto = texto, 
                    RegistroResultado = regRes, 
                    Tipo = "int", 
                    ResultadoFloat = resVal 
                };

            }

            // FLOAT * FLOAT, FLOAT / FLOAT
            if (left.Tipo == "float64" && right.Tipo == "float64"){
                string regRes = NuevoRegistroTmpFloat();
                string texto = left.Texto + right.Texto;
                if (operador == "*"){
                    texto += $"    fmul {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n";

                }else if (operador == "/"){
                    texto += $"    fdiv {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n";

                }
                
                double? resVal = null;
                if (left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue){
                    if (operador == "*"){
                        resVal = left.ResultadoFloat * right.ResultadoFloat;

                    }else if (operador == "/" && right.ResultadoFloat.Value != 0){
                        resVal = left.ResultadoFloat / right.ResultadoFloat;

                    }

                }

                return new CodigoARM64{ 
                    Texto = texto, 
                    RegistroResultado = regRes, 
                    Tipo = "float64", 
                    ResultadoFloat = resVal 
                };

            }

            // INT * FLOAT, INT / FLOAT
            if (left.Tipo == "int" && right.Tipo == "float64"){
                string regLeftFloat = NuevoRegistroTmpFloat();
                string texto = left.Texto;
                texto += $"    scvtf {regLeftFloat}, {left.RegistroResultado}\n";
                texto += right.Texto;
                string regRes = NuevoRegistroTmpFloat();
                if (operador == "*"){
                    texto += $"    fmul {regRes}, {regLeftFloat}, {right.RegistroResultado}\n";

                }else if (operador == "/"){
                    texto += $"    fdiv {regRes}, {regLeftFloat}, {right.RegistroResultado}\n";

                }

                double? resVal = null;
                if (left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue){
                    if (operador == "*"){
                        resVal = left.ResultadoFloat * right.ResultadoFloat;

                    }else if (operador == "/" && right.ResultadoFloat.Value != 0){
                        resVal = left.ResultadoFloat / right.ResultadoFloat;

                    }

                }

                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "float64",
                    ResultadoFloat = resVal
                };

            }

            // FLOAT * INT, FLOAT / INT
            if (left.Tipo == "float64" && right.Tipo == "int"){
                string regRightFloat = NuevoRegistroTmpFloat();
                string texto = left.Texto + right.Texto;
                texto += $"    scvtf {regRightFloat}, {right.RegistroResultado}\n";
                string regRes = NuevoRegistroTmpFloat();
                if (operador == "*"){
                    texto += $"    fmul {regRes}, {left.RegistroResultado}, {regRightFloat}\n";

                }else if (operador == "/"){
                    texto += $"    fdiv {regRes}, {left.RegistroResultado}, {regRightFloat}\n";

                }

                double? resVal = null;
                if (left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue){
                    if (operador == "*"){
                        resVal = left.ResultadoFloat * right.ResultadoFloat;

                    }else if (operador == "/" && right.ResultadoFloat.Value != 0){
                        resVal = left.ResultadoFloat / right.ResultadoFloat;

                    }

                }

                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "float64",
                    ResultadoFloat = resVal
                };

            }

            return null;

        }

        public override object VisitSumaYresta(AnalizadorLexicoParser.SumaYrestaContext context){
            string operador = context.GetChild(1).GetText();
            var left = Visit(context.expr(0)) as CodigoARM64;
            var right = Visit(context.expr(1)) as CodigoARM64;
            if (left == null || right == null) return null;

            // INT + INT
            if (left.Tipo == "int" && right.Tipo == "int"){
                string regRes = NuevoRegistroTmp();
                string texto = (left.Texto ?? "") + (right.Texto ?? "");
                texto += operador == "+"
                    ? $"    add {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n"
                    : $"    sub {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n";

                double? result = left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue
                    ? (operador == "+" ? left.ResultadoFloat + right.ResultadoFloat : left.ResultadoFloat - right.ResultadoFloat)
                    : null;

                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "int",
                    ResultadoFloat = result
                };

            }

            // FLOAT + FLOAT
            if (left.Tipo == "float64" && right.Tipo == "float64"){
                string regRes = NuevoRegistroTmpFloat();
                string texto = (left.Texto ?? "") + (right.Texto ?? "");
                texto += operador == "+"
                    ? $"    fadd {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n"
                    : $"    fsub {regRes}, {left.RegistroResultado}, {right.RegistroResultado}\n";

                double? result = left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue
                    ? (operador == "+" ? left.ResultadoFloat + right.ResultadoFloat : left.ResultadoFloat - right.ResultadoFloat)
                    : null;

                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "float64",
                    ResultadoFloat = result
                };

            }

            // INT + FLOAT
            if (left.Tipo == "int" && right.Tipo == "float64"){
                string regLeftFloat = NuevoRegistroTmpFloat();
                string texto = (left.Texto ?? "") + $"    scvtf {regLeftFloat}, {left.RegistroResultado}\n" + (right.Texto ?? "");
                string regRes = NuevoRegistroTmpFloat();
                texto += operador == "+"
                    ? $"    fadd {regRes}, {regLeftFloat}, {right.RegistroResultado}\n"
                    : $"    fsub {regRes}, {regLeftFloat}, {right.RegistroResultado}\n";

                double? result = left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue
                    ? (operador == "+" ? left.ResultadoFloat + right.ResultadoFloat : left.ResultadoFloat - right.ResultadoFloat)
                    : null;

                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "float64",
                    ResultadoFloat = result
                };

            }

            // FLOAT + INT
            if (left.Tipo == "float64" && right.Tipo == "int"){
                string regRightFloat = NuevoRegistroTmpFloat();
                string texto = (left.Texto ?? "") + (right.Texto ?? "");
                texto += $"    scvtf {regRightFloat}, {right.RegistroResultado}\n";

                string regRes = NuevoRegistroTmpFloat();
                texto += operador == "+"
                    ? $"    fadd {regRes}, {left.RegistroResultado}, {regRightFloat}\n"
                    : $"    fsub {regRes}, {left.RegistroResultado}, {regRightFloat}\n";

                double? result = left.ResultadoFloat.HasValue && right.ResultadoFloat.HasValue
                    ? (operador == "+" ? left.ResultadoFloat + right.ResultadoFloat : left.ResultadoFloat - right.ResultadoFloat)
                    : null;

                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "float64",
                    ResultadoFloat = result
                };

            }

            // STRING + STRING
            if (left.Tipo == "string" && right.Tipo == "string" && operador == "+"){
                string limpioIzq = left.MensajeLiteral?.TrimEnd('\n') ?? "";
                string limpioDer = right.MensajeLiteral?.TrimEnd('\n') ?? "";
                string result = limpioIzq + limpioDer;

                if (!result.EndsWith("\n")){
                    result += "\n";
                
                }

                string valorEscapado = result.Replace("\\", "\\\\").Replace("\"", "\\\"");
                string etiqueta = $"msg{contadorMsg++}";
                string etqPtr = $"{etiqueta}_ptr";
                string etqLen = $"{etiqueta}_len";
                int longitud = Encoding.UTF8.GetByteCount(result);

                dataSection.AppendLine($"{etiqueta}: .asciz \"{valorEscapado}\"");
                dataSection.AppendLine($"{etqPtr}: .quad {etiqueta}");
                dataSection.AppendLine($"{etqLen}: .quad {longitud}");

                return new CodigoARM64{
                    Texto = "",
                    RegistroResultado = etqPtr,
                    RegistroResultadoExtra = etqLen,
                    Tipo = "string",
                    MensajeLiteral = result
                };

            }

            throw new Exception($"Tipos incompatibles para operador '{operador}': {left.Tipo} y {right.Tipo}");

        }

        public override object VisitOperadorLogico(AnalizadorLexicoParser.OperadorLogicoContext context){
            string operador = context.GetChild(1).GetText();
            var left = Visit(context.left) as CodigoARM64;
            var right = Visit(context.right) as CodigoARM64;

            string regLeft = left.RegistroResultado.Replace("x", "w");
            string regRight = right.RegistroResultado.Replace("x", "w");
            string regResW = NuevoRegistroTmp().Replace("x", "w");

            string texto = left.Texto + right.Texto;

            if (operador == "&&") {
                texto += $"    and {regResW}, {regLeft}, {regRight}\n";

            }else if (operador == "||"){
                texto += $"    orr {regResW}, {regLeft}, {regRight}\n";

            }

            return new CodigoARM64{
                Texto = texto,
                RegistroResultado = regResW.Replace("w", "x"),
                Tipo = "bool"
            };

        }

        public override object VisitOperadorRelacional(AnalizadorLexicoParser.OperadorRelacionalContext context){
            string operador = context.GetChild(1).GetText();
            var left = Visit(context.left) as CodigoARM64;
            var right = Visit(context.right) as CodigoARM64;
            string regRes = NuevoRegistroTmp();
            string texto = left.Texto + right.Texto;

            // INT/INT, RUNE/INT, RUNE/RUNE
            if ((left.Tipo == "int" || left.Tipo == "rune") && (right.Tipo == "int" || right.Tipo == "rune")){
                texto += $"    cmp {left.RegistroResultado}, {right.RegistroResultado}\n";
                string cond = operador switch{
                    "==" => "eq",
                    "!=" => "ne",
                    "<"  => "lt",
                    "<=" => "le",
                    ">"  => "gt",
                    ">=" => "ge",
                    _ => throw new Exception($"Operador relacional desconocido: {operador}")
                };

                texto += $"    cset {regRes}, {cond}\n";
                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "bool"
                };

            }

            // FLOAT/FLOAT
            if (left.Tipo == "float64" && right.Tipo == "float64"){
                texto += $"    fcmp {left.RegistroResultado}, {right.RegistroResultado}\n";
                string cond = operador switch{
                    "==" => "eq",
                    "!=" => "ne",
                    "<"  => "lt",
                    "<=" => "le",
                    ">"  => "gt",
                    ">=" => "ge",
                    _ => throw new Exception($"Operador relacional desconocido: {operador}")
                };

                texto += $"    cset {regRes}, {cond}\n";
                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "bool"
                };

            }

            // INT/FLOAT, RUNE/FLOAT, FLOAT/INT, FLOAT/RUNE
            if ((left.Tipo == "float64" && (right.Tipo == "int" || right.Tipo == "rune")) || ((left.Tipo == "int" || left.Tipo == "rune") && right.Tipo == "float64")){
                string regLeftF = left.Tipo == "float64" ? left.RegistroResultado : NuevoRegistroTmpFloat();
                string regRightF = right.Tipo == "float64" ? right.RegistroResultado : NuevoRegistroTmpFloat();
                if (left.Tipo != "float64"){
                    texto += $"    scvtf {regLeftF}, {left.RegistroResultado}\n";

                }

                if (right.Tipo != "float64"){
                    texto += $"    scvtf {regRightF}, {right.RegistroResultado}\n";

                }

                texto += $"    fcmp {regLeftF}, {regRightF}\n";
                string cond = operador switch{
                    "==" => "eq",
                    "!=" => "ne",
                    "<"  => "lt",
                    "<=" => "le",
                    ">"  => "gt",
                    ">=" => "ge",
                    _ => throw new Exception($"Operador relacional desconocido: {operador}")
                };

                texto += $"    cset {regRes}, {cond}\n";
                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "bool"
                };

            }

            // STRINGS/BOOLS (solo == y !=)
            if (left.Tipo == "bool" && right.Tipo == "bool"){
                texto += $"    cmp {left.RegistroResultado}, {right.RegistroResultado}\n";
                string cond = operador switch{
                    "==" => "eq",
                    "!=" => "ne",
                    _ => throw new Exception($"Operador relacional desconocido para bool: {operador}")
                };

                texto += $"    cset {regRes}, {cond}\n";
                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regRes,
                    Tipo = "bool"
                };

            }else if (left.Tipo == "string" && right.Tipo == "string"){
                string regPtrL = NuevoRegistroTmp(); 
                string regPtrR = NuevoRegistroTmp(); 
                string regByteL = NuevoRegistroTmp().Replace("x", "w");
                string regByteR = NuevoRegistroTmp().Replace("x", "w");
                string regResStr = NuevoRegistroTmp(); 
                string etqLoop = NuevaEtiqueta("cmp_str_loop");
                string etqNe   = NuevaEtiqueta("cmp_str_ne");
                string etqEq   = NuevaEtiqueta("cmp_str_eq");
                string etqFin  = NuevaEtiqueta("cmp_str_fin");

                texto += left.Texto;
                texto += right.Texto;

                if (!EsRegistro(left.RegistroResultado)){
                    texto += $"    ldr {regPtrL}, ={left.RegistroResultado}\n";

                }else{
                    texto += $"    mov {regPtrL}, {left.RegistroResultado}\n";

                }

                if (!EsRegistro(right.RegistroResultado)){
                    texto += $"    ldr {regPtrR}, ={right.RegistroResultado}\n";

                }else{
                    texto += $"    mov {regPtrR}, {right.RegistroResultado}\n";

                }

                texto += $"    mov x14, {regPtrL}\n";
                texto += $"    mov x15, {regPtrR}\n";

                texto += $"{etqLoop}:\n";
                texto += $"    ldrb {regByteL}, [x14], #1\n";
                texto += $"    ldrb {regByteR}, [x15], #1\n";
                texto += $"    cmp {regByteL}, {regByteR}\n";
                texto += $"    bne {etqNe}\n";
                texto += $"    cmp {regByteL}, #0\n";
                texto += $"    bne {etqLoop}\n";
                texto += $"    b {etqEq}\n";

                if (operador == "=="){
                    texto += $"{etqEq}:\n";
                    texto += $"    mov {regResStr}, #1\n";
                    texto += $"    b {etqFin}\n";
                    texto += $"{etqNe}:\n";
                    texto += $"    mov {regResStr}, #0\n";

                }else{
                    texto += $"{etqEq}:\n";
                    texto += $"    mov {regResStr}, #0\n";
                    texto += $"    b {etqFin}\n";
                    texto += $"{etqNe}:\n";
                    texto += $"    mov {regResStr}, #1\n";

                }

                texto += $"{etqFin}:\n";

                return new CodigoARM64{
                    Texto = texto,
                    RegistroResultado = regResStr,
                    Tipo = "bool"
                };

            }

            return null;

        }

        public override object VisitSliceIndex(AnalizadorLexicoParser.SliceIndexContext context)
        {
            var entorno = pilaEntornosARM64.Peek();
            var codSlice = Visit(context.expr(0)) as CodigoARM64;
            var codValor = Visit(context.expr(1)) as CodigoARM64;
            string tipoBase = codSlice.Tipo.Substring(6, codSlice.Tipo.Length - 7);

            string regPtr = NuevoRegistroTmp();
            string regLen = NuevoRegistroTmp();
            string regIndice = NuevoRegistroTmp();
            string regTmp = NuevoRegistroTmp();

            string etqLoop = NuevaEtiqueta("index_loop");
            string etqFin = NuevaEtiqueta("index_end");
            string etqFail = NuevaEtiqueta("index_not_found");
            string etqMatch = NuevaEtiqueta("index_found");
            string etqContinue = NuevaEtiqueta("index_continue");

            string etiquetaMenosUno = "index_menos_uno";
            if (!dataSection.ToString().Contains(etiquetaMenosUno))
                dataSection.AppendLine($"{etiquetaMenosUno}: .quad -1");

            var asm = new StringBuilder();
            asm.Append(codSlice.Texto);
            asm.Append(codValor.Texto);

            asm.AppendLine($"    ldr {regPtr}, ={codSlice.RegistroResultado}");
            asm.AppendLine($"    ldr {regPtr}, [{regPtr}]");
            asm.AppendLine($"    ldr {regLen}, ={codSlice.EtiquetaLongitud}");
            asm.AppendLine($"    ldr {regLen}, [{regLen}]");
            asm.AppendLine($"    mov {regIndice}, #0");

            if (tipoBase == "int") {
                string regVal = codValor.RegistroResultado;
                string regValSlice = NuevoRegistroTmp();
                asm.AppendLine($"{etqLoop}:");
                asm.AppendLine($"    cmp {regIndice}, {regLen}");
                asm.AppendLine($"    bge {etqFail}");
                asm.AppendLine($"    mov {regTmp}, {regIndice}");
                asm.AppendLine($"    lsl {regTmp}, {regTmp}, #3");
                asm.AppendLine($"    add {regTmp}, {regPtr}, {regTmp}");
                asm.AppendLine($"    ldr {regValSlice}, [{regTmp}]");
                asm.AppendLine($"    cmp {regValSlice}, {regVal}");
                asm.AppendLine($"    beq {etqMatch}");
                asm.AppendLine($"    add {regIndice}, {regIndice}, #1");
                asm.AppendLine($"    b {etqLoop}");
            }
            else if (tipoBase == "float64") {
                asm.AppendLine($"    fmov d0, {codValor.RegistroResultado}");
                asm.AppendLine($"{etqLoop}:");
                asm.AppendLine($"    cmp {regIndice}, {regLen}");
                asm.AppendLine($"    bge {etqFail}");
                asm.AppendLine($"    mov {regTmp}, {regIndice}");
                asm.AppendLine($"    lsl {regTmp}, {regTmp}, #3");
                asm.AppendLine($"    add {regTmp}, {regPtr}, {regTmp}");
                asm.AppendLine($"    ldr d1, [{regTmp}]");
                asm.AppendLine($"    fcmp d0, d1");
                asm.AppendLine($"    beq {etqMatch}");
                asm.AppendLine($"    add {regIndice}, {regIndice}, #1");
                asm.AppendLine($"    b {etqLoop}");
            }
            else if (tipoBase == "string") {
                string regStrPtr = codValor.RegistroResultado;
                string regStrLen = codValor.RegistroResultadoExtra;
                string regStrActual = NuevoRegistroTmp();
                string regStrLenAct = NuevoRegistroTmp();
                string regByte1 = NuevoRegistroTmp().Replace("x", "w");
                string regByte2 = NuevoRegistroTmp().Replace("x", "w");
                string regCounter = NuevoRegistroTmp();
                string etqLoopBytes = NuevaEtiqueta("cmp_bytes");

                asm.AppendLine($"{etqLoop}:");
                asm.AppendLine($"    cmp {regIndice}, {regLen}");
                asm.AppendLine($"    bge {etqFail}");
                asm.AppendLine($"    mov {regTmp}, {regIndice}");
                asm.AppendLine($"    lsl {regTmp}, {regTmp}, #4");
                asm.AppendLine($"    add {regTmp}, {regPtr}, {regTmp}");
                asm.AppendLine($"    ldr {regStrActual}, [{regTmp}]");
                asm.AppendLine($"    ldr {regStrLenAct}, [{regTmp}, #8]");
                asm.AppendLine($"    cmp {regStrLenAct}, {regStrLen}");
                asm.AppendLine($"    bne {etqContinue}");
                asm.AppendLine($"    mov {regCounter}, #0");
                asm.AppendLine($"{etqLoopBytes}:");
                asm.AppendLine($"    cmp {regCounter}, {regStrLen}");
                asm.AppendLine($"    bge {etqMatch}");
                asm.AppendLine($"    ldrb {regByte1}, [{regStrActual}, {regCounter}]");
                asm.AppendLine($"    ldrb {regByte2}, [{regStrPtr}, {regCounter}]");
                asm.AppendLine($"    cmp {regByte1}, {regByte2}");
                asm.AppendLine($"    bne {etqContinue}");
                asm.AppendLine($"    add {regCounter}, {regCounter}, #1");
                asm.AppendLine($"    b {etqLoopBytes}");
                asm.AppendLine($"{etqContinue}:");
                asm.AppendLine($"    add {regIndice}, {regIndice}, #1");
                asm.AppendLine($"    b {etqLoop}");
            }
            else if (tipoBase == "rune") {
                string regVal = codValor.RegistroResultado.Replace("x", "w");
                string regValSlice = "w1";
                asm.AppendLine($"{etqLoop}:");
                asm.AppendLine($"    cmp {regIndice}, {regLen}");
                asm.AppendLine($"    bge {etqFail}");
                asm.AppendLine($"    mov {regTmp}, {regIndice}");
                asm.AppendLine($"    lsl {regTmp}, {regTmp}, #2");
                asm.AppendLine($"    add {regTmp}, {regPtr}, {regTmp}");
                asm.AppendLine($"    ldr {regValSlice}, [{regTmp}]");
                asm.AppendLine($"    cmp {regValSlice}, {regVal}");
                asm.AppendLine($"    beq {etqMatch}");
                asm.AppendLine($"    add {regIndice}, {regIndice}, #1");
                asm.AppendLine($"    b {etqLoop}");
            }
            else if (tipoBase == "bool") {
                string regVal = codValor.RegistroResultado.Replace("x", "w");
                string regValSlice = "w1";
                asm.AppendLine($"{etqLoop}:");
                asm.AppendLine($"    cmp {regIndice}, {regLen}");
                asm.AppendLine($"    bge {etqFail}");
                asm.AppendLine($"    add {regTmp}, {regPtr}, {regIndice}");
                asm.AppendLine($"    ldrb {regValSlice}, [{regTmp}]");
                asm.AppendLine($"    cmp {regValSlice}, {regVal}");
                asm.AppendLine($"    beq {etqMatch}");
                asm.AppendLine($"    add {regIndice}, {regIndice}, #1");
                asm.AppendLine($"    b {etqLoop}");
            }

            asm.AppendLine($"{etqMatch}:");
            asm.AppendLine($"    mov x0, {regIndice}");
            asm.AppendLine($"    b {etqFin}");
            asm.AppendLine($"{etqFail}:");
            asm.AppendLine($"    ldr x0, ={etiquetaMenosUno}");
            asm.AppendLine($"    ldr x0, [x0]");
            asm.AppendLine($"{etqFin}:");

            return new CodigoARM64 {
                Texto = asm.ToString(),
                RegistroResultado = "x0",
                Tipo = "int"
            };
        }

        public override object VisitLen(AnalizadorLexicoParser.LenContext context){
            var cod = Visit(context.expr()) as CodigoARM64;
            if (cod == null || !cod.Tipo.StartsWith("slice<")){
                return new CodigoARM64{
                    Texto = "// Error: len solo se puede aplicar a slices\n",
                    RegistroResultado = "xzr",
                    Tipo = "int"
                };

            }

            string regLen = NuevoRegistroTmp();
            string regEtiqueta = cod.EtiquetaLongitud;

            var asm = new StringBuilder();
            asm.Append(cod.Texto);
            asm.AppendLine($"    ldr {regLen}, ={regEtiqueta}");
            asm.AppendLine($"    ldr {regLen}, [{regLen}]");

            return new CodigoARM64
            {
                Texto = asm.ToString(),
                RegistroResultado = regLen,
                Tipo = "int"
            };

        }

        public override object VisitSliceAccess(AnalizadorLexicoParser.SliceAccessContext context){
            var entorno = pilaEntornosARM64.Peek();
            string nombreSlice = context.ID().GetText();
            var variable = entorno.ObtenerVariable(nombreSlice);
            string tipoBase = variable.Tipo.Substring(6, variable.Tipo.Length - 7);
            int sizeTipo = tipoBase switch{
                "int" or "float64" => 8,
                "rune" => 4,
                "bool" => 1,
                "string" => 16,
                _ => 8
            };

            var asm = new StringBuilder();
            var codIndex = Visit(context.expr()) as CodigoARM64;
            string regIndex = codIndex.RegistroResultado;

            string regLenPtr = NuevoRegistroTmp();
            string regLen = NuevoRegistroTmp();
            asm.Append(codIndex.Texto);

            asm.AppendLine($"    ldr {regLenPtr}, ={variable.EtiquetaLongitud}");
            asm.AppendLine($"    ldr {regLen}, [{regLenPtr}]");
            asm.AppendLine($"    cmp {regIndex}, {regLen}");
            asm.AppendLine($"    bge acceso_fuera_rango");
            asm.AppendLine($"    cmp {regIndex}, #0");
            asm.AppendLine($"    blt acceso_fuera_rango");

            string regBase = NuevoRegistroTmp();
            string regOffset = NuevoRegistroTmp();
            string regPtr = NuevoRegistroTmp();
            string regValor = tipoBase == "float64" ? NuevoRegistroTmpFloat() : NuevoRegistroTmp();

            asm.AppendLine($"    ldr {regBase}, ={variable.EtiquetaMemoria}");
            asm.AppendLine($"    ldr {regBase}, [{regBase}]");
            asm.AppendLine($"    lsl {regOffset}, {regIndex}, #{(int)Math.Log2(sizeTipo)}");
            asm.AppendLine($"    add {regPtr}, {regBase}, {regOffset}");

            if (tipoBase == "float64"){
                asm.AppendLine($"    ldr {regValor}, [{regPtr}]");
                return new CodigoARM64{
                    Texto = asm.ToString(),
                    RegistroResultado = regValor,
                    Tipo = "float64"
                };

            }else if (tipoBase == "string"){
                string regStrPtr = NuevoRegistroTmp();
                string regStrLen = NuevoRegistroTmp();
                asm.AppendLine($"    ldr {regStrPtr}, [{regPtr}]");
                asm.AppendLine($"    ldr {regStrLen}, [{regPtr}, #8]");
                return new CodigoARM64{
                    Texto = asm.ToString(),
                    RegistroResultado = regStrPtr,
                    RegistroResultadoExtra = regStrLen,
                    Tipo = "string"
                };

            }else if (sizeTipo == 1){
                asm.AppendLine($"    ldrb {ARegW(regValor)}, [{regPtr}]");

            }else if (sizeTipo == 4){
                asm.AppendLine($"    ldr {ARegW(regValor)}, [{regPtr}]");

            }else{
                asm.AppendLine($"    ldr {regValor}, [{regPtr}]");

            }

            return new CodigoARM64 {
                Texto = asm.ToString(),
                RegistroResultado = regValor,
                Tipo = tipoBase
            };

        }

        public override object VisitStrconvAtoi(AnalizadorLexicoParser.StrconvAtoiContext context){
            var cod = Visit(context.expr()) as CodigoARM64;
            if (cod == null || cod.Tipo != "string"){
                return null;
            
            }

            var codigo = new StringBuilder();
            codigo.Append(cod.Texto);

            codigo.AppendLine($"    mov x0, {cod.RegistroResultado}");
            codigo.AppendLine($"    mov x1, {cod.RegistroResultadoExtra}");
            codigo.AppendLine($"    bl atoi");

            return new CodigoARM64{
                Texto = codigo.ToString(),
                Tipo = "int",
                RegistroResultado = "x0"
            };

        }

        public override object VisitStrconvParseFloat(AnalizadorLexicoParser.StrconvParseFloatContext context){
            var cod = Visit(context.expr()) as CodigoARM64;
            if (cod == null) return null;
            var resultado = new CodigoARM64{
                Texto = cod.Texto,
                Tipo = "float64",
                RegistroResultado = "d0"
            };

            if (!string.IsNullOrEmpty(cod.RegistroResultado)){
                resultado.Texto += $"    mov x0, {cod.RegistroResultado}\n";

            }

            if (!string.IsNullOrEmpty(cod.RegistroResultadoExtra)){
                resultado.Texto += $"    mov x1, {cod.RegistroResultadoExtra}\n";

            }

            resultado.Texto += "    bl parse_float\n";
            return resultado;

        }

        public override object VisitTypeOf(AnalizadorLexicoParser.TypeOfContext context){
            var cod = Visit(context.expr()) as CodigoARM64;
            if (cod == null) return null;
            string tipoDetectado = cod.Tipo;

            // Generar literal en sección .data
            string nombreMsg = $"msg_typeof_{contadorMsg++}";
            string literalTipo = tipoDetectado switch{
                "int" => "int",
                "float64" => "float64",
                "string" => "string",
                "bool" => "bool",
                "rune" => "rune",
                _ when tipoDetectado.StartsWith("slice<") => tipoDetectado,
                _ => "unknown"
            };

            dataSection.AppendLine($"{nombreMsg}: .asciz \"{literalTipo}\"");
            var resultado = new CodigoARM64{
                Texto = cod.Texto +
                        $"\n    ldr x0, ={nombreMsg}" +
                        $"\n    mov x1, #{literalTipo.Length}\n",
                Tipo = "string",
                RegistroResultado = "x0",
                RegistroResultadoExtra = "x1"
            };

            return resultado;

        }

        // GET FINAL CODE
        public string GetCodigo(){
            var sb = new StringBuilder();
            // --- Sección .data ---
            sb.AppendLine(".section .data");
            sb.AppendLine("buffer_int: .skip 32");
            sb.AppendLine("msg_punto: .asciz \".\"");
            sb.AppendLine("zero_char: .asciz \"0\"");
            sb.AppendLine("msg_nl: .asciz \"\\n\"");
            sb.AppendLine("msg_menos: .asciz \"-\"");
            sb.AppendLine("flt_100: .double 100.0");
            if (!dataSection.ToString().Contains("msgTrue:")){
                dataSection.AppendLine("msgTrue: .asciz \"True\\n\"");

            }

            if (!dataSection.ToString().Contains("msgFalse:")){
                dataSection.AppendLine("msgFalse: .asciz \"False\\n\"");

            }

            if (!dataSection.ToString().Contains("rune_tmp_char:")){
                dataSection.AppendLine("rune_tmp_char: .asciz \" \"");

            }

            if (!dataSection.ToString().Contains("msg_acceso_fuera_rango:")){
                dataSection.AppendLine("msg_acceso_fuera_rango: .asciz \"Index fuera de rango\"");

            }

            sb.Append(dataSection.ToString());
            // --- Sección .text y punto de entrada ---
            sb.AppendLine(".section .text");
            sb.AppendLine(".global _start");
            sb.Append(funcSection.ToString());
            sb.AppendLine("_start:");
            sb.Append(textSection.ToString());

            sb.AppendLine("    mov x8, 93   // exit syscall");
            sb.AppendLine("    mov x0, 0");
            sb.AppendLine("    svc 0");

            // --- Rutina print_int ---
            sb.AppendLine(@"
            print_int:
                // Guardar registros que se usarán
                stp x29, x30, [sp, #-16]!
                mov x29, sp
                stp x1, x2, [sp, #-16]!
                stp x3, x4, [sp, #-16]!
                stp x5, x6, [sp, #-16]!

                cmp x0, #0
                bge .Lpositive_int
                mov x8, #64
                ldr x1, =msg_menos
                mov x2, #1
                mov x0, #1
                svc 0
                neg x0, x0

            .Lpositive_int:
                ldr x2, =buffer_int
                add x2, x2, #32    
                mov x3, #0        
                mov x6, #10        

            .Lloop:
                udiv x4, x0, x6     
                msub x5, x4, x6, x0  
                add x5, x5, #48    
                sub x2, x2, #1      
                strb w5, [x2]      
                mov x0, x4      
                add x3, x3, #1     
                cmp x0, #0
                bne .Lloop

                mov x0, #1
                mov x1, x2
                mov x2, x3
                mov x8, #64
                svc 0

                ldp x5, x6, [sp], #16
                ldp x3, x4, [sp], #16
                ldp x1, x2, [sp], #16
                ldp x29, x30, [sp], #16
                ret
            ");

            // --- Rutina print_float ---
            sb.AppendLine(@"
            print_float:
                stp x29, x30, [sp, #-16]!
                mov x29, sp
                stp x1, x10, [sp, #-16]!
                sub sp, sp, #32
                stp d1, d2, [sp]
                str d3, [sp, #16]

                fcmp    d0, #0.0
                bge     .Lpos
                mov     x0, #1
                ldr     x1, =msg_menos
                mov     x2, #1
                mov     x8, #64
                svc     0
                fneg    d0, d0
            .Lpos:
                fcvtzs  x1, d0
                scvtf   d1, x1
                mov     x0, x1
                bl      print_int

                mov     x0, #1
                ldr     x1, =msg_punto
                mov     x2, #1
                mov     x8, #64
                svc     0

                fsub    d2, d0, d1
                ldr     x10, =flt_100
                ldr     d3, [x10]
                fmul    d2, d2, d3
                frintz  d2, d2
                fcvtzu  x3, d2

                mov     x0, x3
                bl      print_int

                ldr d3, [sp, #16]
                ldp d1, d2, [sp]
                add sp, sp, #32
                ldp x1, x10, [sp], #16
                ldp x29, x30, [sp], #16
                ret
            ");

            // --- Rutina print_string ---
            sb.AppendLine(@"
            print_string:
                mov x0, 1
                mov x8, 64
                svc 0
                ret
            ");

            // --- Rutina print_bool ---
            sb.AppendLine(@"
            print_bool:
                cmp x0, #0
                beq .Lfalse
            .Ltrue:
                ldr x1, =msgTrue
                mov x2, #5
                b .Lprint
            .Lfalse:
                ldr x1, =msgFalse
                mov x2, #6
            .Lprint:
                mov x0, #1
                mov x8, #64
                svc 0
                ret
            ");

            // --- Rutina print_rune ---
            sb.AppendLine(@"
            print_rune:
                ldr x1, =rune_tmp_char
                strb w0, [x1]
                mov x0, 1
                ldr x1, =rune_tmp_char
                mov x2, 1
                mov x8, 64
                svc 0
                ret
            ");

            // --- Rutina print_newline ---
            sb.AppendLine(@"
            print_newline:
                mov x0, #1
                ldr x1, =msg_nl
                mov x2, #1
                mov x8, #64
                svc 0
                ret
            ");

                // --- Rutina acceso_fuera_rango ---
            sb.AppendLine(@"
            acceso_fuera_rango:
                mov x0, 1
                ldr x1, =msg_nl
                mov x2, 1
                mov x8, 64
                svc 0
                ldr x1, =msg_acceso_fuera_rango
                mov x2, 24
                mov x8, 64
                svc 0
                mov x8, 93
                mov x0, 1
                svc 0
                ret
            ");

            // --- Rutina atoi ---
            sb.AppendLine(@"
            atoi:
                stp x29, x30, [sp, #-16]!
                mov x29, sp
                stp x2, x3, [sp, #-16]!

                // x0 = ptr, x1 = len
                mov x2, #0    
                mov x3, #0 

            atoi_loop:
                cmp x3, x1
                bge atoi_done

                ldrb w4, [x0, x3]    
                sub w4, w4, #'0'     
                cmp w4, #9
                bgt atoi_done
                cmp w4, #0
                blt atoi_done

                mov x5, #10              
                mul x2, x2, x5    
                add x2, x2, x4      
                add x3, x3, #1
                b atoi_loop

            atoi_done:
                mov x0, x2          
                ldp x2, x3, [sp], #16
                ldp x29, x30, [sp], #16
                ret     
            ");

            // --- Rutina parse_float ---
            sb.AppendLine(@"
            parse_float:
                stp x29, x30, [sp, #-16]!
                mov x29, sp
                stp x2, x3, [sp, #-16]!
                stp d1, d2, [sp, #-16]!

                mov x2, #0 
                mov x3, #0 
                mov w4, #0  
                mov x5, #0  
                mov x6, #0   
                mov x9, #10  

            1:  cmp x2, x1
                bge 2f
                ldrb w7, [x0, x2]
                cmp w7, #'.'
                beq 3f
                cmp w7, #'0'
                blt 2f
                cmp w7, #'9'
                bgt 2f
                sub w7, w7, #'0' 
                cmp w4, #0
                beq 4f         

                mul x5, x5, x9
                add x5, x5, x7
                add x6, x6, #1
                b 5f

            4:  
                mul x3, x3, x9
                add x3, x3, x7
                b 5f
            3:  
                mov w4, #1

            5:  add x2, x2, #1
                b 1b
            2:  
                scvtf d1, x3  
                scvtf d2, x5   
                mov x7, #1
            6:  cmp x6, #0
                beq 7f
                mul x7, x7, x9
                sub x6, x6, #1
                b 6b
            7:
                scvtf d3, x7
                fdiv d2, d2, d3
                fadd d0, d1, d2   
                ldp d1, d2, [sp], #16
                ldp x2, x3, [sp], #16
                ldp x29, x30, [sp], #16
                ret
            ");

            return sb.ToString();

        }

    }


}
