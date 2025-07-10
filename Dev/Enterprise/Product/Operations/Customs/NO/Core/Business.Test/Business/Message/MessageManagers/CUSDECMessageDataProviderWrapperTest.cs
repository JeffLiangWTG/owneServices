using System;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact.V902.Elements;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSDECMessageDataProviderWrapper))]
sealed class CUSDECMessageDataProviderWrapperTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertArgumentExceptionThrown("When messageSendingObject is null", "messageSendingObject", () => _ = new CUSDECMessageDataProviderWrapper(null));
		var dataProviderMock = new Mock<MessageSendingObject>(entryHeader);
		AssertNoExceptionThrown("happy path", () => _ = new CUSDECMessageDataProviderWrapper(dataProviderMock.Object));
		entryHeader.CH_CEI_Instruction = ZGuid.Empty;
		AssertNoExceptionThrown("When EntryInstruction is not populated", () => _ = new CUSDECMessageDataProviderWrapper(dataProviderMock.Object));
	});

	public void TestDeclarationType() => CombineAssertions(() =>
	{
		declaration.JE_MessageSubType = "EU";
		entryInstruction.CEI_Style = "4";
		var message1 = GetTestMessage();
		AssertEquals("DeclarationType, when EntryInstruction is not null", new ZString("EU4"), message1.DeclarationType);

		entryHeader.CH_CEI_Instruction = ZGuid.Empty;
		var message2 = GetTestMessage();
		AssertEquals("DeclarationType, when EntryInstruction is null", new ZString("EU"), message2.DeclarationType);
	});

	public void TestAssociationAssignedCode() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("AssociationAssignedCode, when export", new ZString("NEP"), GetTestMessage().AssociationAssignedCode);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("AssociationAssignedCode, when import", new ZString("NEP-I"), GetTestMessage().AssociationAssignedCode);
	});

	public void TestDeclarationReferenceNumber()
	{
		declaration.JE_DeclarationReference = "01011498465KLL";
		var message = GetTestMessage();
		AssertEquals("DeclarationReferenceNumber", "01011498465KLL", message.DeclarationReferenceNumber);
	}

	public void TestLocalReferenceNumber() => CombineAssertions(() =>
	{
		var message1 = GetTestMessage();
		AssertEquals("LocalReferenceNumber, when BGMReference is empty", "<<JOB REFERENCE NUMBER PLACE HOLDER>>", message1.LocalReferenceNumber);

		entryHeader.CH_BGMReference = "9355964402023012400003701";
		var message2 = GetTestMessage();
		AssertEquals("LocalReferenceNumber, when BGMReference is not empty", "9355964402023012400003701", message2.LocalReferenceNumber);
	});

	public void TestDocumentMessageName() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("DocumentMessageName, when export", DocumentMessageNameCodedList.GoodsDeclarationForExportation, GetTestMessage().DocumentMessageName);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("DocumentMessageName, when import", DocumentMessageNameCodedList.GoodsDeclarationForImportation, GetTestMessage().DocumentMessageName);
	});

	public void TestMessageType()
	{
		var message = GetTestMessage();
		AssertEquals("MessageType", new ZString("MA"), message.MessageType);
	}

	public void TestTransactionNature()
	{
		invoiceHeader.JZ_ValuationCode = "01";
		var message = GetTestMessage();
		AssertEquals("ValuationCode", new ZString("01"), message.TransactionNature);
	}

	public void TestControlNumber() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var importer = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_Importer = importer.PK;
		AssertEquals("ControlNumber, when import and Importer without DefermentApprovalNumber account", ZString.Empty, GetTestMessage().ControlNumber);

		importer.AsDeferredDutiesAccount("987654321");
		AssertEquals("ControlNumber, when import and Importer with DefermentApprovalNumber account", "987654321", GetTestMessage().ControlNumber);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		declaration.JE_OH_Supplier = supplier.PK;
		AssertEquals("ControlNumber, when export and Supplier without DefermentApprovalNumber account", ZString.Empty, GetTestMessage().ControlNumber);

		supplier.AsDeferredDutiesAccount("123456789");
		AssertEquals("ControlNumber, when export and Supplier with DefermentApprovalNumber account", "123456789", GetTestMessage().ControlNumber);
	});

	public void TestGoodsNumber()
	{
		declaration.JE_GoodsNumber = "123456789";
		var message = GetTestMessage();
		AssertEquals("GoodsNumber", new ZString("123456789"), message.GoodsNumber);
	}

	public void TestReExportOriginalDeclaration() => CombineAssertions(() =>
	{
		var message = GetTestMessage();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Always initially empty", ZString.Empty, message.RelatedDeclaration);

		entryHeader.CH_ReCalcOrigDecl = "123456789";
		AssertEquals("Always empty when JE_CopyStatus is not 'REX' or 'FIN'", ZString.Empty, message.RelatedDeclaration);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
		AssertEquals("When both JE_CopyStatus and CH_ReCalcOrigDecl", "123456789", message.RelatedDeclaration);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.FinalImport;
		AssertEquals("When both JE_CopyStatus and CH_ReCalcOrigDecl", "123456789", message.RelatedDeclaration);
	});

	public void TestReExportReason() => CombineAssertions(() =>
	{
		var message = GetTestMessage();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Always initially empty", ZString.Empty, message.ReExportReason);

		entryHeader.CH_ReCalcReason = "Some Reason";
		AssertEquals("Always empty when JE_CopyStatus is not 'REX' or 'FIN'", ZString.Empty, message.ReExportReason);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.ReExport;
		AssertEquals("When both JE_CopyStatus and CH_ReCalcReason", "Some Reason", message.ReExportReason);

		declaration.JE_CopyStatus = NODeclarationCopyStatus.Codes.FinalImport;
		AssertEquals("When both JE_CopyStatus and CH_ReCalcReason", "Some Reason", message.ReExportReason);
	});

	public void TestGoodsNumberPosition() => CombineAssertions(() =>
	{
		declaration.JE_Position = "123";
		var message1 = GetTestMessage();
		AssertEquals("GoodsNumberPosition, when SubPosition is empty", "123", message1.GoodsNumberPosition);

		entryInstruction.CEI_SubPosition = "42";
		var message2 = GetTestMessage();
		AssertEquals("GoodsNumberPosition, when SubPosition is not empty", "123/42", message2.GoodsNumberPosition);

		entryHeader.CH_CEI_Instruction = ZGuid.Empty;
		var message3 = GetTestMessage();
		AssertEquals("GoodsNumberPosition, when EntryInstruction is empty", "123", message3.GoodsNumberPosition);
	});

	public void TestGoodsDestination() => CombineAssertions(() =>
	{
		declaration.JE_GoodsDestination = Core.Constants.CountryCodes.Sweden;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("GoodsDestination, when export", new ZString("SE"), GetTestMessage().GoodsDestination);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("GoodsDestination, when import", ZString.Empty, GetTestMessage().GoodsDestination);
	});

	public void TestGoodsOrigin_ImportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		declaration.JE_GoodsOrigin = "SE";
		var message1 = GetTestMessage();
		AssertEquals("GoodsOrigin, when country is other than NO", new ZString("SE"), message1.GoodsOrigin);

		declaration.JE_GoodsOrigin = "NO";
		var message2 = GetTestMessage();
		AssertEquals("GoodsOrigin, when country is NO", new ZString("NO"), message2.GoodsOrigin);
	});

	public void TestGoodsOrigin_ExportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		declaration.JE_GoodsOrigin = "SE";
		var message1 = GetTestMessage();
		AssertEquals("GoodsOrigin, when country is other than NO", new ZString("SE"), message1.GoodsOrigin);

		declaration.JE_GoodsOrigin = "NO";
		var message2 = GetTestMessage();
		AssertEquals("GoodsOrigin, when country is NO", ZString.Empty, message2.GoodsOrigin);
	});

	public void TestCustomsOfficeOfExit() => CombineAssertions(() =>
	{
		declaration.JE_CustomsOffice = NOCustomsOfficesList.Codes._3210;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("CustomsOfficeOfExit, when export", new ZString("3210"), GetTestMessage().CustomsOfficeOfExit);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("CustomsOfficeOfExit, when import", ZString.Empty, GetTestMessage().CustomsOfficeOfExit);
	});

	public void TestLocationOfGoods()
	{
		declaration.JE_LocationOfGoods = "A";
		var message = GetTestMessage();
		AssertEquals("LocationOfGoods", new ZString("A"), message.LocationOfGoods);
	}

	public void TestTransportMode()
	{
		declaration.JE_CustomsTransportMode = "30";
		var message = GetTestMessage();
		AssertEquals("TransportMode", "30", message.TransportMode);
	}

	public void TestTransportNationality()
	{
		declaration.JE_RN_NKTransportNationality = "NO";
		var message = GetTestMessage();
		AssertEquals("TransportNationality", "NO", message.TransportNationality);
	}

	public void TestContainerMode()
	{
		declaration.JE_ContainerMode = "CNT";
		var message = GetTestMessage();
		AssertEquals("ContainerMode", "CNT", message.ContainerMode);
	}

	public void TestExporterCustomsRegNo() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var supplierMvaRegistered = Factory.NewWithValidTestData<OrgHeader>().AsMVARegistered();
		declaration.JE_OH_Supplier = supplierMvaRegistered.PK;
		var message1 = GetTestMessage();
		AssertEquals("ExporterCustomsRegNo, when export and Supplier is company with MVA account", "123456789", message1.ExporterCustomsRegNo);

		var supplierPerson = Factory.NewWithValidTestData<OrgHeader>().WithOrgCusCode(OrgCusCode.CodeTypes.OrganizationNumber, "987654321");
		supplierPerson.OH_Category = UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber;
		declaration.JE_OH_Supplier = supplierPerson.PK;
		var message2 = GetTestMessage();
		AssertEquals("ExporterCustomsRegNo, when export and Supplier is person with associated ORG-number", "987654321", message2.ExporterCustomsRegNo);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("ExporterCustomsRegNo, when import", ZString.Empty, GetTestMessage().ExporterCustomsRegNo);
	});

	public void TestImporterCustomsRegNo() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var importerMvaRegistered = Factory.NewWithValidTestData<OrgHeader>().AsMVARegistered();
		declaration.JE_OH_Importer = importerMvaRegistered.PK;
		var message1 = GetTestMessage();
		AssertEquals("ImporterCustomsRegNo, when Importer is company with MVA account", "123456789", message1.ImporterCustomsRegNo);

		var importerPerson = Factory.NewWithValidTestData<OrgHeader>().WithOrgCusCode(UniversalReferenceConstants.OrgCodeType.SocialSecurityNumber, "987654321");
		importerPerson.OH_Category = UniversalReferenceConstants.OrgHeaderType.NaturalPerson;
		declaration.JE_OH_Importer = importerPerson.PK;
		var message2 = GetTestMessage();
		AssertEquals("ImporterCustomsRegNo, when Importer is person with associated SSN-number", "987654321", message2.ImporterCustomsRegNo);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("ImporterCustomsRegNo, when export", ZString.Empty, GetTestMessage().ImporterCustomsRegNo);
	});

	public void TestSenderAddressType() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		AssertEquals("SenderAddressType, when export", PartyQualifierList.Consignee, GetTestMessage().SenderAddressType);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("SenderAddressType, when import", PartyQualifierList.Seller, GetTestMessage().SenderAddressType);
	});

	public void TestSenderFullName_ImportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var message1 = GetTestMessage();
		AssertEquals("SenderFullName, when Consignor is null", ZString.Empty, message1.SenderFullName);

		CreateAndAssignConsignorToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderFullName, when Consignor is not null", "Mister Anderson", message2.SenderFullName);
	});

	public void TestSenderFullName_ExportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var message1 = GetTestMessage();
		AssertEquals("SenderFullName, when Importer is null", ZString.Empty, message1.SenderFullName);

		CreateAndAssignImporterToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderFullName, when Importer is not null", "Mister Importer", message2.SenderFullName);
	});

	public void TestSenderAddress1_ImportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var message1 = GetTestMessage();
		AssertEquals("SenderAddress1, when Consignor is null", ZString.Empty, message1.SenderAddress1);

		CreateAndAssignConsignorToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderAddress1, when Consignor is not null", "Add1", message2.SenderAddress1);
	});

	public void TestSenderAddress1_ExportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var message1 = GetTestMessage();
		AssertEquals("SenderAddress1, when Importer is null", ZString.Empty, message1.SenderAddress1);

		CreateAndAssignImporterToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderAddress1, when Importer is not null", "Some Street", message2.SenderAddress1);
	});

	public void TestSenderAddress2_ImportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var message1 = GetTestMessage();
		AssertEquals("SenderAddress2, when Consignor is null", ZString.Empty, message1.SenderAddress2);

		CreateAndAssignConsignorToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderAddress2, when Consignor is not null", "Add2", message2.SenderAddress2);
	});

	public void TestSenderAddress2_ExportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var message1 = GetTestMessage();
		AssertEquals("SenderAddress2, when Importer is null", ZString.Empty, message1.SenderAddress2);

		CreateAndAssignImporterToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderAddress2, when Importer is not null", "Nr. 1", message2.SenderAddress2);
	});

	public void TestSenderAddress3_ImportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var message1 = GetTestMessage();
		AssertEquals("SenderAddress3, when Consignor is null", ZString.Empty, message1.SenderAddress3);

		CreateAndAssignConsignorToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderAddress3, when Consignor is not null", "42 Zion ZZ", message2.SenderAddress3);
	});

	public void TestSenderAddress3_ExportDeclaration() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

		var message1 = GetTestMessage();
		AssertEquals("SenderAddress3, when Importer is null", ZString.Empty, message1.SenderAddress3);

		CreateAndAssignImporterToDeclaration(declaration);
		var message2 = GetTestMessage();
		AssertEquals("SenderAddress3, when Importer is not null", "42 Wiesbaden DE", message2.SenderAddress3);
	});

	public void TestDeclarantCustomsRegNo() => CombineAssertions(() =>
	{
		var message1 = GetTestMessage();
		AssertEquals("DeclarantCustomsRegNo, when Declarant is null", ZString.Empty, message1.DeclarantCustomsRegNo);

		var declarant = Factory.New<OrgHeader>();
		declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

		_ = declarant.WithOrgCusCode(OrgCusCode.CodeTypes.OrganizationNumber, "1122334455");
		var message2 = GetTestMessage();
		AssertEquals("DeclarantCustomsRegNo, when Declarant is company without MVA account", "1122334455", message2.DeclarantCustomsRegNo);

		_ = declarant.AsMVARegistered();
		var message3 = GetTestMessage();
		AssertEquals("DeclarantCustomsRegNo, when Declarant is company with MVA account", "123456789", message3.DeclarantCustomsRegNo);
	});

	public void TestPaymentMethod() => CombineAssertions(() =>
	{
		entryHeader.CH_PaymentMethod = "K";
		var message1 = GetTestMessage();
		AssertEquals("PaymentMethod, when CH_PaymentMethod is 'K'", "K", message1.PaymentMethod);

		entryHeader.CH_PaymentMethod = "N";
		var message2 = GetTestMessage();
		AssertEquals("PaymentMethod, when CH_PaymentMethod is 'N'", "N", message2.PaymentMethod);
	});

	public void TestCustomsControllingUnit() => CombineAssertions(() =>
	{
		entryHeader.CH_ToCustomsControllingUnit = "Elsewhere";
		var message1 = GetTestMessage();
		AssertEquals("CustomsControllingUnit, when EntryHeader has ToCustomsControllingUnit", "Elsewhere", message1.CustomsControllingUnit);

		var message2 = GetTestMessage(x => x.CustomsOffice = "Somewhere");
		AssertEquals("CustomsControllingUnit, when MessageSendingObject has CustomsOffice", "Somewhere", message2.CustomsControllingUnit);
	});

	public void TestInitialsOfDeclarant()
	{
		var broker = CreateBroker("ABC");
		declaration.JE_GS_NKCusAgent = broker.GS_Code;
		var message1 = GetTestMessage();
		AssertEquals("InitialsOfDeclarant, when Broker is 'ABC'", "ABC", message1.InitialsOfDeclarant);
	}

	GlbStaff CreateBroker(ZString staffCode)
	{
		var broker = Factory.New<GlbStaff>();
		broker.GS_Code = staffCode;
		broker.GS_FullName = "Frederika Nerkus";
		broker.GS_WorkPhone = "02 5555 9999";
		broker.GS_EmailAddress = "sample@wisetechglobal.com";
		broker.GS_Title = "Headkicker";
		broker.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Norway;

		return broker;
	}

	public void TestIncoTerm()
	{
		invoiceHeader.JZ_IncoTerm = "CIF";
		var message = GetTestMessage();
		AssertEquals("IncoTerm", "CIF", message.IncoTerm);
	}

	public void TestIncoTermPlace()
	{
		invoiceHeader.JZ_IncoTermPlace = "OSLO";
		var message = GetTestMessage();
		AssertEquals("IncoTermPlace", "OSLO", message.IncoTermPlace);
	}

	public void TestCommercialInvoiceAmount() => CombineAssertions(() =>
	{
		invoiceHeader.JZ_InvoiceAmount = 100m;
		invoiceLine.JI_LinePrice = 20m;
		var message1 = GetTestMessage();
		AssertEquals("CommercialInvoiceAmount, when single InvoiceHeader with single InvoiceLine", "100", message1.CommercialInvoiceAmount);

		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine2.JI_LinePrice = 30.00m;
		invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
		var message2 = GetTestMessage();
		AssertEquals("CommercialInvoiceAmount, when single InvoiceHeader with multiple InvoiceLines", "50", message2.CommercialInvoiceAmount);

		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine3 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine3.JI_LinePrice = 150.50m;
		invoiceLine3.JI_CL = entryHeader.MergedLines.AddNew().PK;
		var message3 = GetTestMessage();
		AssertEquals("CommercialInvoiceAmount, when multiple InvoiceHeader with same Currency", "200,5", message3.CommercialInvoiceAmount);

		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.08m);
		invoiceHeader.JZ_RX_NKInvoice_Currency = "NOK";
		invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
		var message4 = GetTestMessage();
		AssertEquals("CommercialInvoiceAmount, when multiple InvoiceHeader with different Currency", "1931,25", message4.CommercialInvoiceAmount);
	});

	public void TestCommercialInvoiceCurrency() => CombineAssertions(() =>
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		var message1 = GetTestMessage();
		AssertEquals("CommercialInvoiceCurrency, with one EUR invoice", "EUR", message1.CommercialInvoiceCurrency);

		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		invoiceHeader2.JZ_RX_NKInvoice_Currency = "EUR";
		invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
		var message2 = GetTestMessage();
		AssertEquals("CommercialInvoiceCurrency, with two EUR invoices", "EUR", message2.CommercialInvoiceCurrency);

		invoiceHeader2.JZ_RX_NKInvoice_Currency = "SEK";
		var message3 = GetTestMessage();
		AssertEquals("CommercialInvoiceCurrency, with one EUR invoice and one SEK invoice", "NOK", message3.CommercialInvoiceCurrency);
	});

	public void TestFreightAmountNOK() => CombineAssertions(() =>
	{
		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.08m);
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		_ = invoiceHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 10m, Core.Constants.CurrencyCodes.UnitedStates);
		invoiceLine.JI_LinePrice = 1000m;
		Factory.Save();
		AssertEquals("[PRE-CONDITION] JI_Calc_FreightInLocalCurrency NOK, invoice 1", 125m, invoiceLine.JI_Calc_FreightInLocalCurrency);

		var message1 = GetTestMessage();
		AssertEquals("FreightAmountNOK, when one invoice", "125", message1.FreightAmountNOK);

		var invoiceHeader2 = declaration.Invoices.AddNew();
		var invoiceLine2 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
		invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		_ = invoiceHeader2.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 30m, Core.Constants.CurrencyCodes.UnitedStates);
		invoiceLine2.JI_LinePrice = 1000m;
		invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
		Factory.Save();
		AssertEquals("[PRE-CONDITION] JI_Calc_FreightInLocalCurrency NOK, invoice 2", 375m, invoiceLine2.JI_Calc_FreightInLocalCurrency);

		var message2 = GetTestMessage();
		AssertEquals("FreightAmountNOK, when two invoices", "500", message2.FreightAmountNOK);

		var invoiceHeader3 = declaration.Invoices.AddNew();
		var invoiceLine3 = (JobComInvoiceLine)invoiceHeader3.InvoiceLines.AddNew();
		invoiceHeader3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
		_ = invoiceHeader3.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 150.5m, Core.Constants.CurrencyCodes.UnitedStates);
		invoiceLine3.JI_LinePrice = 1000m;
		invoiceLine3.JI_CL = entryHeader.MergedLines.AddNew().PK;
		Factory.Save();
		AssertEquals("[PRE-CONDITION] JI_Calc_FreightInLocalCurrency NOK, invoice 3", 1881.25m, invoiceLine3.JI_Calc_FreightInLocalCurrency);

		var message3 = GetTestMessage();
		AssertEquals("FreightAmountNOK, when three invoices", "2381,25", message3.FreightAmountNOK);
	});

	public void TestCurrencyExchangeRate() => CombineAssertions(() =>
	{
		invoiceHeader.JZ_RX_NKInvoice_Currency = "NOK";
		var message1 = GetTestMessage();
		AssertEquals("CurrencyExchangeRate, when one NOK invoice", "100", message1.CurrencyExchangeRate);

		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 0.08m);
		invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
		Factory.Save();
		var message2 = GetTestMessage();
		AssertEquals("CurrencyExchangeRate, when one USD invoice", "0,08", message2.CurrencyExchangeRate);

		CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.Sweden, 1.041111111m);
		invoiceHeader.JZ_RX_NKInvoice_Currency = "SEK";
		var message3 = GetTestMessage();
		AssertEquals("CurrencyExchangeRate, when exchange rate is too precise (TVINN type N8)", "104,111", message3.CurrencyExchangeRate);

		var invoiceHeader2 = declaration.Invoices.AddNew();
		invoiceHeader2.JZ_RX_NKInvoice_Currency = "USD";
		var invoiceLine2 = (JobComInvoiceLine)invoiceHeader2.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryHeader.MergedLines.AddNew().PK;
		Factory.Save();
		var message4 = GetTestMessage();
		AssertEquals("CurrencyExchangeRate, when one USD invoice and one SEK invoice", "100", message4.CurrencyExchangeRate);
	});

	public void TestExchangeRateHasNoTrailingZeroes() => CombineAssertions(() =>
	{
		AssertExchangeRateIsTrimmed(Core.Constants.CurrencyCodes.UnitedStates, "13,456", 13.456789m);
		AssertExchangeRateIsTrimmed(Core.Constants.CurrencyCodes.EuropeanUnion, "12,123", 12.123456789m);
		AssertExchangeRateIsTrimmed(Core.Constants.CurrencyCodes.Sweden, "145,678", 1.456789m);
		AssertExchangeRateIsTrimmed(Core.Constants.CurrencyCodes.Japan, "7,234", 0.07234m);
		AssertExchangeRateIsTrimmed(Core.Constants.CurrencyCodes.Denmark, "140", 1.40000004m);
		AssertExchangeRateIsTrimmed(Core.Constants.CurrencyCodes.Australia, "7,2", 7.200001m);

		void AssertExchangeRateIsTrimmed(ZString currency, ZString expectedExchangeRate, ZDecimal actualExchangeRate)
		{
			CurrencyConverterTestHelper.SetExchangeRate(Factory, currency, actualExchangeRate);
			invoiceHeader.JZ_RX_NKInvoice_Currency = currency;
			Factory.Save();
			var wrapper = GetTestMessage();
			AssertEquals($"Currency: {currency}", expectedExchangeRate, wrapper.CurrencyExchangeRate);
		}
	});

	public void TestDocumentMessageSummaryCollection_Type() => CombineAssertions(() =>
	{
		var message = GetTestMessage();
		var collection = message.DocumentMessageSummaryCollection;
		AssertType<ImmutableArray<CUSDECDocumentMessageSummaryWrapper>>(collection);
		AssertSame(collection, message.DocumentMessageSummaryCollection);
	});

	public void TestDocumentMessageSummaryCollection_Count() => CombineAssertions(() =>
	{
		AssertCount("When message has one invoice header", 1);

		var invoiceHeader2 = declaration.Invoices.AddNew();
		AssertCount("When message has two invoice headers but only one linked to entry header", 1);

		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		AssertCount("When message has two invoice headers and two invoice lines but only one linked to entry header", 1);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		AssertCount("When message has two invoice headers", 2);

		var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
		var entryLine3 = entryHeader.MergedLines.AddNew();
		invoiceLine3.JI_CL = entryLine3.PK;
		AssertCount("When message has two invoice headers and three invoice lines", 2);

		void AssertCount(string message, int expected)
		{
			var collection = GetTestMessage().DocumentMessageSummaryCollection;
			AssertEquals(message, expected, collection.Count);
		}
	});

	public void TestDocumentMessageSummaryCollection_Values() => CombineAssertions(() =>
	{
		invoiceHeader.JZ_InvoiceNumber = "69";
		var invoiceHeader2 = declaration.Invoices.AddNew();
		invoiceHeader2.JZ_InvoiceNumber = "420";
		var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		var collection = GetTestMessage().DocumentMessageSummaryCollection;
		AssertEquals("[PRE-CONDITION] summary count", expected: 2, collection.Count);

		var summary1 = collection.ElementAt(0);
		var summary2 = collection.ElementAt(1);
		AssertType<CUSDECDocumentMessageSummaryWrapper>("summary 1", summary1);
		AssertType<CUSDECDocumentMessageSummaryWrapper>("summary 2", summary2);
		AssertEquals("summary 1 invoice number", "69", summary1.InvoiceNumber);
		AssertEquals("summary 2 invoice number", "420", summary2.InvoiceNumber);
	});

	public void TestTaxAndFeeTotals() => CombineAssertions(() =>
	{
		_ = entryLine.Fees.AddNew("GA100", 1m);
		_ = entryLine.Fees.AddNew("GA200", 2m);
		_ = entryLine.Fees.AddNew("MV1", 4m);

		var message = GetTestMessage();
		var feeLines = message.TotalFeeCollection;

		AssertEquals("Types of fees count", expected: 2, feeLines.Count);
		var fee1 = feeLines.ElementAt(0);
		var fee2 = feeLines.ElementAt(1);
		AssertEquals("Fee GA code", "GA", fee1.FeeCode);
		AssertEquals("Fee GA total", 3m, fee1.Amount);
		AssertEquals("Fee MV code", "MV", fee2.FeeCode);
		AssertEquals("Fee MV total", 4m, fee2.Amount);

		AssertEquals("Total Fee amount", "7", message.TotalFeeAmount);
	});

	public void TestTaxAndFeeTotals_WhenLandedCosting() => CombineAssertions(() =>
	{
		_ = entryLine.Fees.AddNew("GA100", 1m);
		_ = entryLine.Fees.AddNew("GA200", 2m);
		var mvaFee = entryLine.Fees.AddNew("MV1", 4m);
		mvaFee.CF_IsLandedCostOnly = true;

		var message = GetTestMessage();
		var feeLines = message.TotalFeeCollection;

		AssertEquals("Types of fees count", expected: 1, feeLines.Count);
		var fee1 = feeLines.ElementAt(0);
		AssertEquals("Fee GA code", "GA", fee1.FeeCode);
		AssertEquals("Fee GA total", 3m, fee1.Amount);
		AssertEquals("Total Fee amount", "3", message.TotalFeeAmount);
	});

	public void TestControlTotals() => CombineAssertions(() =>
	{
		entryInstruction.CEI_PackageCount = 73;
		var details = GetTestMessage();
		AssertEquals("CNT - No of lines", "1", details.TotalNoOfItemLines);
		AssertEquals("CNT - No of Packages", "73", details.TotalNoOfPackages);
	});

	CUSDECMessageDataProviderWrapper GetTestMessage(Action<MessageSendingObject> setup = null)
	{
		var msgSendingObj = new MessageSendingObject(entryHeader);
		setup?.Invoke(msgSendingObj);
		return new(msgSendingObj);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;
	CusEntryInstruction entryInstruction;

	void CreateAndAssignImporterToDeclaration(JobDeclaration jobDeclaration)
	{
		var importer = CreateOrgHeaderWithMainAddress(fullName: "Mister Importer",
			address1: "Some Street",
			address2: "Nr. 1",
			postCode: "42",
			city: "Wiesbaden",
			country: "DE");
		jobDeclaration.JE_OH_Importer = importer.PK;
	}

	void CreateAndAssignConsignorToDeclaration(JobDeclaration jobDeclaration)
	{
		var sender = CreateOrgHeaderWithMainAddress(fullName: "Mister Anderson",
			address1: "Add1",
			address2: "Add2",
			postCode: "42",
			city: "Zion",
			country: "ZZ");
		declaration.JE_OH_Supplier = sender.PK;
	}

	OrgHeader CreateOrgHeaderWithMainAddress(string fullName,
		string address1,
		string address2,
		string postCode,
		string city,
		string country)
	{
		return OrgHeaderTestDataHelper.CreateOrgHeaderWithMainAddress(Factory, fullName, address1, address2, postCode, city, country);
	}
}
