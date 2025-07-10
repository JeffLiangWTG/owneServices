using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	class CommonCartageRatingAdaptersProviderTest : RatingAdaptersProviderTest
	{
		public void TestGetAdapters_ForCostWhenNoLegs_LogWarning()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			AssertEquals("Adapters count", 0, adapters.Count);
			AssertCollectionContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("No legs or services found"), interactor.errors);
			interactor.errors.Clear();
			var move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.AddNew();
			adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			AssertCollectionNotContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("No legs or services found"), interactor.errors);
		}

		public void TestGetAdapters_ForCost_CartageRatingMergeChargeOption()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var interactor = new TestUIInteractor();
			var move = cartage.LooseBookedMoves.AddNew();
			move.CartageLegs.AddNew();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			AssertContainsExactElementsInAnyOrder("Merge charge option - for Cost", new[] { MergeChargeOptions.WithinAdapter, MergeChargeOptions.WithinAdapter }, adapters.Select(x => x.MergeCharges));
		}

		public void TestGetAdapters_ForRevenueWhenNoMoves_LogWarning()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);
			AssertEquals("Adapters count", 0, adapters.Count);
			AssertCollectionContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("No booked moves found"), interactor.errors);
			interactor.errors.Clear();
			var move = cartage.BookedMovesCollection.AddNew();
			move.CartageLegs.AddNew();
			adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);
			AssertEquals("Adapters count", 1, adapters.Count);
			AssertCollectionNotContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("No booked moves found"), interactor.errors);
		}

		public void TestGetAdapters_ForRevenue_CartageRatingMergeChargeOption()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Mixed;
			var interactor = new TestUIInteractor();
			var move = cartage.BookedMovesCollection.AddNew();
			move.CartageLegs.AddNew();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateRevenue);
			var adapter = adapters.Single();
			AssertEquals("Merge charge option - for Revenue", MergeChargeOptions.WithinAdapter, adapter.MergeCharges);
		}

		public void TestGetAdapters_LooseCartage_SingleMovement_SingleLeg_NoService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.All(x => x.JobServices.All(s => !s.IsEnabled)));
		}

		public void TestGetAdapters_LooseCartage_MultipleMovements_SingleLeg_NoService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.AddNew();
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.All(x => x.JobServices.All(s => !s.IsEnabled)));
		}

		public void TestGetAdapters_LooseCartage_SingleMovement_SingleLeg_SingleService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			var service = containerMove.Container.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service.ES_Completed = ZDateTime.Today;
			service.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			var moveAdapter = adapters.Single(x => x.JobServices.Count > 0);
			AssertEquals(Constants.FreightServiceType.Codes.Cleaning, moveAdapter.JobServices.Single(x => x.IsEnabled).ServiceCode);
		}

		public void TestGetAdapters_LooseCartage_MultipleMovements_SingleLeg_SingleService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.AddNew();
			var service1 = containerMove.Container.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1;
			var service2 = looseMove.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			AssertContainsExactElementsInAnyOrder(new[] { Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Cleaning }, adapters.SelectMany(x => x.JobServices.Where(s => s.IsEnabled).Select(s => s.ServiceCode)));
		}

		public void TestGetAdapters_LooseCartage_SingleMovement_SingleLeg_MultipleServices()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			var service1 = containerMove.Container.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1;
			var service2 = containerMove.Container.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.Where(x => typeof(CartageRatingAdapter) == x.GetType()).All(x => x.JobServices.All(s => !s.IsEnabled)));
			AssertContainsExactElementsInAnyOrder(new[] { Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Washing }, adapters.SelectMany(x => x.JobServices.Where(s => s.IsEnabled).Select(s => s.ServiceCode)));
		}

		public void TestGetAdapters_LooseCartage_MultipleMovements_SingleLeg_MultipleServices()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.AddNew();
			var service1 = containerMove.Container.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1;
			var service2 = containerMove.Container.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1;
			var service3 = looseMove.Services.AddNew();
			service3.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service3.ES_Completed = ZDateTime.Today;
			service3.ES_ServiceCount = 1;
			var service4 = looseMove.Services.AddNew();
			service4.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			service4.ES_Completed = ZDateTime.Today;
			service4.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			var serviceCodes = adapters.SelectMany(x => x.JobServices.Where(s => s.IsEnabled).Select(s => s.ServiceCode));
			var expectedCodes = new[] { Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Washing, Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Washing, };
			AssertContainsExactElementsInAnyOrder(expectedCodes, serviceCodes);
		}

		public void TestGetAdapters_LooseCartage_SingleMovement_MultipleLegs_NoService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			containerMove.CartageLegs.AddNew();
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.All(x => x.JobServices.All(s => !s.IsEnabled)));
		}

		public void TestGetAdapters_LooseCartage_MultipleMovements_MultipleLegs_NoService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			containerMove.CartageLegs.AddNew();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.AddNew();
			looseMove.CartageLegs.AddNew();
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.All(x => x.JobServices.All(s => !s.IsEnabled)));
		}

		public void TestGetAdapters_LooseCartage_SingleMovement_MultipleLegs_SingleService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			containerMove.CartageLegs.AddNew();
			var service = containerMove.Container.Services.AddNew();
			service.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service.ES_Completed = ZDateTime.Today;
			service.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.Where(x => x.GetType() == typeof(CartageRatingAdapter)).All(x => x.JobServices.All(s => !s.IsEnabled)));
			AssertContainsExactElementsInAnyOrder(new[] { Constants.FreightServiceType.Codes.Cleaning }, adapters.SelectMany(x => x.JobServices.Where(s => s.IsEnabled).Select(s => s.ServiceCode)));
		}

		public void TestGetAdapters_LooseCartage_MultipleMovements_MultipleLegs_SingleService()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			containerMove.CartageLegs.AddNew();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.AddNew();
			looseMove.CartageLegs.AddNew();
			var service1 = containerMove.Container.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1;
			var service2 = looseMove.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.Where(x => x.GetType() == typeof(CartageRatingAdapter)).All(x => x.JobServices.All(s => !s.IsEnabled)));
			AssertContainsExactElementsInAnyOrder(new[] { Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Cleaning }, adapters.SelectMany(x => x.JobServices.Where(s => s.IsEnabled).Select(s => s.ServiceCode)));
		}

		public void TestGetAdapters_LooseCartage_SingleMovement_MultipleLeg_MultipleServices()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			containerMove.CartageLegs.AddNew();
			var service1 = containerMove.Container.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1;
			var service2 = containerMove.Container.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.Where(x => typeof(CartageRatingAdapter) == x.GetType()).All(x => x.JobServices.All(s => !s.IsEnabled)));
			AssertContainsExactElementsInAnyOrder(new[] { Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Washing }, adapters.SelectMany(x => x.JobServices.Where(s => s.IsEnabled).Select(s => s.ServiceCode)));
		}

		public void TestGetAdapters_LooseCartage_MultipleMovements_MultipleLeg_MultipleServices()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var containerMove = cartage.ContainerBookedMoves.AddNew();
			containerMove.CartageLegs.AddNew();
			containerMove.CartageLegs.AddNew();
			var looseMove = cartage.LooseBookedMoves.AddNew();
			looseMove.CartageLegs.AddNew();
			looseMove.CartageLegs.AddNew();
			var service1 = containerMove.Container.Services.AddNew();
			service1.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service1.ES_Completed = ZDateTime.Today;
			service1.ES_ServiceCount = 1;
			var service2 = containerMove.Container.Services.AddNew();
			service2.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			service2.ES_Completed = ZDateTime.Today;
			service2.ES_ServiceCount = 1;
			var service3 = looseMove.Services.AddNew();
			service3.ES_ServiceCode = Constants.FreightServiceType.Codes.Cleaning;
			service3.ES_Completed = ZDateTime.Today;
			service3.ES_ServiceCount = 1;
			var service4 = looseMove.Services.AddNew();
			service4.ES_ServiceCode = Constants.FreightServiceType.Codes.Washing;
			service4.ES_Completed = ZDateTime.Today;
			service4.ES_ServiceCount = 1;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);
			Assert(adapters.Where(x => typeof(CartageLegRatingAdapter) == x.GetType()).All(x => x.JobServices.Count == 0));
			var expectedServices = new[] { Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Cleaning, Constants.FreightServiceType.Codes.Washing, Constants.FreightServiceType.Codes.Washing, };
			AssertContainsExactElementsInAnyOrder(expectedServices, adapters.SelectMany(x => x.JobServices.Where(s => s.IsEnabled).Select(s => s.ServiceCode)));
		}

		public void TestGetAdapterForQuickCalculate_ManyContainers_DifferentWeightUnits()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var move1 = cartage.ContainerBookedMoves.AddNew();
			move1.EW_WeightUQ = Constants.Weight.Pounds;
			move1.CartageLegs.AddNew();
			var container1 = move1.Container;
			container1.JC_ContainerNum = "TEST41000";
			container1.JC_RC = ZGuid.NewZGuid();
			container1.JC_GrossWeightUQ = Constants.Weight.Pounds;
			container1.JC_TareWeight = 0;
			container1.JC_Calc_NetWeight = 1000;
			var move2 = cartage.ContainerBookedMoves.AddNew();
			move2.EW_WeightUQ = Constants.Weight.Kilograms;
			move2.CartageLegs.AddNew();
			var container2 = move2.Container;
			container2.JC_RC = ZGuid.NewZGuid();
			container2.JC_ContainerNum = "TEST42000";
			container2.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container2.JC_TareWeight = 0;
			container2.JC_Calc_NetWeight = 1000;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var calculateAdapter = adaptersProvider.GetForQuickCalculate(interactor);
			var measures = ((RateableMeasureSet)calculateAdapter.QuickMeasures);
			var containers = measures.GetContainerListWithCalculatedWeightVolume();
			AssertEquals(2, containers.Sum(c => c.ContainerCount));
			// Containers always have their weight stored in KGs
			AssertEquals(1453.59237m, containers.Sum(c => c.Weight));
			// Note: It appears that the moves are converted to measures in an unpredictable order.
			// so it may convert to KG, or it may convert to LB, depends on which one gets retrieved
			// first.
			var weightUnit = measures.GetUnit(MeasureType.Weight);
			var weightValue = measures.GetActual(MeasureType.Weight);
			if (weightUnit == "LB")
			{
				AssertEquals(3204.622622m, weightValue);
			}
			else if (weightUnit == "KG")
			{
				AssertEquals(1453.59237m, weightValue);
			}
		}

		public void TestGetAdapterForQuickCalculate_ManyContainers_DifferentPackUnits()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var move1 = cartage.ContainerBookedMoves.AddNew();
			move1.CartageLegs.AddNew();
			var container1 = move1.Container;
			container1.JC_ContainerNum = "TEST41000";
			container1.JC_RC = ZGuid.NewZGuid();
			move1.EW_BookedPackCount = 100;
			move1.EW_F3_NKPackType = "PLT";
			var move2 = cartage.ContainerBookedMoves.AddNew();
			move2.CartageLegs.AddNew();
			var container2 = move2.Container;
			container2.JC_RC = ZGuid.NewZGuid();
			container2.JC_ContainerNum = "TEST42000";
			move2.EW_BookedPackCount = 200;
			move2.EW_F3_NKPackType = "BOX";
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var calculateAdapter = adaptersProvider.GetForQuickCalculate(interactor);
			var measures = ((RateableMeasureSet)calculateAdapter.QuickMeasures);
			var packs = measures.GetActual(MeasureType.Package);
			// When packages of different units are combined they will still accumulate but without units
			AssertEquals(string.Empty, measures.GetUnit(MeasureType.Package));
			AssertEquals(300m, packs);
		}

		public void TestGetAdapterForQuickCalculate_ManyContainers_SameUnits()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var move1 = cartage.ContainerBookedMoves.AddNew();
			move1.EW_WeightUQ = Constants.Weight.Kilograms;
			move1.CartageLegs.AddNew();
			var container1 = move1.Container;
			container1.JC_ContainerNum = "TEST41000";
			container1.JC_RC = ZGuid.NewZGuid();
			container1.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container1.JC_TareWeight = 0;
			container1.JC_Calc_NetWeight = 1000;
			move1.EW_BookedPackCount = 100;
			move1.EW_F3_NKPackType = "PLT";
			move1.EW_VolumeUQ = Constants.Volume.CubicMetres;
			move1.EW_BookedVolume = 1000m;
			var move2 = cartage.ContainerBookedMoves.AddNew();
			move2.EW_WeightUQ = Constants.Weight.Kilograms;
			move2.CartageLegs.AddNew();
			var container2 = move2.Container;
			container2.JC_RC = ZGuid.NewZGuid();
			container2.JC_ContainerNum = "TEST42000";
			container2.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container2.JC_TareWeight = 0;
			container2.JC_Calc_NetWeight = 2000;
			move2.EW_BookedPackCount = 200;
			move2.EW_F3_NKPackType = "PLT";
			move2.EW_VolumeUQ = Constants.Volume.CubicMetres;
			move2.EW_BookedVolume = 1000m;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var calculateAdapter = adaptersProvider.GetForQuickCalculate(interactor);
			var measures = ((RateableMeasureSet)calculateAdapter.QuickMeasures);
			var containers = measures.GetContainerListWithCalculatedWeightVolume();
			var packs = measures.GetActual(MeasureType.Package);
			var units = measures.GetActual(MeasureType.Unit);
			AssertEquals("PLT", measures.GetUnit(MeasureType.Package));
			AssertEquals(2, containers.Sum(c => c.ContainerCount));
			AssertEquals(3000m, containers.Sum(c => c.Weight));
			AssertEquals(3000m, measures.GetActual(MeasureType.Weight));
			AssertEquals("KG", measures.GetUnit(MeasureType.Weight));
			AssertEquals(2000m, measures.GetActual(MeasureType.Volume));
			AssertEquals("M3", measures.GetUnit(MeasureType.Volume));
			AssertEquals(300m, packs);
			AssertEquals(300m, units);
		}

		public void TestGetAdapterForQuickCalculate_LooseMovementHasNoLeg()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			var move = cartage.LooseBookedMoves.AddNew();
			move.EW_BookedWeight = 999;
			move.EW_WeightUQ = "KG";
			move.EW_BookedVolume = .12;
			move.EW_VolumeUQ = "M3";
			move.EW_BookedPackCount = 34;
			move.EW_F3_NKPackType = "BOX";
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var calculateAdapter = adaptersProvider.GetForQuickCalculate(interactor);
			var measures = ((RateableMeasureSet)calculateAdapter.QuickMeasures);
			AssertEquals(999m, measures.GetActual(MeasureType.Weight));
			AssertEquals(.12m, measures.GetActual(MeasureType.Volume));
			AssertEquals(34m, measures.GetActual(MeasureType.Package));
			AssertEquals(0m, measures.GetActual(MeasureType.ContainerCount));
		}

		public void TestGetAdapterForQuickCalculate_ContainerizedMovementHasNoLeg()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Containerized;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_ContainerNum = "TEST41000";
			container.JC_RC = ZGuid.NewZGuid();
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var calculateAdapter = adaptersProvider.GetForQuickCalculate(interactor);
			var measures = ((RateableMeasureSet)calculateAdapter.QuickMeasures);
			AssertEquals(1m, measures.GetActual(MeasureType.ContainerCount));
			AssertEquals(0m, measures.GetActual(MeasureType.Weight));
			AssertEquals(0m, measures.GetActual(MeasureType.Volume));
			AssertEquals(0m, measures.GetActual(MeasureType.Package));
		}

		public void TestGetAdapterForQuickCalculate_NoMovement_ContainerizedMode() => TestGetAdapterForQuickCalculate_NoMovement(Constants.CartageContainerMode.Containerized);
		public void TestGetAdapterForQuickCalculate_NoMovement_LooseMode() => TestGetAdapterForQuickCalculate_NoMovement(Constants.CartageContainerMode.Loose);
		void TestGetAdapterForQuickCalculate_NoMovement(ZString containerMode)
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ContainerMode = containerMode;
			var interactor = new TestUIInteractor();
			var adaptersProvider = new CommonCartageRatingAdaptersProvider(cartage);
			var calculateAdapter = adaptersProvider.GetForQuickCalculate(interactor);
			AssertNull(calculateAdapter);
			Assert(interactor.errors.Contains(@"Please ensure the minimum criteria required to use Autorating / Quick Calculator has been saved on the job:
No legs or services found"));
		}
	}
}
