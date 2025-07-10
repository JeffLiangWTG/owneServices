using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentContainerMatcherTest : TestCaseWithFactory
	{
		public void TestNoMatch()
		{
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "MBL001", "S001");
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST0000016", "20FR");
			Factory.Save();
			var references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("TEST4100021", "22P1", "", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			var matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			AssertEquals(null, matcher.GetBestMatch());
			references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("TEST0000016", "22P1", "", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			AssertEquals(container2, matcher.GetBestMatch());
			var container3 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST0000016", "20FR");
			Factory.Save();
			AssertEquals(null, matcher.GetBestMatch());
		}

		public void TestNoMatch_NonFCLContainer()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100021", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "MBL001", "S001", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			var references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("TEST4100021", "22G0", "", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			var matcher = new AgencyShipmentContainerMatcher(container.Booking, references);
			AssertEquals(null, matcher.GetBestMatch());
		}

		public void TestMatchByContainerNumber()
		{
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "MBL001", "S001");
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST0000016", "20GP");
			var container3 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST4100021", "20FR");
			Factory.Save();
			var references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("TEST4100021", "22G0", "", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			var matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			var bestMatch = matcher.GetBestMatch();
			AssertEquals(container3, bestMatch);
		}

		public void TestMatchByReleaseNumber()
		{
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "RN1", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "MBL001", "S001");
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST0000016", "20GP", "RN2");
			var container3 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST4100021", "20FR", "RN3");
			Factory.Save();
			var references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("", "22G0", "RN3", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			var matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			var bestMatch = matcher.GetBestMatch();
			AssertEquals(container3, bestMatch);
		}

		public void TestMatchByISOCode()
		{
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "MBL001", "S001");
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST0000016", "20GP");
			var container3 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST4100013", "20FR");
			Factory.Save();
			var references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("TEST4100013", "22P1", "", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			var matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			var bestMatch = matcher.GetBestMatch();
			AssertEquals(container3, bestMatch);
		}

		public void TestBestMach()
		{
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "", "AUSYD", "USSFO", "USS ESSES", "001", "12345", "MBL001", "S001");
			var container2 = container1.Booking.ShippingContainers.AddNew();
			container2.JC_ReleaseNum = "RN2";
			Factory.Save();
			var references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("", "22P1", "RN2", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			var matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			AssertEquals(container2, matcher.GetBestMatch());
			references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("TEST4100013", "22P1", "RN2", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			AssertEquals(container1, matcher.GetBestMatch());
			var container3 = container1.Booking.ShippingContainers.AddNew();
			container3.JC_ReleaseNum = "RN2";
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20FR").PK;
			Factory.Save();
			references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("", "22P1", "RN2", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			AssertEquals(container3, matcher.GetBestMatch());
			var container4 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container1.Booking, "TEST4100013", "20FR", "");
			Factory.Save();
			references = AgencyShipmentContainerUniversalTestHelper.CreateReferences("TEST4100013", "22P1", "", "MBL001", new SailingReference { VesselName = "USS ESSES", VoyageNumber = "001" });
			matcher = new AgencyShipmentContainerMatcher(container1.Booking, references);
			AssertEquals(container4, matcher.GetBestMatch());
		}
	}
}
