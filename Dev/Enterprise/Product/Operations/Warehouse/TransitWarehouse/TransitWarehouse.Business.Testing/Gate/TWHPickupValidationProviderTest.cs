using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TWHPickupValidationProviderTest : TestCaseWithFactory
	{
		#region Facility Not Found

		public void TestGet_WhenNonMatchingOrgAndAddressCode_ThenRejectResponseWithFNF()
		{
			TestValidationResult("", "INVALID", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.FNF);
		}

		public void TestGet_WhenNonMatchingAddressCode_ThenRejectResponseWithFNF()
		{
			TestValidationResult("", "COMPANY1", "INVALID", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.FNF);
		}

		public void TestGet_GivenHouseBill_WhenNoMatchingFacility_ThenRejectResponseWithFNF()
		{
			TestValidationResult("BADCC", "INVALID", "INVALID", "VEH1", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Decline, ValidationErrorCode.FNF);
		}

		public void TestGet_GivenMasterBill_WhenNoMatchingFacility_ThenRejectResponseWithFNF()
		{
			TestValidationResult("BADCC", "INVALID", "INVALID", "VEH1", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Decline, ValidationErrorCode.FNF);
		}

		#endregion

		#region Given No Reference Number Type

		public void TestGet_GivenNoReferenceType_WhenMatchesDTU_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Accept, nameof(ReferenceNumberTypes.VehicleReference), 8, Weight.Kilograms, 9, Volume.CubicMetres, 2);
		}

		public void TestGet_GivenNoReferenceNumberType_WhenMatchMasterBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "MB_DLL3", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 6, Weight.Kilograms, 7, Volume.CubicMetres);
		}

		public void TestGet_GivenNoReferenceNumberType_WhenMatchHouseBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "HB_DCN1", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), (decimal)0.1, Weight.Kilograms, (decimal)5.5, Volume.CubicMetres);
		}

		#endregion

		#region Job Found

		public void TestGet_GivenValidVehicleReference_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Accept, nameof(ReferenceNumberTypes.VehicleReference), 8, Weight.Kilograms, 9, Volume.CubicMetres, 2);
		}

		public void TestGet_GivenValidContainerReference_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "CON1", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ContainerNumber), 2, Weight.Kilograms, 2, Volume.CubicMetres, 2);
		}

		public void TestGet_GivenMasterBill_WhenMatchWithDLLMasterBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "MB_DLL3", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 6, Weight.Kilograms, 7, Volume.CubicMetres);
		}

		public void TestGet_GivenHouseBill_WhenMatchWithDCNHouseBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "HB_DCN1", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), (decimal)0.1, Weight.Kilograms, (decimal)5.5, Volume.CubicMetres);
		}

		public void TestGet_GivenDCNReferenceType_WhenMatchesDCN_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "DCNRef001", nameof(ReferenceNumberTypes.DispatchConsignment), ValidationResult.Accept, nameof(ReferenceNumberTypes.DispatchConsignment), 114, Weight.Kilograms, 14, Volume.CubicMetres);
		}

		public void TestGet_GivenForwardingShipmentNumber_WhenMatchesDCN_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "FSH_DCN001", nameof(ReferenceNumberTypes.ForwardingShipmentNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ForwardingShipmentNumber), 115, Weight.Kilograms, 15, Volume.CubicMetres);
		}

		public void TestGet_GivenDLLReferenceType_WhenMatchesDLL_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "DLLRef001", nameof(ReferenceNumberTypes.DispatchLoadList), ValidationResult.Accept, nameof(ReferenceNumberTypes.DispatchLoadList), 116, Weight.Kilograms, 16, Volume.CubicMetres);
		}

		public void TestGet_GivenForwardingConsolNumber_WhenMatchesDLL_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "FCO_DLL001", nameof(ReferenceNumberTypes.ForwardingConsolNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ForwardingConsolNumber), 117, Weight.Kilograms, 17, Volume.CubicMetres);
		}

		public void TestGet_GivenNoReferenceNumberType_WhenReferenceNumberDoesMatch_ThenAcceptResponse()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "VEH1", ZString.Empty, ValidationResult.Accept, null);
		}

		public void TestGet_GivenCommunityCode_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Accept, nameof(ReferenceNumberTypes.VehicleReference), 8, Weight.Kilograms, 9, Volume.CubicMetres, 2);
		}

		public void TestGet_GivenNoCommunityCode_WhenMatchByOrgAndAddressCode_ThenAcceptResponse()
		{
			TestValidationResult("", "COMPANY1", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Accept, null);
		}

		#endregion

		#region Given Invalid Reference Number Type

		public void TestGet_GivenInvalidReferenceNumberType_ThenRejectWithINV()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "INVALID", "INVALIDNUMBERREFERENCETYPE", ValidationResult.Decline, ValidationErrorCode.INV);
		}

		#endregion

		#region Job Not Found

		public void TestGet_WhenMatchingReferenceNumber_IncorrectReferenceType_Vehicle_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "CON1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_WhenMatchingReferenceNumber_IncorrectReferenceType_Container_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_WhenNonMatchingReferenceNumber_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "INVALID", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenNoReferenceNumberType_WhenReferenceNumberDoesMatch_ThenRejectWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "INVALID", ZString.Empty, ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenCompletedDCN_WhenMatchByHouseBill_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "HB_DCN4", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		#endregion

		#region Given Reference Number Type Then Match Only

		public void TestGet_GivenContainerNumberReferenceNumberType_ThenMatchOnlyForDTUs()
		{
			TestValidationResult("", "COMPANY1", "ADDRESS1", "MB_DLL3", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenMasterBillReferenceNumberType_ThenMatchOnlyForDLLs()
		{
			TestValidationResult("", "COMPANY1", "ADDRESS1", "HB_DCN1", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenHouseBillReferenceNumberType_ThenMatchOnlyForDCNs()
		{
			TestValidationResult("", "COMPANY1", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		#endregion

		#region Multiple Job Matches

		public void TestGet_GivenMultipleDLLs_WhenMatchByMasterBill_ThenChooseLastDLL()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "MB_DLLX", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 271, Weight.Kilograms, 314, Volume.CubicMetres);
		}

		public void TestGet_GivenMultipleDCNs_WhenMatchByHouseBill_ThenChooseLastDCN()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "HB_DCNX", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), 37, Weight.Kilograms, 333, Volume.CubicMetres);
		}

		#endregion

		#region Given Job Where Some Packages Are Booked

		public void TestGet_GivenDTUWithSomeBookedPackages_WhenMatchByReference_ThenAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "CON8", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ContainerNumber), 999, Weight.Kilograms, 999, Volume.CubicMetres);
		}

		public void TestGet_GivenDLLWithSomeBookedPackages_WhenMatchByMasterBill_ThenAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "MB_DLL11", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 8, Weight.Kilograms, 12, Volume.CubicMetres);
		}

		public void TestGet_GivenDCNWithSomeBookedPackages_WhenMatchByHouseBill_ThenAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "HB_DCN7", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), 77, Weight.Kilograms, 77, Volume.CubicMetres);
		}

		#endregion

		#region Given Two Warehouses

		public void TestGet_GivenWarehouse1And2_WhenDTUIsForWarehouse1AndRequestChecksWarehouse2ViaReference_ThenRejectResponseWithJNF()
		{
			(var warehouse1, var warehouse2) = SetupTestDataForTwoWarehouses();

			var dtu = Helper.CreateDispatchTransportationUnit("DTUX", warehouse1.PK, "EXT1");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var rcn = Helper.CreateReceiveConsignment("RCNX", warehouse1.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse1.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll);

			Factory.Save();

			TestValidationResult("CCABC2", "COMPANY2", "ADDRESS2", "EXT1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, null, null, null, null, null, null, expectedErrorCode: ValidationErrorCode.JNF);
		}

		public void TestGet_GivenWarehouse1And2_WhenDCNIsInWarehouse1AndRequestChecksWarehouse2ViaHouseBill_ThenRejectResponseWithJNF()
		{
			(var warehouse1, var warehouse2) = SetupTestDataForTwoWarehouses();

			var dtu = Helper.CreateDispatchTransportationUnit("DTUX", warehouse1.PK, "EXT1");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var rcn = Helper.CreateReceiveConsignment("RCNX", warehouse1.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse1.PK);
			dcn.WDC_HouseBillNumber = "HB1_CCABC1";

			var packageState = Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll);

			Factory.Save();

			TestValidationResult("CCABC2", "COMPANY2", "ADDRESS2", "HB1_CCABC1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, null, null, null, null, null, null, expectedErrorCode: ValidationErrorCode.JNF);
		}

		public void TestGet_GivenWarehouse1And2_WhenDLLIsForWarehouse1AndRequestChecksWarehouse2ViaMasterBill_ThenRejectResponseWithJNF()
		{
			(var warehouse1, var warehouse2) = SetupTestDataForTwoWarehouses();

			var dtu = Helper.CreateDispatchTransportationUnit("DTUX", warehouse1.PK, "EXT1");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse1.PK);

			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCNX", warehouse1.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse1.PK);

			var packageState = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll);

			var masterbill = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill.CE_ParentID = dll.PK;
			masterbill.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill.CE_EntryNum = "MB1_CCABC1";
			masterbill.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			Factory.Save();

			TestValidationResult("CCABC2", "COMPANY2", "ADDRESS2", "MB1_CCABC1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, null, null, null, null, null, null, expectedErrorCode: ValidationErrorCode.JNF);
		}

		#endregion

		public void TestGet_GivenMultiplePackageStatesOfDifferentStatus_WhenSummingWeightAndVolume_ThenAcceptResponseWithValidPackageStates()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "COMPANYX";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_Code = "ADDRESSX";
			var orgCusCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC789", string.Empty);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;

			var warehouse = Helper.CreateTRWWarehouse();
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = orgAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, "VEH1");
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK, "VEH1");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll, weight: 8, weightUQ: Weight.Kilograms, volume: 12, volumeUQ: Volume.CubicMetres);
			var wps2 = Helper.CreatePackageState(rcn, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.AdjustedOut, dispatchLoadList: dll, dispatchConsignment: dcn, receiveUnit: rtu, weight: 123, weightUQ: Weight.Kilograms, volume: 456, volumeUQ: Volume.CubicMetres);
			var wps3 = Helper.CreatePackageState(rcn, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Departed, dispatchLoadList: dll, dispatchConsignment: dcn, receiveUnit: rtu, dispatchUnit: dtu, weight: 123, weightUQ: Weight.Kilograms, volume: 456, volumeUQ: Volume.CubicMetres);
			var wps4 = Helper.CreatePackageState(rcn, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Finalized, dispatchLoadList: dll, dispatchConsignment: dcn, receiveUnit: rtu, dispatchUnit: dtu, weight: 123, weightUQ: Weight.Kilograms, volume: 456, volumeUQ: Volume.CubicMetres);

			Factory.Save();

			ITWHValidationProvider validationProvider = new TWHPickupValidationProvider();
			var request = new TWHValidationRequest()
			{
				FacilityCode = "CC789",
				OrgCode = "COMPANYX",
				AddressCode = "ADDRESSX",
				ReferenceNumber = "VEH1",
				ReferenceNumberType = nameof(ReferenceNumberTypes.VehicleReference),
			};
			var response = (TWHValidationResponse)validationProvider.Get(request);

			CombineAssertions(() =>
			{
				AssertEquals(ValidationResult.Accept, response.Result);
				AssertEquals(nameof(ReferenceNumberTypes.VehicleReference), response.ReferenceNumberType);
				AssertEquals((decimal)8, response.GrossWeightValue);
				AssertEquals(Weight.Kilograms, response.GrossWeightUnit);
				AssertEquals((decimal)12, response.GrossVolumeValue);
				AssertEquals(Volume.CubicMetres, response.GrossVolumeUnit);
				AssertEquals(1, response.QuantityValue);
			});
		}

		void TestValidationResult(string communityCode, string org, string address, string referenceNumber, string referenceNumberType, ValidationResult expectedResult, string expectedReferenceNumberType = null, decimal? expectedWeight = 0, string expectedWeightUnit = Weight.Kilograms, decimal? expectedVolume = 0, string expectedVolumeUnit = Volume.CubicMetres, int? expectedQuantity = 1, ValidationErrorCode? expectedErrorCode = null)
		{
			ITWHValidationProvider dataProvider = new TWHPickupValidationProvider();
			var request = new TWHValidationRequest()
			{
				FacilityCode = communityCode,
				OrgCode = org,
				AddressCode = address,
				ReferenceNumber = referenceNumber,
				ReferenceNumberType = referenceNumberType,
			};
			var response = (TWHValidationResponse)dataProvider.Get(request);

			CombineAssertions(() =>
			{
				AssertEquals(expectedResult, response.Result);
				AssertEquals(expectedReferenceNumberType, response.ReferenceNumberType);
				AssertEquals(expectedWeight, response.GrossWeightValue);
				AssertEquals(expectedWeightUnit, response.GrossWeightUnit);
				AssertEquals(expectedVolume, response.GrossVolumeValue);
				AssertEquals(expectedVolumeUnit, response.GrossVolumeUnit);
				AssertEquals(expectedQuantity, response.QuantityValue);
			});

			if (expectedErrorCode != null)
			{
				AssertEquals(expectedErrorCode, response?.MessageCode);
			}
		}

		void TestValidationResult(string communityCode, string org, string address, string referenceNumber, string referenceNumberType, ValidationResult expectedResult, ValidationErrorCode? expectedErrorCode = null)
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC1234");

			ITWHValidationProvider dataProvider = new TWHPickupValidationProvider();
			var request = new TWHValidationRequest
			{
				FacilityCode = communityCode,
				OrgCode = org,
				AddressCode = address,
				ReferenceNumber = referenceNumber,
				ReferenceNumberType = referenceNumberType,
			};
			var response = (TWHValidationResponse)dataProvider.Get(request);

			AssertEquals(expectedResult, response?.Result);

			if (expectedErrorCode != null)
			{
				AssertEquals(expectedErrorCode, response?.MessageCode);
			}
		}

		void SetupTestData(string name, string address, string communityCode)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = name;
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_Code = address;
			var orgCusCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, communityCode, string.Empty);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;

			Factory.Save();

			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = orgAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, "VEH1");
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK, "VEH1");
			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, containerID: "CON1", containerType: "20GP");
			var dtu3 = Helper.CreateDispatchTransportationUnit("DTU3", warehouse.PK, "VEH3");
			var dtu4 = Helper.CreateDispatchTransportationUnit("DTU4", warehouse.PK, "VEH4");
			var dtu5 = Helper.CreateDispatchTransportationUnit("DTU5", warehouse.PK, "VEH5");
			var dtu6 = Helper.CreateDispatchTransportationUnit("DTU6", warehouse.PK, "VEH6");
			var dtu7 = Helper.CreateDispatchTransportationUnit("DTU7", warehouse.PK, "VEH7");
			var dtu8 = Helper.CreateDispatchTransportationUnit("DTU8", warehouse.PK, "CON8", dtuUnitType: TransportUnitTypes.Container);
			var dtu9 = Helper.CreateDispatchTransportationUnit("DTU9", warehouse.PK, "VEH9");
			var dtu10 = Helper.CreateDispatchTransportationUnit("DTU10", warehouse.PK, "VEH10");
			var dtu11 = Helper.CreateDispatchTransportationUnit("DTU11", warehouse.PK, "VEH11");
			var dtu12 = Helper.CreateDispatchTransportationUnit("DTU12", warehouse.PK, "VEH12");
			var dtu13 = Helper.CreateDispatchTransportationUnit("DTU13", warehouse.PK, "VEH13");
			var dtu14 = Helper.CreateDispatchTransportationUnit("DTU14", warehouse.PK, "VEH13");

			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			var dll3 = Helper.CreateDispatchLoadList("DLL3", warehouse.PK);
			var dll4 = Helper.CreateDispatchLoadList("DLL4", warehouse.PK);
			var dll5 = Helper.CreateDispatchLoadList("DLL5", warehouse.PK);
			var dll6 = Helper.CreateDispatchLoadList("DLL6", warehouse.PK);
			var dll7 = Helper.CreateDispatchLoadList("DLL7", warehouse.PK);
			var dll8 = Helper.CreateDispatchLoadList("DLL8", warehouse.PK);
			var dll9 = Helper.CreateDispatchLoadList("DLL9", warehouse.PK);
			var dll10 = Helper.CreateDispatchLoadList("DLL10", warehouse.PK);
			var dll11 = Helper.CreateDispatchLoadList("DLL11", warehouse.PK);
			var dll12 = Helper.CreateDispatchLoadList("DLL12", warehouse.PK);
			var dll13 = Helper.CreateDispatchLoadList("DLL13", warehouse.PK);
			var dll14 = Helper.CreateDispatchLoadList("DLL14", warehouse.PK);
			var dll15 = Helper.CreateDispatchLoadList("DLL15", warehouse.PK);
			var dll16 = Helper.CreateDispatchLoadList("DLL16", warehouse.PK);
			dll5.WDL_SystemCreateTimeUtc = ZDateTime.Now;
			dll6.WDL_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			dll15.WDL_JobID = "DLLRef001";

			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu2.PK);
			Helper.CreateDispatchDLLDTUPivot(dll3.PK, dtu3.PK);
			Helper.CreateDispatchDLLDTUPivot(dll4.PK, dtu4.PK);
			Helper.CreateDispatchDLLDTUPivot(dll5.PK, dtu5.PK);
			Helper.CreateDispatchDLLDTUPivot(dll6.PK, dtu5.PK);
			Helper.CreateDispatchDLLDTUPivot(dll7.PK, dtu6.PK);
			Helper.CreateDispatchDLLDTUPivot(dll8.PK, dtu6.PK);
			Helper.CreateDispatchDLLDTUPivot(dll9.PK, dtu7.PK);
			Helper.CreateDispatchDLLDTUPivot(dll10.PK, dtu8.PK);
			Helper.CreateDispatchDLLDTUPivot(dll11.PK, dtu9.PK);
			Helper.CreateDispatchDLLDTUPivot(dll12.PK, dtu10.PK);
			Helper.CreateDispatchDLLDTUPivot(dll13.PK, dtu11.PK);
			Helper.CreateDispatchDLLDTUPivot(dll14.PK, dtu12.PK);
			Helper.CreateDispatchDLLDTUPivot(dll15.PK, dtu13.PK);
			Helper.CreateDispatchDLLDTUPivot(dll16.PK, dtu14.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var dcn4 = Helper.CreateDispatchConsignment("DCN4", warehouse.PK);
			var dcn5 = Helper.CreateDispatchConsignment("DCN5", warehouse.PK);
			var dcn6 = Helper.CreateDispatchConsignment("DCN6", warehouse.PK);
			var dcn7 = Helper.CreateDispatchConsignment("DCN7", warehouse.PK);
			var dcn8 = Helper.CreateDispatchConsignment("DCN8", warehouse.PK);
			var dcn9 = Helper.CreateDispatchConsignment("DCN9", warehouse.PK);
			var dcn10 = Helper.CreateDispatchConsignment("DCN10", warehouse.PK);
			var dcn11 = Helper.CreateDispatchConsignment("DCN11", warehouse.PK);
			dcn1.WDC_HouseBillNumber = "HB_DCN1";
			dcn2.WDC_HouseBillNumber = "HB_DCNX";
			dcn3.WDC_HouseBillNumber = "HB_DCNX";
			dcn4.WDC_HouseBillNumber = "HB_DCN4";
			dcn7.WDC_HouseBillNumber = "HB_DCN7";
			dcn2.WDC_SystemCreateTimeUtc = ZDateTime.Now;
			dcn3.WDC_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			dcn4.WDC_CompleteTime = DateTimeOffset.Now;
			dcn8.WDC_JobID = "DCNRef001";

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1, weight: 3, weightUQ: Weight.Kilograms, volume: 7, volumeUQ: Volume.CubicMetres);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1, weight: 5, weightUQ: Weight.Kilograms, volume: 2, volumeUQ: Volume.CubicMetres);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll2, weight: 1, weightUQ: Weight.Kilograms, volume: 1, volumeUQ: Volume.CubicMetres);
			var wps4 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll2, weight: 1, weightUQ: Weight.Kilograms, volume: 1, volumeUQ: Volume.CubicMetres);
			var wps5 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll3, weight: 6, weightUQ: Weight.Kilograms, volume: 7, volumeUQ: Volume.CubicMetres);
			var wps6 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll4, dispatchConsignment: dcn1, weight: (decimal)0.1, weightUQ: Weight.Kilograms, volume: (decimal)5.5, volumeUQ: Volume.CubicMetres);
			var wps7 = Helper.CreatePackageState(rcn1, 1, "PLT", "P7", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll5, weight: 271, weightUQ: Weight.Kilograms, volume: 314, volumeUQ: Volume.CubicMetres);
			var wps8 = Helper.CreatePackageState(rcn1, 1, "PLT", "P8", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll6, weight: 9, weightUQ: Weight.Kilograms, volume: 16, volumeUQ: Volume.CubicMetres);
			var wps9 = Helper.CreatePackageState(rcn1, 1, "PLT", "P9", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll7, dispatchConsignment: dcn2, weight: 37, weightUQ: Weight.Kilograms, volume: 333, volumeUQ: Volume.CubicMetres);
			var wps10 = Helper.CreatePackageState(rcn1, 1, "PLT", "P10", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll8, dispatchConsignment: dcn3, weight: 19, weightUQ: Weight.Kilograms, volume: 21, volumeUQ: Volume.CubicMetres);
			var wps11 = Helper.CreatePackageState(rcn1, 1, "PLT", "P11", TransitWarehouseStatuses.Codes.Departed, dispatchLoadList: dll9, dispatchConsignment: dcn4, receiveUnit: rtu, dispatchUnit: dtu7, weight: 44, weightUQ: Weight.Kilograms, volume: 44, volumeUQ: Volume.CubicMetres);
			var wps12 = Helper.CreatePackageState(rcn1, 1, "PLT", "P12", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll10, weight: 999, weightUQ: Weight.Kilograms, volume: 999, volumeUQ: Volume.CubicMetres);
			var wps13 = Helper.CreatePackageState(rcn1, 1, "PLT", "P13", TransitWarehouseStatuses.Codes.Departed, dispatchLoadList: dll10, dispatchConsignment: dcn5, receiveUnit: rtu, dispatchUnit: dtu8, weight: 123, weightUQ: Weight.Kilograms, volume: 456, volumeUQ: Volume.CubicMetres);
			var wps14 = Helper.CreatePackageState(rcn1, 1, "PLT", "P14", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll11, weight: 8, weightUQ: Weight.Kilograms, volume: 12, volumeUQ: Volume.CubicMetres);
			var wps15 = Helper.CreatePackageState(rcn1, 1, "PLT", "P15", TransitWarehouseStatuses.Codes.Departed, dispatchLoadList: dll11, dispatchConsignment: dcn6, receiveUnit: rtu, dispatchUnit: dtu9, weight: 123, weightUQ: Weight.Kilograms, volume: 456, volumeUQ: Volume.CubicMetres);
			var wps16 = Helper.CreatePackageState(rcn1, 1, "PLT", "P16", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll12, dispatchConsignment: dcn7, weight: 77, weightUQ: Weight.Kilograms, volume: 77, volumeUQ: Volume.CubicMetres);
			var wps17 = Helper.CreatePackageState(rcn1, 1, "PLT", "P17", TransitWarehouseStatuses.Codes.Departed, dispatchLoadList: dll12, dispatchConsignment: dcn7, receiveUnit: rtu, dispatchUnit: dtu9, weight: 123, weightUQ: Weight.Kilograms, volume: 456, volumeUQ: Volume.CubicMetres);
			var wps18 = Helper.CreatePackageState(rcn1, 1, "PLT", "P18", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll13, dispatchConsignment: dcn8, weight: 114, weightUQ: Weight.Kilograms, volume: 14, volumeUQ: Volume.CubicMetres);
			var wps19 = Helper.CreatePackageState(rcn1, 1, "PLT", "P19", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll14, dispatchConsignment: dcn9, weight: 115, weightUQ: Weight.Kilograms, volume: 15, volumeUQ: Volume.CubicMetres);
			var wps20 = Helper.CreatePackageState(rcn1, 1, "PLT", "P20", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll15, dispatchConsignment: dcn10, weight: 116, weightUQ: Weight.Kilograms, volume: 16, volumeUQ: Volume.CubicMetres);
			var wps21 = Helper.CreatePackageState(rcn1, 1, "PLT", "P21", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll16, dispatchConsignment: dcn11, weight: 117, weightUQ: Weight.Kilograms, volume: 17, volumeUQ: Volume.CubicMetres);

			var masterbill1 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill1.CE_ParentID = dll3.PK;
			masterbill1.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill1.CE_EntryNum = "MB_DLL3";
			masterbill1.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var masterbill2 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill2.CE_ParentID = dll6.PK;
			masterbill2.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill2.CE_EntryNum = "MB_DLLX";
			masterbill2.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var masterbill3 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill3.CE_ParentID = dll5.PK;
			masterbill3.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill3.CE_EntryNum = "MB_DLLX";
			masterbill3.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var masterbill4 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill4.CE_ParentID = dll11.PK;
			masterbill4.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill4.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill4.CE_EntryNum = "MB_DLL11";
			masterbill4.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var forwardingShipmentNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			forwardingShipmentNumber.CE_ParentID = dcn9.PK;
			forwardingShipmentNumber.CE_ParentTable = WhsItemDispatchConsignmentSchema.Constants.TableName;
			forwardingShipmentNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			forwardingShipmentNumber.CE_EntryNum = "FSH_DCN001";
			forwardingShipmentNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber;

			var forwardingConsolNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			forwardingConsolNumber.CE_ParentID = dll16.PK;
			forwardingConsolNumber.CE_ParentTable = WhsItemDispatchLoadListSchema.Constants.TableName;
			forwardingConsolNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			forwardingConsolNumber.CE_EntryNum = "FCO_DLL001";
			forwardingConsolNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingConsolNumber;

			Factory.Save();
		}

		(WhsWarehouse whs1, WhsWarehouse whs2) SetupTestDataForTwoWarehouses()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "COMPANY1";
			org2.OH_Code = "COMPANY2";
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_OH = org1.PK;
			orgAddress2.OA_OH = org2.PK;
			orgAddress1.OA_Code = "ADDRESS1";
			orgAddress2.OA_Code = "ADDRESS2";

			var orgCusCode1 = org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CCABC1", "");
			var orgCusCode2 = org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CCABC2", "");
			orgCusCode1.OK_OA_PremisesAddress = orgAddress1.PK;
			orgCusCode2.OK_OA_PremisesAddress = orgAddress2.PK;

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			var warehouse1 = Helper.CreateTRWWarehouse("WH1");
			warehouse1.WW_GB_RelatedCompanyBranch = branch1.PK;
			warehouse1.WW_OA_WarehouseAddress = orgAddress1.PK;
			warehouse1.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var warehouse2 = Helper.CreateTRWWarehouse("WH2");
			warehouse2.WW_GB_RelatedCompanyBranch = branch2.PK;
			warehouse2.WW_OA_WarehouseAddress = orgAddress2.PK;
			warehouse2.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var row1 = Helper.CreateRowAndGenerateLocations(warehouse1, "Dock", 2, 2);
			var location1 = row1.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			return (warehouse1, warehouse2);
		}

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;
	}
}
