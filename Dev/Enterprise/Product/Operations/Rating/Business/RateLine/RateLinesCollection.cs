using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public interface IRateLinesWithParentEntry
	{
		RateEntry Master { get; }
		int Count { get; }
		RateLine InsertNew(int position);
		int OverrideTariffLines(IEnumerable<RateLine> rateLines);
	}

	public class RateLinesCollection : DependentBusinessObjectCollection<RateLine, RateEntry>, IRateLinesWithParentEntry, IImportCollectionElementMatchingSupporter
	{
		public RateLinesCollection(RateEntry master)
			: base(master)
		{
			IsManagedForDataRefresh = false;
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			var pk = Guid.NewGuid();
			using (pk.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.AddNewCore(bizoType, pk);
			}
		}

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			using (row.MarkAsInConstruction(RateLinesSchema.PK, Factory))
			{
				return base.CreateBusinessObjectFromRow(row);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete.IsDeleted)
			{
				return;
			}

			using (elementToDelete.MarkAsInDeletion())
			{
				if (ParentIsAdditionalTariff)
				{
					if (((RateLine)elementToDelete).TL_CompanyTariffLevel == TariffLevel)   // Ie overrides the base tariff
					{
						base.RemoveAndDelete(elementToDelete);  // remove overridden line
						Load(); // reload collection to bring back base rate line
					}
				}
				else
				{
					base.RemoveAndDelete(elementToDelete);
				}
			}
		}

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var rateLine = (RateLine)child;
			using (rateLine.GetValidationSuspender())
			{
				if (Master != null)
				{
					rateLine.TL_TI = Master.PK;

					if (!rateLine.IsInDatabase && rateLine.ChargeCode != null)
					{
						rateLine.TL_RateCalculator = rateLine.ChargeCode.AC_RateCalculator;
					}

					if (!Master.SuspendSettingRateLineTariff)
					{
						rateLine.TL_CompanyTariffLevel = TariffLevel;
					}
					rateLine.TL_LineOrder = (ZByte)RatingHelper.MaxPlus1(this, RateLinesSchema.TL_LineOrder.Name);

					rateLine.TL_RX_NKCurrency = Master.TI_RX_NKCurrency;
					if (Master.IsFreightEntry())
					{
						rateLine.TL_WeightVolume = Master.Unit;
					}

					if (Master.IsWHS() || Master.IsTRW() || Master.IsTWU())
					{
						rateLine.UseOnlyActualWeightMeasure = true;
					}
				}
			}
		}

		#endregion

		#region Sort Collection

		public void Sort()
		{
			this.Sort(RateLinesSchema.TL_LineOrder.Name, System.ComponentModel.ListSortDirection.Ascending);
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		#endregion

		#region Relationship Column

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return RateLinesSchema.TL_TI; }
		}

		#endregion

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			if (!TariffLevel.IsEmpty)
			{
				result.AddToFilter(RateLinesSchema.TL_CompanyTariffLevel, SQLComparisonOperator.LessThanOrEqualTo, TariffLevel);
			}

			return result;
		}

		public override void Load()
		{
			base.Load();
			RemoveOverriddenTariffRateLines();

			if (Master != null && Master.IsReadOnlyDueToGlobalPublisher)
			{
				Master.SetReadOnlyIncludingChildren(true);
			}
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly || Master != null && Master.IsReadOnlyDueToGlobalPublisher; }
		}

		protected override bool AllowNewCore
		{
			get { return base.AllowNewCore && !ParentIsAdditionalTariff; }
		}

		void RemoveOverriddenTariffRateLines()
		{
			if (ParentIsAdditionalTariff)
			{
				for (var i = Count - 1; i >= 0; i--)
				{
					var line = this[i];
					line.ReadOnly = ReadOnly || line.TL_CompanyTariffLevel < TariffLevel;

					for (var j = Count - 1; j >= 0; j--)
					{
						if (line.TL_AC == this[j].TL_AC && line.TL_CompanyTariffLevel < this[j].TL_CompanyTariffLevel)
						{
							RemoveLineKeepRelationshipsIntact(line);
							break;
						}
					}
				}
			}
		}

		void RemoveLineKeepRelationshipsIntact(RateLine line)
		{
			using (line.SuspendSettingHasChanges())
			{
				var tL_TI = line.TL_TI;
				Remove(line);
				line.TL_TI = tL_TI;
			}
		}

		public int OverrideTariffLines(IEnumerable<RateLine> rateLines)
		{
			var indexOfFirstNewLine = -1;

			foreach (RateLine lineToClone in rateLines)
			{
				if (lineToClone.IsTariffLineInherited)
				{
					var newLine = lineToClone.Clone(this);
					newLine.TL_CompanyTariffLevel = TariffLevel;
					newLine.TL_LineOrder = lineToClone.TL_LineOrder;
					newLine.TL_FeeChargeTypeInfo.ClearValue();
					newLine.TL_FeeChargeLevelInfo.ClearValue();

					RemoveLineKeepRelationshipsIntact(lineToClone);

					newLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;

					if (indexOfFirstNewLine < 0)
					{
						indexOfFirstNewLine = Elements.IndexOf(newLine);
					}
				}
			}

			return indexOfFirstNewLine;
		}

		public bool ParentIsAdditionalTariff
		{
			get { return Master?.ParentRatingHeader.IsAdditionalTariff() ?? false; }
		}

		ZByte TariffLevel
		{
			get { return Master?.ParentRatingHeader.IsTariff() ?? false ? Master.ParentRatingHeader.TH_GlobalRateLevel : ZByte.Zero; }
		}

		#region Insert New

		public RateLine InsertNew(int position)
		{
			IList collectionAsList = this;
			var newLine = AddNew();
			RemoveLineKeepRelationshipsIntact(newLine);

			if (position >= 0 && position < collectionAsList.Count)
			{
				collectionAsList.Insert(position, newLine);
			}
			else
			{
				collectionAsList.Add(newLine);
			}

			foreach (RateLine line in this)
			{
				line.TL_LineOrder = (ZByte)Math.Min(byte.MaxValue, collectionAsList.IndexOf(line));
			}

			return newLine;
		}

		#endregion

		#region IImportCollectionElementMatchingSupporter

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

