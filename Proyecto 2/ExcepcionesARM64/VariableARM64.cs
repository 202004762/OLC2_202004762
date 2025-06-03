namespace Proyecto_1.ExcepcionesARM64{
    public class VariableARM64{
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string EtiquetaMemoria { get; set; }
        public string EtiquetaLongitud { get; set; } = "";
        public string ValorStr { get; set; }
        public int CantidadElementos { get; set; } = 0;

        public VariableARM64(string nombre, string tipo, string etiquetaMemoria, string valorStr = "", string etiquetaLongitud = ""){
            Nombre = nombre;
            Tipo = tipo;
            EtiquetaMemoria = etiquetaMemoria;
            ValorStr = valorStr;
            EtiquetaLongitud = etiquetaLongitud;

        }

    }


}
