using Assets.Scripts;
using Assets.Scripts.DataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using JetBrains.Annotations;

/////////////////////////////////// Clase AStarMind, que emplea el algoritmo A* ///////////////////////////////////
public class AStarMind : AbstractPathMind
{
    /////////////////////////////////// Atributos de la clase /////////////////////////////////// 
    
    private List<Node> openList = new List<Node>();              //Lista abierta, donde se meteran los nodos no evaluados
    private List<Node> plan = new List<Node>();                  //Lista plan, donde se guardaran los nodos que conforman el plan que debe hacer el agente para llegar a la meta

    /////////////////////////////////// Metodo AStarMethod ///////////////////////////////////

    public void AStarMethod(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals) //Realiza el algoritmo A*
    {
        bool goalReached = false;                                                       //Booleano que indica si se ha llegado a un nodo meta

        //Primero, creamos el nodo origen, calculamos su heuristica
        int heuristic = Math.Abs((goals[0].ColumnId - currentPos.ColumnId)) + Math.Abs((goals[0].RowId - currentPos.RowId)); // Heuristica utilizada: Suma de Distancias Manhattan
        openList.Add(new Node(null, currentPos.ColumnId, currentPos.RowId, heuristic));                                      // Agregamos el nodo origen creado a la lista abierta

        //Bucle A*
        while(openList.Count != 0 && !goalReached){           //Mientras la lista tenga nodos dentro y goalReached valga false

     
            Node currentNode = openList.ElementAt(0);         //Almacenamos el primer elemento de la lista en una variable llamada currentNode

            openList.RemoveAt(0);                             //Eliminamos el primer elemento de la lista

            
            if (goal(currentNode, goals)) {                   //Comprobamos si currentNode es meta con el metodo goal, en caso afirmativo
                plan.Add(currentNode);                        //Agregamos el nodo a la lista plan
                goalReached = true;                           //Cambiamos el valor de goalReached de false a true
                Debug.Log("Meta alcanzada");                  //Mostramos un mensaje por la consola
            }
            else                                              //En caso de que currentNode no sea un nodo meta
            {
                expand(currentNode, boardInfo, goals);        //Usamos el metodo expand para expandir el nodo

                openList.Sort();                              // Ordenamos la lista abierta con el metodo Sort, que usara el metodo CompareTo de la clase Nodo
            }

            //En el caso de que en la lista abierta hayan mas nodos que el resultado de multiplicar el numero de columnas por el numero de filas del tablero
            if (openList.Count > (boardInfo.NumColumns * boardInfo.NumRows)) 
            {
                Debug.Log("No hay solucion");                 //Mostramos por pantalla que no hay solucion
                break;                                        //Salimos del bucle
            }
        }

        if (plan.Count != 0)                                 //Verificamos, una vez terminado el bucle, si hay algun elemento en la lista plan, en caso afirmativo
        {
            int i = 0;                                       //Declaramos una variable auxiliar i que tendra como valor inicial cero

            while (plan.ElementAt(i).father != null)         //Mientras el valor de la variable father del nodo actual sea distinto de null
            {
                plan.Add(plan.ElementAt(i).father);          //Agregamos a la lista plan al padre referenciado en la variable father, construyendo el plan desde la meta hasta el origen
                i++;                                         //Aumentamos el valor de i
            }

            plan.Reverse();                                  //Al final del bucle, el plan esta ordenado desde la meta al origen, por lo que usamos el metodo Reverse para ordenarlo
        }
        else                                                 //Si la lista plan esta vacia al terminar el bucle
        {
            Debug.Log("No hay solucion");                    //Mostramos por pantalla que no hay solucion
        }
    }

