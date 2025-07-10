using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	class VisitedPortValidationForHeaderTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Date()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var port = header.VisitedPorts.AddNew();
			port.CY_Code = "TRIST-001";
			port.Validation.ValidateCY_Date();
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(port.CY_DateInfo, MandatoryValidation.YouHaveNotEntered);
				port.CY_Date = DateTime.Now.AddDays(-3);
				AssertNoMessageErrorContaining("After set value, there should not be message error on CY_Date", port.CY_DateInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				port.CY_Date = ZDateTime.Empty;
				AssertNoMessageErrorContaining("When Transport Mode is AIR and Manifest Type CIKONC, there should not be message error on CY_Date", port.CY_DateInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_ManifestType = TRManifestTypes.Codes.HAVIHR;
				port.CY_Date = ZDateTime.Empty;
				AssertHasMessageErrorContaining(port.CY_DateInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				header.AMA_ManifestType = TRManifestTypes.Codes.CIKONC;
				port.CY_Date = ZDateTime.Empty;
				AssertNoMessageErrorContaining("When Transport Mode is SEA and Manifest Type CIKONC , there should not be message error on CY_Date", port.CY_DateInfo, MandatoryValidation.YouHaveNotEntered);
				header.AMA_ManifestType = TRManifestTypes.Codes.DENIHR;
				port.CY_Date = ZDateTime.Empty;
				AssertHasMessageErrorContaining(port.CY_DateInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}
}
