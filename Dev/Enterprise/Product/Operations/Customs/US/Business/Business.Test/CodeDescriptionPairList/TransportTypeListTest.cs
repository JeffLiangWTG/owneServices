using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TransportTypeListTest : TestCaseWithFactory
	{
		public void TestIsIssuerSCACNotAllowed()
		{
			AssertEquals(true, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Auto));
			AssertEquals(true, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Pedestrian));
			AssertEquals(true, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Road));
			AssertEquals(true, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.PassengerHandCarried));

			AssertEquals(false, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Air));
			AssertEquals(false, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.BorderWaterBorne));
			AssertEquals(false, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.FixedTransportInstallations));
			AssertEquals(false, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Mail));
			AssertEquals(false, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Rail));
			AssertEquals(false, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Sea));
			AssertEquals(false, TransportTypeList.IsIssuerSCACNotAllowed(TransportTypeList.Codes.Truck));
		}

		public void TestIsPortOfDischargeMandatory()
		{
			AssertEquals(true, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Mail));
			AssertEquals(true, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.PassengerHandCarried));
			AssertEquals(true, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.FixedTransportInstallations));

			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Auto));
			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Pedestrian));
			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Road));
			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Air));
			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.BorderWaterBorne));
			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Rail));
			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Sea));
			AssertEquals(false, TransportTypeList.IsPortOfDischargeMandatory(TransportTypeList.Codes.Truck));
		}

		public void TestIsMasterBillMandatory()
		{
			AssertEquals(true, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.Air));
			AssertEquals(false, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.Auto));
			AssertEquals(true, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.BorderWaterBorne));
			AssertEquals(true, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.Rail));
			AssertEquals(false, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.Mail));
			AssertEquals(true, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.Sea));
			AssertEquals(true, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.Truck));
			AssertEquals(false, TransportTypeList.IsMasterBillSCACMandatory(TransportTypeList.Codes.PassengerHandCarried));
		}

		public void TestMasterBillRelevant()
		{
			AssertEquals(true, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.Air));
			AssertEquals(false, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.Auto));
			AssertEquals(true, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.BorderWaterBorne));
			AssertEquals(true, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.Rail));
			AssertEquals(true, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.Mail));
			AssertEquals(true, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.Sea));
			AssertEquals(true, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.Truck));
			AssertEquals(true, TransportTypeList.IsMasterBillRelevant(TransportTypeList.Codes.PassengerHandCarried));
		}

		public void TestIsBorderTransportType()
		{
			AssertEquals(false, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Air));
			AssertEquals(true, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Auto));
			AssertEquals(true, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.BorderWaterBorne));
			AssertEquals(false, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.FixedTransportInstallations));
			AssertEquals(false, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Mail));
			AssertEquals(false, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.PassengerHandCarried));
			AssertEquals(true, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Pedestrian));
			AssertEquals(true, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Rail));
			AssertEquals(true, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Road));
			AssertEquals(false, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Sea));
			AssertEquals(true, TransportTypeList.IsBorderTransportType(TransportTypeList.Codes.Truck));
			AssertEquals(false, TransportTypeList.IsBorderTransportType(""));
		}

		public void TestConvertFromTransportCode()
		{
			AssertEquals("Convert AirContainer", TransportTypeList.Codes.Air, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.AirContainer));
			AssertEquals("Convert AirNonContainer", TransportTypeList.Codes.Air, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.AirNonContainer));
			AssertEquals("Convert Auto", TransportTypeList.Codes.Auto, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.Auto));
			AssertEquals("Convert BorderWaterBorne", TransportTypeList.Codes.BorderWaterBorne, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.BorderWaterBorne));
			AssertEquals("Convert FixedTransportInstallations", TransportTypeList.Codes.FixedTransportInstallations, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.FixedTransportInstallations));
			AssertEquals("Convert Mail", TransportTypeList.Codes.Mail, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.Mail));
			AssertEquals("Convert PassengerHandCarried", TransportTypeList.Codes.PassengerHandCarried, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.PassengerHandCarried));
			AssertEquals("Convert Pedestrian", TransportTypeList.Codes.Pedestrian, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.Pedestrian));
			AssertEquals("Convert RailContainer", TransportTypeList.Codes.Rail, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.RailContainer));
			AssertEquals("Convert RailNonContainer", TransportTypeList.Codes.Rail, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.RailNonContainer));
			AssertEquals("Convert RoadOther", TransportTypeList.Codes.Road, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.RoadOther));
			AssertEquals("Convert TruckContainer", TransportTypeList.Codes.Truck, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.TruckContainer));
			AssertEquals("Convert TruckNonContainer", TransportTypeList.Codes.Truck, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.TruckNonContainer));
			AssertEquals("Convert VesselContainer", TransportTypeList.Codes.Sea, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.VesselContainer));
			AssertEquals("Convert VesselNonContainer", TransportTypeList.Codes.Sea, TransportTypeList.ConvertFromTransportCode(TransportModeCodes.Codes.VesselNonContainer));
		}

		public void TestConvertFromTransportMode()
		{
			AssertArrayEqualsByElements("Convert AirContainer", new ZString[] { TransportModeCodes.Codes.AirContainer, TransportModeCodes.Codes.AirNonContainer }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Air));
			AssertArrayEqualsByElements("Convert Auto", new ZString[] { TransportModeCodes.Codes.Auto }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Auto));
			AssertArrayEqualsByElements("Convert BorderWaterBorne", new ZString[] { TransportModeCodes.Codes.BorderWaterBorne }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.BorderWaterBorne));
			AssertArrayEqualsByElements("Convert FixedTransportInstallations", new ZString[] { TransportModeCodes.Codes.FixedTransportInstallations }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.FixedTransportInstallations));
			AssertArrayEqualsByElements("Convert Mail", new ZString[] { TransportModeCodes.Codes.Mail }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Mail));
			AssertArrayEqualsByElements("Convert PassengerHandCarried", new ZString[] { TransportModeCodes.Codes.PassengerHandCarried }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.PassengerHandCarried));
			AssertArrayEqualsByElements("Convert Pedestrian", new ZString[] { TransportModeCodes.Codes.Pedestrian }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Pedestrian));
			AssertArrayEqualsByElements("Convert RailContainer", new ZString[] { TransportModeCodes.Codes.RailContainer, TransportModeCodes.Codes.RailNonContainer }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Rail));
			AssertArrayEqualsByElements("Convert RoadOther", new ZString[] { TransportModeCodes.Codes.RoadOther }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Road));
			AssertArrayEqualsByElements("Convert TruckContainer", new ZString[] { TransportModeCodes.Codes.TruckContainer, TransportModeCodes.Codes.TruckNonContainer }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Truck));
			AssertArrayEqualsByElements("Convert VesselContainer", new ZString[] { TransportModeCodes.Codes.VesselContainer, TransportModeCodes.Codes.VesselNonContainer }, TransportTypeList.ConvertFromTransportMode(TransportTypeList.Codes.Sea));
			AssertArrayEqualsByElements("Convert Exception", System.Array.Empty<ZString>(), TransportTypeList.ConvertFromTransportMode("XXX"));
		}

		public void TestIsNonAMSBillType()
		{
			AssertEquals(true, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.Auto));
			AssertEquals(true, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.Pedestrian));
			AssertEquals(true, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.Road));
			AssertEquals(true, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.Mail));
			AssertEquals(true, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.PassengerHandCarried));
			AssertEquals(true, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.FixedTransportInstallations));
			AssertEquals(false, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.Air));
			AssertEquals(false, TransportTypeList.IsNonAMSBillType(TransportTypeList.Codes.Sea));
		}
	}
}
