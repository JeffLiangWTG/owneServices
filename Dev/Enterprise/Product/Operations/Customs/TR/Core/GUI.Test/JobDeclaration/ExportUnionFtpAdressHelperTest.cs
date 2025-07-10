using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class ExportUnionFtpAdressHelperTest : TestCaseWithFactory
	{
		public void TestGeneratePaymentLink()
		{
			var fTPSettings = TRCustomsDataRegistry.Instance.FTPSettings.GetFallBackValueAtAllLevels(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty);

			fTPSettings.ExportUnionUserCode = "user99";
			fTPSettings.ExportUnionUserPassword = "password99";
			fTPSettings.FTPAddress = ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet;

			using (TRCustomsDataRegistry.Instance.FTPSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fTPSettings))
			{
				AssertEquals("https://istanbul.ebirlik.net/ebnet/app?USERCODE=user99&PASSWORDFREE=password99", ExportUnionFtpAdressHelper.GeneratePaymentLink());
			}
		}

		public void TestGeneratePaymentLinkReturnsEmpty()
		{
			var fTPSettings = TRCustomsDataRegistry.Instance.FTPSettings.GetFallBackValueAtAllLevels(
				GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty,
				Guid.Empty);

			fTPSettings.ExportUnionUserCode = "user99";
			fTPSettings.ExportUnionUserPassword = "password99";
			fTPSettings.FTPAddress = string.Empty;

			using (TRCustomsDataRegistry.Instance.FTPSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fTPSettings))
			{
				AssertEquals(string.Empty, ExportUnionFtpAdressHelper.GeneratePaymentLink());
			}

			fTPSettings.FTPAddress = "istanbul.ebirlik.net";
			fTPSettings.ExportUnionUserCode = string.Empty;
			fTPSettings.ExportUnionUserPassword = string.Empty;

			using (TRCustomsDataRegistry.Instance.FTPSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, fTPSettings))
			{
				AssertEquals(string.Empty, ExportUnionFtpAdressHelper.GeneratePaymentLink());
			}
		}

		public void TestConvertFtpAddressToPaymentLinkAdress()
		{
			var result = ExportUnionFtpAdressHelper.ConvertFtpAddressToPaymentLinkAdress("ftpistanbul.ebirlik.net");
			AssertEquals($"https://istanbul.ebirlik.net", result);

			result = ExportUnionFtpAdressHelper.ConvertFtpAddressToPaymentLinkAdress("ankara.ebirlik.net");
			AssertEquals("ankara.ebirlik.net", result);

			result = ExportUnionFtpAdressHelper.ConvertFtpAddressToPaymentLinkAdress(string.Empty);
			AssertEquals(string.Empty, result);
		}
	}
}
