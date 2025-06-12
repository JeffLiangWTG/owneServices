using CargoWise.eServices.Monitoring.HealthCheck.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CargoWise.eHub.Products.NZCustoms.Gateway.HealthCheck
{
    public class NZCustomsGatewayHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {
        public NZCustomsGatewayHealthCheckHttpTaskAsyncHandler()
            : base(new IHealthCheckItemProvider[] { new NZCustomsGatewayHealthCheckItemProvider() })
		{
		}
    }
}