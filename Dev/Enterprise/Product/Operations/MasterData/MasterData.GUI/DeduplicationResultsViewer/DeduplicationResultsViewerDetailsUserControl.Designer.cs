namespace Enterprise.MasterData.GUI
{
	partial class DeduplicationResultsViewerDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PotentialDuplicatesUserControl = new Enterprise.MasterData.GUI.PotentialDuplicatesUserControl();
			this.MasterInformationControl = new Enterprise.MasterData.GUI.MasterCandidateInformationControl();
			this.CandidateInformationControl = new Enterprise.MasterData.GUI.MasterCandidateInformationControl();
			this.MainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MainContentTableLayoutContainer = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderContentTableLayoutContainer = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.HeaderPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.MasterTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.MasterTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MasterButtonTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OpenMasterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExclusionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeactivateMasterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CandidateTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.CandidateTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CandidateButtonTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.OpenTargetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.IgnoreSplitButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.IgnoreButtonToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.IgnoreSplitButton = new System.Windows.Forms.ToolStripSplitButton();
			this.ForMeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ForEveryoneMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RemoveIgnoreMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MergeButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.MergeButton = new System.Windows.Forms.ToolStripSplitButton();
			this.RetainMaster = new System.Windows.Forms.ToolStripMenuItem();
			this.RetainCandidate = new System.Windows.Forms.ToolStripMenuItem();
			this.OpenButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OpenButtonToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.OpenButtonSplitButton = new System.Windows.Forms.ToolStripSplitButton();
			this.PersonMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.StaffMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ContactMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ApplicantMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DeactivateCandidateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LinkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ContentPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.ButtonTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PotentialDuplicatesUserControl.SuspendLayout();
			this.MasterInformationControl.SuspendLayout();
			this.CandidateInformationControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			this.HeaderContentTableLayoutContainer.SuspendLayout();
			this.HeaderPanel.SuspendLayout();
			this.MasterTableLayoutPanel.SuspendLayout();
			this.MasterButtonTableLayoutPanel.SuspendLayout();
			this.CandidateTableLayoutPanel.SuspendLayout();
			this.CandidateButtonTableLayoutPanel.SuspendLayout();
			this.IgnoreSplitButtonPanel.SuspendLayout();
			this.IgnoreButtonToolStrip.SuspendLayout();
			this.MergeButtonPanel.SuspendLayout();
			this.ToolStrip.SuspendLayout();
			this.OpenButtonPanel.SuspendLayout();
			this.OpenButtonToolStrip.SuspendLayout();
			this.ContentPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterData.Business.IDeduplicationResultDetail);
			// 
			// PotentialDuplicatesUserControl
			// 
			this.PotentialDuplicatesUserControl.AllowDrop = true;
			this.PotentialDuplicatesUserControl.AutoScroll = true;
			this.PotentialDuplicatesUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PotentialDuplicatesUserControl, ".");
			this.PotentialDuplicatesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PotentialDuplicatesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PotentialDuplicatesUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.PotentialDuplicatesUserControl.Name = "PotentialDuplicatesUserControl";
			this.PotentialDuplicatesUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.PotentialDuplicatesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 400, true);
			this.PotentialDuplicatesUserControl.TabIndex = 0;
			// 
			// MasterInformationControl
			// 
			this.MasterInformationControl.AllowDrop = true;
			this.MasterInformationControl.AutoSize = true;
			this.MasterInformationControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.MasterInformationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterInformationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.MasterInformationControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MasterInformationControl.Name = "MasterInformationControl";
			this.MasterInformationControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.MasterInformationControl.TabIndex = 0;
			// 
			// CandidateInformationControl
			// 
			this.CandidateInformationControl.AllowDrop = true;
			this.CandidateInformationControl.AutoSize = true;
			this.CandidateInformationControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CandidateInformationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CandidateInformationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 0, true);
			this.CandidateInformationControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CandidateInformationControl.Name = "CandidateInformationControl";
			this.CandidateInformationControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.CandidateInformationControl.TabIndex = 1;
			// 
			// MainContentTableLayoutContainer
			// 
			this.MainContentTableLayoutContainer.AutoScroll = true;
			this.MainContentTableLayoutContainer.HorizontalScroll.Maximum = 0;
			this.MainContentTableLayoutContainer.HorizontalScroll.Visible = false;
			this.MainContentTableLayoutContainer.Controls.Add(MainSplitContainer);
			this.MainContentTableLayoutContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainContentTableLayoutContainer.ColumnCount = 1;
			this.MainContentTableLayoutContainer.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.MainContentTableLayoutContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainContentTableLayoutContainer.Name = "mainContentTableLayoutContainer";
			this.MainContentTableLayoutContainer.RowCount = 1;
			this.MainContentTableLayoutContainer.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.MainContentTableLayoutContainer.TabIndex = 0;
			this.MainContentTableLayoutContainer.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MainContentTableLayoutContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.MainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainSplitContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MainSplitContainer.Name = "MainSplitContainer";
			this.MainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.PotentialDuplicatesUserControl);
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.HeaderContentTableLayoutContainer);
			this.MainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 600, true);
			this.MainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(400);
			this.MainSplitContainer.SplitterWidth = 2;
			this.MainSplitContainer.TabIndex = 2;
			// 
			// HeaderContentSplitContainer
			// 
			this.HeaderContentTableLayoutContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderContentTableLayoutContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderContentTableLayoutContainer.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderContentTableLayoutContainer.Name = "HeaderContentTableLayoutContainer";
			this.HeaderContentTableLayoutContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35)));
			this.HeaderContentTableLayoutContainer.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100));
			this.HeaderContentTableLayoutContainer.Controls.Add(this.HeaderPanel);
			this.HeaderContentTableLayoutContainer.Controls.Add(this.ContentPanel);
			this.HeaderContentTableLayoutContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 200, true);
			this.HeaderContentTableLayoutContainer.TabIndex = 0;
			// 
			// HeaderPanel
			//
			this.HeaderPanel.BackColor = System.Drawing.SystemColors.Control;
			this.HeaderPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.HeaderPanel.ColumnCount = 4;
			this.HeaderPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.HeaderPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0)));
			this.HeaderPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.HeaderPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0)));
			this.HeaderPanel.Controls.Add(this.MasterTableLayoutPanel, 0, 0);
			this.HeaderPanel.Controls.Add(this.CandidateTableLayoutPanel, 2, 0);
			this.HeaderPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.HeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HeaderPanel.Name = "HeaderPanel";
			this.HeaderPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 1, true);
			this.HeaderPanel.RowCount = 1;
			this.HeaderPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(34)));
			this.HeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 35, true);
			this.HeaderPanel.TabIndex = 0;
			// 
			// MasterTableLayoutPanel
			//
			this.MasterTableLayoutPanel.ColumnCount = 2;
			this.MasterTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MasterTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.MasterTableLayoutPanel.Controls.Add(this.MasterTitleLabel, 0, 0);
			this.MasterTableLayoutPanel.Controls.Add(this.MasterButtonTableLayoutPanel, 1, 0);
			this.MasterTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MasterTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MasterTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MasterTableLayoutPanel.Name = "MasterTableLayoutPanel";
			this.MasterTableLayoutPanel.RowCount = 1;
			this.MasterTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32)));
			this.MasterTableLayoutPanel.TabIndex = 2;
			// 
			// MasterTitleLabel
			// 
			this.MasterTitleLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("8F8AB926-39BA-49EE-96E5-05712B1F473C", "Master Information");
			this.MasterTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.MasterTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterTitleLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MasterTitleLabel.Name = "MasterTitleLabel";
			this.MasterTitleLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 10, 0, 0, true);
			this.MasterTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 28, true);
			this.MasterTitleLabel.TabIndex = 0;
			this.MasterTitleLabel.UseMnemonic = false;
			// 
			// MasterButtonTableLayoutPanel
			// 
			this.MasterButtonTableLayoutPanel.AutoSize = true;
			this.MasterButtonTableLayoutPanel.ColumnCount = 3;
			this.MasterButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MasterButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MasterButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.MasterButtonTableLayoutPanel.Controls.Add(this.OpenMasterButton, 0, 0);
			this.MasterButtonTableLayoutPanel.Controls.Add(this.ExclusionButton, 1, 0);
			this.MasterButtonTableLayoutPanel.Controls.Add(this.DeactivateMasterButton, 2, 0);
			this.MasterButtonTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.MasterButtonTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 0, true);
			this.MasterButtonTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.MasterButtonTableLayoutPanel.Name = "MasterButtonTableLayoutPanel";
			this.MasterButtonTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 4, 2, 4, true);
			this.MasterButtonTableLayoutPanel.RowCount = 1;
			this.MasterButtonTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 18, true);
			this.MasterButtonTableLayoutPanel.TabIndex = 1;
			// 
			// OpenMasterButton
			// 
			this.OpenMasterButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("4636b83f-2ac9-4701-b466-fbc6721c3f0e", "Open");
			this.OpenMasterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 8, true);
			this.OpenMasterButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.OpenMasterButton.Name = "OpenMasterButton";
			this.OpenMasterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 23, true);
			this.OpenMasterButton.TabIndex = 0;
			this.OpenMasterButton.ToolTipCaption = null;
			this.OpenMasterButton.UseVisualStyleBackColor = true;
			this.OpenMasterButton.Click += new System.EventHandler(this.OpenMasterButton_Click);
			// 
			// ExclusionButton
			// 
			this.ExclusionButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("18079a6c-0f96-438f-a75b-c9c4c56ba60d", "Exclude");
			this.ExclusionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 8, true);
			this.ExclusionButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.ExclusionButton.Name = "ExclusionButton";
			this.ExclusionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.ExclusionButton.TabIndex = 1;
			this.ExclusionButton.ToolTipCaption = null;
			this.ExclusionButton.UseVisualStyleBackColor = true;
			this.ExclusionButton.Click += new System.EventHandler(this.ExclusionButton_Click);
			// 
			// DeactivateMasterButton
			// 
			this.DeactivateMasterButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("1220e311-58a1-4f20-9d51-ecc74199c0a0", "Deactivate");
			this.DeactivateMasterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 8, true);
			this.DeactivateMasterButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.DeactivateMasterButton.Name = "DeactivateMasterButton";
			this.DeactivateMasterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.DeactivateMasterButton.TabIndex = 2;
			this.DeactivateMasterButton.ToolTipCaption = null;
			this.DeactivateMasterButton.UseVisualStyleBackColor = true;
			this.DeactivateMasterButton.Click += new System.EventHandler(this.DeactivateMasterButton_Click);
			// 
			// CandidateTableLayoutPanel
			// 
			this.CandidateTableLayoutPanel.BackColor = System.Drawing.SystemColors.Control;
			this.CandidateTableLayoutPanel.ColumnCount = 2;
			this.CandidateTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)));
			this.CandidateTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CandidateTableLayoutPanel.Controls.Add(this.CandidateTitleLabel, 0, 0);
			this.CandidateTableLayoutPanel.Controls.Add(this.CandidateButtonTableLayoutPanel, 1, 0);
			this.CandidateTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CandidateTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CandidateTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CandidateTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CandidateTableLayoutPanel.Name = "CandidateTableLayoutPanel";
			this.CandidateTableLayoutPanel.RowCount = 1;
			this.CandidateTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(33)));
			this.CandidateTableLayoutPanel.TabIndex = 1;
			// 
			// CandidateTitleLabel
			// 
			this.CandidateTitleLabel.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("B1829DC3-CC5A-4135-958A-38313FAD48D1", "Candidate Information");
			this.CandidateTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Larger | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CandidateTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.CandidateTitleLabel.Name = "CandidateTitleLabel";
			this.CandidateTitleLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 10, 0, 0, true);
			this.CandidateTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 28, true);
			this.CandidateTitleLabel.TabIndex = 0;
			this.CandidateTitleLabel.UseMnemonic = false;
			// 
			// CandidateButtonTableLayoutPanel
			// 
			this.CandidateButtonTableLayoutPanel.AutoSize = true;
			this.CandidateButtonTableLayoutPanel.ColumnCount = 6;
			this.CandidateButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CandidateButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CandidateButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CandidateButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CandidateButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CandidateButtonTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.CandidateButtonTableLayoutPanel.Controls.Add(this.OpenTargetButton, 4, 0);
			this.CandidateButtonTableLayoutPanel.Controls.Add(this.IgnoreSplitButtonPanel, 5, 0);
			this.CandidateButtonTableLayoutPanel.Controls.Add(this.MergeButtonPanel, 2, 0);
			this.CandidateButtonTableLayoutPanel.Controls.Add(this.OpenButtonPanel, 3, 0);
			this.CandidateButtonTableLayoutPanel.Controls.Add(this.DeactivateCandidateButton, 0, 0);
			this.CandidateButtonTableLayoutPanel.Controls.Add(this.LinkButton, 1, 0);
			this.CandidateButtonTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Right;
			this.CandidateButtonTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 0, true);
			this.CandidateButtonTableLayoutPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CandidateButtonTableLayoutPanel.Name = "CandidateButtonTableLayoutPanel";
			this.CandidateButtonTableLayoutPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 4, 2, 4, true);
			this.CandidateButtonTableLayoutPanel.RowCount = 1;
			this.CandidateButtonTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.CandidateButtonTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 18, true);
			this.CandidateButtonTableLayoutPanel.TabIndex = 1;
			// 
			// OpenTargetButton
			// 
			this.OpenTargetButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("4636b83f-2ac9-4701-b466-fbc6721c3f0e", "Open");
			this.OpenTargetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 8, true);
			this.OpenTargetButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.OpenTargetButton.Name = "OpenTargetButton";
			this.OpenTargetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 23, true);
			this.OpenTargetButton.TabIndex = 9;
			this.OpenTargetButton.ToolTipCaption = null;
			this.OpenTargetButton.UseVisualStyleBackColor = true;
			this.OpenTargetButton.Click += new System.EventHandler(this.OpenTargetButton_Click);
			// 
			// IgnoreSplitButtonPanel
			// 
			this.IgnoreSplitButtonPanel.AutoSize = true;
			this.IgnoreSplitButtonPanel.BackColor = System.Drawing.Color.Silver;
			this.IgnoreSplitButtonPanel.Controls.Add(this.IgnoreButtonToolStrip);
			this.IgnoreSplitButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 8, true);
			this.IgnoreSplitButtonPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.IgnoreSplitButtonPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
			this.IgnoreSplitButtonPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
			this.IgnoreSplitButtonPanel.Name = "IgnoreSplitButtonPanel";
			this.IgnoreSplitButtonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.IgnoreSplitButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.IgnoreSplitButtonPanel.TabIndex = 8;
			// 
			// IgnoreButtonToolStrip
			// 
			this.IgnoreButtonToolStrip.BackColor = System.Drawing.SystemColors.ControlLight;
			this.IgnoreButtonToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IgnoreButtonToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
			this.IgnoreButtonToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.IgnoreButtonToolStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.IgnoreButtonToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.IgnoreSplitButton});
			this.IgnoreButtonToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.IgnoreButtonToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.IgnoreButtonToolStrip.Name = "IgnoreButtonToolStrip";
			this.IgnoreButtonToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.IgnoreButtonToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 21, true);
			this.IgnoreButtonToolStrip.TabIndex = 1;
			this.IgnoreButtonToolStrip.Text = "zToolStrip1";
			// 
			// IgnoreSplitButton
			// 
			this.IgnoreSplitButton.BackColor = System.Drawing.SystemColors.ControlLight;
			this.IgnoreSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.IgnoreSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.ForMeMenuItem,
			this.ForEveryoneMenuItem,
			this.RemoveIgnoreMenuItem});
			this.IgnoreSplitButton.Margin = new System.Windows.Forms.Padding(0);
			this.IgnoreSplitButton.Name = "IgnoreSplitButton";
			this.IgnoreSplitButton.Size = new System.Drawing.Size(27, 42);
			this.IgnoreSplitButton.ButtonClick += new System.EventHandler(this.ForMeMenuItem_Click);
			// 
			// ForMeMenuItem
			// 
			this.ForMeMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.ForMeMenuItem.Name = "ForMeMenuItem";
			this.ForMeMenuItem.Size = new System.Drawing.Size(133, 44);
			this.ForMeMenuItem.Click += new System.EventHandler(this.ForMeMenuItem_Click);
			// 
			// ForEveryoneMenuItem
			// 
			this.ForEveryoneMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.ForEveryoneMenuItem.Name = "ForEveryoneMenuItem";
			this.ForEveryoneMenuItem.Size = new System.Drawing.Size(133, 44);
			this.ForEveryoneMenuItem.Click += new System.EventHandler(this.ForEveryoneMenuItem_Click);
			// 
			// RemoveIgnoreMenuItem
			// 
			this.RemoveIgnoreMenuItem.Name = "RemoveIgnoreMenuItem";
			this.RemoveIgnoreMenuItem.Size = new System.Drawing.Size(133, 44);
			this.RemoveIgnoreMenuItem.Click += new System.EventHandler(this.RemoveIgnoreMenuItem_Click);
			// 
			// MergeButtonPanel
			// 
			this.MergeButtonPanel.AutoSize = true;
			this.MergeButtonPanel.BackColor = System.Drawing.Color.Silver;
			this.MergeButtonPanel.Controls.Add(this.ToolStrip);
			this.MergeButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 8, true);
			this.MergeButtonPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.MergeButtonPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
			this.MergeButtonPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
			this.MergeButtonPanel.Name = "MergeButtonPanel";
			this.MergeButtonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.MergeButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.MergeButtonPanel.TabIndex = 7;
			// 
			// ToolStrip
			// 
			this.ToolStrip.BackColor = System.Drawing.SystemColors.ControlLight;
			this.ToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
			this.ToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.ToolStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.MergeButton});
			this.ToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 21, true);
			this.ToolStrip.TabIndex = 1;
			// 
			// MergeButton
			// 
			this.MergeButton.BackColor = System.Drawing.SystemColors.ControlLight;
			this.MergeButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.MergeButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.RetainMaster,
			this.RetainCandidate});
			this.MergeButton.Margin = new System.Windows.Forms.Padding(0);
			this.MergeButton.Name = "MergeButton";
			this.MergeButton.Size = new System.Drawing.Size(27, 42);
			this.MergeButton.ButtonClick += new System.EventHandler(this.RetainMaster_Click);
			// 
			// RetainMaster
			// 
			this.RetainMaster.BackColor = System.Drawing.SystemColors.ControlLight;
			this.RetainMaster.Name = "RetainMaster";
			this.RetainMaster.Size = new System.Drawing.Size(133, 44);
			this.RetainMaster.Click += new System.EventHandler(this.RetainMaster_Click);
			// 
			// RetainCandidate
			// 
			this.RetainCandidate.BackColor = System.Drawing.SystemColors.ControlLight;
			this.RetainCandidate.Name = "RetainCandidate";
			this.RetainCandidate.Size = new System.Drawing.Size(133, 44);
			this.RetainCandidate.Click += new System.EventHandler(this.RetainCandidate_Click);
			// 
			// OpenButtonPanel
			// 
			this.OpenButtonPanel.AutoSize = true;
			this.OpenButtonPanel.BackColor = System.Drawing.Color.Silver;
			this.OpenButtonPanel.Controls.Add(this.OpenButtonToolStrip);
			this.OpenButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 8, true);
			this.OpenButtonPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.OpenButtonPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
			this.OpenButtonPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 23, true);
			this.OpenButtonPanel.Name = "OpenButtonPanel";
			this.OpenButtonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.OpenButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.OpenButtonPanel.TabIndex = 6;
			// 
			// OpenButtonToolStrip
			// 
			this.OpenButtonToolStrip.BackColor = System.Drawing.SystemColors.ControlLight;
			this.OpenButtonToolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OpenButtonToolStrip.GripMargin = new System.Windows.Forms.Padding(0);
			this.OpenButtonToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.OpenButtonToolStrip.ImageScalingSize = new System.Drawing.Size(40, 40);
			this.OpenButtonToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.OpenButtonSplitButton});
			this.OpenButtonToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.OpenButtonToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.OpenButtonToolStrip.Name = "OpenButtonToolStrip";
			this.OpenButtonToolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.OpenButtonToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 21, true);
			this.OpenButtonToolStrip.TabIndex = 1;
			this.OpenButtonToolStrip.Text = "zToolStrip2";
			// 
			// OpenButtonSplitButton
			// 
			this.OpenButtonSplitButton.BackColor = System.Drawing.SystemColors.ControlLight;
			this.OpenButtonSplitButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.OpenButtonSplitButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OpenButtonSplitButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.PersonMenuItem,
			this.StaffMenuItem,
			this.ContactMenuItem,
			this.ApplicantMenuItem});
			this.OpenButtonSplitButton.Margin = new System.Windows.Forms.Padding(0);
			this.OpenButtonSplitButton.Name = "OpenButtonSplitButton";
			this.OpenButtonSplitButton.Size = new System.Drawing.Size(27, 42);
			this.OpenButtonSplitButton.ButtonClick += new System.EventHandler(this.PersonMenuItem_Click);
			// 
			// PersonMenuItem
			// 
			this.PersonMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.PersonMenuItem.Name = "PersonMenuItem";
			this.PersonMenuItem.Size = new System.Drawing.Size(133, 44);
			this.PersonMenuItem.Click += new System.EventHandler(this.PersonMenuItem_Click);
			// 
			// StaffMenuItem
			// 
			this.StaffMenuItem.BackColor = System.Drawing.SystemColors.Control;
			this.StaffMenuItem.Name = "StaffMenuItem";
			this.StaffMenuItem.Size = new System.Drawing.Size(133, 44);
			this.StaffMenuItem.Click += new System.EventHandler(this.StaffMenuItem_Click);
			// 
			// ContactMenuItem
			// 
			this.ContactMenuItem.Name = "ContactMenuItem";
			this.ContactMenuItem.Size = new System.Drawing.Size(133, 44);
			this.ContactMenuItem.Click += new System.EventHandler(this.ContactMenuItem_Click);
			// 
			// ApplicantMenuItem
			// 
			this.ApplicantMenuItem.Name = "ApplicantMenuItem";
			this.ApplicantMenuItem.Size = new System.Drawing.Size(133, 44);
			this.ApplicantMenuItem.Click += new System.EventHandler(this.ApplicantMenuItem_Click);
			// 
			// DeactivateCandidateButton
			// 
			this.DeactivateCandidateButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("1220e311-58a1-4f20-9d51-ecc74199c0a0", "Deactivate");
			this.DeactivateCandidateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 8, true);
			this.DeactivateCandidateButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.DeactivateCandidateButton.Name = "DeactivateCandidateButton";
			this.DeactivateCandidateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.DeactivateCandidateButton.TabIndex = 3;
			this.DeactivateCandidateButton.ToolTipCaption = null;
			this.DeactivateCandidateButton.UseVisualStyleBackColor = true;
			this.DeactivateCandidateButton.Click += new System.EventHandler(this.DeactivateCandidateButton_Click);
			// 
			// LinkButton
			// 
			this.LinkButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5aef98b7-2390-45d2-b1d2-8949273fc1b1", "Link");
			this.LinkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 8, true);
			this.LinkButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 1, 2, 1, true);
			this.LinkButton.Name = "LinkButton";
			this.LinkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 23, true);
			this.LinkButton.TabIndex = 4;
			this.LinkButton.ToolTipCaption = null;
			this.LinkButton.UseVisualStyleBackColor = true;
			this.LinkButton.Click += new System.EventHandler(this.LinkButton_Click);
			// 
			// ContentPanel
			// 
			this.ContentPanel.AutoScroll = true;
			this.ContentPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
			this.ContentPanel.ColumnCount = 4;
			this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0)));
			this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.ContentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(0)));
			this.ContentPanel.Controls.Add(this.MasterInformationControl, 0, 0);
			this.ContentPanel.Controls.Add(this.CandidateInformationControl, 2, 0);
			this.ContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContentPanel.Name = "ContentPanel";
			this.ContentPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0, true);
			this.ContentPanel.RowCount = 1;
			this.ContentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ContentPanel.TabIndex = 0;
			this.ContentPanel.SizeChanged += new System.EventHandler(this.OnSizeChanged);
			// 
			// ButtonTableLayoutPanel
			// 
			this.ButtonTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ButtonTableLayoutPanel.Name = "ButtonTableLayoutPanel";
			this.ButtonTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 44, true);
			this.ButtonTableLayoutPanel.TabIndex = 0;
			// 
			// DeduplicationResultsViewerDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainContentTableLayoutContainer);
			this.Name = "DeduplicationResultsViewerDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 1200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PotentialDuplicatesUserControl.ResumeLayout(true);
			this.PotentialDuplicatesUserControl.PerformLayout();
			this.MasterInformationControl.ResumeLayout(true);
			this.MasterInformationControl.PerformLayout();
			this.CandidateInformationControl.ResumeLayout(true);
			this.CandidateInformationControl.PerformLayout();
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel1.PerformLayout();
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.MainSplitContainer.PerformLayout();
			this.HeaderContentTableLayoutContainer.ResumeLayout(false);
			this.HeaderContentTableLayoutContainer.PerformLayout();
			this.HeaderPanel.ResumeLayout(false);
			this.HeaderPanel.PerformLayout();
			this.MasterTableLayoutPanel.ResumeLayout(false);
			this.MasterTableLayoutPanel.PerformLayout();
			this.MasterButtonTableLayoutPanel.ResumeLayout(false);
			this.MasterButtonTableLayoutPanel.PerformLayout();
			this.CandidateTableLayoutPanel.ResumeLayout(false);
			this.CandidateTableLayoutPanel.PerformLayout();
			this.CandidateButtonTableLayoutPanel.ResumeLayout(false);
			this.CandidateButtonTableLayoutPanel.PerformLayout();
			this.IgnoreSplitButtonPanel.ResumeLayout(false);
			this.IgnoreSplitButtonPanel.PerformLayout();
			this.IgnoreButtonToolStrip.ResumeLayout(false);
			this.IgnoreButtonToolStrip.PerformLayout();
			this.MergeButtonPanel.ResumeLayout(false);
			this.MergeButtonPanel.PerformLayout();
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			this.OpenButtonPanel.ResumeLayout(false);
			this.OpenButtonPanel.PerformLayout();
			this.OpenButtonToolStrip.ResumeLayout(false);
			this.OpenButtonToolStrip.PerformLayout();
			this.ContentPanel.ResumeLayout(false);
			this.ContentPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private MasterCandidateInformationControl MasterInformationControl;
		private MasterCandidateInformationControl CandidateInformationControl;
		private CargoWise.Windows.UI.KSplitContainer MainSplitContainer;
		private CargoWise.Windows.UI.KTableLayoutPanel MainContentTableLayoutContainer;
		private CargoWise.Windows.UI.KTableLayoutPanel HeaderContentTableLayoutContainer;
		private CargoWise.Windows.UI.KTableLayoutPanel HeaderPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel ContentPanel;
		private PotentialDuplicatesUserControl PotentialDuplicatesUserControl;
		private ZArchitecture.GUI.ZToolStrip ToolStrip;
		private System.Windows.Forms.ToolStripSplitButton MergeButton;
		private System.Windows.Forms.ToolStripMenuItem RetainMaster;
		private System.Windows.Forms.ToolStripMenuItem RetainCandidate;
		private CargoWise.Windows.UI.KTableLayoutPanel ButtonTableLayoutPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel CandidateTableLayoutPanel;
		private ZArchitecture.ZLabel MasterTitleLabel;
		private ZArchitecture.ZLabel CandidateTitleLabel;
		private CargoWise.Windows.UI.KTableLayoutPanel MasterTableLayoutPanel;
		private CargoWise.Windows.UI.KTableLayoutPanel MasterButtonTableLayoutPanel;
		private ZArchitecture.GUI.ZButton OpenMasterButton;
		private ZArchitecture.GUI.ZButton ExclusionButton;
		private ZArchitecture.GUI.ZButton DeactivateMasterButton;
		private CargoWise.Windows.UI.KTableLayoutPanel CandidateButtonTableLayoutPanel;
		private ZArchitecture.GUI.ZButton DeactivateCandidateButton;
		private ZArchitecture.GUI.ZButton LinkButton;
		private ZArchitecture.GUI.ZPanel MergeButtonPanel;
		private ZArchitecture.GUI.ZPanel OpenButtonPanel;
		private ZArchitecture.GUI.ZToolStrip OpenButtonToolStrip;
		private System.Windows.Forms.ToolStripSplitButton OpenButtonSplitButton;
		private System.Windows.Forms.ToolStripMenuItem PersonMenuItem;
		private System.Windows.Forms.ToolStripMenuItem StaffMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ContactMenuItem;
		private ZArchitecture.GUI.ZPanel IgnoreSplitButtonPanel;
		private ZArchitecture.GUI.ZToolStrip IgnoreButtonToolStrip;
		private System.Windows.Forms.ToolStripSplitButton IgnoreSplitButton;
		private System.Windows.Forms.ToolStripMenuItem ForMeMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ForEveryoneMenuItem;
		private System.Windows.Forms.ToolStripMenuItem RemoveIgnoreMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ApplicantMenuItem;
		private ZArchitecture.GUI.ZButton OpenTargetButton;
	}
}
