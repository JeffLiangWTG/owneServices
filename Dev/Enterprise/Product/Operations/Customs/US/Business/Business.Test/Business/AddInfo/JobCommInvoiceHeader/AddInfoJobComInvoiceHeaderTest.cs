using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceHeader))]
	sealed class AddInfoJobComInvoiceHeaderTest : AddInfoAbstractTest
	{
		public void TestUS_ZoneStatus()
		{
			var dateForZoneStatusTest = ZDateTime.Today;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = ZoneStatusList.Codes.PrivilegedForeign;
			var invoice = declaration.Invoices.AddNew();
			for (int i = 0; i < invoice.AddInfoLookups.US_ZoneStatusList.Count; i++)
			{
				invoice.US_PrivilegedStatusDate = dateForZoneStatusTest;
				invoice.US_ZoneStatus = invoice.AddInfoLookups.US_ZoneStatusList[i].Code;
				if (invoice.US_ZoneStatus == ZoneStatusList.Codes.PrivilegedForeign)
				{
					AssertEquals("US_PrivilegedStatusDate should NOT be empty because US_PrivilegedStatusDate allowed for PrivilegedForeign Zone Status", dateForZoneStatusTest, invoice.US_PrivilegedStatusDate);
				}
				else
				{
					AssertEquals("US_PrivilegedStatusDate should be empty because US_PrivilegedStatusDate allowed only for PrivilegedForeign Zone Status", ZDateTime.Empty, invoice.US_PrivilegedStatusDate);
				}
			}
		}

		public void TestECCNNumberListOnInvoice()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C35, USAESLicenseCode.Codes.C36 });

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCCComplianceStatement, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber);
			var cusCode6A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "6A002", "6A002 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var cusCode5A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "5A002", "5A002 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var cusCode4A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "4A003", "4A003 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var cusCode3A = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ECCNNumber, "3A003", "3A003 Description", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode6A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode5A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode4A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C35);
			helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3A.PK.ToGuid(), Universal.RefCusCodeListAttributeTypes.Codes.LicenseType, USAESLicenseCode.Codes.C36);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			invoice.US_LicenseType = USAESLicenseCode.Codes.C35;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "00000000";
			invoiceLine.US_LicenseType = USAESLicenseCode.Codes.C37;

			var eccnCandidates = invoice.AddInfoLookups.US_ECCNList;
			eccnCandidates.Load();
			AssertEquals("ECCN Numbers with LicenseType C35", 3, eccnCandidates.Count);
			var item1 = invoice.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "6A002").FirstOrDefault();
			var item2 = invoice.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "4A003").FirstOrDefault();
			var item3 = invoice.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "5A002").FirstOrDefault();
			AssertEquals("ECCNList with LicenseType C35 -6A002", "6A002 Description", item1.ZZD_Description);
			AssertEquals("ECCNList with LicenseType C35 -4A003", "4A003 Description", item2.ZZD_Description);
			AssertEquals("ECCNList with LicenseType C35 -5A002", "5A002 Description", item3.ZZD_Description);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C36;
			var eccnCandidates2 = invoice.AddInfoLookups.US_ECCNList;
			eccnCandidates2.Load();
			AssertEquals("ECCN Numbers with LicenseType C36", 1, eccnCandidates2.Count);
			var item4 = invoice.AddInfoLookups.US_ECCNList.Where(x => x.ZZD_Code == "3A003").FirstOrDefault();
			AssertEquals("ECCNList with LicenseType C36 -3A003", "3A003 Description", item4.ZZD_Description);

			invoice.US_LicenseType = USAESLicenseCode.Codes.C37;
			var eccnCandidates3 = invoice.AddInfoLookups.US_ECCNList;
			eccnCandidates3.Load();
			AssertEquals("ECCN Numbers with LicenseType C37", 0, eccnCandidates3.Count);
		}

		public void TestDeliveryLocationFlags()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleK;
			AssertEquals(true, invoice.US_IsScheduleKTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsScheduleDTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsOtherTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsISOCountryCodeTermsOfDeliveryLocation);
			invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleD;
			AssertEquals(false, invoice.US_IsScheduleKTermsOfDeliveryLocation);
			AssertEquals(true, invoice.US_IsScheduleDTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsOtherTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsISOCountryCodeTermsOfDeliveryLocation);
			invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.Other;
			AssertEquals(false, invoice.US_IsScheduleKTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsScheduleDTermsOfDeliveryLocation);
			AssertEquals(true, invoice.US_IsOtherTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsISOCountryCodeTermsOfDeliveryLocation);
			invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ISOCountryCode;
			AssertEquals(false, invoice.US_IsScheduleKTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsScheduleDTermsOfDeliveryLocation);
			AssertEquals(false, invoice.US_IsOtherTermsOfDeliveryLocation);
			AssertEquals(true, invoice.US_IsISOCountryCodeTermsOfDeliveryLocation);
		}

		public void TestPaymentTermsAndPaymentTermsDesc()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.US_PaymentTerms = "";
			AssertEquals("US_PaymentTermsDesc", "", invoice.US_PaymentTermsDesc);
			PaymentTermsTypeList list = new PaymentTermsTypeList();
			foreach (ICodeDescription pair in list)
			{
				invoice.US_PaymentTerms = pair.Code;
				AssertEquals("US_PaymentTermsDesc", pair.Description, invoice.US_PaymentTermsDesc);
			}

			invoice.US_PaymentTerms = "ZZ";
			AssertEquals("US_PaymentTermsDesc", "", invoice.US_PaymentTermsDesc);
		}

		public void TestValidationType()
		{
			JobComInvoiceHeader invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AddInfoJobComInvoiceHeader addInfoInvoiceHeader = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
			AssertEquals("IsExport", false, addInfoInvoiceHeader.IsExport);
			AssertEquals("AddInfoJobComInvoiceHeader validation type", typeof(FormalImportAddInfoJobComInvoiceHeaderValidation), addInfoInvoiceHeader.Validation.GetType());
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Export;
			addInfoInvoiceHeader = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
			AssertEquals("IsExport", true, addInfoInvoiceHeader.IsExport);
			AssertEquals("AddInfoJobComInvoiceHeader validation type", typeof(ExportAddInfoJobComInvoiceHeaderValidation), addInfoInvoiceHeader.Validation.GetType());
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			addInfoInvoiceHeader = new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);
			Assert("IsDrawback", addInfoInvoiceHeader.IsDrawback);
			AssertEquals("AddInfoJobComInvoiceHeader validation type", typeof(DrawbackAddInfoJobComInvoiceHeaderValidation), addInfoInvoiceHeader.Validation.GetType());
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			invoice = reconDec.Invoices.AddNew();
			AssertEquals("AddInfoJobComInvoiceHeader validation type", typeof(ReconAddInfoJobComInvoiceHeaderValidation), invoice.AddInfoValidation.GetType());
		}

		public override void TestIsExport()
		{
			JobComInvoiceHeader invoiceHeader = Factory.New<JobComInvoiceHeader>();
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Export;
			AddInfoJobComInvoiceHeader addInfo = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
			AssertEquals("IsExport", true, addInfo.IsExport);
			invoiceHeader.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("IsExport", false, addInfo.IsExport);
		}

		public override void TestIsDrawback()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			AddInfoJobComInvoiceHeader addInfo = new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
			Assert("IsDrawback", addInfo.IsDrawback);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert("IsDrawback", !addInfo.IsDrawback);
		}

		public void TestGetNewValidation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			AddInfoJobComInvoiceHeader addInfo = new AddInfoJobComInvoiceHeader(invoice.JZ_AddInfoInfo);
			AssertEquals(typeof(FormalImportAddInfoJobComInvoiceHeaderValidation), addInfo.Validation.GetType());
		}

		public void TestMarkInvoiceLinesMarkAsNeedValidation()
		{
			var supplier1 = Factory.New<OrgHeader>();
			supplier1.OH_Code = "SUPPLIER1234";
			supplier1.OH_FullName = "SUPPLIER 1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = supplier1.PK;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier1.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.MarkLightValidationAsValidForTesting();
			Assert("InvoiceLine should validate on save", !invoiceLine.ShouldValidateOnSave);
			invoice.US_ECCN = "123";
			Assert("InvoiceLine should validate on save after change ECCN", invoiceLine.ShouldValidateOnSave);

			invoiceLine.MarkLightValidationAsValidForTesting();
			Assert("InvoiceLine should validate on save", !invoiceLine.ShouldValidateOnSave);
			invoice.US_LicenseType = "123";
			Assert("InvoiceLine should validate on save after change ECCN", invoiceLine.ShouldValidateOnSave);

			invoiceLine.MarkLightValidationAsValidForTesting();
			Assert("InvoiceLine should validate on save", !invoiceLine.ShouldValidateOnSave);
			declaration.US_ECCN = "123";
			Assert("InvoiceLine should validate on save after change ECCN", invoiceLine.ShouldValidateOnSave);

			invoiceLine.MarkLightValidationAsValidForTesting();
			Assert("InvoiceLine should validate on save", !invoiceLine.ShouldValidateOnSave);
			declaration.US_LicenseType = "123";
			Assert("InvoiceLine should validate on save after change ECCN", invoiceLine.ShouldValidateOnSave);
		}

		public void TestSetValueForUS_TransactionsRelated()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("US_TransactionsRelated field has value 'Y'.", YesNoDefaultList.Codes.Yes, invoice.US_TransactionsRelated);

			invoice.US_TransactionsRelated = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			AssertEquals("US_TransactionsRelated field has value 'N'.", YesNoDefaultList.Codes.No, invoice.US_TransactionsRelated);

			invoice.US_TransactionsRelated = ZString.Empty;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			AssertEquals("US_TransactionsRelated field shouldn't be set for Recon declaration", ZString.Empty, invoice.US_TransactionsRelated);
		}

		protected override Type GetExpectedLookupsType() => typeof(AddInfoJobComInvoiceHeaderLookups);

		protected override Type GetExpectedValidationType() => typeof(AddInfoJobComInvoiceHeaderValidation);

		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobComInvoiceHeader>().GetAddInfo();
	}
}
