using CargoWise.EntityFramework;

namespace Enterprise.Freight.QuotedBookings.Module.Test
{
	internal class OneOffQuoteControllerForTest : OneOffQuoteController
	{
		public IBusiness GetNewBusinessEntityInLocalFactoryForTest()
		{
			return base.GetNewBusinessEntityInLocalFactory();
		}

		internal IBusiness GetLoadedBusinessEntityInLocalFactoryExposed(IBusiness bizO)
		{
			return GetLoadedBusinessEntityInLocalFactory(bizO);
		}
	}
}
