namespace Enterprise.Warehouse.Environment.Module
{
	partial class AssignProductToPickFaceControl
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
			this.AssignProductToPickFacesLabel = new Enterprise.ZArchitecture.ZHeaderLabel();
			this.ProductFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProductFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// AssignProductToPickFacesLabel
			// 
			this.AssignProductToPickFacesLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.AssignProductToPickFacesLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(52)))), ((int)(((byte)(121)))));
			this.AssignProductToPickFacesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.AssignProductToPickFacesLabel.Name = "AssignProductToPickFacesLabel";
			this.AssignProductToPickFacesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(435, 23, true);
			this.AssignProductToPickFacesLabel.TabIndex = 1;
			this.AssignProductToPickFacesLabel.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("a63992a0-3fd8-44e3-9a75-a778781469e9", "Assign Product to Pick Faces");
			// 
			// ProductFindBox
			// 
			this.ProductFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductFindBox, "ProductPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Environment.Module.AssignProductToPickFaceActionMethodApplicator)(null)).ProductPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Environment.Module.AssignProductToPickFaceActionMethodApplicator)(null)).Products)));
			this.ProductFindBox.BindToList = "Products";
			this.ProductFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ProductFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 47, true);
			this.ProductFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.ProductFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsConfigProduct;
			this.ProductFindBox.Name = "ProductFindBox";
			this.ProductFindBox.ShouldResize = true;
			this.ProductFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 15, true);
			this.ProductFindBox.TabIndex = 3;
			// 
			// AssignProductToPickFaceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ProductFindBox);
			this.Controls.Add(this.AssignProductToPickFacesLabel);
			this.Name = "AssignProductToPickFaceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 214, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProductFindBox.ResumeLayout(true);
			this.ProductFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZHeaderLabel AssignProductToPickFacesLabel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ProductFindBox;
	}
}
