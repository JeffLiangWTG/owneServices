namespace Enterprise.Customs.GUI
{
	partial class ShipmentDetailsIncoTermsUserControl
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
			this.IncoTermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncoTermExplainButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncoTermDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermDropEdit, "JE_ShipmentIncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ShipmentIncoTerm)));
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncoTermDropEdit.Name = "IncoTermDropEdit";
			this.IncoTermDropEdit.PreBoundMaxLength = 3;
			this.IncoTermDropEdit.ShouldResizeByMaxLength = true;
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.IncoTermDropEdit.TabIndex = 0;
			// 
			// IncoTermExplainButton
			// 
			this.IncoTermExplainButton.IsCaptionOverridden = true;
			this.IncoTermExplainButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 0, true);
			this.IncoTermExplainButton.Name = "IncoTermExplainButton";
			this.IncoTermExplainButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.IncoTermExplainButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 21, true);
			this.IncoTermExplainButton.TabIndex = 1;
			this.IncoTermExplainButton.Text = "...";
			this.IncoTermExplainButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.IncoTermExplainButton.ToolTipCaption = null;
			this.IncoTermExplainButton.Click += new System.EventHandler(this.IncoTermExplainButton_Click);
			// 
			// ShipmentDetailsIncoTermsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncoTermDropEdit);
			this.Controls.Add(this.IncoTermExplainButton);
			this.Name = "ShipmentDetailsIncoTermsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit IncoTermDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton IncoTermExplainButton;
	}
}
