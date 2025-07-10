using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.DataTransfer.Universal.Testing
{
	[TestedType(typeof(CustomsEntryHeaderDataObjectWriter))]
	sealed class CustomsEntryHeaderDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[ExpectNoExceptions]
		public void TestPopulateAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Taiwan;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			var charges = invoice.Charges;
			_ = charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 18898.00m, Core.Constants.CurrencyCodes.Taiwan);
			_ = charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 250.80m, Core.Constants.CurrencyCodes.Taiwan);
			_ = charges.AddNew(Common.CustomsChargeTypeList.Codes.AdditionCharge, 18700.00m, Core.Constants.CurrencyCodes.Taiwan);
			var charge = charges.AddNew(Common.CustomsChargeTypeList.Codes.DeductionCharge, 2839.02, Core.Constants.CurrencyCodes.Taiwan);
			charge.J7_IsIncludedInITOT = true;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 32;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			invoiceLine1.JI_EnteredUnitPrice = 2004.89m;
			invoiceLine1.JI_VatPymntMthd = "CAS";

			declaration.ResumeApportionment();
			_ = declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer(true));

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var charge1 = entryHeader.Charges.AddNew();
			charge1.C1_ChargeAmount = 500m;
			charge1.C1_MethodOfPayment = "DEF";
			var charge2 = entryHeader.Charges.AddNew();
			charge2.C1_ChargeAmount = 100.01m;
			charge2.C1_MethodOfPayment = "CAS";

			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.InvoiceLines.Add(invoiceLine1);
			entryLine1.CL_ValueForVAT = 150.2m;

			var incomingPayResponseNo1 = "ABI31100394446";
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, incomingPayResponseNo1, "A10", 100m, 200m);
			TWXmlTestCaseWithFactory.GenerateCusEntryPayInfo(entryHeader, incomingPayResponseNo1, "D10", 300m);

			var writer = new CustomsEntryHeaderDataObjectWriterForTest(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.Taiwan));
			var entryHeaderData = new EntryHeader();
			writer.PopulateAddInfoForTest(entryHeader, entryHeaderData);
			var collection = entryHeaderData.AddInfoCollection;
			CombineAssertions(() =>
			{
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalEXPDisbursedAmountInInvoiceCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalIMPFOBAmountInInvoiceCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalInternationalFreightAmountInInvoiceCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalInternationalInsuranceAmountInInvoiceCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalAdditionsInInvoiceCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalAdditionsInInvoiceCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalDeductionsInInvoiceCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalDeductionsInInvoiceCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalCustomsValueInInvoiceCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalCustomsValueInInvoiceCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalCustomsValueInLocalCurrency, BaseAddInfo.GetStringRepresentation(entryHeader.CH_TotalCustomsValueInLocalCurrency));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.BusinessTaxBaseAmount, BaseAddInfo.GetStringRepresentation(entryHeader.BusinessTaxBaseAmount));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalCashTaxAmount, BaseAddInfo.GetStringRepresentation(entryHeader.TotalCashTaxAmount));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.TotalNonCashTaxAmount, BaseAddInfo.GetStringRepresentation(entryHeader.TotalNonCashTaxAmount));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.ConfirmedBusinessTaxBaseAmount, BaseAddInfo.GetStringRepresentation(entryHeader.CH_ConfirmedBusinessTaxBase));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.ConfirmedTotalCashTaxAmount, BaseAddInfo.GetStringRepresentation(entryHeader.CH_ConfirmedTotalDutyTaxFee));
				AssertAddInfoCollectionInclude(collection, Constants.AddInfoKeys.CusEntryHeader.ConfirmedTotalNonCashTaxAmount, BaseAddInfo.GetStringRepresentation(entryHeader.CH_ConfirmedTotalDutyTaxFeeDeferred));
			});
		}

		[ExpectNoExceptions]
		void AssertAddInfoCollectionInclude(List<AddInfo> collection, string key, string value)
		{
			NUnit.Framework.Assert.That(collection.Any(x => x.Key.GetValueOrDefault() == key && x.Value.GetValueOrDefault() == value), Is.True, $"AddInfoCollection should include key:{key}, value:{value}");
		}

		[ExpectNoExceptions]
		public void TestGetNewCustomsEntryNumberDataObjectWriter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			SetupEntryPayInfo(entryHeader.EntryPayInfos.AddNew(), new ZDateTime(2021, 10, 29), new ZDate(2021, 10, 30), "A10", "BRK", "PEN", true, false, 101m, "Reference1234", "DAI11101354198");
			SetupEntryPayInfo(entryHeader.EntryPayInfos.AddNew(), new ZDateTime(2021, 11, 29), new ZDate(2021, 11, 30), "A20", "CLI", "CLR", false, true, 202m, "Reference5678", "BCD22212465209");

			var entryNumberBO = SetupCusEntryNumber(Factory.BOFactory);
			entryNumberBO.Parent = entryHeader;
			var writer = new CustomsEntryHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryHeader)), new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.Taiwan));
			var entryHeaderData = writer.GetDataObject(entryHeader);
			var entryNumberDataObject = entryHeaderData.EntryNumberCollection.Cast<UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber>().First();
			AssertContents(entryNumberDataObject);

			var entryHeaderPaymentInformations = entryHeaderData.PaymentInformationCollection.Cast<EntryHeaderPaymentInformation>();
			NUnit.Framework.Assert.That(entryHeaderPaymentInformations.Count(), Is.EqualTo(2));
			AssertCustomsEntryHeaderPaymentInformationData(entryHeaderPaymentInformations.ElementAt(0), new ZDateTime(2021, 10, 29), new ZDateTime(2021, 10, 30), "A10", "進口稅", "BRK", "Broker", "PEN", "Pending", true, false, 101m, "Reference1234", "DAI11101354198");
			AssertCustomsEntryHeaderPaymentInformationData(entryHeaderPaymentInformations.ElementAt(1), new ZDateTime(2021, 11, 29), new ZDateTime(2021, 11, 30), "A20", "平衡稅", "CLI", "Client", "CLR", "Clear", false, true, 202m, "Reference5678", "BCD22212465209");
		}

		void SetupEntryPayInfo(Business.CusEntryPayInfo entryPayInfo, ZDateTime paymentDate, ZDate receiptDate, ZString transactionType, ZString paymentParty, ZString paymentStatus, ZBool cusResReceived, ZBool remAdvReceived, ZDecimal paymentAmount, ZString paymentReference, ZString incomingPayResponseNo)
		{
			entryPayInfo.C9_PaymentDate = paymentDate;
			entryPayInfo.C9_ReceiptDate = receiptDate;
			entryPayInfo.C9_TransactionType = transactionType;
			entryPayInfo.C9_PaymentParty = paymentParty;
			entryPayInfo.C9_PaymentStatus = paymentStatus;
			entryPayInfo.C9_CusResReceived = cusResReceived;
			entryPayInfo.C9_RemAdvReceived = remAdvReceived;
			entryPayInfo.C9_PaymentAmount = paymentAmount;
			entryPayInfo.C9_PaymentReference = paymentReference;
			entryPayInfo.C9_IncomingPayResponseNo = incomingPayResponseNo;
		}

		[ExpectNoExceptions]
		public void AssertCustomsEntryHeaderPaymentInformationData(EntryHeaderPaymentInformation entryHeaderPaymentInformationDataObject, ZDateTime expectedPaymentDate, ZDateTime expectedEntryReleaseDate, ZString expectedTransactionTypeCode, ZString expectedTransactionTypeDescription, ZString expectedPaymentPartyCode, ZString expectedPaymentPartyDescription, ZString expectedPaymentStatusCode, ZString expectedPaymentStatusDescription, ZBool expectedCusResReceived, ZBool expectedRemAdvReceived, ZDecimal expectedPaymentAmount, ZString expectedPaymentReference, ZString expectedIncomingPayResponseNo)
		{
			NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject, Is.Not.EqualTo(default(EntryHeaderPaymentInformation)), "Precondition: entryHeaderPaymentInformationDataObject - should not be [null]");
			CombineAssertions(delegate
			{
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.PaymentDate, Is.EqualTo(expectedPaymentDate), "entryHeaderPaymentInformationDataObject.PaymentDate");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.EntryReleaseDate, Is.EqualTo(expectedEntryReleaseDate), "entryHeaderPaymentInformationDataObject.EntryReleaseDate");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.TransactionType.Code, Is.EqualTo(expectedTransactionTypeCode), "entryHeaderPaymentInformationDataObject.TransactionType.Code");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.TransactionType.Description, Is.EqualTo(expectedTransactionTypeDescription), "entryHeaderPaymentInformationDataObject.TransactionType.Description");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.PaymentParty.Code, Is.EqualTo(expectedPaymentPartyCode), "entryHeaderPaymentInformationDataObject.PaymentParty.Code");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.PaymentParty.Description, Is.EqualTo(expectedPaymentPartyDescription), "entryHeaderPaymentInformationDataObject.PaymentParty.Description");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.PaymentStatus.Code, Is.EqualTo(expectedPaymentStatusCode), "entryHeaderPaymentInformationDataObject.PaymentStatus.Code");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.PaymentStatus.Description, Is.EqualTo(expectedPaymentStatusDescription), "entryHeaderPaymentInformationDataObject.PaymentStatus.Description");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.CustomsResponseReceived, Is.EqualTo(expectedCusResReceived), "entryHeaderPaymentInformationDataObject.CustomsResponseReceived");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.RemittanceAdviceReceived, Is.EqualTo(expectedRemAdvReceived), "entryHeaderPaymentInformationDataObject.RemittanceAdviceReceived");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.PaymentAmount, Is.EqualTo(expectedPaymentAmount), "entryHeaderPaymentInformationDataObject.PaymentAmount");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.PaymentReference, Is.EqualTo(expectedPaymentReference), "entryHeaderPaymentInformationDataObject.PaymentReference");
				NUnit.Framework.Assert.That(entryHeaderPaymentInformationDataObject.IncomingPaymentResponseNumber, Is.EqualTo(expectedIncomingPayResponseNo), "entryHeaderPaymentInformationDataObject.IncomingPaymentResponseNumber");
			});
		}

		CusEntryNumber SetupCusEntryNumber(BusinessObjectFactory factory, ZString entryNum, ZString entryType, ZBool entryIsSystemGenerated)
		{
			return SetupCusEntryNumber(factory.New<CusEntryNumber>(), entryNum, entryType, entryIsSystemGenerated, Core.Constants.CountryCodes.Taiwan, new ZDateTime(2021, 8, 9), "C1");
		}

		CusEntryNumber SetupCusEntryNumber(CusEntryNumber cusEntryNumber, ZString entryNum, ZString entryType, ZBool entryIsSystemGenerated, ZString countryCode, ZDateTime issueDate, ZString entryStatus)
		{
			cusEntryNumber.CE_EntryNum = entryNum;
			cusEntryNumber.CE_EntryType = entryType;
			cusEntryNumber.CE_EntryIsSystemGenerated = entryIsSystemGenerated;
			cusEntryNumber.CE_RN_NKCountryCode = countryCode;
			cusEntryNumber.CE_IssueDate = issueDate;
			cusEntryNumber.CE_EntryStatus = entryStatus;
			return cusEntryNumber;
		}

		CusEntryNumber SetupCusEntryNumber(BusinessObjectFactory factory)
		{
			return SetupCusEntryNumber(factory, "CE00001", Common.Shared.SharedJobMessageTypeList.Codes.Import, false);
		}

		[ExpectNoExceptions]
		void AssertContents(UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber entryNumberDataObject, ZBool entryIsSystemGenerated, ZString number, ICodeDescription type, ZString entryStatus, ZString entryStatusDescription, ZDateTime issueDate)
		{
			NUnit.Framework.Assert.That(entryNumberDataObject, Is.Not.EqualTo(default(UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber)), "Precondition: entryNumberDataObject - should not be [null]");
			CombineAssertions(delegate
			{
				NUnit.Framework.Assert.That(entryNumberDataObject.EntryIsSystemGenerated, Is.EqualTo(entryIsSystemGenerated), "entryNumberDataObject.EntryIsSystemGenerated");
				NUnit.Framework.Assert.That(entryNumberDataObject.Number, Is.EqualTo(number), "entryNumberDataObject.Number");
				NUnit.Framework.Assert.That(entryNumberDataObject.Type, Is.Not.EqualTo(default(EntryType)), "entryNumberDataObject.Type - should not be [null]");
				NUnit.Framework.Assert.That(entryNumberDataObject.Type.Code, Is.EqualTo(type.Code).Using(CustomComparers.TypeComparison), "entryNumberDataObject.Type.Code");
				NUnit.Framework.Assert.That(entryNumberDataObject.Type.Description, Is.EqualTo(type.Description).Using(CustomComparers.TypeComparison), "entryNumberDataObject.Type.Description");
				NUnit.Framework.Assert.That(entryNumberDataObject.EntryStatus.Code, Is.EqualTo(entryStatus), "entryNumberDataObject.EntryStatus");
				NUnit.Framework.Assert.That(entryNumberDataObject.EntryStatus.Description, Is.EqualTo(entryStatusDescription), "entryNumberDataObject.EntryStatus Description");
				NUnit.Framework.Assert.That(entryNumberDataObject.IssueDate, Is.EqualTo(issueDate), "entryNumberDataObject.IssueDate");
			}

			);
		}

		[ExpectNoExceptions]
		void AssertContents(UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber entryNumberDataObject)
		{
			AssertContents(entryNumberDataObject, false, "CE00001", CodeDescriptionPairForTesting.New(Common.Shared.SharedJobMessageTypeList.Codes.Import, Common.Shared.SharedJobMessageTypeList.Descriptions.Import, 0), "C1", "已放行 (C1 免審免驗通關)", new ZDateTime(2021, 8, 9));
		}

		class CustomsEntryHeaderDataObjectWriterForTest : CustomsEntryHeaderDataObjectWriter
		{
			public CustomsEntryHeaderDataObjectWriterForTest(IDataWritingManager manager, UniversalDataObjectWriterHelper helper) : base(manager, helper)
			{
			}

			public void PopulateAddInfoForTest(Business.CusEntryHeader entryHeaderBO, EntryHeader entryHeaderData) => PopulateAddInfo(entryHeaderBO, entryHeaderData);
		}
	}
}
