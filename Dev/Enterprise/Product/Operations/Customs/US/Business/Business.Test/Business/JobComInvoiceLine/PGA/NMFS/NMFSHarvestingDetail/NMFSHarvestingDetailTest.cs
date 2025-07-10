using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSHarvestingDetail))]
	public class NMFSHarvestingDetailTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<NMFSHarvestingDetail>
	{
		public void TestCloneInNewFactory()
		{
			var originalBO = Factory.New<NMFSHarvestingDetail>();
			originalBO.HarvestingVessles.AddNew();

			var newBO = (NMFSHarvestingDetail)originalBO.Clone();

			AssertEquals(1, newBO.HarvestingVessles.Count);

			var fac = new BusinessObjectFactory();
			var newFacClone = (NMFSHarvestingDetail)originalBO.Clone(new BusinessObjectCloneArgs(fac, System.Array.Empty<string>(), typeof(NMFSHarvestingDetail), false));
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.Factory.GetHashCode());
			AssertEquals("Same Factory", fac.GetHashCode(), newFacClone.HarvestingVessles[0].Factory.GetHashCode());
			AssertNotEquals("Different Factory", originalBO.Factory.GetHashCode(), newFacClone.HarvestingVessles[0].Factory.GetHashCode());
		}

		public void TestProperties()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			NMFSLine.HarvestingDetails.RemoveAndDeleteAll();
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.ETP;
			harvestingDetail.US_GearType = GearTypeList.Codes.Longline;
			AssertEquals("NMFSLine.US_SourceTypeInfo.ReadOnly", true, NMFSLine.US_SourceTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.IsSIMPProgramType", false, harvestingDetail.IsSIMPProgramType);
			AssertEquals("harvestingDetail.IsCOAProgramType", false, harvestingDetail.IsCOAProgramType);
			AssertEquals("harvestingDetail.US_ContainsYellowfinTunaInfo.ReadOnly", false, harvestingDetail.US_ContainsYellowfinTunaInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_GearTypeDesc", GearTypeList.Descriptions.Longline, harvestingDetail.US_GearTypeDesc);
			AssertEquals("harvestingDetail.US_GearTypeInfo.ReadOnly", false, harvestingDetail.US_GearTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_HarvestedCountryInfo.ReadOnly", false, harvestingDetail.US_HarvestedCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchDesc", OceanGeographicAreaCodeList.Descriptions.ETP, harvestingDetail.US_OceanAreaOfCatchDesc);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly", false, harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_VesselCountryInfo.ReadOnly", false, harvestingDetail.US_VesselCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_GearStartDateInfo.ReadOnly", true, harvestingDetail.US_GearStartDateInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_ContactPartyTypeInfo.ReadOnly", true, harvestingDetail.US_ContactPartyTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OA_ContactPartyInfo.ReadOnly", true, harvestingDetail.US_OA_ContactPartyInfo.ReadOnly);
			AssertEquals("harvestingDetail.ContactPartyOrgPKInfo.ReadOnly", true, harvestingDetail.ContactPartyOrgPKInfo.ReadOnly);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals("NMFSLine.US_SourceTypeInfo.ReadOnly", true, NMFSLine.US_SourceTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.IsSIMPProgramType", false, harvestingDetail.IsSIMPProgramType);
			AssertEquals("harvestingDetail.IsCOAProgramType", false, harvestingDetail.IsCOAProgramType);
			AssertEquals("harvestingDetail.US_ContainsYellowfinTunaInfo.ReadOnly", true, harvestingDetail.US_ContainsYellowfinTunaInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_GearTypeDesc", GearTypeList.Descriptions.Longline, harvestingDetail.US_GearTypeDesc);
			AssertEquals("harvestingDetail.US_GearTypeInfo.ReadOnly", false, harvestingDetail.US_GearTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_HarvestedCountryInfo.ReadOnly", false, harvestingDetail.US_HarvestedCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchDesc", OceanGeographicAreaCodeList.Descriptions.ETP, harvestingDetail.US_OceanAreaOfCatchDesc);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly", false, harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_VesselCountryInfo.ReadOnly", false, harvestingDetail.US_VesselCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_GearStartDateInfo.ReadOnly", true, harvestingDetail.US_GearStartDateInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_ContactPartyTypeInfo.ReadOnly", true, harvestingDetail.US_ContactPartyTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OA_ContactPartyInfo.ReadOnly", true, harvestingDetail.US_OA_ContactPartyInfo.ReadOnly);
			AssertEquals("harvestingDetail.ContactPartyOrgPKInfo.ReadOnly", true, harvestingDetail.ContactPartyOrgPKInfo.ReadOnly);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals("NMFSLine.US_SourceTypeInfo.ReadOnly", false, NMFSLine.US_SourceTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.IsSIMPProgramType", true, harvestingDetail.IsSIMPProgramType);
			AssertEquals("harvestingDetail.IsCOAProgramType", false, harvestingDetail.IsCOAProgramType);
			AssertEquals("harvestingDetail.US_GearTypeDesc", GearTypeList.Descriptions.Longline, harvestingDetail.US_GearTypeDesc);
			AssertEquals("harvestingDetail.US_GearTypeInfo.ReadOnly", false, harvestingDetail.US_GearTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_HarvestedCountryInfo.ReadOnly", false, harvestingDetail.US_HarvestedCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchDesc", OceanGeographicAreaCodeList.Descriptions.ETP, harvestingDetail.US_OceanAreaOfCatchDesc);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly", false, harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_VesselCountryInfo.ReadOnly", false, harvestingDetail.US_VesselCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_GearStartDateInfo.ReadOnly", false, harvestingDetail.US_GearStartDateInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_ContactPartyTypeInfo.ReadOnly", false, harvestingDetail.US_ContactPartyTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OA_ContactPartyInfo.ReadOnly", false, harvestingDetail.US_OA_ContactPartyInfo.ReadOnly);
			AssertEquals("harvestingDetail.ContactPartyOrgPKInfo.ReadOnly", false, harvestingDetail.ContactPartyOrgPKInfo.ReadOnly);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			AssertEquals("NMFSLine.US_SourceTypeInfo.ReadOnly", false, NMFSLine.US_SourceTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.IsSIMPProgramType", false, harvestingDetail.IsSIMPProgramType);
			AssertEquals("harvestingDetail.IsCOAProgramType", true, harvestingDetail.IsCOAProgramType);
			AssertEquals("harvestingDetail.US_GearTypeDesc", GearTypeList.Descriptions.Longline, harvestingDetail.US_GearTypeDesc);
			AssertEquals("harvestingDetail.US_GearTypeInfo.ReadOnly", false, harvestingDetail.US_GearTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_HarvestedCountryInfo.ReadOnly", false, harvestingDetail.US_HarvestedCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchDesc", OceanGeographicAreaCodeList.Descriptions.ETP, harvestingDetail.US_OceanAreaOfCatchDesc);
			AssertEquals("harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly", false, harvestingDetail.US_OceanAreaOfCatchInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_VesselCountryInfo.ReadOnly", false, harvestingDetail.US_VesselCountryInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_GearStartDateInfo.ReadOnly", false, harvestingDetail.US_GearStartDateInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_ContactPartyTypeInfo.ReadOnly", false, harvestingDetail.US_ContactPartyTypeInfo.ReadOnly);
			AssertEquals("harvestingDetail.US_OA_ContactPartyInfo.ReadOnly", false, harvestingDetail.US_OA_ContactPartyInfo.ReadOnly);
			AssertEquals("harvestingDetail.ContactPartyOrgPKInfo.ReadOnly", false, harvestingDetail.ContactPartyOrgPKInfo.ReadOnly);
		}

		public void TestClone()
		{
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			NMFSLine.US_SourceType = "HBA";
			harvestingDetail.US_GearType = GearTypeList.Codes.Baitboat;
			harvestingDetail.US_HarvestedCountry = "US";
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.EPO;
			harvestingDetail.US_VesselCountry = "NZ";
			harvestingDetail.US_GearStartDate = ZDateTime.BrettsBirthday;
			harvestingDetail.US_GearDescription = "XX";
			harvestingDetail.US_ContactPartyType = "ZZ";
			OrgAddress contactPartyAddress = Factory.NewWithValidTestData<OrgAddress>();
			harvestingDetail.US_OA_ContactParty = contactPartyAddress.PK;

			var clonedHarvestingDetail = (NMFSHarvestingDetail)harvestingDetail.Clone();
			AssertEquals("clonedHarvestingDetail.US_GearType", GearTypeList.Codes.Baitboat, clonedHarvestingDetail.US_GearType);
			AssertEquals("clonedHarvestingDetail.US_HarvestedCountry", "US", clonedHarvestingDetail.US_HarvestedCountry);
			AssertEquals("clonedHarvestingDetail.US_OceanAreaOfCatch", OceanGeographicAreaCodeList.Codes.EPO, clonedHarvestingDetail.US_OceanAreaOfCatch);
			AssertEquals("clonedHarvestingDetail.US_GearStartDate", ZDateTime.BrettsBirthday, clonedHarvestingDetail.US_GearStartDate);
			AssertEquals("clonedHarvestingDetail.US_GearDescription", "XX", clonedHarvestingDetail.US_GearDescription);
			AssertEquals("clonedHarvestingDetail.US_VesselCountry", "NZ", clonedHarvestingDetail.US_VesselCountry);
			AssertEquals("clonedHarvestingDetail.US_ContactPartyType", "ZZ", clonedHarvestingDetail.US_ContactPartyType);
			AssertEquals("clonedHarvestingDetail.US_OA_ContactParty", contactPartyAddress.PK, clonedHarvestingDetail.US_OA_ContactParty);
			AssertEquals("clonedHarvestingDetail.ContactPartyOrgPK", contactPartyAddress.OA_OH, clonedHarvestingDetail.ContactPartyOrgPK);
		}

		public void TestUS_OceanAreaOfCatchDesc()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = "SVH";
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			Assert(!harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			harvestingDetail.US_OceanAreaOfCatchDesc = "TEST";
			AssertEquals("TEST", harvestingDetail.US_OceanAreaOfCatchDesc);

			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.A;
			Assert(harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			AssertEquals(harvestingDetail.AddInfoLookups.OceanAreaCodeList.GetDescriptionFromCode(OceanGeographicAreaCodeList.Codes.A), harvestingDetail.US_OceanAreaOfCatchDesc);

			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			Assert(harvestingDetail.US_OceanAreaOfCatchDesc.IsEmpty);

			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.OTH;
			Assert(!harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
			harvestingDetail.US_OceanAreaOfCatch = OceanGeographicAreaCodeList.Codes.A;
			Assert(harvestingDetail.US_OceanAreaOfCatchDesc_ReadOnly);
		}

		public void TestContactPartyAddress()
		{
			var harvestingDetail = NMFSLine.HarvestingDetails.AddNew();
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.AquacultureFacility;
			AssertEquals("harvestingDetail.ContactPartyOrgPK", manufacturerOrgHeader.PK, harvestingDetail.ContactPartyOrgPK);
			AssertEquals("harvestingDetail.US_OA_ContactParty", manufacturerOrgHeader.MainAddress.PK, harvestingDetail.US_OA_ContactParty);
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.Consignee;
			AssertEquals("harvestingDetail.ContactPartyOrgPK", ultimateConsigneeOrgHeader.PK, harvestingDetail.ContactPartyOrgPK);
			AssertEquals("harvestingDetail.US_OA_ContactParty", ultimateConsigneeOrgHeader.MainAddress.PK, harvestingDetail.US_OA_ContactParty);
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.Exporter;
			AssertEquals("harvestingDetail.ContactPartyOrgPK", foreignExporterOrgHeader.PK, harvestingDetail.ContactPartyOrgPK);
			AssertEquals("harvestingDetail.US_OA_ContactParty", foreignExporterOrgHeader.MainAddress.PK, harvestingDetail.US_OA_ContactParty);
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.Buyer;
			AssertEquals("harvestingDetail.ContactPartyOrgPK", buyer.PK, harvestingDetail.ContactPartyOrgPK);
			AssertEquals("harvestingDetail.US_OA_ContactParty", buyer.MainAddress.PK, harvestingDetail.US_OA_ContactParty);
			harvestingDetail.US_ContactPartyType = EntityRoleCodeList.Codes.Consignor;
			AssertEquals("harvestingDetail.ContactPartyOrgPK", supplier.PK, harvestingDetail.ContactPartyOrgPK);
			AssertEquals("harvestingDetail.US_OA_ContactParty", supplier.MainAddress.PK, harvestingDetail.US_OA_ContactParty);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			var harvestingDetail = nmfsLine.HarvestingDetails.AddNew();
			harvestingDetail.US_HarvestedCountry = NMFSConstants.InternationalWaters;
			return harvestingDetail;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return NMFSLine.HarvestingDetails.AddNew();
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
				}
				return declaration;
			}
		}

		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get
			{
				if (invoice == null)
				{
					invoice = Declaration.Invoices.AddNew();
					buyer = Factory.New<OrgHeader>();
					invoice.BuyerOrgPK = buyer.PK;
					supplier = Factory.New<OrgHeader>();
					invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
				}
				return invoice;
			}
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Invoice.JobComInvoiceLines.AddNew();
					manufacturerOrgHeader = Factory.New<OrgHeader>();
					invoiceLine.JI_OA_ManufacturerAddress = manufacturerOrgHeader.MainAddress.PK;
					ultimateConsigneeOrgHeader = Factory.New<OrgHeader>();
					invoiceLine.JI_OA_ConsigneeAddress = ultimateConsigneeOrgHeader.MainAddress.PK;
					foreignExporterOrgHeader = Factory.New<OrgHeader>();
					invoiceLine.JI_OA_ExporterAddress = foreignExporterOrgHeader.MainAddress.PK;
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		NMFSLine NMFSLine
		{
			get
			{
				if (nmfsLine == null)
				{
					nmfsLine = InvoiceLine.NMFSLines.AddNew();
					nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
					nmfsLine.HarvestingDetails.RemoveAndDeleteAll();
				}
				return nmfsLine;
			}
		}
		NMFSLine nmfsLine;

		OrgHeader manufacturerOrgHeader;
		OrgHeader ultimateConsigneeOrgHeader;
		OrgHeader foreignExporterOrgHeader;
		OrgHeader buyer;
		OrgHeader supplier;

		#endregion
	}
}
