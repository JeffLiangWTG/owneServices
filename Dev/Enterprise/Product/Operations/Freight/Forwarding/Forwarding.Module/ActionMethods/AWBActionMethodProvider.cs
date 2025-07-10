using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Module
{
	public class AWBActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			var result = new List<OperationalActionMethod>();

			if (typeof(ForwardingConsol).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new PrintFinalMasterActionMethod());
				result.Add(new PrintMAWBBarcodeLabelsActionMethod());
			}

			return (result.Count > 0) ? result.ToArray() : null;
		}
	}
}
