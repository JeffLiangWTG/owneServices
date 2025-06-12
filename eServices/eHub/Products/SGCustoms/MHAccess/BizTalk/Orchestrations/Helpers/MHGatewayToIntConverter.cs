using System;

namespace CargoWise.eHub.Products.SGCustoms.MHAccess.Orchestrations.Helpers
{
    public static class MHGatewayToIntConverter
    {
        public static int ToInt(string str) => string.IsNullOrWhiteSpace(str) ? 0 : Convert.ToInt32(str);
    }
}
