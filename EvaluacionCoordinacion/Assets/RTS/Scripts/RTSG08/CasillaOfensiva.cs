using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace es.ucm.fdi.iav.rts.g08
{
    public class CasillaOfensiva
    {
        private TipoEquipo _colorEquipo;
        private int influencia;
        private MapaCasilla casilla;

        public void ActualizaAtaque()
        {
            _colorEquipo = casilla._colorEquipo;
            influencia = casilla._influenciaActual;
        }

        public CasillaOfensiva(MapaCasilla other)
        {
            casilla = other;
            _colorEquipo = casilla._colorEquipo;
            influencia = casilla._influenciaActual;
        }

        public int CompareTo(CasillaOfensiva other)
        {
            int result = casilla._influenciaActual - other.casilla._influenciaActual;
            if (this.Equals(other) && result == 0)
                return 0;
            else return result;
        }

        public bool Equals(CasillaOfensiva other)
        {
            return (this.casilla.Equals(other));
        }

        public override bool Equals(object obj)
        {
            CasillaOfensiva other = (CasillaOfensiva)obj;
            return Equals(other);
        }

        public MapaCasilla GetCasilla()
        {
            return casilla;
        }
    }

    public class ComparerAtaque : IComparer<CasillaOfensiva>
    {
        public int Compare(CasillaOfensiva x, CasillaOfensiva y)
        {
            int result = y.GetCasilla()._influenciaActual - x.GetCasilla()._influenciaActual;
            if (this.Equals(y) && result == 0)
                return 0;
            else return result;
        }
    }
}


