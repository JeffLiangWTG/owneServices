using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Accounts;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TransactionsTest : BasePageWithAuthorisationTest
	{
		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.Transactions;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new TransactionsForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebAccountsModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebInvoicingAndStatements; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Env.Licence.Accountant }; }
		}

		#endregion

		class TransactionsForTest : Transactions
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}
		}
	}
}