    /////////////////////////////////// Metodo GetNextMove ///////////////////////////////////

    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)  //Devuelve el movimiento que debe hacer el agente
    {
        if (plan.Count == 0)                                                    //Si la lista plan esta vacia
        {
            AStarMethod(boardInfo, currentPos, goals);                          //Hace una llamada al metodo AStarMethod

            return Locomotion.MoveDirection.None;                               //Le dice al agente que no haga ningun movimiento
        }
        else                                                                    //Si la lista no esta vacia
        {

            Node move = plan.ElementAt(0);                                      //Move toma el valor del nodo que se encuentra en la posicion 0 de la lista plan
            plan.RemoveAt(0);                                                   //Eliminamos dicho nodo de la lista plan

            //En este punto, se comprueban las diferencias entre las coordenadas de la celda del nodo y la de personaje, para determinar el movimiento del agente
            
            if (currentPos.ColumnId == move.x && currentPos.RowId > move.y)     //Si las coordenadas x son iguales pero la y del jugador es mayor que la del nodo
            {
                return Locomotion.MoveDirection.Down;                           //Se le dice al agente que se mueva para abajo
            }

            if (currentPos.ColumnId == move.x && currentPos.RowId < move.y)     //Si las coordenadas x son iguales pero la y del jugador es menor que la del nodo
            {
                return Locomotion.MoveDirection.Up;                             //Se le dice al agente que se mueva para arriba
            }

            if (currentPos.ColumnId < move.x && currentPos.RowId == move.y)     //Si las coordenadas y son iguales pero la x del jugador es menor que la del nodo
            {
                return Locomotion.MoveDirection.Right;                          //Se le dice al agente que se mueva a la derecha
            }
            //Si no se cumple ninguna de las condiciones anteriores
            return Locomotion.MoveDirection.Left;                               //Se le dice al agente que se mueva a la izquierda          
        }
    }

    public override void Repath()
    {
        
    }

    /////////////////////////////////// Metodo goal ///////////////////////////////////

    public bool goal(Node node, CellInfo[] goal) //Comprueba si el nodo que se esta evaluando es meta
    {
        if (node.x == goal[0].ColumnId && node.y == goal[0].RowId)             //Si las coordenadas del nodo coinciden con las coordenadas de las metas
        {
            return true;                                                       //Devuelve true
        }
        else                                                                   //En el caso contrario
        {
            return false;                                                      //Devuelve false
        }
    }

    /////////////////////////////////// Metodo expand ///////////////////////////////////

    public void expand(Node currentNode, BoardInfo board, CellInfo[] goals) //Expande el nodo recibido para obtener a los hijos del mismo
    {
        CellInfo actualPosition = new CellInfo(currentNode.x, currentNode.y);   //Usamos una variable actualPosition que tendrá las coordenas del nodo pasado

        //Guardamos los hijos del nodo actual en un array
        CellInfo[] childs = actualPosition.WalkableNeighbours(board);           //En un array de CellInfo guardamos lo que devuelve la funcion WalkeableNeighbours
                                                                                //Devolviendo nulo si el vecino no es caminable y CellInfo en caso de que lo sea
        //Creamos los nodos correspondientes a los hijos
        for (int i = 0; i < childs.Length; i++) {

            if (childs[i] != null) {                                            //Si el elemento evaluado es distinto de nulo
                //Calculo de la heuristica del hijo 
                int heuristic = Math.Abs((goals[0].ColumnId - childs[i].ColumnId)) + Math.Abs((goals[0].RowId - childs[i].RowId));

                bool repeatedNode = false;                                      //Variable auxiliar para determinar si un nodo esta repetido
               
                Node nodeToInsert = new Node(currentNode, childs[i].ColumnId, childs[i].RowId, heuristic); //Creacion del nodo hijo

                foreach (Node node in openList)                                 //Se comprueba si el nodo creado es repetido
                {
                    if(nodeToInsert.x == node.x && nodeToInsert.y == node.y)    //En el caso de que haya un nodo con las mismas coordenadas en la lista abierta
                    {
                        repeatedNode = true;                                    //Indicamos que se trata de un nodo repetido(Por ejemplo, podria ser el padre)
                    }
                }

                if (!repeatedNode)                                              //En el caso de que el nodo no este repetido
                {

                    openList.Add(nodeToInsert);                                 //Se inserta el nodo en la lista abierta
                }
            }
        }
    }
}
