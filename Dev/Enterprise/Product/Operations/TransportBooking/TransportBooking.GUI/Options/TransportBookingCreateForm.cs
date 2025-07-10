using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Options
{
	public partial class TransportBookingCreateForm : ZChildForm
	{
		public TransportBookingCreateForm()
		{
			InitializeComponent();
		}

		public TransportBookingCreateForm(TransportBookingDocumentOptions options)
			: base(options)
		{
			InitializeComponent();
			this.MessageLabel.CaptionResourceString = options.MessageLabelText;

			ShowHidePanels();
		}

		void ShowHidePanels()
		{
			var height = MainStatusBar.Height;

			height += ShowAndGetPanelHeight(MessagePanel, !DocumentOptions.ShowTemplateSelection);
			height += ShowAndGetPanelHeight(TemplatePanel, DocumentOptions.ShowTemplateSelection);
			height += ShowAndGetPanelHeight(CreateButtonsPanel, !DocumentOptions.ShowAutoDelivery);

			ClientSize = ControlDpiScalingHelper.NewScaledSize(ClientSize.Width, height, false);
		}

		int ShowAndGetPanelHeight(Control control, bool visible)
		{
			control.Visible = visible;
			return visible ? control.Height : 0;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void CreateCancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		void CreateOpenButton_Click(object sender, EventArgs e)
		{
			ValidateAndContinue(TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner);
		}

		void CreateOpenInstructionButton_Click(object sender, EventArgs e)
		{
			ValidateAndContinue(TransportBookingDocumentOptions.DeliveryOption.OpenInstructionDesigner);
		}

		void ValidateAndContinue(TransportBookingDocumentOptions.DeliveryOption deliveryOption)
		{
			if (BusinessEntity.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DocumentOptions.DeliveryOptions = deliveryOption;
				DialogResult = DialogResult.Yes;
			}
		}

		TransportBookingDocumentOptions DocumentOptions
		{
			get { return (TransportBookingDocumentOptions)BusinessEntity; }
		}
	}
}
