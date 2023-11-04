using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////// Clase Nodo, que se usaran para utilizar el algoritmo A* ///////////////////////////////////
public class Node : IComparable<Node>
{
    /////////////////////////////////// Atributos de la clase /////////////////////////////////// 
    

    public Node father;         //Almacena al padre del nodo
    public int x;               //Almacena la coordenada x de la celda correspondiente al nodo
    public int y;               //Almacena la coordenada y de la celda correspondiente al nodo
    public int g;               //Almacena el coste total para llegar al nodo desde el nodo origen, conocida como g
    public int hStar;           //Almacena el valor de la heutistica del nodo, conocida como h*
    public int fStar;           //Almacena el valor de f*, que es la suma de h* y g

    /////////////////////////////////// Constructor de la Clase /////////////////////////////////// 
    
    public Node(Node fatherP,int xP, int yP, int hP) 
    {
        father = fatherP;       //Asignamos a la variable father el nodo que se le pasa al constructor
        x = xP;                 //Asignamos a la variable x la coordenada de la celda pasada en el constructor
        y = yP;                 //Asignamos a la variable y la coordenada de la celda pasada en el constructor

        if (father != null)     //Comprueba si el nodo pasado al constructor es distinto de nulo
        {
            g = father.g + 1;   //Si es distinto de nulo, asigamos a g el valor g de su padre mas el coste de ir al nodo en cuestion
        }
        else
        {
            g = 0;              //Si es nulo, quiere decir que el nodo que queremos crear es la raiz, por lo tanto, asignamos a g el valor cero
        }

        hStar = hP;             //Asignamos a la variable hStar la heuristica pasada al constructor
        fStar = g + hStar;      //Asignamos a la variable fStar la suma del valor de su variuable g y hStar
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
