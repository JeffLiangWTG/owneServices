using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class ForwardingControl
	{
		ZDropEdit INCOTERMDropDownEdit;
		ZGroupBox ShipperCODGroupBox;
		ZDropEdit ShipperCODPaymentTypeDropEdit;
		ZCalcEdit CODAmountTextBox;
		ZDropEdit TransportModeDropEdit;
		ZDropEdit ContainerModeDropEdit;
		ZTextBox DGContactTextBox;
		ZTextBox BOLNoTextBox;

		void InitializeComponent()
		{
			this.INCOTERMDropDownEdit = new ZDropEdit();
			this.ShipperCODGroupBox = new ZGroupBox();
			this.ShipperCODPaymentTypeDropEdit = new ZDropEdit();
			this.CODAmountTextBox = new ZCalcEdit();
			this.BOLNoTextBox = new ZTextBox();
			this.TransportModeDropEdit = new ZDropEdit();
			this.ContainerModeDropEdit = new ZDropEdit();
			this.DGContactTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipperCODGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsOrder);
			// 
			// INCOTERMDropDownEdit
			// 
			this.INCOTERMDropDownEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.INCOTERMDropDownEdit, "WD_INCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.WhsOrder)(null)).WD_INCO)));
			this.INCOTERMDropDownEdit.CaptionResourceString = null;
			this.INCOTERMDropDownEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 28, true);
			this.INCOTERMDropDownEdit.Name = "INCOTERMDropDownEdit";
			this.INCOTERMDropDownEdit.PreBoundMaxLength = 3;
			this.INCOTERMDropDownEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.INCOTERMDropDownEdit.TabIndex = 3;
			// 
			// ShipperCODGroupBox
			// 
			this.ShipperCODGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocsControl|5bcf3654-f6eb-4497-9342-d6135c38399e", "Shipper COD");
			this.ShipperCODGroupBox.Controls.Add(this.ShipperCODPaymentTypeDropEdit);
			this.ShipperCODGroupBox.Controls.Add(this.CODAmountTextBox);
			this.ShipperCODGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 135, true);
			this.ShipperCODGroupBox.Name = "ShipperCODGroupBox";
			this.ShipperCODGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 69, true);
			this.ShipperCODGroupBox.TabIndex = 7;
			this.ShipperCODGroupBox.TabStop = false;
			// 
			// ShipperCODPaymentTypeDropEdit
			// 
			this.ShipperCODPaymentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperCODPaymentTypeDropEdit, "WD_CODPayMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.WhsOrder)(null)).WD_CODPayMethod)));
			this.ShipperCODPaymentTypeDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocsControl|101b3c6a-a493-4a1c-9cf1-32e1433a49fc", "Type");
			this.ShipperCODPaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 40, true);
			this.ShipperCODPaymentTypeDropEdit.Name = "ShipperCODPaymentTypeDropEdit";
			this.ShipperCODPaymentTypeDropEdit.PreBoundMaxLength = 3;
			this.ShipperCODPaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.ShipperCODPaymentTypeDropEdit.TabIndex = 1;
			// 
			// CODAmountTextBox
			// 
			this.BindingSource.SetBindingMember(this.CODAmountTextBox, "WD_ShipperCODAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.WhsOrder)(null)).WD_ShipperCODAmount)));
			this.CODAmountTextBox.CaptionResourceString = null;
			this.CODAmountTextBox.DecimalPlaces = 2;
			this.CODAmountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 14, true);
			this.CODAmountTextBox.Name = "CODAmountTextBox";
			this.CODAmountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.CODAmountTextBox.TabIndex = 0;
			this.CODAmountTextBox.Text = "0.00";
			this.CODAmountTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BOLNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.BOLNoTextBox, "WD_BOLNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsOrder)(null)).WD_BOLNo)));
			this.BOLNoTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("DocsControl|5a48513e-d91d-4d99-96ce-1f219850deb7", "Waybill");
			this.BOLNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 2, true);
			this.BOLNoTextBox.Name = "BOLNoTextBox";
			this.BOLNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.BOLNoTextBox.TabIndex = 0;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "WD_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.WhsOrder)(null)).WD_TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("7a0d2154-e57d-4fd7-9b22-cd8b2193e7e2", "Transport Mode");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 54, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.TransportModeDropEdit.TabIndex = 4;
			// 
			// ContainerModeDropEdit
			// 
			this.ContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeDropEdit, "WD_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.WhsOrder)(null)).WD_ContainerMode)));
			this.ContainerModeDropEdit.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("19005ece-7274-402c-acfb-8c19f1cfe4bc", "Container Mode");
			this.ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 80, true);
			this.ContainerModeDropEdit.Name = "ContainerModeDropEdit";
			this.ContainerModeDropEdit.PreBoundMaxLength = 3;
			this.ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.ContainerModeDropEdit.TabIndex = 5;
			// 
			// DGContactTextBox
			// 
			this.BindingSource.SetBindingMember(this.DGContactTextBox, "DGContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WhsOrder)(null)).DGContact)));
			this.DGContactTextBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderEntryUserControl|8f0042c7-1d0e-4e36-bcb4-7b16176faf30", "DG Contact");
			this.DGContactTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 106, true);
			this.DGContactTextBox.Name = "DGContactTextBox";
			this.DGContactTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 20, true);
			this.DGContactTextBox.TabIndex = 6;
			// 
			// ForwardingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DGContactTextBox);
			this.Controls.Add(this.ContainerModeDropEdit);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Controls.Add(this.INCOTERMDropDownEdit);
			this.Controls.Add(this.ShipperCODGroupBox);
			this.Controls.Add(this.BOLNoTextBox);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 206, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 206, true);
			this.Name = "ForwardingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 206, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipperCODGroupBox.ResumeLayout(false);
			this.ShipperCODGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
