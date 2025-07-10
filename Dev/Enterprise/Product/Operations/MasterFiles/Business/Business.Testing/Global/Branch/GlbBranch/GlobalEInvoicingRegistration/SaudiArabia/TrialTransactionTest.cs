using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia.Testing
{
	public class TrialTransactionTest : TestCaseWithFactory
	{
		const string CreditNotePreviousTransactionHash = "pzjhVbx14uakrgPfq+Wcv6nmY2VIjaHp5ZGKVqj9WlE=";
		const string DebitNotePreviousTransactionHash = "ZzCkU8X3VygSAuf1Ty6qCEBPu19jUrSCq5LCQ3YV7/w=";

		public void TestCreateTrialTransactionXmlWithNoCustomsCodes()
		{
			var supplier = CreateSupplierForTests();
			var customer = CreateCustomerForTests();
			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml();

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		public void TestCreateTrialTransactionXmlWithVatRegistration()
		{
			var supplierVatRegistration = "123456789";
			var customerVatRegistration = "987654321";
			var supplier = CreateSupplierForTests(vatRegistrationNumber: supplierVatRegistration);
			var customer = CreateCustomerForTests(vatRegistrationNumber: customerVatRegistration);
			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml(supplierVatRegistration: supplierVatRegistration, customerVatRegistration: customerVatRegistration);

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		public void TestCreateTrialTransactionXmlWithCompanyRegistrationNumber()
		{
			var supplierCRN = "7654321";
			var customerCRN = "9876453";
			var supplier = CreateSupplierForTests(customsCodes: new (string Type, string Code)[] { (OrgCusCode.CodeTypes.CompanyRegistrationNumber, supplierCRN) });
			var customer = CreateCustomerForTests(customsCodes: new (string Type, string Code)[] { (OrgCusCode.CodeTypes.CompanyRegistrationNumber, customerCRN) });

			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml(
				supplierSchemeId: OrgCusCode.CodeTypes.CompanyRegistrationNumber, supplierSchemeValue: supplierCRN,
				customerSchemeId: OrgCusCode.CodeTypes.CompanyRegistrationNumber, customerSchemeValue: customerCRN);

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		public void TestCreateTrialTransactionXmlWithCorporationCode()
		{
			var supplierGCR = "7654321";
			var customerGCR = "9876453";
			var supplier = CreateSupplierForTests(customsCodes: new (string Type, string Code)[] { (OrgCusCode.CodeTypes.CorporationCode, supplierGCR) });
			var customer = CreateCustomerForTests(customsCodes: new (string Type, string Code)[] { (OrgCusCode.CodeTypes.CorporationCode, customerGCR) });

			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml(
				supplierSchemeId: OrgCusCode.CodeTypes.CompanyRegistrationNumber, supplierSchemeValue: supplierGCR,
				customerSchemeId: OrgCusCode.CodeTypes.CompanyRegistrationNumber, customerSchemeValue: customerGCR);

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		public void TestCreateTrialTransactionXmlWithSaudiArabiaTaxIdentificationNumber()
		{
			var supplierTIN = "7654321";
			var customerTIN = "9876453";
			var supplier = CreateSupplierForTests(customsCodes: new (string Type, string Code)[] { (SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN, supplierTIN) });
			var customer = CreateCustomerForTests(customsCodes: new (string Type, string Code)[] { (SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN, customerTIN) });

			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml(
				customerSchemeId: SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN, customerSchemeValue: customerTIN);

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		public void TestCreateTrialTransactionXmlWithSaudiArabiaNationalId()
		{
			var supplierNAT = "7654321";
			var customerNAT = "9876453";
			var supplier = CreateSupplierForTests(customsCodes: new (string Type, string Code)[] { (SaudiArabiaOrgCusCodeInfo.OrgCusCodes.NAT, supplierNAT) });
			var customer = CreateCustomerForTests(customsCodes: new (string Type, string Code)[] { (SaudiArabiaOrgCusCodeInfo.OrgCusCodes.NAT, customerNAT) });

			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml(
				customerSchemeId: SaudiArabiaOrgCusCodeInfo.OrgCusCodes.NAT, customerSchemeValue: customerNAT);

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		public void TestCreateTrialTransactionXmlWithPassportID()
		{
			var supplierPAS = "7654321";
			var customerPAS = "9876453";
			var supplier = CreateSupplierForTests(customsCodes: new (string Type, string Code)[] { (OrgCusCode.CodeTypes.PassportID, supplierPAS) });
			var customer = CreateCustomerForTests(customsCodes: new (string Type, string Code)[] { (OrgCusCode.CodeTypes.PassportID, customerPAS) });

			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml(
				customerSchemeId: OrgCusCode.CodeTypes.PassportID, customerSchemeValue: customerPAS);

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		public void TestCreateTrialTransactionXmlWithMultipleCustomsCodes()
		{
			var gcr = "0123";
			var crn = "2345";
			var tin = "4567";
			var pas = "6789";
			var customsCodes = new (string Type, string Code)[]
			{
				(OrgCusCode.CodeTypes.CorporationCode, gcr),
				(OrgCusCode.CodeTypes.CompanyRegistrationNumber, crn),
				(SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN, tin),
				(OrgCusCode.CodeTypes.PassportID, pas)
			};
			var supplier = CreateSupplierForTests(customsCodes: customsCodes);
			var customer = CreateCustomerForTests(customsCodes: customsCodes);

			var (expectedInvoiceXml, expectedCreditNoteXml, expectedDebitNoteXml) = GetExpectedTransactionsXml(
				supplierSchemeId: OrgCusCode.CodeTypes.CompanyRegistrationNumber, supplierSchemeValue: crn,
				customerSchemeId: SaudiArabiaOrgCusCodeInfo.OrgCusCodes.TIN, customerSchemeValue: tin);

			var (invoiceXml, creditNoteXml, debitNoteXml) = GenerateTransactionsXml(supplier, customer);

			AssertMultilineASCIIEquals(expectedInvoiceXml, invoiceXml);
			AssertMultilineASCIIEquals(expectedCreditNoteXml, creditNoteXml);
			AssertMultilineASCIIEquals(expectedDebitNoteXml, debitNoteXml);
		}

		Supplier CreateSupplierForTests(string vatRegistrationNumber = null, (string Type, string Code)[] customsCodes = null)
		{
			var saBranch = Factory.NewWithValidTestData<GlbBranch>();
			saBranch.GB_Code = "SAB";
			var saOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			saOrgProxy.OH_FullName = "SA Test Proxy Org";
			saOrgProxy.MainAddress.OA_Address1 = "3709 KING ABDULLAH ROA";
			saOrgProxy.MainAddress.OA_Address2 = "AL MURSALAT";
			saOrgProxy.MainAddress.OA_City = "RIYAD";
			saOrgProxy.MainAddress.OA_State = "RIYADH PROVINC";
			saOrgProxy.MainAddress.OA_RN_NKCountryCode = "SA";
			saOrgProxy.MainAddress.OA_PostCode = "12462";
			saOrgProxy.OH_RL_NKClosestPort = "SAABT";
			if (!string.IsNullOrEmpty(vatRegistrationNumber))
			{
				saOrgProxy.PrimaryRegistrationNumber.NumberTypeForDisplay = "SA:VAT";
				saOrgProxy.PrimaryRegistrationNumber.Number = vatRegistrationNumber;
			}
			if (customsCodes != null)
			{
				foreach (var customsCode in customsCodes)
				{
					saOrgProxy.CustomsCodes.AddNew(customsCode.Type, customsCode.Code, Core.Constants.CountryCodes.SaudiArabia);
				}
			}
			saBranch.GB_OH_OrgProxy = saOrgProxy.PK;
			Factory.Save();

			return new Supplier(saBranch);
		}

		Customer CreateCustomerForTests(string vatRegistrationNumber = null, (string Type, string Code)[] customsCodes = null)
		{
			var saDebtor = Factory.NewWithValidTestData<OrgHeader>();
			saDebtor.OH_FullName = "SA Test Debtor";
			saDebtor.MainAddress.OA_Address1 = "1548 KING FAHD ROAD";
			saDebtor.MainAddress.OA_Address2 = "AL SHUMAISI DIST";
			saDebtor.MainAddress.OA_City = "RIYADH";
			saDebtor.MainAddress.OA_State = "RIYADH PROVINCE";
			saDebtor.MainAddress.OA_RN_NKCountryCode = "SA";
			saDebtor.MainAddress.OA_PostCode = "12633";

			var arAddress = saDebtor.Addresses.AddNew(OrgAddressType.Receivables, ZBool.True);
			arAddress.OA_Address1 = "7232 KHALID BIN AL WALID STREET";
			arAddress.OA_Address2 = "AL SHARAFEYYAH";
			arAddress.OA_City = "JEDDAH";
			arAddress.OA_State = "MAKKAH";
			arAddress.OA_RN_NKCountryCode = "SA";
			arAddress.OA_PostCode = "23218";

			saDebtor.OH_RL_NKClosestPort = "SAABT";
			if (!string.IsNullOrEmpty(vatRegistrationNumber))
			{
				saDebtor.PrimaryRegistrationNumber.NumberTypeForDisplay = "SA:VAT";
				saDebtor.PrimaryRegistrationNumber.Number = vatRegistrationNumber;
			}
			if (customsCodes != null)
			{
				foreach (var customsCode in customsCodes)
				{
					saDebtor.CustomsCodes.AddNew(customsCode.Type, customsCode.Code, Core.Constants.CountryCodes.SaudiArabia);
				}
			}
			Factory.Save();

			return new Customer(saDebtor);
		}

		(string invoiceXml, string creditNoteXml, string debitNoteXml) GetExpectedTransactionsXml(
			string supplierVatRegistration = null, string supplierSchemeId = null, string supplierSchemeValue = null,
			string customerVatRegistration = null, string customerSchemeId = null, string customerSchemeValue = null)
		{
			return (
				GetExpectedInvoiceXml(supplierVatRegistration, supplierSchemeId, supplierSchemeValue, customerVatRegistration, customerSchemeId, customerSchemeValue),
				GetExpectedCreditNoteXml(supplierVatRegistration, supplierSchemeId, supplierSchemeValue, customerVatRegistration, customerSchemeId, customerSchemeValue),
				GetExpectedDebitNoteXml(supplierVatRegistration, supplierSchemeId, supplierSchemeValue, customerVatRegistration, customerSchemeId, customerSchemeValue)
				);
		}

		string GetExpectedInvoiceXml(
			string supplierVatRegistration = null, string supplierSchemeId = null, string supplierSchemeValue = null,
			string customerVatRegistration = null, string customerSchemeId = null, string customerSchemeValue = null)
		{
			return $@"<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
	<cbc:ProfileID>reporting:1.0</cbc:ProfileID>
	<cbc:ID>00001000</cbc:ID>
	<cbc:UUID>8d487816-70b8-4ade-a618-9d620b73814a</cbc:UUID>
	<cbc:IssueDate>2023-07-01</cbc:IssueDate>
	<cbc:IssueTime>00:00:00</cbc:IssueTime>
	<cbc:InvoiceTypeCode name=""0100000"">388</cbc:InvoiceTypeCode>
	<cbc:DocumentCurrencyCode>SAR</cbc:DocumentCurrencyCode>
	<cbc:TaxCurrencyCode>SAR</cbc:TaxCurrencyCode>
	<cac:AdditionalDocumentReference>
		<cbc:ID>ICV</cbc:ID>
		<cbc:UUID>1</cbc:UUID>
	</cac:AdditionalDocumentReference>
	<cac:AdditionalDocumentReference>
		<cbc:ID>PIH</cbc:ID>
		<cac:Attachment>
			<cbc:EmbeddedDocumentBinaryObject mimeCode=""text/plain"">NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==</cbc:EmbeddedDocumentBinaryObject>
		</cac:Attachment>
	</cac:AdditionalDocumentReference>
	<cac:AccountingSupplierParty>
		<cac:Party>{(string.IsNullOrEmpty(supplierSchemeId)
			? string.Empty
			: $@"
			<cac:PartyIdentification>
				<cbc:ID schemeID=""{supplierSchemeId}"">{supplierSchemeValue ?? string.Empty}</cbc:ID>
			</cac:PartyIdentification>")}
			<cac:PostalAddress>
				<cbc:StreetName>KING ABDULLAH ROA</cbc:StreetName>
				<cbc:BuildingNumber>3709</cbc:BuildingNumber>
				<cbc:CitySubdivisionName>AL MURSALAT</cbc:CitySubdivisionName>
				<cbc:CityName>RIYAD</cbc:CityName>
				<cbc:PostalZone>12462</cbc:PostalZone>
				<cbc:CountrySubentity>RIYADH PROVINC</cbc:CountrySubentity>
				<cac:Country>
					<cbc:IdentificationCode>SA</cbc:IdentificationCode>
				</cac:Country>
			</cac:PostalAddress>
			<cac:PartyTaxScheme>{(string.IsNullOrEmpty(supplierVatRegistration)
				? string.Empty
				: $@"
				<cbc:CompanyID>{supplierVatRegistration}</cbc:CompanyID>")}
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:PartyTaxScheme>
			<cac:PartyLegalEntity>
				<cbc:RegistrationName>SA Test Proxy Org</cbc:RegistrationName>
			</cac:PartyLegalEntity>
		</cac:Party>
	</cac:AccountingSupplierParty>
	<cac:AccountingCustomerParty>
		<cac:Party>{(string.IsNullOrEmpty(customerSchemeId)
			? string.Empty
			: $@"
			<cac:PartyIdentification>
				<cbc:ID schemeID=""{customerSchemeId}"">{customerSchemeValue ?? string.Empty}</cbc:ID>
			</cac:PartyIdentification>")}
			<cac:PostalAddress>
				<cbc:StreetName>KHALID BIN AL WALID STREET</cbc:StreetName>
				<cbc:BuildingNumber>7232</cbc:BuildingNumber>
				<cbc:CitySubdivisionName>AL SHARAFEYYAH</cbc:CitySubdivisionName>
				<cbc:CityName>JEDDAH</cbc:CityName>
				<cbc:PostalZone>23218</cbc:PostalZone>
				<cbc:CountrySubentity>MAKKAH</cbc:CountrySubentity>
				<cac:Country>
					<cbc:IdentificationCode>SA</cbc:IdentificationCode>
				</cac:Country>
			</cac:PostalAddress>
			<cac:PartyTaxScheme>{(string.IsNullOrEmpty(customerVatRegistration)
				? string.Empty
				: $@"
				<cbc:CompanyID>{customerVatRegistration}</cbc:CompanyID>")}
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:PartyTaxScheme>
			<cac:PartyLegalEntity>
				<cbc:RegistrationName>SA Test Debtor</cbc:RegistrationName>
			</cac:PartyLegalEntity>
		</cac:Party>
	</cac:AccountingCustomerParty>
	<cac:Delivery>
		<cbc:ActualDeliveryDate>2023-07-01</cbc:ActualDeliveryDate>
	</cac:Delivery>
	<cac:PaymentMeans>
		<cbc:PaymentMeansCode>1</cbc:PaymentMeansCode>
	</cac:PaymentMeans>
	<cac:TaxTotal>
		<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
		<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID=""SAR"">100</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID>S</cbc:ID>
				<cbc:Percent>15</cbc:Percent>
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>
	</cac:TaxTotal>
	<cac:TaxTotal>
		<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
	</cac:TaxTotal>
	<cac:LegalMonetaryTotal>
		<cbc:LineExtensionAmount currencyID=""SAR"">100.00</cbc:LineExtensionAmount>
		<cbc:TaxExclusiveAmount currencyID=""SAR"">100.00</cbc:TaxExclusiveAmount>
		<cbc:TaxInclusiveAmount currencyID=""SAR"">115.00</cbc:TaxInclusiveAmount>
		<cbc:AllowanceTotalAmount currencyID=""SAR"">0</cbc:AllowanceTotalAmount>
		<cbc:PayableAmount currencyID=""SAR"">115.00</cbc:PayableAmount>
	</cac:LegalMonetaryTotal>
	<cac:InvoiceLine>
		<cbc:ID>1</cbc:ID>
		<cbc:InvoicedQuantity>1</cbc:InvoicedQuantity>
		<cbc:LineExtensionAmount currencyID=""SAR"">100</cbc:LineExtensionAmount>
		<cac:TaxTotal>
			<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
			<cbc:RoundingAmount currencyID=""SAR"">115.00</cbc:RoundingAmount>
		</cac:TaxTotal>
		<cac:Item>
			<cbc:Name>Freight</cbc:Name>
			<cac:ClassifiedTaxCategory>
				<cbc:ID>S</cbc:ID>
				<cbc:Percent>15</cbc:Percent>
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:ClassifiedTaxCategory>
		</cac:Item>
		<cac:Price>
			<cbc:PriceAmount currencyID=""SAR"">100</cbc:PriceAmount>
		</cac:Price>
	</cac:InvoiceLine>
</Invoice>";
		}

		string GetExpectedCreditNoteXml(
			string supplierVatRegistration = null, string supplierSchemeId = null, string supplierSchemeValue = null,
			string customerVatRegistration = null, string customerSchemeId = null, string customerSchemeValue = null)
		{
			return $@"<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
	<cbc:ProfileID>reporting:1.0</cbc:ProfileID>
	<cbc:ID>00001001</cbc:ID>
	<cbc:UUID>322efd74-5b1b-41e0-843c-1d13bec4475e</cbc:UUID>
	<cbc:IssueDate>2023-07-01</cbc:IssueDate>
	<cbc:IssueTime>00:00:00</cbc:IssueTime>
	<cbc:InvoiceTypeCode name=""0100000"">381</cbc:InvoiceTypeCode>
	<cbc:DocumentCurrencyCode>SAR</cbc:DocumentCurrencyCode>
	<cbc:TaxCurrencyCode>SAR</cbc:TaxCurrencyCode>
	<cac:BillingReference>
		<cac:InvoiceDocumentReference>
			<cbc:ID>00001000</cbc:ID>
		</cac:InvoiceDocumentReference>
	</cac:BillingReference>
	<cac:AdditionalDocumentReference>
		<cbc:ID>ICV</cbc:ID>
		<cbc:UUID>2</cbc:UUID>
	</cac:AdditionalDocumentReference>
	<cac:AdditionalDocumentReference>
		<cbc:ID>PIH</cbc:ID>
		<cac:Attachment>
			<cbc:EmbeddedDocumentBinaryObject mimeCode=""text/plain"">pzjhVbx14uakrgPfq+Wcv6nmY2VIjaHp5ZGKVqj9WlE=</cbc:EmbeddedDocumentBinaryObject>
		</cac:Attachment>
	</cac:AdditionalDocumentReference>
	<cac:AccountingSupplierParty>
		<cac:Party>{(string.IsNullOrEmpty(supplierSchemeId)
			? string.Empty
			: $@"
			<cac:PartyIdentification>
				<cbc:ID schemeID=""{supplierSchemeId}"">{supplierSchemeValue ?? string.Empty}</cbc:ID>
			</cac:PartyIdentification>")}
			<cac:PostalAddress>
				<cbc:StreetName>KING ABDULLAH ROA</cbc:StreetName>
				<cbc:BuildingNumber>3709</cbc:BuildingNumber>
				<cbc:CitySubdivisionName>AL MURSALAT</cbc:CitySubdivisionName>
				<cbc:CityName>RIYAD</cbc:CityName>
				<cbc:PostalZone>12462</cbc:PostalZone>
				<cbc:CountrySubentity>RIYADH PROVINC</cbc:CountrySubentity>
				<cac:Country>
					<cbc:IdentificationCode>SA</cbc:IdentificationCode>
				</cac:Country>
			</cac:PostalAddress>
			<cac:PartyTaxScheme>{(string.IsNullOrEmpty(supplierVatRegistration)
				? string.Empty
				: $@"
				<cbc:CompanyID>{supplierVatRegistration}</cbc:CompanyID>")}
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:PartyTaxScheme>
			<cac:PartyLegalEntity>
				<cbc:RegistrationName>SA Test Proxy Org</cbc:RegistrationName>
			</cac:PartyLegalEntity>
		</cac:Party>
	</cac:AccountingSupplierParty>
	<cac:AccountingCustomerParty>
		<cac:Party>{(string.IsNullOrEmpty(customerSchemeId)
			? string.Empty
			: $@"
			<cac:PartyIdentification>
				<cbc:ID schemeID=""{customerSchemeId}"">{customerSchemeValue ?? string.Empty}</cbc:ID>
			</cac:PartyIdentification>")}
			<cac:PostalAddress>
				<cbc:StreetName>KHALID BIN AL WALID STREET</cbc:StreetName>
				<cbc:BuildingNumber>7232</cbc:BuildingNumber>
				<cbc:CitySubdivisionName>AL SHARAFEYYAH</cbc:CitySubdivisionName>
				<cbc:CityName>JEDDAH</cbc:CityName>
				<cbc:PostalZone>23218</cbc:PostalZone>
				<cbc:CountrySubentity>MAKKAH</cbc:CountrySubentity>
				<cac:Country>
					<cbc:IdentificationCode>SA</cbc:IdentificationCode>
				</cac:Country>
			</cac:PostalAddress>
			<cac:PartyTaxScheme>{(string.IsNullOrEmpty(customerVatRegistration)
				? string.Empty
				: $@"
				<cbc:CompanyID>{customerVatRegistration}</cbc:CompanyID>")}
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:PartyTaxScheme>
			<cac:PartyLegalEntity>
				<cbc:RegistrationName>SA Test Debtor</cbc:RegistrationName>
			</cac:PartyLegalEntity>
		</cac:Party>
	</cac:AccountingCustomerParty>
	<cac:Delivery>
		<cbc:ActualDeliveryDate>2023-07-01</cbc:ActualDeliveryDate>
	</cac:Delivery>
	<cac:PaymentMeans>
		<cbc:PaymentMeansCode>1</cbc:PaymentMeansCode>
		<cbc:InstructionNote>CANCELLATION_OR_TERMINATION</cbc:InstructionNote>
	</cac:PaymentMeans>
	<cac:TaxTotal>
		<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
		<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID=""SAR"">100</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID>S</cbc:ID>
				<cbc:Percent>15</cbc:Percent>
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>
	</cac:TaxTotal>
	<cac:TaxTotal>
		<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
	</cac:TaxTotal>
	<cac:LegalMonetaryTotal>
		<cbc:LineExtensionAmount currencyID=""SAR"">100.00</cbc:LineExtensionAmount>
		<cbc:TaxExclusiveAmount currencyID=""SAR"">100.00</cbc:TaxExclusiveAmount>
		<cbc:TaxInclusiveAmount currencyID=""SAR"">115.00</cbc:TaxInclusiveAmount>
		<cbc:AllowanceTotalAmount currencyID=""SAR"">0</cbc:AllowanceTotalAmount>
		<cbc:PayableAmount currencyID=""SAR"">115.00</cbc:PayableAmount>
	</cac:LegalMonetaryTotal>
	<cac:InvoiceLine>
		<cbc:ID>1</cbc:ID>
		<cbc:InvoicedQuantity>1</cbc:InvoicedQuantity>
		<cbc:LineExtensionAmount currencyID=""SAR"">100</cbc:LineExtensionAmount>
		<cac:TaxTotal>
			<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
			<cbc:RoundingAmount currencyID=""SAR"">115.00</cbc:RoundingAmount>
		</cac:TaxTotal>
		<cac:Item>
			<cbc:Name>Freight</cbc:Name>
			<cac:ClassifiedTaxCategory>
				<cbc:ID>S</cbc:ID>
				<cbc:Percent>15</cbc:Percent>
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:ClassifiedTaxCategory>
		</cac:Item>
		<cac:Price>
			<cbc:PriceAmount currencyID=""SAR"">100</cbc:PriceAmount>
		</cac:Price>
	</cac:InvoiceLine>
</Invoice>";
		}

		string GetExpectedDebitNoteXml(
			string supplierVatRegistration = null, string supplierSchemeId = null, string supplierSchemeValue = null,
			string customerVatRegistration = null, string customerSchemeId = null, string customerSchemeValue = null)
		{
			return $@"<Invoice xmlns=""urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"" xmlns:cac=""urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"" xmlns:cbc=""urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"" xmlns:ext=""urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"">
	<cbc:ProfileID>reporting:1.0</cbc:ProfileID>
	<cbc:ID>00001002</cbc:ID>
	<cbc:UUID>ad2c3a8b-1cc8-4060-8a35-0d6aa8ff6fcc</cbc:UUID>
	<cbc:IssueDate>2023-07-01</cbc:IssueDate>
	<cbc:IssueTime>00:00:00</cbc:IssueTime>
	<cbc:InvoiceTypeCode name=""0100000"">383</cbc:InvoiceTypeCode>
	<cbc:DocumentCurrencyCode>SAR</cbc:DocumentCurrencyCode>
	<cbc:TaxCurrencyCode>SAR</cbc:TaxCurrencyCode>
	<cac:BillingReference>
		<cac:InvoiceDocumentReference>
			<cbc:ID>00001000</cbc:ID>
		</cac:InvoiceDocumentReference>
	</cac:BillingReference>
	<cac:AdditionalDocumentReference>
		<cbc:ID>ICV</cbc:ID>
		<cbc:UUID>3</cbc:UUID>
	</cac:AdditionalDocumentReference>
	<cac:AdditionalDocumentReference>
		<cbc:ID>PIH</cbc:ID>
		<cac:Attachment>
			<cbc:EmbeddedDocumentBinaryObject mimeCode=""text/plain"">ZzCkU8X3VygSAuf1Ty6qCEBPu19jUrSCq5LCQ3YV7/w=</cbc:EmbeddedDocumentBinaryObject>
		</cac:Attachment>
	</cac:AdditionalDocumentReference>
	<cac:AccountingSupplierParty>
		<cac:Party>{(string.IsNullOrEmpty(supplierSchemeId)
			? string.Empty
			: $@"
			<cac:PartyIdentification>
				<cbc:ID schemeID=""{supplierSchemeId}"">{supplierSchemeValue ?? string.Empty}</cbc:ID>
			</cac:PartyIdentification>")}
			<cac:PostalAddress>
				<cbc:StreetName>KING ABDULLAH ROA</cbc:StreetName>
				<cbc:BuildingNumber>3709</cbc:BuildingNumber>
				<cbc:CitySubdivisionName>AL MURSALAT</cbc:CitySubdivisionName>
				<cbc:CityName>RIYAD</cbc:CityName>
				<cbc:PostalZone>12462</cbc:PostalZone>
				<cbc:CountrySubentity>RIYADH PROVINC</cbc:CountrySubentity>
				<cac:Country>
					<cbc:IdentificationCode>SA</cbc:IdentificationCode>
				</cac:Country>
			</cac:PostalAddress>
			<cac:PartyTaxScheme>{(string.IsNullOrEmpty(supplierVatRegistration)
				? string.Empty
				: $@"
				<cbc:CompanyID>{supplierVatRegistration}</cbc:CompanyID>")}
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:PartyTaxScheme>
			<cac:PartyLegalEntity>
				<cbc:RegistrationName>SA Test Proxy Org</cbc:RegistrationName>
			</cac:PartyLegalEntity>
		</cac:Party>
	</cac:AccountingSupplierParty>
	<cac:AccountingCustomerParty>
		<cac:Party>{(string.IsNullOrEmpty(customerSchemeId)
			? string.Empty
			: $@"
			<cac:PartyIdentification>
				<cbc:ID schemeID=""{customerSchemeId}"">{customerSchemeValue ?? string.Empty}</cbc:ID>
			</cac:PartyIdentification>")}
			<cac:PostalAddress>
				<cbc:StreetName>KHALID BIN AL WALID STREET</cbc:StreetName>
				<cbc:BuildingNumber>7232</cbc:BuildingNumber>
				<cbc:CitySubdivisionName>AL SHARAFEYYAH</cbc:CitySubdivisionName>
				<cbc:CityName>JEDDAH</cbc:CityName>
				<cbc:PostalZone>23218</cbc:PostalZone>
				<cbc:CountrySubentity>MAKKAH</cbc:CountrySubentity>
				<cac:Country>
					<cbc:IdentificationCode>SA</cbc:IdentificationCode>
				</cac:Country>
			</cac:PostalAddress>
			<cac:PartyTaxScheme>{(string.IsNullOrEmpty(customerVatRegistration)
				? string.Empty
				: $@"
				<cbc:CompanyID>{customerVatRegistration}</cbc:CompanyID>")}
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:PartyTaxScheme>
			<cac:PartyLegalEntity>
				<cbc:RegistrationName>SA Test Debtor</cbc:RegistrationName>
			</cac:PartyLegalEntity>
		</cac:Party>
	</cac:AccountingCustomerParty>
	<cac:Delivery>
		<cbc:ActualDeliveryDate>2023-07-01</cbc:ActualDeliveryDate>
	</cac:Delivery>
	<cac:PaymentMeans>
		<cbc:PaymentMeansCode>1</cbc:PaymentMeansCode>
		<cbc:InstructionNote>CANCELLATION_OR_TERMINATION</cbc:InstructionNote>
	</cac:PaymentMeans>
	<cac:TaxTotal>
		<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
		<cac:TaxSubtotal>
			<cbc:TaxableAmount currencyID=""SAR"">100</cbc:TaxableAmount>
			<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
			<cac:TaxCategory>
				<cbc:ID>S</cbc:ID>
				<cbc:Percent>15</cbc:Percent>
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:TaxCategory>
		</cac:TaxSubtotal>
	</cac:TaxTotal>
	<cac:TaxTotal>
		<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
	</cac:TaxTotal>
	<cac:LegalMonetaryTotal>
		<cbc:LineExtensionAmount currencyID=""SAR"">100.00</cbc:LineExtensionAmount>
		<cbc:TaxExclusiveAmount currencyID=""SAR"">100.00</cbc:TaxExclusiveAmount>
		<cbc:TaxInclusiveAmount currencyID=""SAR"">115.00</cbc:TaxInclusiveAmount>
		<cbc:AllowanceTotalAmount currencyID=""SAR"">0</cbc:AllowanceTotalAmount>
		<cbc:PayableAmount currencyID=""SAR"">115.00</cbc:PayableAmount>
	</cac:LegalMonetaryTotal>
	<cac:InvoiceLine>
		<cbc:ID>1</cbc:ID>
		<cbc:InvoicedQuantity>1</cbc:InvoicedQuantity>
		<cbc:LineExtensionAmount currencyID=""SAR"">100</cbc:LineExtensionAmount>
		<cac:TaxTotal>
			<cbc:TaxAmount currencyID=""SAR"">15.00</cbc:TaxAmount>
			<cbc:RoundingAmount currencyID=""SAR"">115.00</cbc:RoundingAmount>
		</cac:TaxTotal>
		<cac:Item>
			<cbc:Name>Freight</cbc:Name>
			<cac:ClassifiedTaxCategory>
				<cbc:ID>S</cbc:ID>
				<cbc:Percent>15</cbc:Percent>
				<cac:TaxScheme>
					<cbc:ID>VAT</cbc:ID>
				</cac:TaxScheme>
			</cac:ClassifiedTaxCategory>
		</cac:Item>
		<cac:Price>
			<cbc:PriceAmount currencyID=""SAR"">100</cbc:PriceAmount>
		</cac:Price>
	</cac:InvoiceLine>
</Invoice>";
		}

		(string invoiceXml, string creditNoteXml, string debitNoteXml) GenerateTransactionsXml(Supplier supplier, Customer customer)
		{
			return (
				TrialTransaction.Invoice(supplier, customer).ToXml(),
				TrialTransaction.CreditNote(supplier, customer, CreditNotePreviousTransactionHash).ToXml(),
				TrialTransaction.DebitNote(supplier, customer, DebitNotePreviousTransactionHash).ToXml()
			);
		}
	}
}
