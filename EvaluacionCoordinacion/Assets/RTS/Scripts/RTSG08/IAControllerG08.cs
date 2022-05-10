/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/

using UnityEngine;
using System.Collections.Generic;

namespace es.ucm.fdi.iav.rts.g08
{
    public class Extractor : Unit
    {
        ExtractionUnit extractor;
        public bool extrayendo;
        LimitedAccess melange;
        public Extractor(ExtractionUnit ext)
        {
            extractor = ext;
            extrayendo = false;
            melange = null;
        }

        public void extrayendoRecurso(LimitedAccess melange_)
        {
            melange = melange_;
            extrayendo = true;
        }

        public ExtractionUnit getExtractor()
        {
            return extractor;
        }

        public LimitedAccess getMelange()
        {
            return melange;
        }
    }

    public enum Estrategia
    {

        ECONOMIA,
        DEFENSA,
        DEFENSADOBLE,
        ATAQUE,
        ATAQUEDOBLE,
        NONE
        ////  Farming consiste en priorizar la compra de extractores y con las unidades militares que se tenga defender estos extractores
        //Farming,
        ////  Defensivo consiste en jugar de forma defensiva ante los ataque enemigos, colocando unidades defendiendo estructuras 
        //Defensivo,
        ////  Ofensivo consiste en jugar de forma agresiva, atacando directamente a la base enemiga y a las zonas con mayor influencias del enemigo
        //Ofensivo,
        ////  Guerrilla consiste en ataques de pocas unidades a estructuras, extractores o anidades enemigas y luego reagruparse.
        //Guerrilla,
        ////  Entra en estado de emergencia cuando quedan pocas unidades ofensivas
        //Emergencia,
        ////  
        //NONE
    }


    // El controlador táctico que proporciono de ejemplo... simplemente manda órdenes RANDOM, y no hace ninguna interpretación (localizar puntos de ruta bien, análisis táctico, acción coordinada...) 
    public class IAControllerG08 : RTSAIController
    {
        // No necesita guardar mucha información porque puede consultar la que desee por sondeo, incluida toda la información de instalaciones y unidades, tanto propias como ajenas
        private int MyIndex { get; set; }
        private int enemyIndex { get; set; }
        private Estrategia estrategiaPrev;


        private List<LimitedAccess> _resources;
        private List<Tower> Torretas;
        

        private TipoEquipo _ownTeam;
        private TipoEquipo _enemyTeam;
        // Mis listas completas de instalaciones y unidades
        private List<BaseFacility> _allyBase;
        private List<ProcessingFacility> _allyFactory;
        private List<Extractor> _allyExtractors;
        private List<ExplorationUnit> _allyExplorers;
        private List<DestructionUnit> _allyDestroyers;

        // Las listas completas de instalaciones y unidades del enemigo
        private List<BaseFacility> _enemyBase;
        private List<ProcessingFacility> _enemyFactory;
        private List<ExtractionUnit> _enemyExtractores;
        private List<ExplorationUnit> _enemyExplorers;
        private List<DestructionUnit> _enemyDetroyers;
        bool estrategiaRush = false;

        private int minDestroyersToDefend = 2;
        private int ataquesFallidos = 0;

        // Número de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;

        // Última unidad creada
        private Unit LastUnit { get; set; }

        // Despierta el controlado y configura toda estructura interna que sea necesaria
        private void Awake()
        {
            Name = "IAV22G08";
            Author = "G08";
        }

