using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Testing.GUI
{
	class TransitWarehouseIntegrationTest : BaseRatingIntegrationTest
	{
		const string FumigationCargeDescription = "Service Job Fumigation Charge";
		const string PackageWeightCargeDescription = "Package Weight Charge";
		const string PackageVolumeCargeDescription = "Package Volume Charge";
		const string PackageQuantityCargeDescription = "Package Quantity Charge";
		const string PacktypeCargeDescription = "Packtype Charge";

		#region TestRatingOnPacktype_Consignment

		public void TestRatingOnPacktype_DispatchConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TRW, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 20m);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";

			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 70m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT + 1 Carton @ AUD 20.00/CTN"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, dcn, bookedBy);
		}

		public void TestRatingOnPacktype_ReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TRW, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceive);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 20m);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 70m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
70.00 AUD		1 Pallet @ AUD 50.00/PLT + 1 Carton @ AUD 20.00/CTN"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		public void TestRatingOnJobService_DispatchConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWFUM", RatingConstants.RateCategory.TRW, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch, "FUM");
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 10;

			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var service = Helper.CreateJobService(dcn.PK, dcn.TablePrefix, "FUM", ZDateTime.Now);
			service.ES_ServiceId = "Service1";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = "TRWFUM: Base Rate AUD 10.00 (Service ID Service1)"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, dcn, bookedBy);
		}

		public void TestRatingOnJobService_ReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWFUM", RatingConstants.RateCategory.TRW, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceive, "FUM");
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 10;

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var service = Helper.CreateJobService(rcn.PK, rcn.TablePrefix, "FUM", ZDateTime.Now);
			service.ES_ServiceId = "Service1";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = "TRWFUM: Base Rate AUD 10.00 (Service ID Service1)"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		public void TestRatingOnPacktype_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 20m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";

			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 70m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT + 1 Carton @ AUD 20.00/CTN"
				},
			};
			AutorateAndAssert(expected, dtu, localClient);
		}

		public void TestRatingOnPacktype_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 20m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 70m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT + 1 Carton @ AUD 20.00/CTN"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		#endregion

		#region TestRatingOnPackageQty_Consignment

		public void TestRatingOnPackageQty_DispatchConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var chargeCode = CreateChargeCode("TRWQTY", RatingConstants.RateCategory.TRW, PackageQuantityCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TRW, warehouse, 10m, RatingConstants.Units.PK);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";

			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Package(s) @ AUD 10.00/Package"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, dcn, bookedBy);
		}

		public void TestRatingOnPackageQty_ReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var chargeCode = CreateChargeCode("TRWQTY", RatingConstants.RateCategory.TRW, PackageQuantityCargeDescription, ChargeCodeGroupList.Codes.TRWReceive);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TRW, warehouse, 10m, RatingConstants.Units.PK);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
20.00 AUD		2 Package(s) @ AUD 10.00/Package"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		public void TestRatingOnPackageQty_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWQTY", RatingConstants.RateCategory.TWU, PackageQuantityCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TWU, warehouse, 10m, RatingConstants.Units.PK);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";

			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Package(s) @ AUD 10.00/Package"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);
		}

		public void TestRatingOnPackageQty_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWQTY", RatingConstants.RateCategory.TWU, PackageQuantityCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TWU, warehouse, 10m, RatingConstants.Units.PK);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 2 Package(s) @ AUD 10.00/Package"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		#endregion

		#region TestRatingOnCondition

		public void TestRatingOnCondition_ReceiveConsignment() => TestRatingOnCondition_Core<WhsItemReceiveConsignment>(RatingConstants.RateCategory.TRW, ChargeCodeGroupList.Codes.TRWReceive);
		public void TestRatingOnCondition_DispatchConsignment() => TestRatingOnCondition_Core<WhsItemDispatchConsignment>(RatingConstants.RateCategory.TRW, ChargeCodeGroupList.Codes.TRWDispatch);
		public void TestRatingOnCondition_ReceiveTransportationUnit() => TestRatingOnCondition_Core<WhsItemReceiveTransportationUnit>(RatingConstants.RateCategory.TWU, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
		public void TestRatingOnCondition_DispatchTransportationUnit() => TestRatingOnCondition_Core<WhsItemDispatchTransportationUnit>(RatingConstants.RateCategory.TWU, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);

		void TestRatingOnCondition_Core<RatingParentType>(string rateCategory, string chargeCodeGroup) where RatingParentType : class, IJobHeaderParent, IBusiness
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWPKG", rateCategory, PacktypeCargeDescription, chargeCodeGroup);
			var rateEntry = Helper.CreateRateEntry(clientRate, rateCategory, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);
			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "<Packages.Count>&gt;=3";

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 20m);

			var rateLine2 = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);
			rateLine2.TL_Condition = RateLineConditions.UserDefined;
			rateLine2.TL_ConditionalExpression = "<Packages.Count>&lt;3";

			var calculator2 = rateLine2.Calculator;
			calculator2.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 25m);
			calculator2.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 10m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, bookedBy.MainAddress);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now.AddDays(-1);
			dtu.WDH_GateOutTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, bookedBy.MainAddress);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG2", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var arrivedPackageState3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG3", TransitWarehouseStatuses.Codes.Departed, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, Constants.PkgUnit.Carton, "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);

			Factory.Save();

			var testedParent = new object[] { rcn, dcn, rtu, dtu }.Single(e => e is RatingParentType) as RatingParentType;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 90m,
					RevenueCalculationDescription = rcn is RatingParentType ?
chargeCode.AC_Code + @"
Receive Consignment RC1
90.00 AUD		1 Pallet @ AUD 50.00/PLT + 2 Carton @ AUD 20.00/CTN" :
chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT + 2 Carton @ AUD 20.00/CTN"
				},
			};

			if (rateCategory == RatingConstants.RateCategory.TRW)
			{
				EnterpriseServiceAutorateAndAssert(expected, testedParent, bookedBy);
			}
			else
			{
				AutorateAndAssert(expected, testedParent, bookedBy);
			}
		}

		#endregion

		#region TestRatingOnPackageVolume_Consignment

		public void TestRatingOnPackageVolume_DispatchConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var chargeCode = CreateChargeCode("TRWVOLUME", RatingConstants.RateCategory.TRW, PackageVolumeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TRW, warehouse, 10m, RatingConstants.Units.M3);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_Volume = 10;
			departedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";
			departedPackageState2.Package.KP_Volume = 500;
			departedPackageState2.Package.KP_VolumeUQ = Constants.Volume.CubicInches;

			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, entryNum: "JOB3");
			bookedPackageState.Package.KP_Volume = 500;
			bookedPackageState.Package.KP_VolumeUQ = Constants.Volume.CubicInches;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 100.08m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 10.0082 Cubic Meter(s) @ AUD 10.00/M3"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, dcn, bookedBy);
		}

		public void TestRatingOnPackageVolume_ReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var chargeCode = CreateChargeCode("TRWVOLUME", RatingConstants.RateCategory.TRW, PackageVolumeCargeDescription, ChargeCodeGroupList.Codes.TRWReceive);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TRW, warehouse, 10m, RatingConstants.Units.M3);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB1");
			arrivedPackageState1.Package.KP_Volume = 10;
			arrivedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB2");
			arrivedPackageState2.Package.KP_Volume = 500;
			arrivedPackageState2.Package.KP_VolumeUQ = Constants.Volume.CubicInches;
			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, entryNum: "JOB3");
			bookedPackageState.Package.KP_Volume = 500;
			bookedPackageState.Package.KP_VolumeUQ = Constants.Volume.CubicInches;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 100.08m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
