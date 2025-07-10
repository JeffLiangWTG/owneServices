using System;
using CargoWise.Application;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Moq;

namespace Enterprise.Customs.Business.Testing
{
	public class WarehouseCustomsDetailsChangeOfRegimeForTesting : IWarehouseCustomsDetailsChangeOfRegime
	{
		public static (IDisposable disposable, WarehouseCustomsDetailsChangeOfRegimeForTesting customsDetails) Setup(string key)
		{
			var customsDetails = new WarehouseCustomsDetailsChangeOfRegimeForTesting()
			{
				IntoRegimeType = CustomsRegime.InwardProcessing,
				OutOfRegimeType = CustomsRegime.BondedWarehouse
			};
			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject(It.IsAny<object>())).Returns(customsDetails);
			var warehouseCustomsDetailsChangeOfRegimesConfiguration = new KeyObjectHandleDictionaryObject { { key, objectHandleMock.Object } };

			return (ObjectFactory.Substitute("WarehouseCustomsDetailsChangeOfRegimes", warehouseCustomsDetailsChangeOfRegimesConfiguration), customsDetails);
		}

		public CustomsRegime IntoRegimeType { get; set; }
		public CustomsRegime OutOfRegimeType { get; set; }
		public OrganizationAddress NewWarehouse { get; set; }
	}
}
