using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Validate Pallet IDs For Multiple Putaway")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PutawayMultiplePalletsWebServiceResponse ValidatePalletIDsForMultiplePutaway(string[] palletIDs, bool isReassigning)
		{
			return HandleWebServiceRequest<PutawayMultiplePalletsWebServiceResponse>(result =>
			{
				var inventoryCollection = WebServiceHelper.LoadWhsInventoryByPalletIDs(result, Factory, SecurityHeader.WarehouseCode, palletIDs);
				if (result.NoError())
				{
					ValidatePalletIDForMultiplePutawayCore(palletIDs, isReassigning, result);
				}
			});
		}

		void ValidatePalletIDForMultiplePutawayCore(string[] palletIDs, bool isReassigning, PutawayMultiplePalletsWebServiceResponse response)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var inventories = LoadInventoriesForPalletIDs(response, warehouse, palletIDs);
			VerifyAllPalletIDsNonBonded(response, inventories);
			PutawayHelper.CheckIfPalletIDsIsHeldForPutaway(response, inventories.ToArray());
			VerifyAllPalletIDsInSameDDL(response, warehouse, staff, palletIDs);
			CheckIfPalletIDsNotOnAnotherPutawayJob(response, warehouse, staff, palletIDs, isReassigning);

			if (response.NoError())
			{
				var inventoriesToPutaway = inventories.Where(inventory => inventory.WI_InDocketLineUnits > 0).ToArray();
				if (inventoriesToPutaway.Any())
				{
					var helper = ObjectFactory.Get<ICreatePutawayTransferHelper>();
					var errorMessage = helper.CreateTransfer(Factory, warehouse, inventoriesToPutaway, true, staff.GS_Code);
					if (!errorMessage.IsNullOrEmpty())
					{
						response.LogError(ErrorTypes.BusinessValidationError, errorMessage);
					}

					UpdatePutawayJobWithPalletIDsAndPopulatePalletInfos(response, warehouse, staff, palletIDs, isReassigning, inventoriesToPutaway);
				}
				else
				{
					response.LogError(ErrorTypes.BusinessValidationError, Res.GetString("D296569B-D267-4883-9DC1-EC35CDB0CFD3", "There are no items to putaway."));
				}
			}
		}

		IEnumerable<WhsInventoryView> LoadInventoriesForPalletIDs(PutawayMultiplePalletsWebServiceResponse response, WhsWarehouse warehouse, string[] palletIDs)
		{
			var inventories = Factory.Load<WhsInventoryView>(PutawayHelper.FindInventoryQuery(palletIDs, warehouse.PK));
			AddInventoryDocketFetchHints(inventories, Factory);
			var notFoundPalletIDs = Enumerable.Except(palletIDs, inventories.Select(i => i.WI_PalletID.ToString()), StringComparer.OrdinalIgnoreCase);
			if (notFoundPalletIDs.Any())
			{
				response.LogBusinessValidationError(
					Res.GetString("ea02b9ed-3f2d-4129-8b79-5264fc7d7a7b", "The Pallet ID(s) '{0}' do not exist on an Un-Finalized Receipt.", string.Join(", ", notFoundPalletIDs)));
			}

			return inventories;
		}

		static void VerifyAllPalletIDsNonBonded(PutawayMultiplePalletsWebServiceResponse response, IEnumerable<WhsInventoryView> inventories)
		{
			if (response.NoError())
			{
				var bondedInventory = inventories.FirstOrDefault(i => i.Docket.WD_DocketSubType == ReceiveType.Codes.Customs && !i.IsReceivedIntoDockDoor);
				if (bondedInventory != null)
				{
					response.LogBusinessValidationError(Res.GetString("5ebc23e7-ee99-44b2-be71-ce9bfccc163f", "Bonded Pallet '{0}' should be unloaded on the desktop.", bondedInventory.WI_PalletID));
				}
			}
		}

		#region VerifyAllPalletIDsInSameDDL

		void VerifyAllPalletIDsInSameDDL(PutawayMultiplePalletsWebServiceResponse response, WhsWarehouse warehouse, GlbStaff staff, string[] palletIDs)
		{
			if (response.NoError())
			{
				var putawayLines = PutawayHelper.LoadUnfinalisedPutawayLines(Factory, warehouse, staff);
				var palletIDsToCheck = putawayLines.Select(l => l.WPL_PalletID.ToString()).ToList();
				palletIDsToCheck.AddRange(palletIDs);

				var palletID_DDLs = GetPalletIDDockdoorLocations(palletIDsToCheck, warehouse.PK);
				var existingDDLPk = palletID_DDLs.FirstOrDefault(l => l.LocPK != ZGuid.Empty).LocPK;
				var (palletID, locPK) = palletID_DDLs.FirstOrDefault(i => i.LocPK != ZGuid.Empty && i.LocPK != existingDDLPk);
				if (!string.IsNullOrEmpty(palletID) && locPK != ZGuid.Empty)
				{
					response.LogBusinessValidationError(Res.GetString("b127c37f-8ad2-4069-8962-7e4a75cb0bfd", "The pallet ID '{0}' is not in the same dock door as those previously scanned and those on the current Putaway Job.", palletID));
				}
			}
		}

		IEnumerable<(ZString PalletID, ZGuid LocPK)> GetPalletIDDockdoorLocations(List<string> palletIDsToCheck, ZGuid whsPK)
		{
			var receiveLines =
				Factory.Load<WhsReceiveLine>(PutawayHelper.FindReceiveLineQuery(palletIDsToCheck, whsPK))
					.DistinctBy(rl => rl.PK)
					.Where(rl => rl.WE_TransactionQuantity > 0);
			return receiveLines.Select(rl => (rl.WE_PalletID, rl.WE_WL));
		}

		#endregion

		void CheckIfPalletIDsNotOnAnotherPutawayJob(PutawayMultiplePalletsWebServiceResponse response, WhsWarehouse warehouse, GlbStaff staff, string[] palletIDs, bool isReassigning)
		{
			if (response.NoError())
			{
				var lines = PutawayLinesFromDifferentJobWithSameIDAsScannedPalletIDs(warehouse, staff, palletIDs);
				PutawayHelper.CheckIfPalletIDsNotOnAnotherPutawayJob(response, lines, isReassigning);
			}
		}

		#region PutawayLinesFromDifferentJobWithSameIDAsScannedPalletIDs

		IEnumerable<WhsPutawayLine> putawayLinesFromDifferentJobWithSameID;
		IEnumerable<WhsPutawayLine> PutawayLinesFromDifferentJobWithSameIDAsScannedPalletIDs(WhsWarehouse warehouse, GlbStaff staff, string[] palletIDs)
			=> putawayLinesFromDifferentJobWithSameID ?? (putawayLinesFromDifferentJobWithSameID = PutawayHelper.LoadPutawayLinesFromDifferentJobsWithMatchingPalletIDs(Factory, warehouse, staff, palletIDs));

		#endregion

		#region CreateAndOrUpdatePutawayJobs

		void UpdatePutawayJobWithPalletIDsAndPopulatePalletInfos(PutawayMultiplePalletsWebServiceResponse response, WhsWarehouse warehouse, GlbStaff staff, string[] palletIDs, bool isReassigning, WhsInventoryView[] inventoriesToPutaway)
		{
			if (response.NoError())
			{
				if (isReassigning)
				{
					UpdateAssociatedPutawayTransferLinesPutawayBy(inventoriesToPutaway, staff);
					DeleteNotPuttingAwayLinesForPalletIDs(warehouse, staff, palletIDs);
				}

				var putawayJob = PutawayHelper.LoadUnfinalisedPutawayJob(Factory, warehouse, staff) ?? PutawayHelper.CreatePutawayJob(Factory, warehouse, staff);
				var invGroupedByPalletID = FindInventoriesRequiringPutaway(inventoriesToPutaway, putawayJob);

				// Select Arbitrary inventory per unique pallet id
				PutawayHelper.CreatePutawayLinesOnPutawayJob(warehouse, staff, putawayJob, invGroupedByPalletID.Select(inv => inv.First()));
				PopulatePalletInfos(response, invGroupedByPalletID);

				var concurrencyErrorMessage = Res.GetString("785c9275-97f6-45d9-bfb1-4bf6758f9378", "Another user has changed the putaway job while you have been working on it. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		void UpdateAssociatedPutawayTransferLinesPutawayBy(IEnumerable<WhsInventoryView> inventories, GlbStaff staff)
		{
			var putawayTransferLines = inventories.Select(inv => PutawayHelper.GetPutawayTransferLineFromInventory(inv)).WhereNotNull();
			putawayTransferLines.ForEach(tl => tl.WE_GS_NKPutawayBy = staff.GS_Code);
		}

		void DeleteNotPuttingAwayLinesForPalletIDs(WhsWarehouse warehouse, GlbStaff staff, string[] palletIDs)
		{
			var putawayLines = PutawayLinesFromDifferentJobWithSameIDAsScannedPalletIDs(warehouse, staff, palletIDs);
			PutawayHelper.DeleteLinesWhichAreNotPuttingAway(putawayLines);
		}
		IEnumerable<IGrouping<ZString, WhsInventoryView>> FindInventoriesRequiringPutaway(WhsInventoryView[] inventories, WhsPutawayJob putawayJob)
		{
			var palletIDsOnJob = putawayJob.Lines.Select(l => l.WPL_PalletID).ToHashSet();
			return inventories.GroupBy(i => i.WI_PalletID).Where(g => !palletIDsOnJob.Contains(g.Key));
		}

		void PopulatePalletInfos(PutawayMultiplePalletsWebServiceResponse response, IEnumerable<IGrouping<ZString, WhsInventoryView>> invsForCreateLines)
		{
			var palletInfos = new List<PutawayPalletInfo>();
			foreach (var invGroup in invsForCreateLines)
			{
				var referenceInv = invGroup.First();
				var referenceDocketLine = (WhsReceiveLine)referenceInv.InDocketLine;
				var client = referenceInv.Client;

				var location = referenceDocketLine.DestinationLocation;
				var palletInfo = new PutawayPalletInfo(
					invGroup.Key,
					location?.WLV_LocationString ?? "",
					location?.WLV_LocationString_UserFriendly ?? "",
					location?.FormattedCheckDigit ?? "",
					referenceInv.Client.OH_Code,
					referenceInv.WI_WD.ToGuid());

				palletInfo.ConsolidateProductInfos(
					invGroup.Select(inv => inv.WI_OP_PartNum.ToString()),
					client.PartAttributeManager.PartAttributeName1,
					invGroup.Select(inv => inv.WI_PartAttrib1.ToString()),
					client.PartAttributeManager.PartAttributeName2,
					invGroup.Select(inv => inv.WI_PartAttrib2.ToString()),
					client.PartAttributeManager.PartAttributeName3,
					invGroup.Select(inv => inv.WI_PartAttrib3.ToString()),
					invGroup.Select(inv => inv.WI_SerialNumber.ToString()),
					invGroup.Select(inv => inv.WI_ExpiryDate.IsValid ? inv.WI_ExpiryDate.ToDateTime() : DateTime.MinValue),
					invGroup.Select(inv => inv.WI_PackingDate.IsValid ? inv.WI_PackingDate.ToDateTime() : DateTime.MinValue));
				palletInfos.Add(palletInfo);
			}

			response.PalletInfos = palletInfos.ToArray();
		}

		#endregion
	}
}
