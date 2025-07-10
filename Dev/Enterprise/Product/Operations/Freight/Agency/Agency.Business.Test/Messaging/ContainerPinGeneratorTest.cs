using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ContainerPinGeneratorTest : BaseAgencyTest
	{
		public void TestDefaults()
		{
			ContainerPinGenerator generator = new ContainerPinGenerator();
			AssertEquals("PinLength", 8, generator.PinLength);
			AssertEquals("EmptyContainersShareAPin", true, generator.EmptyContainersShareAPin);
		}

		public void TestNewPin()
		{
			Regex patternLength8 = new Regex("^[" + ContainerPinGenerator.Alphabet + "]{8}$");
			Regex patternLength10 = new Regex("^[" + ContainerPinGenerator.Alphabet + ContainerPinGenerator.Numbers + "]{10}$");

			string pin1, pin2, pin3;
			ContainerPinGenerator generator = new ContainerPinGenerator();

			generator.PinLength = 8;
			generator.CharacterType = ContainerPinGenerator.CharacterTypes.Alpha;
			AssertMatch("pin1", patternLength8, pin1 = generator.NewPin());
			AssertMatch("pin2", patternLength8, pin2 = generator.NewPin());
			AssertMatch("pin3", patternLength8, pin3 = generator.NewPin());
			AssertNotEquals("pin1 != pin2", pin1, pin2);
			AssertNotEquals("pin2 != pin3", pin2, pin3);
			AssertNotEquals("pin3 != pin1", pin3, pin1);

			generator.PinLength = 10;
			AssertMatch("pin1", patternLength10, pin1 = generator.NewPin());
			AssertMatch("pin2", patternLength10, pin2 = generator.NewPin());
			AssertMatch("pin3", patternLength10, pin3 = generator.NewPin());
			AssertNotEquals("pin1 != pin2", pin1, pin2);
			AssertNotEquals("pin2 != pin3", pin2, pin3);
			AssertNotEquals("pin3 != pin1", pin3, pin1);
		}

		public void TestPopulateEmptyPins_WithEmptyShare()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_IsEmptyContainer = false;
			container1.JC_ContainerImportDORelease = "";

			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_IsEmptyContainer = false;
			container2.JC_ContainerImportDORelease = "";

			AgencyShipmentContainer container3 = shipment.RealContainers.AddNew();
			container3.JC_IsEmptyContainer = true;
			container3.JC_ContainerImportDORelease = "";

			AgencyShipmentContainer container4 = shipment.RealContainers.AddNew();
			container4.JC_IsEmptyContainer = true;
			container4.JC_ContainerImportDORelease = "";

			ContainerPinGenerator generator = new ContainerPinGenerator();
			generator.PinLength = 8;
			generator.EmptyContainersShareAPin = true;

			AgencyShipmentContainer[] containers = shipment.RealContainers.ToArray<AgencyShipmentContainer>();

			generator.PopulateEmptyPins(containers);

			foreach (AgencyShipmentContainer container in containers)
			{
				AssertNotEquals("", container.JC_ContainerImportDORelease);
			}

			AssertNotEquals("non-empty containers should not match", container1.JC_ContainerImportDORelease, container2.JC_ContainerImportDORelease);
			AssertEquals("empty-containers should match", container3.JC_ContainerImportDORelease, container4.JC_ContainerImportDORelease);

			string[] pins = Array.ConvertAll(containers, (c) => c.JC_ContainerImportDORelease.ToString());
			generator.PopulateEmptyPins(containers);

			AssertArrayEqualsByElements("pins should not have changed", pins, Array.ConvertAll(containers, (c) => c.JC_ContainerImportDORelease.ToString()));

			generator.PinLength = 6;
			generator.PopulateEmptyPins(containers);

			Assert("All PINs regenerated to match new format", containers.All(c => c.JC_ContainerImportDORelease.Length == 6));
		}

		public void TestPopulateEmptyPins_WithoutEmptyShare()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();

			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			container1.JC_IsEmptyContainer = false;
			container1.JC_ContainerImportDORelease = "";

			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			container2.JC_IsEmptyContainer = false;
			container2.JC_ContainerImportDORelease = "";

			AgencyShipmentContainer container3 = shipment.RealContainers.AddNew();
			container3.JC_IsEmptyContainer = true;
			container3.JC_ContainerImportDORelease = "";

			AgencyShipmentContainer container4 = shipment.RealContainers.AddNew();
			container4.JC_IsEmptyContainer = true;
			container4.JC_ContainerImportDORelease = "";

			ContainerPinGenerator generator = new ContainerPinGenerator();
			generator.PinLength = 8;
			generator.EmptyContainersShareAPin = false;

			AgencyShipmentContainer[] containers = shipment.RealContainers.ToArray<AgencyShipmentContainer>();

			generator.PopulateEmptyPins(containers);

			foreach (AgencyShipmentContainer container in containers)
			{
				AssertNotEquals("", container.JC_ContainerImportDORelease);
			}

			AssertNotEquals("non-empty containers should not match", container1.JC_ContainerImportDORelease, container2.JC_ContainerImportDORelease);
			AssertNotEquals("empty-containers should not match", container3.JC_ContainerImportDORelease, container4.JC_ContainerImportDORelease);

			string[] pins = Array.ConvertAll(containers, (c) => c.JC_ContainerImportDORelease.ToString());
			generator.PopulateEmptyPins(containers);

			AssertArrayEqualsByElements("pins should not have changed", pins, Array.ConvertAll(containers, (c) => c.JC_ContainerImportDORelease.ToString()));

			generator.PinLength = 6;
			generator.PopulateEmptyPins(containers);

			Assert("All PINs regenerated to match new format", containers.All(c => c.JC_ContainerImportDORelease.Length == 6));
		}
	}
}
