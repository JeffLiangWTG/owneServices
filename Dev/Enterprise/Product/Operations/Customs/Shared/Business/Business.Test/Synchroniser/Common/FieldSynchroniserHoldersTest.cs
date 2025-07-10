using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class FieldSynchroniserHoldersTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			testHolders.Add(testSync, true, false);
			AssertEquals("Test sync is there", true, testHolders.Contains(testSync));
			AssertEquals("Test sync is enabled", true, testSync.IsEnabled);
			AssertEquals("Test sync is detect enabled", false, testSync.DetectEnabled);

			testHolders.Remove(testSync);
			testHolders.Add(testSync, true, true);
			AssertEquals("Test sync is there", true, testHolders.Contains(testSync));
			AssertEquals("Test sync is enabled", true, testSync.IsEnabled);
			AssertEquals("Test sync is detect enabled", true, testSync.DetectEnabled);

			testHolders.Remove(testSync);
			testHolders.Add(testSync, false, true);
			AssertEquals("Test sync is there", true, testHolders.Contains(testSync));
			AssertEquals("Test sync is enabled", false, testSync.IsEnabled);
			AssertEquals("Test sync is detect enabled", true, testSync.DetectEnabled);
		}

		public void TestRemove()
		{
			var testContainer2 = testDec.CusContainers.AddNew();
			var sync2 = new CusContainerSynchroniser(testContainer2, consolContainer);

			testHolders.Add(sync2, true, false);
			testHolders.Add(testSync, true, false);

			AssertEquals("PreCondition:Test sync is there", true, testHolders.Contains(testSync));
			AssertEquals("PreCondition:Sync2 is there", true, testHolders.Contains(sync2));

			testHolders.Remove(container, consolContainer);
			AssertEquals("Test sync is removed", false, testHolders.Contains(testSync));
			AssertEquals("Sync2 is there", true, testHolders.Contains(sync2));
		}

		public void TestFind()
		{
			var testContainer2 = testDec.CusContainers.AddNew();
			var sync2 = new CusContainerSynchroniser(testContainer2, consolContainer);

			testHolders.Add(sync2, true, false);
			testHolders.Add(testSync, true, false);

			BusinessObjectSynchroniser result = testHolders.Find(container, consolContainer);
			AssertEquals("TestSync picked up", testSync, result);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var container2 = newFactory.Load<BaseCusContainer>(container.PK);
			var consolContainer2 = newFactory.Load<ForwardingContainer>(consolContainer.PK);
			AssertEquals("Should match as required by Data Refresh Bus", testSync, testHolders.Find(container2, consolContainer2));
		}

		public void TestFindMatchingType()
		{
			var consolContainer2 = consol.Containers.AddNew();
			var container2 = testDec.CusContainers.AddNew();

			var sync2 = new CusContainerSynchroniser2(container, consolContainer);
			var sync3 = new CusContainerSynchroniser2(container, consolContainer2);
			var sync4 = new CusContainerSynchroniser(container2, consolContainer);

			testHolders.Add(sync4, true, false);
			testHolders.Add(sync3, true, false);
			testHolders.Add(sync2, true, false);
			testHolders.Add(testSync, true, false);

			AssertEquals("Should match sync2 based on type", sync2, testHolders.Find<CusContainerSynchroniser2>(container, consolContainer));
			AssertEquals("Should match TestSync based on type", testSync, testHolders.Find<CusContainerSynchroniser>(container, consolContainer));
			AssertEquals("Should match sync3 based on type", sync3, testHolders.Find<CusContainerSynchroniser2>(container, consolContainer2));
			AssertNull("Should not match as not HouseBillSynchroniser2 for houseBill2 and Shipment", testHolders.Find<CusContainerSynchroniser2>(container2, consolContainer));
			AssertEquals("Should match sync4 based on type", sync4, testHolders.Find<CusContainerSynchroniser>(container2, consolContainer));

			AssertEquals("Should match TestSync as it's the first one on the list", testSync, testHolders.FindMatchingDestination<CusContainerSynchroniser>(container));
			AssertEquals("Should match sync3 as it's the first one on the list", sync3, testHolders.FindMatchingDestination<CusContainerSynchroniser2>(container));
			AssertEquals("Should match sync4 based on type", sync4, testHolders.FindMatchingDestination<CusContainerSynchroniser>(container2));
			AssertNull("Should not match as not HouseBillSynchroniser2 for houseBill2", testHolders.FindMatchingDestination<CusContainerSynchroniser2>(container2));

			AssertEquals("Should match sync4 as it's the first one on the list", sync4, testHolders.FindMatchingSource<CusContainerSynchroniser>(consolContainer));
			AssertEquals("Should match sync2 as it's the first one on the list", sync2, testHolders.FindMatchingSource<CusContainerSynchroniser2>(consolContainer));
			AssertEquals("Should match sync3 based on type", sync3, testHolders.FindMatchingSource<CusContainerSynchroniser2>(consolContainer2));
			AssertNull("Should not match as not HouseBillSynchroniser for consolContainer2", testHolders.FindMatchingSource<CusContainerSynchroniser>(consolContainer2));
		}

		class CusContainerSynchroniser2 : CusContainerSynchroniser
		{
			public CusContainerSynchroniser2(BaseCusContainer destination, ForwardingContainer source)
				: base(destination, source)
			{
			}
		}

		BusinessObjectSynchroniserList testHolders;
		CusContainerSynchroniser testSync;
		BaseJobDeclaration testDec;
		ForwardingConsol consol;
		ForwardingContainer consolContainer;
		BaseCusContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = BaseJobDeclaration.New(Factory);
			container = testDec.CusContainers.AddNew();
			consol = Factory.New<ForwardingConsol>();
			consolContainer = consol.Containers.AddNew();
			_ = consol.Shipments.AddNew();

			testHolders = new BusinessObjectSynchroniserList();
			testSync = new CusContainerSynchroniser(container, consolContainer);
		}
	}
}
