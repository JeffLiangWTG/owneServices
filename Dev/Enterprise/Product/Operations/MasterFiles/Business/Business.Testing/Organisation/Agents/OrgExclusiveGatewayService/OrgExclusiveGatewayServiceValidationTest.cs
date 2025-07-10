using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgExclusiveGatewayServiceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestO7_RS_NKGatewayServiceValidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var agentPort = header.AppointedGatewayAgentPorts.AddNew();
			var exclusiveGatewayService = agentPort.ExclusiveGatewayServices.AddNew();

			exclusiveGatewayService.Validation.ValidateO7_RS_NKGatewayService();
			Assert(exclusiveGatewayService.O7_RS_NKGatewayServiceInfo.HasNotification("Please enter an Exclusive Gateway Service.")); // MandatoryValidation

			exclusiveGatewayService.O7_RS_NKGatewayService = "PAI";
			exclusiveGatewayService.Validation.ValidateO7_RS_NKGatewayService();
			Assert(exclusiveGatewayService.O7_RS_NKGatewayServiceInfo.HasNotification("Enter a valid Exclusive Gateway Service.")); // ListValidation - wrong code

			var slSTD = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
			Assert(!slSTD.RS_IsGateway);
			exclusiveGatewayService.O7_RS_NKGatewayService = "STD";
			exclusiveGatewayService.Validation.ValidateO7_RS_NKGatewayService();
			Assert(exclusiveGatewayService.O7_RS_NKGatewayServiceInfo.HasNotification("Enter a valid Exclusive Gateway Service.")); // ListValidation - non-gateway service level

			var slGateway = Factory.NewWithValidTestData<RefServiceLevel>();
			slGateway.RS_Code = "GW1";
			slGateway.RS_IsGateway = true;
			exclusiveGatewayService.O7_RS_NKGatewayService = "GW1";
			exclusiveGatewayService.Validation.ValidateO7_RS_NKGatewayService();
			Assert(!exclusiveGatewayService.O7_RS_NKGatewayServiceInfo.HasErrors());
		}

		public void TestO7_RS_NKShipmentServiceLevelValidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var agentPort = header.AppointedGatewayAgentPorts.AddNew();
			var exclusiveGatewayService = agentPort.ExclusiveGatewayServices.AddNew();

			exclusiveGatewayService.Validation.ValidateO7_RS_NKShipmentServiceLevel();
			Assert(exclusiveGatewayService.O7_RS_NKShipmentServiceLevelInfo.HasNotification("Please enter a Supported Shipment G/W Service Level.")); // MandatoryValidation

			exclusiveGatewayService.O7_RS_NKShipmentServiceLevel = "PAI";
			exclusiveGatewayService.Validation.ValidateO7_RS_NKShipmentServiceLevel();
			Assert(exclusiveGatewayService.O7_RS_NKShipmentServiceLevelInfo.HasNotification("Enter a valid Supported Shipment G/W Service Level.")); // ListValidation - wrong code

			var slSTD = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
			Assert(!slSTD.RS_IsGateway);
			exclusiveGatewayService.O7_RS_NKShipmentServiceLevel = "STD";
			exclusiveGatewayService.Validation.ValidateO7_RS_NKShipmentServiceLevel();
			Assert(exclusiveGatewayService.O7_RS_NKShipmentServiceLevelInfo.HasNotification("Enter a valid Supported Shipment G/W Service Level.")); // ListValidation - non-gateway service level

			var slGateway = Factory.NewWithValidTestData<RefServiceLevel>();
			slGateway.RS_Code = "GW1";
			slGateway.RS_IsGateway = true;
			exclusiveGatewayService.O7_RS_NKShipmentServiceLevel = "GW1";
			exclusiveGatewayService.Validation.ValidateO7_RS_NKShipmentServiceLevel();
			Assert(!exclusiveGatewayService.O7_RS_NKShipmentServiceLevelInfo.HasErrors());
		}

		public void TestDuplicated()
		{
			var sl1 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl1.RS_Code = "GW1";
			sl1.RS_IsGateway = true;

			var sl2 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl2.RS_Code = "GW2";
			sl2.RS_IsGateway = true;

			var sl3 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl3.RS_Code = "GW3";
			sl3.RS_IsGateway = true;

			var header = Factory.NewWithValidTestData<OrgHeader>();
			var agentPort = header.AppointedGatewayAgentPorts.AddNew();
			var exclusiveGatewayService1 = agentPort.ExclusiveGatewayServices.AddNew();
			var exclusiveGatewayService2 = agentPort.ExclusiveGatewayServices.AddNew();

			exclusiveGatewayService1.O7_RS_NKGatewayService = "GW2";
			exclusiveGatewayService1.O7_RS_NKShipmentServiceLevel = "GW1";

			exclusiveGatewayService2.O7_RS_NKGatewayService = "GW3";
			exclusiveGatewayService2.O7_RS_NKShipmentServiceLevel = "GW1";

			exclusiveGatewayService1.Validation.ValidateAll();
			Assert(!exclusiveGatewayService1.HasRowErrors);
			Assert(!exclusiveGatewayService1.O7_RS_NKGatewayServiceInfo.HasErrors());
			Assert(!exclusiveGatewayService1.O7_RS_NKShipmentServiceLevelInfo.HasErrors());

			exclusiveGatewayService2.Validation.ValidateAll();
			Assert(!exclusiveGatewayService2.HasRowErrors);
			Assert(!exclusiveGatewayService2.O7_RS_NKGatewayServiceInfo.HasErrors());
			Assert(!exclusiveGatewayService2.O7_RS_NKShipmentServiceLevelInfo.HasErrors());

			exclusiveGatewayService1.O7_RS_NKGatewayService = "GW3";

			exclusiveGatewayService1.Validation.ValidateAll();
			Assert(exclusiveGatewayService1.HasRowErrors);
			Assert(!exclusiveGatewayService1.O7_RS_NKGatewayServiceInfo.HasErrors());
			Assert(!exclusiveGatewayService1.O7_RS_NKShipmentServiceLevelInfo.HasErrors());

			exclusiveGatewayService2.Validation.ValidateAll();
			Assert(exclusiveGatewayService2.HasRowErrors);
			Assert(!exclusiveGatewayService2.O7_RS_NKGatewayServiceInfo.HasErrors());
			Assert(!exclusiveGatewayService2.O7_RS_NKShipmentServiceLevelInfo.HasErrors());

			exclusiveGatewayService1.O7_RS_NKGatewayService = "GW2";

			exclusiveGatewayService1.Validation.ValidateAll();
			Assert(!exclusiveGatewayService1.HasRowErrors);
			Assert(!exclusiveGatewayService1.O7_RS_NKGatewayServiceInfo.HasErrors());
			Assert(!exclusiveGatewayService1.O7_RS_NKShipmentServiceLevelInfo.HasErrors());

			exclusiveGatewayService2.Validation.ValidateAll();
			Assert(!exclusiveGatewayService2.HasRowErrors);
			Assert(!exclusiveGatewayService2.O7_RS_NKGatewayServiceInfo.HasErrors());
			Assert(!exclusiveGatewayService2.O7_RS_NKShipmentServiceLevelInfo.HasErrors());
		}
	}
}
