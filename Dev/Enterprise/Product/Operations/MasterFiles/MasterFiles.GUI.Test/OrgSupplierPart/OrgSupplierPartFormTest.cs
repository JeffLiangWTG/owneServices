using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(OrgSupplierPartForm))]
	public class OrgSupplierPartFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new OrgSupplierPartForm(part);
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestDimensionLabel()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				Assert("DimensionLabel is too narrow for translation", form.DimensionLabel.Width >= 80);
				AssertEquals("DimensionLabel is not right-aligned", System.Drawing.ContentAlignment.MiddleRight, form.DimensionLabel.TextAlign);
			}
		}

		[RequiresSTA]
		public void TestDefaultTabOnBottomPageControl()
		{
			var part = Factory.New<OrgSupplierPart>();

			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				AssertEquals(form.MainTabPage, form.MainTabControl.SelectedTab);
				AssertEquals("CustomsTabPage", form.BottomTabControl.SelectedTab.Name);
			}

			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.PlugInIDToSelectOnLoaded = ControllerIDs.WhsConfigProduct;
				form.Show();
				AssertEquals(form.MainTabPage, form.MainTabControl.SelectedTab);
				AssertEquals("RelatedOrgsTabPage", form.BottomTabControl.SelectedTab.Name);
			}
		}

		public void TestConstructor()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				AssertNotNull("LandedCosting not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.LandedCostingHistoryView));
				var plugIn = form.BottomTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.SupplierPart);
				AssertNotNull("Customs not plugged in", plugIn);
				AssertEquals(Env.Security.CustomsSupplierPartModifyCustoms, plugIn.SecurityCheckpoint);
				AssertNotNull("Warehouse not plugged in", form.BottomTabControl.PlugIns.GetPlugIn(ControllerIDs.WhsConfigProduct));
			}
		}

		public void TestFormCaption()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				AssertEquals("Product", form.FormCaption);
				part.OP_PartNum = "PARTA";
				AssertEquals("Product: PARTA", form.FormCaption);
				part.OP_Desc = "PARTADESC";
				AssertEquals("Product: PARTA - PARTADESC", form.FormCaption);
			}
		}

		public void TestNMFCTabPageIsNotHidden()
		{
			GlbCompany.CurrentCompany.SetCountry("US");
			AssertNMFCTabPageVisibility(true);
			GlbCompany.CurrentCompany.SetCountry("CA");
			AssertNMFCTabPageVisibility(true);
			GlbCompany.CurrentCompany.SetCountry("MX");
			AssertNMFCTabPageVisibility(true);
		}

		[RequiresSTA]
		public void TestNMFCTabPageIsHidden()
		{
			AssertNMFCTabPageVisibility(false);
		}

		void AssertNMFCTabPageVisibility(ZBool isHidden)
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				AssertEquals(isHidden, form.MainTabControl.TabPages.Contains(form.NMFCTabPage));
			}
		}

		public void TestSelectAndReturnPickFaceGrid()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				var grid = form.SelectAndReturnPickFaceGrid();
				AssertEquals("Warehouse tab on Details tab is selected", true, form.IsWarehouseTabPageSelected());
				AssertEquals(form.BottomTabControl.SelectedTab.Name, "WarehouseTabPage");
				AssertEquals(grid.Name, "zGrid1");
				AssertEquals(grid.Columns[0].ColumnStyle.HeaderText, "Client");
				AssertEquals(grid.Columns[1].ColumnStyle.HeaderText, "Warehouse");
				AssertEquals(grid.Columns[2].ColumnStyle.HeaderText, "Location");
			}
		}

		[RequiresSTA]
		public void TestSubstancePKInsteadOfDIDGInUNDGsGrid()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				var substancePKColumnInfo = form.UNDGsGrid.ColumnStyles.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "SubstancePK");

				var dIDGColumnInfo = form.UNDGsGrid.ColumnStyles.Cast<ZGridColumnInfo>()
						.FirstOrDefault((c) => c.ColumnName == "DI_DG");

				AssertNotNull(substancePKColumnInfo);
				AssertNull(dIDGColumnInfo);
			}
		}

		public void TestGetWarehouseTabPageName()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartForm(part))
			{
				var warehouseTabPage = form.GetWarehouseTabPageName();

				AssertEquals(warehouseTabPage, "MainTabPage+WarehouseTabPage");
			}
		}

		#region TestWorkflowCustomFields

		public void TestWorkflowCustomFields()
		{
			var client0 = Factory.NewWithValidTestData<OrgHeader>();
			var part0 = Factory.New<OrgSupplierPart>();
			part0.OP_PartNum = "PartNum0";
			var relation0 = part0.RelatedOrganisations.AddOrganisationIfNotExist(client0.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation0.OU_FormLayoutController = true;

			// create a template with custom fields for client1
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var template1 = CreateWorkflowTemplate(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
			template1.P0_OH_Client = client1.PK;
			AddCustomField(template1, "client1 stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(template1, "client1 intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(template1, "client1 dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(template1, "client1 boolField", AddOnColumnDataType.Codes.Boolean);

			// create a template with only 1 custom field for client2
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var template2 = CreateWorkflowTemplate(WorkflowDescriptors.OrgSupplierPartWorkflowDescriptorCode);
			template2.P0_OH_Client = client2.PK;
			AddCustomField(template2, "client2 stringField", AddOnColumnDataType.Codes.String);

			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PartNum1";
			var relation1 = part1.RelatedOrganisations.AddOrganisationIfNotExist(client1.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation1.OU_FormLayoutController = true;

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PartNum2";
			var relation2 = part2.RelatedOrganisations.AddOrganisationIfNotExist(client2.PK, OrgPartRelation.RelationshipTypes.Owner);
			relation2.OU_FormLayoutController = true;

			Factory.Save();

			// no workflow custom fields
			using (var form0 = new OrgSupplierPartFormTestOne(part0))
			{
				form0.Show();

				form0.MainTabControl.SelectedTab = form0.AdditionalDetailsTabPage;
				ProcessTemplateCustomFieldsControl customFieldsControl = FindControl<ProcessTemplateCustomFieldsControl>(form0.Controls, "processTemplateCustomFieldsControl1");
				var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];
				AssertEquals(0, rowLayoutPanel.Controls.Count);
			}

			// 4 workflow custom fields
			using (var form1 = new OrgSupplierPartFormTestOne(part1))
			{
				form1.Show();

				form1.MainTabControl.SelectedTab = form1.AdditionalDetailsTabPage;
				ProcessTemplateCustomFieldsControl customFieldsControl = FindControl<ProcessTemplateCustomFieldsControl>(form1.Controls, "processTemplateCustomFieldsControl1");
				var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];
				AssertEquals(4, rowLayoutPanel.Controls.Count);

				// control will be sorted order by type then order by caption
				AssertControl(rowLayoutPanel.Controls[0], typeof(ZCheckBox), "client1 boolField");
				AssertControl(rowLayoutPanel.Controls[1], typeof(ZDateEdit), "client1 dateTimeField");
				AssertControl(rowLayoutPanel.Controls[2], typeof(ZCalcEdit), "client1 intField");
				AssertControl(rowLayoutPanel.Controls[3], typeof(ZTextBox), "client1 stringField");
			}

			// 1 workflow custom field
			using (var form2 = new OrgSupplierPartFormTestOne(part2))
			{
				form2.Show();

				form2.MainTabControl.SelectedTab = form2.AdditionalDetailsTabPage;
				ProcessTemplateCustomFieldsControl customFieldsControl = FindControl<ProcessTemplateCustomFieldsControl>(form2.Controls, "processTemplateCustomFieldsControl1");
				var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];
				AssertEquals(1, rowLayoutPanel.Controls.Count);
				AssertControl(rowLayoutPanel.Controls[0], typeof(ZTextBox), "client2 stringField");
			}
		}

		void AssertControl(Control ctrl, Type type, string caption)
		{
			AssertEquals(type, ctrl.GetType());
			AssertEquals(caption, ctrl.GetExtension<LabelCaptionRenderer>().Caption);
		}

		static T FindControl<T>(Control.ControlCollection controls, string name)
			where T : Control
		{
			foreach (Control control in controls)
			{
				var controlAsT = control as T;
				if (controlAsT != null && controlAsT.Name == name)
				{
					return controlAsT;
				}

				var childControl = FindControl<T>(control.Controls, name);
				if (childControl != null)
				{
					return childControl;
				}
			}

			return null;
		}

		ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		#endregion

		public void TestBOMTabPage()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = part.PK.ToString().Replace("-", "");
			BOM.OE_OP_Component = part.PK;
			BOM.Component.OP_PartNum = "A";
			BOM.Component.OP_Desc = "A";
			//							A
			//						/	|
			//					B		C
			//				/		\
			//			E				F
			OrgPartBOM b = CreateNewWithPart(BOM, "B");
			OrgPartBOM c = CreateNewWithPart(BOM, "C");
			OrgPartBOM e = CreateNewWithPart(b, "E");
			OrgPartBOM f = CreateNewWithPart(b, "F");
			Factory.Save();
			using (OrgSupplierPartFormTestOne form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				TabPageNotificationsExposer.ExposeTabPageNotifications(form.MainTabControl, form.BusinessEntity);
				AssertEquals(true, form.MainTabControl.TabPages.Contains(form.BOMTabPage));
				AssertNotNull(form.BOMTabPage.Controls["BillOfMaterialsGrid"]);
				AssertNotNull(form.BOMTabPage.Controls["BillOfMaterialsViewGrid"]);
				((TabControl)form.BOMTabPage.Parent).SelectedTab = form.BOMTabPage;
				ZGrid mainGrid = (ZGrid)form.BOMTabPage.Controls["BillOfMaterialsGrid"];
				AssertEquals(2, mainGrid.List.Count);
				ZGrid detailsGrid = (ZGrid)form.BOMTabPage.Controls["BillOfMaterialsViewGrid"];
				mainGrid.CurrentRowIndex = 0;
				AssertEquals(2, detailsGrid.List.Count);
				mainGrid.CurrentRowIndex = 1;
				AssertEquals(0, detailsGrid.List.Count);
				form.TryEditBO(null, new EventArgs());
				AssertEquals("Product: C - C", ((ZForm)form.SupplierPartController.LastShownForm).FormCaption);
				form.SupplierPartController.LastShownForm.Dispose();
			}
		}

		public void TestSecondaryProductsVisibility_InwardProcessingSupported()
		{
			AssertSecondaryProductsVisibility(supportsInwardProcessing: true);
		}

		public void TestSecondaryProductsVisibility_InwardProcessingNotSupported()
		{
			AssertSecondaryProductsVisibility(supportsInwardProcessing: false);
		}

		void AssertSecondaryProductsVisibility(bool supportsInwardProcessing)
		{
			var mock = new Mock<ISupportedForProcessing>();

			mock.Setup(m => m.IsSupportedForProcessing()).Returns(supportsInwardProcessing);

			using (ObjectFactory.Substitute(mock.Object))
			{
				var part = Factory.New<OrgSupplierPart>();
				using (var form = new OrgSupplierPartFormTestOne(part))
				{
					form.Show();
					((TabControl)form.BOMTabPage.Parent).SelectedTab = form.BOMTabPage;
					var groupBox = form.BOMTabPage.Controls["SecondaryProductsGroupBox"];
					var splitter = form.BOMTabPage.Controls["Splitter2"];
					AssertEquals("Secondary Products should be shown based on Inward Processing Support.", supportsInwardProcessing, groupBox.Visible);
					AssertEquals("Secondary Products should be shown based on Inward Processing Support.", supportsInwardProcessing, splitter.Visible);
				}
			}
		}

		public void Test_AddBarcodeForStockUnitPackType_ShouldDefaultFlagToTrue()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "KG";

			var partBarcode1 = part.PartBarcodes.AddNew();
			partBarcode1.PH_Barcode = "Test1";
			partBarcode1.PH_F3_NKPackType = "BAR";

			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				((TabControl)form.ProductBarcodesTabPage.Parent).SelectedTab = form.ProductBarcodesTabPage;
				ZGrid mainGrid = (ZGrid)form.ProductBarcodesTabPage.Controls["PartBarcodesGrid"];

				mainGrid.ListManager.Position = 0;
				mainGrid.Select(0);
				var selectedItem = form.PartBarcodesGridCurrentElement;
				selectedItem.PH_F3_NKPackType = part.OP_StockKeepingUnit;

				AssertEquals("Default the flag to true when adding a Barcode for the Stock Unit PackType, unless one already exists for the Stock Unit Pack Type.", true, selectedItem.PH_UseForDocuments);
			}
		}

		public void Test_AddBarcodeForStockUnitPackTypeAlreadyExists_ShouldNotChangeFlag()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "KG";

			var partBarcode1 = part.PartBarcodes.AddNew();
			partBarcode1.PH_Barcode = "Test1";
			partBarcode1.PH_F3_NKPackType = part.OP_StockKeepingUnit;
			partBarcode1.PH_UseForDocuments = true;

			var partBarcode2 = part.PartBarcodes.AddNew();
			partBarcode2.PH_Barcode = "Test2";
			partBarcode2.PH_F3_NKPackType = "BAG";

			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				TabPageNotificationsExposer.ExposeTabPageNotifications(form.ProductBarcodesTabPage, form.BusinessEntity);
				((TabControl)form.ProductBarcodesTabPage.Parent).SelectedTab = form.ProductBarcodesTabPage;
				ZGrid mainGrid = (ZGrid)form.ProductBarcodesTabPage.Controls["PartBarcodesGrid"];

				mainGrid.ListManager.Position = 1;
				mainGrid.Select(1);
				var selectedItem = form.PartBarcodesGridCurrentElement;
				selectedItem.PH_F3_NKPackType = part.OP_StockKeepingUnit;

				AssertEquals("Default the flag to true when adding a Barcode for the Stock Unit PackType, unless one already exists for the Stock Unit Pack Type.", false, selectedItem.PH_UseForDocuments);
			}
		}

		public void TestAuditPluginEnabled()
		{
			var part = Factory.New<OrgSupplierPart>();
			using (var form = new OrgSupplierPartForm(part))
			{
				form.Show();
				Assert(form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		class OrgSupplierPartFormForCancelTesting : OrgSupplierPartForm
		{
			public OrgSupplierPartFormForCancelTesting(OrgSupplierPart part)
				: base(part)
			{
			}

			protected internal override void HandleApplyPostingButtonClickUnsafe(bool closeOnSave)
			{
				throw new DeveloperNotificationException("FK violation exception", new DeveloperNotificationException("The DELETE statement conflicted with the REFERENCE constraint"));
			}
		}

		public void TestDeleteErrorInactiveMessage()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_IsActive = true;
			part.OP_PartNum = "TESTPART1";
			part.OP_Desc = "Product #1";
			Factory.Save();

			using (var form = new OrgSupplierPartFormForCancelTesting(part))
			using (var saveAndCloseButton = new ZToolStripButton())
			{
				ZFormPostingButtonsStrategy.SetupPosting(form, saveAndCloseButton, null, null);
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();
				Application.DoEvents();

				var reloadedForm = new ZFormUtilitiesTest().GetFormCreatedByReloading(form);
				var partInReloadedForm = (OrgSupplierPart)reloadedForm.BusinessEntity;
				AssertEquals("Old form is disposed.", true, form.IsDisposed);
				AssertEquals("This record is in use by other records in the system. Would you like to mark this record as Inactive?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Object is deactivated.", true, partInReloadedForm.IsCancelled);
				AssertEquals("DisplayMode is Edit.", ODisplayMode.Edit, reloadedForm.DisplayMode);

				foreach (var tempForm in Application.OpenForms)
				{
					if (tempForm is OrgSupplierPartForm)
					{
						((OrgSupplierPartForm)tempForm).Dispose();
						break;
					}
				}
			}
		}

		public void TestIsAutoReplenishmentIsVisible()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			AssertEquals("Precondition.", true, Env.CurrentUser.IsSupportUser);
			TestIsAutoReplenishControlVisible();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition.", false, Env.CurrentUser.IsSupportUser);
				TestIsAutoReplenishControlVisible();
			}
		}

		void TestIsAutoReplenishControlVisible()
		{
			var part = Factory.New<OrgSupplierPart>();

			using (var form = new OrgSupplierPartFormTestOne(part))
			{
				form.Show();
				((TabControl)form.BOMTabPage.Parent).SelectedTab = form.BOMTabPage;

				var isAutoReplenish = form.BOMTabPage.Controls["BOMTopPanel"]?.Controls["IsAutoReplenish"];
				AssertNotNull(isAutoReplenish);
				AssertEquals("CheckBox IsAutoReplenish should not be hidden.", true, isAutoReplenish.Visible);
			}
		}

		public class OrgSupplierPartFormTestOne : OrgSupplierPartForm
		{
			public OrgSupplierPartFormTestOne(OrgSupplierPart part)
				: base(part)
			{
				if (part.OP_PartNum.IsEmpty)
				{
					part.OP_PartNum = part.PK.ToString().Replace("-", "");
				}
			}

			public new ZTemplateTabControl MainTabControl => base.MainTabControl;

			public new ZTabPage NMFCTabPage => base.NMFCTabPage;

			public new ZTabControl BottomTabControl => base.BottomTabControl;

			public new ZTabPage AdditionalDetailsTabPage => base.AdditionalDetailsTabPage;

			public new ZTabPage MainTabPage => base.MainTabPage;

			public new ZTabPage BOMTabPage => base.BOMTabPage;

			public new ZGrid UNDGsGrid => base.UNDGsGrid;

			public new ZTabPage ProductBarcodesTabPage => base.ProductBarcodesTabPage;

			public OrgSupplierPartBarcode PartBarcodesGridCurrentElement => PartBarcodesGrid.ListManager.GetCurrent() as OrgSupplierPartBarcode;

			public new void TryEditBO(object sender, EventArgs e)
			{
				base.TryEditBO(sender, e);
			}

			public new ZController SupplierPartController
			{
				get { return base.SupplierPartController; }
			}
		}

		OrgPartBOM CreateNewWithPart(OrgPartBOM parent, string componentPartNum)
		{
			OrgPartBOM result = Factory.New<OrgPartBOM>();
			result.OE_OP_MainProduct = parent.Component.PK;
			var orgSupPart = Factory.New<OrgSupplierPart>();
			orgSupPart.OP_PartNum = Guid.NewGuid().ToString().Replace("-", "");
			result.OE_OP_Component = orgSupPart.PK;
			result.Component.OP_PartNum = componentPartNum;
			result.Component.OP_Desc = componentPartNum;

			return result;
		}

		OrgPartBOM BOM
		{
			get
			{
				if (fbom == null)
				{
					fbom = Factory.New<OrgPartBOM>();
					OrgSupplierPart part = Factory.New<OrgSupplierPart>();
					part.OP_PartNum = part.PK.ToString().Replace("-", "");
					OrgSupplierPart subPart = Factory.New<OrgSupplierPart>();
					subPart.OP_PartNum = subPart.PK.ToString().Replace("-", "");
					fbom.OE_OP_MainProduct = part.PK;
					fbom.OE_OP_Component = subPart.PK;
					OrgPartBOM bom1 = CreateNewWithPart(fbom, "Test");
				}
				return fbom;
			}
		}
		OrgPartBOM fbom;
	}
}
