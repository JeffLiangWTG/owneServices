using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		//DocUsage
		public void TestDocUsage_List()
		{
			JobRequiredDocument requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			requiredDocument.FillWithValidTestData();
			requiredDocument.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			AssertEquals("DocUsage_List.Count", 2, requiredDocument.Lookups.DocUsage_List.Count);
			Assert(requiredDocument.Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Debtor));
			Assert(requiredDocument.Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Creditor));

			requiredDocument.EQ_DocType = Constants.RefDocTypes.WithholdingTaxExemption;
			AssertEquals("DocUsage_List.Count", 2, requiredDocument.Lookups.DocUsage_List.Count);
			Assert(requiredDocument.Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Debtor));
			Assert(requiredDocument.Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Creditor));

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			requiredDocument = organisation.RequiredDocuments.AddNew();
			requiredDocument.FillWithValidTestData();
			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;

			AssertEquals("Should only contain 4", 4, requiredDocument.Lookups.DocUsage_List.Count);

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;

			AssertEquals("Should only contain 11", 12, requiredDocument.Lookups.DocUsage_List.Count);

			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
			AssertEquals("DocUsage_List.Count", 2, requiredDocument.Lookups.DocUsage_List.Count);
			Assert(requiredDocument.Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Debtor));
			Assert(requiredDocument.Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Creditor));

			IDocsAndCartageParent shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			JobRequiredDocument requiredDocument2 = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			requiredDocument2.FillWithValidTestData();
			requiredDocument2.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;

			AssertEquals("Should only contain 4", 4, requiredDocument2.Lookups.DocUsage_List.Count);
		}

		//RefCountry
		public void TestRefCountry_List()
		{
			JobRequiredDocument requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			AssertNotNull("Country list should not be empty", requiredDocument.Lookups.RefCountry_List);
		}

		//DocPeriod
		public void TestDocPeriod_List()
		{
			JobRequiredDocument requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			AssertEquals("Should only contain 2", 2, requiredDocument.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));
			Assert(requiredDocument.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment));

			requiredDocument.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			AssertEquals("Should contain only 1", 1, requiredDocument.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));

			requiredDocument.EQ_DocType = Constants.RefDocTypes.WithholdingTaxExemption;
			AssertEquals("Should contain only 1", 1, requiredDocument.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));

			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			JobRequiredDocument requiredDocument1 = organisation.RequiredDocuments.AddNew();
			requiredDocument1.FillWithValidTestData();

			requiredDocument1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			AssertEquals("Should contain only 1", 1, requiredDocument1.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument1.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));

			requiredDocument1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			AssertEquals("Should contain only 1", 1, requiredDocument1.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument1.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));

			requiredDocument1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			AssertEquals("Should contain only 1", 1, requiredDocument1.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument1.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));

			requiredDocument1.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
			requiredDocument1.EQ_DocType = "";
			AssertEquals("Should contain only 1", 1, requiredDocument1.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument1.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));

			requiredDocument1.EQ_DocType = Constants.RefDocTypes.WithholdingTaxExemption;
			AssertEquals("Should contain only 1", 1, requiredDocument1.Lookups.DocPeriod_List.Count);
			Assert(requiredDocument1.Lookups.DocPeriod_List.ContainsCode(Constants.JobRequiredDocuments.DocumentPeriods.Periodic));
		}

		//Category Type
		public void TestCategoryType_List()
		{
			JobRequiredDocument requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			AssertCollectionNotContains("Shouldn't contain nothing", "", requiredDocument.Lookups.CategoryType_List);
		}

		public void TestDocType_List_BuyerSupplierContainsSCL()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			OrgSupplierBuyerLink link = organisation.SupplierLinks.AddNew();
			JobRequiredDocument requiredDoc = link.RequiredDocuments.AddNew();

			RefDocType sclDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_ReferenceType, Constants.ReferenceTypes.SupplyChainLogistics));//Constants.ReferenceTypes.SupplyChainLogistics
			RefDocType csrDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_ReferenceType, Constants.ReferenceTypes.ClientSupplierRelationship));//Constants.ReferenceTypes.csr
			sclDocType.RT_ReferenceType = Constants.ReferenceTypes.SupplyChainLogistics;
			csrDocType.RT_ReferenceType = Constants.ReferenceTypes.ClientSupplierRelationship;

			Assert("There Should be at least one doc type in the list", requiredDoc.Lookups.DocType_List.Count > 0);
			Assert("List Should Contain Supply Chain Logistics Reference Type.", requiredDoc.Lookups.CategoryType_List.ContainsCode(sclDocType.RT_ReferenceType));
			Assert("List Should Not Contain Client/Supplier/Relationship Reference Type.", !requiredDoc.Lookups.CategoryType_List.ContainsCode(csrDocType.RT_ReferenceType));
			Assert("List Should Not Contain Client/Supplier/Relationship Reference Type.", !requiredDoc.Lookups.CategoryType_List.ContainsCode(Constants.ReferenceTypes.All));
		}

		public void TestDocType_List()
		{
			AssertNotNull(Lookups.DocType_List);

			Assert("The miscellaneous document type must be present for all objects.", Lookups.DocType_List.ContainsCode(Core.Constants.RefDocTypes.MiscellaneousDocument));
			Assert("The He Xiao Dan document type must be present for all objects.", Lookups.DocType_List.ContainsCode(Core.Constants.RefDocTypes.HeXiaoDan));

			Assert(!Lookups.DocType_List.ContainsCode(Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument));
			Assert(!Lookups.DocType_List.ContainsCode(Core.Constants.RefDocTypes.InternallyCreatedPublicDocument));

			IDocumentFactoryProvider provider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory documentFactory = provider.GetFactory(Factory);
			IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
			IDocumentFactory masterFactory = documentFactoryProvider.GetFactory(Factory);

			var parent = masterFactory.RetrieveExistingOrCreateStorageMainForPK(((BusinessObject)Shipment).PK,
				((IDocManagerSupport)Shipment).DocManagerInfo.DocManagerCode);

			var newDocument = (IeDoc)((BusinessObjectFactory)masterFactory).New<IStorageDocs>();
			newDocument.SetImageDataStream(new MemoryStream(new byte[] { 1, 1, 1, 1, 1, 1, 1 }));
			newDocument.IsPublished = true;
			newDocument.DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			parent.Documents.Add(newDocument);

			masterFactory.Save();

			foreach (ICodeDescription codeDescription in Lookups.DocType_List)
			{
				Assert("A storage doc attached to the same type of business object must have the same allowed document types.",
					newDocument.DocType_List.ContainsCode(codeDescription.Code));

				AssertEquals("The same document types must have the same descriptions.", newDocument.DocType_List[codeDescription.Code].Description, codeDescription.Description);
			}
		}

		public void TestComplianceReportExemptDocumentLookups()
		{
			JobRequiredDocument requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			requiredDocument.FillWithValidTestData();
			requiredDocument.EQ_ParentID = Factory.NewWithValidTestData<OrgHeader>().PK;
			requiredDocument.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDocument.ParentType = typeof(OrgHeader);
			Lookups = requiredDocument.Lookups;

			AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
			AssertEquals("Category List does not contain Compliance Report", false, Lookups.CategoryType_List.ContainsCode(Constants.ReferenceTypes.ComplianceReport));

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			AddComplianceReportConfig(complianceConfig, "TST");
			AddComplianceReportConfig(complianceConfig, "TS2", "Test Tax Report 2");

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				Lookups.ResetDocAndCategoryTypeList();
				AssertEquals("Category List contains Compliance Report", true, Lookups.CategoryType_List.ContainsCode(Constants.ReferenceTypes.ComplianceReport));
				requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
				AssertEquals("Related Country is set to Current Company's Country", Env.CurrentCompany.Country.Code, requiredDocument.EQ_RN_NKRelatedCountry);
				AssertEquals("Document Owner is set to Current Company's Org Proxy", Env.CurrentCompany.OrganisationPK, requiredDocument.EQ_OH_DocumentOwner);

				Assert(Lookups.DocType_List.ContainsCode("TST"));
				AssertEquals("Exempt from Test Tax Report", Lookups.DocType_List.GetDescriptionFromCode("TST"));
				Assert(Lookups.DocType_List.ContainsCode("TS2"));
				AssertEquals("Exempt from Test Tax Report 2", Lookups.DocType_List.GetDescriptionFromCode("TS2"));

				requiredDocument.EQ_DocType = "TST";

				AssertEquals(2, Lookups.DocUsage_List.Count);
				Assert("Contains Creditor", Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Creditor));
				Assert("Contains Debtor", Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Debtor));
			}

			var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

			using (Environment.Env.SetTemporaryUserContext(Env.CurrentUser.PK, Factory.LoadTop1<GlbBranch>(branchQuery).PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
				AssertNotEquals("Document Owner is different to Current Company's Org Proxy", Env.CurrentCompany.OrganisationPK, requiredDocument.EQ_OH_DocumentOwner);

				AssertReadOnlyRequiredDocument(requiredDocument, "TST");
			}
		}

		public void TestComplianceReportLookupsForReportableSmallBusiness()
		{
			var requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			requiredDocument.FillWithValidTestData();
			requiredDocument.EQ_ParentID = Factory.NewWithValidTestData<OrgHeader>().PK;
			requiredDocument.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDocument.ParentType = typeof(OrgHeader);

			Lookups = requiredDocument.Lookups;

			AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
			AssertEquals("Category List does not contain Compliance Report", false, Lookups.CategoryType_List.ContainsCode(Constants.ReferenceTypes.ComplianceReport));

			var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, Env.CurrentCompany.PK);
			branchQuery.AddToFilter(GlbBranchSchema.GB_IsActive, true);

			using (Environment.Env.SetTemporaryUserContext(Env.CurrentUser.PK, Factory.LoadTop1<GlbBranch>(branchQuery).PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
				Lookups.ResetDocAndCategoryTypeList();

				requiredDocument.EQ_DocCategory = "";
				Assert(requiredDocument.EQ_OH_DocumentOwner.IsEmpty);

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				AddComplianceReportConfig(complianceConfig, "TST");
				using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
				{
					assertRSBDocType();
				}

				AddComplianceReportConfig(complianceConfig, "RSB", "Test Report with code duplicating RSB DocType");
				using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
				{
					assertRSBDocType();
				}

				AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
				AssertEquals("Related Country is set to Current Company's Country", Env.CurrentCompany.Country.Code, requiredDocument.EQ_RN_NKRelatedCountry);
				AssertEquals("Document Owner is set to Current Company's Org Proxy", Env.CurrentCompany.OrganisationPK, requiredDocument.EQ_OH_DocumentOwner);
				Assert("Document is not Read Only", !requiredDocument.ReadOnly);

				Lookups.ResetDocAndCategoryTypeList();
				Assert("Without Compliance Reports Configuration the CTR Category is not on the List anymore", !Lookups.CategoryType_List.ContainsCode(Constants.ReferenceTypes.ComplianceReport));
				Assert("List still contains RSB DocType because we have CTR Category", Lookups.DocType_List.ContainsCode("RSB"));
				AssertEquals("RSB DocType Description", "PTRS Reportable Small Business", Lookups.DocType_List["RSB"].Description);
			}

			AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
			AssertNotEquals("Document Owner is different to Current Company's Org Proxy", Env.CurrentCompany.OrganisationPK, requiredDocument.EQ_OH_DocumentOwner);
			AssertReadOnlyRequiredDocument(requiredDocument, "RSB");

			void assertRSBDocType()
			{
				requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
				requiredDocument.EQ_DocType = "RSB";

				AssertEquals("Related Country is set to Current Company's Country", Env.CurrentCompany.Country.Code, requiredDocument.EQ_RN_NKRelatedCountry);
				AssertEquals("Document Owner is set to Current Company's Org Proxy", Env.CurrentCompany.OrganisationPK, requiredDocument.EQ_OH_DocumentOwner);

				AssertEquals(2, Lookups.DocType_List.Count);
				Assert(Lookups.DocType_List.ContainsCode("RSB")); // AU PTRS Reportable Small Business DocType
				AssertEquals("RSB DocType Description", "PTRS Reportable Small Business", Lookups.DocType_List["RSB"].Description);
				Assert(Lookups.DocType_List.ContainsCode("TST"));

				AssertEquals(2, Lookups.DocUsage_List.Count);
				Assert("Contains Creditor", Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Creditor));
				Assert("Contains Debtor", Lookups.DocUsage_List.ContainsCode(JobRequiredDocument.DocUsage.Debtor));
			}
		}

		void AddComplianceReportConfig(ComplianceReportConfigurationCollection complianceConfig, string reportCode = "TST", string reportTitle = "Test Tax Report")
		{
			var report = complianceConfig.AddNew();
			report.ReportCode = reportCode;
			report.ReportTitle = reportTitle;
			report.ReportPeriodicity = "RNG";
			report.Country = Env.CurrentCompany.Country.Code;
			report.TaxRegistrationType = "ABN";
			report.ReportBaseTablePrefix = AccTransactionHeaderSchema.Constants.Prefix;
		}

		void AssertReadOnlyRequiredDocument(JobRequiredDocument requiredDocument, string docType)
		{
			Lookups = requiredDocument.Lookups;
			Lookups.ResetDocAndCategoryTypeList();
			Assert("Document is Read Only", requiredDocument.ReadOnly);

			Assert("Category is ReadOnly", requiredDocument.EQ_DocCategoryInfo.ReadOnly);
			Assert("To avoid Validation Error when logged in a Company without Compliance Reports Configuration", Lookups.CategoryType_List.ContainsCode(Constants.ReferenceTypes.ComplianceReport));
			AssertEquals("CTR DocCategory Description", "Compliance Report", Lookups.CategoryType_List[Constants.ReferenceTypes.ComplianceReport].Description);

			Assert("DocType is ReadOnly", requiredDocument.EQ_DocTypeInfo.ReadOnly);
			AssertEquals(1, Lookups.DocType_List.Count);
			Assert("List still contains current Value to avoid List Validation error", Lookups.DocType_List.ContainsCode(docType));
			AssertEquals("RSB DocType Description is not provided because we only need to bypass Validation", string.Empty, Lookups.DocType_List[docType].Description);
		}

		public void TestDocTypeListAvoidExcessiveFactoryLoads()
		{
			BusinessObjectFactory.StartLogging();
			try
			{
				AssertGreaterThan(Lookups.DocType_List.Count, 2);
				AssertContainsExactElementsInAnyOrder("Should not be duplicate lookups during calculation of the list", Array.Empty<string>(), BusinessObjectFactory.DebugLog.Split(new[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToLookup(s => s).Where(e => e.Count() > 2).Select(e => e.Key + " " + e.Count() + " times"));
			}
			finally
			{
				BusinessObjectFactory.StopLogging();
			}
		}

		public void TestFreightDocTypes_ShouldBeHiddenInProductivityWiseMode()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			var categoryTypeList = Factory.NewWithValidTestData<JobRequiredDocument>().Lookups.AllCategoryType_List_ForTest;

			AssertCollectionContains("With ProductivityWise mode disabled, collection should return as normal, and yet...", Constants.ReferenceTypes.ClientSupplierRelationship, categoryTypeList.ToArray().Select(type => type.Code));
			AssertCollectionContains("With ProductivityWise mode disabled, collection should return as normal, and yet...", Constants.ReferenceTypes.SupplyChainLogistics, categoryTypeList.ToArray().Select(type => type.Code));

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var newCategoryTypeList = Factory.NewWithValidTestData<JobRequiredDocument>().Lookups.AllCategoryType_List_ForTest;
			AssertCollectionNotContains("With ProductivityWise mode disabled, collection should not return Freight-related docTypes, and yet...", new ZString[]
			{
				Constants.ReferenceTypes.ClientSupplierRelationship,
				Constants.ReferenceTypes.SupplyChainLogistics
			}, newCategoryTypeList.ToArray().Select(type => type.Code));
		}

		#region Implementation

		JobRequiredDocumentLookups Lookups;
		IDocsAndCartageParent Shipment;

		protected override void SetUp()
		{
			base.SetUp();

			Shipment = (IDocsAndCartageParent)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			Lookups = Shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew().Lookups;
		}

		#endregion
	}
}
