namespace Enterprise.Customs.NZ.GUI
{
	partial class ConsolidatedDeclarationDetailsUserControl
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
			this.TSWCombinedStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.ConsolidatedDeclaration);
			// 
			// TSWCombinedStatusTextBox
			// 
			this.BindingSource.SetBindingMember(TSWCombinedStatusTextBox, "LeadDeclaration.JE_TSWCombinedStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.JobDeclaration)(null)).JE_TSWCombinedStatusDesc)));
			this.TSWCombinedStatusTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("87f885fa-c43d-49a0-a87a-0eb0ae8eaab4", "TSW", "TSW combined Agency status description");
			this.TSWCombinedStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TSWCombinedStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 0, true);
			this.TSWCombinedStatusTextBox.Name = "TSWCombinedStatusTextBox";
			this.TSWCombinedStatusTextBox.ReadOnly = true;
			this.TSWCombinedStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 15, true);
			this.TSWCombinedStatusTextBox.TabIndex = 2;
			// 
			// EntryStatusTextBox
			// 
			this.BindingSource.SetBindingMember(EntryStatusTextBox, "CustomsStatusDescription");
			EntryStatusTextBox.Name = "EntryStatusTextBox";
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ConsolidatedDeclaration)(null)).CustomsStatusDescription)));
			this.EntryStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 0, true);
			this.EntryStatusTextBox.Name = "EntryStatusTextBox";
			this.EntryStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 15, true);
			this.EntryStatusTextBox.TabIndex = 1;
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "LeadDeclaration.DeclarationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).DeclarationNumber)));
			this.EntryNumberTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("3a1b24d0-1731-4012-9c8b-dffa1c0a0e4a", "Entry Number");
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 15, true);
			this.EntryNumberTextBox.TabIndex = 0;
			// 
			// ConsolidatedDeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryNumberTextBox);
			this.Controls.Add(this.EntryStatusTextBox);
			this.Controls.Add(this.TSWCombinedStatusTextBox);
			this.Name = "ConsolidatedDeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(715, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZTextBox EntryStatusTextBox;
		public ZArchitecture.ZTextBox EntryNumberTextBox;
		public ZArchitecture.ZTextBox TSWCombinedStatusTextBox;
	}
}
