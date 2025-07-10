namespace Enterprise.Freight.Agency.GUI
{
	partial class SplitBookingsControl
	{
		void InitializeComponent()
		{
			this.gridsPlaceHolder = new CargoWise.Windows.UI.KPanel();
			headerPanel = new CargoWise.Windows.UI.KPanel();
			originalBillOfLading = new Enterprise.ZArchitecture.ZTextBox();
			dischargeTextBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			voyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			loadTextBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			vesselTextBox = new Enterprise.ZArchitecture.ZTextBox();
			OriginalShipmentNumber = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			headerPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.SplitBookingsHeader);
			// 
			// headerPanel
			// 
			headerPanel.Controls.Add(originalBillOfLading);
			headerPanel.Controls.Add(dischargeTextBox);
			headerPanel.Controls.Add(voyageTextBox);
			headerPanel.Controls.Add(loadTextBox);
			headerPanel.Controls.Add(vesselTextBox);
			headerPanel.Controls.Add(OriginalShipmentNumber);
			headerPanel.Dock = System.Windows.Forms.DockStyle.Top;
			headerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			headerPanel.Name = "headerPanel";
			headerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 80, true);
			headerPanel.TabIndex = 0;
			// 
			// originalBillOfLading
			// 
			this.BindingSource.SetBindingMember(originalBillOfLading, "OriginalShipment.JS_CFSReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.SplitBookingsHeader)(null)).OriginalShipment.JS_CFSReference)));
			originalBillOfLading.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitBookingsControl|bf0cc673-6f76-444b-b72a-f45bb752074f", "Booking Reference");
			originalBillOfLading.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 8, true);
			originalBillOfLading.Name = "originalBillOfLading";
			originalBillOfLading.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			originalBillOfLading.TabIndex = 1;
			// 
			// dischargeTextBox
			// 
			dischargeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(dischargeTextBox, "OriginalShipment.Sailings.JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.SplitBookingsHeader)(null)).OriginalShipment.Sailings)).SyncRoot)).JX_JB_RL_NKPortOfDischarge)));
			dischargeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 56, true);
			dischargeTextBox.Name = "dischargeTextBox";
			dischargeTextBox.PreBoundMaxLength = 5;
			dischargeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			dischargeTextBox.TabIndex = 5;
			// 
			// voyageTextBox
			// 
			this.BindingSource.SetBindingMember(voyageTextBox, "OriginalShipment.Sailings.JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.SplitBookingsHeader)(null)).OriginalShipment.Sailings)).SyncRoot)).JX_JV_VoyageFlight)));
			voyageTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitBookingsControl|9b254866-e364-49e5-901e-0d66f972c3ce", "Voyage");
			voyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 32, true);
			voyageTextBox.Name = "voyageTextBox";
			voyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			voyageTextBox.TabIndex = 3;
			// 
			// loadTextBox
			// 
			loadTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(loadTextBox, "OriginalShipment.Sailings.JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.SplitBookingsHeader)(null)).OriginalShipment.Sailings)).SyncRoot)).JX_JA_RL_NKPortOfLoading)));
			loadTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 56, true);
			loadTextBox.Name = "loadTextBox";
			loadTextBox.PreBoundMaxLength = 5;
			loadTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			loadTextBox.TabIndex = 4;
			// 
			// vesselTextBox
			// 
			this.BindingSource.SetBindingMember(vesselTextBox, "OriginalShipment.Sailings.JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.SplitBookingsHeader)(null)).OriginalShipment.Sailings)).SyncRoot)).JX_JV_NKVessel)));
			vesselTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitBookingsControl|06a5d76e-33c8-4206-9754-1b3e40aa6c7e", "Vessel");
			vesselTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 32, true);
			vesselTextBox.Name = "vesselTextBox";
			vesselTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			vesselTextBox.TabIndex = 2;
			// 
			// OriginalShipmentNumber
			// 
			this.BindingSource.SetBindingMember(OriginalShipmentNumber, "OriginalShipment.JS_UniqueConsignRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.SplitBookingsHeader)(null)).OriginalShipment.JS_UniqueConsignRef)));
			OriginalShipmentNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			OriginalShipmentNumber.Name = "OriginalShipmentNumber";
			OriginalShipmentNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			OriginalShipmentNumber.TabIndex = 0;
			// 
			// gridsPlaceHolder
			// 
			this.gridsPlaceHolder.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridsPlaceHolder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.gridsPlaceHolder.Name = "gridsPlaceHolder";
			this.gridsPlaceHolder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 490, true);
			this.gridsPlaceHolder.TabIndex = 1;
			this.gridsPlaceHolder.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, true);
			// 
			// SplitBookingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.gridsPlaceHolder);
			this.Controls.Add(headerPanel);
			this.Name = "SplitBookingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 570, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			headerPanel.ResumeLayout(false);
			headerPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		CargoWise.Windows.UI.KPanel gridsPlaceHolder;
		CargoWise.Windows.UI.KPanel headerPanel;
		Enterprise.ZArchitecture.ZTextBox originalBillOfLading;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox dischargeTextBox;
		Enterprise.ZArchitecture.ZTextBox voyageTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox loadTextBox;
		Enterprise.ZArchitecture.ZTextBox vesselTextBox;
		Enterprise.ZArchitecture.ZTextBox OriginalShipmentNumber;
	}
}
