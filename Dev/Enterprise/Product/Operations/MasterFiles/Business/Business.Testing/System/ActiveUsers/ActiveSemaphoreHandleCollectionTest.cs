using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveSemaphoreHandleCollection))]
	sealed class ActiveSemaphoreHandleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ActiveSemaphoreHandleCollection>
	{
		protected override ActiveSemaphoreHandleCollection GetCollectionToTest()
		{
			return new ActiveSemaphoreHandleCollection(Factory, User);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return
				new ActiveSemaphoreHandle(Factory, User)
				{
					AS_ServiceClass = "LGN",
					AS_LockInfo = "EnterpriseActiveLogin",
					AS_UseCount = 1,
					AS_AcquiredTimeUTC = ZDateTime.UtcNow.AddMinutes(-5)
				};
		}

		ActiveUser User
		{
			get { return user ?? (user = new ActiveUser(Factory) { AU_GS = GlbStaff.CurrentUser.PK }); }
		}
		ActiveUser user;
	}
}
