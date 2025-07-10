using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	class InvoiceLinesForEntryLineCollectionPerformanceTest : TestCaseWithFactory
	{
		public void TestLoad_ShouldNotLoopThroughAllInvoiceLines()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var invoiceLine1 = Factory.New<JobComInvoiceLineForInvoiceLinesForEntryLineCollectionTest>();
			invoiceLine1.JI_CL = entryLine1.PK;
			declaration.InvoiceLines.Add(invoiceLine1);
			invoiceLine1.JI_CL_cnt = 0;
			var collection1 = new InvoiceLinesForEntryLineCollection(entryLine1);
			var collection2 = new InvoiceLinesForEntryLineCollection(entryLine2);
			collection1.Load();
			collection2.Load();
			AssertLessThan(invoiceLine1.JI_CL_cnt, 2);
		}
	}

	class JobComInvoiceLineForInvoiceLinesForEntryLineCollectionTest : BaseJobComInvoiceLine
	{
		public JobComInvoiceLineForInvoiceLinesForEntryLineCollectionTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZGuid JI_CL
		{
			get
			{
				JI_CL_cnt++;
				return base.JI_CL;
			}
			set
			{
				base.JI_CL = value;
			}
		}

		public int JI_CL_cnt { get; set; }
	}
}
