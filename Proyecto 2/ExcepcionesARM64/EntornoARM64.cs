namespace Proyecto_1.ExcepcionesARM64{
    public class EntornoARM64{
        public Dictionary<string, VariableARM64> Variables { get; set; } = new();
        public EntornoARM64 Padre { get; set; } = null;
        public EntornoARM64(EntornoARM64 padre = null){
            Padre = padre;

        }

        public VariableARM64 ObtenerVariable(string nombre){
            if (Variables.ContainsKey(nombre)){
                return Variables[nombre];

            }else if (Padre != null){
                return Padre.ObtenerVariable(nombre);

            }else{
                return null;

            }

        }

        public bool ContieneEnEsteAmbito(string nombre)
            => Variables.ContainsKey(nombre);

        public void Declarar(string nombre, VariableARM64 variable)
            => Variables[nombre] = variable;

    }


}
