using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Security.Cryptography;
using RoR2.Stats;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AlexTheDragon
{
    [R2APISubmoduleDependency(nameof(ArtifactAPI), nameof(DirectorAPI), nameof(InteractablesAPI))]
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.AlexTheDragon.OneRun100", "One Run 100%", "1.0.0")]
    public class OneRun100 : BaseUnityPlugin
    {
        DirectorCard lesserWisp, jellyfish, beetle, lemurian, hermitCrab, imp, vulture, roboBallMini, miniMushroom, bell, beetleGuard, bison, golem, parent, clayBruiser, greaterWisp, lemurianBruiser,
            nullifier, beetleQueen, clayBoss, stoneTitanBlackBeach, stoneTitanDampCave, stoneTitanGolemPlains, stoneTitanGooLake, vagrant, magmaWorm, roboBallBoss,
            gravekeeper, impBoss, grandParent, electricWorm, lunarGolem, lunarWisp, lunarExploder, scavenger = new DirectorCard();

        /*
        Empty = 11;
        Cube = 7;
        Diamond = 5;
        int Triangle = 3;
        int Circle = 1;
        */


        byte[] transcriptionRecipe = {7, 7, 7,
                                      5, 5, 5,
                                      7, 7, 7};
        byte[] inputRecipe = {0,0,0,
                              0,0,0,
                              0,0,0};

        
        bool artifactTranscriptionWasOn = false;
        private HUD hud = null;

        public void Awake()
        {
            
            if (transcriptionEvent == null)
                transcriptionEvent = new UnityEvent();
            Logger.LogMessage("Ready for action!"); //REMOVE BEFORE RELEASE
            AchievementHooks();
            EscapeHook();
            LunarScavengerHooks();
            AddHooks();
            ArtifactHooks();

            OneRun100.Transcription = ScriptableObject.CreateInstance<ArtifactDef>();
            OneRun100.Transcription.nameToken = "Artifact of Transcription";
            OneRun100.Transcription.descriptionToken = "Logged enemies cannot spawn.";
            OneRun100.Transcription.smallIconDeselectedSprite = LoadSprite(OneRunDone.Properties.Resources.texTranscriptionResizedDisabled);
            OneRun100.Transcription.smallIconSelectedSprite = LoadSprite(OneRunDone.Properties.Resources.texTranscriptionResizedEnabled);
            OneRun100.Transcription.unlockableDef = ScriptableObject.CreateInstance<UnlockableDef>(); //WARNING: ADD A CORRECT UNLOCKABLEDEF BEFORE RELEASE
            //OneRun100.Transcription.pickupModelPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/GenericPickup");
            OneRun100.Transcription.cachedName = "Transcription";
            //OneRun100.Transcription.pickupModelPrefab.
            ArtifactAPI.Add(Transcription);

            //On.RoR2.UI.PauseScreenController.Awake += MyFunc;



            On.RoR2.UI.PauseScreenController.Awake += (orig, self) =>
            {
                Logger.LogMessage("PauseScreenController on!");

                PauseScreenController psc = self;
                //hud.mainContainer.transform
                GameObject myObject = new GameObject("Tester");
                myObject.transform.SetParent(psc.mainPanel);
                RectTransform rectTransform = myObject.AddComponent<RectTransform>();
                Vector3 localScale = new Vector3();
                Vector3 localPosition = new Vector3();
                localScale.Set(0.2f, 0.2f, 0.2f);
                localPosition.Set(230f, 100f, 0f);
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.localScale = localScale;
                rectTransform.localPosition = localPosition;
                myObject.AddComponent<Image>();
                myObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("textures/itemicons/yeah");

                orig(self);
            };
            On.RoR2.UI.PauseScreenController.OpenSettingsMenu += (orig, self) =>
            {
                Logger.LogMessage("Opening SettingsMenu");
                orig(self);
            };

            /*foreach (GameObject gameObject in Resources.LoadAll<GameObject>("Prefabs/"))
            {
                Logger.LogMessage(gameObject.name);
            }*/


            /*if (//disable artifact)
            {
                ArtifactDef artifactDef = //Get Artifact that they want to disable
                RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, false);
            }*/


            /*if (complete a prismatic trial)
            {
            UnlockAchievement("CompletePrismaticTrial");
                if (playing as Merc && not falling below 100% hp)
                {
                    UnlockAchievement("MercCompleteTrialWithFullHealth");
                }

            }*/

            //[Message:One Run 100%] GenericPickup(Clone) (UnityEngine.GameObject)

            //[Message:One Run 100%] AltarSkeletonBody (UnityEngine.GameObject) [nhukukamamsd opinion]

            //[Message:One Run 100%] TimedChest (UnityEngine.GameObject)

            //[Message:One Run 100%] FusionCellDestructibleBody(Clone) (UnityEngine.GameObject) (??? Rallypoint Delta) (big boomers?)

            //[Message:One Run 100%] VultureEggBody(Clone) (UnityEngine.GameObject) 
        }


        private Run.TimeStamp fadeOutTime;
        private bool lunarFinished;
        private bool removedItem;
        public static ArtifactDef Transcription;
        private UnityEvent transcriptionEvent;
        bool goneThroughArenaOnce = false;
        /// <summary>
        /// Removes and then reintroduces every vanilla monster back to their respective stage. THIS ONLY GETS CALLED ONCE WE REMOVE THE PREVIOUSLY ACTIVE TRANSCRIPTION.
        /// </summary>
        private void ResetAllMonsters()
        {
            //Remove all monsters first, since the Director has OCD and starts screaming if he has more cards than he wants
            DirectorCardCategorySelection d = RoR2Content.mixEnemyMonsterCards;
            foreach (DirectorCardCategorySelection.Category category in d.categories)
            {
                foreach (DirectorCard card in category.cards)
                {
                    DirectorAPI.Helpers.RemoveExistingMonster(card.spawnCard.name);
                }
            }
            DirectorAPI.Helpers.RemoveExistingMonster("cscHermitCrab");
            DirectorAPI.Helpers.RemoveExistingMonster("cscGrandParent");
            DirectorAPI.Helpers.RemoveExistingMonster("cscClayBoss");
            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanDampCaves");
            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanGolemPlains");
            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanGooLake");
            DirectorAPI.Helpers.RemoveExistingMonster("cscLunarGolem");
            DirectorAPI.Helpers.RemoveExistingMonster("cscLunarWisp");
            DirectorAPI.Helpers.RemoveExistingMonster("cscLunarExploder");
            DirectorAPI.Helpers.RemoveExistingMonster("cscScav");


            DirectorAPI.MonsterCategory basicMonsters = DirectorAPI.MonsterCategory.BasicMonsters;
            DirectorAPI.MonsterCategory minibosses = DirectorAPI.MonsterCategory.Minibosses;
            DirectorAPI.MonsterCategory champions = DirectorAPI.MonsterCategory.Champions;
            bool looped;
            if (Run.instance && Run.instance.GetType() == typeof(Run) && Run.instance.loopClearCount > 0) //This line has been taken from RoR2.Achievements.LoopOnceAchievement.Check();
            {
                looped = true;
            }
            else
            {
                looped = false;
            }

            DirectorAPI.Stage distantRoost = DirectorAPI.Stage.DistantRoost;
            DirectorAPI.Stage titanicPlains = DirectorAPI.Stage.TitanicPlains;
            DirectorAPI.Stage wetlandAspect = DirectorAPI.Stage.WetlandAspect;
            DirectorAPI.Stage abandonedAqueduct = DirectorAPI.Stage.AbandonedAqueduct;
            DirectorAPI.Stage rallypointDelta = DirectorAPI.Stage.RallypointDelta;
            DirectorAPI.Stage scorchedAcres = DirectorAPI.Stage.ScorchedAcres;
            DirectorAPI.Stage abyssalDepths = DirectorAPI.Stage.AbyssalDepths;
            DirectorAPI.Stage sirenCall = DirectorAPI.Stage.SirensCall;
            DirectorAPI.Stage skyMeadow = DirectorAPI.Stage.SkyMeadow;
            DirectorAPI.Stage gildedCoast = DirectorAPI.Stage.GildedCoast;
            DirectorAPI.Stage bulwarksAmbry = DirectorAPI.Stage.ArtifactReliquary;
            DirectorAPI.Stage arena = DirectorAPI.Stage.VoidCell;


            //SunderedGrove, Commencement, Arena
            DirectorAPI.Stage custom = DirectorAPI.Stage.Custom;


            //Distant Roost
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanBlackBeach, champions, distantRoost);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleQueen, champions, distantRoost);
            DirectorAPI.Helpers.AddNewMonsterToStage(vagrant, champions, distantRoost);
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, distantRoost); //?
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, distantRoost); //?
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, distantRoost);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, distantRoost);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetle, basicMonsters, distantRoost);
            if (looped)
            {
                DirectorAPI.Helpers.AddNewMonsterToStage(jellyfish, basicMonsters, distantRoost);
            }

            //Titanic Plains
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanGolemPlains, champions, titanicPlains);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleQueen, champions, titanicPlains);
            DirectorAPI.Helpers.AddNewMonsterToStage(vagrant, champions, titanicPlains);
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, titanicPlains); //?
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, titanicPlains); //?
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, titanicPlains);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetle, basicMonsters, titanicPlains);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, titanicPlains);
            if (looped)
            {
                DirectorAPI.Helpers.AddNewMonsterToStage(hermitCrab, basicMonsters, titanicPlains);
                DirectorAPI.Helpers.AddNewMonsterToStage(jellyfish, basicMonsters, titanicPlains);
            }


            //Wetland Aspect
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanBlackBeach, champions, wetlandAspect);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleQueen, champions, wetlandAspect);
            DirectorAPI.Helpers.AddNewMonsterToStage(vagrant, champions, wetlandAspect);
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, wetlandAspect); //?
            DirectorAPI.Helpers.AddNewMonsterToStage(bell, minibosses, wetlandAspect); //?
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, wetlandAspect);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, wetlandAspect);
            DirectorAPI.Helpers.AddNewMonsterToStage(jellyfish, basicMonsters, wetlandAspect);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetle, basicMonsters, wetlandAspect);


            //Abandoned Aqueduct
            DirectorAPI.Helpers.AddNewMonsterToStage(clayBoss, champions, abandonedAqueduct);
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanGooLake, champions, abandonedAqueduct);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleQueen, champions, abandonedAqueduct);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleGuard, minibosses, abandonedAqueduct);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, abandonedAqueduct);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, abandonedAqueduct);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, abandonedAqueduct);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetle, basicMonsters, abandonedAqueduct);
            if (looped)
            {
                DirectorAPI.Helpers.AddNewMonsterToStage(clayBruiser, minibosses, abandonedAqueduct);
            }

            //Rallypoint Delta 
            DirectorAPI.Helpers.AddNewMonsterToStage(clayBoss, champions, rallypointDelta);
            DirectorAPI.Helpers.AddNewMonsterToStage(magmaWorm, champions, rallypointDelta);
            DirectorAPI.Helpers.AddNewMonsterToStage(impBoss, champions, rallypointDelta);
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, rallypointDelta);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, rallypointDelta);
            DirectorAPI.Helpers.AddNewMonsterToStage(bison, minibosses, rallypointDelta); //?
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, rallypointDelta);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, rallypointDelta);
            DirectorAPI.Helpers.AddNewMonsterToStage(imp, basicMonsters, rallypointDelta);
            if (looped)
            {
                DirectorAPI.Helpers.AddNewMonsterToStage(electricWorm, champions, rallypointDelta);
                DirectorAPI.Helpers.AddNewMonsterToStage(nullifier, minibosses, rallypointDelta);
            }


            //Scorched Acres
            DirectorAPI.Helpers.AddNewMonsterToStage(clayBoss, champions, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(gravekeeper, champions, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(impBoss, champions, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleGuard, minibosses, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(clayBruiser, minibosses, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetle, basicMonsters, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, scorchedAcres);
            DirectorAPI.Helpers.AddNewMonsterToStage(imp, basicMonsters, scorchedAcres);
            if (looped)
            {
                DirectorAPI.Helpers.AddNewMonsterToStage(vulture, basicMonsters, scorchedAcres);
            }

            //Abyssal Depths 
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanDampCave, champions, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(impBoss, champions, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(magmaWorm, champions, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(electricWorm, champions, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurianBruiser, minibosses, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(bell, minibosses, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(imp, basicMonsters, abyssalDepths);
            DirectorAPI.Helpers.AddNewMonsterToStage(hermitCrab, basicMonsters, abyssalDepths);

            //Sundered Grove 
            DirectorAPI.Helpers.AddNewMonsterToStage(clayBoss, champions, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanBlackBeach, champions, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(vagrant, champions, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurianBruiser, basicMonsters, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(jellyfish, basicMonsters, custom, "rootjungle");
            DirectorAPI.Helpers.AddNewMonsterToStage(miniMushroom, basicMonsters, custom, "rootjungle");

            //Siren's Call 
            DirectorAPI.Helpers.AddNewMonsterToStage(magmaWorm, champions, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(roboBallBoss, champions, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(vagrant, champions, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(bell, minibosses, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurianBruiser, minibosses, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetle, basicMonsters, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(jellyfish, basicMonsters, sirenCall);
            DirectorAPI.Helpers.AddNewMonsterToStage(vulture, basicMonsters, sirenCall);
            if (looped)
            {
                DirectorAPI.Helpers.AddNewMonsterToStage(nullifier, minibosses, sirenCall);
            }

            //Sky Meadow 
            DirectorAPI.Helpers.AddNewMonsterToStage(roboBallBoss, champions, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(magmaWorm, champions, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(electricWorm, champions, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(grandParent, champions, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(parent, minibosses, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurianBruiser, minibosses, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(miniMushroom, basicMonsters, skyMeadow);
            DirectorAPI.Helpers.AddNewMonsterToStage(bell, basicMonsters, skyMeadow);

            //Commencement
            DirectorAPI.Helpers.AddNewMonsterToStage(lunarGolem, basicMonsters, custom, "moon2");
            DirectorAPI.Helpers.AddNewMonsterToStage(lunarWisp, basicMonsters, custom, "moon2");
            DirectorAPI.Helpers.AddNewMonsterToStage(lunarExploder, basicMonsters, custom, "moon2");

            //Gilded Coast
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanBlackBeach, champions, gildedCoast);
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, gildedCoast);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, gildedCoast);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurianBruiser, minibosses, gildedCoast);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, gildedCoast);

            //Arena
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanBlackBeach, champions, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleQueen, champions, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(vagrant, champions, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(impBoss, champions, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(roboBallBoss, champions, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(gravekeeper, champions, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(grandParent, champions, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(greaterWisp, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(bell, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurianBruiser, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(bison, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(clayBruiser, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(parent, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetleGuard, minibosses, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(beetle, basicMonsters, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(jellyfish, basicMonsters, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(imp, basicMonsters, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(vulture, basicMonsters, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(roboBallMini, basicMonsters, arena);
            DirectorAPI.Helpers.AddNewMonsterToStage(miniMushroom, basicMonsters, arena);

            //Bulwark's Ambry
            DirectorAPI.Helpers.AddNewMonsterToStage(vagrant, champions, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(stoneTitanDampCave, champions, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(electricWorm, champions, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurianBruiser, minibosses, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(golem, minibosses, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(lemurian, basicMonsters, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(lesserWisp, basicMonsters, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(jellyfish, basicMonsters, bulwarksAmbry);
            DirectorAPI.Helpers.AddNewMonsterToStage(scavenger, champions, bulwarksAmbry);

            DirectorAPI.Helpers.TryApplyChangesNow();
        }

        /// <summary>
        /// Loads the DirectorCards needed for reintroduction of enemies.
        /// </summary>
        private void InitialCardLoading()
        {
            DirectorCardCategorySelection d = RoR2Content.mixEnemyMonsterCards;
            foreach (DirectorCardCategorySelection.Category category in d.categories)
            {
                foreach (DirectorCard card in category.cards)
                {
                    switch (card.spawnCard.name.ToLower())
                    {
                        case "csclesserwisp":
                            {
                                lesserWisp = card;
                                break;
                            }
                        case "cscjellyfish":
                            {
                                jellyfish = card;
                                break;
                            }
                        case "cscbeetle":
                            {
                                beetle = card;
                                break;
                            }
                        case "csclemurian":
                            {
                                lemurian = card;
                                break;
                            }
                        case "cscimp":
                            {
                                imp = card;
                                break;
                            }
                        case "cscvulture":
                            {
                                vulture = card;
                                break;
                            }
                        case "cscroboballmini":
                            {
                                roboBallMini = card;
                                break;
                            }
                        case "cscminimushroom":
                            {
                                miniMushroom = card;
                                break;
                            }
                        case "cscbell":
                            {
                                bell = card;
                                break;
                            }
                        case "cscbeetleguard":
                            {
                                beetleGuard = card;
                                break;
                            }
                        case "cscbison":
                            {
                                bison = card;
                                break;
                            }
                        case "cscgolem":
                            {
                                golem = card;
                                break;
                            }
                        case "cscparent":
                            {
                                parent = card;
                                break;
                            }
                        case "cscclaybruiser":
                            {
                                clayBruiser = card;
                                break;
                            }
                        case "cscgreaterwisp":
                            {
                                greaterWisp = card;
                                break;
                            }
                        case "csclemurianbruiser":
                            {
                                lemurianBruiser = card;
                                break;
                            }
                        case "cscnullifier":
                            {
                                nullifier = card;
                                break;
                            }
                        case "cscbeetlequeen":
                            {
                                beetleQueen = card;
                                break;
                            }
                        case "csctitanblackbeach":
                            {
                                stoneTitanBlackBeach = card;
                                break;
                            }
                        case "cscvagrant":
                            {
                                vagrant = card;
                                break;
                            }
                        case "cscmagmaworm":
                            {
                                magmaWorm = card;
                                break;
                            }
                        case "cscroboballboss":
                            {
                                roboBallBoss = card;
                                break;
                            }
                        case "cscgravekeeper":
                            {
                                gravekeeper = card;
                                break;
                            }
                        case "cscimpboss":
                            {
                                impBoss = card;
                                break;
                            }
                        case "cscelectricworm":
                            {
                                electricWorm = card;
                                break;
                            }
                        case "cscscav":
                            {
                                scavenger = card;
                                break;
                            }
                        default: break;
                    }
                }
            }

            //Load the other ones manually B]
            var allCSC = Resources.LoadAll<CharacterSpawnCard>("SpawnCards/CharacterSpawnCards");
            foreach (CharacterSpawnCard csc in allCSC)
            {
                if (csc != null)
                {
                    switch (csc.name)
                    {

                        case "cscHermitCrab":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = false,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 1,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Far,
                                    selectionWeight = 1
                                };
                                hermitCrab = dCard;
                                break;
                            }
                        case "cscGrandparent":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 2
                                };
                                grandParent = dCard;
                                break;
                            }
                        case "cscClayBoss":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 1
                                };
                                clayBoss = dCard;
                                break;
                            }
                        case "cscTitanDampCave":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 1
                                };
                                stoneTitanDampCave = dCard;
                                break;
                            }
                        case "cscTitanGolemPlains":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 1
                                };
                                stoneTitanGolemPlains = dCard;
                                break;
                            }
                        case "cscTitanGooLake":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 1
                                };
                                stoneTitanGooLake = dCard;
                                break;
                            }
                        case "cscLunarGolem":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 3
                                };
                                lunarGolem = dCard;
                                break;
                            }
                        case "cscLunarWisp":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 1
                                };
                                lunarWisp = dCard;
                                break;
                            }
                        case "cscLunarExploder":
                            {
                                var dCard = new DirectorCard
                                {
                                    spawnCard = csc,
                                    allowAmbushSpawn = true,
                                    forbiddenUnlockableDef = null,
                                    minimumStageCompletions = 0,
                                    preventOverhead = false,
                                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                                    selectionWeight = 1
                                };
                                lunarExploder = dCard;
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Checks if this certain DirectorCard has been unlocked by checking the strings of each currently unlocked unlockable.
        /// </summary>
        /// <param name="DC">DirectorCard for this monster.</param>
        private bool IsMonsterNotUnlocked(DirectorCard DC)
        {
            foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
            {
                int i = 0;
                UserProfile userProfile = user.userProfile;
                StatSheet statSheet = userProfile.statSheet;
                int unlockableCount = statSheet.GetUnlockableCount();
                int num = 0;
                while (i < unlockableCount)
                {
                    string unlockableName = statSheet.GetUnlockable(i).cachedName;
                    if (unlockableName.StartsWith("Logs.") & !unlockableName.StartsWith("Logs.Stages."))
                    {
                        //“Logs.LemurianBody.0”
                        unlockableName = unlockableName.ToLower().Substring(5);
                        unlockableName = Regex.Replace(unlockableName, @"body\.0", "");
                        unlockableName = Regex.Replace(unlockableName, @"\.0", "");
                        unlockableName = "csc" + unlockableName;
                        //we have unlocked this monster log
                        if (unlockableName == DC.spawnCard.prefab.name)
                        {
                            return false;
                        }

                        num++;

                    }
                    i++;
                }
            }
            return true;

        }

        /// <summary>
        /// Adds the hooks for Run.Awake (InitialCardLoading(), activating Transcription) and Run.PickNextStageScene (Guaranteed Rallypoint Delta at stage 3).
        /// </summary>
        private void AddHooks()
        {

            On.RoR2.Run.Awake += (orig, self) => { //Gets called once a run starts. NO INSTANCES EXIST YET. 
                artifactTranscriptionWasOn = false; //Since technically speaking we didn't have a stage where the artifact was on.
                goneThroughArenaOnce = false; //Make sure that we do actually have 2 void portals
                InitialCardLoading();

                orig(self);
            };

            On.RoR2.Stage.BeginServer += (orig, self) => //Called everytime you enter a new stage
            {
                Logger.LogMessage(Stage.instance.sceneDef.cachedName); //REMOVE BEFORE RELEASE
                if (Stage.instance.sceneDef.cachedName != "bazaar") //Bazaar doesn't have a stage.instance and I'm scared to figure out why
                {
                    if (RunArtifactManager.instance.IsArtifactEnabled(Transcription))
                    {
                        artifactTranscriptionWasOn = true;
                        RemoveAllLoggedMonsters();
                    }
                    else
                    {
                        if (artifactTranscriptionWasOn)
                        {
                            artifactTranscriptionWasOn = false;
                            ResetAllMonsters();
                        }
                    }
                }


                orig(self);
            };

            On.RoR2.Run.PickNextStageScene += (orig, self, choices) => //Gets called at the start of a stage. Run.instance does exist, but ClassicStageInfo.instance does not.
            {
                if (Run.instance.stageClearCount == 0) //REMOVE BEFORE RELEASE
                {
                    Run.instance.nextStageScene = Run.instance.startingScenes[0].destinations[0].destinations[0].destinations[0].destinations[0];
                    return;
                }
                if (Run.instance.stageClearCount == 1) //Guarantees Rallypoint Delta at stage 3 (we have to call it at stage 2, which would be 1 stage cleared).
                {
                    Run.instance.nextStageScene = Run.instance.startingScenes[0].destinations[0].destinations[0];
                    return;
                }
                orig(self, choices);
            };

            On.EntityStates.Huntress.ArrowRain.OnEnter += (orig, self) => //REMOVE BEFORE RELEASE
            {
                /*foreach (KeyValuePair<NetworkInstanceId, NetworkIdentity> a in NetworkServer.objects) //CommandCube(Clone)
                {
                    //Logger.LogMessage(a.Value.gameObject.name);
                    if (a.Value.gameObject.name == "PortalArena")
                    {
                        Logger.LogMessage(a.Value.gameObject.name);
                        GenericInteraction arenaPortal = a.Value.gameObject.GetComponent<GenericInteraction>();
                        Logger.LogMessage(arenaPortal.onActivation.GetPersistentMethodName(0));
                        Logger.LogMessage(arenaPortal.onActivation.GetPersistentMethodName(1));
                        arenaPortal.Networkinteractability = Interactability.Disabled;
                    }
                }*/
                ClassicStageInfo component = new ClassicStageInfo();
                if (SceneInfo.instance.GetComponent<ClassicStageInfo>() != null)
                {
                    component = SceneInfo.instance.GetComponent<ClassicStageInfo>();
                }
                else Logger.LogMessage("Nice try with that GetComponent there");
                if (ClassicStageInfo.instance != null)
                {
                    component = ClassicStageInfo.instance;
                }
                else Logger.LogMessage("Nice try with that CSI.instance there");
                if (Stage.instance != null)
                {
                    Logger.LogMessage("Stage instance does exist");
                }
                else Logger.LogMessage("Nah dude this stage just doesn't exist\n");
                

                foreach (DirectorCard iC in component.interactableCards)
                {
                    if (iC != null)
                        if (iC.spawnCard != null)
                            if (iC.spawnCard.name != null)
                                Logger.LogMessage(iC.spawnCard.name);
                }
                if (component.bonusInteractibleCreditObjects != null)
                {
                    for (int i = 0; i < component.bonusInteractibleCreditObjects.Length; i++)
                    {
                        ClassicStageInfo.BonusInteractibleCreditObject bonusInteractibleCreditObject = component.bonusInteractibleCreditObjects[i];
                        Logger.LogMessage(bonusInteractibleCreditObject.objectThatGrantsPointsIfEnabled.name);
                    }
                }
                orig(self);
            };

            On.EntityStates.Huntress.BlinkState.OnEnter += (orig, self) => //REMOVE BEFORE RELEASE
            {
                        orig(self);
            };

            On.RoR2.Run.SetEventFlag += (orig, self, name) => //Makes sure we have 2 void portals, one for the log (<10:00) and one for MUL-T (stage 7+).
            {
                if(name == "ArenaPortalTaken")
                {
                    if (!goneThroughArenaOnce)
                    {
                        goneThroughArenaOnce = true;
                        return;
                    }
                }

                orig(self, name);
            };


            /*foreach (string itemName in ItemCatalog.itemNames) //add all items
            {
                Logger.LogMessage("Adding: " + itemName);
                AddItemFromString(itemName);
            }
            foreach (EquipmentIndex EqIn in EquipmentCatalog.allEquipment) //add all equipment
            {
                EquipmentDef EqDef = EquipmentCatalog.GetEquipmentDef(EqIn);
                string eqName = EqDef.name;
                Logger.LogMessage("Adding: " + eqName);
                AddItemFromString(eqName);
            }*/




        }
        /// <summary>
        /// Adds the hooks neccesary for the Transcription Portal, so we can input the correct combination.
        /// </summary>
        private void ArtifactHooks() 
        {
            //This is way easier than forcing the event to go into PDC.actions.action, and creating the correct hash value for it.

            On.RoR2.PortalDialerController.Awake += (orig, self) => //Create the hook for the Transcription portal for this specific PortalDialerController (and remove the previous one).
            {
                transcriptionEvent.RemoveAllListeners();
                transcriptionEvent.AddListener(delegate { self.OpenArtifactPortalServer(Transcription); });
                orig(self);
            };

            On.RoR2.PortalDialerController.PortalDialerPreDialState.OnEnter += (orig, self) => //Called when we press the lablaptop.
            {
                foreach (KeyValuePair<NetworkInstanceId, NetworkIdentity> a in NetworkServer.objects) //CommandCube(Clone)
                {
                    if (a.Value.gameObject.name.StartsWith("PortalDialerButton"))
                    {
                        PortalDialerButtonController pDBC = a.Value.gameObject.GetComponent<PortalDialerButtonController>();
                        for (int i = 1; i < 10; i++)
                        {
                            if (pDBC.name.EndsWith(i.ToString()))
                            {
                                inputRecipe[i - 1] = (byte)pDBC.currentDigitDef.value; //Orders it correctly.
                            }
                        }
                    }
                }
            
                orig(self);
            };

            On.RoR2.PortalDialerController.PerformActionServer += (orig, self, sequence) => //Called upon removal of all ingredients
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Boolean RoR2.PortalDialerController::PerformActionServer(System.Byte[])' called on client");
                    return false;
                }
                for(int i = 0; i < 9; i++)
                {
                    //I know this type of if/else only works if we have 1 recipe, but we won't have any more.
                    if (inputRecipe[i] == transcriptionRecipe[i]) //If this ingredient in the inputRecipe is the same as the transcriptionRecipe
                    {
                        //Keep going
                    }
                    else 
                    {
                        return orig(self, sequence); //Just do whatever you were going to do before.
                    }
                }
                transcriptionEvent.Invoke(); //Spawn the portal B]
                return true; //Return a true so the game knows that it's a correct combination and disables interaction on the buttons and the laptop.
            };

        }

        /// <summary>
        /// Adds the hooks neccesary for the Lunar Scavenger encounter, removes a "Beads of Fealty" upon obelisk usage, makes the player recieve the "CompleteUnknownEnding" and Mastery achievement, and makes sure the player leaves the area instead of losing.
        /// </summary>
        private void LunarScavengerHooks()
        {
            On.EntityStates.Interactables.MSObelisk.TransitionToNextStage.FixedUpdate += (orig, self) => //When we use the Obelisk (and get teleported)
            {
                if (!this.removedItem)
                {
                    for (int i = 0; i < CharacterMaster.readOnlyInstancesList.Count; i++)
                    {
                        if (CharacterMaster.readOnlyInstancesList[i].inventory.GetItemCount(RoR2Content.Items.LunarTrinket) > 0)
                        {
                            this.removedItem = true; //So we don't remove an item every time FixedUpdate gets called (which is a lot.)
                            CharacterMaster.readOnlyInstancesList[i].inventory.RemoveItem(RoR2Content.Items.LunarTrinket); //remove a "Beads of Fealty"
                        }
                    }
                }
                orig(self);
            };

            On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.OnEnter += (orig, self) => //When we kill the Lunar Scavenger
            {
                UnlockAchievement("CompleteUnknownEnding");
                UnlockMasteryAchievement();
                removedItem = false;
                lunarFinished = false;
                float timeBeforeFadeOutCompletion = EntityStates.Missions.LunarScavengerEncounter.FadeOut.duration - 5f; //5 seconds before we enter idle mode (and lose)
                fadeOutTime = Run.TimeStamp.now + timeBeforeFadeOutCompletion;
                orig(self);
            };

            On.EntityStates.Missions.LunarScavengerEncounter.FadeOut.FixedUpdateServer += (orig, self) => //Gets called every frame to see if the fadeOutTime has passed
            {
                if (fadeOutTime.hasPassed && !lunarFinished)
                {
                    lunarFinished = true; //This is needed, otherwise the game has a stroke trying to force you to go to the next stage
                    Run.instance.AdvanceStage(Run.instance.nextStageScene); //Teleport player to next stage
                }
                orig(self);
            };

        }

        /// <summary>
        /// Removes all logged monsters by analyzing the strings of currently unlocked unlockables. WARNING: The game does not like having no choices, so we throw a scavenger. The game does not spawn this scavenger at earlier stages, however, due to the fact that it's too expensive (thank you Director, very cool).
        /// </summary>
        private void RemoveAllLoggedMonsters()
        {
            foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
            {
                int i = 0;
                UserProfile userProfile = user.userProfile;
                StatSheet statSheet = userProfile.statSheet;
                int unlockableCount = statSheet.GetUnlockableCount();
                while (i < unlockableCount)
                {
                    string unlockableName = statSheet.GetUnlockable(i).cachedName;  //“Logs.LemurianBody.0”
                    if (unlockableName.StartsWith("Logs.") & !unlockableName.StartsWith("Logs.Stages."))
                    {
                        unlockableName = Regex.Replace(unlockableName, @"Body\.0", ""); //Removes "Body.0"
                        unlockableName = Regex.Replace(unlockableName, @"\.0", ""); //Removes ".0" when "Body" is not named (like with Nullifier), works fine for vanilla B)
                        unlockableName = unlockableName.Substring(5);
                        unlockableName = "csc" + unlockableName;
                        if (unlockableName == "cscTitan")
                        {
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanGolemPlains");
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanBlackBeach");
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanDampCave");
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanGooLake");
                        }
                        if (unlockableName == "cscWisp")
                        {
                            unlockableName = "cscLesserWisp";
                        }
                        DirectorAPI.Helpers.RemoveExistingMonster(unlockableName);
                    }
                    i++;
                }
            }
            DirectorAPI.Helpers.AddNewMonster(scavenger, DirectorAPI.MonsterCategory.Champions); //No more choice error. Let's just hope they don't find out heheheheheheeheheheheeeeh.
            DirectorAPI.Helpers.TryApplyChangesNow();
        }

        /// <summary>
        /// The hook neccesary to make sure we can complete the escape sequence WITHOUT winning. Warning: This function DOES NOT work with orig(self).
        /// </summary>
        private void EscapeHook()
        {
            On.RoR2.EscapeSequenceController.CompleteEscapeSequence += (orig, self) => //This function does not work with orig(self).
            {
                UnlockAchievement("CompleteMainEnding");
                UnlockMasteryAchievement();
                DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.ruleBook.FindDifficulty());
                if (difficultyDef != null && difficultyDef.countsAsHardMode)
                {
                    UnlockAchievement("CompleteMainEndingHard");
                }
                else
                {
                    Logger.LogMessage("MainEndingHard not unlocked due incorrect difficulty");
                }
                Run.instance.AdvanceStage(Run.instance.nextStageScene);
            };
        }

        /// <summary>
        /// The hooks needed to add the unlocked items (via achievements) to the item pool, both via LocalUser and NetworkUser. NetworkUser gets called by the server, and LocalUser... by the local user.
        /// </summary>
        private void AchievementHooks()
        {
            On.RoR2.UserAchievementManager.GrantAchievement += (orig, self, achievementDef) =>  //This works on User unlocks, like via UnlockAchievement().
            {
                foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
                {
                    GenericStaticEnumerable<AchievementDef, AchievementManager.Enumerator> allAchievementDefs = AchievementManager.allAchievementDefs;//get all the achievementdefs
                    foreach (AchievementDef userAchievDef in allAchievementDefs)
                    {
                        if (userAchievDef == achievementDef) //did we actually get this achievement?
                        {
                            AddItemFromString(achievementDef.unlockableRewardIdentifier); //add this item to the item pool
                        }
                    }
                }
                orig(self, achievementDef);
            };

            On.RoR2.NetworkUser.ServerHandleUnlock += (orig, self, unlockableDef) => //This is NetworkUser, like what usually happens when someone unlocks an achievement.
            {
                AddItemFromString(unlockableDef.cachedName);
                orig(self, unlockableDef);
            };
        }

        /// <summary>
        /// Removes this specific monster from the game.
        /// </summary>
        /// <param name="monsterName">The name of the monster, i.e. "cscLesserWisp"</param>
        private void RemoveMonster(string monsterName)
        {
            if (monsterName == "cscWisp")
            {
                monsterName = "cscLesserWisp";
            }
            ClassicStageInfo csi = ClassicStageInfo.instance;
            foreach (WeightedSelection<DirectorCard>.ChoiceInfo CI in csi.monsterSelection.choices)
            {
                if (CI.value != null)
                {
                    if (CI.value.spawnCard != null)
                    {

                        string spawnCardName = CI.value.spawnCard.name;
                        if (monsterName == "cscTitan")
                        {
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanGolemPlains");
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanBlackBeach");
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanDampCave");
                            DirectorAPI.Helpers.RemoveExistingMonster("cscTitanGooLake");
                            DirectorAPI.Helpers.TryApplyChangesNow();
                            break;
                        }
                        if (spawnCardName.ToLower() == monsterName.ToLower())
                        {
                            DirectorAPI.Helpers.RemoveExistingMonster(spawnCardName);
                            DirectorAPI.Helpers.TryApplyChangesNow();
                            break;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Load this resource into a 64x64 sprite. Only used for the sprites of Transcription.
        /// </summary>
        /// <param name="resourceName">The byte array that we need to load.</param>
        /// <returns></returns>
        private Sprite LoadSprite(byte[] resourceName)
        {
            Texture2D texture2D = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            texture2D.LoadImage(resourceName, false);
            return Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f));
        }
        
        /// <summary>
        /// Add an item to the item pool by using a string (get the unlockableRewardIdentifier from AchievementDef by doing achievementDef.unlockableRewardIdentifier).
        /// </summary>
        /// <param name="unlockableRewardIdentifier">The unlockableRewardIndentifier, e.g. "Item.Bear"</param>
        public void AddItemFromString(string unlockableRewardIdentifier)
        {
            string pattern = @"\w+\."; //this just means "[infinite letters until]."
            bool equipment = false;
            unlockableRewardIdentifier = Regex.Replace(unlockableRewardIdentifier, pattern, ""); //remove "[infinite letters until]." so we have the itemname remaining

            foreach (EquipmentIndex i in EquipmentCatalog.equipmentList)
            {
                EquipmentDef EqDef = EquipmentCatalog.GetEquipmentDef(i);
                string equipmentString = EqDef.name;
                if (unlockableRewardIdentifier == equipmentString)
                {
                    Run.instance.availableEquipment.Add(EquipmentCatalog.FindEquipmentIndex(unlockableRewardIdentifier));
                    equipment = true;
                    break; //So we don't search everything if we already have it
                }
            }
            if (!equipment) //it doesn't matter if we try to find itemindex for characters or logs, due to the fact that they won't have the same name as an available item, and will not result in an ItemIndex that we can use
            {
                Run.instance.availableItems.Add(ItemCatalog.FindItemIndex(unlockableRewardIdentifier)); //Add the item from this string into the available items
            }
            Run.instance.BuildDropTable(); //Makes it so that everything we added actually gets put into the game pool so we can get it on the next items, you can see it that old items do not have it with command, but hopefully that won't matter :]
            UpdateDroppedCommandDroplets();
        }

        /// <summary>
        /// Update the currently dropped command droplets with the new itempool.
        /// </summary>
        public void UpdateDroppedCommandDroplets()
        {
            foreach (KeyValuePair<NetworkInstanceId, NetworkIdentity> a in NetworkServer.objects) //CommandCube(Clone)
            {
                if (a.Value.gameObject.name.StartsWith("CommandCube"))
                {
                    a.Value.gameObject.GetComponent<PickupPickerController>().SetOptionsFromPickupForCommandArtifact(a.Value.gameObject.GetComponent<PickupIndexNetworker>().NetworkpickupIndex);
                }
            }
        }

        /// <summary>
        /// Unlock a specific achievement by using it's stringname. This counts as a "RoR2.UserAchievementManager.GrantAchievement", not "RoR2.NetworkUser.ServerHandleUnlock" (what the game uses normally).
        /// </summary>
        /// <param name="achievement">The name of the achievement, e.g. "CommandoNonLunarEndurance" (check RoR2.Achievements' [RegisterAchievement()], there it's the first variable)</param>
        public void UnlockAchievement(string achievement) //Unlock a specific achievement
        {
            AchievementDef achievementDef = AchievementManager.GetAchievementDef(achievement);
            if (achievementDef != null)
            {
                foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
                {
                    AchievementManager.GetUserAchievementManager(user).GrantAchievement(achievementDef);
                }
            }
        }


        /// <summary>
        /// Unlock the mastery achievement and survivor log for the currently played character, has a difficultycheck inside.
        /// </summary>
        public void UnlockMasteryAchievement()
        {

            string defaultString = "ClearGameMonsoon";  //default template for achievement is "[Character Name]ClearGameMonsoon", let's hope this isn't a problem with other characters :|
            string userCharacterName = "|DEFAULT NAME| ( this is a bug :'[ )";
            DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.ruleBook.FindDifficulty());
            foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
            {
                UserProfile userProfile = user.userProfile;
                userCharacterName = (SurvivorCatalog.GetSurvivorDef(SurvivorCatalog.GetSurvivorIndexFromBodyIndex(BodyCatalog.FindBodyIndex(user.cachedBody)))).cachedName;
                ulong totalWins = userProfile.statSheet.GetStatValueULong(PerBodyStatDef.totalWins, userCharacterName);
                totalWins += 1;
                userProfile.statSheet.PushStatValue(PerBodyStatDef.totalWins, BodyCatalog.FindBodyIndex(user.cachedBody), totalWins);
            }
            if (difficultyDef != null && difficultyDef.countsAsHardMode)
            {
                foreach (LocalUser user in LocalUserManager.readOnlyLocalUsersList)
                {
                    CharacterBody userBody = user.cachedBody;
                    BodyIndex userBodyIndex = BodyCatalog.FindBodyIndex(userBody);
                    SurvivorIndex userSurvivorIndex = SurvivorCatalog.GetSurvivorIndexFromBodyIndex(userBodyIndex);
                    SurvivorDef userSurvivorDef = SurvivorCatalog.GetSurvivorDef(userSurvivorIndex);
                    userCharacterName = userSurvivorDef.cachedName;

                    string achievementString = userCharacterName + defaultString;
                    AchievementDef achievementDef = AchievementManager.GetAchievementDef(achievementString);
                    if (achievementDef != null)
                    {
                        AchievementManager.GetUserAchievementManager(user).GrantAchievement(achievementDef);
                    }
                    Logger.LogMessage("Unlocking Mastery Achievement for LocalUser, who is playing as " + userCharacterName);
                }
            }
            else
            {
                Logger.LogMessage("Mastery Achievement not unlocked due to invalid difficulty.");
            }
        }
    }
}