using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemReceiveConsignmentRatingAdaptersProviderTest : WhsTransitTestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetAdapters_ReceiveConsignment_NoPackages()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;

			Factory.Save();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();
			var adaptersProvider = new WhsItemReceiveConsignmentRatingAdaptersProvider(rcn);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);
			AssertEquals(1, adapters.Count);
		}
	}
}
