using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class OneStopContainerEventRequestLineTestCase : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "Container1";
			OneStopContainerEventRequest request = OneStopContainerEventRequest.New(container);
			AssertEquals(typeof(OneStopContainerEventRequest), request.GetType());
		}

		public void TestPortOfLoading()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			OneStopContainerEventRequest request = OneStopContainerEventRequest.New(container);
			AssertEquals("No consol or declaration, empty port of loading expected", ZString.Empty, request.PortOfLoading);

			BusinessObject cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_RL_NKOrigin] = "GBLON";
			cusContainer[CusContainerSchema.CO_JC] = container.PK;
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			AssertEquals("Port of loading expected to come from declaration", "GBLON", request.PortOfLoading);

			CommonConsol consol = Factory.New<CommonConsol>();
			container.JC_JK = consol.PK;
			consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals("Port of loading expected to come from consol", "AUSYD", request.PortOfLoading);
		}

		public void TestPortOfDischarge()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			OneStopContainerEventRequest request = OneStopContainerEventRequest.New(container);
			AssertEquals("No consol or declaration, empty port of discharge expected", ZString.Empty, request.PortOfDischarge);

			BusinessObject cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = "NLAMS";
			cusContainer[CusContainerSchema.CO_JC] = container.PK;
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			AssertEquals("Port of discharge expected to come from declaration", "NLAMS", request.PortOfDischarge);

			CommonConsol consol = Factory.New<CommonConsol>();
			container.JC_JK = consol.PK;
			consol.JK_RL_NKDischargePort = "USLAX";
			AssertEquals("Port of discharge expected to come from consol", "USLAX", request.PortOfDischarge);
		}
	}
}
