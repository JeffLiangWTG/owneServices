using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Integration.Freight;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Freight.Forwarding.Module
{
	public class ShipmentActionMethodProvider : OperationalActionMethodProvider
	{
		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			if (typeof(ForwardingShipment).IsAssignableFrom(actionSupporter.RootType))
			{
				var result = new List<OperationalActionMethod>();
				if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
				{
					result.Add(new CalculateGreenhouseGasEmissionsActionMethod());
				}
				result.Add(new UpdateCTStatusActionMethod());
				result.Add(new UpdateLastKnownTransitWarehouseActionMethod());

				return result.ToArray();
			}

			return null;
		}
	}
}