100.08 AUD		10.0082 Cubic Meter(s) @ AUD 10.00/M3"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		public void TestRatingOnPackageVolume_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWVOLUME", RatingConstants.RateCategory.TWU, PackageVolumeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TWU, warehouse, 10m, RatingConstants.Units.M3);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_Volume = 10;
			departedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";
			departedPackageState2.Package.KP_Volume = 500;
			departedPackageState2.Package.KP_VolumeUQ = Constants.Volume.CubicInches;

			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, entryNum: "JOB3");
			bookedPackageState.Package.KP_Volume = 500;
			bookedPackageState.Package.KP_VolumeUQ = Constants.Volume.CubicInches;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 100.08m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 10.0082 Cubic Meter(s) @ AUD 10.00/M3"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);
		}

		public void TestRatingOnPackageVolume_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWVOLUME", RatingConstants.RateCategory.TWU, PackageVolumeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TWU, warehouse, 10m, RatingConstants.Units.M3);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB1");
			arrivedPackageState1.Package.KP_Volume = 10;
			arrivedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB2");
			arrivedPackageState2.Package.KP_Volume = 500;
			arrivedPackageState2.Package.KP_VolumeUQ = Constants.Volume.CubicInches;
			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, entryNum: "JOB3");
			bookedPackageState.Package.KP_Volume = 500;
			bookedPackageState.Package.KP_VolumeUQ = Constants.Volume.CubicInches;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 100.08m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 10.0082 Cubic Meter(s) @ AUD 10.00/M3"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		#endregion

		#region TestRatingOnPackageWeight_Consignment

		public void TestRatingOnPackageWeight_DispatchConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TRW, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TRW, warehouse, 10m, RatingConstants.Units.KG);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_Weight = 10;
			departedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.Package.KP_Weight = 500;
			departedPackageState2.Package.KP_WeightUQ = Constants.Weight.Grams;

			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn, entryNum: "JOB3");
			bookedPackageState.Package.KP_Weight = 500;
			bookedPackageState.Package.KP_WeightUQ = Constants.Weight.Grams;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 105m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 10.5 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, dcn, bookedBy);
		}

		public void TestRatingOnPackageWeight_ReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TRW, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWReceive);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TRW, warehouse, 10m, RatingConstants.Units.KG);

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState1.Package.KP_Weight = 10;
			arrivedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState2.Package.KP_Weight = 500;
			arrivedPackageState2.Package.KP_WeightUQ = Constants.Weight.Grams;
			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			bookedPackageState.Package.KP_Weight = 500;
			bookedPackageState.Package.KP_WeightUQ = Constants.Weight.Grams;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 105m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
105.00 AUD		10.5 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		public void TestRatingOnPackageWeight_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TWU, warehouse, 10m, RatingConstants.Units.KG);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_Weight = 10;
			departedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.Package.KP_Weight = 500;
			departedPackageState2.Package.KP_WeightUQ = Constants.Weight.Grams;

			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			bookedPackageState.Package.KP_Weight = 500;
			bookedPackageState.Package.KP_WeightUQ = Constants.Weight.Grams;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 105m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 10.5 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);
		}

		public void TestRatingOnPackageWeight_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var warehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TWU, warehouse, 10m, RatingConstants.Units.KG);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState1.Package.KP_Weight = 10;
			arrivedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState2.Package.KP_Weight = 500;
			arrivedPackageState2.Package.KP_WeightUQ = Constants.Weight.Grams;
			var bookedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			bookedPackageState.Package.KP_Weight = 500;
			bookedPackageState.Package.KP_WeightUQ = Constants.Weight.Grams;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 105m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 10.5 Kilogram(s) @ AUD 10.00/KG"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		public void TestRatingOnChargeableWeight_Air_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.AIR;
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Pounds);
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.ConversionFactor = ConversionFactor.Empty;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState1.Package.KP_Weight = 325m;
			arrivedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			arrivedPackageState1.Package.KP_Volume = 2.28m;
			arrivedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			Factory.Save();

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.186 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 3585.93m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 717.186 Pound(s) @ AUD 5.00/LB"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		public void TestRatingOnChargeableWeight_Air_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.AIR;
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Pounds);
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.ConversionFactor = ConversionFactor.Empty;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_Weight = 325m;
			departedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			departedPackageState1.Package.KP_Volume = 2.28m;
			departedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			Factory.Save();

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.186 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 3585.93m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 717.186 Pound(s) @ AUD 5.00/LB"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);
		}

		public void TestRatingOnChargeableWeight_Sea_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.SEA;
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Pounds);
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.ConversionFactor = ConversionFactor.Empty;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Sea;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState1.Package.KP_Weight = 325m;
			arrivedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			arrivedPackageState1.Package.KP_Volume = 2.28m;
			arrivedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			Factory.Save();

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 3585.93m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 717.186 Pound(s) @ AUD 5.00/LB"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		public void TestRatingOnChargeableWeight_Sea_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.SEA;
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Pounds);
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.ConversionFactor = ConversionFactor.Empty;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Sea;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_Weight = 325m;
			departedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			departedPackageState1.Package.KP_Volume = 2.28m;
			departedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			Factory.Save();

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 3585.93m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 717.186 Pound(s) @ AUD 5.00/LB"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);
		}

		public void TestRatingOnChargeableWeight_Road_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.ROA;
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Pounds);
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.ConversionFactor = ConversionFactor.Empty;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Road;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState1.Package.KP_Weight = 325m;
			arrivedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			arrivedPackageState1.Package.KP_Volume = 2.28m;
			arrivedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			Factory.Save();

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 3585.93m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 717.186 Pound(s) @ AUD 5.00/LB"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		public void TestRatingOnChargeableWeight_Road_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWWEIGHT", RatingConstants.RateCategory.TWU, PackageWeightCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.ROA;
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, Constants.Weight.Pounds);
			rateLine.UseOnlyActualWeightMeasure = false;
			rateLine.ConversionFactor = ConversionFactor.Empty;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Road;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_Weight = 325m;
			departedPackageState1.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			departedPackageState1.Package.KP_Volume = 2.28m;
			departedPackageState1.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			Factory.Save();

			// 325 KG = 716.502 LB
			// 2.28 M3 = 139134.37 CI = 717.187 LB (194 CI = 1 LB)	- wins as 717.186 > 716.502
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 3585.93m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 717.186 Pound(s) @ AUD 5.00/LB"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);
		}
		#endregion

		#region TestRatingWithAdditionalServices_ReceiveConsignment

		public void TestRatingWithAdditionalServices_ReceiveConsignment()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var anotherWarehouse = Helper.CreateWarehouse("TW2", "A", 1, 1);
			anotherWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();
			var bookedBy = data.Org1;
			var consignee = data.Org2;
			var consignor = data.Org3;
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var fumigationCharge = CreateChargeCode("TRWFUM", RatingConstants.RateCategory.TRW, FumigationCargeDescription, ChargeCodeGroupList.Codes.TRWReceive, Core.Constants.FreightServiceType.Codes.Fumigation);
			var fumigationWarehouse1Charge = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.TRW, data.Whs1, 80m, RatingConstants.Units.SV);
			var fumigationWarehouse2Charge = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.TRW, anotherWarehouse, 60m, RatingConstants.Units.SV);
			var fumigationAllWarehouseCharge = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.TRW, null, 50m, RatingConstants.Units.SV);
			var fumigationForAnotherRateCategory = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.CST, null, 20m, RatingConstants.Units.SV);

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK, bookedBy, consignor, consignee);
			var fumigationService = receiveConsignment.Services.AddNew();
			fumigationService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 3m;
			fumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			fumigationService.ES_Completed = ZDateTime.Now.AddDays(-2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = fumigationCharge.AC_Code,
					JR_OSSellAmt = 240m,
					RevenueCalculationDescription = fumigationCharge.AC_Code + ": 3 Transit Receive Fumigation @ AUD 80.00/Transit Receive Fumigation"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, receiveConsignment, data.Org1);
		}

		public void TestRatingWithAdditionalServices_OtherChargeGroup_ReceiveConsignment()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var anotherWarehouse = Helper.CreateWarehouse("TW2", "A", 1, 1);
			anotherWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();
			var bookedBy = data.Org1;
			var consignee = data.Org2;
			var consignor = data.Org3;
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var fumigationChargeForInwards = CreateChargeCode("INWFUM", RatingConstants.RateCategory.TRW, FumigationCargeDescription, ChargeCodeGroupList.Codes.TRWReceive, Core.Constants.FreightServiceType.Codes.Fumigation);
			CreateRateEntryWithUnitCalculator(clientRate, fumigationChargeForInwards, RatingConstants.RateCategory.TRW, data.Whs1, 80m, RatingConstants.Units.SV);

			var fumigationChargeForOutwards = CreateChargeCode("OWDFUM", RatingConstants.RateCategory.TRW, FumigationCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch, Core.Constants.FreightServiceType.Codes.Fumigation);
			CreateRateEntryWithUnitCalculator(clientRate, fumigationChargeForOutwards, RatingConstants.RateCategory.TRW, null, 50m, RatingConstants.Units.SV);

			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", data.Whs1.PK, bookedBy, consignor, consignee);
			var fumigationService = receiveConsignment.Services.AddNew();
			fumigationService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 3m;
			fumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			fumigationService.ES_Completed = ZDateTime.Now.AddDays(-2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = fumigationChargeForInwards.AC_Code,
					JR_OSSellAmt = 240m,
					RevenueCalculationDescription = fumigationChargeForInwards.AC_Code + ": 3 Transit Receive Fumigation @ AUD 80.00/Transit Receive Fumigation"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, receiveConsignment, data.Org1);
		}

		#endregion

		#region TestRatingWithAdditionalServices_DispatchConsignment

		public void TestRatingWithAdditionalServices_DispatchConsignment()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var anotherWarehouse = Helper.CreateWarehouse("TW2", "A", 1, 1);
			anotherWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			var bookedBy = data.Org1;
			var consignee = data.Org2;
			var consignor = data.Org3;
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var fumigationCharge = CreateChargeCode("TRWFUM", RatingConstants.RateCategory.TRW, FumigationCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch, Core.Constants.FreightServiceType.Codes.Fumigation);
			var fumigationWarehouseSpecificCharge = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.TRW, data.Whs1, 80m, RatingConstants.Units.SV);
			var fumigationChargeForAnotherWarehouse = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.TRW, anotherWarehouse, 60m, RatingConstants.Units.SV);
			var fumigationAllWarehouseCharge = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.TRW, null, 50m, RatingConstants.Units.SV);
			var fumigationForAnotherRateCategory = CreateRateEntryWithUnitCalculator(clientRate, fumigationCharge, RatingConstants.RateCategory.CST, null, 20m, RatingConstants.Units.SV);

			var dispatchConsignment = Helper.CreateDispatchConsignment("DC1", data.Whs1.PK, bookedBy, consignor, consignee);

			var fumigationService = dispatchConsignment.Services.AddNew();
			fumigationService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 3m;
			fumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			fumigationService.ES_Completed = ZDateTime.Now.AddDays(-2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = fumigationCharge.AC_Code,
					JR_OSSellAmt = 240m,
					RevenueCalculationDescription = fumigationCharge.AC_Code + ": 3 Transit Dispatch Fumigation @ AUD 80.00/Transit Dispatch Fumigation"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, dispatchConsignment, data.Org1);
		}

		public void TestRatingWithAdditionalServices_OtherChargeCode_DispatchConsignment()
		{
			var data = new TransitTestDataSimpleEnvironment(Factory);
			var anotherWarehouse = Helper.CreateWarehouse("TW2", "A", 1, 1);
			anotherWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			var bookedBy = data.Org1;
			var consignee = data.Org2;
			var consignor = data.Org3;
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);

			var fumigationChargeForOutwards = CreateChargeCode("OWDFUM", RatingConstants.RateCategory.TRW, FumigationCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch, Core.Constants.FreightServiceType.Codes.Fumigation);
			CreateRateEntryWithUnitCalculator(clientRate, fumigationChargeForOutwards, RatingConstants.RateCategory.TRW, data.Whs1, 80m, RatingConstants.Units.SV);

			var fumigationChargeForInwards = CreateChargeCode("INWFUM", RatingConstants.RateCategory.TRW, FumigationCargeDescription, ChargeCodeGroupList.Codes.TRWReceive, Core.Constants.FreightServiceType.Codes.Fumigation);
			CreateRateEntryWithUnitCalculator(clientRate, fumigationChargeForInwards, RatingConstants.RateCategory.TRW, null, 50m, RatingConstants.Units.SV);

			var dispatchConsignment = Helper.CreateDispatchConsignment("DC1", data.Whs1.PK, bookedBy, consignor, consignee);
			var fumigationService = dispatchConsignment.Services.AddNew();
			fumigationService.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 3m;
			fumigationService.ES_Duration = new TimeSpan(2, 30, 0);
			fumigationService.ES_Completed = ZDateTime.Now.AddDays(-2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = fumigationChargeForOutwards.AC_Code,
					JR_OSSellAmt = 240m,
					RevenueCalculationDescription = fumigationChargeForOutwards.AC_Code + ": 3 Transit Dispatch Fumigation @ AUD 80.00/Transit Dispatch Fumigation"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, dispatchConsignment, data.Org1);
		}

		#endregion

		#region TestRatingOnCommodity

		public void TestRatingOnCommodity_DispatchConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TRW, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatch);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_RH_NKCommodityCode = "HAZ";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";
			departedPackageState2.Package.KP_RH_NKCommodityCode = "AAA";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Package(s) @ AUD 5.00/Package"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, dcn, bookedBy);
		}

		public void TestRatingOnCommodity_ReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TRW, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceive);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB1");
			arrivedPackageState1.Package.KP_RH_NKCommodityCode = "HAZ";
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB2");
			arrivedPackageState2.Package.KP_RH_NKCommodityCode = "AAA";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
