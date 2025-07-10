using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business
{
	public class CO2eInstructionMatcher
	{
		readonly DtbBooking booking;
		CO2eMatchResult result;
		Dictionary<ZGuid, List<LoadedPackageInfo>> loadedPackages;

		public CO2eInstructionMatcher(DtbBooking booking)
		{
			this.booking = booking;
		}

		public CO2eMatchResult FindAllMatches()
		{
			result = new CO2eMatchResult();
			loadedPackages = new Dictionary<ZGuid, List<LoadedPackageInfo>>();

			booking.Instructions.ForEach(instruction =>
			{
				UnLoadInstructionPackages(instruction);
				LoadInstructionPackages(instruction);
			});
			VerifyResult();

			return result;
		}

		void LoadInstructionPackages(DtbBookingInstruction instruction)
		{
			if (instruction.IsPickUp)
			{
				instruction.PackageDivots.OrderByDescending(d => d.Package.IsOuter)
					.ForEach(d => LoadPackageDivot(d));
			}
			else if (instruction.IsMulti)
			{
				if (instruction.Confirmations.Any(c => c.IsPickUp && c.PackageDivot == null && c.HasPackages))
				{
					instruction.PackageDivots.OrderByDescending(d => d.Package.IsOuter)
						.ForEach(d => LoadPackageDivot(d));
				}
				else
				{
#if NETFRAMEWORK
					instruction.Confirmations
						.Where(c => c.IsPickUp && c.PackageDivot != null)
						.DistinctBy(c => c.PackageDivot.PK)
						.OrderByDescending(c => c.PackageDivot.Package.IsOuter)
						.ForEach(c => LoadPackageDivot(c.PackageDivot));
#else
					IEnumerableExtensions.DistinctBy(instruction.Confirmations
						.Where(c => c.IsPickUp && c.PackageDivot != null),
						c => c.PackageDivot.PK)
						.OrderByDescending(c => c.PackageDivot.Package.IsOuter)
						.ForEach(c => LoadPackageDivot(c.PackageDivot));
#endif
				}
			}
		}

		void UnLoadInstructionPackages(DtbBookingInstruction instruction)
		{
			if (instruction.IsDelivery)
			{
				instruction.PackageDivots.OrderByDescending(d => d.Package.IsOuter)
					.ForEach(d => UnLoadPackageDivot(d));
			}
			else if (instruction.IsMulti)
			{
				if (instruction.Confirmations.Any(c => c.IsDelivery && c.PackageDivot == null && c.HasPackages))
				{
					instruction.PackageDivots.OrderByDescending(d => d.Package.IsOuter)
						.ForEach(d => UnLoadPackageDivot(d));
				}
				else
				{
#if NETFRAMEWORK
					instruction.Confirmations
						.Where(c => c.IsDelivery && c.PackageDivot != null)
						.DistinctBy(c => c.PackageDivot.PK)
						.OrderByDescending(c => c.PackageDivot.Package.IsOuter)
						.ForEach(c => UnLoadPackageDivot(c.PackageDivot));
#else
					IEnumerableExtensions.DistinctBy(instruction.Confirmations
						.Where(c => c.IsDelivery && c.PackageDivot != null),
						c => c.PackageDivot.PK)
						.OrderByDescending(c => c.PackageDivot.Package.IsOuter)
						.ForEach(c => UnLoadPackageDivot(c.PackageDivot));
#endif
				}
			}
		}

		void LoadPackageDivot(DtbBookingInstructionPkgDivot divot)
		{
			var package = divot.Package;
			if (result.HasPackageBeenLoadedAlready(package, divot.Instruction))
			{
				return;
			}

			var maxQuantity = package.KP_PackageQty;
			var quantityToBeLoaded = divot.KD_Quantity;
			if (quantityToBeLoaded == 0)
			{
				return;
			}

			if (loadedPackages.TryGetValue(package.PK, out var loadedDivots))
			{
				var loadedQuantity = loadedDivots.Sum(t => t.LoadedQuantity);
				var newLoadedQuantity = loadedQuantity + quantityToBeLoaded;
				newLoadedQuantity = Math.Min(maxQuantity, newLoadedQuantity);
				var actualLoadedQuantity = newLoadedQuantity - loadedQuantity;

				if (actualLoadedQuantity == 0)
				{
					return;
				}

				loadedPackages[package.PK].Add(new LoadedPackageInfo(divot, false, actualLoadedQuantity));
				result.Add(new CO2eLoadAction(divot, package, actualLoadedQuantity));
				if (!divot.IsEmptyContainer(ConfirmationTypes.Codes.PickUp))
				{
					var innerPackages = package.GetAllPackages();
					innerPackages.ForEach(innerPackage => LoadInnerPackage(innerPackage, divot, actualLoadedQuantity));
				}
			}
			else
			{
				loadedPackages[package.PK] = new List<LoadedPackageInfo> { new LoadedPackageInfo(divot, false, quantityToBeLoaded) };
				result.Add(new CO2eLoadAction(divot, package, quantityToBeLoaded));
				if (!divot.IsEmptyContainer(ConfirmationTypes.Codes.PickUp))
				{
					var innerPackages = package.GetAllPackages();
					innerPackages.ForEach(innerPackage => LoadInnerPackage(innerPackage, divot, quantityToBeLoaded));
				}
			}
		}

		void LoadInnerPackage(PkgPackage innerPackage, DtbBookingInstructionPkgDivot divot, ZInt quantityOfOuterPackageToBeLoaded)
		{
			if (result.Actions.LastOrDefault(action => action.IsPickup && action.GetDivot().PKEquals(divot)) is not CO2eLoadAction latestLoadAction)
			{
				return;
			}

			var quantity = quantityOfOuterPackageToBeLoaded * innerPackage.KP_PackageQty;
			latestLoadAction.InnerPackagesActions.Add(new CO2eLoadAction(divot, innerPackage, quantity));
			if (!loadedPackages.TryGetValue(innerPackage.PK, out var loadedDivots))
			{
				loadedPackages[innerPackage.PK] = new List<LoadedPackageInfo>();
			}
			loadedPackages[innerPackage.PK].Add(new LoadedPackageInfo(divot, true, quantity));
		}

		void UnLoadPackageDivot(DtbBookingInstructionPkgDivot divot)
		{
			var package = divot.Package;
			var quantityToBeUnLoaded = divot.KD_Quantity;

			if (quantityToBeUnLoaded == 0)
			{
				return;
			}

			if (loadedPackages.TryGetValue(package.PK, out var loadedDivots))
			{
				foreach (var loadedDivot in loadedDivots)
				{
					var loadedQuantity = loadedDivot.LoadedQuantity;
					if (loadedQuantity == 0)
					{
						continue;
					}

					if (quantityToBeUnLoaded <= loadedQuantity)
					{
						loadedDivot.LoadedQuantity = loadedQuantity - quantityToBeUnLoaded;
						result.Add(new CO2eUnLoadAction(divot, loadedDivot.PackageDivot, package, quantityToBeUnLoaded));
						package.GetAllPackages().ForEach(innerPackage => UnLoadInnerPackageIfNeeded(innerPackage, divot, quantityToBeUnLoaded));
						break;
					}

					result.Add(new CO2eUnLoadAction(divot, loadedDivot.PackageDivot, package, loadedQuantity));
					package.GetAllPackages().ForEach(innerPackage => UnLoadInnerPackageIfNeeded(innerPackage, divot, loadedQuantity));
					quantityToBeUnLoaded -= loadedQuantity;
					loadedDivot.LoadedQuantity = 0;
				}
			}
		}

		void UnLoadInnerPackageIfNeeded(PkgPackage innerPackage, DtbBookingInstructionPkgDivot unLoadBy, ZInt quantityOfOuterPackageToBeUnloaded)
		{
			if (!result.IsPackageLoadedAsInner(innerPackage))
			{
				return;
			}

			if (loadedPackages.TryGetValue(innerPackage.PK, out var loadedDivots))
			{
				var latestUnLoadAction = result.Actions.LastOrDefault(action => !action.IsPickup && action.GetDivot().PKEquals(unLoadBy)) as CO2eUnLoadAction;
				if (latestUnLoadAction?.IsEmptyContainer ?? false)
				{
					return;
				}

				var quantity = innerPackage.KP_PackageQty * quantityOfOuterPackageToBeUnloaded;
				foreach (var loadedDivot in loadedDivots.Where(divot => divot.LoadedFromOuterPackage))
				{
					var loadedQuantity = loadedDivot.LoadedQuantity;
					if (loadedQuantity == 0)
					{
						continue;
					}

					if (quantity <= loadedQuantity)
					{
						loadedDivot.LoadedQuantity = loadedQuantity - quantity;
						latestUnLoadAction.InnerPackagesActions.Add(new CO2eUnLoadAction(unLoadBy, loadedDivot.PackageDivot, innerPackage, quantity));
						break;
					}

					latestUnLoadAction.InnerPackagesActions.Add(new CO2eUnLoadAction(unLoadBy, loadedDivot.PackageDivot, innerPackage, loadedQuantity));
					quantity -= loadedQuantity;
					loadedDivot.LoadedQuantity = 0;
				}
			}
		}

		void VerifyResult()
		{
			var actions = result.GetActionsIncludingInner();
			result.Actions.RemoveAll(action =>
			{
				if (action is not CO2eLoadAction loadAction)
				{
					return false;
				}

				return !actions.Any(x => x is CO2eUnLoadAction unloadAction && unloadAction.LoadBy.PKEquals(loadAction.LoadBy));
			});
		}
	}

	class LoadedPackageInfo
	{
		public DtbBookingInstructionPkgDivot PackageDivot { get; }
		public ZBool LoadedFromOuterPackage { get; }
		public ZInt LoadedQuantity { get; set; }

		public LoadedPackageInfo(DtbBookingInstructionPkgDivot packageDivot, ZBool loadedFromOuterPackage, ZInt loadedQuantity)
		{
			PackageDivot = packageDivot;
			LoadedFromOuterPackage = loadedFromOuterPackage;
			LoadedQuantity = loadedQuantity;
		}
	}
}
