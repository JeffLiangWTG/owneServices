using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	partial class DeclarationSynchroniserTest
	{
		public void TestSynchroniseBillsInAirMode()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
			var mb1 = dec.Bills.AddNew();
			mb1.CU_BillNum = "MB1";
			mb1.CU_BillType = BillTypeList.Codes.MasterBill;
			var mb2 = dec.Bills.AddNew();
			mb2.CU_BillNum = "MB2";
			mb2.CU_BillType = BillTypeList.Codes.MasterBill;
			var hb = dec.Bills.AddNew();
			hb.CU_BillNum = "HB";
			hb.CU_BillType = BillTypeList.Codes.HouseBill;
			hb.CU_CU_ParentBill = mb1.PK;

			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentTableCode = dec.TablePrefix;
			header.BH_ParentID = dec.PK;

			header.BH_OverrideFreightDefaults = true;
			header.BH_OverrideFreightDefaults = false;

			AssertEquals(2, header.Bills.Count);
			AssertNotNull(header.Bills.FirstOrDefault(x => x.B0_HouseBillNumber == "HB" && x.B0_MasterBillNumber == "MB1"));
			AssertNotNull(header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "MB2"));

			hb.CU_CU_ParentBill = mb2.PK;
			AssertNotNull(header.Bills.FirstOrDefault(x => x.B0_HouseBillNumber == "HB" && x.B0_MasterBillNumber == "MB2"));
			AssertNotNull(header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "MB1"));

			mb2.CU_BillNum = "MB3";
			AssertNotNull(header.Bills.FirstOrDefault(x => x.B0_HouseBillNumber == "HB" && x.B0_MasterBillNumber == "MB3"));
			AssertNotNull(header.Bills.FirstOrDefault(x => x.B0_MasterBillNumber == "MB1"));
		}

		public void TestSynchronizeBill()
		{
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;

			AssertEquals("header.Bills.Count", 1, header.Bills.Count);
			var inBondBill = header.Bills[0];

			AssertEquals("inBondBill.B0_IssuerCode", ZString.Empty, inBondBill.B0_IssuerCode);
			AssertEquals("inBondBill.B0_IssuerCodeInfo.ReadOnly", true, inBondBill.B0_IssuerCodeInfo.ReadOnly);
			bill.US_UI_NKBillIssuerSCAC = "ABDC";
			AssertEquals("inBondBill.B0_IssuerCode", "ABDC", inBondBill.B0_IssuerCode);
			AssertEquals("inBondBill.B0_IssuerCodeInfo.ReadOnly", true, inBondBill.B0_IssuerCodeInfo.ReadOnly);

			AssertEquals("inBondBill.B0_MasterBillNumber", ZString.Empty, inBondBill.B0_MasterBillNumber);
			AssertEquals("inBondBill.B0_MasterBillNumberInfo.ReadOnly", true, inBondBill.B0_MasterBillNumberInfo.ReadOnly);
			bill.CU_BillNum = "MB1";
			AssertEquals("inBondBill.B0_MasterBillNumber", "MB1", inBondBill.B0_MasterBillNumber);
			bill.CU_BillNum = "MB1-123";
			AssertEquals("inBondBill.B0_MasterBillNumber", "MB1123", inBondBill.B0_MasterBillNumber);
			AssertEquals("inBondBill.B0_MasterBillNumberInfo.ReadOnly", true, inBondBill.B0_MasterBillNumberInfo.ReadOnly);

			AssertEquals("inBondBill.B0_ManifestQty", ZInt.Zero, inBondBill.B0_ManifestQty);
			AssertEquals("inBondBill.B0_ManifestQtyInfo.ReadOnly", true, inBondBill.B0_ManifestQtyInfo.ReadOnly);
			declaration.JE_TotalNoOfPacks = 145;
			bill.CU_NoOfPacks = 151.51m;
			AssertEquals("inBondBill.B0_ManifestQty", 145, inBondBill.B0_ManifestQty);
			AssertEquals("inBondBill.B0_ManifestQtyInfo.ReadOnly", true, inBondBill.B0_ManifestQtyInfo.ReadOnly);

			AssertEquals("inBondBill.B0_ManifestUQ", "PK", inBondBill.B0_ManifestUQ);
			AssertEquals("inBondBill.B0_ManifestUQInfo.ReadOnly", true, inBondBill.B0_ManifestUQInfo.ReadOnly);
			declaration.JE_TotalNoOfPacksPackType = "GD";
			bill.CU_PackType = "DK";
			AssertEquals("inBondBill.B0_ManifestUQ", "GD", inBondBill.B0_ManifestUQ);
			AssertEquals("inBondBill.B0_ManifestUQInfo.ReadOnly", true, inBondBill.B0_ManifestUQInfo.ReadOnly);

			var bill2 = declaration.Bills.AddNew();
			bill.CU_NoOfPacks = 151.51m;
			AssertEquals("inBondBill.B0_ManifestQty", 145, inBondBill.B0_ManifestQty);
			AssertEquals("inBondBill.B0_ManifestQtyInfo.ReadOnly", true, inBondBill.B0_ManifestQtyInfo.ReadOnly);
			AssertEquals("inBondBill.B0_ManifestUQ", "GD", inBondBill.B0_ManifestUQ);
			AssertEquals("inBondBill.B0_ManifestUQInfo.ReadOnly", true, inBondBill.B0_ManifestUQInfo.ReadOnly);

			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("inBondBill.B0_ManifestQty", 151, inBondBill.B0_ManifestQty);
			AssertEquals("inBondBill.B0_ManifestQtyInfo.ReadOnly", true, inBondBill.B0_ManifestQtyInfo.ReadOnly);
			AssertEquals("inBondBill.B0_ManifestUQ", "DK", inBondBill.B0_ManifestUQ);
			AssertEquals("inBondBill.B0_ManifestUQInfo.ReadOnly", true, inBondBill.B0_ManifestUQInfo.ReadOnly);

			AssertEquals("inBondBill.B0_Weight", ZDecimal.Zero, inBondBill.B0_Weight);
			AssertEquals("inBondBill.B0_WeightInfo.ReadOnly", true, inBondBill.B0_WeightInfo.ReadOnly);
			bill.US_Weight = 563.44m;
			AssertEquals("inBondBill.B0_Weight", 563.44m, inBondBill.B0_Weight);
			AssertEquals("inBondBill.B0_WeightInfo.ReadOnly", true, inBondBill.B0_WeightInfo.ReadOnly);

			AssertEquals("inBondBill.B0_WeightUQ", ZString.Empty, inBondBill.B0_WeightUQ);
			AssertEquals("inBondBill.B0_WeightUQInfo.ReadOnly", true, inBondBill.B0_WeightUQInfo.ReadOnly);
			bill.US_WeightUQ = "KG";
			AssertEquals("inBondBill.B0_WeightUQ", "KG", inBondBill.B0_WeightUQ);
			AssertEquals("inBondBill.B0_WeightUQInfo.ReadOnly", true, inBondBill.B0_WeightUQInfo.ReadOnly);

			AssertEquals("inBondBill.B0_Volume", ZDecimal.Zero, inBondBill.B0_Volume);
			AssertEquals("inBondBill.B0_VolumeInfo.ReadOnly", false, inBondBill.B0_VolumeInfo.ReadOnly);
			bill.US_Volume = 43.34m;
			AssertEquals("inBondBill.B0_Volume", 43.34m, inBondBill.B0_Volume);
			AssertEquals("inBondBill.B0_VolumeInfo.ReadOnly", false, inBondBill.B0_VolumeInfo.ReadOnly);

			AssertEquals("inBondBill.B0_VolumeUQ", ZString.Empty, inBondBill.B0_VolumeUQ);
			AssertEquals("inBondBill.B0_VolumeUQInfo.ReadOnly", false, inBondBill.B0_VolumeUQInfo.ReadOnly);
			bill.US_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			AssertEquals("inBondBill.B0_VolumeUQ", VolumeUnitList.Codes.CubicDecimeters, inBondBill.B0_VolumeUQ);
			AssertEquals("inBondBill.B0_VolumeUQInfo.ReadOnly", false, inBondBill.B0_VolumeUQInfo.ReadOnly);
			bill.US_VolumeUQ = Core.Constants.Volume.CubicInches;
			AssertEquals("inBondBill.B0_VolumeUQ", VolumeUnitList.Codes.CubicInches, inBondBill.B0_VolumeUQ);
			AssertEquals("inBondBill.B0_VolumeUQInfo.ReadOnly", false, inBondBill.B0_VolumeUQInfo.ReadOnly);

			declaration.Packages.RemoveAndDeleteAll();
			AssertEquals("inBondBill.MoveDetails.Count", 1, inBondBill.MoveDetails.Count);
			var moveDetail = inBondBill.MoveDetails[0];
			AssertEquals("moveDetail.Containers.Count", 0, moveDetail.Containers.Count);

			var package = declaration.Packages.AddNew();
			package.CW_MarksAndNos = "MARKS 1";
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			AssertEquals("moveDetail.Containers.Count", 1, moveDetail.Containers.Count);
			var inBondContainer = moveDetail.Containers[0];
			AssertEquals("inBondContainer.Commodities.Count", 1, inBondContainer.Commodities.Count);
			var commodity = inBondContainer.Commodities[0];
			AssertEquals("commodity.BY_MarksAndNumbers", "MARKS 1", commodity.BY_MarksAndNumbers);
		}

		public void TestSynchronizeBillOrganizations()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_RL_NKClosestPort = "USLAX";
			importer.OH_FullName = "First importer";
			var importerAddress = importer.Addresses.AddNew();
			importerAddress.OA_Address1 = "Test for synch";
			importerAddress.OA_City = "city";
			var importerContact = importer.Contacts.AddNew();
			importerContact.OC_ContactName = "IMPORTER";
			importerContact.OC_Email = "importer@test.com";

			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerAddress.PK;
			declaration.ImporterDocumentaryAddress.ContactPK = importerContact.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUMEL";
			supplier.OH_FullName = "FIRST SUPPLIER";
			var supplierAddress = supplier.Addresses.AddNew();
			supplierAddress.OA_Address1 = "Test for supplier synch";
			supplierAddress.OA_City = "test city";
			var supplierContact = supplier.Contacts.AddNew();
			supplierContact.OC_ContactName = "SUPPLIER";
			supplierContact.OC_Email = "supplier@test.com";
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
			declaration.SupplierDocumentaryAddress.ContactPK = supplierContact.PK;

			var notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFY PARTY 1";
			var notifyPartyAddress = notifyParty.Addresses.AddNew();
			notifyPartyAddress.OA_Address1 = "TEST 2";
			notifyPartyAddress.OA_City = "CITY2";
			declaration.DocAddresses.FindOrCreateWithDocAddressType(notifyPartyAddress.PK, MasterFiles.Integration.DocAddressType.NotifyParty);

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB1";

			AssertEquals("header.Bills.Count", 1, header.Bills.Count);
			var inBondBill = header.Bills[0];
			AssertEquals("inBondBill.B0_MasterBillNumber", "MB1", inBondBill.B0_MasterBillNumber);

			var inBondForeignShipper = inBondBill.ForeignShipper;
			AssertEquals(supplierAddress.PK, inBondForeignShipper.E2_OA_Address);
			AssertEquals(supplierContact.OC_ContactName, inBondBill.ForeignShipper.E2_Contact);
			AssertEquals("inBondForeignShipper.ReadOnly", true, inBondForeignShipper.ReadOnly);

			var inBondConsignee = inBondBill.Consignee;
			AssertEquals(importerAddress.PK, inBondConsignee.E2_OA_Address);
			AssertEquals(importerContact.OC_ContactName, inBondConsignee.E2_Contact);
			AssertEquals("inBondConsignee.ReadOnly", true, inBondConsignee.ReadOnly);

			var inBondNotifyParty = inBondBill.NotifyParty;
			AssertEquals(notifyPartyAddress.PK, inBondNotifyParty.E2_OA_Address);
			AssertEquals("inBondNotifyParty.ReadOnly", true, inBondNotifyParty.ReadOnly);

			//address override
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(true, inBondConsignee.E2_AddressOverride);
			AssertEquals("First importer", inBondConsignee.E2_CompanyName);
			AssertEquals("Test for synch", inBondConsignee.E2_Address1);
			AssertEquals("city", inBondConsignee.E2_City);
			AssertEquals("US", inBondConsignee.E2_RN_NKCountryCode);
			AssertEquals("", inBondConsignee.E2_Postcode);
			AssertEquals("IMPORTER", inBondConsignee.E2_Contact);
			AssertEquals("importer@test.com", inBondConsignee.E2_Email);
			AssertEquals("", inBondConsignee.E2_Phone);

			declaration.SupplierDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(true, inBondForeignShipper.E2_AddressOverride);
			AssertEquals("FIRST SUPPLIER", inBondForeignShipper.E2_CompanyName);
			AssertEquals("Test for supplier synch", inBondForeignShipper.E2_Address1);
			AssertEquals("test city", inBondForeignShipper.E2_City);
			AssertEquals("US", inBondForeignShipper.E2_RN_NKCountryCode);
			AssertEquals("", inBondForeignShipper.E2_Postcode);
			AssertEquals("SUPPLIER", inBondForeignShipper.E2_Contact);
			AssertEquals("supplier@test.com", inBondForeignShipper.E2_Email);
			AssertEquals("", inBondForeignShipper.E2_Phone);

			declaration.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			AssertEquals(true, inBondNotifyParty.E2_AddressOverride);
			AssertEquals("NOTIFY PARTY 1", inBondNotifyParty.E2_CompanyName);
			AssertEquals("TEST 2", inBondNotifyParty.E2_Address1);
			AssertEquals("CITY2", inBondNotifyParty.E2_City);
			AssertEquals("US", inBondNotifyParty.E2_RN_NKCountryCode);
			AssertEquals("", inBondNotifyParty.E2_Postcode);
			AssertEquals("The Import Manager", inBondNotifyParty.E2_Contact);
			AssertEquals("", inBondNotifyParty.E2_Email);
			AssertEquals("", inBondNotifyParty.E2_Phone);
		}

		public void TestSynchronizeBillIssuerCode()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableENS = true;
				declaration.JE_MasterBillIssuerSCAC = "AA";
				declaration.JE_MasterBill = "00198765432";
				declaration.JE_HouseBillIssuerSCAC = "BB";
				declaration.JE_HouseBill = "INC280222";

				var header = Factory.New<CusInBondHeader>();
				header.BH_ParentTableCode = declaration.TablePrefix;
				header.BH_ParentID = declaration.PK;

				header.BH_OverrideFreightDefaults = true;
				header.BH_OverrideFreightDefaults = false;
				AssertEquals("header.Bills.Count", 1, header.Bills.Count);
				CombineAssertions("Sync for AIR", () =>
				{
					var bill = header.Bills[0];
					AssertEquals("bill.B0_IssuerCode", "AA", bill.B0_IssuerCode);
					AssertEquals("bill.B0_MasterBillNumber", "00198765432", bill.B0_MasterBillNumber);
					AssertEquals("bill.B0_HouseBillIssuerCode", ZString.Empty, bill.B0_HouseBillIssuerCode);
					AssertEquals("bill.B0_HouseBillNumber", "INC280222", bill.B0_HouseBillNumber);
				});

				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_MasterBillIssuerSCAC = "APLU";
				declaration.JE_MasterBill = "MB28022022";
				declaration.JE_HouseBillIssuerSCAC = "SCAC";
				declaration.JE_HouseBill = "HB28022022";
				header.BH_OverrideFreightDefaults = true;
				header.BH_OverrideFreightDefaults = false;
				AssertEquals("header.Bills.Count", 1, header.Bills.Count);
				CombineAssertions("Sync for SEA", () =>
				{
					var bill = header.Bills[0];
					AssertEquals("bill.B0_IssuerCode", "APLU", bill.B0_IssuerCode);
					AssertEquals("bill.B0_MasterBillNumber", "MB28022022", bill.B0_MasterBillNumber);
					AssertEquals("bill.B0_HouseBillIssuerCode", "SCAC", bill.B0_HouseBillIssuerCode);
					AssertEquals("bill.B0_HouseBillNumber", "HB28022022", bill.B0_HouseBillNumber);
				});
			}
		}
	}
}
