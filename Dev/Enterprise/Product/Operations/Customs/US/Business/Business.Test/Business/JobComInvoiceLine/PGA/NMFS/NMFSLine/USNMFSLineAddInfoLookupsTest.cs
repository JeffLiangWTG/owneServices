using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
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
	class USNMFSLineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentTypeList()
		{
			Line.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals("DocumentTypeList", Factory.GetCachedValue<NMFSAMRDocumentIdentifierList>(), Line.AddInfoLookups.DocumentTypeList);
			Line.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertEquals("DocumentTypeList", Factory.GetCachedValue<CodeDescriptionPairList>(), Line.AddInfoLookups.DocumentTypeList);
			Line.US_ProgramType = "";
			AssertEquals("DocumentTypeList", Factory.GetCachedValue<NMFS370DocumentIdentifierList>(), Line.AddInfoLookups.DocumentTypeList);
			Line.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals("DocumentTypeList", Factory.GetCachedValue<NMFSHMSDocumentIdentifierList>(), Line.AddInfoLookups.DocumentTypeList);
			Line.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals("DocumentTypeList", Factory.GetCachedValue<NMFS370DocumentIdentifierList>(), Line.AddInfoLookups.DocumentTypeList);
		}

		public void TestDolphinSafeStatusList()
		{
			AssertEquals("DolphinSafeStatusList", Factory.GetCachedValue<DolphinSafeStatusList>(), Line.AddInfoLookups.DolphinSafeStatusList);
		}

		public void TestWeightUQList()
		{
			AssertEquals("WeightUQList", Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Line.AddInfoLookups.WeightUQList);
		}

		public void TestNMFSPrograms()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("NMFSPrograms", NMFSProgramCodeList.GetListFor(Factory, false, false, false, false, false, false), Line.AddInfoLookups.NMFSPrograms);
			InvoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			AssertEquals("NMFSPrograms", NMFSProgramCodeList.GetListFor(Factory, true, false, false, false, false, false), Line.AddInfoLookups.NMFSPrograms);
			InvoiceLine.US_NMFSAMRInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("NMFSPrograms", NMFSProgramCodeList.GetListFor(Factory, true, true, false, false, false, false), Line.AddInfoLookups.NMFSPrograms);
			InvoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("NMFSPrograms", NMFSProgramCodeList.GetListFor(Factory, true, true, true, false, false, false), Line.AddInfoLookups.NMFSPrograms);
			InvoiceLine.US_NMFSSIMPInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("NMFSPrograms", NMFSProgramCodeList.GetListFor(Factory, true, true, true, true, false, false), Line.AddInfoLookups.NMFSPrograms);
			InvoiceLine.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("NMFSPrograms", NMFSProgramCodeList.GetListFor(Factory, true, true, true, true, true, false), Line.AddInfoLookups.NMFSPrograms);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("NMFSPrograms", NMFSProgramCodeList.GetListFor(Factory, false, false, false, false, false, true), Line.AddInfoLookups.NMFSPrograms);
		}

		public void TestFishStateList()
		{
			AssertEquals("FishStateList", Factory.GetCachedValue<FishStateList>(), Line.AddInfoLookups.FishStateList);
			Assert(Line.AddInfoLookups.FishStateList.ContainsCode("FRK"));
			Assert(Line.AddInfoLookups.FishStateList.ContainsCode("FRT"));
			Assert(Line.AddInfoLookups.FishStateList.ContainsCode("FZK"));
			Assert(Line.AddInfoLookups.FishStateList.ContainsCode("FZT"));
			Line.US_Commodity = FishStateList.Codes.FreshKrill;
			AssertEquals("FishStateList_FreshKrill", Factory.GetCachedValue<FishStateList>().GetDescriptionFromCode("FRK"), Line.AddInfoLookups.FishStateList.GetDescriptionFromCode(Line.US_Commodity));
			Line.US_Commodity = FishStateList.Codes.FreshToothfish;
			AssertEquals("FishStateList_FreshToothfish", Factory.GetCachedValue<FishStateList>().GetDescriptionFromCode("FRT"), Line.AddInfoLookups.FishStateList.GetDescriptionFromCode(Line.US_Commodity));
			Line.US_Commodity = FishStateList.Codes.FrozenKrill;
			AssertEquals("FishStateList_FrozenKrill", Factory.GetCachedValue<FishStateList>().GetDescriptionFromCode("FZK"), Line.AddInfoLookups.FishStateList.GetDescriptionFromCode(Line.US_Commodity));
			Line.US_Commodity = FishStateList.Codes.FrozenToothfish;
			AssertEquals("FishStateList_FrozenToothfish", Factory.GetCachedValue<FishStateList>().GetDescriptionFromCode("FZT"), Line.AddInfoLookups.FishStateList.GetDescriptionFromCode(Line.US_Commodity));
		}

		public void TestCountries()
		{
			AssertEquals("Countries", typeof(RefCountryCollection), Line.AddInfoLookups.Countries.GetType());
		}

		public void TestOceanAreaCodeList()
		{
			AssertEquals("OceanAreaCodeList", Factory.GetCachedValue<OceanGeographicAreaCodeList>(), Line.AddInfoLookups.OceanAreaCodeList);
		}

		public void TestProcessingTypeList()
		{
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var dataGrouping = Core.Constants.CountryCodes.UnitedStates;
			var nmfsCategoryCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NMFSCategoryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(nmfsCategoryCodeType, "NMFSCategoryCode", dataGrouping);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, nmfsCategoryCodeType, "BBF", "Baitboat: Freezer", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(dataGrouping, nmfsCategoryCodeType, "BBI", "Baitboat: Ice-well", startDate, endDate);
			Factory.Save();
			AssertEquals("ProcessingTypeList", typeof(CodeDescriptionPairList), Line.AddInfoLookups.ProcessingTypeList.GetType());
			AssertEquals(2, Line.AddInfoLookups.ProcessingTypeList.Count);
		}

		public void TestSpeciesCodeList()
		{
			AssertEquals("SpeciesCodeList", typeof(ZZRefCusCodeListCombinedCollection), Line.AddInfoLookups.SpeciesCodeList.GetType());
		}

		public void TestAuthorizationTypes()
		{
			AssertEquals(2, Line.AddInfoLookups.AuthorizationTypes.Count);
			AssertEquals("1", ((CodeDescriptionPair)line.AddInfoLookups.AuthorizationTypes[0]).Code);
			AssertEquals("2", ((CodeDescriptionPair)line.AddInfoLookups.AuthorizationTypes[1]).Code);
		}

		public void TestSourceTypesList()
		{
			Line.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			AssertEquals("SourceTypes", SourceTypeCodesList.GetListForNMFS(Factory, Line.US_ProgramType), Line.AddInfoLookups.SourceTypes);

			Line.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			AssertEquals("SourceTypes", SourceTypeCodesList.GetListForNMFS(Factory, Line.US_ProgramType), Line.AddInfoLookups.SourceTypes);
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

		NMFSLine Line
		{
			get { return line ?? (line = InvoiceLine.NMFSLines.AddNew()); }
		}
		NMFSLine line;

		#endregion
	}
}
