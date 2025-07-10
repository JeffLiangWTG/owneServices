using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsItemDispatchTransportationUnitDataObjectReaderForTruck : ShipmentDataObjectReader<WhsItemDispatchTransportationUnit>
	{
		public override DataContextType DataContextType => DataContextType.TransitDispatchHeader;

		public WhsItemDispatchTransportationUnitDataObjectReaderForTruck(IColumnIndexer loadList, IColumnIndexer warehouseFromConsol, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
			LoadList = Argument.NotNull(loadList, nameof(loadList));
			WarehouseFromConsol = Argument.NotNull(warehouseFromConsol, nameof(warehouseFromConsol));
		}

		protected override string GetBusinessObjectHumanReadableName(WhsItemDispatchTransportationUnit dtu) => dtu == null ? Res.GetString("cf401f69-bf39-42a7-be48-e30e4332e413", "Dispatch Transportation Unit") : dtu.HumanReadableName.ToString();

		readonly IColumnIndexer LoadList;
		readonly IColumnIndexer WarehouseFromConsol;

		Vehicle Vehicle
		{
			get
			{
				vehicle ??= dataObject.GetVehicle();
				return vehicle;
			}
		}
		Vehicle vehicle;

		#region CombinedReferenceMatcher

		protected override IMatchingBusinessEntityFinder<WhsItemDispatchTransportationUnit> GetCombinedReferenceMatcher()
		{
			return null;
		}

		#endregion

		#region ExistingBusinessObjectUsingModuleSpecificBusinessRules

		protected override WhsItemDispatchTransportationUnit GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			WhsItemDispatchTransportationUnit dtu = null;
			if (dataObject.IsFromGateBooking())
			{
				if (Vehicle == null)
				{
					throw new DataObjectReadFailureException(Res.GetString("b0998daa-8bfd-4a76-bead-a472b2a2beb5", "Vehicle info is missing from UXML."));
				}
				var vehicleReference = Vehicle.Registration?.Number;
				if (vehicleReference.HasValue && !vehicleReference.Value.IsEmpty)
				{
					var isLoosePackage = dataObject.GetContainerNumbers().Count == 0;
					dtu = WhsTransitDispatchTransportationUnitMatchingHelper.GetExistingDTUForGateBooking(factory, dataObject, vehicleReference.Value, WarehouseFromConsol, logger, isMatchGateMovementBookingUniversalLinkEnabled: isLoosePackage);
				}
			}
			else
			{
				dtu = WhsTransitDispatchTransportationUnitMatchingHelper.GetExistingDTUByRunSheetNumber(factory, dataObject, WarehouseFromConsol);
			}
			return dtu;
		}

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsItemDispatchTransportationUnit header)
		{
			var headerRow = GetColumnIndexer(header);
			if (PivotDoesNotExist(header))
			{
				var pivot = factory.New<WhsItemDispatchLoadListDTUPivot>();
				pivot.WLD_WDL_TransitDispatchLoadList = LoadList.GetValue(WhsItemDispatchLoadListSchema.PK);
				pivot.WLD_WDH_TransitDispatchTransportationUnit = header.PK;
			}
			headerRow.SetValue(WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse, WarehouseFromConsol.GetValue(WhsWarehouseSchema.PK));
			if (IsNewBO)
			{
				headerRow.SetValue(WhsItemDispatchTransportationUnitSchema.WDH_ReferenceNumber, NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseDispatchID));
				PopulateUniversalLink(header);
			}

			if (Vehicle != null)
			{
				SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference, Vehicle.Registration?.Number);
				var driverName = dataObject.GetDriverName();
				SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_SignedBy, driverName);
			}
			else
			{
				SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_VehicleReference, dataObject.VoyageFlightNo);
			}
			var bookingParty = TransitUniversalHelper.GetBookingParty(factory, logger) as OrgHeader;
			OrgAddressImportHelper.PopulateTransportOrg(dataObject, null, bookingParty, header, logger, factory);
			OrgAddressImportHelper.PopulateBillToPartyOrg(dataObject, header, logger, factory);

			SetValue(headerRow, WhsItemDispatchTransportationUnitSchema.WDH_UnitType, TransportUnitTypes.Vehicle);
			TransitUniversalHelper.CreatePkgPackageAndExtension(header.PackageJob, logger, factory, dataObject.VoyageFlightNo);

			PopulateAdditionalReferences(header);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		bool PivotDoesNotExist(WhsItemDispatchTransportationUnit header)
		{
			var query = new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, header.PK);
			query.AddToFilter(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDL_TransitDispatchLoadList, LoadList.GetValue(WhsItemDispatchLoadListSchema.PK));

			return factory.BOFactory.GetDatabaseCount(typeof(WhsItemDispatchLoadListDTUPivot), query) == 0
				&& factory.LoadTop1<WhsItemDispatchLoadListDTUPivot>(query) == null;
		}

		void PopulateUniversalLink(WhsItemDispatchTransportationUnit dtu)
		{
			if (dataObject.IsFromGateBooking())
			{
				PopulateUniversalLinkToGateBooking(dtu);
				var isLoosePackagesBooking = dataObject.GetContainerNumbers().Count == 0;
				if (isLoosePackagesBooking)
				{
					PopulateUniversalLinkToGateMovementBooking(dtu);
				}
			}
		}

		void PopulateUniversalLinkToGateMovementBooking(WhsItemDispatchTransportationUnit dtu)
		{
			var firstSubShipmentFromGateBooking = dataObject.GetFirstSubShipmentFromGateBooking();
			if (firstSubShipmentFromGateBooking != null)
			{
				var subShipmentSourceDataContext = Shipment.GetSourceDataObject(firstSubShipmentFromGateBooking).DataContext;
				var linkCreator = new UniversalJobLinkCreator(dtu.Factory, dtu, null, subShipmentSourceDataContext, logger, true);
				linkCreator.TryCreateJobLink(DataContextType.GateMovementBooking);

				// Log Booking Confirmed
				var gateMovementBookingNumber = firstSubShipmentFromGateBooking.GetMatchingDataSourceValue(DataContextType.GateMovementBooking) ?? ZString.Empty;
				WhsTransitLogHelper.LogBookingConfirmed(dtu, nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
			}
		}

		void PopulateUniversalLinkToGateBooking(WhsItemDispatchTransportationUnit dtu)
		{
			var sourceDataContext = Shipment.GetSourceDataObject(dataObject).DataContext;
			var linkCreator = new UniversalJobLinkCreator(dtu.Factory, dtu, null, sourceDataContext, logger, true);
			linkCreator.TryCreateJobLink(DataContextType.GateBooking);

			// Log Booking Confirmed
			var gatebookingNumber = dataObject.GetMatchingDataSourceValue(DataContextType.GateBooking) ?? ZString.Empty;
			WhsTransitLogHelper.LogBookingConfirmed(dtu, nameof(DataContextType.GateBooking), gatebookingNumber);
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(WhsItemDispatchTransportationUnit unit)
		{
			var referenceHelper = new WhsTransitAdditionalReferencesHelper(logger, factory);
			var additionalReferences = new List<TransitAdditionalReferenceInfo>();

			var runSheetDataSource = dataObject.GetMatchingDataSource(DataContextType.TransportConsignmentRunSheet);
			referenceHelper.CollectTransitAdditionalReferenceInfo(additionalReferences, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, runSheetDataSource?.Key);
			new TransitWarehouseCusEntryNumReferenceCollectionReader(additionalReferences.ToArray(), unit, logger, factory).ReadIntoCollectionRetainingUnmatchedElements();
		}

		#endregion

	}
}
