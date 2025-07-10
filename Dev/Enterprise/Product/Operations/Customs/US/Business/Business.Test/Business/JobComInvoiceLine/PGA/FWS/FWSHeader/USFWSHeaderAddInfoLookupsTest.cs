using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USFWSHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestProcessingCodes()
		{
			var list = Header.AddInfoLookups.ProcessingCodes;
			AssertEquals("ProcessingCodes", Factory.GetCachedValue<FWSProcessingCodeList>(), list);
		}

		public void TestProductTypes()
		{
			var list = Header.AddInfoLookups.ProductTypes;
			AssertEquals("ProductTypes", Factory.GetCachedValue<GlobalUniqueProductCodeQualifierList>(), list);
		}

		public void TestSpeciesOrigins()
		{
			var list = Header.AddInfoLookups.SpeciesOrigins;
			AssertEquals("SpeciesOrigins", typeof(RefCountryCollection), list.GetType());
		}

		public void TestYesNoList()
		{
			var list = Header.AddInfoLookups.YesNoList;
			AssertEquals("YesNoList", YesNoDefaultList.GetCachedYesNoList(Factory), list);
		}

		public void TestOrganisations()
		{
			var list = Header.AddInfoLookups.Organisations;
			AssertEquals("Organisations", typeof(OrganisationsFindBoxCollection), list.GetType());
		}

		public void TestHighSeaAreas()
		{
			var list = Header.AddInfoLookups.HighSeaAreas;
			AssertEquals("HighSeaAreas", Factory.GetCachedValue<OceanGeographicAreaCodeList>(), list);
		}

		public void TestWildlifeSources()
		{
			var list = Header.AddInfoLookups.WildlifeSources;
			AssertNotNull(list);

			Header.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			list = Header.AddInfoLookups.WildlifeSources;
			AssertEquals("Wildlife source for export PGA", 10, list.Count);
		}

		public void TestIdentityTypes()
		{
			var list = Header.AddInfoLookups.IdentityTypes;
			AssertEquals("IdentityTypes", ItemIdentityNumberQualifierList.GetListForFWS(Factory), list);
		}

		public void TestHybridTypes()
		{
			var list = Header.AddInfoLookups.HybridTypes;
			AssertEquals("HybridTypes", Factory.GetCachedValue<FWSHybridTypeList>(), list);
		}

		public void TestWildlifeCategoryCodes()
		{
			var list = Header.AddInfoLookups.WildlifeCategoryCodes;
			AssertEquals("WildlifeCategoryCodes", Factory.GetCachedValue<FWSWildlifeCategoryCodesList>(), list);
			AssertEquals("WildlifeCategoryCodes contains 'SAL'", true, list.ContainsCode(FWSWildlifeCategoryCodesList.Codes.Salmonids));
		}

		public void TestWildlifeDescriptionCodes()
		{
			var list = Header.AddInfoLookups.WildlifeDescriptionCodes;
			AssertEquals("WildlifeDescriptionCodes", Factory.GetCachedValue<FWSWildlifeDescriptionCodesList>(), list);
		}

		public void TestUnitOfMeasureList()
		{
			var list = Header.AddInfoLookups.UnitOfMeasureList;
			AssertEquals("UnitOfMeasureList", Factory.GetCachedValue<FWSUnitOfMeasureList>(), list);
		}

		public void TestFIRMSList()
		{
			var list = Header.AddInfoLookups.FIRMSList;
			AssertEquals("FIRMSList", typeof(ZZRefCusCodeListCombinedCollection), list.GetType());
		}

		public void TestPurposeCodeList()
		{
			var list = Header.AddInfoLookups.PurposeCodeList;
			AssertEquals("PurposeCodeList", typeof(FWSPurposeCodeList), list.GetType());
		}

		public void TestCertificationCodeList()
		{
			var list = Header.AddInfoLookups.CertificationCodeList;
			AssertEquals("CertificationCodeList", typeof(FWSCertificationCodeList), list.GetType());
		}

		public void TestFWSCertifyingIndividualList()
		{
			var lookups = Header.AddInfoLookups;
			Assert(lookups.FWSCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.CustomsBroker));
			Assert(lookups.FWSCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.FWSImporter));
			Assert(lookups.FWSCertifyingIndividualList.ContainsCode(PartyTypeList.Codes.FWSForeignExporter));
		}

		#region Implementation
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
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

		FWSHeader Header
		{
			get { return header ?? (header = InvoiceLine.FWSHeaders.AddNew()); }
		}
		FWSHeader header;
		#endregion
	}
}
