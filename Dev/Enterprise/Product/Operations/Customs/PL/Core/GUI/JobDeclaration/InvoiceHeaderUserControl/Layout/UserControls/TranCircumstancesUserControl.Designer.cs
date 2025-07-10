namespace Enterprise.Customs.PL.GUI
{
	partial class TranCircumstancesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TranCircumstances1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalTranCircumstancesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalTranCircumstancesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TranCircumstances1DropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader);
			// 
			// TranCircumstances1DropEdit
			// 
			this.TranCircumstances1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TranCircumstances1DropEdit, "TranCircumstanceCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(null)).TranCircumstanceCode1)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TranCircumstances1DropEdit, false);
			this.TranCircumstances1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TranCircumstances1DropEdit.Name = "TranCircumstances1DropEdit";
			this.TranCircumstances1DropEdit.PreBoundMaxLength = 5;
			this.TranCircumstances1DropEdit.ShowDescriptionBox = false;
			this.TranCircumstances1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.TranCircumstances1DropEdit.TabIndex = 0;
			// 
			// AdditionalTranCircumstancesTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalTranCircumstancesTextBox, "AdditionalTranCircumstanceCodesAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.JobComInvoiceHeader)(null)).AdditionalTranCircumstanceCodesAsString)));
			this.AdditionalTranCircumstancesTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalTranCircumstancesTextBox, false);
			this.AdditionalTranCircumstancesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 0, true);
			this.AdditionalTranCircumstancesTextBox.Name = "AdditionalTranCircumstancesTextBox";
			this.AdditionalTranCircumstancesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.AdditionalTranCircumstancesTextBox.TabIndex = 1;
			this.AdditionalTranCircumstancesTextBox.TabStop = false;
			// 
			// AdditionalTranCircumstancesEditButton
			// 
			this.AdditionalTranCircumstancesEditButton.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("AdditionalTranCircumstancesEditButton_Click|C87CC51B-FCD0-4C27-AF08-9D5EBA707924", "More...");
			this.AdditionalTranCircumstancesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 0, true);
			this.AdditionalTranCircumstancesEditButton.Name = "AdditionalTranCircumstancesEditButton";
			this.AdditionalTranCircumstancesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.AdditionalTranCircumstancesEditButton.TabIndex = 2;
			this.AdditionalTranCircumstancesEditButton.ToolTipCaption = null;
			this.AdditionalTranCircumstancesEditButton.Click += new System.EventHandler(this.AdditionalTranCircumstancesEditButton_Click);
			// 
			// TranCircumstancesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TranCircumstances1DropEdit);
			this.Controls.Add(this.AdditionalTranCircumstancesTextBox);
			this.Controls.Add(this.AdditionalTranCircumstancesEditButton);
			this.Name = "TranCircumstancesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TranCircumstances1DropEdit.ResumeLayout(true);
			this.TranCircumstances1DropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TranCircumstances1DropEdit;
		internal Enterprise.ZArchitecture.ZTextBox AdditionalTranCircumstancesTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton AdditionalTranCircumstancesEditButton;
	}
}
