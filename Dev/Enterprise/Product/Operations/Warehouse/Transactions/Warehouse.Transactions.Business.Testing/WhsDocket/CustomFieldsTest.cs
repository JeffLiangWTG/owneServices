using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDocket))]
	class CustomFieldsTest : TestICustomFieldProvider
	{
		protected override BusinessObject GetBizo()
		{
			return Factory.New<WhsOrder>();
		}
	}
}
