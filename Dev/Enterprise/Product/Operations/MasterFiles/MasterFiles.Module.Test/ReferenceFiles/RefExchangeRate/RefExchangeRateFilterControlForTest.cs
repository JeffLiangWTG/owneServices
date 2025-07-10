using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefExchangeRateFilterControlForTest : RefExchangeRateFilterControl
	{
		public RefExchangeRateFilterControlForTest(RefExchangeRateCollection collection, RefExchangeRateFilterBusinessObject bizo) : base(collection, bizo)
		{
		}
	}
}
