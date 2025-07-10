using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Manual Putaway Multiple Pallet")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPalletWebServiceResponse ManualPutawayMultiplePallet(string locationString)
		{
			return HandleWebServiceRequest<WhsPalletWebServiceResponse>(result => ManualPutawayMultiplePalletCore(result, locationString));
		}

		void ManualPutawayMultiplePalletCore(WhsPalletWebServiceResponse response, string locationString)
		{
			var location = !string.IsNullOrEmpty(locationString)
				? WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, locationString)
				: null;

			ValidateLocationForPutaway(response, location);

			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var putawayJob = PutawayHelper.LoadUnfinalisedPutawayJob(Factory, warehouse, staff);
			var putawayLines = PutawayHelper.LoadUnfinalisedPutawayLines(Factory, warehouse, staff);

			if (putawayJob == null || putawayLines.IsNullOrEmpty())
			{
				response.LogBusinessValidationError(Res.GetString("015c5ea4-fb1a-41a2-b0bb-9e36a0097d10", "Putaway Job and Putaway Transfer Lines not found."));
			}

			if (response.NoError())
			{
				var palletIDs = putawayLines.Select(x => (x.WPL_PalletID).ToString()).Distinct().ToList();
				var inventoryCollection = GetInventory(palletIDs);
				if (inventoryCollection != null)
				{
					var inventoriesAndReceiveLines = GetReceiveLinesForPutaway(inventoryCollection);
					var putawayTransferLinesToFinalise = GetPutawayTransfersToFinalise(inventoriesAndReceiveLines, response, location).ToArray();
					var receiveLines = inventoriesAndReceiveLines.Select(line => line.ReceiveLine).ToArray();

					ValidateInventoryForBondedOrInwardProcessingLocations(response, location, putawayTransferLinesToFinalise);
					ValidateInvsHasUnfinalisedTransfer(response, putawayTransferLinesToFinalise, palletIDs);
					ValidateMultiplePalletIdsForCrossDock(response, putawayTransferLinesToFinalise);
					CheckLocationCapacity(Factory, response, receiveLines, location);
					if (response.NoError())
					{
						PutawayMultiplePallets(receiveLines, putawayTransferLinesToFinalise);
					}
				}
			}

			static void ValidateMultiplePalletIdsForCrossDock(WhsPalletWebServiceResponse response, IEnumerable<WhsTransferLine> transferLines)
			{
				var groupedTransferLines = transferLines.GroupBy(line => line.WE_PalletID).ToArray();
				foreach (var transferLineGroup in groupedTransferLines)
				{
					CheckIfAllOrNoneAreCrossDocked(response, transferLineGroup, transferLineGroup.Key);
				}
			}

			IEnumerable<WhsInventoryView> GetInventory(List<string> palletIDs)
			{
				var inventoryCollection =
					WebServiceHelper.LoadWhsInventoryByTransferFromPalletIDs(
						response,
						Factory,
						SecurityHeader.WarehouseCode,
						palletIDs,
						true).Where(i => i.WI_InDocketLineType == DocketType.Codes.Transfer);

				return response.NoError() ? inventoryCollection : null;
			}

			void PutawayMultiplePallets(IEnumerable<WhsReceiveLine> receiveLines, IEnumerable<WhsTransferLine> transferLines)
			{
				if (PutawayHelper.AreAllPutawayTransfersUnfinalized(response, transferLines))
				{
					PutawayMultiplePalletsCore();
				}

				void PutawayMultiplePalletsCore()
				{
					var parentTransferLines = transferLines.Where(l => l.WE_WE_MatchingLine.IsEmpty).ToArray();
					if (parentTransferLines.Any())
					{
						var docketWisePutawayTransferLines = parentTransferLines.GroupBy(x => x.Docket);
						foreach (var docketGroup in docketWisePutawayTransferLines)
						{
							ValidateAndFinaliseDocket(response, docketGroup.Key, docketGroup.ToList());
							if (!response.NoError())
							{
								break;
							}
						}
					}

					if (response.NoError())
					{
						putawayJob.WPJ_FinalizedTimeUtc = ZDateTime.UtcNow;
						putawayLines.ForEach(x => x.FinalizeLine());
						var concurrencyErrorMessage = Res.GetString("d596eac0-596e-4e3d-be45-c59145548123", "Another user has changed the putaway job while you have been creating it. Please restart the operation and try again.");
						WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
						response.ReferencesOfReceiveThatCouldBeAutoFinalised = GetAllReceivesToAutoFinalise(receiveLines, parentTransferLines, Factory, staff);
					}
				}
			}
		}

		void ValidateAndFinaliseDocket(WhsPalletWebServiceResponse response, WhsTransfer docket, IEnumerable<WhsTransferLine> docketLines)
		{
			docket.RunPreSaveValidation();
			if (docket.HasErrors)
			{
				response.LogError(ErrorTypes.BusinessValidationError, GetErrorMessageForDocket(docket));
			}
			else
			{
				docket.ValidateAndFinaliseDocketLines(docketLines, false);
				if (docket.HasErrors)
				{
					response.LogError(ErrorTypes.BusinessValidationError, GetErrorMessageForDocket(docket));
				}
			}
		}

		void ValidateInventoryForBondedOrInwardProcessingLocations(WhsPalletWebServiceResponse response, WhsLocation location, IEnumerable<WhsTransferLine> transferLines)
		{
			if (response.NoError())
			{
				if (transferLines.Any(i => (i.TransferFromLocation.IsInBondedArea) != location.IsInBondedArea))
				{
					response.LogBusinessValidationError(Res.GetString("06a96322-1285-4772-94e0-da0d47eb03f0", "You cannot putaway stock 'from a non-bonded area to a bonded area' or 'from a bonded area to a non-bonded area'."));
				}
				else if (location.IsInInwardProcessingArea)
				{
					response.LogBusinessValidationError(Res.GetString("d20cbb01-dbb7-4ca5-aa3b-459bd06f710b", "You cannot putaway stock in an Inward Processing area."));
				}
			}
		}

		void ValidateInvsHasUnfinalisedTransfer(WhsPalletWebServiceResponse response, IEnumerable<WhsTransferLine> transferLines, IEnumerable<string> palletIDs)
		{
			if (response.NoError())
			{
				var loadedPalletIDs = transferLines.Where(line => !line.IsFinalised).Select(i => i.WE_TransferFromPalletId.ToString()).Distinct();
				var palletsWithoutunfinalisedTransfers = Enumerable.Except(palletIDs, loadedPalletIDs, StringComparer.OrdinalIgnoreCase);

				if (palletsWithoutunfinalisedTransfers.Any())
				{
					response.LogBusinessValidationError(Res.GetString("d6b6a1aa-ad63-42be-97a6-aadf9cf043f7", "Pallet ID(s) '{0}' are missing an un-finalized putaway transfer.", string.Join(", ", palletsWithoutunfinalisedTransfers)));
				}
			}
		}

		void ValidateLocationForPutaway(WhsPalletWebServiceResponse response, WhsLocation location)
		{
			if (response.NoError() && location == null)
			{
				response.LogBusinessValidationError(Res.GetString("fe2e3d93-d502-49ea-af4f-bdc87fa773f3", "Invalid location"));
			}
		}
	}
}
