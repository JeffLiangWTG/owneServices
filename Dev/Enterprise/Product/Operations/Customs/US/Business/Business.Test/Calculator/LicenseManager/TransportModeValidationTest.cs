using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;

namespace Enterprise.Customs.US.Business.LicenseManager.Testing
{
	sealed class TransportModeValidationTest : TestCaseWithFactory
	{
		public void TestGetTransportModeError()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C60, USAESLicenseCode.Codes.S94, USAESLicenseCode.Codes.SGB });

			AssertEquals("GetTransportModeError", "", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.SGB, "", Factory, ZDateTime.Today));
			AssertEquals("GetTransportModeError", "", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.SGB, TransportTypeList.Codes.Air, Factory, ZDateTime.Today));
			AssertEquals("GetTransportModeError", string.Format(TransportModeValidation.LicenseTypeIsInvalidForTransportModeMessage, "", TransportTypeList.Codes.FixedTransportInstallations), LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.SGB, TransportTypeList.Codes.FixedTransportInstallations, Factory, ZDateTime.Today));

			AssertEquals("GetTransportModeError", string.Format(TransportModeValidation.LicenseTypeIsInvalidForTransportModeMessage, "s", TransportTypeList.Codes.FixedTransportInstallations + "," + TransportTypeList.Codes.PassengerHandCarried), LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.S94, TransportTypeList.Codes.PassengerHandCarried, Factory, ZDateTime.Today));
			AssertEquals("GetTransportModeError", string.Format(TransportModeValidation.LicenseTypeIsInvalidForTransportModeMessage, "s", TransportTypeList.Codes.FixedTransportInstallations + "," + TransportTypeList.Codes.PassengerHandCarried), LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.S94, TransportTypeList.Codes.FixedTransportInstallations, Factory, ZDateTime.Today));
			AssertEquals("GetTransportModeError", "", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.C60, "", Factory, ZDateTime.Today));
		}

		public void TestGetTransportModeWithInvalidateDate()
		{
			USAESLicenseCodeTestHelper.CreateNewOrGetExistingCusCodeList(Factory, new ZString[] { USAESLicenseCode.Codes.C30 });
			Factory.Save();
			AssertEquals("", LicenseValidationHelper.GetTransportModeError(USAESLicenseCode.Codes.C30, TransportTypeList.Codes.FixedTransportInstallations, Factory, ZDateTime.Invalid));
		}
	}
}
