using Proyecto_1.AnalizadorLexico;


namespace Proyecto_1.ExcepcionesARM64{
    public class FuncionARM64{
        public string Nombre;
        public List<(string nombre, string tipo)> Parametros;
        public string TipoRetorno;
        public AnalizadorLexicoParser.ListaInstrContext Cuerpo;

    }


}