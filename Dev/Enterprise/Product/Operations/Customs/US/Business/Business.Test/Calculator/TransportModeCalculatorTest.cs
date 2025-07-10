using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TransportModeCalculatorTest : TestCaseWithFactory
	{
		public void TestJE_Calc_USTransportMode()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.AirContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.Auto, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.BorderWaterBorne, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.FixedTransportInstallations, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.Mail, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.PassengerHandCarried, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.Pedestrian, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.RailContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.RoadOther, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.VesselContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.TruckContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = "ZZZ";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			AssertEquals("IsContainerised", true, declaration.IsContainerised);
			AssertEquals("", declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.AirNonContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.Auto, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.BorderWaterBorne;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.BorderWaterBorne, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.FixedTransportInstallations, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.Mail, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.PassengerHandCarried, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.Pedestrian, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.RailNonContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.RoadOther, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.VesselNonContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals(TransportModeCodes.Codes.TruckNonContainer, declaration.JE_Calc_USTransportMode);

			declaration.JE_TransportMode = "ZZZ";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("IsContainerised", false, declaration.IsContainerised);
			AssertEquals("", declaration.JE_Calc_USTransportMode);
		}
	}
}
