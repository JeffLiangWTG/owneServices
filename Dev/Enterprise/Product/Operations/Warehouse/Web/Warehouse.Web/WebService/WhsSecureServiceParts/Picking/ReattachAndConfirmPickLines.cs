using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region ReattachAndConfirmPickLines

		[WebMethod(Description = "Reattach and Confirm pick lines")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickWebServiceResponse ReattachAndConfirmPickLines(WhsPickLineInfo[] pickLinesToSave, PackageInfo packageToPackInto)
		{
			return HandleWebServiceRequest<WhsPickWebServiceResponse>(r => ReattachAndConfirmPickLines(r, pickLinesToSave, packageToPackInto));
		}

		void ReattachAndConfirmPickLines(WhsPickWebServiceResponse response, WhsPickLineInfo[] pickLinesToSave, PackageInfo packageToPackInto)
		{
			var linesThatCannotBeSaved = new List<WhsPickLineInfo>();
			var oper = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var groupedPickLines = GroupPickLinesByPick(pickLinesToSave);
			var whs = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

			// instead of passing part attribute infos from RF we can populate them here
			var pickInfoForPassedPickLineInfos = new WhsPickInfo();
			foreach (var pickLineInfo in pickLinesToSave)
			{
				pickLineInfo.LinkToPickInfo(pickInfoForPassedPickLineInfos);
				var bomProduct = pickLineInfo.BOMProductPK == Guid.Empty ? null : Factory.Load<OrgSupplierPart>(pickLineInfo.BOMProductPK);
				pickLineInfo.FillProductAndAttributesListsOnParent(Factory.Load<OrgSupplierPart>(pickLineInfo.ProductPK), bomProduct, Factory.Load<OrgHeader>(pickLineInfo.ClientPK), whs);
			}

			foreach (var pickPair in groupedPickLines)
			{
				var package = packageToPackInto != null ? GetPackageFromInfo(response, pickPair.Key, packageToPackInto) : null;
				if (packageToPackInto == null || package != null)
				{
					var groupedAvailableInventories = RollUpAvailableInventoriesToPickIntoDictionary(pickPair.Key);
					linesThatCannotBeSaved.AddRange(ReattachAndConfirmPickLinesCore(oper, pickPair.Value, groupedAvailableInventories, package));
				}
			}

			var pickInfo = new WhsPickInfo();
			pickInfo.Lines.AddRange(linesThatCannotBeSaved);

			response.Pick = pickInfo;
			WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => Res.GetString("2b473ae9-28ec-4cb5-8904-f3d04a8a975a", "While you have been working with this job another user has made changes. Please restart the operation and try again."));
		}

		#region GroupPickLinesByPick

		Dictionary<WhsPick, Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickLineInfo>>>> GroupPickLinesByPick(WhsPickLineInfo[] pickLinesToSave)
		{
			var result = new Dictionary<WhsPick, Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickLineInfo>>>>();
			var query = new ZQuery(WhsPickLineSchema.PK, pickLinesToSave.SelectMany(l => l.PKs));
			var allPickLines = Factory.Load<WhsPickLine>(query);

			foreach (var pickLine in allPickLines)
			{
				var transactionLine = (WhsPickableDocketLine)pickLine.DocketLine;
				var transaction = transactionLine.PickableDocket;
				var pick = transaction.Pick;

				if (!result.TryGetValue(pick, out var orderedInventoriesDictionary))
				{
					orderedInventoriesDictionary = new Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickLineInfo>>>();
					result.Add(pick, orderedInventoriesDictionary);
				}

				var orderedInventory = pick.OrderedInventories.GetOrderedInventoryForLine(transactionLine);
				if (!orderedInventoriesDictionary.TryGetValue(orderedInventory, out var availableInventoriesDictionary))
				{
					availableInventoriesDictionary = new Dictionary<string, List<WhsPickLineInfo>>();
					orderedInventoriesDictionary.Add(orderedInventory, availableInventoriesDictionary);
				}

				var pickLineInfo = pickLinesToSave.Single(l => l.PKs.Any(pk => pk == pickLine.PK.ToGuid()));
				var availableInventoryKey = GetAvailableInventoriesDictionaryKey(pickLineInfo);
				if (!availableInventoriesDictionary.TryGetValue(availableInventoryKey, out var pickLineInfos))
				{
					pickLineInfos = new List<WhsPickLineInfo>();
					availableInventoriesDictionary.Add(availableInventoryKey, pickLineInfos);
				}

				pickLineInfos.Add(pickLineInfo);
			}

			return result;
		}

		#region GetAvailableInventoriesDictionaryKey

		const string Separator = "~";

		string GetAvailableInventoriesDictionaryKey(WhsPickLineInfo pickLineInfo)
		{
			return pickLineInfo.PalletID + Separator + pickLineInfo.Location;
		}

		#endregion

		#endregion

		#region GetPackageFromInfo

		PkgPackage GetPackageFromInfo(WebServiceResponse response, WhsPick pick, PackageInfo packageToPackInto)
		{
			PkgPackage result = null;

			var singleOrder = pick.Orders.Count == 1 ? (WhsOrder)pick.Orders[0] : null;
			if (singleOrder != null)
			{
				var package = WebServiceHelper.GetPackageByPackagePK(singleOrder, packageToPackInto.PK);
				if (package == null)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("09521e50-e489-42dd-9184-d33cf702b2ec", "Package ID '{0}' does not exist.", packageToPackInto.PackageID);
				}
				else if (package.IsClosed)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("8ad13e7e-62e9-4ee5-864d-8a3e20347e48", "Package '{0}' is already closed.", packageToPackInto.PackageID);
				}
				else
				{
					result = package;
				}
			}

			return result;
		}

		#endregion

		#region RollUpAvailableInventoriesIntoDictionary

		Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickAvailableInventory>>> RollUpAvailableInventoriesToPickIntoDictionary(WhsPick pick)
		{
			var dictionary = new Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickAvailableInventory>>>();

			foreach (WhsPickOrderedInventory orderedInventory in pick.OrderedInventories)
			{
				var orderedInventoriesDictionary = new Dictionary<string, List<WhsPickAvailableInventory>>();
				List<WhsPickAvailableInventory> availableInventoriesList = null;

				foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
				{
					string availableInventoryDictionaryKey = GetAvailableInventoriesDictionaryKey(availableInventory);

					if (orderedInventoriesDictionary.TryGetValue(availableInventoryDictionaryKey, out availableInventoriesList))
					{
						availableInventoriesList.Add(availableInventory);
					}
					else
					{
						availableInventoriesList = new List<WhsPickAvailableInventory> { availableInventory };

						orderedInventoriesDictionary.Add(availableInventoryDictionaryKey, availableInventoriesList);
					}
				}

				if (availableInventoriesList != null)
				{
					dictionary.Add(orderedInventory, orderedInventoriesDictionary);
				}
			}

			return dictionary;
		}

		#region GetAvailableInventoriesDictionaryKey

		string GetAvailableInventoriesDictionaryKey(WhsPickAvailableInventory availableInventory)
		{
			return availableInventory.PalletID + Separator + availableInventory.Location?.WLV_LocationString ?? ZString.Empty;
		}

		#endregion

		#endregion

		#region ReattachAndConfirmPickLinesCore

		List<WhsPickLineInfo> ReattachAndConfirmPickLinesCore(GlbStaff rfGunUser, Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickLineInfo>>> pickLinesToSaveDictionary,
			Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickAvailableInventory>>> availableInventoriesToPickDictionary, PkgPackage packageToPackInto)
		{
			var pickLineInfosThatCouldNotBePicked = ReattachAndConfirmPickLines_OrderedInventoriesLevel(rfGunUser, pickLinesToSaveDictionary, availableInventoriesToPickDictionary, packageToPackInto);
			LoadPickNoAndOrderRefForPickLineInfos(pickLineInfosThatCouldNotBePicked);

			return pickLineInfosThatCouldNotBePicked;
		}

		#region ReattachAndConfirmPickLines_OrderedInventoriesLevel

		List<WhsPickLineInfo> ReattachAndConfirmPickLines_OrderedInventoriesLevel(GlbStaff rfGunUser, Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickLineInfo>>> pickLinesToSaveDictionary,
			Dictionary<WhsPickOrderedInventory, Dictionary<string, List<WhsPickAvailableInventory>>> availableInventoriesToPickDictionary, PkgPackage packageToPackInto)
		{
			var result = new List<WhsPickLineInfo>();
			foreach (var orderedInventory in pickLinesToSaveDictionary.Keys)
			{
				if (availableInventoriesToPickDictionary.TryGetValue(orderedInventory, out var orderedInventoriesToPickDictionary))
				{
					var pickLinesThatCouldNotBePicked = ReattachAndConfirmPickLines_AvailableInventoriesLevel(rfGunUser, pickLinesToSaveDictionary[orderedInventory], orderedInventoriesToPickDictionary, packageToPackInto);
					result.AddRange(pickLinesThatCouldNotBePicked);
				}
				else
				{
					foreach (var pickLineInfoList in pickLinesToSaveDictionary[orderedInventory].Values)
					{
						result.AddRange(pickLineInfoList);
					}
				}
			}
			return result;
		}

		IEnumerable<WhsPickLineInfo> ReattachAndConfirmPickLines_AvailableInventoriesLevel(GlbStaff rfGunUser, Dictionary<string, List<WhsPickLineInfo>> pickLinesToSaveDictionary,
			Dictionary<string, List<WhsPickAvailableInventory>> availableInventoriesToPickDictionary, PkgPackage packageToPackInto)
		{
			var result = new List<WhsPickLineInfo>();
			foreach (string availableInventoryDictionaryKey in pickLinesToSaveDictionary.Keys)
			{
				if (availableInventoriesToPickDictionary.TryGetValue(availableInventoryDictionaryKey, out var availableInventoriesToPick))
				{
					var pickLinesThatCouldNotBePicked = ReattachAndConfirmPickLines_PickLinesLevel(rfGunUser, pickLinesToSaveDictionary[availableInventoryDictionaryKey], availableInventoriesToPick, packageToPackInto);
					result.AddRange(pickLinesThatCouldNotBePicked);
				}
				else
				{
					result.AddRange(pickLinesToSaveDictionary[availableInventoryDictionaryKey]);
				}
			}
			return result;
		}

		IEnumerable<WhsPickLineInfo> ReattachAndConfirmPickLines_PickLinesLevel(GlbStaff rfGunUser, IEnumerable<WhsPickLineInfo> pickLineInfoList, IEnumerable<WhsPickAvailableInventory> availableInventoryList, PkgPackage packageToPackInto)
		{
			var result = new List<WhsPickLineInfo>();
			var pickedAvailableInventoryList = new HashSet<WhsPickAvailableInventory>();
			var pickLinesWhichShouldNotBeChanged = new HashSet<WhsPickLine>();
			var pickLinePKs = new HashSet<Guid>(pickLineInfoList.SelectMany(p => p.PKs));

			foreach (var availableInventory in availableInventoryList)
			{
				foreach (var pickLine in availableInventory.PickLines.Where(pl => pickLinePKs.Contains(pl.PK.ToGuid())))
				{
					if ((pickLine.WZ_GS_NKAssignedTo.IsEmpty || pickLine.WZ_GS_NKAssignedTo == rfGunUser.GS_Code)
						&& !pickLine.IsPickedFromPutawayLocation)
					{
						pickedAvailableInventoryList.Add(availableInventory);
					}
					else
					{
						pickLinesWhichShouldNotBeChanged.Add(pickLine);
					}
				}
			}

			var candidatePickLines = new Lazy<WhsPickLine[]>(() => Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, pickLineInfoList.SelectMany(l => l.PKs))));

			foreach (var pickLineInfo in pickLineInfoList)
			{
				// scenario when confirmed serial matches the expected
				var allocated = TryToAllocateIntoExistingPickLine(pickLineInfo, rfGunUser, pickedAvailableInventoryList, packageToPackInto, candidatePickLines);

				// scenario when we need to try switching pickline to another inventory
				if (!allocated)
				{
					TryToSwitchPickLineToTargetSerialNumber(rfGunUser, availableInventoryList, packageToPackInto, pickLineInfo, pickLinesWhichShouldNotBeChanged, result);
				}
			}
			return result;
		}

		void TryToSwitchPickLineToTargetSerialNumber(GlbStaff rfGunUser, IEnumerable<WhsPickAvailableInventory> availableInventoryList, PkgPackage packageToPackInto, WhsPickLineInfo pickLineInfo,
			HashSet<WhsPickLine> pickLinesWhichShouldNotBeChanged, List<WhsPickLineInfo> result)
		{
			var oldAItoPLpair =
				(from avlInv in availableInventoryList
				 from pl in avlInv.PickLines
				 select new { AvlInv = avlInv, PickLine = pl }).SingleOrDefault(o => o.PickLine.PK == pickLineInfo.PKs.Single());

			if (oldAItoPLpair != null && !pickLinesWhichShouldNotBeChanged.Contains(oldAItoPLpair.PickLine))
			{
				var pickLine = oldAItoPLpair.PickLine;
				TryToSwitchPickLineToTargetSerial(rfGunUser, availableInventoryList, packageToPackInto, pickLineInfo, result, pickLine);
			}
			else
			{
				result.Add(pickLineInfo);
			}
		}

		void TryToSwitchPickLineToTargetSerial(GlbStaff rfGunUser, IEnumerable<WhsPickAvailableInventory> availableInventoryList, PkgPackage packageToPackInto, WhsPickLineInfo pickLineInfo,
			List<WhsPickLineInfo> result, WhsPickLine pickLine)
		{
			var newAItoINVpair =
				(from avlInv in availableInventoryList
				 from inv in avlInv.Inventory.Cast<WhsInventoryView>()
				 select new { AvlInv = avlInv, Inventory = inv }).SingleOrDefault(o => IsSerialNumbersMatching(pickLineInfo, o.Inventory));

			var pickLineSwitchedToTargetSerial = false;
			if (newAItoINVpair != null)
			{
				var newInventory = newAItoINVpair.Inventory;
				var pickLineWithScannedSerial = newInventory.CommittedPickLines.SingleOrDefault(); // should have only one as we are using serial numbers or nothing
				if ((pickLineWithScannedSerial == null || !pickLineWithScannedSerial.IsPicked) && IsSwappable(newInventory, pickLineWithScannedSerial))
				{
					SwitchPickLineToTargetSerial(rfGunUser, packageToPackInto, pickLineInfo, pickLine, newInventory, pickLineWithScannedSerial);
					pickLineSwitchedToTargetSerial = true;
				}
			}

			if (!pickLineSwitchedToTargetSerial)
			{
				result.Add(pickLineInfo);
			}
		}

		void SwitchPickLineToTargetSerial(GlbStaff rfGunUser, PkgPackage packageToPackInto, WhsPickLineInfo pickLineInfo, WhsPickLine pickLine, WhsInventoryView newInventory, WhsPickLine pickLineWithScannedSerial)
		{
			var originalPackage = GetOriginalPackageFromPickLineAndDeleteOldDivot(pickLine, pickLineInfo);
			if (pickLineWithScannedSerial != null)
			{
				pickLineWithScannedSerial.WZ_WE_InventoryLine = pickLine.WZ_WE_InventoryLine;
				SwapPackageLinks(pickLineWithScannedSerial, pickLineInfo);

				var transactionLine = (WhsPickableDocketLine)pickLineWithScannedSerial.DocketLine;
				transactionLine?.ReleaseLines.ClearCollection();
			}

			pickLine.WZ_WE_InventoryLine = newInventory.PK; // switch pickline to new inventory
			pickLine.WZ_GS_NKAssignedTo = rfGunUser.GS_Code;
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;

			UpdateReleaseLines(pickLineInfo, pickLine, originalPackage ?? packageToPackInto);
		}

		#region IsSwappable

		bool IsSwappable(WhsInventoryView inventory, WhsPickLine committedPickLine)
		{
			// Serialized stock, should only have 1 unit and max of 1 pick line
			return inventory.WI_TotalUnits > 0 &&
				(committedPickLine == null || (!committedPickLine.IsPickedFromPutawayLocation && committedPickLine.DocketLine.WE_SerialNumber.IsEmpty));
		}

		#endregion

		#region SwapPackageLinks

		void SwapPackageLinks(WhsPickLine pickLineTo, WhsPickLineInfo pickLineInfo)
		{
			var orderLineTo = (WhsPickableDocketLine)pickLineTo.DocketLine;
			orderLineTo.ClearReleaseLines();

			var divot = GetPackageDivot(pickLineTo);
			if (divot != null)
			{
				var packageTo = divot.ParentPackage;
				var releaseLineTo = FindReleaseLineBySerialNumber(orderLineTo, pickLineTo.InventoryLine.WE_SerialNumber);
				divot.DeleteForRepacking(releaseLineTo);

				pickLineTo.ClearReleaseCapturedAttributes(); // must delete this before packing and after removing the divot
															 // Allocate PickLine with swapped Inventory back into Original Package

				packageTo.Pack(pickLineTo, releaseLineTo);
			}
			else
			{
				pickLineTo.ClearReleaseCapturedAttributes();
			}
		}

		#endregion

		#region GetOriginalPackageFromPickLineAndDeleteOldDivot

		PkgPackage GetOriginalPackageFromPickLineAndDeleteOldDivot(WhsPickLine originalPickLine, WhsPickLineInfo pickLineInfo)
		{
			PkgPackage originalPackage = null;

			// look for release line with old serial number
			var orderLine = (WhsPickableDocketLine)originalPickLine.DocketLine;
			var releaseLine = FindReleaseLineBySerialNumber(orderLine, originalPickLine.InventoryLine.WE_SerialNumber);
			var divot = GetPackageDivot(originalPickLine);
			if (divot != null)
			{
				originalPackage = divot.ParentPackage;
				divot.DeleteForRepacking(releaseLine);
			}

			orderLine.ClearReleaseLines();

			return originalPackage;
		}

		PkgPackageItemDivot GetPackageDivot(WhsPickLine pickLine)
		{
			var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, pickLine.PK);
			return Factory.Load<PkgPackageItemDivot>(query).SingleOrDefault();
		}

		#endregion

		#region UpdateReleaseLines

		void UpdateReleaseLines(WhsPickLineInfo pickLineInfo, WhsPickLine pickLine, PkgPackage packageToPackInto)
		{
			var partAttributes = pickLineInfo.PartAttributes;
			if (partAttributes.HasSerialNumberAttribute &&
				(packageToPackInto != null || partAttributes.HasReleaseCapturedAttribute))
			{
				// release capturing - adds release captured attributes to the pickline.
				UpdateReleaseCapturedAttributes(pickLineInfo, pickLine);

				// rf Pick and Pack / pre-packed items
				if (packageToPackInto != null)
				{
					var releaseLine = FindReleaseLineBySerialNumber((WhsPickableDocketLine)pickLine.DocketLine, pickLineInfo.SerialNumber);
					PickLineUpdater.PackPickLineForAttributeNeutral(pickLine, releaseLine, packageToPackInto);
				}
			}
		}

		void UpdateReleaseCapturedAttributes(WhsPickLineInfo pickLineInfo, WhsPickLine pickLine)
		{
			var partAttributes = pickLineInfo.PartAttributes;
			if (partAttributes.HasReleaseCapturedAttribute)
			{
				var partAttrib1 = partAttributes.Attribute1IsReleaseCaptured ? pickLineInfo.Attribute1 : "";
				var partAttrib2 = partAttributes.Attribute2IsReleaseCaptured ? pickLineInfo.Attribute2 : "";
				var partAttrib3 = partAttributes.Attribute3IsReleaseCaptured ? pickLineInfo.Attribute3 : "";
				var serialNumber = partAttributes.IsSerialNumberReleaseCaptured ? pickLineInfo.SerialNumber : "";

				pickLine.SetReleaseCapturedAttributes(partAttrib1, partAttrib2, partAttrib3, serialNumber);
			}
		}

		WhsReleaseLine FindReleaseLineBySerialNumber(WhsPickableDocketLine pickableDocketLine, string serialNum)
			=> pickableDocketLine.ReleaseLines.Cast<WhsReleaseLine>().FirstOrDefault(x => x.SerialNumber.EqualsIgnoringCase(serialNum));

		#endregion

		#region TryToAllocateIntoExistingPickLineWithRemovalFromCollection

		bool TryToAllocateIntoExistingPickLine(WhsPickLineInfo pickLineInfo, GlbStaff rfGunUser, IEnumerable<WhsPickAvailableInventory> availableInventoryList, PkgPackage packageToPackInto, Lazy<WhsPickLine[]> candidatePickLines)
		{
			var pickLine = availableInventoryList.SelectMany(ai => ai.PickLines)
				.SingleOrDefault(pl => IsSerialNumbersMatching(pickLineInfo, pl.Inventory));
			var result = IsExactPickLineOrFromSamePackage(pickLineInfo, pickLine, candidatePickLines);
			if (result)
			{
				if (pickLine.WZ_GS_NKAssignedTo.IsEmpty)
				{
					pickLine.WZ_GS_NKAssignedTo = rfGunUser.GS_Code;
				}
				pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				UpdateReleaseLines(pickLineInfo, pickLine, packageToPackInto);
			}

			return result;
		}

		bool IsExactPickLineOrFromSamePackage(WhsPickLineInfo pickLineInfo, WhsPickLine newPickLine, Lazy<WhsPickLine[]> candidatePickLines)
		{
			var result = newPickLine != null;
			var oldPickLinePK = pickLineInfo.PKs.FirstOrDefault();
			if (oldPickLinePK != Guid.Empty && newPickLine != null && oldPickLinePK != newPickLine.PK.ToGuid())
			{
				var oldPickLine = Factory.Load<WhsPickLine>(oldPickLinePK);
				if (oldPickLine.IsPicked)
				{
					oldPickLine = candidatePickLines.Value.FirstOrDefault(pl => !pl.IsPicked);
				}
				if (oldPickLine != null)
				{
					pickLineInfo.PKs = new Guid[] { oldPickLine.PK.ToGuid() };
					var oldPickLineDivot = GetPackageDivot(oldPickLine);
					var newPickLineDivot = GetPackageDivot(newPickLine);
					result = (oldPickLineDivot == null && newPickLineDivot == null) ||
						(oldPickLineDivot != null && newPickLineDivot != null && oldPickLineDivot.KI_KP_Package == newPickLineDivot.KI_KP_Package);
				}
			}

			return result;
		}

		#endregion

		#region IsSerialNumbersMatching

		bool IsSerialNumbersMatching(WhsPickLineInfo pickLineInfo, WhsInventoryView inventory)
			=> pickLineInfo.PartAttributes.IsSerialNumberUsedByOrganisation && pickLineInfo.SerialNumber.Equals(inventory.WI_SerialNumber, StringComparison.OrdinalIgnoreCase);

		#endregion

		#endregion

		#region LoadPickNoAndOrderRefForPickLineInfos

		void LoadPickNoAndOrderRefForPickLineInfos(IEnumerable<WhsPickLineInfo> pickLineInfosThatCouldNotBePicked)
		{
			if (pickLineInfosThatCouldNotBePicked.Any())
			{
				// Code below tries to find the Order and PickNo for the serial number that couldn't be swapped.
				// It can't use .PKs as that is for the original pick line.
				var queryParams = new ZSqlParameterCollection();
				string rawQuery = GetPickNoAndOrderRefQuery(pickLineInfosThatCouldNotBePicked, queryParams);

				var dynamicCollection = new DynamicBusinessObjectCollection(Factory);
				dynamicCollection.Load(rawQuery, queryParams);

				foreach (DynamicBusinessObject dynamicBizO in dynamicCollection)
				{
					foreach (var pickLine in pickLineInfosThatCouldNotBePicked)
					{
						if (dynamicBizO["ClientPK"].ToString() == pickLine.ClientPK.ToString() &&
							dynamicBizO["ProductPK"].ToString() == pickLine.ProductPK.ToString() &&
							SerialNumberMatchesOnPickLine(pickLine, dynamicBizO))
						{
							pickLine.AttachedToPick = dynamicBizO["PickNo"].ToString();
							pickLine.AttachedToOrder = dynamicBizO["OrderRef"].ToString();
							break;
						}
					}
				}
			}
		}

		bool SerialNumberMatchesOnPickLine(WhsPickLineInfo pickLine, DynamicBusinessObject dynamicBizO)
			=> pickLine.PartAttributes.IsSerialNumberUsedByOrganisation && dynamicBizO["SerialNumber"].ToString() == pickLine.SerialNumber;

		string GetPickNoAndOrderRefQuery(IEnumerable<WhsPickLineInfo> pickLineInfosThatCouldNotBePicked, ZSqlParameterCollection queryParams)
		{
			var result = @"
				select
					WI_OH_Client as ClientPK,
					WI_OP as ProductPK,
					WI_PartAttrib1 as PartAttrib1,
					WI_PartAttrib2 as PartAttrib2,
					WI_PartAttrib3 as PartAttrib3,
					WI_SerialNumber as SerialNumber,
					WP_PickNo as PickNo,
					WD_ExternalReference as OrderRef
				from
					dbo.WhsPickLine
					join dbo.WhsInventoryView on WZ_WE_InventoryLine = WI_WE_InDocketLine
					join dbo.WhsDocketLine on WZ_WE_TransactionLine = WE_PK
					join dbo.WhsDocket on WD_PK = WE_WD
					left join dbo.WhsPick on WP_PK = WD_WP
				where
					-- Filter out In-Transit Transfers and regular transfers for the serialised stock
					WD_DocketType != 'TFR' and
				";

			var isFirstElement = true;
			var i = 0;

			foreach (var pickLineInfo in pickLineInfosThatCouldNotBePicked)
			{
				var clientParam = "@Client" + i.ToString(CultureInfo.InvariantCulture);
				var productParam = "@Product" + i.ToString(CultureInfo.InvariantCulture);
				var serialNumberParam = "@SerialNumber" + i.ToString(CultureInfo.InvariantCulture);
				i++;

				queryParams.Add(clientParam, pickLineInfo.ClientPK, WhsInventoryViewSchema.WI_OH_Client);
				queryParams.Add(productParam, pickLineInfo.ProductPK, WhsInventoryViewSchema.WI_OP);

				if (!isFirstElement)
				{
					result += (NoResString)" or "; // SQL Expression.
				}
				else
				{
					isFirstElement = false;
				}

				result += FormattableString.Invariant($"(WI_OH_Client = {clientParam} and WI_OP = {productParam}");

				if (pickLineInfo.PartAttributes.IsSerialNumberUsedByOrganisation)
				{
					result += FormattableString.Invariant($" and WI_SerialNumber = {serialNumberParam}) AND WI_SerialNumber <> ''"); // SQL Expression.
					queryParams.Add(serialNumberParam, pickLineInfo.SerialNumber, WhsInventoryViewSchema.WI_SerialNumber);
				}
				else
				{
					result += ")";
				}
			}
			return result;
		}

		#endregion

		#endregion

		#endregion
	}
}
