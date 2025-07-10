using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class RelatedOrganizationUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestSerialNumberColumnsInitialised()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				gridPartRelations.Focus();
				gridPartRelations.Select(0);
				Application.DoEvents();

				AssertEquals(nameof(OrgPartRelation.OU_UseSerialNumber), false,
					gridPartRelations.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == nameof(OrgPartRelation.OU_UseSerialNumber)).IsUnavailable);
				AssertEquals(nameof(OrgPartRelation.OU_IsSerialNumberReleaseCaptured), false,
					gridPartRelations.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == nameof(OrgPartRelation.OU_IsSerialNumberReleaseCaptured)).IsUnavailable);
			}
		}

		[RequiresSTA]
		public void TestSerialNumberCheckboxes()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				gridPartRelations.Focus();
				gridPartRelations.Select(0);
				Application.DoEvents();

				var relatedOrganizationUserControl = FindControl<RelatedOrganizationUserControl>(form.Controls, "relatedOrganizationsControl1");
				relatedOrganizationUserControl.SelectAttributesTab();
				var serialNumberCheckBox = FindControl<ZCheckBox>(form.Controls, "UseSerialNumberCheckBox");
				var isSerialNumberReleaseCapturedCheckBox = FindControl<ZCheckBox>(form.Controls, "IsSerialNumberReleaseCapturedCheckBox");
				AssertEquals(true, serialNumberCheckBox.Visible);
				AssertEquals(true, isSerialNumberReleaseCapturedCheckBox.Visible);
			}
		}

		public void TestUsersAreWarnedWhenTheyHaveChangedOU_OH()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.FillWithValidTestData();

			var relation1 = product.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_OH = org2.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			var relation3 = product.RelatedOrganisations.AddNew();
			relation3.OU_OH = org3.PK;
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

			GlbCompany.CurrentCompany.SetCountry("AU");
			var cusClassification = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();

			var cusClassPartPivot = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
			cusClassPartPivot[CusClassPartPivotSchema.Constants.CI_OP] = product.PK;
			cusClassPartPivot[CusClassPartPivotSchema.Constants.CI_CC] = cusClassification.PK;
			cusClassPartPivot[CusClassPartPivotSchema.Constants.CI_OH] = relation1.OU_OH;

			var cusClassification1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();
			var cusClassPartPivot1 = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
			cusClassPartPivot1[CusClassPartPivotSchema.Constants.CI_OP] = product.PK;
			cusClassPartPivot1[CusClassPartPivotSchema.Constants.CI_CC] = cusClassification1.PK;
			cusClassPartPivot1[CusClassPartPivotSchema.Constants.CI_OH] = relation3.OU_OH;

			using (var form = new OrgSupplierPartForm(product))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.FocusTheRelationsGridForTesting();

				AssertEquals("PreCondition:The first row is selected", relation1, form.GetCurrentlySelectedRelationRowForTesting());

				relation1.OU_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

				AssertEquals(string.Format(RelatedOrganizationUserControl.PartyClassificationOverridesExist, "AU"), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Classification override has not been deleted", false, cusClassPartPivot.IsDeleted);
				AssertEquals("relation1.OU_OH has been reverted", org.PK, relation1.OU_OH);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				form.SelectMainTabPageForTest();
				form.FocusTheRelationsGridForTesting();
				form.SelectTheRelationAtForTesting(1);
				AssertEquals("PreCondition:The second row is selected", relation2, form.GetCurrentlySelectedRelationRowForTesting());

				OrgHeader org4 = Factory.NewWithValidTestData<OrgHeader>();
				relation2.OU_OH = org4.PK;

				AssertNull("No warning", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("OU_OH has been changed", org4.PK, relation2.OU_OH);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				form.SelectMainTabPageForTest();
				form.FocusTheRelationsGridForTesting();
				form.SelectTheRelationAtForTesting(2);

				AssertEquals("PreCondition:The second row is selected", relation3, form.GetCurrentlySelectedRelationRowForTesting());

				relation3.OU_OH = org4.PK;
				AssertEquals(string.Format(RelatedOrganizationUserControl.PartyClassificationOverridesExist, "AU"), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Classification override has been deleted", true, cusClassPartPivot1.IsDeleted);
				AssertEquals("And the change stays", org4.PK, relation3.OU_OH);
			}
		}

		[RequiresSTA]
		public void TestUsersAreWarnedBeforeRelationIsDeleted()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			OrgSupplierPart product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();

			OrgPartRelation relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			OrgSupplierPart product2 = Factory.New<OrgSupplierPart>();
			product2.FillWithValidTestData();

			OrgPartRelation relation2 = product2.RelatedOrganisations[0];
			relation2.OU_OH = org.PK;

			GlbCompany.CurrentCompany.SetCountry("AU");
			BusinessObject cusClassificationAU = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassification>();

			BusinessObject cusClassPartPivot = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseCusClassPartPivot>();
			cusClassPartPivot[CusClassPartPivotSchema.Constants.CI_OP] = product1.PK;
			cusClassPartPivot[CusClassPartPivotSchema.Constants.CI_CC] = cusClassificationAU.PK;
			cusClassPartPivot[CusClassPartPivotSchema.Constants.CI_OH] = relation1.OU_OH;

			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				SelectFirstRelationAndPostDeleteKeyForTesting(form);

				AssertEquals(string.Format(RelatedOrganizationUserControl.PartyClassificationOverridesExist, "AU"), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Relation is not deleted yet as users answered No", false, relation1.IsDeleted);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				SelectFirstRelationAndPostDeleteKeyForTesting(form);
				AssertEquals("Relation is deleted along with party overrides", true, relation1.IsDeleted);
				AssertEquals("Relation is deleted along with party overrides", false, cusClassPartPivot.IsDeleted);
			}

			using (OrgSupplierPartForm form = new OrgSupplierPartForm(product2))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				SelectFirstRelationAndPostDeleteKeyForTesting(form);
				AssertEquals("Relation is deleted without any problem", true, relation2.IsDeleted);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void SelectFirstRelationAndPostDeleteKeyForTesting(OrgSupplierPartForm form)
		{
			var control = form.relatedOrganizationsControl1;
			control.GridPartRelations.Focus();
			control.GridPartRelations.Select(0);
			Application.DoEvents();

			KeySender.PostKeyDown(control.GridPartRelations, Keys.Delete);
			Application.DoEvents();
		}

		#region TestWorkflowCustomFields

		public void TestWorkflowCustomFields()
		{
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = part1.PK.ToString().Replace("-", "");
			var relation1 = part1.RelatedOrganisations.AddOrganisationIfNotExist(client1.PK, OrgPartRelation.RelationshipTypes.Owner);
			var template1 = CreateWorkflowTemplate(WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode);
			template1.P0_OH_Client = client1.PK;

			AddCustomField(template1, "client1 stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(template1, "client1 intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(template1, "client1 dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(template1, "client1 boolField", AddOnColumnDataType.Codes.Boolean);

			// client2 and part2 don't have specific relation template, but should pick up custom fields from the template that is non client-specific template
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = part2.PK.ToString().Replace("-", "");
			var relation2 = part2.RelatedOrganisations.AddOrganisationIfNotExist(client2.PK, OrgPartRelation.RelationshipTypes.Owner);

			var templateNonClientSpecific = CreateWorkflowTemplate(WorkflowDescriptors.OrgPartRelationWorkflowDescriptorCode);
			templateNonClientSpecific.P0_OH_Client = ZGuid.Empty;
			AddCustomField(templateNonClientSpecific, "generic client stringField", AddOnColumnDataType.Codes.String);

			Factory.Save();

			using (OrgSupplierPartForm form = new OrgSupplierPartForm(part1))
			{
				form.Show();

				var relatedOrganizationUserControl = FindControl<RelatedOrganizationUserControl>(form.Controls, "relatedOrganizationsControl1");
				var customFieldsControl = FindControl<ProcessTemplateCustomFieldsControl>(relatedOrganizationUserControl.Controls, "CustomFieldsControl1");
				var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];
				AssertEquals(4, rowLayoutPanel.Controls.Count);

				// control will be sorted order by type then order by caption
				AssertControl(rowLayoutPanel.Controls[0], typeof(ZCheckBox), "client1 boolField");
				AssertControl(rowLayoutPanel.Controls[1], typeof(ZDateEdit), "client1 dateTimeField");
				AssertControl(rowLayoutPanel.Controls[2], typeof(ZCalcEdit), "client1 intField");
				AssertControl(rowLayoutPanel.Controls[3], typeof(ZTextBox), "client1 stringField");
			}

			// client2 and part2 don't have specific relation template, but should pick up custom fields from non client-specific template
			using (OrgSupplierPartForm form2 = new OrgSupplierPartForm(part2))
			{
				form2.Show();

				var relatedOrganizationUserControl = FindControl<RelatedOrganizationUserControl>(form2.Controls, "relatedOrganizationsControl1");
				var customFieldsControl = FindControl<ProcessTemplateCustomFieldsControl>(relatedOrganizationUserControl.Controls, "CustomFieldsControl1");
				var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];
				AssertEquals(1, rowLayoutPanel.Controls.Count);

				AssertControl(rowLayoutPanel.Controls[0], typeof(ZTextBox), "generic client stringField");
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

		#region TestLinkLabelClickOpensBarcodeParsingModule

		public void TestLinkLabelClickOpensBarcodeParsingModule()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();
				form.SelectFirstRelationAndClickLinkLabelTesting();

				var forms = Application.OpenForms.Cast<Form>().ToList();
				var bpForm = forms.FirstOrDefault(f => f.Text.Contains("Barcode Parsing"));
				AssertNotNull(bpForm);
				bpForm.Close();
			}
		}

		#endregion

		#region TestHiTi

		[RequiresSTA]
		public void TestHiTi()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;
			relation1.OU_Hi = 15;
			relation1.OU_Ti = 10;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				gridPartRelations.Focus();
				gridPartRelations.Select(0);
				Application.DoEvents();

				var hiTextBox = FindControl<ZCalcEdit>(form.Controls, "OU_HiCalcEdit");
				var tiTextBox = FindControl<ZCalcEdit>(form.Controls, "OU_TiCalcEdit");
				AssertEquals("Hi is incorrect.", "15", hiTextBox.Text);
				AssertEquals("Ti is incorrect.", "10", tiTextBox.Text);
			}
		}

		#endregion

		#region TestDefaultHoldCode

		public void TestDefaultHoldCode()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();

			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			var validHoldCode = Factory.Load<IWhsInventoryHeldCode>(new ZQuery(WhsInventoryHeldCodeSchema.WHC_Code, "DAM")).Single();
			relation1.OU_WHC_DefaultInventoryHoldCode = validHoldCode.PK;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				var defaultInventoryHoldCodeColumn = gridPartRelations.Columns.Single(column => column.ColumnName == "OU_WHC_DefaultInventoryHoldCode");

				var inventoryHoldCodeFindBox = FindControl<ZGuidFindBox>(form.Controls, "DefaultHoldCodeFindBox");
				AssertEquals("DAM", inventoryHoldCodeFindBox.Text);
			}
		}

		#endregion

		#region TestPreventReceivingOvers 

		[RequiresSTA]
		public void TestPreventReceivingOversColumnInitialised()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				gridPartRelations.Focus();
				gridPartRelations.Select(0);
				Application.DoEvents();

				AssertEquals(nameof(OrgPartRelation.OU_PreventReceivingOvers), false,
					gridPartRelations.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == nameof(OrgPartRelation.OU_PreventReceivingOvers)).IsUnavailable);
			}
		}

		public void TestPreventReceivingOversCheckBox()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				gridPartRelations.Focus();
				gridPartRelations.Select(0);
				Application.DoEvents();

				var preventReceivingOversCheckBox = FindControl<ZCheckBox>(form.Controls, "PreventReceivingOversCheckBox");
				AssertEquals(true, preventReceivingOversCheckBox.Visible);

				preventReceivingOversCheckBox.Checked = true;
				AssertEquals(true, relation1.OU_PreventReceivingOvers);

				preventReceivingOversCheckBox.Checked = false;
				AssertEquals(false, relation1.OU_PreventReceivingOvers);
			}
		}

		#endregion

		#region TestReceiveOverageTolerancePercent  

		public void TestReceiveOverageTolerancePercentInitialised()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				gridPartRelations.Focus();
				gridPartRelations.Select(0);
				Application.DoEvents();

				AssertEquals(nameof(OrgPartRelation.OU_ReceiveOverageTolerancePercent), false,
					gridPartRelations.ColumnStyles.Cast<ZGridColumnInfo>().Single(c => c.ColumnName == nameof(OrgPartRelation.OU_ReceiveOverageTolerancePercent)).IsUnavailable);
			}
		}

		[RequiresSTA]
		public void TestReceiveOverageTolerancePercentCalcEdit()
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var product1 = Factory.New<OrgSupplierPart>();
			product1.FillWithValidTestData();
			var relation1 = product1.RelatedOrganisations[0];
			relation1.OU_OH = org.PK;
			relation1.OU_PreventReceivingOvers = true;
			relation1.OU_ReceiveOverageTolerancePercent = 500;

			using (var form = new OrgSupplierPartForm(product1))
			{
				form.Show();

				form.SelectMainTabPageForTest();
				form.SelectPartRelationTabPageForTest();

				var gridPartRelations = FindControl<ZGrid>(form.Controls, "GridPartRelations");
				gridPartRelations.Focus();
				gridPartRelations.Select(0);
				Application.DoEvents();

				var receiveOverageTolerancePercentCalcEdit = FindControl<ZCalcEdit>(form.Controls, "ReceiveOverageTolerancePercentCalcEdit");
				AssertEquals(true, receiveOverageTolerancePercentCalcEdit.Visible);
				AssertEquals("500", receiveOverageTolerancePercentCalcEdit.Text);
			}
		}

		#endregion
	}
}
