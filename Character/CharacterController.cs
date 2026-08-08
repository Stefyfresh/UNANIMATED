using Arcade.Unlockables;
using Rhythm;
using UnityEngine;

namespace UNANIMATED.Character
{
    public static class CharacterController
    {
        public static RhythmCharacterSelector spawner;
        public static string customPrimaryCharacter;
        public static string customSecondaryCharacter;

        // public static GameplayCharacterInfo[] validCharacters =
        // {
        //     new("Beat", false),
        //     new("Beat (Hoodie)", false),
        //     new("Beat (Guitar)", false),
        //     new("Beat (Up)", false),
        //     new("Beat (Nothing)", false),
        //     new("Clef", false),
        //     new("Quaver", false),
        //     new("Quaver (Acoustic)", false),
        //     new("Quaver (CQC)", false),
        //     new("Treble", false),
        //     new("Rest", false),
        //     new("Rest (OMF)", false),
        //     new("Eve", false),
        //     new("Grace", false),
        //     new("Crest", true),
        //     new("Crest (Maid)", true),
        //     new("DC", true),
        //     new("Poco", true),
        //     new("Apoco", true),
        //     new("Penny", true),
        //     new("Sforzando", true),
        //     new("JamieP", true),
        //     new("Quaver (Shrimp)", true),
        // };

        public static void Reset()
        {
            customPrimaryCharacter = FileStorage.beatmapOptions.primaryCharacter;
            customSecondaryCharacter = FileStorage.beatmapOptions.primaryCharacter;
            // spawner = null;
        }





        public static void ParseCharacterCommand(CommandEventInfo currentCommand)
        {
            // Get relevant variables
            CharacterOverride type = System.Enum.Parse<CharacterOverride>(currentCommand.GetStringParam(0));
            float time = currentCommand.Duration / 1000f;

            // Logging
            UNANIMATED.Logger.LogInfo($"Parsed Character command at {currentCommand.Time} ms: {type} | params {string.Join(", ", currentCommand.Parameters)} | length {time * 1000:0} ms");



            switch (type)
            {
                case CharacterOverride.Reset:
                    Reset();
                    ReplacePrimaryCharacter();
                    ReplaceSecondaryCharacter();
                    ReplaceBackgroundCharacters();
                    break;
                case CharacterOverride.SetCharacter:
                    customPrimaryCharacter = currentCommand.GetStringParam(1);
                    if (customPrimaryCharacter == "Reset") customPrimaryCharacter = FileStorage.beatmapOptions.primaryCharacter;
                    ReplacePrimaryCharacter();

                    customSecondaryCharacter = currentCommand.GetStringParam(2);
                    if (customSecondaryCharacter == "Reset") customSecondaryCharacter = FileStorage.beatmapOptions.secondaryCharacter;
                    ReplaceSecondaryCharacter();

                    ReplaceBackgroundCharacters();
                    break;
            }
        }




        public static void ReplacePrimaryCharacter()
        {
            if (string.IsNullOrWhiteSpace(customPrimaryCharacter)) customPrimaryCharacter = FileStorage.beatmapOptions.primaryCharacter;


            if (spawner.index.TryGetCharacter(customPrimaryCharacter, out CharacterIndex.Character character2))
            {
                GameObject newCharacterGO = Object.Instantiate(character2.prefab, spawner.player.transform);
                RhythmPlayerAnimator currentAnimator = spawner.player.currentAnimator;
                spawner.player.SetCurrentAnimator(newCharacterGO.GetComponent<RhythmPlayerAnimator>());

                if (currentAnimator) Object.Destroy(currentAnimator.gameObject);

            }
            else
            {
                UNANIMATED.Logger.LogWarning($"[Character] Could not set assist character \"{customPrimaryCharacter}\" as it does not exist!");
            }
        }



