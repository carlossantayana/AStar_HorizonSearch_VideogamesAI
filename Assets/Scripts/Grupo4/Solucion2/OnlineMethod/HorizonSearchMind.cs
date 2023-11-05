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

/////////////////////////////////// Clase HorizonSearchMind, que emplea búsqueda por horizontes utilizando recorrido en profundidad como base ///////////////////////////////////
public class HorizonSearchMind : AbstractPathMind
{
    /////////////////////////////////// Atributos de la clase /////////////////////////////////// 

    private Stack<Node> openList = new Stack<Node>();               //Lista abierta para cada búsqueda
    private List<Node> treeLeafs = new List<Node>();                //Lista de los nodos terminales en cada búsqueda
    private Node plan;                                              //Nodo usado para almacenar la mejor acción a realizar en cada búsqueda
    private List<CellInfo> enemies = new List<CellInfo>();          //Lista de celdas de enemigos (vivos o no)
    private int numEnemies;                                         //Entero que almacena el número de enemigos inicial durante toda la ejecución
    private int enemiesAlive;                                       //Entero que almacena el número de enemigos vivos

    public int depth;                                               //Miembro público que almacena el horizonte de la búsqueda (modificable desde el editor)

    /////////////////////////////////// Metodos de la clase //////////////////////////////////////////

    private void Awake()
    {
        numEnemies = GameObject.Find("Loader").GetComponent<Loader>().numEnemies;               //Almacenamos el número de enemigos especificado en el loader
        enemiesAlive = numEnemies;                                                              //Al principio todos los enemigos están vivos
    }

    private void Start()
    {
        findEnemies();                                                                          //Se buscan las celdas de los enemigos en un inicio
    }

    /////////////////////////////////// Metodo HorizonSearchMethod ///////////////////////////////////

