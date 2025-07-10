using System.Drawing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public static class EPaymentProviderLogoFinder
	{
		public static Image FindEPaymentProviderLogo(ZString paymentProviderCode)
		{
			Image logo = null;
			switch (paymentProviderCode)
			{
				case EPaymentProviderCodes.Codes.OFX:
					logo = AccountingMasterFilesRegistry.Instance.OFXLogo.Value;
					break;
			}
			return logo;
		}
	}
}
