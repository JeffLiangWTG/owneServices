using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class JobComInvoiceLineViewCollection : TypeSafeJobComInvoiceLineViewCollection
	{
		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineDependentCollection completeCollection)
			: base(invoice, completeCollection)
		{
		}

		public bool RequiresPriorNoticeReporting
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in this)
				{
					if (invoiceLine.RequiresPriorNoticeReporting())
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool HasFDATariffsToBeDeclared
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in this)
				{
					if (invoiceLine.IsFDADeclared)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool HasOGATariffsToBeDeclared
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in this)
				{
					if (invoiceLine.IsFCCDeclared || invoiceLine.IsDOTDeclared)
					{
						return true;
					}
				}

				return false;
			}
		}

		public ZInt TotalNonSecondaryInvoiceLines
		{
			get
			{
				ZInt result = 0;

				foreach (JobComInvoiceLine invoiceLine in this)
				{
					if (!invoiceLine.IsSecondaryTariffLine)
					{
						result++;
					}
				}

				return result;
			}
		}

		public ZDecimal TotalPackQty
		{
			get
			{
				if (totalInvoiceQtyCached == null)
				{
					totalInvoiceQtyCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0m;

						foreach (JobComInvoiceLine invoiceLine in this)
						{
							if (invoiceLine.IsNormalOrParentOrSetXLine)
							{
								result += invoiceLine.US_ManifestQty;
							}
						}

						return result;
					}
					);
				}
				return totalInvoiceQtyCached.Value;
			}
		}
		CachedProperty<ZDecimal> totalInvoiceQtyCached;

		public bool HasNonContainerisedInvoiceLines
		{
			get
			{
				foreach (JobComInvoiceLine invoiceLine in this)
				{
					if (invoiceLine.ContainersPivot.Count == 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public IEnumerable<ILineNumberRange> GetLineNumberRangesRelatedToInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			if (Contains(invoiceLine))
			{
				List<LineNumberRange> list;
				if (FullLineNumberRangeList.TryGetValue(invoiceLine.PK, out list))
				{
					foreach (LineNumberRange range in list)
					{
						yield return range;
					}
				}
			}
		}

		public void AddFetchHintsForTariffRuleIfNeeded()
		{
			if (haveRunFetchHintsForTariffRule == null)
			{
				haveRunFetchHintsForTariffRule = new CachedProperty<bool>(Factory, () =>
				{
					foreach (var line in this.OfType<JobComInvoiceLine>().Where(x => x.ManufacturerAddress == null))
					{
						var tariff = line.ImportTariff;
						if (tariff != null)
						{
							var query = USCTariffRule.Loader.GetTariffRange(tariff.UE_Tariff);
							Factory.AddFetchHint(USCTariffRuleSchema.Instance, query);
						}
					}
					return false;
				});
			}
			var run = haveRunFetchHintsForTariffRule.Value;
		}
		CachedProperty<bool> haveRunFetchHintsForTariffRule;
		public void RefreshFullLineNumberRangeList()
		{
			fullLineNumberRangeList = null;
		}

		Dictionary<ZGuid, List<LineNumberRange>> FullLineNumberRangeList
		{
			get { return fullLineNumberRangeList ?? (fullLineNumberRangeList = CreateFullLineRangeList()); }
		}
		Dictionary<ZGuid, List<LineNumberRange>> fullLineNumberRangeList;

		Dictionary<ZGuid, List<LineNumberRange>> CreateFullLineRangeList()
		{
			Dictionary<ZGuid, List<LineNumberRange>> fullLineNumberRangeList = new Dictionary<ZGuid, List<LineNumberRange>>();
			List<JobComInvoiceLine> list = new List<JobComInvoiceLine>(this.ToArray<JobComInvoiceLine>());
			list.Sort(new InvoiceLineComparer());
			SortedList<ZShort, JobComInvoiceLine> sortedList = new SortedList<ZShort, JobComInvoiceLine>();
			foreach (JobComInvoiceLine line in list)
			{
				if (line.ParentTariffLine == null)
				{
					foreach (InvoiceLineGroupingRange range in line.LineGroupingRanges)
					{
						if (!sortedList.ContainsKey(range.US_StartSequenceNo))
						{
							sortedList.Add(range.US_StartSequenceNo, line);
						}
						for (ZShort i = range.US_StartSequenceNo + 1; i <= range.US_EndSequenceNo; i++)
						{
							if (!sortedList.ContainsKey(i))
							{
								sortedList.Add(i, line);
							}
						}
					}
				}
			}

			ZShort lineNumber = ZShort.Zero;
			foreach (JobComInvoiceLine line in sortedList.Values)
			{
				if (line.IsValidForAII)
				{
					AddLineNumber(fullLineNumberRangeList, ++lineNumber, line.PK);
				}
				foreach (JobComInvoiceLine childLine in line.ChildLines)
				{
					if (childLine.IsValidForAII)
					{
						AddLineNumber(fullLineNumberRangeList, ++lineNumber, childLine.PK);
					}
				}
			}
			return fullLineNumberRangeList;
		}

		void AddLineNumber(Dictionary<ZGuid, List<LineNumberRange>> fullLineNumberRangeList, ZShort lineNumber, ZGuid invoiceLinePK)
		{
			List<LineNumberRange> lineNumberRangeList = null;
			if (!fullLineNumberRangeList.TryGetValue(invoiceLinePK, out lineNumberRangeList))
			{
				lineNumberRangeList = new List<LineNumberRange>();
				lineNumberRangeList.Add(new LineNumberRange(lineNumber, lineNumber));
				fullLineNumberRangeList.Add(invoiceLinePK, lineNumberRangeList);
				return;
			}

			LineNumberRange foundRange = null;
			foreach (LineNumberRange range in lineNumberRangeList)
			{
				if (range.StartNumber < lineNumber && (range.EndNumber + 1) == lineNumber)
				{
					foundRange = range;
					break;
				}
			}

			if (foundRange == null)
			{
				foundRange = new LineNumberRange(lineNumber, lineNumber);
				lineNumberRangeList.Add(foundRange);
			}
			else
			{
				foundRange.EndNumber = lineNumber;
			}
		}
	}
}
