using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public class WhsTransitReceiveTransportationUnitDataObjectReaderForTruck : ShipmentDataObjectReader<WhsItemReceiveTransportationUnit>
	{
		public WhsTransitReceiveTransportationUnitDataObjectReaderForTruck(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer warehouseRow, IColumnIndexer asnRow, Vehicle vehicle)
			: base(dataObject, logger, factory)
		{
			if (!dataObject.IsFromDataSource(DataContextType.GateBooking))
			{
				throw new DataObjectReadFailureException(Res.GetString("7fd74a5b-6935-4ce9-a8e5-402eab4f8507", "Gate Booking Data Source is missing from UXML."));
			}
			AsnRow = asnRow ?? throw new DataObjectReadFailureException(Res.GetString("1c983854-82ab-4906-8fab-b55b806b3190", "ASN is missing."));
			warehouse = warehouseRow;
		}

		readonly IColumnIndexer AsnRow;

		IColumnIndexer warehouse;
		IColumnIndexer Warehouse
		{
			get
			{
				warehouse ??= WarehouseMatchingHelper.GetWarehouse(dataObject, factory, logger);
				if (warehouse == null)
				{
					WarehouseMatchingHelper.ThrowForNoMatchingWarehouse(dataObject, logger);
				}
				return warehouse;
			}
		}

		Vehicle vehicleDO;
		Vehicle VehicleDO
		{
			get
			{
				vehicleDO ??= dataObject.GetVehicle();
				if (vehicleDO == null)
				{
					throw new DataObjectReadFailureException(Res.GetString("05ea6934-dd39-4942-b64f-eb7593ba4484", "Vehicle Information is missing from UXML."));
				}

				return vehicleDO;
			}
		}

		IOrgHeader bookingParty;
		IOrgHeader BookingParty
		{
			get
			{
				bookingParty ??= TransitUniversalHelper.GetBookingParty(factory, logger);
				if (bookingParty == null)
				{
					throw new DataObjectReadFailureException(Res.GetString("b7f7ada6-0c1b-404e-8662-29e3b068e504", "Booking Party is missing from UXML."));
				}
				return bookingParty;
			}
		}

		OrgAddress matchedTransportOrgAddress;
		OrgAddress MatchedTransportOrgAddress
		{
			get
			{
				if (matchedTransportOrgAddress == null)
				{
					var transportOrgAddress = dataObject.GetOrgAddress(nameof(DocAddressType.TransportCompanyDocumentaryAddress)) ?? throw new DataObjectReadFailureException(Res.GetString("04c4c6ce-24d3-41a1-9976-1fa490106611", "Transport Company details are missing from UXML."));
					var gateAddressMatcher = ObjectFactory.New<IGateManagementOrganisationDataObjectReader>(transportOrgAddress, logger, factory);
					matchedTransportOrgAddress = gateAddressMatcher.GetMatched() as OrgAddress;
				}
				return matchedTransportOrgAddress;
			}
		}

		ZString? driverName;
		ZString? DriverName
		{
			get
			{
				return driverName ??= dataObject.VehicleRun.CrewCollection.FirstOrDefault(cc => cc.FullName.HasValue)?.FullName;
			}
		}

		protected override string GetBusinessObjectHumanReadableName(WhsItemReceiveTransportationUnit rtu) => rtu == null ? Res.GetString("99be46fd-8509-4371-a30d-d532ac291853", "Receive Transportation Unit") : rtu.HumanReadableName.ToString();

		public override DataContextType DataContextType => DataContextType.TransitReceiveHeader;

		#region GetExistingBusinessObject

		protected override WhsItemReceiveTransportationUnit GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var isLoosePackage = dataObject.GetContainerNumbers().Count == 0;
			return WhsTransitReceiveTransportationUnitMatchingHelper.GetExistingRTUForGateBooking(
				factory,
				dataObject,
				Warehouse,
				VehicleDO.Registration.Number,
				MatchedTransportOrgAddress,
				isMatchGateMovementBookingUniversalLinkEnabled: isLoosePackage);
		}

		protected override IMatchingBusinessEntityFinder<WhsItemReceiveTransportationUnit> GetCombinedReferenceMatcher() => null;

		#endregion

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(WhsItemReceiveTransportationUnit unit)
		{
			var headerRow = GetColumnIndexer(unit);
			if (!IsNewBO)
			{
				// It has packages with RTU (arrived packages)
				var arrivedPackageQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));
				arrivedPackageQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WRH_TransitReceiveHeader, unit.PK);
				var arrivedPackages = factory.Load<WhsItemPackageState>(arrivedPackageQuery);
				if (arrivedPackages.Any())
				{
					var arrivedPackagesInOrder = arrivedPackages.Select(p => p.Package.KP_PackageID).OrderBy(id => id);
					throw new DataObjectReadFailureException(Res.GetString("5caf3858-80a5-4fb1-a660-e0d9800b5f83", "Package IDs: {0} were received into the Warehouse on Receive Transportation Unit '{1}'. System cannot create a Receive Instruction for this Vehicle.",
						ZString.Join(", ", arrivedPackagesInOrder.ToArray()),
						unit.WRH_VehicleReference));
				}
			}
			else
			{
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_UnitType, TransportUnitTypes.Vehicle);
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber, NumberFountainHelper.GetNextReferenceNumber(factory.BOFactory, Env.NumberFountains.TransitWarehouseReceiveID));
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse, Warehouse.GetValue(WhsWarehouseSchema.PK));
				PopulateUniversalLink(unit);
			}

			SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference, VehicleDO.Registration?.Number);

			if (unit.PackageStates.Any(pkg => !pkg.WPS_UnloadedTime.IsEmpty) &&
				headerRow.GetValue(WhsItemReceiveTransportationUnitSchema.WRH_SignedBy) is var signedBy && signedBy != string.Empty &&
				signedBy != DriverName?.ToString())
			{
				throw new DataObjectReadFailureException(Res.GetString("6e03a1be-a163-4931-a0d7-507759d6ca2e", "Cannot update the Truck Driver Name. Some packages have already been unloaded."));
			}

			SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_SignedBy, DriverName);

			if (!Warehouse.GetValue(WhsWarehouseSchema.WW_DefaultInboundDockDoor).IsEmpty && unit.WRH_WL_StagingLocation.IsEmpty)
			{
				SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_WL_StagingLocation, Warehouse.GetValue(WhsWarehouseSchema.WW_DefaultInboundDockDoor));
			}

			TransitUniversalHelper.CreatePkgPackageAndExtension(unit.PackageJob, logger, factory, VehicleDO.Registration?.Number);

			if (unit.PackageStates.Any(pkg => !pkg.WPS_UnloadedTime.IsEmpty))
			{
				var errorMessage = Res.GetString("dc461c90-8487-4415-b2e7-b279e872d3af", "Cannot update the Truck Transport Company Address. Some packages have already been unloaded.");
				var existingTransportCompanyAddress = unit.DocAddresses?.Where(address => address.DocAddressType == DocAddressType.TransportCompanyDocumentaryAddress).SingleOrDefault();

				var newDepartureAddress = dataObject.OrganizationAddressCollection?.SingleOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.DepartureCFSLocalTransportAddress));
				var newArrivalAddress = dataObject.OrganizationAddressCollection?.SingleOrDefault(address => address.AddressType.ToString() == nameof(DocAddressType.ArrivalCFSLocalTransportAddress));

				if (IsUpdatingAddress(existingTransportCompanyAddress, newDepartureAddress) ||
					IsUpdatingAddress(existingTransportCompanyAddress, newArrivalAddress))
				{
					throw new DataObjectReadFailureException(errorMessage);
				}
			}

			OrgAddressImportHelper.PopulateTransportOrg(dataObject, null, BookingParty as OrgHeader, unit, logger, factory);
			OrgAddressImportHelper.PopulateBillToPartyOrg(dataObject, unit, logger, factory);
			SetValue(headerRow, WhsItemReceiveTransportationUnitSchema.WRH_TransportProviderIsKnown, WhsTransitKnownConsignorHelper.IsTransportCompanyKnown(unit));

			LinkASNAndRTU(unit.PK);
		}

		bool IsUpdatingAddress(JobDocAddress jobDocAddress, OrganizationAddress orgAddress)
		{
			return jobDocAddress != null && orgAddress != null &&
					((jobDocAddress.Address1 != ZString.Empty && jobDocAddress.Address1 != orgAddress.Address1.GetValueOrDefault()) ||
					(jobDocAddress.Address2 != ZString.Empty && jobDocAddress.Address2 != orgAddress.Address2.GetValueOrDefault()));
		}

		void PopulateUniversalLink(WhsItemReceiveTransportationUnit rtu)
		{
			if (dataObject.IsFromGateBooking())
			{
				PopulateUniversalLinkToGateBooking(rtu);
				var isLoosePackagesBooking = dataObject.GetContainerNumbers().Count == 0;
				if (isLoosePackagesBooking)
				{
					PopulateUniversalLinkToGateMovementBooking(rtu);
				}
			}
		}

		void PopulateUniversalLinkToGateMovementBooking(WhsItemReceiveTransportationUnit rtu)
		{
			var firstSubShipmentFromGateBooking = dataObject.GetFirstSubShipmentFromGateBooking();
			if (firstSubShipmentFromGateBooking != null)
			{
				var sourceDataContext = UniversalShipment.GetSourceDataObject(firstSubShipmentFromGateBooking).DataContext;
				var linkCreator = new UniversalJobLinkCreator(rtu.Factory, rtu, null, sourceDataContext, logger, true);
				linkCreator.TryCreateJobLink(DataContextType.GateMovementBooking);

				// Log Booking Confirmed
				var gateMovementBookingNumber = firstSubShipmentFromGateBooking.GetMatchingDataSourceValue(DataContextType.GateMovementBooking) ?? ZString.Empty;
				WhsTransitLogHelper.LogBookingConfirmed(rtu, nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber);
			}
		}

		void PopulateUniversalLinkToGateBooking(WhsItemReceiveTransportationUnit rtu)
		{
			var sourceDataContext = UniversalShipment.GetSourceDataObject(dataObject).DataContext;
			var linkCreator = new UniversalJobLinkCreator(rtu.Factory, rtu, null, sourceDataContext, logger, true);
			linkCreator.TryCreateJobLink(DataContextType.GateBooking);

			// Log Booking Confirmed
			var gatebookingNumber = dataObject.GetMatchingDataSourceValue(DataContextType.GateBooking) ?? ZString.Empty;
			WhsTransitLogHelper.LogBookingConfirmed(rtu, nameof(DataContextType.GateBooking), gatebookingNumber);
		}

		#endregion

		#region Link ASN and RTU

		void LinkASNAndRTU(ZGuid rtuPK)
		{
			if (AsnRow != null)
			{
				var asnPK = AsnRow.GetValue(WhsItemReceiveASNSchema.PK);
				var pivotQuery = new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtuPK);
				pivotQuery.AddToFilter(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPK));

				if (factory.RowFactory.Load(WhsItemReceiveASNRTUPivotSchema.Constants.TableName, pivotQuery).Length == 0)
				{
					var pivotRow = factory.RowFactory.NewRowWithPK(WhsItemReceiveASNRTUPivotSchema.Instance);

					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtuPK);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_WRP_TransitReceiveASN, asnPK);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemCreateTimeUtc, ZDateTime.UtcNow);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemCreateUser, User.InterchangeUserCode);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemLastEditTimeUtc, ZDateTime.UtcNow);
					SetValue(pivotRow, WhsItemReceiveASNRTUPivotSchema.WAR_SystemLastEditUser, User.InterchangeUserCode);
				}
			}
		}

		#endregion
	}
}
