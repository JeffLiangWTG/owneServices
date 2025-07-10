using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLine))]
	sealed class AddInfoJobComInvoiceLineTest : AddInfoAbstractTest
	{
		public void TestUS_LicenseNoReadOnly()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C33 });
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_LicenseType = ZString.Empty;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_LicenseType = ZString.Empty;
			Assert("should not read-only", !invoiceLine.US_LicenseNoInfo.ReadOnly);
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C33;
			Assert("should read-only", invoiceLine.US_LicenseNoInfo.ReadOnly);
			invoiceLine.US_LicenseNo = "ABC";
			Assert("should not read-only", !invoiceLine.US_LicenseNoInfo.ReadOnly);
		}

		public void TestUS_UC_NKCountryOfOriginExportReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals(false, invoiceLine.US_UC_NKCountryOfExportInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.US_UC_NKCountryOfExportInfo.ReadOnly);
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			AssertEquals(false, invoiceLine.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals(false, invoiceLine.US_UC_NKCountryOfExportInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.US_UC_NKCountryOfExportInfo.ReadOnly);
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals(false, invoiceLine.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.US_UC_NKCountryOfOriginInfo.ReadOnly);
			AssertEquals(false, invoiceLine.US_UC_NKCountryOfExportInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.US_UC_NKCountryOfExportInfo.ReadOnly);
		}

		public void TestUS_SPI_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var secondaryLine = invoice.InvoiceLines.AddNew();
			Assert(!secondaryLine.US_SPIInfo.ReadOnly);
			secondaryLine.JI_ParentID = invoiceLine.PK;
			Assert(!secondaryLine.US_SPIInfo.ReadOnly);
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038501Tariff.UE_Tariff; //99038501
			testHelper.ChildLine.US_SupTariff = testHelper.Test9802005060Tariff.UE_Tariff; //9802005060
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Assert(testHelper.ChildLine.US_SPIInfo.ReadOnly);
		}

		public void TestUS_WHSEntryNumberMaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("invoiceLine.US_WHSEntryNumberInfo.MaxLength", 20, invoiceLine.US_WHSEntryNumberInfo.MaxLength);
		}

		public void TestUS_TransactionRelatedReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(false, invoiceLine.US_TransactionsRelatedInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.US_TransactionsRelatedInfo.ReadOnly);
			AssertEquals(false, invoiceLine.US_TransactionsRelatedInfo.ReadOnly);
			AssertEquals(false, invoiceLine2.US_TransactionsRelatedInfo.ReadOnly);
			invoiceLine2.JI_ParentID = invoiceLine.PK;
			AssertEquals(false, invoiceLine.US_TransactionsRelatedInfo.ReadOnly);
			AssertEquals(true, invoiceLine2.US_TransactionsRelatedInfo.ReadOnly);
		}

		public void TestUS_SelectedRateTypeInfoReadOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "10000000";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);
			AssertEquals("IsSpecificSpecificDutyRate", true, tariff.IsSpecificSpecificDutyRate);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			AssertEquals("invoiceLine.US_SelectedRateTypeInfo.ReadOnly", false, invoiceLine.US_SelectedRateTypeInfo.ReadOnly);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			AssertEquals("IsSpecificSpecificDutyRate", false, tariff.IsSpecificSpecificDutyRate);
			AssertEquals("invoiceLine.US_SelectedRateTypeInfo.ReadOnly", true, invoiceLine.US_SelectedRateTypeInfo.ReadOnly);
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			AssertEquals("IsSpecificSpecificDutyRate", true, tariff.IsSpecificSpecificDutyRate);
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertNull("ImportTariff", invoiceLine.ImportTariff);
			AssertEquals("invoiceLine.US_SelectedRateTypeInfo.ReadOnly", true, invoiceLine.US_SelectedRateTypeInfo.ReadOnly);
		}

		public void TestValidationType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("IsImport", true, declaration.IsImport);
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			AddInfoJobComInvoiceLine addInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
			AssertEquals("Validation type", typeof(ACSImportAddInfoJobComInvoiceLineValidation), addInfo.Validation.GetType());
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals("Validation type", typeof(ACEImportAddInfoJobComInvoiceLineValidation), addInfo.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("IsExport", declaration.IsExport);
			Assert("IsExport", addInfo.IsExport);
			AssertEquals("Validation type", typeof(ExportAddInfoJobComInvoiceLineValidation), addInfo.Validation.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert("IsDrawback", declaration.IsDrawback);
			Assert("IsDrawback", addInfo.IsDrawback);
			AssertEquals("Validation type", typeof(ACSDrawbackAddInfoJobComInvoiceLineValidation), addInfo.Validation.GetType());
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Assert("IsACEDrawback", declaration.IsACEDrawback);
			Assert("IsACEDrawback", addInfo.IsACEDrawback);
			AssertEquals("Validation type", typeof(ACEDrawbackAddInfoJobComInvoiceLineValidation), addInfo.Validation.GetType());
		}

		public override void TestIsExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AddInfoJobComInvoiceLine addInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
			AssertEquals("PreCondition: IsExportDeclaration should be true", true, invoiceLine.IsExport);
			AssertEquals("IsExport", true, addInfo.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExport", false, addInfo.IsExport);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.JZ_JE = ZGuid.Invalid;
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			addInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
			AssertNull("PreCondition: InvoiceLine's Declaration should be null", invoiceLine.Declaration);
			AssertEquals("IsExport", false, addInfo.IsExport);
			invoice.JZ_JE = declaration.PK;
			AssertNotNull("PreCondition: InvoiceLine's Declaration should be not null", invoiceLine.Declaration);
			AssertEquals("IsExport", true, addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AddInfoJobComInvoiceLine addInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
			Assert("IsDrawback", addInfo.IsDrawback);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsDrawback", !addInfo.IsDrawback);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			invoice.JZ_JE = ZGuid.Invalid;
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			addInfo = new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
			AssertNull("PreCondition: InvoiceLine's Declaration should be null", invoiceLine.Declaration);
			Assert("IsDrawback", !addInfo.IsDrawback);
			invoice.JZ_JE = declaration.PK;
			AssertNotNull("PreCondition: InvoiceLine's Declaration should be not null", invoiceLine.Declaration);
			Assert("IsDrawback", addInfo.IsDrawback);
		}

		public void TestAddInfoValidationType()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = reconDeclaration.InvoiceLines.AddNew();
			AssertEquals(typeof(ReconAddInfoJobComInvoiceLineValidation), invoiceLine.AddInfoValidation.GetType());
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.Invoices.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			AssertEquals(typeof(ACSImportAddInfoJobComInvoiceLineValidation), invoiceLine2.AddInfoValidation.GetType());
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			AssertEquals(typeof(ACEImportAddInfoJobComInvoiceLineValidation), invoiceLine2.AddInfoValidation.GetType());
		}

		public void TestAddInfoValidationTypeWhenDifferentMessageTypeDeclaration()
		{
			var fakeDeclaration = Factory.New<JobDeclaration>();
			fakeDeclaration.MakeNonPersistent();
			fakeDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			fakeDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var standaloneInvoice = Factory.New<JobComInvoiceHeader>();
			standaloneInvoice.JZ_JE = fakeDeclaration.PK;
			standaloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			var invoiceLine = standaloneInvoice.InvoiceLines.AddNew();
			fakeDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				invoiceLine.Validation.ValidateJI_LinePrice();
			});
		}

		public void TestSetValueForUS_TransactionsRelated()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("US_TransactionsRelated field has value 'Y'.", YesNoDefaultList.Codes.Yes, invoiceLine.US_TransactionsRelated);

			invoiceLine.US_TransactionsRelated = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			AssertEquals("US_TransactionsRelated field has value 'N'.", YesNoDefaultList.Codes.No, invoiceLine.US_TransactionsRelated);

			invoiceLine.US_TransactionsRelated = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			invoiceLine.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("US_TransactionsRelated field shouldn't be set for Recon declaration", ZString.Empty, invoiceLine.US_TransactionsRelated);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoJobComInvoiceLineLookups);

		protected override Type GetExpectedValidationType() => typeof(ExportAddInfoJobComInvoiceLineValidation);

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.GetAddInfo();
		}
	}
}
