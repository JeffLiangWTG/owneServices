using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public sealed partial class VAT404DocumentSendingForm
	{
		new void InitializeComponent()
		{
			this.zPanel1 = new ZPanel();
			this.backButton = new ZButton();
			this.searchButton = new ZButton();
			this.sendButton = new ZButton();
			this.cancelButton = new ZButton();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.vaT404FilterUserControl1 = new VAT404FilterUserControl();
			this.vaT404DocumentUserControl1 = new VAT404DocumentUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.vaT404FilterUserControl1.SuspendLayout();
			this.vaT404DocumentUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 476, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 24, true);
			this.MainStatusBar.TabIndex = 2;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(VAT404DocumentInstruction);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.backButton);
			this.zPanel1.Controls.Add(this.searchButton);
			this.zPanel1.Controls.Add(this.sendButton);
			this.zPanel1.Controls.Add(this.cancelButton);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 452, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 24, true);
			this.zPanel1.TabIndex = 1;
			// 
			// BackButton
			// 
			this.backButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("36933833-5cdf-4a2a-860f-0630baecf9f2", "&Back");
			this.backButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.backButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(582, 0, true);
			this.backButton.Name = "BackButton";
			this.backButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 24, true);
			this.backButton.TabIndex = 0;
			this.backButton.UseVisualStyleBackColor = true;
			this.backButton.Click += new System.EventHandler(this.BackButton_Click);
			// 
			// SearchButton
			// 
			this.searchButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("f9634ac5-9c44-4b37-83c0-2fb725d65472", "&Search");
			this.searchButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.searchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(641, 0, true);
			this.searchButton.Name = "SearchButton";
			this.searchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 24, true);
			this.searchButton.TabIndex = 1;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.SearchButton_Click);
			// 
			// SendButton
			// 
			this.sendButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("9cd55e17-1d69-4c6c-a7d7-12f61303d3ad", "&Send");
			this.sendButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.sendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(699, 0, true);
			this.sendButton.Name = "SendButton";
			this.sendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 24, true);
			this.sendButton.TabIndex = 2;
			this.sendButton.UseVisualStyleBackColor = true;
			this.sendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("917925e0-ff08-44b0-83d4-ff9a63448c33", "&Cancel");
			this.cancelButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 0, true);
			this.cancelButton.Name = "CancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 24, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.vaT404FilterUserControl1);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.vaT404DocumentUserControl1);
			this.mainSplitContainer.Panel2Collapsed = true;
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 452, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.mainSplitContainer.TabIndex = 2;
			// 
			// vaT404FilterUserControl1
			// 
			this.vaT404FilterUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vaT404FilterUserControl1, ".");
			this.vaT404FilterUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vaT404FilterUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.vaT404FilterUserControl1.Name = "vaT404FilterUserControl1";
			this.vaT404FilterUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 452, true);
			this.vaT404FilterUserControl1.TabIndex = 0;
			// 
			// vaT404DocumentUserControl1
			// 
			this.vaT404DocumentUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vaT404DocumentUserControl1, ".");
			this.vaT404DocumentUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.vaT404DocumentUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.vaT404DocumentUserControl1.Name = "vaT404DocumentUserControl1";
			this.vaT404DocumentUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 77, true);
			this.vaT404DocumentUserControl1.TabIndex = 0;
			// 
			// VAT404DocumentSendingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 537, true);
			this.Controls.Add(this.mainSplitContainer);
			this.Controls.Add(this.zPanel1);
			this.DataSourceType = typeof(VAT404DocumentInstruction);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 537, true);
			this.Name = "VAT404DocumentSendingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.mainSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.vaT404FilterUserControl1.ResumeLayout(true);
			this.vaT404FilterUserControl1.PerformLayout();
			this.vaT404DocumentUserControl1.ResumeLayout(true);
			this.vaT404DocumentUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

	}
}
