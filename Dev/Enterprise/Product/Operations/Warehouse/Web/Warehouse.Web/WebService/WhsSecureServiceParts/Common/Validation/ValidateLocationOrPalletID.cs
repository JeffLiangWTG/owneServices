using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region ValidateLocationOrPalletID

		[WebMethod(Description = "Validate Location or Pallet ID")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public LocationOrPalletIDWebServiceResponse ValidateLocationOrPalletID(Guid docketPK, string locationOrPalletID, Guid productPK, bool checkIfHaveSOHInTheLocation, bool checkOnlyPalletID)
		{
			return HandleWebServiceRequest<LocationOrPalletIDWebServiceResponse>(r => ValidateLocationOrPalletIDCore(r, docketPK, locationOrPalletID, productPK, checkIfHaveSOHInTheLocation, checkOnlyPalletID));
		}

		void ValidateLocationOrPalletIDCore(LocationOrPalletIDWebServiceResponse response, Guid docketPK, string locationOrPalletID, Guid productPK, bool checkIfHaveSOHInTheLocation, bool checkOnlyPalletID = false)
		{
			if (docketPK == Guid.Empty)
			{
				response.LogBusinessValidationError(Res.GetString("e85835c5-06d0-41f4-a870-ca9cb10e730a", "Please provide a Receipt PK."));
			}
			else if (string.IsNullOrEmpty(locationOrPalletID))
			{
				response.LogBusinessValidationError(Res.GetString("b5647015-380d-4800-ae51-805831d1bdc2", "Please provide a {0}.", checkOnlyPalletID ? Res.GetString("54195891-2d0c-49ce-a0d7-4f77ac0ca256", "Pallet ID") : Res.GetString("8619a52d-d0af-4c08-92a7-61bebcd01271", "Location")));
			}
			else
			{
				var location = WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, locationOrPalletID);
				if (response.NoError())
				{
					if (location != null)
					{
						// location with <locationOrPalletID> exists
						if (checkOnlyPalletID)
						{
							response.LogBusinessValidationError(Res.GetString("2d409c26-ac34-4f43-81d0-e2496ae75a15", "Must enter a valid Pallet ID not a Location."));
						}
						else
						{
							ValidateForLocationDuringUnload(response, docketPK, productPK, location, checkIfHaveSOHInTheLocation);
							ValidateIsDockDoorLocation(response, location);
						}
					}
					else
					{
						// location with <locationOrPalletID> doesn't exist
						if (checkOnlyPalletID)
						{
							ValidateForPalletIDDuringUnload(response, docketPK, locationOrPalletID);
						}
						else
						{
							response.LogBusinessValidationError(Res.GetString("9befb069-73dd-43d5-8a13-c2bf5081b98f", "Must enter a valid Location."));
						}
					}
				}
			}
		}

		void ValidateIsDockDoorLocation(LocationOrPalletIDWebServiceResponse response, WhsLocation location)
		{
			if (location != null && location.IsDockDoorLocation)
			{
				response.ErrorMessage = Res.GetString("e91e9b8c-f464-4181-accf-974a73a10984", "You cannot use a Dock door location.");
			}
		}

		void ValidateForLocationDuringUnload(LocationOrPalletIDWebServiceResponse response, Guid docketPK, Guid productPK, WhsLocation location, bool checkIfHaveSOHInTheLocation)
		{
			SetLocationResponseFields(Factory, response, location);
			response.WarnUserStockOnHandInTheLocationExist = checkIfHaveSOHInTheLocation && CheckStockOnHandExcludingReceipt(docketPK, location);
			response.HasPalletSpaces = location.WLV_PalletFloorSpaces * location.WLV_PalletStackHeight > 0;

			if (productPK != Guid.Empty && response.IsFixed)
			{
				var receive = Factory.Load<WhsReceive>(docketPK);
				var part = Factory.Load<OrgSupplierPart>(productPK);
				if (receive != null && part != null)
				{
					response.ErrorMessage = WhsValidationHelper.GetErrorFixLocationAndProductAssignedToTheLocation(location, receive.Client, part);
				}
			}
		}

		void ValidateForPalletIDDuringUnload(LocationOrPalletIDWebServiceResponse response, Guid docketPK, string locationOrPalletID)
		{
			var inventories = WebServiceHelper.LoadWhsInventoryByPalletID(response, Factory, SecurityHeader.WarehouseCode, locationOrPalletID);
			var inventoriesOnThePallet = WhsInventoryLineInfoCollection.GetWhsInventoryLineInfoCollectionWithFetchHints(inventories);
			if (response.Error == ErrorTypes.None)
			{
				response.PalletID = locationOrPalletID;
				response.InventoriesOnThePallet = inventoriesOnThePallet;
				var receive = Factory.Load<WhsReceive>(docketPK);
				if (receive == null)
				{
					response.LogBusinessValidationError(Res.GetString("b0cac925-ae8b-474b-af8c-83c8dc79df5e", "Receive record could not be found."));
				}
				else
				{
					var validationError = ValidatePalletDuringUnload(receive, locationOrPalletID);
					if (string.IsNullOrEmpty(validationError))
					{
						var jobsWithSamePalletID = LoadAllJobsForSpecifiedPalletID(receive.WD_WW_Whs, locationOrPalletID);

						if (WarehouseDataRegistry.Instance.WarnWhenDuplicatePalletIdScanned.Value && jobsWithSamePalletID.Any(r => r.PK == docketPK))
						{
							response.Error = ErrorTypes.PalletAlreadyUnloaded;
							response.ErrorMessage = Res.GetString("ec584bef-3f37-46b8-b36f-a61b6999861e",
								"Pallet ID {0} is already unloaded.", locationOrPalletID);
						}
						else if (jobsWithSamePalletID.Any(r => r.PK != docketPK))
						{
							response.LogBusinessValidationError(Res.GetString("855947e8-7257-4889-a835-4d4ace262a8a",
								"Pallet ID {0} exists on another Job.", locationOrPalletID));
						}
						else
						{
							var palletIDQuery = new ZQuery(WhsDocketLineSchema.WE_PalletID, locationOrPalletID);
							var isPutawayTransferCreatedForThisPalletID = receive.Lines.Find(palletIDQuery).Cast<WhsReceiveLine>()
								.Any(l => l.HasPutawayTransfer);
							if (isPutawayTransferCreatedForThisPalletID)
							{
								response.LogBusinessValidationError(Res.GetString("347fb511-315f-4b2c-8ced-f8e38ef49e4b",
									"Pallet ID is assigned to a putaway transfer. Use a different Pallet ID."));
							}
						}
					}
					else
					{
						response.LogBusinessValidationError(validationError);
					}
				}
			}
		}

		WhsDocket[] LoadAllJobsForSpecifiedPalletID(ZGuid warehousePK, string palletID)
		{
			var whsInventorySubQuery = new ZDBOnlySubQuery(typeof(WhsInventoryView), WhsDocketSchema.PK, WhsInventoryViewSchema.WI_WD);
			whsInventorySubQuery.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.Equal, palletID);
			whsInventorySubQuery.AddToFilter(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, string.Empty);
			whsInventorySubQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddToFilter(WhsDocketSchema.WD_WW_Whs, warehousePK);
			query.AddSubQuery(whsInventorySubQuery, JoinCondition.And);

			return Factory.Load<WhsDocket>(query);
		}

		string ValidatePalletDuringUnload(WhsReceive receive, string palletId)
		{
			var validationError = WarehouseValidationHelper.ValidatePalletID(palletId);
			if (string.IsNullOrEmpty(validationError) && WarehouseDataRegistry.Instance.TotalPalletsValidation.Value)
			{
				validationError = ValidateTotalPallets(receive, palletId);
			}
			return validationError;
		}

		string ValidateTotalPallets(WhsReceive receive, string palletId)
		{
			var totalPalletsValidationError = string.Empty;
			if (receive.Lines.Any() && receive.AsnLines.Any() && !string.IsNullOrEmpty(palletId))
			{
				var isAlreadyUnloadedPalletID = receive.Lines.Any(l => l.WE_PalletID.EqualsIgnoringCase(palletId));
				if (!isAlreadyUnloadedPalletID && receive.TotalPalletsReceived >= receive.WD_TotalPallets)
				{
					totalPalletsValidationError = Res.GetString("124FCBA1-A844-4453-9BF7-7E340CF914ED",
						"Total number of pallets unloaded is already equal to or more than the expected number of pallets. You cannot unload more pallets.");
				}
			}

			return totalPalletsValidationError;
		}

		#endregion
	}
}
