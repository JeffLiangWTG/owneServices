using System.Linq;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRatingAdaptersProviderTest : DtbConsignmentTestCaseWithFactory
	{
		#region TestGetAdapters

		#region TestGetAdapters_Loose

		public void TestGetAdapters_Loose()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			AssertEquals("Precondition", 0, new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);

			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			AssertEquals(0, new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);

			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			AssertEquals(0, new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);

			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			var adapters = new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals(1, adapters.Count);
			AssertEquals(typeof(DtbConsignmentRatingAdapter), adapters.Single().GetType());
			AssertEquals(FreightMode.LRO, adapters[0].FreightMode);
		}

		#endregion

		#region TestGetAdapters_FCL_FTL

		public void TestGetAdapters_FCL_FTL()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var container = Helper.CreatePackageContainer("CONT123");
			consignment.PackageJob.Packages.Add(container);
			AssertEquals("Precondition", 0, new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);

			var pickup = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			consignment.PackageJob.Packages.Add(container);
			AssertEquals(0, new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);

			var multi = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			consignment.PackageJob.Packages.Add(container);
			AssertEquals(0, new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);

			var delivery = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);
			consignment.PackageJob.Packages.Add(container);
			var adapters = new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals(1, adapters.Count);
			AssertEquals(typeof(DtbConsignmentRatingAdapter), adapters[0].GetType());
			AssertEquals(FreightMode.FRO, adapters[0].FreightMode);

			container.Container.K0_ContainerMode = Constants.ContainerModes.FTL;
			adapters = new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals(1, adapters.Count);
			AssertEquals(typeof(DtbConsignmentRatingAdapter), adapters[0].GetType());
			AssertEquals(FreightMode.FRO, adapters[0].FreightMode);
		}

		#endregion

		#region TestGetAdapters_Both

		public void TestGetAdapters_Both()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			var pallet = Helper.CreatePackage("PLT1", 1, "PLT");
			var container = Helper.CreatePackageContainer("CONT123");
			consignment.PackageJob.Packages.Add(container);
			consignment.PackageJob.Packages.Add(pallet);
			AssertEquals("Precondition", 0, new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue).Count);

			var pickup = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var delivery = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery);

			var adapters = new DtbConsignmentRatingAdaptersProvider(consignment).GetAdapters(null, AutoRateOptions.AutorateRevenue);
			AssertEquals(2, adapters.Count);
			AssertContainsExactElementsInAnyOrder(new[] { FreightMode.FRO, FreightMode.LRO }, adapters.Select(a => a.FreightMode));
		}

		#endregion

		#region TestGetAdapters_ConsignmentHasNoInstructions_LogWarning

		public void TestGetAdapters_ConsignmentHasNoInstructions_LogWarning()
		{
			var consignment = Helper.CreateConsignment("LTC001");
			consignment.Addresses.DeleteAll();

			var interactor = new RatingAdaptersProviderTest.TestUIInteractor();
			var adaptersProvider = new DtbConsignmentRatingAdaptersProvider(consignment);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);

			AssertEquals("Adapters count", 0, adapters.Count);
			AssertCollectionContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("No pickup/delivery addresses found"), interactor.errors);
		}

		#endregion

		#endregion
	}
}
