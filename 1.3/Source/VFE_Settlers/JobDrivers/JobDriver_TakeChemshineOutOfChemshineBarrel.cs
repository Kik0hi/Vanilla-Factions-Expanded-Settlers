﻿using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VFE_Settlers.JobDrivers
{
	public class JobDriver_TakeChemshineOutOfChemshineBarrel : JobDriver
	{
		private const TargetIndex BarrelInd = TargetIndex.A;

		private const TargetIndex ChemshineToHaulInd = TargetIndex.B;

		private const TargetIndex StorageCellInd = TargetIndex.C;

		private const int Duration = 200;

		protected Buildings.Building_ChemshineBarrel Barrel => (Buildings.Building_ChemshineBarrel)job.GetTarget(TargetIndex.A).Thing;

		protected Thing Beer => job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Barrel, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
				.FailOn(() => !Barrel.Fermented)
				.WithProgressBarToilDelay(TargetIndex.A);
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Thing thing = Barrel.TakeOutChemshine();
				GenPlace.TryPlaceThing(thing, pawn.Position, base.Map, ThingPlaceMode.Near);
				StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
				if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, base.Map, currentPriority, pawn.Faction, out IntVec3 foundCell))
				{
					job.SetTarget(TargetIndex.C, foundCell);
					job.SetTarget(TargetIndex.B, thing);
					job.count = thing.stackCount;
				}
				else
				{
					EndJobWith(JobCondition.Incompletable);
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			yield return Toils_Reserve.Reserve(TargetIndex.B);
			yield return Toils_Reserve.Reserve(TargetIndex.C);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
			yield return carryToCell;
			yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, storageMode: true);
		}
	}
}
