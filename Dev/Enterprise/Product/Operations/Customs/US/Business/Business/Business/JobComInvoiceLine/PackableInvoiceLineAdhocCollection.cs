using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class PackableInvoiceLineAdhocCollection : ActiveBusinessObjectCollection<JobComInvoiceLine>
	{
		public PackableInvoiceLineAdhocCollection(JobDeclaration declaration)
			: base(declaration.Factory, new AdhocCollectionRelationship(typeof(JobComInvoiceLine)))
		{
			this.declaration = declaration;
		}

		public void HookEventsAndReload()
		{
			((IList)this).Clear();
			declaration.InvoiceLines.CountChanged -= InvoiceLines_CountChanged;
			declaration.InvoiceLines.CountChanged += InvoiceLines_CountChanged;
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				HookInvoiceLineEvent(invoiceLine);
				if (IsPackable(invoiceLine))
				{
					Add(invoiceLine);
				}
			}
		}

		public void UnHookEvents()
		{
			((IList)this).Clear();
			declaration.InvoiceLines.CountChanged -= InvoiceLines_CountChanged;
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				UnHookInvoiceLineEvent(invoiceLine);
			}
		}

		public ICodeDescriptionPairList UniqueProductList
		{
			get
			{
				if (uniqueProductListCached == null)
				{
					uniqueProductListCached = new CachedProperty<CodeDescriptionPairList>(Factory, () =>
					{
						var result = new CodeDescriptionPairList();
						foreach (var line in this)
						{
							if (!line.JI_PartNo.IsEmpty && !result.ContainsCode(line.JI_PartNo))
							{
								result.AddPair(line.JI_PartNo, line.PartDescription);
							}
						}
						return result;
					});
				}
				return uniqueProductListCached.Value;
			}
		}
		CachedProperty<CodeDescriptionPairList> uniqueProductListCached;

		void HookInvoiceLineEvent(JobComInvoiceLine invoiceLine)
		{
			UnHookInvoiceLineEvent(invoiceLine);
			invoiceLine.JI_OPInfo.ValueChanged += PackableRelatedDataChanged;
			invoiceLine.JI_InvoiceQuantityInfo.ValueChanged += PackableRelatedDataChanged;
			invoiceLine.JI_BondedWhsQuantityInfo.ValueChanged += PackableRelatedDataChanged;
			invoiceLine.JI_ParentIDInfo.ValueChanged += PackableRelatedDataChanged;
			invoiceLine.US_JI_ParentProductInfo.ValueChanged += PackableRelatedDataChanged;
		}

		void UnHookInvoiceLineEvent(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.JI_OPInfo.ValueChanged -= PackableRelatedDataChanged;
			invoiceLine.JI_InvoiceQuantityInfo.ValueChanged -= PackableRelatedDataChanged;
			invoiceLine.JI_BondedWhsQuantityInfo.ValueChanged -= PackableRelatedDataChanged;
			invoiceLine.JI_ParentIDInfo.ValueChanged -= PackableRelatedDataChanged;
			invoiceLine.US_JI_ParentProductInfo.ValueChanged -= PackableRelatedDataChanged;
		}

		void PackableRelatedDataChanged(object sender, System.EventArgs e)
		{
			var invoiceLine = sender as JobComInvoiceLine;
			if (invoiceLine == null)
			{
				var addInfo = sender as AddInfoJobComInvoiceLine;
				if (addInfo != null)
				{
					invoiceLine = addInfo.InvoiceLine;
				}
			}
			if (invoiceLine != null)
			{
				if (IsPackable(invoiceLine))
				{
					if (!Contains(invoiceLine))
					{
						Add(invoiceLine);
					}
				}
				else
				{
					RemoveAndDeleteWHSPackDetails(invoiceLine);
				}
				declaration.WHSPacks.RefreshBinding();
				declaration.WHSPackFilteredLines.RefreshBinding();
			}
		}

		void RemoveAndDeleteWHSPackDetails(JobComInvoiceLine invoiceLine)
		{
			this.Relationship.RemoveFromRelationship(invoiceLine);
			var affectedWHSPacks = new List<WHSPack>();
			foreach (var whsPackLine in declaration.WHSPackLines.OfType<WHSPackLine>().Where(x => x.US_JI_InvoiceLine == invoiceLine.PK).ToArray())
			{
				var whsPack = whsPackLine.WHSPack;
				if (whsPack != null && !affectedWHSPacks.Contains(whsPack))
				{
					affectedWHSPacks.Add(whsPack);
				}
				whsPackLine.Delete();
			}
			if (Count == 0)
			{
				declaration.WHSPacks.RemoveAndDeleteAll();
			}
			else
			{
				affectedWHSPacks.Where(x => x.WHSPackLines.Count == 0).DeleteAll();
			}
		}

		void InvoiceLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var invoiceLine = e.BizObject as JobComInvoiceLine;
			if (invoiceLine != null)
			{
				if (e.ItemRemoved)
				{
					UnHookInvoiceLineEvent(invoiceLine);
					RemoveAndDeleteWHSPackDetails(invoiceLine);
				}
				else if (e.ItemAdded)
				{
					HookInvoiceLineEvent(invoiceLine);
					if (IsPackable(invoiceLine))
					{
						Add(invoiceLine);
					}
				}
				declaration.WHSPacks.RefreshBinding();
				declaration.WHSPackFilteredLines.RefreshBinding();
			}
		}

		bool IsPackable(JobComInvoiceLine invoiceLine)
		{
			return invoiceLine.IsWHSPackable;
		}

		readonly JobDeclaration declaration;
	}
}
