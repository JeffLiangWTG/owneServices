using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class NMFSHarvestingDetailWrapperTest : TestCaseWithFactory
	{
		public void TestINMFSHarvestingDetailMembersForOTH()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = "SVH";
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			Assert(!harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			harvestingDetail.US_OceanAreaOfCatchDesc = "TEST";
			INMFSHarvestingDetail wrapper = new NMFSHarvestingDetailWrapper(false, new[] { harvestingDetail });
			AssertEquals("TEST", wrapper.GeographicLocation);

			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.CAR;
			wrapper = new NMFSHarvestingDetailWrapper(false, new[] { harvestingDetail });
			AssertEquals(OceanGeographicAreaCodeList.Codes.CAR, wrapper.GeographicLocation);
		}

		public void TestINMFSHarvestingDetailMembersFor370()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			var harvestingDetail1 = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = "ZZ";
			harvestingDetail1.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			harvestingDetail1.US_OceanAreaOfCatchDesc = "Ignored";
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail1.US_VesselCountry = "US";

			var harvestingDetail2 = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail2.US_HarvestedCountry = "ZZ";
			harvestingDetail2.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			harvestingDetail2.US_OceanAreaOfCatchDesc = "Ignored";
			harvestingDetail2.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail2.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail2.US_VesselCountry = "AU";

			var harvestingDetail3 = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail3.US_HarvestedCountry = "ZZ";
			harvestingDetail3.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			harvestingDetail3.US_OceanAreaOfCatchDesc = "Ignored";
			harvestingDetail3.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail3.US_ContainsYellowfinTuna = ZBool.True;
			harvestingDetail3.US_VesselCountry = "AU";

			INMFSHarvestingDetail wrapper = new NMFSHarvestingDetailWrapper(true, new[] { harvestingDetail1, harvestingDetail2, harvestingDetail3 });
			AssertEquals("CountryCode", "ZZ", wrapper.CountryCode);
			AssertEquals("GeographicLocation", OceanGeographicAreaCodeList.Codes.OTH, wrapper.GeographicLocation);
			AssertEquals("ProcessingTypeCode", GearTypeList.Codes.Longline, wrapper.ProcessingTypeCode);
			AssertEquals("ContainsYellowfinTuna", ZBool.True, wrapper.ContainsYellowfinTuna);
			var harvestingVessels = wrapper.HarvestingVessels.ToList();
			AssertEquals("HarvestingVessels", 3, harvestingVessels.Count);
			AssertEquals(harvestingVessels[0], "AU");
			AssertEquals(harvestingVessels[1], "AU");
			AssertEquals(harvestingVessels[2], "US");
		}

		public void TestINMFSHarvestingDetailMembersForSIM()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = "HBA";
			NMFSLine.HarvestingDetails.RemoveAndDeleteAll();
			var harvestingDetail1 = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail1.US_HarvestedCountry = "ZZ";
			harvestingDetail1.US_GeographicLocation = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail1.US_GearType = GearTypeList.Codes.Longline;
			harvestingDetail1.US_GearStartDate = ZDateTime.BrettsBirthday;
			harvestingDetail1.US_GearDescription = "NDR";
			harvestingDetail1.US_ContactPartyType = "XX";

			var orgAddress = Factory.New<OrgAddress>();
			harvestingDetail1.US_OA_ContactParty = orgAddress.PK;

			INMFSHarvestingDetail wrapper = new NMFSHarvestingDetailWrapper(false, new[] { harvestingDetail1 });
			AssertEquals("CountryCode", "ZZ", wrapper.CountryCode);
			AssertEquals("GeographicLocation", OceanGeographicAreaCodeList.Codes.ETP, wrapper.GeographicLocation);
			AssertEquals("ProcessingTypeCode", GearTypeList.Codes.Longline, wrapper.ProcessingTypeCode);
			AssertEquals("SourceTypeCode", "HBA", wrapper.SourceTypeCode);
			AssertEquals("ProcessingStartDate", ZDateTime.BrettsBirthday, wrapper.ProcessingStartDate);
			AssertEquals("ProcessingDescription", "NDR", wrapper.ProcessingDescription);
			AssertEquals("ContactPartyType", "XX", wrapper.ContactPartyType);
			AssertEquals("ContactPartyDetails", ((IPGAContactDetails)OrgHeaderWrapper.New(orgAddress)).CompanyAddress, wrapper.ContactPartyDetails.CompanyAddress);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		NMFSLine NMFSLine
		{
			get { return nmfsLine ?? (nmfsLine = InvoiceLine.NMFSLines.AddNew()); }
		}
		NMFSLine nmfsLine;
		#endregion
	}
}
