using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Resources {
    public static class Database {

        public enum ResType {
            Base,
            Mountable,
            Consumable,
        }

        private static Dictionary<string, ResBase> Data;

        public static void Initialize () {
            Data = new Dictionary<string, ResBase>();
        }

        public static void RegisterResource (ResBase res) {
            Data.Add(res.ID, res);
        }

        // ---------------------------------------

        public static ResBase GetResource (string id) {
            ResBase res;
            if (Data.TryGetValue(id, out res)) return res;
            return null;
        }

    }
}
