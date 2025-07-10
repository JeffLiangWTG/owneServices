using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI
{
	partial class EntryInstructionDetailsUserControl
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
			this.SealsTabPage = new ZTabPage();
			this.SealsUserControl = new ZDynamicControlCreationUserControl();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SealsTabPage.SuspendLayout();
			this.EntryInstructionTabControl.SuspendLayout();
			this.SuspendLayout();

			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			//
			// EntryInstructionTabControl
			//
			this.EntryInstructionTabControl.Controls.Add(this.SealsTabPage);
			// 
			// SealsTabPage
			// 
			this.SealsTabPage.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("BD91AD9E-DDBC-4D6D-8135-0377C4ABA836", "Seals");
			this.SealsTabPage.Controls.Add(this.SealsUserControl);
			this.SealsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.SealsTabPage.Name = "SealsTabPage";
			this.SealsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.SealsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			this.SealsTabPage.TabIndex = 4;
			this.SealsTabPage.UseVisualStyleBackColor = true;
			this.SealsTabPage.TabVisible = false;
			// 
			// AuthorisationsUserControl
			// 
			this.SealsUserControl.AllowDrop = true;
			this.SealsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SealsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SealsUserControl.Name = "SealsUserControl";
			this.SealsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1323, 399, true);
			this.SealsUserControl.TabIndex = 0;

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryInstructionTabControl.ResumeLayout(false);
			this.EntryInstructionTabControl.PerformLayout();
			this.SealsTabPage.ResumeLayout(false);
			this.SealsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZTabPage SealsTabPage;
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl SealsUserControl;
	}
}
