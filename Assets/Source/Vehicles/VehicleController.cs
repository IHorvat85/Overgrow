using UnityEngine;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Vehicles {
    public class VehicleController : MonoBehaviour {

        public Vehicle vehicle { get; set; }
        public int vehicleNumber;

        public MeshRenderer hullRenderer;
        public TextMesh numberRenderer;

        private GameObject minimapIcon;

        // ---------------------------------------

        public Controls.Zone.ControlModes.IControlMode ControlMode;

        private const float DefaultFollowRange = 10;
        private const float DefaultCollectionRange = 10;

        // ---------------------------------------

        public float[] RandomMovementArray;
        public float RandomMovementT;

        // ---------------------------------------

        void Start () {
            RandomMovementArray = World.ProcGen.Noise.GetNoiseArray(16);
            RandomMovementT = UnityEngine.Random.Range(1f, RandomMovementArray.Length - 1);

            Controls.Zone.ZoneCamera.SetPlayerVehicle(this);
        }

        void Update () {
            if (IsVehicleFunctional()) {

                ControlMode.CheckInputs(this);

                UpdateRandomMovement();

                UpdateMinimapPosition();

                // update camera position
                Interface.ZoneMapUIController.InstanceRef.MoveCameraToVehicle(this);

            }
        }

        // ---------------------------------------

        private void UpdateRandomMovement () {
            RandomMovementT += Time.deltaTime * 1;
            RandomMovementT = RandomMovementT % (RandomMovementArray.Length - 2);
        }

        private void UpdateMinimapPosition () {
            if (minimapIcon == null) return;

            // calculate vehicle position as normalized value
            Utility.Vec2Int zoneSize = World.ZoneMapGenerator.ZoneSize;
            Vector2 anchors = new Vector2((transform.position.x / zoneSize.X) + 0.5f, (transform.position.y / zoneSize.Y) + 0.5f);

            // assign pos to min/max point anchors
            RectTransform rt = minimapIcon.GetComponent<RectTransform>();
            rt.anchorMin = anchors;
            rt.anchorMax = anchors;
        }

        // ---------------------------------------

        private bool CheckNullReferences () {
            if (this.vehicle == null) {
                Utility.ErrorLog.ReportError("Vehicle reference on VehicleController " + vehicleNumber + " is null!");
                return true;
            }
            if (this.vehicle.Hull == null) {
                Utility.ErrorLog.ReportError("Vehicle hull reference on VehicleController " + vehicleNumber + " is null!");
                return true;
            }
            return false;
        }

        // ---------------------------------------

        public bool IsVehicleFunctional () {
            // todo: this, to distinguish wrecks
            return true;
        }

        // ---------------------------------------

        public void ApplyMovementForces (Vector2 dirForce, float angForce) {

            dirForce *= 5000;
            angForce *= 3000;

            Rigidbody2D rigidbody = GetComponent<Rigidbody2D>();
            rigidbody.AddForce(dirForce * vehicle.Hull.EnginePowerStrafe * Time.deltaTime, ForceMode2D.Force);
            rigidbody.AddTorque(angForce * vehicle.Hull.EnginePowerTurn * Time.deltaTime, ForceMode2D.Force);
        }

        // ---------------------------------------

        public void AssignVehicle (Vehicle veh) {
            this.vehicle = veh;

            UpdateGraphicsFull();
            UpdatePhysicalProperties();

            UpdateMountingPoints();

            // create a minimap icon
            minimapIcon = Interface.ZoneMapUIController.GenerateMinimapVehiclePoint(vehicleNumber);

            // initialize control mode
            switch (veh.Hull.CtrlMode) {
                case VehicleHull.ControlMode.Car:
                    ControlMode = new Controls.Zone.ControlModes.CMCar();
                    break;
                case VehicleHull.ControlMode.Tank:
                    ControlMode = new Controls.Zone.ControlModes.CMTank();
                    break;
                case VehicleHull.ControlMode.Hover:
                    ControlMode = new Controls.Zone.ControlModes.CMHover();
                    break;
                default:
                    ControlMode = new Controls.Zone.ControlModes.CMNull();
                    break;
            }
        }

        private void UpdateGraphicsFull () {
            if (CheckNullReferences()) return;
            
            // Get main texture and assign
            Texture2D hullTexture;
            hullTexture = Utility.DefinitionLoader.GetImage(vehicle.Hull.GraphicIDDiffuse);
            if (hullTexture == null) {
                Utility.ErrorLog.ReportError("Cannot find graphic with ID " + vehicle.Hull.GraphicIDDiffuse + " to assign to VehicleController!");
                return;
            }
            else {
                // set properties of texture (filtering, transparency..?)
                hullTexture.filterMode = FilterMode.Bilinear;
            }

            hullRenderer.material.mainTexture = hullTexture;

            // get emission texture and assign
            Texture2D emissionTexture;
            emissionTexture = Utility.DefinitionLoader.GetImage(vehicle.Hull.GraphicIDEmission);
            if (emissionTexture == null) {
                Utility.ErrorLog.ReportError("Cannot find graphic with ID " + vehicle.Hull.GraphicIDEmission + " to assign to VehicleController!");
                return;
            }
            else {
                // set properties of texture (filtering, transparency..?)
                emissionTexture.filterMode = FilterMode.Bilinear;
            }

            hullRenderer.material.SetTexture("_EmissionMap", emissionTexture);

            // assign number renderer
            numberRenderer.transform.localPosition = (Vector3)vehicle.Hull.NumberRendererPos + new Vector3(0, 0, -1);
            numberRenderer.transform.localScale = (Vector3)vehicle.Hull.NumberRendererSize + new Vector3(0, 0, 1);
            numberRenderer.text = vehicleNumber.ToString();

            // set random offset for scratches texture
            hullRenderer.material.SetTextureOffset("_DetailAlbedoMap", new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f)));

        }

        private void UpdatePhysicalProperties () {

            Rigidbody2D rigidbody2d = GetComponent<Rigidbody2D>();

            // calculate total mass and apply to rigidbody
            float totalMass = vehicle.CalculateTotalMass();
            rigidbody2d.mass = totalMass;

            // update collider
            PolygonCollider2D coll = hullRenderer.gameObject.AddComponent<PolygonCollider2D>();
            coll.points = vehicle.Hull.CollisionPolygon;

            // set renderer size
            hullRenderer.transform.localScale = (Vector3)vehicle.Hull.Size + new Vector3(0, 0, 1);

            // set linear and angular drag based on control mode
            switch (vehicle.Hull.CtrlMode) {
                case VehicleHull.ControlMode.Car:
                    
                    break;
                case VehicleHull.ControlMode.Tank:

                    break;
                case VehicleHull.ControlMode.Hover:
                    rigidbody2d.drag = 1;
                    
                    break;
                default:
                    break;
            }



        }

        private void UpdateMountingPoints () {

            // todo: pull mounts from vehicle reference and instantiate / assign GO's

        }

        // ---------------------------------------

        public float GetAssistRange () {

            // todo: if this has engineering class, return the repair range on that

            // otherwise, return the default follow range
            return DefaultFollowRange;
        }
        public float GetCollectionRange () {

            // todo: if this has engineering class, return the repair range on that

            // otherwise, return the default collection range
            return DefaultCollectionRange;
        }
        public float GetWeaponRange () {

            // todo: loop thru all installed weapons and return the highest range

            return 0;
        }

    }
}