5.00 AUD		1 Package(s) @ AUD 5.00/Package"
				},
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		public void TestRatingOnCommodity_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			departedPackageState1.Package.KP_RH_NKCommodityCode = "HAZ";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";
			departedPackageState2.Package.KP_RH_NKCommodityCode = "AAA";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Package(s) @ AUD 5.00/Package"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);
		}

		public void TestRatingOnCommodity_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);

			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_RH_NKCommodityCode = "HAZ";
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState1.Package.KP_RH_NKCommodityCode = "HAZ";
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState2.Package.KP_RH_NKCommodityCode = "AAA";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Package(s) @ AUD 5.00/Package"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);
		}

		#endregion

		#region TestRatingOnContainer

		public void TestRatingOnContainer_ReceiveTransportaiontUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = now.AddDays(-2);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			// Set Container for RateEntry.
			rateEntry.TI_RC = rtu.Container.K0_RC_ContainerType;

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, rtu2, localClient);
		}

		public void TestRatingOnContainer_DispatchTransportaiontUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Sea;
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			// Set Container for RateEntry.
			rateEntry.TI_RC = dtu.Container.K0_RC_ContainerType;

			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			dtu2.WDH_GateInTime = now.AddDays(-2);
			dtu2.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu2);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};
			AutorateAndAssert(expected, dtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, dtu2, localClient);
		}

		public void TestRatingOnContainer_ReceiveTransportaiontUnit_IsOnPallets()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			rateLine.TL_IsOnPallets = true;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = now.AddDays(-2);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu3 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU3", warehouse.PK, location.PK);
			rtu3.WRH_GateInTime = now.AddDays(-2);
			rtu3.WRH_UnloadCompleteTime = now;
			rtu3.WRH_UnloadCompleteNotYetProcessedTime = rtu3.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu3, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var cntRTU = helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");
			cntRTU.WRH_GateInTime = now.AddHours(-1);
			cntRTU.WRH_UnloadCompleteTime = now;
			cntRTU.WRH_UnloadCompleteNotYetProcessedTime = cntRTU.WRH_UnloadCompleteTime;

			var uldRTU = helper.CreateReceiveTransportationUnitWithContainerType("AAA1", warehouse.PK, warehouse.DefaultLocation.PK, containerID: "AAA1", containerType: "AAA");
			uldRTU.WRH_GateInTime = now.AddHours(-1);
			uldRTU.WRH_UnloadCompleteTime = now;
			uldRTU.WRH_UnloadCompleteNotYetProcessedTime = uldRTU.WRH_UnloadCompleteTime;

			// Set Container for RateEntry.
			rateEntry.TI_RC = rtu.Container.K0_RC_ContainerType;

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			var arrivedPackageState4 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			var arrivedPackageState5 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG5", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu3, dispatchConsignment: dcn);

			var cntPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, cntRTU.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			cntPackageState.WPS_UnloadedTime = now;
			cntPackageState.WPS_UnloadedNotYetProcessedTime = cntPackageState.WPS_UnloadedTime;
			cntPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			cntPackageState.WPS_WRH_TransitReceiveHeader = rtu3.PK;
			cntPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var uldPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, uldRTU.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			uldPackageState.WPS_UnloadedTime = now;
			uldPackageState.WPS_UnloadedNotYetProcessedTime = uldPackageState.WPS_UnloadedTime;
			uldPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			uldPackageState.WPS_WRH_TransitReceiveHeader = rtu3.PK;
			uldPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 20GP Container(s) @ AUD 5.00/Container"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, rtu2, localClient);
			AutorateAndAssert(expected, rtu3, localClient);
		}

		public void TestRatingOnContainer_DispatchTransportaiontUnit_IsOnPallets()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.CN);
			rateLine.TL_IsOnPallets = true;

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Sea;
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			dtu2.WDH_GateInTime = now.AddDays(-2);
			dtu2.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dtu3 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU3", warehouse.PK);
			dtu3.WDH_GateInTime = now.AddDays(-2);
			dtu3.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu3, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var cntDTU = Helper.CreateDispatchTransportationUnitWithContainerType("CNTDTU", warehouse.PK, containerID: "20GP1", containerType: "20GP");
			cntDTU.WDH_GateInTime = now.AddHours(-1);
			cntDTU.WDH_LoadCompleteTime = now;

			var uldDTU = Helper.CreateDispatchTransportationUnitWithContainerType("ULDDTU", warehouse.PK, containerID: "AAA1", containerType: "AAA");
			uldDTU.WDH_GateInTime = now.AddHours(-1);
			uldDTU.WDH_LoadCompleteTime = now;

			// Set Container for RateEntry.
			rateEntry.TI_RC = dtu.Container.K0_RC_ContainerType;

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";
			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";

			var departedPackageState3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP3", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu2);
			departedPackageState3.WPS_WL_LastLocation = location.PK;
			departedPackageState3.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState3.WPS_IsSecure = true;
			departedPackageState3.WPS_SecurityStatus = "SEC";
			var departedPackageState4 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Carton, "PKGDEP4", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu2);
			departedPackageState4.WPS_WL_LastLocation = location.PK;
			departedPackageState4.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState4.WPS_IsSecure = true;
			departedPackageState4.WPS_SecurityStatus = "SEC";

			var departedPackageState5 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP5", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu3);
			departedPackageState5.WPS_WL_LastLocation = location.PK;
			departedPackageState5.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState5.WPS_IsSecure = true;
			departedPackageState5.WPS_SecurityStatus = "SEC";

			var cntPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, cntDTU.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			cntPackageState.WPS_LoadedTime = now;
			cntPackageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			cntPackageState.WPS_WDH_TransitDispatchHeader = dtu3.PK;
			cntPackageState.WPS_WDL_LoadList = dll.PK;
			cntPackageState.WPS_IsSecure = true;
			cntPackageState.WPS_SecurityStatus = "SEC";

			var uldPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, uldDTU.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			uldPackageState.WPS_LoadedTime = now;
			uldPackageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			uldPackageState.WPS_WDH_TransitDispatchHeader = dtu3.PK;
			uldPackageState.WPS_WDL_LoadList = dll.PK;
			uldPackageState.WPS_IsSecure = true;
			uldPackageState.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 20GP Container(s) @ AUD 5.00/Container"
				},
			};
			AutorateAndAssert(expected, dtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, dtu2, localClient);
			AutorateAndAssert(expected, dtu3, localClient);
		}

		#endregion

		#region TestRatingOnFreightMode

		public void TestRatingOnFreightMode_ReceiveTransportaiontUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.SEA;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Sea;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = now.AddDays(-2);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu3 = helper.CreateReceiveTransportationUnitWithContainerType("RTU3", warehouse.PK, location.PK, "CNT1", "20GP");
			rtu3.WRH_GateInTime = now.AddDays(-2);
			rtu3.WRH_UnloadCompleteTime = now;
			rtu3.WRH_UnloadCompleteNotYetProcessedTime = rtu3.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu3, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu4 = helper.CreateReceiveTransportationUnitWithContainerType("RTU4", warehouse.PK, location.PK, "ULD1", "AAA");
			rtu4.WRH_GateInTime = now.AddDays(-2);
			rtu4.WRH_UnloadCompleteTime = now;
			rtu4.WRH_UnloadCompleteNotYetProcessedTime = rtu4.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu4, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			var arrivedPackageState3 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu3, dispatchConsignment: dcn);
			var arrivedPackageState4 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu4, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, rtu2, localClient);

			AutorateAndAssert(expected, rtu3, localClient);
			AutorateAndAssert(expected2, rtu4, localClient);
		}

		public void TestRatingOnFreightMode_ReceiveTransportaiontUnit_Vehicle()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.ROA;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Sea;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu1.WRH_GateInTime = now.AddDays(-2);
			rtu1.WRH_UnloadCompleteTime = now;
			rtu1.WRH_UnloadCompleteNotYetProcessedTime = rtu1.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu1, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu2 = helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, location.PK, "VEH1", "DROP");
			rtu2.WRH_GateInTime = now.AddDays(-2);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu1, dispatchConsignment: dcn);
			var arrivedPackageState3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			var arrivedPackageState4 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Bag, "PKG4", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, rtu2, localClient);

			var expected2 = Array.Empty<AssertionCharge>();

			AutorateAndAssert(expected2, rtu1, localClient, expectedErrors: new string[] { "Error Please ensure the minimum criteria required to use Autorating / Quick Calculator has been saved on the job:\r\n\tCost: Could not determine Transport Mode.\r\nPlease ensure the minimum criteria required to use Autorating / Quick Calculator has been saved on the job:\r\n\tRevenue: Could not determine Transport Mode." });
		}

		public void TestRatingOnFreightMode_DispatchTransportaiontUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.SEA;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Sea;
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll2.WDL_TransportMode = TransportModes.Air;
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			dtu2.WDH_GateInTime = now.AddDays(-2);
			dtu2.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dtu3 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU3", warehouse.PK, "CNT1", "20GP");
			dtu3.WDH_GateInTime = now.AddDays(-2);
			dtu3.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu3, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dtu4 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU4", warehouse.PK, "ULD1", "AAA");
			dtu4.WDH_GateInTime = now.AddDays(-2);
			dtu4.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu4, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu2.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu3.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu4.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu2);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";

			var departedPackageState3 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP3", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu3);
			departedPackageState3.WPS_WL_LastLocation = location.PK;
			departedPackageState3.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState3.WPS_IsSecure = true;
			departedPackageState3.WPS_SecurityStatus = "SEC";

			var departedPackageState4 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP4", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu4);
			departedPackageState4.WPS_WL_LastLocation = location.PK;
			departedPackageState4.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState4.WPS_IsSecure = true;
			departedPackageState4.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, dtu2, localClient);

			AutorateAndAssert(expected, dtu3, localClient);
			AutorateAndAssert(expected2, dtu4, localClient);
		}

		public void TestRatingOnFreightMode_DispatchTransportaiontUnit_Vehicle()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_Mode = Core.Constants.RateMode.ROA;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Sea;

			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll2.WDL_TransportMode = TransportModes.Air;

			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu1.WDH_GateInTime = now.AddDays(-2);
			dtu1.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu1, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "VEH1", "DROP");
			dtu2.WDH_GateInTime = now.AddDays(-2);
			dtu2.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu2.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu1);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu2);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, dtu2, localClient);

			var expected2 = Array.Empty<AssertionCharge>();

			AutorateAndAssert(expected2, dtu1, localClient, expectedErrors: new string[] { "Error Please ensure the minimum criteria required to use Autorating / Quick Calculator has been saved on the job:\r\n\tCost: Could not determine Transport Mode.\r\nPlease ensure the minimum criteria required to use Autorating / Quick Calculator has been saved on the job:\r\n\tRevenue: Could not determine Transport Mode." });
		}

		#endregion

		#region TestRatingOnCarrier

		public void TestRatingOnCarrier_ReceiveTransportaiontUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var transportCompany = Helper.CreateClient("TRC");
			var transportCompany2 = Helper.CreateClient("TRC2");
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_OH_TransportProvider = transportCompany.PK;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Sea;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = now.AddDays(-2);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, rtu2, localClient);
		}

		public void TestRatingOnCarrier_DispatchTransportaiontUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var transportCompany = Helper.CreateClient("TRC");
			var transportCompany2 = Helper.CreateClient("TRC2");
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(localClient);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TWU, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TWU, warehouse);
			rateEntry.TI_OH_TransportProvider = transportCompany.PK;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Sea;
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			dtu2.WDH_GateInTime = now.AddDays(-2);
			dtu2.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu2);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 50,
					RevenueCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, dtu2, localClient);
		}

		#endregion

		#region TestRatingCostOnTransportCompany

		public void TestRatingCostOnTransportCompany_ReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var transportCompany = Helper.CreateClient("TRC");
			var transportCompany2 = Helper.CreateClient("TRC2");
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;

			var costing = Helper.CreateCosting(transportCompany);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", $"{RatingConstants.RateCategory.TWU} - {PacktypeCargeDescription} {ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit}", ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit, "", Constants.ChargeType.Margin);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.TWU, RateMode.ALL);
			rateEntry.TI_RateStartDate = ZDateTime.Now.AddDays(-5).Date;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Sea;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = now.AddDays(-2);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = now.AddDays(-2);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);
			Helper.CreateJobDocAddressFromAddress(rtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn2, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu2, dispatchConsignment: dcn);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 50m,
					CostCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, rtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, rtu2, localClient);
		}

		public void TestRatingCostOnTransportCompany_DispatchTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var transportCompany = Helper.CreateClient("TRC");
			var transportCompany2 = Helper.CreateClient("TRC2");
			var localClient = Helper.CreateClient("Org1");
			localClient.OH_IsDebtor = true;

			var chargeCode = Helper.CreateChargeCode("TRWPKG", $"{RatingConstants.RateCategory.TWU} - {PacktypeCargeDescription} {ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit}", ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit, "", Constants.ChargeType.Margin);
			var costing = Helper.CreateCosting(transportCompany);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.TWU, RateMode.ALL);
			rateEntry.TI_RateStartDate = ZDateTime.Now.AddDays(-5).Date;
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Pallet, 0m, 50m);

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Sea;
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddDays(-2);
			dtu.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			dtu2.WDH_GateInTime = now.AddDays(-2);
			dtu2.WDH_LoadCompleteTime = now;
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany2.MainAddress);
			Helper.CreateJobDocAddressFromAddress(dtu2, DocAddressTypes.Codes.ClientRequestedBillingParty, localClient.MainAddress);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu2.PK);

			var departedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			departedPackageState1.WPS_WL_LastLocation = location.PK;
			departedPackageState1.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState1.WPS_IsSecure = true;
			departedPackageState1.WPS_SecurityStatus = "SEC";

			var departedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKGDEP2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu2);
			departedPackageState2.WPS_WL_LastLocation = location.PK;
			departedPackageState2.WPS_WL_ReceiveLocation = location.PK;
			departedPackageState2.WPS_IsSecure = true;
			departedPackageState2.WPS_SecurityStatus = "SEC";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSCostAmt = 50m,
					CostCalculationDescription = chargeCode.AC_Code + ": 1 Pallet @ AUD 50.00/PLT"
				},
			};

			AutorateAndAssert(expected, dtu, localClient);

			var expected2 = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected2, dtu2, localClient);
		}

		#endregion

		#region TestRatingOnCompletedReceiveConsignment

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnCompletedReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();
			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			bookedBy.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(bookedBy);
			var chargeCode = CreateChargeCode("TRWPKG", RatingConstants.RateCategory.TRW, PacktypeCargeDescription, ChargeCodeGroupList.Codes.TRWReceive);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var rateLine = rateEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.PK);

			var calculator = rateLine.Calculator;
			calculator[Calculator.Items.Operator.UNT] = (ZDecimal)5m;

			var loadedTime = ZDateTimeOffset.Now.AddDays(-2);
			var completeTime = loadedTime.AddDays(-3);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = completeTime;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = completeTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = completeTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			packageState1.WPS_UnloadedTime = completeTime;
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = loadedTime;
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = completeTime;
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			var packageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB3");
			packageState3.WPS_UnloadedTime = completeTime;
			packageState3.WPS_UnloadedNotYetProcessedTime = packageState3.WPS_UnloadedTime;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 15m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
