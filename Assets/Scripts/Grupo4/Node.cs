using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////// Clase Nodo  ///////////////////////////////////
public class Node : IComparable<Node>
{
    /////////////////////////////////// Atributos de la clase /////////////////////////////////// 
    

    public Node parent;         //Almacena al padre del nodo
    public int x;               //Almacena la coordenada x de la celda correspondiente al nodo
    public int y;               //Almacena la coordenada y de la celda correspondiente al nodo
    public int g;               //Almacena el coste total para llegar al nodo desde el nodo origen, conocido como g
    public int hStar;           //Almacena el valor de la heuristica del nodo, conocido como h*
    public int fStar;           //Almacena el valor de f*, que es la suma de h* y g

    /////////////////////////////////// Constructor de la Clase /////////////////////////////////// 
    
    public Node(Node parentP,int xP, int yP, int hP) 
    {
        parent = parentP;       //Asignamos a la variable parent el nodo que se le pasa al constructor
        x = xP;                 //Asignamos a la variable x la coordenada de la celda pasada en el constructor
        y = yP;                 //Asignamos a la variable y la coordenada de la celda pasada en el constructor

        if (parent != null)     //Comprueba si el padre del nodo instanciado es distinto de nulo
        {
            g = parent.g + 1;   //Si es distinto de nulo, asignamos a g el valor g de su padre mas el coste de ir al nodo en cuestion (en este caso siempre sera 1)
        }
        else
        {
            g = 0;              //Si es nulo, quiere decir que el nodo que queremos crear es la raiz, por lo tanto, asignamos a g el valor cero
        }

        hStar = hP;             //Asignamos a la variable hStar la heuristica pasada al constructor
        fStar = g + hStar;      //Asignamos a la variable fStar la suma del valor de su variable g y hStar
    }

    /////////////////////////////////// Metodo Compare To, proveniente de la interfaz IComparable /////////////////////////////////// 
    
    public int CompareTo(Node other)    //Este metodo permitira ordenar los nodos en funcion del valor de sus variables fStar
    {
        if (fStar > other.fStar)        //Si el valor de fStar del nodo es mayor que el del otro nodo
        {
            return 1;
        }
        else if (fStar < other.fStar)   //Si el valor de fStar del nodo es menor que el del otro nodo
        {
            return -1;
        }
        else                            //Si el valor de fStar del nodo es igual que el del otro nodo
        {
            return 0;

        } 
    }
}
