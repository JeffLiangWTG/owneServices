using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Shared.CW;

namespace Enterprise.MasterFiles.Business.Testing
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This is used by reflection code in UnitTestRunner.cs.")]
	public sealed class ZEnvironmentListener : BaseTestListener
	{
		ZEnvironmentListener()
		{
		}

		#region Singleton

		public static ZEnvironmentListener Instance => instance ?? (instance = new ZEnvironmentListener());
		static ZEnvironmentListener instance;

		#endregion

		#region Resetters

		BusinessObjectFactory UserContextFactory => GlbStaff.CurrentUser?.Factory ?? GlbBranch.CurrentBranch?.Factory ?? GlbDepartment.CurrentDepartment?.Factory;

		FactoryChangeMonitor monitor;
		(string loginName, ILoginToken loginToken, Guid branchPk, Guid departmentPk) contextBeforeTest;

		void CaptureUserContext()
			=> contextBeforeTest = (
				Env.CurrentUser.LoginName,
				Env.CurrentUser.LoginToken,
				Env.CurrentBranchPK,
				Env.CurrentDepartmentPK
			);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void ResetUserContext()
			=> Env.SetUserContext(new UserContext(contextBeforeTest.loginName, contextBeforeTest.branchPk, contextBeforeTest.departmentPk, contextBeforeTest.loginToken, factory: null) { Licence = Env.Licence });

		bool UserContextHasBeenModified
			=> (GlbStaff.CurrentUser == null || GlbBranch.CurrentBranch == null || GlbCompany.CurrentCompany == null) ||
					!ReferenceEquals(UserContextFactory, monitor.Factory) ||
					monitor.WasModified;

		public override void StartAllTests(DateTime startTime)
		{
			base.StartAllTests(startTime);
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest = null;
		}

		public override void BeforeEachTest(DateTime startTime)
		{
			base.BeforeEachTest(startTime);

			monitor = new FactoryChangeMonitor(UserContextFactory);
			CaptureUserContext();
			Env.SetupDataBeforeTest();
		}

		public override void AfterEachTest(DateTime endTime)
		{
			base.AfterEachTest(endTime);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			EnvForTest.ResetSetupDataAfterTestIfNeeded();
			Enterprise.ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest = null;
			SharedRegistry.Instance = null;

			try
			{
				var checkResult = String.Format("ZEnvironmentListener.AfterEachTest: Properties are null: {0}, {1}, {2}",
					GlbStaff.CurrentUser == null ? "GlbStaff.CurrentUser " : "",
					GlbBranch.CurrentBranch == null ? "GlbBranch.CurrentBranch" : "",
					GlbCompany.CurrentCompany == null ? "GlbCompany.CurrentCompany" : "");

				Assertion.Assert(checkResult, !(GlbStaff.CurrentUser == null || GlbBranch.CurrentBranch == null || GlbCompany.CurrentCompany == null));
			}
			finally
			{
				if (UserContextHasBeenModified)
				{
					ResetUserContext();
				}

				monitor.Dispose();
				DigestReporterProvider.ResetAllDigestReportersAfterTest();
			}
		}

		#endregion

		#region ZEnvironmentListenerTest

		public class ZEnvironmentListenerTest : TestCaseWithFactory
		{
			public void TestFactoryChangesAreRolledBack()
			{
				int CountBizos()
					=> GlbStaff.CurrentUser.Factory.Load<DummyBusinessObject>(new ZQuery()).Length;

				var countBefore = CountBizos();

				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());

				GlbStaff.CurrentUser.Factory.NewWithValidTestData<DummyBusinessObject>();

				AssertEquals("PRE: We created one in the test, which should be returned in our Load", countBefore + 1, CountBizos());

				listener.AfterEachTest(ZDateTime.Now.ToDateTime());

				AssertEquals("Any New'd objects in the test should be rolled back", countBefore, CountBizos());
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
			public void TestInitialDataIsRestored()
			{
				ZEnvironmentListener listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				decimal oldValue = Env.CurrentCompany.ExchangeRate.ForeignToLocal(10m, 0.5m);
				bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;
				GlbCompany currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				currentCompany.GC_IsReciprocal = !originalIsReciprocal;
				Factory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
				ZString initialCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.SouthAfrica;
				AssertEquals("ZA", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				currentCompany.GC_IsReciprocal = originalIsReciprocal;
				Factory.Save();
				Db.Connection.RollbackTransaction();
				Db.Connection.BeginTransaction();
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals(initialCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("OldValue", oldValue, Env.CurrentCompany.ExchangeRate.ForeignToLocal(10m, 0.5m));
			}

			public void TestCountryIsReset()
			{
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				GlbCompany.CurrentCompany.SetCountry("FR");
				AssertEquals("PreCondition: AppTransactionCount", 1, Db.Connection.AppTransactionCount);
				Db.Connection.RollbackTransaction();
				Assert("PreCondition: IsInTransaction", !Db.Connection.IsInTransaction);
				Db.Connection.BeginTransaction();
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals("AU", GlbCompany.CurrentCompany.Country.Code);
			}

			public void TestHomePortIsReset()
			{
				var originalHomePortTimeZone = GlbBranch.CurrentBranch.HomePort.RL_R3;

				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				GlbBranch.CurrentBranch.HomePort.RL_R3 = GlbBranch.CurrentBranch.Factory.NewWithValidTestData<RefTimeZoneSet>().PK;
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals(originalHomePortTimeZone, GlbBranch.CurrentBranch.HomePort.RL_R3);

				listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				GlbBranch.CurrentBranch.HomePort.RL_R3 = GlbBranch.CurrentBranch.Factory.NewWithValidTestData<RefTimeZoneSet>().PK;
				GlbBranch.CurrentBranch.Factory.Save();
				Db.Connection.RollbackTransaction();
				Db.Connection.BeginTransaction();
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals(originalHomePortTimeZone, GlbBranch.CurrentBranch.HomePort.RL_R3);

				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				GlbBranch.CurrentBranch.HomePort.Delete();
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals(originalHomePortTimeZone, GlbBranch.CurrentBranch.HomePort.RL_R3);
			}

			public void TestHomePortTimeZoneIsReset()
			{
				var originalTimeZoneOffsetMinutesFromUtc = GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC;
				var newTimeZoneOffsetMinutesFromUtc = originalTimeZoneOffsetMinutesFromUtc - 300;

				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = newTimeZoneOffsetMinutesFromUtc;
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals(originalTimeZoneOffsetMinutesFromUtc, GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC);

				listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = newTimeZoneOffsetMinutesFromUtc;
				GlbBranch.CurrentBranch.Factory.Save();
				Db.Connection.RollbackTransaction();
				Db.Connection.BeginTransaction();
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals(originalTimeZoneOffsetMinutesFromUtc, GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC);

				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				GlbBranch.CurrentBranch.HomePort.TimeZoneSet.Delete();
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				AssertEquals(originalTimeZoneOffsetMinutesFromUtc, GlbBranch.CurrentBranch.HomePort.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC);
			}

			public void TestEnvInstanceIsReset()
			{
				var initialInstance = Env.Instance;
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				using (var provider = new NullEnvProvider())
				{
					provider.Enable();
					AssertNotEquals(Env.Instance.GetType(), initialInstance.GetType());
					AssertNotEquals(EnvProxy.Instance.GetType(), initialInstance.GetType());
					listener.AfterEachTest(ZDateTime.Now.ToDateTime());
					AssertEquals(initialInstance.GetType(), Env.Instance.GetType());
					AssertEquals(Env.Instance, EnvProxy.Instance);
				}
			}

			public void TestChangesAreRolledBackWhenPropertiesAreSaved()
			{
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());

				Assert("Pre-condition", !GlbCompany.CurrentCompany.GC_IsWHTRegistered);

				GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
				GlbCompany.CurrentCompany.Factory.Save();

				Assert(GlbCompany.CurrentCompany.GC_IsWHTRegistered);

				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
			}

			public void TestListenerCatchesChangesMadeInAnotherFactory_Company()
			{
				var originalUserContext = Env.CurrentUserContext;
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());

				Assert("Pre-condition", !GlbCompany.CurrentCompany.GC_IsWHTRegistered);
				GlbCompany.CurrentCompany.Factory.RefreshEnabled = false;

				var companyInCurrentFactory = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				companyInCurrentFactory.GC_IsWHTRegistered = true;
				Factory.Save();

				Assert(companyInCurrentFactory.GC_IsWHTRegistered);

				listener.AfterEachTest(ZDateTime.Now.ToDateTime());

				AssertNotEquals("user context has been updated as Company was changed in another factory",
					Env.CurrentUserContext.GetHashCode(), originalUserContext);
			}

			public void TestListenerCatchesChangesMadeInAnotherFactory_Branch()
			{
				var originalUserContext = Env.CurrentUserContext;
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());

				Assert("Pre-condition", GlbBranch.CurrentBranch.GB_Email.IsEmpty);

				GlbBranch.CurrentBranch.Factory.RefreshEnabled = false;

				var branchInTestFactory = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
				branchInTestFactory.GB_Email = "a@b.com";
				Factory.Save();

				AssertEquals("Expected branch to have changed", "a@b.com", branchInTestFactory.GB_Email);

				listener.AfterEachTest(ZDateTime.Now.ToDateTime());

				Assert("user context has been changed as Branch was changed in another factory", !ReferenceEquals(Env.CurrentUserContext, originalUserContext));
			}

			public void TestListenerCatchesChangesMadeInAnotherFactory_Staff()
			{
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());

				Assert("Pre-condition", GlbStaff.CurrentUser.GS_EmailAddress.IsEmpty);
				GlbStaff.CurrentUser.Factory.RefreshEnabled = false;

				var currentUserInTestFactory = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
				currentUserInTestFactory.GS_EmailAddress = "doesanyonestilluse@hotmail.com";
				Factory.Save();

				Assert(!currentUserInTestFactory.GS_EmailAddress.IsEmpty);

				try
				{
					listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				}
				catch (AssertionFailedError errorMessage)
				{
					AssertStartsWith("Should not allow tests that change CurrentUser by loading in another factory",
						"CurrentUser record should not be edited", errorMessage.Message.Replace("&nbsp;", " "));
				}
			}

			public void TestDbEnvIsReset()
			{
				var listener = new ZEnvironmentListener();
				using (var provider = new NullEnvProvider())
				{
					listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
					provider.Enable();
					listener.AfterEachTest(ZDateTime.Now.ToDateTime());
					AssertEquals("WinFormsDbEnvironment", DbEnv.Instance.GetType().Name);
				}
			}

			public void TestListenerCatchesDepartmentDeleted()
			{
				var originalUserContext = Env.CurrentUserContext;
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());

				GlbDepartment.CurrentDepartment.Delete();
				var code = GlbDepartment.CurrentDepartment.GE_Code;
				AssertType<DeletedRowInaccessibleException>(ExceptionReporterTestListener.Instance[0].InnerException);

				ExceptionReporterTestListener.Instance.Clear();
				listener.AfterEachTest(ZDateTime.Now.ToDateTime());
				Assert("User context has been changed as Department was deleted", !ReferenceEquals(Env.CurrentUserContext, originalUserContext));
				code = GlbDepartment.CurrentDepartment.GE_Code;
				AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			}

			public void TestCurrentThreadContextIsOverriden()
			{
				var listener = new ZEnvironmentListener();
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				var env = DisposableEnvironment.ForBranch(Env.CurrentBranch.PK);
				AssertExceptionThrown<AssertionFailedError>(() => listener.AfterEachTest(ZDateTime.Now.ToDateTime()));
				listener.BeforeEachTest(ZDateTime.Now.ToDateTime());
				AssertNoExceptionThrown(() => listener.AfterEachTest(ZDateTime.Now.ToDateTime()));
			}
		}
	}
	#endregion
}
