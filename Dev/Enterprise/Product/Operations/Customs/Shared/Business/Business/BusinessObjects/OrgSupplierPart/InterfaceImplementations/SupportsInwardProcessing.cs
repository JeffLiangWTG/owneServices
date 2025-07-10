using Enterprise.Customs.DataRegistry.Business;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public class SupportsInwardProcessing : ISupportedForProcessing
	{
		public static bool IsSupported() => CustomsDataRegistry.Instance.EnableInwardProcessing.Value;

		public bool IsSupportedForProcessing() => IsSupported();
	}
}
