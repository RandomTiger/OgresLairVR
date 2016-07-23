using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace OgresLair.Game
{
    public class GameConstants
    {
        // player
        public static readonly float playerMaxHealth        = 100.0f;     
        public static readonly float playerTurnSpeed        = 6.0f;
        public static readonly float playerInitalFirePower  = 100.0f;
        public static readonly float playerFireFrequency    = 0.2f;   

        // turret
        public static readonly float turretInitialHealth    = 30.0f;    
        public static readonly float turretFirePower        = 50.0f;
        public static readonly float turretFireFrequency    = 0.2f;

        // baddy generator
        public static readonly float baddyInitialHealth     = 10.0f;    
        public static readonly int baddyGeneratorMaxBaddies = 10;
        public static readonly float baddyDrainAmountPerSec = 10.0f;
        public static readonly float baddyGenerateTime      = 10.0f;

        // moving walls
        public static readonly float wallPushDrainAmountPerSec = 5.0f;

        // ogre 
        public static readonly float ogreDrainAmountPerSec = 15.0f;

        // pick ups
        public static readonly int pickupGemBlueScoreChange     = 25;
        public static readonly int pickupGemGreenScoreChange    = 10;
        public static readonly int pickupGemPinkScoreChange     = 50;
        public static readonly int pickupFoodHealthChange       = 30;



        // Engine constants
        public static readonly int playerWeaponMaxParticles = 100;
        public static readonly int turretWeaponMaxParticles = 40;       
    }
}