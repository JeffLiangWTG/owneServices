using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class AccExchangeRateConfigurationsHelperTest : TestCaseWithFactory
	{
		public void TestLoadAndDelete()
		{
			var exRateConfig1 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			var exRateConfig2 = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			//Change exchange rate config to avoid duplication
			exRateConfig1.JCE_TransportMode = "SEA";
			exRateConfig2.JCE_TransportMode = "AIR";
			Factory.Save();

			var exRateConfigQuery1 = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration));
			exRateConfigQuery1.AddToFilter(AccExchangeRateConfigurationViewSchema.PK, new List<ZGuid>() { exRateConfig1.PK, exRateConfig2.PK });
			AssertEquals(true, Factory.Exists(typeof(AccExchangeRateConfiguration), exRateConfigQuery1, false));

			AccExchangeRateConfigurationsHelper.LoadAndDelete(Factory, exRateConfigQuery1);
			Factory.Save();

			var exRateConfigQuery2 = new ZDBOnlyQuery(typeof(AccExchangeRateConfiguration));
			exRateConfigQuery2.AddToFilter(AccExchangeRateConfigurationViewSchema.PK, exRateConfig1.PK);
			exRateConfigQuery2.AddToFilter(JoinCondition.Or, AccExchangeRateConfigurationViewSchema.PK, exRateConfig2.PK);
			AssertEquals(false, Factory.Exists(typeof(AccExchangeRateConfiguration), exRateConfigQuery2, false));
		}

		[TestDate(2025, 2, 1)]
		public void TestGetExchangeRateDate()
		{
			var exchangeRateConfigurationRateConsumerMock = new Mock<IAccExchangeRateConfigurationRateConsumer>();
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.ConsolExchangeRateDate).Returns(() => new ZDateTime(2025, 2, 2));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.HistoricalRateFromActualArrivalDate).Returns(() => new ZDateTime(2025, 2, 3));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.HistoricalRateFromActualDepartureDate).Returns(() => new ZDateTime(2025, 2, 4));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.HistoricalRateFromEstimatedArrivalDate).Returns(() => new ZDateTime(2025, 2, 5));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.HistoricalRateFromEstimatedDepartureDate).Returns(() => new ZDateTime(2025, 2, 6));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.HistoricalRateFromEstimatedArrivalAtLoadPortDate).Returns(() => new ZDateTime(2025, 2, 7));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.HistoricalRateFromActualArrivalAtLoadPortDate).Returns(() => new ZDateTime(2025, 2, 8));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.PickupDate).Returns(() => new ZDateTime(2025, 2, 9));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.DeliveryDate).Returns(() => new ZDateTime(2025, 2, 10));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.RequiredDate).Returns(() => new ZDateTime(2025, 2, 11));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.FinalizedDate).Returns(() => new ZDateTime(2025, 2, 12));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.HouseBillIssueDate).Returns(() => new ZDateTime(2025, 2, 13));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.ShipmentOrBrokeragePickupDate).Returns(() => new ZDateTime(2025, 2, 14));
			exchangeRateConfigurationRateConsumerMock.Setup(x => x.ShipmentOrBrokerageDeliveryDate).Returns(() => new ZDateTime(2025, 2, 15));

			var rateConsumer = exchangeRateConfigurationRateConsumerMock.Object;

			AssertExchangeRateDate(ZDateTime.Invalid, ZString.Empty);
			AssertExchangeRateDate(ZDateTime.Today, Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 2), Core.Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 3), Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 4), Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 5), Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 6), Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 7), Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalAtLoadPortDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 8), Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalAtLoadPortDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 9), Core.Constants.JobBillingExchangeRatePreference.Code.PickupDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 10), Core.Constants.JobBillingExchangeRatePreference.Code.DeliveryDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 11), Core.Constants.JobBillingExchangeRatePreference.Code.RequiredDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 12), Core.Constants.JobBillingExchangeRatePreference.Code.FinalizedDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 13), Core.Constants.JobBillingExchangeRatePreference.Code.HouseBillIssueDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 14), Core.Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokeragePickupDate);
			AssertExchangeRateDate(new ZDateTime(2025, 2, 15), Core.Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokerageDeliveryDate);

			void AssertExchangeRateDate(ZDateTime expectedRateDate, ZString reference)
			{
				var actualExchangeRateDate = AccExchangeRateConfigurationsHelperForTest.GetExchangeRateDate(rateConsumer, reference);

				AssertEquals(expectedRateDate, actualExchangeRateDate);
			}
		}

		public void TestDependencyInjection()
		{
			AssertNotNull(AccExchangeRateConfigurationsHelperForTest);
			AssertType<AccExchangeRateConfigurationsHelper>(AccExchangeRateConfigurationsHelperForTest);
		}

		IAccExchangeRateConfigurationsHelper AccExchangeRateConfigurationsHelperForTest => accExchangeRateConfigurationsHelperForTest ??= ObjectFactory.Get<IAccExchangeRateConfigurationsHelper>();
		IAccExchangeRateConfigurationsHelper accExchangeRateConfigurationsHelperForTest;
	}
}
