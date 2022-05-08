using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace es.ucm.fdi.iav.rts.g08
{
    public class CasillaDefensiva
    {
        private TipoEquipo _colorEquipo;
        private int influencia;
        private MapaCasilla casilla;

        public void ActualizaDefensa()
        {
            _colorEquipo = casilla._colorEquipo;
            influencia = casilla.GetInfluenciaDef();
        }

        public CasillaDefensiva(MapaCasilla other)
        {
            casilla = other;
            _colorEquipo = casilla._colorEquipo;
            influencia = casilla.GetInfluenciaDef();
        }

        public int CompareTo(CasillaDefensiva other)
        {
            int result = casilla._influenciaActual - other.casilla._influenciaActual;
            if (this.Equals(other) && result == 0)
                return 0;
            else return result;
        }

        public bool Equals(CasillaDefensiva other)
        {
            return (this.casilla.Equals(other) && this.casilla.Equals(other));
        }

        public override bool Equals(object obj)
        {
            CasillaDefensiva other = (CasillaDefensiva)obj;
            return Equals(other);
        }

        public MapaCasilla GetCasilla()
        {
            return casilla;
        }
    }

    public class ComparerDef : IComparer<CasillaDefensiva>
    {
        public int Compare(CasillaDefensiva x, CasillaDefensiva y)
        {
            int result = y.GetCasilla().GetInfluenciaDef() - x.GetCasilla().GetInfluenciaDef();
            if (this.Equals(y) && result == 0)
                return 0;
            else return result;
        }
    }
}


