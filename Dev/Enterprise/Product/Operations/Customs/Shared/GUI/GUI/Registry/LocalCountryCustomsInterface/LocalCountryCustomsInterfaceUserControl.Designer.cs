namespace Enterprise.Customs.DataRegistry.GUI
{
	partial class LocalCountryCustomsInterfaceUserControl
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
			this.RecipientIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubmissionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InterfaceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ABMInterfaceActivatedLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SubmissionTypeDropEdit.SuspendLayout();
			this.InterfaceTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DataRegistry.Business.LocalCountryCustomsInterface);
			// 
			// RecipientIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.RecipientIDTextBox, "RecipientID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DataRegistry.Business.LocalCountryCustomsInterface)(null)).RecipientID)));
			this.RecipientIDTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("80b32029-22ed-4f25-ae2a-96496f4f0508", "Recipient ID");
			this.RecipientIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 60, true);
			this.RecipientIDTextBox.Name = "RecipientIDTextBox";
			this.RecipientIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.RecipientIDTextBox.TabIndex = 2;
			// 
			// SubmissionTypeDropEdit
			// 
			this.SubmissionTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubmissionTypeDropEdit, "SubmissionType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DataRegistry.Business.LocalCountryCustomsInterface)(null)).SubmissionType)));
			this.SubmissionTypeDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("8f603abc-f79b-456a-b95c-c82cd38170f0", "", "Submit Type", "Submission Type", "");
			this.SubmissionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 8, true);
			this.SubmissionTypeDropEdit.Name = "SubmissionTypeDropEdit";
			this.SubmissionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.SubmissionTypeDropEdit.TabIndex = 0;
			// 
			// InterfaceTypeDropEdit
			// 
			this.InterfaceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InterfaceTypeDropEdit, "InterfaceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DataRegistry.Business.LocalCountryCustomsInterface)(null)).InterfaceType)));
			this.InterfaceTypeDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("C225C118-C2DB-462E-8103-81FF1C7FB16F", "", "Submit Type", "Interface Type", "");
			this.InterfaceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 34, true);
			this.InterfaceTypeDropEdit.Name = "InterfaceTypeDropEdit";
			this.InterfaceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.InterfaceTypeDropEdit.TabIndex = 1;
			// 
			// ABMInterfaceActivatedLabel
			// 
			this.ABMInterfaceActivatedLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("f101c36c-78e1-4296-b4c4-e1531866cd95", "ABM Interface is configured for this company!");
			this.ABMInterfaceActivatedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ABMInterfaceActivatedLabel.ForeColor = System.Drawing.Color.Purple;
			this.ABMInterfaceActivatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 57, true);
			this.ABMInterfaceActivatedLabel.Name = "ABMInterfaceActivatedLabel";
			this.ABMInterfaceActivatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 23, true);
			this.ABMInterfaceActivatedLabel.TabIndex = 2;
			this.ABMInterfaceActivatedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.ABMInterfaceActivatedLabel.Visible = false;
			// 
			// LocalCountryCustomsInterfaceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ABMInterfaceActivatedLabel);
			this.Controls.Add(this.RecipientIDTextBox);
			this.Controls.Add(this.SubmissionTypeDropEdit);
			this.Controls.Add(this.InterfaceTypeDropEdit);
			this.Name = "LocalCountryCustomsInterfaceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SubmissionTypeDropEdit.ResumeLayout(true);
			this.SubmissionTypeDropEdit.PerformLayout();
			this.InterfaceTypeDropEdit.ResumeLayout(true);
			this.InterfaceTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox RecipientIDTextBox;
		internal ZArchitecture.GUI.ZDropEdit SubmissionTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit InterfaceTypeDropEdit;
		internal ZArchitecture.ZLabel ABMInterfaceActivatedLabel;
	}
}
