namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class GoodsShippingLocationAndGIKUserControl
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
			this.GoodsShippingLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GIKEnabledCheckBox = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsShippingLocationDropEdit.SuspendLayout();
			this.GIKEnabledCheckBox.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsHeader);
			// 
			// GoodsShippingLocationDropEdit
			// 
			this.GoodsShippingLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsShippingLocationDropEdit, "MovementHeader.BM_LocationOfGoodsCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_LocationOfGoodsCode)));
			this.GoodsShippingLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 4, true);
			this.GoodsShippingLocationDropEdit.Name = "GoodsShippingLocationDropEdit";
			this.GoodsShippingLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 18, true);
			this.GoodsShippingLocationDropEdit.TabIndex = 0;
			this.GoodsShippingLocationDropEdit.ShowDescriptionBox = false;
			// 
			// GIKEnabledCheckBox
			// 
			this.BindingSource.SetBindingMember(this.GIKEnabledCheckBox, "MovementHeader.IsGIKEnabled");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).MovementHeader.IsGIKEnabled)));
			this.GIKEnabledCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.GIKEnabledCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GIKEnabledCheckBox.CaptionResourceString = null;
			this.GIKEnabledCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 4, true);
			this.GIKEnabledCheckBox.Name = "GIKEnabledCheckBox";
			this.GIKEnabledCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.GIKEnabledCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.GIKEnabledCheckBox.TabIndex = 1;
			this.GIKEnabledCheckBox.UseVisualStyleBackColor = true;
			//
			// GoodsShippingLocationAndGIKUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GoodsShippingLocationDropEdit);
			this.Controls.Add(this.GIKEnabledCheckBox);
			this.Name = "GoodsShippingLocationAndGIKUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsShippingLocationDropEdit.ResumeLayout(true);
			this.GoodsShippingLocationDropEdit.PerformLayout();
			this.GIKEnabledCheckBox.ResumeLayout(true);
			this.GIKEnabledCheckBox.PerformLayout();
		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZDropEdit GoodsShippingLocationDropEdit;
		public ZArchitecture.GUI.ZCheckBox GIKEnabledCheckBox;
	}
}
