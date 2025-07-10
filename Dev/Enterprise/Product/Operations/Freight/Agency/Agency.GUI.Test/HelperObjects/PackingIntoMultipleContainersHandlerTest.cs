using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class PackingIntoMultipleContainersHandlerTest : TestCaseWithFactory
	{
		public void TestHandle_MultipleContainersException()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			AgencyShipmentPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_Description = "Line";
			line.Containers.Add(container1);
			line.Containers.Add(container2);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("Precondition:", (c) => c.JC_ContainerCode, new AgencyShipmentContainer[] { container1, container2 }, line.Containers.ToArray<AgencyShipmentContainer>());
			PackedIntoMultipleContainersHandler.Handle(shipment);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("Should have resolved the duplication.", (c) => c.JC_ContainerCode, new AgencyShipmentContainer[] { container1 }, line.Containers.ToArray<AgencyShipmentContainer>());
				AssertMultilineASCIIEquals("Correct dialog shown", "Information Another user has made changes to the packing that conflicts with your own changes. Please verify that the packing is correct and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertHasWarning(line.JL_JCInfo, "Another user has set this line as packed into 'TEST4100029'.");
			});
		}

		public void TestHandle_NoActualDuplicate()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_Description = "Line";
			line.Containers.Add(container1);
			Factory.Save();
			AssertContainsExactElementsInAnyOrder("Precondition:", (c) => c.JC_ContainerCode, new AgencyShipmentContainer[] { container1 }, line.Containers.ToArray<AgencyShipmentContainer>());
			PackedIntoMultipleContainersHandler.Handle(shipment);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder("Should not have mangled the packing.", (c) => c.JC_ContainerCode, new AgencyShipmentContainer[] { container1 }, line.Containers.ToArray<AgencyShipmentContainer>());
				AssertMultilineASCIIEquals("Correct dialog shown", "Information Another user has made changes to the packing that conflicts with your own changes. Please verify that the packing is correct and try again.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertNoWarnings(line.JL_JCInfo);
			});
		}
	}
}
