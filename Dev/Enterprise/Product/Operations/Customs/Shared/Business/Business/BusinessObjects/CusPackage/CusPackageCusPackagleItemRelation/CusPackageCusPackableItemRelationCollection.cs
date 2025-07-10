using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusPackageCusPackableItemRelationCollection : NonPersistentBusinessObjectCollection<CusPackageCusPackableItemRelation>
	{
		public CusPackageCusPackableItemRelationCollection(CusPackage package)
			: base(package.Factory)
		{
			Package = Argument.NotNull(package, nameof(package));
		}

		CusPackage Package { get; }

		void BuildElements()
		{
			var packingList = Package.CusPackageJob?.PackingList;
			if (packingList != null)
			{
				var packableItems = packingList.PackableItems;
				var packableItemsDic = packableItems.GroupBy(x => x.CUI_JI).ToDictionary(x => x.Key, y => y.ToArray());
				var sortedInvoiceLines = GetSortedInvoiceLines(packingList);
				if (sortedInvoiceLines != null)
				{
					foreach (var invoiceLine in sortedInvoiceLines)
					{
						var invoiceLinePK = invoiceLine.PK;
						if (!packableItemsDic.ContainsKey(invoiceLinePK))
						{
							var newItem = invoiceLine.CreateNewCusPackableItem();
							packableItemsDic.Add(invoiceLinePK, new[] { newItem });
							packableItems.Add(newItem);
						}
					}
				}
				packableItemsDic.Values.ForEach(x => x.ForEach(y => Add(new CusPackageCusPackableItemRelation(Package, y))));
				ReSort();
			}
		}

		protected virtual IEnumerable<BaseJobComInvoiceLine> GetSortedInvoiceLines(CusPackingList packingList)
		{
			var lineComparer = new BaseJobComInvoiceLine.LineComparer();
			return packingList.Declaration?.FilteredInvoiceLines.Cast<BaseJobComInvoiceLine>().OrderBy(x => x, lineComparer) ?? Enumerable.Empty<BaseJobComInvoiceLine>();
		}

		public void RebuildElements()
		{
			RemoveAll();
			BuildElements();
		}

		public override void Load()
		{
			RebuildElements();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected class CusPackageCusPackableItemRelationLineComparer : PropertyComparer
		{
			public CusPackageCusPackableItemRelationLineComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction)
				: base(propertyDescriptor, direction)
			{
			}
			#region IComparer Members

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				const int comparedSameValue = 0;
				int result = 0;

				if (x != y)
				{
					var lineX = (CusPackageCusPackableItemRelation)x;
					var lineY = (CusPackageCusPackableItemRelation)y;
					if (PropertyDescriptor.DisplayName == CusPackageCusPackableItemRelation.Schema.Sequence
							|| PropertyDescriptor.DisplayName == CusPackageCusPackableItemRelation.Schema.InvoiceNumber
							|| PropertyDescriptor.DisplayName == CusPackageCusPackableItemRelation.Schema.InvoiceLineNumber)
					{
						result = CompareInvoiceNumberAndLineNumber(lineX, lineY, Direction);
					}
					else
					{
						result = base.Compare(x, y);
						if (result == comparedSameValue)
						{
							result = CompareInvoiceNumberAndLineNumber(lineX, lineY, ListSortDirection.Ascending);
						}
					}
				}
				return result;
			}

			int CompareInvoiceNumberAndLineNumber(CusPackageCusPackableItemRelation x, CusPackageCusPackableItemRelation y, ListSortDirection direction)
			{
				var result = 0;
				var xPackableItemIsDeleted = x.PackableItem.IsDeleted;
				var yPackableItemIsDeleted = y.PackableItem.IsDeleted;

				if (xPackableItemIsDeleted && !yPackableItemIsDeleted)
				{
					result = -1;
				}
				else if (yPackableItemIsDeleted && !xPackableItemIsDeleted)
				{
					result = 1;
				}
				else if (!xPackableItemIsDeleted && !yPackableItemIsDeleted)
				{
					const int comparedSameValue = 0;
					result = x.Sequence.CompareTo(y.Sequence);
					if (result == comparedSameValue)
					{
						result = x.InvoiceNumber.CompareTo(y.InvoiceNumber);
					}
					if (result == comparedSameValue)
					{
						result = x.InvoiceLineNumber.CompareTo(y.InvoiceLineNumber);
					}
					if (result == comparedSameValue)
					{
						result = x.PK.CompareTo(y.PK);
					}
				}
				result = direction == ListSortDirection.Ascending ? result : -result;
				return result;
			}

			#endregion
		}

		protected override IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return new CusPackageCusPackableItemRelationLineComparer(property, direction);
		}

		public void DeletePackableItemRelationByPackableItem(CusPackableItem itemToDelete)
		{
			var relationToDelete = this.Cast<CusPackageCusPackableItemRelation>().FirstOrDefault(x => x.PackableItem.Equals(itemToDelete));
			if (relationToDelete != null)
			{
				RemoveAndDelete(relationToDelete);
			}
		}
	}
}