10.00 AUD		2 Package(s) @ AUD 5.00/Package
5.00 AUD		1 Package(s) @ AUD 5.00/Package"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		#endregion

		#region TestRatingOnUnloadCompleteDate

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnUnloadCompleteDate()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = now.AddDays(-5);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = unloadedTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = unloadedTime;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			packageState1.WPS_UnloadedTime = now.AddDays(-3);
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = now.AddDays(-2);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = now.AddDays(-2);
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			packageState2.WPS_LoadedTime = now.AddDays(-1);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 15m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
10.00 AUD		2 Package x Day (1 Package(s) x 2 Day(s)) @ AUD 5.00/Package x Day
5.00 AUD		1 Package x Day @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnUnloadCompleteDateFallback()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var loadedTime = ZDateTimeOffset.Now;
			var completeTime = loadedTime.AddDays(-5);
			var unloadedTime = completeTime.AddDays(2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = completeTime;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			packageState1.WPS_UnloadedTime = unloadedTime;
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = loadedTime;
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = unloadedTime;
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			var packageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn, entryNum: "JOB3");
			packageState3.WPS_UnloadedTime = unloadedTime;
			packageState3.WPS_UnloadedNotYetProcessedTime = packageState3.WPS_UnloadedTime;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 75m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
75.00 AUD		15 Package x Day (3 Package(s) x 5 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnUnloadCompleteDateWithMultipleRTUs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = now.AddDays(-5);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var unloadedTime1 = now.AddDays(-3);
			rtu1.WRH_GateInTime = unloadedTime1.AddHours(-1);
			rtu1.WRH_UnloadCompleteTime = unloadedTime1;
			rtu1.WRH_UnloadCompleteNotYetProcessedTime = rtu1.WRH_UnloadCompleteTime;
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var unloadedTime2 = now.AddDays(-2);
			rtu2.WRH_GateInTime = unloadedTime2.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = unloadedTime2;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu1, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			packageState1.WPS_UnloadedTime = unloadedTime1.AddHours(-5); // Shouldn't affect our result but shows it's different from the RTU UnloadCompleteTime
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = now.AddDays(-2);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = unloadedTime2.AddHours(-5);
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			packageState2.WPS_LoadedTime = now.AddDays(-1);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
10.00 AUD		2 Package x Day (1 Package(s) x 2 Day(s)) @ AUD 5.00/Package x Day
10.00 AUD		2 Package x Day (1 Package(s) x 2 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnUnloadCompleteDateWithMultipleRTUsAndSameUnloadAndLoadTime()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = now.AddDays(-5);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var unloadedTime = now.AddDays(-3);
			var gateInTime = unloadedTime.AddHours(-1);
			rtu1.WRH_GateInTime = gateInTime;
			rtu1.WRH_UnloadCompleteTime = unloadedTime;
			rtu1.WRH_UnloadCompleteNotYetProcessedTime = rtu1.WRH_UnloadCompleteTime;
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = gateInTime;
			rtu2.WRH_UnloadCompleteTime = unloadedTime;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var loadedTime = now.AddDays(-1);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu1, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			var packageUnloadedTime = unloadedTime.AddHours(-5); // Shouldn't affect our result but shows it's different from the RTU UnloadCompleteTime
			packageState1.WPS_UnloadedTime = packageUnloadedTime;
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = loadedTime;
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = packageUnloadedTime;
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			packageState2.WPS_LoadedTime = loadedTime;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 30m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
30.00 AUD		6 Package x Day (2 Package(s) x 3 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnUnloadCompleteDateWithMultipleRTUsAndFallback()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = now.AddDays(-5);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var unloadedTime = now.AddDays(-2);
			rtu2.WRH_GateInTime = unloadedTime.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = unloadedTime;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu1, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			packageState1.WPS_UnloadedTime = now.AddDays(-4);
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = now.AddDays(-2);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = unloadedTime.AddHours(-5);
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			packageState2.WPS_LoadedTime = now.AddDays(-1);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 25m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
15.00 AUD		3 Package x Day (1 Package(s) x 3 Day(s)) @ AUD 5.00/Package x Day
10.00 AUD		2 Package x Day (1 Package(s) x 2 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnUnloadCompleteDateWithMultipleRTUsAndLastDayIsBeforeFirstDay()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = now.AddDays(-5);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu1.WRH_GateInTime = now;
			rtu1.WRH_UnloadCompleteTime = now.AddDays(5);
			rtu1.WRH_UnloadCompleteNotYetProcessedTime = rtu1.WRH_UnloadCompleteTime;
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = now.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu1, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
5.00 AUD		1 Package x Day @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		#endregion

		#region TestShortTermStorage

		#region TestShortTermStorage_NoFreeDays

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAir_NoFreeDays()
		{
			TestShortTermStorage_NoFreeDaysCore(TransportModes.Air);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAirSea_NoFreeDays()
		{
			TestShortTermStorage_NoFreeDaysCore(TransportModes.AirSea);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSea_NoFreeDays()
		{
			TestShortTermStorage_NoFreeDaysCore(TransportModes.Sea);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSeaAir_NoFreeDays()
		{
			TestShortTermStorage_NoFreeDaysCore(TransportModes.SeaAir);
		}

		void TestShortTermStorage_NoFreeDaysCore(string transportMode)
		{
			var testData = new ShortTermStorageTestData(Factory, Helper, transportMode);
			using (CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				RawDataRegistry.Instance.CFSAirFreightLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 0);
				RawDataRegistry.Instance.CFSSeaFreightLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 0);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = testData.ChargeCode.AC_Code,
						JR_OSSellAmt = 120m,
						RevenueCalculationDescription = testData.ChargeCode.AC_Code + @"
Receive Consignment RC1
120.00 AUD		4 Package x Day (4 Package(s) x 1 Day(s)) @ AUD 10.00/Package x Day + 16 Package x Day (4 Package(s) x 4 Day(s)) @ AUD 5.00/Package x Day"
					},
				};

				EnterpriseServiceAutorateAndAssert(expected, testData.RCN, testData.Consignee);
			}
		}

		#endregion

		#region TestShortTermStorage_PackageLoadedTimeBeforeToday

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAir_NoFreeDays_PackageLoadedTimeBeforeToday()
		{
			TestShortTermStorage_PackageLoadedTimeBeforeTodayCore(TransportModes.Air);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAirSea_NoFreeDays_PackageLoadedTimeBeforeToday()
		{
			TestShortTermStorage_PackageLoadedTimeBeforeTodayCore(TransportModes.AirSea);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSea_NoFreeDays_PackageLoadedTimeBeforeToday()
		{
			TestShortTermStorage_PackageLoadedTimeBeforeTodayCore(TransportModes.Sea);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSeaAir_NoFreeDays_PackageLoadedTimeBeforeToday()
		{
			TestShortTermStorage_PackageLoadedTimeBeforeTodayCore(TransportModes.SeaAir);
		}

		void TestShortTermStorage_PackageLoadedTimeBeforeTodayCore(string transportMode)
		{
			var testData = new ShortTermStorageTestData(Factory, Helper, transportMode);
			var loadedPackageStates = testData.RCN.PackageStates.Where(p => !p.WPS_LoadedTime.IsEmpty);
			foreach (var loadedPackageState in loadedPackageStates)
			{
				loadedPackageState.WPS_LoadedTime = ZDateTimeOffset.Now.AddDays(-2);
			}
			Factory.Save();

			using (CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				RawDataRegistry.Instance.CFSAirFreightLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 0);
				RawDataRegistry.Instance.CFSSeaFreightLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 0);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = testData.ChargeCode.AC_Code,
						JR_OSSellAmt = 90m,
						RevenueCalculationDescription = testData.ChargeCode.AC_Code + @"
Receive Consignment RC1
60.00 AUD		2 Package x Day (2 Package(s) x 1 Day(s)) @ AUD 10.00/Package x Day + 8 Package x Day (2 Package(s) x 4 Day(s)) @ AUD 5.00/Package x Day
30.00 AUD		6 Package x Day (2 Package(s) x 3 Day(s)) @ AUD 5.00/Package x Day"
					}
				};

				EnterpriseServiceAutorateAndAssert(expected, testData.RCN, testData.Consignee);
			}
		}

		#endregion

		#region TestShortTermStorage_UseFreeDays

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAir_UseClientFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.Air, true);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAirSea_UseClientFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.AirSea, true);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSea_UseClientFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.Sea, true);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSeaAir_UseClientFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.SeaAir, true);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsNotAirOrSea_UseClientFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.Road, true);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsEmpty_UseClientFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore("", true);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAir_UseRegistryFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.Air, false);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAirSea_UseRegistryFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.AirSea, false);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSea_UseRegistryFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.Sea, false);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSeaAir_UseRegistryFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.SeaAir, false);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsNotAirOrSea_UseRegistryFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore(TransportModes.Road, false);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsEmpty_UseRegistryFreeDays()
		{
			TestShortTermStorage_UseFreeDaysCore("", false);
		}

		void TestShortTermStorage_UseFreeDaysCore(string transportMode, bool isUsingClientFreeDays, bool hasDG = false)
		{
			var testData = new ShortTermStorageTestData(Factory, Helper, transportMode, hasDG: hasDG);
			if (isUsingClientFreeDays)
			{
				testData.Consignee.MiscServ.OM_IMAirDepotFreeDays = 2;
				testData.Consignee.MiscServ.OM_IMSeaDepotFreeDays = 2;
			}
			Factory.Save();

			using (CFSDataRegistry.Instance.CFSAirFreightUseClientFreeDays.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isUsingClientFreeDays))
			using (CFSDataRegistry.Instance.CFSSeaFreightUseClientFreeDays.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isUsingClientFreeDays))
			{
				if (!isUsingClientFreeDays)
				{
					RawDataRegistry.Instance.CFSAirFreightLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
					RawDataRegistry.Instance.CFSSeaFreightLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
				}

				if (hasDG)
				{
					RawDataRegistry.Instance.CFSAirFreightDGLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
					RawDataRegistry.Instance.CFSSeaFreightDGLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
				}

				if (transportMode == TransportModes.Air || transportMode == TransportModes.AirSea || transportMode == TransportModes.Sea || transportMode == TransportModes.SeaAir)
				{
					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = testData.ChargeCode.AC_Code,
							JR_OSSellAmt = 80m,
							RevenueCalculationDescription = testData.ChargeCode.AC_Code + @"
Receive Consignment RC1
80.00 AUD		16 Package x Day (4 Package(s) x 4 Day(s)) @ AUD 5.00/Package x Day"
						}
					};

					EnterpriseServiceAutorateAndAssert(expected, testData.RCN, testData.Consignee);
				}
				else
				{
					var expected = new[]
					{
						new AssertionCharge
						{
							ChargeCode = testData.ChargeCode.AC_Code,
							JR_OSSellAmt = 120m,
							RevenueCalculationDescription = testData.ChargeCode.AC_Code + @"
Receive Consignment RC1
120.00 AUD		4 Package x Day (4 Package(s) x 1 Day(s)) @ AUD 10.00/Package x Day + 16 Package x Day (4 Package(s) x 4 Day(s)) @ AUD 5.00/Package x Day"
						}
					};

					EnterpriseServiceAutorateAndAssert(expected, testData.RCN, testData.Consignee);
				}
			}
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAir_UseRegistryFreeDays_HasDangerousGoods()
		{
			TestShortTermStorage_UseFreeDays_HasDGCore(TransportModes.Air);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsAirSea_UseRegistryFreeDays_HasDangerousGoods()
		{
			TestShortTermStorage_UseFreeDays_HasDGCore(TransportModes.AirSea);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSea_UseRegistryFreeDays_HasDangerousGoods()
		{
			TestShortTermStorage_UseFreeDays_HasDGCore(TransportModes.Sea);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_TransportModeIsSeaAir_UseRegistryFreeDays_HasDangerousGoods()
		{
			TestShortTermStorage_UseFreeDays_HasDGCore(TransportModes.SeaAir);
		}

		void TestShortTermStorage_UseFreeDays_HasDGCore(string transportMode)
		{
			var testData = new ShortTermStorageTestData(Factory, Helper, transportMode, hasDG: true);
			Factory.Save();

			RawDataRegistry.Instance.CFSAirFreightDGLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);
			RawDataRegistry.Instance.CFSSeaFreightDGLCLStorageFreeDays.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 2);

			var expected = new[]
			{
					new AssertionCharge
					{
						ChargeCode = testData.ChargeCode.AC_Code,
						JR_OSSellAmt = 20m,
						RevenueCalculationDescription = testData.ChargeCode.AC_Code + @"
Receive Consignment RC1
20.00 AUD		4 Package x Day (1 Package(s) x 4 Day(s)) @ AUD 5.00/Package x Day"
					},
				};

			EnterpriseServiceAutorateAndAssert(expected, testData.RCN, testData.Consignee);
		}

		#endregion

		#region TestShortTermStorage_UnloadComplateDateForRatingRegistry

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_UnloadComplateDateForRatingRegistry_EUD_DifferentUnloadCompleteDay()
		{
			WarehouseDataRegistry.Instance.UnloadCompleteDateForRatingOfStorage.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, UnloadCompleteDateForRatingOfStorageList.Codes.EachUnloadCompleteDate);

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = now.AddDays(-5);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = unloadedTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = unloadedTime.AddDays(1);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = unloadedTime.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = unloadedTime;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			// ****************************************************************************
			// Data Setup
			// PKG1, UnloadedComplete: 2022-06-09 01:00:00, LoadedTime: 2022-06-10 01:00:00
			// PKG1, UnloadedComplete: 2022-06-08 01:00:00, LoadedTime: 2022-06-10 01:00:00
			// ****************************************************************************
			// Rating Result
			// 1x package * 2 days
			// 1x package * 3 days
			// ****************************************************************************
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			packageState1.WPS_UnloadedTime = now.AddDays(-3);
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = now;
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = now.AddDays(-3);
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			packageState2.WPS_LoadedTime = now;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 25m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
15.00 AUD		3 Package x Day (1 Package(s) x 3 Day(s)) @ AUD 5.00/Package x Day
10.00 AUD		2 Package x Day (1 Package(s) x 2 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestShortTermStorage_UnloadComplateDateForRatingRegistry_LUD_DifferentUnloadCompleteDay()
		{
			WarehouseDataRegistry.Instance.UnloadCompleteDateForRatingOfStorage.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, UnloadCompleteDateForRatingOfStorageList.Codes.LastUnloadCompleteDate);

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_CompleteTime = now.AddDays(-5);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = unloadedTime.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = unloadedTime.AddDays(1);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			rtu2.WRH_GateInTime = unloadedTime.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = unloadedTime;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			// ****************************************************************************
			// Data Setup
			// PKG1, UnloadedComplete: 2022-06-09 01:00:00, LoadedTime: 2022-06-10 00:55:00
			// PKG1, UnloadedComplete: 2022-06-08 01:00:00, LoadedTime: 2022-06-10 00:57:00
			// ****************************************************************************
			// Rating Result
			// 2x package * 2 days
			// ****************************************************************************
			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			packageState1.WPS_UnloadedTime = now.AddDays(-3);
			packageState1.WPS_UnloadedNotYetProcessedTime = packageState1.WPS_UnloadedTime;
			packageState1.WPS_LoadedTime = now.AddMinutes(-5);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			packageState2.WPS_UnloadedTime = now.AddDays(-3);
			packageState2.WPS_UnloadedNotYetProcessedTime = packageState2.WPS_UnloadedTime;
			packageState2.WPS_LoadedTime = now.AddMinutes(-3);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
20.00 AUD		4 Package x Day (2 Package(s) x 2 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		#endregion

		class ShortTermStorageTestData
		{
			public ShortTermStorageTestData(BusinessObjectFactory factory, WhsTransitTestHelper helper, string transportMode, bool hasDG = false)
			{
				Factory = factory;
				Helper = helper;

				Consignee = Helper.CreateClient("CONE");
				ChargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
				RCN = CreateShortTermStorageTestData(Consignee, ChargeCode, transportMode, hasDG);
			}

			BusinessObjectFactory Factory { get; }

			WhsTransitTestHelper Helper { get; }

			public WhsItemReceiveConsignment RCN { get; }

			public OrgHeader Consignee { get; }

			public AccChargeCode ChargeCode { get; }

			WhsItemReceiveConsignment CreateShortTermStorageTestData(OrgHeader consignee, AccChargeCode chargeCode, string transportMode, bool hasDG)
			{
				var warehouse = Helper.CreateTRWWarehouse();
				var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
				var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

				var bookedBy = Helper.CreateClient("Org1");
				var consignor = Helper.CreateClient("Org2");

				Factory.Save();

				var clientRate = Helper.CreateClientRate(consignee);
				var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
				if (hasDG)
				{
					rateEntry.TI_RH_NKCommodityCode = "HAZ";
				}
				var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);

				var calculator = rateLine.GetCalculator<TimeCalculator>();
				calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 4, 5, QuantityUnit.DY);
				calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 4, 10);
				Factory.Save();

				var loadedTime = ZDateTimeOffset.Now;
				var completeTime = loadedTime.AddDays(-5);
				var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
				rcn.WRC_CompleteTime = completeTime;
				rcn.WRC_TransportMode = transportMode;
				var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
				var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
				rtu.WRH_GateInTime = completeTime.AddHours(-1);
				rtu.WRH_UnloadCompleteTime = completeTime;
				rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
				var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
				var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
				var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
				arrivedPackageState.WPS_UnloadedTime = completeTime;
				arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;
				var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
				arrivedPackageState2.WPS_UnloadedTime = completeTime;
				arrivedPackageState2.WPS_UnloadedNotYetProcessedTime = arrivedPackageState2.WPS_UnloadedTime;
				var loadedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
				loadedPackageState.WPS_UnloadedTime = completeTime;
				loadedPackageState.WPS_UnloadedNotYetProcessedTime = loadedPackageState.WPS_UnloadedTime;
				loadedPackageState.WPS_LoadedTime = loadedTime.AddMinutes(-1);
				var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
				loadedPackageState2.WPS_UnloadedTime = completeTime;
				loadedPackageState2.WPS_UnloadedNotYetProcessedTime = loadedPackageState2.WPS_UnloadedTime;
				loadedPackageState2.WPS_LoadedTime = loadedTime;
				var bookedPackageState = Helper.CreatePackageState(rcn, 10, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);

				if (hasDG)
				{
					var undgSubstance = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
					undgSubstance.DG_ExceptedQuantityCode = "E1";
					var undgDataItem = Helper.CreateUNDGDataItem(arrivedPackageState.Package.PK, arrivedPackageState.Package.TablePrefix, undgSubstance, 2, 3);

					arrivedPackageState.Package.UNDGs.Add(undgDataItem);
					arrivedPackageState.Package.KP_RH_NKCommodityCode = "HAZ";
				}

				Factory.Save();

				return rcn;
			}
		}

		#endregion

		#region TestRatingOnOpenReceiveConsignment

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnOpenReceiveConsignment_HasUnloadedCompleteTime_ShouldReturnStorageRate()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: unloadedTime);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 15m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
15.00 AUD		3 Package x Day (1 Package(s) x 3 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnOpenReceiveConsignment_NoUnloadedCompleteTime_ShouldReturnNothing()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");
			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnOpenReceiveConsignment_NotAllPackagesHaveUnloadedCompleteTime_ShouldReturnStorageRateForValidPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var calculator = rateLine.Calculator;

			calculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: unloadedTime);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn,
				dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn,
				dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 15m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
