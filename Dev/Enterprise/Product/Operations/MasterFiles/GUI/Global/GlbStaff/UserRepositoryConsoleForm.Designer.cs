using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class UserRepositoryConsoleForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserRepositoryConsoleForm));
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ObjectDefinitionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ObjectDefinitionTextBox = new Enterprise.ZArchitecture.GUI.ZSqlTextBox();
			this.SaveScriptFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OpenScriptFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DbObjectTypeImageList = new System.Windows.Forms.ImageList(this.components);
			this.EnterpriseObjectListView = new CargoWise.Windows.UI.KListView();
			this.EntObjName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.EntObjType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.CargoWiseOneLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShowEnterpriseObjectDefinitionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UserRepositoryObjectListView = new CargoWise.Windows.UI.KListView();
			this.UserRepObjSchema = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.UserRepObjName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.UserRepObjType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.UserRepositoryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DropUserRepositoryObjectButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ExecuteUserRepositoryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ShowUseRepositoryObjectDefinitionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ObjectViewSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TopLevelSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.EntObjSchema = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ObjectDefinitionGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ObjectViewSplitContainer)).BeginInit();
			this.ObjectViewSplitContainer.Panel1.SuspendLayout();
			this.ObjectViewSplitContainer.Panel2.SuspendLayout();
			this.ObjectViewSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopLevelSplitContainer)).BeginInit();
			this.TopLevelSplitContainer.Panel1.SuspendLayout();
			this.TopLevelSplitContainer.Panel2.SuspendLayout();
			this.TopLevelSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 642, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 24, true);
			this.MainStatusBar.MouseHover += new System.EventHandler(this.MainStatusBar_MouseHover);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Style = System.Windows.Forms.StatusBarPanelStyle.Text;
			this.MessageStatusBarPanel.Width = 957;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.UserRepository);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4ca6cb8e-745b-4526-b5e5-64925726bdcb", "Close");
			this.CloseButton.Font = new System.Drawing.Font(OFont.NormalFontName, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.CloseButton.Image = ((System.Drawing.Image)(resources.GetObject("CloseButton.Image")));
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(822, 260, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 53, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ObjectDefinitionGroupBox
			// 
			this.ObjectDefinitionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ObjectDefinitionGroupBox.Controls.Add(this.ObjectDefinitionTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ObjectDefinitionGroupBox, false);
			this.ObjectDefinitionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
			this.ObjectDefinitionGroupBox.Name = "ObjectDefinitionGroupBox";
			this.ObjectDefinitionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 251, true);
			this.ObjectDefinitionGroupBox.TabIndex = 0;
			this.ObjectDefinitionGroupBox.TabStop = false;
			// 
			// ObjectDefinitionTextBox
			// 
			this.ObjectDefinitionTextBox.AcceptsReturn = true;
			this.ObjectDefinitionTextBox.AcceptsTab = true;
			this.ObjectDefinitionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ObjectDefinitionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ObjectDefinitionTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ObjectDefinitionTextBox, false);
			this.ObjectDefinitionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ObjectDefinitionTextBox.Multiline = true;
			this.ObjectDefinitionTextBox.Name = "ObjectDefinitionTextBox";
			this.ObjectDefinitionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ObjectDefinitionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 232, true);
			this.ObjectDefinitionTextBox.TabIndex = 0;
			// 
			// SaveScriptFileButton
			// 
			this.SaveScriptFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SaveScriptFileButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0efb8a44-5f48-4972-9b15-21618a903ace", "Save");
			this.SaveScriptFileButton.Font = new System.Drawing.Font(OFont.NormalFontName, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SaveScriptFileButton.Image = ((System.Drawing.Image)(resources.GetObject("SaveScriptFileButton.Image")));
			this.SaveScriptFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 260, true);
			this.SaveScriptFileButton.Name = "SaveScriptFileButton";
			this.SaveScriptFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 53, true);
			this.SaveScriptFileButton.TabIndex = 2;
			this.SaveScriptFileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.SaveScriptFileButton.UseVisualStyleBackColor = true;
			this.SaveScriptFileButton.Click += new System.EventHandler(this.SaveScriptFileButton_Click);
			// 
			// OpenScriptFileButton
			// 
			this.OpenScriptFileButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OpenScriptFileButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("aabbb6b7-bcc0-48a9-a137-994010cac2f7", "Open");
			this.OpenScriptFileButton.Font = new System.Drawing.Font(OFont.NormalFontName, 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OpenScriptFileButton.Image = ((System.Drawing.Image)(resources.GetObject("OpenScriptFileButton.Image")));
			this.OpenScriptFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 260, true);
			this.OpenScriptFileButton.Name = "OpenScriptFileButton";
			this.OpenScriptFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 53, true);
			this.OpenScriptFileButton.TabIndex = 1;
			this.OpenScriptFileButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.OpenScriptFileButton.UseVisualStyleBackColor = true;
			this.OpenScriptFileButton.Click += new System.EventHandler(this.OpenScriptFileButton_Click);
			// 
			// DbObjectTypeImageList
			// 
			ZArchitecture.GUI.ZImageListDpiScalingHelper.SetScaledImagesFromImageListStreamer(this.DbObjectTypeImageList, (System.Windows.Forms.ImageListStreamer)(resources.GetObject("DbObjectTypeImageList.ImageStream")));
			this.DbObjectTypeImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.DbObjectTypeImageList.Images.SetKeyName(0, "SCALAR_FUNCTION");
			this.DbObjectTypeImageList.Images.SetKeyName(1, "TABLE_FUNCTION");
			this.DbObjectTypeImageList.Images.SetKeyName(2, "VIEW");
			this.DbObjectTypeImageList.Images.SetKeyName(3, "PROCEDURE");
			this.DbObjectTypeImageList.Images.SetKeyName(4, "USER_TABLE");
			// 
			// EnterpriseObjectListView
			// 
			this.EnterpriseObjectListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.EnterpriseObjectListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.EntObjSchema,
			this.EntObjName,
			this.EntObjType});
			this.EnterpriseObjectListView.FullRowSelect = true;
			this.EnterpriseObjectListView.GridLines = true;
			this.EnterpriseObjectListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.EnterpriseObjectListView.HideSelection = false;
			this.EnterpriseObjectListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 31, true);
			this.EnterpriseObjectListView.MultiSelect = false;
			this.EnterpriseObjectListView.Name = "EnterpriseObjectListView";
			this.EnterpriseObjectListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 232, true);
			this.EnterpriseObjectListView.SmallImageList = this.DbObjectTypeImageList;
			this.EnterpriseObjectListView.TabIndex = 1;
			this.EnterpriseObjectListView.UseCompatibleStateImageBehavior = false;
			this.EnterpriseObjectListView.View = System.Windows.Forms.View.Details;
			this.EnterpriseObjectListView.DoubleClick += new System.EventHandler(this.DbObjectListView_DoubleClick);
			// 
			// EntObjName
			// 
			this.EntObjName.Text = "Name";
			this.EntObjName.Width = 300;
			// 
			// EntObjType
			// 
			this.EntObjType.Text = "Type";
			this.EntObjType.Width = 100;
			// 
			// CargoWiseOneLabel
			// 
			this.CargoWiseOneLabel.AutoSize = true;
			this.CargoWiseOneLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("08b4cbac-e198-4bf8-8f4a-22011b049d7c", "Main Database");
			this.CargoWiseOneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.CargoWiseOneLabel.Name = "DatabaseLabel";
			this.CargoWiseOneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 13, true);
			this.CargoWiseOneLabel.TabIndex = 0;
			// 
			// ShowEnterpriseObjectDefinitionButton
			// 
			this.ShowEnterpriseObjectDefinitionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowEnterpriseObjectDefinitionButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c39b23dd-27a9-4099-8f61-a33cb4520811", "Show Definition");
			this.ShowEnterpriseObjectDefinitionButton.Font = new System.Drawing.Font(OFont.NormalFontName, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ShowEnterpriseObjectDefinitionButton.Image = ((System.Drawing.Image)(resources.GetObject("ShowEnterpriseObjectDefinitionButton.Image")));
			this.ShowEnterpriseObjectDefinitionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 269, true);
			this.ShowEnterpriseObjectDefinitionButton.Name = "ShowEnterpriseObjectDefinitionButton";
			this.ShowEnterpriseObjectDefinitionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 47, true);
			this.ShowEnterpriseObjectDefinitionButton.TabIndex = 2;
			this.ShowEnterpriseObjectDefinitionButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.ShowEnterpriseObjectDefinitionButton.UseVisualStyleBackColor = true;
			this.ShowEnterpriseObjectDefinitionButton.Click += new System.EventHandler(this.ShowEnterpriseObjectDefinitionButton_Click);
			// 
			// UserRepositoryObjectListView
			// 
			this.UserRepositoryObjectListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.UserRepositoryObjectListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.UserRepObjSchema,
			this.UserRepObjName,
			this.UserRepObjType});
			this.UserRepositoryObjectListView.FullRowSelect = true;
			this.UserRepositoryObjectListView.GridLines = true;
			this.UserRepositoryObjectListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.UserRepositoryObjectListView.HideSelection = false;
			this.UserRepositoryObjectListView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 31, true);
			this.UserRepositoryObjectListView.MultiSelect = false;
			this.UserRepositoryObjectListView.Name = "UserRepositoryObjectListView";
			this.UserRepositoryObjectListView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 232, true);
			this.UserRepositoryObjectListView.SmallImageList = this.DbObjectTypeImageList;
			this.UserRepositoryObjectListView.TabIndex = 1;
			this.UserRepositoryObjectListView.UseCompatibleStateImageBehavior = false;
			this.UserRepositoryObjectListView.View = System.Windows.Forms.View.Details;
			this.UserRepositoryObjectListView.DoubleClick += new System.EventHandler(this.DbObjectListView_DoubleClick);
			// 
			// UserRepObjSchema
			// 
			this.UserRepObjSchema.Text = "Schema";
			this.UserRepObjSchema.Width = 50;
			// 
			// UserRepObjName
			// 
			this.UserRepObjName.Text = "Name";
			this.UserRepObjName.Width = 300;
			// 
			// UserRepObjType
			// 
			this.UserRepObjType.Text = "Type";
			this.UserRepObjType.Width = 110;
			// 
			// UserRepositoryLabel
			// 
			this.UserRepositoryLabel.AutoSize = true;
			this.UserRepositoryLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f57db4e0-7996-4212-8ab6-58dff261b1c5", "User Repository");
			this.UserRepositoryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.UserRepositoryLabel.Name = "UserRepositoryLabel";
			this.UserRepositoryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.UserRepositoryLabel.TabIndex = 0;
			// 
			// DropUserRepositoryObjectButton
			// 
			this.DropUserRepositoryObjectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.DropUserRepositoryObjectButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6eaa05c8-e3c5-4aa7-9ff0-7984475ace72", "Delete from User Repository");
			this.DropUserRepositoryObjectButton.Font = new System.Drawing.Font(OFont.NormalFontName, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DropUserRepositoryObjectButton.Image = ((System.Drawing.Image)(resources.GetObject("DropUserRepositoryObjectButton.Image")));
			this.DropUserRepositoryObjectButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 269, true);
			this.DropUserRepositoryObjectButton.Name = "DropUserRepositoryObjectButton";
			this.DropUserRepositoryObjectButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 47, true);
			this.DropUserRepositoryObjectButton.TabIndex = 4;
			this.DropUserRepositoryObjectButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.DropUserRepositoryObjectButton.UseVisualStyleBackColor = true;
			this.DropUserRepositoryObjectButton.Click += new System.EventHandler(this.DropUserRepositoryObjectButton_Click);
			// 
			// ExecuteUserRepositoryButton
			// 
			this.ExecuteUserRepositoryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ExecuteUserRepositoryButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a44dfac1-ea0b-4389-aa54-1bbb7512c9c7", "Execute on User Repository");
			this.ExecuteUserRepositoryButton.Font = new System.Drawing.Font(OFont.NormalFontName, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ExecuteUserRepositoryButton.Image = ((System.Drawing.Image)(resources.GetObject("ExecuteUserRepositoryButton.Image")));
			this.ExecuteUserRepositoryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(169, 269, true);
			this.ExecuteUserRepositoryButton.Name = "ExecuteUserRepositoryButton";
			this.ExecuteUserRepositoryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 47, true);
			this.ExecuteUserRepositoryButton.TabIndex = 3;
			this.ExecuteUserRepositoryButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.ExecuteUserRepositoryButton.UseVisualStyleBackColor = true;
			this.ExecuteUserRepositoryButton.Click += new System.EventHandler(this.ExecuteUserRepositoryButton_Click);
			// 
			// ShowUseRepositoryObjectDefinitionButton
			// 
			this.ShowUseRepositoryObjectDefinitionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ShowUseRepositoryObjectDefinitionButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ff18780d-e5b1-4d72-90c4-85493fda8813", "Show Definition");
			this.ShowUseRepositoryObjectDefinitionButton.Font = new System.Drawing.Font(OFont.NormalFontName, 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ShowUseRepositoryObjectDefinitionButton.Image = ((System.Drawing.Image)(resources.GetObject("ShowUseRepositoryObjectDefinitionButton.Image")));
			this.ShowUseRepositoryObjectDefinitionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 269, true);
			this.ShowUseRepositoryObjectDefinitionButton.Name = "ShowUseRepositoryObjectDefinitionButton";
			this.ShowUseRepositoryObjectDefinitionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 47, true);
			this.ShowUseRepositoryObjectDefinitionButton.TabIndex = 2;
			this.ShowUseRepositoryObjectDefinitionButton.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
			this.ShowUseRepositoryObjectDefinitionButton.UseVisualStyleBackColor = true;
			this.ShowUseRepositoryObjectDefinitionButton.Click += new System.EventHandler(this.ShowUseRepositoryObjectDefinitionButton_Click);
			// 
			// ObjectViewSplitContainer
			// 
			this.ObjectViewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ObjectViewSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ObjectViewSplitContainer.Name = "ObjectViewSplitContainer";
			// 
			// ObjectViewSplitContainer.Panel1
			// 
			this.ObjectViewSplitContainer.Panel1.Controls.Add(this.EnterpriseObjectListView);
			this.ObjectViewSplitContainer.Panel1.Controls.Add(this.CargoWiseOneLabel);
			this.ObjectViewSplitContainer.Panel1.Controls.Add(this.ShowEnterpriseObjectDefinitionButton);
			this.ObjectViewSplitContainer.Panel1MinSize = 250;
			// 
			// ObjectViewSplitContainer.Panel2
			// 
			this.ObjectViewSplitContainer.Panel2.Controls.Add(this.UserRepositoryObjectListView);
			this.ObjectViewSplitContainer.Panel2.Controls.Add(this.UserRepositoryLabel);
			this.ObjectViewSplitContainer.Panel2.Controls.Add(this.DropUserRepositoryObjectButton);
			this.ObjectViewSplitContainer.Panel2.Controls.Add(this.ExecuteUserRepositoryButton);
			this.ObjectViewSplitContainer.Panel2.Controls.Add(this.ShowUseRepositoryObjectDefinitionButton);
			this.ObjectViewSplitContainer.Panel2MinSize = 495;
			this.ObjectViewSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 319, true);
			this.ObjectViewSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(485);
			this.ObjectViewSplitContainer.TabIndex = 0;
			// 
			// TopLevelSplitContainer
			// 
			this.TopLevelSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopLevelSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopLevelSplitContainer.Name = "TopLevelSplitContainer";
			this.TopLevelSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TopLevelSplitContainer.Panel1
			// 
			this.TopLevelSplitContainer.Panel1.Controls.Add(this.ObjectViewSplitContainer);
			this.TopLevelSplitContainer.Panel1MinSize = 200;
			// 
			// TopLevelSplitContainer.Panel2
			// 
			this.TopLevelSplitContainer.Panel2.Controls.Add(this.ObjectDefinitionGroupBox);
			this.TopLevelSplitContainer.Panel2.Controls.Add(this.SaveScriptFileButton);
			this.TopLevelSplitContainer.Panel2.Controls.Add(this.CloseButton);
			this.TopLevelSplitContainer.Panel2.Controls.Add(this.OpenScriptFileButton);
			this.TopLevelSplitContainer.Panel2MinSize = 200;
			this.TopLevelSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 642, true);
			this.TopLevelSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(319);
			this.TopLevelSplitContainer.TabIndex = 24;
			// 
			// EntObjSchema
			// 
			this.EntObjSchema.Text = "Schema";
			this.EntObjSchema.Width = 50;
			// 
			// UserRepositoryConsoleForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 666, true);
			this.Controls.Add(this.TopLevelSplitContainer);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.UserRepository);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 700, true);
			this.Name = "UserRepositoryConsoleForm";
			this.Load += new System.EventHandler(this.UserRepositoryConsoleForm_Load);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopLevelSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ObjectDefinitionGroupBox.ResumeLayout(false);
			this.ObjectDefinitionGroupBox.PerformLayout();
			this.ObjectViewSplitContainer.Panel1.ResumeLayout(false);
			this.ObjectViewSplitContainer.Panel1.PerformLayout();
			this.ObjectViewSplitContainer.Panel2.ResumeLayout(false);
			this.ObjectViewSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ObjectViewSplitContainer)).EndInit();
			this.ObjectViewSplitContainer.ResumeLayout(false);
			this.ObjectViewSplitContainer.PerformLayout();
			this.TopLevelSplitContainer.Panel1.ResumeLayout(false);
			this.TopLevelSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopLevelSplitContainer)).EndInit();
			this.TopLevelSplitContainer.ResumeLayout(false);
			this.TopLevelSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZGroupBox ObjectDefinitionGroupBox;
		private ZArchitecture.GUI.ZButton SaveScriptFileButton;
		private ZArchitecture.GUI.ZButton OpenScriptFileButton;
		private System.Windows.Forms.ImageList DbObjectTypeImageList;
		private CargoWise.Windows.UI.KListView EnterpriseObjectListView;
		private System.Windows.Forms.ColumnHeader EntObjName;
		private System.Windows.Forms.ColumnHeader EntObjType;
		private Enterprise.ZArchitecture.ZLabel CargoWiseOneLabel;
		private ZArchitecture.GUI.ZButton ShowEnterpriseObjectDefinitionButton;
		private CargoWise.Windows.UI.KListView UserRepositoryObjectListView;
		private System.Windows.Forms.ColumnHeader UserRepObjName;
		private System.Windows.Forms.ColumnHeader UserRepObjType;
		private Enterprise.ZArchitecture.ZLabel UserRepositoryLabel;
		private ZArchitecture.GUI.ZButton DropUserRepositoryObjectButton;
		private ZArchitecture.GUI.ZButton ExecuteUserRepositoryButton;
		private ZArchitecture.GUI.ZButton ShowUseRepositoryObjectDefinitionButton;
		private CargoWise.Windows.UI.KSplitContainer ObjectViewSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer TopLevelSplitContainer;
		private ZArchitecture.GUI.ZSqlTextBox ObjectDefinitionTextBox;
		private System.Windows.Forms.ColumnHeader UserRepObjSchema;
		private System.Windows.Forms.ColumnHeader EntObjSchema;
	}
}
