using System.Linq;
using Enterprise.Customs.ASYCUDA.Business;
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class TransferHeaderMessageChooserItemValidation : MessageChooserItemValidation
	{
		public TransferHeaderMessageChooserItemValidation(TransferHeaderMessageChooserItem parent) : base(parent)
		{
		}

		protected new TransferHeaderMessageChooserItem Parent => (TransferHeaderMessageChooserItem)base.Parent;

		bool IsArrivalMessage => Parent.Chooser.IsArrivalMessage;

		protected override void CheckChecked()
		{
			if (Parent.Checked)
			{
				if (Parent.Chooser.SelectedCount > 1)
				{
					Parent.CheckedInfo.AddError(Res.GetString("370DF656-3090-46ED-9FC2-E0596F416576", "More than 1 transfer is selected"));
				}
				else if (!Parent.TransferHeader.TransferBills.Any(x => x.ATB_ABL_Bill.IsValid))
				{
					Parent.CheckedInfo.AddWarning(TransferHasNoValidBills);
				}
				else if (IsArrivalMessage && Parent.TransferHeader.TransferBills.All(x => x.ATB_MessageStatus == US.AIM.Messaging.AIMTransferStatusCodes.Codes.Arrived))
				{
					Parent.CheckedInfo.AddWarning(TransferAlreadyArrivedWarning);
				}
			}
		}

		public static string TransferAlreadyArrivedWarning => Res.GetString("BC8AF0BB-F759-46A0-BEE1-2A896D2021F5", "This transfer appears to be already Arrived.  Are you sure you wish to send this again?");
		public static string TransferHasNoValidBills => Res.GetString("CAE7D869-1970-418C-988F-3DCFA410E1D1", "This transfer does not have any valid bill. No message will be sent.");
	}
}
