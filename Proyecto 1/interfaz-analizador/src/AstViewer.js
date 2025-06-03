import React from 'react';
import Tree from 'react-d3-tree';

const containerStyles = {
    width: '100%',
    height: '500px',
    background: '#ffffff', // blanco o cambia a otro color claro como '#f0f0f0'
    padding: '10px',
    color: '#000', // texto oscuro
  };
  

function parseAst(text) {
  let index = 0;

  function parseNode() {
    if (text[index] === '(') {
      index++;
      let label = '';
      while (text[index] !== ' ' && text[index] !== ')') {
        label += text[index++];
      }
      const children = [];
      while (text[index] !== ')' && index < text.length) {
        if (text[index] === ' ') {
          index++;
        } else {
          children.push(parseNode());
        }
      }
      index++; // skip ')'
      return { name: label, children };
    } else {
      let label = '';
      while (index < text.length && text[index] !== ' ' && text[index] !== ')') {
        label += text[index++];
      }
      return { name: label };
    }
  }

  return parseNode();
}

const AstViewer = ({ ast }) => {
  if (!ast) return null;

  let parsedTree;
  try {
    parsedTree = parseAst(ast);
  } catch (error) {
    return <div>Error al procesar AST: {error.message}</div>;
  }

  return (
    <div style={containerStyles}>
      <h3>Árbol AST (visual)</h3>
      <Tree data={parsedTree} orientation="vertical" />
    </div>
  );
};

export default AstViewer;
