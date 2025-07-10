using System.Linq;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineLinkControllingMsgHeaderValidation : AutoInvoiceLineLinkControllingMsgHeaderValidation
	{
		public InvoiceLineLinkControllingMsgHeaderValidation(AutoInvoiceLineLinkControllingMsgHeader parent) : base(parent)
		{
		}

		public new InvoiceLineLinkControllingMsgHeader Parent => (InvoiceLineLinkControllingMsgHeader)base.Parent;

		protected override void CheckIsLinkedCMHeader()
		{
			base.CheckIsLinkedCMHeader();

			var parent = Parent;
			var linkedItem = parent.ControllingMessageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().FirstOrDefault(x => x.InvoicelinePK == parent.Invoiceline.PK);
			if (linkedItem != null)
			{
				parent.IsLinkedCMHeaderInfo.AddAllNotificationsFrom(linkedItem.LinkInfo);
			}
		}
	}
}
