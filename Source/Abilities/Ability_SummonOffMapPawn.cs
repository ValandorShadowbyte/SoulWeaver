using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Ability = VEF.Abilities.Ability;

namespace SoulSerpent
{
    public class Ability_SummonOffMapPawn : Ability
    {
        public override bool CanAutoCast => false;

        private List<Pawn> availablePawns = new List<Pawn>();

        public override bool IsEnabledForPawn(out string reason)
        {
            // Only work with marked pawns that are off-map
            availablePawns.Clear();
            
            // Get marked pawns from the soulweaver hediff
            var soulWeaver = SoulSerpentUtils.TryGetHediff<Hediff_Soulweaver>(pawn, SoulSerpentDefs.VS_Soulweaver);
            if (soulWeaver != null && soulWeaver.MarkedPawns != null)
            {
                // Add marked pawns that are off-map
                availablePawns.AddRange(soulWeaver.MarkedPawns.Where(p =>
                    p != null &&
                    !p.Dead &&
                    !p.Destroyed &&
                    p.MapHeld != this.pawn.Map &&
                    p.Faction != Faction.OfMechanoids && // Don't allow mechanoids
                    p.RaceProps.Humanlike)); // Only humanlike pawns
            }

            if (availablePawns.Count == 0)
            {
                reason = "VS.NoMarkedOffMapPawnsAvailable".Translate();
                return false;
            }

            return base.IsEnabledForPawn(out reason);
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            // Open selection dialog
            Find.WindowStack.Add(new Dialog_SelectOffMapPawn(this.pawn, availablePawns, (selectedPawn) =>
            {
                if (selectedPawn != null)
                {
                    base.Cast(targets);
                    SummonPawn(selectedPawn);
                }
            }));
        }

        private void SummonPawn(Pawn pawnToSummon)
        {
            Map targetMap = this.pawn.Map;

            // Remove from world pawns if it's there
            if (Find.WorldPawns.Contains(pawnToSummon))
            {
                Find.WorldPawns.RemovePawn(pawnToSummon);
            }

            // Handle faction changes for non-player pawns
            if (pawnToSummon.Faction != Faction.OfPlayer && !pawnToSummon.Faction.HostileTo(Faction.OfPlayer))
            {
                // Make them join the player faction if they're not hostile
                pawnToSummon.SetFaction(Faction.OfPlayer, this.pawn);
            }

            // Find a random edge cell for spawning
            IntVec3 edgeCell;
            if (!CellFinder.TryFindRandomEdgeCellWith(cell => cell.Walkable(targetMap), targetMap, CellFinder.EdgeRoadChance_Neutral, out edgeCell))
            {
                // Fallback to any edge cell
                edgeCell = CellFinder.RandomEdgeCell(targetMap);
            }

            // Use edge walk-in arrival mode with edge spawn center
            PawnsArrivalModeWorker_EdgeWalkIn arrivalWorker = new PawnsArrivalModeWorker_EdgeWalkIn();
            IncidentParms parms = new IncidentParms
            {
                target = targetMap,
                spawnCenter = edgeCell,
                spawnRotation = Rot4.FromAngleFlat((targetMap.Center - edgeCell).AngleFlat)
            };
            arrivalWorker.Arrive(new List<Pawn> { pawnToSummon }, parms);

            // Add arrival effects
            this.AddEffecterToMaintain(EffecterDefOf.Skip_Entry.Spawn(pawnToSummon.Position, targetMap), pawnToSummon.Position, 60);

            // Play arrival sound
            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(pawnToSummon.Position, targetMap));

            // Stun the summoned pawn briefly
            pawnToSummon.stances.stunner.StunFor(60, this.pawn, false);

