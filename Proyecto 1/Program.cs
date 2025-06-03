using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Antlr4.Runtime;
using Proyecto_1;
using Proyecto_1.AnalizadorLexico;
using Proyecto_1.Excepciones;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Activar CORS para permitir conexión desde React
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Aplicar política de CORS
app.UseCors("PermitirTodo");

// Ruta POST /analizar
app.MapPost("/analizar", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var entrada = await reader.ReadToEndAsync();

    var resultado = new List<string>();
    string ast = "";

    try
    {
        var entradaParseada = new AntlrInputStream(entrada);

        var analisisLexico = new AnalizadorLexicoLexer(entradaParseada);
        var lexicalErrorListener = new LexicalErrorListener();
        analisisLexico.RemoveErrorListeners();
        analisisLexico.AddErrorListener(lexicalErrorListener);

        var listaTokens = new CommonTokenStream(analisisLexico);
        var analisisSintactico = new AnalizadorLexicoParser(listaTokens);
        var syntaxErrorListener = new SyntaxErrorListener();
        analisisSintactico.RemoveErrorListeners();
        analisisSintactico.AddErrorListener(syntaxErrorListener);

        var arbol = analisisSintactico.inicio();
        ast = arbol.ToStringTree(analisisSintactico);

        Entorno entornoInicial = new Entorno("main", null);
        Visitor visitor = new Visitor(entornoInicial);
        visitor.Visit(arbol);

        var simbolos = visitor.ObtenerTablaDeSimbolos();


        resultado.AddRange(lexicalErrorListener.erroresLexicos.Select(e => e.ToString()));
        resultado.AddRange(syntaxErrorListener.erroresSintacticos.Select(e => e.ToString()));
        resultado.AddRange(visitor.erroresSemanticos.Select(e => e.ToString()));
        resultado.AddRange(visitor.listaSalida.Select(o => o.ToString()));

        await context.Response.WriteAsJsonAsync(new { resultado, ast, simbolos });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.Run();