        // El método de pensar que sobreescribe e implementa el controlador, para percibir (hacer mapas de influencia, etc.) y luego actuar.
        protected override void Think()
        {
            // Actualizo el mapa de influencia 
            // ...

            // Para las órdenes aquí estoy asumiendo que tengo dinero de sobra y que se dan las condiciones de todas las cosas...
            // (Ojo: esto no debería hacerse porque si me equivoco, causaré fallos en el juego... hay que comprobar que cada llamada tiene sentido y es posible hacerla)

            // Aquí lo suyo sería elegir bien la acción a realizar. 
            // En este caso como es para probar, voy dando a cada vez una orden de cada tipo, todo de seguido y muy aleatorio...
            //Debug.Log("Think");
            switch (ThinkStepNumber)
            {
                case 0: // El primer contacto, un paso especial
                    InitIA();
                    //Debug.Log("Extractores init: " + _allyExtractors.Count);
                    int rand = Random.Range(1, 4);
                    Debug.Log("Random: " + rand);
                    if (rand == 1)
                    {
                        Debug.Log("SalióRush");
                        estrategiaRush = true;
                    }
                    break;

                case 1:
                    //Debug.Log("Step1");
                    //MantenerEconomía();
                    Debug.Log("EstrategiaRush: " + estrategiaRush);
                    if (estrategiaRush)
                        InicioRush();
                    else
                        ThinkStepNumber++;
                    break;

                case 2:
                    AnalizarYSeleccionarEstrategia();
                    break;

                //case 3:
                //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.DESTRUCTION);
                //    break;

                //case 4:
                //    RTSGameManager.Instance.MoveUnit(this, LastUnit, OtherBaseFacility.transform);
                //    break;

                //case 5:
                //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXTRACTION);
                //    break;

                //case 6:
                //    RTSGameManager.Instance.MoveUnit(this, LastUnit, OtherBaseFacility.transform);
                //    break;

                //case 7:
                //    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyProcessingFacility.transform);
                //    break;

                //case 8:
                //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                //    break;

                //case 9:
                //    RTSGameManager.Instance.MoveUnit(this, LastUnit, OtherBaseFacility.transform);
                //    break;

                //case 10:
                //    LastUnit = RTSGameManager.Instance.CreateUnit(this, MyBaseFacility, RTSGameManager.UnitType.EXPLORATION);
                //    break;

                //case 11:
                //    RTSGameManager.Instance.MoveUnit(this, LastUnit, MyProcessingFacility.transform);
                //    // No lo hago... pero también se podrían crear y mover varias unidades en el mismo momento, claro...
                //    break;

                //case 12:
                //    Stop = true;
                //    break;
            }
            //Debug.Log("Controlador automático " + MyIndex + " ha finalizado el paso de pensamiento " + ThinkStepNumber);
            //ThinkStepNumber++;            //ThinkStepNumber++;
        }

        private void InitIA()
        {

            //audioSource = MapManager.GetInstance().gameObject.GetComponent<AudioSource>();
            // Coger indice asignado por el gestor del juego
            MyIndex = RTSGameManager.Instance.GetIndex(this);
            _ownTeam = RTSGameManager.Instance.GetBaseFacilities(MyIndex)[0].GetComponent<Unidad>().getUnitType();

            if (_ownTeam.Equals(TipoEquipo.FREMEN))
            {
                _enemyTeam = TipoEquipo.HARKONNEN;
            }
            else
            {
                _enemyTeam = TipoEquipo.FREMEN;
            }

            // Obtengo referencias a las cosas de mi enemigo cogiendo la lista de indices
            //correspondientes a cada jugador
            var indexList = RTSGameManager.Instance.GetIndexes();
            //Quito mi indice de esa lista
            indexList.Remove(MyIndex);
            //Asumo que el primer indice es el de mi enemigo
            enemyIndex = indexList[0];

            // Obtengo lista de accesos limitados
            _resources = RTSScenarioManager.Instance.LimitedAccesses;

            _allyExtractors = new List<Extractor>();

            //Inicializamos a parte la lista de los extractores para gestionar mejor su movimiento porque
            //si no, cuando hay varios que van a la misma casilla para extraer recursos se suelen quedar
            //"pillados"
            GetUpdatedLists();

            //Envíamos a los estractores que ya tenemos en juego a su objetivo de extraer recursos
            //teniendo en cuenta lo previamente mentado.
            gestionaExtractores();

            estrategiaPrev = Estrategia.NONE;

            //if ((RTSGameManager.Instance.GetMoney(MyIndex) <= 0
            //    && MiFactoria.Count <= 0
            //    && MisExtractores.Count <= 0
            //    && MisExploradores.Count <= 0
            //    && MisDestructores.Count <= 0)
            //    || MiBase.Count <= 0)
            //{
            //    throw new Exception("No hay condiciones suficientes para jugar");
            //}
            //Pasamos a AIGameLoop()
            ThinkStepNumber++;

            //audioSource.clip = getRdy;
            //audioSource.Play();
        }

        private void GetUpdatedLists()
        {
            _allyBase = RTSGameManager.Instance.GetBaseFacilities(MyIndex);
            _allyFactory = RTSGameManager.Instance.GetProcessingFacilities(MyIndex);

            _allyExtractors.Clear();
            foreach (ExtractionUnit extractor in RTSGameManager.Instance.GetExtractionUnits(MyIndex))
            {
                _allyExtractors.Add(new Extractor(extractor));
            }


            _allyExplorers = RTSGameManager.Instance.GetExplorationUnits(MyIndex);

            _allyDestroyers = RTSGameManager.Instance.GetDestructionUnits(MyIndex);
            //MisExtractores = RTSGameManager.Instance.GetExtractionUnits(MyIndex);

            _enemyBase = RTSGameManager.Instance.GetBaseFacilities(enemyIndex);
            _enemyFactory = RTSGameManager.Instance.GetProcessingFacilities(enemyIndex);
            _enemyExtractores = RTSGameManager.Instance.GetExtractionUnits(enemyIndex);
            _enemyExplorers = RTSGameManager.Instance.GetExplorationUnits(enemyIndex);
            _enemyDetroyers = RTSGameManager.Instance.GetDestructionUnits(enemyIndex);

            Torretas = RTSScenarioManager.Instance.Towers;
        }