15.00 AUD		3 Package x Day (1 Package(s) x 3 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnOpenReceiveConsignment_NotAllPackagesHaveUnloadedCompleteTime_UnitCalc_ShouldReturnStorageRateForValidPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var rateLine = CreateRateEntryWithUnitCalculator(clientRate, chargeCode, RatingConstants.RateCategory.TRW, warehouse, 10m, QuantityUnit.PK);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: unloadedTime);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn,
				dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1");

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn,
				dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2");
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 20m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
10.00 AUD		1 Package(s) @ AUD 10.00/Package
10.00 AUD		1 Package(s) @ AUD 10.00/Package"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		[TestDate(2022, 6, 10, 1, 0, 0)]
		public void TestRatingOnOpenReceiveConsignment_NotAllPackagesHaveUnloadedCompleteTime_TimeAndUnitCalc_ShouldReturnStorageRateForValidPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var chargeCode = Helper.CreateChargeCode("TRWPKG", "Transit Warehouse Charge", ChargeCodeGroupList.Codes.TRWReceive, "");

			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);
			var unitRateLine = rateEntry.AddRateLine(chargeCode.AC_Code, UnitCalculator.Code, QuantityUnit.PK);
			unitRateLine.GetCalculator<UnitCalculator>().PerUnit = 3m;
			var timeRateLine = rateEntry.AddRateLine(chargeCode, TimeCalculator.Code, QuantityUnit.PK);
			timeRateLine.GetCalculator<TimeCalculator>().AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var unloadedRtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: unloadedTime);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: unloadedRtu,
				dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: unloadedRtu,
				dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			var openRtu = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK);
			var packageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG3", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: openRtu,
				dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var packageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG4", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: openRtu,
				dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 42m,
					RevenueCalculationDescription = chargeCode.AC_Code + @"
