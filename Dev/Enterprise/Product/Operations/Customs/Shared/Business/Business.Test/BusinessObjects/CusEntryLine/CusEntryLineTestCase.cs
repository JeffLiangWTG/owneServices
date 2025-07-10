using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryLineTestCase : TestCaseWithFactory
	{
		public void TestCusEntryLineFetchStrategy()
		{
			CusEntryLineForTesting line = Factory.New<CusEntryLineForTesting>();
			AssertEquals(line.GetFetchStrategy().GetType(), typeof(FetchStrategies.CusEntryLineFetchStrategy));
		}

		public void TestUniqueKey()
		{
			var creator = new MergedDeclarationCreator<BaseJobDeclaration>(Factory, DeclarationApplicationCodeList.Codes.Builtin);
			creator.Entry1.EntryNumber = "B1234";
			creator.EntryLine1.CL_LineNumber = 4;
			AssertEquals("UniqueKey", "B1234-4", creator.EntryLine1.UniqueKey);
		}

		public void TestFOBValue()
		{
			BaseJobDeclaration dec = BaseJobDeclaration.New(Factory);

			BaseJobComInvoiceHeader invoiceHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var aUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			invoiceHeader.JZ_RX_NKInvoice_Currency = aUD.RX_Code;

			BaseJobComInvoiceLine invLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invLine1.JI_LinePrice = 1000m;
			BaseJobComInvoiceLine invLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invLine2.JI_LinePrice = 500m;

			LineMerger merger = new LineMerger(dec);
			merger.DoMerge();

			AssertEquals(new Money(1500m, aUD), dec.CustomsEntryHeaders[0].MergedLines[0].FOB);
		}

		class CusEntryLineForTesting : CusEntryLine
		{
			public CusEntryLineForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			new internal IBusinessObjectFetchStrategy GetFetchStrategy() => base.GetFetchStrategy();
		}
	}
}
