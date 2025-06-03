using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

namespace Proyecto_1.Excepciones{
    public class SyntaxErrorListener : IAntlrErrorListener<IToken>{
        public List<Errores> erroresSintacticos = new List<Errores>();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e){
            erroresSintacticos.Add(new Errores("Sintactico", msg, line, charPositionInLine));

        }


    }

}
