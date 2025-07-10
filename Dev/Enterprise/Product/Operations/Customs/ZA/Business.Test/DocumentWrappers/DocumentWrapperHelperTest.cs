using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders.Testing;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	sealed class DocumentWrapperHelperTest : TestCaseWithFactory
	{
		public void TestFormatTariffCode()
		{
			AssertEquals("XXX", DocumentWrapperHelper.FormatTariffCode("XXX"));
			AssertEquals("1234.56.78 (9)", DocumentWrapperHelper.FormatTariffCode("123456789"));
			AssertEquals("1234.56.78", DocumentWrapperHelper.FormatTariffCode("12345678"));
			AssertEquals("1234.56", DocumentWrapperHelper.FormatTariffCode("123456"));
			AssertEquals("1234", DocumentWrapperHelper.FormatTariffCode("1234"));
		}

		public void TestFormatRebateItemCode()
		{
			AssertEquals("XXX", DocumentWrapperHelper.FormatNonDutyTariffCode("XXX"));
			AssertEquals("123.34.56.78.90", DocumentWrapperHelper.FormatNonDutyTariffCode("12334567890"));
			AssertEquals("123.34.56.78", DocumentWrapperHelper.FormatNonDutyTariffCode("123345678"));
			AssertEquals("123.34.56", DocumentWrapperHelper.FormatNonDutyTariffCode("1233456"));
		}

		public void TestFindMatchingEntryLine()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var tariffStartDate = ZDateTime.Today.AddYears(-1);
			var tariffEndDate = ZDateTime.Today.AddYears(1);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "10000000", tariffStartDate, tariffEndDate);
			universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "20000000", tariffStartDate, tariffEndDate);
			Factory.Save();
			AssertNull(null, DocumentWrapperHelper.FindMatchingEntryLine(null, null));
			AssertNull(null, DocumentWrapperHelper.FindMatchingEntryLine(null, new LineLevelInformationForTest()));
			AssertNull(null, DocumentWrapperHelper.FindMatchingEntryLine(null, new LineLevelInformationForTest { LineNumber = "0001", TariffCode = "10000000" }));
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "10000000";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "20000000";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.InvoiceLines.Add(invoiceLine2);
			entryLine2.CL_LineNumber = 2;
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, null));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest()));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0001" }));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { TariffCode = "10000000" }));
			AssertEquals(entryLine1.PK, DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0001", TariffCode = "10000000" })?.PK);
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0001", TariffCode = "20000000" }));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0001", TariffCode = "30000000" }));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0002", TariffCode = "10000000" }));
			AssertEquals(entryLine2.PK, DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0002", TariffCode = "20000000" })?.PK);
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0002", TariffCode = "30000000" }));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0003", TariffCode = "10000000" }));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0003", TariffCode = "20000000" }));
			AssertNull(DocumentWrapperHelper.FindMatchingEntryLine(entryHeader, new LineLevelInformationForTest { LineNumber = "0003", TariffCode = "30000000" }));
		}

		public void TestGetFallbackOrgAddressInfo()
		{
			AssertExceptionThrown<ArgumentNullException>(() => DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, null, null, null));
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, null, null, OrgCusCode.CodeTypes.SupplierCode));
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, null, null, OrgCusCode.CodeTypes.CustomsClientCode));
			AssertExceptionThrown<ArgumentNullException>(() => DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "" }, null, null));
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "" }, null, OrgCusCode.CodeTypes.SupplierCode));
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "" }, null, OrgCusCode.CodeTypes.CustomsClientCode));
			AssertExceptionThrown<ArgumentNullException>(() => DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "UNK" }, null, null));
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "UNK" }, null, OrgCusCode.CodeTypes.SupplierCode));
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "UNK" }, null, OrgCusCode.CodeTypes.CustomsClientCode));
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG-1";
			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CUS-1", Core.Constants.CountryCodes.SouthAfrica);
			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "SUP-1", Core.Constants.CountryCodes.SouthAfrica);
			org1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT-1", Core.Constants.CountryCodes.SouthAfrica);
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "ORG-2";
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CUS-2", Core.Constants.CountryCodes.SouthAfrica);
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "SUP-2", Core.Constants.CountryCodes.SouthAfrica);
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT-2", Core.Constants.CountryCodes.SouthAfrica);
			var org2Dup = Factory.NewWithValidTestData<OrgHeader>();
			org2Dup.OH_FullName = "ORG-2 DUP";
			org2Dup.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "CUS-2", Core.Constants.CountryCodes.SouthAfrica);
			org2Dup.CustomsCodes.AddNew(OrgCusCode.CodeTypes.SupplierCode, "SUP-2", Core.Constants.CountryCodes.SouthAfrica);
			org2Dup.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT-2", Core.Constants.CountryCodes.SouthAfrica);
			// code in source does not match any organization
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "UNK" }, org1, OrgCusCode.CodeTypes.CustomsClientCode));
			AssertNull(DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "UNK" }, org1, OrgCusCode.CodeTypes.SupplierCode));
			IAddressInformation info;
			// empty source, correct code should be is picked from current organization
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, null, org1, OrgCusCode.CodeTypes.CustomsClientCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("CUS-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, null, org1, OrgCusCode.CodeTypes.SupplierCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("SUP-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			// empty source, correct code should be is picked from current organization
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "" }, org1, OrgCusCode.CodeTypes.CustomsClientCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("CUS-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "" }, org1, OrgCusCode.CodeTypes.SupplierCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("SUP-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			// source matches current organization
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "CUS-1" }, org1, OrgCusCode.CodeTypes.CustomsClientCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("CUS-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "SUP-1" }, org1, OrgCusCode.CodeTypes.SupplierCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("SUP-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			// source matches current organization
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "CUS-2" }, org2Dup, OrgCusCode.CodeTypes.CustomsClientCode);
			AssertEquals("ORG-2 DUP", info?.Name);
			AssertEquals("CUS-2", info?.OrganizationCode);
			AssertEquals("VAT-2", info?.VATRegistrationNo);
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "SUP-2" }, org2Dup, OrgCusCode.CodeTypes.SupplierCode);
			AssertEquals("ORG-2 DUP", info?.Name);
			AssertEquals("SUP-2", info?.OrganizationCode);
			AssertEquals("VAT-2", info?.VATRegistrationNo);
			// source does not match current organization, but there is organzation in database with matching code
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "CUS-1" }, org2, OrgCusCode.CodeTypes.CustomsClientCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("CUS-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "SUP-1" }, org2, OrgCusCode.CodeTypes.SupplierCode);
			AssertEquals("ORG-1", info?.Name);
			AssertEquals("SUP-1", info?.OrganizationCode);
			AssertEquals("VAT-1", info?.VATRegistrationNo);
			// source does not match current organization, but there are several organzations in database with matching code
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "CUS-2" }, org1, OrgCusCode.CodeTypes.CustomsClientCode);
			// not checking name, because either org2 or org2Dup can be selected
			AssertEquals("CUS-2", info?.OrganizationCode);
			AssertEquals("VAT-2", info?.VATRegistrationNo);
			info = DocumentWrapperHelper.GetFallbackOrgAddressInfo(Factory, new AddressInformationForTest { OrganizationCode = "SUP-2" }, org1, OrgCusCode.CodeTypes.SupplierCode);
			// not checking name, because either org2 or org2Dup can be selected
			AssertEquals("SUP-2", info?.OrganizationCode);
			AssertEquals("VAT-2", info?.VATRegistrationNo);
		}

		public void TestGetUnregisteredTraderPrefixedNumber()
		{
			AssertEquals("Null IAddressInformation should be empty", ZString.Empty, DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(null));
			var addInfo = new AddressInformationForTest();
			AssertEquals("Empty IAddressInformation should be empty", ZString.Empty, DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(addInfo));
			addInfo.OrganizationCodeQualifier = "999";
			addInfo.OrganizationCode = "ABC123";
			AssertEquals("Wrong Code should be empty", ZString.Empty, DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(addInfo));
			addInfo.OrganizationCode = "";
			addInfo.OrganizationCodeQualifier = Edifact.D96B.Elements.CodeListQualifierList.CitizenIdentification.ToString();
			AssertEquals("Empty Org code should be empty", ZString.Empty, DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(addInfo));
			addInfo.OrganizationCode = "ABC123";
			AssertEquals("ID", "IDN: ABC123", DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(addInfo));
			addInfo.OrganizationCodeQualifier = Edifact.D96B.Elements.CodeListQualifierList.TaxPartyIdentification.ToString();
			AssertEquals("Tax", "TAX: ABC123", DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(addInfo));
			addInfo.OrganizationCodeQualifier = Edifact.D96B.Elements.CodeListQualifierList.PassportNumber.ToString();
			AssertEquals("Passport", "PAS: ABC123", DocumentWrapperHelper.GetUnregisteredTraderPrefixedNumber(addInfo));
		}
	}
}