Receive Consignment RC1
30.00 AUD		6 Package x Day (2 Package(s) x 3 Day(s)) @ AUD 5.00/Package x Day
6.00 AUD		2 Package(s) @ AUD 3.00/Package
6.00 AUD		2 Package(s) @ AUD 3.00/Package"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		#endregion

		#region TestChargeApplyWithoutFreeDays_NotAllPackagesHasUnloadCompleteTime_ValidReturnStorageRate

		[TestDate(2023, 7, 18, 1, 0, 0)]
		public void TestChargeApplyWithoutFreeDays_NotAllPackagesHasUnloadCompleteTime_ValidReturnStorageRate()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);

			var fltChargeCode = Helper.CreateChargeCode("FLTTRC", "Flat Charge Transit Receive", ChargeCodeGroupList.Codes.TRWReceive, "");
			var fltRateLine = rateEntry.AddRateLine(fltChargeCode, FlatCalculator.Code);
			fltRateLine.GetCalculator<FlatCalculator>().BaseRate = 10.0M;

			var dstorChargeCode = Helper.CreateChargeCode("DSTOR2", "Storage", ChargeCodeGroupList.Codes.TRWReceive, "");
			var dstorRateLine = rateEntry.AddRateLine(dstorChargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var dstorCalculator = dstorRateLine.Calculator;
			dstorCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);

			var untChargeCode = Helper.CreateChargeCode("UNTTRC", "Per Unit Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var untRateLine = rateEntry.AddRateLine(untChargeCode, UnitCalculator.Code, QuantityUnit.KG, "USD");
			untRateLine.GetCalculator<UnitCalculator>().PerUnit = 1.0M;

			Factory.Save();

			var unloadedTime = ZDateTimeOffset.Now.AddDays(-2);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_TransportMode = TransportModes.Air;
			rcn.WRC_CompleteTime = ZDateTimeOffset.Empty;

			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: unloadedTime);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: ZDateTimeOffset.Empty);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu1, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1", weight: 1);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu2, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB2", weight: 2);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = fltChargeCode.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = fltChargeCode.AC_Code + @"
