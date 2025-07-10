using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class ReceiveFromWorkOrderCustomPropertiesProvider
	{
		public static ICustomPropertyContainer GetCustomPropertiesForAssembledReceive()
		{
			var customProperties = new CustomPropertyContainer<WhsReceiveLine>();
			customProperties.AddCustomProperty("IsLeftoverComponent",
				Res.GetString("a10bcf75-92f7-4617-a662-092d66b0e72c", "Is Leftover Component"),
				typeof(ZBool), l => !l.BOMComponentLinks.Any());

			return customProperties;
		}
	}
}
