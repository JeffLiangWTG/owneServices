using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface IOrderStatusListProvider
		{
			CodeDescriptionPairList GetOrderStatusList();
			CodeDescriptionPairList GetOrderLineStatusList();
		}
	}
}
