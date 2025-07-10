using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AIILineCollection : DependentBusinessObjectCollection<AIILine, JobComInvoiceLine>
	{
		public AIILineCollection(JobComInvoiceLine master)
			: base(master)
		{
		}

		public AIILine AddNew(InvoiceLineGroupingRange groupingRange)
		{
			AIILine result = AddNew();
			result.US_CY_LineGroupRef = groupingRange != null ? groupingRange.PK : ZGuid.Empty;
			return result;
		}

		public ZDecimal TotalInvoiceAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (AIILine aiiLine in this)
				{
					if (!aiiLine.US_SupLine)
					{
						result += aiiLine.US_InvAmount;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalInvoiceQty
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (AIILine aiiLine in this)
				{
					if (!aiiLine.US_SupLine)
					{
						result += aiiLine.US_InvQty;
					}
				}
				return result;
			}
		}

		public ZDecimal TotalFirstCustomsQty(bool supLine)
		{
			ZDecimal result = ZDecimal.Zero;
			foreach (AIILine aiiLine in this)
			{
				if (supLine == aiiLine.US_SupLine)
				{
					result += aiiLine.US_CustomsQty;
				}
			}
			return result;
		}

		public ZDecimal TotalSecondCustomsQty(bool supLine)
		{
			ZDecimal result = ZDecimal.Zero;
			foreach (AIILine aiiLine in this)
			{
				if (supLine == aiiLine.US_SupLine)
				{
					result += aiiLine.US_SecondQty;
				}
			}
			return result;
		}

		public ZDecimal TotalThirdCustomsQty(bool supLine)
		{
			ZDecimal result = ZDecimal.Zero;
			foreach (AIILine aiiLine in this)
			{
				if (supLine == aiiLine.US_SupLine)
				{
					result += aiiLine.US_ThirdQty;
				}
			}
			return result;
		}

		public void UpdateLineGroupingDetails()
		{
			if (Count > 0)
			{
				InvoiceLineGroupingRange[] ranges = GetSortedLineGroupingRanges();
				AIILine[] sortedLines = GetUpdateLineGroupingReferenceAndSortedLines(ranges);
				foreach (AIILine line in sortedLines)
				{
					InvoiceLineGroupingRange range = GetBestGroupingRange(ranges);
					if (range != null)
					{
						line.US_CY_LineGroupRef = range.PK;
					}
					else
					{
						line.Delete();
					}
				}
			}
		}

		#region Implementation

		InvoiceLineGroupingRange GetBestGroupingRange(InvoiceLineGroupingRange[] sortedRanges)
		{
			return sortedRanges.Length > 0 ? sortedRanges[sortedRanges.Length - 1] : null;
		}

		AIILine[] GetUpdateLineGroupingReferenceAndSortedLines(InvoiceLineGroupingRange[] sortedRanges)
		{
			List<AIILine> lines = new List<AIILine>(new TypedEnumerable<AIILine>(this));
			lines.Sort(new AIILineComparer());
			foreach (InvoiceLineGroupingRange range in sortedRanges)
			{
				int noOfSequences = range.NoOfSequences;
				for (int i = 1; i <= noOfSequences; i++)
				{
					AIILine aiiLine;
					if (lines.Count > 0)
					{
						aiiLine = lines[0];
						aiiLine.US_CY_LineGroupRef = range.PK;
						lines.Remove(aiiLine);
					}
				}
			}
			return lines.ToArray();
		}

		InvoiceLineGroupingRange[] GetSortedLineGroupingRanges()
		{
			List<InvoiceLineGroupingRange> ranges = new List<InvoiceLineGroupingRange>();
			if (Master.IsLineGroupingEnabled)
			{
				JobComInvoiceLine parentTariffLine = Master.ParentTariffLine;
				InvoiceLineGroupingRangeCollection lineGroupingRanges = null;
				if (parentTariffLine == null)
				{
					lineGroupingRanges = Master.LineGroupingRanges;
				}
				else
				{
					lineGroupingRanges = parentTariffLine.LineGroupingRanges;
				}
				ranges.AddRange(lineGroupingRanges.OfType<InvoiceLineGroupingRange>());
			}
			else
			{
				InvoiceLineGroupingRange firstGroupingRange = Master.FirstGroupingRange;
				if (firstGroupingRange != null)
				{
					ranges.Add(firstGroupingRange);
				}
			}
			ranges.Sort(new Comparison<InvoiceLineGroupingRange>(RangeCompare));
			return ranges.ToArray();
		}

		int RangeCompare(InvoiceLineGroupingRange x, InvoiceLineGroupingRange y)
		{
			int result = x.US_StartSequenceNo.CompareTo(y.US_StartSequenceNo);
			if (result == 0)
			{
				result = x.US_EndSequenceNo.CompareTo(y.US_EndSequenceNo);
			}
			return result;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusAddInfoSchema.B7_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USAIILine);
			return result;
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			AIILine child = (AIILine)dependent;
			child.B7_Type = CusAddInfoTypeAttribute.Codes.USAIILine;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (AIILine)child;
			Master.CurrentNonCommittedAIILine = newElement;
			newElement.US_UnitPrice = Master.UnitPrice;
			newElement.US_98InvCurrPerUnit = Master.US_98InvCurrPerUnit;
		}

		protected override void OnNonCommittedAdded(BusinessObject bizOAdded)
		{
			base.OnNonCommittedAdded(bizOAdded);
			var aiiLine = (AIILine)bizOAdded;
			var invoiceLine = aiiLine.Parent;
			if (invoiceLine != null)
			{
				invoiceLine.CurrentNonCommittedAIILine = null;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion
	}
}
