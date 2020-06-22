using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Interface {
    public interface ITooltipData {
        string GetTooltipText ();
        Vector2 GetTooltipSize ();
    }
}
