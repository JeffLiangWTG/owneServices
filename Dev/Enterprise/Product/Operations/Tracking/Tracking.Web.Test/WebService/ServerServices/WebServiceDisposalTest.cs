using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Tracking.Web.WebService;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WebServiceDisposalTest : TestCaseWithFactory
	{
		#region Test Cases

		public void TestExecuteAndDisposeDBConnection()
		{
			var originalProvider = Env.GetCurrentProvider();
			using (var webProvider = new WebServiceEnvironmentProvider())
			{
				webProvider.Enable();
				using (Db.DisposableActionForDbConnection())
				{
					var dbConnectionHashCode = Db.Connection.GetHashCode();
					var result = WebService.HasDBConnection();

					Assert("Should have the db connection", result);
					Assert("Should call ExecuteAndDispose", WebService.ExecuteAndDisposeHasBeenCalled);
					using (Db.DisposableActionForDbConnection())
					{
						AssertNotEquals("Should reset the db connection for the thread", dbConnectionHashCode, Db.Connection.GetHashCode());
					}
				}
			}
			originalProvider.Enable();
		}

		public void TestExecuteAndDisposeFactory()
		{
			var cachedFactoryHashCode = WebService.Factory.GetHashCode();
			var result = WebService.HasFactory();

			Assert("Should have the factory", result);
			Assert("Should call ExecuteAndDispose", WebService.ExecuteAndDisposeHasBeenCalled);
			AssertNotEquals("Should reset the factory", cachedFactoryHashCode, WebService.Factory.GetHashCode());
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			WebService.Dispose();
		}

		protected override void SetUp()
		{
			base.SetUp();
			WebService = new WebServiceForTesting();
		}

		WebServiceForTesting WebService;

		class WebServiceForTesting : WebServiceWithFactory
		{
			public bool HasDBConnection()
			{
				return ExecuteAndDispose(() =>
				{
					return Db.Connection != null;
				});
			}

			public bool HasFactory()
			{
				return ExecuteAndDispose(() =>
				{
					return Factory != null;
				});
			}

			public new BusinessObjectFactory Factory => base.Factory;
		}

		#endregion
	}
}
