#if DEBUG

using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class EInvoicingSettingsHelper
	{
		public static Mock<T> CreateAndHookSettingsMock<T>(string countryCode = "") where T : class, IEInvoicingCredentialSettings
		{
			var credentialsMock = new Mock<T>();
			var settingsMock = new Mock<ICountryEInvoicingObjectFactorySettings>();
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			if (string.IsNullOrEmpty(countryCode))
			{
				globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(settingsMock.Object);
			}
			else
			{
				globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.Is<ZString>(y => y == countryCode))).Returns(settingsMock.Object);
			}

			settingsMock.Setup(x => x.Credentials).Returns(credentialsMock.Object);
			ObjectFactory.Substitute(globalFactoryMock.Object);

			credentialsMock.Setup(x => x.PasswordType).Returns(PasswordTypesList.Codes.EIM);
			return credentialsMock;
		}

		public static Mock<T> CreateAndHookSettingsMockForBranch<T>(string countryCode = "") where T : class, IEInvoicingCredentialSettings
		{
			var credentialsMock = CreateAndHookSettingsMock<T>(countryCode: countryCode);
			credentialsMock.Setup(x => x.IsBranchCredentialsRequired).Returns(true);
			return credentialsMock;
		}

		public static Mock<T> CreateAndHookSettingsMockForCompany<T>(string countryCode = "") where T : class, IEInvoicingCredentialSettings
		{
			var credentialsMock = CreateAndHookSettingsMock<T>(countryCode: countryCode);
			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			return credentialsMock;
		}

		#region Certificate Withs

		public static Mock<IEInvoicingCertificateCredentialSettings> WithExpiryWarningDays(this Mock<IEInvoicingCertificateCredentialSettings> credentialsMock, int expiryWarningDaysValue)
		{
			credentialsMock.Setup(x => x.ExpiryWarningDays).Returns(expiryWarningDaysValue);
			return credentialsMock;
		}

		public static Mock<IEInvoicingCertificateCredentialSettings> WithHiddenColumns(this Mock<IEInvoicingCertificateCredentialSettings> credentialsMock, string[] hiddenColumnsValue)
		{
			credentialsMock.Setup(x => x.HiddenColumns).Returns(hiddenColumnsValue);
			return credentialsMock;
		}

		#endregion

	}
}

#endif
