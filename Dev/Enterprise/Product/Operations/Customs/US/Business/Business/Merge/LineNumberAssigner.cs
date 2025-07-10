using System.Collections.Generic;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	class LineNumberAssigner : Customs.Business.LineNumberAssigner
	{
		public LineNumberAssigner(Customs.Business.CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			lastChildLineNums = new Dictionary<string, short>();
		}

		protected override IComparer<Customs.Business.CusEntryLine> GetEntryLineComparerBeforeLineNumbering(Customs.Business.CusEntryHeader entry)
		{
			return new EntrySummaryEntryLineComparerForNumbering();
		}

		protected override void AssignLineNumber(Customs.Business.CusEntryLine rateEntryLine)
		{
			CusEntryLine entryLine = (CusEntryLine)rateEntryLine;

			if (entryLine.RandomLine.US_ZoneStatus == ZoneStatusList.Codes.Domestic && entryLine.Header != null && entryLine.Header.IsACECargoRelease)
			{
				entryLine.CL_LineNumber = 0;//This is not reported to Customs in SE messages. Should remain as zero.
			}
			else
			{
				short childLineNo = 0;

				if (entryLine.ParentLine == null)
				{
					base.AssignLineNumber(rateEntryLine);
				}
				else
				{
					var key = new EntryLineNumberKey(entryLine.ParentLine, entryLine.ParentLine.IsSetXLine && !entryLine.IsSecondaryTariffLine);
					var keyString = key.ToString();
					lastChildLineNums.TryGetValue(keyString, out childLineNo);
					childLineNo++;
					lastChildLineNums[keyString] = childLineNo;

					if (entryLine.IsSetVLine && !entryLine.ParentLine.IsSetVLine && !entryLine.IsSecondaryTariffLine)
					{
						entryLine.CL_LineNumber = ++lastLineNumber;
					}
					else
					{
						entryLine.CL_LineNumber = entryLine.ParentLine.CL_LineNumber;
					}
				}

				entryLine.US_ChildLineNum = childLineNo;
			}
		}

		readonly Dictionary<string, short> lastChildLineNums;

		class EntryLineNumberKey
		{
			public EntryLineNumberKey(CusEntryLine entryLine, bool forXAndVLines)
			{
				this.entryLine = entryLine;
				this.forXAndVLines = forXAndVLines;
			}

			internal readonly CusEntryLine entryLine;
			readonly bool forXAndVLines;

			public override string ToString()
			{
				return entryLine.PK.ToStringKey() + forXAndVLines;
			}
		}
	}
}
