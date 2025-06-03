namespace Proyecto_1.Excepciones{
    public class Errores{
        public string Tipo{ get; set; }
        public string Descripcion{ get; set; }
        public int Linea{ get; set; }
        public int Columna{ get; set; }

        public Errores(string tipo, string descripcion, int linea, int columna){
            Tipo = tipo;
            Descripcion = descripcion;
            Linea = linea;
            Columna = columna;

        }

        public override string ToString(){
            return $"[{Tipo}] Linea {Linea}, Columna {Columna}: {Descripcion}";

        }


    }

}
