using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region PutawayStockInDockDoorOrPackingStation

		[WebMethod(Description = "Putaway Stock In DockDoor or Packing Station")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PutawayStockInDockDoorOrPackingStationResponse PutawayStockInDockDoorOrPackingStation(Guid jobPk, PickJobType pickJobType, string pickPutawayLocation)
		{
			return HandleWebServiceRequest<PutawayStockInDockDoorOrPackingStationResponse>(r => PutawayStockInDockDoorOrPackingStationCore(jobPk, pickJobType, pickPutawayLocation, r));
		}

		void PutawayStockInDockDoorOrPackingStationCore(Guid jobPk, PickJobType pickJobType, string pickPutawayLocation, PutawayStockInDockDoorOrPackingStationResponse response)
		{
			var location = WebServiceHelper.GetLocationByLocationString(Factory, SecurityHeader.WarehouseCode, response, pickPutawayLocation);
			var pickJobWrapper = CreatePickJobWrapper(jobPk, pickJobType);
			if (pickJobWrapper.IsPickJobLoaded)
			{
				ValidatePickPutawayLocation(location, pickJobWrapper.CheckIfPackingStationIsAllowed(), pickPutawayLocation, response);

				// Inventory may already be putaway to a DDL or PST
				// If so, remainder of job must also be putaway to the same location
				CheckLocationMatchesExpectedLocation(location, pickJobWrapper, response);

				if (response.NoError())
				{
					var isDockDoorLocation = location.IsDockDoorLocation;
					OverrideDockDoorLocationIfRequired(location, isDockDoorLocation, pickJobWrapper, response);
					SetDockDoorPutawayTime(isDockDoorLocation, pickJobWrapper, response);

					var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName).GS_Code;
					var (inTransitLines, packagesToClose) = pickJobWrapper.GetTransferLinesToPutawayAndPackagesToClose(jobPk, pickJobType, location, rfUser, response);

					if (response.Error != ErrorTypes.BusinessValidationError && inTransitLines.Length == 0)
					{
						response.Error = ErrorTypes.WarningOnly;
						response.ErrorMessage = Res.GetString("257bae95-d942-475c-8708-24316c71079d", "No stock found to Put. Either nothing is In-Transit for the current job and staff, or Put has already been completed.");
					}
					else
					{
						var kitPackagesToClose = PutawayOutboundTransferLines(location, inTransitLines, rfUser, response, isClosingPackagesWhenPutToDockDoor: pickJobWrapper.IsClosingPackagesWhenPutToDockDoor);
						if (response.NoError())
						{
							ClosePackagesIfNecessary(packagesToClose.Union(kitPackagesToClose));
							WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => Res.GetString("dcb99e56-d674-4ddf-ad7b-d6a4bb46c510", "While you have been working with this job another user has made changes. Please restart the operation and try again."));
						}
					}
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("0df4ee8d-437a-4891-88e4-576774d4fcad", "Error loading job."));
			}

			void ClosePackagesIfNecessary(IEnumerable<PkgPackage> packagesToClose)
			{
				if (pickJobWrapper.IsClosingPackagesWhenPutToDockDoor && location.IsDockDoorLocation)
				{
					packagesToClose.ForEach(package => package.KP_ClosedTimeUtc = ZDateTime.UtcNow);
				}
			}
		}

		PickJobWrapper CreatePickJobWrapper(Guid jobPk, PickJobType pickJobType)
		{
			return pickJobType switch
			{
				PickJobType.Pick => new PickJobWrapperForPick(Factory, jobPk, Factory.Load<WhsPick>(jobPk)),
				PickJobType.TrolleyJob => new PickJobWrapperForTrolleyJob(Factory, jobPk, Factory.Load<WhsPickTrolleyJob>(jobPk)),
				PickJobType.PickByLabelJob => new PickJobWrapperForPickByLabelJob(Factory, jobPk, Factory.Load<WhsPickByLabelJob>(jobPk)),
				_ => throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Invalid PickJobType {0}", pickJobType))
			};
		}

		void ValidatePickPutawayLocation(WhsLocation location, bool isPackingStationAllowed, string passedLocationString, WebServiceResponse response)
		{
			if (response.NoError())
			{
				if (location == null)
				{
					response.LogBusinessValidationError(Res.GetString("9a33dc83-9a29-488e-90be-6f2875aed0a8", "Location {0} does not exist in warehouse {1}", passedLocationString, WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode).WW_WarehouseNameMultilingual));
				}
				else if (!location.IsDockDoorLocation)
				{
					if (!isPackingStationAllowed)
					{
						response.LogBusinessValidationError(Res.GetString("16ab194f-7332-4d3e-b7b2-7f407984fe76", "Location {0} is not an Outbound Dock Door Location.", location.WLV_LocationString_UserFriendly));
					}
					else if (!location.IsPackingStationLocation)
					{
						response.LogBusinessValidationError(Res.GetString("9bec8d89-c9b5-41a0-a721-9f697fca660c", "Location {0} is not an Outbound Dock Door Location nor a Packing Station Location.", location.WLV_LocationString_UserFriendly));
					}
					else if (location.WLV_LocationStatus != LocationStatus.Codes.Normal)
					{
						response.LogBusinessValidationError(Res.GetString("5ed1f73a-e76b-4211-8c9c-f11ae49dd8be", "Location {0} is an invalid Packing Station Location.", location.WLV_LocationString_UserFriendly));
					}
				}
			}
		}

		void CheckLocationMatchesExpectedLocation(WhsLocation location, PickJobWrapper pickJobWrapper, PutawayStockInDockDoorOrPackingStationResponse response)
		{
			if (response.NoError())
			{
				var assignedPutawayLocation = pickJobWrapper.GetAssignedLocationForJob();
				if (assignedPutawayLocation != null)
				{
					ValidateLocationWithAssignedPutawayLocation(location, assignedPutawayLocation, response);
				}
			}
		}

		void ValidateLocationWithAssignedPutawayLocation(WhsLocation location, WhsLocationInfo assignedPutawayLocation, PutawayStockInDockDoorOrPackingStationResponse response)
		{
			if (assignedPutawayLocation.LocationPK != Guid.Empty)
			{
				if (location.PK != assignedPutawayLocation.LocationPK)
				{
					response.LogBusinessValidationError(Res.GetString("02d7bcf0-7ea7-48e5-aac6-e8b1d8f01d28", "Location {0} is not valid for this job as some inventory has already been putaway to {1}.", location.WLV_LocationString_UserFriendly, assignedPutawayLocation.LocationString_UserFriendly));
					SetExpectedLocationOnResponse(response, assignedPutawayLocation);
				}
			}
			else if (!location.IsPackingStationLocation && assignedPutawayLocation.LocationClass.Equals(LocationClasses.Codes.PST))
			{
				response.LogBusinessValidationError(Res.GetString("ef2fae77-c2c9-449d-a0fc-b4e0d4a2a043", "Location {0} is not valid for this job as some parts of the pick requires a Packing Station.", location.WLV_LocationString_UserFriendly, assignedPutawayLocation.LocationString_UserFriendly));
				SetExpectedLocationOnResponse(response, assignedPutawayLocation);
			}
		}

		void OverrideDockDoorLocationIfRequired(
			WhsLocation location,
			bool isDockDoorLocation,
			PickJobWrapper pickJobWrapper,
			PutawayStockInDockDoorOrPackingStationResponse response)
		{
			if (isDockDoorLocation)
			{
				var currentDDL = pickJobWrapper.GetCurrentDockDoorLocationForJob();
				if (location.PK != currentDDL.LocationPK)
				{
					var allowOverrideErrorMessage = pickJobWrapper.GetAllowPickDockDoorLocationOverride();
					if (!string.IsNullOrEmpty(allowOverrideErrorMessage))
					{
						response.LogBusinessValidationError(allowOverrideErrorMessage);
					}
					else
					{
						pickJobWrapper.OverrideDockDoorLocation(location.PK);
						currentDDL = GetNewDockDoorLocationInfo(location);
						response.LogError(
							ErrorTypes.Information,
							Res.GetString("4160e275-b44e-4374-9af8-c32438fe172a", "Overriding Dock Door Location to {0}.", location.WLV_LocationString_UserFriendly));
					}
					SetExpectedLocationOnResponse(response, currentDDL);
				}
			}

			static WhsLocationInfo GetNewDockDoorLocationInfo(WhsLocation location)
				=> new(location.PK.ToGuid(), location.WLV_LocationString, location.WLV_LocationString_UserFriendly, location.WLV_LocationClass);
		}

		void SetDockDoorPutawayTime(
			bool isDockDoorLocation,
			PickJobWrapper pickJobWrapper,
			PutawayStockInDockDoorOrPackingStationResponse response)
		{
			if (isDockDoorLocation && response.NoError())
			{
				var errorMessage = pickJobWrapper.SetDockDoorAssignmentPutawayTime();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					response.LogBusinessValidationError(errorMessage);
				}
			}
		}

		IEnumerable<PkgPackage> PutawayOutboundTransferLines(WhsLocation location, IEnumerable<WhsTransferLine> inTransitLines, ZString rfUser, WebServiceResponse response, bool isClosingPackagesWhenPutToDockDoor = false)
		{
			AddFetchHintForPutawayTransferLines(inTransitLines);

			var (transferLinesToIgnoreWhenSettingLocation, kitPackages) = WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(Factory, location.PK, inTransitLines.ToArray(), rfUser, isClosingPackagesWhenPutToDockDoor);

			var errorMessage = WhsPackingConsolidationService.PutawayTransferLines(location, inTransitLines, transferLinesToIgnoreWhenSettingLocation);
			if (!string.IsNullOrEmpty(errorMessage))
			{
				response.LogBusinessValidationError(errorMessage);
			}

			return kitPackages;
		}

		void AddFetchHintForPutawayTransferLines(IEnumerable<WhsTransferLine> inTransitLines)
		{
			var transitLinePKs = inTransitLines.Select(l => l.PK).ToArray();
			Factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_MatchingLine, transitLinePKs));
			Factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, transitLinePKs));
			Factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, transitLinePKs));
		}

		static string JobNotFoundErrorMessage => Res.GetString("0df4ee8d-437a-4891-88e4-576774d4fcad", "Error loading job.");

		static void SetExpectedLocationOnResponse(PutawayStockInDockDoorOrPackingStationResponse response, WhsLocationInfo location)
		{
			response.ExpectedLocationPK = location.LocationPK;
			response.ExpectedLocationString = location.LocationString;
			response.ExpectedLocationString_UserFriendly = location.LocationString_UserFriendly;
			response.ExpectedLocationClass = location.LocationClass;
		}

		#endregion
	}
}
