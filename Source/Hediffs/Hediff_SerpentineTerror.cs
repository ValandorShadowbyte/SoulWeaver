using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using static UnityEngine.Networking.UnityWebRequest;

namespace SoulSerpent
{
    // modeled off of VanillaPsycastsExpanded.Hediff_PsychicDrone
    //[StaticConstructorOnStartup] //don't think we need this
    public class Hediff_SerpentineTerror : Hediff_Overlay
    {
        private List<Mote> maintainedMotes = new List<Mote>();
        private List<Pawn> affectedPawns = new List<Pawn>();

        public override string OverlayPath => "Effects/Snakesage/SerpentineTerror/SerpentineTerrorAOE";

        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            maintainedMotes.Add(SpawnMoteAttached(VPE_DefOf.VPE_PsycastAreaEffectMaintained, ability.GetRadiusForPawn(), 0.0f));
        }

        public override void Tick()
        {
            base.Tick();

            foreach (Mote mote in maintainedMotes)
            {
                mote.Maintain();
            }

            Pawn target = null;

            if (Find.TickManager.TicksGame % 180 != 0)
            {
                GenRadial.RadialDistinctThingsAround(pawn.Position, pawn.Map, ability.GetRadiusForPawn(), true)
                    .OfType<Pawn>()
                    .Where(x => !affectedPawns.Contains(x) && !x.InMentalState && x.HostileTo(pawn) && x.RaceProps.IsFlesh)
                    .TryRandomElement(out target);
            }

            if (target != null)
            {
                var stateDef = MentalStateDefOf.Berserk;

                if (ModsConfig.OdysseyActive)
                {
                    stateDef = Rand.Bool ? stateDef : MentalStateDefOf.Terror;
                }
                
                if (ModsConfig.AnomalyActive)
                {
                    stateDef = Rand.Bool ? stateDef : MentalStateDefOf.PanicFlee;
                }

                if (target.mindState.mentalStateHandler.TryStartMentalState(stateDef, causedByPsycast: true))
                {
                    affectedPawns.Add(target);
                }
            }
        }

        public override void Draw()
        {
            var drawPos = pawn.DrawPos;
            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();

            var matrix = new Matrix4x4();
            var num = ability.GetRadiusForPawn() * 2f;
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(0, Vector3.up), new Vector3(num, 1f, num));

            Graphics.DrawMesh(MeshPool.plane10, matrix, OverlayMat, 0, (Camera) null, 0, MatPropertyBlock);
        }

        public Mote SpawnMoteAttached(ThingDef moteDef, float scale, float rotationRate)
        {
            var moteAttachedScaled = MoteMaker.MakeAttachedOverlay(pawn, moteDef, Vector3.zero) as MoteAttachedScaled;
            moteAttachedScaled.maxScale = scale;
            moteAttachedScaled.rotationRate = rotationRate;
            
            if (moteAttachedScaled.def.mote.needsMaintenance)
            {
                moteAttachedScaled.Maintain();
            }

            return (Mote) moteAttachedScaled;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn>(ref affectedPawns, "affectedPawns", LookMode.Reference);
            Scribe_Collections.Look<Mote>(ref maintainedMotes, "maintainedMotes", LookMode.Reference);
        }
    }
}
