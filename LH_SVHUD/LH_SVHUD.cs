using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LH_SVHUD
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class LH_SVHUD : BaseUnityPlugin
    {
        public const string pluginGuid = "LH_SVHUD";
        public const string pluginName = "LH_SVHUD";
        public const string pluginVersion = "0.0.1";

        public static int StateToggle = 1;
        static GameObject FUHUD;
        static GameObject HPBar;
        static float HPBarVectY;
        static GameObject EnergyBar;
        static float EnergyBarVectY;
        static GameObject ShieldBar;
        static float ShieldBarVectY;
        static GameObject Target;
        static GameObject BuffIconsBottom;
        static GameObject Scanner;
        static GameObject MinimapTemp;
        static GameObject Credits;
        static GameObject ButtonsPanel;
        static GameObject XPBar;
        static GameObject SteerMode;
        static GameObject EnergyControl;
        static GameObject HeatControl;
        static GameObject InfoText;
        static float InfoTextRectY;
        static float InfoTextVectY;
        //static GameObject InfoPanel;
        static GameObject SideInfo;
        static GameObject FleetControl;
        static bool FleetControlBool;
        static float FleetControlRectY;

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(LH_SVHUD));
        }
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (GameObject.Find("MainMenu") == null && GameObject.Find("Player") != null)
                {
                    StateToggle++;
                    if (StateToggle > 3)
                        StateToggle = 1;
                    if (FUHUD == null)
                        FUHUDSetup();
                    FUHUDUpdate();
                }
            }
        }
        public void LateUpdate()
        {
            if (GameObject.Find("MainMenu") == null && StateToggle == 3 && Target.transform.Find("BtnHail").gameObject.activeSelf)
            {
                Target.transform.Find("BtnHail").gameObject.SetActive(false);
                Target.transform.Find("TargetText").GetComponent<Text>().text = "";
            }
        }

        [HarmonyPatch(typeof(WarpIn), "WarpStart")]
        [HarmonyPostfix]
        public static void SetFUHUD(SpaceShip __instance)
        {
            if (FUHUD == null)
                FUHUDSetup();
            if (__instance.CompareTag("Player"))
                FUHUDUpdate();
        }

        [HarmonyPatch(typeof(MinimapControl), "UpdateMap")]
        [HarmonyPostfix]
        public static void PreventFUHUDReset(MinimapControl __instance)
        {
            if (StateToggle == 3 && __instance.status == 1)
            {
                if (FUHUD == null)
                    FUHUDSetup();
                if (FUHUD.activeSelf)
                    FUHUD.SetActive(false);
                if (MinimapTemp.activeSelf)
                    MinimapTemp.SetActive(false);
                if (SideInfo.activeSelf)
                    SideInfo.SetActive(false);
            }
        }

        [HarmonyPatch(typeof(PlayerUIControl), "BlinkHeatSlot")]
        [HarmonyPrefix]
        public static bool PreventOverheatBlink()
        {
            if (StateToggle == 3)
                return false;
            else
                return true;
        }

        [HarmonyPatch(typeof(PlayerUIControl), "LateUpdate")]
        [HarmonyPostfix]
        public static void MinimizeInfoText(Text ___infoText, Rigidbody ___rb)
        {
            if (StateToggle == 2)
                ___infoText.text = Lang.Get(0, 145) + ": <b>" + ___rb.velocity.magnitude.ToString("0.0") + "</b>";
        }

        [HarmonyPatch(typeof(SideInfo), "Reset")]
        [HarmonyPostfix]
        public static void KeepSideInfoOFF()
        {
            if (FUHUD == null)
                FUHUDSetup();
            if (StateToggle == 3)
                SideInfo.SetActive(false);
        }

        [HarmonyPatch(typeof(SpaceShip), "FixedUpdate")]
        [HarmonyPostfix]
        public static void UpdateHeatCursor(SpaceShip __instance, PlayerControl ___pc)
        {
            if (StateToggle == 3 && ___pc)
            {
                int num2 = 0;
                for (int i = 0; i < __instance.weaponSlotHeat.Length; i++)
                {
                    int num3 = 2;
                    if (__instance.weaponSlotHeat[i] < __instance.slotHeatCap[i] * 0.5f)
                    {
                        num3 = 0;
                        if (__instance.weaponSlotHeat[i] > 10f)
                            num3 = 1;
                    }
                    if (__instance.weaponSlotHeat[i] > (float)__instance.slotHeatCap[i] * 0.8f)
                        num3 = 3;
                    if (num3 > num2)
                        num2 = num3;
                }
                ___pc.SetCursor(num2 + 1);
            }
        }

        [HarmonyPatch(typeof(CargoSystem), "Start")]
        [HarmonyPrefix]
        public static void PreventCargoError()
        {
            if (GameObject.Find("MainMenu") == null && StateToggle == 3)
            {
                if (FUHUD == null)
                    FUHUDSetup();
                FUHUD.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(CargoSystem), "Start")]
        [HarmonyPostfix]
        public static void ButPersistFUHUD()
        {
            if (GameObject.Find("MainMenu") == null && StateToggle == 3)
                FUHUD.SetActive(false);
        }

        [HarmonyPatch(typeof(GameManager), "PlayerDeath")]
        [HarmonyPrefix]
        public static void FixDying()
        {
            if (StateToggle == 3)
                FUHUD.SetActive(true);
        }
        public static void FUHUDSetup()
        {
            FUHUD = GameObject.FindGameObjectWithTag("PlayerUI");
            HPBar = FUHUD.transform.GetChild(3).gameObject;
            HPBarVectY = HPBar.transform.localPosition.y;
            EnergyBar = FUHUD.transform.GetChild(4).gameObject;
            EnergyBarVectY = EnergyBar.transform.localPosition.y;
            ShieldBar = FUHUD.transform.GetChild(5).gameObject;
            ShieldBarVectY = ShieldBar.transform.localPosition.y;
            Target = FUHUD.transform.parent.Find("Target").gameObject;
            BuffIconsBottom = FUHUD.transform.parent.Find("BuffIconsBottom").gameObject;
            Scanner = FUHUD.transform.parent.Find("Scanner").gameObject;
            MinimapTemp = FUHUD.transform.parent.Find("MinimapTemp").gameObject;
            Credits = FUHUD.transform.Find("Credits").gameObject;
            ButtonsPanel = FUHUD.transform.Find("ButtonsPanel").gameObject;
            XPBar = FUHUD.transform.Find("XPBar").gameObject;
            SteerMode = FUHUD.transform.Find("SteerMode").gameObject;
            EnergyControl = FUHUD.transform.Find("EnergyControl").gameObject;
            HeatControl = FUHUD.transform.Find("HeatControl").gameObject;
            InfoText = FUHUD.transform.Find("InfoText").gameObject;
            InfoTextRectY = InfoText.GetComponent<RectTransform>().sizeDelta.y;
            InfoTextVectY = InfoText.transform.localPosition.y;
            //InfoPanel = FUHUD.transform.parent.Find("InfoPanel").gameObject;
            SideInfo = FUHUD.transform.parent.Find("SideInfo").gameObject;
            FleetControl = FUHUD.transform.parent.Find("FleetControl").gameObject;
            FleetControlRectY = FleetControl.GetComponent<RectTransform>().sizeDelta.y;
        }
        public static void FUHUDUpdate()
        {
            if (StateToggle == 1)
            {
                FleetControl.SetActive(FleetControlBool);
                FUHUD.SetActive(true);
                Target.SetActive(true);
                BuffIconsBottom.SetActive(true);
                Scanner.SetActive(true);
                MinimapTemp.SetActive(true);
                //InfoPanel.SetActive(true);
                SideInfo.SetActive(true);
            }
            if (StateToggle == 2)
            {
                Credits.SetActive(false);
                ButtonsPanel.SetActive(false);
                XPBar.SetActive(false);
                SteerMode.SetActive(false);
                EnergyControl.SetActive(false);
                HeatControl.SetActive(false);
                InfoText.GetComponent<RectTransform>().sizeDelta = new Vector2(InfoText.GetComponent<RectTransform>().sizeDelta.x, 20);
                InfoText.transform.localPosition = new Vector3(InfoText.transform.localPosition.x, InfoText.transform.localPosition.y + 130, InfoText.transform.localPosition.z);
                FleetControl.GetComponent<RectTransform>().sizeDelta = new Vector2(FleetControl.GetComponent<RectTransform>().sizeDelta.x, -671);
                HPBar.transform.localPosition = new Vector3(HPBar.transform.localPosition.x, HPBar.transform.localPosition.y + 59, HPBar.transform.localPosition.z);
                EnergyBar.transform.localPosition = new Vector3(EnergyBar.transform.localPosition.x, EnergyBar.transform.localPosition.y + 59, EnergyBar.transform.localPosition.z);
                ShieldBar.transform.localPosition = new Vector3(ShieldBar.transform.localPosition.x, ShieldBar.transform.localPosition.y + 59, ShieldBar.transform.localPosition.z);
            }
            if (StateToggle == 3)
            {
                Credits.SetActive(true);
                ButtonsPanel.SetActive(true);
                XPBar.SetActive(true);
                SteerMode.SetActive(true);
                EnergyControl.SetActive(true);
                HeatControl.SetActive(true);
                InfoText.GetComponent<RectTransform>().sizeDelta = new Vector2(InfoText.GetComponent<RectTransform>().sizeDelta.x, InfoTextRectY);
                InfoText.transform.localPosition = new Vector3(InfoText.transform.localPosition.x, InfoTextVectY, InfoText.transform.localPosition.z);
                FleetControl.GetComponent<RectTransform>().sizeDelta = new Vector2(FleetControl.GetComponent<RectTransform>().sizeDelta.x, FleetControlRectY);
                FleetControlBool = FleetControl.activeSelf;
                FleetControl.SetActive(false);
                HPBar.transform.localPosition = new Vector3(HPBar.transform.localPosition.x, HPBarVectY, HPBar.transform.localPosition.z);
                EnergyBar.transform.localPosition = new Vector3(EnergyBar.transform.localPosition.x, EnergyBarVectY, EnergyBar.transform.localPosition.z);
                ShieldBar.transform.localPosition = new Vector3(ShieldBar.transform.localPosition.x, ShieldBarVectY, ShieldBar.transform.localPosition.z);
                FUHUD.SetActive(false);
                Target.SetActive(false);
                BuffIconsBottom.SetActive(false);
                Scanner.SetActive(false);
                MinimapTemp.SetActive(false);
                //InfoPanel.SetActive(false);
                SideInfo.SetActive(false);
            }
        }
    }
}

