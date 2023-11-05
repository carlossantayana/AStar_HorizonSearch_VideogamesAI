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
using static UnityEngine.EventSystems.EventTrigger;

/////////////////////////////////// Clase HillClimbing, que emplea el algoritmo A* ///////////////////////////////////
public class HorizonSearchMind : AbstractPathMind
{
    /////////////////////////////////// Atributos de la clase /////////////////////////////////// 

    private Stack<Node> openList = new Stack<Node>();
    private List<Node> treeLeafs = new List<Node>();
    private Node plan;
    private List<GameObject> enemies = new List<GameObject>();
    private int numEnemies; 

    public int depth = 1;

    /////////////////////////////////// Metodos de la clase //////////////////////////////////////////

    private void Awake()
    {
        numEnemies = GameObject.Find("Loader").GetComponent<Loader>().numEnemies;
    }

    /////////////////////////////////// Metodo HorizonSearchMethod ///////////////////////////////////

    public void horizonSearchMethod(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
    {
        if (numEnemies > 0)
        {
            CellInfo enemyPos = SelectNeariestEnemy(currentPos);

            int heuristic = Math.Abs((enemyPos.ColumnId - currentPos.ColumnId)) + Math.Abs((enemyPos.RowId - currentPos.RowId)); // Heuristica utilizada: Suma de Distancias Manhattan

            openList.Push(new Node(null, currentPos.ColumnId, currentPos.RowId, heuristic));                                      // Agregamos el nodo origen creado a la lista abierta

            Node currentNode = openList.ElementAt(0);

            //if (checkEnemiesState())
            //{
            //foreach (GameObject gameObject in enemies)
            //{
            //    if (gameObject.GetComponent<EnemyBehaviour>().CurrentPosition().RowId == currentNode.y
            //        && gameObject.GetComponent<EnemyBehaviour>().CurrentPosition().ColumnId == currentNode.x)
            //    {
            //        enemies.Remove(gameObject);
            //    }
            //}

            //updateNumEnemies();
            // enemyPos = SelectNeariestEnemy(currentPos);
            // }
            //else
            // {

            //Bucle Profundidad
            while (openList.Count != 0)
            {
                Node currentNodeDepth = openList.Pop();

                if (currentNodeDepth.g < depth)
                {
                    expand(currentNodeDepth, boardInfo, enemyPos);
                }
                else
                {
                    treeLeafs.Add(currentNodeDepth);
                }
            }
            //  }
        }

        if (numEnemies == 0)
        {
            int heuristic = Math.Abs((goals[0].ColumnId - currentPos.ColumnId)) + Math.Abs((goals[0].RowId - currentPos.RowId)); // Heuristica utilizada: Suma de Distancias Manhattan

            openList.Push(new Node(null, currentPos.ColumnId, currentPos.RowId, heuristic));                                      // Agregamos el nodo origen creado a la lista abierta

            Node currentNode = openList.ElementAt(0);

            //Bucle Profundidad
            while (openList.Count != 0)
            {
                Node currentNodeDepth = openList.Pop();

                if (currentNodeDepth.g < depth)
                {
                    expand(currentNodeDepth, boardInfo, goals[0]);
                }
                else
                {
                    treeLeafs.Add(currentNodeDepth);
                }
            }
        }

        treeLeafs.Sort();

        plan = treeLeafs.ElementAt(0);

        if (plan.father != null)
        {
            for (int i = 0; i < depth - 1; i++)
            {
                plan = plan.father;
            }
        }

        if (plan != null)
        {
            Debug.Log("Accion encontrada");
        }
        else
        {
            Debug.Log("Accion no encontrada");
        }

        //treeLeafs.Clear();
        //openList.Clear();
        //enemies.Clear();
    }

    /////////////////////////////////// Metodo GetNextMove ///////////////////////////////////

    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)  //Devuelve el movimiento que debe hacer el agente
    {
        treeLeafs.Clear();
        openList.Clear();
        enemies.Clear();

        findEnemies();
        horizonSearchMethod(boardInfo, currentPos, goals);                          //Hace una llamada al metodo HorizonSearchMethod

        if (plan != null)
        {
            Node move = plan;                                      //Move toma el valor del nodo que se encuentra en la posicion 0 de la lista plan
            plan = null;                                                   //Eliminamos dicho nodo de la lista plan

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
        else
        {
            return Locomotion.MoveDirection.None;
        }

        //if (plan == null)                                                    //Si la lista plan esta vacia
        //{
        //    findEnemies();
        //    horizonSearchMethod(boardInfo, currentPos, goals);                          //Hace una llamada al metodo HorizonSearchMethod

        //    return Locomotion.MoveDirection.None;                               //Le dice al agente que no haga ningun movimiento
        //}
        //else                                                                    //Si la lista no esta vacia
        //{

        //    Node move = plan;                                      //Move toma el valor del nodo que se encuentra en la posicion 0 de la lista plan
        //    plan = null;                                                   //Eliminamos dicho nodo de la lista plan

        //    //En este punto, se comprueban las diferencias entre las coordenadas de la celda del nodo y la de personaje, para determinar el movimiento del agente

        //    if (currentPos.ColumnId == move.x && currentPos.RowId > move.y)     //Si las coordenadas x son iguales pero la y del jugador es mayor que la del nodo
        //    {
        //        return Locomotion.MoveDirection.Down;                           //Se le dice al agente que se mueva para abajo
        //    }

        //    if (currentPos.ColumnId == move.x && currentPos.RowId < move.y)     //Si las coordenadas x son iguales pero la y del jugador es menor que la del nodo
        //    {
        //        return Locomotion.MoveDirection.Up;                             //Se le dice al agente que se mueva para arriba
        //    }

        //    if (currentPos.ColumnId < move.x && currentPos.RowId == move.y)     //Si las coordenadas y son iguales pero la x del jugador es menor que la del nodo
        //    {
        //        return Locomotion.MoveDirection.Right;                          //Se le dice al agente que se mueva a la derecha
        //    }
        //    //Si no se cumple ninguna de las condiciones anteriores
        //    return Locomotion.MoveDirection.Left;                               //Se le dice al agente que se mueva a la izquierda          
        //}
    }

    public override void Repath()
    {

    }

    /////////////////////////////////// Metodo goal ///////////////////////////////////

    //public bool goal(Node node, CellInfo[] goal) //Comprueba si el nodo que se esta evaluando es meta
    //{
    //    if (node.x == goal[0].ColumnId && node.y == goal[0].RowId)             //Si las coordenadas del nodo coinciden con las coordenadas de las metas
    //    {
    //        return true;                                                       //Devuelve true
    //    }
    //    else                                                                   //En el caso contrario
    //    {
    //        return false;                                                      //Devuelve false
    //    }
    //}

    public bool goal(Node node, CellInfo objetive) //Comprueba si el nodo que se esta evaluando es meta
    {
        if (node.x == objetive.ColumnId && node.y == objetive.RowId)             //Si las coordenadas del nodo coinciden con las coordenadas de las metas
        {
            return true;                                                       //Devuelve true
        }
        else                                                                   //En el caso contrario
        {
            return false;                                                      //Devuelve false
        }
    }

    /////////////////////////////////// Metodo expand ///////////////////////////////////

    //public void expand(Node currentNode, BoardInfo board, CellInfo[] goals) //Expande el nodo recibido para obtener a los hijos del mismo
    //{
    //    CellInfo actualPosition = new CellInfo(currentNode.x, currentNode.y);   //Usamos una variable actualPosition que tendrá las coordenas del nodo pasado

    //    //Guardamos los hijos del nodo actual en un array
    //    CellInfo[] childs = actualPosition.WalkableNeighbours(board);           //En un array de CellInfo guardamos lo que devuelve la funcion WalkeableNeighbours
    //                                                                            //Devolviendo nulo si el vecino no es caminable y CellInfo en caso de que lo sea
    //                                                                            //Creamos los nodos correspondientes a los hijos
    //    for (int i = 0; i < childs.Length; i++)
    //    {

    //        if (childs[i] != null)
    //        {                                            //Si el elemento evaluado es distinto de nulo
    //            //Calculo de la heuristica del hijo 
    //            int heuristic = Math.Abs((goals[0].ColumnId - childs[i].ColumnId)) + Math.Abs((goals[0].RowId - childs[i].RowId));

    //            bool repeatedNode = false;                                      //Variable auxiliar para determinar si un nodo esta repetido

    //            Node nodeToInsert = new Node(currentNode, childs[i].ColumnId, childs[i].RowId, heuristic); //Creacion del nodo hijo

    //            foreach (Node node in openList)                                 //Se comprueba si el nodo creado es repetido
    //            {
    //                if (nodeToInsert.x == node.x && nodeToInsert.y == node.y)    //En el caso de que haya un nodo con las mismas coordenadas en la lista abierta
    //                {
    //                    repeatedNode = true;                                    //Indicamos que se trata de un nodo repetido(Por ejemplo, podria ser el padre)
    //                }
    //            }

    //            if (!repeatedNode)                                              //En el caso de que el nodo no este repetido
    //            {

    //                openList.Push(nodeToInsert);                                //Se inserta el nodo en la lista abierta
    //            }
    //        }
    //    }
    //}

    public void expand(Node currentNode, BoardInfo board, CellInfo objetive) //Expande el nodo recibido para obtener a los hijos del mismo
    {
        CellInfo actualPosition = new CellInfo(currentNode.x, currentNode.y);   //Usamos una variable actualPosition que tendrá las coordenas del nodo pasado

        //Guardamos los hijos del nodo actual en un array
        CellInfo[] childs = actualPosition.WalkableNeighbours(board);           //En un array de CellInfo guardamos lo que devuelve la funcion WalkeableNeighbours
                                                                                //Devolviendo nulo si el vecino no es caminable y CellInfo en caso de que lo sea
                                                                                //Creamos los nodos correspondientes a los hijos
        for (int i = 0; i < childs.Length; i++)
        {

            if (childs[i] != null)
            {                                            //Si el elemento evaluado es distinto de nulo
                //Calculo de la heuristica del hijo 
                int heuristic = Math.Abs((objetive.ColumnId - childs[i].ColumnId)) + Math.Abs((objetive.RowId - childs[i].RowId));

                //bool repeatedNode = false;                                      //Variable auxiliar para determinar si un nodo esta repetido

                Node nodeToInsert = new Node(currentNode, childs[i].ColumnId, childs[i].RowId, heuristic); //Creacion del nodo hijo

                //foreach (Node node in openList)                                 //Se comprueba si el nodo creado es repetido
                //{
                //    if (nodeToInsert.x == node.x && nodeToInsert.y == node.y)    //En el caso de que haya un nodo con las mismas coordenadas en la lista abierta
                //    {
                //        repeatedNode = true;                                    //Indicamos que se trata de un nodo repetido(Por ejemplo, podria ser el padre)
                //    }
                //}

                //if (!repeatedNode)                                              //En el caso de que el nodo no este repetido
                //{
                    openList.Push(nodeToInsert);                                 //Se inserta el nodo en la lista abierta
                //}

                //repeatedNode = false;

                //foreach (Node node in treeLeafs)                                 //Se comprueba si el nodo creado es repetido
                //{
                //    if (nodeToInsert.x == node.x && nodeToInsert.y == node.y)    //En el caso de que haya un nodo con las mismas coordenadas en la lista abierta
                //    {
                //        repeatedNode = true;                                    //Indicamos que se trata de un nodo repetido(Por ejemplo, podria ser el padre)
                //    }
                //}

                if (goal(nodeToInsert, objetive)/* && !repeatedNode*/)
                {
                    treeLeafs.Add(nodeToInsert);
                }
            }
        }
    }

    ///////////////////////////////////// Metodo SelectNearestEnemy //////////////////////////////////////////
    public CellInfo SelectNeariestEnemy(CellInfo currentPos)
    {
        int minDistance = 0;
        CellInfo enemyCell = null;

        for (int i = 0; i < numEnemies; i++)
        {
            GameObject enemy = enemies.ElementAt(i);
            CellInfo enemyPos = enemy.GetComponent<EnemyBehaviour>().CurrentPosition();

            int distanceToEnemy = Math.Abs((enemyPos.ColumnId - currentPos.ColumnId)) + Math.Abs((enemyPos.RowId - currentPos.RowId));

            if (i == 0)
            {
                minDistance = distanceToEnemy;
                enemyCell = enemyPos;
            }
            else
            {
                if (distanceToEnemy < minDistance)
                {
                    minDistance = distanceToEnemy;
                    enemyCell = enemyPos;
                }
            }
        }

        return enemyCell;
    }

    public void updateNumEnemies()
    {
        numEnemies = enemies.Count();
    }

    public void findEnemies()       //Busca los enemigos en la escena y los agrega a la lista, actualzia el numero de enemigos
    {
        for (int i = 0; i < numEnemies; i++)
        {
            GameObject enemy = GameObject.Find("Enemy_" + i);
            if (enemy != null)
            {
                enemies.Add(enemy);
            }
        }
        updateNumEnemies();
    }

    //public bool checkEnemiesState()
    //{
    //    for (int i = 0; i < numEnemies; i++)
    //    {
    //        GameObject enemy = GameObject.Find("Enemy_" + i);
    //        if (enemy == null)
    //        {
    //            enemies.RemoveAt(i);
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}