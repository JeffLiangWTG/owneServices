namespace Enterprise.Customs.TR.GUI
{
	partial class ImportMessageUserControl
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
			this.EntryFeesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			// 
			// EntryLinesMessagesTabControl
			// 
			this.EntryLinesMessagesTabControl.Controls.Add(this.EntryFeesTabPage);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryFeesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.EntryLinesTabPage, 0);
			this.EntryLinesMessagesTabControl.Controls.SetChildIndex(this.MessageTabPage, 0);
			// BindingSource
			// EntryFeesTabPage
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			this.EntryFeesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EntryFeesTabPage.Name = "EntryFeesTabPage";
			this.EntryFeesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 462, true);
			this.EntryFeesTabPage.TabIndex = 2;
			this.EntryFeesTabPage.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("58a6a484-b509-4a03-bec9-a9f3d0aa855f", "Entry Fees");
		}

		#endregion


		private ZArchitecture.GUI.ZTabPage EntryFeesTabPage;
	}
}
