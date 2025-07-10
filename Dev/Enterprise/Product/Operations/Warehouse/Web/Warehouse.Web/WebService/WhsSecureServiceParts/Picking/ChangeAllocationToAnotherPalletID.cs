using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ChangeAllocationToAnotherPalletID

		[WebMethod(Description = "Allocate another palletID for a pickLines")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPalletIDNeutralWebServiceResponse ChangeAllocationToAnotherPalletID(Guid[] pickLinePKs, string newPalletID)
		{
			if (pickLinePKs == null || pickLinePKs.Length == 0)
			{
				throw new ArgumentException("pickLinesPK cannot be empty");
			}

			if (string.IsNullOrWhiteSpace(newPalletID))
			{
				throw new ArgumentException("newPalletID cannot be empty");
			}

			return HandleWebServiceRequest<WhsPalletIDNeutralWebServiceResponse>(r => ChangeAllocationToAnotherPalletID(r, pickLinePKs, newPalletID));
		}

		void ChangeAllocationToAnotherPalletID(WhsPalletIDNeutralWebServiceResponse response, Guid[] pickLinePKs, string newPalletID)
		{
			var originalPickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, pickLinePKs));
			var firstPickLine = originalPickLines.FirstOrDefault();

			if (firstPickLine == null)
			{
				response.LogBusinessValidationError(Res.GetString("f07be39c-2551-4ed7-b7e9-2963136c880b", "No pick lines found to be allocated. Please reload this pick and try again."));
			}
			else
			{
				var pick = firstPickLine.Pick;

				var originalPalletIDs = originalPickLines.Select(l => (string)l.Inventory.WI_PalletID).Distinct().ToArray();

				if (originalPalletIDs.All(pltId => pltId != newPalletID))
				{
					var pickedUnits = originalPickLines.Sum(l => l.WZ_Units);
					var orderedInventories = originalPickLines
						.Select(pl => pl.DocketLine).Cast<WhsPickableDocketLine>()
						.Select(l => pick.OrderedInventories.GetOrderedInventoryForLine(l))
						.Distinct().ToArray();

					if (orderedInventories.Length == 1) // we are going to handle only case when we have a single ordered inventory for those picklines
					{
						var orderedInventory = orderedInventories.Single();

						var availableInventories = orderedInventory.GetAvailableInventoriesForPalletIDs(originalPalletIDs.Concat(new[] { newPalletID })).Cast<WhsPickAvailableInventory>().ToArray();
						var availableInventoriesForNewPalletID = availableInventories.Where(ai => ai.PalletID == newPalletID).ToArray();

						var availableInventoryForNewPalletId = availableInventoriesForNewPalletID.FirstOrDefault();
						var originalInventory = firstPickLine.Inventory;
						if (availableInventoryForNewPalletId != null && availableInventoryForNewPalletId.Location.PK != originalInventory.Location.PK)
						{
							response.LogBusinessValidationError(Res.GetString("3e9efc10-6057-4fd7-8b79-2a0f8cdc4ecf", "You can only pick a pallet from the allocated location."));
						}
						else if (originalInventory.Location.WLV_LocationStatus != LocationStatus.Codes.Normal)
						{
							response.LogBusinessValidationError(Res.GetString("5f78edaa-daa9-4fc3-a29a-64fee8421392", "Cannot Reallocate to another Pallet, as location status must be NOR - (Normal)."));
						}
						else
						{
							var availableInventoryForNewPalletIdWhichWillFitAllPickLines = availableInventoriesForNewPalletID.FirstOrDefault(ai => ai.QuantityUnPicked >= pickedUnits);
							if (availableInventoryForNewPalletIdWhichWillFitAllPickLines != null)
							{
								if (availableInventoryForNewPalletIdWhichWillFitAllPickLines.CanUpdatePickLineQuantity)
								{
									AllocateToNewPalletID(originalPickLines, availableInventories, availableInventoryForNewPalletIdWhichWillFitAllPickLines, pickedUnits, response);
								}
								else
								{
									response.LogBusinessValidationError(GetUnableToUpdateAllocatedQuantityMessage(newPalletID));
								}
							}
							else if (originalPickLines.Length == 1)
							{
								TryToSwapPickLine(newPalletID, originalPickLines[0], availableInventories, pickedUnits, response);
							}
						}
					}

					if (response.ErrorMessage.IsNullOrEmpty())
					{
						if (response.NewPickLine == null)
						{
							response.LogBusinessValidationError(Res.GetString("eeb59ee9-d27e-4248-a46b-8931620792fa", "Pallet {0} cannot be selected for this pick.", newPalletID));
						}

						// if we use complete pallets picking we have to reload ALL complete pallets info, because it may be changed during allocation
						if (response.NewPickLine != null && response.NewPickLine.PartAttributes.CompletePalletPickingUsed)
						{
							response.CompletePalletPickingPallets = GetCompletePallets(pick);
						}
					}
				}
			}
		}

		static string GetUnableToUpdateAllocatedQuantityMessage(string newPalletID) => Res.GetString("79082433-d1b2-4ef2-a63e-e6c0088b929e", "Unable to pick Pallet {0} because either the Product is a Component Line on a Sales Order or the Pick is in the process of Cartonization.", newPalletID);

		#region AllocateToNewPalletID

		void AllocateToNewPalletID(WhsPickLine[] pickLines, WhsPickAvailableInventory[] availableInventories, WhsPickAvailableInventory availableInventoryForNewPalletIdWhichWillFitAllPickLines, decimal pickedUnits, WhsPalletIDNeutralWebServiceResponse result)
		{
			var orders = new HashSet<WhsDocket>();
			var taskPK = pickLines[0].WZ_P9_Task;

			var packedItemValueAndPackages = GetPackedItemValueAndUnpackPackages(pickLines);
			foreach (var pickLine in pickLines)
			{
				var order = pickLine.DocketLine.Docket;
				if (pickLine.IsReserveLine)
				{
					pickLine.WZ_Units = 0;
				}
				else
				{
					pickLine.Delete();
				}

				orders.Add(order);
			}

			foreach (var availableInventory in availableInventories)
			{
				availableInventory.ClearPickLinesCache();
			}

			using (new DisposableList(orders.OfType<WhsPickableDocket>().Select(o => o.SuspendUpdatingReleaseTotals_DoNotUse())))
			{
				availableInventoryForNewPalletIdWhichWillFitAllPickLines.PickLineQuantity += pickedUnits;
			}

			var addedPickLines = availableInventoryForNewPalletIdWhichWillFitAllPickLines.PickLines.Where(pl => !pl.IsInDatabase).ToArray();
			if (!packedItemValueAndPackages.IsNullOrEmpty())
			{
				PackAddedPicklinesIntoPackages(packedItemValueAndPackages, addedPickLines);
				addedPickLines = availableInventoryForNewPalletIdWhichWillFitAllPickLines.PickLines.Where(pl => !pl.IsInDatabase).ToArray();
			}
			UpdateTasksForPickLines(taskPK, addedPickLines);

			result.NewPickLine = new WhsPickLineInfo(new PickLineGroupingInfo(addedPickLines.First()), addedPickLines.Select(pl => pl.PK).ToArray(), addedPickLines.Sum(pl => pl.WZ_Units), new WhsPickInfo());
			Factory.Save();
		}

		#endregion

		#region GetPackedItemValueAndUnpackPackages

		Dictionary<ZGuid, Dictionary<PkgPackage, ZDecimal>> GetPackedItemValueAndUnpackPackages(WhsPickLine[] pickLines)
		{
			var linesForPickLines = pickLines.Select(pl => (WhsPickableDocketLine)pl.DocketLine).Distinct().ToArray();
			var releaseLines = PackageHelper.GetReleaseLinesByKey(linesForPickLines);
			var divotQuery = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, pickLines.Select(p => p.PK));
			var packageDivots = Factory.Load<PkgPackageItemDivot>(divotQuery);

			var result = new Dictionary<ZGuid, Dictionary<PkgPackage, ZDecimal>>();
			foreach (var divot in packageDivots)
			{
				var divotPkg = divot.ParentPackage;
				var pickline = (WhsPickLine)divot.PackedItem;

				if (!result.TryGetValue(pickline.WZ_WE_TransactionLine, out var pkgAndPackedQty))
				{
					result[pickline.WZ_WE_TransactionLine] = pkgAndPackedQty = new Dictionary<PkgPackage, ZDecimal>();
				}

				if (pkgAndPackedQty.TryGetValue(divotPkg, out var packedQty))
				{
					pkgAndPackedQty[divotPkg] = packedQty + divot.KI_PackedQty;
				}
				else
				{
					pkgAndPackedQty[divotPkg] = divot.KI_PackedQty;
				}

				divot.DeleteForRepacking(releaseLines[divot.PackedItem.Key]);
			}

			return result;
		}

		#endregion

		#region PackAddedPicklinesIntoPackages

		void PackAddedPicklinesIntoPackages(Dictionary<ZGuid, Dictionary<PkgPackage, ZDecimal>> packedItemValueAndPackages, WhsPickLine[] addedPickLines)
		{
			var docketLinesForPickLines = addedPickLines.Select(pl => (WhsPickableDocketLine)pl.DocketLine).Distinct().ToArray();
			var groupedReleaseLines = PackageHelper.GetReleaseLinesByKey(docketLinesForPickLines);

			foreach (var pickLine in addedPickLines)
			{
				if (packedItemValueAndPackages.TryGetValue(pickLine.WZ_WE_TransactionLine, out var packageDictionary))
				{
					RePackPickLine(pickLine, packageDictionary);
				}
			}

			void RePackPickLine(WhsPickLine pickLine, Dictionary<PkgPackage, ZDecimal> packageDictionary)
			{
				var pickLineQty = pickLine.WZ_Units;

				foreach (var pkgAndPackedQty in packageDictionary.ToArray())
				{
					var releaseLine = groupedReleaseLines[((IPackableItem)pickLine).Key];
					var package = pkgAndPackedQty.Key;
					var qtyToPack = pkgAndPackedQty.Value;

					if (qtyToPack >= pickLineQty)
					{
						package.Pack(pickLine, releaseLine);
						packageDictionary[package] = qtyToPack - pickLineQty;
						pickLineQty = 0m;
					}
					else
					{
						var newPickLine = pickLine.Split(qtyToPack);
						package.Pack(newPickLine, releaseLine);
						pickLineQty -= qtyToPack;
						packageDictionary[package] = 0m;
					}

					if (packageDictionary[package] == 0m)
					{
						packageDictionary.Remove(package);
					}

					if (pickLineQty == 0m)
					{
						break;
					}
				}
			}
		}

		#endregion

		#region TryToSwapPickLine

		void TryToSwapPickLine(string newPalletID, WhsPickLine originalPickLine, WhsPickAvailableInventory[] availableInventories, decimal pickedUnits, WhsPalletIDNeutralWebServiceResponse result)
		{
			var originalPickLineInventory = originalPickLine.Inventory;
			var originalAvailableInventoryForOriginalPickLine = availableInventories.FirstOrDefault(ai => ai.PickLines.Contains(originalPickLine) && ai.PickLineQuantity == originalPickLine.WZ_Units);
			if (originalAvailableInventoryForOriginalPickLine != null)
			{
				if (originalAvailableInventoryForOriginalPickLine.CanUpdatePickLineQuantity)
				{
					TryToSwapPickLine();
				}
				else
				{
					result.LogBusinessValidationError(GetUnableToUpdateAllocatedQuantityMessage(newPalletID));
				}
			}

			void TryToSwapPickLine()
			{
				var potentialPickLinesToSwap = availableInventories
					.Where(ai => ai.PalletID == newPalletID && ai.Inventory.Count == 1)
					.SelectMany(ai => ai.PickLinesCommittedFromAllPicks)
					.Cast<WhsPickLine>()
					.Where(pl => pl.WZ_Units == pickedUnits && !pl.IsPickFinalisedOrCancelled && !pl.IsPickedFromPutawayLocation && pl.DocketLine is WhsPickableDocketLine);

				var (availableInventoriesForSwappingPickLine, pickLineToSwap, wasUnableToUpdateAllocatedQty) = FindPickLineToSwap(potentialPickLinesToSwap);

				if (pickLineToSwap != null)
				{
					var taskPK = originalPickLine.WZ_P9_Task;
					var packedItemValueAndPackages = GetPackedItemValueAndUnpackPackages(new[] { originalPickLine, pickLineToSwap });
					var addedPickLines = SwapPickLines(originalPickLine, pickLineToSwap, availableInventories, originalAvailableInventoryForOriginalPickLine, availableInventoriesForSwappingPickLine.Cast<WhsPickAvailableInventory>());
					if (!packedItemValueAndPackages.IsNullOrEmpty())
					{
						PackAddedPicklinesIntoPackages(packedItemValueAndPackages, addedPickLines);
					}
					UpdateTasksForPickLines(taskPK, addedPickLines);

					result.NewPickLine = new WhsPickLineInfo(addedPickLines.First(), new WhsPickInfo());
					Factory.Save();
				}
				else if (wasUnableToUpdateAllocatedQty)
				{
					result.LogBusinessValidationError(GetUnableToUpdateAllocatedQuantityMessage(newPalletID));
				}
			}

			(WhsPickAvailableInventoryCollection, WhsPickLine, bool) FindPickLineToSwap(IEnumerable<WhsPickLine> potentialPickLinesToSwap)
			{
				var wasUnableToUpdateAllocatedQty = false;
				WhsPickAvailableInventoryCollection availableInventoriesForSwappingPickLine = null;
				foreach (var potentialPickLineToSwap in potentialPickLinesToSwap)
				{
					availableInventoriesForSwappingPickLine = GetAvailableInventoryCollection(potentialPickLineToSwap.DocketLine, originalPickLineInventory.WI_PalletID, newPalletID);

					foreach (WhsPickAvailableInventory availableInventory in availableInventoriesForSwappingPickLine)
					{
						if (availableInventory.Inventory.Contains(originalPickLineInventory))
						{
							if (availableInventory.CanUpdatePickLineQuantity)
							{
								return (availableInventoriesForSwappingPickLine, potentialPickLineToSwap, false);
							}
							else
							{
								wasUnableToUpdateAllocatedQty = true;
							}
						}
					}
				}

				return (null, null, wasUnableToUpdateAllocatedQty);
			}
		}

		WhsPickAvailableInventoryCollection GetAvailableInventoryCollection(WhsDocketLine docketLine, string palletID, string newPalletID)
		{
			var pick = (docketLine.Docket as WhsPickableDocket)?.Pick;
			return pick == null
				? new WhsPickAvailableInventoryCollection(Factory)
				: pick.OrderedInventories.Cast<WhsPickOrderedInventory>().First(i => i.Owners.Contains(docketLine)).GetAvailableInventoriesForPalletIDs(new[] { palletID, newPalletID });
		}

		WhsPickLine[] SwapPickLines(WhsPickLine originalPickLine, WhsPickLine pickLineToSwap,
			IEnumerable<WhsPickAvailableInventory> availableInventoriesForOriginalPickLine,
			WhsPickAvailableInventory originalInventoryForOriginalPickLine,
			IEnumerable<WhsPickAvailableInventory> availableInventoriesForSwappingPickLine)
		{
			// Not sure if all 'Sequence contains no matching element' issues are handled so will keep the ErrorReporter for a while. Created WI00648157 and will remove them in it.
			var units = originalPickLine.WZ_Units;
			var pickerOfOriginalPickLine = originalPickLine.WZ_GS_NKAssignedTo;

			if (!availableInventoriesForSwappingPickLine.Any(ai => ai.Inventory.Contains(pickLineToSwap.Inventory)))
			{
				ErrorReporter.ReportOnce("Swapping PickLine, originalInventoryForSwappingPickLine is null.");
			}
			var originalInventoryForSwappingPickLine = availableInventoriesForSwappingPickLine.First(ai => ai.Inventory.Contains(pickLineToSwap.Inventory));

			if (!availableInventoriesForSwappingPickLine.Any(ai => ai.Inventory.Contains(originalPickLine.Inventory)))
			{
				ErrorReporter.ReportOnce("Swapping PickLine, targetInventoryForSwappingPickLine is null.");
			}
			var targetInventoryForSwappingPickLine = availableInventoriesForSwappingPickLine.First(ai => ai.Inventory.Contains(originalPickLine.Inventory));

			if (!availableInventoriesForOriginalPickLine.Any(ai => ai.Inventory.Contains(pickLineToSwap.Inventory)))
			{
				ErrorReporter.ReportOnce("Swapping PickLine, targetInventoryForOriginalPickLine is null.");
			}
			var targetInventoryForOriginalPickLine = availableInventoriesForOriginalPickLine.First(ai => ai.Inventory.Contains(pickLineToSwap.Inventory));

			var previousPickLineQtyForOrigPickLineInv = originalInventoryForOriginalPickLine.PickLineQuantity;
			using (originalInventoryForOriginalPickLine.AllowPickLineQuantityReduction())
			{
				originalInventoryForOriginalPickLine.PickLineQuantity -= units;
			}

			if (originalInventoryForOriginalPickLine.PickLineQuantity != previousPickLineQtyForOrigPickLineInv - units)
			{
				ErrorReporter.ReportOnce($"Swapping PickLine, Pick line quantity for originalInventoryForOriginalPickLine did not update after decrementing {units} units." +
					$" {string.Join("|", originalInventoryForOriginalPickLine.PickLineQuantityInfo.GetWarnings().Select(w => w.Message))}" +
					$" {string.Join("|", originalInventoryForOriginalPickLine.PickLineQuantityInfo.GetErrors().Select(w => w.Message))}");
			}

			var previousPickLineQtyForSwapPickLineInv = originalInventoryForSwappingPickLine.PickLineQuantity;
			using (originalInventoryForSwappingPickLine.AllowPickLineQuantityReduction())
			{
				originalInventoryForSwappingPickLine.PickLineQuantity -= units;
			}

			if (originalInventoryForSwappingPickLine.PickLineQuantity != previousPickLineQtyForSwapPickLineInv - units)
			{
				ErrorReporter.ReportOnce($"Swapping PickLine, Pick line quantity for originalInventoryForSwappingPickLine did not update after decrementing {units} units." +
					$" {string.Join("|", originalInventoryForSwappingPickLine.PickLineQuantityInfo.GetWarnings().Select(w => w.Message))}" +
					$" {string.Join("|", originalInventoryForSwappingPickLine.PickLineQuantityInfo.GetErrors().Select(w => w.Message))}");
			}

			var previousPickLineQtyForSwapPickLineTargetInv = targetInventoryForSwappingPickLine.PickLineQuantity;
			targetInventoryForSwappingPickLine.PickLineQuantity += units;
			if (targetInventoryForSwappingPickLine.PickLineQuantity != previousPickLineQtyForSwapPickLineTargetInv + units)
			{
				ErrorReporter.ReportOnce($"Swapping PickLine, Pick line quantity for targetInventoryForSwappingPickLine did not update after incrementing {units} units." +
					$" {string.Join("|", targetInventoryForSwappingPickLine.PickLineQuantityInfo.GetWarnings().Select(w => w.Message))}" +
					$" {string.Join("|", targetInventoryForSwappingPickLine.PickLineQuantityInfo.GetErrors().Select(w => w.Message))}");
			}

			var previousPickLineQtyForOrigPickLineTargetInv = targetInventoryForOriginalPickLine.PickLineQuantity;
			targetInventoryForOriginalPickLine.PickLineQuantity += units;
			if (targetInventoryForOriginalPickLine.PickLineQuantity != previousPickLineQtyForOrigPickLineTargetInv + units)
			{
				ErrorReporter.ReportOnce($"Swapping PickLine, Pick line quantity for targetInventoryForOriginalPickLine did not update after incrementing {units} units." +
					$" {string.Join("|", targetInventoryForOriginalPickLine.PickLineQuantityInfo.GetWarnings().Select(w => w.Message))}" +
					$" {string.Join("|", targetInventoryForOriginalPickLine.PickLineQuantityInfo.GetErrors().Select(w => w.Message))}");
			}

			if (!targetInventoryForOriginalPickLine.PickLines.Any(pl => pl.HasChanges))
			{
				ErrorReporter.ReportOnce("Swapping PickLine, changes for targetInventoryForOriginalPickLine is null.");
			}
			var addedPickLine = targetInventoryForOriginalPickLine.PickLines.First(pl => pl.HasChanges);
			addedPickLine.WZ_GS_NKAssignedTo = pickerOfOriginalPickLine;

			if (!targetInventoryForSwappingPickLine.PickLines.Any(pl => pl.HasChanges))
			{
				ErrorReporter.ReportOnce("Swapping PickLine, changes for targetInventoryForSwappingPickLine is null.");
			}
			return new[] { addedPickLine, targetInventoryForSwappingPickLine.PickLines.First(pl => pl.HasChanges) };
		}

		void UpdateTasksForPickLines(ZGuid originalPickLineTaskPK, WhsPickLine[] newPickLines)
		{
			if (originalPickLineTaskPK != ZGuid.Empty)
			{
				newPickLines.ForEach(pl => pl.WZ_P9_Task = originalPickLineTaskPK);
			}
		}

		#endregion

		#endregion
	}
}
