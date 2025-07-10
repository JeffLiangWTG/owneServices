using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class AdditionalLineLinkEntryLineCollection : ActiveBusinessObjectCollection<AdditionalInvoiceLineEntryLineLink>
	{
		public AdditionalLineLinkEntryLineCollection(CusEntryLine entryLine)
			: base(entryLine)
		{
			this.entryLine = entryLine;
		}

		public readonly CusEntryLine entryLine;

		public IEnumerable<BaseJobComInvoiceLine> AdditionalInvoiceLines
		{
			get
			{
				foreach (AdditionalInvoiceLineEntryLineLink link in this)
				{
					yield return link.InvoiceLine;
				}
			}
		}

		protected override void SetRelationshipDefaultsForElementCore(AdditionalInvoiceLineEntryLineLink newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BU_CL = entryLine.PK;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();

			BaseJobDeclaration declaration = entryLine.Declaration;

			if (declaration != null && !declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine)
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}
	}
}
