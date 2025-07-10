using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Business.Putaway;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Load unfinalised Pallet infos for logged in user and warehouse")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PutawayMultiplePalletsWebServiceResponse LoadUnfinalisedPutawayPalletInfos()
		{
			return HandleWebServiceRequest_WithValidateWarehouseAndStaff<PutawayMultiplePalletsWebServiceResponse>(r => LoadUnfinalisedPutawayPalletInfos(r));
		}

		void LoadUnfinalisedPutawayPalletInfos(PutawayMultiplePalletsWebServiceResponse response)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			if (staff != null && warehouse != null)
			{
				LoadPalletInfos();
				response.ShowStockOnHandWarningOnPutaway = WarehouseDataRegistry.Instance.SOHLocationWarning.Value;
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service."));
			}

			void LoadPalletInfos()
			{
				var palletInfos = new List<PutawayPalletInfo>();
				var putawayPalletInfoWithLocation = new List<(PutawayPalletInfo PalletInfo, WhsLocation Location)>();
				var putawayLines = LoadOrCreateUnfinalisedJobForUser().Lines;
				if (putawayLines.Any())
				{
					var palletIDs = putawayLines.Select(p => p.WPL_PalletID.ToString());
					var receiveLines = Factory.Load<WhsReceiveLine>(PutawayHelper.FindReceiveLineQuery(palletIDs, warehouse.PK)).DistinctBy(rl => rl.PK);
					AddPutawayTransferFetchHints(receiveLines, Factory);

					var unputawayLines =
						receiveLines
						.GroupBy(r => r.WE_PalletID)
						.Where(g => (g.First().PutawayTransferLine?.WE_OriginalInventoryStatus ?? string.Empty) != InventoryStatus.Codes.Putaway);

					unputawayLines
						.ForEach(g =>
						{
							var referenceLine = g.First();
							var docket = referenceLine.Docket;
							var client = docket.Client;
							var location = referenceLine.DestinationLocation;

							var palletInfo = new PutawayPalletInfo(
								g.Key,
								location?.WLV_LocationString ?? "",
								location?.WLV_LocationString_UserFriendly ?? "",
								location?.FormattedCheckDigit ?? "",
								location?.PK.ToGuid() ?? Guid.Empty,
								client.OH_Code,
								docket.PK.ToGuid(),
								string.Empty,
								string.Empty);

							palletInfo.ConsolidateProductInfos(
								g.Select(line => line.ProductCode.ToString()),
								client.PartAttributeManager.PartAttributeName1,
								g.Select(line => line.WE_PartAttrib1.ToString()),
								client.PartAttributeManager.PartAttributeName2,
								g.Select(line => line.WE_PartAttrib2.ToString()),
								client.PartAttributeManager.PartAttributeName3,
								g.Select(line => line.WE_PartAttrib3.ToString()),
								g.Select(line => line.WE_SerialNumber.ToString()),
								g.Select(line => line.WE_ExpiryDate.IsValid ? line.WE_ExpiryDate.ToDateTime() : DateTime.MinValue),
								g.Select(line => line.WE_PackingDate.IsValid ? line.WE_PackingDate.ToDateTime() : DateTime.MinValue));
							putawayPalletInfoWithLocation.Add((palletInfo, location));
						});

					var notPutawayPallets = putawayPalletInfoWithLocation.Select(p => p.PalletInfo.PalletID).ToHashSet();
					putawayLines.Where(l => notPutawayPallets.Contains(l.WPL_PalletID)).ForEach(l => l.WPL_IsPuttingAway = true);
				}

				if (response.NoError())
				{
					var concurrencyErrorMessage = Res.GetString("9a06a9e1-fa33-4f34-abba-fbfb068772ed", "Another user has changed the Putaway Job while you have been working on it. Please restart the operation and try again.");
					WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
				}

				putawayPalletInfoWithLocation.Sort(new SortPutwayPalletInfoWithLocation());
				palletInfos.AddRange(putawayPalletInfoWithLocation.Select(p => p.PalletInfo));
				response.PalletInfos = palletInfos.ToArray();
			}

			WhsPutawayJob LoadOrCreateUnfinalisedJobForUser()
			{
				var putawayJob = PutawayHelper.LoadUnfinalisedPutawayJob(Factory, warehouse, staff);
				if (putawayJob == null)
				{
					putawayJob = Factory.New<WhsPutawayJob>();
					putawayJob.WPJ_GS_NKUser = staff.GS_Code;
					putawayJob.WPJ_WW_Warehouse = warehouse.PK;
				}
				return putawayJob;
			}
		}
	}
}
