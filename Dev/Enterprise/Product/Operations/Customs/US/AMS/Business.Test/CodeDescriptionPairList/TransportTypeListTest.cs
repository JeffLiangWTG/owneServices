using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class TransportTypeListTest : TestCaseWithFactory
	{
		public void TestGetFreightTransportType()
		{
			AssertEquals("", TransportTypeList.GetFreightTransportType(TransportTypeList.Codes.Air));
			AssertEquals(Core.Constants.TransportModes.Rail, TransportTypeList.GetFreightTransportType(TransportTypeList.Codes.Rail));
			AssertEquals(Core.Constants.TransportModes.Sea, TransportTypeList.GetFreightTransportType(TransportTypeList.Codes.VesselNonContainer));
			AssertEquals(Core.Constants.TransportModes.Sea, TransportTypeList.GetFreightTransportType(TransportTypeList.Codes.VesselContainer));
		}

		public void TestGetConveyanceTransportTypeList()
		{
			var list = TransportTypeList.GetConveyanceTransportTypeList(Factory);
			AssertEquals(3, list.Count);
			AssertEquals(TransportTypeList.Descriptions.VesselNonContainer, list.GetDescriptionFromCode(TransportTypeList.Codes.VesselNonContainer));
			AssertEquals(TransportTypeList.Descriptions.VesselContainer, list.GetDescriptionFromCode(TransportTypeList.Codes.VesselContainer));
			AssertEquals(TransportTypeList.Descriptions.Rail, list.GetDescriptionFromCode(TransportTypeList.Codes.Rail));
		}
	}
}
