namespace Enterprise.MasterFiles.GUI
{
	partial class RefLanguageTextTranslationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.translationsOfCurrentValueGrid = new Enterprise.ZArchitecture.ZGrid();
			this.allValuesLabel = new Enterprise.ZArchitecture.ZLabel();
			this.allValuesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.translationsOfCurrentValueGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.allValuesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 400, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefLanguageText);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.translationsOfCurrentValueGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.allValuesLabel);
			this.splitContainer1.Panel2.Controls.Add(this.allValuesGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 394, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(201);
			this.splitContainer1.TabIndex = 1;
			// 
			// translationsOfCurrentValueGrid
			// 
			this.translationsOfCurrentValueGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.translationsOfCurrentValueGrid, "AllTranslationsOfCurrentValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllTranslationsOfCurrentValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLanguageTextPageEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).Language)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLanguageTextPageEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).LanguageDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLanguageTextPageEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).English)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLanguageTextPageEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllTranslationsOfCurrentValue)).SyncRoot)).Translation)));
			this.translationsOfCurrentValueGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6464770E-6749-4B30-9F87-42D90A0EC830", "Lang.", "Language Code", "");
			zTextBoxColumnStyleInfo1.ColumnName = "Language";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("C01FD357-D511-4044-8D74-93875695B446", "Language");
			zTextBoxColumnStyleInfo2.ColumnName = "LanguageDescription";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("A651489D-E040-4F70-A0D4-D00AD1192247", "English");
			zTextBoxColumnStyleInfo3.ColumnName = "English";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("740DFBDF-E361-49D1-921F-248DDE5ADD4D", "Translation");
			zTextBoxColumnStyleInfo4.ColumnName = "Translation";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.translationsOfCurrentValueGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.translationsOfCurrentValueGrid.CopySelectedRowsAllowed = true;
			this.translationsOfCurrentValueGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.translationsOfCurrentValueGrid.GridId = "37EB9D8A-DC47-42A2-9027-718A69D66519";
			this.translationsOfCurrentValueGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.translationsOfCurrentValueGrid.LayoutKey = "zGrid1";
			this.translationsOfCurrentValueGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.translationsOfCurrentValueGrid.Name = "translationsOfCurrentValueGrid";
			this.translationsOfCurrentValueGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 201, true);
			this.translationsOfCurrentValueGrid.TabIndex = 0;
			// 
			// allValuesLabel
			// 
			this.allValuesLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.allValuesLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.allValuesLabel.Name = "allValuesLabel";
			this.allValuesLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 17, true);
			this.allValuesLabel.TabIndex = 2;
			this.allValuesLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("736BCA38-7B6B-4CFF-B2F2-F255DE4D223C", "All English Translations");
			this.allValuesLabel.Text = "All English Translations";
			// 
			// allValuesGrid
			// 
			this.allValuesGrid.AllowNavigation = false;
			this.allValuesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.allValuesGrid, "AllValuesInCurrentLanguage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllValuesInCurrentLanguage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLanguageTextPageEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllValuesInCurrentLanguage)).SyncRoot)).English)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefLanguageTextPageEntry)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefLanguageTextPage)(null)).AllValuesInCurrentLanguage)).SyncRoot)).Translation)));
			this.allValuesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E6749A5A-FB79-4BCF-8F55-032137755EE7", "English");
			zTextBoxColumnStyleInfo5.ColumnName = "English";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5362DAD8-BA73-4658-AA54-54DA149221FC", "Translation");
			zTextBoxColumnStyleInfo6.ColumnName = "Translation";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.allValuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.allValuesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.allValuesGrid.CopySelectedRowsAllowed = true;
			this.allValuesGrid.GridId = "3EBF3282-704F-4906-9BFA-4BD1230D2CEE";
			this.allValuesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.allValuesGrid.LayoutKey = "allValuesGrid";
			this.allValuesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 20, true);
			this.allValuesGrid.Name = "allValuesGrid";
			this.allValuesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 169, true);
			this.allValuesGrid.TabIndex = 1;
			// 
			// zPostingButtonsUserControl
			// 
			this.zPostingButtonsUserControl.AllowDrop = true;
			this.zPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 400, true);
			this.zPostingButtonsUserControl.Name = "zPostingButtonsUserControl";
			this.zPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.zPostingButtonsUserControl.TabIndex = 3;
			// 
			// RefLanguageTextTranslationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 424, true);
			this.Controls.Add(this.zPostingButtonsUserControl);
			this.Controls.Add(this.splitContainer1);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefLanguageText);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 300, true);
			this.Name = "RefLanguageTextTranslationForm";
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPostingButtonsUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.translationsOfCurrentValueGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.allValuesGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal ZArchitecture.ZGrid translationsOfCurrentValueGrid;
		internal ZArchitecture.ZGrid allValuesGrid;
		internal Core.Forms.ZPostingButtonsUserControl zPostingButtonsUserControl;
		private ZArchitecture.ZLabel allValuesLabel;
	}
}
