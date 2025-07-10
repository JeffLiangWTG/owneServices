using Enterprise.Environment;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class DangerousGoodsControl
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
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.dangerousGoodsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.dangerousGoodsDisplayGrid = new Enterprise.ZArchitecture.GUI.ZGridWithoutColumnStylesSerialisation();
			this.dangerousGoodsClassColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.dangerousGoodsSubstanceColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dangerousGoodsDisplayGrid)).BeginInit();
			this.dangerousGoodsDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// DangerousGoodsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.dangerousGoodsCheckBox, "JK_IsHazardous");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.CommonConsol)(null)).JK_IsHazardous)));
			this.dangerousGoodsCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DangerousGoodsControl|7e180e7f-4d66-4baf-45a5-0df3eae5180f", "Is Hazardous");
			this.dangerousGoodsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.dangerousGoodsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.dangerousGoodsCheckBox.Name = "DangerousGoodsCheckBox";
			this.dangerousGoodsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.dangerousGoodsCheckBox.TabIndex = 9;
			// 
			// DangerousGoodsColumns
			// 
			this.dangerousGoodsClassColumn.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.dangerousGoodsClassColumn.ColumnName = "JKD_Class";
			this.dangerousGoodsClassColumn.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DangerousGoodsControl|7d9a0273-022f-fd8b-483b-49ec208ba826", "DG Class");
			this.dangerousGoodsClassColumn.BindToList = "Lookups.DGClassLookup";
			this.dangerousGoodsClassColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			this.dangerousGoodsSubstanceColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.dangerousGoodsSubstanceColumn.ColumnName = "JKD_Calc_Substance";
			this.dangerousGoodsSubstanceColumn.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DangerousGoodsControl|abed15e4-9e5a-6c8b-447f-72d42af3c266", "DG Substance");
			this.dangerousGoodsSubstanceColumn.BindToList = "Lookups.Substance";
			// 
			// DangerousGoodsDisplayGrid
			// 
			this.BindingSource.SetBindingMember(this.dangerousGoodsDisplayGrid, "ConsolDGRestrictionCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ConsolDGRestrictionCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ConsolDGRestrictions)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ConsolDGRestrictionCollection)).SyncRoot)).JKD_Class)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ConsolDGRestrictions)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.ForwardingConsol)(null)).ConsolDGRestrictionCollection)).SyncRoot)).JKD_Calc_Substance)));
			this.dangerousGoodsDisplayGrid.Name = "DangerousGoodsDisplayGrid";
			this.dangerousGoodsDisplayGrid.ColumnHeadersVisible = true;
			this.dangerousGoodsDisplayGrid.ColumnStyles.Add(dangerousGoodsClassColumn);
			this.dangerousGoodsDisplayGrid.ColumnStyles.Add(dangerousGoodsSubstanceColumn);
			this.dangerousGoodsDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dangerousGoodsDisplayGrid.LayoutKey = "zGrid1";
			this.dangerousGoodsDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.dangerousGoodsDisplayGrid.Name = "DangerousGoodsDisplayGrid";
			this.dangerousGoodsDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(190, 80, true);
			this.dangerousGoodsDisplayGrid.TabIndex = 10;
			this.dangerousGoodsDisplayGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.dangerousGoodsDisplayGrid.GridId = "fd161ed9-daa6-478d-4d58-7309c49e30c3";
			//
			// Make "Is Hazardous" tickbox toggle the grid being readonly
			//
			this.dangerousGoodsDisplayGrid.ReadOnly = !Env.Security.MaintainConsolAllowEditOfAcceptedDangerousGoodsOnConsol.IsAllowed;
			this.dangerousGoodsCheckBox.CheckStateChanged += DangerousGoodsCheckBox_CheckStateChanged;

			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.dangerousGoodsCheckBox);
			this.Controls.Add(this.dangerousGoodsDisplayGrid);
			this.Name = "DangerousGoodsControl";
			this.Controls.SetChildIndex(this.dangerousGoodsDisplayGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.dangerousGoodsDisplayGrid)).EndInit();
			this.dangerousGoodsDisplayGrid.ResumeLayout(false);
			this.dangerousGoodsDisplayGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox dangerousGoodsCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGridWithoutColumnStylesSerialisation dangerousGoodsDisplayGrid;
		private Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo dangerousGoodsClassColumn;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo dangerousGoodsSubstanceColumn;
	}
}
