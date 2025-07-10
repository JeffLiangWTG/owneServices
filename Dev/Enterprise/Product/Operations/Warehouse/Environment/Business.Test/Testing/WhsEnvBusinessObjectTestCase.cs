using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public abstract class WhsEnvBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		public WhsEnvBusinessObjectTestCase()
		{
		}

		public void TestOperationalActionFields()
		{
			var testChildren = false; // Child objects should be tested on their own OR are not our responsibility

			var fieldsTester = ObjectFactory.Get<IOperationalActionFieldTester>();
			AssertNoExceptionThrown(() => fieldsTester.TestFields(GetExpectedBusinessObjectType(), testChildren));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		#region Date Asserts

		public void AssertDateEquals(string message, ZDateTime date1, ZDateTime date2)
		{
			bool equals = date1.Year == date2.Year && date1.Month == date2.Month &&
				date1.Day == date2.Day && date1.Hour == date2.Hour;

			AssertEquals(message, true, equals);
		}

		#endregion

		#region Properties

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctionsEnv(Factory);
				}
				return helper;
			}
		}

		TestNotificationBuffer notify;
		protected TestNotificationBuffer Notify
		{
			get
			{
				if (notify == null)
				{
					notify = GetNotify();
				}
				return notify;
			}
		}

		protected virtual TestNotificationBuffer GetNotify()
		{
			return new TestNotificationBuffer();
		}

		#endregion
	}
}
