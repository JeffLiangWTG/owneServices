using System.Collections;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CustomLabelControlFactoryTest : TestCaseWithFactory
	{
		public void TestAddDefaultColumns()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.Addresses.AddNew();
			using (var form = new ZForm(organisation))
			using (var grid = new ZGrid() { BindTo = "Addresses" })
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgAddressSchema.OA_Code.Name, 160));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgAddressSchema.OA_Address1.Name, 160));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgAddressSchema.OA_Address2.Name, 160));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgAddressSchema.OA_City.Name, 160));
				var originalColumnStyles = new ArrayList(grid.ColumnStyles);

				form.Controls.Add(grid);
				grid.SetDataBinding(organisation, "Addresses");
				form.Show();
				Application.DoEvents();

				var customLabelInfoList = new CustomLabelInfoList(typeof(OrgAddress), null, (NoResString)"", Factory);
				customLabelInfoList.Add("LabelName", OrgAddressSchema.OA_City.Name, (NoResString)"Column Caption");
				var controlFactory = new TestCustomLabelControlFactory(customLabelInfoList);

				controlFactory.ClearCustomFieldGridColumns(grid);
				AssertEquals("Column should be removed from DefaultColumns of grid when call the method ClearCustomFieldGridColumns", false, grid.DefaultColumns.Contains(OrgAddressSchema.OA_City.Name));
				controlFactory.AddOrRefreshCustomFieldGridColumns(grid, string.Empty);
				AssertEquals("Column should be added to the DefaultColumns of grid when call the method AddOrRefreshCustomFieldGridColumns", true, grid.DefaultColumns.Contains(OrgAddressSchema.OA_City.Name));

				controlFactory.ClearCustomFieldGridColumns(grid);
				AssertEquals("Column should be removed from DefaultColumns of grid when call the method ClearCustomFieldGridColumns", false, grid.DefaultColumns.Contains(OrgAddressSchema.OA_City.Name));
				controlFactory.ResetToOriginalCustomColumns(grid, string.Empty, originalColumnStyles);
				AssertEquals("Column should be added to the DefaultColumns of grid when call the method ResetToOriginalCustomColumns", true, grid.DefaultColumns.Contains(OrgAddressSchema.OA_City.Name));
			}
		}

		[RequiresSTA]
		public void TestResetToOriginalCustomColumns()
		{
			var organisation = Factory.New<OrgHeader>();
			using (var form = new ZForm(organisation))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.PK.Name, 170));
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.OH_Code.Name, 10) { Caption = "Original caption" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.OH_FullName.Name, 160));

				var originalColumnStyles = new ArrayList(grid.ColumnStyles);
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				CustomLabelInfoList.Add("LabelName", OrgHeaderSchema.OH_Code.Name, (NoResString)"Column Caption");
				ControlFactory.AddOrRefreshCustomFieldGridColumns(grid, string.Empty);
				AssertEquals("Column should be added to the grid", true, grid.Columns.Contains(OrgHeaderSchema.OH_Code.Name));
				AssertEquals("Column should have the correct caption", "Column Caption", grid.Columns[OrgHeaderSchema.OH_Code.Name].ColumnStyle.HeaderText);

				ControlFactory.ResetToOriginalCustomColumns(grid, string.Empty, originalColumnStyles);
				AssertEquals("Column should still exist in the grid", true, grid.Columns.Contains(OrgHeaderSchema.OH_Code.Name));
				AssertEquals("Column should have the original column caption", "Original caption", grid.Columns[OrgHeaderSchema.OH_Code.Name].ColumnStyle.HeaderText);
			}
		}

		[RequiresSTA]
		public void TestAddOrRefreshCustomFieldGridColumns()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			using (ZForm form = new ZForm(organisation))
			using (ZGrid grid = new ZGrid())
			{
				grid.Columns.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.OH_Code.Name, 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				CustomLabelInfoList.Add("LabelName", OrgHeaderSchema.OH_Code.Name, (NoResString)"Column Caption");
				ControlFactory.AddOrRefreshCustomFieldGridColumns(grid, "");
				AssertEquals("Column should be added to the grid", true, grid.Columns.Contains(OrgHeaderSchema.OH_Code.Name));
				AssertEquals("Column should have the correct caption", "Column Caption", grid.Columns[OrgHeaderSchema.OH_Code.Name].ColumnStyle.HeaderText);

				CustomLabelInfoList.Add("LabelName", OrgHeaderSchema.OH_Code.Name, (NoResString)"Renamed Column Caption");
				ControlFactory.AddOrRefreshCustomFieldGridColumns(grid, "");
				AssertEquals("Column should still exist in the grid", true, grid.Columns.Contains(OrgHeaderSchema.OH_Code.Name));
				AssertEquals("Column should have the correct, now renamed, caption", "Renamed Column Caption", grid.Columns[OrgHeaderSchema.OH_Code.Name].ColumnStyle.HeaderText);
			}
		}

		public void TestGetCustomisedMappingName()
		{
			CustomLabelInfoList.Add("LabelName", OrgHeaderSchema.OH_Code.Name, (NoResString)"Code", CustomLabelStyles.ShowByDefault);
			CustomLabelInfoList.Add("LabelName2", OrgHeaderSchema.OH_FullName.Name, (NoResString)"Desc", CustomLabelStyles.AvailableByDefault);

			string[] customisedColumns = ControlFactory.GetCustomisedMappingNames("", false);

			AssertEquals(2, customisedColumns.Length);
		}

		[RequiresSTA]
		public void TestAddOrRefreshCustomFieldGridColumns_WithColumnPrefix()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			using (ZForm form = new ZForm(organisation))
			using (ZGrid grid = new ZGrid())
			{
				grid.Columns.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.OH_Code.Name, 10));
				form.Controls.Add(grid);
				form.Show();
				Application.DoEvents();

				CustomLabelInfoList.Add("LabelName", OrgHeaderSchema.OH_Code.Name, (NoResString)"Column Caption");
				ControlFactory.AddOrRefreshCustomFieldGridColumns(grid, "Prefix.");
				AssertEquals("Column should be added to the grid", true, grid.Columns.Contains("Prefix+" + OrgHeaderSchema.OH_Code.Name));
				AssertEquals("Column should have the correct caption", "Column Caption", grid.Columns["Prefix+" + OrgHeaderSchema.OH_Code.Name].ColumnStyle.HeaderText);
			}
		}

		public void TestNewCustomFieldGridColumn()
		{
			ZGridColumnInfo column = GetNewCustomFieldGridColumnForTest(CustomLabelStyles.AvailableByDefault);
			AssertEquals("Column Type", true, column is ZDateEditColumnStyleInfo);
			AssertEquals("caption", column.Caption);
		}

		public void TestNewCustomFieldGridColumn_ShownFalseAvailableTrue()
		{
			ZGridColumnInfo column = GetNewCustomFieldGridColumnForTest(CustomLabelStyles.AvailableByDefault);
			AssertEquals("Unavailable when ShownByDefault=false and AvailableByDefault=true", false, column.IsUnavailable);
		}

		public void TestNewCustomFieldGridColumn_ShownTrueAvailableFalse()
		{
			ZGridColumnInfo column = GetNewCustomFieldGridColumnForTest(CustomLabelStyles.ShowByDefault);
			AssertEquals("Unavailable when ShownByDefault=false and AvailableByDefault=true", false, column.IsUnavailable);
		}

		public void TestNewCustomFieldGridColumn_ShownFalseAvailableFalse()
		{
			ZGridColumnInfo column = GetNewCustomFieldGridColumnForTest(CustomLabelStyles.None);
			AssertEquals("Unavailable when ShownByDefault=false and AvailableByDefault=true", true, column.IsUnavailable);
		}

		public void TestNewCustomFieldGridColumn_ShownTrueAvailableTrue()
		{
			ZGridColumnInfo column = GetNewCustomFieldGridColumnForTest(CustomLabelStyles.ShowByDefault | CustomLabelStyles.AvailableByDefault);
			AssertEquals("Unavailable when ShownByDefault=false and AvailableByDefault=true", false, column.IsUnavailable);
		}

		public void TestTextBoxControlStyle_UpperCase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CustomLabelInfoList list = new CustomLabelInfoList(typeof(OrgHeader), factory.New<OrgHeader>(), (NoResString)"", factory);

			list.Add("label", "prop", typeof(ZString), (NoResString)"caption", (NoResString)"hint", CustomLabelStyles.ShowByDefault | CustomLabelStyles.UpperCase);
			using (ZTextBox ctrl = (ZTextBox)new CustomLabelControlFactory(list).NewCustomFieldControl(list[0]))
			{
				AssertEquals("CustomLabelStyles.UpperCase", CharacterCasing.Upper, ctrl.CharacterCasing);
			}
		}

		public void TestMultiLineTextBoxStyle()
		{
			CustomLabelInfoList list = new CustomLabelInfoList(typeof(OrgHeader), null, (NoResString)"", Factory);
			list.Add("MultiLineTextBox", "prop", typeof(ZString), (NoResString)"caption", (NoResString)"hint", CustomLabelStyles.MultiLineTextBox);
			ZGridColumnInfo column = new ZDropEditColumnStyleInfo();
			TestCustomLabelControlFactory testFactory = new TestCustomLabelControlFactory(list);
			ZGridColumnInfo column1 = testFactory.NewCustomFieldGridColumn(list[0], "", column);
			AssertEquals("Column should be multi-line text box", true, column1 is ZMultiLineTextBoxColumnInfo);
		}

		#region Test Classes

		class TestCustomLabelControlFactory : CustomLabelControlFactory
		{
			public TestCustomLabelControlFactory(CustomLabelInfoList customFields)
				: base(customFields)
			{
			}

			public new ZGridColumnInfo NewCustomFieldGridColumn(CustomLabelInfoBase field, string mappingPrefix, ZGridColumnInfo column)
			{
				return base.NewCustomFieldGridColumn(field, mappingPrefix, column);
			}
		}

		#endregion

		#region Implementation

		ZGridColumnInfo GetNewCustomFieldGridColumnForTest(CustomLabelStyles styles)
		{
			CustomLabelInfoList list = new CustomLabelInfoList(typeof(OrgHeader), null, (NoResString)"", Factory);
			list.Add("label", "prop", typeof(ZDateTime), (NoResString)"caption", (NoResString)"hint", styles);
			TestCustomLabelControlFactory controlFactory = new TestCustomLabelControlFactory(list);
			ZGridColumnInfo column = new ZDropEditColumnStyleInfo();

			ZGridColumnInfo column1 = controlFactory.NewCustomFieldGridColumn(list[0], "", column);
			return column1;
		}

		CustomLabelInfoList CustomLabelInfoList
		{
			get
			{
				if (fCustomLabelInfoList == null)
				{
					fCustomLabelInfoList = new CustomLabelInfoList(typeof(OrgHeader), null, (NoResString)"", Factory);
				}
				return fCustomLabelInfoList;
			}
		}
		CustomLabelInfoList fCustomLabelInfoList;

		TestCustomLabelControlFactory ControlFactory
		{
			get
			{
				if (fControlFactory == null)
				{
					fControlFactory = new TestCustomLabelControlFactory(CustomLabelInfoList);
				}
				return fControlFactory;
			}
		}
		TestCustomLabelControlFactory fControlFactory;

		#endregion
	}
}
