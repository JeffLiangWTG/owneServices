using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestsSubclassesOf(typeof(WhsTransitModule))]
	public abstract class WhsTransitModuleTest<T> : GlowOnlyModuleTest<T>
		where T : WhsTransitModule, new()
	{
		protected override bool ExpectedAllowView => false;

		protected override bool ExpectedAllowEdit => false;

		protected override bool ExpectedSupportsWorkflow => true;

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.TransitWarehouse;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.TransitWarehouse;

		#region TestWarehouseFilter

		public void TestWarehouseFilter()
		{
			if (!string.IsNullOrWhiteSpace(ExpectedWarehouseSchema))
			{
				using (var module = new T())
				{
					using (var filterControl = (IDisposable)module.GetNewFilterControlForGrid())
					{
						var controller = (ZFilterStripControl)filterControl;
						var filter = (ModuleGuidFilter)controller.FilterBusinessObject.ModuleFilters[ExpectedWarehouseSchema];
						filter.IsActive = true;
						AssertEquals(filter.Visibility, FilterVisibility.AlwaysAppliedAndHidden);
						AssertEquals(filter.DefaultProperty, TransitWarehouseHelper.GetTransitWarehouseInCurrentBranch(Factory).PK);
						AssertEquals(filter.ComparisonOperator, ModuleGuidFilter.ComparisonConstants.Exact);

						controller.Find();
						AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}

				var branch = Helper.CreateGlbBranch("BR2");
				TWInCurrentBranch.WW_GB_RelatedCompanyBranch = branch.PK;
				Factory.Save();

				using (var module = new T())
				{
					using (var filterControl = (IDisposable)module.GetNewFilterControlForGrid())
					{
						var controller = (ZFilterStripControl)filterControl;
						var filter = (ModuleGuidFilter)controller.FilterBusinessObject.ModuleFilters[ExpectedWarehouseSchema];
						filter.IsActive = true;
						AssertEquals(filter.Visibility, FilterVisibility.AlwaysAppliedAndHidden);
						AssertEquals(filter.DefaultProperty, ZGuid.Empty);
						AssertEquals(filter.ComparisonOperator, ModuleGuidFilter.ComparisonConstants.Exact);

						controller.Find();
						AssertEquals(
							$"You are logged into branch '{GlbBranch.CurrentBranch.GB_Code} - {GlbBranch.CurrentBranch.GB_BranchName}' that doesn't have a Transit Warehouse setup. \r\nPlease log into a branch that is linked to a Transit Warehouse.",
							UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}

			Assert(true);
		}

		protected virtual string ExpectedWarehouseSchema => string.Empty;

		#endregion

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		protected WhsWarehouse TWInCurrentBranch;

		protected override void SetUp()
		{
			base.SetUp();

			TWInCurrentBranch = TransitWarehouseHelper.GetTransitWarehouseInCurrentBranch(Factory);
			if (TWInCurrentBranch == null)
			{
				TWInCurrentBranch = Helper.CreateTransitWarehouseInCurrentBranch();
				Factory.Save();
			}
		}
	}
}
