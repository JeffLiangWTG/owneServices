using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Rating.GUI.RateChooser
{
	partial class RateChooserCardTemplate
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
            this.SelectionPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.cbIsSelected = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.rateChooserCard1 = new Enterprise.Rating.GUI.RateChooser.UIControls.RateChooserCard();
            this.pnlApply = new System.Windows.Forms.FlowLayoutPanel();
            this.lblSelectRelatedRatesText = new Enterprise.ZArchitecture.ZLabel();
            this.btnApply = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SelectionPanel.SuspendLayout();
            this.rateChooserCard1.SuspendLayout();
            this.pnlApply.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Rating.GUI.ChooserRateRow);
            this.BindingSource.SupportMultipleTwoWayBoundPropertiesOnOneControl = true;
            // 
            // SelectionPanel
            // 
            this.SelectionPanel.AutoSize = true;
            this.SelectionPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.SelectionPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelectionPanel.Controls.Add(this.cbIsSelected);
            this.SelectionPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.SelectionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SelectionPanel.Name = "SelectionPanel";
            this.SelectionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 978, true);
            this.SelectionPanel.TabIndex = 0;
            // 
            // cbIsSelected
            // 
            this.cbIsSelected.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbIsSelected.AutoSize = true;
            this.BindingSource.SetBindingMember(this.cbIsSelected, "IsSelected");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Rating.GUI.ChooserRateRow)(null)).IsSelected)));
            this.cbIsSelected.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 8, true);
            this.cbIsSelected.Name = "cbIsSelected";
            this.cbIsSelected.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.cbIsSelected.TabIndex = 0;
            this.cbIsSelected.UseVisualStyleBackColor = true;
            // 
            // MainPanel
            // 
            this.MainPanel.AutoSize = true;
            this.MainPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 0, true);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1739, 0, true);
            this.MainPanel.TabIndex = 1;
            // 
            // rateChooserCard1
            // 
            this.rateChooserCard1.AllowDrop = true;
            this.rateChooserCard1.AutoSize = true;
            this.rateChooserCard1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BindingSource.SetBindingMember(this.rateChooserCard1, ".");
            this.rateChooserCard1.Dock = System.Windows.Forms.DockStyle.Top;
            this.rateChooserCard1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 0, true);
            this.rateChooserCard1.Name = "rateChooserCard1";
            this.rateChooserCard1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1739, 131, true);
            this.rateChooserCard1.TabIndex = 2;
            // 
            // pnlApply
            // 
            this.pnlApply.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlApply.Controls.Add(this.lblSelectRelatedRatesText);
            this.pnlApply.Controls.Add(this.btnApply);
            this.pnlApply.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlApply.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 953);
            this.pnlApply.Name = "pnlApply";
            this.pnlApply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1739, 25, true);
            this.pnlApply.TabIndex = 3;
            // 
            // lblSelectRelatedRatesText
            // 
            this.lblSelectRelatedRatesText.AutoSize = true;
            this.lblSelectRelatedRatesText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.lblSelectRelatedRatesText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
            this.lblSelectRelatedRatesText.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 1, 0, true);
            this.lblSelectRelatedRatesText.Name = "lblSelectRelatedRatesText";
            this.lblSelectRelatedRatesText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 13, true);
            this.lblSelectRelatedRatesText.TabIndex = 1;
            this.lblSelectRelatedRatesText.Text = "SelectRelatedRatesText";
            this.lblSelectRelatedRatesText.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            // 
            // btnApply
            // 
            this.btnApply.IsCaptionOverridden = true;
            this.btnApply.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 1, true);
            this.btnApply.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, 1, 1, 311, true);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = Res.GetString("C1C22945-EF81-4E47-8196-8756EE4C0E71", "Apply");
            this.btnApply.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnApply.ToolTipCaption = null;
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // RateChooserCardTemplate
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.pnlApply);
            this.Controls.Add(this.rateChooserCard1);
            this.Controls.Add(this.MainPanel);
            this.Controls.Add(this.SelectionPanel);
            this.Name = "RateChooserCardTemplate";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1772, 978, true);
            this.MouseLeave += new System.EventHandler(this.RateChooserCardTemplate_MouseLeave);
            this.MouseHover += new System.EventHandler(this.RateChooserCardTemplate_MouseHover);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SelectionPanel.ResumeLayout(false);
            this.SelectionPanel.PerformLayout();
            this.rateChooserCard1.ResumeLayout(true);
            this.rateChooserCard1.PerformLayout();
            this.pnlApply.ResumeLayout(false);
            this.pnlApply.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel SelectionPanel;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZCheckBox cbIsSelected;
		private UIControls.RateChooserCard rateChooserCard1;
		private FlowLayoutPanel pnlApply;
		private ZArchitecture.ZLabel lblSelectRelatedRatesText;
		private ZArchitecture.GUI.ZButton btnApply;
	}
}
