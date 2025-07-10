using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class AdditionalLineLinkInvoiceLineCollection : ActiveBusinessObjectCollection<AdditionalInvoiceLineEntryLineLink>
	{
		public AdditionalLineLinkInvoiceLineCollection(BaseJobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory, new AdhocCollectionRelationship(typeof(AdditionalInvoiceLineEntryLineLink)))
		{
			this.invoiceLine = invoiceLine;
		}

		public readonly BaseJobComInvoiceLine invoiceLine;

		public IEnumerable<CusEntryLine> AdditionalEntryLines => this.Select(link => link.EntryLine).Where(x => x != null);

		public AdditionalInvoiceLineEntryLineLink AddLinkIfNoneExists(CusEntryLine entryLine)
		{
			var result = GetPivotFor(entryLine);
			if (result == null)
			{
				result = AddNew();
				result.BU_CL = entryLine.PK;

				if (!entryLine.AdditionalInvoiceLineLinks.Contains(result))
				{
					entryLine.AdditionalInvoiceLineLinks.Add(result);
				}
			}
			return result;
		}

		public void DeleteLinkIfExistsFor(CusEntryLine entryLine)
		{
			this.FirstOrDefault(link => link.EntryLine == entryLine)?.Delete();
		}

		public void DeleteLinkIfExistsFor(ZString messageType)
		{
			foreach (var link in GetEntryLineFor(messageType).ToArray())
			{
				link.Delete();
			}
		}

		public IEnumerable<CusEntryLine> GetEntryLineFor(ZString messageType)
		{
			if (!messageType.IsEmpty)
			{
				foreach (var link in this)
				{
					if ((link.EntryLine?.Header?.CH_MessageType ?? ZString.Empty) == messageType)
					{
						yield return link.EntryLine;
					}
				}
			}
		}

		public AdditionalInvoiceLineEntryLineLink GetPivotFor(CusEntryLine entryLine)
		{
			return this.FirstOrDefault(link => link.BU_CL == entryLine.PK && link.EntryLine == entryLine);
		}

		public bool ContainsPivotFor(CusEntryLine entryLine)
		{
			return GetPivotFor(entryLine) != null;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void SetRelationshipDefaultsForElementCore(AdditionalInvoiceLineEntryLineLink newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BU_JI = invoiceLine.PK;
		}

		public void Load()
		{
			((System.Collections.IList)this).Clear();

			var query = new ZQuery(CusUnderbondDecSchema.BU_JI, invoiceLine.PK);
			var declaration = invoiceLine.Declaration;
			if (declaration != null && !declaration.NeedsAdditionalLinkBetweenInvoiceLineAndEntryLine)
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				query = new ZQuery(CusUnderbondDecSchema.BU_ClusterKey, GetRelatedClusterKeys());
				query.AddToFilter(CusUnderbondDecSchema.BU_JI, invoiceLine.PK);
				query.FetchOnlyFromLocalCache = !invoiceLine.IsInDatabase;
			}

			AddRange(Factory.Load<AdditionalInvoiceLineEntryLineLink>(query));
		}

		IEnumerable<ZInt> GetRelatedClusterKeys()
		{
			yield return invoiceLine.JI_ClusterKey;
			var additionalDeclarationClusterKeys = invoiceLine.InvoiceHeader?.AdditionalDeclarations.Select(declaration => declaration.JE_ClusterKey);
			if (additionalDeclarationClusterKeys != null)
			{
				foreach (var clusterKey in additionalDeclarationClusterKeys)
				{
					yield return clusterKey;
				}
			}
		}
	}
}
