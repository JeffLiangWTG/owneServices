using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccPlaceOfSupplyConfigurationControl))]
	sealed class AccPlaceOfSupplyConfigurationControlTest : BasherTest
	{
		public override Form GetFormToBash() => CreateFormForTest();

		public void TestBranchColumnVisibility()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = CreateFormForTest())
			{
				var grid = form.FindSingle<ZGrid>("PlaceOfSupplyConfigurationGrid");
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == AccPOSConfiguration.Schema.PSC_NK_Branch);
				AssertNotNull("Branch column should be visible when registry is enabled", column);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = CreateFormForTest())
			{
				var grid = form.FindSingle<ZGrid>("PlaceOfSupplyConfigurationGrid");
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == AccPOSConfiguration.Schema.PSC_NK_Branch);
				AssertNull("Branch column should be hidden when registry is disabled", column);
			}
		}

		public void TestSupplyTypeColumnVisibility()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (var form = CreateFormForTest())
			{
				var grid = form.FindSingle<ZGrid>("PlaceOfSupplyConfigurationGrid");
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == AccPOSConfiguration.Schema.PSC_SupplyType);
				AssertNotNull("Supply Type column should be visible when registry is enabled", column);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (var form = CreateFormForTest())
			{
				var grid = form.FindSingle<ZGrid>("PlaceOfSupplyConfigurationGrid");
				var column = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName == AccPOSConfiguration.Schema.PSC_SupplyType);
				AssertNull("Supply Type column should be hidden when registry is disabled", column);
			}
		}

		#region Implementation

		ZChildForm CreateFormForTest()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Hungary;
			company.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			company.GC_Name = "Name";
			company.GC_Address1 = "Address 1";
			company.GC_City = "City";
			company.GC_PostCode = "1234";
			Factory.Save();     // Required to prevent basher test failures due to HasChanges = true

			var form = new ZChildForm() { CaptionRenderingEnabled = true };
			var userControl = new AccPlaceOfSupplyConfigurationControl();
			userControl.Dock = DockStyle.Fill;
			form.Controls.Add(userControl);
			form.SetDataBinding(company, nameof(GlbCompany.AccPlaceOfSupplyConfigurations));
			return form;
		}

		#endregion
	}
}
