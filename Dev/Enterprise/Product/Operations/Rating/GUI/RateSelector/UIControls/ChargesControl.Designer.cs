using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Views;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	partial class ChargesControl
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
            this.pnlTop = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.lblGroupName = new Enterprise.ZArchitecture.ZLabel();
            this.pnlItemsContainer = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.RateSelector.Models.ChargesViewModel);
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.Color.LightGray;
            this.pnlTop.Controls.Add(this.lblGroupName);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.ForeColor = System.Drawing.Color.DimGray;
            this.pnlTop.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 21, true);
            this.pnlTop.TabIndex = 0;
            // 
            // lblGroupName
            // 
            this.lblGroupName.AutoSize = true;
            this.BindingSource.SetBindingMember(this.lblGroupName, "GroupName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Rating.GUI.RateSelector.Models.ChargesViewModel)(null)).GroupName)));
            this.lblGroupName.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblGroupName.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.lblGroupName.IsFontBold = true;
            this.lblGroupName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.lblGroupName.Name = "lblGroupName";
            this.lblGroupName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 13, true);
            this.lblGroupName.TabIndex = 0;
            this.lblGroupName.Text = "GroupName";
            // 
            // pnlItemsContainer
            // 
            this.pnlItemsContainer.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlItemsContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 21, true);
            this.pnlItemsContainer.Name = "pnlItemsContainer";
            this.pnlItemsContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 16, true);
            this.pnlItemsContainer.TabIndex = 1;
            // 
            // ChargesControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.pnlItemsContainer);
            this.Controls.Add(this.pnlTop);
            this.Name = "ChargesControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 129, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel pnlTop;
		private ZArchitecture.ZLabel lblGroupName;
		private ZArchitecture.GUI.ZPanel pnlItemsContainer;
	}
}
