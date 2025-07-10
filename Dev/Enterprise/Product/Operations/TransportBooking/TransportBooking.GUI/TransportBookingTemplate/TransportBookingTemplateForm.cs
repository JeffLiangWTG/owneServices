using System;
using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingTemplateForm : ZTemplateForm, IButtonDeleteTextOverride
	{
		public TransportBookingTemplateForm(DtbBookingTmpl transportBookingTemplate)
			: base(transportBookingTemplate)
		{
		}

		public override string FormCaption
		{
			get
			{
				var templateID = Template != null ? ZString.Format("{0} - {1}", Template.KT_Code, Template.KT_DescriptionMultilingual) : ZString.Empty;
				return Res.GetString("TransportBookings|TransportBookingTemplate|FromCaptionPrefix", "Transport Booking Template {0}", templateID);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			Unhook();

			base.SetDataBinding(dataSource, dataMember);

			Hook();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateRatingVisibility();
		}

		void Hook()
		{
			if (Template != null)
			{
				Template.KT_RatingFreightModeInfo.ValueChanged += new EventHandler(KT_RatingFreightModeInfo_ValueChanged);
			}

			UpdateRatingVisibility();
		}

		void Unhook()
		{
			if (Template != null)
			{
				Template.KT_RatingFreightModeInfo.ValueChanged -= new EventHandler(KT_RatingFreightModeInfo_ValueChanged);
			}
		}

		void KT_RatingFreightModeInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateRatingVisibility();
		}

		void UpdateRatingVisibility()
		{
			if (Template != null && BookingInstructionTemplateGrid != null)
			{
				var freightMode = Template.KT_RatingFreightMode;
				var showContainerRateable = freightMode == RatingFreightModes.Codes.Containerised || freightMode == RatingFreightModes.Codes.Both;
				var showLooseRateable = freightMode == RatingFreightModes.Codes.Loose || freightMode == RatingFreightModes.Codes.Both;

				BookingInstructionTemplateGrid.RunAfterBind(delegate
				{
					BookingInstructionTemplateGrid.SetColumnVisible(showContainerRateable, DtbBookingInstructionTmplSchema.Constants.K2_IsContainerRateable);
					BookingInstructionTemplateGrid.SetColumnVisible(showLooseRateable, DtbBookingInstructionTmplSchema.Constants.K2_IsLooseRateable);
				});
			}
		}

		DtbBookingTmpl Template
		{
			get { return (DtbBookingTmpl)DataSource; }
		}

		string IButtonDeleteTextOverride.DeleteButtonText
		{
			get { return Res.GetString("498ea9e7-3e3c-4b06-af97-fb083e516b41", "Deactivate"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.DirectionDropEdit = new ZDropEdit();
			this.zTextBox1 = new ZArchitecture.ZTextBox();
			this.DescriptionTextbox = new ZArchitecture.ZTranslatableTextControl();
			this.BookingInstructionTemplateGroupBox = new ZGroupBox();
			this.BookingInstructionTemplateGrid = new ZArchitecture.ZGrid();
			this.BookingTemplatePanel = new ZPanel();
			this.zCheckBox1 = new ZCheckBox();
			this.RatingFreightModeDropEdit = new ZDropEdit();
			this.IsHiddenCheckBox = new ZCheckBox();
			this.MainTabPage.SuspendLayout();
			this.BookingInstructionTemplateGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BookingInstructionTemplateGrid)).BeginInit();
			this.BookingTemplatePanel.SuspendLayout();
			this.MainTabPage.Controls.Add(this.BookingInstructionTemplateGroupBox);
			this.MainTabPage.Controls.Add(this.BookingTemplatePanel);
			// 
			// DirectionDropEdit
			// 
			this.DirectionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DirectionDropEdit, "KT_Direction");
			this.DirectionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 64, true);
			this.DirectionDropEdit.Name = "DirectionDropEdit";
			this.DirectionDropEdit.PreBoundMaxLength = 3;
			this.DirectionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.DirectionDropEdit.TabIndex = 2;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "KT_Code");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 12, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// DescriptionTextbox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextbox, "KT_Description");
			this.DescriptionTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 38, true);
			this.DescriptionTextbox.Name = "DescriptionTextbox";
			this.DescriptionTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.DescriptionTextbox.TabIndex = 1;
			// 
			// BookingInstructionTemplateGroupBox
			// 
			this.BookingInstructionTemplateGroupBox.CaptionResourceString = Enterprise.TransportBookings.GUI.Res.GetData("TransportBookingTemplateForm|aa3f1022-4e11-4e6e-a664-e588b3662431", "Instructions");
			this.BookingInstructionTemplateGroupBox.Controls.Add(this.BookingInstructionTemplateGrid);
			this.BookingInstructionTemplateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingInstructionTemplateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 118, true);
			this.BookingInstructionTemplateGroupBox.Name = "BookingInstructionTemplateGroupBox";
			this.BookingInstructionTemplateGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.BookingInstructionTemplateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 229, true);
			this.BookingInstructionTemplateGroupBox.TabIndex = 1;
			this.BookingInstructionTemplateGroupBox.TabStop = false;
			// 
			// BookingInstructionTemplateGrid
			// 
			this.BookingInstructionTemplateGrid.AllowNavigation = false;
			this.BookingInstructionTemplateGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.BookingInstructionTemplateGrid, "Instructions");
			this.BookingInstructionTemplateGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "K2_Sequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "K2_InstructionType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "K2_OrgType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = "K2_PackageType";
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "K2_DropMode";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo1.ColumnName = "K2_IsContainerRateable";
			zCheckBoxColumnStyleInfo2.ColumnName = "K2_IsLooseRateable";
			this.BookingInstructionTemplateGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.BookingInstructionTemplateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.BookingInstructionTemplateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.BookingInstructionTemplateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.BookingInstructionTemplateGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.BookingInstructionTemplateGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.BookingInstructionTemplateGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.BookingInstructionTemplateGrid.CopySelectedRowsAllowed = true;
			this.BookingInstructionTemplateGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingInstructionTemplateGrid.GridId = "bed937cf-830c-495a-86a4-2a119b895e90";
			this.BookingInstructionTemplateGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BookingInstructionTemplateGrid.LayoutKey = "BookingInstructionTemplateGrid";
			this.BookingInstructionTemplateGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 18, true);
			this.BookingInstructionTemplateGrid.Name = "BookingInstructionTemplateGrid";
			this.BookingInstructionTemplateGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 206, true);
			this.BookingInstructionTemplateGrid.TabIndex = 0;
			// 
			// BookingTemplatePanel
			// 
			this.BookingTemplatePanel.Controls.Add(this.IsHiddenCheckBox);
			this.BookingTemplatePanel.Controls.Add(this.RatingFreightModeDropEdit);
			this.BookingTemplatePanel.Controls.Add(this.zCheckBox1);
			this.BookingTemplatePanel.Controls.Add(this.DirectionDropEdit);
			this.BookingTemplatePanel.Controls.Add(this.DescriptionTextbox);
			this.BookingTemplatePanel.Controls.Add(this.zTextBox1);
			this.BookingTemplatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.BookingTemplatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BookingTemplatePanel.Name = "BookingTemplatePanel";
			this.BookingTemplatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 118, true);
			this.BookingTemplatePanel.TabIndex = 0;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.zCheckBox1, "KT_IsSystem");
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 13, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 18, true);
			this.zCheckBox1.TabIndex = 3;
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// RatingFreightModeDropEdit
			// 
			this.RatingFreightModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RatingFreightModeDropEdit, "KT_RatingFreightMode");
			this.RatingFreightModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 90, true);
			this.RatingFreightModeDropEdit.Name = "RatingFreightModeDropEdit";
			this.RatingFreightModeDropEdit.PreBoundMaxLength = 3;
			this.RatingFreightModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.RatingFreightModeDropEdit.TabIndex = 4;
			// 
			// IsHiddenCheckBox
			// 
			this.IsHiddenCheckBox.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.IsHiddenCheckBox, "KT_IsActive");
			this.IsHiddenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsHiddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(509, 37, true);
			this.IsHiddenCheckBox.Name = "IsHiddenCheckBox";
			this.IsHiddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 18, true);
			this.IsHiddenCheckBox.TabIndex = 5;
			this.IsHiddenCheckBox.UseVisualStyleBackColor = true;
			this.BookingInstructionTemplateGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BookingInstructionTemplateGrid)).EndInit();
			this.BookingTemplatePanel.ResumeLayout(false);
			this.BookingTemplatePanel.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}
	}
}
