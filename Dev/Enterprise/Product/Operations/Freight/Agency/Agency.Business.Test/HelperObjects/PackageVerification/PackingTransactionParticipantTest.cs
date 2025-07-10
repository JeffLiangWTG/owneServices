using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class PackingTransactionParticipantTest : TestCaseWithFactory
	{
		public void TestSaveWithoutDuplicates()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_DetailedDescription = "Line";
			line.JL_JC = ZGuid.Empty;
			line.JL_ContainerPackingOrder = 1;
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			Factory.Save();
			line.JL_JC = container1.PK;
			line.JL_ContainerPackingOrder = 1;
			BusinessObjectFactory.SaveTogether(Factory, new PackingTransactionParticipant(shipment));
			const string expected = "[Line]:[TEST4100013]\r\n";
			AssertMultilineASCIIEquals("", expected, BaseAgencyTest.ContainerPackPivots(shipment));
		}

		public void TestSaveWithoutPacklines()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			Factory.Save();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			BusinessObjectFactory.SaveTogether(Factory, new PackingTransactionParticipant(shipment));
			AssertMultilineASCIIEquals("", "", BaseAgencyTest.ContainerPackPivots(shipment));
		}

		public void TestSaveDeleted()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_DetailedDescription = "Line";
			line.JL_JC = ZGuid.Empty;
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			Factory.Save();
			shipment.Delete();
			BusinessObjectFactory.SaveTogether(Factory, new PackingTransactionParticipant(shipment));
			AssertMultilineASCIIEquals("", "", BaseAgencyTest.ContainerPackPivots(shipment));
		}
	}
}
