using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g08
{
    public enum TipoUnidad
    {
        MILITAR, DEFENSA
    }

    public class Unidad : MonoBehaviour
    {
        public TipoEquipo _duenhoUnidad;
        public TipoUnidad _unidad;
        public int _influencia;
        public int _rango = 0;

        private MapaCasilla _casillaAct;
        private MapaCasilla _casillaPrev;
        public Unidad(Unidad unitCopy)
        {
            _duenhoUnidad = unitCopy._duenhoUnidad;
            _unidad = unitCopy._unidad;
            _influencia = unitCopy._influencia;
            _rango = unitCopy._rango;
        }

        public TipoEquipo getUnitType()
        {
            return _duenhoUnidad;
        }

        private void Update()
        {
            //Controlar si entran o salen de las casillas para catualizar el mapa de influencia
            _casillaAct = MapManager.GetInstance().GetCasillaCercana(transform);
            if (_casillaPrev != null && _casillaAct != _casillaPrev)
            {
                MapManager.GetInstance().ActualizaPrioridadAlSalir(_casillaPrev, this);
                MapManager.GetInstance().ActualizaPrioridadAlEntrar(_casillaAct, this);
                Debug.Log("update unidad");

            }
            else if (_casillaPrev == null) MapManager.GetInstance().ActualizaPrioridadAlEntrar(_casillaAct, this);

            _casillaPrev = _casillaAct;

        }

        private void OnDestroy()
        {
            if (_casillaPrev)
            {
                MapManager.GetInstance().ActualizaPrioridadAlSalir(_casillaPrev, this);
            }
        }

    }
}



