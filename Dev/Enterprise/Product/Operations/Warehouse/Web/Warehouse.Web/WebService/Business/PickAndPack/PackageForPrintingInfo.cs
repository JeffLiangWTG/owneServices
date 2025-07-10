using System;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PackageForPrintingInfo : PackageInfo
	{
		public bool SupportsCarrierLabelIntegration { get; set; }
	}
}