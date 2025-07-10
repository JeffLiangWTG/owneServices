using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[UseSnapshotProtection]
	sealed class ContainerPackLineConcurrencyCheckTest : TestCase
	{
		public void TestRegister()
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.New<CommonConsol>();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAA";

			var shipment = consol.Shipments.AddNew();

			AssertEquals("concurrency check has been not been registered yet", false, ContainerPackLineConcurrencyCheck.IsRegistered(factory));

			var packLine = shipment.OuterPackLines.AddNew();

			AssertContainsExactElementsInAnyOrder("packline packed into container AAA", new[] { container }, packLine.Containers);
			AssertContainsExactElementsInAnyOrder("packline packed into container AAA", new[] { packLine }, container.PackLines);

			AssertEquals("concurrency check has been registered", true, ContainerPackLineConcurrencyCheck.IsRegistered(factory));

			factory.Save();

			AssertEquals("concurrency check has not been unregistered", true, ContainerPackLineConcurrencyCheck.IsRegistered(factory));
		}

		public void TestContainerPackLineConcurrency_PackedIntoDifferentContainer()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var consol = factory1.New<CommonConsol>();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBB";

			var shipment = consol.Shipments.AddNew();

			var packLine = shipment.OuterPackLines.AddNew();

			packLine.SetContainer(consol, null);

			AssertContainsExactElementsInAnyOrder("packline is not packed", System.Array.Empty<CommonContainer>(), packLine.Containers);
			AssertContainsExactElementsInAnyOrder("no packlines in container AAA", System.Array.Empty<PackLine>(), container1.PackLines);
			AssertContainsExactElementsInAnyOrder("no packlines in container BBB", System.Array.Empty<PackLine>(), container2.PackLines);

			factory1.Save();

			packLine.SetContainer(consol, container1);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consol_factory2 = factory2.Load<CommonConsol>(consol.PK);

			var container1_factory2 = factory2.Load<CommonContainer>(container1.PK);
			var container2_factory2 = factory2.Load<CommonContainer>(container2.PK);

			var packLine_factory2 = factory2.Load<PackLine>(packLine.PK);

			packLine_factory2.SetContainer(consol_factory2, container2_factory2);

			AssertContainsExactElementsInAnyOrder("packline packed into container BBB", new[] { "BBB" }, FormatContainers(packLine_factory2.Containers));
			AssertContainsExactElementsInAnyOrder("no packlines in container AAA", System.Array.Empty<PackLine>(), container1_factory2.PackLines);
			AssertContainsExactElementsInAnyOrder("packline packed into container BBB", new[] { packLine_factory2 }, container2_factory2.PackLines);

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("packlines <-> containers concurrency check should kick in");
			}
			catch (ZCannotSaveException ex)
			{
				AssertEquals("Another user has made changes to the packing that conflict with your own changes.", ex.Message);
				AssertEquals("Exception should be marked as retriable", true, ex.ShouldReprocess);
			}

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;

			var container1_factory3 = factory3.Load<CommonContainer>(container1.PK);
			var container2_factory3 = factory3.Load<CommonContainer>(container2.PK);

			var packLine_factory3 = factory3.Load<PackLine>(packLine.PK);

			AssertContainsExactElementsInAnyOrder("packline remains packed only into container AAA", new[] { "AAA" }, FormatContainers(packLine_factory3.Containers));
			AssertContainsExactElementsInAnyOrder("packline packed into container AAA", new[] { packLine_factory3 }, container1_factory3.PackLines);
			AssertContainsExactElementsInAnyOrder("no packlines in container BBB", System.Array.Empty<PackLine>(), container2_factory3.PackLines);
		}

		public void TestContainerPackLineConcurrency_RePackedIntoDifferentContainer()
		{
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var consol = factory1.New<CommonConsol>();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAA";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBB";

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CCC";

			var shipment = consol.Shipments.AddNew();

			var packLine = shipment.OuterPackLines.AddNew();

			packLine.SetContainer(consol, container1);

			AssertContainsExactElementsInAnyOrder("packline packed into container AAA", new[] { "AAA" }, FormatContainers(packLine.Containers));
			AssertContainsExactElementsInAnyOrder("packline packed into container AAA", new[] { packLine }, container1.PackLines);
			AssertContainsExactElementsInAnyOrder("no packlines in container BBB", System.Array.Empty<PackLine>(), container2.PackLines);
			AssertContainsExactElementsInAnyOrder("no packlines in container CCC", System.Array.Empty<PackLine>(), container3.PackLines);

			factory1.Save();

			packLine.SetContainer(consol, container2);

			AssertContainsExactElementsInAnyOrder("packline packed into container BBB", new[] { "BBB" }, FormatContainers(packLine.Containers));
			AssertContainsExactElementsInAnyOrder("no packlines in container AAA", System.Array.Empty<PackLine>(), container1.PackLines);
			AssertContainsExactElementsInAnyOrder("packline packed into container BBB", new[] { packLine }, container2.PackLines);
			AssertContainsExactElementsInAnyOrder("no packlines in container CCC", System.Array.Empty<PackLine>(), container3.PackLines);

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consol_factory2 = factory2.Load<CommonConsol>(consol.PK);

			var container1_factory2 = factory2.Load<CommonContainer>(container1.PK);
			var container2_factory2 = factory2.Load<CommonContainer>(container2.PK);
			var container3_factory2 = factory2.Load<CommonContainer>(container3.PK);

			var packLine_factory2 = factory2.Load<PackLine>(packLine.PK);

			packLine_factory2.SetContainer(consol_factory2, container3_factory2);

			AssertContainsExactElementsInAnyOrder("packline packed into container CCC", new[] { "CCC" }, FormatContainers(packLine_factory2.Containers));
			AssertContainsExactElementsInAnyOrder("no packlines in container AAA", System.Array.Empty<PackLine>(), container1_factory2.PackLines);
			AssertContainsExactElementsInAnyOrder("no packlines in container BBB", System.Array.Empty<PackLine>(), container1_factory2.PackLines);
			AssertContainsExactElementsInAnyOrder("packline packed into container CCC", new[] { packLine_factory2 }, container3_factory2.PackLines);

			factory1.Save();

			string concurrencyExceptionMessage = string.Empty;

			try
			{
				factory2.Save();
				Fail("JobContainerPackPivot deleted concurrency error");
			}
			catch (ZSaveConcurrencyException ex)
			{
				concurrencyExceptionMessage = ex.InnerException.Message;

				ZExceptionReporting.HandleSaveException(ex);
			}

			try
			{
				factory2.Save();
				Fail("JobContainerPackPivot deleted concurrency error is not resolved so factory will never save");
			}
			catch (ZCannotSaveException)
			{
				Assert(true);
			}

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;

			var container1_factory3 = factory3.Load<CommonContainer>(container1.PK);
			var container2_factory3 = factory3.Load<CommonContainer>(container2.PK);
			var container3_factory3 = factory3.Load<CommonContainer>(container3.PK);

			var packLine_factory3 = factory3.Load<PackLine>(packLine.PK);

			AssertContainsExactElementsInAnyOrder("packline remains packed only into container BBB", new[] { "BBB" }, FormatContainers(packLine_factory3.Containers));
			AssertContainsExactElementsInAnyOrder("no packlines in container AAA", System.Array.Empty<PackLine>(), container1_factory3.PackLines);
			AssertContainsExactElementsInAnyOrder("packline packed into container BBB", new[] { packLine_factory3 }, container2_factory3.PackLines);
			AssertContainsExactElementsInAnyOrder("no packlines in container CCC", System.Array.Empty<PackLine>(), container3_factory3.PackLines);
		}

		public void TestContainerPackLineConcurrency_MissingJobConShipLink()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();

			factory1.RefreshEnabled = false;
			factory2.RefreshEnabled = false;

			var consol1 = factory1.New<CommonConsol>();
			var shipment1 = factory1.New<CommonShipment>();
			var container1 = factory1.New<CommonContainer>();

			consol1.JK_UniqueConsignRef = "C1001";
			shipment1.JS_UniqueConsignRef = "S1001";
			container1.JC_ContainerNum = "CCCC1001";

			consol1.Shipments.Add(shipment1);
			consol1.Containers.Add(container1);

			factory1.Save();

			var shipment2 = factory2.Load<CommonShipment>(shipment1.PK);
			var packline2 = shipment2.OuterPackLines.AddNew();

			consol1.Shipments.Remove(shipment1);

			factory1.Save();

			packline2.SetContainer(container1.PK);

			try
			{
				factory2.Save();
				Fail("Should have missing JobConShipLink concurrency error");
			}
			catch (ZConcurrencyCheckFailureException ex)
			{
				AssertEquals(@"Another user has made changes that conflict with your own changes.
Shipment 'S1001' has Packs allocated to Container 'CCCC1001' but the Shipment has been removed from the Container's parent Consol 'C1001'.", ex.Message);
			}

			var factory3 = new BusinessObjectFactory();
			factory3.RefreshEnabled = false;

			var consol3 = factory3.Load<CommonConsol>(consol1.PK);
			var shipment3 = factory3.Load<CommonShipment>(shipment1.PK);

			AssertEquals("Container should be attached to consol.", container1.PK, consol3.Containers[0].PK);
			AssertEquals("Shipment isn't attached to consol.", 0, consol3.Shipments.Count);
			AssertEquals("Shipment shouldn't have packlines", 0, shipment3.OuterPackLines.Count);
		}

		public void TestContainerPackLineConcurrency_WhenPackingPackLineIntoContainerFromAWBMasterConsol()
		{
			var factory = new BusinessObjectFactory();

			var shipment1 = factory.New<CommonShipment>();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			var packLine = shipment1.OuterPackLines.AddNew();

			var awbColoadConsol = factory.New<CommonConsol>();
			awbColoadConsol.JK_AgentType = Core.Constants.AgentType.AWBCoload;
			awbColoadConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			awbColoadConsol.Shipments.Add(shipment1);

			var awbMasterConsol = factory.New<CommonConsol>();
			awbMasterConsol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
			awbMasterConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			awbColoadConsol.JK_JK_MasterConsol = awbMasterConsol.PK;

			var container1 = factory.New<CommonContainer>();
			awbMasterConsol.Containers.Add(container1);

			packLine.SetContainer(container1.PK);

			AssertNoExceptionThrown(factory.Save);
		}

		string[] FormatContainers(CommonContainerManyToManyCollection collection)
		{
			return collection.Cast<CommonContainer>()
				.Select(container => container.JC_ContainerNum.ToString())
				.ToArray();
		}
	}
}
