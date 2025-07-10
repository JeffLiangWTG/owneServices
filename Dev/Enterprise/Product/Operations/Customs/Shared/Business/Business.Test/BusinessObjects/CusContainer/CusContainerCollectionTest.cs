using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusContainerCollection<BaseCusContainer>))]
	public class CusContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestReadOnly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			jobDeclaration.JE_JS = shipment.PK;
			jobDeclaration.JE_OverrideFreightDefaults = true;
			var containers = jobDeclaration.CusContainers;
			Assert("Should be false when the synchronisation from shipment is inactive.", !containers.ReadOnly);
			AssertEquals("Should be same as the ShouldSynchroniseWithShipment of Declaration", jobDeclaration.ShouldSynchroniseWithShipment(), containers.ReadOnly);
			jobDeclaration.JE_OverrideFreightDefaults = false;
			Assert("Should be true when the synchronisation from shipment is active.", containers.ReadOnly);
			AssertEquals("Should be same as the ShouldSynchroniseWithShipment of Declaration", jobDeclaration.ShouldSynchroniseWithShipment(), containers.ReadOnly);
			var message = jobDeclaration.Messages.AddNew();
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			AssertEquals("Should be same as the ShouldSynchroniseWithShipment of Declaration", jobDeclaration.ShouldSynchroniseWithShipment(), containers.ReadOnly);
			message.EM_Status = EDIMessage.Status.Discarded;
			AssertEquals("Should be same as the ShouldSynchroniseWithShipment of Declaration", jobDeclaration.ShouldSynchroniseWithShipment(), containers.ReadOnly);
			jobDeclaration.JE_JS = ZGuid.Empty;
			Assert("Should be false when the relevant shipment is null.", !containers.ReadOnly);
			AssertEquals("Should be same as the ShouldSynchroniseWithShipment of Declaration", jobDeclaration.ShouldSynchroniseWithShipment(), containers.ReadOnly);
		}

		public void TestTotalGoodsWeight()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var containers = declaration.CusContainers;
			var container1 = containers.AddNew();
			container1.CO_Weight = 1.22m;
			var container2 = containers.AddNew();
			container2.CO_Weight = 3.54m;
			AssertEquals("Containers.TotalGoodsWeight", 4.76m, containers.TotalGoodsWeight);
		}
		public void TestGetElementWithoutPackingGroups()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.Bills.AddNew();
			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();
			container1.PackingGroups.AddNew();
			AssertEquals(container2, declaration.CusContainers.GetElementWithoutPackingGroups());

			container2.PackingGroups.AddNew();
			AssertNull(declaration.CusContainers.GetElementWithoutPackingGroups());
		}

		public virtual void TestCountChanged()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_ContainerMode = "";
			var collection = new BaseCusContainerCollection<BaseCusContainer>(declaration, Factory);
			collection.AddNew();
			AssertEquals(Core.Constants.ContainerModes.Containerised, declaration.JE_ContainerMode);
		}

		public void TestCusContainerCollection()
		{
			var aDeclaration = BaseJobDeclaration.New(Factory);
			AssertNotNull("Declaration is created", aDeclaration);

			var aCollection = aDeclaration.CusContainers;
			AssertNotNull("Failed to create Collection", aCollection);

			aCollection.AddNew();
			AssertNotNull("Failed to add new container", aCollection[0]);
		}

		public void TestDoDefaultSort()
		{
			var aDeclaration = BaseJobDeclaration.New(Factory);
			var aCollection = new BaseCusContainerCollection<BaseCusContainer>(aDeclaration, Factory);
			var container4 = aCollection.AddNew();
			container4.CO_ContainerNumber = "2";
			container4.CO_FCL_LCL_AIR = "LCL";

			var container1 = aCollection.AddNew();
			container1.CO_ContainerNumber = "1";

			var container3 = aCollection.AddNew();
			container3.CO_ContainerNumber = "2";
			container3.CO_FCL_LCL_AIR = "FCL";

			var container2 = aCollection.AddNew();
			container2.CO_ContainerNumber = "2";

			AssertEquals("PreCondition : Container1", container4, aCollection[0]);
			AssertEquals("PreCondition : Container2", container1, aCollection[1]);
			AssertEquals("PreCondition : Container3", container3, aCollection[2]);
			AssertEquals("PreCondition : Container4", container2, aCollection[3]);

			aCollection.DoDefaultSort();

			AssertEquals("Container1", container1, aCollection[0]);
			AssertEquals("Container2", container2, aCollection[1]);
			AssertEquals("Container3", container3, aCollection[2]);
			AssertEquals("Container4", container4, aCollection[3]);
		}

		public void TestFind()
		{
			var aDeclaration = BaseJobDeclaration.New(Factory);
			var aCollection = new BaseCusContainerCollection<BaseCusContainer>(aDeclaration, Factory);
			var container1 = aCollection.AddNew();
			container1.CO_ContainerNumber = TestContainerNumber1;

			var container2 = aCollection.AddNew();
			container2.CO_ContainerNumber = TestContainerNumber2;

			var container3 = aCollection.AddNew();
			container3.CO_ContainerNumber = TestContainerNumber3;

			var container4 = aCollection.AddNew();
			container4.CO_ContainerNumber = TestContainerNumber4;

			AssertEquals("Container 1", container1, aCollection.Find(TestContainerNumber1));
			AssertEquals("Container 2", container2, aCollection.Find(TestContainerNumber2));

			aCollection.Remove(container2);

			AssertEquals("Container 2 removed", null, aCollection.Find(TestContainerNumber2));
			AssertEquals("Container 3", container3, aCollection.Find(TestContainerNumber3));
			AssertEquals("Container 4", container4, aCollection.Find(TestContainerNumber4));
		}

		public void TestHasContainerModeOf()
		{
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();
			BaseCusContainer container3 = declaration.CusContainers.AddNew();

			container1.CO_FCL_LCL_AIR = "FCL";
			container2.CO_FCL_LCL_AIR = "LCL";
			container3.CO_FCL_LCL_AIR = "LCL";

			AssertEquals(true, declaration.CusContainers.HasContainerModeOf("FCL"));
			AssertEquals(true, declaration.CusContainers.HasContainerModeOf("LCL"));
			AssertEquals(false, declaration.CusContainers.HasContainerModeOf("FCG"));
		}

		public void TestAddContainerAddsToConsol()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			BaseCusContainer container = declaration.CusContainers.AddNew();
			CommonContainer freightContainer = container.JobContainer;
			AssertEquals(true, consol.Containers.Contains(freightContainer));
		}

		public void TestFindOrCreate()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			declaration.CusContainers.FindOrCreate("1");

			AssertEquals(1, declaration.CusContainers.Count);
			AssertEquals("1", declaration.CusContainers[0].CO_ContainerNumber);

			declaration.CusContainers.FindOrCreate("1");
			AssertEquals(1, declaration.CusContainers.Count);

			declaration.CusContainers.FindOrCreate("2");
			AssertEquals(2, declaration.CusContainers.Count);
		}

		#region Implementation

		const string TestContainerNumber1 = "GALU0192039";
		const string TestContainerNumber2 = "TREU3839209";
		const string TestContainerNumber3 = "WERU3738493";
		const string TestContainerNumber4 = "CXCU9303039";

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			BaseJobDeclaration aDeclaration = BaseJobDeclaration.New(Factory);
			return new BaseCusContainerCollection<BaseCusContainer>(aDeclaration, Factory);
		}

		protected virtual BaseJobDeclaration GetDeclarationToTestAddingNewPackingRecord()
		{
			return BaseJobDeclaration.New(Factory);
		}

		#endregion
	}
}