Receive Consignment RC1
10.00 AUD		Base Rate AUD 10.00"
				},
				new AssertionCharge
				{
					ChargeCode = untChargeCode.AC_Code,
					JR_OSSellAmt = 3m,
					RevenueCalculationDescription = untChargeCode.AC_Code + @"
Receive Consignment RC1
2.00 USD		2 Kilogram(s) @ USD 1.00/KG
1.00 USD		1 Kilogram(s) @ USD 1.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = dstorChargeCode.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = dstorChargeCode.AC_Code + @"
Receive Consignment RC1
10.00 AUD		2 Package x Day (1 Package(s) x 2 Day(s)) @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		#endregion

		#region TestChargeApplyWithinFreeDays_HasUnloadCompleteTime_NotReturnStorageRate

		[TestDate(2023, 7, 18, 1, 0, 0)]
		public void TestChargeApplyWithinFreeDays_HasUnloadCompleteTime_NotReturnStorageRate()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);

			var fltChargeCode = Helper.CreateChargeCode("FLTTRC", "Flat Charge Transit Receive", ChargeCodeGroupList.Codes.TRWReceive, "");
			var fltRateLine = rateEntry.AddRateLine(fltChargeCode, FlatCalculator.Code);
			fltRateLine.GetCalculator<FlatCalculator>().BaseRate = 10.0M;

			var dstorChargeCode = Helper.CreateChargeCode("DSTOR2", "Storage", ChargeCodeGroupList.Codes.TRWReceive, "");
			var dstorRateLine = rateEntry.AddRateLine(dstorChargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var dstorCalculator = dstorRateLine.Calculator;
			dstorCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);

			var untChargeCode = Helper.CreateChargeCode("UNTTRC", "Per Unit Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var untRateLine = rateEntry.AddRateLine(untChargeCode, UnitCalculator.Code, QuantityUnit.KG, "USD");
			untRateLine.GetCalculator<UnitCalculator>().PerUnit = 1.0M;

			Factory.Save();

			var unloadedTime = ZDateTimeOffset.Now;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: unloadedTime);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1", weight: 1);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = fltChargeCode.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = fltChargeCode.AC_Code + ": Base Rate AUD 10.00"
				},
				new AssertionCharge
				{
					ChargeCode = untChargeCode.AC_Code,
					JR_OSSellAmt = 1m,
					RevenueCalculationDescription = untChargeCode.AC_Code + ": 1 Kilogram(s) @ USD 1.00/KG"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		#endregion

		#region TestChargeApplyWithoutFreeDays_HasUnloadCompleteTime_ShouldReturnStorageRate

		[TestDate(2023, 7, 18, 1, 0, 0)]
		public void TestChargeApplyWithoutFreeDays_HasUnloadCompleteTime_ShouldReturnStorageRate()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			Factory.Save();

			var bookedBy = Helper.CreateClient("Org1");
			var consignee = Helper.CreateClient("Org2");
			var consignor = Helper.CreateClient("Org3");
			var clientRate = Helper.CreateClientRate(consignee);
			var rateEntry = Helper.CreateRateEntry(clientRate, RatingConstants.RateCategory.TRW, warehouse);

			var fltChargeCode = Helper.CreateChargeCode("FLTTRC", "Flat Charge Transit Receive", ChargeCodeGroupList.Codes.TRWReceive, "");
			var fltRateLine = rateEntry.AddRateLine(fltChargeCode, FlatCalculator.Code);
			fltRateLine.GetCalculator<FlatCalculator>().BaseRate = 10.0M;

			var dstorChargeCode = Helper.CreateChargeCode("DSTOR2", "Storage", ChargeCodeGroupList.Codes.TRWReceive, "");
			var dstorRateLine = rateEntry.AddRateLine(dstorChargeCode, TimeCalculator.Code, QuantityUnit.PK);
			var dstorCalculator = dstorRateLine.Calculator;
			dstorCalculator.AddRateLineItem(Calculator.Items.Operator.UNT, ZDecimal.Zero, 5, QuantityUnit.DY);

			var untChargeCode = Helper.CreateChargeCode("UNTTRC", "Per Unit Charge", ChargeCodeGroupList.Codes.TRWReceive, "");
			var untRateLine = rateEntry.AddRateLine(untChargeCode, UnitCalculator.Code, QuantityUnit.KG, "USD");
			untRateLine.GetCalculator<UnitCalculator>().PerUnit = 1.0M;

			Factory.Save();

			var unloadedTime = ZDateTimeOffset.Now.AddDays(-1);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK, bookedBy, consignor, consignee);
			rcn.WRC_TransportMode = TransportModes.Air;
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK, bookedBy, consignor, consignee);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, gateIn: unloadedTime.AddHours(-1), unLoadCompleteTime: unloadedTime);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu, entryNum: "JOB1", weight: 1);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = fltChargeCode.AC_Code,
					JR_OSSellAmt = 10m,
					RevenueCalculationDescription = fltChargeCode.AC_Code + @"
