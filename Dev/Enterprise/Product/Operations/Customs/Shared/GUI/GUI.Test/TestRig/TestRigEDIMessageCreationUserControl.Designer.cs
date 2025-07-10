namespace Enterprise.Customs.GUI.Testing
{
	public partial class TestRigEDIMessageCreationUserControl
	{
		void InitializeComponent()
		{
			this.ApplicationCodeTextBox = new CargoWise.Windows.UI.KTextBox();
			this.ApplicationCodeLabel = new CargoWise.Windows.UI.KLabel();
			this.CreationFromLabel = new CargoWise.Windows.UI.KLabel();
			this.CreateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CreationFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CreationToLabel = new CargoWise.Windows.UI.KLabel();
			this.CreationToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageTypeTextBox = new CargoWise.Windows.UI.KTextBox();
			this.MessageTypeLabel = new CargoWise.Windows.UI.KLabel();
			this.MessageTextTextBox = new CargoWise.Windows.UI.KTextBox();
			this.MesageTextLabel = new CargoWise.Windows.UI.KLabel();
			this.UseJobBranchCheckBox = new CargoWise.Windows.UI.KCheckBox();
			this.NoOfMsgPerJobCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MessagePerJobLabel = new CargoWise.Windows.UI.KLabel();
			this.InterchangeCW1ReferenceTextBox = new CargoWise.Windows.UI.KTextBox();
			this.InterchangeCW1ReferenceLabel = new CargoWise.Windows.UI.KLabel();
			this.InterchangeDetailsGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.InterchangeStatusTextBox = new CargoWise.Windows.UI.KTextBox();
			this.TransportTypeLabel = new CargoWise.Windows.UI.KLabel();
			this.InterchangeStatusLabel = new CargoWise.Windows.UI.KLabel();
			this.TransportTypeTextBox = new CargoWise.Windows.UI.KTextBox();
			this.InterchangeCustomsRerenceTextBox = new CargoWise.Windows.UI.KTextBox();
			this.InterchangeCustomsRerenceLabel = new CargoWise.Windows.UI.KLabel();
			this.CreateEDIMessageGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.CreateMessageAndInterchangePanel = new CargoWise.Windows.UI.KPanel();
			this.CreateBothMessageAndInterchangeRadioButton = new CargoWise.Windows.UI.KRadioButton();
			this.CreateInterchangeRadioButton = new CargoWise.Windows.UI.KRadioButton();
			this.CreateMessageRadioButton = new CargoWise.Windows.UI.KRadioButton();
			this.CreateIncomingAndOutgoingPanel = new CargoWise.Windows.UI.KPanel();
			this.CreateBothOutgoingAndIncomingRadioButton = new CargoWise.Windows.UI.KRadioButton();
			this.CreateOutgoingRadioButton = new CargoWise.Windows.UI.KRadioButton();
			this.CreateIncomingRadioButton = new CargoWise.Windows.UI.KRadioButton();
			this.MessageStatusLabel = new CargoWise.Windows.UI.KLabel();
			this.MessageStatusTextBox = new CargoWise.Windows.UI.KTextBox();
			this.CreationFromDateEdit.SuspendLayout();
			this.CreationToDateEdit.SuspendLayout();
			this.InterchangeDetailsGroupBox.SuspendLayout();
			this.CreateEDIMessageGroupBox.SuspendLayout();
			this.CreateMessageAndInterchangePanel.SuspendLayout();
			this.CreateIncomingAndOutgoingPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// ApplicationCodeTextBox
			// 
			this.ApplicationCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.ApplicationCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 45, true);
			this.ApplicationCodeTextBox.MaxLength = 3;
			this.ApplicationCodeTextBox.Name = "ApplicationCodeTextBox";
			this.ApplicationCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.ApplicationCodeTextBox.TabIndex = 5;
			// 
			// ApplicationCodeLabel
			// 
			this.ApplicationCodeLabel.AutoSize = true;
			this.ApplicationCodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 45, true);
			this.ApplicationCodeLabel.Name = "ApplicationCodeLabel";
			this.ApplicationCodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 13, true);
			this.ApplicationCodeLabel.TabIndex = 4;
			this.ApplicationCodeLabel.Text = "Application Code:";
			this.ApplicationCodeLabel.UseMnemonic = false;
			// 
			// CreationFromLabel
			// 
			this.CreationFromLabel.AutoSize = true;
			this.CreationFromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 24, true);
			this.CreationFromLabel.Name = "CreationFromLabel";
			this.CreationFromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 13, true);
			this.CreationFromLabel.TabIndex = 0;
			this.CreationFromLabel.Text = "Job Creation Date From:";
			this.CreationFromLabel.UseMnemonic = false;
			// 
			// CreateButton
			// 
			this.CreateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateButton.IsCaptionOverridden = true;
			this.CreateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 306, true);
			this.CreateButton.Name = "CreateButton";
			this.CreateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 23, true);
			this.CreateButton.TabIndex = 18;
			this.CreateButton.Text = "&Create";
			this.CreateButton.ToolTipCaption = null;
			this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// CreationFromDateEdit
			// 
			this.CreationFromDateEdit.AllowDrop = true;
			this.CreationFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.CreationFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 24, true);
			this.CreationFromDateEdit.Name = "CreationFromDateEdit";
			this.CreationFromDateEdit.TabIndex = 1;
			// 
			// CreationToLabel
			// 
			this.CreationToLabel.AutoSize = true;
			this.CreationToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 24, true);
			this.CreationToLabel.Name = "CreationToLabel";
			this.CreationToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 13, true);
			this.CreationToLabel.TabIndex = 2;
			this.CreationToLabel.Text = "To:";
			this.CreationToLabel.UseMnemonic = false;
			// 
			// CreationToDateEdit
			// 
			this.CreationToDateEdit.AllowDrop = true;
			this.CreationToDateEdit.AutoCompleteMonthThreshold = 1;
			this.CreationToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 24, true);
			this.CreationToDateEdit.Name = "CreationToDateEdit";
			this.CreationToDateEdit.TabIndex = 3;
			// 
			// MessageTypeTextBox
			// 
			this.MessageTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.MessageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 45, true);
			this.MessageTypeTextBox.MaxLength = 3;
			this.MessageTypeTextBox.Name = "MessageTypeTextBox";
			this.MessageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.MessageTypeTextBox.TabIndex = 7;
			// 
			// MessageTypeLabel
			// 
			this.MessageTypeLabel.AutoSize = true;
			this.MessageTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 45, true);
			this.MessageTypeLabel.Name = "MessageTypeLabel";
			this.MessageTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 13, true);
			this.MessageTypeLabel.TabIndex = 6;
			this.MessageTypeLabel.Text = "Message Type:";
			this.MessageTypeLabel.UseMnemonic = false;
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 220, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(471, 81, true);
			this.MessageTextTextBox.TabIndex = 17;
			// 
			// MesageTextLabel
			// 
			this.MesageTextLabel.AutoSize = true;
			this.MesageTextLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 222, true);
			this.MesageTextLabel.Name = "MesageTextLabel";
			this.MesageTextLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 13, true);
			this.MesageTextLabel.TabIndex = 16;
			this.MesageTextLabel.Text = "Message Text:";
			this.MesageTextLabel.UseMnemonic = false;
			// 
			// UseJobBranchCheckBox
			// 
			this.UseJobBranchCheckBox.AutoSize = true;
			this.UseJobBranchCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.UseJobBranchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 67, true);
			this.UseJobBranchCheckBox.Name = "UseJobBranchCheckBox";
			this.UseJobBranchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 16, true);
			this.UseJobBranchCheckBox.TabIndex = 10;
			this.UseJobBranchCheckBox.Text = "Use Job Branch";
			this.UseJobBranchCheckBox.UseVisualStyleBackColor = true;
			// 
			// NoOfMsgPerJobCalcEdit
			// 
			this.NoOfMsgPerJobCalcEdit.AllowDrop = true;
			this.NoOfMsgPerJobCalcEdit.DecimalPlaces = 0;
			this.NoOfMsgPerJobCalcEdit.Decimals = 0;
			this.NoOfMsgPerJobCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 67, true);
			this.NoOfMsgPerJobCalcEdit.Name = "NoOfMsgPerJobCalcEdit";
			this.NoOfMsgPerJobCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.NoOfMsgPerJobCalcEdit.TabIndex = 9;
			this.NoOfMsgPerJobCalcEdit.Text = "1";
			this.NoOfMsgPerJobCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NoOfMsgPerJobCalcEdit.TrackDisposedAccess = true;
			// 
			// MessagePerJobLabel
			// 
			this.MessagePerJobLabel.AutoSize = true;
			this.MessagePerJobLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(35, 67, true);
			this.MessagePerJobLabel.Name = "MessagePerJobLabel";
			this.MessagePerJobLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.MessagePerJobLabel.TabIndex = 8;
			this.MessagePerJobLabel.Text = "No. Of Msg. Per Job:";
			this.MessagePerJobLabel.UseMnemonic = false;
			// 
			// InterchangeCW1ReferenceTextBox
			// 
			this.InterchangeCW1ReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.InterchangeCW1ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 17, true);
			this.InterchangeCW1ReferenceTextBox.MaxLength = 64;
			this.InterchangeCW1ReferenceTextBox.Name = "InterchangeCW1ReferenceTextBox";
			this.InterchangeCW1ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.InterchangeCW1ReferenceTextBox.TabIndex = 3;
			// 
			// InterchangeCW1ReferenceLabel
			// 
			this.InterchangeCW1ReferenceLabel.AutoSize = true;
			this.InterchangeCW1ReferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 19, true);
			this.InterchangeCW1ReferenceLabel.Name = "InterchangeCW1ReferenceLabel";
			this.InterchangeCW1ReferenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 13, true);
			this.InterchangeCW1ReferenceLabel.TabIndex = 2;
			this.InterchangeCW1ReferenceLabel.Text = "CW1 Reference:";
			this.InterchangeCW1ReferenceLabel.UseMnemonic = false;
			// 
			// InterchangeDetailsGroupBox
			// 
			this.InterchangeDetailsGroupBox.Controls.Add(this.InterchangeStatusTextBox);
			this.InterchangeDetailsGroupBox.Controls.Add(this.TransportTypeLabel);
			this.InterchangeDetailsGroupBox.Controls.Add(this.InterchangeStatusLabel);
			this.InterchangeDetailsGroupBox.Controls.Add(this.TransportTypeTextBox);
			this.InterchangeDetailsGroupBox.Controls.Add(this.InterchangeCustomsRerenceTextBox);
			this.InterchangeDetailsGroupBox.Controls.Add(this.InterchangeCW1ReferenceLabel);
			this.InterchangeDetailsGroupBox.Controls.Add(this.InterchangeCustomsRerenceLabel);
			this.InterchangeDetailsGroupBox.Controls.Add(this.InterchangeCW1ReferenceTextBox);
			this.InterchangeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 153, true);
			this.InterchangeDetailsGroupBox.Name = "InterchangeDetailsGroupBox";
			this.InterchangeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 63, true);
			this.InterchangeDetailsGroupBox.TabIndex = 15;
			this.InterchangeDetailsGroupBox.TabStop = false;
			this.InterchangeDetailsGroupBox.Text = "Interchange";
			// 
			// InterchangeStatusTextBox
			// 
			this.InterchangeStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.InterchangeStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 38, true);
			this.InterchangeStatusTextBox.MaxLength = 3;
			this.InterchangeStatusTextBox.Name = "InterchangeStatusTextBox";
			this.InterchangeStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.InterchangeStatusTextBox.TabIndex = 5;
			this.InterchangeStatusTextBox.Text = "QUE";
			// 
			// TransportTypeLabel
			// 
			this.TransportTypeLabel.AutoSize = true;
			this.TransportTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 38, true);
			this.TransportTypeLabel.Name = "TransportTypeLabel";
			this.TransportTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 13, true);
			this.TransportTypeLabel.TabIndex = 6;
			this.TransportTypeLabel.Text = "Transport Type:";
			this.TransportTypeLabel.UseMnemonic = false;
			// 
			// InterchangeStatusLabel
			// 
			this.InterchangeStatusLabel.AutoSize = true;
			this.InterchangeStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 38, true);
			this.InterchangeStatusLabel.Name = "InterchangeStatusLabel";
			this.InterchangeStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			this.InterchangeStatusLabel.TabIndex = 4;
			this.InterchangeStatusLabel.Text = "Status:";
			this.InterchangeStatusLabel.UseMnemonic = false;
			// 
			// TransportTypeTextBox
			// 
			this.TransportTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.TransportTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 38, true);
			this.TransportTypeTextBox.MaxLength = 3;
			this.TransportTypeTextBox.Name = "TransportTypeTextBox";
			this.TransportTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.TransportTypeTextBox.TabIndex = 7;
			this.TransportTypeTextBox.Text = "XTT";
			// 
			// InterchangeCustomsRerenceTextBox
			// 
			this.InterchangeCustomsRerenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.InterchangeCustomsRerenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 17, true);
			this.InterchangeCustomsRerenceTextBox.MaxLength = 64;
			this.InterchangeCustomsRerenceTextBox.Name = "InterchangeCustomsRerenceTextBox";
			this.InterchangeCustomsRerenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.InterchangeCustomsRerenceTextBox.TabIndex = 1;
			// 
			// InterchangeCustomsRerenceLabel
			// 
			this.InterchangeCustomsRerenceLabel.AutoSize = true;
			this.InterchangeCustomsRerenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(37, 19, true);
			this.InterchangeCustomsRerenceLabel.Name = "InterchangeCustomsRerenceLabel";
			this.InterchangeCustomsRerenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 13, true);
			this.InterchangeCustomsRerenceLabel.TabIndex = 0;
			this.InterchangeCustomsRerenceLabel.Text = "Customs Reference:";
			this.InterchangeCustomsRerenceLabel.UseMnemonic = false;
			// 
			// CreateEDIMessageGroupBox
			// 
			this.CreateEDIMessageGroupBox.Controls.Add(this.CreateMessageAndInterchangePanel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.CreateIncomingAndOutgoingPanel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.MessageStatusLabel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.MessageStatusTextBox);
			this.CreateEDIMessageGroupBox.Controls.Add(this.InterchangeDetailsGroupBox);
			this.CreateEDIMessageGroupBox.Controls.Add(this.MessagePerJobLabel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.NoOfMsgPerJobCalcEdit);
			this.CreateEDIMessageGroupBox.Controls.Add(this.UseJobBranchCheckBox);
			this.CreateEDIMessageGroupBox.Controls.Add(this.MesageTextLabel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.MessageTextTextBox);
			this.CreateEDIMessageGroupBox.Controls.Add(this.MessageTypeLabel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.MessageTypeTextBox);
			this.CreateEDIMessageGroupBox.Controls.Add(this.CreationToDateEdit);
			this.CreateEDIMessageGroupBox.Controls.Add(this.CreationToLabel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.CreationFromDateEdit);
			this.CreateEDIMessageGroupBox.Controls.Add(this.CreateButton);
			this.CreateEDIMessageGroupBox.Controls.Add(this.CreationFromLabel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.ApplicationCodeLabel);
			this.CreateEDIMessageGroupBox.Controls.Add(this.ApplicationCodeTextBox);
			this.CreateEDIMessageGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreateEDIMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CreateEDIMessageGroupBox.Name = "CreateEDIMessageGroupBox";
			this.CreateEDIMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 336, true);
			this.CreateEDIMessageGroupBox.TabIndex = 0;
			this.CreateEDIMessageGroupBox.TabStop = false;
			this.CreateEDIMessageGroupBox.Text = "Create EDIMessage";
			// 
			// CreateMessageAndInterchangePanel
			// 
			this.CreateMessageAndInterchangePanel.Controls.Add(this.CreateBothMessageAndInterchangeRadioButton);
			this.CreateMessageAndInterchangePanel.Controls.Add(this.CreateInterchangeRadioButton);
			this.CreateMessageAndInterchangePanel.Controls.Add(this.CreateMessageRadioButton);
			this.CreateMessageAndInterchangePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 107, true);
			this.CreateMessageAndInterchangePanel.Name = "CreateMessageAndInterchangePanel";
			this.CreateMessageAndInterchangePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 23, true);
			this.CreateMessageAndInterchangePanel.TabIndex = 12;
			// 
			// CreateBothMessageAndInterchangeRadioButton
			// 
			this.CreateBothMessageAndInterchangeRadioButton.AutoSize = true;
			this.CreateBothMessageAndInterchangeRadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CreateBothMessageAndInterchangeRadioButton.Checked = true;
			this.CreateBothMessageAndInterchangeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 4, true);
			this.CreateBothMessageAndInterchangeRadioButton.Name = "CreateBothMessageAndInterchangeRadioButton";
			this.CreateBothMessageAndInterchangeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.CreateBothMessageAndInterchangeRadioButton.TabIndex = 2;
			this.CreateBothMessageAndInterchangeRadioButton.TabStop = true;
			this.CreateBothMessageAndInterchangeRadioButton.Text = "Create Both";
			this.CreateBothMessageAndInterchangeRadioButton.UseVisualStyleBackColor = true;
			// 
			// CreateInterchangeRadioButton
			// 
			this.CreateInterchangeRadioButton.AutoSize = true;
			this.CreateInterchangeRadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CreateInterchangeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(268, 2, true);
			this.CreateInterchangeRadioButton.Name = "CreateInterchangeRadioButton";
			this.CreateInterchangeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 16, true);
			this.CreateInterchangeRadioButton.TabIndex = 1;
			this.CreateInterchangeRadioButton.Text = "Create Interchange";
			this.CreateInterchangeRadioButton.UseVisualStyleBackColor = true;
			// 
			// CreateMessageRadioButton
			// 
			this.CreateMessageRadioButton.AutoSize = true;
			this.CreateMessageRadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CreateMessageRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 4, true);
			this.CreateMessageRadioButton.Name = "CreateMessageRadioButton";
			this.CreateMessageRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 16, true);
			this.CreateMessageRadioButton.TabIndex = 0;
			this.CreateMessageRadioButton.Text = "Create Message";
			this.CreateMessageRadioButton.UseVisualStyleBackColor = true;
			// 
			// CreateIncomingAndOutgoingPanel
			// 
			this.CreateIncomingAndOutgoingPanel.Controls.Add(this.CreateBothOutgoingAndIncomingRadioButton);
			this.CreateIncomingAndOutgoingPanel.Controls.Add(this.CreateOutgoingRadioButton);
			this.CreateIncomingAndOutgoingPanel.Controls.Add(this.CreateIncomingRadioButton);
			this.CreateIncomingAndOutgoingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 86, true);
			this.CreateIncomingAndOutgoingPanel.Name = "CreateIncomingAndOutgoingPanel";
			this.CreateIncomingAndOutgoingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 24, true);
			this.CreateIncomingAndOutgoingPanel.TabIndex = 11;
			// 
			// CreateBothOutgoingAndIncomingRadioButton
			// 
			this.CreateBothOutgoingAndIncomingRadioButton.AutoSize = true;
			this.CreateBothOutgoingAndIncomingRadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CreateBothOutgoingAndIncomingRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 3, true);
			this.CreateBothOutgoingAndIncomingRadioButton.Name = "CreateBothOutgoingAndIncomingRadioButton";
			this.CreateBothOutgoingAndIncomingRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.CreateBothOutgoingAndIncomingRadioButton.TabIndex = 2;
			this.CreateBothOutgoingAndIncomingRadioButton.Text = "Create Both";
			this.CreateBothOutgoingAndIncomingRadioButton.UseVisualStyleBackColor = true;
			// 
			// CreateOutgoingRadioButton
			// 
			this.CreateOutgoingRadioButton.AutoSize = true;
			this.CreateOutgoingRadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CreateOutgoingRadioButton.Checked = true;
			this.CreateOutgoingRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 3, true);
			this.CreateOutgoingRadioButton.Name = "CreateOutgoingRadioButton";
			this.CreateOutgoingRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 16, true);
			this.CreateOutgoingRadioButton.TabIndex = 1;
			this.CreateOutgoingRadioButton.TabStop = true;
			this.CreateOutgoingRadioButton.Text = "Create Outgoing";
			this.CreateOutgoingRadioButton.UseVisualStyleBackColor = true;
			// 
			// CreateIncomingRadioButton
			// 
			this.CreateIncomingRadioButton.AutoSize = true;
			this.CreateIncomingRadioButton.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.CreateIncomingRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 3, true);
			this.CreateIncomingRadioButton.Name = "CreateIncomingRadioButton";
			this.CreateIncomingRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 16, true);
			this.CreateIncomingRadioButton.TabIndex = 0;
			this.CreateIncomingRadioButton.Text = "Create Incoming";
			this.CreateIncomingRadioButton.UseVisualStyleBackColor = true;
			// 
			// MessageStatusLabel
			// 
			this.MessageStatusLabel.AutoSize = true;
			this.MessageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 133, true);
			this.MessageStatusLabel.Name = "MessageStatusLabel";
			this.MessageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.MessageStatusLabel.TabIndex = 13;
			this.MessageStatusLabel.Text = "Message Status:";
			this.MessageStatusLabel.UseMnemonic = false;
			// 
			// MessageStatusTextBox
			// 
			this.MessageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 133, true);
			this.MessageStatusTextBox.MaxLength = 3;
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.MessageStatusTextBox.TabIndex = 14;
			this.MessageStatusTextBox.Text = "QUE";
			// 
			// TestRigEDIMessageCreationUserControl
			// 
			this.Controls.Add(this.CreateEDIMessageGroupBox);
			this.Name = "TestRigEDIMessageCreationUserControl";
			this.Size = new System.Drawing.Size(926, 504);
			this.CreationFromDateEdit.ResumeLayout(true);
			this.CreationFromDateEdit.PerformLayout();
			this.CreationToDateEdit.ResumeLayout(true);
			this.CreationToDateEdit.PerformLayout();
			this.InterchangeDetailsGroupBox.ResumeLayout(false);
			this.InterchangeDetailsGroupBox.PerformLayout();
			this.CreateEDIMessageGroupBox.ResumeLayout(false);
			this.CreateEDIMessageGroupBox.PerformLayout();
			this.CreateMessageAndInterchangePanel.ResumeLayout(false);
			this.CreateMessageAndInterchangePanel.PerformLayout();
			this.CreateIncomingAndOutgoingPanel.ResumeLayout(false);
			this.CreateIncomingAndOutgoingPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		private CargoWise.Windows.UI.KTextBox ApplicationCodeTextBox;
		private CargoWise.Windows.UI.KLabel ApplicationCodeLabel;
		private CargoWise.Windows.UI.KLabel CreationFromLabel;
		private ZArchitecture.GUI.ZButton CreateButton;
		private ZArchitecture.GUI.ZDateEdit CreationFromDateEdit;
		private CargoWise.Windows.UI.KLabel CreationToLabel;
		private ZArchitecture.GUI.ZDateEdit CreationToDateEdit;
		private CargoWise.Windows.UI.KTextBox MessageTypeTextBox;
		private CargoWise.Windows.UI.KLabel MessageTypeLabel;
		private CargoWise.Windows.UI.KTextBox MessageTextTextBox;
		private CargoWise.Windows.UI.KLabel MesageTextLabel;
		private CargoWise.Windows.UI.KCheckBox UseJobBranchCheckBox;
		private ZArchitecture.ZCalcEdit NoOfMsgPerJobCalcEdit;
		private CargoWise.Windows.UI.KLabel MessagePerJobLabel;
		private CargoWise.Windows.UI.KTextBox InterchangeCW1ReferenceTextBox;
		private CargoWise.Windows.UI.KLabel InterchangeCW1ReferenceLabel;
		private CargoWise.Windows.UI.KGroupBox InterchangeDetailsGroupBox;
		private CargoWise.Windows.UI.KTextBox InterchangeStatusTextBox;
		private CargoWise.Windows.UI.KLabel TransportTypeLabel;
		private CargoWise.Windows.UI.KLabel InterchangeStatusLabel;
		private CargoWise.Windows.UI.KTextBox TransportTypeTextBox;
		private CargoWise.Windows.UI.KTextBox InterchangeCustomsRerenceTextBox;
		private CargoWise.Windows.UI.KLabel InterchangeCustomsRerenceLabel;
		private CargoWise.Windows.UI.KGroupBox CreateEDIMessageGroupBox;
		private CargoWise.Windows.UI.KLabel MessageStatusLabel;
		private CargoWise.Windows.UI.KTextBox MessageStatusTextBox;
		private CargoWise.Windows.UI.KPanel CreateIncomingAndOutgoingPanel;
		private CargoWise.Windows.UI.KRadioButton CreateBothOutgoingAndIncomingRadioButton;
		private CargoWise.Windows.UI.KRadioButton CreateOutgoingRadioButton;
		private CargoWise.Windows.UI.KRadioButton CreateIncomingRadioButton;
		private CargoWise.Windows.UI.KPanel CreateMessageAndInterchangePanel;
		private CargoWise.Windows.UI.KRadioButton CreateBothMessageAndInterchangeRadioButton;
		private CargoWise.Windows.UI.KRadioButton CreateInterchangeRadioButton;
		private CargoWise.Windows.UI.KRadioButton CreateMessageRadioButton;
	}
}
