using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MiningInformationCollection))]
	sealed class MiningInformationCollectionTest : CusCodeDataCollectionTest<MiningInformation>
	{
		public void TestAllowNew()
		{
			Assert(InvoiceLine.MiningInformations.AllowNew);

			InvoiceLine.US_DisclaimSanctions = true;
			Assert(!InvoiceLine.MiningInformations.AllowNew);
		}

		protected override CusCodeDataCollection<MiningInformation> GetCusCodeDataCollection()
		{
			return new MiningInformationCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<MiningInformation>();
			result.CY_ParentID = InvoiceLine.PK;
			result.CY_ParentTableCode = InvoiceLine.TablePrefix;
			return result;
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
