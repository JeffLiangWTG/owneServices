using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ServiceTypeDateFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJobServiceType()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);

			filter.JobServiceType = string.Empty;
			AssertNoNotifications(filter.JobServiceTypeInfo);

			filter.JobServiceType = "123";
			AssertHasError(filter.JobServiceTypeInfo, "Enter a valid selection.");

			filter.JobServiceType = new FreightServiceTypes()[0].Code;
			AssertNoNotifications(filter.JobServiceTypeInfo);
		}
	}
}
