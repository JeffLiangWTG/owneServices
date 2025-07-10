
namespace Enterprise.Customs.US.GUI
{
	partial class CensusWarningQueryForm
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
		private new void InitializeComponent()
		{
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GiveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PortCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DateFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Label = new Enterprise.ZArchitecture.ZLabel();
			this.EntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FilerTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.summaryLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DateFromDateEdit.SuspendLayout();
			this.DateToDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 179, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 24, true);
			this.MainStatusBar.TabIndex = 9;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.CensusWarningQuery);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 144, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 7;
			this.SendButton.Text = "&Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// GiveUpButton
			// 
			this.GiveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GiveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 144, true);
			this.GiveUpButton.Name = "GiveUpButton";
			this.GiveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GiveUpButton.TabIndex = 8;
			this.GiveUpButton.Text = "&Cancel";
			this.GiveUpButton.UseVisualStyleBackColor = true;
			this.GiveUpButton.Click += new System.EventHandler(this.GiveUpButton_Click);
			// 
			// PortCodeTextBox
			// 
			this.PortCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PortCodeTextBox, "DistrictPortCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CensusWarningQuery)(null)).DistrictPortCode)));
			this.PortCodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CensusWarningQueryForm|b4834971-03ab-4c35-af58-1ef47a724dd0", "Entry Port");
			this.PortCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 108, true);
			this.PortCodeTextBox.Name = "PortCodeTextBox";
			this.PortCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 18, true);
			this.PortCodeTextBox.TabIndex = 6;
			// 
			// DateFromDateEdit
			// 
			this.DateFromDateEdit.AllowDrop = true;
			this.DateFromDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DateFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateFromDateEdit, "DateFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CensusWarningQuery)(null)).DateFrom)));
			this.DateFromDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CensusWarningQueryForm|4469efe3-214f-44be-9eee-1895e94878cc", "Entry Accepted From");
			this.DateFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 78, true);
			this.DateFromDateEdit.Name = "DateFromDateEdit";
			this.DateFromDateEdit.TabIndex = 4;
			// 
			// DateToDateEdit
			// 
			this.DateToDateEdit.AllowDrop = true;
			this.DateToDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DateToDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateToDateEdit, "DateTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.CensusWarningQuery)(null)).DateTo)));
			this.DateToDateEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CensusWarningQueryForm|dddab81f-b9e9-400b-b3d7-8093dff4447c", "To");
			this.DateToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 78, true);
			this.DateToDateEdit.Name = "DateToDateEdit";
			this.DateToDateEdit.TabIndex = 5;
			// 
			// Label
			// 
			this.Label.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Label, false);
			this.Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 50, true);
			this.Label.Name = "Label";
			this.Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
			this.Label.TabIndex = 2;
			this.Label.Text = "-";
			// 
			// EntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryNumberTextBox, "EntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CensusWarningQuery)(null)).EntryNumber)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EntryNumberTextBox, false);
			this.EntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 47, true);
			this.EntryNumberTextBox.Name = "EntryNumberTextBox";
			this.EntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 18, true);
			this.EntryNumberTextBox.TabIndex = 3;
			// 
			// FilerTextBox
			// 
			this.BindingSource.SetBindingMember(this.FilerTextBox, "EntryFilerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.CensusWarningQuery)(null)).EntryFilerCode)));
			this.FilerTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CensusWarningQueryForm|f66f8de2-4c4e-4e5d-a55d-de4e45fa5059", "Entry Number");
			this.FilerTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 47, true);
			this.FilerTextBox.Name = "FilerTextBox";
			this.FilerTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 18, true);
			this.FilerTextBox.TabIndex = 1;
			// 
			// summaryLabel
			// 
			this.summaryLabel.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("CensusWarningQueryForm|d503cba5-104b-4ddc-a735-baa736ea620f", "Queries may be made upon an Entry Number, specific date period or upon a specific Entry Port.");
			this.summaryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 8, true);
			this.summaryLabel.Name = "summaryLabel";
			this.summaryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 27, true);
			this.summaryLabel.TabIndex = 0;
			// 
			// CensusWarningQueryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 203, true);
			this.Controls.Add(this.summaryLabel);
			this.Controls.Add(this.Label);
			this.Controls.Add(this.EntryNumberTextBox);
			this.Controls.Add(this.FilerTextBox);
			this.Controls.Add(this.DateToDateEdit);
			this.Controls.Add(this.DateFromDateEdit);
			this.Controls.Add(this.GiveUpButton);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.PortCodeTextBox);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.CensusWarningQuery);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.CensusWarningQuery";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CensusWarningQueryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Census Warning Query";
			this.Controls.SetChildIndex(this.PortCodeTextBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.GiveUpButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DateFromDateEdit, 0);
			this.Controls.SetChildIndex(this.DateToDateEdit, 0);
			this.Controls.SetChildIndex(this.FilerTextBox, 0);
			this.Controls.SetChildIndex(this.EntryNumberTextBox, 0);
			this.Controls.SetChildIndex(this.Label, 0);
			this.Controls.SetChildIndex(this.summaryLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DateFromDateEdit.ResumeLayout(true);
			this.DateFromDateEdit.PerformLayout();
			this.DateToDateEdit.ResumeLayout(true);
			this.DateToDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
		internal Enterprise.ZArchitecture.GUI.ZButton GiveUpButton;
		private Enterprise.ZArchitecture.ZTextBox PortCodeTextBox;
		private ZArchitecture.GUI.ZDateEdit DateFromDateEdit;
		private ZArchitecture.GUI.ZDateEdit DateToDateEdit;
		private ZArchitecture.ZLabel Label;
		private ZArchitecture.ZTextBox EntryNumberTextBox;
		private ZArchitecture.ZTextBox FilerTextBox;
		private ZArchitecture.ZLabel summaryLabel;
	}
}
