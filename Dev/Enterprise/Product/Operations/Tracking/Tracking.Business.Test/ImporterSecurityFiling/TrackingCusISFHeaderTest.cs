using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingCusISFHeader))]
	[MasterFiles.Business.Testing.CountrySpecificTest(Constants.CountryCodes.UnitedStates)]
	sealed class TrackingCusISFHeaderTest : CusISFHeaderTest
	{
		#region Milestones

		public void TestMilestones()
		{
			var testISF = Factory.NewWithValidTestData<TrackingCusISFHeader>();
			AssertNotNull(testISF.Milestones);
			AssertEquals(0, testISF.Milestones.Count);

			var milestone1 = testISF.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "lastCompleted1";
			milestone1.P9_IsPublished = true;
			milestone1.SetMilestoneActualDateForTest(DateTime.Now);
			milestone1.P9_Sequence = 1;

			var milestone2 = testISF.WorkflowItems.Milestones.AddNew();
			milestone2.P9_Description = "lastCompleted2";
			milestone2.P9_IsPublished = true;
			milestone2.SetMilestoneActualDateForTest(DateTime.Now);
			milestone2.P9_Sequence = 1;
			AssertEquals(0, testISF.Milestones.Count);

			testISF.ReloadMilestones();
			AssertEquals(2, testISF.Milestones.Count);
		}

		#endregion

		public void TestGetOrgCusCodeMatchingCountryAndCodes()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "AUSYD";
			testOrg.CompanyData.OB_GB_ControllingBranch = ZGuid.Empty;
			Assert(testOrg.CompanyData.OB_GB_ControllingBranch.IsEmpty);
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@cargowise.com";
			testContact.OC_WebAccessEnabled = true;
			testContact.SetHashedPassword("test");

			TrackingSiteUser testSiteUser = new TrackingSiteUser();
			testSiteUser.Login(testOrg.OH_Code, testContact.OC_Email, testContact.PasswordForTesting);

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			RefCountry uSC = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);
			RefCountry australiaC = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.Australia);
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "00-1234123AU", australiaC);
			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "00-1234123US", uSC);
			organisation.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokeragePrinter, "WrongCodeUS", uSC);

			Factory.Save();
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_OH_Importer = organisation.PK;
			AssertEquals(header.BF_ImporterCode, "00-1234123US");
			AssertEquals(header.BF_ImporterCodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
		}

		public void TestDuplicate()
		{
			var originalHeader = Factory.New<TrackingCusISFHeader>();
			AssertEquals(6, originalHeader.DocAddresses.Count);

			originalHeader.Lines.AddNew();
			originalHeader.Lines.AddNew();
			AssertEquals(2, originalHeader.Lines.Count);

			var duplicateHeader = originalHeader.Duplicate();
			AssertEquals(6, duplicateHeader.DocAddresses.Count);
			AssertEquals(2, duplicateHeader.Lines.Count);

			Assert(duplicateHeader.DocAddresses[0].HasChanges);
			Assert(duplicateHeader.DocAddresses[1].HasChanges);
			Assert(duplicateHeader.DocAddresses[2].HasChanges);
			Assert(duplicateHeader.DocAddresses[3].HasChanges);
			Assert(duplicateHeader.DocAddresses[4].HasChanges);
			Assert(duplicateHeader.DocAddresses[5].HasChanges);

			Assert(duplicateHeader.Lines[0].HasChanges);
			Assert(duplicateHeader.Lines[1].HasChanges);
		}

		public void TestDocAddressesSortedBySequence()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			AssertEquals(header.DocAddresses.Count, 6);
			ZGuid addressPK = header.SellingParty.PK;
			var newAddress = header.DocAddressesExcludeManufacturer.AddNew();
			newAddress.E2_AddressType = DocAddressTypes.GetCode(Factory, DocAddressType.SellingParty);
			AssertEquals(header.DocAddresses.Count, 7);
			AssertEquals("should be same address", addressPK, header.SellingParty.PK);
		}

		public void TestCollectionContainsOnlyBills()
		{
			TrackingCusISFHeader header = Factory.New<TrackingCusISFHeader>();
			CusISFBill bill1 = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "123";
			CusISFBill bill2 = header.ReferenceDatas.AddNew();
			bill2.BB_BillType = BillTypeList.Codes.ISFBondNumber;
			bill2.BB_BillNum = "321";
			AssertEquals("both bills are in reference data collection", header.ReferenceDatas.Count, 2);
			AssertEquals("there is only one element in collection", header.BillNumbersReferences.Count, 1);
			Assert("bill with type HouseBillOfLading should be in collection", header.BillNumbersReferences.Contains(bill1));

			CusISFBill bill = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill1.BB_BillNum = "123";

			bill = header.ReferenceDatas.AddNew();
			bill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill1.BB_BillNum = "123";

			bill = header.ReferenceDatas.AddNew();

			AssertEquals("all bills are in reference data collection", header.ReferenceDatas.Count, 5);
			AssertEquals("there are four elements in BillNumbersReferences collection", header.BillNumbersReferences.Count, 4);
		}

		public void TestHasTransportsInfoForWeb()
		{
			TrackingCusISFHeader headerToTest = Factory.New<TrackingCusISFHeader>();
			AssertNotNull("Should create transport on first access", headerToTest.FirstTransport);
			AssertEquals("Transport should be SEA mode", headerToTest.FirstTransport.JW_TransportMode, Constants.TransportModes.Sea);
			Assert("Transport should not be shown because of empty fields", !headerToTest.HasTransportsInfoForWeb);
			headerToTest.Transports[0].JW_Vessel = "Titanic";
			Assert("Transport can be shown because at least one of fields is not empty", headerToTest.HasTransportsInfoForWeb);
		}

		public void TestOnSavingDeletedFirstTransportDoesNotThrowException()
		{
			var header = Factory.New<TrackingCusISFHeader>();
			AssertNotNull(header.FirstTransport);
			header.Transports.AddNew();
			AssertEquals(false, header.HasTransportsInfoForWeb);
			AssertEquals(false, header.FirstTransport.IsInDatabase);
			header.FirstTransport.Delete();
			AssertEquals(true, header.FirstTransport.IsDeleted);
			AssertEquals(true, header.Transports.Count > 0);
			AssertNoExceptionThrown(() =>
			{
				header.Factory.Save();
			});
		}

		public void TestLinesForReporting()
		{
			TrackingCusISFHeader headerToTest = Factory.New<TrackingCusISFHeader>();
			CusISFLine test = Factory.New<CusISFLine>();
			test.BL_TextProductCode = "NO!";
			test.BL_RN_NKGoodsOrigin = "DE";

			AssertEquals(string.Format("Product: NO!{0}Origin: DE{0}Tariff: {0}Manufacturer: ", System.Environment.NewLine), headerToTest.GenerateLineDetailsForEmailReporting(test));
		}

		public void TestContainerForReporting()
		{
			TrackingCusISFHeader headerToTest = Factory.New<TrackingCusISFHeader>();
			CusISFEquip test = Factory.New<CusISFEquip>();
			test.BE_EquipCode = "CD";
			test.BE_ContainerNum = "abc111";
			test.BE_ContainerISO = "1234";
			AssertEquals(string.Format("Code: CD{0}Container No: abc111{0}ISO Code: 1234", System.Environment.NewLine), headerToTest.GenerateContinerDetailsForEmailReporting(test));
		}

		public void TestReferencesForReporting()
		{
			TrackingCusISFHeader headerToTest = Factory.New<TrackingCusISFHeader>();
			CusISFBill test = headerToTest.ReferenceDatas.AddNew();
			test.BB_BillType = "BM";
			test.BB_BillNum = "a1";
			test.BB_CustomsStatus = "sln";
			AssertEquals(string.Format("Type: House Bill of Lading{0}No: a1{0}Bill Status: sln", System.Environment.NewLine), headerToTest.GenerateReferencesDetailsForEmailReporting(test));
		}

		public void TestLoggedInOrgIs_WithoutAddressOverride()
		{
			var helper = new TestHelper(Factory);
			var header = Factory.NewWithValidTestData<TrackingCusISFHeader>();
			header.Consolidator.E2_OA_Address = helper.TestOrg.MainAddress.PK;

			Factory.Save();

			header.SiteUser = helper.TestSiteUser;

			var accessControlled = header as IAccessControlled;
			Assert("Logged in Org should be a Consolidator", accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFConsolidator));
			Assert("Logged in Org should NOT be a Selling party", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFSellingParty));
			Assert("Logged in Org should NOT be a Booking Party", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFBookingParty));
			Assert("Logged in Org should NOT be a Buying Party", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFBuyingParty));
			Assert("Logged in Org should NOT be an Importer", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFImporter));
			Assert("Logged in Org should NOT be a Manufacturer", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFManufacturer));
			Assert("Logged in Org should NOT be a Ship To Location", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFShipToLocation));
			Assert("Logged in Org should NOT be a Stuffing Location", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFStuffingLocation));
			Assert("Logged in Org should NOT be a Sending Agent", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.SendingAgent));

			header.Consolidator.E2_OA_Address = ZGuid.Empty;
			header.SellingParty.E2_OA_Address = helper.TestOrg.MainAddress.PK;
			Factory.Save();

			Assert("Logged in Org should NOT be a Consolidator", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFConsolidator));
			Assert("Logged in Org should be a Selling party", accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFSellingParty));
			Assert("Logged in Org should NOT be a Booking Party", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFBookingParty));
			Assert("Logged in Org should NOT be a Buying Party", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFBuyingParty));
			Assert("Logged in Org should NOT be an Importer", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFImporter));
			Assert("Logged in Org should NOT be a Manufacturer", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFManufacturer));
			Assert("Logged in Org should NOT be a Ship To Location", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFShipToLocation));
			Assert("Logged in Org should NOT be a Stuffing Location", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.ISFStuffingLocation));
			Assert("Logged in Org should NOT be a Sending Agent", !accessControlled.LoggedInOrgIs(ISFAccessRules.Roles.SendingAgent));
		}

		public void TestGracefullyHandlesDeletedTransportObject()
		{
			var header = Factory.New<TrackingCusISFHeader>();
			AssertNotNull("Should create transport on first access", header.FirstTransport);

			header.FirstTransport.Delete();
			Assert("Should not have transport infos if deleted", !header.HasTransportsInfoForWeb);
		}
	}
}
