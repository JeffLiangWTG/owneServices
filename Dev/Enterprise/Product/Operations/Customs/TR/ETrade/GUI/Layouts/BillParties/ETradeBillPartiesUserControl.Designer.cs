using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.ETrade.GUI
{
	partial class ETradeBillPartiesUserControl
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
			this.SupplementaryDeclarationNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupplementaryDeclarationDeliveryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SupplementaryDeclarationRegNoIdNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeliveryPartyDetailsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupplementaryDeclarationDeliveryDateDateEdit.SuspendLayout();
			this.DeliveryPartyDetailsSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.ETrade.Business.AsycudaBill);
			// 
			// DeliveryPartyDetailsSeparatorUserControl
			// 
			this.DeliveryPartyDetailsSeparatorUserControl.AllowDrop = true;
			this.DeliveryPartyDetailsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.TR.ETrade.GUI.Res.GetData("6D8131C6-338B-4BBD-ACC1-6D6BEEEDEAE4", "Delivery Party Details");
			this.DeliveryPartyDetailsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 0, true);
			this.DeliveryPartyDetailsSeparatorUserControl.Name = "DeliveryPartyDetailsSeparatorUserControl";
			this.DeliveryPartyDetailsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.DeliveryPartyDetailsSeparatorUserControl.TabIndex = 0;
			// 
			// SupplementaryDeclarationNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupplementaryDeclarationNameTextBox, "SupplementaryDeclarationName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupplementaryDeclarationName)));
			this.SupplementaryDeclarationNameTextBox.CaptionResourceString = null;
			this.SupplementaryDeclarationNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 16, true);
			this.SupplementaryDeclarationNameTextBox.Name = "SupplementaryDeclarationNameTextBox";
			this.SupplementaryDeclarationNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SupplementaryDeclarationNameTextBox.TabIndex = 0;
			// 
			// SupplementaryDeclarationDeliveryDateDateEdit
			// 
			this.SupplementaryDeclarationDeliveryDateDateEdit.AllowDrop = true;
			this.SupplementaryDeclarationDeliveryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.SupplementaryDeclarationDeliveryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SupplementaryDeclarationDeliveryDateDateEdit, "SupplementaryDeclarationDeliveryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupplementaryDeclarationDeliveryDate)));
			this.SupplementaryDeclarationDeliveryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 68, true);
			this.SupplementaryDeclarationDeliveryDateDateEdit.Name = "SupplementaryDeclarationDeliveryDateDateEdit";
			this.SupplementaryDeclarationDeliveryDateDateEdit.TabIndex = 1;
			// 
			// SupplementaryDeclarationRegNoIdNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupplementaryDeclarationRegNoIdNoTextBox, "SupplementaryDeclarationRegNoIdNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.ETrade.Business.AsycudaBill)(null)).SupplementaryDeclarationRegNoIdNo)));
			this.SupplementaryDeclarationRegNoIdNoTextBox.CaptionResourceString = null;
			this.SupplementaryDeclarationRegNoIdNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 42, true);
			this.SupplementaryDeclarationRegNoIdNoTextBox.Name = "SupplementaryDeclarationRegNoIdNoTextBox";
			this.SupplementaryDeclarationRegNoIdNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SupplementaryDeclarationRegNoIdNoTextBox.TabIndex = 2;
			// 
			// ETradeBillPartiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DeliveryPartyDetailsSeparatorUserControl);
			this.Controls.Add(this.SupplementaryDeclarationRegNoIdNoTextBox);
			this.Controls.Add(this.SupplementaryDeclarationDeliveryDateDateEdit);
			this.Controls.Add(this.SupplementaryDeclarationNameTextBox);
			this.Name = "ETradeBillPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 108, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupplementaryDeclarationDeliveryDateDateEdit.ResumeLayout(true);
			this.SupplementaryDeclarationDeliveryDateDateEdit.PerformLayout();
			this.DeliveryPartyDetailsSeparatorUserControl.ResumeLayout(true);
			this.DeliveryPartyDetailsSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox SupplementaryDeclarationNameTextBox;
		private ZArchitecture.GUI.ZDateEdit SupplementaryDeclarationDeliveryDateDateEdit;
		private ZArchitecture.ZTextBox SupplementaryDeclarationRegNoIdNoTextBox;
		SeparatorUserControl DeliveryPartyDetailsSeparatorUserControl;
	}
}
