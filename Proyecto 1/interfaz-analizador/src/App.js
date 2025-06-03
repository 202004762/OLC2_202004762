import React, { useState, useRef } from 'react';
import axios from 'axios';
import './App.css';
import AstViewer from './AstViewer';

function App() {
  const [codigo, setCodigo] = useState('');
  const [resultado, setResultado] = useState([]);
  const [erroresL, setErroresL] = useState([]);
  const [erroresS, setErroresS] = useState([]);
  const [erroresSem, setErroresSem] = useState([]);
  const [ast, setAst] = useState('');
  const [mostrarErrores, setMostrarErrores] = useState(false);
  const [mostrarSimbolos, setMostrarSimbolos] = useState(false);
  const fileInputRef = useRef(null);
  const [simbolos, setSimbolos] = useState([]);

  const analizar = async () => {
    try {
      const response = await axios.post('http://localhost:5000/analizar', codigo, {
        headers: { 'Content-Type': 'text/plain' },
      });

      const salida = response.data.resultado;
      setAst(response.data.ast);

      setErroresL(salida.filter(item => item.includes('Lexico')));
      setErroresS(salida.filter(item => item.includes('Sintactico')));
      setErroresSem(salida.filter(item => item.includes('Semantico')));
      setSimbolos(response.data.simbolos || []);

      const soloSalida = salida.filter(
        item => !item.includes('Lexico') && !item.includes('Sintactico') && !item.includes('Semantico')
      );
      setResultado(soloSalida);
    } catch (error) {
      setResultado([`❌ Error al conectar con la API: ${error.message}`]);
    }
  };

  const abrirArchivo = (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (e) => setCodigo(e.target.result);
    reader.readAsText(file);
  };

  const guardarArchivo = () => {
    const blob = new Blob([codigo], { type: 'text/plain' });
    const enlace = document.createElement('a');
    enlace.href = URL.createObjectURL(blob);
    enlace.download = 'codigo.glt';
    enlace.click();
  };

  return (
    <div className="App">
      {/* Menú superior */}
      <div className="menu-bar">
        <button onClick={() => fileInputRef.current.click()}>📂 Abrir</button>
        <button onClick={() => setCodigo('')}>📄 Nuevo</button>
        <button onClick={guardarArchivo}>💾 Guardar</button>
        <button onClick={() => setMostrarErrores(!mostrarErrores)}>⚠️ Errores</button>
        <button onClick={() => setMostrarSimbolos(!mostrarSimbolos)}>📊 Tabla</button>
      </div>

      <input type="file" accept=".glt" ref={fileInputRef} style={{ display: 'none' }} onChange={abrirArchivo} />

      <h2>Analizador de Código</h2>

      {/* Área de escritura */}
      <section>
        <textarea
          value={codigo}
          onChange={(e) => setCodigo(e.target.value)}
          rows={12}
          cols={80}
          placeholder="Escribe tu código aquí..."
        />
        <br />
        <button onClick={analizar}>Ejecutar</button>
      </section>

      {/* Resultados */}
      <section>
        <h3>📤 Salida del Analizador</h3>
        <pre>
          {resultado.map((linea, index) => (
            <div key={index}>{linea}</div>
          ))}
        </pre>
      </section>

      {/* Errores */}
      {mostrarErrores && (
        <div className="errores">
          <h3>Errores Léxicos</h3>
          <pre>{erroresL.join('\n') || '✔️ Sin errores léxicos'}</pre>
          <h3>Errores Sintácticos</h3>
          <pre>{erroresS.join('\n') || '✔️ Sin errores sintácticos'}</pre>
          <h3>Errores Semánticos</h3>
          <pre>{erroresSem.join('\n') || '✔️ Sin errores semánticos'}</pre>
        </div>
      )}

      {/* Tabla de símbolos */}
      {mostrarSimbolos && simbolos.length > 0 && (
        <div className="tabla-simbolos">
          <h3>📊 Tabla de Símbolos</h3>
          <table>
            <thead>
              <tr>
                <th>Identificador</th>
                <th>Tipo</th>
                <th>Valor</th>
                <th>Entorno</th>
              </tr>
            </thead>
            <tbody>
              {simbolos.map((sim, index) => (
                <tr key={index}>
                  <td>{sim.identificador}</td>
                  <td>{sim.tipo}</td>
                  <td>{sim.valor}</td>
                  <td>{sim.entorno}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* AST visual */}
      {ast && <AstViewer ast={ast} />}
    </div>
  );
}

export default App;
