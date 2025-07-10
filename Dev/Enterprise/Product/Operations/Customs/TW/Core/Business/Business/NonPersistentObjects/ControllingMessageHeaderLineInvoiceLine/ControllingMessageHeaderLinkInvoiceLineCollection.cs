using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class ControllingMessageHeaderLinkInvoiceLineCollection : NonPersistentBusinessObjectCollection<ControllingMessageHeaderLinkInvoiceLine>
	{
		public ControllingMessageHeaderLinkInvoiceLineCollection(CusTWControllingMessageHeader supporter)
			: base(supporter.Factory)
		{
			ControllingMessageHeader = supporter;
		}

		CusTWControllingMessageHeader ControllingMessageHeader { get; }

		InvoiceLineCompleteCollection InvoiceLines => ControllingMessageHeader.EntryInstruction?.JobDeclaration?.InvoiceLines;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ControllingMessageHeaderLinkInvoiceLine(ControllingMessageHeader, null);
		}

		public override void Load()
		{
			RebuildElements();
		}

		public void RebuildElements()
		{
			RemoveAll();
			BuildElements();
		}

		void BuildElements()
		{
			if (InvoiceLines != null)
			{
				foreach (JobComInvoiceLine line in InvoiceLines)
				{
					AddControllingMessageHeaderLinkInvoiceLine(line, line.InvoiceLineRelatedControllingMsgHeadersGenPivots.Contains(ControllingMessageHeader));
				}
			}
		}

		void AddControllingMessageHeaderLinkInvoiceLine(JobComInvoiceLine line, ZBool isLink)
		{
			var controllingMessageHeaderLinkInvoiceLine = new ControllingMessageHeaderLinkInvoiceLine(ControllingMessageHeader, line);
			using (controllingMessageHeaderLinkInvoiceLine.SuspendSettingHasChanges())
			using (controllingMessageHeaderLinkInvoiceLine.SuspendLinkSetting())
			using (controllingMessageHeaderLinkInvoiceLine.GetValidationSuspender())
			{
				controllingMessageHeaderLinkInvoiceLine.Link = isLink;
				Add(controllingMessageHeaderLinkInvoiceLine);
			}
		}

		public void DeleteControllingMessageHeaderLinkInvoiceLineByInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var linkedItem = this.Cast<ControllingMessageHeaderLinkInvoiceLine>().FirstOrDefault(x => x.InvoicelinePK == invoiceLine.PK);
			if (linkedItem != null)
			{
				RemoveAndDelete(linkedItem);
			}
		}
	}
}
