using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public static class EPaymentUrlLauncher
	{
		public static void LaunchEPaymentProviderURL(ZString paymentProviderCode)
		{
			var url = ZString.Empty;
			switch (paymentProviderCode)
			{
				case EPaymentProviderCodes.Codes.OFX:
					url = AccountingMasterFilesRegistry.Instance.OFXWebURL.Value;
					break;
			}

			if (!url.IsEmpty)
			{
				WebUrlLauncher.Launch(url);
			}
		}

		public static void LaunchEPaymentProductMarketingURL()
		{
			var url = AccountingMasterFilesRegistry.Instance.EPaymentProductMarketingWebURL.Value;
			if (!url.IsNullOrEmpty())
			{
				WebUrlLauncher.Launch(url);
			}
		}
	}
}
