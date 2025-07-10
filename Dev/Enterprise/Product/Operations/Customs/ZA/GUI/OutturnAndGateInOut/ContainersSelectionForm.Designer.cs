namespace Enterprise.Customs.ZA.GUI
{
	partial class ContainersSelectionForm
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
		new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.BottonPanel = new CargoWise.Windows.UI.KPanel();
            this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
            this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BottonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
            this.ContainersGrid.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 457, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("94445286-e7d4-4996-8563-5693c188bac5", "Cancel");
            this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(682, 3, true);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.ToolTipCaption = null;
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("bee2dd44-3543-40f7-a072-fe9a6070a313", "OK");
            this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 3, true);
            this.okButton.Name = "okButton";
            this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.okButton.TabIndex = 1;
            this.okButton.ToolTipCaption = null;
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // BottonPanel
            // 
            this.BottonPanel.Controls.Add(this.cancelButton);
            this.BottonPanel.Controls.Add(this.okButton);
            this.BottonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 426, true);
            this.BottonPanel.Name = "BottonPanel";
            this.BottonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 31, true);
            this.BottonPanel.TabIndex = 3;
            // 
            // ContainersGrid
            // 
            this.ContainersGrid.AllowNavigation = false;
            this.ContainersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ContainersGrid, "SourceContainers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_ContainerNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_EmptyFullIndicator)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_RC_ContainerType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_Seal1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_Seal2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_Seal3)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_SealingPartyName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_SealingPartyType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_NumberOfPackages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_CommodityCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_GoodsWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_GoodsWeightUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ManifestBase.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO)(null)).SourceContainers)).SyncRoot)).ACN_StowageLocation)));
            this.ContainersGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo1.ColumnName = "ACN_ContainerNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "ACN_EmptyFullIndicator";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zGuidFindBoxColumnStyleInfo1.ColumnName = "ACN_RC_ContainerType";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo2.ColumnName = "ACN_Seal1";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo3.ColumnName = "ACN_Seal2";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo4.ColumnName = "ACN_Seal3";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo5.ColumnName = "ACN_SealingPartyName";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "ACN_SealingPartyType";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("c61dddcf-43b2-4a28-b37e-b9d2377bb756", "Packages");
            zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zCalcEditColumnStyleInfo1.ColumnName = "ACN_NumberOfPackages";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("6642dfef-7fd7-48bf-b1cc-4cab1bff4895", "Commodity Code");
            zTextBoxColumnStyleInfo6.ColumnName = "ACN_CommodityCode";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "ACN_GoodsWeight";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo7.ColumnName = "ACN_GoodsWeightUQ";
            zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("639b16f7-74d6-49ca-8a40-a8f9b070f396", "Stow Location");
            zTextBoxColumnStyleInfo8.ColumnName = "ACN_StowageLocation";
            zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.ContainersGrid.GridId = "994d5c54-a04d-41e9-b561-7b63ff379965";
            this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ContainersGrid.IsWholeRowSelectedOnClick = true;
            this.ContainersGrid.LayoutKey = "ContainersGrid";
            this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 44, true);
            this.ContainersGrid.Name = "ContainersGrid";
            this.ContainersGrid.ReadOnly = true;
            this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 379, true);
            this.ContainersGrid.TabIndex = 0;
            // 
            // TopPanel
            // 
            this.TopPanel.Controls.Add(this.zLabel1);
            this.TopPanel.Controls.Add(this.ContainersGrid);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 426, true);
            this.TopPanel.TabIndex = 5;
            // 
            // zLabel1
            // 
            this.zLabel1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("508e80c5-d3b4-47b5-8bab-a94b143f96cd", "Please select one container to copy:");
            this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.zLabel1.IsFontBold = true;
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 15, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 24, true);
            this.zLabel1.TabIndex = 1;
            this.zLabel1.UseMnemonic = false;
            // 
            // ContainersSelectionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 481, true);
            this.Controls.Add(this.TopPanel);
            this.Controls.Add(this.BottonPanel);
            this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.BaseAsycudaManifestHeaderCopyBO);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 500, true);
            this.Name = "ContainersSelectionForm";
            this.Text = "ContainersSelectionForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.BottonPanel, 0);
            this.Controls.SetChildIndex(this.TopPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BottonPanel.ResumeLayout(false);
            this.BottonPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
            this.ContainersGrid.ResumeLayout(false);
            this.ContainersGrid.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZButton cancelButton;
		internal ZArchitecture.GUI.ZButton okButton;
		private CargoWise.Windows.UI.KPanel BottonPanel;
		internal ZArchitecture.ZGrid ContainersGrid;
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZArchitecture.ZLabel zLabel1;
	}
}
