using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Module.Testing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Module.Testing
{
	[TestedType(typeof(TransitHandlingUnitModule))]
	public class TransitHandlingUnitModuleTest : HandlingUnitModuleTest<TransitHandlingUnitModule>
	{
		protected override Type ExpectedFilterBusinessObjectType => typeof(TransitHandlingUnitFilterBusinessObject);

		protected override Type ExpectedFilterControlType => typeof(TransitHandlingUnitFilterControl);

		protected override Type ExpectedCollectionType => typeof(PkgHandlingUnitCollection);

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.PkgHandlingUnit;

		#region TestWarehouseFilter

		public void TestWarehouseFilter()
		{
			if (!string.IsNullOrWhiteSpace(ExpectedWarehouseSchema))
			{
				using (var module = new TransitHandlingUnitModule())
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

				using (var module = new TransitHandlingUnitModule())
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

		string ExpectedWarehouseSchema => TransitHandlingUnitFilterBusinessObject.Schema.Warehouse;

		#endregion

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.TransitHandlingUnit;

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.TransitWarehouse;

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
