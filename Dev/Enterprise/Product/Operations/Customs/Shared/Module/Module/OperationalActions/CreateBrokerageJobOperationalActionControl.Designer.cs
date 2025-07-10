namespace Enterprise.Customs.Module.OperationalActions
{
	partial class CreateBrokerageJobOperationalActionControl
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
			this.CheckedListBoxQuestions = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Module.OperationalActions.CreateBrokerageJobOperationalActionMethodApplicator);
			// 
			// CheckedListBoxQuestions
			// 
			this.CheckedListBoxQuestions.BindingItems = null;
			this.BindingSource.SetBindingMember(this.CheckedListBoxQuestions, "QuestionList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((Enterprise.Customs.Module.OperationalActions.CreateBrokerageJobOperationalActionMethodApplicator)(null)).QuestionList)));
			this.CheckedListBoxQuestions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CheckedListBoxQuestions.FormattingEnabled = true;
			this.CheckedListBoxQuestions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CheckedListBoxQuestions.Name = "CheckedListBoxQuestions";
			this.CheckedListBoxQuestions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 200, true);
			this.CheckedListBoxQuestions.TabIndex = 0;
			// 
			// CreateBrokerageJobOperationalActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CheckedListBoxQuestions);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 200, true);
			this.Name = "CreateBrokerageJobOperationalActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckedListBox CheckedListBoxQuestions;
	}
}
