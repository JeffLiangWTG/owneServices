using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CustomLabelsGridLayoutPersisterTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestRefreshGridLayout_ResetToOriginalColumns()
		{
			var org = Factory.New<OrgHeader>();
			var configOrg = org.ConfigOrgProvider.ConfigOrg;
			var proxyOrg = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			configOrg.OH_Code = "AAA";
			configOrg.CustomLabels.RemoveAll();
			configOrg.MiscServ.OM_IMPartAttrib1Name = "CCC";
			proxyOrg.OH_Code = "BBB";
			configOrg.CustomLabels.RemoveAll();
			CLearPartAttributes(proxyOrg);
			Factory.Save();

			string[] orginalVisibleColumns = null;
			using (var form = new ZGridTestForm(org))
			{
				form.Show();
				form.InternalGrid.SetColumnVisible(false, OrgHeaderSchema.PK.Name);
				form.InternalGrid.SetColumnVisible(false, OrgHeaderSchema.OH_Language.Name);
				form.InternalGrid.SetColumnVisible(true, OrgHeaderSchema.OH_Code.Name);
				form.InternalGrid.SetColumnVisible(true, OrgHeaderSchema.OH_FullName.Name);

				orginalVisibleColumns = form.InternalGrid.Columns.GetVisibleColumnMappingNames();
				form.InternalGrid.Columns.HasLayoutChanged = true;
				form.InternalGrid.SaveUserLayoutSettings();
				form.Close();
			}

			Factory.CleanUp();

			using (var form = new ZGridTestForm(org))
			using (new CustomLabelsGridLayoutPersister(form.InternalGrid, form.DataSource as OrgHeader, "", "OH"))
			{
				form.Show();
				CombineAssertions("Visibility should be same as last time user setting", () =>
				{
					AssertEquals(false, form.InternalGrid.Columns[OrgHeaderSchema.PK.Name].IsVisible);
					AssertEquals(false, form.InternalGrid.Columns[OrgHeaderSchema.OH_Language.Name].IsVisible);
					AssertEquals(true, form.InternalGrid.Columns[OrgHeaderSchema.OH_Code.Name].IsVisible);
					AssertEquals(true, form.InternalGrid.Columns[OrgHeaderSchema.OH_FullName.Name].IsVisible);
				});

				var currentVisibleColumns = form.InternalGrid.Columns.GetVisibleColumnMappingNames();
				for (int i = 0; i < currentVisibleColumns.Length; i++)
				{
					AssertEquals("All visible columns should be value equals to respective original column with same order", true, currentVisibleColumns[i].Equals(orginalVisibleColumns[i]));
				}

				form.Close();
			}
		}

		public void TestRefreshGridLayout_RefreshNewConfigOrg()
		{
			Form.Controls.Add(Grid);
			Form.Show();
			Application.DoEvents();
			var configOrg = Organisation.ConfigOrgProvider.ConfigOrg;
			var proxyOrg = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			configOrg.OH_Code = "AAA";
			configOrg.CustomLabels.RemoveAll();
			CLearPartAttributes(configOrg);
			proxyOrg.OH_Code = "BBB";
			proxyOrg.CustomLabels.RemoveAll();
			CLearPartAttributes(proxyOrg);

			proxyOrg.MiscServ.OM_IMPartAttrib1Name = "AAA";
			proxyOrg.MiscServ.OM_IMPartAttrib1Type = "BBB";
			proxyOrg.MiscServ.OM_IMPartAttrib1IsMandatory = true;
			Factory.Save();

			var gridLayoutPersister = LayoutPersister;
			gridLayoutPersister.RefreshGridLayout();

			CombineAssertions("PartAttributeName should be get from proxy org", () =>
			{
				AssertEquals("Should be equal", proxyOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
				AssertPartAttributeInfo(gridLayoutPersister.CurrentConfigOrgForGridLayout, "AAA", "BBB", true);
			});

			configOrg.MiscServ.OM_IMPartAttrib1Name = "CCC";
			configOrg.MiscServ.OM_IMPartAttrib1Type = "DDD";
			configOrg.MiscServ.OM_IMPartAttrib1IsMandatory = false;
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();

			CombineAssertions("PartAttributeName should be get from current config org", () =>
			{
				AssertNotNull("Pre-Condition: CurrentConfigOrgForGridLayout should not be null", gridLayoutPersister.CurrentConfigOrgForGridLayout);
				AssertEquals("Should be equal", configOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
				AssertPartAttributeInfo(gridLayoutPersister.CurrentConfigOrgForGridLayout, "CCC", "DDD", false);
			});
		}

		[RequiresSTA]
		public void TestGridLayout_DoesntThrowExceptionWhenGridIsNotBound()
		{
			ZTextBoxColumnStyleInfo column1 = new ZTextBoxColumnStyleInfo();
			column1.ColumnName = DummyDependentBizoSchema.ZD1_Code.Name;
			ZTextBoxColumnStyleInfo column2 = new ZTextBoxColumnStyleInfo();
			column2.ColumnName = DummyDependentBizoSchema.ZD1_NumberUnitCode.Name;
			Grid.ColumnStyles.Add(column1);
			Grid.ColumnStyles.Add(column2);

			Form.Controls.Add(Grid);
			Form.Show();
			Application.DoEvents();

			CustomLabelsGridLayoutPersister gridLayoutPersisterEnabled = LayoutPersister;
			Grid.ReOrderColumns(new string[] { DummyDependentBizoSchema.ZD1_NumberUnitCode.Name, DummyDependentBizoSchema.ZD1_Code.Name });

			AssertNoExceptionThrown("Changing custom labels", () => Organisation.CustomLabels.AddNew().OT_Caption = "X");
			AssertNoExceptionThrown("Closing form", Form.Close);
		}

		[RequiresSTA]
		public void TestRefreshGridLayout_GetCorrectNewConfigOrgFromOrgProxy()
		{
			Form.Controls.Add(Grid);
			Form.Show();
			Application.DoEvents();

			var configOrg = Organisation.ConfigOrgProvider.ConfigOrg;
			var proxyOrg = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			configOrg.OH_Code = "AAA";
			configOrg.CustomLabels.RemoveAll();
			CLearPartAttributes(configOrg);
			proxyOrg.OH_Code = "BBB";
			proxyOrg.CustomLabels.RemoveAll();
			CLearPartAttributes(proxyOrg);
			Factory.Save();

			var gridLayoutPersister = LayoutPersister;
			gridLayoutPersister.RefreshGridLayout();
			AssertEquals("CurrentConfigOrgForGridLayout should be null", null, gridLayoutPersister.CurrentConfigOrgForGridLayout);

			configOrg.CustomLabels.AddNew().OT_Caption = "XXX";
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();
			AssertEquals("Should be equal", configOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
			AssertEquals("Should contain one CustomLabel", 1, gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels.Count);
			AssertEquals("Should equal", "XXX", gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels[0].OT_Caption);

			proxyOrg.CustomLabels.AddNew().OT_Caption = "YYY";
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();
			AssertEquals("Should be equal", configOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
			AssertEquals("Should contain one CustomLabel", 1, gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels.Count);
			AssertEquals("Should equal", "XXX", gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels[0].OT_Caption);

			configOrg.CustomLabels[0].Delete();
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();
			AssertEquals("Should be equal", proxyOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
			AssertEquals("Should contain one CustomLabel", 1, gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels.Count);
			AssertEquals("Should equal", "YYY", gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels[0].OT_Caption);

			proxyOrg.CustomLabels[0].Delete();
			Factory.Save();

			proxyOrg.MiscServ.OM_IMPartAttrib1Name = "AAA";
			proxyOrg.MiscServ.OM_IMPartAttrib1Type = "BBB";
			proxyOrg.MiscServ.OM_IMPartAttrib1IsMandatory = true;
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();
			AssertEquals("Should be equal", proxyOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
			AssertPartAttributeInfo(gridLayoutPersister.CurrentConfigOrgForGridLayout, "AAA", "BBB", true);

			configOrg.MiscServ.OM_IMPartAttrib1Name = "CCC";
			configOrg.MiscServ.OM_IMPartAttrib1Type = "DDD";
			configOrg.MiscServ.OM_IMPartAttrib1IsMandatory = false;
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();
			AssertNotNull("CurrentConfigOrgForGridLayout should not be null", gridLayoutPersister.CurrentConfigOrgForGridLayout);

			CLearPartAttributes(configOrg);
			gridLayoutPersister.RefreshGridLayout();

			configOrg.MiscServ.OM_IMPartAttrib1Name = "CCC";
			configOrg.MiscServ.OM_IMPartAttrib1Type = "DDD";
			configOrg.MiscServ.OM_IMPartAttrib1IsMandatory = false;
			proxyOrg.CustomLabels.AddNew().OT_Caption = "YYY";
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();
			AssertEquals("Should be equal", configOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
			AssertPartAttributeInfo(gridLayoutPersister.CurrentConfigOrgForGridLayout, "CCC", "DDD", false);
			AssertEquals("Should contain one CustomLabel", 1, gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels.Count);
			AssertEquals("Should equal", "YYY", gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels[0].OT_Caption);

			CLearPartAttributes(configOrg);
			Factory.Save();

			gridLayoutPersister.RefreshGridLayout();
			AssertEquals("Should be equal", proxyOrg.PK, gridLayoutPersister.CurrentConfigOrgForGridLayout.PK);
			AssertPartAttributeInfo(gridLayoutPersister.CurrentConfigOrgForGridLayout, "AAA", "BBB", true);
			AssertEquals("Should contain one CustomLabel", 1, gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels.Count);
			AssertEquals("Should equal", "YYY", gridLayoutPersister.CurrentConfigOrgForGridLayout.CustomLabels[0].OT_Caption);
		}

		[RequiresSTA]
		public void TestRefreshGridLayout_ShouldNotThrowExceptionWhenOrgProxyIsNull()
		{
			Form.Controls.Add(Grid);
			Form.Show();
			Application.DoEvents();

			var rawOrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.BrettsGuid;

			var gridLayoutPersister = LayoutPersister;
			AssertNoExceptionThrown("Should not throw exception when OrgProxy is null.", gridLayoutPersister.RefreshGridLayout);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = rawOrgProxy;
			AssertNoExceptionThrown("Should not throw exception when OrgProxy is not null.", gridLayoutPersister.RefreshGridLayout);
		}

		#region Implementation

		class ZGridTestForm : ZForm
		{
			public ZGridTestForm() : base() { }
			public ZGridTestForm(object dataSource) : base(dataSource) { }

			protected override void InitialiseForm()
			{
				InternalGrid = new ZGrid();
				InternalGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.PK.Name, 80));
				InternalGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.OH_Code.Name, 80));
				InternalGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.OH_FullName.Name, 80));
				InternalGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(OrgHeaderSchema.OH_Language.Name, 80));

				InternalGrid.BindingContext = new BindingContext();
				InternalGrid.SetDataBinding(DataSource, "");
				Controls.Add(InternalGrid);
			}

			public ZGrid InternalGrid;
		}

		void CLearPartAttributes(OrgHeader orgHeader)
		{
			orgHeader.MiscServ.OM_IMPartAttrib1Name = ZString.Empty;
			orgHeader.MiscServ.OM_IMPartAttrib2Name = ZString.Empty;
			orgHeader.MiscServ.OM_IMPartAttrib3Name = ZString.Empty;

			orgHeader.MiscServ.OM_IMPartAttrib1Type = ZString.Empty;
			orgHeader.MiscServ.OM_IMPartAttrib2Type = ZString.Empty;
			orgHeader.MiscServ.OM_IMPartAttrib3Type = ZString.Empty;

			orgHeader.MiscServ.OM_IMPartAttrib1IsMandatory = false;
			orgHeader.MiscServ.OM_IMPartAttrib2IsMandatory = false;
			orgHeader.MiscServ.OM_IMPartAttrib3IsMandatory = false;
		}

		void AssertPartAttributeInfo(OrgHeader orgHeader, string partAttribName, string partAttribType, bool partAttribIsMandatory)
		{
			AssertEquals(orgHeader.MiscServ.OM_IMPartAttrib1Name, partAttribName);
			AssertEquals(orgHeader.MiscServ.OM_IMPartAttrib1Type, partAttribType);
			AssertEquals(orgHeader.MiscServ.OM_IMPartAttrib1IsMandatory, partAttribIsMandatory);
		}

		ZForm Form
		{
			get { return form ?? (form = new ZForm()); }
		}
		ZForm form;

		CustomLabelsGridLayoutPersister LayoutPersister
		{
			get { return layoutPersister ?? (layoutPersister = new CustomLabelsGridLayoutPersister(Grid, Organisation)); }
		}
		CustomLabelsGridLayoutPersister layoutPersister;

		ZGrid Grid
		{
			get { return grid ?? (grid = new ZGrid()); }
		}
		ZGrid grid;

		OrgHeader Organisation
		{
			get { return organisation ?? (organisation = Factory.New<OrgHeader>()); }
		}
		OrgHeader organisation;

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
			if (layoutPersister != null)
			{
				layoutPersister.Dispose();
			}
		}

		#endregion
	}
}
