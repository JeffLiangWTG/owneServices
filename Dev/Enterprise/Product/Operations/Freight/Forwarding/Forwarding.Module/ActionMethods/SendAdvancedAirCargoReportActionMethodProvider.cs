using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Module
{
	public class SendAdvancedAirCargoReportActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			var result = new List<OperationalActionMethod>();

			if (typeof(ForwardingConsol).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new SendACASConsolActionMethod());
				result.Add(new SendCCTConsolActionMethod());
			}
			if (typeof(ForwardingShipment).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new SendACASShipmentActionMethod());
				result.Add(new SendCCTShipmentActionMethod());
			}
			return (result.Count > 0) ? result.ToArray() : null;
		}
	}
}
