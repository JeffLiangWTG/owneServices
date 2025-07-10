using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class TransportSupporterTestCase<T> : TestCaseWithFactory
			where T : TransportSupporter
	{
		public void TestDistanceCalculationCheckpoint()
		{
			var supporter = GetNewTransportSupporter();
			AssertEquals(ExpectedDistanceCalculationCheckpoint, supporter.DistanceCalculationCheckpoint);
		}

		protected abstract SecurityCheckpoint ExpectedDistanceCalculationCheckpoint { get; }
		protected abstract TransportSupporter GetNewTransportSupporter();

		protected override void SetUp()
		{
			base.SetUp();
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
		}
		protected ZString StoredCountry;

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
		}

		protected abstract ZString TestingCountry { get; }
	}
}
