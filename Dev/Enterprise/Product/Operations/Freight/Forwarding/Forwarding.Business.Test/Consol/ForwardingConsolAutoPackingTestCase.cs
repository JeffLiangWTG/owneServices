using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolAutoPackingTestCase : ConsolAutoPackingTestCase
	{
		#region Implementation

		protected override CommonConsol CreateConsol()
		{
			return Factory.New<ForwardingConsol>();
		}

		#endregion
	}
}
