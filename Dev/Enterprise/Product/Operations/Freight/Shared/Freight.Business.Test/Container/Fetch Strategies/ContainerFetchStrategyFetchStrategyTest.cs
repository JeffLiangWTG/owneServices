using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerFetchStrategyFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			var consol = Factory.Load<CommonConsol>(CreateConsolWithContainersInNewFactory());

			Factory.ResetDatabaseLoadCount();

			foreach (CommonContainer container in consol.Containers)
			{
				var cartage = container.DestinationCartage;
				var declaration = container.Declaration;

				foreach (var penalty in container.ImportPenalties)
				{
					var penaltyType = penalty.CPY_PenaltyType;
				}
			}

			//CusContainer: 1
			//JobBookedCtgMove: 1
			//JobContainer: 1
			//JobContainerPenalty: 1

			//Hits: 4/1

			AssertMaxDbHits(4, Factory);
		}

		public void TestFetchForView()
		{
			CombineAssertions(() =>
			{
				CheckDBHitCount(nameof(CommonContainer.AdditionalReferenceNumbersAsString), 1, true);
				CheckDBHitCount(nameof(CommonContainer.AdditionalReferenceNumbersAsString), 4, false);
			});
		}

		#region Implementation

		void CheckDBHitCount(string columnName, int maxDbHits, bool fetchForView)
		{
			var factory = new BusinessObjectFactory();

			var containers = CreateContainers(factory);
			if (fetchForView)
			{
				foreach (var container in containers)
				{
					container.FetchStrategy.FetchForView(new[] { new TableColumn("", columnName) });
				}
			}
			factory.ResetDatabaseLoadCount();

			foreach (var container in containers)
			{
				object value = container[columnName];
			}

			var message = ZString.Format("column name: {0}, max db hit count: {1}, actual count: {2}", columnName, maxDbHits, factory.DatabaseLoadCount);

			Assert(message, factory.DatabaseLoadCount <= maxDbHits);
		}

		static List<CommonContainer> CreateContainers(BusinessObjectFactory factory)
		{
			List<CommonContainer> result = new List<CommonContainer>();

			for (int i = 0; i < 4; i++)
			{
				var container = factory.New<CommonContainer>();
				CreateContainerPenalty(container, Constants.ContainerPenaltyPenaltyType.Codes.Detention,
					Constants.ContainerPenaltyCreditorType.Codes.Carrier,
					Constants.ContainerPenaltyTimeUnit.Codes.Days);
				CreateContainerPenalty(container, Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime,
					Constants.ContainerPenaltyCreditorType.Codes.Transport,
					Constants.ContainerPenaltyTimeUnit.Codes.Hours);
				CreateContainerPenalty(container, Constants.ContainerPenaltyPenaltyType.Codes.Storage,
					Constants.ContainerPenaltyCreditorType.Codes.CTO,
					Constants.ContainerPenaltyTimeUnit.Codes.Days);

				result.Add(container);
			}

			factory.Save();

			return result;
		}

		static void CreateContainerPenalty(CommonContainer container, ZString penaltyType, ZString creditorType, ZString timeUnit)
		{
			var penalty = container.ImportPenalties.AddNew();
			penalty.CPY_PenaltyType = penaltyType;
			penalty.CPY_CreditorType = creditorType;
			penalty.CPY_RL_NKLocation = "AUSYD";
			penalty.CPY_Duration = new ZDateTime(2020, 1, 1);
			penalty.CPY_TimeUnit = timeUnit;
			penalty.CPY_RX_NKCurrency = "AUD";
		}

		ZGuid CreateConsolWithContainersInNewFactory()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<CommonConsol>();

			for (int i = 0; i < 12; i++)
			{
				consol.Containers.AddNew();
			}

			factory.Save();

			return consol.PK;
		}

		#endregion
	}
}
