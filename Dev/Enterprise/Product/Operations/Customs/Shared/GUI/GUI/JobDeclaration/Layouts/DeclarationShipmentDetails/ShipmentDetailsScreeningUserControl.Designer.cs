namespace Enterprise.Customs.GUI
{
	partial class ShipmentDetailsScreeningUserControl
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
			this.ScreenButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ScreeningStatusDropEdit = new Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ScreeningStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// ScreenButton
			// 
			this.ScreenButton.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ScreenButton.IsCaptionOverridden = true;
			this.ScreenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 0, true);
			this.ScreenButton.Name = "ScreenButton";
			this.ScreenButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ScreenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.ScreenButton.TabIndex = 1;
			this.ScreenButton.TabStop = false;
			this.ScreenButton.Text = "...";
			this.ScreenButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ScreenButton.ToolTipCaption = null;
			this.ScreenButton.UseVisualStyleBackColor = true;
			this.ScreenButton.Click += new System.EventHandler(this.ScreenButton_Click);
			// 
			// ScreeningStatusDropEdit
			// 
			this.ScreeningStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ScreeningStatusDropEdit, "JE_ScreeningStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ScreeningStatus)));
			this.ScreeningStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ScreeningStatusDropEdit.Name = "ScreeningStatusDropEdit";
			this.ScreeningStatusDropEdit.PreBoundMaxLength = 3;
			this.ScreeningStatusDropEdit.ShouldResizeByMaxLength = true;
			this.ScreeningStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 20, true);
			this.ScreeningStatusDropEdit.TabIndex = 0;
			// 
			// ShipmentDetailsScreeningUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ScreenButton);
			this.Controls.Add(this.ScreeningStatusDropEdit);
			this.Name = "ShipmentDetailsScreeningUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ScreeningStatusDropEdit.ResumeLayout(true);
			this.ScreeningStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton ScreenButton;
		internal Enterprise.DeniedPartyScreening.GUI.DeniedPartyScreeningStatusDropEdit ScreeningStatusDropEdit;
	}
}
