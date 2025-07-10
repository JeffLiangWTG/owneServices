using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveSemaphoreHandle))]
	sealed class ActiveSemaphoreHandleTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ActiveSemaphoreHandle(Factory, new ActiveUser(Factory));
		}
	}
}
