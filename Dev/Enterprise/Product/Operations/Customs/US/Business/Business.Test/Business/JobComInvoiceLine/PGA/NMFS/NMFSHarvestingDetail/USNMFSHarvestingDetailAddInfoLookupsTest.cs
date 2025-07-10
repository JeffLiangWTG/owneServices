using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USNMFSHarvestingDetailAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRefVessels()
		{
			AssertEquals("RefVessels", typeof(RefVesselCollection), AddInfo.Lookups.RefVessels.GetType());
		}

		public void TestHarvestedMethods()
		{
			var harvestedMethods = AddInfo.Lookups.HarvestedMethods;
			AssertEquals("HarvestedMethods", typeof(CodeDescriptionPairList), harvestedMethods.GetType());
			AssertEquals(SourceTypeCodesList.Descriptions.Vessel, harvestedMethods.GetDescriptionFromCode(SourceTypeCodesList.Codes.Vessel));
			AssertEquals(SourceTypeCodesList.Descriptions.HarvestOfCaptureFisheries, harvestedMethods.GetDescriptionFromCode(SourceTypeCodesList.Codes.HarvestOfCaptureFisheries));
			AssertEquals(SourceTypeCodesList.Descriptions.HatcheryBasedAquaculture, harvestedMethods.GetDescriptionFromCode(SourceTypeCodesList.Codes.HatcheryBasedAquaculture));
			AssertEquals(SourceTypeCodesList.Descriptions.SmallVesselHarvest, harvestedMethods.GetDescriptionFromCode(SourceTypeCodesList.Codes.SmallVesselHarvest));
		}

		public void TestGearTypeList()
		{
			AssertEquals("GearTypeList", Factory.GetCachedValue<GearTypeList>(), AddInfo.Lookups.GearTypeList);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals("GearTypeList", GearTypeList.GetListFor370Program(Factory), AddInfo.Lookups.GearTypeList);
		}

		public void TestCountries()
		{
			AssertEquals("Countries", typeof(RefCountryCollection), AddInfo.Lookups.Countries.GetType());
		}

		public void TestOceanAreaCodeList()
		{
			AssertEquals("OceanAreaCodeList", Factory.GetCachedValue<OceanGeographicAreaCodeList>(), AddInfo.Lookups.OceanAreaCodeList);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals("OceanAreaCodeList", OceanGeographicAreaCodeList.GetListFor370Program(Factory), AddInfo.Lookups.OceanAreaCodeList);
		}

		public void TestContactPartyTypesList()
		{
			AssertEquals("ContactPartyTypes", EntityRoleCodeList.GetListForNMFSSIM(Factory), AddInfo.Lookups.ContactPartyTypes);
		}

		public void TestGearDescriptionsList()
		{
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals("GearDescriptions", ProcessingTypeCodeList.GetListForNMFS(Factory, NMFSLine.US_ProgramType), AddInfo.Lookups.GearDescriptions);
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

		USNMFSHarvestingDetailAddInfo AddInfo
		{
			get { return addInfo ?? (addInfo = new USNMFSHarvestingDetailAddInfo(NMFSLine.HarvestingDetails.AddNew().B7_AddInfoDataInfo)); }
		}
		USNMFSHarvestingDetailAddInfo addInfo;

		#endregion
	}
}
