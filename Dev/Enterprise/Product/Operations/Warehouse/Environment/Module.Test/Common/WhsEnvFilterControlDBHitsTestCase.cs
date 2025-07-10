using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	public abstract class WhsEnvFilterControlDBHitsTestCase<TCollection, TFilterBizO> : FilterControlDBHitsTestCase<TCollection, TFilterBizO>
		where TCollection : IBusinessObjectCollection
		where TFilterBizO : FilterBusinessObject
	{
		#region Properties

		protected WhsTestHelperFunctionsEnv Helper => helper ?? (helper = GetNewTestHelperFunctions());
		WhsTestHelperFunctionsEnv helper;

		protected virtual WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnv(Factory);
		}

		protected TestNotificationBuffer Notify => notify ?? (notify = new TestNotificationBuffer());
		TestNotificationBuffer notify;

		#endregion
	}
}
