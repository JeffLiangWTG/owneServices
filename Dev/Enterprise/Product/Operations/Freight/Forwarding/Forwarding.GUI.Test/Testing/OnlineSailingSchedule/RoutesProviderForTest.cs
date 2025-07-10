using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules;
using OssServiceModel = Enterprise.Freight.OnlineSailingSchedules.ServiceModel;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class RoutesProviderForTest : IRoutesProvider
	{
		public Route[] GetRoutes(string searchParams, IServiceRequestManager requestManager)
		{
			var carrier = new OssServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" };

			var voyage = new OssServiceModel.Voyage
			{
				Code = "52w",
				TradeLane = new OssServiceModel.TradeLane { Name = "AAA" },
				Operator = new OssServiceModel.Carrier { Code = "CMAC", Name = "CMA CGM" },
				Vessel = new OssServiceModel.Vessel { VesselName = "NYK METEOR", ImoNumber = "9308390" }
			};

			var leg = new OssServiceModel.Leg
			{
				LoadPort = new OssServiceModel.Port { Unloco = "NLRTM" },
				DischargePort = new OssServiceModel.Port { Unloco = "USLGB" },
				Etd = new DateTime(2017, 8, 10),
				Eta = new DateTime(2017, 8, 15),
				Voyage = voyage
			};

			var serviceRoute = new OssServiceModel.Route { Carrier = carrier, Legs = new OssServiceModel.Leg[] { leg } };
			var route = new Route(new BusinessObjectFactory());
			route.SetValues(serviceRoute);

			return new Route[] { route };
		}
	}
}
