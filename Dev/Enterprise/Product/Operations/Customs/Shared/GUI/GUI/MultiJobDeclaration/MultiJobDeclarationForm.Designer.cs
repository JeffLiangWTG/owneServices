using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class MultiJobDeclarationForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		ZLabel TypeLabel;
		ZDropEdit messageTypeDropEdit;
		ZDropEdit transportModeDropEdit;
		ZButton postButton;
		ZButton cancelButton;
		Enterprise.ZArchitecture.ZGrid grid;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.grid = new Enterprise.ZArchitecture.ZGrid();
			this.TypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.messageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.transportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.postButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 429, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 24, true);
			this.MainStatusBar.TabIndex = 8;
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.grid.BindTo = "Collection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.MultiJobDeclarationHeader)(null)).Collection)));
			this.grid.CaptionVisible = false;
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "grid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 51, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 343, true);
			this.grid.TabIndex = 5;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			// 
			// TypeLabel
			// 
			this.TypeLabel.AutoSize = true;
			this.TypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 16, true);
			this.TypeLabel.Name = "TypeLabel";
			this.TypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.TypeLabel.TabIndex = 0;
			this.TypeLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D7A79FE9-A216-428E-B9F6-5AB2F1AD2EA5", "Type:");
			// 
			// messageTypeDropEdit
			// 
			this.messageTypeDropEdit.BindTo = "Declaration+JE_MessageType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.MultiJobDeclarationHeader)(null)).Declaration.JE_MessageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.MultiJobDeclarationHeader)(null)).Declaration.JE_MessageType)));
			this.messageTypeDropEdit.BindToList = "Declaration+Lookups+MessageTypeList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.MultiJobDeclarationHeader)(null)).Declaration.Lookups.MessageTypeList)));
			this.messageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(59, 13, true);
			this.messageTypeDropEdit.Name = "messageTypeDropEdit";
			this.messageTypeDropEdit.PreBoundMaxLength = 3;
			this.messageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.messageTypeDropEdit.TabIndex = 1;
			// 
			// transportModeDropEdit
			// 
			this.transportModeDropEdit.BindTo = "Declaration+JE_TransportMode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.MultiJobDeclarationHeader)(null)).Declaration.JE_TransportModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.MultiJobDeclarationHeader)(null)).Declaration.JE_TransportMode)));
			this.transportModeDropEdit.BindToList = "Declaration+Lookups+TransportTypeList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.MultiJobDeclarationHeader)(null)).Declaration.Lookups.TransportTypeList)));
			this.transportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 13, true);
			this.transportModeDropEdit.Name = "transportModeDropEdit";
			this.transportModeDropEdit.PreBoundMaxLength = 3;
			this.transportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 20, true);
			this.transportModeDropEdit.TabIndex = 3;
			// 
			// postButton
			// 
			this.postButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.postButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(543, 400, true);
			this.postButton.Name = "postButton";
			this.postButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.postButton.TabIndex = 6;
			this.postButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("92D7BAD3-F811-45B7-8C11-5439739763DE", "Save");
			this.postButton.UseVisualStyleBackColor = true;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(629, 400, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3ADAD3CE-626C-43D0-9201-C23D50441CE2", "Cancel");
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// MultiJobDeclarationForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 453, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.postButton);
			this.Controls.Add(this.transportModeDropEdit);
			this.Controls.Add(this.grid);
			this.Controls.Add(this.messageTypeDropEdit);
			this.Controls.Add(this.TypeLabel);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.MultiJobDeclarationHeader";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 487, true);
			this.Name = "MultiJobDeclarationForm";
			this.Controls.SetChildIndex(this.TypeLabel, 0);
			this.Controls.SetChildIndex(this.messageTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.grid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.transportModeDropEdit, 0);
			this.Controls.SetChildIndex(this.postButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
