using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class UnsafePackingTransactionParticipantTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSaveWithDuplicates()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			AgencyShipment shipment = factory.New<AgencyShipment>();
			AgencyShipmentPackLine line = shipment.OuterPackLines.AddNew();
			line.JL_DetailedDescription = "Line";
			line.JL_JC = ZGuid.Empty;
			line.JL_ContainerPackingOrder = 1;
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerNum = "TEST4100013";
			container1.JC_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_ContainerNum = "TEST4100029";
			container2.JC_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			factory.Save();
			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			using (TestCaseWithFactory.GetFactoryIsolater(factory))
			using (TestCaseWithFactory.GetFactoryIsolater(otherFactory))
			{
				AgencyShipmentPackLine lineInOtherFactory = otherFactory.Load<AgencyShipmentPackLine>(line.PK);
				lineInOtherFactory.JL_JC = container2.PK;
				lineInOtherFactory.JL_ContainerPackingOrder = 1;
				otherFactory.Save();
			}

			line.JL_JC = container1.PK;
			line.JL_ContainerPackingOrder = 1;
			try
			{
				BusinessObjectFactory.SaveTogether(factory, new PackingTransactionParticipant(shipment));
				Fail("should have thrown a PackedIntoMultipleContainersException");
			}
			catch (PackedIntoMultipleContainersException)
			{
			}

			const string expected = "[Line]:[TEST4100029]\r\n";
			AssertMultilineASCIIEquals("", expected, BaseAgencyTest.ContainerPackPivots(shipment));
		}
	}
}
