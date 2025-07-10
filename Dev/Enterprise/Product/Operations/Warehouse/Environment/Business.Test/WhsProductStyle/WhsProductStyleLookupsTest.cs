using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsProductStyleLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Implementation

		protected WhsProductStyle Params
		{
			get { return prodParams ?? (prodParams = Factory.New<WhsProductStyle>()); }
		}

		WhsProductStyle prodParams;

		#endregion
	}
}
