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
    private List<CellInfo> enemies = new List<CellInfo>();
    private int numEnemies;
    private int enemiesAlive;

    public int depth;

    /////////////////////////////////// Metodos de la clase //////////////////////////////////////////

    private void Awake()
    {
        numEnemies = GameObject.Find("Loader").GetComponent<Loader>().numEnemies;
        enemiesAlive = numEnemies;
    }

    private void Start()
    {
        findEnemies();
    }

    /////////////////////////////////// Metodo HorizonSearchMethod ///////////////////////////////////

    public void horizonSearchMethod(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
    {
        if (enemiesAlive > 0)
        {
            CellInfo enemyPos = SelectNeariestEnemy(currentPos);

            int heuristic = Math.Abs((enemyPos.ColumnId - currentPos.ColumnId)) + Math.Abs((enemyPos.RowId - currentPos.RowId)); // Heuristica utilizada: Suma de Distancias Manhattan

            openList.Push(new Node(null, currentPos.ColumnId, currentPos.RowId, heuristic));                                      // Agregamos el nodo origen creado a la lista abierta

            Node currentNode = openList.ElementAt(0);

            //Bucle Profundidad
            while (openList.Count != 0)
            {
                Node currentNodeDepth = openList.Pop();

                if (currentNodeDepth.g < depth)
                {
                    if (!goal(currentNodeDepth, enemyPos))
                    {
                        expand(currentNodeDepth, boardInfo, enemyPos);
                    }
                    else
                    {
                        treeLeafs.Add(currentNodeDepth);
                    }
                }
                else
                {
                    treeLeafs.Add(currentNodeDepth);
                }
            }
        }

        if (enemiesAlive == 0)
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
                    if (!goal(currentNodeDepth, goals[0]))
                    {
                        expand(currentNodeDepth, boardInfo, goals[0]);
                    }
                    else
                    {
                        treeLeafs.Add(currentNodeDepth);
                    }
                }
                else
                {
                    treeLeafs.Add(currentNodeDepth);
                }
            }
        }

        if (treeLeafs.Count != 0)
        {
            treeLeafs.Sort();
            plan = treeLeafs.ElementAt(0);
        }
        else
        {
            plan = null;
        }
        
        if (plan != null)
        {
            Debug.Log("Accion encontrada");

            if (plan.parent != null)
            {
                while (plan.parent.parent != null)
                {
                    plan = plan.parent;
                }
            }
        }
        else
        {
            Debug.Log("Accion no encontrada");
        }
    }

    /////////////////////////////////// Metodo GetNextMove ///////////////////////////////////

    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)  //Devuelve el movimiento que debe hacer el agente
    {
        updateEnemies();
        treeLeafs.Clear();
        openList.Clear();
        
        horizonSearchMethod(boardInfo, currentPos, goals);                          //Hace una llamada al metodo HorizonSearchMethod

        if (plan != null)
        {
            Node move = plan;                                      
            plan = null;                                                   

            if (currentPos.ColumnId == move.x && currentPos.RowId > move.y)     
            {
                return Locomotion.MoveDirection.Down;                           
            }

            if (currentPos.ColumnId == move.x && currentPos.RowId < move.y)     
            {
                return Locomotion.MoveDirection.Up;                             
            }

            if (currentPos.ColumnId < move.x && currentPos.RowId == move.y)     
            {
                return Locomotion.MoveDirection.Right;                          
            }

            return Locomotion.MoveDirection.Left;                               
        }
        else
        {
            return Locomotion.MoveDirection.None;
        }
    }

    public override void Repath()
    {

    }

    /////////////////////////////////// Metodo goal ///////////////////////////////////

    public bool goal(Node node, CellInfo objetive) //Comprueba si el nodo que se esta evaluando es el objetivo
    {
        if (node.x == objetive.ColumnId && node.y == objetive.RowId)           //Si las coordenadas del nodo coinciden con las coordenadas del objetivo
        {
            return true;                                                       //Devuelve true
        }
        else                                                                   //En el caso contrario
        {
            return false;                                                      //Devuelve false
        }
    }

    /////////////////////////////////// Metodo expand ///////////////////////////////////

    public void expand(Node currentNode, BoardInfo board, CellInfo objetive)    //Expande el nodo recibido para obtener a los hijos del mismo
    {
        CellInfo actualPosition = new CellInfo(currentNode.x, currentNode.y);   //Usamos una variable actualPosition que tendrá las coordenas del nodo actual

        //Guardamos los hijos del nodo actual en un array
        CellInfo[] childs = actualPosition.WalkableNeighbours(board);           //En un array de CellInfo guardamos lo que devuelve la funcion WalkeableNeighbours
                                                                                //Devolviendo nulo si el vecino no es caminable y CellInfo en caso de que lo sea
                                                                                //Creamos los nodos correspondientes a los hijos
        for (int i = 0; i < childs.Length; i++)
        {

            if (childs[i] != null){                                             //Si el elemento evaluado es distinto de nulo                                            
                //Calculo de la heuristica del hijo 
                int heuristic = Math.Abs((objetive.ColumnId - childs[i].ColumnId)) + Math.Abs((objetive.RowId - childs[i].RowId));

                bool repeatedNode = false;                                      //Variable auxiliar para determinar si un nodo esta repetido

                Node nodeToInsert = new Node(currentNode, childs[i].ColumnId, childs[i].RowId, heuristic); //Creacion del nodo hijo

                foreach (Node node in openList)                                 //Se comprueba si el nodo creado es repetido
                {
                    if (nodeToInsert.x == node.x && nodeToInsert.y == node.y)    //En el caso de que haya un nodo con las mismas coordenadas en la lista abierta
                    {
                        repeatedNode = true;                                    //Indicamos que se trata de un nodo repetido(Por ejemplo, podria ser el padre)
                    }
                }

                if (!repeatedNode)                                              //En el caso de que el nodo no este repetido
                {
                    openList.Push(nodeToInsert);                                 //Se inserta el nodo en la lista abierta por el principio (recorrido en profundidad)
                }
            }
        }
    }

    ///////////////////////////////////// Metodo SelectNearestEnemy //////////////////////////////////////////
    public CellInfo SelectNeariestEnemy(CellInfo currentPos)
    {
        int minDistance = int.MaxValue;
        CellInfo enemyCell = null;

        for (int i = 0; i < numEnemies; i++)
        {
            if (enemies.ElementAt(i) != null)
            {
                CellInfo enemyPos = enemies.ElementAt(i);

                int distanceToEnemy = Math.Abs((enemyPos.ColumnId - currentPos.ColumnId)) + Math.Abs((enemyPos.RowId - currentPos.RowId));

                if (distanceToEnemy < minDistance)
                {
                    minDistance = distanceToEnemy;
                    enemyCell = enemyPos;
                }
            }
        }

        return enemyCell;
    }

    ///////////////////////////////////// Metodo updateEnemies //////////////////////////////////////////
    public void updateEnemies()
    {
        for (int i = 0; i < numEnemies; i++)
        {
            if(enemies.ElementAt(i) != null)
            {
                GameObject enemy = GameObject.Find("Enemy_" + i);

                if(enemy != null)
                {
                    CellInfo enemyCell = enemy.GetComponent<EnemyBehaviour>().CurrentPosition();

                    enemies[i] = enemyCell;
                }
                else
                {
                    enemies[i] = null;
                    enemiesAlive--;
                }
            }
        }
    }

    ///////////////////////////////////// Metodo findEnemies //////////////////////////////////////////
    public void findEnemies()       //Busca los enemigos en la escena y los agrega a la lista, actualiza el numero de enemigos
    {
        for (int i = 0; i < numEnemies; i++)
        {
            GameObject enemy = GameObject.Find("Enemy_" + i);

            CellInfo enemyCell = enemy.GetComponent<EnemyBehaviour>().CurrentPosition();

            enemies.Add(enemyCell);
        }
    }
}