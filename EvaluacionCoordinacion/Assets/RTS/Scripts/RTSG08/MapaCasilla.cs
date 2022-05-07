using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g08
{
    public class MapaCasilla : MonoBehaviour
    {
        public ColorEquipo _colorEquipo;
        private List<Unidad> _ejercitoHarkonnen;
        private List<Unidad> _ejercitoFremen;
        private List<Unidad> _ejercitoGraben;
        public int _influenciaActual;
        private int _filas;
        private int _columnas;
        private int _prioHarkonnen, _prioFremen, _prioGraben;
        private int _defensaHarkonnen, _defensaFremen;
        private CasillaOfensiva _cOfensiva;
        private CasillaDefensiva _cDefensiva;
        void Start()
        {
            _influenciaActual = 0;
            _colorEquipo = ColorEquipo.VACIO;

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
                case ColorEquipo.HARKONNEN:
                    cl = Color.yellow;
                    break;
                case ColorEquipo.FREMEN:
                    cl = Color.blue;
                    break;
                case ColorEquipo.GRABEN:
                    cl = Color.green;
                    break;
                case ColorEquipo.NEUTRO:
                    cl = Color.gray;
                    break;
                case ColorEquipo.VACIO:
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
                case ColorEquipo.HARKONNEN:
                    _ejercitoHarkonnen.Add(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioHarkonnen += influencia;
                    }
                    break;
                case ColorEquipo.GRABEN:
                    _ejercitoGraben.Add(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioGraben += influencia;
                    }
                    break;
                case ColorEquipo.FREMEN:
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
                case ColorEquipo.HARKONNEN:
                    _ejercitoHarkonnen.Remove(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioHarkonnen -= influencia;
                    }
                    break;
                case ColorEquipo.GRABEN:
                    unidadesVerdes.Remove(unidad);
                    if (unidad._unidad.Equals(TipoUnidad.MILITAR))
                    {
                        _prioGraben -= influencia;
                    }
                    break;
                case ColorEquipo.FREMEN:
                    unidadesAzules.Remove(unidad);
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
        private void ModificaInfluenciaAlEntrar(ColorEquipo equipo, Unidad unidad, int influencia)
        {
            if (_influenciaActual < 0)
            {
                _influenciaActual = 0;
            }

            if (_prioHarkonnen < 0) _prioHarkonnen = 0;
            if (_prioFremen < 0) _prioFremen = 0;
            if (_prioGraben < 0) _prioGraben = 0;


            //Si es del mismo tipo que la casilla, la casilla es neutral o está vacía
            if (equipo.Equals(_colorEquipo) || _colorEquipo.Equals(ColorEquipo.NEUTRO) || _colorEquipo.Equals(ColorEquipo.VACIO))
            {
                //Si es una unidad militar
                if (unidad.Equals(TipoUnidad.MILITAR))
                {
                    //La casilla es del equipo de la unidad entrante
                    _colorEquipo = equipo;

                    //Actualizamos valor de la prioridadMilitar
                    ActualizaPrioridadCasilla(_colorEquipo);
                }
                //Si es una unidad de defensa
                else
                {
                    switch (teamType_)
                    {
                        case ColorEquipo.AMARILLO:
                            _defensaHarkonnen += influencia;
                            break;
                        case ColorEquipo.AZUL:
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
                    _colorEquipo = GetMayorPrio();

                    //si la casilla esta vacia o es neutral la prioridad militar es cero
                    if (_colorEquipo.Equals(ColorEquipo.VACIO) || _colorEquipo.Equals(ColorEquipo.NEUTRO))
                    {
                        _influenciaActual = 0;
                    }
                    else ActualizaPrioridadCasilla(_colorEquipo);
                }
                else
                    switch (equipo)
                    {
                        case ColorEquipo.HARKONNEN:
                            _defensaHarkonnen += influencia;
                            break;
                        case ColorEquipo.FREMEN:
                            _defensaFremen += influencia;
                            break;

                    }
            }
        }

        private void ModificaInfluenciaAlSalir(ColorEquipo tipoEquipo, TipoUnidad unidad, int influencia)
        {
            if (_influenciaActual < 0)
            {
                _influenciaActual = 0;
            }

            if (_prioHarkonnen < 0) _prioHarkonnen = 0;
            if (_prioFremen < 0) _prioFremen = 0;
            if (_prioGraben < 0) _prioGraben = 0;

            // si salgo en una casilla de mi equipo o neutral
            if (tipoEquipo.Equals(_colorEquipo) || _colorEquipo.Equals(ColorEquipo.NEUTRO))
            {
                //si es militar
                if (unidad.Equals(TipoUnidad.MILITAR))
                {
                    _colorEquipo = GetMayorPrio();

                    if (_colorEquipo.Equals(ColorEquipo.NEUTRO) || tipoEquipo.Equals(ColorEquipo.VACIO))
                    {
                        _influenciaActual = 0;
                    }
                    else
                    {
                        ActualizaPrioridadCasilla(_colorEquipo);
                    }
                }
                else// si soy de defensa
                    switch (tipoEquipo)
                    {
                        case ColorEquipo.HARKONNEN:
                            _defensaHarkonnen -= influencia;
                            break;
                        case ColorEquipo.FREMEN:
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
                        case ColorEquipo.HARKONNEN:
                            _defensaHarkonnen -= influencia;
                            break;
                        case ColorEquipo.FREMEN:
                            _defensaFremen -= influencia;
                            break;

                    }
                }

            }

        }
        public int GetInfluenciaDef()
        {
            if (_colorEquipo.Equals(ColorEquipo.FREMEN)) return _defensaFremen;

            return _defensaHarkonnen;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


