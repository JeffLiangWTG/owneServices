using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Integration.Customs;
using ICommonInvoice = Enterprise.Customs.Business.ICommonInvoice;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(JobComInvoiceHeader))]
sealed class JobComInvoiceHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
{
	public override string GetLocalCurrencyCode() => Core.Constants.CurrencyCodes.Poland;

	public void TestLookups()
	{
		var header = NewInvoiceHeader;
		header.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		CombineAssertions(() =>
		{
			AssertType<JobComInvoiceHeaderLookups>("Import type", header.Lookups);
			header.JobDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceHeaderLookups>("Export type", header.Lookups);
		});
	}

	public override void TestChargeTypeList()
	{
		ICommonInvoice commonInvoice = invoice;
		var chargeTypeList = commonInvoice.ChargeTypeList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", chargeTypeList, commonInvoice.ChargeTypeList);
			AssertEquals("CodesAsString", "071, 1ST, 2ST, AB, AD, AE, AF, AG, AH, AI, AJ, AK, AL, AN, BA, BB, BC, BD, BE, BF, BG", chargeTypeList.CodesAsString);
		});
	}

	public void TestAdditionalInfosType() => AssertType<AdditionalInfoCollection>(invoice.AdditionalInfos);

	public void TestPreviousDocuments() => AssertType<PreviousDocumentCollection>(invoice.PreviousDocuments);

	public new void TestICusCodeDataTypeSupporter()
	{
		ICusCodeDataTypeSupporter supporter = invoice;

		CombineAssertions(() =>
		{
			AssertEquals("DES", GetExpectedInvoiceHeaderDescriptionType(), supporter.GetCusCodeDataTypes()[EU.Business.CusCodeDataTypeList.Codes.DescriptionCode]);
			AssertEquals("DV1", typeof(TranCircumstance), supporter.GetCusCodeDataTypes()[CusCodeDataTypeList.Codes.DV1]);
			AssertEquals("Count", 2, supporter.GetCusCodeDataTypes().Count);
		});
	}

	public void TestTranCircumstanceCode1_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoice.TranCircumstanceCode1Info);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Transaction Circumstances", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Tran. Circ.", resourceStringData.ShortCaption);
			AssertEquals("MediumCaption", "Tran. Circumstances", resourceStringData.MediumCaption);
		});
	}

	public void TestAdditionalTranCircumstances()
	{
		CombineAssertions(() =>
		{
			AssertEquals("TranCircumstanceCode1", typeof(TranCircumstanceCollection), invoice.AdditionalTranCircumstanceCodes.GetType());
			AssertEquals("IsRegisteredEditableChildObject", true, invoice.IsRegisteredEditableChildObject(invoice.AdditionalTranCircumstanceCodes));
		});
	}

	public void TestAdditionalTranCircumstanceCodesAsString_ReadOnly()
	{
		Assert("AdditionalTranCircumstanceCodesAsString_ReadOnly", Factory.New<JobDeclaration>().Invoices.AddNew().AdditionalTranCircumstanceCodesAsString_ReadOnly);
	}

	public void TestGetNewValidation()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertType<ImportJobComInvoiceHeaderValidation>("Import Validation", invoice.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertType<ExportJobComInvoiceHeaderValidation>("Export Validation", invoice.Validation);

			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<JobComInvoiceHeaderValidation>("Other Validation", invoice.Validation);
		});
	}

	public void TestZG_AgreedPlaceCode_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoice.ZG_AgreedPlaceCodeInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Incoterm Place Code", resourceStringData.Caption);
			AssertEquals("ShortCaption", "Incoterm Place", resourceStringData.ShortCaption);
		});
	}

	public void TestZG_ValuationMethod_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoice.ZG_ValuationMethodInfo);
		AssertEquals("Caption", "Valuation Method", resourceStringData.Caption);
	}

	public void TestZG_TransportChargesMethodOfPayment_Caption()
	{
		var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoice.ZG_TransportChargesMethodOfPaymentInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Transport Charges Method of Payment", resourceStringData.Caption);
			AssertEquals("ShortCaption", "MoP", resourceStringData.ShortCaption);
			AssertEquals("MediumCaption", "Tran. Charges MoP", resourceStringData.MediumCaption);
		});
	}

	public void TestJZ_IncoTerm_ReadOnly() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import JZ_IncoTerm", false, invoice.JZ_IncoTermInfo.ReadOnly);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("ExitSummaryDeclaration JZ_IncoTerm", true, invoice.JZ_IncoTermInfo.ReadOnly);
	});

	public void TestJZ_IncoTermPlace_ReadOnly() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import JZ_IncoTermPlace", false, invoice.JZ_IncoTermPlaceInfo.ReadOnly);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("ExitSummaryDeclaration JZ_IncoTermPlace", true, invoice.JZ_IncoTermPlaceInfo.ReadOnly);
	});

	public void TestZG_AgreedPlaceCodeReadOnly() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import ZG_AgreedPlaceCode", false, invoice.ZG_AgreedPlaceCodeInfo.ReadOnly);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("ExitSummaryDeclaration ZG_AgreedPlaceCode", true, invoice.ZG_AgreedPlaceCodeInfo.ReadOnly);
	});

	public void TestJZ_ValuationCode_ReadOnly() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import JZ_ValuationCode", false, invoice.JZ_ValuationCodeInfo.ReadOnly);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("ExitSummaryDeclaration JZ_ValuationCode", true, invoice.JZ_ValuationCodeInfo.ReadOnly);
	});

	public void TestJZ_NetWeight_ReadOnly() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import JZ_NetWeight", false, invoice.JZ_NetWeightInfo.ReadOnly);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("ExitSummaryDeclaration JZ_NetWeight", true, invoice.JZ_NetWeightInfo.ReadOnly);
	});

	public void TestJZ_NetWeightUQ_ReadOnly() => CombineAssertions(() =>
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertEquals("Import JZ_NetWeightUQ", false, invoice.JZ_NetWeightUQInfo.ReadOnly);

		declaration.JE_MessageType = PLJobMessageTypeList.Codes.ExitSummary;
		AssertEquals("ExitSummaryDeclaration JZ_NetWeightUQ", true, invoice.JZ_NetWeightUQInfo.ReadOnly);
	});

	public void TestSetDefaultSupplierAddress()
	{
		invoice.JZ_OA_SupplierAddress = ZGuid.Empty;
		var supplierOrg = Factory.New<OrgHeader>();
		invoice.SupplierOrgPK = supplierOrg.PK;
		AssertEquals("Supplier Address set from Supplier Org", supplierOrg.MainAddress.PK, invoice.JZ_OA_SupplierAddress);
	}

	public void TestSetDefaultBuyerAddress()
	{
		invoice.JZ_OA_BuyerAddress = ZGuid.Empty;
		var buyerOrg = Factory.New<OrgHeader>();
		invoice.BuyerOrgPK = buyerOrg.PK;
		AssertEquals("Buyer Address set from Buyer Org", buyerOrg.MainAddress.PK, invoice.JZ_OA_BuyerAddress);
	}

	public void TestSetDefaultExporterAddress()
	{
		invoice.JZ_OA_ExporterAddress = ZGuid.Empty;
		var exporterOrg = Factory.New<OrgHeader>();
		invoice.ExporterOrgPK = exporterOrg.PK;
		AssertEquals("Exporter Address set from Exporter Org", exporterOrg.MainAddress.PK, invoice.JZ_OA_ExporterAddress);
	}

	public void TestSetDefaultSellerAddress()
	{
		invoice.JZ_OA_SellerAddress = ZGuid.Empty;
		var sellerOrg = Factory.New<OrgHeader>();
		invoice.SellerOrgPK = sellerOrg.PK;
		AssertEquals("Seller Address set from Seller Org", sellerOrg.MainAddress.PK, invoice.JZ_OA_SellerAddress);
	}

	public void TestSetDefaultConsigneeAddress()
	{
		invoice.JZ_OA_ConsigneeAddress = ZGuid.Empty;
		var consigneeOrg = Factory.New<OrgHeader>();
		invoice.ConsigneeOrgPK = consigneeOrg.PK;
		AssertEquals("Consignee Address set from Consignee Org", consigneeOrg.MainAddress.PK, invoice.JZ_OA_ConsigneeAddress);
	}

	public void TestSetDefaultInvoicerAddress()
	{
		invoice.JZ_OA_InvoicerAddress = ZGuid.Empty;
		var invoicerOrg = Factory.New<OrgHeader>();
		invoice.InvoicerOrgPK = invoicerOrg.PK;
		AssertEquals("Invoicer Address set from Invoicer Org", invoicerOrg.MainAddress.PK, invoice.JZ_OA_InvoicerAddress);
	}

	public void TestSetDefaultManufacturerAddress()
	{
		invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
		var manufacturerOrg = Factory.New<OrgHeader>();
		invoice.ManufacturerOrgPK = manufacturerOrg.PK;
		AssertEquals("Manufacturer Address set from Manufacturer Org", manufacturerOrg.MainAddress.PK, invoice.JZ_OA_ManufacturerAddress);
	}

	protected override Type ExpectedTypeOfGroupCharges => typeof(JobComInvApportionedChargeCollection<InvoiceApportionCharge>);

	protected override Type ExpectedTypeOfCharges => typeof(InvoiceChargeCollection<InvoiceCharge>);

	protected override string OverseasFreightCode => PLCustomsChargeTypeList.Codes.AK;

	protected override CodeDescriptionPairList GetExpectedCustomsChargeTypeList() => new PLCustomsChargeTypeList();

	protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

	protected override BusinessObject GetNewBusinessObject() => invoice;

	protected override IReadOnlyList<(string messageType, string document)> DefaultInvoiceDocuments =>
		new[]
		{
			(MessageTypeList.Codes.Import, EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N935),
			(MessageTypeList.Codes.Export, EU.Business.UniversalReferenceConstants.SupportingDocumentTypes.N380)
		};

	protected override void SetUp()
	{
		base.SetUp();
		declaration = (JobDeclaration)GetNewDeclaration();
		declaration.AutoCreateChargesBasedOnIncoTerm = false;
		invoice = declaration.Invoices.AddNew();
	}

	new JobDeclaration declaration;
	JobComInvoiceHeader invoice;
}