        #region extractores
        private LimitedAccess getMelangeToFarm(Vector3 initPos)
        {
            LimitedAccess actMelange = null;
            float distance = 100000;
            foreach (LimitedAccess melange in _resources)
            {
                float melangeDistance = (initPos - melange.transform.position).magnitude;
                if (melange.OccupiedBy == null && melangeDistance < distance)
                {
                    actMelange = melange;
                    distance = melangeDistance;
                }
            }
            //actMelange.GetComponent<Renderer>().material.color = Color.cyan;
            return actMelange;
        }
        private void gestionaExtractores()
        {
            foreach (Extractor extractor in _allyExtractors)
            {
                if (extractor.getExtractor().Resources > 0) return;

                LimitedAccess currMelange = extractor.getMelange();
                if (currMelange && currMelange.OccupiedBy == null)
                {
                    extractor.getExtractor().Move(this, currMelange.transform);
                }
                else
                {
                    LimitedAccess nuevaMelange = getMelangeToFarm(extractor.getExtractor().transform.position);
                    if (nuevaMelange)
                    {
                        extractor.getExtractor().Move(this, nuevaMelange.transform);
                        extractor.extrayendoRecurso(nuevaMelange);
                    }
                }

            }
        }
        #endregion

        #region MetodosAuxiliares
        private void CrearMilitar(bool emergencia)
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            Unit unidadCreada;

            if (myMoney >= RTSGameManager.Instance.DestructionUnitCost && _allyDestroyers.Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                unidadCreada = RTSGameManager.Instance.CreateUnit(this, _allyBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();
                //Debug.Log("Crea Destructor: " + _allyDestroyers.Count);
                RTSGameManager.Instance.MoveUnit(this, unidadCreada, _enemyExtractores[Random.Range(0, _enemyExtractores.Count - 1)].transform);

            }
            else if (myMoney < RTSGameManager.Instance.DestructionUnitCost && emergencia || _allyDestroyers.Count == RTSGameManager.Instance.DestructionUnitsMax)
            {
                if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost && _allyExplorers.Count < RTSGameManager.Instance.ExplorationUnitsMax)
                {
                    unidadCreada = RTSGameManager.Instance.CreateUnit(this, _allyBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();
                    Debug.Log("Crea Explorer: " + _allyExplorers.Count);
                    RTSGameManager.Instance.MoveUnit(this, unidadCreada, _enemyExtractores[Random.Range(0, _enemyExtractores.Count - 1)].transform);
                }
            }
              
           
        }
        private int calcularFuerzaAliada()
        {
            return _allyExplorers.Count * 1 + _allyDestroyers.Count * 3;
        }
        private int calcularFuerzaEnemiga()
        {
            return _enemyExplorers.Count * 1 + _enemyDetroyers.Count * 3;
        }


        private bool peligroAtaqueEnemigo()//Cualquier tipo de ataque
        {

            bool ataqueInminente = false;

            foreach (DestructionUnit desUnit in _enemyDetroyers)
            {
                foreach (Extractor extUn in _allyExtractors)
                {
                    if (Vector3.Distance(desUnit.transform.position, extUn.transform.position) < 30)
                    {
                        ataqueInminente = true;

                    }
                }


                if (Vector3.Distance(desUnit.transform.position, _allyBase[0].transform.position) < 30)
                    ataqueInminente = true;
            }

            foreach (ExplorationUnit expUnit in _enemyExplorers)
            {
                foreach(Extractor extUn in _allyExtractors)
                {
                    if (Vector3.Distance(expUnit.transform.position,extUn.transform.position) < 30)
                    {
                        ataqueInminente = true;

                    }
                }

                if (Vector3.Distance(expUnit.transform.position, _allyBase[0].transform.position) < 45)
                    ataqueInminente = true;
            }

            return ataqueInminente;
        }

        #endregion

        #region EstrategiasIniciales
        private void InicioDefault()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            if (_allyDestroyers.Count >= _allyExtractors.Count && myMoney >= RTSGameManager.Instance.ExtractionUnitCost && _allyExtractors.Count < RTSGameManager.Instance.ExtractionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, _allyBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                //_allyExtractors.Add(actExtractor);
                RTSGameManager.Instance.MoveUnit(this, actExtractor.getExtractor(), getMelangeToFarm(_allyFactory[0].transform.position).transform.position);
                Debug.Log("Crea Extractor: " + _allyExtractors.Count);
            }
            else if (_allyDestroyers.Count < _allyExtractors.Count && myMoney >= RTSGameManager.Instance.DestructionUnitCost && _allyDestroyers.Count < RTSGameManager.Instance.DestructionUnitsMax)
            {
                RTSGameManager.Instance.CreateUnit(this, _allyBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();
                Debug.Log("Crea Destructor: " + _allyDestroyers.Count);

            }


            GetUpdatedLists();
        }


