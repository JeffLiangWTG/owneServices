using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(VoyageExRate))]
	sealed class VoyageExRateBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			VoyageExRate rate = Factory.New<JobVoyage>().ExRates.AddNew();
			rate.E8_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			return rate;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var rate = Factory.NewWithValidTestData<JobVoyage>().ExRates.AddNew();
			rate.E8_RX_NKExCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			return rate;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rate = factory.New<JobVoyage>().ExRates.AddNew();
			rate.E8_RX_NKExCurrency = factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;

			return rate;
		}

		#endregion
	}
}
