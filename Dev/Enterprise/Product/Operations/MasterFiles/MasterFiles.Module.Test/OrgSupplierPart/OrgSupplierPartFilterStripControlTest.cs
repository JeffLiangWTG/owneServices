using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgSupplierPartFilterStripControlTest : TestCaseWithFactory
	{
		#region TestOverrideShouldShowNumberLoadedMessageBox

		[RequiresSTA]
		public void TestOverrideShouldShowNumberLoadedMessageBox()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			bool searched = false;

			using (ZForm form = new ZForm())
			using (OrgSupplierPartModuleForTest module = new OrgSupplierPartModuleForTest())
			{
				OrgSupplierPartFilterStripControl filter = (OrgSupplierPartFilterStripControl)module.EmbeddedControl;
				filter.OverrideShouldShowNumberLoadedMessageBox = true;
				form.Controls.Add(filter);
				form.Show();

				filter.PerformSearch += delegate
				{ searched = true; };
				filter.FirePerformSearch();
				Assert("Should have searched", searched);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestCustomFieldsColumns

		[RequiresSTA]
		public void TestCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new OrgSupplierPartCollection(Factory);
			var filter = new OrgSupplierPartFilterStripBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new OrgSupplierPartFilterStripControl(collection, filter))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}

		#endregion

		class OrgSupplierPartModuleForTest : OrgSupplierPartModule
		{
			public OrgSupplierPartModuleForTest() { }
		}

		[RequiresSTA]
		public void TestFilteredGridFields()
		{
			var collection = new OrgSupplierPartCollection(Factory);
			var filter = new OrgSupplierPartFilterStripBusinessObject();
			using var filterControl = new OrgSupplierPartFilterStripControl(collection, filter);
			using var form = new ZForm();
			form.Controls.Add(filterControl);

			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals(OrgSupplierPart.Schema.OP_PartNum, "Code", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_PartNum));
				AssertEquals(OrgSupplierPart.Schema.OP_Desc, "Description", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Desc));
				AssertEquals(OrgSupplierPart.Schema.OP_RH_NKCommodityCode, "Commodity", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_RH_NKCommodityCode));
				AssertEquals(OrgSupplierPart.Schema.OP_Brand, "Brand", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Brand));
				AssertEquals(OrgSupplierPart.Schema.OP_Model, "Model", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Model));
				AssertEquals(OrgSupplierPart.Schema.AllOwners, "Owner(s)", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.AllOwners));
				AssertEquals(OrgSupplierPart.Schema.AllSuppliers, "Supplier(s)", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.AllSuppliers));
				AssertEquals(OrgSupplierPart.Schema.OP_Cubic, "Cubic", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Cubic));
				AssertEquals(OrgSupplierPart.Schema.OP_CubicUQ, "Cubic UQ", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_CubicUQ));
				AssertEquals(OrgSupplierPart.Schema.OP_MeasureUQ, "Measure UQ", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_MeasureUQ));
				AssertEquals(OrgSupplierPart.Schema.OP_NetWeight, "Net Weight", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_NetWeight));
				AssertEquals(OrgSupplierPart.Schema.OP_Weight, "Gross Weight", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Weight));
				AssertEquals(OrgSupplierPart.Schema.OP_WeightUQ, "Weight UQ", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_WeightUQ));
				AssertEquals(OrgSupplierPart.Schema.OP_QtyInStock, "Stock Qty", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_QtyInStock));
				AssertEquals(OrgSupplierPart.Schema.OP_StockKeepingUnit, "Stock UQ", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_StockKeepingUnit));
				AssertEquals(OrgSupplierPart.Schema.OP_LastCost, "Last Cost", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_LastCost));
				AssertEquals(OrgSupplierPart.Schema.OP_WeightedCost, "Weighted Cost", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_WeightedCost));
				AssertEquals(OrgSupplierPart.Schema.OP_Height, "Height", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Height));
				AssertEquals(OrgSupplierPart.Schema.OP_Depth, "Depth", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Depth));
				AssertEquals(OrgSupplierPart.Schema.OP_Width, "Width", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Width));
				AssertEquals(OrgSupplierPart.Schema.OP_Division, "Division", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Division));
				AssertEquals(OrgSupplierPart.Schema.OP_Department, "Department", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_Department));
				AssertEquals(OrgSupplierPart.Schema.OP_OrderMultipleQty, "Order Multiple", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_OrderMultipleQty));
				AssertEquals(OrgSupplierPart.Schema.OP_OrderMultipleUnit, "Order Multiple UQ", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_OrderMultipleUnit));
				AssertEquals(OrgSupplierPart.Schema.OP_VendorPackQty, "Vendor Pack", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_VendorPackQty));
				AssertEquals(OrgSupplierPart.Schema.OP_F3_NKPackType, "Vendor Pack UQ", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_F3_NKPackType));
				AssertEquals(OrgSupplierPart.Schema.AllLocalParts, "Local Part(s)", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.AllLocalParts));
				AssertEquals(OrgSupplierPart.Schema.AllLocalPartDescriptions, "Local Part Description(s)", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.AllLocalPartDescriptions));
				AssertEquals(OrgSupplierPart.Schema.OP_IsActive, "Is Active?", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.OP_IsActive));
				AssertEquals(OrgSupplierPart.Schema.AllProductCategories, "Category(s)", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.AllProductCategories));
				AssertEquals(OrgSupplierPart.Schema.IsBOMProduct, "Is BOM", filterControl.FilteredGrid.GetColumnCaption(OrgSupplierPart.Schema.IsBOMProduct));
			});
		}
	}
}
