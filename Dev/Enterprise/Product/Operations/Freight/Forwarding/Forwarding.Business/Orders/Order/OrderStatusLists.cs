using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderStatusLists : Integration.Forwarding.IOrderStatusListProvider
	{
		public CodeDescriptionPairList GetOrderStatusList()
		{
			CodeDescriptionPairList orderHeaderStatuses = new CodeDescriptionPairList(OLookUpEditType.OrderHeaderStatus);
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddRange(Env.Registry.OrderHeaderStatusList);
			result.AddRange(orderHeaderStatuses);

			if (GlbBranch.CurrentBranch.OrgProxy != null &&
				GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderStatusList != null)
			{
				result.AddRange(new ReadOnlyCodeDescriptionPairList(GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderStatusList));
			}

			return result;
		}

		public CodeDescriptionPairList GetOrderLineStatusList()
		{
			CodeDescriptionPairList orderLineStatuses = new CodeDescriptionPairList(OLookUpEditType.OrderLineStatus);
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddRange(Env.Registry.OrderLineStatusList);
			result.AddRange(orderLineStatuses);

			if (GlbBranch.CurrentBranch.OrgProxy != null &&
				GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderLineStatusList != null)
			{
				result.AddRange(new ReadOnlyCodeDescriptionPairList(GlbBranch.CurrentBranch.OrgProxy.MiscServ.OrderLineStatusList));
			}

			return result;
		}
	}
}
