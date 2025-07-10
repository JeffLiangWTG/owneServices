using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	partial class RefContainerStockFetchStrategyTest
	{
		public void TestLastMovementFetchHinted()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ConcatenateMultipleFetchHintTypes = false;
				{
					BusinessObjectFactory createFactory = new BusinessObjectFactory();

					RefContainer containerType = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

					RefContainerStock stock1 = createFactory.New<RefContainerStock>();
					stock1.R6_ContainerNum = "TEST4100013";
					stock1.R6_RC = containerType.PK;

					RefContainerStock stock2 = createFactory.New<RefContainerStock>();
					stock2.R6_ContainerNum = "TEST4100029";
					stock2.R6_RC = containerType.PK;

					ContainerMovement movement2a = stock2.Movements.AddNew();
					movement2a.E9_MovementDate = ZDateTime.Empty;
					movement2a.E9_OtherLocation = "movement2a";

					RefContainerStock stock3 = createFactory.New<RefContainerStock>();
					stock3.R6_ContainerNum = "TEST4100034";
					stock3.R6_RC = containerType.PK;

					ContainerMovement movement3a = stock3.Movements.AddNew();
					movement3a.E9_MovementDate = ZDateTime.Today.AddDays(-2);
					movement3a.E9_OtherLocation = "movement3a";

					RefContainerStock stock4 = createFactory.New<RefContainerStock>();
					stock4.R6_ContainerNum = "TEST4100050";
					stock4.R6_RC = containerType.PK;

					ContainerMovement movement4a = stock4.Movements.AddNew();
					movement4a.E9_MovementDate = ZDateTime.Today.AddDays(-2);
					movement4a.E9_OtherLocation = "movement4b";

					ContainerMovement movement4b = stock4.Movements.AddNew();
					movement4b.E9_MovementDate = ZDateTime.Today.AddDays(-1);
					movement4b.E9_OtherLocation = "movement4b";

					createFactory.Save();
				}

				{
					BusinessObjectFactory loadFactory = new BusinessObjectFactory();
					RefContainerStock stock1 = RefContainerStock.Load(loadFactory, "TEST4100013");
					RefContainerStock stock2 = RefContainerStock.Load(loadFactory, "TEST4100029");
					RefContainerStock stock3 = RefContainerStock.Load(loadFactory, "TEST4100034");
					RefContainerStock stock4 = RefContainerStock.Load(loadFactory, "TEST4100050");

					loadFactory.ResetDatabaseLoadCount();

					foreach (RefContainerStock stock in new RefContainerStock[] { stock1, stock2, stock3, stock4 })
					{
						stock.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn("", "LastMovement+E9_MovementType") });
					}

					AssertEquals("stock1", null, stock1.LastMovement == null ? null : stock1.LastMovement.E9_OtherLocation.ToString());
					AssertEquals("stock2", null, stock2.LastMovement == null ? null : stock2.LastMovement.E9_OtherLocation.ToString());
					AssertEquals("stock3", "movement3a", stock3.LastMovement == null ? null : stock3.LastMovement.E9_OtherLocation.ToString());
					AssertEquals("stock4", "movement4b", stock4.LastMovement == null ? null : stock4.LastMovement.E9_OtherLocation.ToString());

					AssertMaxDbHits("LastMovement fetch hints should be in 1 query.", 1, loadFactory);
					AssertEquals("All fetch hints loaded in 1 statement", 1, loadFactory.DatabaseLoadCount);
				}
			}
		}

		public void TestLastMovementFetchHinted_Concatenate()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.ConcatenateMultipleFetchHintTypes = true;
				{
					BusinessObjectFactory createFactory = new BusinessObjectFactory();

					RefContainer containerType = createFactory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

					RefContainerStock stock1 = createFactory.New<RefContainerStock>();
					stock1.R6_ContainerNum = "TEST4100013";
					stock1.R6_RC = containerType.PK;

					RefContainerStock stock2 = createFactory.New<RefContainerStock>();
					stock2.R6_ContainerNum = "TEST4100029";
					stock2.R6_RC = containerType.PK;

					ContainerMovement movement2a = stock2.Movements.AddNew();
					movement2a.E9_MovementDate = ZDateTime.Empty;
					movement2a.E9_OtherLocation = "movement2a";

					RefContainerStock stock3 = createFactory.New<RefContainerStock>();
					stock3.R6_ContainerNum = "TEST4100034";
					stock3.R6_RC = containerType.PK;

					ContainerMovement movement3a = stock3.Movements.AddNew();
					movement3a.E9_MovementDate = ZDateTime.Today.AddDays(-2);
					movement3a.E9_OtherLocation = "movement3a";

					RefContainerStock stock4 = createFactory.New<RefContainerStock>();
					stock4.R6_ContainerNum = "TEST4100050";
					stock4.R6_RC = containerType.PK;

					ContainerMovement movement4a = stock4.Movements.AddNew();
					movement4a.E9_MovementDate = ZDateTime.Today.AddDays(-2);
					movement4a.E9_OtherLocation = "movement4b";

					ContainerMovement movement4b = stock4.Movements.AddNew();
					movement4b.E9_MovementDate = ZDateTime.Today.AddDays(-1);
					movement4b.E9_OtherLocation = "movement4b";

					createFactory.Save();
				}

				{
					BusinessObjectFactory loadFactory = new BusinessObjectFactory();
					RefContainerStock stock1 = RefContainerStock.Load(loadFactory, "TEST4100013");
					RefContainerStock stock2 = RefContainerStock.Load(loadFactory, "TEST4100029");
					RefContainerStock stock3 = RefContainerStock.Load(loadFactory, "TEST4100034");
					RefContainerStock stock4 = RefContainerStock.Load(loadFactory, "TEST4100050");

					loadFactory.ResetDatabaseLoadCount();

					foreach (RefContainerStock stock in new RefContainerStock[] { stock1, stock2, stock3, stock4 })
					{
						stock.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn("", "LastMovement+E9_MovementType") });
					}

					AssertEquals("stock1", null, stock1.LastMovement == null ? null : stock1.LastMovement.E9_OtherLocation.ToString());
					AssertEquals("stock2", null, stock2.LastMovement == null ? null : stock2.LastMovement.E9_OtherLocation.ToString());
					AssertEquals("stock3", "movement3a", stock3.LastMovement == null ? null : stock3.LastMovement.E9_OtherLocation.ToString());
					AssertEquals("stock4", "movement4b", stock4.LastMovement == null ? null : stock4.LastMovement.E9_OtherLocation.ToString());

					AssertMaxDbHits("LastMovement fetch hints should be in 1 query.", 1, loadFactory);
					AssertEquals("All fetch hints loaded in 1 statement", 1, loadFactory.DatabaseLoadCount);
				}
			}
		}
	}
}
