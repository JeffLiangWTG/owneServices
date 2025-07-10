using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CASCCode3Collection))]
	public class CASCCode3CollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<CASCCode3>
	{
		public void TestAllowNew()
		{
			CASCCode3Collection cASCCode3Collection = new CASCCode3Collection(InvoiceLine);
			AssertEquals("Max Count 50", 50, cASCCode3Collection.MaxCount);
		}

		protected override Customs.Business.CusCodeDataCollection<CASCCode3> GetCusCodeDataCollection()
		{
			return new CASCCode3Collection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CASCCode3 result = Factory.New<CASCCode3>();
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
