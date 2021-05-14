using UnityEngine;

namespace Globals
{
    public static class GV
    {
        // Stats
        public const ushort MAXAp = 10;
        public const ushort MAXStat = 500;

        //Abilities
        public const ushort MAXDmg = 100;
        public const ushort MAXRange = 20;
        public const ushort MAXArea = 20;
        public const ushort MAXDuration = 20;
        public const ushort MAXMultiplier = 5;
        public const byte MAXAbilities = 5;     // 5 because we now handle the base attack as an ability

        //Items
        public const byte MAXStackSize = 20;

        //Inventory
        public const byte MAXInventorySize = 24;
        public const float PONDERATION = 0.75f;

        //Action Points
        public const byte AttackCost = 2;

        #region UI const values
        //Tweening
        public const float moveAnimationTime = 1.8f;
        public const float fadeAnimationTime = 0.8f;
        public const float shortAnimationTime = 0.2f;
        public const float shortAnimationTimeAlt = 0.4f;
        public const float pauseAnimationTime = 2.0f;
        //Components DisplayTurn
        public const string turnpanelLeftTag = "TurnPanelLeft";
        public const string turnpanelRightTag = "TurnPanelRight";
        public const string activeTurnDisplayTag = "ActiveTurnDisplay";
        //Components PlayerHUD
        public const string topLeftPanelTag = "ScreenTopLeft";
        public const string topRightPanelTag = "ScreenTopRight";
        public const string bottomLeftPanelTag = "ScreenBottomLeft";
        public const string bottomRightPanelTag = "ScreenBottomRight";
        public const string spellMenuTag = "SpellMenu";
        public const string mousePanelTag = "MousePanel";
        public const string characterIconTag = "CharacterIcon";
        public const string characterNameTag = "CharacterName";
        public const string characterStatsHPTag = "CharacterStatsHP";
        public const string characterStatsAPTag = "CharacterStatsAP";
        public const string characterStatsHPSliderTag = "CharacterStatsHPSlider";
        public const string characterStatsAPSliderTag = "CharacterStatsAPSlider";
        //Components Display Win_Lose
        public const string winloseTag = "WinLose";
        //Components End Turn
        public const string buttonEndTurnTag = "ButtonEndTurn";
        //Text
        public const string player = "Player Turn";
        public const string enemy = "Enemy Turn";
        #endregion

        #region AudioManager const values
        public const string master = "Master";
        public const string fx = "FX";
        public const string ambience = "Ambience";
        public const string actionFX = "Action FX";
        public const string uiFX = "UI FX";
        public const string music = "Music";
        public const string backgroundMusic = "Background Music";
        public const int channelID_sfx = 0;
        public const int channelID_music = 1;
        public const int channelID_ambience = 2;
        public const int channelID_ui = 3;
        #endregion

        #region DIALOGUE
        public const string trigger = "NextInput";
        public const string options = "Options";
        public const string skipdiagTrigger = "SkipDialogueTrigger";
        public const string quickDeathTrigger = "QuickDeath";
        public const string bossBattleTrigger = "IsInBossBattle";
        public const string bossBattleState = "BossBattleState";
        public const string dialogueTag = "Dialogue";
        public const int minUnitOnField = 2;
        public const int minTurnQuickDeath = 2;
        #endregion

        #region Static Strings

        // Static Strings
        public static readonly string TextureExt = ".png";
        public static readonly string ApplicationResourcePath = Application.dataPath + "/Resources/";
        public static readonly string TexturesPath = $"{ApplicationResourcePath}Sprites/";
        public static readonly string ResourcePath = "Assets/Resources/";
        public static readonly string SoBasePath = $"{ResourcePath}SO/";
        public static readonly string AbilitiesSoPath = $"{SoBasePath}Abilities/";
        public static readonly string ItemsSoPath = $"{SoBasePath}Items/";
        public static readonly string InventorySoPath = $"{SoBasePath}Inventory/";
        public static readonly string UnitSoPath = $"{SoBasePath}Units/";
        
        public const string EditorInventoryAsset = "EditorInventory.asset";
        public const string PlayerInventoryAsset = "PlayerInventory.asset";
        public const string UnitTypesAsset = "UnitTypes.asset";

        #endregion
    }
}