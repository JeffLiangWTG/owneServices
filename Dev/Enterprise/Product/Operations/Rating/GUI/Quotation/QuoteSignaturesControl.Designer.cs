using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class QuoteSignaturesControl
	{
		private ZGroupBox SignatoriesGroupBox;
		internal ZButton CurrentUserButton2;
		internal ZButton OverallRepButton2;
		internal ZButton CurrentUserButton1;
		internal ZButton OverallRepButton1;
		private ZCodeFindBox SecondSignatoryFindBox;
		private ZCodeFindBox FirstSignatoryFindBox;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SignatoriesGroupBox = new ZGroupBox();
			this.CurrentUserButton2 = new ZButton();
			this.OverallRepButton2 = new ZButton();
			this.CurrentUserButton1 = new ZButton();
			this.OverallRepButton1 = new ZButton();
			this.SecondSignatoryFindBox = new ZCodeFindBox();
			this.FirstSignatoryFindBox = new ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SignatoriesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Quote);
			// 
			// SignatoriesGroupBox
			// 
			this.SignatoriesGroupBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteSignaturesControl|ba6621e4-b4f1-4cda-bc6f-c35d1764a1b1", "Signatories");
			this.SignatoriesGroupBox.Controls.Add(this.CurrentUserButton2);
			this.SignatoriesGroupBox.Controls.Add(this.OverallRepButton2);
			this.SignatoriesGroupBox.Controls.Add(this.CurrentUserButton1);
			this.SignatoriesGroupBox.Controls.Add(this.OverallRepButton1);
			this.SignatoriesGroupBox.Controls.Add(this.SecondSignatoryFindBox);
			this.SignatoriesGroupBox.Controls.Add(this.FirstSignatoryFindBox);
			this.SignatoriesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SignatoriesGroupBox.Name = "SignatoriesGroupBox";
			this.SignatoriesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 72, true);
			this.SignatoriesGroupBox.TabIndex = 12;
			this.SignatoriesGroupBox.TabStop = false;
			// 
			// CurrentUserButton2
			// 
			this.CurrentUserButton2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteSignaturesControl|a9ed15d6-d239-45fb-b2a3-8e1e34d1d2e4", "Current", "Current User", "");
			this.CurrentUserButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 40, true);
			this.CurrentUserButton2.Name = "CurrentUserButton2";
			this.CurrentUserButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.CurrentUserButton2.TabIndex = 5;
			this.CurrentUserButton2.Click += new System.EventHandler(this.CurrentUserButton_Click);
			// 
			// OverallRepButton2
			// 
			this.OverallRepButton2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteSignaturesControl|6ec36023-c6d0-4d55-b659-0384977148f3", "Overall Rep");
			this.OverallRepButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 40, true);
			this.OverallRepButton2.Name = "OverallRepButton2";
			this.OverallRepButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.OverallRepButton2.TabIndex = 4;
			this.OverallRepButton2.Click += new System.EventHandler(this.OverallRepButton_Click);
			// 
			// CurrentUserButton1
			// 
			this.CurrentUserButton1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteSignaturesControl|5b46f827-6dda-4b74-8b6d-58dcd5507f2e", "Current", "Current User", "");
			this.CurrentUserButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 16, true);
			this.CurrentUserButton1.Name = "CurrentUserButton1";
			this.CurrentUserButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.CurrentUserButton1.TabIndex = 2;
			this.CurrentUserButton1.Click += new System.EventHandler(this.CurrentUserButton_Click);
			// 
			// OverallRepButton1
			// 
			this.OverallRepButton1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("QuoteSignaturesControl|21808802-c3c1-4dde-a9d6-4c9cbb117b7c", "Overall Rep");
			this.OverallRepButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 16, true);
			this.OverallRepButton1.Name = "OverallRepButton1";
			this.OverallRepButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.OverallRepButton1.TabIndex = 1;
			this.OverallRepButton1.Click += new System.EventHandler(this.OverallRepButton_Click);
			// 
			// SecondSignatoryFindBox
			// 
			this.BindingSource.SetBindingMember(this.SecondSignatoryFindBox, "TH_GS_NKSecondSignatory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).TH_GS_NKSecondSignatory);
			this.SecondSignatoryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 40, true);
			this.SecondSignatoryFindBox.Name = "SecondSignatoryFindBox";
			this.SecondSignatoryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 21, true);
			this.SecondSignatoryFindBox.TabIndex = 3;
			// 
			// FirstSignatoryFindBox
			// 
			this.BindingSource.SetBindingMember(this.FirstSignatoryFindBox, "TH_GS_NKFirstSignatory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Quote)(null)).TH_GS_NKFirstSignatory);
			this.FirstSignatoryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 16, true);
			this.FirstSignatoryFindBox.Name = "FirstSignatoryFindBox";
			this.FirstSignatoryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 21, true);
			this.FirstSignatoryFindBox.TabIndex = 0;
			// 
			// QuoteSignaturesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SignatoriesGroupBox);
			this.Name = "QuoteSignaturesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 72, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SignatoriesGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