Receive Consignment RC1
10.00 AUD		Base Rate AUD 10.00"
				},
				new AssertionCharge
				{
					ChargeCode = untChargeCode.AC_Code,
					JR_OSSellAmt = 1m,
					RevenueCalculationDescription = untChargeCode.AC_Code + @"
Receive Consignment RC1
1.00 USD		1 Kilogram(s) @ USD 1.00/KG"
				},
				new AssertionCharge
				{
					ChargeCode = dstorChargeCode.AC_Code,
					JR_OSSellAmt = 5m,
					RevenueCalculationDescription = dstorChargeCode.AC_Code + @"
Receive Consignment RC1
5.00 AUD		1 Package x Day @ AUD 5.00/Package x Day"
				}
			};

			EnterpriseServiceAutorateAndAssert(expected, rcn, bookedBy);
		}

		#endregion

		#region Implementation

		RateEntry CreateRateEntryWithUnitCalculator(ClientRate clientRate, AccChargeCode chargeCode, string rateCategory, WhsWarehouse warehouse, ZDecimal perUnitCost, string currencyCode)
		{
			var entry = Helper.CreateRateEntry(clientRate, rateCategory, warehouse);

			var rateLine = entry.AddRateLine(chargeCode.AC_Code, UnitCalculator.Code, currencyCode);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = perUnitCost;

			return entry;
		}

		AccChargeCode CreateChargeCode(string chargeCode, string rateCategory, string description, string chargeCodeGroup, string chargeSubCodeGroup = "")
		{
			return Helper.CreateChargeCode(chargeCode, $"{rateCategory} - {chargeCodeGroup} {description}", chargeCodeGroup, chargeSubCodeGroup);
		}

		new WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;

		#endregion
	}
}
