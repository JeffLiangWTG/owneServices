using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI
{
	partial class RoundingsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DefaultRoundingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultRoundingsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.DefaultRoundingsCollection);
			// 
			// DefaultRoundingsGrid
			// 
			this.DefaultRoundingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DefaultRoundingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.DefaultRoundings)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.DefaultRoundings)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.DefaultRoundings)(null)).RateCategoriesAndGroupsList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.DefaultRoundings)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.DefaultRoundings)(null)).RoundingType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.DefaultRoundings)(null)).RoundingTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Rating.Business.DefaultRoundings)(null)).RoundingFactor)));
			this.DefaultRoundingsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "RateCategoriesAndGroupsList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RoundingsControl|0307fa99-6c33-4c5c-92b4-187ccb46db52", "Rate");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RoundingsControl|f57e62f2-cb06-4e41-a18a-f245fa0124dc", "Rate Type");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RoundingsControl|74493740-1943-49ad-bc5d-76a7009aada9", "Rounding Factor");
			zCalcEditColumnStyleInfo1.ColumnName = "RoundingFactor";
			zCalcEditColumnStyleInfo1.Decimals = 4;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Rating.GUI.Res.GetData("RoundingsControl|04ac5f28-272e-400e-90d7-5766c354cfc3", "Rounding");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "RoundingTypeList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RoundingsControl|04ac5f28-272e-400e-90d7-5766c354cfc3", "Rounding");
			zDropEditColumnStyleInfo2.ColumnName = "RoundingType";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Rating.GUI.Res.GetData("RoundingsControl|04ac5f28-272e-400e-90d7-5766c354cfc3", "Rounding");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.DefaultRoundingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DefaultRoundingsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DefaultRoundingsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DefaultRoundingsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DefaultRoundingsGrid.GridId = "fe8e8e8d-59fa-48e4-a14c-5d3105ea27ff";
			this.DefaultRoundingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DefaultRoundingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DefaultRoundingsGrid.LayoutKey = "DefaultRoundingsGrid";
			this.DefaultRoundingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DefaultRoundingsGrid.Name = "DefaultRoundingsGrid";
			this.DefaultRoundingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			this.DefaultRoundingsGrid.TabIndex = 0;
			// 
			// RoundingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultRoundingsGrid);
			this.Name = "RoundingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 396, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DefaultRoundingsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid DefaultRoundingsGrid;

	}
}
