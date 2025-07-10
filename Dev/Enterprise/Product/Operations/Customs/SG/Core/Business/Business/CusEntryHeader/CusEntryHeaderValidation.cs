
using System.Globalization;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusEntryHeaderValidation : Customs.Business.CusEntryHeaderValidation
	{
		public CusEntryHeaderValidation(CusEntryHeader parent)
			: base(parent)
		{
		}

		public new CusEntryHeader Parent
		{
			get { return (CusEntryHeader)base.Parent; }
		}

		protected override void CheckEntryNumber()
		{
			base.CheckEntryNumber();
			if (Parent.Declaration != null && Parent.Declaration.MergedLinesCount > SGConstants.MaxEntryLines)
			{
				var messageToShow = string.Format(CultureInfo.CurrentCulture, SGConstants.MaxLinesValidation.MaxEntryLimitForDeclaration + "\r\nThe merged line count is currently {0} entry lines.", Parent.Declaration.MergedLinesCount);
				Parent.EntryNumberInfo.AddMessageError(messageToShow);
			}
		}
	}
}
