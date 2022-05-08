using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g08
{   
    public enum TipoEquipo { FREMEN, HARKONNEN, GRABEN, VACIO, NEUTRO }
    public class Equipo : MonoBehaviour
    {
        public TipoEquipo _equipo;
        public TipoEquipo miEquipo()
        {
            return _equipo;
        }
    }
}

