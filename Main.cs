using System.Collections;
using System.Linq;
using FireRateToggle;
using GHPC.Player;
using GHPC.State;
using GHPC.UI.Hud;
using GHPC.Vehicle;
using GHPC.Weapons;
using MelonLoader;
using PactIncreasedLethality;
using UnityEngine;

[assembly: MelonInfo(typeof(FireRateToggleMod), "Fire-Rate Toggle", "1.0.1", "ATLAS")]
[assembly: MelonGame("Radian Simulations LLC", "GHPC")]

namespace FireRateToggle
{
    internal class FRT : MonoBehaviour {
        private WeaponSystem weapon;
        private AmmoFeed feed;
        private uint idx = 0;
        private float[] firerates; 

        static float[] bradley_firerates = { 0.3f, 0.5f}; // fast, slow 
        static float[] bmp2_firerates = { 0.1f, 0.2f };
        static string[] alert = { "High fire rate selected", "Low fire rate selected" };

        private void Awake() { 
            weapon = GetComponent<WeaponSystem>();
            feed = weapon.Feed;
            firerates = weapon.name == "25mm Gun M242" ? bradley_firerates : bmp2_firerates;
        }

        private void Update() {
            if (PlayerInput.Instance.CurrentPlayerWeapon.Weapon != weapon) return;

            if (Input.GetKeyDown(KeyCode.B)) {
                idx = idx ^ 1;
                weapon._cycleTimeSeconds = firerates[idx];
                feed._totalCycleTime = firerates[idx];

                if (!weapon.WeaponSound.SingleShotByDefault)
                {
                    weapon.WeaponSound.FinalStopLoop();
                    weapon.WeaponSound.SingleShotMode = !weapon.WeaponSound.SingleShotMode;
                }

                FireRateToggleMod.alert_hud.AddAlertMessage(alert[idx], 2f);
            }
        }
    }

    public class FireRateToggleMod : MelonMod
    {
        public static Vehicle[] vics;
        public static AlertHud alert_hud;

        public IEnumerator GetVics(GameState _)
        {
            vics = GameObject.FindObjectsByType<Vehicle>(FindObjectsSortMode.None);
            alert_hud = GameObject.Find("_APP_GHPC_").transform.Find("UIHUDCanvas/system alert text").GetComponent<AlertHud>();

            foreach (Vehicle v in vics) {
                if (v.gameObject.GetComponent<AlreadyConverted>() != null) continue;

                if (v.UniqueName == "M2BRADLEY" || v.UniqueName == "M2BRADLEY(ALT)" || v.UniqueName == "BMP2" || v.UniqueName == "BMP2_SA") {
                    v.LoadoutManager._weaponsManager.Weapons[0].Weapon.gameObject.AddComponent<FRT>();

                    v.gameObject.AddComponent<AlreadyConverted>();
                }
            }

            yield break;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (Util.menu_screens.Contains(sceneName)) return;

            StateController.RunOrDefer(GameState.GameReady, new GameStateEventHandler(GetVics), GameStatePriority.Medium);
        }
    }
}
