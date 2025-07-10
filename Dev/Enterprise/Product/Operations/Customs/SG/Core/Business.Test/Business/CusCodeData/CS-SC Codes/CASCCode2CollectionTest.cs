using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CASCCode2Collection))]
	public class CASCCode2CollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<CASCCode2>
	{
		public void TestAllowNew()
		{
			CASCCode2Collection cASCCode2Collection = new CASCCode2Collection(InvoiceLine);
			AssertEquals("Max Count 50", 50, cASCCode2Collection.MaxCount);
		}

		protected override Customs.Business.CusCodeDataCollection<CASCCode2> GetCusCodeDataCollection()
		{
			return new CASCCode2Collection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CASCCode2 result = Factory.New<CASCCode2>();
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
