using Antlr4.Runtime;
using System.Collections.Generic;
using System.IO;

namespace Proyecto_1.Excepciones{
    public class LexicalErrorListener : IAntlrErrorListener<int>{
        public List<Errores> erroresLexicos = new List<Errores>();

        public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e){
            erroresLexicos.Add(new Errores("Lexico", msg, line, charPositionInLine));

        }

    }


}
