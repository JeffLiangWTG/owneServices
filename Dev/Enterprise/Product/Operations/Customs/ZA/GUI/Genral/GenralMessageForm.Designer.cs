using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	partial class GenralMessageForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.LocalProfileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.MessageSubTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.MessageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.zWebBrowser1 = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
            this.DateReceivedTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.LocalProfileTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.zTabControl1 = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.zTabPage1 = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.zTabPage2 = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.SaveButtonUserControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zGroupBox1.SuspendLayout();
            this.zTabControl1.SuspendLayout();
            this.zTabPage1.SuspendLayout();
            this.zTabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 374, true);
            this.MainTabControl.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.MainTabControl.TabIndex = 0;
            // 
            // MainTabPage
            // 
            this.MainTabPage.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("443CA1C9-3CA3-4041-BEA0-3CEABF46F4D9", "Original Message");
            this.MainTabPage.Controls.Add(this.zTabControl1);
            this.MainTabPage.Controls.Add(this.zGroupBox1);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 347, true);
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 347, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 347, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 374, true);
            // 
            // SaveButtonUserControl
            // 
            this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 6, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.GENRALMessage);
            // 
            // LocalProfileNameTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalProfileNameTextBox, "LocalProfileName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Nullable<CargoWise.Types.ZString>)(((Enterprise.Customs.ZA.Business.GENRALMessage)(null)).LocalProfileName)));
            this.LocalProfileNameTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("6279446a-9169-4125-8968-8e6adfae40ba", "Local Profile Name");
            this.LocalProfileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 39, true);
            this.LocalProfileNameTextBox.Name = "LocalProfileNameTextBox";
            this.LocalProfileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
            this.LocalProfileNameTextBox.TabIndex = 3;
            // 
            // MessageSubTypeTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageSubTypeTextBox, "EM_MessageSubType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.GENRALMessage)(null)).EM_MessageSubType)));
            this.MessageSubTypeTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("45be16f7-b8a0-4018-8862-cdecdc6f6693", "Sub-Type");
            this.MessageSubTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 39, true);
            this.MessageSubTypeTextBox.Name = "MessageSubTypeTextBox";
            this.MessageSubTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
            this.MessageSubTypeTextBox.TabIndex = 5;
            // 
            // MessageTypeTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageTypeTextBox, "EM_MessageType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.GENRALMessage)(null)).EM_MessageType)));
            this.MessageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 39, true);
            this.MessageTypeTextBox.Name = "MessageTypeTextBox";
            this.MessageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
            this.MessageTypeTextBox.TabIndex = 4;
            // 
            // MessageTextTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageTextTextBox, "EM_MessageText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.GENRALMessage)(null)).EM_MessageText)));
            this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.MessageTextTextBox.Multiline = true;
            this.MessageTextTextBox.Name = "MessageTextTextBox";
            this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
            this.MessageTextTextBox.TabIndex = 6;
            // 
            // zWebBrowser1
            // 
            this.zWebBrowser1.AllowWebBrowserDrop = false;
            this.zWebBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zWebBrowser1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.zWebBrowser1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.zWebBrowser1.Name = "zWebBrowser1";
            this.zWebBrowser1.ScriptErrorsSuppressed = true;
            this.zWebBrowser1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.zWebBrowser1.TabIndex = 7;
            this.zWebBrowser1.Url = new System.Uri("about:blank", System.UriKind.Absolute);
            // 
            // DateReceivedTextBox
            // 
            this.BindingSource.SetBindingMember(this.DateReceivedTextBox, "EM_MessageDateTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ZA.Business.GENRALMessage)(null)).EM_MessageDateTime)));
            this.DateReceivedTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("3286001e-aebf-41c8-bee9-38b23ccbdedf", "Date Received");
            this.DateReceivedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(381, 13, true);
            this.DateReceivedTextBox.Name = "DateReceivedTextBox";
            this.DateReceivedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
            this.DateReceivedTextBox.TabIndex = 1;
            // 
            // StatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.StatusTextBox, "EM_Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.GENRALMessage)(null)).EM_Status)));
            this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(567, 13, true);
            this.StatusTextBox.Name = "StatusTextBox";
            this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
            this.StatusTextBox.TabIndex = 2;
            // 
            // LocalProfileTextBox
            // 
            this.BindingSource.SetBindingMember(this.LocalProfileTextBox, "EM_MessageOwner");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.Business.GENRALMessage)(null)).EM_MessageOwner)));
            this.LocalProfileTextBox.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("a7e4da99-9b77-4df1-8b96-ddb7a3eadb34", "Local Profile");
            this.LocalProfileTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 13, true);
            this.LocalProfileTextBox.Name = "LocalProfileTextBox";
            this.LocalProfileTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 20, true);
            this.LocalProfileTextBox.TabIndex = 0;
            // 
            // zGroupBox1
            // 
            this.zGroupBox1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2371ecaf-11fd-439c-915f-c05cd7b1b59d", "Details");
            this.zGroupBox1.Controls.Add(this.LocalProfileTextBox);
            this.zGroupBox1.Controls.Add(this.LocalProfileNameTextBox);
            this.zGroupBox1.Controls.Add(this.StatusTextBox);
            this.zGroupBox1.Controls.Add(this.MessageSubTypeTextBox);
            this.zGroupBox1.Controls.Add(this.DateReceivedTextBox);
            this.zGroupBox1.Controls.Add(this.MessageTypeTextBox);
            this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
            this.zGroupBox1.Name = "zGroupBox1";
            this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 72, true);
            this.zGroupBox1.TabIndex = 0;
            this.zGroupBox1.TabStop = false;
            // 
            // zTabControl1
            // 
            this.zTabControl1.Controls.Add(this.zTabPage1);
            this.zTabControl1.Controls.Add(this.zTabPage2);
            this.zTabControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 83, true);
            this.zTabControl1.Name = "zTabControl1";
            this.zTabControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
            this.zTabControl1.TabIndex = 9;
            // 
            // zTabPage1
            //
			this.zTabPage1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("E7D17827-DBC3-4C5A-A965-580C4F7643E7", "Interpretation");
			this.zTabPage1.Controls.Add(this.zWebBrowser1);
            this.zTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zTabPage1.Name = "zTabPage1";
            this.zTabPage1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
            this.zTabPage1.TabIndex = 0;
            this.zTabPage1.UseVisualStyleBackColor = true;
            // 
            // zTabPage2
            //
			this.zTabPage2.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("2FB8AA91-7460-416B-A93F-71BA7346B36F", "Text");
			this.zTabPage2.Controls.Add(this.MessageTextTextBox);
            this.zTabPage2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zTabPage2.Name = "zTabPage2";
            this.zTabPage2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPage2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
            this.zTabPage2.TabIndex = 1;
            this.zTabPage2.UseVisualStyleBackColor = true;
            // 
            // GenralMessageForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("GenralMessageForm|948e8d12-7563-40a8-a0c3-6c7990a9824c", "GENRAL Message");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 430, true);
            this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.GENRALMessage);
            this.IsPostOnly = true;
            this.Name = "GenralMessageForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Text = "GenralMessageForm";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.SaveButtonUserControl.ResumeLayout(true);
            this.SaveButtonUserControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zGroupBox1.ResumeLayout(false);
            this.zGroupBox1.PerformLayout();
            this.zTabControl1.ResumeLayout(false);
            this.zTabControl1.PerformLayout();
            this.zTabPage1.ResumeLayout(false);
            this.zTabPage1.PerformLayout();
            this.zTabPage2.ResumeLayout(false);
            this.zTabPage2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox MessageTypeTextBox;
		private ZArchitecture.ZTextBox MessageSubTypeTextBox;
		private ZArchitecture.ZTextBox LocalProfileNameTextBox;
		private ZWebBrowser zWebBrowser1;
		private ZArchitecture.ZTextBox MessageTextTextBox;
		private ZArchitecture.ZTextBox StatusTextBox;
		private ZArchitecture.ZTextBox DateReceivedTextBox;
		private ZArchitecture.ZTextBox LocalProfileTextBox;
		private ZTabControl zTabControl1;
		private ZTabPage zTabPage1;
		private ZTabPage zTabPage2;
		private ZGroupBox zGroupBox1;
	}
}
