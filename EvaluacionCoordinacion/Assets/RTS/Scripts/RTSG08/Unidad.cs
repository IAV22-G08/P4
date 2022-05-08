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

        //Referencia a la casilla en la que nos encontrabamos en la iteracion actual
        private MapaCasilla _casillaAct;
        //Referencia a la casilla en la que nos encontramos en la iteracion anterior
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

        //Gestión del movimiento de las unidades para actualizar el mapa de influencia
        private void Update()
        {
            _casillaAct = MapManager.GetInstance().GetCasillaCercana(transform);

            //Ha habido cambio de casilla
            if (_casillaPrev != null && _casillaAct != _casillaPrev)
            {
                MapManager.GetInstance().ActualizaPrioridadAlSalir(_casillaPrev, this);
                MapManager.GetInstance().ActualizaPrioridadAlEntrar(_casillaAct, this);
            }
            //Si la prevCasilla es null, significa que estamos en la primera iteración del bucle
            else if (_casillaPrev == null) MapManager.GetInstance().ActualizaPrioridadAlEntrar(_casillaAct, this);

            _casillaPrev = _casillaAct;

        }

        private void OnDestroy()
        {
            if (_casillaPrev)
            {
                //Cuando se destruye esta entidad hay que quitar los valores de influencia de la misma en el mapa
                MapManager.GetInstance().ActualizaPrioridadAlSalir(_casillaPrev, this);
            }
        }

    }
}



