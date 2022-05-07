using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts.g08
{   
    public enum ColorEquipo { FREMEN, HARKONNEN, GRABEN, VACIO, NEUTRO }
    public class Equipo : MonoBehaviour
    {
        public ColorEquipo _equipo;
        public ColorEquipo miEquipo()
        {
            return _equipo;
        }
    }
}

