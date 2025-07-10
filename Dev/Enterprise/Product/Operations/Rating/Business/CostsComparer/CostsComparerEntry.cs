using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class CostsComparerEntry : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CostsComparerEntry(CostsComparer parent, RateEntry entry, List<RateLine> rateLinesInclRelated)
			: base(entry.Factory)
		{
			this.Entry = entry;
			this.Parent = parent;
			this.RateLinesInclRelated = rateLinesInclRelated;
		}

		public RateEntry Entry { get; private set; }
		readonly List<RateLine> RateLinesInclRelated;

		public override bool IsDeleted
		{
			get { return Entry.IsDeleted || base.IsDeleted; }
		}

		#region Parent

		public CostsComparer Parent { get; set; }

		#endregion

		const string SummaryColumnCaptionPrefix = "SummaryColumn";
		static readonly int SummaryColumnCaptionPrefixLength = SummaryColumnCaptionPrefix.Length;

		public object SummaryColumn(string propertyName, Type type)
		{
			var columnNumber = Convert.ToInt32(propertyName.Substring(SummaryColumnCaptionPrefixLength, propertyName.Length - SummaryColumnCaptionPrefixLength));
			return GetSummaryColumn(columnNumber, type);
		}

		protected override bool MatchesFilterCore(ZQuery filter, System.Data.DataRow row, System.Data.DataTable table, string tableName, string identifier)
		{
			if (filter == null || filter.IsEmpty)
			{
				return true;
			}

			if (row == null)
			{
				row = ((INeedRow)Entry).Row;
				if (row != null && row.Table != null)
				{
					table = row.Table;
					tableName = row.Table.TableName;
				}
			}

			return row != null && base.MatchesFilterCore(filter, row, table, tableName, identifier);
		}

		public IZType GetSummaryColumn(int columnNumber, Type type)
		{
			if (Parent != null && columnNumber <= Parent.SummaryItems.Count)
			{
				var keyItem = Parent.SummaryItems.Keys[columnNumber - 1];
				ChargesSummaryItem itemInLocalCurrency;

				if (SummaryItems.TryGetValue(keyItem, out itemInLocalCurrency))
				{
					var item = new ChargesSummaryItem(itemInLocalCurrency);

					if (Parent.CurrencyObj != null)
					{
						item.Convert(Parent.CurrencyConverter, GlbCompany.CurrentCompany.LocalCurrency, Parent.CurrencyObj);
					}

					if (type == typeof(ZString))
					{
						return item.GetStringValue(Parent.CurrencyObj);
					}

					return item.GetDecimalValue(Parent.CurrencyObj);
				}
			}

			if (type == typeof(ZString))
			{
				return ZString.Empty;
			}

			return ZDecimal.Zero;
		}

		#region RateLines

		public SimpleRateLinesCollection RateLines
		{
			get
			{
				InitialiseRateLines();
				return fRateLines;
			}
		}

		void InitialiseRateLines()
		{
			if (fRateLines == null)
			{
				fRateLines = new SimpleRateLinesCollection(Factory);
				RegisterEditableChildObject(fRateLines);

				chargeSummaryItemLists = new Dictionary<RateLine, IList<ChargesSummaryItem>>();

				foreach (var line in RateLinesInclRelated)
				{
					if (!Parent.SingleChargeCodeComparisonOnly || line.ChargeCode.PK == Parent.ChargeCodePK)
					{
						AddToSummaryItems(RateLinesInclRelated, line);
						fRateLines.Add(line);
					}
				}

				fRateLines.SetReadOnlyIncludingChildren(true);
				AppendPerUnitToSlidingCharges();
			}
		}

		void AddToSummaryItems(List<RateLine> rateLines, RateLine line)
		{
			var nonFreightSameWeightVolume = !Parent.ShowAllCharges && (AllRateLinesHaveSameWeightVolume(rateLines) || HaveSameWeightVolumeAsFreightCharge(line));

			var isFreightChargeUnit = line.TL_WeightVolume.IsEmpty ||
										line.TL_WeightVolume == RatingConstants.Units.HB ||
										line.TL_WeightVolume == Entry.Unit ||
										nonFreightSameWeightVolume;

			if (!isFreightChargeUnit)
			{
				line.AddRowWarning(GetUnitIsDifferenToFreightChargeRowWarning());
				return;
			}

			var chargesSummaryItems = line.Calculator.GetCostsComparerChargesSummary(rateLines);
			if (chargesSummaryItems == null)
			{
				line.AddRowWarning(GetComplexNatureRowWarning());
				return;
			}

			AddToUnconvertedList(line, chargesSummaryItems);
			foreach (var itemToConvert in chargesSummaryItems)
			{
				if (!itemToConvert.Convert(Parent.CurrencyConverter, line.Currency, GlbCompany.CurrentCompany.LocalCurrency))
				{
					RemoveFromUnconvertedList(line);
					line.AddRowWarning(GetNoBuyExchangeRateWarning(line));
					return;
				}
			}

			for (var i = 0; i < chargesSummaryItems.Count; i++)
			{
				ChargesSummaryItem itemOnTheList;
				if (SummaryItems.TryGetValue(chargesSummaryItems[i], out itemOnTheList))
				{
					SummaryItems[chargesSummaryItems[i]] = itemOnTheList + chargesSummaryItems[i];
				}
				else
				{
					SummaryItems.Add(chargesSummaryItems[i], chargesSummaryItems[i]);
				}
			}
		}

		bool AllRateLinesHaveSameWeightVolume(List<RateLine> rateLines)
		{
			var sameWeightVolume = false;

			var indexWeightVolume = rateLines.FindIndex(r => !r.TL_WeightVolume.IsEmpty);
			if (indexWeightVolume >= 0)
			{
				var weightVolume = rateLines[indexWeightVolume].TL_WeightVolume;
				sameWeightVolume = indexWeightVolume == rateLines.Count - 1
						 || rateLines.FindIndex(indexWeightVolume + 1, r => !r.TL_WeightVolume.IsEmpty && r.TL_WeightVolume != weightVolume) == -1;
			}
			else
			{
				sameWeightVolume = true;
			}

			return sameWeightVolume;
		}

		bool HaveSameWeightVolumeAsFreightCharge(RateLine rateline)
		{
			var sameWeightVolume = false;

			if (!rateline.Parent.IsFreightEntry())
			{
				var matchedFreightEntries = rateline.Parent.GetRelatedEntries(RateEntry.RelatedEntriesToFindType.Freight);
				var indexWeightVolume = matchedFreightEntries.FindIndex(r => !r.Unit.IsEmpty);
				if (indexWeightVolume >= 0)
				{
					var weightVolume = matchedFreightEntries[indexWeightVolume].Unit;
					sameWeightVolume = rateline.TL_WeightVolume == weightVolume
						&& matchedFreightEntries.FindIndex(indexWeightVolume + 1, r => !r.Unit.IsEmpty && r.Unit != weightVolume) == -1;
				}
			}

			return sameWeightVolume;
		}

		void AddToUnconvertedList(RateLine line, IList<ChargesSummaryItem> chargesSummaryItems)
		{
			if (!chargeSummaryItemLists.ContainsKey(line))
			{
				var copiedItems = new List<ChargesSummaryItem>();

				foreach (var item in chargesSummaryItems)
				{
					copiedItems.Add(new ChargesSummaryItem(item));
				}

				chargeSummaryItemLists.Add(line, copiedItems);
			}
		}

		void RemoveFromUnconvertedList(RateLine line)
		{
			chargeSummaryItemLists.Remove(line);
		}

		SimpleRateLinesCollection fRateLines;

		Dictionary<RateLine, IList<ChargesSummaryItem>> chargeSummaryItemLists;

		public IList<ChargesSummaryItem> GetChargesSummaryItems(RateLine line)
		{
			if (chargeSummaryItemLists == null)
			{
				InitialiseRateLines();
			}

			return chargeSummaryItemLists != null && chargeSummaryItemLists.ContainsKey(line)
					? chargeSummaryItemLists[line]
					: new List<ChargesSummaryItem>();
		}

		#region

		public static string GetUnitIsDifferenToFreightChargeRowWarning()
		{
			return Res.GetString("3b330ed2-b0e1-491e-b093-45223137a464", "This charge is expressed in a unit that is different to the Freight charge. Therefore it cannot be used in a cost comparison.");
		}

		public static string GetNoBuyExchangeRateWarning(RateLine line)
		{
			return Res.GetString("a131fd57-a642-46b8-ba40-214d4b0d3aa8", "{0} has no current Buy exchange rate to local currency.", line.Currency.RX_Code);
		}

		public static string GetComplexNatureRowWarning()
		{
			return Res.GetString("5897b0be-6e68-4fa2-98c5-09546d93d2d1", "This calculator cannot be logically used in a cost comparison due to its complex nature.");
		}

		#endregion

		#endregion

		#region Charges Summary

		internal SortedList<ChargesSummaryItem, ChargesSummaryItem> SummaryItems
		{
			get
			{
				if (fSummaryItems == null)
				{
					fSummaryItems = new SortedList<ChargesSummaryItem, ChargesSummaryItem>();
					InitialiseRateLines();
				}

				return fSummaryItems;
			}
		}

		SortedList<ChargesSummaryItem, ChargesSummaryItem> fSummaryItems;

		void AppendPerUnitToSlidingCharges()
		{
			var untKey = new ChargesSummaryItem(Calculator.Items.Operator.UNT, 0m);
			ChargesSummaryItem untItem;

			if (SummaryItems.TryGetValue(untKey, out untItem))
			{
				var removeUnt = false;
				for (var i = 0; i < SummaryItems.Count; i++)
				{
					var item = SummaryItems.Values[i];
					if (item.Type == Calculator.Items.Operator.Plus || item.Type == Calculator.Items.Operator.Minus)
					{
						SummaryItems[SummaryItems.Keys[i]] = item + untItem;
						removeUnt = true;
					}
				}

				if (removeUnt)
				{
					SummaryItems.Remove(untKey);
				}
			}
		}

		#endregion

	}
}

