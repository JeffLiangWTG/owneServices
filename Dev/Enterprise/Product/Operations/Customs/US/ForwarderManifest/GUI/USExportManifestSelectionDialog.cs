using System;
using CargoWise.Common;
using Enterprise.Customs.US.ForwarderManifest.Business;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class USExportManifestSelectionDialog : ASYCUDA.GUI.AsycudaItemSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		public USExportManifestSelectionDialog() : base()
		{
			InitializeComponent();
		}

		public USExportManifestSelectionDialog(UEMMessageChooser messageChooser, string itemsType)
			: base(messageChooser, itemsType)
		{
			chooser = Argument.NotNull(messageChooser, nameof(messageChooser));
			ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 472);
			InitializeComponent();
			InitializeColumns();
			InitializeMessages();
		}
		readonly UEMMessageChooser chooser;

		void InitializeColumns()
		{
			var blActionTypeColumn = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			blActionTypeColumn.ColumnName = nameof(UEMMessageChooserItem.ActionType);
			blActionTypeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			blActionTypeColumn.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("163AEA02-0E96-40DB-AC0B-396872B77D20", "BL Action Type");

			var manifestQtyTextBoxColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			manifestQtyTextBoxColumn.ColumnName = nameof(UEMMessageChooserItem.ManifestQty);
			manifestQtyTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			manifestQtyTextBoxColumn.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("8416AA30-BED1-4060-81CC-F06507C30215", "Manifest Qty");

			ItemsGrid.ColumnStyles.Add(blActionTypeColumn);
			ItemsGrid.ColumnStyles.Add(manifestQtyTextBoxColumn);
		}

		void InitializeMessages()
		{
			var messageControl = new ZArchitecture.GUI.ZTabControl();

			messageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			messageControl.Name = "MessageDetailsTabControl";
			messageControl.SelectedIndex = 0;
			messageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 195, true);
			messageControl.TabIndex = 2;

			var messageDetailsTabPage = new ZArchitecture.GUI.ZTabPage();
			messageDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.ForwarderManifest.GUI.Res.GetData("E39B97D3-C3D5-4197-9AB7-1640CE4A7197", "Message Details");
			messageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			messageDetailsTabPage.Name = "MessageTextTabPage";
			messageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			messageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 173, true);
			messageDetailsTabPage.TabIndex = 1;
			messageDetailsTabPage.UseVisualStyleBackColor = true;

			var messageTextTextBox = new ZArchitecture.ZTextBox();
			LabelCaptionRenderProvider.SetLabelCaptionVisible(messageTextTextBox, false);
			BindingSource.SetBindingMember(messageTextTextBox, "ChooserItems.Message");
			messageTextTextBox.CaptionResourceString = null;
			messageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			messageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			messageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			messageTextTextBox.Multiline = true;
			messageTextTextBox.Name = "MessageTextTextBox";
			messageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			messageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 167, true);
			messageTextTextBox.TabIndex = 0;

			messageDetailsTabPage.Controls.Add(messageTextTextBox);
			messageControl.Controls.Add(messageDetailsTabPage);
			Controls.Add(messageControl);
		}
	}
}