            // Message for marked pawn summoning
            Messages.Message("VS.MarkedPawnSummoned".Translate(pawnToSummon.LabelShort),
                new LookTargets(pawnToSummon), MessageTypeDefOf.PositiveEvent);
        }
    }

    public class Dialog_SelectOffMapPawn : Window
    {
        private Pawn caster;
        private List<Pawn> availablePawns;
        private Pawn selectedPawn;
        private Vector2 scrollPosition;
        private float scrollViewHeight;
        private Action<Pawn> onSelect;

        private const float HeaderHeight = 35f;
        private const float PawnElementHeight = 60f;
        private static readonly Vector2 ButSize = new Vector2(150f, 38f);

        public override Vector2 InitialSize => new Vector2(500f, 600f);

        public Dialog_SelectOffMapPawn(Pawn caster, List<Pawn> availablePawns, Action<Pawn> onSelect) : base()
        {
            this.caster = caster;
            this.availablePawns = availablePawns;
            this.onSelect = onSelect;
            this.closeOnAccept = false;
            this.absorbInputAroundWindow = true;
        }

        public override void DoWindowContents(Rect rect)
        {
            Rect titleRect = rect;
            titleRect.yMax -= ButSize.y + 4f;

            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "VS.SelectOffMapPawn".Translate());
            Text.Font = GameFont.Small;

            titleRect.yMin += 39f;
            DisplayPawns(titleRect);

            Rect buttonRect = rect;
            buttonRect.yMin = buttonRect.yMax - ButSize.y;

            if (selectedPawn != null)
            {
                if (Widgets.ButtonText(new Rect(buttonRect.xMax - ButSize.x, buttonRect.y, ButSize.x, ButSize.y), "Accept".Translate()))
                {
                    Accept();
                }
                if (Widgets.ButtonText(new Rect(buttonRect.x, buttonRect.y, ButSize.x, ButSize.y), "Close".Translate()))
                {
                    Close();
                }
            }
            else
            {
                if (Widgets.ButtonText(new Rect((buttonRect.width - ButSize.x) / 2f, buttonRect.y, ButSize.x, ButSize.y), "Close".Translate()))
                {
                    Close();
                }
            }
        }

        private void DisplayPawns(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            rect = rect.ContractedBy(4f);

            GUI.BeginGroup(rect);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, scrollViewHeight);
            float y = 0f;

            Widgets.BeginScrollView(rect.AtZero(), ref scrollPosition, viewRect);

            for (int i = 0; i < availablePawns.Count; i++)
            {
                float width = rect.width;
                if (scrollViewHeight > rect.height)
                    width -= 16f;

                DrawPawn(new Rect(0f, y, width, PawnElementHeight), i);
                y += PawnElementHeight;
            }

            if (Event.current.type == EventType.Layout)
                scrollViewHeight = y;

            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private void DrawPawn(Rect rect, int index)
        {
            Pawn pawn = availablePawns[index];

            if (index % 2 == 1)
                Widgets.DrawLightHighlight(rect);

            if (Mouse.IsOver(rect))
                Widgets.DrawHighlight(rect);

            if (selectedPawn == pawn)
                Widgets.DrawHighlightSelected(rect);

            // Draw pawn portrait
            Rect portraitRect = new Rect(rect.x + 4f, rect.y + 4f, 50f, 50f);
            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            GUI.DrawTexture(portraitRect, PortraitsCache.Get(pawn, new Vector2(50f, 50f), Rot4.South));
            GUI.color = Color.white;

            // Draw pawn info
            Rect infoRect = new Rect(portraitRect.xMax + 8f, rect.y + 4f, rect.width - portraitRect.width - 12f, rect.height - 8f);

            // Name and faction (all pawns are marked)
            string pawnInfo = pawn.LabelShortCap;
            if (pawn.Faction != null)
            {
                pawnInfo += " (" + pawn.Faction.Name + ")";
            }

            Widgets.Label(infoRect.TopPart(0.5f), pawnInfo);

            // Additional info
            string additionalInfo = "";
            if (pawn.Faction != null)
            {
                if (pawn.Faction.HostileTo(Faction.OfPlayer))
                    additionalInfo += "Hostile".Translate();
                else if (pawn.Faction == Faction.OfPlayer)
                    additionalInfo += "Colonist".Translate();
                else
                    additionalInfo += "Neutral".Translate();
            }

            Widgets.Label(infoRect.BottomPart(0.5f), additionalInfo);

            // Tooltip
            if (Mouse.IsOver(rect))
            {
                string tooltipText = pawn.LabelShortCap + "\n" +
                    "Faction".Translate() + ": " + (pawn.Faction?.Name ?? "None".Translate()) + "\n" +
                    "Health".Translate() + ": " + pawn.health.summaryHealth.SummaryHealthPercent.ToStringPercent() + "\n" +
                    "VS.MarkedPawnTooltip".Translate();
                
                TooltipHandler.TipRegion(rect, () => tooltipText, pawn.thingIDNumber);
            }

            if (Widgets.ButtonInvisible(rect))
            {
                selectedPawn = pawn;
                SoundDefOf.RowTabSelect.PlayOneShotOnCamera();
            }
        }

        private void Accept()
        {
            if (onSelect != null)
                onSelect(selectedPawn);
            Close();
        }
    }
}