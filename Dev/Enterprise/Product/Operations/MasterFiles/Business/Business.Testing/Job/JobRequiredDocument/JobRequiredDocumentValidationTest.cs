using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class JobRequiredDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckEQ_DocNumber_CheckLengthForACP()
		{
			var expectedError = "The maximum length for Power of Attorney for Attorney for Customs Procedures (ACP) is 10 characters.";
			PopulateACPDocument(Doc1);
			Doc1.EQ_DocNumber = "12345678901";
			AssertHasError(Doc1.EQ_DocNumberInfo, expectedError);

			Doc1.EQ_DocNumber = "1234567890";
			AssertNoError(Doc1.EQ_DocNumberInfo, expectedError);
		}

		public void TestValidateDuplicateRows()
		{
			var expectedError = "There should be only one CSR + POA + ACP + JP document tracking record of the same document owner under the same organization.";
			PopulateACPDocument(Doc1);
			PopulateACPDocument(Doc2);
			Doc1.EQ_OH_DocumentOwner = Doc2.EQ_OH_DocumentOwner = Factory.NewWithValidTestData<OrgHeader>().PK;

			Doc1.Validation.ValidateAll();
			Doc2.Validation.ValidateAll();
			AssertHasRowError(Doc1, expectedError);
			AssertHasRowError(Doc2, expectedError);

			Doc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.China;
			Doc1.Validation.ValidateAll();
			Doc2.Validation.ValidateAll();
			AssertNoRowError(Doc1, expectedError);
			AssertNoRowError(Doc2, expectedError);
		}

		public void TestCheckEQ_OH_DocumentOwner()
		{
			var port1 = Factory.New<RefUNLOCO>();
			port1.RL_Code = "JPTY6";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_RL_NKClosestPort = port1.RL_Code;

			var port2 = Factory.New<RefUNLOCO>();
			port2.RL_Code = "US123";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_RL_NKClosestPort = port2.RL_Code;
			Factory.Save();

			var expectedError = "Please select the Attorney for Customs Procedures (ACP) for this Power of Attorney (POA).";
			PopulateACPDocument(Doc1);
			Doc1.Validation.ValidateEQ_OH_DocumentOwner();
			AssertHasError(Doc1.EQ_OH_DocumentOwnerInfo, expectedError);

			var expectedMessageError = "Attorney for Customs Procedures (ACP) must be from Japan. The entered organization is from 'United States'.";
			Doc1.EQ_OH_DocumentOwner = org1.PK;
			AssertNoMessageError(Doc1.EQ_OH_DocumentOwnerInfo, expectedMessageError);

			Doc1.EQ_OH_DocumentOwner = org2.PK;
			AssertHasMessageError(Doc1.EQ_OH_DocumentOwnerInfo, expectedMessageError);
		}

		void PopulateACPDocument(JobRequiredDocument doc)
		{
			doc.EQ_DocCategory  = Constants.ReferenceTypes.ClientSupplierRelationship;
			doc.EQ_DocType = Constants.RefDocTypes.PowerOfAttorney;
			doc.EQ_DocUsage = JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures;
			doc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Japan;
		}

		public void TestCorrectValidationMessageShownForNotAllowDuplicatedDocType()
		{
			var docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_AllowMultiplePeriodicDocs = false;
			docType.RT_DocType = "TST";
			docType.RT_ReferenceType = Constants.ReferenceTypes.SupplyChainLogistics;
			Factory.Save();

			PrepareJobRequiredDocument(Doc1);
			PrepareJobRequiredDocument(Doc2);
			Assert(Doc2.EQ_DocTypeInfo.HasError("The type TST does not allow multiple periodic documents, but there is already TST type for this job with the same valid to date. If it's not shown - please reload the form."));
		}

		void PrepareJobRequiredDocument(JobRequiredDocument doc)
		{
			doc.EQ_DocCategory = Constants.ReferenceTypes.SupplyChainLogistics;
			doc.EQ_DocType = "TST";
			doc.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			doc.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-1);
			doc.EQ_ValidToDate = ZDateTime.Today.AddDays(1);
		}

		[ExpectNoExceptions()]
		public void TestCheckEQ_ValidToDate()
		{
			Doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			Doc1.EQ_ValidToDate = ZDateTime.Empty;
			Assert("EQ_ValidToDate should not have errors because the document is not periodic.", !Doc1.EQ_ValidToDateInfo.HasErrors());

			Doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			Doc1.EQ_ValidToDate = ZDateTime.Empty;
			Assert("EQ_ValidToDate must be specified if the document period is periodic.", Doc1.EQ_ValidToDateInfo.HasErrors());

			Doc1.EQ_ValidToDate = ZDateTime.Now.AddYears(1);
			Assert("EQ_ValidToDate should not have errors if a valid expiry date is specified.", !Doc1.EQ_ValidToDateInfo.HasErrors());

			Doc1.EQ_ValidToDate = ZDateTime.Now.AddDays(-1);
			Assert("EQ_ValidToDate should have a warning if the expiry date is in the past.", Doc1.EQ_ValidToDateInfo.HasWarnings());

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			Doc1.EQ_ValidToDate = ZDateTime.Empty;
			Assert("EQ_ValidToDate should not have warning if expiry date is empty.", !Doc1.HasWarnings);

			Doc1.EQ_ValidToDate = ZDateTime.Now.AddDays(50);
			Assert("EQ_ValidToDate should not have warnings as it is a once per shipment document.", !Doc1.HasWarnings);

			Doc1.EQ_ValidToDate = ZDateTime.Now.AddDays(30);
			Assert("EQ_ValidToDate should not have warnings as it is a once per shipment document.", !Doc1.HasWarnings);

			Doc1.EQ_ValidToDate = ZDateTime.Now.AddDays(-3);
			Assert("EQ_ValidToDate should have a warning if the expiry date is in the past.", Doc1.HasWarnings);

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			Doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			Doc1.EQ_ValidToDate = new ZDateTime("1/01/0001 12:00:00 AM");
			Assert("EQ_ValidToDate check should not fail with exception when Date is NOT valid", Doc1.HasErrors);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				Doc1.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Italy;
				Doc1.EQ_ValidToDate = new ZDateTime("03/03/2017 12:00:00 AM");
				Doc1.EQ_DateReceived = new ZDateTimeOffset("02/02/2017 12:00:00 AM");
				AssertNoErrors(Doc1.EQ_DateReceivedInfo);
				Doc1.EQ_ValidToDate = new ZDateTime("03/03/2018 12:00:00 AM");
				Assert("EQ_ValidToDate must be in the same year as EQ_ReceivedDate", Doc1.EQ_ValidToDateInfo.HasErrors());
			}
		}

		public void TestCheckEQ_DocPeriod()
		{
			Doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			Assert("EQ_DocPeriod should not have errors because it is a valid code", !Doc1.EQ_DocPeriodInfo.HasErrors());

			Doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			Assert("EQ_DocPeriod should not have errors because it is a valid code", !Doc1.EQ_DocPeriodInfo.HasErrors());

			Doc1.EQ_DocPeriod = "XXX";
			Assert("EQ_DocPeriod should have errors because it is a invalid code", Doc1.EQ_DocPeriodInfo.HasErrors());

			Doc1.EQ_DocPeriod = "";
			Assert("EQ_DocPeriod should have errors because it is empty", Doc1.EQ_DocPeriodInfo.HasErrors());
		}

		public void TestCheckEQ_DateReceived()
		{
			Doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			Doc1.EQ_DateReceived = ZDateTimeOffset.Empty;
			Assert("EQ_DateReceived should not have errors because the document is not periodic.", !Doc1.EQ_DateReceivedInfo.HasErrors());

			Doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			Doc1.EQ_DateReceived = ZDateTimeOffset.Empty;
			Assert("EQ_DateReceived must be specified if the document period is periodic.", Doc1.EQ_DateReceivedInfo.HasErrors());

			Doc1.EQ_DateReceived = ZDateTimeOffset.Now;
			Assert("EQ_DateReceived should not have errors if a valid expiry date is specified.", !Doc1.EQ_DateReceivedInfo.HasErrors());
			Assert("EQ_DateReceived should not have a warning if a valid expiry date is specified.", !Doc1.EQ_DateReceivedInfo.HasWarnings());

			Doc1.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(1);
			Assert("EQ_DateReceived should not have a warning if a valid expiry date is specified.", !Doc1.EQ_DateReceivedInfo.HasWarnings());

			Doc1.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(2);
			Assert("EQ_DateReceived should have a warning if the expiry date is in the past.", Doc1.EQ_DateReceivedInfo.HasWarnings());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				Doc1.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Italy;
				Doc1.EQ_ValidToDate = new ZDateTime("03/03/2017 12:00:00 AM");
				Doc1.EQ_DateReceived = new ZDateTimeOffset("02/02/2017 12:00:00 AM");
				AssertNoErrors(Doc1.EQ_DateReceivedInfo);
				Doc1.EQ_DateReceived = new ZDateTimeOffset("02/02/2016 12:00:00 AM");
				Assert("EQ_DateReceived must be in the same year as EQ_ValidToDate", Doc1.EQ_DateReceivedInfo.HasErrors());
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEQ_DateReceived_Values()
		{
			Doc1.EQ_DateReceived = new ZDateTimeOffset("2025-01-13 01:30:00 -05:00");
			AssertEquals(new ZDateTimeOffset(2025, 1, 13, 17, 30, 0, TimeSpan.FromHours(11)), Doc1.EQ_DateReceived);
			Doc1.EQ_DateReceived = new ZDateTimeOffset("2025-01-13 01:30:00 +00:00");
			AssertEquals(new ZDateTimeOffset(2025, 1, 13, 12, 30, 0, TimeSpan.FromHours(11)), Doc1.EQ_DateReceived);
			Doc1.EQ_DateReceived = new ZDateTimeOffset("2025-01-13 01:30:00 +03:00");
			AssertEquals(new ZDateTimeOffset(2025, 1, 13, 9, 30, 0, TimeSpan.FromHours(11)), Doc1.EQ_DateReceived);
		}

		public void TestCheckEQ_RN_NKRelatedCountry()
		{
			Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			Assert("EQ_RN_NKRelatedCountry should not have errors because the code is valid", !Doc1.EQ_RN_NKRelatedCountryInfo.HasErrors());

			Doc1.EQ_RN_NKRelatedCountry = "XX";
			Assert("EQ_RN_NKRelatedCountry should have errors because the code is invalid", Doc1.EQ_RN_NKRelatedCountryInfo.HasErrors());

			Doc1.EQ_RN_NKRelatedCountry = "";
			Assert("EQ_RN_NKRelatedCountry should have no errors if blank", !Doc1.EQ_RN_NKRelatedCountryInfo.HasErrors());

			Doc1.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			Doc1.EQ_RN_NKRelatedCountry = "";
			Assert("EQ_RN_NKRelatedCountry should have errors if blank for document type EXV", Doc1.EQ_RN_NKRelatedCountryInfo.HasErrors());

			Doc1.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
			Doc1.EQ_DocType = "";
			Doc1.EQ_RN_NKRelatedCountry = "";
			Assert("EQ_RN_NKRelatedCountry should have errors if blank for document category CTR", Doc1.EQ_RN_NKRelatedCountryInfo.HasErrors());
		}

		public void TestCheckEQ_DocCategory()
		{
			Doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			AssertNoErrors("EQ_DocCategory should have no errors because Supply Chain Logistics is a valid Code", Doc1.EQ_DocCategoryInfo);

			Doc1.EQ_DocCategory = "";
			Assert("EQ_DocCategory should have errors because Category must be something", Doc1.EQ_DocCategoryInfo.HasErrors());

			Doc1.EQ_DocCategory = "XXX";
			AssertHasErrors("EQ_DocCategory should have errors because XXX is not a valid Code", Doc1.EQ_DocCategoryInfo);

			var requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			requiredDocument.FillWithValidTestData();
			requiredDocument.EQ_ParentID = Factory.NewWithValidTestData<OrgHeader>().PK;
			requiredDocument.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDocument.ParentType = typeof(OrgHeader);

			AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
			Assert("Should be editable", !requiredDocument.ReadOnly);
			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
			Assert("Should still be editable", !requiredDocument.ReadOnly);
			AssertEquals("EQ_OH_DocumentOwner", Env.CurrentCompany.OrganisationPK, requiredDocument.EQ_OH_DocumentOwner.ToGuid());
			AssertHasErrors("EQ_DocCategory should have errors because there is no Compliance Reports configured in the Registry", requiredDocument.EQ_DocCategoryInfo);
			requiredDocument.EQ_DocType = "TST";
			AssertHasErrors("EQ_DocType should have errors because there is no 'TST' Compliance Reports configured in the Registry", requiredDocument.EQ_DocTypeInfo);

			requiredDocument.EQ_OH_DocumentOwner = Factory.NewWithValidTestData<OrgHeader>().PK;
			requiredDocument.Lookups.ResetDocAndCategoryTypeList();

			Assert("Should be ReadOnly because Owner is not an Org Proxy of the Current Company", requiredDocument.ReadOnly);
			requiredDocument.Validation.ValidateEQ_DocCategory();
			AssertNoErrors("EQ_DocCategory should have no errors because it is ReadOnly", requiredDocument.EQ_DocCategoryInfo);
			requiredDocument.Validation.ValidateEQ_DocType();
			AssertNoErrors("EQ_DocType should have no errors because it is ReadOnly", requiredDocument.EQ_DocTypeInfo);

			requiredDocument.EQ_OH_DocumentOwner = Env.CurrentCompany.OrganisationPK;
			Assert("Should be back to editable", !requiredDocument.ReadOnly);

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var report = complianceConfig.AddNew();
			report.ReportCode = "TST";
			report.ReportTitle = "Test Tax Report";
			report.ReportPeriodicity = "RNG";
			report.Country = Environment.Env.CurrentCompany.Country.Code;
			report.TaxRegistrationType = "ABN";
			report.ReportBaseTablePrefix = AccTransactionHeaderSchema.Constants.Prefix;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			requiredDocument.Lookups.ResetDocAndCategoryTypeList();
			requiredDocument.Validation.ValidateEQ_DocCategory();
			AssertNoErrors("EQ_DocCategory should have no errors because Registry has Compliance Reports configuration", requiredDocument.EQ_DocCategoryInfo);
		}

		public void TestCheckEQ_DocDescription()
		{
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.MasterBill;
			Doc1.EQ_DocDescription = "";
			Assert("EQ_DocDescription should not have errors because the document is not miscellaneous.", !Doc1.EQ_DocDescriptionInfo.HasErrors());

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Doc1.EQ_DocDescription = "";
			Assert("EQ_DocDescription should have errors because a miscellaneous document requires a description.", Doc1.EQ_DocDescriptionInfo.HasErrors());

			Doc1.EQ_DocDescription = "Test description";
			Assert("EQ_DocDescription should not have errors because it has a valid description.", !Doc1.EQ_DocDescriptionInfo.HasErrors());
		}

		public void TestCheckEQ_DocType()
		{
			//Test mandatory validation
			Doc1.EQ_DocType = "";
			Assert("Expect error on EQ_DocType", Doc1.EQ_DocTypeInfo.HasErrors());

			//Test list validation
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.ArrivalNotice;
			Assert("Expect no error on EQ_DocType", !Doc1.EQ_DocTypeInfo.HasErrors());
			Doc2.EQ_DocType = "XXX";
			Assert("Expect error on EQ_DocType", Doc2.EQ_DocTypeInfo.HasErrors());

			//Test unique validation
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Assert("Expect no error on EQ_DocType", !Doc1.EQ_DocTypeInfo.HasErrors());
			Doc2.EQ_DocType = Core.Constants.RefDocTypes.HouseAirWaybill;
			Assert("Expect error on EQ_DocType", Doc2.EQ_DocTypeInfo.HasErrors());
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Assert("Expect no error on EQ_DocType", !Doc1.EQ_DocTypeInfo.HasErrors());
			Doc2.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Assert("Expect no error on EQ_DocType", !Doc2.EQ_DocTypeInfo.HasErrors());

			Doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.All;
			Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Antarctica;

			Doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			Doc2.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Doc2.EQ_DocUsage = JobRequiredDocument.DocUsage.All;
			Doc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Antarctica;

			Doc1.Validation.ValidateAll();
			Doc2.Validation.ValidateAll();
			Assert("Expect error on EQ_DocType", Doc1.EQ_DocTypeInfo.HasErrors());
			Assert("Expect error on EQ_DocType", Doc2.EQ_DocTypeInfo.HasErrors());

			Doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.All;
			Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Antarctica;

			Doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			Doc2.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Doc2.EQ_DocUsage = JobRequiredDocument.DocUsage.All;
			Doc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;

			Doc1.Validation.ValidateAll();
			Doc2.Validation.ValidateAll();
			Assert("Expect no error on EQ_DocType", !Doc2.EQ_DocTypeInfo.HasErrors());

			Doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.All;
			Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Antarctica;

			Doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			Doc2.EQ_DocType = Core.Constants.RefDocTypes.AgentsInstruction;
			Doc2.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			Doc2.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Antarctica;

			Doc1.Validation.ValidateAll();
			Doc2.Validation.ValidateAll();
			Assert("Expect no error on EQ_DocType", !Doc2.EQ_DocTypeInfo.HasErrors());

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			Doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;

			Assert("Expect no error on EQ_DocType", !Doc1.EQ_DocTypeInfo.HasErrors());

			OrgHeader organisation = Factory.New<OrgHeader>();
			JobRequiredDocument doc3 = organisation.RequiredDocuments.AddNew();

			doc3.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			doc3.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			Assert("Expect no error on EQ_DocType", !doc3.EQ_DocTypeInfo.HasErrors());

			JobRequiredDocument doc4 = organisation.RequiredDocuments.AddNew();
			doc4.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			doc4.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			Assert("Expect no error on EQ_DocType", !doc4.EQ_DocTypeInfo.HasErrors());

			JobRequiredDocument doc5 = organisation.RequiredDocuments.AddNew();
			doc5.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			doc5.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			Assert("Expect no error on EQ_DocType", !doc5.EQ_DocTypeInfo.HasErrors());

			JobRequiredDocument doc6 = organisation.RequiredDocuments.AddNew();
			doc6.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc6.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			Assert("Expect error on EQ_DocType", doc6.EQ_DocTypeInfo.HasErrors());

			Env.Security.GetDocumentTypeUploadCheckPoint(Core.Constants.RefDocTypes.KnownConsignorAgreement).IsAllowed = false;
			JobRequiredDocument doc7 = organisation.RequiredDocuments.AddNew();
			doc7.EQ_DocType = Core.Constants.RefDocTypes.KnownConsignorAgreement;
			Assert("EQ_DocType should not check security permissions.", !doc7.EQ_DocTypeInfo.HasErrors());
			doc7.EQ_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			Assert("RT_DocType should be allowed", !doc7.EQ_DocTypeInfo.HasErrors());
		}

		public void TestDocTypeValidationGetDataFromDB()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "org111";
			Factory.Save();

			AssertEquals(0, organisation.RequiredDocuments.Count);

			JobRequiredDocument doc1 = organisation.RequiredDocuments.AddNew();
			doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			OrgHeader organisationInAnotherFactory = factory2.Load<OrgHeader>(organisation.PK);

			JobRequiredDocument doc2 = organisationInAnotherFactory.RequiredDocuments.AddNew();
			doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc2.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			factory2.Save();
			AssertEquals(1, organisationInAnotherFactory.RequiredDocuments.Count);

			doc1.RunPreSaveValidation();
			Assert(doc1.EQ_DocTypeInfo.HasErrors());

			var agentsInvoiceDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Constants.RefDocTypes.AgentsInvoice));
			AssertNotNull(agentsInvoiceDocType);
			if (!agentsInvoiceDocType.RT_AllowMultiplePeriodicDocs)
			{
				AssertHasError(doc1.EQ_DocTypeInfo, "The type AGI does not allow multiple periodic documents, but there is already AGI type for this job with the same valid to date. If it's not shown - please reload the form.");
			}
		}

		public void TestDocTypeValidationForComplianceReportCategory()
		{
			var requiredDocument = Factory.NewWithValidTestData<JobRequiredDocument>();
			requiredDocument.FillWithValidTestData();
			requiredDocument.EQ_ParentID = Factory.NewWithValidTestData<OrgHeader>().PK;
			requiredDocument.EQ_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			requiredDocument.ParentType = typeof(OrgHeader);

			Assert("Should be editable", !requiredDocument.ReadOnly);
			AssertEquals("Compliance Report Configuration Count", 0, AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value.Count);
			requiredDocument.EQ_DocCategory = Constants.ReferenceTypes.ComplianceReport;
			Assert("Should still be editable", !requiredDocument.ReadOnly);
			AssertEquals("EQ_OH_DocumentOwner", Env.CurrentCompany.OrganisationPK, requiredDocument.EQ_OH_DocumentOwner.ToGuid());
			AssertHasErrors("EQ_DocCategory should have errors because there is no Compliance Reports configured in the Registry", requiredDocument.EQ_DocCategoryInfo);

			requiredDocument.EQ_DocType = "TST";
			AssertHasErrors("EQ_DocType should have errors because there is no 'TST' Compliance Reports configured in the Registry", requiredDocument.EQ_DocTypeInfo);

			requiredDocument.EQ_OH_DocumentOwner = Factory.NewWithValidTestData<OrgHeader>().PK;
			requiredDocument.Lookups.ResetDocAndCategoryTypeList();

			Assert("Should be ReadOnly because Owner is not an Org Proxy of the Current Company", requiredDocument.ReadOnly);
			requiredDocument.Validation.ValidateEQ_DocCategory();
			AssertNoErrors("EQ_DocCategory should have no errors because it is ReadOnly", requiredDocument.EQ_DocCategoryInfo);
			requiredDocument.Validation.ValidateEQ_DocType();
			AssertNoErrors("EQ_DocType should have no errors because it is ReadOnly", requiredDocument.EQ_DocTypeInfo);

			requiredDocument.EQ_OH_DocumentOwner = Env.CurrentCompany.OrganisationPK;
			Assert("Should be back to editable", !requiredDocument.ReadOnly);

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var report = complianceConfig.AddNew();
			report.ReportCode = "TST";
			report.ReportTitle = "Test Tax Report";
			report.ReportPeriodicity = "RNG";
			report.Country = Environment.Env.CurrentCompany.Country.Code;
			report.TaxRegistrationType = "ABN";
			report.ReportBaseTablePrefix = AccTransactionHeaderSchema.Constants.Prefix;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			requiredDocument.Lookups.ResetDocAndCategoryTypeList();
			AssertEquals("EQ_RN_NKRelatedCountry", Environment.Env.CurrentCompany.Country.Code, requiredDocument.EQ_RN_NKRelatedCountry);

			requiredDocument.Validation.ValidateEQ_DocCategory();
			AssertNoErrors("EQ_DocCategory should have no errors because Registry has Compliance Reports configuration", requiredDocument.EQ_DocCategoryInfo);
			requiredDocument.Validation.ValidateEQ_DocType();
			AssertNoErrors("EQ_DocType should have no errors because Registry has matching Compliance Report configuration", requiredDocument.EQ_DocTypeInfo);

			requiredDocument.EQ_RN_NKRelatedCountry = "UA";
			requiredDocument.Validation.ValidateEQ_DocType();
			AssertHasErrors("EQ_DocType should have an error because Country code does not match one in 'TST' Compliance Report configuration", requiredDocument.EQ_DocTypeInfo);

			requiredDocument.EQ_RN_NKRelatedCountry = Environment.Env.CurrentCompany.Country.Code;
			requiredDocument.Validation.ValidateEQ_DocType();
			AssertNoErrors("EQ_DocType should not have errors because Country code matches one in 'TST' Compliance Report configuration", requiredDocument.EQ_DocTypeInfo);
		}

		public void TestPeriodicalDocumentWithDifferentDocumentNumber()
		{
			var docType = Factory.Load<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Core.Constants.RefDocTypes.Invoice)).FirstOrDefault();
			Assert(!docType.RT_AllowMultiplePeriodicDocs);

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "org111";
			Factory.Save();

			AssertEquals(0, organisation.RequiredDocuments.Count);

			var doc1 = organisation.RequiredDocuments.AddNew();
			doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			doc1.EQ_DocType = Core.Constants.RefDocTypes.Invoice;
			doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			doc1.EQ_ValidToDate = ZDateTime.BrettsBirthday;
			doc1.EQ_DocNumber = "EGI Clone No.1";
			Factory.Save();
			AssertEquals(1, organisation.RequiredDocuments.Count);

			var doc2 = organisation.RequiredDocuments.AddNew();
			doc2.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			doc2.EQ_DocType = Core.Constants.RefDocTypes.Invoice;
			doc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.Periodic;
			doc2.EQ_ValidToDate = ZDateTime.BrettsBirthday;
			doc2.EQ_DocNumber = "EGI Clone No.1";

			Assert(doc2.EQ_DocTypeInfo.HasErrors());
			var inivoiceDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, Constants.RefDocTypes.Invoice));
			AssertNotNull(inivoiceDocType);
			if (!inivoiceDocType.RT_AllowMultiplePeriodicDocs)
			{
				AssertHasError(doc2.EQ_DocTypeInfo, "The type INV does not allow multiple periodic documents, but there is already INV type for this job with the same valid to date. If it's not shown - please reload the form.");
			}

			doc2.EQ_DocNumber = "EGI Clone No.2";
			Assert(!doc2.EQ_DocTypeInfo.HasErrors());

			Factory.Save();
			AssertEquals(2, organisation.RequiredDocuments.Count);

			doc2.EQ_DocNumber = "EGI Clone No.1";
			Assert(doc2.EQ_DocTypeInfo.HasErrors());
			if (!inivoiceDocType.RT_AllowMultiplePeriodicDocs)
			{
				AssertHasError(doc2.EQ_DocTypeInfo, "The type INV does not allow multiple periodic documents, but there is already INV type for this job with the same valid to date. If it's not shown - please reload the form.");
			}

			docType.RT_AllowMultiplePeriodicDocs = true;
			Factory.Save();
			doc2.Validation.ValidateEQ_DocType();
			AssertNoError(doc2.EQ_DocTypeInfo, "There is already INV type for this job. If it's not shown - please reload the form.");

			doc1.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			doc2.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			AssertHasError(doc2.EQ_DocTypeInfo, "There is already INV type for this job. If it's not shown - please reload the form.");
		}

		public void TestCheckEQ_DocNumber()
		{
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PackingList;
			Doc1.EQ_DateReceived = ZDateTimeOffset.Now;
			Doc1.Validation.ValidateAll();
			AssertEquals("Precondition - doc number is empty by default", ZString.Empty, Doc1.EQ_DocNumber);
			AssertNoErrors("Non-HXD documents shouldn't need a doc number", Doc1.EQ_DocNumberInfo);

			Doc2.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
			Doc2.Validation.ValidateAll();
			AssertEquals("Precondition - doc number is empty by default", ZString.Empty, Doc2.EQ_DocNumber);
			AssertEquals("Precondition - received date is empty by default", ZDateTimeOffset.Empty, Doc2.EQ_DateReceived);
			AssertNoErrors("HXD documents shouldn't need a doc number if they haven't been received", Doc2.EQ_DocNumberInfo);

			Doc2.EQ_DateReceived = ZDateTimeOffset.Now;
			Doc2.Validation.ValidateAll();
			AssertHasErrors("HXD documents need a doc number if they have been received", Doc2.EQ_DocNumberInfo);

			Doc2.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			Doc2.EQ_DocNumber = "";
			AssertHasErrors("EXV documents need a doc number", Doc2.EQ_DocNumberInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				Doc2.EQ_RN_NKRelatedCountry = "IT";
				Doc2.Validation.ValidateAll();
				AssertNoErrors(Doc2.EQ_DocNumberInfo);

				Doc2.EQ_RN_NKRelatedCountry = "AU";
				Doc2.Validation.ValidateAll();
				Assert(Doc2.EQ_DocNumberInfo.HasError("Please enter a Document Number."));

				ZQuery query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
				var nonCurrentCompany = (GlbCompany)Factory.Load(typeof(GlbCompany), query)[0];

				var attrib = Doc2.Attributes.AddNew();
				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
				attrib.D0_AttribValue = nonCurrentCompany.PK.ToString();
				Doc2.Validation.ValidateAll();
				AssertNoErrors(Doc2.EQ_DocNumberInfo);

				attrib.D0_AttribValue = GlbCompany.CurrentCompany.PK.ToString();
				Doc2.Validation.ValidateAll();
				Assert(Doc2.EQ_DocNumberInfo.HasError("Please enter a Document Number."));
			}
		}

		public void TestCheckRD_DocUsage()
		{
			Doc1.EQ_DocUsage = "";
			AssertHasErrors(Doc1.EQ_DocUsageInfo);

			Doc1.EQ_DocUsage = "XXX";
			Doc1.Validation.ValidateEQ_DocUsage();
			AssertHasErrors(Doc1.EQ_DocUsageInfo);

			Doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.All;
			Doc1.Validation.ValidateEQ_DocUsage();
			AssertHasErrors(Doc1.EQ_DocUsageInfo);

			Doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			Doc1.Validation.ValidateEQ_DocUsage();
			AssertNoErrors(Doc1.EQ_DocUsageInfo);
		}

		public void TestCheckEQ_SntToCustomsBroker()
		{
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
			Doc1.EQ_SntToCustomsBroker = ZDateTime.Now;
			AssertEquals("Precondition - received date is empty by default", ZDateTimeOffset.Empty, Doc1.EQ_DateReceived);
			Doc1.Validation.ValidateAll();
			AssertHasWarnings("HXD documents with a sent to broker date should have a received date too", Doc1.EQ_SntToCustomsBrokerInfo);

			Doc1.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-2);
			Doc1.Validation.ValidateAll();
			AssertNoWarnings(Doc1.EQ_SntToCustomsBrokerInfo);
		}

		public void TestCheckEQ_RcvFromCustomsBroker()
		{
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
			AssertEquals("Precondition - received date is empty by default", ZDateTimeOffset.Empty, Doc1.EQ_DateReceived);
			AssertEquals("Precondition - sent to broker date is empty by default", ZDateTime.Empty, Doc1.EQ_SntToCustomsBroker);
			Doc1.EQ_RcvFromCustomsBroker = ZDateTime.Now;
			Doc1.Validation.ValidateAll();
			AssertHasWarnings("HXD documents with a received from broker date should have a sent to broker date too", Doc1.EQ_RcvFromCustomsBrokerInfo);

			Doc1.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-2);
			Doc1.EQ_SntToCustomsBroker = ZDateTime.Now.AddDays(-1);
			Doc1.Validation.ValidateAll();
			AssertNoWarnings(Doc1.EQ_RcvFromCustomsBrokerInfo);
		}

		public void TestCheckEQ_ReturnToShipper()
		{
			JobHeader testJH1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			testJH1.JH_ParentID = ((BusinessObject)Shipment).PK;
			testJH1.JH_ParentTableCode = "JS";
			testJH1.JH_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "CN$$$";
			Factory.Save();

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;

			var outstandingAccHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			outstandingAccHeader.AH_TransactionNum = "33";
			outstandingAccHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			outstandingAccHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			outstandingAccHeader.AH_OH = testOrg.PK;
			outstandingAccHeader.AH_JH = testJH1.PK;
			outstandingAccHeader.AH_OutstandingAmount = 20M;
			outstandingAccHeader.AH_InvoiceAmount = 20M;
			outstandingAccHeader.AH_InvoiceDate = ZDateTime.Now;
			outstandingAccHeader.AH_PostDate = ZDateTime.Now;
			outstandingAccHeader.AH_RX_NKTransactionCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			var outstandingAccHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			outstandingAccHeader.AH_TransactionNum = "44";
			outstandingAccHeader2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			outstandingAccHeader2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			outstandingAccHeader2.AH_OH = testOrg.PK;
			outstandingAccHeader2.AH_JH = testJH1.PK;
			outstandingAccHeader2.AH_OutstandingAmount = 30M;
			outstandingAccHeader2.AH_InvoiceAmount = 30M;
			outstandingAccHeader2.AH_InvoiceDate = ZDateTime.Now;
			outstandingAccHeader2.AH_PostDate = ZDateTime.Now;
			outstandingAccHeader2.AH_RX_NKTransactionCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			Factory.Save();

			AssertEquals("Precondition - return to shipper is empty by default", ZDateTime.Empty, Doc1.EQ_ReturnToShipper);
			AssertNoWarnings("Return to shipper should not have warnings", Doc1.EQ_ReturnToShipperInfo);

			Doc1.EQ_ReturnToShipper = ZDateTime.Now;
			AssertHasWarnings("Return to shipper should have a warning", Doc1.EQ_ReturnToShipperInfo);

			outstandingAccHeader.Delete();
			outstandingAccHeader2.Delete();
			testJH1.MarkAsInactive();
			Factory.Save();

			AssertEquals("Precondition - received from broker is empty by default", ZDateTime.Empty, Doc1.EQ_RcvFromCustomsBroker);
			Doc1.Validation.ValidateAll();
			AssertHasWarnings("Return to shipper should have an error", Doc1.EQ_ReturnToShipperInfo);

			Doc1.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-4);
			Doc1.EQ_SntToCustomsBroker = ZDateTime.Now.AddDays(-3);
			Doc1.EQ_RcvFromCustomsBroker = ZDateTime.Now.AddDays(-2);
			Doc1.Validation.ValidateAll();
			AssertNoWarnings("Return to shipper should not have an error", Doc1.EQ_ReturnToShipperInfo);
		}

		public void TestCheckEQ_CreditControlDoc()
		{
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
			Doc1.EQ_CreditControlDoc = true;
			AssertNoWarnings("HXD credit control doc with no invoices should not have a warning", Doc1.EQ_CreditControlDocInfo);

			JobHeader testJH1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			testJH1.JH_ParentID = ((BusinessObject)Shipment).PK;
			testJH1.JH_ParentTableCode = "JS";
			testJH1.JH_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_RL_NKClosestPort = "CN$$$";
			Factory.Save();

			var outstandingAccHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			outstandingAccHeader.AH_TransactionNum = "33";
			outstandingAccHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			outstandingAccHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			outstandingAccHeader.AH_OH = testOrg.PK;
			outstandingAccHeader.AH_JH = testJH1.PK;
			outstandingAccHeader.AH_OutstandingAmount = 20M;
			outstandingAccHeader.AH_InvoiceAmount = 20M;
			outstandingAccHeader.AH_InvoiceDate = ZDateTime.Now;
			outstandingAccHeader.AH_PostDate = ZDateTime.Now;
			outstandingAccHeader.AH_RX_NKTransactionCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			var outstandingAccHeader2 = Factory.NewWithValidTestData<AccTransactionHeader>();
			outstandingAccHeader.AH_TransactionNum = "44";
			outstandingAccHeader2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			outstandingAccHeader2.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			outstandingAccHeader2.AH_OH = testOrg.PK;
			outstandingAccHeader2.AH_JH = testJH1.PK;
			outstandingAccHeader2.AH_OutstandingAmount = 30M;
			outstandingAccHeader2.AH_InvoiceAmount = 30M;
			outstandingAccHeader2.AH_InvoiceDate = ZDateTime.Now;
			outstandingAccHeader2.AH_PostDate = ZDateTime.Now;
			outstandingAccHeader2.AH_RX_NKTransactionCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			Factory.Save();

			Doc1.Validation.ValidateEQ_CreditControlDoc();
			AssertHasWarnings("HXD credit control doc with outstanding invoices should have a warning", Doc1.EQ_CreditControlDocInfo);

			Doc1.EQ_CreditControlDoc = false;
			AssertNoWarnings("HXD non-credit control doc with outstanding invoices should not have a warning", Doc1.EQ_CreditControlDocInfo);
		}

		public void TestHXDCodesShouldntCauseErrorsIfAccessedFromForeignBranch_W00040447()
		{
			string originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

				Doc1.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
				Factory.Save();

				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

				var factory2 = new BusinessObjectFactory();
				var loadedDoc = factory2.Load<JobRequiredDocument>(Doc1.PK);
				loadedDoc.ParentType = Doc1.ParentType;
				loadedDoc.Validation.ValidateEQ_DocType();
				AssertNoErrors("HXD doc type shouldn't cause error when loaded in another branch", loadedDoc.EQ_DocTypeInfo);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		public void TestMoreThanOneHXDDocumentShouldBeAllowed()
		{
			string originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);

				Doc1.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;
				Doc2.EQ_DocType = Core.Constants.RefDocTypes.HeXiaoDan;

				Doc1.Validation.ValidateEQ_DocType();
				Doc2.Validation.ValidateEQ_DocType();

				AssertNoErrors("Multiple He Xiao Dan documents should be allowed", Doc1.EQ_DocTypeInfo);
				AssertNoErrors("Multiple He Xiao Dan documents should be allowed", Doc2.EQ_DocTypeInfo);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountry);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "We really want a 4 character year")]
		public void TestCheckEQ_ValidToDateIsValidZDateTimeRange()
		{
			Doc1.EQ_DocPeriod = Constants.JobRequiredDocuments.DocumentPeriods.Periodic;

			Doc1.EQ_ValidToDate = ZDateTime.Now.AddYears(11);
			AssertHasErrorContaining(Doc1.EQ_ValidToDateInfo, "is more than 10 years from now and thus is not valid.");

			Doc1.EQ_ValidToDate = ZDateTime.Now.AddYears(7);
			AssertNoErrors("EQ_ValidToDateInfo should have no errors if date is less than 10 years from now", Doc1.EQ_ValidToDateInfo);

			Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			Doc1.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			Doc1.EQ_ValidToDate = ZDateTime.Now.AddYears(6);
			const string DATE_FORMAT = "dd-MMM-yyyy";
			AssertHasWarningContaining(Doc1.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(Doc1.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 1));

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			Doc1.EQ_ValidToDate = ZDateTime.Now.AddYears(6);
			AssertHasWarningContaining(Doc1.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(Doc1.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 1));

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyForwarding;
			Doc1.EQ_ValidToDate = ZDateTime.Now.AddYears(6);
			AssertHasWarningContaining(Doc1.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(Doc1.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 1));

			Doc1.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			AssertHasWarningContaining(Doc1.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(Doc1.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 1));

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			AssertHasWarningContaining(Doc1.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(Doc1.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 5));

			Doc1.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			AssertHasWarningContaining(Doc1.EQ_ValidToDateInfo, DateRangeValidation.WarningForFutureYear(Doc1.EQ_ValidToDate.ToString(DATE_FORMAT, CultureInfo.InvariantCulture), 5));
		}

		public void TestRowErrorForCostaRica()
		{
			var reqDoc = Factory.New<JobRequiredDocument>();

			var errorMessageExvDocumentType = @"Attribute: 'COSTA RICA EXV DOCUMENT TYPE' is required for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'";
			var errorMessageIssuingAuthorityName = @"Attribute: 'ISSUING AUTHORITY NAME' is required for Country/Region: 'Costa Rica', Document Type: 'EXV' and Usage: 'DBT'";

			var errorMessages = new string[] { errorMessageExvDocumentType, errorMessageIssuingAuthorityName };
			TestRowErrorForCostaRicaEXVAttributes();

			void TestRowErrorForCostaRicaEXVAttributes()
			{
				reqDoc.ClearRowNotifications();

				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Australia;
				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;

				foreach (var errorMessage in errorMessages)
				{
					AssertNoRowError(reqDoc, errorMessage);
				}

				reqDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.CostaRica;
				AssertEquals(errorMessages.Length, reqDoc.RowErrors.Count());
				foreach (var errorMessage in errorMessages)
				{
					Assert("Row validation should be added for CR country", reqDoc.RowErrors.Contains(errorMessage));
				}

				reqDoc.ClearRowNotifications();

				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
				foreach (var errorMessage in errorMessages)
				{
					AssertNoRowError(reqDoc, errorMessage);
				}

				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				AssertEquals(errorMessages.Length, reqDoc.RowErrors.Count());
				foreach (var errorMessage in errorMessages)
				{
					Assert("Row validation should be added for DBT Usage", reqDoc.RowErrors.Contains(errorMessage));
				}

				reqDoc.ClearRowNotifications();

				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.DemandLetter;
				foreach (var errorMessage in errorMessages)
				{
					AssertNoRowError(reqDoc, errorMessage);
				}

				reqDoc.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
				Assert("Precondition", reqDoc.EQ_DocUsage.IsEmpty);
				reqDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				foreach (var errorMessage in errorMessages)
				{
					Assert("Row validation should be added for EXV Document type", reqDoc.RowErrors.Contains(errorMessage));
				}
			}
		}

		public void TestCheckEQ_DocumentNotes()
		{
			var requiredDoc = Factory.New<JobRequiredDocument>();
			AssertNoErrors(requiredDoc.EQ_DocumentNotesInfo);
			requiredDoc.EQ_DocumentNotes = "这是中文";
			AssertNoErrors("Entering non-Western characters should not cause validation error", requiredDoc.EQ_DocumentNotesInfo);
		}

		DocManagerInfo SetupInvalidDocType()
		{
			const string docType = "AAA";
			const string docDesc = $"{docType}-Desc";

			var organisation = Factory.New<OrgHeader>();
			organisation.OH_Code = "TestCode";

			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_DocType = docType;
			refDocType.RT_Desc = docDesc;
			refDocType.RT_IsActive = true;
			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			Factory.Save();

			var docManagerInfo = ((IDocManagerSupport)organisation).DocManagerInfo;
			docManagerInfo.AddFileOrDocument(new byte[1], "test.txt", docType, description: docDesc);
			docManagerInfo.Save();

			var requiredDocuments = ((IHaveRequiredDocuments)docManagerInfo.StorageMain.DocumentOwner).RequiredDocuments;
			AssertEquals("There should now be 1 storage doc.", 1, docManagerInfo.AllEDocs.Count);
			AssertEquals("There should now be 1 required document.", 1, requiredDocuments.Count);

			var requiredDoc = requiredDocuments.GetElementsForTypeSortedByValidDate("AAA").FirstOrDefault();

			AssertNotNull("Required document type should be AAA", requiredDoc);
			AssertEquals("Required document type should be 'AAA-Desc'", docDesc, requiredDoc.EQ_DocDescription);

			refDocType.RT_ReferenceType = Core.Constants.ReferenceTypes.Accounting;
			Factory.Save();

			return docManagerInfo;
		}

		public void TestCheckEQ_DocType_InvalidDocType_WithEDoc()
		{
			var docManagerInfo = SetupInvalidDocType();

			var requiredDocuments = ((IHaveRequiredDocuments)docManagerInfo.StorageMain.DocumentOwner).RequiredDocuments;
			var requiredDoc = requiredDocuments.GetElementsForTypeSortedByValidDate("AAA").FirstOrDefault();

			requiredDoc.Delete();
			docManagerInfo.MasterFactory.Save();

			using (var tempFile = TempFile.New())
			{
				File.WriteAllBytes(tempFile.Filename, new byte[] { 1, 2, 3 });
				docManagerInfo.AddFileOrDocument(tempFile.Filename, "MSC");
				docManagerInfo.Save();
			}

			AssertEquals("There should now be 2 storage docs.", 2, docManagerInfo.AllEDocs.Count);
			AssertEquals("There should now be 1 required document.", 1, requiredDocuments.Count);

			requiredDoc = requiredDocuments.GetElementsForTypeSortedByValidDate("AAA").FirstOrDefault();
			AssertEquals("Required document type should have warning", true, requiredDoc.HasWarnings);
		}

		public void TestCheckEQ_DocType_InvalidDocType_WithoutEDoc()
		{
			var docManagerInfo = SetupInvalidDocType();

			var requiredDocuments = ((IHaveRequiredDocuments)docManagerInfo.StorageMain.DocumentOwner).RequiredDocuments;

			var eDocs = docManagerInfo.AllEDocs.Cast<IeDoc>().Single();
			docManagerInfo.AllEDocs.Remove(eDocs);
			eDocs.IsDeleted = true;
			docManagerInfo.MasterFactory.Save();

			AssertEquals("There should now be 0 storage doc.", 0, docManagerInfo.AllEDocs.Count);
			AssertEquals("There should now be 1 required document.", 1, requiredDocuments.Count);

			var requiredDoc = requiredDocuments.GetElementsForTypeSortedByValidDate("AAA").FirstOrDefault();
			requiredDoc.EQ_DocCategory = "TES";
			docManagerInfo.MasterFactory.Save();
			AssertEquals("Required document type should have error", true, requiredDoc.HasErrors);
		}

		public void TestCheckEQ_DocType_InvalidDocType_Updated()
		{
			var docManagerInfo = SetupInvalidDocType();

			var requiredDocuments = ((IHaveRequiredDocuments)docManagerInfo.StorageMain.DocumentOwner).RequiredDocuments;

			AssertEquals("There should now be 1 storage doc.", 1, docManagerInfo.AllEDocs.Count);
			AssertEquals("There should now be 1 required document.", 1, requiredDocuments.Count);

			var requiredDoc = requiredDocuments.GetElementsForTypeSortedByValidDate("AAA").FirstOrDefault();
			requiredDoc.EQ_DocType = "BBB";
			docManagerInfo.MasterFactory.Save();
			AssertEquals("Required document type should have error", true, requiredDoc.HasErrors);
		}

		#region Implementation

		IDocsAndCartageParent Shipment;
		JobRequiredDocument Doc1, Doc2;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = (IDocsAndCartageParent)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)), TestBusinessObjectKind.MinimumRequiredToSave);

			Doc1 = Shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			Doc2 = Shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
		}

		#endregion
	}
}
