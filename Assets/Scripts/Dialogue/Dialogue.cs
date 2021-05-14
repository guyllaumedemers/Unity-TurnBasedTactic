using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Dialogue
{
    private static readonly Dictionary<string, Dictionary<string, string>> dialogues = new Dictionary<string, Dictionary<string, string>>
    {
        { "en", new Dictionary<string, string>
            {
            /// Player
                {"Player_BattleOpening", "You stand no chance against the power of the light."},
                {"Player_Winning", "There's is nothing for you here demon, this is our land."},
                {"Player_AskForSurrender", "Surrender, you stood no chance against the human race."},
                {"Player_Loosing", "Evil is upon us but the human race wil survive this pleague."},
                {"Player_Dying", "You won't destroy us until there's one of us still standing."},
                {"Player_QuickLost", "How could this be?" },
            /// Enemy
                {"Enemy_BattleOpening", "Our master will protect us against the light, human."},
                {"Enemy_Winning", "Your childrens and wifes will make a nice addition to the master's collection."},
                {"Enemy_AskForSurrender", "Surrender, the force of evil are always victorious."},
                {"Enemy_Loosing", "You seem to be a stronger opponent than we though."},
                {"Enemy_Dying", "Our master revenge will fall upon you in the darkness of the night."},
                {"Enemy_QuickLost", "How could this be?"},
            /// Player Boss Battle
                {"Player_BossBattleOpening", "At last, we meet again."},
                {"Player_BBWinning", "I will avenge my father."},
                {"Player_BBLoosing", "..."},
            /// Enemy Boss Battle
                {"Boss_BossBattleOpening", "You shouldn't have come here insect."},
                {"Boss_BBWinning", "Your soul will be lost in the aether."},
                {"Boss_BBLoosing", "How?! Miserable Human."}
            }
        }
    };

    public static string DisplayText(string locomotion, string key)
    {
        if (!dialogues[locomotion].ContainsKey(key))
            return null;
        return dialogues[locomotion][key];
    }
}
