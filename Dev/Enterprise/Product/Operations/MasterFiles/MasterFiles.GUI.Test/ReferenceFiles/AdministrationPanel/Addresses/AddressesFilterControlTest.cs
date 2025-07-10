using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.MasterFiles.GUI.AddressesFilterBusinessObject;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressesFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestValidationStatusFilterAlwaysVisible()
		{
			var collection = new MDMAdminPanelAddressCollection(Factory);
			var filterBizO = new AddressesFilterBusinessObject();
			using (var form = new ZForm())
			using (var control = new AddressesFilterControlForTest(collection, filterBizO))
			{
				form.Controls.Add(control);
				form.Show();
				ModuleTextFilter validationStatusFilter = null;
				AssertNoExceptionThrown("Always Visible Filters contains validation status filter", () => validationStatusFilter = control.FilterBusinessObject.AlwaysVisibleModuleFilters.First(f => f.Description == AddressFilterConstants.ValidationStatus) as ModuleTextFilter);
				AssertNotNull(validationStatusFilter);
				AssertEquals("filter default property is INV", AddressValidationStatus.Invalid, validationStatusFilter.Property);
				AssertEquals("SQL comparision operator is equal", SQLComparisonOperator.Equal, validationStatusFilter.SqlComparisonOperator);

				validationStatusFilter.Property = "123";
				validationStatusFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

				control.ResetFilterStrips();
				AssertNoExceptionThrown("Always Visible Filters still contains validation status filter", () => validationStatusFilter = control.FilterBusinessObject.AlwaysVisibleModuleFilters.First(f => f.Description == AddressFilterConstants.ValidationStatus) as ModuleTextFilter);
				AssertNotNull(validationStatusFilter);
				AssertEquals("filter default property goes back to INV", AddressValidationStatus.Invalid, validationStatusFilter.Property);
				AssertEquals("SQL comparision operator goes back to equal", SQLComparisonOperator.Equal, validationStatusFilter.SqlComparisonOperator);
			}
		}

		[RequiresSTA]
		public void TestBackgroundValidationStatusIcon()
		{
			MDMAdminPanelAddressCollection collection = new MDMAdminPanelAddressCollection(Factory);
			AddressesFilterBusinessObject filterBizO = new AddressesFilterBusinessObject();
			using (var form = new ZForm())
			using (var control = new AddressesFilterControlForTest(collection, filterBizO))
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("The BackgroundValidationStatusIcon should be hidden", false, control.BackgroundValidationStatusIcon.Visible);
				AssertEquals("The ToolTip should be 'The background validation is running...'", "The background validation is running...", ToolTipService.GetToolTip(control.BackgroundValidationStatusIcon));
			}
		}

		[RequiresSTA]
		public void TestShowTextInRecordsFoundLabelWhenSearch()
		{
			var collection = new MDMAdminPanelAddressCollection(Factory);
			var filterBizO = new AddressesFilterBusinessObject();
			using (var form = new ZForm())
			using (var control = new AddressesFilterControlForTest(collection, filterBizO))
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", string.Empty, control.ToolStripRecordsFoundLabelForTest.Text);

				control.LoadData();
				AssertEquals("Should show text in label", "    Found\r\n  no records", control.ToolStripRecordsFoundLabelForTest.Text);
			}
		}

		[RequiresSTA]
		public void TestFilterControlContainsColumnParentTypeAndCode()
		{
			var collection = new MDMAdminPanelAddressCollection(Factory);
			var filterBizO = new AddressesFilterBusinessObject();
			using (var control = new AddressesFilterControlForTest(collection, filterBizO))
			{
				var columnStyle = control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "MDM_TableName"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "MDM_NaturalKey"));
			}
		}
	}
}
