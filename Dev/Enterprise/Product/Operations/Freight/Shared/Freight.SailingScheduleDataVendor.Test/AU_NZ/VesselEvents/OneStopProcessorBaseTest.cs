using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	public abstract class OneStopProcessorBaseTest : TestCaseWithFactory
	{
		public abstract void TestProcessWithEmptyTable();
		public abstract void TestProcessWithSomeDataAlreadyInTable();
		protected abstract void SetupTestData();
		protected abstract void AssertRecordHasBeenUpdated();

		protected void AssertFactoryContains(BusinessObjectFactory factory, IEnumerable<BusinessObject> businessObjects)
		{
			foreach (var businessObject in businessObjects)
			{
				var query = new ZQuery(businessObject.PKSchemaColumn, businessObject.PK);
				query.FetchOnlyFromLocalCache = true;
				var loadedBusinessObject = factory.LoadTop1(businessObject.GetType(), query);
				AssertNotNull("Factory contains " + businessObject.HumanReadableName, loadedBusinessObject);
			}
		}

		protected void AssertFactoryNotContains(BusinessObjectFactory factory, IEnumerable<BusinessObject> businessObjects)
		{
			foreach (var businessObject in businessObjects)
			{
				var query = new ZQuery(businessObject.PKSchemaColumn, businessObject.PK);
				query.FetchOnlyFromLocalCache = true;
				var loadedBusinessObject = factory.LoadTop1(businessObject.GetType(), query);
				AssertNull("Factory does not contain " + businessObject.HumanReadableName, loadedBusinessObject);
			}
		}
	}
}
