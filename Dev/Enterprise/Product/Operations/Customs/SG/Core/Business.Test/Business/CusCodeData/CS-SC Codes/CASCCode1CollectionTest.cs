using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CASCCode1Collection))]
	public class CASCCode1CollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<CASCCode1>
	{
		public void TestAllowNew()
		{
			CASCCode1Collection cASCCode1Collection = new CASCCode1Collection(InvoiceLine);
			AssertEquals("Max Count 50", 50, cASCCode1Collection.MaxCount);
		}

		protected override Customs.Business.CusCodeDataCollection<CASCCode1> GetCusCodeDataCollection()
		{
			return new CASCCode1Collection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CASCCode1 result = Factory.New<CASCCode1>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = "JI";
			return result;
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>());
			}
		}

		JobComInvoiceLine invoiceLine;
	}
}
