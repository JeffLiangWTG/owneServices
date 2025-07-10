using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	internal sealed class JobConsolBulkPostingModuleTest : BulkPostingModuleTest
	{
		#region Implementation

		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return new JobConsolModuleForBulkPostingModuleInternalsTesting();
		}

		protected override ZBool IsPostingConsolBulk => ZBool.True;

		#endregion
	}

	class JobConsolModuleForBulkPostingModuleInternalsTesting : JobConsolModule, IBulkPostingModuleInternalsForTesting
	{
		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest => lastUsedPostingOptionForTest;

		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				var menuItem = GetNewActionMenuItems().FindByText("&Post");
				IMenuItem iMenuItem = menuItem as IMenuItem;
				if (menuItem != null && iMenuItem == null)
				{
					throw new InvalidOperationException("Post menu item should have type ZMenuItem");
				}
				return iMenuItem;
			}
		}

		protected override void SetLastUsedPostingOptionForTest(JobInvoicingPostingOption postingOption)
		{
			lastUsedPostingOptionForTest = postingOption;
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}

		JobInvoicingPostingOption lastUsedPostingOptionForTest;
	}
}
