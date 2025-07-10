using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// This is the datasource for the RateLine grid
	/// </summary>
	public class FilteredRateLinesCollection : BusinessObjectCollectionView<RateLine>, IRateLinesWithParentEntry, IImportCollectionElementMatchingSupporter
	{
		ZQuery filter;

		public FilteredRateLinesCollection(RateLinesCollection allLines, ZQuery filter) : base(allLines)
		{
			this.filter = filter;
			Rebuild();
		}

		public RateEntry Master => ((RateLinesCollection)CollectionToFilter).Master;

		protected override bool AllowSort => false;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return filter == null
				|| !element.IsInDatabase
				|| element.HasChanges
				|| element.MatchesFilter(filter);
		}

		/// <summary>
		/// Prevent the base class constructor doing a Rebuild.
		/// </summary>
		protected override void RebuildOnConstruction() { }

		internal void SetFilter(ZQuery filter)
		{
			if (!Equals(this.filter, filter))
			{
				this.filter = filter;
				Rebuild();
				SortByLineOrder();
			}
		}

		protected override BusinessObject CreateBusinessObjectFromRow(System.Data.DataRow row)
		{
			using (row.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.CreateBusinessObjectFromRow(row);
			}
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			var pk = Guid.NewGuid();
			using (pk.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.AddNewCore(bizoType, pk);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (CollectionToFilter.Contains(elementToDelete))
			{
				// The special logic for removing overrides on delete is in RateLinesCollection.RemoveAndDelete.
				// We have to call it directly since the base method doesn't.
				CollectionToFilter.RemoveAndDelete(elementToDelete);
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
			SortByLineOrder();
		}

		public RateLine InsertNew(int position)
		{
			var allLines = (RateLinesCollection)CollectionToFilter;
			int allLinesPosition;
			if (position >= 0 && position < Count)
			{
				var lineAtPosition = this[position];
				allLinesPosition = allLines.Cast<RateLine>().IndexOf(x => ReferenceEquals(x, lineAtPosition));
			}
			else
			{
				allLinesPosition = -1;
			}

			var result = allLines.InsertNew(allLinesPosition);
			SortByLineOrder();
			return result;
		}

		/// <summary>
		/// Line can't be sorted by the user. They are always in LineOrder.
		/// </summary>
		void SortByLineOrder() => Sort(RateLinesSchema.TL_LineOrder.Name, ListSortDirection.Ascending);

		public int OverrideTariffLines(IEnumerable<RateLine> rateLines)
		{
			var allLines = (RateLinesCollection)CollectionToFilter;
			var index = allLines.OverrideTariffLines(rateLines);
			if (index >= 0)
			{
				var newLine = allLines[index];
				Rebuild();
				SortByLineOrder();
				index = this.IndexOf(x => ReferenceEquals(x, newLine));
			}

			return index;
		}

		#region IImportCollectionElementMatchingSupporter

		// This region should be the same code as RateLinesCollection.cs
		// When importing through ADAW via a RateEntry grid, when it gets to the RateLine,
		// it will use RateLinesCollection to import it. This is because it looks at the child
		// collection which refers to RateLine*s*.
		//
		// When importing through ADAW via RateLine grid, it will use
		// FilteredRateLinesCollection to do the importing since it's the datasource
		// of the grid that holds the RateLines

		string IImportCollectionElementMatchingSupporter.MatchingColumnName => String.Empty;

		bool IImportCollectionElementMatchingSupporter.IsGenericColumnMatchingAllowed => false;

		bool IImportCollectionElementMatchingSupporter.FindGenericColumnMatches => true;

		BusinessObject IImportCollectionElementMatchingSupporter.GetMatchingBizObject(string value)
		{
			return null;
		}

		void IImportCollectionElementMatchingSupporter.PrepareForReuse(BusinessObject matchedBizO)
		{
		}

		#endregion
	}
}

