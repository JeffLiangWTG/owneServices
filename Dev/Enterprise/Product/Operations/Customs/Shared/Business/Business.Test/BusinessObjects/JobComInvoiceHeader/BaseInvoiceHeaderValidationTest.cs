using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseInvoiceHeaderValidationTest : TestCaseWithFactory
	{
		public void TestValidateJZ_Calc_TNIIsCalled()
		{
			TestInvoice invoice = Factory.New<TestInvoice>();
			invoice.RunPreSaveValidation();
			Assert("JZ_Calc_TNICalled should be true", invoice.CheckJZ_Calc_TNICalled);
		}

		public void TestBondedWarehousingInvoiceCannotHaveDifferentImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "O234";
			importer.MainAddress.OA_Address1 = "1";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "O236";
			importer2.MainAddress.OA_Address1 = "1";

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "O235";
			warehouse.MainAddress.OA_Address1 = "1";
			var declarationMock = Factory.NewMoq<BaseJobDeclarationForTesting>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive", true);
			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration) { CallBase = true };
			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>()).Returns(true);
			declarationMock.Protected().Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = importer2.PK;
			AssertNoMessageError(invoice.JZ_OH_BuyerInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"));

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoice.Validation.ValidateJZ_OH_Buyer();
			AssertHasMessageError(invoice.JZ_OH_BuyerInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"));
			invoice.JZ_OH_Buyer = importer.PK;
			AssertNoMessageError(invoice.JZ_OH_BuyerInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"));
			invoice.JZ_OH_Buyer = ZGuid.Empty;
			AssertNoMessageError(invoice.JZ_OH_BuyerInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentImporterMessage("Inventory Management"));
		}

		public void TestNoValidExchangeRateExistMessageWarningAdded()
		{
			var declarationMock = Factory.NewMoq<BaseJobDeclarationForTesting>();
			var declaration = declarationMock.Object;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 42.0;

			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "OOO";

			AssertNoMessageErrors("Must have no pre-existing errors", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo);

			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

			AssertHasMessageErrorContaining(invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo, "Please report missing exchange rate data to your system administrator.");

			var rate = newCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = "CUS";
			rate.RE_StartDate = ZDateTime.Now.AddSeconds(-1);
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(1);
			rate.RE_SellRate = 1.234m;

			invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

			AssertNoMessageErrors("No currency errors expected", invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo);
		}

		public void TestBondedWarehousingInvoiceCannotHaveDifferentSupplier()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "O234";
			supplier.MainAddress.OA_Address1 = "1";
			supplier.CompanyData.OB_IMUsedBondedWhs = true;
			var supplier2 = Factory.New<OrgHeader>();
			supplier2.OH_Code = "O236";
			supplier2.MainAddress.OA_Address1 = "1";

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "O235";
			warehouse.MainAddress.OA_Address1 = "1";
			var declarationMock = Factory.NewMoq<BaseJobDeclaration>();
			declarationMock.Protected().Setup<bool>("GetIsWHSUniversalXMLActive", true);
			declarationMock.Protected().Setup<bool>("IsOutwardBondedWarehousingEnabledCore").Returns(true);
			var declaration = declarationMock.Object;
			var helperMock = new Mock<BondedWarehousingHelper>(declaration) { CallBase = true };
			helperMock.Protected().Setup<bool>("IsMarkedForBondedWarehousingCore", ItExpr.IsAny<BaseJobComInvoiceLine>()).Returns(true);
			declarationMock.Protected().Setup<BondedWarehousingHelper>("GetNewBondedWarehousingHelper").Returns(helperMock.Object);
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.SetSupportsBondedWarehousingForTesting(true);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier2.PK;
			CombineAssertions(() =>
			{
				AssertNoMessageError("no error when no invoice line", invoice.JZ_OH_SupplierInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage("Inventory Management"));
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
				invoice.Validation.ValidateJZ_OH_Supplier();
				AssertHasMessageError("error when different supplier", invoice.JZ_OH_SupplierInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage("Inventory Management"));
				invoice.JZ_OH_Supplier = supplier.PK;
				AssertNoMessageError("no error when same", invoice.JZ_OH_SupplierInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage("Inventory Management"));
				invoice.JZ_OH_Supplier = ZGuid.Empty;
				AssertNoMessageError("no error when set empty", invoice.JZ_OH_SupplierInfo, BaseJobDeclaration.BondedWarehousingInvoiceCannotHaveDifferentSupplierMessage("Inventory Management"));
			});
		}

		public void TestJZ_Calc_Balance()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100.121m;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_InvoiceQuantity = 1;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_LinePrice = 100.124m;
			declaration.ResumeApportionment();
			AssertNoMessageError(invoice.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);
			invoiceLine.JI_LinePrice = 100.128m;
			declaration.ResumeApportionment();
			AssertHasMessageError(invoice.JZ_Calc_BalanceInfo, InvoiceHeaderValidation.UnbalancedInvoiceMessage);
		}

		public void TestValidateJZ_InvoiceCurrExRateForFallbackWarning()
		{
			var mockDeclaration = Factory.NewMoq<BaseJobDeclaration>();
			mockDeclaration.Setup(m => m.DateOfValuation).Returns(new ZDateTime(2005, 1, 2)); //NZ does not allow fallback // Then put another test in NZ. This is for base. 
			BaseJobDeclaration testDec = mockDeclaration.Object;

			TestCaseHelper.ClearTable("RefExchangeRate");

			var foreignCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, testDec.LocalCurrencyCode));
			RefExchangeRate rateOn1Jan = foreignCurrency.ExchangeRates.AddNew();
			rateOn1Jan.RE_ExRateType = "CUS";
			rateOn1Jan.RE_StartDate = new ZDateTime(2005, 1, 1);
			rateOn1Jan.RE_ExpiryDate = new ZDateTime(2005, 1, 1);
			rateOn1Jan.RE_SellRate = 0.7m;

			var mockInvoice = Factory.NewMoq<BaseJobComInvoiceHeader>();
			mockInvoice.Protected().Setup<ZBool>("IsJZ_InvoiceCurrExRateUserEnterableCore").Returns(new ZBool(false));
			BaseJobComInvoiceHeader invoice = mockInvoice.Object;

			invoice.JZ_JE = testDec.PK;
			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;
			AssertEquals("Exchange rate defaulted", 0.7m, invoice.JZ_InvoiceCurrExRate);

			RefExchangeRate rateOn2Jan = foreignCurrency.ExchangeRates.AddNew();
			rateOn2Jan.RE_ExRateType = "CUS";
			rateOn2Jan.RE_StartDate = new ZDateTime(2005, 1, 2);
			rateOn2Jan.RE_ExpiryDate = new ZDateTime(2005, 1, 2);
			rateOn2Jan.RE_SellRate = 0.8m;
			AssertEquals("PreCondition:EffectiveExchange rate", 0.8m, invoice.EffectiveExchangeRateForInvoiceCurr);

			invoice.RunPreSaveValidation();
			AssertEquals("JZ_InvoiceCurrExRate is not user-enterable", false, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertHasWarningContaining(invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is ");

			mockInvoice = Factory.NewMoq<BaseJobComInvoiceHeader>();
			mockInvoice.Protected().Setup<ZBool>("IsJZ_InvoiceCurrExRateUserEnterableCore").Returns(new ZBool(true));
			invoice = mockInvoice.Object;

			invoice.JZ_JE = testDec.PK;
			invoice.JZ_RX_NKInvoice_Currency = foreignCurrency.RX_Code;

			AssertEquals("JZ_InvoiceCurrExRate is user-enterable", true, invoice.IsJZ_InvoiceCurrExRateUserEnterable);
			AssertNoWarningContaining(invoice.JZ_InvoiceCurrExRateInfo, "This exchange rate was set when the currency was set. But the current rate for the valuation date is ");
		}

		public void TestChangingChargeAmountCurrencyValidatesIncoTerms()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			var mockInvoice = Factory.NewMoq<BaseJobComInvoiceHeader>();
			var invoice = mockInvoice.Object;
			testDec.Invoices.Add(invoice);
			invoice.JZ_JE = testDec.PK;
			var mockValidation = new Mock<InvoiceHeaderValidation>(invoice) { CallBase = true };
			mockInvoice.Protected().Setup<JobComInvoiceHeaderValidation>("GetNewValidation").Returns(mockValidation.Object);
			mockValidation.Protected().Setup<InvoiceHeaderValidation.TypeOfValidationForMissingMandatoryChargesForIncoterm>("ValidationForMissingMandatoryCharges").Returns(Enterprise.Customs.Business.InvoiceHeaderValidation.TypeOfValidationForMissingMandatoryChargesForIncoterm.MessageError);
			invoice.JZ_IncoTerm = "CIF";
			const string messageErrorText = "The following charges are missing for Incoterm";
			Assert("Does have MessageError", invoice.JZ_IncoTermInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
			BaseInvoiceCharge oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			BaseInvoiceCharge oNS = invoice.Charges.AddNew();
			oNS.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasInsurance;
			Assert("Still has MessageError", invoice.JZ_IncoTermInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
			oFT.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			Assert("Still has MessageError", invoice.JZ_IncoTermInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
			oNS.J7_RX_NKCurrency = testDec.LocalCurrencyCode;
			Assert("Does not have MessageError", !invoice.JZ_IncoTermInfo.GetMessageErrors().ContainsNotificationContaining(messageErrorText));
		}

		public const string DuplicateInvoiceNumberWithOtherDec = "This invoice number already exists";

		public void TestValidateJZ_InvoiceNumberForDuplicates()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "ORG1";

			var org2 = OrgHeader.New(Factory);
			org2.OH_Code = "ORG2";

			var org3 = OrgHeader.New(Factory);
			org3.OH_Code = "ORG3";

			ZString messageType;
			{
				var dec0 = BaseJobDeclaration.New(Factory);
				dec0.JE_OH_Importer = org1.PK;
				dec0.JE_OH_Supplier = org2.PK;
				messageType = dec0.JE_MessageType;
				dec0.Delete();
			}

			var dec1 = BaseJobDeclaration.New(Factory);
			var dec2 = BaseJobDeclaration.New(Factory);
			var declarationInvoice11 = dec1.Invoices.AddNew();
			var declarationInvoice12 = dec1.Invoices.AddNew();
			var declarationInvoice21 = dec2.Invoices.AddNew();

			var standaloneInvoice1 = Factory.New<BaseJobComInvoiceHeader>();
			var standaloneInvoice2 = Factory.New<BaseJobComInvoiceHeader>();

			dec1.JE_MessageType = messageType;
			dec2.JE_MessageType = messageType;
			standaloneInvoice1.JZ_MessageType = messageType;
			standaloneInvoice2.JZ_MessageType = messageType;

			CombineAssertions(() =>
			{
				foreach (var setOrganizationsOnDeclaration in new[] { true, false })
				{
					// complete match
					AssertDuplicate(true, declarationInvoice11, "ABC", org1, org2, declarationInvoice12, "ABC", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, declarationInvoice11, "ABC", org1, org2, declarationInvoice21, "ABC", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, declarationInvoice11, "ABC", org1, org2, standaloneInvoice1, "ABC", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, standaloneInvoice1, "ABC", org1, org2, declarationInvoice11, "ABC", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, standaloneInvoice1, "ABC", org1, org2, standaloneInvoice2, "ABC", org1, org2, setOrganizationsOnDeclaration);

					// different invoice number
					AssertDuplicate(false, declarationInvoice11, "ABC", org1, org2, declarationInvoice12, "ABC2", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, declarationInvoice11, "ABC", org1, org2, declarationInvoice21, "ABC2", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, declarationInvoice11, "ABC", org1, org2, standaloneInvoice1, "ABC2", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, standaloneInvoice1, "ABC", org1, org2, declarationInvoice11, "ABC2", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, standaloneInvoice1, "ABC", org1, org2, standaloneInvoice2, "ABC2", org1, org2, setOrganizationsOnDeclaration);

					// short invoice number
					AssertDuplicate(false, declarationInvoice11, "00", org1, org2, declarationInvoice12, "00", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, declarationInvoice11, "00", org1, org2, declarationInvoice21, "00", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, declarationInvoice11, "00", org1, org2, standaloneInvoice1, "00", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, standaloneInvoice1, "00", org1, org2, declarationInvoice11, "00", org1, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(false, standaloneInvoice1, "00", org1, org2, standaloneInvoice2, "00", org1, org2, setOrganizationsOnDeclaration);

					// different importer
					AssertDuplicate(true, declarationInvoice11, "ABC", org1, org2, declarationInvoice12, "ABC", org3, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, declarationInvoice11, "ABC", org1, org2, declarationInvoice21, "ABC", org3, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, declarationInvoice11, "ABC", org1, org2, standaloneInvoice1, "ABC", org3, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, standaloneInvoice1, "ABC", org1, org2, declarationInvoice11, "ABC", org3, org2, setOrganizationsOnDeclaration);
					AssertDuplicate(true, standaloneInvoice1, "ABC", org1, org2, standaloneInvoice2, "ABC", org3, org2, setOrganizationsOnDeclaration);

					// different supplier
					AssertDuplicate(true, declarationInvoice11, "ABC", org1, org2, declarationInvoice12, "ABC", org1, org3, setOrganizationsOnDeclaration);
					AssertDuplicate(false, declarationInvoice11, "ABC", org1, org2, declarationInvoice21, "ABC", org1, org3, setOrganizationsOnDeclaration);
					AssertDuplicate(false, declarationInvoice11, "ABC", org1, org2, standaloneInvoice1, "ABC", org1, org3, setOrganizationsOnDeclaration);
					AssertDuplicate(false, standaloneInvoice1, "ABC", org1, org2, declarationInvoice11, "ABC", org1, org3, setOrganizationsOnDeclaration);
					AssertDuplicate(false, standaloneInvoice1, "ABC", org1, org2, standaloneInvoice2, "ABC", org1, org3, setOrganizationsOnDeclaration);
				}
			});
		}

		void AssertDuplicate
		(
			bool expectedToBeDuplicate,
			BaseJobComInvoiceHeader invoice1, string invoiceNumber1, OrgHeader importer1, OrgHeader supplier1,
			BaseJobComInvoiceHeader invoice2, string invoiceNumber2, OrgHeader importer2, OrgHeader supplier2,
			bool setOrganizationsOnDeclaration
		)
		{
			AssertNotEquals("(pre-condition)", invoice1.PK, invoice2.PK);

			var dec1 = invoice1.JobDeclaration;
			var dec2 = invoice2.JobDeclaration;

			invoice1.JZ_InvoiceNumber = invoiceNumber1;

			if (dec1 != null && setOrganizationsOnDeclaration)
			{
				dec1.JE_OH_Importer = importer1.PK;
				dec1.JE_OH_Supplier = supplier1.PK;
			}
			else
			{
				invoice1.JZ_OH_Buyer = importer1.PK;
				invoice1.JZ_OH_Supplier = supplier1.PK;
			}

			if (dec2 != null && setOrganizationsOnDeclaration)
			{
				dec2.JE_OH_Importer = importer2.PK;
				dec2.JE_OH_Supplier = supplier2.PK;
			}
			else
			{
				invoice2.JZ_OH_Buyer = importer2.PK;
				invoice2.JZ_OH_Supplier = supplier2.PK;
			}

			AssertEquals("(pre-condition) invoice directions should match", invoice1.JZ_MessageType, invoice2.JZ_MessageType);

			Factory.Save();

			invoice2.JZ_InvoiceNumber = invoiceNumber2;

			AssertEquals
			(
				$"({invoiceNumber1} ({dec1?.JE_DeclarationReference ?? "standalone"}), '{importer1.OH_Code}', '{supplier1.OH_Code}') <-> " +
				$"({invoiceNumber2} ({dec2?.JE_DeclarationReference ?? "standalone"}), '{importer2.OH_Code}', '{supplier2.OH_Code}') " +
				$"(setOrganizationsOnDeclaration={setOrganizationsOnDeclaration})",
				expectedToBeDuplicate,
				invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec)
			);

			// cleanup, to avoid interference with other cases

			invoice1.JZ_InvoiceNumber = ZString.Empty;
			invoice1.JZ_OH_Buyer = ZGuid.Empty;
			invoice1.JZ_OH_Supplier = ZGuid.Empty;
			invoice2.JZ_InvoiceNumber = ZString.Empty;
			invoice2.JZ_OH_Buyer = ZGuid.Empty;
			invoice2.JZ_OH_Supplier = ZGuid.Empty;

			if (dec1 != null)
			{
				dec1.JE_OH_Importer = ZGuid.Empty;
				dec1.JE_OH_Supplier = ZGuid.Empty;
			}

			if (dec2 != null)
			{
				dec2.JE_OH_Importer = ZGuid.Empty;
				dec2.JE_OH_Supplier = ZGuid.Empty;
			}
		}

		public void TestValidateJZ_InvoiceNumberForDuplicatesByCompany()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "TS1";
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var company2 = Factory.New<GlbCompany>();
			company1.GC_Code = "TS2";
			company1.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var branch11 = company1.Branches.AddNew();
			var branch12 = company1.Branches.AddNew();
			var branch21 = company2.Branches.AddNew();
			branch11.GB_Code = "B11";
			branch12.GB_Code = "B12";
			branch21.GB_Code = "B21";

			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "ORG1";

			var org2 = OrgHeader.New(Factory);
			org2.OH_Code = "ORG2";

			var dec11 = BaseJobDeclaration.New(Factory);
			dec11.JE_DeclarationReference = "B00DEC4";
			var dec12 = BaseJobDeclaration.New(Factory);
			dec12.JE_DeclarationReference = "B00DEC1";
			var dec21 = BaseJobDeclaration.New(Factory);
			dec21.JE_DeclarationReference = "B00DEC3";
			var dec12B = BaseJobDeclaration.New(Factory);
			dec12B.JE_DeclarationReference = "B00DEC2";

			var declarationInvoice11 = dec11.Invoices.AddNew();
			var declarationInvoice12 = dec12.Invoices.AddNew();
			var declarationInvoice21 = dec21.Invoices.AddNew();
			var declarationInvoice12B = dec12B.Invoices.AddNew();

			var standaloneInvoice11 = Factory.New<BaseJobComInvoiceHeader>();
			var standaloneInvoice12 = Factory.New<BaseJobComInvoiceHeader>();
			var standaloneInvoice21 = Factory.New<BaseJobComInvoiceHeader>();

			dec11.JE_GB = branch11.PK;
			dec12.JE_GB = branch12.PK;
			dec21.JE_GB = branch21.PK;
			dec12B.JE_GB = branch12.PK;
			standaloneInvoice11.JZ_GB = branch11.PK;
			standaloneInvoice12.JZ_GB = branch12.PK;
			standaloneInvoice21.JZ_GB = branch21.PK;

			var declarations = new BaseJobDeclaration[]
			{
				dec11,
				dec12,
				dec12B,
				dec21
			};

			var declarationInvoices = new BaseJobComInvoiceHeader[]
			{
				declarationInvoice11,
				declarationInvoice12,
				declarationInvoice12B,
				declarationInvoice21
			};

			var standaloneInvoices = new BaseJobComInvoiceHeader[]
			{
				standaloneInvoice11,
				standaloneInvoice12,
				standaloneInvoice21
			};

			foreach (var declaration in declarations)
			{
				declaration.JE_OH_Importer = org1.PK;
				declaration.JE_OH_Supplier = org2.PK;
			}

			foreach (var invoice in declarationInvoices)
			{
				invoice.JZ_InvoiceNumber = "DEC-INV-123";
			}

			foreach (var invoice in standaloneInvoices)
			{
				invoice.JZ_MessageType = dec11.JE_MessageType;
				invoice.JZ_InvoiceNumber = "STD-INV-123";
				invoice.JZ_OH_Buyer = org1.PK;
				invoice.JZ_OH_Supplier = org2.PK;
			}

			Factory.Save();

			foreach (var invoice in declarationInvoices.Concat(standaloneInvoices))
			{
				invoice.Validation.ValidateJZ_InvoiceNumber();
			}

			AssertHasWarningContaining(declarationInvoice11.JZ_InvoiceNumberInfo, DuplicateInvoiceNumberWithOtherDec);
			AssertHasWarningContaining(declarationInvoice11.JZ_InvoiceNumberInfo, "This invoice number already exists in job(s): B00DEC2, B00DEC1");
			AssertHasWarningContaining(declarationInvoice12.JZ_InvoiceNumberInfo, "This invoice number already exists in job(s): B00DEC4, B00DEC2");
			AssertHasWarningContaining(declarationInvoice12B.JZ_InvoiceNumberInfo, "This invoice number already exists in job(s): B00DEC4, B00DEC1");
			AssertNoWarningContaining(declarationInvoice21.JZ_InvoiceNumberInfo, DuplicateInvoiceNumberWithOtherDec);
			AssertHasWarningContaining(standaloneInvoice11.JZ_InvoiceNumberInfo, "This invoice number already exists in job(s): STANDALONE INVOICE");
			AssertHasWarningContaining(standaloneInvoice12.JZ_InvoiceNumberInfo, "This invoice number already exists in job(s): STANDALONE INVOICE");
			AssertNoWarningContaining(standaloneInvoice21.JZ_InvoiceNumberInfo, DuplicateInvoiceNumberWithOtherDec);
		}

		public void TestValidateJZ_InvoiceNumberForDuplicateOnDrawback()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var org1 = OrgHeader.New(Factory);
				org1.OH_Code = "ORG1";

				var org2 = OrgHeader.New(Factory);
				org2.OH_Code = "ORG2";

				var testDec1 = BaseJobDeclaration.New(Factory);
				testDec1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				testDec1.JE_DeclarationReference = "B00000001";
				testDec1.JE_OH_Importer = org1.PK;

				var invoice1 = testDec1.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Does not have warning", false, invoice1.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
				Factory.Save();
				invoice1.Validation.ValidateJZ_InvoiceNumber();
				AssertEquals("Does not have warning", false, invoice1.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				var testDec2 = BaseJobDeclaration.New(Factory);
				testDec2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				testDec2.JE_OH_Importer = org1.PK;

				var invoice2 = testDec2.Invoices.AddNew();
				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Has expected warning", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
				AssertEquals("Referencing correct declaration", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining("B00000001"));

				invoice2.JZ_InvoiceNumber = "INVNO2";
				AssertEquals("Does not have warning", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
				testDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Does not have warning", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				testDec2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				invoice2.Validation.ValidateJZ_InvoiceNumber();
				AssertEquals("Has expected warning again ", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				testDec2.JE_OH_Importer = org2.PK;
				invoice2.Validation.ValidateJZ_InvoiceNumber();
				AssertEquals("Does not have warning now", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				testDec2.JE_OH_Importer = org1.PK;
				invoice2.Validation.ValidateJZ_InvoiceNumber();
				AssertEquals("Has expected warning again", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				invoice1.Delete();
				Factory.Save();
				invoice2.Validation.ValidateJZ_InvoiceNumber();
				AssertEquals("Does not have warning now, and no exception occured", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
			}
		}

		public void TestValidateJZ_InvoiceNumberForDuplicateNotOnDrawback()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var org1 = OrgHeader.New(Factory);
				org1.OH_Code = "ORG1";

				var org2 = OrgHeader.New(Factory);
				org2.OH_Code = "ORG2";

				var testDec1 = BaseJobDeclaration.New(Factory);
				testDec1.JE_MessageType = JobMessageTypeList.Codes.Import;
				testDec1.JE_DeclarationReference = "B00000001";
				testDec1.JE_OH_Supplier = org1.PK;

				var invoice1 = testDec1.Invoices.AddNew();
				invoice1.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Does not have warning", false, invoice1.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
				Factory.Save();
				invoice1.Validation.ValidateJZ_InvoiceNumber();
				AssertEquals("Does not have warning", false, invoice1.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				var testDec2 = BaseJobDeclaration.New(Factory);
				testDec2.JE_MessageType = JobMessageTypeList.Codes.Import;

				var invoice2 = testDec2.Invoices.AddNew();
				invoice2.JZ_OH_Supplier = org1.PK;
				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Has expected warning", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
				AssertEquals("Referencing correct declaration", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining("B00000001"));

				invoice2.JZ_InvoiceNumber = "INVNO2";
				AssertEquals("Does not have warning", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
				testDec2.JE_MessageType = JobMessageTypeList.Codes.Export;

				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Does not have warning", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
				testDec2.JE_MessageType = JobMessageTypeList.Codes.Import;

				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Has expected warning now", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				invoice2.JZ_OH_Supplier = org2.PK;
				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Does not have warning now", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				invoice2.JZ_OH_Supplier = org1.PK;
				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Has expected warning now", true, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				CustomsDataRegistry.Instance.WarnUserWhenCommercialInvoiceHasBeenUsedBefore.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				invoice2.JZ_InvoiceNumber = "INVNO";
				AssertEquals("Does not have warning now", false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
			}
		}

		public void TestValidateJZ_InvoiceNumberForDuplicateOnStandaloneInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var org1 = OrgHeader.New(Factory);
				org1.OH_Code = "ORG1";

				var org2 = OrgHeader.New(Factory);
				org2.OH_Code = "ORG2";

				var testDec2 = BaseJobDeclaration.New(Factory);
				testDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
				testDec2.JE_DeclarationReference = "B00000001";
				testDec2.JE_OH_Supplier = org1.PK;

				var standaloneInvoice1 = Factory.New<BaseJobComInvoiceHeader>();
				standaloneInvoice1.JZ_MessageType = JobMessageTypeList.Codes.Import;
				standaloneInvoice1.JZ_InvoiceNumber = "INVNO";
				standaloneInvoice1.JZ_OH_Supplier = org1.PK;
				Factory.Save();

				testDec2.Invoices.Add(standaloneInvoice1);
				standaloneInvoice1.Validation.ValidateJZ_InvoiceNumber();
				AssertEquals("Does not have warning", false, standaloneInvoice1.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
			}
		}

		public void TestValidateJZ_InvoiceNumberWhereInvoiceNumberLengthIsShort()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var supplier = OrgHeader.New(Factory);
				supplier.OH_Code = "ORG1";

				var dec = BaseJobDeclaration.New(Factory);
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_DeclarationReference = "B00000001";
				dec.JE_OH_Supplier = supplier.PK;

				var invoice = dec.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "1";
				Factory.Save();

				AssertEquals(false, invoice.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				var dec2 = BaseJobDeclaration.New(Factory);
				dec2.JE_MessageType = JobMessageTypeList.Codes.Import;

				var invoice2 = dec2.Invoices.AddNew();
				invoice2.JZ_OH_Supplier = supplier.PK;
				invoice2.JZ_InvoiceNumber = "1";
				AssertEquals(false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));

				invoice2.JZ_InvoiceNumber = "1";
				AssertEquals(false, invoice2.JZ_InvoiceNumberInfo.GetWarnings().ContainsNotificationContaining(DuplicateInvoiceNumberWithOtherDec));
			}
		}
	}
}
