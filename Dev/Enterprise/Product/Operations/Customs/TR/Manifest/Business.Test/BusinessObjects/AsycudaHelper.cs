using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TR.Manifest.Business.BusinessObjects.Testing
{
	public class AsycudaHelper : TestCaseWithFactory
	{
		public void TestConwvertWeightQty()
		{
			var weight = 5000;
			var sourceUnitCode = "G";
			var targetUnitCode = "KG";
			ZDecimal expectedValue = 5;
			ZDecimal expectedValue2 = 0;
			var result = Business.AsycudaHelper.ConvertWeightQty(weight, sourceUnitCode, targetUnitCode);
			AssertEquals(expectedValue, result);
			sourceUnitCode = "";
			targetUnitCode = "KG";
			result = Business.AsycudaHelper.ConvertWeightQty(weight, sourceUnitCode, targetUnitCode);
			AssertEquals(expectedValue2, result);
			sourceUnitCode = "G";
			targetUnitCode = "";
			result = Business.AsycudaHelper.ConvertWeightQty(weight, sourceUnitCode, targetUnitCode);
			AssertEquals(expectedValue2, result);
			sourceUnitCode = "YIGIT";
			targetUnitCode = "KG";
			result = Business.AsycudaHelper.ConvertWeightQty(weight, sourceUnitCode, targetUnitCode);
			AssertEquals(expectedValue2, result);
			sourceUnitCode = "G";
			targetUnitCode = "MIKE";
			result = Business.AsycudaHelper.ConvertWeightQty(weight, sourceUnitCode, targetUnitCode);
			AssertEquals(expectedValue2, result);
		}

		public void TestLoadManifestHeadersForMasterBill()
		{
			var currentYear = (ZString)ZDateTime.Today.Year.ToString();
			currentYear = currentYear.SubstringSafe(2);

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header.AMA_JobReference = "MAN-001";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_MasterBill = "01234567890";
			header.RegistrationNumber = currentYear + "IM340300IM202356";
			header.RegistrationDate = new ZDateTime(ZDateTime.Today.Year, 10, 10);
			Factory.Save();

			var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "TR";
			header2.AMA_JobReference = "MAN-002";
			header2.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header2.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header2.AMA_MasterBill = "01234567890";

			var loadedHeader = Business.AsycudaHelper.LoadManifestHeadersForMasterBill(header2.Factory, header2.AMA_MasterBill, Core.Constants.TransportModes.Sea, header2.PK, header2.AMA_ApplicationCode);

			CombineAssertions(() =>
			{
				AssertNotNull(loadedHeader);
				AssertEquals("MAN-001", loadedHeader.AMA_JobReference);
				var registrationNumber = currentYear + "IM340300IM202356";
				AssertEquals(registrationNumber, loadedHeader.RegistrationNumber);
				AssertEquals(ShipmentTypeList.Codes.Import23, loadedHeader.AMA_Nature);
			});
		}
	}
}
