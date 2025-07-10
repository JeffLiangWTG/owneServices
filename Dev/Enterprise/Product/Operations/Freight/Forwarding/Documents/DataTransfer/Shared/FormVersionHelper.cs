using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	static class FormVersionHelper
	{
		public static string GetOCMFormVersion()
		{
			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				return "4.0.0"; // Programatic version number
			}

			if (FreightDataRegistry.Instance.EnableBookingConfirmation.Value)
			{
				return "3.0.0"; // Programatic version number
			}

			return "2.5.0"; // Programatic version number
		}
	}
}
