using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g08
{
    public class MapaCasilla : MonoBehaviour
    {
        public TipoEquipo _colorEquipo;
        private List<Unidad> _ejercitoHarkonnen = new List<Unidad>();
        private List<Unidad> _ejercitoFremen = new List<Unidad>();
        private List<Unidad> _ejercitoGraben = new List<Unidad>();
        public int _influenciaActual;
        public int _filas;
        private int _columnas;
        public int _prioHarkonnen, _prioFremen, _prioGraben;
        private int _defensaHarkonnen, _defensaFremen;
        private CasillaOfensiva _cOfensiva;
        private CasillaDefensiva _cDefensiva;

        public void setMatrixPos(int x, int y) { Debug.Log(x + " " + y + "\n"); _filas = x; _columnas = y;  }

        public int getFilas() { return _filas; }

        public int getColumnas() { return _columnas; }

        void Start()
        {
            _influenciaActual = 0;
            _colorEquipo = TipoEquipo.VACIO;

            CambiaColor();

            //Casillas ofensivas y defensivas
            _cOfensiva = new CasillaOfensiva(this);
            _cDefensiva = new CasillaDefensiva(this);
        }

        private void CambiaColor()
        {
            Color cl = Color.red;
            switch (_colorEquipo)
            {
                case TipoEquipo.HARKONNEN:
                    cl = Color.yellow;
                    break;
                case TipoEquipo.FREMEN:
                    cl = Color.blue;
                    break;
                case TipoEquipo.GRABEN:
                    cl = Color.green;
                    break;
                case TipoEquipo.NEUTRO:
                    cl = Color.gray;
                    break;
                case TipoEquipo.VACIO:
                    cl = Color.white;
                    break;
                default:
                    break;
            }

            cl.a = 0.2f;
            gameObject.GetComponent<MeshRenderer>().material.color = cl;
        }
        public void UnidadEntraCasilla(Unidad unidad, int influencia)
        {
            switch (unidad._duenhoUnidad)
            {
                case TipoEquipo.HARKONNEN:
                    _ejercitoHarkonnen.Add(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioHarkonnen += influencia;
                    }
                    break;
                case TipoEquipo.GRABEN:
                    _ejercitoGraben.Add(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioGraben += influencia;
                    }
                    break;
                case TipoEquipo.FREMEN:
                    _ejercitoFremen.Add(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioFremen += influencia;
                    }
                    break;
                default:
                    _influenciaActual = 0;
                    break;
            }
            ModificaInfluenciaAlEntrar(unidad._duenhoUnidad, unidad._unidad, influencia);
            CambiaColor();
        }

        public void UnidadSaleCasilla(Unidad unidad, int influencia)
        {
            switch (unidad._duenhoUnidad)
            {
                case TipoEquipo.HARKONNEN:
                    _ejercitoHarkonnen.Remove(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioHarkonnen -= influencia;
                    }
                    break;
                case TipoEquipo.GRABEN:
                    _ejercitoGraben.Remove(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioGraben -= influencia;
                    }
                    break;
                case TipoEquipo.FREMEN:
                    _ejercitoFremen.Remove(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioFremen -= influencia;
                    }
                    break;
                default:
                    break;
            }

            ModificaInfluenciaAlSalir(unidad._duenhoUnidad, unidad._unidad, influencia);
            CambiaColor();
        }
        private void ModificaInfluenciaAlEntrar(TipoEquipo equipo, TipoUnidad unidad, int influencia)
        {
            if (_influenciaActual < 0)
            {
                _influenciaActual = 0;
            }

            if (_prioHarkonnen < 0) _prioHarkonnen = 0;
            if (_prioFremen < 0) _prioFremen = 0;
            if (_prioGraben < 0) _prioGraben = 0;


            //Si es del mismo tipo que la casilla, la casilla es neutral o está vacía
            if (equipo.Equals(_colorEquipo) || _colorEquipo.Equals(TipoEquipo.NEUTRO) || _colorEquipo.Equals(TipoEquipo.VACIO))
            {
                //Si es una unidad militar
                if (unidad.Equals(TipoUnidad.MILITAR))
                {
                    //La casilla es del equipo de la unidad entrante
                    _colorEquipo = equipo;

                    //Actualizamos valor de la prioridadMilitar
                    ActualizaPrioridad(_colorEquipo);
                }
                //Si es una unidad de defensa
                else
                {
                    switch (equipo)
                    {
                        case TipoEquipo.HARKONNEN:
                            _defensaHarkonnen += influencia;
                            break;
                        case TipoEquipo.FREMEN:
                            _defensaFremen += influencia;
                            break;

                    }
                }

            }
            //Si no es del mismo equipo
            else if (!equipo.Equals(_colorEquipo))
            {
                //es una unidad Militar
                if (unidad.Equals(TipoUnidad.MILITAR))
                {
                    //cogemos el team con mayor influencia en la casilla
                    _colorEquipo = getMaxInfluencia();

                    //si la casilla esta vacia o es neutral la prioridad militar es cero
                    if (_colorEquipo.Equals(TipoEquipo.VACIO) || _colorEquipo.Equals(TipoEquipo.NEUTRO))
                    {
                        _influenciaActual = 0;
                    }
                    else ActualizaPrioridad(_colorEquipo);
                }
                else
                    switch (equipo)
                    {
                        case TipoEquipo.HARKONNEN:
                            _defensaHarkonnen += influencia;
                            break;
                        case TipoEquipo.FREMEN:
                            _defensaFremen += influencia;
                            break;

                    }
            }
        }

        private void ModificaInfluenciaAlSalir(TipoEquipo tipoEquipo, TipoUnidad unidad, int influencia)
        {
            if (_influenciaActual < 0)
            {
                _influenciaActual = 0;
            }

            if (_prioHarkonnen < 0) _prioHarkonnen = 0;
            if (_prioFremen < 0) _prioFremen = 0;
            if (_prioGraben < 0) _prioGraben = 0;

            // si salgo en una casilla de mi equipo o neutral
            if (tipoEquipo.Equals(_colorEquipo) || _colorEquipo.Equals(TipoEquipo.NEUTRO))
            {
                //si es militar
                if (unidad.Equals(TipoUnidad.MILITAR))
                {
                    _colorEquipo = getMaxInfluencia();

                    if (_colorEquipo.Equals(TipoEquipo.NEUTRO) || tipoEquipo.Equals(TipoEquipo.VACIO))
                    {
                        _influenciaActual = 0;
                    }
                    else
                    {
                        ActualizaPrioridad(_colorEquipo);
                    }
                }
                else// si soy de defensa
                    switch (tipoEquipo)
                    {
                        case TipoEquipo.HARKONNEN:
                            _defensaHarkonnen -= influencia;
                            break;
                        case TipoEquipo.FREMEN:
                            _defensaFremen -= influencia;
                            break;

                    }
            }
            //si salgo de una casilla que no es de mi equipo
            else if (!tipoEquipo.Equals(_colorEquipo))
            {
                //si soy de defensa
                if (!unidad.Equals(TipoUnidad.MILITAR))
                {
                    switch (tipoEquipo)
                    {
                        case TipoEquipo.HARKONNEN:
                            _defensaHarkonnen -= influencia;
                            break;
                        case TipoEquipo.FREMEN:
                            _defensaFremen -= influencia;
                            break;

                    }
                }

            }

        }
        public int GetInfluenciaDef()
        {
            if (_colorEquipo.Equals(TipoEquipo.FREMEN)) return _defensaFremen;

            return _defensaHarkonnen;
        }

        private void ActualizaPrioridad(TipoEquipo dominanUnit)
        {
            switch (dominanUnit)
            {
                case TipoEquipo.HARKONNEN:
                    _influenciaActual = _prioHarkonnen;
                    break;
                case TipoEquipo.FREMEN:
                    _influenciaActual = _prioFremen;
                    break;
                case TipoEquipo.GRABEN:
                    _influenciaActual = _prioGraben;
                    break;
                case TipoEquipo.VACIO:
                    _influenciaActual = 0;
                    break;
                case TipoEquipo.NEUTRO:
                    _influenciaActual = 0;
                    break;
                default:
                    _influenciaActual = 0;
                    break;
            }
        }

        private TipoEquipo getMaxInfluencia()
        {

            if (_prioGraben == 0 && _prioFremen == 0 && _prioHarkonnen == 0)
            {
                return TipoEquipo.VACIO;
            }
            else if (_prioHarkonnen > _prioFremen && _prioHarkonnen > _prioGraben)
            {
                return TipoEquipo.HARKONNEN;
            }
            else if (_prioFremen > _prioHarkonnen && _prioFremen > _prioGraben)
            {
                return TipoEquipo.FREMEN;
            }
            else if (_prioGraben == 0 && _prioFremen == _prioHarkonnen)
            {
                return TipoEquipo.NEUTRO;
            }
            else
            {
                return TipoEquipo.GRABEN;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


