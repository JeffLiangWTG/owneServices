namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class DeclarationDetailsUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsHeader);
			// 
			// LrnTextBox
			// 
			this.BindingSource.SetBindingMember(this.LrnTextBox, "Header.LrnRegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureMovementHeader)(null)).Header.LrnRegistrationNumber)));
			this.LrnTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LrnTextBox.Name = "LrnTextBox";
			this.LrnTextBox.TabIndex = 0;
			this.LrnTextBox.TabStop = false;
			// 
			// DeclarationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LrnTextBox);
			this.Name = "DeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 259, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZTextBox LrnTextBox;
	}
}
