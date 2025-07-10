using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using USDeclaration = Enterprise.Customs.US.Business.JobDeclaration;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	internal class USFreightWrapperFromDeclarationTest : FreightWrapperFromDeclarationTest
	{
		public void TestTransportModeWorksForUSSpecificCodes()
		{
			USDeclaration declaration = Factory.New<USDeclaration>();
			declaration.JE_TransportMode = Enterprise.Customs.US.Business.TransportTypeList.Codes.BorderWaterBorne;
			FreightWrapper wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("wrapper.ShipmentTransportMode.CodeAndDescription", "SEA - Sea", wrapper.ShipmentTransportMode.CodeAndDescription);
		}

		public void TestDescriptionsAreShortDescriptionsForTransportModes()
		{
			USDeclaration declaration = Factory.New<USDeclaration>();
			declaration.JE_TransportMode = Enterprise.Customs.US.Business.TransportTypeList.Codes.Air;
			FreightWrapper wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("wrapper.ShipmentTransportMode.CodeAndDescription", "AIR - Air", wrapper.ShipmentTransportMode.CodeAndDescription);
		}

		public void TestTransportModeOtherReturnsEmptyDescription()
		{
			USDeclaration declaration = Factory.New<USDeclaration>();
			declaration.JE_TransportMode = Enterprise.Customs.US.Business.TransportTypeList.Codes.FixedTransportInstallations;
			FreightWrapper wrapper = FreightWrapper.New(declaration, Factory)[0];
			AssertEquals("Precondition: wrapper.ShipmentTransportMode.Code", "", wrapper.ShipmentTransportMode.Code);
			AssertEquals("wrapper.ShipmentTransportMode.Description", "Unknown", wrapper.ShipmentTransportMode.Description);
		}
	}
}