        public static void ReplaceSecondaryCharacter()
        {
            if (string.IsNullOrWhiteSpace(customSecondaryCharacter)) customSecondaryCharacter = FileStorage.beatmapOptions.secondaryCharacter;

            if (spawner.index.TryGetCharacter(customSecondaryCharacter, out CharacterIndex.Character character2))
            {
                GameObject newCharacterGO = Object.Instantiate(character2.assistPrefab, spawner.assist.transform);
                RhythmAssistAnimator currentAnimator = spawner.assist.currentAnimator;
                spawner.assist.currentAnimator = newCharacterGO.GetComponent<RhythmAssistAnimator>();
                spawner.assist.GetComponents();

                if (currentAnimator) Object.Destroy(currentAnimator.gameObject);

                // if (spawner.assist.atRest)
                // {
                spawner.assist.sprite.color = currentAnimator._sprite.color;
                // }
            }
            else
            {
                UNANIMATED.Logger.LogWarning($"[Character] Could not set assist character \"{customSecondaryCharacter}\" as it does not exist!");
            }
        }



        public static void ReplaceBackgroundCharacters()
        {
            Utils.DestroyChildren(spawner.BGCharL);
            Utils.DestroyChildren(spawner.BGCharM);
            Utils.DestroyChildren(spawner.BGCharML);
            Utils.DestroyChildren(spawner.BGCharMR);
            Utils.DestroyChildren(spawner.BGCharR);

            spawner.InstantiateBG(customPrimaryCharacter, customSecondaryCharacter);
        }



        // public class GameplayCharacterInfo(string name, bool isDLC)
        // {
        //     public string name = name;
        //     public bool isDLC = isDLC;
        // }



        // public static void ParseGameplayCommand(HitObjectInfo currentCommand)
        // {

        //     // Get relevant variables
        //     GameplayOverride type = (GameplayOverride)int.Parse(currentCommand.hitSample[0]);
        //     float secondData = float.Parse(currentCommand.hitSample[1]);
        //     float thirdData = float.Parse(currentCommand.hitSample[2]);
        //     // float fourthData = float.Parse(currentCommand.hitSample[3]);
        //     float time = 0;
        //     if (currentCommand.IsHoldType()) time = (int.Parse(currentCommand.objectParams[0]) - currentCommand.time) / 1000f;

        //     // Logging
        //     UNANIMATED.Logger.LogInfo($"Parsed gameplay command at {currentCommand.time} ms: {type} | params {string.Join(", ", currentCommand.hitSample)} | length {time * 1000:0} ms");



        //     switch (type)
        //     {
        //         case GameplayOverride.Normal:
        //             Reset();
        //             ReplacePrimaryCharacter();
        //             ReplaceSecondaryCharacter();
        //             ReplaceBackgroundCharacters();
        //             break;
        //         case GameplayOverride.Character:
        //             int primaryIndex = (int)secondData;
        //             if (primaryIndex != 0)
        //             {
        //                 customPrimaryCharacter = CharacterIndexToString(primaryIndex - 2);
        //                 ReplacePrimaryCharacter();
        //             }

        //             int secondaryIndex = (int)thirdData;
        //             if (secondaryIndex != 0)
        //             {
        //                 customSecondaryCharacter = CharacterIndexToString(secondaryIndex - 2);
        //                 ReplaceSecondaryCharacter();
        //             }

        //             ReplaceBackgroundCharacters();
        //             break;
        //     }
        // }






        // public static string CharacterIndexToString(int index)
        // {
        //     string output = string.Empty;
        //     try
        //     {
        //         if (index >= 0)
        //         {
        //             output = validCharacters[index].name;
        //             if (validCharacters[index].isDLC) UNANIMATED.Logger.LogWarning($"[Gameplay] A character command was given with a DLC character. This may not work correctly!!");
        //         }
        //     }
        //     catch (System.Exception)
        //     {
        //         UNANIMATED.Logger.LogWarning($"[Gameplay] Could not find character at index {index}!");
        //     }
        //     return output;
        // }
    }
}