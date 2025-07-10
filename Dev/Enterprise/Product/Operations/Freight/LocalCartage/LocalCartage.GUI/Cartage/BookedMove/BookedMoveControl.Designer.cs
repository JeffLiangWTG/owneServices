namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class BookedMoveControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DistanceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcDropEdit2 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit3 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zCalcDropEdit4 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zLabel10 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcDropEdit5 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.zLabel11 = new Enterprise.ZArchitecture.ZLabel();
			this.zCalcEdit3 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit4 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LooseDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DistancePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PostcodeDistanceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcDropEdit1 = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.PickupAddressControl = new Enterprise.Freight.LocalCartage.GUI.BookedMovePickupControl();
			this.DeliveryAddressControl = new Enterprise.Freight.LocalCartage.GUI.BookedMoveDeliveryControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LooseDetailsPanel.SuspendLayout();
			this.DistancePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove);
			// 
			// DistanceButton
			// 
			this.DistanceButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("BookedMoveControl|d6f441c3-ace7-47c1-b8a5-2c6bb0717361", "Calculate Distance");
			this.DistanceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 108, true);
			this.DistanceButton.Name = "DistanceButton";
			this.DistanceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 23, true);
			this.DistanceButton.TabIndex = 3;
			this.DistanceButton.UseVisualStyleBackColor = true;
			// 
			// zLabel9
			// 
			this.BindingSource.SetBindingMember(this.zLabel9, "EW_DistanceUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_DistanceUnit)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel9, false);
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 41, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.zLabel9.TabIndex = 1;
			// 
			// zCalcDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit2, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_BookedPackCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_F3_NKPackType)));
			this.zCalcDropEdit2.BindToAmount = "EW_BookedPackCount";
			this.zCalcDropEdit2.BindToUnit = "EW_F3_NKPackType";
			this.zCalcDropEdit2.Decimals = 2;
			this.zCalcDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 0, true);
			this.zCalcDropEdit2.Name = "zCalcDropEdit2";
			this.zCalcDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zCalcDropEdit2.TabIndex = 0;
			this.zCalcDropEdit2.UnitPreBoundMaxLength = 3;
			// 
			// zCalcDropEdit3
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit3, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_BookedLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_DimUnit)));
			this.zCalcDropEdit3.BindToAmount = "EW_BookedLength";
			this.zCalcDropEdit3.BindToUnit = "EW_DimUnit";
			this.zCalcDropEdit3.Decimals = 2;
			this.zCalcDropEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 63, true);
			this.zCalcDropEdit3.Name = "zCalcDropEdit3";
			this.zCalcDropEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zCalcDropEdit3.TabIndex = 3;
			this.zCalcDropEdit3.UnitPreBoundMaxLength = 2;
			// 
			// zCalcDropEdit4
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit4, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_BookedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_WeightUQ)));
			this.zCalcDropEdit4.BindToAmount = "EW_BookedWeight";
			this.zCalcDropEdit4.BindToUnit = "EW_WeightUQ";
			this.zCalcDropEdit4.Decimals = 2;
			this.zCalcDropEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 21, true);
			this.zCalcDropEdit4.Name = "zCalcDropEdit4";
			this.zCalcDropEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zCalcDropEdit4.TabIndex = 1;
			this.zCalcDropEdit4.UnitPreBoundMaxLength = 2;
			// 
			// zLabel10
			// 
			this.BindingSource.SetBindingMember(this.zLabel10, "EW_DimUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_DimUnit)));
			this.zLabel10.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 85, true);
			this.zLabel10.Name = "zLabel10";
			this.zLabel10.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.zLabel10.TabIndex = 6;
			// 
			// zCalcDropEdit5
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit5, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_BookedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_VolumeUQ)));
			this.zCalcDropEdit5.BindToAmount = "EW_BookedVolume";
			this.zCalcDropEdit5.BindToUnit = "EW_VolumeUQ";
			this.zCalcDropEdit5.Decimals = 3;
			this.zCalcDropEdit5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 42, true);
			this.zCalcDropEdit5.Name = "zCalcDropEdit5";
			this.zCalcDropEdit5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zCalcDropEdit5.TabIndex = 2;
			this.zCalcDropEdit5.UnitPreBoundMaxLength = 2;
			// 
			// zLabel11
			// 
			this.BindingSource.SetBindingMember(this.zLabel11, "EW_DimUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_DimUnit)));
			this.zLabel11.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 106, true);
			this.zLabel11.Name = "zLabel11";
			this.zLabel11.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.zLabel11.TabIndex = 7;
			// 
			// zCalcEdit3
			// 
			this.zCalcEdit3.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit3, "EW_BookedWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_BookedWidth)));
			this.zCalcEdit3.DecimalPlaces = 2;
			this.zCalcEdit3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 84, true);
			this.zCalcEdit3.Name = "zCalcEdit3";
			this.zCalcEdit3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.zCalcEdit3.TabIndex = 4;
			this.zCalcEdit3.Text = "0.000";
			this.zCalcEdit3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit4
			// 
			this.zCalcEdit4.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.zCalcEdit4, "EW_BookedHeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_BookedHeight)));
			this.zCalcEdit4.DecimalPlaces = 2;
			this.zCalcEdit4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(53, 105, true);
			this.zCalcEdit4.Name = "zCalcEdit4";
			this.zCalcEdit4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.zCalcEdit4.TabIndex = 5;
			this.zCalcEdit4.Text = "0.000";
			this.zCalcEdit4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LooseDetailsPanel
			// 
			this.LooseDetailsPanel.Controls.Add(this.zCalcDropEdit2);
			this.LooseDetailsPanel.Controls.Add(this.zCalcEdit4);
			this.LooseDetailsPanel.Controls.Add(this.zCalcEdit3);
			this.LooseDetailsPanel.Controls.Add(this.zLabel11);
			this.LooseDetailsPanel.Controls.Add(this.zCalcDropEdit5);
			this.LooseDetailsPanel.Controls.Add(this.zCalcDropEdit3);
			this.LooseDetailsPanel.Controls.Add(this.zLabel10);
			this.LooseDetailsPanel.Controls.Add(this.zCalcDropEdit4);
			this.LooseDetailsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LooseDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LooseDetailsPanel.Name = "LooseDetailsPanel";
			this.LooseDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 133, true);
			this.LooseDetailsPanel.TabIndex = 0;
			// 
			// DistancePanel
			// 
			this.DistancePanel.Controls.Add(this.PostcodeDistanceCalcEdit);
			this.DistancePanel.Controls.Add(this.zCalcDropEdit1);
			this.DistancePanel.Controls.Add(this.zLabel9);
			this.DistancePanel.Controls.Add(this.DistanceButton);
			this.DistancePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.DistancePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(576, 0, true);
			this.DistancePanel.Name = "DistancePanel";
			this.DistancePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 133, true);
			this.DistancePanel.TabIndex = 2;
			// 
			// PostcodeDistanceCalcEdit
			// 
			this.PostcodeDistanceCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PostcodeDistanceCalcEdit, "PostcodeDistance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).PostcodeDistance)));
			this.PostcodeDistanceCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.PostcodeDistanceCalcEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.PostcodeDistanceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 42, true);
			this.PostcodeDistanceCalcEdit.Name = "PostcodeDistanceCalcEdit";
			this.PostcodeDistanceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.PostcodeDistanceCalcEdit.TabIndex = 0;
			this.PostcodeDistanceCalcEdit.Text = "0.000";
			this.PostcodeDistanceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcDropEdit1, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_Distance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonBookedCtgMove)(null)).EW_DistanceUnit)));
			this.zCalcDropEdit1.BindToAmount = "EW_Distance";
			this.zCalcDropEdit1.BindToUnit = "EW_DistanceUnit";
			this.zCalcDropEdit1.Decimals = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcDropEdit1, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 84, true);
			this.zCalcDropEdit1.Name = "zCalcDropEdit1";
			this.zCalcDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.zCalcDropEdit1.TabIndex = 2;
			this.zCalcDropEdit1.UnitPreBoundMaxLength = 3;
			// 
			// PickupAddressControl
			// 
			this.BindingSource.SetBindingMember(this.PickupAddressControl, ".");
			this.PickupAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.PickupAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 0, true);
			this.PickupAddressControl.Name = "PickupAddressControl";
			this.PickupAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 133, true);
			this.PickupAddressControl.TabIndex = 1;
			// 
			// DeliveryAddressControl
			// 
			this.BindingSource.SetBindingMember(this.DeliveryAddressControl, ".");
			this.DeliveryAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.DeliveryAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(692, 0, true);
			this.DeliveryAddressControl.Name = "DeliveryAddressControl";
			this.DeliveryAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 133, true);
			this.DeliveryAddressControl.TabIndex = 3;
			// 
			// BookedMoveControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DeliveryAddressControl);
			this.Controls.Add(this.DistancePanel);
			this.Controls.Add(this.PickupAddressControl);
			this.Controls.Add(this.LooseDetailsPanel);
			this.Name = "BookedMoveControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1126, 133, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LooseDetailsPanel.ResumeLayout(false);
			this.LooseDetailsPanel.PerformLayout();
			this.DistancePanel.ResumeLayout(false);
			this.DistancePanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel zLabel9;
		private Enterprise.ZArchitecture.GUI.ZButton DistanceButton;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit2;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit3;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit4;
		private Enterprise.ZArchitecture.ZLabel zLabel10;
		internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit5;
		private Enterprise.ZArchitecture.ZLabel zLabel11;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit3;
		private Enterprise.ZArchitecture.ZCalcEdit zCalcEdit4;
		private Enterprise.ZArchitecture.GUI.ZPanel LooseDetailsPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel DistancePanel;
		private BookedMovePickupControl PickupAddressControl;
		private BookedMoveDeliveryControl DeliveryAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCalcDropEdit zCalcDropEdit1;
		private Enterprise.ZArchitecture.ZCalcEdit PostcodeDistanceCalcEdit;


	}
}
