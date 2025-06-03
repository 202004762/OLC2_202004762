namespace Proyecto_1.Excepciones{
    public class Entorno {

        public String nombre { get; set; }
        public Dictionary<string, Simbolos> variables { get; set; }
        public Entorno? punteroAPadre { get; set; }
        public Entorno? punteroASiguiente { get; set; }
        public int ultimaPosicion { get; set; }

        public Entorno(String nombre, Entorno? entorno){
            this.nombre = nombre;
            this.variables = new Dictionary<string, Simbolos>();
            this.punteroAPadre = entorno;
            this.ultimaPosicion = 0;

        }

        public Simbolos? buscarVariable(string nombre){
            for (Entorno? ent = this; ent != null; ent = ent.punteroAPadre){
                if (ent.variables.ContainsKey(nombre)){
                    //Console.WriteLine("La estoy encontrando en padre");
                    return ent.variables[nombre];

                }

            }

            for (Entorno? ent = this; ent != null; ent = ent.punteroASiguiente){
                if (ent.variables.ContainsKey(nombre)){
                    //Console.WriteLine("La estoy encontrando en siguiente");
                    return ent.variables[nombre];  

                }

            }

            return null;

        }

        public void guardarVariable(string nombre, Simbolos simbolo){
            if (variables.ContainsKey(nombre)){
                Console.WriteLine("Variable " + nombre + " ya existe");

            }else{
                variables.Add(nombre, simbolo);

            }

        }

        public void actualizarValorSimbolo(string nombre, object valor, Func<object, string> determinarTipo, List<Errores> erroresSemanticos){
            for (Entorno? ent = this; ent != null; ent = ent.punteroAPadre){
                if (ent.variables.ContainsKey(nombre)){
                    Simbolos simbolo = ent.variables[nombre];
                    string tipoSimbolo = simbolo.tipo;
                    string tipoValor = determinarTipo(valor);
                    if (tipoSimbolo == "float64" && tipoValor == "int"){
                        simbolo.valor = Convert.ToDouble(valor);

                    }else if (tipoSimbolo == "int" && tipoValor == "float64"){
                        simbolo.valor = Convert.ToInt32(valor);

                    }else if (tipoSimbolo.StartsWith("slice<") && tipoValor.StartsWith("slice<")){
                        if (tipoSimbolo != tipoValor){
                            erroresSemanticos.Add(new Errores("Semantico", $"No se puede asignar un slice de tipo '{tipoValor}' a un slice de tipo '{tipoSimbolo}'.", 0, 0));
                            return;

                        }

                        if (valor is List<object> listaValores && simbolo.valor is List<object> listaDestino){
                            listaDestino.Clear();
                            listaDestino.AddRange(listaValores);

                        }else{
                            simbolo.valor = valor;

                        }

                    }else if (tipoSimbolo.StartsWith("matriz<") && tipoValor.StartsWith("matriz<")){
                        if (tipoSimbolo != tipoValor){
                            erroresSemanticos.Add(new Errores("Semantico", $"No se puede asignar una matriz de tipo '{tipoValor}' a una de tipo '{tipoSimbolo}'.", 0, 0));
                            return;

                        }

                        if (valor is List<object> nuevaMatriz && simbolo.valor is List<object> matrizDestino){
                            matrizDestino.Clear();
                            matrizDestino.AddRange(nuevaMatriz);

                        } else {
                            simbolo.valor = valor;

                        }

                    }else if (tipoSimbolo != tipoValor){
                        erroresSemanticos.Add(new Errores("Semantico", $"No se puede asignar un valor de tipo '{tipoValor}' a la variable '{nombre}' de tipo '{tipoSimbolo}'.", 0, 0));
                        return;

                    }else{
                        simbolo.valor = valor;

                    }

                    ent.variables[nombre] = simbolo;
                    Console.WriteLine($"Variable '{nombre}' se actualizo al valor: {valor}");
                    return;

                }

            }

            erroresSemanticos.Add(new Errores("Semantico", $"No se pudo encontrar la variable '{nombre}' en el entorno actual ni en sus entornos superiores.", 0, 0));

        }


    }

}