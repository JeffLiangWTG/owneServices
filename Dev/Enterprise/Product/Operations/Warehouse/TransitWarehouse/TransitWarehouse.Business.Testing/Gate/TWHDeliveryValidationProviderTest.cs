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
	public class TWHDeliveryValidationProviderTest : TestCaseWithFactory
	{
		#region Facility Not Found

		public void TestGet_WhenNonMatchingOrgCode_ThenRejectResponseWithFNF()
		{
			TestValidationResult(null, "INVALID", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.FNF);
		}

		public void TestGet_WhenNonMatchingOrgAndAddressCode_ThenRejectResponseWithFNF()
		{
			TestValidationResult(null, "COMPANY1", "INVALID", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.FNF);
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

		public void TestGet_GivenNoReferenceType_WhenMatchesRTU_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "VEH1", null, ValidationResult.Accept, nameof(ReferenceNumberTypes.VehicleReference), 8, Weight.Kilograms, 9, Volume.CubicMetres, 2);
		}

		public void TestGet_GivenNoReferenceType_WhenMatchesHouseBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "HB_RCN4", null, ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), (decimal)30.1, Weight.Kilograms, (decimal)50.5, Volume.CubicMetres);
		}

		public void TestGet_GivenNoReferenceType_WhenMatchesMasterBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", "COMPANY1", "ADDRESS1", "MB_ASN3", null, ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 3, Weight.Kilograms, 3, Volume.CubicMetres);
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

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "CON2", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ContainerNumber), 2, Weight.Kilograms, 2, Volume.CubicMetres, 2);
		}

		public void TestGet_GivenMasterBill_WhenMatchWithASNMasterBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "MB_ASN3", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 3, Weight.Kilograms, 3, Volume.CubicMetres);
		}

		public void TestGet_GivenHouseBill_WhenMatchWithRCNHouseBill_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "HB_RCN4", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), (decimal)30.1, Weight.Kilograms, (decimal)50.5, Volume.CubicMetres);
		}

		public void TestGet_GivenRCNReferenceType_WhenMatchesRCN_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "RCNRef001", nameof(ReferenceNumberTypes.ReceiveConsignment), ValidationResult.Accept, nameof(ReferenceNumberTypes.ReceiveConsignment), 102, Weight.Kilograms, 103, Volume.CubicMetres);
		}

		public void TestGet_GivenForwardingShipmentNumber_WhenMatchesRCN_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "FSH_RCN001", nameof(ReferenceNumberTypes.ForwardingShipmentNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ForwardingShipmentNumber), 350, Weight.Kilograms, 35, Volume.CubicMetres);
		}

		public void TestGet_GivenASNReferenceType_WhenMatchesASN_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "ASNRef001", nameof(ReferenceNumberTypes.AdvancedShippingNotice), ValidationResult.Accept, nameof(ReferenceNumberTypes.AdvancedShippingNotice), 450, Weight.Kilograms, 45, Volume.CubicMetres);
		}

		public void TestGet_GivenForwardingConsolNumber_WhenMatchesASN_ThenReturnAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "FCO_ASN001", nameof(ReferenceNumberTypes.ForwardingConsolNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ForwardingConsolNumber), 77, Weight.Kilograms, 7, Volume.CubicMetres);
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

		public void TestGet_GivenInvalidReferenceNumberType_ThenRejectWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "INVALID", "INVALIDNUMBERREFERENCETYPE", ValidationResult.Decline, ValidationErrorCode.INV);
		}

		#endregion

		#region Job Not Found

		public void TestGet_WhenMatchingReferenceNumber_IncorrectReferenceType_Vehicle_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "CON2", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_WhenMatchingReferenceNumber_IncorrectReferenceType_Container_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_WhenNonMatchingReferenceNumber_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "INVALID", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenNoReferenceNumberType_WhenReferenceNumberDoesNotMatch_ThenAcceptResponse()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "INVALID", ZString.Empty, ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenCompletedRCN_WhenMatchByHouseBill_ThenRejectResponseWithJNF()
		{
			TestValidationResult("CC1234", "COMPANY1", "ADDRESS1", "HB_RCN9", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		#endregion

		#region Given Reference Number Type Then Match Only

		public void TestGet_GivenContainerNumberReferenceNumberType_ThenMatchOnlyForRTUs()
		{
			TestValidationResult("", "COMPANY1", "ADDRESS1", "MB_ASN3", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenMasterBillReferenceNumberType_ThenMatchOnlyForASNs()
		{
			TestValidationResult("", "COMPANY1", "ADDRESS1", "HB_RCN4", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		public void TestGet_GivenHouseBillReferenceNumberType_ThenMatchOnlyForRCNs()
		{
			TestValidationResult("", "COMPANY1", "ADDRESS1", "VEH1", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Decline, ValidationErrorCode.JNF);
		}

		#endregion

		#region Dangerous Goods Violation

		public void TestGet_WhenViolatesDangerousGoodsValidation_ThenRejectResponseWithVDG()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "COMPANY1";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_Code = "ADDRESS1";

			var orgCusCode = org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "", string.Empty);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;

			var warehouse = Helper.CreateTRWWarehouse();
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = orgAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu = Helper.CreateReceiveTransportationUnit("RTUX", warehouse.PK, location.PK, "VEH1");
			var asn1 = Helper.CreateReceiveASN("ASNX", warehouse.PK);
			var asnPivot = Helper.CreateReceiveASNRTUPivot(rtu.PK, asn1.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCNX", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn1, 1, "BOX", "PKG1", "BKD", receiveASN: asn1);

			var undgSubstance1 = Helper.CreateUNDGSubstance("ABCD", "1.1D", "ABCDa");
			var undgSubstance2 = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
			var undgSubstance3 = Helper.CreateUNDGSubstance("CDEF", "2.5D", "CDEFa");
			var undgSubstance4 = Helper.CreateUNDGSubstance("DEFG", "3.5D", "DEFGa");

			Factory.Save();

			var undgDataItem1 = Helper.CreateUNDGDataItem(packageState.WPS_KP_Package, "KP", undgSubstance1, 101000, 76000, "G", "L");
			var undgDataItem2 = Helper.CreateUNDGDataItem(packageState.WPS_KP_Package, "KP", undgSubstance2, 50000, 50000, "G", "L");
			var undgDataItem3 = Helper.CreateUNDGDataItem(packageState.WPS_KP_Package, "KP", undgSubstance3, 250000, 300000, "G", "L");
			var undgDataItem4 = Helper.CreateUNDGDataItem(packageState.WPS_KP_Package, "KP", undgSubstance4, 251000, 301000, "G", "L");
			var countryReference = Helper.CreateCountryReference(referenceCode: "1234", country: "AU");
			Helper.CreateCountryReferencePivot(countryReference: countryReference, dgCode: "CDEF");
			Helper.CreateCountryReferencePivot(countryReference: countryReference, dgCode: "DEFG");

			Helper.CreateWhsUNDGLimit(warehouse, undgSubstanceCode: "ABCD", totalWeightLimit: 100, totalWeightLimitUQ: "KG", totalVolumeLimit: 75, totalVolumeLimitUQ: "M3");
			Helper.CreateWhsUNDGLimit(warehouse, undgClass: "1", totalWeightLimit: 150, totalWeightLimitUQ: "KG", totalVolumeLimit: 125, totalVolumeLimitUQ: "M3");
			Helper.CreateWhsUNDGLimit(warehouse, countryReference: countryReference, totalWeightLimit: 500, totalWeightLimitUQ: "KG", totalVolumeLimit: 600, totalVolumeLimitUQ: "M3");

			Factory.Save();

			ITWHValidationProvider dataProvider = new TWHDeliveryValidationProvider();
			var request = new TWHValidationRequest()
			{
				FacilityCode = null,
				OrgCode = "COMPANY1",
				AddressCode = "ADDRESS1",
				ReferenceNumber = "VEH1",
				ReferenceNumberType = nameof(ReferenceNumberTypes.VehicleReference),
			};
			var result = (TWHValidationResponse)dataProvider.Get(request);

			AssertEquals(ValidationResult.Decline, result.Result);
			AssertEquals(ValidationErrorCode.VDG, result.MessageCode);
			AssertEquals("Violation of Dangerous Goods Validation\r\nDG ABCDa would put the whs. at 101% weight capacity.\r\nDG ABCDa would put the whs. at 101% volume capacity.\r\nICPE 1234 would put the whs. at 100% weight capacity.\r\nICPE 1234 would put the whs. at 100% volume capacity.\r\nUNDG Class 1 would put the whs. at 101% weight capacity.\r\nUNDG Class 1 would put the whs. at 101% volume capacity.", result.Message);
		}

		#endregion

		#region Multiple Job Matches

		public void TestGet_GivenMultipleASNs_WhenMatchByMasterBill_ThenChooseLastASN()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "MB_ASNX", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 4, Weight.Kilograms, 13, Volume.CubicMetres);
		}

		public void TestGet_GivenMultipleRCNs_WhenMatchByHouseBill_ThenChooseLastRCN()
		{
			SetupTestData("COMPANY1", "ADDRESS1", "CC_123");

			TestValidationResult("CC_123", string.Empty, string.Empty, "HB_RCNX", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), 17, Weight.Kilograms, 133, Volume.CubicMetres);
		}

		#endregion

		#region Given Job Where Some Packages Are Booked

		public void TestGet_GivenRTUWithSomeBookedPackages_WhenMatchByReference_ThenAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "CON9", nameof(ReferenceNumberTypes.ContainerNumber), ValidationResult.Accept, nameof(ReferenceNumberTypes.ContainerNumber), 111, Weight.Kilograms, 111, Volume.CubicMetres);
		}

		public void TestGet_GivenASNWithSomeBookedPackages_WhenMatchByMasterBill_ThenAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "MB_ASN12", nameof(ReferenceNumberTypes.MasterBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.MasterBill), 545, Weight.Kilograms, 545, Volume.CubicMetres);
		}

		public void TestGet_GivenRCNWithSomeBookedPackages_WhenMatchByHouseBill_ThenAcceptResponse()
		{
			SetupTestData("COMPANY1", "ADDRESS1", string.Empty);

			TestValidationResult(string.Empty, "COMPANY1", "ADDRESS1", "HB_RCN10", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Accept, nameof(ReferenceNumberTypes.HouseBill), 22, Weight.Kilograms, 22, Volume.CubicMetres);
		}

		#endregion

		#region Given Two Warehouses

		public void TestGet_GivenWarehouse1And2_WhenRTUIsForWarehouse1AndRequestChecksWarehouse2ViaReference_ThenRejectResponseWithJNF()
		{
			(var warehouse1, var warehouse2) = SetupTestDataForTwoWarehouses();
			var row = Helper.CreateRowAndGenerateLocations(warehouse1, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu = Helper.CreateReceiveTransportationUnit("RTUX", warehouse1.PK, location.PK, "VEH1");
			var asn = Helper.CreateReceiveASN("ASNX", warehouse1.PK);
			var asnPivot = Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var rcn = Helper.CreateReceiveConsignment("RCNX", warehouse1.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "PKG1", "BKD", receiveASN: asn);

			Factory.Save();

			TestValidationResult("CCABC2", "COMPANY2", "ADDRESS2", "VEH1", nameof(ReferenceNumberTypes.VehicleReference), ValidationResult.Decline, null, null, null, null, null, null, expectedErrorCode: ValidationErrorCode.JNF);
		}

		public void TestGet_GivenWarehouse1And2_WhenRCNIsInWarehouse1AndRequestChecksWarehouse2ViaHouseBill_ThenRejectResponseWithJNF()
		{
			(var warehouse1, var warehouse2) = SetupTestDataForTwoWarehouses();
			var row1 = Helper.CreateRowAndGenerateLocations(warehouse1, "Dock", 2, 2);
			var location1 = row1.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu = Helper.CreateReceiveTransportationUnit("RTUX", warehouse1.PK, location1.PK, "VEH1");
			var asn = Helper.CreateReceiveASN("ASNX", warehouse1.PK);
			var asnPivot = Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var rcn = Helper.CreateReceiveConsignment("RCNX", warehouse1.PK);
			rcn.WRC_HouseBillNumber = "HB1_CCABC1";

			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "PKG1", "BKD", receiveASN: asn);

			Factory.Save();

			TestValidationResult("CCABC2", "COMPANY2", "ADDRESS2", "HB1_CCABC1", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Decline, null, null, null, null, null, null, expectedErrorCode: ValidationErrorCode.JNF);
		}

		public void TestGet_GivenWarehouse1And2_WhenASNIsForWarehouse1AndRequestChecksWarehouse2ViaMasterBill_ThenRejectResponseWithJNF()
		{
			(var warehouse1, var warehouse2) = SetupTestDataForTwoWarehouses();

			var row = Helper.CreateRowAndGenerateLocations(warehouse1, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu = Helper.CreateReceiveTransportationUnit("RTUX", warehouse1.PK, location.PK, "VEH1");
			var asn = Helper.CreateReceiveASN("ASNX1", warehouse1.PK);
			var asnPivot = Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var masterbill = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill.CE_ParentID = asn.PK;
			masterbill.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill.CE_EntryNum = "MB1_CCABC1";
			masterbill.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var rcn = Helper.CreateReceiveConsignment("RCNX", warehouse1.PK);

			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "PKG1", "BKD", receiveASN: asn);

			Factory.Save();

			TestValidationResult("CCABC2", "COMPANY2", "ADDRESS2", "MB1_CCABC1", nameof(ReferenceNumberTypes.HouseBill), ValidationResult.Decline, null, null, null, null, null, null, expectedErrorCode: ValidationErrorCode.JNF);
		}

		#endregion

		void TestValidationResult(string communityCode, string org, string address, string referenceNumber, string referenceNumberType, ValidationResult expectedResult, string expectedReferenceNumberType = null, decimal? expectedWeight = 0, string expectedWeightUnit = Weight.Kilograms, decimal? expectedVolume = 0, string expectedVolumeUnit = Volume.CubicMetres, int? expectedQuantity = 1, ValidationErrorCode? expectedErrorCode = null)
		{
			ITWHValidationProvider dataProvider = new TWHDeliveryValidationProvider();
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

			ITWHValidationProvider dataProvider = new TWHDeliveryValidationProvider();
			var request = new TWHValidationRequest()
			{
				FacilityCode = communityCode,
				OrgCode = org,
				AddressCode = address,
				ReferenceNumber = referenceNumber,
				ReferenceNumberType = referenceNumberType,
			};
			var result = (TWHValidationResponse)dataProvider.Get(request);

			AssertEquals(expectedResult, result?.Result);

			if (expectedErrorCode != null)
			{
				AssertEquals(expectedErrorCode, result?.MessageCode);
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
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = orgAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			warehouse.WW_IsDangerousGoodsManagementEnabled = true;

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");

			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK, "VEH1");
			var rtu2 = Helper.CreateReceiveTransportationUnitWithContainerType("CON2", warehouse.PK, location.PK, containerID: "CON2", containerType: "20GP", vehicleRef: "CON2");
			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, location.PK, "VEH3");
			var rtu4 = Helper.CreateReceiveTransportationUnit("RTU4", warehouse.PK, location.PK, "VEH4");
			var rtu5 = Helper.CreateReceiveTransportationUnit("RTU5", warehouse.PK, location.PK, "VEH5");
			var rtu6 = Helper.CreateReceiveTransportationUnit("RTU6", warehouse.PK, location.PK, "VEH6");
			var rtu7 = Helper.CreateReceiveTransportationUnit("RTU7", warehouse.PK, location.PK, "VEH7");
			var rtu8 = Helper.CreateReceiveTransportationUnit("RTU8", warehouse.PK, location.PK, "VEH8");
			var rtu9 = Helper.CreateReceiveTransportationUnit("RTU9", warehouse.PK, location.PK, "CON9", rtuUnitType: TransportUnitTypes.Container);
			var rtu10 = Helper.CreateReceiveTransportationUnit("RTU10", warehouse.PK, location.PK, "CON10");
			var rtu11 = Helper.CreateReceiveTransportationUnit("RTU11", warehouse.PK, location.PK, "CON11");
			var rtu12 = Helper.CreateReceiveTransportationUnit("RTU12", warehouse.PK, location.PK, "VEH12");
			var rtu13 = Helper.CreateReceiveTransportationUnit("RTU13", warehouse.PK, location.PK, "VEH13");
			var rtu14 = Helper.CreateReceiveTransportationUnit("RTU14", warehouse.PK, location.PK, "VEH14");

			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			var asn3 = Helper.CreateReceiveASN("ASN3", warehouse.PK);
			var asn4 = Helper.CreateReceiveASN("ASN4", warehouse.PK);
			var asn5 = Helper.CreateReceiveASN("ASN5", warehouse.PK);
			var asn6 = Helper.CreateReceiveASN("ASN6", warehouse.PK);
			var asn7 = Helper.CreateReceiveASN("ASN7", warehouse.PK);
			var asn8 = Helper.CreateReceiveASN("ASN8", warehouse.PK);
			var asn9 = Helper.CreateReceiveASN("ASN9", warehouse.PK);
			var asn10 = Helper.CreateReceiveASN("ASN10", warehouse.PK);
			var asn11 = Helper.CreateReceiveASN("ASN11", warehouse.PK);
			var asn12 = Helper.CreateReceiveASN("ASN12", warehouse.PK);
			var asn13 = Helper.CreateReceiveASN("ASN13", warehouse.PK);
			var asn14 = Helper.CreateReceiveASN("ASN14", warehouse.PK);
			var asn15 = Helper.CreateReceiveASN("ASN15", warehouse.PK);
			var asn16 = Helper.CreateReceiveASN("ASN16", warehouse.PK);
			asn5.WRP_SystemCreateTimeUtc = ZDateTime.Now;
			asn6.WRP_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			asn15.WRP_ReferenceNumber = "ASNRef001";

			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn2.PK);
			Helper.CreateReceiveASNRTUPivot(rtu3.PK, asn3.PK);
			Helper.CreateReceiveASNRTUPivot(rtu4.PK, asn4.PK);
			Helper.CreateReceiveASNRTUPivot(rtu5.PK, asn5.PK);
			Helper.CreateReceiveASNRTUPivot(rtu5.PK, asn6.PK);
			Helper.CreateReceiveASNRTUPivot(rtu6.PK, asn7.PK);
			Helper.CreateReceiveASNRTUPivot(rtu6.PK, asn8.PK);
			Helper.CreateReceiveASNRTUPivot(rtu7.PK, asn9.PK);
			Helper.CreateReceiveASNRTUPivot(rtu8.PK, asn10.PK);
			Helper.CreateReceiveASNRTUPivot(rtu9.PK, asn11.PK);
			Helper.CreateReceiveASNRTUPivot(rtu10.PK, asn12.PK);
			Helper.CreateReceiveASNRTUPivot(rtu11.PK, asn13.PK);
			Helper.CreateReceiveASNRTUPivot(rtu12.PK, asn14.PK);
			Helper.CreateReceiveASNRTUPivot(rtu13.PK, asn15.PK);
			Helper.CreateReceiveASNRTUPivot(rtu14.PK, asn16.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rcn4 = Helper.CreateReceiveConsignment("RCN4", warehouse.PK);
			var rcn5 = Helper.CreateReceiveConsignment("RCN5", warehouse.PK);
			var rcn6 = Helper.CreateReceiveConsignment("RCN6", warehouse.PK);
			var rcn7 = Helper.CreateReceiveConsignment("RCN7", warehouse.PK);
			var rcn8 = Helper.CreateReceiveConsignment("RCN8", warehouse.PK);
			var rcn9 = Helper.CreateReceiveConsignment("RCN9", warehouse.PK);
			var rcn10 = Helper.CreateReceiveConsignment("RCN10", warehouse.PK);
			var rcn11 = Helper.CreateReceiveConsignment("RCN11", warehouse.PK);
			var rcn12 = Helper.CreateReceiveConsignment("RCN12", warehouse.PK);
			var rcn13 = Helper.CreateReceiveConsignment("RCN13", warehouse.PK);
			var rcn14 = Helper.CreateReceiveConsignment("RCN14", warehouse.PK);
			var rcn15 = Helper.CreateReceiveConsignment("RCN15", warehouse.PK);
			var rcn16 = Helper.CreateReceiveConsignment("RCN16", warehouse.PK);
			rcn4.WRC_HouseBillNumber = "HB_RCN4";
			rcn7.WRC_HouseBillNumber = "HB_RCNX";
			rcn7.WRC_SystemCreateTimeUtc = ZDateTime.Now;
			rcn8.WRC_HouseBillNumber = "HB_RCNX";
			rcn8.WRC_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			rcn9.WRC_HouseBillNumber = "HB_RCN9";
			rcn9.WRC_CompleteTime = DateTimeOffset.Now;
			rcn10.WRC_HouseBillNumber = "HB_RCN10";
			rcn13.WRC_JobID = "RCNRef001";

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1, weight: 3, weightUQ: Weight.Kilograms, volume: 7, volumeUQ: Volume.CubicMetres);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1, weight: 5, weightUQ: Weight.Kilograms, volume: 2, volumeUQ: Volume.CubicMetres);
			var wps3 = Helper.CreatePackageState(rcn2, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2, weight: 1, weightUQ: Weight.Kilograms, volume: 1, volumeUQ: Volume.CubicMetres);
			var wps4 = Helper.CreatePackageState(rcn2, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2, weight: 1, weightUQ: Weight.Kilograms, volume: 1, volumeUQ: Volume.CubicMetres);
			var wps5 = Helper.CreatePackageState(rcn3, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn3, weight: 3, weightUQ: Weight.Kilograms, volume: 3, volumeUQ: Volume.CubicMetres);
			var wps6 = Helper.CreatePackageState(rcn4, 1, "PLT", "P6", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn4, weight: (decimal)30.1, weightUQ: Weight.Kilograms, volume: (decimal)50.5, volumeUQ: Volume.CubicMetres);
			var wps7 = Helper.CreatePackageState(rcn5, 1, "PLT", "P7", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn5, weight: 4, weightUQ: Weight.Kilograms, volume: 13, volumeUQ: Volume.CubicMetres);
			var wps8 = Helper.CreatePackageState(rcn6, 1, "PLT", "P8", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn6, weight: 9, weightUQ: Weight.Kilograms, volume: 16, volumeUQ: Volume.CubicMetres);
			var wps9 = Helper.CreatePackageState(rcn7, 1, "PLT", "P9", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn7, weight: 17, weightUQ: Weight.Kilograms, volume: 133, volumeUQ: Volume.CubicMetres);
			var wps10 = Helper.CreatePackageState(rcn8, 1, "PLT", "P10", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn8, weight: 80, weightUQ: Weight.Kilograms, volume: 90, volumeUQ: Volume.CubicMetres);
			var wps11 = Helper.CreatePackageState(rcn9, 1, "PLT", "P11", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn9, receiveUnit: rtu7);
			var wps12 = Helper.CreatePackageState(rcn10, 1, "PLT", "P12", TransitWarehouseStatuses.Codes.Unpacking, receiveASN: asn10, receiveUnit: rtu8, weight: 55, weightUQ: Weight.Kilograms, volume: 55, volumeUQ: Volume.CubicMetres);
			var wps13 = Helper.CreatePackageState(rcn10, 1, "PLT", "P13", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn10, weight: 22, weightUQ: Weight.Kilograms, volume: 22, volumeUQ: Volume.CubicMetres);
			var wps14 = Helper.CreatePackageState(rcn11, 1, "PLT", "P14", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn11, weight: 111, weightUQ: Weight.Kilograms, volume: 111, volumeUQ: Volume.CubicMetres);
			var wps15 = Helper.CreatePackageState(rcn11, 1, "PLT", "P15", TransitWarehouseStatuses.Codes.Unpacking, receiveASN: asn11, receiveUnit: rtu9, weight: 22, weightUQ: Weight.Kilograms, volume: 22, volumeUQ: Volume.CubicMetres);
			var wps16 = Helper.CreatePackageState(rcn12, 1, "PLT", "P16", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn12, weight: 545, weightUQ: Weight.Kilograms, volume: 545, volumeUQ: Volume.CubicMetres);
			var wps17 = Helper.CreatePackageState(rcn12, 1, "PLT", "P17", TransitWarehouseStatuses.Codes.Unpacking, receiveASN: asn12, receiveUnit: rtu10, weight: 22, weightUQ: Weight.Kilograms, volume: 22, volumeUQ: Volume.CubicMetres);
			var wps18 = Helper.CreatePackageState(rcn13, 1, "PLT", "P18", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn13, weight: 102, weightUQ: Weight.Kilograms, volume: 103, volumeUQ: Volume.CubicMetres);
			var wps19 = Helper.CreatePackageState(rcn14, 1, "PLT", "P19", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn14, weight: 350, weightUQ: Weight.Kilograms, volume: 35, volumeUQ: Volume.CubicMetres);
			var wps20 = Helper.CreatePackageState(rcn15, 1, "PLT", "P20", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn15, weight: 450, weightUQ: Weight.Kilograms, volume: 45, volumeUQ: Volume.CubicMetres);
			var wps21 = Helper.CreatePackageState(rcn16, 1, "PLT", "P21", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn16, weight: 77, weightUQ: Weight.Kilograms, volume: 7, volumeUQ: Volume.CubicMetres);

			var masterbill1 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill1.CE_ParentID = asn3.PK;
			masterbill1.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill1.CE_EntryNum = "MB_ASN3";
			masterbill1.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var masterbill2 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill2.CE_ParentID = asn5.PK;
			masterbill2.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill2.CE_EntryNum = "MB_ASNX";
			masterbill2.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var masterbill3 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill3.CE_ParentID = asn6.PK;
			masterbill3.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill3.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill3.CE_EntryNum = "MB_ASNX";
			masterbill3.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var masterbill4 = Factory.NewWithValidTestData<CusEntryNumber>();
			masterbill4.CE_ParentID = asn12.PK;
			masterbill4.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			masterbill4.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			masterbill4.CE_EntryNum = "MB_ASN12";
			masterbill4.CE_EntryType = AdditionalReferenceTypes.Codes.MasterBill;

			var forwardingShipmentNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			forwardingShipmentNumber.CE_ParentID = rcn14.PK;
			forwardingShipmentNumber.CE_ParentTable = WhsItemReceiveConsignmentSchema.Constants.TableName;
			forwardingShipmentNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			forwardingShipmentNumber.CE_EntryNum = "FSH_RCN001";
			forwardingShipmentNumber.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.ForwardingShipmentNumber;

			var forwardingConsolNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			forwardingConsolNumber.CE_ParentID = asn16.PK;
			forwardingConsolNumber.CE_ParentTable = WhsItemReceiveASNSchema.Constants.TableName;
			forwardingConsolNumber.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			forwardingConsolNumber.CE_EntryNum = "FCO_ASN001";
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

			return (warehouse1, warehouse2);
		}

		protected WhsTransitTestHelper Helper
		{
			get { return helper ?? (helper = new WhsTransitTestHelper(Factory)); }
		}
		WhsTransitTestHelper helper;
	}
}
