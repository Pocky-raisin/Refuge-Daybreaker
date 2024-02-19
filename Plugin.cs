using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using MoreSlugcats;
using System.Runtime.CompilerServices;
using SlugBase.SaveData;

namespace SlugTemplate
{
    [BepInPlugin(MOD_ID, "pogger.dayBreaker", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        private const string MOD_ID = "pogger.dayBreaker";
        public static readonly SlugcatStats.Name ImperishableName = new SlugcatStats.Name("DB.Wildfire", false);
        public static readonly SlugcatStats.Name ChandlerName = new SlugcatStats.Name("DB.Candle", false);
        public SlugBaseSaveData chandlerSaveData = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(new DeathPersistentSaveData(ChandlerName));
        public SlugBaseSaveData imperishableSaveData = SlugBase.SaveData.SaveDataExtension.GetSlugBaseData(new DeathPersistentSaveData(ImperishableName));
        private int spearCraftCountUp = 0;

        // Add hooks
        public void OnEnable()
        {
            On.Creature.HypothermiaUpdate += hypothermiaModify;
            On.Creature.Violence += damageImmune;
            On.Creature.Violence += deflectSpears;        
            On.DeathPersistentSaveData.ctor += dBreakerDoInitialSaveStuff;
            On.Lizard.Bite += alwaysSurviveLizards;
            On.Player.CanEatMeat += tempEdible;
            On.Player.ctor += foodStuff;
            On.Player.ctor += immunities;
            On.Player.Die += dBreakerMinKarmaOnDeath;
            On.Player.Update += dBreakerMinKarma;
            On.PlayerGraphics.Update += forgeGlowMake;
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.SaveState.IncreaseKarmaCapOneStep += increaseKarmaSingleStep;
            On.SlugcatStats.SpearSpawnExplosiveRandomChance += makeSpearsBoom;
            On.SlugcatStats.SpearSpawnModifier += dBreakerExtraSpears;
            On.Spear.HitSomething += spearResist;
            On.Player.SpearStick += noStickSpears;
            On.Player.GrabUpdate += makeFireSpears;
            Debug.Log("Hooks Set");
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {

        }

        public bool spearResist(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if(result.obj is Player && (result.obj as Player).slugcatStats.name == ChandlerName /*&& (result.obj as Player).KarmaCap >= 4 && (result.obj as Player).KarmaCap <= 7*/)
            {
                self.spearDamageBonus *= 0.01f;
            }
            return orig(self, result, eu);
        }

        public void deflectSpears(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            if(self is Player player)
            {
                imperishableSaveData.TryGet<int>("minKarma", out int check);
                if ((player.slugcatStats.name == ChandlerName && player.KarmaCap >= 8) || (player.slugcatStats.name == ImperishableName && check >= 8))
                {
                    if(type == Creature.DamageType.Stab || type == Creature.DamageType.Bite)
                    {
                        if(self.graphicsModule != null && source != null && self.room != null)
                        {
                            self.room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, self.mainBodyChunk);
                            self.room.AddObject(new StationaryEffect(source.pos, new Color(1f, 1f, 1f), self.graphicsModule as LizardGraphics, StationaryEffect.EffectType.FlashingOrb));

                        }
                        return;
                    }
                    
                }
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);
        }

        public bool noStickSpears(On.Player.orig_SpearStick orig, Player self, Weapon source, float dmg, BodyChunk chunk, PhysicalObject.Appendage.Pos hitAppendage, Vector2 direction)
        {
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if ((self.slugcatStats.name == ChandlerName && self.KarmaCap >= 8) || (self.slugcatStats.name == ImperishableName && check >= 8))
            {
                return false;
            }
            else
            {
                return orig(self, source, dmg, chunk, hitAppendage, direction);
            }
        }

        public void dBreakerDoInitialSaveStuff(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcatName)
        {
            imperishableSaveData.TryGet<bool>("alreadyDidSaveSetup", out bool val1);
            chandlerSaveData.TryGet<bool>("alreadyDidSaveSetup", out bool val2);
            if (!val1)
            {
                imperishableSaveData.Set<int>("minKarma", 9);
                imperishableSaveData.Set<int>("maxKarma", 9);
                imperishableSaveData.Set<bool>("didK9Miracle", false);
                imperishableSaveData.Set<bool>("didK8Miracle", false);
                imperishableSaveData.Set<bool>("didK7Miracle", false);
                imperishableSaveData.Set<bool>("didK6Miracle", false);
                imperishableSaveData.Set<bool>("didK5Miracle", false);
                imperishableSaveData.Set<bool>("didK4Miracle", false);
                imperishableSaveData.Set<bool>("didK3Miracle", false);
                imperishableSaveData.Set<bool>("didK2Miracle", false);
                imperishableSaveData.Set<bool>("didK1Miracle", false);
                imperishableSaveData.Set<bool>("alreadyDidSaveSetup", true);
            }
            if (!val2)
            {
                chandlerSaveData.Set<bool>("didK4Miracle", false);
                chandlerSaveData.Set<bool>("didK5Miracle", false);
                chandlerSaveData.Set<bool>("didK6Miracle", false);
                chandlerSaveData.Set<bool>("didK7Miracle", false);
                chandlerSaveData.Set<bool>("didK8Miracle", false);
                chandlerSaveData.Set<bool>("didK9Miracle", false);
                chandlerSaveData.Set<bool>("alreadyDidSaveSetup", true);
            }
            orig(self, slugcatName);
        }

        public void dBreakerMinKarma(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            imperishableSaveData.TryGet<int>("minKarma", out int minKarma);
            imperishableSaveData.TryGet<int>("maxKarma", out int maxKarma);
            if (self.slugcatStats.name == ImperishableName)
            {
                
                if (minKarma <= 6)
                {
                    maxKarma = minKarma + 2;
                }
                else
                {
                    maxKarma = 9;
                }
                imperishableSaveData.Set<int>("maxKarma", maxKarma);
                if (self.KarmaCap > maxKarma)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = maxKarma;
                }
                if(self.Karma > self.KarmaCap)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap = self.KarmaCap;
                }
                if (self.Karma < minKarma)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma = minKarma;
                }
                
            }
        }

        public void increaseKarmaSingleStep(On.SaveState.orig_IncreaseKarmaCapOneStep orig, SaveState self)
        {
            if(self.saveStateNumber == ChandlerName)
            {
                if(self.deathPersistentSaveData.karmaCap < 8)
                {
                    self.deathPersistentSaveData.karmaCap++;
                }
            }
            else
            {
                orig(self);
            }
        }

        public void decreaseMinKarmaOneStep(Player player)
        {
            if(player.slugcatStats.name == ImperishableName)
            {
                imperishableSaveData.TryGet<int>("minKarma", out int minKarma);
                minKarma--;
                imperishableSaveData.Set<int>("minKarma", minKarma);
            }
        }

        public void dBreakerMinKarmaOnDeath(On.Player.orig_Die orig, Player self)
        {
            if(self.slugcatStats.name == ImperishableName)
            {
                imperishableSaveData.TryGet<int>("minKarma", out int minKarma);
                if(self.Karma == minKarma)
                {
                    (self.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = true;
                }
            }
            orig(self);
        }

        float dBreakerExtraSpears(On.SlugcatStats.orig_SpearSpawnModifier orig, SlugcatStats.Name index, float originalChance)
        {
            if (index == Plugin.ChandlerName)
            {
                return Mathf.Pow(originalChance, 0.75f);
            }
            if (index == Plugin.ImperishableName)
            {
                return Mathf.Pow(originalChance, 0.7f);
            }

            return orig(index, originalChance);
        }
        
        float makeSpearsBoom(On.SlugcatStats.orig_SpearSpawnExplosiveRandomChance orig, SlugcatStats.Name index)
        {
            if (index == ChandlerName)
            {
                return 0.01f;
            }
            if (index == ImperishableName)
            {
                return 0.015f;
            }
            return orig(index);
        }

        void forgeGlowMake(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (self.player.slugcatStats.name == ChandlerName && self.player.KarmaCap >= 3)
            {
                if(self.lightSource != null)
                {
                    self.lightSource.stayAlive = true;
                    self.lightSource.setPos = new Vector2?(self.player.mainBodyChunk.pos);
                }
                if (self.lightSource == null)
                {
                    self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, new Color(1f, 0.56078431372f, 0.2431372549f), self.player);
                    self.lightSource.requireUpKeep = true;
                    self.lightSource.setRad = new float?(300f);
                    self.lightSource.setAlpha = new float?(1f);
                    self.player.room.AddObject(self.lightSource);
                }
            }
            else if(self.player.slugcatStats.name == ImperishableName && check >= 2)
            {
                if (self.lightSource != null)
                {
                    self.lightSource.stayAlive = true;
                    self.lightSource.setPos = new Vector2?(self.player.mainBodyChunk.pos);
                }
                if (self.lightSource == null)
                {
                    self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, new Color(1f, 0.76078431372f, 0.4431372549f), self.player);
                    self.lightSource.requireUpKeep = true;
                    self.lightSource.setRad = new float?(300f);
                    self.lightSource.setAlpha = new float?(1f);
                    self.player.room.AddObject(self.lightSource);
                }
            }
        }

        void hypothermiaModify(On.Creature.orig_HypothermiaUpdate orig, Creature creature)
        {
            orig(creature);
            if(creature.abstractCreature.world.game.IsStorySession && creature.abstractCreature.world.game.StoryCharacter == ChandlerName)
            {
                creature.HypothermiaGain *= 1.1f;
            } else if (creature.abstractCreature.world.game.IsStorySession && creature.abstractCreature.world.game.StoryCharacter == ImperishableName)
            {
                creature.HypothermiaGain *= 1.2f;
                
            }
            if (creature is Player player && (player.slugcatStats.name == ChandlerName && player.KarmaCap >= 3 && player.KarmaCap <= 6))
            {
                creature.HypothermiaGain *= 0.75f;
            }
            if (creature.abstractCreature.world.game.IsStorySession && (creature.abstractCreature.world.game.StoryCharacter == ChandlerName || creature.abstractCreature.world.game.StoryCharacter == ImperishableName))
            {
                creature.Hypothermia += creature.HypothermiaGain * (1f - creature.HypothermiaGain);
            }
        }

        void damageImmune(On.Creature.orig_Violence orig, Creature self, BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType type, float damage, float stunBonus)
        {
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (self is Player player && type == Creature.DamageType.Explosion && ((player.slugcatStats.name == ChandlerName && player.KarmaCap >= 4) || (player.slugcatStats.name == ImperishableName && check >= 4)))
            {
                return;
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

        }

        void immunities(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if ((self.slugcatStats.name == ChandlerName && self.KarmaCap >= 7) || (self.slugcatStats.name == ImperishableName && check >= 5))
            {
                self.abstractCreature.HypothermiaImmune = true;
            }
            if(self.slugcatStats.name == ChandlerName || (self.slugcatStats.name == ImperishableName && check >= 1))
            {
                self.abstractCreature.lavaImmune = true;
            }
        }

        void alwaysSurviveLizards(On.Lizard.orig_Bite orig, Lizard self, BodyChunk chunk)
        {
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (chunk.owner is Player player && (player.slugcatStats.name == ChandlerName && player.KarmaCap >= 8 || (player.slugcatStats.name == ImperishableName && check >= 8)))
            {
                self.lizardParams.biteDamageChance= 0;
                self.lizardParams.biteDamage = 0;
            }
            orig(self, chunk);
        }
        
        void foodStuff(On.Player.orig_ctor orig, Player self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (self.slugcatStats.name == ChandlerName && self.KarmaCap >= 5)
            {
                self.slugcatStats.foodToHibernate = 5;
            }
            else if(self.slugcatStats.name == ImperishableName && check <= 4)
            {
                self.slugcatStats.maxFood = 6;
            }
        }

        bool tempEdible(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
        {
            if(self.slugcatStats.name == ChandlerName && self.KarmaCap >= 5)
            {
                return (crit is IPlayerEdible) && crit.dead;
            }
            else
            {
                return orig(self, crit);
            }
        }

        void makeFireSpears(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            Debug.Log("AAA");
            orig(self, eu);
            Debug.Log("BBB");

            for (int i = 0; i < 2; i++)
            {
                Debug.Log("LLL");
                if (self.grasps[i] != null)
                {
                    Debug.Log("NNN");
                    if (self.grasps[i].grabbed is Spear spear)
                    {
                        Debug.Log("MMM");
                        if (!spear.bugSpear && !spear.abstractSpear.electric && !spear.abstractSpear.explosive)
                        {
                            Debug.Log("CCC");
                            if (spearCraftCountUp <= 200)
                            {
                                Debug.Log("DDD");
                                imperishableSaveData.TryGet<int>("minKarma", out int check);
                                if (self.slugcatStats.name == ChandlerName && self.KarmaCap >= 6)
                                {
                                    Debug.Log("EEE");
                                    spearCraftCountUp++;
                                }
                                else if (self.slugcatStats.name == ImperishableName && check >= 6)
                                {
                                    Debug.Log("FFF");
                                    spearCraftCountUp += 200;
                                }
                            }
                            else
                            {
                                Debug.Log("GGG");
                                spearCraftCountUp = 0;
                                self.ReleaseGrasp(i);
                                spear.abstractPhysicalObject.realizedObject.RemoveFromRoom();
                                AbstractSpear spear1 = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, 0.5f);
                                self.room.abstractRoom.AddEntity(spear1);
                                spear1.RealizeInRoom();
                                if (self.FreeHand() != -1)
                                {
                                    Debug.Log("HHH");
                                    self.SlugcatGrab(spear1.realizedObject, self.FreeHand());
                                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.firstChunk.pos, 1f, UnityEngine.Random.Range(0.8f, 1.2f));
                                }
                            }
                            Debug.Log("III");
                        }
                    }
               
                }
                Debug.Log("JJJ");
            }
            Debug.Log("ForLoopEnded");
        }

        void causeFear()
        {

        }

        void doubleJump()
        {

        }

        void damageGrabber()
        {

        }

    }
}
