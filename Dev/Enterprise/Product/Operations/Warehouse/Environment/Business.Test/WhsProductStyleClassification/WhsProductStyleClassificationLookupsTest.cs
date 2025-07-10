using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsProductStyleClassificationLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Implementation

		protected WhsProductStyleClassification Params
		{
			get { return prodParams ?? (prodParams = Factory.New<WhsProductStyleClassification>());  }
		}

		WhsProductStyleClassification prodParams;

		#endregion
	}
}
