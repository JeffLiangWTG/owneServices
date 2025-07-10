
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.SADH
{
	partial class SADHEntryForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

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
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				if (FormDataManager != null)
				{
					FormDataManager.FormData.D1_ModeOfTransportAtTheBorderInfo.ValueChanged -= D1_ModeTransportAtBorderInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CopyCountryDestinationVerticalLabel = new CargoWise.Windows.UI.VerticalLabel();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FormBottomBorderLabel = new CargoWise.Windows.UI.KLabel();
			this.FormRightBorderLabel = new CargoWise.Windows.UI.KLabel();
			this.FormLeftBorderLabel = new CargoWise.Windows.UI.KLabel();
			this.Section54Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section54SignatureAndNameLabel = new CargoWise.Windows.UI.KLabel();
			this.Section54DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.SectionJPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SectionJDescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section53Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section53DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section52Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section52CodePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section52CodeLabel = new CargoWise.Windows.UI.KLabel();
			this.Section52NotValidForLabel = new CargoWise.Windows.UI.KLabel();
			this.Section52DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section51fPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section51ePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section51dPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section51cPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section51bPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section51aPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section51Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section51NumberLabel = new CargoWise.Windows.UI.KLabel();
			this.Section51DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.SectionCPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SectionCDescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section50Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section50PlaceAndDateLabel = new CargoWise.Windows.UI.KLabel();
			this.Section50RepresentedByLabel = new CargoWise.Windows.UI.KLabel();
			this.Section50SignatureLabel = new CargoWise.Windows.UI.KLabel();
			this.Section50NoLabel = new CargoWise.Windows.UI.KLabel();
			this.Section50DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.SectionBPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SectionBDescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section49Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section49DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section48Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section48DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section47bPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section47Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section47NumberLabel = new CargoWise.Windows.UI.KLabel();
			this.Section47DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section46Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section46DescriptionLAbel = new CargoWise.Windows.UI.KLabel();
			this.SectionA1Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SectionA1DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section45Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section45DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section44Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section44NumberLabel = new CargoWise.Windows.UI.KLabel();
			this.Section44DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Setion42Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section42TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section42CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Section42DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section41Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section41TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section41CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Section41DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section43Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section43DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section40Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section40DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section39Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section39DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section38Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section38DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section37Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section37DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section35Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section35CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.Section35DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section36Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section36DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section34Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section34CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Section34DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section33Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section33DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section31Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section31NumberLabel = new CargoWise.Windows.UI.KLabel();
			this.Section31DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section31bPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section31bQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.Section31bQuantityLabel = new CargoWise.Windows.UI.KLabel();
			this.Section31bMarksNumbersLabel = new CargoWise.Windows.UI.KLabel();
			this.Section31bMarksNumbersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section31bDescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section32Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section32DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section31bDescriptionTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section31bDescriptionPanel = new CargoWise.Windows.UI.KLabel();
			this.LargeSixLabel2 = new CargoWise.Windows.UI.KLabel();
			this.Section30Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section30DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section28Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section28DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section26Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section26Description2Label = new CargoWise.Windows.UI.KLabel();
			this.Section26DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section25Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section25DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Section25Description2Label = new CargoWise.Windows.UI.KLabel();
			this.Section25DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section27Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section27CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Section27DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section24Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section24DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section23Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section23ExchangeRateTextbox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Section23DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section22Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section22TotalAmountInvoicedCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.Section22DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section21Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DepartureTransportIDBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartureTransportIDLabel = new CargoWise.Windows.UI.KLabel();
			this.Section21FlightDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.Section21FlightDateLabel = new CargoWise.Windows.UI.KLabel();
			this.Section21FlightNoLabel = new CargoWise.Windows.UI.KLabel();
			this.Section21FlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section21DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section21VesselLabel = new CargoWise.Windows.UI.KLabel();
			this.Section21VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Section20Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section20DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.Section20DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section19Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section19DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section18Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Box18TransportIDBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Box18TransportIDLabel = new CargoWise.Windows.UI.KLabel();
			this.Section18DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section14Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section14TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section14DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section17bPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section17bTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section17bLabel = new CargoWise.Windows.UI.KLabel();
			this.Section15bPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section15bTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section15bLabel = new CargoWise.Windows.UI.KLabel();
			this.Section17Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section17CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Section17DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section16Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section16CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Section16DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section15Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section15CodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Section15DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.CopyCountryDestinationBorderLabel = new CargoWise.Windows.UI.KLabel();
			this.LargeSixLabel = new CargoWise.Windows.UI.KLabel();
			this.Section13Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section13DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section12Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section12DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section11Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section11Description2Label = new CargoWise.Windows.UI.KLabel();
			this.Section11DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section10Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section10Description2Label = new CargoWise.Windows.UI.KLabel();
			this.Section10DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section9Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section9NoLabel = new CargoWise.Windows.UI.KLabel();
			this.Section9DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section8Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section8Textbox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section8OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.Section8ZDescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section7Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section7DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section6Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section6CalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.Section6DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section5Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section5DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section4Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section4DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section3Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section3DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section2Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section2Textbox = new Enterprise.ZArchitecture.ZTextBox();
			this.Section2OrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.Section2DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.Section1Panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Section1DescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.SectionAPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SectionALeftBorder = new CargoWise.Windows.UI.KLabel();
			this.SectionADescriptionLabel = new CargoWise.Windows.UI.KLabel();
			this.EuropeanCommunityLabel = new CargoWise.Windows.UI.KLabel();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.Section54Panel.SuspendLayout();
			this.SectionJPanel.SuspendLayout();
			this.Section53Panel.SuspendLayout();
			this.Section52Panel.SuspendLayout();
			this.Section52CodePanel.SuspendLayout();
			this.Section51Panel.SuspendLayout();
			this.SectionCPanel.SuspendLayout();
			this.Section50Panel.SuspendLayout();
			this.SectionBPanel.SuspendLayout();
			this.Section49Panel.SuspendLayout();
			this.Section48Panel.SuspendLayout();
			this.Section47Panel.SuspendLayout();
			this.Section46Panel.SuspendLayout();
			this.SectionA1Panel.SuspendLayout();
			this.Section45Panel.SuspendLayout();
			this.Section44Panel.SuspendLayout();
			this.Setion42Panel.SuspendLayout();
			this.Section41Panel.SuspendLayout();
			this.Section43Panel.SuspendLayout();
			this.Section40Panel.SuspendLayout();
			this.Section39Panel.SuspendLayout();
			this.Section38Panel.SuspendLayout();
			this.Section37Panel.SuspendLayout();
			this.Section35Panel.SuspendLayout();
			this.Section36Panel.SuspendLayout();
			this.Section34Panel.SuspendLayout();
			this.Section33Panel.SuspendLayout();
			this.Section31Panel.SuspendLayout();
			this.Section31bPanel.SuspendLayout();
			this.Section32Panel.SuspendLayout();
			this.Section30Panel.SuspendLayout();
			this.Section28Panel.SuspendLayout();
			this.Section26Panel.SuspendLayout();
			this.Section25Panel.SuspendLayout();
			this.Section27Panel.SuspendLayout();
			this.Section24Panel.SuspendLayout();
			this.Section23Panel.SuspendLayout();
			this.Section22Panel.SuspendLayout();
			this.Section21Panel.SuspendLayout();
			this.Section20Panel.SuspendLayout();
			this.Section19Panel.SuspendLayout();
			this.Section18Panel.SuspendLayout();
			this.Section14Panel.SuspendLayout();
			this.Section17bPanel.SuspendLayout();
			this.Section15bPanel.SuspendLayout();
			this.Section17Panel.SuspendLayout();
			this.Section16Panel.SuspendLayout();
			this.Section15Panel.SuspendLayout();
			this.Section13Panel.SuspendLayout();
			this.Section12Panel.SuspendLayout();
			this.Section11Panel.SuspendLayout();
			this.Section10Panel.SuspendLayout();
			this.Section9Panel.SuspendLayout();
			this.Section8Panel.SuspendLayout();
			this.Section7Panel.SuspendLayout();
			this.Section6Panel.SuspendLayout();
			this.Section5Panel.SuspendLayout();
			this.Section4Panel.SuspendLayout();
			this.Section3Panel.SuspendLayout();
			this.Section2Panel.SuspendLayout();
			this.Section1Panel.SuspendLayout();
			this.SectionAPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 736, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MainPanel
			// 
			this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
							| System.Windows.Forms.AnchorStyles.Left)
							| System.Windows.Forms.AnchorStyles.Right)));
			this.MainPanel.AutoScroll = true;
			this.MainPanel.BackColor = System.Drawing.SystemColors.Window;
			this.MainPanel.Controls.Add(this.CopyCountryDestinationVerticalLabel);
			this.MainPanel.Controls.Add(this.MessageTypeDropEdit);
			this.MainPanel.Controls.Add(this.FormBottomBorderLabel);
			this.MainPanel.Controls.Add(this.FormRightBorderLabel);
			this.MainPanel.Controls.Add(this.FormLeftBorderLabel);
			this.MainPanel.Controls.Add(this.Section54Panel);
			this.MainPanel.Controls.Add(this.SectionJPanel);
			this.MainPanel.Controls.Add(this.Section53Panel);
			this.MainPanel.Controls.Add(this.Section52Panel);
			this.MainPanel.Controls.Add(this.Section51fPanel);
			this.MainPanel.Controls.Add(this.Section51ePanel);
			this.MainPanel.Controls.Add(this.Section51dPanel);
			this.MainPanel.Controls.Add(this.Section51cPanel);
			this.MainPanel.Controls.Add(this.Section51bPanel);
			this.MainPanel.Controls.Add(this.Section51aPanel);
			this.MainPanel.Controls.Add(this.Section51Panel);
			this.MainPanel.Controls.Add(this.SectionCPanel);
			this.MainPanel.Controls.Add(this.Section50Panel);
			this.MainPanel.Controls.Add(this.SectionBPanel);
			this.MainPanel.Controls.Add(this.Section49Panel);
			this.MainPanel.Controls.Add(this.Section48Panel);
			this.MainPanel.Controls.Add(this.Section47bPanel);
			this.MainPanel.Controls.Add(this.Section47Panel);
			this.MainPanel.Controls.Add(this.Section46Panel);
			this.MainPanel.Controls.Add(this.SectionA1Panel);
			this.MainPanel.Controls.Add(this.Section45Panel);
			this.MainPanel.Controls.Add(this.Section44Panel);
			this.MainPanel.Controls.Add(this.Setion42Panel);
			this.MainPanel.Controls.Add(this.Section41Panel);
			this.MainPanel.Controls.Add(this.Section43Panel);
			this.MainPanel.Controls.Add(this.Section40Panel);
			this.MainPanel.Controls.Add(this.Section39Panel);
			this.MainPanel.Controls.Add(this.Section38Panel);
			this.MainPanel.Controls.Add(this.Section37Panel);
			this.MainPanel.Controls.Add(this.Section35Panel);
			this.MainPanel.Controls.Add(this.Section36Panel);
			this.MainPanel.Controls.Add(this.Section34Panel);
			this.MainPanel.Controls.Add(this.Section33Panel);
			this.MainPanel.Controls.Add(this.Section31Panel);
			this.MainPanel.Controls.Add(this.Section31bPanel);
			this.MainPanel.Controls.Add(this.LargeSixLabel2);
			this.MainPanel.Controls.Add(this.Section30Panel);
			this.MainPanel.Controls.Add(this.Section28Panel);
			this.MainPanel.Controls.Add(this.Section26Panel);
			this.MainPanel.Controls.Add(this.Section25Panel);
			this.MainPanel.Controls.Add(this.Section27Panel);
			this.MainPanel.Controls.Add(this.Section24Panel);
			this.MainPanel.Controls.Add(this.Section23Panel);
			this.MainPanel.Controls.Add(this.Section22Panel);
			this.MainPanel.Controls.Add(this.Section21Panel);
			this.MainPanel.Controls.Add(this.Section20Panel);
			this.MainPanel.Controls.Add(this.Section19Panel);
			this.MainPanel.Controls.Add(this.Section18Panel);
			this.MainPanel.Controls.Add(this.Section14Panel);
			this.MainPanel.Controls.Add(this.Section17bPanel);
			this.MainPanel.Controls.Add(this.Section15bPanel);
			this.MainPanel.Controls.Add(this.Section17Panel);
			this.MainPanel.Controls.Add(this.Section16Panel);
			this.MainPanel.Controls.Add(this.Section15Panel);
			this.MainPanel.Controls.Add(this.CopyCountryDestinationBorderLabel);
			this.MainPanel.Controls.Add(this.LargeSixLabel);
			this.MainPanel.Controls.Add(this.Section13Panel);
			this.MainPanel.Controls.Add(this.Section12Panel);
			this.MainPanel.Controls.Add(this.Section11Panel);
			this.MainPanel.Controls.Add(this.Section10Panel);
			this.MainPanel.Controls.Add(this.Section9Panel);
			this.MainPanel.Controls.Add(this.Section8Panel);
			this.MainPanel.Controls.Add(this.Section7Panel);
			this.MainPanel.Controls.Add(this.Section6Panel);
			this.MainPanel.Controls.Add(this.Section5Panel);
			this.MainPanel.Controls.Add(this.Section4Panel);
			this.MainPanel.Controls.Add(this.Section3Panel);
			this.MainPanel.Controls.Add(this.Section2Panel);
			this.MainPanel.Controls.Add(this.Section1Panel);
			this.MainPanel.Controls.Add(this.SectionAPanel);
			this.MainPanel.Controls.Add(this.EuropeanCommunityLabel);
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 685, true);
			this.MainPanel.TabIndex = 0;
			// 
			// CopyCountryDestinationVerticalLabel
			// 
			this.CopyCountryDestinationVerticalLabel.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold);
			this.CopyCountryDestinationVerticalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 146, true);
			this.CopyCountryDestinationVerticalLabel.Name = "CopyCountryDestinationVerticalLabel";
			this.CopyCountryDestinationVerticalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 239, true);
			this.CopyCountryDestinationVerticalLabel.TabIndex = 4;
			this.CopyCountryDestinationVerticalLabel.Text = Res.GetString("F97F99AB-F56A-4706-9E72-0A6E26A9CD2A", "Copy the country of destination");
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.BindTo = "D1_MessageType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_MessageTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_MessageType)));
			this.MessageTypeDropEdit.BindToList = "Lookups+MessageTypeList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.MessageTypeList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTypeDropEdit, false);
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 5, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 3;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.MessageTypeDropEdit.TabIndex = 1;
			// 
			// FormBottomBorderLabel
			// 
			this.FormBottomBorderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1229, true);
			this.FormBottomBorderLabel.Name = "FormBottomBorderLabel";
			this.FormBottomBorderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 1, true);
			this.FormBottomBorderLabel.TabIndex = 75;
			// 
			// FormRightBorderLabel
			// 
			this.FormRightBorderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(820, 93, true);
			this.FormRightBorderLabel.Name = "FormRightBorderLabel";
			this.FormRightBorderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 1145, true);
			this.FormRightBorderLabel.TabIndex = 82;
			// 
			// FormLeftBorderLabel
			// 
			this.FormLeftBorderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 475, true);
			this.FormLeftBorderLabel.Name = "FormLeftBorderLabel";
			this.FormLeftBorderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 762, true);
			this.FormLeftBorderLabel.TabIndex = 6;
			// 
			// Section54Panel
			// 
			this.Section54Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section54Panel.Controls.Add(this.Section54SignatureAndNameLabel);
			this.Section54Panel.Controls.Add(this.Section54DescriptionLabel);
			this.Section54Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 1109, true);
			this.Section54Panel.Name = "Section54Panel";
			this.Section54Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 121, true);
			this.Section54Panel.TabIndex = 74;
			// 
			// Section54SignatureAndNameLabel
			// 
			this.Section54SignatureAndNameLabel.AutoSize = true;
			this.Section54SignatureAndNameLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section54SignatureAndNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 42, true);
			this.Section54SignatureAndNameLabel.Name = "Section54SignatureAndNameLabel";
			this.Section54SignatureAndNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 10, true);
			this.Section54SignatureAndNameLabel.TabIndex = 1;
			this.Section54SignatureAndNameLabel.Text = Res.GetString("31445A99-209E-4361-8577-CC17CF49B51D", "Signature and name of declarant/representative:");
			// 
			// Section54DescriptionLabel
			// 
			this.Section54DescriptionLabel.AutoSize = true;
			this.Section54DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section54DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section54DescriptionLabel.Name = "Section54DescriptionLabel";
			this.Section54DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 10, true);
			this.Section54DescriptionLabel.TabIndex = 0;
			this.Section54DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("15B1AD75-A47D-45D2-8C03-DB81A069B3A9", "54 Place and date:");
			// 
			// SectionJPanel
			// 
			this.SectionJPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SectionJPanel.Controls.Add(this.SectionJDescriptionLabel);
			this.SectionJPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 1109, true);
			this.SectionJPanel.Name = "SectionJPanel";
			this.SectionJPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 121, true);
			this.SectionJPanel.TabIndex = 73;
			// 
			// SectionJDescriptionLabel
			// 
			this.SectionJDescriptionLabel.AutoSize = true;
			this.SectionJDescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SectionJDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.SectionJDescriptionLabel.Name = "SectionJDescriptionLabel";
			this.SectionJDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 10, true);
			this.SectionJDescriptionLabel.TabIndex = 0;
			this.SectionJDescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("F31D6B8A-A0EF-4EEB-8087-B8E3D259A0DF", "J  CONTROL BY OFFICE OF DESTINATION");
			// 
			// Section53Panel
			// 
			this.Section53Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section53Panel.Controls.Add(this.Section53DescriptionLabel);
			this.Section53Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 1069, true);
			this.Section53Panel.Name = "Section53Panel";
			this.Section53Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 41, true);
			this.Section53Panel.TabIndex = 72;
			// 
			// Section53DescriptionLabel
			// 
			this.Section53DescriptionLabel.AutoSize = true;
			this.Section53DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section53DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section53DescriptionLabel.Name = "Section53DescriptionLabel";
			this.Section53DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 10, true);
			this.Section53DescriptionLabel.TabIndex = 0;
			this.Section53DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("B736F91E-C19F-4599-9F16-C79862A215B9", "53 Office of destination (and country)");
			// 
			// Section52Panel
			// 
			this.Section52Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section52Panel.Controls.Add(this.Section52CodePanel);
			this.Section52Panel.Controls.Add(this.Section52NotValidForLabel);
			this.Section52Panel.Controls.Add(this.Section52DescriptionLabel);
			this.Section52Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 1069, true);
			this.Section52Panel.Name = "Section52Panel";
			this.Section52Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 41, true);
			this.Section52Panel.TabIndex = 71;
			// 
			// Section52CodePanel
			// 
			this.Section52CodePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section52CodePanel.Controls.Add(this.Section52CodeLabel);
			this.Section52CodePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, -1, true);
			this.Section52CodePanel.Name = "Section52CodePanel";
			this.Section52CodePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 41, true);
			this.Section52CodePanel.TabIndex = 2;
			// 
			// Section52CodeLabel
			// 
			this.Section52CodeLabel.AutoSize = true;
			this.Section52CodeLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section52CodeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section52CodeLabel.Name = "Section52CodeLabel";
			this.Section52CodeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 10, true);
			this.Section52CodeLabel.TabIndex = 0;
			this.Section52CodeLabel.Text = Enterprise.Customs.GUI.Res.GetString("8A9C80FB-8268-4156-96D9-7F29D7761C88", "Code");
			// 
			// Section52NotValidForLabel
			// 
			this.Section52NotValidForLabel.AutoSize = true;
			this.Section52NotValidForLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section52NotValidForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 22, true);
			this.Section52NotValidForLabel.Name = "Section52NotValidForLabel";
			this.Section52NotValidForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 10, true);
			this.Section52NotValidForLabel.TabIndex = 1;
			this.Section52NotValidForLabel.Text = Enterprise.Customs.GUI.Res.GetString("9111E39B-FE51-4543-9E45-40BEE4F1E3EA", "not valid for");
			// 
			// Section52DescriptionLabel
			// 
			this.Section52DescriptionLabel.AutoSize = true;
			this.Section52DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section52DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section52DescriptionLabel.Name = "Section52DescriptionLabel";
			this.Section52DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 10, true);
			this.Section52DescriptionLabel.TabIndex = 0;
			this.Section52DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("0A0B194F-3CCF-435C-B3BF-535400C2356A", "52 Guarantee");
			// 
			// Section51fPanel
			// 
			this.Section51fPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section51fPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(686, 1046, true);
			this.Section51fPanel.Name = "Section51fPanel";
			this.Section51fPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 24, true);
			this.Section51fPanel.TabIndex = 70;
			// 
			// Section51ePanel
			// 
			this.Section51ePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section51ePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 1046, true);
			this.Section51ePanel.Name = "Section51ePanel";
			this.Section51ePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 24, true);
			this.Section51ePanel.TabIndex = 69;
			// 
			// Section51dPanel
			// 
			this.Section51dPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section51dPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 1046, true);
			this.Section51dPanel.Name = "Section51dPanel";
			this.Section51dPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 24, true);
			this.Section51dPanel.TabIndex = 68;
			// 
			// Section51cPanel
			// 
			this.Section51cPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section51cPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 1046, true);
			this.Section51cPanel.Name = "Section51cPanel";
			this.Section51cPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 24, true);
			this.Section51cPanel.TabIndex = 67;
			// 
			// Section51bPanel
			// 
			this.Section51bPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section51bPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(201, 1046, true);
			this.Section51bPanel.Name = "Section51bPanel";
			this.Section51bPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 24, true);
			this.Section51bPanel.TabIndex = 66;
			// 
			// Section51aPanel
			// 
			this.Section51aPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section51aPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 1046, true);
			this.Section51aPanel.Name = "Section51aPanel";
			this.Section51aPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 24, true);
			this.Section51aPanel.TabIndex = 65;
			// 
			// Section51Panel
			// 
			this.Section51Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section51Panel.Controls.Add(this.Section51NumberLabel);
			this.Section51Panel.Controls.Add(this.Section51DescriptionLabel);
			this.Section51Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 1004, true);
			this.Section51Panel.Name = "Section51Panel";
			this.Section51Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 66, true);
			this.Section51Panel.TabIndex = 64;
			// 
			// Section51NumberLabel
			// 
			this.Section51NumberLabel.AutoSize = true;
			this.Section51NumberLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section51NumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section51NumberLabel.Name = "Section51NumberLabel";
			this.Section51NumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 10, true);
			this.Section51NumberLabel.TabIndex = 1;
			this.Section51NumberLabel.Text = Enterprise.Customs.GUI.Res.GetString("4DDC68D9-2EC5-486D-B276-5EAD0330F338", "51");
			// 
			// Section51DescriptionLabel
			// 
			this.Section51DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section51DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 3, true);
			this.Section51DescriptionLabel.Name = "Section51DescriptionLabel";
			this.Section51DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 58, true);
			this.Section51DescriptionLabel.TabIndex = 0;
			this.Section51DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("A974BBDC-807D-4511-AE71-B7424E3C1C80", "Intended offices of transit (and country)");
			// 
			// SectionCPanel
			// 
			this.SectionCPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SectionCPanel.Controls.Add(this.SectionCDescriptionLabel);
			this.SectionCPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(553, 926, true);
			this.SectionCPanel.Name = "SectionCPanel";
			this.SectionCPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 121, true);
			this.SectionCPanel.TabIndex = 63;
			// 
			// SectionCDescriptionLabel
			// 
			this.SectionCDescriptionLabel.AutoSize = true;
			this.SectionCDescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SectionCDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.SectionCDescriptionLabel.Name = "SectionCDescriptionLabel";
			this.SectionCDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 10, true);
			this.SectionCDescriptionLabel.TabIndex = 0;
			this.SectionCDescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("A4EA5A78-4919-4D5A-AEA0-F315D6860163", "C  OFFICE OF DEPARTURE");
			// 
			// Section50Panel
			// 
			this.Section50Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section50Panel.Controls.Add(this.Section50PlaceAndDateLabel);
			this.Section50Panel.Controls.Add(this.Section50RepresentedByLabel);
			this.Section50Panel.Controls.Add(this.Section50SignatureLabel);
			this.Section50Panel.Controls.Add(this.Section50NoLabel);
			this.Section50Panel.Controls.Add(this.Section50DescriptionLabel);
			this.Section50Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 926, true);
			this.Section50Panel.Name = "Section50Panel";
			this.Section50Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(471, 121, true);
			this.Section50Panel.TabIndex = 62;
			// 
			// Section50PlaceAndDateLabel
			// 
			this.Section50PlaceAndDateLabel.AutoSize = true;
			this.Section50PlaceAndDateLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section50PlaceAndDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 102, true);
			this.Section50PlaceAndDateLabel.Name = "Section50PlaceAndDateLabel";
			this.Section50PlaceAndDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 10, true);
			this.Section50PlaceAndDateLabel.TabIndex = 4;
			this.Section50PlaceAndDateLabel.Text = Enterprise.Customs.GUI.Res.GetString("AD1A07C2-8ED9-44E1-BA86-7019CBC7D05F", "Place and date:");
			// 
			// Section50RepresentedByLabel
			// 
			this.Section50RepresentedByLabel.AutoSize = true;
			this.Section50RepresentedByLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section50RepresentedByLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 81, true);
			this.Section50RepresentedByLabel.Name = "Section50RepresentedByLabel";
			this.Section50RepresentedByLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 10, true);
			this.Section50RepresentedByLabel.TabIndex = 3;
			this.Section50RepresentedByLabel.Text = Enterprise.Customs.GUI.Res.GetString("9A18846B-6448-41AD-9E06-BBD061FE78BC", "represented by");
			// 
			// Section50SignatureLabel
			// 
			this.Section50SignatureLabel.AutoSize = true;
			this.Section50SignatureLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section50SignatureLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 5, true);
			this.Section50SignatureLabel.Name = "Section50SignatureLabel";
			this.Section50SignatureLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 10, true);
			this.Section50SignatureLabel.TabIndex = 2;
			this.Section50SignatureLabel.Text = Enterprise.Customs.GUI.Res.GetString("27376145-02D3-43BF-9B32-59935739F315", "Signature");
			// 
			// Section50NoLabel
			// 
			this.Section50NoLabel.AutoSize = true;
			this.Section50NoLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section50NoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 5, true);
			this.Section50NoLabel.Name = "Section50NoLabel";
			this.Section50NoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 10, true);
			this.Section50NoLabel.TabIndex = 1;
			this.Section50NoLabel.Text = Enterprise.Customs.GUI.Res.GetString("A4253F0E-F489-4A3F-A07B-C8AC685FDAE9", "No");
			// 
			// Section50DescriptionLabel
			// 
			this.Section50DescriptionLabel.AutoSize = true;
			this.Section50DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section50DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section50DescriptionLabel.Name = "Section50DescriptionLabel";
			this.Section50DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 10, true);
			this.Section50DescriptionLabel.TabIndex = 0;
			this.Section50DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("6368675C-6DB9-4B42-A040-6C46A6A5BB08", "50 Principal");
			// 
			// SectionBPanel
			// 
			this.SectionBPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SectionBPanel.Controls.Add(this.SectionBDescriptionLabel);
			this.SectionBPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 817, true);
			this.SectionBPanel.Name = "SectionBPanel";
			this.SectionBPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 110, true);
			this.SectionBPanel.TabIndex = 61;
			// 
			// SectionBDescriptionLabel
			// 
			this.SectionBDescriptionLabel.AutoSize = true;
			this.SectionBDescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SectionBDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.SectionBDescriptionLabel.Name = "SectionBDescriptionLabel";
			this.SectionBDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 10, true);
			this.SectionBDescriptionLabel.TabIndex = 0;
			this.SectionBDescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("3F3F5B1B-73C5-4BDB-9B33-1A13439A96E3", "B  ACCOUNTING DETAILS");
			// 
			// Section49Panel
			// 
			this.Section49Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section49Panel.Controls.Add(this.Section49DescriptionLabel);
			this.Section49Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 777, true);
			this.Section49Panel.Name = "Section49Panel";
			this.Section49Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 41, true);
			this.Section49Panel.TabIndex = 60;
			// 
			// Section49DescriptionLabel
			// 
			this.Section49DescriptionLabel.AutoSize = true;
			this.Section49DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section49DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section49DescriptionLabel.Name = "Section49DescriptionLabel";
			this.Section49DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 10, true);
			this.Section49DescriptionLabel.TabIndex = 0;
			this.Section49DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("CB40DBA9-E1B2-496C-A5D0-CA3BAEFD7447", "49 Identification of warehouse");
			// 
			// Section48Panel
			// 
			this.Section48Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section48Panel.Controls.Add(this.Section48DescriptionLabel);
			this.Section48Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 777, true);
			this.Section48Panel.Name = "Section48Panel";
			this.Section48Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 41, true);
			this.Section48Panel.TabIndex = 59;
			// 
			// Section48DescriptionLabel
			// 
			this.Section48DescriptionLabel.AutoSize = true;
			this.Section48DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section48DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section48DescriptionLabel.Name = "Section48DescriptionLabel";
			this.Section48DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 10, true);
			this.Section48DescriptionLabel.TabIndex = 0;
			this.Section48DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("AE597D57-7348-4B50-AF1A-2E4186F09E6A", "48 Deferred payment");
			// 
			// Section47bPanel
			// 
			this.Section47bPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section47bPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 777, true);
			this.Section47bPanel.Name = "Section47bPanel";
			this.Section47bPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 150, true);
			this.Section47bPanel.TabIndex = 58;
			// 
			// Section47Panel
			// 
			this.Section47Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section47Panel.Controls.Add(this.Section47NumberLabel);
			this.Section47Panel.Controls.Add(this.Section47DescriptionLabel);
			this.Section47Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 777, true);
			this.Section47Panel.Name = "Section47Panel";
			this.Section47Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 150, true);
			this.Section47Panel.TabIndex = 57;
			// 
			// Section47NumberLabel
			// 
			this.Section47NumberLabel.AutoSize = true;
			this.Section47NumberLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section47NumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section47NumberLabel.Name = "Section47NumberLabel";
			this.Section47NumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 10, true);
			this.Section47NumberLabel.TabIndex = 0;
			this.Section47NumberLabel.Text = Enterprise.Customs.GUI.Res.GetString("04B7291C-EBBB-4DAF-A767-7E5EB3C9E087", "47");
			// 
			// Section47DescriptionLabel
			// 
			this.Section47DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section47DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 3, true);
			this.Section47DescriptionLabel.Name = "Section47DescriptionLabel";
			this.Section47DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 142, true);
			this.Section47DescriptionLabel.TabIndex = 1;
			this.Section47DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("B4FEC627-1BC0-4302-89FF-81A2A64AFC66", "Calculation of taxes");
			// 
			// Section46Panel
			// 
			this.Section46Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section46Panel.Controls.Add(this.Section46DescriptionLAbel);
			this.Section46Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 737, true);
			this.Section46Panel.Name = "Section46Panel";
			this.Section46Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 41, true);
			this.Section46Panel.TabIndex = 56;
			// 
			// Section46DescriptionLAbel
			// 
			this.Section46DescriptionLAbel.AutoSize = true;
			this.Section46DescriptionLAbel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section46DescriptionLAbel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section46DescriptionLAbel.Name = "Section46DescriptionLAbel";
			this.Section46DescriptionLAbel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 10, true);
			this.Section46DescriptionLAbel.TabIndex = 0;
			this.Section46DescriptionLAbel.Text = Enterprise.Customs.GUI.Res.GetString("BBF1DD58-30E6-4EBF-9F39-3B0272CD3E9D", "46 Statistical value");
			// 
			// SectionA1Panel
			// 
			this.SectionA1Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SectionA1Panel.Controls.Add(this.SectionA1DescriptionLabel);
			this.SectionA1Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 697, true);
			this.SectionA1Panel.Name = "SectionA1Panel";
			this.SectionA1Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 41, true);
			this.SectionA1Panel.TabIndex = 54;
			// 
			// SectionA1DescriptionLabel
			// 
			this.SectionA1DescriptionLabel.AutoSize = true;
			this.SectionA1DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SectionA1DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SectionA1DescriptionLabel.Name = "SectionA1DescriptionLabel";
			this.SectionA1DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 10, true);
			this.SectionA1DescriptionLabel.TabIndex = 0;
			this.SectionA1DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("BF15D315-68FE-40D8-8D7A-DBFB75F81EB2", "A.1. Code");
			// 
			// Section45Panel
			// 
			this.Section45Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section45Panel.Controls.Add(this.Section45DescriptionLabel);
			this.Section45Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 697, true);
			this.Section45Panel.Name = "Section45Panel";
			this.Section45Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 41, true);
			this.Section45Panel.TabIndex = 55;
			// 
			// Section45DescriptionLabel
			// 
			this.Section45DescriptionLabel.AutoSize = true;
			this.Section45DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section45DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section45DescriptionLabel.Name = "Section45DescriptionLabel";
			this.Section45DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 10, true);
			this.Section45DescriptionLabel.TabIndex = 0;
			this.Section45DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("6A5DBF95-438D-4189-9F64-56C30912F15C", "45 Adjustment");
			// 
			// Section44Panel
			// 
			this.Section44Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section44Panel.Controls.Add(this.Section44NumberLabel);
			this.Section44Panel.Controls.Add(this.Section44DescriptionLabel);
			this.Section44Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 674, true);
			this.Section44Panel.Name = "Section44Panel";
			this.Section44Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 104, true);
			this.Section44Panel.TabIndex = 53;
			// 
			// Section44NumberLabel
			// 
			this.Section44NumberLabel.AutoSize = true;
			this.Section44NumberLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section44NumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section44NumberLabel.Name = "Section44NumberLabel";
			this.Section44NumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 10, true);
			this.Section44NumberLabel.TabIndex = 1;
			this.Section44NumberLabel.Text = Enterprise.Customs.GUI.Res.GetString("473E3ABC-7160-4B65-A1A6-9EFFA3FB4B93", "44");
			// 
			// Section44DescriptionLabel
			// 
			this.Section44DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section44DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 3, true);
			this.Section44DescriptionLabel.Name = "Section44DescriptionLabel";
			this.Section44DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 96, true);
			this.Section44DescriptionLabel.TabIndex = 0;
			this.Section44DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("C382E6D1-BB06-4E73-8DF6-E5D98DB65736",
				"Additional information/ Documents produced/ Certificates and authorizations");
			// 
			// Setion42Panel
			// 
			this.Setion42Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Setion42Panel.Controls.Add(this.Section42TextBox);
			this.Setion42Panel.Controls.Add(this.Section42CalcEdit);
			this.Setion42Panel.Controls.Add(this.Section42DescriptionLabel);
			this.Setion42Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(642, 657, true);
			this.Setion42Panel.Name = "Setion42Panel";
			this.Setion42Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 41, true);
			this.Setion42Panel.TabIndex = 51;
			// 
			// Section42TextBox
			// 
			this.Section42TextBox.BindTo = "Currency";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).CurrencyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Currency)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section42TextBox, false);
			this.Section42TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 18, true);
			this.Section42TextBox.Name = "Section42TextBox";
			this.Section42TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.Section42TextBox.TabIndex = 2;
			// 
			// Section42CalcEdit
			// 
			this.Section42CalcEdit.BindTo = "D1_ItemPrice";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ItemPrice)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ItemPriceInfo)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section42CalcEdit, false);
			this.Section42CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section42CalcEdit.Name = "Section42CalcEdit";
			this.Section42CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.Section42CalcEdit.TabIndex = 1;
			this.Section42CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Section42DescriptionLabel
			// 
			this.Section42DescriptionLabel.AutoSize = true;
			this.Section42DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section42DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section42DescriptionLabel.Name = "Section42DescriptionLabel";
			this.Section42DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 10, true);
			this.Section42DescriptionLabel.TabIndex = 0;
			this.Section42DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("96AC3CC2-4707-4B61-B00C-1E9F42AE2A6C", "42 Item price");
			// 
			// Section41Panel
			// 
			this.Section41Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section41Panel.Controls.Add(this.Section41TextBox);
			this.Section41Panel.Controls.Add(this.Section41CalcEdit);
			this.Section41Panel.Controls.Add(this.Section41DescriptionLabel);
			this.Section41Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 657, true);
			this.Section41Panel.Name = "Section41Panel";
			this.Section41Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 41, true);
			this.Section41Panel.TabIndex = 50;
			// 
			// Section41TextBox
			// 
			this.Section41TextBox.BindTo = "D1_SupplementaryUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_SupplementaryUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_SupplementaryUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section41TextBox, false);
			this.Section41TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 18, true);
			this.Section41TextBox.Name = "Section41TextBox";
			this.Section41TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 20, true);
			this.Section41TextBox.TabIndex = 2;
			// 
			// Section41CalcEdit
			// 
			this.Section41CalcEdit.BindTo = "D1_SupplementaryQty";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_SupplementaryQty)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_SupplementaryQtyInfo)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section41CalcEdit, false);
			this.Section41CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section41CalcEdit.Name = "Section41CalcEdit";
			this.Section41CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 20, true);
			this.Section41CalcEdit.TabIndex = 1;
			this.Section41CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Section41DescriptionLabel
			// 
			this.Section41DescriptionLabel.AutoSize = true;
			this.Section41DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section41DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section41DescriptionLabel.Name = "Section41DescriptionLabel";
			this.Section41DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 10, true);
			this.Section41DescriptionLabel.TabIndex = 0;
			this.Section41DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("E0226EFC-0FDD-4C1C-9067-8543737B299E", "41 Supplementary units");
			// 
			// Section43Panel
			// 
			this.Section43Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section43Panel.Controls.Add(this.Section43DescriptionLabel);
			this.Section43Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 657, true);
			this.Section43Panel.Name = "Section43Panel";
			this.Section43Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 41, true);
			this.Section43Panel.TabIndex = 52;
			// 
			// Section43DescriptionLabel
			// 
			this.Section43DescriptionLabel.AutoSize = true;
			this.Section43DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section43DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section43DescriptionLabel.Name = "Section43DescriptionLabel";
			this.Section43DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 10, true);
			this.Section43DescriptionLabel.TabIndex = 0;
			this.Section43DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("41D3FE40-55F3-4835-AC7F-956E9E8B85A1", "43 VM code");
			// 
			// Section40Panel
			// 
			this.Section40Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section40Panel.Controls.Add(this.Section40DescriptionLabel);
			this.Section40Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 617, true);
			this.Section40Panel.Name = "Section40Panel";
			this.Section40Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(299, 41, true);
			this.Section40Panel.TabIndex = 49;
			// 
			// Section40DescriptionLabel
			// 
			this.Section40DescriptionLabel.AutoSize = true;
			this.Section40DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section40DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section40DescriptionLabel.Name = "Section40DescriptionLabel";
			this.Section40DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 10, true);
			this.Section40DescriptionLabel.TabIndex = 0;
			this.Section40DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("A6E094F9-5040-49B0-B4E0-9AAF0AD2BBA9", "40 Summary declaration/Previous document");
			// 
			// Section39Panel
			// 
			this.Section39Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section39Panel.Controls.Add(this.Section39DescriptionLabel);
			this.Section39Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 577, true);
			this.Section39Panel.Name = "Section39Panel";
			this.Section39Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 41, true);
			this.Section39Panel.TabIndex = 48;
			// 
			// Section39DescriptionLabel
			// 
			this.Section39DescriptionLabel.AutoSize = true;
			this.Section39DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section39DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section39DescriptionLabel.Name = "Section39DescriptionLabel";
			this.Section39DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 10, true);
			this.Section39DescriptionLabel.TabIndex = 0;
			this.Section39DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("C9300B61-91D0-4E7C-BED2-04FD9A5824CA", "39 Quota");
			// 
			// Section38Panel
			// 
			this.Section38Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section38Panel.Controls.Add(this.Section38DescriptionLabel);
			this.Section38Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 577, true);
			this.Section38Panel.Name = "Section38Panel";
			this.Section38Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 41, true);
			this.Section38Panel.TabIndex = 47;
			// 
			// Section38DescriptionLabel
			// 
			this.Section38DescriptionLabel.AutoSize = true;
			this.Section38DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section38DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section38DescriptionLabel.Name = "Section38DescriptionLabel";
			this.Section38DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 10, true);
			this.Section38DescriptionLabel.TabIndex = 0;
			this.Section38DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("1AA33A87-F31A-463A-A20B-2D1C25D0E40A", "38 Net mass (kg)");
			// 
			// Section37Panel
			// 
			this.Section37Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section37Panel.Controls.Add(this.Section37DescriptionLabel);
			this.Section37Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 577, true);
			this.Section37Panel.Name = "Section37Panel";
			this.Section37Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 41, true);
			this.Section37Panel.TabIndex = 46;
			// 
			// Section37DescriptionLabel
			// 
			this.Section37DescriptionLabel.AutoSize = true;
			this.Section37DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section37DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section37DescriptionLabel.Name = "Section37DescriptionLabel";
			this.Section37DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 10, true);
			this.Section37DescriptionLabel.TabIndex = 0;
			this.Section37DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("3E101C01-370F-4311-B825-75AEB80DC36F", "37 PROCEDURE");
			// 
			// Section35Panel
			// 
			this.Section35Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section35Panel.Controls.Add(this.Section35CalcDropEdit);
			this.Section35Panel.Controls.Add(this.Section35DescriptionLabel);
			this.Section35Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 537, true);
			this.Section35Panel.Name = "Section35Panel";
			this.Section35Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 41, true);
			this.Section35Panel.TabIndex = 44;
			// 
			// Section35CalcDropEdit
			// 
			this.Section35CalcDropEdit.BindToAmount = "D1_GrossMass";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_GrossMass)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_GrossMassInfo)));
			this.Section35CalcDropEdit.BindToList = "Lookups+WeightUQList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.WeightUQList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.WeightUQList)));
			this.Section35CalcDropEdit.BindToUnit = "D1_GrossMassUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_GrossMassUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_GrossMassUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section35CalcDropEdit, false);
			this.Section35CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section35CalcDropEdit.Name = "Section35CalcDropEdit";
			this.Section35CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.Section35CalcDropEdit.TabIndex = 1;
			this.Section35CalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// Section35DescriptionLabel
			// 
			this.Section35DescriptionLabel.AutoSize = true;
			this.Section35DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section35DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section35DescriptionLabel.Name = "Section35DescriptionLabel";
			this.Section35DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 10, true);
			this.Section35DescriptionLabel.TabIndex = 0;
			this.Section35DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("8A664FD4-B86A-4C96-9D2D-2A6C2B6C5628", "35 Gross mass");
			// 
			// Section36Panel
			// 
			this.Section36Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section36Panel.Controls.Add(this.Section36DescriptionLabel);
			this.Section36Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 537, true);
			this.Section36Panel.Name = "Section36Panel";
			this.Section36Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 41, true);
			this.Section36Panel.TabIndex = 45;
			// 
			// Section36DescriptionLabel
			// 
			this.Section36DescriptionLabel.AutoSize = true;
			this.Section36DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section36DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section36DescriptionLabel.Name = "Section36DescriptionLabel";
			this.Section36DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 10, true);
			this.Section36DescriptionLabel.TabIndex = 0;
			this.Section36DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("F40AA209-3105-43CE-9496-02782B7A630B", "36 Pref.");
			// 
			// Section34Panel
			// 
			this.Section34Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section34Panel.Controls.Add(this.Section34CodeFindBox);
			this.Section34Panel.Controls.Add(this.Section34DescriptionLabel);
			this.Section34Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 537, true);
			this.Section34Panel.Name = "Section34Panel";
			this.Section34Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 41, true);
			this.Section34Panel.TabIndex = 43;
			// 
			// Section34CodeFindBox
			// 
			this.Section34CodeFindBox.BindTo = "D1_RN_NKItemCountryOfOrigin";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RN_NKItemCountryOfOriginInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RN_NKItemCountryOfOrigin)));
			this.Section34CodeFindBox.BindToList = "Lookups+CountryList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.CountryList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section34CodeFindBox, false);
			this.Section34CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section34CodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.Section34CodeFindBox.Name = "Section34CodeFindBox";
			this.Section34CodeFindBox.PreBoundMaxLength = 2;
			this.Section34CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.Section34CodeFindBox.TabIndex = 1;
			// 
			// Section34DescriptionLabel
			// 
			this.Section34DescriptionLabel.AutoSize = true;
			this.Section34DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section34DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section34DescriptionLabel.Name = "Section34DescriptionLabel";
			this.Section34DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 10, true);
			this.Section34DescriptionLabel.TabIndex = 0;
			this.Section34DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("7953C57F-7327-4A0E-8D59-09E6C644B93C", "34 Country origin Code");
			// 
			// Section33Panel
			// 
			this.Section33Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section33Panel.Controls.Add(this.Section33DescriptionLabel);
			this.Section33Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 497, true);
			this.Section33Panel.Name = "Section33Panel";
			this.Section33Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 41, true);
			this.Section33Panel.TabIndex = 42;
			// 
			// Section33DescriptionLabel
			// 
			this.Section33DescriptionLabel.AutoSize = true;
			this.Section33DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section33DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section33DescriptionLabel.Name = "Section33DescriptionLabel";
			this.Section33DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 10, true);
			this.Section33DescriptionLabel.TabIndex = 0;
			this.Section33DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("55F353DB-1E9D-4965-B419-8DFF4A90F5D0", "33 Commodity Code");
			// 
			// Section31Panel
			// 
			this.Section31Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section31Panel.Controls.Add(this.Section31NumberLabel);
			this.Section31Panel.Controls.Add(this.Section31DescriptionLabel);
			this.Section31Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 497, true);
			this.Section31Panel.Name = "Section31Panel";
			this.Section31Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 178, true);
			this.Section31Panel.TabIndex = 40;
			// 
			// Section31NumberLabel
			// 
			this.Section31NumberLabel.AutoSize = true;
			this.Section31NumberLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section31NumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section31NumberLabel.Name = "Section31NumberLabel";
			this.Section31NumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 10, true);
			this.Section31NumberLabel.TabIndex = 0;
			this.Section31NumberLabel.Text = Enterprise.Customs.GUI.Res.GetString("B2DCE61F-B87C-41A8-A84D-0C5F56484A03", "31");
			// 
			// Section31DescriptionLabel
			// 
			this.Section31DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section31DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 3, true);
			this.Section31DescriptionLabel.Name = "Section31DescriptionLabel";
			this.Section31DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 170, true);
			this.Section31DescriptionLabel.TabIndex = 1;
			this.Section31DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("E394A663-FF61-4AA3-AB8D-BAE9CB699B6A", "Packages and description of goods");
			// 
			// Section31bPanel
			// 
			this.Section31bPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section31bPanel.Controls.Add(this.Section31bQuantityCalcDropEdit);
			this.Section31bPanel.Controls.Add(this.Section31bQuantityLabel);
			this.Section31bPanel.Controls.Add(this.Section31bMarksNumbersLabel);
			this.Section31bPanel.Controls.Add(this.Section31bMarksNumbersTextBox);
			this.Section31bPanel.Controls.Add(this.Section31bDescriptionLabel);
			this.Section31bPanel.Controls.Add(this.Section32Panel);
			this.Section31bPanel.Controls.Add(this.Section31bDescriptionTextbox);
			this.Section31bPanel.Controls.Add(this.Section31bDescriptionPanel);
			this.Section31bPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 497, true);
			this.Section31bPanel.Name = "Section31bPanel";
			this.Section31bPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(440, 178, true);
			this.Section31bPanel.TabIndex = 41;
			// 
			// Section31bQuantityCalcDropEdit
			// 
			this.Section31bQuantityCalcDropEdit.BindToAmount = "D1_ItemQty";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ItemQty)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ItemQtyInfo)));
			this.Section31bQuantityCalcDropEdit.BindToList = "Lookups+InvoiceLineUQList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.InvoiceLineUQList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.InvoiceLineUQList)));
			this.Section31bQuantityCalcDropEdit.BindToUnit = "D1_ItemUQ";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ItemUQInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ItemUQ)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section31bQuantityCalcDropEdit, false);
			this.Section31bQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 20, true);
			this.Section31bQuantityCalcDropEdit.Name = "Section31bQuantityCalcDropEdit";
			this.Section31bQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.Section31bQuantityCalcDropEdit.TabIndex = 2;
			this.Section31bQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// Section31bQuantityLabel
			// 
			this.Section31bQuantityLabel.AutoSize = true;
			this.Section31bQuantityLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section31bQuantityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 26, true);
			this.Section31bQuantityLabel.Name = "Section31bQuantityLabel";
			this.Section31bQuantityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 10, true);
			this.Section31bQuantityLabel.TabIndex = 1;
			this.Section31bQuantityLabel.Text = Enterprise.Customs.GUI.Res.GetString("7F589969-D2C4-4233-ADAE-11CF0DE65EE4", "Quantity");
			// 
			// Section31bMarksNumbersLabel
			// 
			this.Section31bMarksNumbersLabel.AutoSize = true;
			this.Section31bMarksNumbersLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section31bMarksNumbersLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 110, true);
			this.Section31bMarksNumbersLabel.Name = "Section31bMarksNumbersLabel";
			this.Section31bMarksNumbersLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 10, true);
			this.Section31bMarksNumbersLabel.TabIndex = 6;
			this.Section31bMarksNumbersLabel.Text = Enterprise.Customs.GUI.Res.GetString("821AB606-C7A7-46FD-ADB7-A41C173DE9B5", "Marks/Numbers");
			// 
			// Section31bMarksNumbersTextBox
			// 
			this.Section31bMarksNumbersTextBox.BindTo = "D1_MarksAndNumbers";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_MarksAndNumbersInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_MarksAndNumbers)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section31bMarksNumbersTextBox, false);
			this.Section31bMarksNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 104, true);
			this.Section31bMarksNumbersTextBox.Multiline = true;
			this.Section31bMarksNumbersTextBox.Name = "Section31bMarksNumbersTextBox";
			this.Section31bMarksNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 62, true);
			this.Section31bMarksNumbersTextBox.TabIndex = 7;
			// 
			// Section31bDescriptionLabel
			// 
			this.Section31bDescriptionLabel.AutoSize = true;
			this.Section31bDescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section31bDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 47, true);
			this.Section31bDescriptionLabel.Name = "Section31bDescriptionLabel";
			this.Section31bDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 10, true);
			this.Section31bDescriptionLabel.TabIndex = 4;
			this.Section31bDescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("928CD285-6C94-4FAD-9F79-74BC7620D7F1", "Description");
			// 
			// Section32Panel
			// 
			this.Section32Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section32Panel.Controls.Add(this.Section32DescriptionLabel);
			this.Section32Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, -1, true);
			this.Section32Panel.Name = "Section32Panel";
			this.Section32Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 41, true);
			this.Section32Panel.TabIndex = 3;
			// 
			// Section32DescriptionLabel
			// 
			this.Section32DescriptionLabel.AutoSize = true;
			this.Section32DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section32DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section32DescriptionLabel.Name = "Section32DescriptionLabel";
			this.Section32DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 10, true);
			this.Section32DescriptionLabel.TabIndex = 0;
			this.Section32DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("402D17B3-A188-4148-86ED-0394D5D800C2", "32 Item No");
			// 
			// Section31bDescriptionTextbox
			// 
			this.Section31bDescriptionTextbox.BindTo = "D1_DescriptionOfGoods";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_DescriptionOfGoodsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_DescriptionOfGoods)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section31bDescriptionTextbox, false);
			this.Section31bDescriptionTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 41, true);
			this.Section31bDescriptionTextbox.Multiline = true;
			this.Section31bDescriptionTextbox.Name = "Section31bDescriptionTextbox";
			this.Section31bDescriptionTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 62, true);
			this.Section31bDescriptionTextbox.TabIndex = 5;
			// 
			// Section31bDescriptionPanel
			// 
			this.Section31bDescriptionPanel.AutoSize = true;
			this.Section31bDescriptionPanel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section31bDescriptionPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section31bDescriptionPanel.Name = "Section31bDescriptionPanel";
			this.Section31bDescriptionPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 10, true);
			this.Section31bDescriptionPanel.TabIndex = 0;
			this.Section31bDescriptionPanel.Text = Enterprise.Customs.GUI.Res.GetString("139CA228-2815-4BB6-8D29-FF0665570D3F", "Marks and numbers – Container No(s) – Number and kind");
			// 
			// LargeSixLabel2
			// 
			this.LargeSixLabel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.LargeSixLabel2.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
			this.LargeSixLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 457, true);
			this.LargeSixLabel2.Name = "LargeSixLabel2";
			this.LargeSixLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 41, true);
			this.LargeSixLabel2.TabIndex = 5;
			this.LargeSixLabel2.Text = Enterprise.Customs.GUI.Res.GetString("BC0ADC18-78E2-49F8-81F6-437724BB494E", "6");
			this.LargeSixLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			 
			//  
			// 
			// Section30Panel
			// 
			this.Section30Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section30Panel.Controls.Add(this.Section30DescriptionLabel);
			this.Section30Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 457, true);
			this.Section30Panel.Name = "Section30Panel";
			this.Section30Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 41, true);
			this.Section30Panel.TabIndex = 38;
			// 
			// Section30DescriptionLabel
			// 
			this.Section30DescriptionLabel.AutoSize = true;
			this.Section30DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section30DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section30DescriptionLabel.Name = "Section30DescriptionLabel";
			this.Section30DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 10, true);
			this.Section30DescriptionLabel.TabIndex = 0;
			this.Section30DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("43D356AE-29CA-479A-ABCE-880A97F43B4D", "30 Location of goods");
			// 
			// Section28Panel
			// 
			this.Section28Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section28Panel.Controls.Add(this.Section28DescriptionLabel);
			this.Section28Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 417, true);
			this.Section28Panel.Name = "Section28Panel";
			this.Section28Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 81, true);
			this.Section28Panel.TabIndex = 39;
			// 
			// Section28DescriptionLabel
			// 
			this.Section28DescriptionLabel.AutoSize = true;
			this.Section28DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section28DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section28DescriptionLabel.Name = "Section28DescriptionLabel";
			this.Section28DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 10, true);
			this.Section28DescriptionLabel.TabIndex = 0;
			this.Section28DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("4244BCD6-7299-4B54-BC5F-804DF7DAF032", "28 Financial and banking data");
			// 
			// Section26Panel
			// 
			this.Section26Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section26Panel.Controls.Add(this.Section26Description2Label);
			this.Section26Panel.Controls.Add(this.Section26DescriptionLabel);
			this.Section26Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(167, 417, true);
			this.Section26Panel.Name = "Section26Panel";
			this.Section26Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 41, true);
			this.Section26Panel.TabIndex = 35;
			// 
			// Section26Description2Label
			// 
			this.Section26Description2Label.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section26Description2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 14, true);
			this.Section26Description2Label.Name = "Section26Description2Label";
			this.Section26Description2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 23, true);
			this.Section26Description2Label.TabIndex = 1;
			this.Section26Description2Label.Text = Enterprise.Customs.GUI.Res.GetString("BB05D120-B75C-4116-BA42-AD41C8A7677D", "transport");
			// 
			// Section26DescriptionLabel
			// 
			this.Section26DescriptionLabel.AutoSize = true;
			this.Section26DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section26DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section26DescriptionLabel.Name = "Section26DescriptionLabel";
			this.Section26DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 10, true);
			this.Section26DescriptionLabel.TabIndex = 0;
			this.Section26DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("21A90E7D-1630-4175-A98B-94CE8DDB7DEE", "26 Inland mode of");
			// 
			// Section25Panel
			// 
			this.Section25Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section25Panel.Controls.Add(this.Section25DropEdit);
			this.Section25Panel.Controls.Add(this.Section25Description2Label);
			this.Section25Panel.Controls.Add(this.Section25DescriptionLabel);
			this.Section25Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 417, true);
			this.Section25Panel.Name = "Section25Panel";
			this.Section25Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 41, true);
			this.Section25Panel.TabIndex = 34;
			// 
			// Section25DropEdit
			// 
			this.Section25DropEdit.BindTo = "D1_ModeOfTransportAtTheBorder";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ModeOfTransportAtTheBorderInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_ModeOfTransportAtTheBorder)));
			this.Section25DropEdit.BindToList = "Lookups+ModeOfTransportAtBorderList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.ModeOfTransportAtBorderList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section25DropEdit, false);
			this.Section25DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section25DropEdit.Name = "Section25DropEdit";
			this.Section25DropEdit.PreBoundMaxLength = 3;
			this.Section25DropEdit.ShowDescriptionBox = false;
			this.Section25DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 20, true);
			this.Section25DropEdit.TabIndex = 1;
			// 
			// Section25Description2Label
			// 
			this.Section25Description2Label.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section25Description2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(54, 14, true);
			this.Section25Description2Label.Name = "Section25Description2Label";
			this.Section25Description2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 23, true);
			this.Section25Description2Label.TabIndex = 2;
			this.Section25Description2Label.Text = Enterprise.Customs.GUI.Res.GetString("FF280BD5-0F39-425F-825C-E7786B1EB72F", "at the border");
			// 
			// Section25DescriptionLabel
			// 
			this.Section25DescriptionLabel.AutoSize = true;
			this.Section25DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section25DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section25DescriptionLabel.Name = "Section25DescriptionLabel";
			this.Section25DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 10, true);
			this.Section25DescriptionLabel.TabIndex = 0;
			this.Section25DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("4C7D9FFF-57F9-42DD-8E96-E34EEA21FAD6", "25 Transport mode");
			// 
			// Section27Panel
			// 
			this.Section27Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section27Panel.Controls.Add(this.Section27CodeFindBox);
			this.Section27Panel.Controls.Add(this.Section27DescriptionLabel);
			this.Section27Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 417, true);
			this.Section27Panel.Name = "Section27Panel";
			this.Section27Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 41, true);
			this.Section27Panel.TabIndex = 36;
			// 
			// Section27CodeFindBox
			// 
			this.Section27CodeFindBox.BindTo = "D1_RL_NKPlaceOfUnloading";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKPlaceOfUnloadingInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKPlaceOfUnloading)));
			this.Section27CodeFindBox.BindToList = "Lookups+DischargeList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.DischargeList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section27CodeFindBox, false);
			this.Section27CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section27CodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Section27CodeFindBox.Name = "Section27CodeFindBox";
			this.Section27CodeFindBox.PreBoundMaxLength = 5;
			this.Section27CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.Section27CodeFindBox.TabIndex = 1;
			// 
			// Section27DescriptionLabel
			// 
			this.Section27DescriptionLabel.AutoSize = true;
			this.Section27DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section27DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section27DescriptionLabel.Name = "Section27DescriptionLabel";
			this.Section27DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 10, true);
			this.Section27DescriptionLabel.TabIndex = 0;
			this.Section27DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("5FBF5E77-7AED-4171-B648-C71C59F27BF1", "27 Place of unloading");
			// 
			// Section24Panel
			// 
			this.Section24Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section24Panel.Controls.Add(this.Section24DescriptionLabel);
			this.Section24Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(719, 377, true);
			this.Section24Panel.Name = "Section24Panel";
			this.Section24Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 41, true);
			this.Section24Panel.TabIndex = 33;
			// 
			// Section24DescriptionLabel
			// 
			this.Section24DescriptionLabel.AutoSize = true;
			this.Section24DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section24DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section24DescriptionLabel.Name = "Section24DescriptionLabel";
			this.Section24DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 10, true);
			this.Section24DescriptionLabel.TabIndex = 0;
			this.Section24DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("A59FE310-36FB-4D6D-A69A-EA3BC769F9E1", "24 Nature of transaction");
			// 
			// Section23Panel
			// 
			this.Section23Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section23Panel.Controls.Add(this.Section23ExchangeRateTextbox);
			this.Section23Panel.Controls.Add(this.Section23DescriptionLabel);
			this.Section23Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 377, true);
			this.Section23Panel.Name = "Section23Panel";
			this.Section23Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 41, true);
			this.Section23Panel.TabIndex = 32;
			// 
			// Section23ExchangeRateTextbox
			// 
			this.Section23ExchangeRateTextbox.BindTo = "ExchangeRate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).ExchangeRate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).ExchangeRateInfo)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section23ExchangeRateTextbox, false);
			this.Section23ExchangeRateTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section23ExchangeRateTextbox.Name = "Section23ExchangeRateTextbox";
			this.Section23ExchangeRateTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.Section23ExchangeRateTextbox.TabIndex = 1;
			this.Section23ExchangeRateTextbox.Text = Enterprise.Customs.GUI.Res.GetString("B249FC15-D351-441C-BFB5-8B9C9BD19EF2", "0.000000");
			this.Section23ExchangeRateTextbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Section23DescriptionLabel
			// 
			this.Section23DescriptionLabel.AutoSize = true;
			this.Section23DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section23DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section23DescriptionLabel.Name = "Section23DescriptionLabel";
			this.Section23DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 10, true);
			this.Section23DescriptionLabel.TabIndex = 0;
			this.Section23DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("CD7CEEBF-4D3A-41C5-B0E0-36442405B84E", "23 Exchange rate");
			// 
			// Section22Panel
			// 
			this.Section22Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section22Panel.Controls.Add(this.Section22TotalAmountInvoicedCalcFindBox);
			this.Section22Panel.Controls.Add(this.Section22DescriptionLabel);
			this.Section22Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 377, true);
			this.Section22Panel.Name = "Section22Panel";
			this.Section22Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 41, true);
			this.Section22Panel.TabIndex = 31;
			// 
			// Section22TotalAmountInvoicedCalcFindBox
			// 
			this.Section22TotalAmountInvoicedCalcFindBox.BindToAmount = "D1_InvoiceTotalAmount";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_InvoiceTotalAmount)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_InvoiceTotalAmountInfo)));
			this.Section22TotalAmountInvoicedCalcFindBox.BindToList = "Lookups+CurrencyList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.CurrencyList)));
			this.Section22TotalAmountInvoicedCalcFindBox.BindToUnit = "D1_RX_InvoiceCurrency";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RX_InvoiceCurrency)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RX_InvoiceCurrencyInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((object)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RX_InvoiceCurrency)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section22TotalAmountInvoicedCalcFindBox, false);
			this.Section22TotalAmountInvoicedCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section22TotalAmountInvoicedCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			this.Section22TotalAmountInvoicedCalcFindBox.Name = "Section22TotalAmountInvoicedCalcFindBox";
			this.Section22TotalAmountInvoicedCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.Section22TotalAmountInvoicedCalcFindBox.TabIndex = 1;
			// 
			// Section22DescriptionLabel
			// 
			this.Section22DescriptionLabel.AutoSize = true;
			this.Section22DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section22DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section22DescriptionLabel.Name = "Section22DescriptionLabel";
			this.Section22DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 10, true);
			this.Section22DescriptionLabel.TabIndex = 0;
			this.Section22DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("66B4DB21-134D-4FD8-9A90-28B4460CF44C", "22 Currency and total amount invoiced");
			// 
			// Section21Panel
			// 
			this.Section21Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section21Panel.Controls.Add(this.DepartureTransportIDBox);
			this.Section21Panel.Controls.Add(this.DepartureTransportIDLabel);
			this.Section21Panel.Controls.Add(this.Section21FlightDateDateEdit);
			this.Section21Panel.Controls.Add(this.Section21FlightDateLabel);
			this.Section21Panel.Controls.Add(this.Section21FlightNoLabel);
			this.Section21Panel.Controls.Add(this.Section21FlightNoTextBox);
			this.Section21Panel.Controls.Add(this.Section21DescriptionLabel);
			this.Section21Panel.Controls.Add(this.Section21VesselLabel);
			this.Section21Panel.Controls.Add(this.Section21VesselCodeFindBox);
			this.Section21Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 377, true);
			this.Section21Panel.Name = "Section21Panel";
			this.Section21Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 41, true);
			this.Section21Panel.TabIndex = 30;
			// 
			// DepartureTransportIDLabel
			// 
			this.DepartureTransportIDLabel.AutoSize = true;
			this.DepartureTransportIDLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DepartureTransportIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.DepartureTransportIDLabel.Name = "DepartureTransportIDLabel";
			this.DepartureTransportIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 9, true);
			this.DepartureTransportIDLabel.TabIndex = 8;
			this.DepartureTransportIDLabel.Text = Enterprise.Customs.GUI.Res.GetString("C2C83075-FF61-4576-BDFD-01765CFC1BB3", "Transport ID");
			// 
			// DepartureTransportIDBox
			// 
			this.BindingSource.SetBindingMember(this.DepartureTransportIDBox, "D1_DepartureTransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_DepartureTransportID)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DepartureTransportIDBox, false);
			this.DepartureTransportIDBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 18, true);
			this.DepartureTransportIDBox.Name = "DepartureTransportIDBox";
			this.DepartureTransportIDBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.DepartureTransportIDBox.TabIndex = 11;
			// 
			// Section21FlightDateDateEdit
			// 
			this.Section21FlightDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.Section21FlightDateDateEdit.AutoCompleteYear = true;
			this.Section21FlightDateDateEdit.BindTo = "D1_FlightDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_FlightDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_FlightDateInfo)));
			this.Section21FlightDateDateEdit.IsFixedReadOnly = false;
			this.Section21FlightDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 18, true);
			this.Section21FlightDateDateEdit.Name = "Section21FlightDateDateEdit";
			this.Section21FlightDateDateEdit.TabIndex = 6;
			// 
			// Section21FlightDateLabel
			// 
			this.Section21FlightDateLabel.AutoSize = true;
			this.Section21FlightDateLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section21FlightDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 24, true);
			this.Section21FlightDateLabel.Name = "Section21FlightDateLabel";
			this.Section21FlightDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 10, true);
			this.Section21FlightDateLabel.TabIndex = 4;
			this.Section21FlightDateLabel.Text = Enterprise.Customs.GUI.Res.GetString("4983E23F-867F-40DD-B874-8706097C8923", "Flight Date");
			// 
			// Section21FlightNoLabel
			// 
			this.Section21FlightNoLabel.AutoSize = true;
			this.Section21FlightNoLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section21FlightNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 24, true);
			this.Section21FlightNoLabel.Name = "Section21FlightNoLabel";
			this.Section21FlightNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 10, true);
			this.Section21FlightNoLabel.TabIndex = 1;
			this.Section21FlightNoLabel.Text = Enterprise.Customs.GUI.Res.GetString("7184696A-CD9F-4686-A978-D573C0E98C2A", "Flight No");
			// 
			// Section21FlightNoTextBox
			// 
			this.Section21FlightNoTextBox.BindTo = "D1_FlightNo";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_FlightNoInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_FlightNo)));
			this.Section21FlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 18, true);
			this.Section21FlightNoTextBox.Name = "Section21FlightNoTextBox";
			this.Section21FlightNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.Section21FlightNoTextBox.TabIndex = 3;
			// 
			// Section21DescriptionLabel
			// 
			this.Section21DescriptionLabel.AutoSize = true;
			this.Section21DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section21DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section21DescriptionLabel.Name = "Section21DescriptionLabel";
			this.Section21DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 10, true);
			this.Section21DescriptionLabel.TabIndex = 0;
			this.Section21DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("9034F123-1F48-45CB-8895-E1CD3E47A839", "21 Identity and nationality of active means of transport crossing the border");
			// 
			// Section21VesselLabel
			// 
			this.Section21VesselLabel.AutoSize = true;
			this.Section21VesselLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section21VesselLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 24, true);
			this.Section21VesselLabel.Name = "Section21VesselLabel";
			this.Section21VesselLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 10, true);
			this.Section21VesselLabel.TabIndex = 2;
			this.Section21VesselLabel.Text = Enterprise.Customs.GUI.Res.GetString("1D787D66-968F-4C1B-9E99-EB064B598E40", "Vessel");
			// 
			// Section21VesselCodeFindBox
			// 
			this.Section21VesselCodeFindBox.BindTo = "D1_VesselCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_VesselCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_VesselCode)));
			this.Section21VesselCodeFindBox.BindToList = "Lookups+VesselList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.VesselList)));
			this.Section21VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 18, true);
			this.Section21VesselCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.Section21VesselCodeFindBox.Name = "Section21VesselCodeFindBox";
			this.Section21VesselCodeFindBox.ShowDescriptionBox = false;
			this.Section21VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.Section21VesselCodeFindBox.TabIndex = 5;
			// 
			// Section20Panel
			// 
			this.Section20Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section20Panel.Controls.Add(this.Section20DropEdit);
			this.Section20Panel.Controls.Add(this.Section20DescriptionLabel);
			this.Section20Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 337, true);
			this.Section20Panel.Name = "Section20Panel";
			this.Section20Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 41, true);
			this.Section20Panel.TabIndex = 29;
			// 
			// Section20DropEdit
			// 
			this.Section20DropEdit.BindTo = "D1_DeliveryTerms";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_DeliveryTermsInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_DeliveryTerms)));
			this.Section20DropEdit.BindToList = "Lookups+DeliveryTermsList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.DeliveryTermsList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section20DropEdit, false);
			this.Section20DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section20DropEdit.Name = "Section20DropEdit";
			this.Section20DropEdit.PreBoundMaxLength = 3;
			this.Section20DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 20, true);
			this.Section20DropEdit.TabIndex = 1;
			// 
			// Section20DescriptionLabel
			// 
			this.Section20DescriptionLabel.AutoSize = true;
			this.Section20DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section20DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section20DescriptionLabel.Name = "Section20DescriptionLabel";
			this.Section20DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 10, true);
			this.Section20DescriptionLabel.TabIndex = 0;
			this.Section20DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("456AF475-7A15-4460-AE9D-6B84B43E8772", "20 Delivery terms");
			// 
			// Section19Panel
			// 
			this.Section19Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section19Panel.Controls.Add(this.Section19DescriptionLabel);
			this.Section19Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 337, true);
			this.Section19Panel.Name = "Section19Panel";
			this.Section19Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 41, true);
			this.Section19Panel.TabIndex = 28;
			// 
			// Section19DescriptionLabel
			// 
			this.Section19DescriptionLabel.AutoSize = true;
			this.Section19DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section19DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section19DescriptionLabel.Name = "Section19DescriptionLabel";
			this.Section19DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(31, 10, true);
			this.Section19DescriptionLabel.TabIndex = 0;
			this.Section19DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("39C695F6-7A90-41BC-A572-82A0F90B903A", "19 Ctr.");
			// 
			// Section18Panel
			// 
			this.Section18Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section18Panel.Controls.Add(this.Box18TransportIDBox);
			this.Section18Panel.Controls.Add(this.Box18TransportIDLabel);
			this.Section18Panel.Controls.Add(this.Section18DescriptionLabel);
			this.Section18Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 337, true);
			this.Section18Panel.Name = "Section18Panel";
			this.Section18Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 41, true);
			this.Section18Panel.TabIndex = 27;
			// 
			// Box18TransportIDBox
			// 
			this.BindingSource.SetBindingMember(this.Box18TransportIDBox, "D1_Box18TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_Box18TransportID)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Box18TransportIDBox, false);
			this.Box18TransportIDBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 18, true);
			this.Box18TransportIDBox.Name = "Box18TransportIDBox";
			this.Box18TransportIDBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 17, true);
			this.Box18TransportIDBox.TabIndex = 5;
			// 
			// Box18TransportIDLabel
			// 
			this.Box18TransportIDLabel.AutoSize = true;
			this.Box18TransportIDLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Box18TransportIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.Box18TransportIDLabel.Name = "Box18TransportIDLabel";
			this.Box18TransportIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 9, true);
			this.Box18TransportIDLabel.TabIndex = 4;
			this.Box18TransportIDLabel.Text = Enterprise.Customs.GUI.Res.GetString("AB9AEF0C-4260-454F-96FC-7FED9FC5AE99", "Transport ID (Inland)");
			// 
			// Section18DescriptionLabel
			// 
			this.Section18DescriptionLabel.AutoSize = true;
			this.Section18DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section18DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section18DescriptionLabel.Name = "Section18DescriptionLabel";
			this.Section18DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 10, true);
			this.Section18DescriptionLabel.TabIndex = 0;
			this.Section18DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("E408284E-1930-47D6-A9A3-66EE92974E33", "18 Identity and nationality of means of transport on arrival");
			// 
			// Section14Panel
			// 
			this.Section14Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section14Panel.Controls.Add(this.Section14TextBox);
			this.Section14Panel.Controls.Add(this.Section14DescriptionLabel);
			this.Section14Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 235, true);
			this.Section14Panel.Name = "Section14Panel";
			this.Section14Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 103, true);
			this.Section14Panel.TabIndex = 17;
			// 
			// Section14TextBox
			// 
			this.Section14TextBox.BindTo = "DeclarantRepresentativeFormattedAddress";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).DeclarantRepresentativeFormattedAddressInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).DeclarantRepresentativeFormattedAddress)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section14TextBox, false);
			this.Section14TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 22, true);
			this.Section14TextBox.Multiline = true;
			this.Section14TextBox.Name = "Section14TextBox";
			this.Section14TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 78, true);
			this.Section14TextBox.TabIndex = 1;
			// 
			// Section14DescriptionLabel
			// 
			this.Section14DescriptionLabel.AutoSize = true;
			this.Section14DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section14DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section14DescriptionLabel.Name = "Section14DescriptionLabel";
			this.Section14DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 10, true);
			this.Section14DescriptionLabel.TabIndex = 0;
			this.Section14DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("89CD94B3-9AAC-4CA9-8E5C-84DE195ACF73", "14 Declarant/Representative");
			// 
			// Section17bPanel
			// 
			this.Section17bPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section17bPanel.Controls.Add(this.Section17bTextbox);
			this.Section17bPanel.Controls.Add(this.Section17bLabel);
			this.Section17bPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(719, 257, true);
			this.Section17bPanel.Name = "Section17bPanel";
			this.Section17bPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 41, true);
			this.Section17bPanel.TabIndex = 24;
			// 
			// Section17bTextbox
			// 
			this.Section17bTextbox.BindTo = "CountryOfDestinationFormatted";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).CountryOfDestinationFormattedInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).CountryOfDestinationFormatted)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section17bTextbox, false);
			this.Section17bTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Section17bTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section17bTextbox.Name = "Section17bTextbox";
			this.Section17bTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.Section17bTextbox.TabIndex = 1;
			// 
			// Section17bLabel
			// 
			this.Section17bLabel.AutoSize = true;
			this.Section17bLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section17bLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section17bLabel.Name = "Section17bLabel";
			this.Section17bLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 10, true);
			this.Section17bLabel.TabIndex = 0;
			this.Section17bLabel.Text = Enterprise.Customs.GUI.Res.GetString("A4C3F032-F3B6-483D-97A2-81FC472144A0", "17 Country dest. code");
			// 
			// Section15bPanel
			// 
			this.Section15bPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section15bPanel.Controls.Add(this.Section15bTextbox);
			this.Section15bPanel.Controls.Add(this.Section15bLabel);
			this.Section15bPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 257, true);
			this.Section15bPanel.Name = "Section15bPanel";
			this.Section15bPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 41, true);
			this.Section15bPanel.TabIndex = 23;
			// 
			// Section15bTextbox
			// 
			this.Section15bTextbox.BindTo = "CountryOfDispatchExportCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).CountryOfDispatchExportCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).CountryOfDispatchExportCode)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section15bTextbox, false);
			this.Section15bTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.Section15bTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section15bTextbox.Name = "Section15bTextbox";
			this.Section15bTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.Section15bTextbox.TabIndex = 1;
			// 
			// Section15bLabel
			// 
			this.Section15bLabel.AutoSize = true;
			this.Section15bLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section15bLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section15bLabel.Name = "Section15bLabel";
			this.Section15bLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 10, true);
			this.Section15bLabel.TabIndex = 0;
			this.Section15bLabel.Text = Enterprise.Customs.GUI.Res.GetString("380EACBF-EF54-4F0D-A8AB-2BC06017FFBF", "15 C disp./exp. Code");
			// 
			// Section17Panel
			// 
			this.Section17Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section17Panel.Controls.Add(this.Section17CodeFindBox);
			this.Section17Panel.Controls.Add(this.Section17DescriptionLabel);
			this.Section17Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(619, 297, true);
			this.Section17Panel.Name = "Section17Panel";
			this.Section17Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 41, true);
			this.Section17Panel.TabIndex = 26;
			// 
			// Section17CodeFindBox
			// 
			this.Section17CodeFindBox.BindTo = "D1_RL_NKCountryOfDestination";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKCountryOfDestinationInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKCountryOfDestination)));
			this.Section17CodeFindBox.BindToList = "Lookups+FinalDestinationList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.FinalDestinationList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section17CodeFindBox, false);
			this.Section17CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section17CodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Section17CodeFindBox.Name = "Section17CodeFindBox";
			this.Section17CodeFindBox.PreBoundMaxLength = 5;
			this.Section17CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.Section17CodeFindBox.TabIndex = 1;
			// 
			// Section17DescriptionLabel
			// 
			this.Section17DescriptionLabel.AutoSize = true;
			this.Section17DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section17DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section17DescriptionLabel.Name = "Section17DescriptionLabel";
			this.Section17DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 10, true);
			this.Section17DescriptionLabel.TabIndex = 0;
			this.Section17DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("470B3E65-6B81-4B98-A378-35267FA42272", "17 Place of destination");
			// 
			// Section16Panel
			// 
			this.Section16Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section16Panel.Controls.Add(this.Section16CodeFindBox);
			this.Section16Panel.Controls.Add(this.Section16DescriptionLabel);
			this.Section16Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 297, true);
			this.Section16Panel.Name = "Section16Panel";
			this.Section16Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 41, true);
			this.Section16Panel.TabIndex = 25;
			// 
			// Section16CodeFindBox
			// 
			this.Section16CodeFindBox.BindTo = "D1_RL_NKCountryOfOrigin";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKCountryOfOriginInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKCountryOfOrigin)));
			this.Section16CodeFindBox.BindToList = "Lookups+OriginList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.OriginList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section16CodeFindBox, false);
			this.Section16CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section16CodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Section16CodeFindBox.Name = "Section16CodeFindBox";
			this.Section16CodeFindBox.PreBoundMaxLength = 5;
			this.Section16CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.Section16CodeFindBox.TabIndex = 1;
			// 
			// Section16DescriptionLabel
			// 
			this.Section16DescriptionLabel.AutoSize = true;
			this.Section16DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section16DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section16DescriptionLabel.Name = "Section16DescriptionLabel";
			this.Section16DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 10, true);
			this.Section16DescriptionLabel.TabIndex = 0;
			this.Section16DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("35FCB58E-311F-4D53-A1DC-455DAEDB5C63", "16 Place of origin");
			// 
			// Section15Panel
			// 
			this.Section15Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section15Panel.Controls.Add(this.Section15CodeFindBox);
			this.Section15Panel.Controls.Add(this.Section15DescriptionLabel);
			this.Section15Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 257, true);
			this.Section15Panel.Name = "Section15Panel";
			this.Section15Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 41, true);
			this.Section15Panel.TabIndex = 22;
			// 
			// Section15CodeFindBox
			// 
			this.Section15CodeFindBox.BindTo = "D1_RL_NKCountryOfDispatchOrExport";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKCountryOfDispatchOrExportInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_RL_NKCountryOfDispatchOrExport)));
			this.Section15CodeFindBox.BindToList = "Lookups+OriginList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.OriginList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section15CodeFindBox, false);
			this.Section15CodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section15CodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.Section15CodeFindBox.Name = "Section15CodeFindBox";
			this.Section15CodeFindBox.PreBoundMaxLength = 5;
			this.Section15CodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.Section15CodeFindBox.TabIndex = 1;
			// 
			// Section15DescriptionLabel
			// 
			this.Section15DescriptionLabel.AutoSize = true;
			this.Section15DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section15DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section15DescriptionLabel.Name = "Section15DescriptionLabel";
			this.Section15DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 10, true);
			this.Section15DescriptionLabel.TabIndex = 0;
			this.Section15DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("6A7D49EB-5E60-45A3-AA95-587AD1C2FFC3", "15 Place of dispatch/export");
			// 
			// CopyCountryDestinationBorderLabel
			// 
			this.CopyCountryDestinationBorderLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.CopyCountryDestinationBorderLabel.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold);
			this.CopyCountryDestinationBorderLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 71, true);
			this.CopyCountryDestinationBorderLabel.Name = "CopyCountryDestinationBorderLabel";
			this.CopyCountryDestinationBorderLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 387, true);
			this.CopyCountryDestinationBorderLabel.TabIndex = 3;
			this.CopyCountryDestinationBorderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LargeSixLabel
			// 
			this.LargeSixLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.LargeSixLabel.Font = new System.Drawing.Font("Tahoma", 16F, System.Drawing.FontStyle.Bold);
			this.LargeSixLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 31, true);
			this.LargeSixLabel.Name = "LargeSixLabel";
			this.LargeSixLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 41, true);
			this.LargeSixLabel.TabIndex = 2;
			this.LargeSixLabel.Text = Enterprise.Customs.GUI.Res.GetString("DC560489-CE7A-4242-A9C3-8B5FACEF9264", "6");
			this.LargeSixLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Section13Panel
			// 
			this.Section13Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section13Panel.Controls.Add(this.Section13DescriptionLabel);
			this.Section13Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(762, 217, true);
			this.Section13Panel.Name = "Section13Panel";
			this.Section13Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 41, true);
			this.Section13Panel.TabIndex = 21;
			// 
			// Section13DescriptionLabel
			// 
			this.Section13DescriptionLabel.AutoSize = true;
			this.Section13DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section13DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section13DescriptionLabel.Name = "Section13DescriptionLabel";
			this.Section13DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 10, true);
			this.Section13DescriptionLabel.TabIndex = 0;
			this.Section13DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("4E929301-4348-4CE8-A838-06AF4F57AA2F", "13 CAP");
			// 
			// Section12Panel
			// 
			this.Section12Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section12Panel.Controls.Add(this.Section12DescriptionLabel);
			this.Section12Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 217, true);
			this.Section12Panel.Name = "Section12Panel";
			this.Section12Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 41, true);
			this.Section12Panel.TabIndex = 20;
			// 
			// Section12DescriptionLabel
			// 
			this.Section12DescriptionLabel.AutoSize = true;
			this.Section12DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section12DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section12DescriptionLabel.Name = "Section12DescriptionLabel";
			this.Section12DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 10, true);
			this.Section12DescriptionLabel.TabIndex = 0;
			this.Section12DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("0ED8A09B-2E84-41AC-A652-7DDC0E055FB4", "12 Value details");
			// 
			// Section11Panel
			// 
			this.Section11Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section11Panel.Controls.Add(this.Section11Description2Label);
			this.Section11Panel.Controls.Add(this.Section11DescriptionLabel);
			this.Section11Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 217, true);
			this.Section11Panel.Name = "Section11Panel";
			this.Section11Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 41, true);
			this.Section11Panel.TabIndex = 19;
			// 
			// Section11Description2Label
			// 
			this.Section11Description2Label.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section11Description2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 16, true);
			this.Section11Description2Label.Name = "Section11Description2Label";
			this.Section11Description2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 23, true);
			this.Section11Description2Label.TabIndex = 1;
			this.Section11Description2Label.Text = Enterprise.Customs.GUI.Res.GetString("A9586411-049E-4FEE-B23E-F054C94F7660", "country");
			// 
			// Section11DescriptionLabel
			// 
			this.Section11DescriptionLabel.AutoSize = true;
			this.Section11DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section11DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section11DescriptionLabel.Name = "Section11DescriptionLabel";
			this.Section11DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 10, true);
			this.Section11DescriptionLabel.TabIndex = 0;
			this.Section11DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("6B8958B2-10BD-4BB6-AF54-83097BB44293", "11 Trad./Prod.");
			// 
			// Section10Panel
			// 
			this.Section10Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section10Panel.Controls.Add(this.Section10Description2Label);
			this.Section10Panel.Controls.Add(this.Section10DescriptionLabel);
			this.Section10Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 217, true);
			this.Section10Panel.Name = "Section10Panel";
			this.Section10Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 41, true);
			this.Section10Panel.TabIndex = 18;
			// 
			// Section10Description2Label
			// 
			this.Section10Description2Label.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section10Description2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 14, true);
			this.Section10Description2Label.Name = "Section10Description2Label";
			this.Section10Description2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 23, true);
			this.Section10Description2Label.TabIndex = 1;
			this.Section10Description2Label.Text = Enterprise.Customs.GUI.Res.GetString("6DD87F70-C029-41DB-933F-C226144B7200", "consigned");
			// 
			// Section10DescriptionLabel
			// 
			this.Section10DescriptionLabel.AutoSize = true;
			this.Section10DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section10DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section10DescriptionLabel.Name = "Section10DescriptionLabel";
			this.Section10DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 10, true);
			this.Section10DescriptionLabel.TabIndex = 0;
			this.Section10DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("49DB93B9-4814-4E9B-B60E-FDA5F34C1630", "10 Country last");
			// 
			// Section9Panel
			// 
			this.Section9Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section9Panel.Controls.Add(this.Section9NoLabel);
			this.Section9Panel.Controls.Add(this.Section9DescriptionLabel);
			this.Section9Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 133, true);
			this.Section9Panel.Name = "Section9Panel";
			this.Section9Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 85, true);
			this.Section9Panel.TabIndex = 16;
			// 
			// Section9NoLabel
			// 
			this.Section9NoLabel.AutoSize = true;
			this.Section9NoLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section9NoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 5, true);
			this.Section9NoLabel.Name = "Section9NoLabel";
			this.Section9NoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 10, true);
			this.Section9NoLabel.TabIndex = 1;
			this.Section9NoLabel.Text = Enterprise.Customs.GUI.Res.GetString("35C6A9CF-5003-4D46-ADBF-2D71822FD8DE", "No");
			// 
			// Section9DescriptionLabel
			// 
			this.Section9DescriptionLabel.AutoSize = true;
			this.Section9DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section9DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section9DescriptionLabel.Name = "Section9DescriptionLabel";
			this.Section9DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 10, true);
			this.Section9DescriptionLabel.TabIndex = 0;
			this.Section9DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("82554904-EDB1-415A-A58D-4E20D5F2C54A", "9 Person responsible for financial settlement");
			// 
			// Section8Panel
			// 
			this.Section8Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section8Panel.Controls.Add(this.Section8Textbox);
			this.Section8Panel.Controls.Add(this.Section8OrganisationFindBox);
			this.Section8Panel.Controls.Add(this.Section8ZDescriptionLabel);
			this.Section8Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 133, true);
			this.Section8Panel.Name = "Section8Panel";
			this.Section8Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 103, true);
			this.Section8Panel.TabIndex = 15;
			// 
			// Section8Textbox
			// 
			this.Section8Textbox.BindTo = "ConsigneeFormattedAddress";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).ConsigneeFormattedAddressInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).ConsigneeFormattedAddress)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section8Textbox, false);
			this.Section8Textbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 22, true);
			this.Section8Textbox.Multiline = true;
			this.Section8Textbox.Name = "Section8Textbox";
			this.Section8Textbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 78, true);
			this.Section8Textbox.TabIndex = 2;
			// 
			// Section8OrganisationFindBox
			// 
			this.Section8OrganisationFindBox.BindTo = "D1_OH_Consignee";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_OH_Consignee)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_OH_ConsigneeInfo)));
			this.Section8OrganisationFindBox.BindToList = "Lookups+ImportersList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.ImportersList)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section8OrganisationFindBox, false);
			this.Section8OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 1, true);
			this.Section8OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.Section8OrganisationFindBox.Name = "Section8OrganisationFindBox";
			this.Section8OrganisationFindBox.ShowDescriptionBox = false;
			this.Section8OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.Section8OrganisationFindBox.TabIndex = 1;
			// 
			// Section8ZDescriptionLabel
			// 
			this.Section8ZDescriptionLabel.AutoSize = true;
			this.Section8ZDescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section8ZDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section8ZDescriptionLabel.Name = "Section8ZDescriptionLabel";
			this.Section8ZDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 10, true);
			this.Section8ZDescriptionLabel.TabIndex = 0;
			this.Section8ZDescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("64C50B6D-3572-4C37-9F5D-B23635856F46", "8 Consignee");
			// 
			// Section7Panel
			// 
			this.Section7Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section7Panel.Controls.Add(this.Section7DescriptionLabel);
			this.Section7Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(588, 93, true);
			this.Section7Panel.Name = "Section7Panel";
			this.Section7Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 41, true);
			this.Section7Panel.TabIndex = 14;
			// 
			// Section7DescriptionLabel
			// 
			this.Section7DescriptionLabel.AutoSize = true;
			this.Section7DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section7DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section7DescriptionLabel.Name = "Section7DescriptionLabel";
			this.Section7DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 10, true);
			this.Section7DescriptionLabel.TabIndex = 0;
			this.Section7DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("BE3C420C-9426-4A85-9D33-5589285672B7", "7 Reference Number");
			// 
			// Section6Panel
			// 
			this.Section6Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section6Panel.Controls.Add(this.Section6CalcDropEdit);
			this.Section6Panel.Controls.Add(this.Section6DescriptionLabel);
			this.Section6Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 93, true);
			this.Section6Panel.Name = "Section6Panel";
			this.Section6Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 41, true);
			this.Section6Panel.TabIndex = 13;
			// 
			// Section6CalcDropEdit
			// 
			this.Section6CalcDropEdit.BindToAmount = "D1_TotalPackages";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_TotalPackages)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_TotalPackagesInfo)));
			this.Section6CalcDropEdit.BindToList = "Lookups+PackagesTypeList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.PackagesTypeList)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.PackagesTypeList)));
			this.Section6CalcDropEdit.BindToUnit = "D1_TotalPackagesPackType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_TotalPackagesPackTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_TotalPackagesPackType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section6CalcDropEdit, false);
			this.Section6CalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, true);
			this.Section6CalcDropEdit.Name = "Section6CalcDropEdit";
			this.Section6CalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.Section6CalcDropEdit.TabIndex = 1;
			this.Section6CalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// Section6DescriptionLabel
			// 
			this.Section6DescriptionLabel.AutoSize = true;
			this.Section6DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section6DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.Section6DescriptionLabel.Name = "Section6DescriptionLabel";
			this.Section6DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 10, true);
			this.Section6DescriptionLabel.TabIndex = 0;
			this.Section6DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("24CA359F-3F6F-4A59-81EE-C6083E3F241C", "6 Total packages");
			// 
			// Section5Panel
			// 
			this.Section5Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section5Panel.Controls.Add(this.Section5DescriptionLabel);
			this.Section5Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 93, true);
			this.Section5Panel.Name = "Section5Panel";
			this.Section5Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 41, true);
			this.Section5Panel.TabIndex = 12;
			// 
			// Section5DescriptionLabel
			// 
			this.Section5DescriptionLabel.AutoSize = true;
			this.Section5DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section5DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section5DescriptionLabel.Name = "Section5DescriptionLabel";
			this.Section5DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 10, true);
			this.Section5DescriptionLabel.TabIndex = 0;
			this.Section5DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("889B6CFE-EF19-45B4-B39D-819AEDC92B85", "5 Items");
			// 
			// Section4Panel
			// 
			this.Section4Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section4Panel.Controls.Add(this.Section4DescriptionLabel);
			this.Section4Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(489, 54, true);
			this.Section4Panel.Name = "Section4Panel";
			this.Section4Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 41, true);
			this.Section4Panel.TabIndex = 10;
			// 
			// Section4DescriptionLabel
			// 
			this.Section4DescriptionLabel.AutoSize = true;
			this.Section4DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section4DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section4DescriptionLabel.Name = "Section4DescriptionLabel";
			this.Section4DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 10, true);
			this.Section4DescriptionLabel.TabIndex = 0;
			this.Section4DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("00B4E8A1-D199-428E-A652-014AFAE3444B", "4 Loading lists");
			// 
			// Section3Panel
			// 
			this.Section3Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section3Panel.Controls.Add(this.Section3DescriptionLabel);
			this.Section3Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 54, true);
			this.Section3Panel.Name = "Section3Panel";
			this.Section3Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 41, true);
			this.Section3Panel.TabIndex = 9;
			// 
			// Section3DescriptionLabel
			// 
			this.Section3DescriptionLabel.AutoSize = true;
			this.Section3DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section3DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section3DescriptionLabel.Name = "Section3DescriptionLabel";
			this.Section3DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 10, true);
			this.Section3DescriptionLabel.TabIndex = 0;
			this.Section3DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("97483429-6B8A-4EC8-B808-6834618C5C29", "3 Forms");
			// 
			// Section2Panel
			// 
			this.Section2Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section2Panel.Controls.Add(this.Section2Textbox);
			this.Section2Panel.Controls.Add(this.Section2OrganisationFindBox);
			this.Section2Panel.Controls.Add(this.Section2DescriptionLabel);
			this.Section2Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 31, true);
			this.Section2Panel.Name = "Section2Panel";
			this.Section2Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 103, true);
			this.Section2Panel.TabIndex = 7;
			// 
			// Section2Textbox
			// 
			this.Section2Textbox.BindTo = "ConsignorFormattedAddress";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).ConsignorFormattedAddressInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).ConsignorFormattedAddress)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section2Textbox, false);
			this.Section2Textbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 22, true);
			this.Section2Textbox.Multiline = true;
			this.Section2Textbox.Name = "Section2Textbox";
			this.Section2Textbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 78, true);
			this.Section2Textbox.TabIndex = 2;
			// 
			// Section2OrganisationFindBox
			// 
			this.Section2OrganisationFindBox.BindTo = "D1_OH_Consignor";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_OH_Consignor)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).D1_OH_ConsignorInfo)));
			this.Section2OrganisationFindBox.BindToList = "Lookups+SuppliersList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.Business.SADH.SADHFormData)(null)).Lookups.SuppliersList)));
			this.Section2OrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 1, true);
			this.Section2OrganisationFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Section2OrganisationFindBox, false);
			this.Section2OrganisationFindBox.Name = "Section2OrganisationFindBox";
			this.Section2OrganisationFindBox.ShowDescriptionBox = false;
			this.Section2OrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.Section2OrganisationFindBox.TabIndex = 1;
			// 
			// Section2DescriptionLabel
			// 
			this.Section2DescriptionLabel.AutoSize = true;
			this.Section2DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section2DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section2DescriptionLabel.Name = "Section2DescriptionLabel";
			this.Section2DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 10, true);
			this.Section2DescriptionLabel.TabIndex = 0;
			this.Section2DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("B1F8E2EB-97D4-4095-A607-7C2DB2EDDB69", "2 Consignor/Exporter");
			// 
			// Section1Panel
			// 
			this.Section1Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Section1Panel.Controls.Add(this.Section1DescriptionLabel);
			this.Section1Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 16, true);
			this.Section1Panel.Name = "Section1Panel";
			this.Section1Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 39, true);
			this.Section1Panel.TabIndex = 8;
			// 
			// Section1DescriptionLabel
			// 
			this.Section1DescriptionLabel.AutoSize = true;
			this.Section1DescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Section1DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Section1DescriptionLabel.Name = "Section1DescriptionLabel";
			this.Section1DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 10, true);
			this.Section1DescriptionLabel.TabIndex = 0;
			this.Section1DescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("7BE217D0-3588-4E06-996E-8C062CAC554E", "1 DECLARATION");
			// 
			// SectionAPanel
			// 
			this.SectionAPanel.Controls.Add(this.SectionALeftBorder);
			this.SectionAPanel.Controls.Add(this.SectionADescriptionLabel);
			this.SectionAPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(557, 6, true);
			this.SectionAPanel.Name = "SectionAPanel";
			this.SectionAPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 89, true);
			this.SectionAPanel.TabIndex = 11;
			// 
			// SectionALeftBorder
			// 
			this.SectionALeftBorder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.SectionALeftBorder.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SectionALeftBorder.Name = "SectionALeftBorder";
			this.SectionALeftBorder.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1, 95, true);
			this.SectionALeftBorder.TabIndex = 0;
			this.SectionALeftBorder.Text = Enterprise.Customs.GUI.Res.GetString("220D796B-111B-4712-B3C3-D96C8C9A5A4C", "label 4");
			// 
			// SectionADescriptionLabel
			// 
			this.SectionADescriptionLabel.AutoSize = true;
			this.SectionADescriptionLabel.Font = new System.Drawing.Font("Tahoma", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SectionADescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.SectionADescriptionLabel.Name = "SectionADescriptionLabel";
			this.SectionADescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 10, true);
			this.SectionADescriptionLabel.TabIndex = 1;
			this.SectionADescriptionLabel.Text = Enterprise.Customs.GUI.Res.GetString("A560CEC9-A7BD-4C1E-B071-2C8BD6DC7CDD", "A  OFFICE OF DESTINATION");
			// 
			// EuropeanCommunityLabel
			// 
			this.EuropeanCommunityLabel.AutoSize = true;
			this.EuropeanCommunityLabel.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
			this.EuropeanCommunityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 6, true);
			this.EuropeanCommunityLabel.Name = "EuropeanCommunityLabel";
			this.EuropeanCommunityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 17, true);
			this.EuropeanCommunityLabel.TabIndex = 0;
			this.EuropeanCommunityLabel.Text = Enterprise.Customs.GUI.Res.GetString("BB36E113-BDB3-4C2E-B67D-6DB42B7589A3", "EUROPEAN COMMUNITY");
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(780, 704, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2B16B68B-2FED-4C0D-A465-FAB0E6BEF047", "Cancel");
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(702, 704, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("094046F4-13B0-4938-8E71-AAAA7DCAD084", "OK");
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// SADHEntryForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(867, 760, true);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.MainPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.SADH.SADHFormData);
			this.DataSourceTypeName = "Enterprise.Customs.Business.SADH.SADHFormData";
			this.Name = "SADHEntryForm";
			this.ShouldSerializeTabPageMethods = false;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2D232425-E29C-4A0E-80BD-AA270F3A69FC", "SAD/H Data Entry");
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.Section54Panel.ResumeLayout(false);
			this.Section54Panel.PerformLayout();
			this.SectionJPanel.ResumeLayout(false);
			this.SectionJPanel.PerformLayout();
			this.Section53Panel.ResumeLayout(false);
			this.Section53Panel.PerformLayout();
			this.Section52Panel.ResumeLayout(false);
			this.Section52Panel.PerformLayout();
			this.Section52CodePanel.ResumeLayout(false);
			this.Section52CodePanel.PerformLayout();
			this.Section51Panel.ResumeLayout(false);
			this.Section51Panel.PerformLayout();
			this.SectionCPanel.ResumeLayout(false);
			this.SectionCPanel.PerformLayout();
			this.Section50Panel.ResumeLayout(false);
			this.Section50Panel.PerformLayout();
			this.SectionBPanel.ResumeLayout(false);
			this.SectionBPanel.PerformLayout();
			this.Section49Panel.ResumeLayout(false);
			this.Section49Panel.PerformLayout();
			this.Section48Panel.ResumeLayout(false);
			this.Section48Panel.PerformLayout();
			this.Section47Panel.ResumeLayout(false);
			this.Section47Panel.PerformLayout();
			this.Section46Panel.ResumeLayout(false);
			this.Section46Panel.PerformLayout();
			this.SectionA1Panel.ResumeLayout(false);
			this.SectionA1Panel.PerformLayout();
			this.Section45Panel.ResumeLayout(false);
			this.Section45Panel.PerformLayout();
			this.Section44Panel.ResumeLayout(false);
			this.Section44Panel.PerformLayout();
			this.Setion42Panel.ResumeLayout(false);
			this.Setion42Panel.PerformLayout();
			this.Section41Panel.ResumeLayout(false);
			this.Section41Panel.PerformLayout();
			this.Section43Panel.ResumeLayout(false);
			this.Section43Panel.PerformLayout();
			this.Section40Panel.ResumeLayout(false);
			this.Section40Panel.PerformLayout();
			this.Section39Panel.ResumeLayout(false);
			this.Section39Panel.PerformLayout();
			this.Section38Panel.ResumeLayout(false);
			this.Section38Panel.PerformLayout();
			this.Section37Panel.ResumeLayout(false);
			this.Section37Panel.PerformLayout();
			this.Section35Panel.ResumeLayout(false);
			this.Section35Panel.PerformLayout();
			this.Section36Panel.ResumeLayout(false);
			this.Section36Panel.PerformLayout();
			this.Section34Panel.ResumeLayout(false);
			this.Section34Panel.PerformLayout();
			this.Section33Panel.ResumeLayout(false);
			this.Section33Panel.PerformLayout();
			this.Section31Panel.ResumeLayout(false);
			this.Section31Panel.PerformLayout();
			this.Section31bPanel.ResumeLayout(false);
			this.Section31bPanel.PerformLayout();
			this.Section32Panel.ResumeLayout(false);
			this.Section32Panel.PerformLayout();
			this.Section30Panel.ResumeLayout(false);
			this.Section30Panel.PerformLayout();
			this.Section28Panel.ResumeLayout(false);
			this.Section28Panel.PerformLayout();
			this.Section26Panel.ResumeLayout(false);
			this.Section26Panel.PerformLayout();
			this.Section25Panel.ResumeLayout(false);
			this.Section25Panel.PerformLayout();
			this.Section27Panel.ResumeLayout(false);
			this.Section27Panel.PerformLayout();
			this.Section24Panel.ResumeLayout(false);
			this.Section24Panel.PerformLayout();
			this.Section23Panel.ResumeLayout(false);
			this.Section23Panel.PerformLayout();
			this.Section22Panel.ResumeLayout(false);
			this.Section22Panel.PerformLayout();
			this.Section21Panel.ResumeLayout(false);
			this.Section21Panel.PerformLayout();
			this.Section20Panel.ResumeLayout(false);
			this.Section20Panel.PerformLayout();
			this.Section19Panel.ResumeLayout(false);
			this.Section19Panel.PerformLayout();
			this.Section18Panel.ResumeLayout(false);
			this.Section18Panel.PerformLayout();
			this.Section14Panel.ResumeLayout(false);
			this.Section14Panel.PerformLayout();
			this.Section17bPanel.ResumeLayout(false);
			this.Section17bPanel.PerformLayout();
			this.Section15bPanel.ResumeLayout(false);
			this.Section15bPanel.PerformLayout();
			this.Section17Panel.ResumeLayout(false);
			this.Section17Panel.PerformLayout();
			this.Section16Panel.ResumeLayout(false);
			this.Section16Panel.PerformLayout();
			this.Section15Panel.ResumeLayout(false);
			this.Section15Panel.PerformLayout();
			this.Section13Panel.ResumeLayout(false);
			this.Section13Panel.PerformLayout();
			this.Section12Panel.ResumeLayout(false);
			this.Section12Panel.PerformLayout();
			this.Section11Panel.ResumeLayout(false);
			this.Section11Panel.PerformLayout();
			this.Section10Panel.ResumeLayout(false);
			this.Section10Panel.PerformLayout();
			this.Section9Panel.ResumeLayout(false);
			this.Section9Panel.PerformLayout();
			this.Section8Panel.ResumeLayout(false);
			this.Section8Panel.PerformLayout();
			this.Section7Panel.ResumeLayout(false);
			this.Section7Panel.PerformLayout();
			this.Section6Panel.ResumeLayout(false);
			this.Section6Panel.PerformLayout();
			this.Section5Panel.ResumeLayout(false);
			this.Section5Panel.PerformLayout();
			this.Section4Panel.ResumeLayout(false);
			this.Section4Panel.PerformLayout();
			this.Section3Panel.ResumeLayout(false);
			this.Section3Panel.PerformLayout();
			this.Section2Panel.ResumeLayout(false);
			this.Section2Panel.PerformLayout();
			this.Section1Panel.ResumeLayout(false);
			this.Section1Panel.PerformLayout();
			this.SectionAPanel.ResumeLayout(false);
			this.SectionAPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		protected internal Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		new protected internal Enterprise.ZArchitecture.GUI.ZButton CancelButton;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel SectionAPanel;
		protected internal CargoWise.Windows.UI.KLabel EuropeanCommunityLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section1Panel;
		protected internal CargoWise.Windows.UI.KLabel Section1DescriptionLabel;
		protected internal CargoWise.Windows.UI.KLabel SectionADescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section2Panel;
		protected internal CargoWise.Windows.UI.KLabel Section2DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section3Panel;
		protected internal CargoWise.Windows.UI.KLabel Section3DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section4Panel;
		protected internal CargoWise.Windows.UI.KLabel Section4DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section8Panel;
		protected internal CargoWise.Windows.UI.KLabel Section8ZDescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section7Panel;
		protected internal CargoWise.Windows.UI.KLabel Section7DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section6Panel;
		protected internal CargoWise.Windows.UI.KLabel Section6DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section5Panel;
		protected internal CargoWise.Windows.UI.KLabel Section5DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section9Panel;
		protected internal CargoWise.Windows.UI.KLabel Section9DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section10Panel;
		protected internal CargoWise.Windows.UI.KLabel Section10DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section12Panel;
		protected internal CargoWise.Windows.UI.KLabel Section12DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section11Panel;
		protected internal CargoWise.Windows.UI.KLabel Section11DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section13Panel;
		protected internal CargoWise.Windows.UI.KLabel Section13DescriptionLabel;
		protected internal CargoWise.Windows.UI.KLabel Section9NoLabel;
		protected internal CargoWise.Windows.UI.KLabel CopyCountryDestinationBorderLabel;
		protected internal CargoWise.Windows.UI.KLabel LargeSixLabel2;
		protected internal CargoWise.Windows.UI.KLabel LargeSixLabel;
		protected internal CargoWise.Windows.UI.KLabel SectionALeftBorder;
		protected internal CargoWise.Windows.UI.KLabel Section10Description2Label;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section16Panel;
		protected internal CargoWise.Windows.UI.KLabel Section16DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section15Panel;
		protected internal CargoWise.Windows.UI.KLabel Section15DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section17Panel;
		protected internal CargoWise.Windows.UI.KLabel Section17DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section17bPanel;
		protected internal CargoWise.Windows.UI.KLabel Section17bLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section15bPanel;
		protected internal Enterprise.ZArchitecture.ZTextBox Section15bTextbox;
		protected internal CargoWise.Windows.UI.KLabel Section15bLabel;
		protected internal CargoWise.Windows.UI.KLabel Section11Description2Label;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section14Panel;
		protected internal CargoWise.Windows.UI.KLabel Section14DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section19Panel;
		protected internal CargoWise.Windows.UI.KLabel Section19DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section20Panel;
		protected internal CargoWise.Windows.UI.KLabel Section20DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section22Panel;
		protected internal CargoWise.Windows.UI.KLabel Section22DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section21Panel;
		protected internal CargoWise.Windows.UI.KLabel Section21DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section24Panel;
		protected internal CargoWise.Windows.UI.KLabel Section24DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section23Panel;
		protected internal CargoWise.Windows.UI.KLabel Section23DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section27Panel;
		protected internal CargoWise.Windows.UI.KLabel Section27DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section26Panel;
		protected internal CargoWise.Windows.UI.KLabel Section26Description2Label;
		protected internal CargoWise.Windows.UI.KLabel Section26DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section25Panel;
		protected internal CargoWise.Windows.UI.KLabel Section25Description2Label;
		protected internal CargoWise.Windows.UI.KLabel Section25DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section28Panel;
		protected internal CargoWise.Windows.UI.KLabel Section28DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section30Panel;
		protected internal CargoWise.Windows.UI.KLabel Section30DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section31bPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section32Panel;
		protected internal CargoWise.Windows.UI.KLabel Section32DescriptionLabel;
		protected internal Enterprise.ZArchitecture.ZTextBox Section31bDescriptionTextbox;
		protected internal CargoWise.Windows.UI.KLabel Section31bDescriptionPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section31Panel;
		protected internal CargoWise.Windows.UI.KLabel Section31NumberLabel;
		protected internal CargoWise.Windows.UI.KLabel Section31DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section34Panel;
		protected internal CargoWise.Windows.UI.KLabel Section34DescriptionLabel;
		protected internal CargoWise.Windows.UI.KLabel Section33DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section35Panel;
		protected internal CargoWise.Windows.UI.KLabel Section35DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section36Panel;
		protected internal CargoWise.Windows.UI.KLabel Section36DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section39Panel;
		protected internal CargoWise.Windows.UI.KLabel Section39DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section38Panel;
		protected internal CargoWise.Windows.UI.KLabel Section38DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section37Panel;
		protected internal CargoWise.Windows.UI.KLabel Section37DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section40Panel;
		protected internal CargoWise.Windows.UI.KLabel Section40DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Setion42Panel;
		protected internal CargoWise.Windows.UI.KLabel Section42DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section41Panel;
		protected internal CargoWise.Windows.UI.KLabel Section41DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section43Panel;
		protected internal CargoWise.Windows.UI.KLabel Section43DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section44Panel;
		protected internal CargoWise.Windows.UI.KLabel Section44NumberLabel;
		protected internal CargoWise.Windows.UI.KLabel Section44DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel SectionA1Panel;
		protected internal CargoWise.Windows.UI.KLabel SectionA1DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section45Panel;
		protected internal CargoWise.Windows.UI.KLabel Section45DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section47Panel;
		protected internal CargoWise.Windows.UI.KLabel Section47NumberLabel;
		protected internal CargoWise.Windows.UI.KLabel Section47DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section46Panel;
		protected internal CargoWise.Windows.UI.KLabel Section46DescriptionLAbel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section49Panel;
		protected internal CargoWise.Windows.UI.KLabel Section49DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section48Panel;
		protected internal CargoWise.Windows.UI.KLabel Section48DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section47bPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel SectionBPanel;
		protected internal CargoWise.Windows.UI.KLabel SectionBDescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section50Panel;
		protected internal CargoWise.Windows.UI.KLabel Section50NoLabel;
		protected internal CargoWise.Windows.UI.KLabel Section50DescriptionLabel;
		protected internal CargoWise.Windows.UI.KLabel Section50PlaceAndDateLabel;
		protected internal CargoWise.Windows.UI.KLabel Section50RepresentedByLabel;
		protected internal CargoWise.Windows.UI.KLabel Section50SignatureLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section51cPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section51bPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section51aPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section51Panel;
		protected internal CargoWise.Windows.UI.KLabel Section51NumberLabel;
		protected internal CargoWise.Windows.UI.KLabel Section51DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel SectionCPanel;
		protected internal CargoWise.Windows.UI.KLabel SectionCDescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section52Panel;
		protected internal CargoWise.Windows.UI.KLabel Section52NotValidForLabel;
		protected internal CargoWise.Windows.UI.KLabel Section52DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section51fPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section51ePanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section51dPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section52CodePanel;
		protected internal CargoWise.Windows.UI.KLabel Section52CodeLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section53Panel;
		protected internal CargoWise.Windows.UI.KLabel Section53DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section54Panel;
		protected internal CargoWise.Windows.UI.KLabel Section54SignatureAndNameLabel;
		protected internal CargoWise.Windows.UI.KLabel Section54DescriptionLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel SectionJPanel;
		protected internal CargoWise.Windows.UI.KLabel SectionJDescriptionLabel;
		protected internal CargoWise.Windows.UI.KLabel FormLeftBorderLabel;
		protected internal CargoWise.Windows.UI.KLabel FormRightBorderLabel;
		protected internal Enterprise.MasterFiles.GUI.ZOrganisationFindBox Section2OrganisationFindBox;
		protected internal Enterprise.ZArchitecture.ZTextBox Section2Textbox;
		protected internal Enterprise.ZArchitecture.ZTextBox Section14TextBox;
		protected internal Enterprise.ZArchitecture.ZTextBox Section8Textbox;
		protected internal Enterprise.MasterFiles.GUI.ZOrganisationFindBox Section8OrganisationFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit Section25DropEdit;
		protected internal CargoWise.Windows.UI.KLabel FormBottomBorderLabel;
		protected internal CargoWise.Windows.UI.KLabel Section31bDescriptionLabel;
		protected internal CargoWise.Windows.UI.KLabel Section31bMarksNumbersLabel;
		protected internal Enterprise.ZArchitecture.ZTextBox Section31bMarksNumbersTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		protected Enterprise.ZArchitecture.GUI.ZPanel Section33Panel;
		protected internal Enterprise.ZArchitecture.ZCalcEdit Section23ExchangeRateTextbox;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		protected internal Enterprise.ZArchitecture.ZTextBox Section17bTextbox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox Section17CodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox Section16CodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox Section34CodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox Section27CodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox Section15CodeFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCalcFindBox Section22TotalAmountInvoicedCalcFindBox;
		protected internal CargoWise.Windows.UI.VerticalLabel CopyCountryDestinationVerticalLabel;
		protected internal Enterprise.ZArchitecture.ZCalcEdit Section42CalcEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit Section6CalcDropEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit Section20DropEdit;
		protected internal Enterprise.ZArchitecture.ZTextBox Section41TextBox;
		protected internal Enterprise.ZArchitecture.ZCalcEdit Section41CalcEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit Section21FlightDateDateEdit;
		protected internal CargoWise.Windows.UI.KLabel Section21FlightNoLabel;
		protected internal Enterprise.ZArchitecture.ZTextBox Section21FlightNoTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZPanel Section18Panel;
		protected internal CargoWise.Windows.UI.KLabel Section18DescriptionLabel;
		protected internal CargoWise.Windows.UI.KLabel Section31bQuantityLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit Section31bQuantityCalcDropEdit;
		protected internal Enterprise.ZArchitecture.ZTextBox Section42TextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit Section35CalcDropEdit;
		protected internal CargoWise.Windows.UI.KLabel Section21FlightDateLabel;
		protected internal CargoWise.Windows.UI.KLabel Section21VesselLabel;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox Section21VesselCodeFindBox;
		protected internal ZArchitecture.ZTextBox Box18TransportIDBox;
		protected internal CargoWise.Windows.UI.KLabel Box18TransportIDLabel;
		protected internal CargoWise.Windows.UI.KLabel DepartureTransportIDLabel;
		protected internal ZArchitecture.ZTextBox DepartureTransportIDBox;
	}
}
