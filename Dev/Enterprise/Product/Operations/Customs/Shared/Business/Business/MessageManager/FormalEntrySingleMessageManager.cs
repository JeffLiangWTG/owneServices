using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class FormalEntrySingleMessageManager : SingleMessageManager
	{
		protected FormalEntrySingleMessageManager(CusEntryHeader entryHeader)
		{
			this.EntryHeader = entryHeader;
		}

		protected readonly CusEntryHeader EntryHeader;

		public override BusinessObject BusinessObject
		{
			get { return EntryHeader; }
		}

		protected internal override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();

			string detailedErrorMessage;
			if (EntryHeader.Declaration != null && !EntryHeader.Declaration.Invoices.AreChargesBalancedForInvoices(out detailedErrorMessage))
			{
				result.AddError(Res.GetString("d25e5a6a-6d5f-4f87-9fc4-04ee359a6de7", "Current apportionment is not balanced, {0}. Please click Brokerage > Perform Apportionment to correct calculation or remove the specified cause of imbalance.", detailedErrorMessage));
			}

			return result;
		}

		protected override BusinessObject BusinessObjectForNotification
		{
			get { return EntryHeader.Declaration; }
		}
	}
}
