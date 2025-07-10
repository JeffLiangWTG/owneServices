namespace Enterprise.Customs.US.DataRegistry.GUI
{
	partial class EntryFilerControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.IsABICertifiedZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EntryFilerCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DataRegistry.Business.EntryFiler);
			// 
			// IsABICertifiedZCheckBox
			// 
			this.IsABICertifiedZCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsABICertifiedZCheckBox, "IsABICertified");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.DataRegistry.Business.EntryFiler)(null)).IsABICertified)));
			this.IsABICertifiedZCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntryFilerControl|151c3892-f431-4c65-b177-4b3b69ac464b", "Is ABI Certified");
			this.IsABICertifiedZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 41, true);
			this.IsABICertifiedZCheckBox.Name = "IsABICertifiedZCheckBox";
			this.IsABICertifiedZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 17, true);
			this.IsABICertifiedZCheckBox.TabIndex = 2;
			// 
			// EntryFilerCodeTextBox
			// 
			this.EntryFilerCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EntryFilerCodeTextBox, "EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.DataRegistry.Business.EntryFiler)(null)).EntryFilerCode)));
			this.EntryFilerCodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("EntryFilerControl|b4f57154-ce67-401a-9148-9c3fbdf531c7", "Entry Filer Code");
			this.EntryFilerCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 9, true);
			this.EntryFilerCodeTextBox.Name = "EntryFilerCodeTextBox";
			this.EntryFilerCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.EntryFilerCodeTextBox.TabIndex = 1;
			// 
			// EntryFilerControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EntryFilerCodeTextBox);
			this.Controls.Add(this.IsABICertifiedZCheckBox);
			this.Name = "EntryFilerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox IsABICertifiedZCheckBox;
		public Enterprise.ZArchitecture.ZTextBox EntryFilerCodeTextBox;
	}
}
