using System;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.GUI
{
	public static class ExportUnionFtpAdressHelper
	{
		public static string GeneratePaymentLink()
		{
			var ftpSettings = TRCustomsDataRegistry.Instance.FTPSettings.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var ftpAddress = ftpSettings.FTPAddressList.GetDescriptionFromCode(ftpSettings.FTPAddress);
			ftpAddress = ConvertFtpAddressToPaymentLinkAdress(ftpAddress);
			var exportUnionUserCode = ftpSettings.ExportUnionUserCode;
			var exportUnionUserPassword = ftpSettings.ExportUnionUserPassword;

			if (string.IsNullOrEmpty(ftpAddress) || string.IsNullOrEmpty(exportUnionUserCode) || string.IsNullOrEmpty(exportUnionUserPassword))
			{
				return string.Empty;
			}

			string paymentLinkUserCode = $"/ebnet/app?USERCODE={exportUnionUserCode}";
			string paymentLinkPassword = $"&PASSWORDFREE={exportUnionUserPassword}";

			return ftpAddress + paymentLinkUserCode + paymentLinkPassword;
		}

		public static string ConvertFtpAddressToPaymentLinkAdress(string ftpAddress)
		{
			if (string.IsNullOrWhiteSpace(ftpAddress))
			{
				return string.Empty;
			}

			if (ftpAddress.StartsWith((NoResString)"ftp", StringComparison.OrdinalIgnoreCase))
			{
				return $"https://{ftpAddress.Substring(3).TrimStart('.')}";
			}

			return ftpAddress;
		}
	}
}
