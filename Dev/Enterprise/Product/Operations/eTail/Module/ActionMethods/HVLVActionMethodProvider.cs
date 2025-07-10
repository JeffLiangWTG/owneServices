using System.Collections.Generic;
using Enterprise.eTail.Business;
using Enterprise.eTail.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.eTail.Module
{
	public class HVLVActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			var result = new List<OperationalActionMethod>();

			if (typeof(ForwardingShipment).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new UpdateHVLVStatusActionMethod());
			}
			else if (typeof(HVLVConsignment).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new UpdateHVLVItemStatusActionMethod());
			}

			return result.ToArray();
		}
	}
}
