using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WarehouseRFRegistryInfoResponse))]
	public class WarehouseRFRegistryInfoResponseTestCase : WebServiceResponseTestCase
	{
		public void TestUserNameAndLanguage()
		{
			var response = new WarehouseRFRegistryInfoResponse();
			AssertEquals("Default response is empty.", null, response.RFRegistryInfo);

			response.RFRegistryInfo = new WhsRFRegistryInfo { Language = Core.SharedConstants.Languages.ChineseSimplified };
			AssertEquals("Updated value is Chinese Simplified.", Core.SharedConstants.Languages.ChineseSimplified, response.RFRegistryInfo.Language);
		}

		public void TestResponseProperties()
		{
			var response = new WarehouseRFRegistryInfoResponse();
			AssertEquals("Default response is empty.", null, response.RFRegistryInfo);

			var registryInfo = new WhsRFRegistryInfo
			{
				ClientCode = "ABC",
				UOMType = "ANY",
				EnableErrorAudio = true,
				Language = Core.SharedConstants.Languages.ChineseSimplified,
				EquipmentRegistrationNumber = "Trolley 1",
				PickGroup = new WhsPickGroupInfo(),
				PickMethod = new WhsPickMethodInfo(),
				PickArea = new WhsAreaInfo(),
				Printer = new PrinterInfo(),
			};

			response.RFRegistryInfo = registryInfo;
			AssertEquals(nameof(registryInfo.ClientCode), registryInfo.ClientCode, response.RFRegistryInfo.ClientCode);
			AssertEquals(nameof(registryInfo.UOMType), registryInfo.UOMType, response.RFRegistryInfo.UOMType);
			AssertEquals(nameof(registryInfo.EnableErrorAudio), registryInfo.EnableErrorAudio, response.RFRegistryInfo.EnableErrorAudio);
			AssertEquals(nameof(registryInfo.Language), registryInfo.Language, response.RFRegistryInfo.Language);
			AssertEquals(nameof(registryInfo.EquipmentRegistrationNumber), registryInfo.EquipmentRegistrationNumber, response.RFRegistryInfo.EquipmentRegistrationNumber);
			AssertEquals(nameof(registryInfo.PickGroup), registryInfo.PickGroup, response.RFRegistryInfo.PickGroup);
			AssertEquals(nameof(registryInfo.PickMethod), registryInfo.PickMethod, response.RFRegistryInfo.PickMethod);
			AssertEquals(nameof(registryInfo.PickArea), registryInfo.PickArea, response.RFRegistryInfo.PickArea);
			AssertEquals(nameof(registryInfo.Printer), registryInfo.Printer, response.RFRegistryInfo.Printer);
		}

		protected override WebServiceResponse GetNewResponse()
		{
			return new WarehouseRFRegistryInfoResponse();
		}
	}
}
