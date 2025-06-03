namespace Proyecto_1.Excepciones{

    public class Simbolos {

        public String identificador;
        public String tipo;
        public Object valor;

        public Simbolos(String identificador, String tipo, Object valor){
            this.identificador = identificador;
            this.tipo = tipo;
            this.valor = valor;

        }

        public Simbolos(String identificador, String tipo) {
            this.identificador = identificador;
            this.tipo = tipo;

        }

    }

}