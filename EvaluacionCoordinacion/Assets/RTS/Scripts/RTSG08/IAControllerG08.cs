using UnityEngine;
using System.Collections.Generic;

namespace es.ucm.fdi.iav.rts.g08
{

    //para saber dónde está siendo atacaado y por quién
    public struct SiendoAtacado
    {
        public bool allybase;
        public Unit enemigoAtacandoBase;
        public bool extractor;
        public Unit enemigoAtacandoExtr;
        
    }

    //para controlar las acciones del extractor
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
        DEFENSAEXTRACTOR,
        ATAQUE,
        ATAQUEDOBLE,
        ATAQUEEXTRACTOR,
        INCURSION,
        CONTRAINCURSION,
        NONE
    }


    // El controlador táctico que proporciono de ejemplo... simplemente manda órdenes RANDOM, y no hace ninguna interpretación (localizar puntos de ruta bien, análisis táctico, acción coordinada...) 
    public class IAControllerG08 : RTSAIController
    {
        
        private int MyIndex { get; set; }
        private int enemyIndex { get; set; }
        private Estrategia estrategiaPrev;
        private Estrategia estrategiaActual;

        //dónde se encuentran las minas para llevar extractores
        private List<LimitedAccess> _resources;
        private List<Tower> Torretas;
        

        private TipoEquipo _ownTeam;
        private TipoEquipo _enemyTeam;
       
        //Aqúí las listas para guardar las entidades del juego, la base y la Factory por si en una partida se quieren poner varios
        private List<BaseFacility> _allyBase;
        private List<ProcessingFacility> _allyFactory;
        private List<Extractor> _allyExtractors;
        private List<ExplorationUnit> _allyExplorers;
        private List<DestructionUnit> _allyDestroyers;

        //Las de los enemigos
        private List<BaseFacility> _enemyBase;
        private List<ProcessingFacility> _enemyFactory;
        private List<ExtractionUnit> _enemyExtractores;
        private List<ExplorationUnit> _enemyExplorers;
        private List<DestructionUnit> _enemyDetroyers;
       
        bool estrategiaRush = false;

        
        private int ataquesFallidos = 0;


        //Para el HUD
        private GUIStyle _labelStyle;
        private GUIStyle _labelSmallStyle;
        private GUIStyle _labelSmallUnits;


        //Para saber cuando ha llegado a un destino una unidad (mediante el método move)
        private float radioLlegada = 3;

        // Número de paso de pensamiento 
        private int ThinkStepNumber { get; set; } = 0;


        //Despierto al controlador de la IA y 
        private void Awake()
        {
            Name = "IAV22G08";
            Author = "G08";
            Equipo equipo = GetComponent<Equipo>();
            Color color = (equipo.miEquipo() == TipoEquipo.HARKONNEN) ? Color.cyan : Color.yellow;


            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 16;
            _labelStyle.normal.textColor = color;

            _labelSmallStyle = new GUIStyle();
            _labelSmallStyle.fontSize = 11;
            _labelSmallStyle.normal.textColor = color;


            _labelSmallUnits = new GUIStyle();
            _labelSmallUnits.fontSize = 11;
            _labelSmallUnits.normal.textColor = color;
        }

        private void OnGUI()
        {
            //Para las métricas el HUD
            float areaWidth = 150;
            float areaHeight = 250;
            if (MyIndex % 2 == 0)
                GUILayout.BeginArea(new Rect(0, 0, areaWidth, areaHeight));
            else
                GUILayout.BeginArea(new Rect(Screen.width - areaWidth, 0, Screen.width, areaHeight));
            GUILayout.BeginVertical();

            //La información relevante como el dinero, la estrategia o las tropas
            GUILayout.Label("[ C" + MyIndex + " ] " + RTSGameManager.Instance.GetMoney(MyIndex) + " solaris", _labelStyle);

            
            GUILayout.Label("Usando la estrategía \n" + estrategiaActual.ToString(), _labelSmallStyle);

            GUILayout.Label("\n\n\nTropas: \n" 
                + "Exploradores: " + _allyExplorers.Count.ToString() + 
                "\nDestructores: " + _allyDestroyers.Count.ToString() + 
                "\nExtractores: " + _allyExtractors.Count.ToString(), _labelSmallUnits);



            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        protected override void Think()
        {
           
            //Hay 3 pasos marcados, el primero para iniciar la IA, el segundo para elegir si realizar una estrategia inicial especial y el tercero en el que se queda en bucle
            switch (ThinkStepNumber)
            {
                case 0: 
                    InitIA();
                    //Hay una probabiliad baja de que haga un ataque relámpago al inicio
                    int rand = Random.Range(1, 15);
                    Debug.Log("Random: " + rand);
                    if (rand == 1)
                    {
                        Debug.Log("SalióRush");
                        estrategiaRush = true;
                    }
                    else
                    {
                        InicioDefault();
                    }
                    break;

                case 1:
                    Debug.Log("EstrategiaRush: " + estrategiaRush);
                    if (estrategiaRush)
                        InicioRush();
                    else
                        ThinkStepNumber++;
                    break;

                case 2:
                    AnalizarYSeleccionarEstrategia();
                    break;

                
            }
           
        }

        private void InitIA()
        {

            
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

            //Obtener referencias a sí misma y a la enemiga

            var indexList = RTSGameManager.Instance.GetIndexes();
            indexList.Remove(MyIndex);
            enemyIndex = indexList[0];

            _resources = RTSScenarioManager.Instance.LimitedAccesses;

            _allyExtractors = new List<Extractor>();

            //Actualziar todas las listas con las que llevamos las entidades y objetos
            GetUpdatedLists();

            //Enviar extractores a los puntos cercanos
            gestionaExtractores();

            estrategiaPrev = Estrategia.NONE;

            ThinkStepNumber++;

           
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
            //Hay que mirar si esán ocupadas para poder ir a ellas
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
            //Crea unidades siguiendo un orden y unas proporciones
            int myMoney = RTSGameManager.Instance.GetMoney(MyIndex);
            Unit unidadCreada;

            //Intenta crear exploradores y destructores con un ratio 2:1 
            //Si es una emergencia y no tiene dinero para un destructor saca explorador ya que necesita urgente tropas

            if (myMoney >= RTSGameManager.Instance.DestructionUnitCost && _allyDestroyers.Count < RTSGameManager.Instance.DestructionUnitsMax && _allyDestroyers.Count <= _allyExplorers.Count  / 2)
            {
                unidadCreada = RTSGameManager.Instance.CreateUnit(this, _allyBase[0], RTSGameManager.UnitType.DESTRUCTION).GetComponent<DestructionUnit>();
                int defenderMalange = Random.Range(0, 2);
                if (defenderMalange == 0)
                        DefendMelange(unidadCreada);

            }
            else if (myMoney < RTSGameManager.Instance.DestructionUnitCost && emergencia || _allyDestroyers.Count == RTSGameManager.Instance.DestructionUnitsMax || _allyDestroyers.Count > _allyExplorers.Count / 2)
            {
                if (myMoney >= RTSGameManager.Instance.ExplorationUnitCost && _allyExplorers.Count < RTSGameManager.Instance.ExplorationUnitsMax)
                {
                    unidadCreada = RTSGameManager.Instance.CreateUnit(this, _allyBase[0], RTSGameManager.UnitType.EXPLORATION).GetComponent<ExplorationUnit>();
                    int defenderMalange = Random.Range(0, 2);
                    if(defenderMalange == 0)
                        DefendMelange(unidadCreada);
                }
            }
              
           
        }


        private void DefendMelange(Unit unidad)
        {

            //Lleva a unidad a una melange cercana para que esté segura para los extraactores
            //Se posiciona en el lado de la base enemiga para protegerlo mejor y no molestar al extractor
            LimitedAccess actMelange = null;
            float distance = 150;
            List<int> melangesDisponiblesADefender = new List<int>();
            for (int x = 0; x < _resources.Count; x++ )
            {
                float melangeDistance = (unidad.transform.position - _resources[x].transform.position).magnitude;
                if (melangeDistance < distance)
                {
                    melangesDisponiblesADefender.Add(x);
                }
            }
            if(melangesDisponiblesADefender.Count > 0)
            {
                int melangeADefender = Random.Range(0, melangesDisponiblesADefender.Count);
                Vector3 dirBaseEnemiga = _enemyBase[0].transform.position - _resources[melangesDisponiblesADefender[melangeADefender]].transform.position;
                dirBaseEnemiga.Normalize();
                Vector3 posicionADefender = _resources[melangesDisponiblesADefender[melangeADefender]].transform.position + dirBaseEnemiga * 10;
               
                RTSGameManager.Instance.MoveUnit(this, unidad, posicionADefender);
            }
           
        }

        //Cálculo de portencia militar
        private int calcularFuerzaAliada()
        {
            return _allyExplorers.Count * 1 + _allyDestroyers.Count * 3;
        }
        private int calcularFuerzaEnemiga()
        {
            return _enemyExplorers.Count * 1 + _enemyDetroyers.Count * 3;
        }


        private SiendoAtacado peligroAtaqueEnemigo()
        {
            //Mirar si están atacando la base y los exploradores
            SiendoAtacado ataqueInminente;
            ataqueInminente.allybase = false;
            ataqueInminente.extractor = false;
            ataqueInminente.enemigoAtacandoBase = null;
            ataqueInminente.enemigoAtacandoExtr = null;
            GetUpdatedLists();

            foreach (DestructionUnit desUnit in _enemyDetroyers)
            {
                foreach (Extractor extUn in _allyExtractors)
                {
                    if (Vector3.Distance(desUnit.transform.position, extUn.getExtractor().transform.position) < 30)
                    {
                        ataqueInminente.extractor = true;
                        ataqueInminente.enemigoAtacandoExtr = desUnit;

                    }
                }


                if (Vector3.Distance(desUnit.transform.position, _allyBase[0].transform.position) < 30)
                {
                    ataqueInminente.allybase = true;
                    ataqueInminente.enemigoAtacandoBase = desUnit;
                }
            }

            foreach (ExplorationUnit expUnit in _enemyExplorers)
            {
                foreach(Extractor extUn in _allyExtractors)
                {
                    if (Vector3.Distance(expUnit.transform.position,extUn.getExtractor().transform.position) < 30)
                    {
                        ataqueInminente.extractor = true;
                        ataqueInminente.enemigoAtacandoExtr = expUnit;

                    }
                }

                if (Vector3.Distance(expUnit.transform.position, _allyBase[0].transform.position) < 45)
                {
                    ataqueInminente.allybase = true;
                    ataqueInminente.enemigoAtacandoBase = expUnit;
                }
                    

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
            //crea tropas y ataca rápido
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

            SiendoAtacado siendoAtacado = peligroAtaqueEnemigo();
            if (siendoAtacado.allybase && !siendoAtacado.extractor)
            {
                DefensaTotal(siendoAtacado.enemigoAtacandoBase);
                estrategiaPrev = Estrategia.DEFENSA;

            }
            //si la fuerza aliada es un 50% mayor atacar, si no potenciar economía
            else if (fuerzaMilitarAliada/fuerzaMilitarEnemiga > 1.5)
            {

                if (ataquesFallidos < 2)
                {
                    AtacarConTodo();
                    estrategiaPrev = Estrategia.ATAQUE;
                }
                else
                {
                    AtacarDoble();
                    estrategiaPrev = Estrategia.ATAQUEDOBLE;
                }
            }
            else if(estrategiaPrev == Estrategia.DEFENSAEXTRACTOR && _allyExtractors.Count < _enemyExtractores.Count)
            {
                //Si va por detrás en economía hace un esfuerzo militar desesperado para mermar la economía enemiga
                ContraIncursion();
            }
            else if(siendoAtacado.allybase && siendoAtacado.extractor)
            {
                DefensaDoble(siendoAtacado.enemigoAtacandoExtr, siendoAtacado.enemigoAtacandoBase);
                estrategiaPrev = Estrategia.DEFENSADOBLE;

            }
            else if (siendoAtacado.extractor)
            {
                DefensaExtractor(siendoAtacado.enemigoAtacandoExtr);
                estrategiaPrev = Estrategia.DEFENSAEXTRACTOR;

            }
            else if (_allyExplorers.Count >= 4)
            {
                AtacarIncursion();
                estrategiaPrev = Estrategia.INCURSION;

            }
            else
            {
                MantenerEconomia();
                estrategiaPrev = Estrategia.ECONOMIA;

            }
        }


        private void AtacarConTodo()
        {
            estrategiaActual = Estrategia.ATAQUE;

            if (estrategiaPrev == Estrategia.ATAQUE)
                return;


            //Ataca con TODO a la base principal
            Transform destino = _enemyBase[0].transform; ;
           
            foreach (DestructionUnit desUnit in _allyDestroyers)
            {
                if (Vector3.Distance(desUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, desUnit, destino);
            }
            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                if (Vector3.Distance(explUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, explUnit, destino);
            }

            estrategiaPrev = Estrategia.ATAQUE;
        }


        private void AtacarDoble()
        {
            estrategiaActual = Estrategia.ATAQUEDOBLE;

            Debug.Log("ESTRATEGIA: AtacarConTodo");
            Transform destino = _enemyBase[0].transform; ;

            //Ataca con destructores a la base enemiga y expoloradores a los extractores

            foreach (DestructionUnit desUnit in _allyDestroyers)
            {
                if (Vector3.Distance(desUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, desUnit, destino);
            }


            destino = _enemyExtractores[Random.Range(0, _enemyExtractores.Count - 1)].transform;
            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                if (Vector3.Distance(explUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, explUnit, destino);
            }

            estrategiaPrev = Estrategia.ATAQUE;
        }


        private void AtacarIncursion()
        {
            estrategiaActual = Estrategia.INCURSION;
            if (estrategiaPrev == Estrategia.INCURSION)
                return;

            Debug.Log("ESTRATEGIA: AtacarConTodo");
            Transform destino = _enemyBase[0].transform; ;

           

            //Con los exploradores disponibles ataca a un extractor enemigo
            destino = _enemyExtractores[Random.Range(0, _enemyExtractores.Count - 1)].transform;
            for (int x = 0; x < _allyExplorers.Count / 2; x++)
            {
                RTSGameManager.Instance.MoveUnit(this, _allyExplorers[x], destino);
            }

            estrategiaPrev = Estrategia.INCURSION;
        }


        private void ContraIncursion()
        {
            estrategiaActual = Estrategia.CONTRAINCURSION;
            if (estrategiaPrev == Estrategia.CONTRAINCURSION)
                return;

            Debug.Log("ESTRATEGIA: AtacarConTodo");
            Transform destino = _enemyBase[0].transform; ;



            //Ataca con la mayoria de tropas para mermar economía
            destino = _enemyExtractores[Random.Range(0, _enemyExtractores.Count - 1)].transform;
            for (int x = 0; x < _allyExplorers.Count; x++)
            {
                RTSGameManager.Instance.MoveUnit(this, _allyExplorers[x], destino);
            }
            
            for (int x = 0; x < _allyDestroyers.Count / 2; x++)
            {
                RTSGameManager.Instance.MoveUnit(this, _allyDestroyers[x], destino);
            }

            estrategiaPrev = Estrategia.INCURSION;
        }


        private void DefensaTotal(Unit enemigoAtacandoBase)
        {
            estrategiaActual = Estrategia.DEFENSA;

            if (estrategiaPrev == Estrategia.DEFENSA)
                return;

            //El grueso de las tropas enemigas se concentra alrededor de la base aliada y se defiende con todo

            Debug.Log("ESTRATEGIA: Defensa");
            Transform destino = _allyBase[0].GetComponent<BaseFacility>().SpawnTransform; ;

            //Crea tropas para manterner la defensa
            CrearMilitar(true);

            foreach (DestructionUnit desUnit in _allyDestroyers)
            {
                if (Vector3.Distance(desUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, desUnit, enemigoAtacandoBase.transform);
            }
            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                if (Vector3.Distance(explUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, explUnit, enemigoAtacandoBase.transform);
            }
        }

        private void DefensaExtractor(Unit enemigoAtacandoExtractor)
        {
            estrategiaActual = Estrategia.DEFENSAEXTRACTOR;

            if (estrategiaPrev == Estrategia.DEFENSAEXTRACTOR)
                return;
            //Los enemigos solo están atacando un extractor
            Transform destino = _allyBase[0].GetComponent<BaseFacility>().SpawnTransform; ;

            //Crea tropas para manterner la defensa
            CrearMilitar(true);

           

            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                if (Vector3.Distance(explUnit.transform.position, enemigoAtacandoExtractor.transform.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, explUnit, enemigoAtacandoExtractor.transform);
            }

            //si no hay exploradores para defender se lleva a la mitad de los destructores
            if(_allyExplorers.Count == 0)
            {
                for(int x = 0; x < _allyDestroyers.Count / 2; x++)
                {
                    if (Vector3.Distance(_allyDestroyers[x].transform.position, enemigoAtacandoExtractor.transform.position) > radioLlegada)
                        RTSGameManager.Instance.MoveUnit(this, _allyDestroyers[x], enemigoAtacandoExtractor.transform);
                }
              
            }
        }


        private void DefensaDoble(Unit enemigoAtacandoExt, Unit enemigoAtacandoBase)
        {
            estrategiaActual = Estrategia.DEFENSA;

            if(estrategiaPrev == estrategiaActual)
            {
                return;
            }

            Transform destino = _allyBase[0].transform; ;


            //Crea tropas para manterner la defensa
            CrearMilitar(true);

            foreach (DestructionUnit desUnit in _allyDestroyers)
            {
                if (Vector3.Distance(desUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, desUnit, enemigoAtacandoBase.transform);
            }
            foreach (ExplorationUnit explUnit in _allyExplorers)
            {
                if (Vector3.Distance(explUnit.transform.position, destino.position) > radioLlegada)
                    RTSGameManager.Instance.MoveUnit(this, explUnit,enemigoAtacandoExt.transform);
            }
        }


        private void MantenerEconomia()
        {
            Debug.Log("ESTRATEGIA: manternerEconomia");
            estrategiaActual = Estrategia.ECONOMIA;

            if (estrategiaPrev == Estrategia.ATAQUE)
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