using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USACEFDAAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGetProgramCodeRelatedProcessingCodeList()
		{
			CombineAssertions("GetProgramCodeRelatedProcessingCodeList", () =>
			{
				AssertEquals("Bla", "ALG, BBA, BDP, BLD, BLO, CGT, HCT, PVE, VAC, XEN, NED, RED, 804, INV, OTC, PHN, PRE, RND, ADD, CCW, DSU, FEE, NSF, PRO, REP, CSU, FFM, INV, ADE, ADR", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList("Bla").CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.BIO, "ALG, BLO, BBA, BDP, CGT, HCT, BLD, PVE, VAC, XEN", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.BIO).CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.COS, "", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.COS).CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.DEV, "NED, RED", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.DEV).CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.DRU, "INV, OTC, PHN, PRE, RND, 804", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.DRU).CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.FOO, "ADD, FEE, CCW, DSU, NSF, PRO", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.FOO).CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.RAD, "REP", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.RAD).CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.TOB, "CSU, INV, FFM", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.TOB).CodesAsString);
				AssertEquals(FDAProgramCodeList.Codes.VME, "ADE, ADR", USACEFDAAddInfoLookups.GetProgramCodeRelatedProcessingCodeList(FDAProgramCodeList.Codes.VME).CodesAsString);
			});
		}

		public void TestLists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(typeof(ACE_FDABaseUQList), fda.AddInfoLookups.FDABaseUQs.GetType());

			fda.US_ProgramCode = FDAProgramCodeList.Codes.DRU;
			AssertEquals(true, fda.AddInfoLookups.FDABaseUQs.ContainsCode(ACE_FDABaseUQList.Codes.CTR));
			AssertEquals(false, fda.AddInfoLookups.FDABaseUQs.ContainsCode(FDABaseUQList.Codes.DOZ));
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), fda.AddInfoLookups.FDAProductsList.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), fda.AddInfoLookups.FDAUQs.GetType());
			AssertEquals(typeof(FDAProgramCodeList), fda.AddInfoLookups.ProgramCodeList.GetType());
			AssertEquals(typeof(ConsigneeCollection), fda.AddInfoLookups.Consignees.GetType());
			AssertEquals(false, fda.AddInfoLookups.ProcessingCodeList.ContainsCode(FDAProcessingCodeList.Codes.BIO_BLO));
			AssertEquals(true, fda.AddInfoLookups.ProcessingCodeList.ContainsCode(FDAProcessingCodeList.Codes.DRU_PHN));

			fda.US_ProgramCode = FDAProgramCodeList.Codes.RAD;
			AssertEquals(1, fda.AddInfoLookups.ProcessingCodeList.Count);
			AssertEquals(true, fda.AddInfoLookups.ProcessingCodeList.ContainsCode(FDAProcessingCodeList.Codes.RAD_REP));
			AssertEquals(3, fda.AddInfoLookups.IdentityNumberQualifierList.Count);
			Assert(fda.AddInfoLookups.IdentityNumberQualifierList.ContainsCode(ItemIdentityNumberQualifierList.Codes.RegisteredNumber));
			Assert(fda.AddInfoLookups.IdentityNumberQualifierList.ContainsCode(ItemIdentityNumberQualifierList.Codes.ModelNumber));
			Assert(fda.AddInfoLookups.IdentityNumberQualifierList.ContainsCode(ItemIdentityNumberQualifierList.Codes.SerialNumber));

			fda.US_ProgramCode = FDAProgramCodeList.Codes.TOB;
			AssertEquals(true, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode("I"));
			AssertEquals(true, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode("L"));
			AssertEquals(false, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode(ProducerFirmTypeList.Codes.M));

			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			AssertEquals(true, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode(ProducerFirmTypeList.Codes.M));
			AssertEquals("Manufacturer", fda.AddInfoLookups.ProducerFirmTypes.GetDescriptionFromCode(ProducerFirmTypeList.Codes.M));
			AssertEquals(true, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode(ProducerFirmTypeList.Codes.C));
			AssertEquals(true, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode(ProducerFirmTypeList.Codes.G));
			AssertEquals(false, fda.AddInfoLookups.FoodFacilityRegistrationExemptionCodes.ContainsCode(FDAPriorNoticeExemptCodeList.Codes.J));

			fda.US_ProgramCode = FDAProgramCodeList.Codes.BIO;
			AssertEquals(true, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode(ProducerFirmTypeList.Codes.M));
			AssertEquals("Manufacturer", fda.AddInfoLookups.ProducerFirmTypes.GetDescriptionFromCode(ProducerFirmTypeList.Codes.M));
			AssertEquals(false, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode(ProducerFirmTypeList.Codes.C));
			AssertEquals(false, fda.AddInfoLookups.ProducerFirmTypes.ContainsCode(ProducerFirmTypeList.Codes.G));
			AssertEquals(false, fda.AddInfoLookups.FDABaseUQs.ContainsCode(FDABaseUQList.Codes.DOZ));
		}

		public void TestFDAProductsList()
		{
			var testHelper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USFDAProductCode;
			testHelper.CreateNewOrGetExistingCusCodeType(listType, "US FDA Product Code");
			testHelper.CreateNewOrGetExistingCusCodeList("US", listType,
			"24DCS18", "ALFALFA BEANS (SEEDS), JUICE OR DRINK;GLASS;ULTRAPASTEURIZED", CargoWise.Types.ZDateTime.MinSmallDateTimeValue, CargoWise.Types.ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			var productList = fda.AddInfoLookups.FDAProductsList;
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), productList.GetType());
			productList.Load();
			AssertEquals("24DCS18", productList[0].ZZD_Code);
		}

		public void TestShipperAddressList()
		{
			AssertAddressList(
				(x, y) => x.ShipperOrgPK = y,
				(x) => x.US_OA_ShipperAddress_ZAddress,
				(x) => x.AddInfoLookups.ShipperAddressList);
		}

		public void TestDeliverToPartyAddressList()
		{
			AssertAddressList(
				(x, y) => x.DeliverToPartyOrgPK = y,
				(x) => x.US_DeliverToPartyAddress_ZAddress,
				(x) => x.AddInfoLookups.DeliverToPartyAddressList);
		}

		public void TestFDAImporterAddressList()
		{
			AssertAddressList(
				(x, y) => x.FDAImporterOrgPK = y,
				(x) => x.US_FDAImporterAddress_ZAddress,
				(x) => x.AddInfoLookups.FDAImporterAddressList);
		}

		public void TestFSVPImporterAddressList()
		{
			AssertAddressList(
				(x, y) => x.FSVPImporterOrgPK = y,
				(x) => x.US_FSVPImporterAddress_ZAddress,
				(x) => x.AddInfoLookups.FSVPImporterAddressList);
		}

		void AssertAddressList(Action<ACEFDA, ZGuid> setValueToProperty, Func<ACEFDA, ZAddress> getZAddress, Func<ACEFDA, ZAddressList> getAddressList)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			var address2 = org.Addresses.AddNew();
			address1.FillWithValidTestData();
			address2.FillWithValidTestData();
			var cap1 = Factory.New<OrgAddressCapability>();
			cap1.PZ_AddressType = OrgAddressType.Delivery.Code;
			cap1.PZ_OA = address1.PK;
			var cap2 = Factory.New<OrgAddressCapability>();
			cap2.PZ_AddressType = OrgAddressType.Pickup.Code;
			cap2.PZ_OA = address2.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var fda = invoiceLine.ACE_FDALines.AddNew();

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var addressList = getAddressList(fda);
				AssertEquals(0, addressList.Count);

				setValueToProperty(fda, org.PK);
				addressList = getAddressList(fda);
				AssertEquals(3, addressList.Count);
			}

			using (USCustomsDataRegistry.Instance.EnableDocAddressForFDA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var addressList = getAddressList(fda);
				AssertEquals(0, addressList.Count);

				var zAddress = getZAddress(fda);
				zAddress.OrgPK = org.PK;
				addressList = getAddressList(fda);
				AssertEquals(3, addressList.Count);
			}
		}
	}
}
