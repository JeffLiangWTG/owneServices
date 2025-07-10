namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageLegTile
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
			this.PickupOrgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryOrgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PickupDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeliveryDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WhatLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SequenceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WaitPointOrgLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WaitPointDateLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonCartageLeg);
			// 
			// PickupOrgLabel
			// 
			this.PickupOrgLabel.AutoEllipsis = true;
			this.PickupOrgLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.PickupOrgLabel, "PickupAddressCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).PickupAddressCode)));
			this.PickupOrgLabel.ForeColor = System.Drawing.Color.Black;
			this.PickupOrgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 15, true);
			this.PickupOrgLabel.Name = "PickupOrgLabel";
			this.PickupOrgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 15, true);
			this.PickupOrgLabel.TabIndex = 0;
			// 
			// DeliveryOrgLabel
			// 
			this.DeliveryOrgLabel.AutoEllipsis = true;
			this.DeliveryOrgLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.DeliveryOrgLabel, "DeliveryAddressCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).DeliveryAddressCode)));
			this.DeliveryOrgLabel.ForeColor = System.Drawing.Color.Black;
			this.DeliveryOrgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 33, true);
			this.DeliveryOrgLabel.Name = "DeliveryOrgLabel";
			this.DeliveryOrgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 15, true);
			this.DeliveryOrgLabel.TabIndex = 1;
			// 
			// PickupDateLabel
			// 
			this.PickupDateLabel.AutoEllipsis = true;
			this.PickupDateLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.PickupDateLabel, "PickupTimeSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).PickupTimeSummary)));
			this.PickupDateLabel.ForeColor = System.Drawing.Color.Black;
			this.PickupDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 15, true);
			this.PickupDateLabel.Name = "PickupDateLabel";
			this.PickupDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 15, true);
			this.PickupDateLabel.TabIndex = 2;
			// 
			// DeliveryDateLabel
			// 
			this.DeliveryDateLabel.AutoEllipsis = true;
			this.DeliveryDateLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.DeliveryDateLabel, "DeliveryTimeSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).DeliveryTimeSummary)));
			this.DeliveryDateLabel.ForeColor = System.Drawing.Color.Black;
			this.DeliveryDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 33, true);
			this.DeliveryDateLabel.Name = "DeliveryDateLabel";
			this.DeliveryDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 15, true);
			this.DeliveryDateLabel.TabIndex = 3;
			// 
			// WhatLabel
			// 
			this.WhatLabel.AutoEllipsis = true;
			this.WhatLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.WhatLabel, "WhatSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).WhatSummary)));
			this.WhatLabel.IsFontBold = true;
			this.WhatLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 0, true);
			this.WhatLabel.Name = "WhatLabel";
			this.WhatLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 16, true);
			this.WhatLabel.TabIndex = 4;
			this.WhatLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// SequenceLabel
			// 
			this.SequenceLabel.AutoEllipsis = true;
			this.SequenceLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.SequenceLabel, "SequenceAndActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).SequenceAndActive)));
			this.SequenceLabel.IsFontBold = true;
			this.SequenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 0, true);
			this.SequenceLabel.Name = "SequenceLabel";
			this.SequenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 16, true);
			this.SequenceLabel.TabIndex = 5;
			this.SequenceLabel.Text = "99 ♦";
			this.SequenceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// WaitPointOrgLabel
			// 
			this.WaitPointOrgLabel.AutoEllipsis = true;
			this.WaitPointOrgLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.WaitPointOrgLabel, "WaitPointAddressCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).WaitPointAddressCode)));
			this.WaitPointOrgLabel.ForeColor = System.Drawing.Color.Black;
			this.WaitPointOrgLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 58, true);
			this.WaitPointOrgLabel.Name = "WaitPointOrgLabel";
			this.WaitPointOrgLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 15, true);
			this.WaitPointOrgLabel.TabIndex = 6;
			// 
			// WaitPointDateLabel
			// 
			this.WaitPointDateLabel.AutoEllipsis = true;
			this.WaitPointDateLabel.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.WaitPointDateLabel, "WaitPointTimeSummary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonCartageLeg)(null)).WaitPointTimeSummary)));
			this.WaitPointDateLabel.ForeColor = System.Drawing.Color.Black;
			this.WaitPointDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 58, true);
			this.WaitPointDateLabel.Name = "WaitPointDateLabel";
			this.WaitPointDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 15, true);
			this.WaitPointDateLabel.TabIndex = 7;
			// 
			// CartageLegTile
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
			this.Controls.Add(this.WaitPointDateLabel);
			this.Controls.Add(this.WaitPointOrgLabel);
			this.Controls.Add(this.SequenceLabel);
			this.Controls.Add(this.WhatLabel);
			this.Controls.Add(this.DeliveryDateLabel);
			this.Controls.Add(this.PickupDateLabel);
			this.Controls.Add(this.DeliveryOrgLabel);
			this.Controls.Add(this.PickupOrgLabel);
			this.ForeColor = System.Drawing.Color.Black;
			this.Name = "CartageLegTile";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PanelBorderColor = System.Drawing.Color.Transparent;
			this.RoundCorners = 0;
			this.ShowBorderAsBottomLine = true;
			this.ShowPanel = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 76, true);
			this.TileColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.TileColorGradient = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
			this.TileGradient = Enterprise.Freight.LocalCartage.GUI.ZGroupBoxTile.TilesGradient.Vertical;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel PickupOrgLabel;
		private Enterprise.ZArchitecture.ZLabel DeliveryOrgLabel;
		private Enterprise.ZArchitecture.ZLabel PickupDateLabel;
		private Enterprise.ZArchitecture.ZLabel DeliveryDateLabel;
		internal Enterprise.ZArchitecture.ZLabel WhatLabel;
		private Enterprise.ZArchitecture.ZLabel SequenceLabel;
		private ZArchitecture.ZLabel WaitPointOrgLabel;
		private ZArchitecture.ZLabel WaitPointDateLabel;
	}
}
