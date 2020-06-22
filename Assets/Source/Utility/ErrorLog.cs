using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Utility {
    class ErrorLog {

        public static void ReportError (string message) {
            Debug.LogError(message);
        }

        public static void ReportError (string message, Exception e) {
            Debug.LogError(message + "\n" + e.Message + "\n" + e.StackTrace);
        }

    }
}
