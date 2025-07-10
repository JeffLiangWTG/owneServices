using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class GoodsShipmentAbstractTests<TGoodsShipment, TMessageSendingObject> : TestCaseWithFactory
		where TGoodsShipment : GoodsShipment
		where TMessageSendingObject : AdditionalDocumentMessageSendingObject
	{
		[ExpectNoExceptions]
		public virtual void TestGovernmentAgencyGoodsItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CreateEntryLineWithInvoiceLine(entryHeader, 3, invoiceHeader, "22");
			CreateEntryLineWithInvoiceLine(entryHeader, 2, invoiceHeader, "11");
			CreateEntryLineWithInvoiceLine(entryHeader, 1, invoiceHeader, "11");
			CreateEntryLineWithInvoiceLine(entryHeader, 4, invoiceHeader, "NO.008 ");
			var goodsShipment = GetGoodsShipment(entryHeader, null, null);
			var goodsItems = goodsShipment.GovernmentAgencyGoodsItems.Cast<IGovernmentAgencyGoodsItem>().ToList();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItems.Count, NUnit.Framework.Is.EqualTo(4));
				for (int i = 0; i < goodsItems.Count; ++i)
				{
					NUnit.Framework.Assert.That((goodsItems[i] as GovernmentAgencyGoodsItem).EntryLine.CL_LineNumber, NUnit.Framework.Is.EqualTo(i + 1).Using(CustomComparers.TypeComparison));
				}

				NUnit.Framework.Assert.That(goodsItems[0].Commodity.Description, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[1].Commodity.Description, NUnit.Framework.Is.EqualTo("11").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[2].Commodity.Description, NUnit.Framework.Is.EqualTo("22").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(goodsItems[3].Commodity.Description, NUnit.Framework.Is.EqualTo("NO.008 ").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalDocuments()
		{
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IAdditionalDocument>)));
		}

		[ExpectNoExceptions]
		public void TestExitDateTime()
		{
			NUnit.Framework.Assert.That(goodsShipment.ExitDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
		}

		[TestDate(2021, 01, 02)]
		[ExpectNoExceptions]
		public virtual void TestItemChargeAmount()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			CurrencyConverterTestHelper.SetExchangeRate(Factory, Core.Constants.CurrencyCodes.UnitedStates, 30.13m, new ZDateTime(2021, 01, 02), Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.JZ_InvoiceAmount = 345241.02m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;

			var charges = invoice.Charges;
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 18898.00m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasInsurance, 250.80m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.AdditionCharge, 18700.00m, Core.Constants.CurrencyCodes.UnitedStates);
			charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.DeductionCharge, 2839.02, Core.Constants.CurrencyCodes.UnitedStates);

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 32;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 2004.89m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 24;
			invoiceLine2.JI_InvoiceUQ = "PCE";
			invoiceLine2.JI_EnteredUnitPrice = 2426.68m;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 16;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			invoiceLine3.JI_EnteredUnitPrice = 3605.33m;

			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_InvoiceQuantity = 3;
			invoiceLine4.JI_InvoiceUQ = "PCE";
			invoiceLine4.JI_EnteredUnitPrice = 7395.55m;

			var invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_InvoiceQuantity = 3;
			invoiceLine5.JI_InvoiceUQ = "PCE";
			invoiceLine5.JI_EnteredUnitPrice = 15282.23m;

			var invoiceLine6 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_InvoiceQuantity = 6;
			invoiceLine6.JI_InvoiceUQ = "PCE";
			invoiceLine6.JI_EnteredUnitPrice = 2205.37m;

			var invoiceLine7 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_InvoiceQuantity = 6;
			invoiceLine7.JI_InvoiceUQ = "PCE";
			invoiceLine7.JI_EnteredUnitPrice = 13982.23m;

			declaration.ResumeApportionment();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var goodsShipment = GetGoodsShipment(entryHeader, null, null);
			NUnit.Framework.Assert.That(goodsShipment.ItemChargeAmount, NUnit.Framework.Is.EqualTo(10394555m).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTotalCIFAmount()
		{
			NUnit.Framework.Assert.That(goodsShipment.TotalCIFAmount, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));
		}

		[ExpectNoExceptions]
		public void TestConsignee()
		{
			entryHeader.EntryInstruction.CEI_OA_Warehouse2 = testHelper.CreateOrganizationForWarehouse2().MainAddress.PK;
			var consigneeForTest = testHelper.CreateOrganizationForConsignee();
			entryHeader.Declaration.ConsigneeDocumentaryAddress.OrganisationPK = consigneeForTest.PK;
			var consignee = goodsShipment.Consignee;
			NUnit.Framework.Assert.That(consignee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
			NUnit.Framework.Assert.That(consignee, NUnit.Framework.Is.TypeOf<Consignee>());
			consigneeForTest.OH_RL_NKClosestPort = "USLAX";
			NUnit.Framework.Assert.That(goodsShipment.Consignee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestConsignment()
		{
			NUnit.Framework.Assert.That(goodsShipment.Consignment.GetType(), NUnit.Framework.Is.EqualTo(typeof(Consignment)));
		}

		public void TestConsignor()
		{
			entryHeader.Declaration.ConsignorDocumentaryAddress.OrganisationPK = testHelper.CreateOrganizationForConsignor().PK;
			entryHeader.EntryInstruction.CEI_OA_Warehouse = testHelper.CreateOrganizationForWarehouse().MainAddress.PK;
			NUnit.Framework.Assert.That(goodsShipment.Consignor.GetType(), NUnit.Framework.Is.EqualTo(typeof(Consignor)));
			AssertNoExceptionThrown(() => GetConsignorName());
		}

		[ExpectNoExceptions]
		public void TestCustomsValuation()
		{
			NUnit.Framework.Assert.That(goodsShipment.CustomsValuation.GetType(), NUnit.Framework.Is.EqualTo(typeof(CustomsValuation)));
		}

		[ExpectNoExceptions]
		public void TestDeliveryDestinationName()
		{
			NUnit.Framework.Assert.That(goodsShipment.DeliveryDestinationName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsShipment.CustomsValuation, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.ICustomsValuation)));
		}

		[ExpectNoExceptions]
		public void TestDutyTaxFees()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			var line = header.MergedLines.AddNew();
			var invoiceLine = line.InvoiceLines.AddNew() as JobComInvoiceLine;
			var charge = header.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			charge.C1_ChargeAmount = 66M;
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			charge.C1_ChargeAmount = 77M;
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			charge.C1_ChargeAmount = 88M;
			var goodsShipment = GetGoodsShipment(header, null, null);
			NUnit.Framework.Assert.That(goodsShipment.DutyTaxFees.Count(), NUnit.Framework.Is.EqualTo(2));
			var dutyTaxFee = goodsShipment.DutyTaxFees.SingleOrDefault(x => x.AdValoremTaxBaseAmount == 143M);
			NUnit.Framework.Assert.That(dutyTaxFee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IGoodsShipmentDutyTaxFee)));
			NUnit.Framework.Assert.That(dutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A19").Using(CustomComparers.TypeComparison));
			dutyTaxFee = goodsShipment.DutyTaxFees.SingleOrDefault(x => x.AdValoremTaxBaseAmount == 88M);
			NUnit.Framework.Assert.That(dutyTaxFee, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IGoodsShipmentDutyTaxFee)));
			NUnit.Framework.Assert.That(dutyTaxFee.TypeCode, NUnit.Framework.Is.EqualTo("A10").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestNotifyParty()
		{
			entryHeader.Declaration.JE_OH_NotifyParty = testHelper.CreateOrganizationForNotifyParty().PK;
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty.GetType(), NUnit.Framework.Is.EqualTo(typeof(NotifyParty)));
			NUnit.Framework.Assert.That(goodsShipment.NotifyParty, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestSeller()
		{
			NUnit.Framework.Assert.That(goodsShipment.Seller, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestTradeTermsConditionCode()
		{
			entryHeader.CH_DeclarationIncoterm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode, NUnit.Framework.Is.EqualTo("CIF").Using(CustomComparers.TypeComparison));
			entryHeader.CH_DeclarationIncoterm = Core.Constants.IncoTerms.FreeOnBoard;
			NUnit.Framework.Assert.That(goodsShipment.TradeTermsConditionCode, NUnit.Framework.Is.EqualTo("FOB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUCR()
		{
			entryHeader.EntryInstruction.UCRNumber = "KK1";
			NUnit.Framework.Assert.That(goodsShipment.UCR, NUnit.Framework.Is.EqualTo("KK1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBuyer()
		{
			var declaration = entryHeader.Declaration;
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = true;
			var buyer = goodsShipment.Buyer;
			NUnit.Framework.Assert.That(buyer, NUnit.Framework.Is.TypeOf(typeof(Buyer)));
			NUnit.Framework.Assert.That(buyer, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
			declaration.ImporterDocumentaryAddress.E2_AddressOverride = false;
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
			declaration.JE_OH_Importer = testHelper.CreateOrganizationForImporter().PK;
			NUnit.Framework.Assert.That(goodsShipment.Buyer, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestExporter()
		{
			entryHeader.Declaration.JE_OH_Supplier = testHelper.CreateOrganizationForSupplier().PK;
			NUnit.Framework.Assert.That(goodsShipment.Exporter.GetType(), NUnit.Framework.Is.EqualTo(typeof(Exporter)));
			NUnit.Framework.Assert.That(goodsShipment.Exporter, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IPartyDetails)));
		}

		[ExpectNoExceptions]
		public void TestGoodsMeasures()
		{
			NUnit.Framework.Assert.That(goodsShipment.GoodsMeasures, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IGoodsMeasure>)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			NUnit.Framework.Assert.That(goodsShipment.AdditionalInformations, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IAdditionalInformation>)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestGoodsShipment_AdditionalDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var doc1 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\sample.pdf"), Core.Constants.FileFormats.PDF);
			var doc2 = ((IDocManagerSupport)declaration).DocManagerInfo.AddFileOrDocument(System.IO.Path.Combine(BaseSourcePath, $@"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Test.xls"), Core.Constants.FileFormats.XLS);
			var messageSendingObject = GetMessageSendingObject(entryHeader);
			var docLine1 = messageSendingObject.SupportingDocuments.AddNew();
			docLine1.EDoc = doc1.UniqueKey;
			var docLine2 = messageSendingObject.SupportingDocuments.AddNew();
			docLine2.EDoc = doc2.UniqueKey;
			var goodsShipment = GetGoodsShipment(entryHeader, messageSendingObject.SupportingDocuments, messageSendingObject.GetAllEDocs());
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IAdditionalDocument>)));
			NUnit.Framework.Assert.That(goodsShipment.AdditionalDocuments.Count(), NUnit.Framework.Is.EqualTo(2));
		}

		protected abstract TGoodsShipment GetGoodsShipment(CusEntryHeader entryHeader, SupportingDocumentCollection supportingDocuments, IStorageDocsBaseCollection[] allEDocs);

		protected abstract TMessageSendingObject GetMessageSendingObject(CusEntryHeader entryHeader);

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			testHelper.CreateInvoiceLineForN5203(entryHeader);
		}
		TestTWCreator testHelper;
		CusEntryHeader entryHeader;

		IGoodsShipment goodsShipment => GetGoodsShipment(entryHeader, null, null);

		protected CusEntryLine CreateEntryLineWithInvoiceLine(CusEntryHeader entryHeader, ZShort lineNumber, JobComInvoiceHeader invoiceHeader, ZString grouping)
		{
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = lineNumber;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Group = grouping;
			invoiceLine.JI_CL = entryLine.PK;
			return entryLine;
		}

		ZString GetConsignorName()
		{
			entryHeader.Declaration.JE_OH_Exporter = testHelper.CreateOrganizationForConsignor().PK;
			entryHeader.EntryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
			return goodsShipment.Consignor?.Name ?? ZString.Empty;
		}
	}
}
