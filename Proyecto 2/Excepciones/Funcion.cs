using Proyecto_1.AnalizadorLexico;
namespace Proyecto_1.Excepciones{
    public class Funcion{

        public string identificador;
        public Dictionary<string, Simbolos> parametros { get; set; }
        public AnalizadorLexicoParser.ListaInstrContext instrucciones { get; set; }
        public List<String> tipoParametros { get; set; }
        public Simbolos retorno { get; set; }
        public Entorno? entorno { get; set; }

        public Funcion(string identificador, Dictionary<string, Simbolos> parametros, AnalizadorLexicoParser.ListaInstrContext instrucciones, List<String> tipoParametros, Simbolos retorno) {
            this.identificador = identificador;
            this.parametros = parametros;
            this.instrucciones = instrucciones;
            this.tipoParametros = tipoParametros;
            this.retorno = retorno;

        }


    }

}