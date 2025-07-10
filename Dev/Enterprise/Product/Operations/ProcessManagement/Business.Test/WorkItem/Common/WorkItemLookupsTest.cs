using CargoWise.EntityFramework.Testing;

namespace Enterprise.ProcessManagement.Business.Test
{
	public abstract class WorkItemLookupsTest<T> : BusinessObjectLookupsTestCase
		where T : WorkItemCommon
	{
	}
}
