using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using MoreSlugcats;
using System.Runtime.CompilerServices;
using SlugBase.SaveData;
using System.Collections.Generic;

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
            On.Player.Update += damageGrabber;
            On.Player.Update += explodeJumpImperishable;
            On.PlayerGraphics.Update += forgeGlowMake;
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            On.SaveState.IncreaseKarmaCapOneStep += increaseKarmaSingleStep;
            On.SlugcatStats.SpearSpawnExplosiveRandomChance += makeSpearsBoom;
            On.SlugcatStats.SpearSpawnModifier += dBreakerExtraSpears;
            On.Spear.HitSomething += spearResist;
            On.Player.SpearStick += noStickSpears;
            On.Player.GrabUpdate += makeFireSpears;
            Debug.Log("Hooks Set");
            ModifyRelations.Apply();
        }
        
        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {

        }

        public bool spearResist(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            if(result.obj is Player && (result.obj as Player).slugcatStats.name == ChandlerName && (result.obj as Player).KarmaCap >= 4 && (result.obj as Player).KarmaCap <= 7 && UnityEngine.Random.value <= 0.15)
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
                    self.lightSource.setRad = new float?(500f);
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
            if (self is Player player && type == Creature.DamageType.Explosion && ((player.slugcatStats.name == ChandlerName && player.KarmaCap >= 4) || (player.slugcatStats.name == ImperishableName && check >= 3)))
            {
                return;
            }
            orig(self, source, directionAndMomentum, hitChunk, hitAppendage, type, damage, stunBonus);

        }

        void immunities(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if ((self.slugcatStats.name == ChandlerName && self.KarmaCap >= 7) || (self.slugcatStats.name == ImperishableName && check >= 4))
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
                return crit.dead;
            }
            else
            {
                return orig(self, crit);
            }
        }

        void makeFireSpears(On.Player.orig_GrabUpdate orig, Player self, bool eu)
        {
            orig(self, eu);

            for (int i = 0; i < 2; i++)
            {
                if (self.grasps[i] != null)
                {
                    if (self.grasps[i].grabbed is Spear spear)
                    {
                        if (!spear.bugSpear && !spear.abstractSpear.electric && !spear.abstractSpear.explosive)
                        {
                            if (spearCraftCountUp <= 200)
                            {
                                imperishableSaveData.TryGet<int>("minKarma", out int check);
                                if (self.slugcatStats.name == ChandlerName && self.KarmaCap >= 6)
                                {
                                    spearCraftCountUp++;
                                }
                                else if (self.slugcatStats.name == ImperishableName && check >= 5)
                                {
                                    spearCraftCountUp += 200;
                                }
                            }
                            else
                            {
                                spearCraftCountUp = 0;
                                self.ReleaseGrasp(i);
                                spear.abstractPhysicalObject.realizedObject.RemoveFromRoom();
                                AbstractSpear spear1 = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), false, 0.5f);
                                self.room.abstractRoom.AddEntity(spear1);
                                spear1.RealizeInRoom();
                                if (self.FreeHand() != -1)
                                {
                                    self.SlugcatGrab(spear1.realizedObject, self.FreeHand());
                                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, self.firstChunk.pos, 1f, UnityEngine.Random.Range(0.8f, 1.2f));
                                }
                            }
                        }
                    }
               
                }
            }
        }

        void explodeJumpImperishable(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (self.slugcatStats.name == ImperishableName && check == 9 && (self.Consious || self.dead))
            {
                if(self.wantToJump > 0 && self.input[0].pckp && !self.pyroJumpped && self.canJump <= 0 && self.eatMeat < 20 && (self.input[0].y >= 0 || (self.input[0].y < 0 && (self.bodyMode == Player.BodyModeIndex.ZeroG || self.gravity <= 0.1f))) && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.CorridorClimb
                    && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.bodyMode != Player.BodyModeIndex.Swimming && self.bodyMode != Player.BodyModeIndex.WallClimb  && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab && self.animation != Player.AnimationIndex.ZeroGPoleGrab
                    && self.onBack == null)
                {
                    self.pyroJumpped = true;
                    self.noGrabCounter = 5;
                    Vector2 Pos = self.firstChunk.pos;
                    for(int i = 0; i < 8; i++)
                    {
                        self.room.AddObject(new Explosion.ExplosionSmoke(Pos, RWCustom.Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
                    }
                    self.room.AddObject(new Explosion.ExplosionLight(Pos, 160f, 1f, 3, new Color(1f, 0.6392f, 0.1529f)));
                    for(int j = 0; j < 10; j++)
                    {
                        Vector2 a = RWCustom.Custom.RNV();
                        self.room.AddObject(new Spark(Pos + a * UnityEngine.Random.value * 40f, a * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.black, null, 4, 18));
                    }
                    self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, Pos, 0.3f + UnityEngine.Random.value * 0.3f, 0.5f + UnityEngine.Random.value * 2f);
                    self.room.InGameNoise(new Noise.InGameNoise(Pos, 8000f, self, 1f));
                    if(self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity == 0f || self.gravity == 0f)
                    {
                        float num3 = (float)self.input[0].x;
                        float num4 = (float)self.input[0].y;
                        while(num4 == 0f && num3 == 0f)
                        {
                            num3 = ((UnityEngine.Random.value <= 0.33) ? 0 : ((UnityEngine.Random.value <= 0.5) ? 1 : -1));
                            num4 = ((UnityEngine.Random.value <= 0.33) ? 0 : ((UnityEngine.Random.value <= 0.5) ? 1 : -1));
                        }
                        self.bodyChunks[0].vel.x = 9f * num3;
                        self.bodyChunks[0].vel.y = 9f * num4;
                        self.bodyChunks[1].vel.x = 8f * num3;
                        self.bodyChunks[1].vel.y = 8f * num4;

                    }
                    else
                    {
                        if (self.input[0].x != 0)
                        {
                            self.bodyChunks[0].vel.y = Mathf.Min(self.bodyChunks[0].vel.y, 0f) + 8f;
                            self.bodyChunks[1].vel.y = Mathf.Min(self.bodyChunks[1].vel.y, 0f) + 7f;
                            self.jumpBoost = 6f;
                        }
                        if (self.input[0].x == 0 || self.input[0].y == 1)
                        {
                            self.bodyChunks[0].vel.y = 16f;
                            self.bodyChunks[1].vel.y = 15f;
                            self.jumpBoost = 10f;
                        }
                        if (self.input[0].y == 1)
                        {
                            self.bodyChunks[0].vel.x = 10f * (float)self.input[0].x;
                            self.bodyChunks[1].vel.x = 8f * (float)self.input[0].x;
                        }
                        else
                        {
                            self.bodyChunks[0].vel.x = 15f * (float)self.input[0].x;
                            self.bodyChunks[1].vel.x = 13f * (float)self.input[0].x;
                        }
                        self.animation = Player.AnimationIndex.Flip;
                        self.bodyMode = Player.BodyModeIndex.Default;


                    }
                }
            }
            else if(self.wantToJump > 0 && self.input[0].pckp && self.eatMeat < 20 && (self.input[0].y < 0 || self.bodyMode == Player.BodyModeIndex.Crawl) && (self.canJump > 0 || self.input[0].y < 0) && self.Consious && !self.pyroJumpped)
            {
                if (self.canJump <= 0)
                {
                    self.pyroJumpped = true;
                    self.bodyChunks[0].vel.y = 8f;
                    self.bodyChunks[1].vel.y = 6f;
                    self.jumpBoost = 6f;
                    self.forceSleepCounter = 0;
                }
                Vector2 pos2 = self.firstChunk.pos;
                for (int k = 0; k < 8; k++)
                {
                    self.room.AddObject(new Explosion.ExplosionSmoke(pos2, RWCustom.Custom.RNV() * 5f * UnityEngine.Random.value, 1f));
                }
                self.room.AddObject(new Explosion.ExplosionLight(pos2, 160f, 1f, 3, new Color(1f, 0.6392f, 0.1529f)));
                for (int l = 0; l < 10; l++)
                {
                    Vector2 a2 = RWCustom.Custom.RNV();
                    self.room.AddObject(new Spark(pos2 + a2 * UnityEngine.Random.value * 40f, a2 * Mathf.Lerp(4f, 30f, UnityEngine.Random.value), Color.black, null, 4, 18));
                }
                self.room.AddObject(new ShockWave(pos2, 200f, 0.2f, 6, false));
                self.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, pos2, 0.3f + UnityEngine.Random.value * 0.3f, 0.5f + UnityEngine.Random.value * 2f);
                self.room.InGameNoise(new Noise.InGameNoise(pos2, 8000f, self, 1f));
                List<Weapon> list = new List<Weapon>();
                for (int m = 0; m < self.room.physicalObjects.Length; m++)
                {
                    for (int n = 0; n < self.room.physicalObjects[m].Count; n++)
                    {
                        if (self.room.physicalObjects[m][n] is Weapon)
                        {
                            Weapon weapon = self.room.physicalObjects[m][n] as Weapon;
                            if (weapon.mode == Weapon.Mode.Thrown && RWCustom.Custom.Dist(pos2, weapon.firstChunk.pos) < 300f)
                            {
                                list.Add(weapon);
                            }
                        }
                        bool flag3;
                        if (ModManager.CoopAvailable && !RWCustom.Custom.rainWorld.options.friendlyFire)
                        {
                            Player player = self.room.physicalObjects[m][n] as Player;
                            flag3 = (player == null || player.isNPC);
                        }
                        else
                        {
                            flag3 = true;
                        }
                        bool flag4 = flag3;
                        if (self.room.physicalObjects[m][n] is Creature && self.room.physicalObjects[m][n] != self && flag4)
                        {
                            Creature creature = self.room.physicalObjects[m][n] as Creature;
                            if (RWCustom.Custom.Dist(pos2, creature.firstChunk.pos) < 200f && (RWCustom.Custom.Dist(pos2, creature.firstChunk.pos) < 60f || self.room.VisualContact(self.abstractCreature.pos, creature.abstractCreature.pos)))
                            {
                                self.room.socialEventRecognizer.WeaponAttack(null, self, creature, true);
                                creature.SetKillTag(self.abstractCreature);
                                
                                    creature.Stun(80);
                            }
                            creature.firstChunk.vel = RWCustom.Custom.DegToVec(RWCustom.Custom.AimFromOneVectorToAnother(pos2, creature.firstChunk.pos)) * 30f;
                            if (creature is TentaclePlant)
                            {
                                for (int num5 = 0; num5 < creature.grasps.Length; num5++)
                                {
                                        creature.ReleaseGrasp(num5);
                                }
                            }
                        }
                    }
                }
                if (list.Count > 0 && self.room.game.IsArenaSession)
                {
                    self.room.game.GetArenaGameSession.arenaSitting.players[0].parries++;
                }
                for (int num6 = 0; num6 < list.Count; num6++)
                {
                    list[num6].ChangeMode(Weapon.Mode.Free);
                    list[num6].firstChunk.vel = RWCustom.Custom.DegToVec(RWCustom.Custom.AimFromOneVectorToAnother(pos2, list[num6].firstChunk.pos)) * 20f;
                    list[num6].SetRandomSpin();
                }
            }
            if (self.canJump > 0 || !self.Consious || self.Stunned || self.animation == Player.AnimationIndex.HangFromBeam || self.animation == Player.AnimationIndex.ClimbOnBeam || self.bodyMode == Player.BodyModeIndex.WallClimb || self.animation == Player.AnimationIndex.AntlerClimb || self.animation == Player.AnimationIndex.VineGrab ||
                self.animation == Player.AnimationIndex.ZeroGPoleGrab || self.bodyMode == Player.BodyModeIndex.Swimming || ((self.bodyMode == Player.BodyModeIndex.ZeroG || self.room.gravity <= 0.5f || self.gravity <= 0.5f) && (self.wantToJump == 0 || !self.input[0].pckp)))
            {
                self.pyroJumpped = false;
            }
        }
        
        void damageGrabber(On.Player.orig_Update orig, Player self, bool eu)
        {
            imperishableSaveData.TryGet<int>("minKarma", out int check);
            if (self.grabbedBy.Count > 0 && self.slugcatStats.name == ImperishableName && check >= 7)
            {
                for(int i = self.grabbedBy.Count - 1; i >= 0; i--)
                {
                    self.grabbedBy[i].grabber.Violence(self.firstChunk, null, null, null, MoreSlugcatsEnums.DamageType.None, 0.3f, 1f);
                    self.grabbedBy[i].grabber.Stun(80);
                }
            }
            orig(self, eu);
        }

    }

    public class ModifyRelations
    {
        public static CreatureTemplate.Relationship.Type newRelation = CreatureTemplate.Relationship.Type.Afraid;
        public static float intensity = 1f;
        public static bool Condition(Creature? crit)
        {
            if (crit != null && crit.Template.type == CreatureTemplate.Type.Slugcat && crit is Player player && player.slugcatStats.name == Plugin.ImperishableName && player.KarmaCap >= 7)

                return true;

            else

                return false;
        }
        public static void Apply()
        {
            On.BigNeedleWormAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.BigSpiderAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.CentipedeAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.DropBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.LizardAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.MirosBirdAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.ScavengerAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.VultureAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };

            On.MoreSlugcats.StowawayBugAI.IUseARelationshipTracker_UpdateDynamicRelationship += (orig, self, dRelation) => {
                Creature? trackedCreature = dRelation?.trackerRep?.representedCreature?.realizedCreature;
                if (Condition(trackedCreature))
                {
                    return new CreatureTemplate.Relationship(newRelation, intensity);
                }
                return orig(self, dRelation);
            };
        }
    }
}