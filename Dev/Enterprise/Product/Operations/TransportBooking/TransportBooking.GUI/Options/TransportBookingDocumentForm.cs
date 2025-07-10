using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Options
{
	public partial class TransportBookingDocumentForm : ZChildForm
	{
		public TransportBookingDocumentForm()
		{
			InitializeComponent();
		}

		public TransportBookingDocumentForm(TransportBookingDocumentOptions options)
			: base(options)
		{
			InitializeComponent();

			ShowHidePanels();
			MessageLabel.ForeColor = options.ShowCommencedError ? Color.Red : Color.Black;
			MessageLabel.CaptionResourceString = options.MessageLabelText;
		}

		void ShowHidePanels()
		{
			var height = MainStatusBar.Height;

			height += ShowAndGetPanelHeight(MessagePanel, !DocumentOptions.ShowTemplateSelection || DocumentOptions.ShowCommencedError);
			height += ShowAndGetPanelHeight(TemplatePanel, DocumentOptions.ShowTemplateSelection);
			height += ShowAndGetPanelHeight(ButtonsPanel, true);

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

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		void DeliverButton_Click(object sender, EventArgs e)
		{
			ValidateAndContinue(TransportBookingDocumentOptions.DeliveryOption.AutoDelivery);
		}

		void OpenStandardDesignerButton_Click(object sender, EventArgs e)
		{
			ValidateAndContinue(TransportBookingDocumentOptions.DeliveryOption.OpenStandardDesigner);
		}

		void OpenInstructionDesignerButton_Click(object sender, EventArgs e)
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

#if DEBUG

namespace Enterprise.TransportBookings.GUI.Options
{
	public partial class TransportBookingDocumentForm
	{
		public TransportBookingDocumentOptions DocumentOptions_ForTesting
		{
			get { return DocumentOptions; }
		}

		public TransportBookingDocumentOptions.DeliveryOption DeliveryOptionsResultForTesting { get; set; }

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (DeliveryOptionsResultForTesting != TransportBookingDocumentOptions.DeliveryOption.None)
			{
				DocumentOptions.DeliveryOptions = DeliveryOptionsResultForTesting;
			}
		}
	}
}

#endif
