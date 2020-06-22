using UnityEngine;
using System.Collections;
using Utility;

namespace Controls {
    public class WorldMouse : MonoBehaviour {

        public World.WorldMapTileController CheckMouseTile () {
            RaycastHit rayHit = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out rayHit, float.MaxValue)) {
                if (rayHit.collider.tag != "VoronoiTile") return null;
                World.WorldMapTileController tileCont = rayHit.collider.transform.parent.GetComponent<World.WorldMapTileController>();
                return tileCont;
            }
            return null;
        }

        private Vector3 GetMousePosWorld () {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }
        private Vec2Int GetTilePos (Vector3 pos) {
            pos += new Vector3(pos.x < 0 ? -1 : 0, pos.y < 0 ? -1 : 0, 0);
            return new Vec2Int((int)pos.x, (int)pos.y);
        }
    }
}
