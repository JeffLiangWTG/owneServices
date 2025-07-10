using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(CommercialInvoiceWrapper))]
	sealed class USCommercialInvoiceWrapperTest : Enterprise.DocumentWrappers.GenericWrappers.Base.Testing.GenericWrapperTest
	{
		public void TestUSBuyer()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "ABC";
			orgHeader.MainAddress.OA_Address1 = "NJ";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "KNZ";
			contact.OC_Phone = "123123";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.All.ToString();
			document.OD_DefaultContact = true;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_InvoiceDate = ZDate.BrettsBirthday;
			invoiceHeader.BuyerOrgPK = orgHeader.PK;
			var invoice = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertNotNull(invoice);
			AssertEquals(invoice.Buyer.ContactPhone, "123123");
			AssertEquals(invoice.Buyer.CompanyName, "ABC");
			AssertEquals(invoice.Buyer.ContactName, "KNZ");
			AssertEquals(invoice.Buyer.CompanyAddress, "NJ");
			AssertEquals(invoice.LCDateNumberOrInvoiceDate, ZDate.BrettsBirthday.ToShortDateString());
		}

		public void TestUSBuyerFromJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.Import;
			var buyerAddress = declaration.DocAddresses.AddNew(Enterprise.MasterFiles.Integration.DocAddressType.BuyerDocumentaryAddress);
			buyerAddress.E2_CompanyName = "SH CS00392108";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.JZ_InvoiceDate = ZDate.BrettsBirthday;
			var invoice = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			invoice = new CommercialInvoiceWrapper(invoiceHeader, Factory);
			AssertEquals("SH CS00392108", invoice.Buyer.CompanyName);
		}

		public override void TestWrapperMappingsEmpty()
		{
			BaseJobComInvoiceHeader nullInvoiceHeader = Factory.GetNull<BaseJobComInvoiceHeader>();
			CommercialInvoiceWrapper wrapperEmpty = new CommercialInvoiceWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "", wrapperEmpty.ToString());
			AssertEquals(nullInvoiceHeader.JZ_IncoTerm, wrapperEmpty.IncoTerm.Code);
			AssertEquals(null, wrapperEmpty.FreightJob);
			AssertEquals(ZString.Empty, wrapperEmpty.InsuredValue.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.InvoiceAmount.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.Importer.CompanyName);
			AssertEquals(ZString.Empty, wrapperEmpty.Supplier.CompanyName);
			AssertEquals(ZString.Empty, wrapperEmpty.Volume.ToString());
			AssertEquals(ZString.Empty, wrapperEmpty.Weight.ToString());
			AssertEquals(ZDecimal.Zero, wrapperEmpty.ExchangeRate);
			AssertEquals(nullInvoiceHeader.JZ_InvoiceDate, wrapperEmpty.InvoiceDate);
			AssertEquals(ZString.Empty, wrapperEmpty.InvoiceNumber);
			AssertEquals(0, wrapperEmpty.InvoiceLines.Count);
			AssertEquals(0, wrapperEmpty.UnclassifiedInvoiceLines.Count);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new CommercialInvoiceWrapper(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CommercialInvoice                       (Default Field: InvoiceNumber)
======================================================================
Name                                    Type
----------------------------------------------------------------------
IncoTerm                                CodeAndDescription
CountryOfOrigin                         Country
FreightJob                              Freight
ExcludedChargesTotal                    Money
ExcludedCommission                      Money
ExcludedDiscount                        Money
ExcludedDutiableOtherCharges            Money
ExcludedExWorksCharges                  Money
ExcludedInlandFreight                   Money
ExcludedLandingCharges                  Money
ExcludedNonDutiableOtherCharges         Money
ExcludedOverseasFreight                 Money
ExcludedOverseasInsurance               Money
ExcludedPackingCharges                  Money
IncludedChargesTotal                    Money
IncludedCommission                      Money
IncludedDiscount                        Money
IncludedDutiableOtherCharges            Money
IncludedExWorksCharges                  Money
IncludedInlandFreight                   Money
IncludedLandingCharges                  Money
IncludedNonDutiableOtherCharges         Money
IncludedOverseasFreight                 Money
IncludedOverseasInsurance               Money
IncludedPackingCharges                  Money
InvoiceAmount                           Money
Buyer                                   Organisation
Importer                                Organisation
Supplier                                Organisation
NoOfPacks                               ValueAndUnit
Volume                                  ValueAndUnit
Weight                                  ValueAndUnit
AdditionalInformation                   String
AdditionalPaymentTerms                  String
ApprovalNumber                          String
DateOfIssue                             DateTime
ExchangeRate                            Decimal
ExportersBankAccountNo                  String
ExportersBankName                       String
ExportersBankSWIFTCode                  String
ImporterRequiredVATNumber               String
InsurancePolicyNumber                   String
InsuredValue                            String
InvoiceDate                             DateTime
InvoiceNumber                           String
JobNumber                               String
LCDateNumber                            String
LCDateNumberOrInvoiceDate               String
LetterOfCreditDate                      String
LetterOfCreditNumber                    String
LocalChamberOfCommerceInfo              String
MarksAndNumbers                         String
NameOfSignatory                         String
NotaryPublicInfo                        String
SignatoryCompany                        String
SupplierRequiredVATNumber               String

InvoiceLines                            CommercialInvoiceLine Collection
UnclassifiedInvoiceLines                CommercialInvoiceLine Collection
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Buyer : 
CountryOfOrigin : NZ - New Zealand
ExcludedChargesTotal : 96.00 NZD
ExcludedCommission : 29.00 NZD
ExcludedDiscount : 31.00 NZD
ExcludedDutiableOtherCharges : 19.00 NZD
ExcludedExWorksCharges : 7.00 NZD
ExcludedInlandFreight : 11.00 NZD
ExcludedLandingCharges : 17.00 NZD
ExcludedNonDutiableOtherCharges : 23.00 NZD
ExcludedOverseasFreight : 3.00 NZD
ExcludedOverseasInsurance : 5.00 NZD
ExcludedPackingCharges : 13.00 NZD
FreightJob : 
Importer : BUY_NAME\nBUY_ADDRESS1\nBUY_ADDRESS2\nBUY_CITY BUY_S BUY_PC
IncludedChargesTotal : 96.00 NZD
IncludedCommission : 29.00 NZD
IncludedDiscount : 31.00 NZD
IncludedDutiableOtherCharges : 19.00 NZD
IncludedExWorksCharges : 7.00 NZD
IncludedInlandFreight : 11.00 NZD
IncludedLandingCharges : 17.00 NZD
IncludedNonDutiableOtherCharges : 23.00 NZD
IncludedOverseasFreight : 3.00 NZD
IncludedOverseasInsurance : 5.00 NZD
IncludedPackingCharges : 13.00 NZD
IncoTerm : UAF
InvoiceAmount : 1,234.56 NZD
NoOfPacks : 100 PKG
Registry : (No Default Field Value Available on Registry)
Supplier : SUP_NAME\nSUP_ADDRESS1\nSUP_ADDRESS2\nSUP_CITY SUP_S SUP_PC
Volume : 4.53 M3
Weight : 340 KG
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var currencyCode = RefCurrency.LoadFromCurrencyCode(Factory, "NZD");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacksPackType = "PKG";
			var invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_RN_NKDefaultOrigin = "NZ";
			invoiceHeader.JZ_OH_Buyer = OrgTestHelper.GetNewOrganisation("BUY", Factory).PK;
			invoiceHeader.JZ_OH_Supplier = OrgTestHelper.GetNewOrganisation("SUP", Factory).PK;
			invoiceHeader.JZ_InvoiceNumber = "NUMBER_ONE";
			invoiceHeader.JZ_InvoiceAmount = 1234.56m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode.RX_Code;
			invoiceHeader.JZ_Volume = 4.53m;
			invoiceHeader.JZ_VolumeUQ = "M3";
			invoiceHeader.JZ_Weight = 340m;
			invoiceHeader.JZ_WeightUQ = "KG";
			invoiceHeader.JZ_InvoiceCurrExRate = 0.56m;
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2006, 8, 9);
			invoiceHeader.JZ_IncoTerm = Enterprise.Core.Constants.IncoTerms.UnpackedAtFactory;
			invoiceHeader.JZ_NoOfPacks = 100m;
			invoiceHeader.Charges.RemoveAll();

			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 3m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 5m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.ExWorks, 7m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 11m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 13m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.LandingCharges, 17m, currencyCode.RX_Code);
			var dutiableOtherCharge = invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 19m, currencyCode.RX_Code);
			dutiableOtherCharge.J7_IsDutiable = true;
			var nonDutiableOtherCharge = invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 23m, currencyCode.RX_Code);
			nonDutiableOtherCharge.J7_IsDutiable = false;
			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.Commission, 29m, currencyCode.RX_Code);
			invoiceHeader.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.Discount, 31m, currencyCode.RX_Code);

			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight, 3m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OverseasInsurance, 5m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.ForeignInlandFreight, 11m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.ExWorks, 7m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.PackingCost, 13m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.LandingCharges, 17m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			var dutiableOtherGroupCharge = invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 19m, currencyCode.RX_Code);
			dutiableOtherGroupCharge.J7_IsDutiable = true;
			dutiableOtherGroupCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			var nonDutiableOtherGroupCharge = invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 23m, currencyCode.RX_Code);
			nonDutiableOtherGroupCharge.J7_IsDutiable = false;
			nonDutiableOtherGroupCharge.J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.Commission, 29m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;
			invoiceHeader.GroupCharges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.Discount, 31m, currencyCode.RX_Code).J7_Calc_IsIncludedInInvoiceAmount = false;

			return new CommercialInvoiceWrapper(invoiceHeader, Factory);
		}
	}
}
