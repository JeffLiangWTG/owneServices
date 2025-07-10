using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBranches()
		{
			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			GlbBranch branch1 = company.Branches.AddNew();
			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = branch1.PK;
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			var branches = header.Lookups.Branches;
			branches.Load();
			AssertCollectionContains(branch1, branches);
			AssertCollectionNotContains(branch2, branches);
		}

		public void TestCarrierCollection()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = false;
			CusInBondBill bill = header.Bills.AddNew();
			var collection = bill.Lookups.CarrierAndFIRMSCollection;
			AssertNotNull(collection);
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "A8"));
			if (usCarrier == null)
			{
				usCarrier = Factory.New<USCarrierCombined>();
				usCarrier.UI_Code = "A8";
				usCarrier.UI_ModeOfTransportation = "40";
				usCarrier.UI_Name = "Test Carrier";
			}

			bill.B0_IssuerCode = usCarrier.UI_Code;
			var resultColl2 = bill.Lookups.CarrierAndFIRMSCollection as USCarrierCombinedCollection;
			AssertEquals(true, resultColl2.Contains(usCarrier));
		}

		public void TestCarrierAndFIRMSCollectionCollection()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = true;
			CusInBondBill bill = header.Bills.AddNew();
			var collection = bill.Lookups.CarrierAndFIRMSCollection;
			AssertNotNull(collection);
			var usCCarrierAndFIRMS = Factory.LoadTop1<USCCarrierAndFIRMS>(new ZQuery(USCCarrierAndFIRMSSchema.US_Code, "F9"));
			if (usCCarrierAndFIRMS == null)
			{
				usCCarrierAndFIRMS = Factory.New<USCCarrierAndFIRMS>();
				usCCarrierAndFIRMS.US_Code = "F9";
				usCCarrierAndFIRMS.US_TransportationMode = TransportModeCodes.Codes.AirNonContainer;
				usCCarrierAndFIRMS.US_Name = "Test CF";
			}

			bill.B0_IssuerCode = usCCarrierAndFIRMS.US_Code;
			var resultColl = bill.Lookups.CarrierAndFIRMSCollection as USCCarrierAndFIRMSCollection;
			AssertEquals(true, collection is USCCarrierAndFIRMSCollection);
		}

		public void TestRegionDistrictPorts()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			var collection = bill.Lookups.RegionDistrictPorts;
			AssertNotNull(collection);
		}

		public void TestShippers()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			ConsignorCollection collection = bill.Lookups.Shippers;
			AssertNotNull(collection);
		}

		public void TestConsignees()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			ConsigneeCollection collection = bill.Lookups.Consignees;
			AssertNotNull(collection);
		}

		public void TestOrganisations()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			OrgHeaderCollection collection = bill.Lookups.Organisations;
			AssertNotNull(collection);
		}

		public void TestManifestUnitList()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondBill bill = header.Bills.AddNew();
			InBondManifestUQList list = bill.Lookups.ManifestUnitList;
			AssertEquals(typeof(InBondManifestUQList), list.GetType());
			Assert(list.ContainsCode(InBondManifestUQList.Codes.BDL));
		}

		public void TestWeightUnitList()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondBill bill = header.Bills.AddNew();
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Weight), bill.Lookups.WeightUnitList);
		}

		public void TestVolumeUnitList()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			var bill = header.Bills.AddNew();
			AssertEquals("Contains codes for ACE M1", true, bill.Lookups.VolumeUnitList.ContainsCode(VolumeUnitList.Codes.OneHundredBoardFeet));
		}

		public void TestForeignPorts()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			ZZRefCusCodeListCombinedCollection collection = bill.Lookups.ForeignPorts;
			AssertNotNull(collection);
		}
	}
}
