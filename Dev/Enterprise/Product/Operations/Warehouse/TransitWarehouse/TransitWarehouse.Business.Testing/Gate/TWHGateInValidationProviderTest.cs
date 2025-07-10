using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using static Enterprise.Core.Constants.GateManagementConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TWHGateInValidationProviderTest : TestCaseWithFactory
	{
		#region Mandatory Fields

		public void TestMandatoryFields_NoGateBookingDataSource()
		{
			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, null, TransitConstants.TransportDirection.Delivery);
			var validationProvider = new TWHGateInValidationProvider();
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Gate booking number must be provided", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ZString.Empty, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Gate booking number must be provided", errors[0]);
		}

		public void TestMandatoryFields_NoGateMovementBookingDataSource()
		{
			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, null, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var validationProvider = new TWHGateInValidationProvider();
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Gate movement booking number must be provided", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ZString.Empty, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Gate movement booking number must be provided", errors[0]);
		}

		public void TestMandatoryFields_NoValidDirection()
		{
			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, null);
			var validationProvider = new TWHGateInValidationProvider();
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Transport direction must be either 'PIC' or 'DLV'", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, "AAA");
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Transport direction must be either 'PIC' or 'DLV'", errors[0]);
		}

		public void TestMandatoryFields_NoArrivalCFSAddress()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(null, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Arrival CFS Address must be provided", errors[0]);

			movementShipment = CreateShipment(new OrganizationAddress(), DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Arrival CFS Address must be provided", errors[0]);

			movementShipment = CreateShipment(new OrganizationAddress() { AddressShortCode = "WH1ORGA", OrganizationCode = ZString.Empty }, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Arrival CFS Address must be provided", errors[0]);

			movementShipment = CreateShipment(new OrganizationAddress() { AddressShortCode = ZString.Empty, OrganizationCode = "WH1111BNE" }, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Arrival CFS Address must be provided", errors[0]);
		}

		public void TestMandatoryFields_NoDepartureCFSAddress()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(ArrivalCFSAddress, null, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Departure CFS Address must be provided", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, new OrganizationAddress(), ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Departure CFS Address must be provided", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, new OrganizationAddress() { AddressShortCode = "WH2ORGA", OrganizationCode = ZString.Empty }, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Departure CFS Address must be provided", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, new OrganizationAddress() { AddressShortCode = ZString.Empty, OrganizationCode = "WH2111BNE" }, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Departure CFS Address must be provided", errors[0]);
		}

		#endregion

		#region EntityMatching

		public void TestEntityMatching_CannotFindArrivalWarehouse()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(new OrganizationAddress() { AddressType = nameof(DocAddressTypes.Codes.ArrivalCFSAddress), OrganizationCode = "AAA", AddressShortCode = "BBB" }, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Cannot find arrival warehouse", errors[0]);
		}

		public void TestEntityMatching_CannotFindDepartureWarehouse()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(ArrivalCFSAddress, new OrganizationAddress() { AddressType = nameof(DocAddressTypes.Codes.DepartureCFSAddress), OrganizationCode = "AAA", AddressShortCode = "BBB" }, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Cannot find departure warehouse", errors[0]);
		}

		public void TestEntityMatching_CannotFindRTU()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, "GBM00000000", ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Matching RTU by GateMovementBooking not found", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, "GB00000000", TransitConstants.TransportDirection.Delivery);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Matching RTU by GateBooking not found", errors[0]);
		}

		public void TestEntityMatching_CannotFindDTU()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, "GBM00000000", ValidGateBookingNumber, TransitConstants.TransportDirection.Pickup);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Matching DTU by GateMovementBooking not found", errors[0]);

			movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, "GB00000000", TransitConstants.TransportDirection.Pickup);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("Matching DTU by GateBooking not found", errors[0]);
		}

		#endregion

		#region AlreadyGatedIn

		public void TestRTUAlreadyGatedIn()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var rtu = Factory.LoadTop1<WhsItemReceiveTransportationUnit>(new ZQuery());
			rtu.WRH_GateInTime = ZDateTimeOffset.Today;
			Factory.Save();

			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("RTU - 'TR00000001' is already gated into the warehouse", errors[0]);
		}

		public void TestDTUAlreadyGatedIn()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var dtu = Factory.LoadTop1<WhsItemDispatchTransportationUnit>(new ZQuery());
			dtu.WDH_GateInTime = ZDateTimeOffset.Today;
			Factory.Save();

			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Pickup);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("DTU - 'TD00000001' is already gated into the warehouse", errors[0]);
		}

		#endregion

		#region UNDG Validation

		public void TestUNDGValidationFailed()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(0, errors.Length);

			var warehouseUNDGLimit = Factory.LoadTop1<WhsUNDGLimit>(new ZQuery());
			warehouseUNDGLimit.WWD_TotalWeightLimit = 10m;
			Factory.Save();

			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(1, errors.Length);
			AssertEquals("DG 5678 would put the whs. at 300% weight capacity.", errors[0]);
		}

		#endregion

		#region NoErrors

		public void TestNoErrors()
		{
			var validationProvider = new TWHGateInValidationProvider();

			var movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Delivery);
			var errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(0, errors.Length);

			movementShipment = CreateShipment(ArrivalCFSAddress, DepartureCFSAddress, ValidGateMovementBookingNumber, ValidGateBookingNumber, TransitConstants.TransportDirection.Pickup);
			errors = validationProvider.ValidateMovement(movementShipment, FacilityValidationTypes.GateIn);
			AssertEquals(0, errors.Length);
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			var arrivalCFS = Helper.CreateWarehouse("WH1");
			var location1 = Helper.CreateLocation(arrivalCFS);
			var rtu = Helper.CreateReceiveTransportationUnit("TR00000001", arrivalCFS.PK, location1.PK);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, ValidGateMovementBookingNumber, nameof(DataContextType.GateMovementBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, ValidGateBookingNumber, nameof(DataContextType.GateBooking));
			var asn = Helper.CreateReceiveASN("TRT00000002", arrivalCFS.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);
			var rcn = Helper.CreateReceiveConsignment("RC00000001", arrivalCFS.PK);
			var package = Helper.CreatePackage(Helper.CreatePackageJob(rcn)) as PkgPackage;
			Helper.CreatePackageState(package, "BKD", rcn, receiveASN: asn);
			var dg = Helper.CreateUNDGSubstance("5678", "3", "5678");
			Helper.CreateUNDGDataItem(package.PK, package.TablePrefix, dg, 30m, 0m);
			Helper.CreateWhsUNDGLimit(arrivalCFS, "5678", totalWeightLimit: 100m);
			arrivalCFS.WW_IsDangerousGoodsManagementEnabled = true;

			var departureCFS = Helper.CreateWarehouse("WH2");
			var dtu = Helper.CreateDispatchTransportationUnit("TD00000001", departureCFS.PK);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, ValidGateMovementBookingNumber, nameof(DataContextType.GateMovementBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, ValidGateBookingNumber, nameof(DataContextType.GateBooking));

			Factory.Save();

			var arrivalCFSAddress = Factory.Load<OrgAddress>(arrivalCFS.WW_OA_WarehouseAddress);
			arrivalCFSAddress.OA_Code = "WH1ORGA";
			var departureCFSAddress = Factory.Load<OrgAddress>(departureCFS.WW_OA_WarehouseAddress);
			departureCFSAddress.OA_Code = "WH2ORGA";
			Factory.Save();
		}

		UniversalShipment CreateShipment(OrganizationAddress arrivalCFSAddress, OrganizationAddress departureCFSAddress, ZString? gateMovementBookingNumber, ZString? gateBookingNumber, ZString? direction)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataSource(DataContextType.GateBooking, ValidGateBookingNumber);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>() { });
			if (arrivalCFSAddress != null)
			{
				shipment.OrganizationAddressCollection.Add(arrivalCFSAddress);
			}
			if (departureCFSAddress != null)
			{
				shipment.OrganizationAddressCollection.Add(departureCFSAddress);
			}

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.DataContext = DataContextFactory.New();
			if (gateMovementBookingNumber.HasValue)
			{
				subShipment.DataContext.AddDataSource(DataContextType.GateMovementBooking, gateMovementBookingNumber.Value);
			}
			if (gateBookingNumber.HasValue)
			{
				subShipment.DataContext.AddDataSource(DataContextType.GateBooking, gateBookingNumber.Value);
			}
			if (direction.HasValue)
			{
				subShipment.TransportBookingDirection = new TransportBookingDirection { Code = direction.Value };
			}

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment });
			return shipment;
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		OrganizationAddress ArrivalCFSAddress => new OrganizationAddress() { AddressType = nameof(DocAddressTypes.Codes.ArrivalCFSAddress), AddressShortCode = "WH1ORGA", OrganizationCode = "WH1111BNE" };
		OrganizationAddress DepartureCFSAddress => new OrganizationAddress() { AddressType = nameof(DocAddressTypes.Codes.DepartureCFSAddress), AddressShortCode = "WH2ORGA", OrganizationCode = "WH2111BNE" };

		ZString ValidGateMovementBookingNumber => "GBM00000827";
		ZString ValidGateBookingNumber => "GB00003753";

		#endregion
	}
}
