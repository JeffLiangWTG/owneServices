using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class SendProductInformationRequestForm : ZChildForm
	{
		public SendProductInformationRequestForm()
		{
			InitializeComponent();
		}

		public SendProductInformationRequestForm(ProductInformation information)
			: base(information)
		{
			ProductInformation = Argument.NotNull(information, nameof(information));
			InitializeComponent();
		}

		protected ProductInformation ProductInformation { get; }

		NonPersistentCopyRecipientCollection NonPersistentAddressOverrideColumnStyleInfoGetCopyRecipients(ProductInformationDeliveryContact deliveryContact) => deliveryContact.EmailToRecipients;

		public override string FormHeading => ResString.GetMultilingualString("Enterprise.Customs.GUI.SendProductInformationRequestForm|SendProductInformationRequest", "Send Product Information Request");

		#region Delivery

		internal void DeliverButton_Click(object sender, EventArgs e)
		{
			if (Sender.TrySend() is SendResult.Failure failure)
			{
				Globals.Message.ShowError(failure.ErrorMessage);
			}
			else
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("Enterprise.Customs.GUI.SendProductInformationRequestForm|Confirmation", "Product Information Delivered Successfully"));
				DialogResult = DialogResult.OK;
			}
		}

		#endregion

		#region Close

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		#endregion

		protected virtual ProductInformationSender Sender
		{
			get { return new ProductInformationSender(ProductInformation); }
		}
	}
}
