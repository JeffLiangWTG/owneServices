
namespace Enterprise.Customs.SG.V4.Business
{
	class LineNumberAssigner : Customs.Business.LineNumberAssigner
	{
		public LineNumberAssigner(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
			EntryHeader.CH_HighestLineNumber = 0;
		}

		protected override void AssignLineNumber(Customs.Business.CusEntryLine entryLine)
		{
			EntryHeader.CH_HighestLineNumber++;
			entryLine.CL_LineNumber = EntryHeader.CH_HighestLineNumber;
		}
	}
}
