using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ExportNonCondensedDeclarationDocumentWrapper))]
	sealed class ExportNonCondensedDeclarationDocumentWrapperTest : ExportCustomsDeclarationDocumentWrapperAbstractTest<ExportNonCondensedDeclarationDocumentWrapper>
	{
		[ExpectNoExceptions]
		public override void TestN5203Declaration()
		{
			NUnit.Framework.Assert.That(GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory).N5203Declaration.GetType(), NUnit.Framework.Is.EqualTo(typeof(ExportNonCondensedDeclarationMessageSendingObject)));
		}

		protected override ExportNonCondensedDeclarationDocumentWrapper GetExportCustomsDeclarationDocumentWrapper(CusEntryHeader cusEntryHeader, BusinessObjectFactory factory) => ExportNonCondensedDeclarationDocumentWrapper.New(cusEntryHeader, factory);

		[TestDate(2019, 08, 30)]
		[ExpectNoExceptions]
		public void TestGoodsItemListSections()
		{
			BasicSetupForTestEntryLines();
			var invoiceLine1 = declaration.Invoices.First().InvoiceLines.Cast<JobComInvoiceLine>().First();

			for (short i = 1; i <= 5; i++)
			{
				var addDoc = invoiceLine1.PermitCusSupportingCollection.AddNew();
				addDoc.CSI_ReferenceNumber = "DocA12" + i.ToString();
				addDoc.CSI_LineNo = new ZShort(i);
			}
			var addDocB1 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			addDocB1.JG_ReferenceNumber = "DocB123";
			var addDocB2 = invoiceLine1.AssignedJobComInvLineRefsCollection.AddNew();
			addDocB2.JG_ReferenceNumber = "DocB223";
			invoiceLine1.CertificateOfOriginNumber = "LN:111";
			invoiceLine1.CertificateOfOriginNumberItemNumber = 11;
			invoiceLine1.JI_NetWeight = 950m;

			var invoiceHeader1 = invoiceLine1.InvoiceHeader;
			var charge1 = invoiceHeader1.Charges.AddNew();
			charge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge1.J7_Amount = 10000m;
			charge1.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge1.J7_IsIncludedInITOT = false;
			var charge2 = invoiceHeader1.Charges.AddNew();
			charge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge2.J7_Amount = 20000m;
			charge2.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge2.J7_IsIncludedInITOT = false;
			var charge3 = invoiceHeader1.Charges.AddNew();
			charge3.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge3.J7_Amount = 30000m;
			charge3.J7_Calc_IsIncludedInInvoiceAmount = true;
			charge3.J7_IsIncludedInITOT = true;
			var charge4 = invoiceHeader1.Charges.AddNew();
			charge4.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge4.J7_Amount = 40000m;
			charge4.J7_Calc_IsIncludedInInvoiceAmount = false;
			charge4.J7_IsIncludedInITOT = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer(true));
			Factory.Save();

			var entryLine1 = invoiceLine1.CusEntryLine;
			entryLine1.CL_Description = "Test EntryLine1 Description Line1";

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);

			var sequence = new ZStringBuilder();
			var brand = new ZStringBuilder();
			var detail = new ZStringBuilder();
			var code = new ZStringBuilder();
			var price = new ZStringBuilder();
			var weight = new ZStringBuilder();
			var fob = new ZStringBuilder();
			var statistic = new ZStringBuilder();
			foreach (var section in wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>())
			{
				sequence.Append(section.Box32ItemNumber);
				brand.Append(section.Box33BrandLine1);
				detail.Append(section.Box33DescriptionOfGoods_Line1);
				detail.Append(section.Box33DescriptionOfGoods_Line2);
				detail.Append(section.Box33DescriptionOfGoods_Line3);
				detail.Append(section.Box33DescriptionOfGoods_Line4);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line1);
				code.Append(section.Box34ImportExportPermitNumberAndItemNumber_Line2);
				code.Append(section.Box35CCCCode);
				code.Append(section.Box35BondedGoodsCodeAndAssignedNumber);
				price.Append(section.Box36UnitPrice_Line1);
				price.Append(section.Box36UnitPrice_Line2);
				price.Append(section.Box36UnitPrice_Line3);
				price.Append(section.Box36UnitPrice_Line4);
				weight.Append(section.Box37NetWeight);
				weight.Append(section.Box38QuantityAndUnit);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line1);
				weight.Append(section.Box39StatisticsQuantityAndUnit_Line2);
				fob.Append(section.Box40FOBValue_Line1);
				fob.Append(section.Box40FOBValue_Line2);
				fob.Append(section.Box40FOBValue_Line3);
				fob.Append(section.Box40FOBValue_Line4);
				statistic.Append(section.Box41ModeOfStatistics_Line1);
				statistic.Append(section.Box41ModeOfStatistics_Line2);
				statistic.Append(section.Box41ModeOfStatistics_Line3);
				statistic.Append(section.Box41ModeOfStatistics_Line4);
			}
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(sequence.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


1











"), "SequenceNum:");

				NUnit.Framework.Assert.That(brand.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"


TOTOTA











"), "Brand:");

				NUnit.Framework.Assert.That(detail.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"



group1








賣方料號:P/N: 1231
買方料號:CPN: 1111
Test Descriptions. line1
型號:Model line



規格:Compositions line1



原進倉報單號碼/項次:LNNE1234567890-2222



原報單號碼/項次:previousDoc-2222



輸出入許可文件號碼/項次:



DocA123-3



DocA124-4



DocA125-5



產地證明書號碼/項次:LN:111-11



主管機關指定代號:DocB223



生產國別:TW






"), "Details:");

				NUnit.Framework.Assert.That(code.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











DocA121-1
DocA122-2
1008.21.00.00-5
YB/DocB123













































Total:

"), "Codes:");

				NUnit.Framework.Assert.That(price.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











USD
300,000














































        ----------


"), "Prices:");

				NUnit.Framework.Assert.That(weight.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











950 KGM
2 SET














































----------------------
950 KGM
2 SET
vvvvvvvvvvvv"), "Weight:");

				NUnit.Framework.Assert.That(fob.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











<Currency(13000000,TWD)>















































----------------------
<Currency(13000000,TWD)>
vvvvvvvvvv
"), "FOB:");

				NUnit.Framework.Assert.That(statistic.ToStringWithNewLineBetweenAppends(), NUnit.Framework.Is.EqualTo(@"











9U


















































"), "Statistic:");
			});
		}

		[ExpectNoExceptions]
		public override void TestPrintOriginAdditionalDocumentOnGoodsDescriptionWhenNotPrintedOnCodes()
		{
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_Description = "Entry Line 1";
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var firstPermitNumber = invoiceLine.PermitCusSupportingCollection.AddNew();
			firstPermitNumber.CSI_ReferenceNumber = "A123456";
			firstPermitNumber.CSI_LineNo = 5;

			var secondPermitNumber = invoiceLine.PermitCusSupportingCollection.AddNew();
			secondPermitNumber.CSI_ReferenceNumber = "B123456";
			secondPermitNumber.CSI_LineNo = 6;

			invoiceLine.CertificateOfOriginNumber = "C567890";
			invoiceLine.CertificateOfOriginNumberItemNumber = 8;

			var wrapper = GetExportCustomsDeclarationDocumentWrapper(entryHeader, Factory);
			var firstSection = wrapper.GoodsItemListSections.Cast<GoodsItemList7Col4RowSection>().First();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line1, NUnit.Framework.Is.EqualTo("A123456-5").Using(CustomComparers.TypeComparison), "firstSection.Box34ImportExportPermitNumberAndItemNumber_Line1");
				NUnit.Framework.Assert.That(firstSection.Box34ImportExportPermitNumberAndItemNumber_Line2, NUnit.Framework.Is.EqualTo("B123456-6").Using(CustomComparers.TypeComparison), "firstSection.Box34ImportExportPermitNumberAndItemNumber_Line2");
				NUnit.Framework.Assert.That(firstSection.Box33DescriptionOfGoods_Line2, NUnit.Framework.Is.EqualTo("產地證明書號碼/項次:C567890-8").Using(CustomComparers.TypeComparison), "firstSection.Box33DescriptionOfGoods_Line2");
			});
		}
	}
}
