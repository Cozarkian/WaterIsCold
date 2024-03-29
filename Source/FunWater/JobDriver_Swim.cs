﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace WaterIsCold
{
    public class JobDriver_Swim : JobDriver
    {
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

        protected override IEnumerable<Toil> MakeNewToils()
        {
			this.FailOn(() => !JoyUtility.EnjoyableOutsideNow(pawn.Map, null));
			this.FailOn(() => pawn.Map.mapTemperature.OutdoorTemp < ModSettings_WaterIsCold.swimTemp - 5f);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			Action swimTick = delegate ()
			{
				JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.GoToNextToil, 1f, null);
			};
			Toil treadWaterToil = Toils_General.Wait(job.def.joyDuration / 3, TargetIndex.C);
			treadWaterToil.tickAction = swimTick;

			//Swim to first spot
			Toil firstSwimToil = Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
			firstSwimToil.tickAction = swimTick;
			firstSwimToil.FailOn(() => this.pawn.Position.GetTerrain(this.pawn.Map) == TerrainDef.Named("Marsh"));
			yield return firstSwimToil;
			yield return treadWaterToil;
			//Swim to second spot
			Toil secondSwimToil = Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
			secondSwimToil.tickAction = swimTick;
			secondSwimToil.FailOn(() => this.pawn.Position.GetTerrain(this.pawn.Map) == TerrainDef.Named("Marsh"));
			yield return secondSwimToil;
			yield return treadWaterToil;
			//Swim back to first spot
			yield return firstSwimToil;
			yield return treadWaterToil;
			//Return to shore
			Toil shoreReturnToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			shoreReturnToil.tickAction = delegate ()
			{
				JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.None, 1f, null);
			};
			yield return shoreReturnToil;
			yield break;
		}



        public override string GetReport()
		{
			TerrainDef terrain = this.pawn.Position.GetTerrain(this.pawn.Map);
			if (!terrain.IsWater)
            {
				return "WIC.goingswim".Translate();
            }
			if(terrain == TerrainDefOf.WaterShallow || terrain == TerrainDefOf.WaterMovingShallow || terrain == TerrainDefOf.WaterOceanShallow)
			{
				return "WIC.wading".Translate();
            }
			if (terrain == TerrainDefOf.WaterOceanDeep)
            {
				return "WIC.bodysurfing".Translate();
            }
			if (terrain == TerrainDefOf.WaterDeep)
            {
				return "WIC.treading".Translate();
            }
			return "WIC.swimming".Translate();
		}
	}
}