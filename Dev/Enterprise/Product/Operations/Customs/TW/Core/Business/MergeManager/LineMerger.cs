using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies() => new Customs.Business.EntryCreationStrategy[] { new EntryCreationStrategy(Declaration) };

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => Declaration.DutyCalculatorStrategy;

		protected override void SortMergedLineInvoiceLines(ICusEntryLineCollection<Customs.Business.CusEntryLine> mergedLines)
		{
			base.SortMergedLineInvoiceLines(mergedLines);
			var mergedNum = 0;
			var calSequenceNumMapping = CalSequenceNumGenerator.Sort(Declaration.InvoiceLines.Cast<JobComInvoiceLine>());
			foreach (CusEntryLine mergedLine in mergedLines)
			{
				var description = ZString.Empty;
				var mergeBy = Declaration?.JE_MergeBy ?? ZString.Empty;
				if (mergeBy == MergeByCodeList.Codes.CondensedDeclaration && mergedLine.InvoiceLines.Count > 1)
				{
					mergedNum++;
					var lineNos = mergedLine.InvoiceLines.Cast<JobComInvoiceLine>().Select(x => calSequenceNumMapping[x.PK]).ToList();
					description = CalSequenceNumGenerator.SummarLineItems(lineNos, mergedNum);
				}
				description = description.Trim().SubstringSafe(0, mergedLine.CL_DescriptionInfo.MaxLength);
				mergedLine.CL_Description = description;
			}
		}

		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
			Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ForEach(x => ReconcileCustomsValue(x));
		}

		void ReconcileCustomsValue(CusEntryHeader entryHeader)
		{
			var totalCustomsValueInInLocalCurrency = entryHeader.CH_TotalCustomsValueInLocalCurrency;
			var customsFactor = entryHeader.CH_CustomsFactor;
			var entryLines = entryHeader.MergedLines.Cast<CusEntryLine>();
			var invoiceLines = entryLines.SelectMany(x => x.InvoiceLines.Cast<JobComInvoiceLine>());
			ReconcileCustomsValue(entryLines, totalCustomsValueInInLocalCurrency, x => x.CL_CustomsValue = x.CL_CustomsValueForCustomsValuation, x => x.CL_CustomsValue);
			ReconcileCustomsValue(invoiceLines, totalCustomsValueInInLocalCurrency, x => x.JI_CVAfterRecon = ((ZDecimal)(customsFactor * x.JI_LinePriceInLocalCurrency)).Round(0), x => x.JI_CVAfterRecon);
		}

		void ReconcileCustomsValue<T>(IEnumerable<T> collection, ZDecimal entryHeaderCustomsValue, Action<T> action, Func<T, decimal> selector)
		{
			collection.ForEach(action);
			var actualTotal = collection.Sum(selector);
			ZDecimal discrepancy = entryHeaderCustomsValue - actualTotal;
			if (!discrepancy.IsEmpty)
			{
				ReconcileHelper.ReconcileValues((IEnumerable<IReconcileCandidate>)collection, discrepancy.ToZInt());
			}
		}

		internal static class CalSequenceNumGenerator
		{
			public static Dictionary<ZGuid, int> Sort(IEnumerable<JobComInvoiceLine> invoiceLines)
			{
				var calSequenceNumMapping = new Dictionary<ZGuid, int>();
				var list = SortToList(invoiceLines);
				var num = 0;
				list.ForEach(x => calSequenceNumMapping[x.PK] = ++num);
				return calSequenceNumMapping;
			}

			public static List<JobComInvoiceLine> SortToList(IEnumerable<JobComInvoiceLine> invoiceLines)
			{
				var list = new List<JobComInvoiceLine>();
				if (invoiceLines?.Any() ?? false)
				{
					invoiceLines.ForEach(x => list.Add(x));
					var lineComparer = new BaseJobComInvoiceLine.LineComparer();
					list.Sort((JobComInvoiceLine line1, JobComInvoiceLine line2) =>
					{
						return lineComparer.Compare(line1, line2);
					});
				}
				return list;
			}

			#region SUMMAR Line Items

			public static ZString SummarLineItems(List<int> lineNos, int mergedNum)
			{
				lineNos.Sort();
				var items = new List<string>();
				var startNo = -1;
				var endNo = -1;
				foreach (var lineNo in lineNos)
				{
					if (startNo < 0)
					{
						startNo = lineNo;
						endNo = lineNo;
					}
					else if ((endNo + 1) != lineNo)
					{
						AddLineItem(items, startNo, endNo);
						startNo = lineNo;
						endNo = lineNo;
					}
					else
					{
						endNo = lineNo;
					}
				}
				AddLineItem(items, startNo, endNo);
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"SUMMAR{0}, INCLUDING ITEMS {1}", mergedNum, items.Count > 0 ? string.Join(",", items) : string.Empty);
			}

			static void AddLineItem(List<string> items, int startNo, int endNo)
			{
				if (startNo >= 0)
				{
					if (endNo != startNo)
					{
						items.Add(string.Format(CultureInfo.InvariantCulture, "{0}-{1}", startNo, endNo));
					}
					else
					{
						items.Add(string.Format(CultureInfo.InvariantCulture, "{0}", startNo));
					}
				}
			}

			#endregion
		}
	}
}