    public void horizonSearchMethod(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
    {
        if (enemiesAlive > 0)                                                                                                      //Si hay enemigos vivos
        {
            CellInfo enemyPos = SelectNeariestEnemy(currentPos);                                                                   //Buscamos el más cercano

            int heuristic = Math.Abs((enemyPos.ColumnId - currentPos.ColumnId)) + Math.Abs((enemyPos.RowId - currentPos.RowId));   //Heuristica utilizada: Suma de Distancias Manhattan al objetivo (enemigo más cercano)

            openList.Push(new Node(null, currentPos.ColumnId, currentPos.RowId, heuristic));                                       //Agregamos el nodo origen creado a la lista abierta
            
            //Bucle Profundidad
            while (openList.Count != 0)                                                                                             //Mientras la lista abierta tenga nodos
            {
                Node currentNodeDepth = openList.Pop();                                                                             //Sacamos el primer nodo (recorrido en profundidad)

                if (currentNodeDepth.g < depth)                                                                                     //Si el nodo no está en la profundidad horizonte
                {
                    if (!goal(currentNodeDepth, enemyPos))                                                                          //Si no es meta
                    {
                        expand(currentNodeDepth, boardInfo, enemyPos);                                                              //Se expande
                    }
                    else                                                                                                            //Si es meta
                    {
                        treeLeafs.Add(currentNodeDepth);                                                                            //No se expande y se añade a la lista de nodos terminales
                    }
                }
                else                                                                                                                //Si el nodo está en la profundidad horizonte
                {
                    treeLeafs.Add(currentNodeDepth);                                                                                //Se añade a la lista de nodos terminales
                }
            }
        }

        if (enemiesAlive == 0)                                                                                                      //Si no hay enemigos, vamos a la meta
        {
            int heuristic = Math.Abs((goals[0].ColumnId - currentPos.ColumnId)) + Math.Abs((goals[0].RowId - currentPos.RowId));    // Heuristica utilizada: Suma de Distancias Manhattan al objetivo (la meta)

            openList.Push(new Node(null, currentPos.ColumnId, currentPos.RowId, heuristic));                                        //Agregamos el nodo origen creado a la lista abierta

            //Bucle Profundidad
            while (openList.Count != 0)                                                                                             //Mientras haya nodos en la lista abierta
            {
                Node currentNodeDepth = openList.Pop();                                                                             //Sacamos el primer nodo (recorrido en profundidad)

                if (currentNodeDepth.g < depth)                                                                                     //Si el nodo no está en la profundidad horizonte
                {
                    if (!goal(currentNodeDepth, goals[0]))                                                                          //Si no es meta
                    {
                        expand(currentNodeDepth, boardInfo, goals[0]);                                                              //Se expande
                    }
                    else                                                                                                            //Si es meta
                    {
                        treeLeafs.Add(currentNodeDepth);                                                                            //No se expande y se añade a la lista de nodos terminales
                    }
                }
                else                                                                                                                //Si el nodo está en la profundidad horizonte
                {
                    treeLeafs.Add(currentNodeDepth);                                                                                //Se añade a la lista de nodos terminales
                }
            }
        }

        if (treeLeafs.Count != 0)                                                                                                   //Si la lista de nodos terminales no está vacía
        {
            treeLeafs.Sort();                                                                                                       //La ordenamos en función de su distancia al objetivo y el coste en llegar a ese nodo (f*)
            plan = treeLeafs.ElementAt(0);                                                                                          //El plan toma el mejor nodo hoja
        }
        else                                                                                                                        //Si la lista está vacía, no hay plan
        {
            plan = null;
        }

        if (plan != null)                                                                                                           //Si hay plan
        {
            Debug.Log("Accion encontrada");

            if (plan.parent != null)                                                                                                //Si el plan tiene padre
            {
                while (plan.parent.parent != null)                                                                                  //Mientras el padre de su padre sea distinto de nul
                {
                    plan = plan.parent;                                                                                             //Hacemos que el plan sea su padre
                }
            }

            //De esta forma, conseguimos que el plan almacene el nodo siguiente a la raíz con la mejor acción a realizar para llegar al objetivo
        }
        else                                                                                                                        //Si no hay plan, no hacemos nada
        {
            Debug.Log("Accion no encontrada");
        }
    }

    /////////////////////////////////// Metodo GetNextMove ///////////////////////////////////

    public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)  //Devuelve el movimiento que debe hacer el agente
    {
        updateEnemies();                                                            //Antes de realizar una búsqueda, actualizamos la lista de enemigos
        treeLeafs.Clear();                                                          //y se limpian las listas de nodos hojas y la lista abierta
        openList.Clear();

        horizonSearchMethod(boardInfo, currentPos, goals);                          //Hace una llamada al metodo HorizonSearchMethod

        if (plan != null)                                                           //Si hay plan
        {
            Node move = plan;                                                       //Almacenamos el move el nodo al que se tiene que mover el personaje                      
            plan = null;                                                            //Limpiamos el plan para la siguiente búsqueda               

            if (currentPos.ColumnId == move.x && currentPos.RowId > move.y)         //En función de la posición actual y la posición del nodo al que se debe mover se elige el movimiento
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
        else                                                                        //En caso de no haber plan, no nos movemos
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

    public void expand(Node currentNode, BoardInfo board, CellInfo objetive)    //Expande el nodo recibido para obtener los hijos del mismo
    {
        CellInfo actualPosition = new CellInfo(currentNode.x, currentNode.y);   //Usamos una variable actualPosition que tendrá las coordenas del nodo actual

        //Guardamos los hijos del nodo actual en un array
        CellInfo[] childs = actualPosition.WalkableNeighbours(board);           //En un array de CellInfo guardamos lo que devuelve la funcion WalkeableNeighbours
                                                                                //Devolviendo nulo si el vecino no es caminable y CellInfo en caso de que lo sea

        //Creamos los nodos correspondientes a los hijos
        for (int i = 0; i < childs.Length; i++)
        {

            if (childs[i] != null)
            {                                             //Si el elemento evaluado es distinto de nulo                                            
                //Calculo de la heuristica del hijo 
                int heuristic = Math.Abs((objetive.ColumnId - childs[i].ColumnId)) + Math.Abs((objetive.RowId - childs[i].RowId));

                bool repeatedNode = false;                                      //Variable auxiliar para determinar si un nodo esta repetido

                Node nodeToInsert = new Node(currentNode, childs[i].ColumnId, childs[i].RowId, heuristic); //Creacion del nodo hijo

                foreach (Node node in openList)                                 //Se comprueba si el nodo creado es repetido
                {
                    if (nodeToInsert.x == node.x && nodeToInsert.y == node.y)   //En el caso de que haya un nodo con las mismas coordenadas en la lista abierta
                    {
                        repeatedNode = true;                                    //Indicamos que se trata de un nodo repetido(Por ejemplo, podria ser el padre)
                    }
                }

                if (!repeatedNode)                                              //En el caso de que el nodo no este repetido
                {
                    openList.Push(nodeToInsert);                                //Se inserta el nodo en la lista abierta por el principio (recorrido en profundidad)
                }
            }
        }
    }

    ///////////////////////////////////// Metodo SelectNearestEnemy //////////////////////////////////////////
    public CellInfo SelectNeariestEnemy(CellInfo currentPos)    //Recibe la celda actual
    {
        int minDistance = int.MaxValue;                                                                                 //Se inicializa la distancia minima a un valor grande
        CellInfo enemyCell = null;                                                                                      //Esta variable almacenará la celda del enemigo más cercano

        for (int i = 0; i < numEnemies; i++)                                                                            //Para cada enemigo
        {
            if (enemies.ElementAt(i) != null)                                                                           //Si está vivo
            {
                CellInfo enemyPos = enemies.ElementAt(i);                                                               //Almacenamos en una variable su celda

                int distanceToEnemy = Math.Abs((enemyPos.ColumnId - currentPos.ColumnId)) + Math.Abs((enemyPos.RowId - currentPos.RowId)); //Calculamos la distancia a ese enemigo

                if (distanceToEnemy < minDistance)                                                                      //Si la distancia calculada es menor que la almacenada en minDistance
                {
                    minDistance = distanceToEnemy;                                                                      //Actualizamos minDistance
                    enemyCell = enemyPos;                                                                               //Guardamos la celda de ese enemigo en enemyCell
                }
            }
        }

        return enemyCell;                                                                                               //Devuelve la celda del enemigo más cercano
    }

    ///////////////////////////////////// Metodo updateEnemies //////////////////////////////////////////
    public void updateEnemies()
    {
        for (int i = 0; i < numEnemies; i++)                                                                //Recorremos la lista de enemigos
        {
            if (enemies.ElementAt(i) != null)                                                                //Si el enemigo no ha muerto en la lista (es distinto de nulo)
            {
                GameObject enemy = GameObject.Find("Enemy_" + i);                                           //Lo buscamos en la jerarquía

                if (enemy != null)                                                                           //Si sigue vivo en la jerarquía
                {
                    CellInfo enemyCell = enemy.GetComponent<EnemyBehaviour>().CurrentPosition();            //Obtenemos su celda

                    enemies[i] = enemyCell;                                                                 //Actualizamos su celda
                }
                else                                                                                        //Si está muerto en la jerarquía
                {
                    enemies[i] = null;                                                                      //Se pone a nulo en la lista
                    enemiesAlive--;                                                                         //Se reduce el número de enemigos vivos
                }
            }
        }
    }

    ///////////////////////////////////// Metodo findEnemies //////////////////////////////////////////
    public void findEnemies() //Busca los enemigos en la escena al inicio y los agrega a la lista
    {
        for (int i = 0; i < numEnemies; i++)                                                                //Recorre la lista en función del número de enemigos especificado en el loader
        {
            GameObject enemy = GameObject.Find("Enemy_" + i);                                               //Se declara una variable a los GameObjects de cada enemigo

            CellInfo enemyCell = enemy.GetComponent<EnemyBehaviour>().CurrentPosition();                    //Se obtiene su celda

            enemies.Add(enemyCell);                                                                         //Se añaden las celdas de los enemigos a la lista
        }
    }
}