        private void InicioRush()
        {
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);


            if(myMoney > RTSGameManager.Instance.ExplorationUnitCost)
            {
                CrearMilitar(false);
            }
            else
            {
                Debug.Log("AtaqueRush");
                foreach (DestructionUnit desUnit in _allyDestroyers)
                {
                    RTSGameManager.Instance.MoveUnit(this, desUnit, _enemyBase[0].transform);
                }
                foreach (ExplorationUnit explUnit in _allyExplorers)
                {
                    RTSGameManager.Instance.MoveUnit(this, explUnit, _enemyBase[0].transform);

                }
                //si el ataque no ha sido inicial no ha sido efectivo y ha perdido todas las tropas
                if(_allyDestroyers.Count + _allyExplorers.Count == 0)ThinkStepNumber++;
            }


          

            GetUpdatedLists();
        }


        #endregion
        #region Estrategias


      
        private void AnalizarYSeleccionarEstrategia()
        {
            float fuerzaMilitarAliada = calcularFuerzaAliada();
            float fuerzaMilitarEnemiga = calcularFuerzaEnemiga();


            //Calcalar distancia de la fuerza enemiga


            //si la fuerza aliada es un 33% mayor atacar, si no potenciar economía
            if(fuerzaMilitarAliada/fuerzaMilitarEnemiga > 2)
            {

                if (ataquesFallidos < 2)
                    AtacarConTodo();
                else
                    AtacarDoble();
            }
            else if (peligroAtaqueEnemigo())
            {
                DefensaTotal();
            }
            else
            {
                MantenerEconomia();
            }
        }


        private void AtacarConTodo()
        {
            if (estrategiaPrev == Estrategia.ATAQUE)
                return;

            Debug.Log("ESTRATEGIA: AtacarConTodo");
            Transform destino = _enemyBase[0].transform; ;
           
            foreach (DestructionUnit desUnit in _allyDestroyers)
            {
                RTSGameManager.Instance.MoveUnit(this, desUnit, destino);
            }
            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                RTSGameManager.Instance.MoveUnit(this, explUnit, destino);
            }

            estrategiaPrev = Estrategia.ATAQUE;
        }


        private void AtacarDoble()
        {

            Debug.Log("ESTRATEGIA: AtacarConTodo");
            Transform destino = _enemyBase[0].transform; ;

            foreach (DestructionUnit desUnit in _allyDestroyers)
            {
                RTSGameManager.Instance.MoveUnit(this, desUnit, destino);
            }


            destino = _enemyExtractores[Random.Range(0, _enemyExtractores.Count - 1)].transform;
            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                RTSGameManager.Instance.MoveUnit(this, explUnit, destino);
            }

            estrategiaPrev = Estrategia.ATAQUE;
        }


        private void DefensaTotal()
        {
            Debug.Log("ESTRATEGIA: Defensa");
            Transform destino = _allyBase[0].transform; ;

            CrearMilitar(true);

            foreach (DestructionUnit desUnit in _allyDestroyers)
            {
                RTSGameManager.Instance.MoveUnit(this, desUnit, destino);
            }
            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                RTSGameManager.Instance.MoveUnit(this, explUnit, destino);
            }
        }


        private void MantenerEconomia()
        {
            Debug.Log("ESTRATEGIA: manternerEconomia");
            if(estrategiaPrev == Estrategia.ATAQUE)
            {
                ataquesFallidos++;
            }
            else
            {
                ataquesFallidos = 0;
            }
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            if (_allyDestroyers.Count >= _allyExtractors.Count && myMoney >= RTSGameManager.Instance.ExtractionUnitCost &&  _allyExtractors.Count < RTSGameManager.Instance.ExtractionUnitsMax)
            {
                Extractor actExtractor = new Extractor(RTSGameManager.Instance.CreateUnit(this, _allyBase[0], RTSGameManager.UnitType.EXTRACTION).GetComponent<ExtractionUnit>());
                _allyExtractors.Add(actExtractor);
                RTSGameManager.Instance.MoveUnit(this, actExtractor.getExtractor(), getMelangeToFarm(_allyFactory[0].transform.position).transform.position);
                //Debug.Log("Crea Extractor: " + _allyExtractors.Count);
            }
            else if(_allyDestroyers.Count < _allyExtractors.Count || _allyExtractors.Count == RTSGameManager.Instance.ExtractionUnitsMax)
            {
                CrearMilitar(false);

            }


            GetUpdatedLists();
        }
        #endregion

    }
}