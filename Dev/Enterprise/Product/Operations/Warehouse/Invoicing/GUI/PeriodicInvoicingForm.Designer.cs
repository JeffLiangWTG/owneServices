using System.ComponentModel;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Invoicing.GUI
{
	public partial class PeriodicInvoicingForm
	{
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZTemplateTabControl zTabControl1;
		ZTabPage EntryPage;
		protected ZStmNoteTabPage zStmNoteTabPage1;
		ZLogsTabPage zEventTabPage1;
		private ZGuidDropEdit WarehouseDropEdit;
		private ZDateEdit BillingDateEdit;
		private ZDateEdit ToDateEdit;
		private ZDateEdit FromDateEdit;
		private ZTextBox NumberTextBox;
		IContainer components;

		new void InitializeComponent()
		{
			this.components = new Container();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.zTabControl1 = new ZTemplateTabControl();
			this.EntryPage = new ZTabPage();
			this.zStmNoteTabPage1 = new ZStmNoteTabPage();
			this.zEventTabPage1 = new ZLogsTabPage();
			this.WarehouseDropEdit = new ZGuidDropEdit();
			this.BillingDateEdit = new ZDateEdit();
			this.ToDateEdit = new ZDateEdit();
			this.FromDateEdit = new ZDateEdit();
			this.NumberTextBox = new ZTextBox();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PostingButtonsUserControl.SuspendLayout();
			this.zTabControl1.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.WarehouseDropEdit.SuspendLayout();
			this.BillingDateEdit.SuspendLayout();
			this.ToDateEdit.SuspendLayout();
			this.FromDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 662, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			this.MainStatusBar.TabIndex = 12;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1009);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PeriodicInvoicing);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 636, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 11;
			// 
			// zTabControl1
			// 
			this.zTabControl1.Controls.Add(this.EntryPage);
			this.zTabControl1.Controls.Add(this.zStmNoteTabPage1);
			this.zTabControl1.Controls.Add(this.zEventTabPage1);
			this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 31, true);
			this.zTabControl1.Name = "zTabControl1";
			this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 599, true);
			this.zTabControl1.TabIndex = 10;
			// 
			// EntryPage
			// 
			this.EntryPage.CaptionResourceString = Res.GetData("PeriodicInvoicingForm|a640141e-580f-4d8e-b8fc-6388e93a2b3a", "Entry");
			this.EntryPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.EntryPage.Name = "EntryPage";
			this.EntryPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 550, true);
			this.EntryPage.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 550, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zEventTabPage1
			// 
			this.zEventTabPage1.ExcludeFromBindingOnSave = true;
			this.zEventTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 40, true);
			this.zEventTabPage1.Name = "zEventTabPage1";
			this.zEventTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zEventTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 555, true);
			this.zEventTabPage1.TabIndex = 2;
			// 
			// WarehouseDropEdit
			// 
			this.WarehouseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseDropEdit, "ET_WW");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoicing)(null)).ET_WW)));
			this.WarehouseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 5, true);
			this.WarehouseDropEdit.Name = "WarehouseDropEdit";
			this.WarehouseDropEdit.PreBoundMaxLength = 3;
			this.WarehouseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 38, true);
			this.WarehouseDropEdit.TabIndex = 3;
			// 
			// BillingDateEdit
			// 
			this.BillingDateEdit.AllowDrop = true;
			this.BillingDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.BillingDateEdit, "ET_BillingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoicing)(null)).ET_BillingDate)));
			this.BillingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 5, true);
			this.BillingDateEdit.Name = "BillingDateEdit";
			this.BillingDateEdit.TabIndex = 5;
			// 
			// ToDateEdit
			// 
			this.ToDateEdit.AllowDrop = true;
			this.ToDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ToDateEdit, "ET_StorageToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoicing)(null)).ET_StorageToDate)));
			this.ToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(919, 5, true);
			this.ToDateEdit.Name = "ToDateEdit";
			this.ToDateEdit.TabIndex = 9;
			// 
			// FromDateEdit
			// 
			this.FromDateEdit.AllowDrop = true;
			this.FromDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.FromDateEdit, "ET_StorageFromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((PeriodicInvoicing)(null)).ET_StorageFromDate)));
			this.FromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(772, 5, true);
			this.FromDateEdit.Name = "FromDateEdit";
			this.FromDateEdit.TabIndex = 7;
			// 
			// NumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.NumberTextBox, "ET_StorageJobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PeriodicInvoicing)(null)).ET_StorageJobNumber)));
			this.NumberTextBox.CaptionResourceString = null;
			this.NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 5, true);
			this.NumberTextBox.Name = "NumberTextBox";
			this.NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 38, true);
			this.NumberTextBox.TabIndex = 1;
			// 
			// InvoicingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 686, true);
			this.Controls.Add(this.WarehouseDropEdit);
			this.Controls.Add(this.BillingDateEdit);
			this.Controls.Add(this.ToDateEdit);
			this.Controls.Add(this.FromDateEdit);
			this.Controls.Add(this.NumberTextBox);
			this.Controls.Add(this.zTabControl1);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.DataSourceAssemblyName = typeof(PeriodicInvoicing).Assembly.FullName;
			this.DataSourceType = typeof(PeriodicInvoicing);
			this.DataSourceTypeName = typeof(PeriodicInvoicing).Name;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "PeriodicInvoicingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "PeriodicInvoicingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.zTabControl1, 0);
			this.Controls.SetChildIndex(this.NumberTextBox, 0);
			this.Controls.SetChildIndex(this.FromDateEdit, 0);
			this.Controls.SetChildIndex(this.ToDateEdit, 0);
			this.Controls.SetChildIndex(this.BillingDateEdit, 0);
			this.Controls.SetChildIndex(this.WarehouseDropEdit, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.zTabControl1.ResumeLayout(false);
			this.zTabControl1.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.WarehouseDropEdit.ResumeLayout(true);
			this.WarehouseDropEdit.PerformLayout();
			this.BillingDateEdit.ResumeLayout(true);
			this.BillingDateEdit.PerformLayout();
			this.ToDateEdit.ResumeLayout(true);
			this.ToDateEdit.PerformLayout();
			this.FromDateEdit.ResumeLayout(true);
			this.FromDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
