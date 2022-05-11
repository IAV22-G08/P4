using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g08
{
    public class MapManager : MonoBehaviour
    {
        private int filas;
        private int columnas;
        private Grid grid;
        private MapaCasilla[,] matriz;
        private static MapManager instance_;
        private bool visible = false;
        private Terrain terrain;

        Vector3 posIni;

        private MapaCasilla MaxprioAzul;
        private MapaCasilla MaxprioAmarillo;

        [Tooltip("Prefab de las casillas")]
        public GameObject ejemplo;

        public Grid getMapGrid() { return grid; }

        private void Awake()
        {
            if (instance_ == null)
            {
                instance_ = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        public static MapManager GetInstance()
        {
            return instance_;
        }

        //  Construye las casillas a lo largo del mapa
        void Start()
        {
            grid = GetComponentInParent<Grid>();
            terrain = GetComponentInParent<Terrain>();

            filas = (int)(terrain.terrainData.size.x / grid.cellSize.x);
            columnas = (int)(terrain.terrainData.size.z / grid.cellSize.z);


            matriz = new MapaCasilla[filas, columnas];
            //Debug.Log("A:" + matriz[1, 1]);
            Vector3 minPos;
            minPos.x = (grid.cellSize.x / 2);
            minPos.z = (grid.cellSize.z / 2);
            for (int i = 0; i < filas; i++)
            {
                for (int j = 0; j < columnas; j++)
                {
                    /*Debug.Log(i + " " + j + "\n"); */matriz[i, j] = new MapaCasilla();
                    GameObject currCasilla = Instantiate(ejemplo, transform);
                    currCasilla.transform.localScale = grid.cellSize;
                    Vector3 pos = new Vector3(minPos.x + (i * grid.cellSize.x), 0, minPos.z + (j * grid.cellSize.z));
                    currCasilla.transform.localPosition = pos;
                    matriz[i, j] = currCasilla.GetComponent<MapaCasilla>();
                    matriz[i, j].setMatrixPos(i, j);
                    if (i == 0 && j == 0) posIni = matriz[i, j].transform.position;

                }
            }
        }

        //  Actualiza una casilla al entrar una unidad en ella
        public void ActualizaPrioridadAlEntrar(MapaCasilla casilla, Unidad unit_)
        {
            casilla.UnidadEntraCasilla(unit_, unit_._influencia);

            if (unit_._unidad == TipoUnidad.DEFENSA) return;

            //Casillas adyacentes
            for (int i = casilla.getFilas() - unit_._rango; i <= casilla.getFilas() + unit_._rango; i++)
            {
                for (int j = casilla.getColumnas() - unit_._rango; j <= casilla.getColumnas() + unit_._rango; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i, j])
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].UnidadEntraCasilla(unit_, unit_._influencia - 1);
                    }
                }
            }

            if (MaxprioAzul == null || MaxprioAzul._prioFremen < casilla._prioFremen)
            {
                MaxprioAzul = casilla;
            }
            if (MaxprioAmarillo == null || MaxprioAmarillo._prioHarkonnen < casilla._prioHarkonnen)
            {
                MaxprioAmarillo = casilla;
            }

        }

        //  Actualiza una casilla al salir
        public void ActualizaPrioridadAlSalir(MapaCasilla casilla, Unidad unit_)
        {
            casilla.UnidadSaleCasilla(unit_, unit_._influencia);
            if (unit_._unidad == TipoUnidad.DEFENSA) return;

            int inf = unit_._influencia - 1;
            //recorremos la submatriz correspondiente
            for (int i = casilla.getFilas() - unit_._rango; i <= casilla.getFilas() + unit_._rango; i++)
            {
                for (int j = casilla.getColumnas() - unit_._rango; j <= casilla.getColumnas() + unit_._rango; j++)
                {
                    //comprobamos que no nos salimos de la matriz
                    if (i >= 0 && i < filas && j >= 0 && j < columnas && casilla != matriz[i, j])
                    {
                        //comprobamos que prioridad le corresponde
                        matriz[i, j].UnidadSaleCasilla(unit_, unit_._influencia - 1);
                       
                    }
                }
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (visible)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = true;
                    }
                }

                visible = !visible;
            }


            if (Input.GetKeyDown(KeyCode.G))
            {
                if (visible)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if(transform.GetChild(i).gameObject.GetComponent<MapaCasilla>()._colorEquipo == TipoEquipo.GRABEN)
                        {
                            transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = true;

                        }
                    }
                }

                visible = !visible;
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                if (visible)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (transform.GetChild(i).gameObject.GetComponent<MapaCasilla>()._colorEquipo == TipoEquipo.HARKONNEN)
                        {
                            transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = true;

                        }
                    }
                }

                visible = !visible;
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (visible)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        if (transform.GetChild(i).gameObject.GetComponent<MapaCasilla>()._colorEquipo == TipoEquipo.FREMEN)
                        {
                            transform.GetChild(i).gameObject.GetComponent<Renderer>().enabled = true;

                        }
                    }
                }

                visible = !visible;
            }
        }

        //Devuelve la casilla en función de un transform
        public MapaCasilla GetCasillaCercana(Transform pos)
        {
            int indX = Mathf.Abs((int)((pos.position.x - posIni.x) / grid.cellSize.x) + 1);
            int indZ = Mathf.Abs((int)((pos.position.z - posIni.z) / grid.cellSize.z));

            return matriz[indX, indZ];
        }

        public MapaCasilla getEnemyMaxPrio(TipoEquipo team)
        {

            if (team == TipoEquipo.HARKONNEN)
            {
                return MaxprioAzul;
            }
            else
            {
                return MaxprioAmarillo;
            }
        }

        public int getEnemyPrio(TipoEquipo team)
        {
            if (team == TipoEquipo.HARKONNEN)
            {
                return MaxprioAzul._prioFremen;
            }
            else
            {
                return MaxprioAmarillo._prioHarkonnen;
            }
        }

        public void ActualizarMapaInfluencia()
        {

        }
    }
};