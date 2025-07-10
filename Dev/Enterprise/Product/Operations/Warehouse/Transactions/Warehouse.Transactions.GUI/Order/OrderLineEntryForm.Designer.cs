using System;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class OrderLineEntryForm
	{
		CargoWise.Windows.UI.KPanel panel1;
		ZPostingButtonsUserControl oPostingButtonsUserControl1;
		ZGroupBox zGroupBox1;
		ZGuidFindBox WarehouseGuidFindBox;
		ZGuidFindBox ClientGuidFindBox;
		ZTextBox ExternalReferenceTextBox;
		ZButton ShowOrderButton;
		ZPanel zPanel1;
		OrderLineEntryUserControl orderLineEntryUserControl1;

		protected override void InitializeComponent()
		{
			this.orderLineEntryUserControl1 = new OrderLineEntryUserControl();
			this.panel1 = new CargoWise.Windows.UI.KPanel();
			this.oPostingButtonsUserControl1 = new ZPostingButtonsUserControl();
			this.zGroupBox1 = new ZGroupBox();
			this.ShowOrderButton = new ZButton();
			this.ExternalReferenceTextBox = new ZTextBox();
			this.WarehouseGuidFindBox = new ZGuidFindBox();
			this.ClientGuidFindBox = new ZGuidFindBox();
			this.zPanel1 = new ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panel1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 640, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsPickableDocketLine);
			// 
			// orderLineEntryUserControl1
			// 
			this.orderLineEntryUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.orderLineEntryUserControl1, ".");
			this.orderLineEntryUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orderLineEntryUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 67, true);
			this.orderLineEntryUserControl1.Name = "orderLineEntryUserControl1";
			this.orderLineEntryUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 534, true);
			this.orderLineEntryUserControl1.TabIndex = 1;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.oPostingButtonsUserControl1);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 601, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 39, true);
			this.panel1.TabIndex = 2;
			// 
			// oPostingButtonsUserControl1
			// 
			this.oPostingButtonsUserControl1.AllowDrop = true;
			this.oPostingButtonsUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 8, true);
			this.oPostingButtonsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl1.Name = "oPostingButtonsUserControl1";
			this.oPostingButtonsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl1.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryForm|92690583-2827-401f-be9c-1577313cfb32", "Order Details");
			this.zGroupBox1.Controls.Add(this.ShowOrderButton);
			this.zGroupBox1.Controls.Add(this.ExternalReferenceTextBox);
			this.zGroupBox1.Controls.Add(this.WarehouseGuidFindBox);
			this.zGroupBox1.Controls.Add(this.ClientGuidFindBox);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 67, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// ShowOrderButton
			// 
			this.ShowOrderButton.Font = new System.Drawing.Font(OFont.NormalFontName, 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ShowOrderButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(741, 15, true);
			this.ShowOrderButton.Name = "ShowOrderButton";
			this.ShowOrderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 22, true);
			this.ShowOrderButton.TabIndex = 4;
			this.ShowOrderButton.Text = "...";
			this.ShowOrderButton.UseVisualStyleBackColor = true;
			this.ShowOrderButton.Click += new EventHandler(this.ShowOrderButton_Click);
			// 
			// ExternalReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExternalReferenceTextBox, "PickableDocket+WD_ExternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((WhsPickableDocketLine)(null)).PickableDocket.WD_ExternalReference)));
			this.ExternalReferenceTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderLineEntryForm|646b4983-b291-4d40-983d-13f97ea8e88b", "Order No");
			this.ExternalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(544, 16, true);
			this.ExternalReferenceTextBox.Name = "ExternalReferenceTextBox";
			this.ExternalReferenceTextBox.ReadOnly = true;
			this.ExternalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 20, true);
			this.ExternalReferenceTextBox.TabIndex = 3;
			// 
			// WarehouseGuidFindBox
			// 
			this.WarehouseGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseGuidFindBox, "PickableDocket+WD_WW_Whs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsPickableDocketLine)(null)).PickableDocket.WD_WW_Whs)));
			this.WarehouseGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 16, true);
			this.WarehouseGuidFindBox.Name = "WarehouseGuidFindBox";
			this.WarehouseGuidFindBox.ReadOnly = true;
			this.WarehouseGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.WarehouseGuidFindBox.TabIndex = 1;
			// 
			// ClientGuidFindBox
			// 
			this.ClientGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientGuidFindBox, "PickableDocket+WD_OH_Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZGuid)(((WhsPickableDocketLine)(null)).PickableDocket.WD_OH_Client)));
			this.ClientGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 40, true);
			this.ClientGuidFindBox.Name = "ClientGuidFindBox";
			this.ClientGuidFindBox.ReadOnly = true;
			this.ClientGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 20, true);
			this.ClientGuidFindBox.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.zGroupBox1);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 67, true);
			this.zPanel1.TabIndex = 0;
			// 
			// OrderLineEntryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 664, true);
			this.Controls.Add(this.orderLineEntryUserControl1);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.panel1);
			this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
			this.DataSourceType = typeof(WhsPickableDocketLine);
			this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsPickableDocketLine";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(803, 700, true);
			this.Name = "OrderLineEntryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.orderLineEntryUserControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panel1.ResumeLayout(false);
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
