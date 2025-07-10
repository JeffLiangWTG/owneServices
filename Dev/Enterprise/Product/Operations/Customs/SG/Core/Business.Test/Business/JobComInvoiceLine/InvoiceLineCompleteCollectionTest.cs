using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(InvoiceLineCompleteCollection))]
	public class InvoiceLineCompleteCollectionTest : Customs.Business.Testing.InvoiceLineCompleteCollectionTest
	{
		public void TestTypedIndexer()
		{
			InvoiceLineCompleteCollection collection = new InvoiceLineCompleteCollection(Declaration);
			JobComInvoiceLine invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new InvoiceLineCompleteCollection(Declaration);
		}

		protected new JobDeclaration Declaration
		{
			get
			{
				return (JobDeclaration)base.Declaration;
			}
		}

		protected override BaseJobDeclaration GetMeANewJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}
	}
}
