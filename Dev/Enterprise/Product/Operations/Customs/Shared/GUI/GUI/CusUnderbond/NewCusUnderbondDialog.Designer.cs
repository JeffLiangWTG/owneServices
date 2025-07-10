namespace Enterprise.Customs.GUI
{
	partial class NewCusUnderbondDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private new Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit UnderbondForDropEdit;
		System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.UnderbondForDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 63, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(473);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.MinWidth = 0;
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0);
			// 
			// UnderbondForDropEdit
			// 
			this.UnderbondForDropEdit.BindTo = "UnderbondParentStringRepresentation";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.NewCusUnderbondNonPersistent)(null)).UnderbondParentStringRepresentationInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.NewCusUnderbondNonPersistent)(null)).UnderbondParentStringRepresentation)));
			this.UnderbondForDropEdit.BindToList = "UnderbondForList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.NewCusUnderbondNonPersistent)(null)).UnderbondForList)));
			this.UnderbondForDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnderbondForDropEdit, false);
			this.UnderbondForDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 8, true);
			this.UnderbondForDropEdit.Name = "UnderbondForDropEdit";
			this.UnderbondForDropEdit.PreBoundMaxLength = 70;
			this.UnderbondForDropEdit.ShowDescriptionBox = false;
			this.UnderbondForDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.UnderbondForDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 20, true);
			this.UnderbondForDropEdit.TabIndex = 0;
			// 
			// CancelButton
			// 
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 34, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3FAB6C6C-6914-4FD5-B85B-9C4B44B13B13", "Cancel");
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 34, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("7B482648-5479-4561-AA87-5A8F1B314A59", "OK");
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// NewCusUnderbondDialog
			// 
			this.AcceptButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(473, 87, true);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.UnderbondForDropEdit);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.NewCusUnderbondNonPersistent";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "NewCusUnderbondDialog";
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("A9B46E01-D343-47A1-9C2D-290B0B73439A", "Create Underbond Movement For");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.UnderbondForDropEdit, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
