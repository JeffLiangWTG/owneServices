using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRServiceTemplate))]
	class MNRServiceTemplateTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<MNRServiceTemplate>();
		}

		#region TestYard
		public void TestYard()
		{
			var whsWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var serviceTemplate = (MNRServiceTemplate)GetNewBusinessObject();
			serviceTemplate.MST_WW_Yard = whsWarehouse.PK;
			AssertEquals(whsWarehouse, serviceTemplate.Yard);
		}
		#endregion
	}
}
