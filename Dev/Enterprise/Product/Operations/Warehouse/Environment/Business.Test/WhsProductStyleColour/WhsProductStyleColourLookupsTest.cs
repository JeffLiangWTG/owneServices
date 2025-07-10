using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsProductStyleColourLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Implementation

		protected WhsProductStyleColour Params
		{
			get { return prodParams ?? (prodParams = Factory.New<WhsProductStyleColour>()); }
		}

		WhsProductStyleColour prodParams;

		#endregion
	}
}
