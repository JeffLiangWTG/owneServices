using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class CusEntryLine : IXVVLine
	{
		public ZBool IsVParentLine => RandomLine.IsVParentLine;

		public ZBool IsVChildLine => RandomLine.IsVChildLine;

		public IEnumerable<CusEntryLine> ChildVLines
		{
			get
			{
				foreach (var child in ChildLines.Where(x => x.IsSetVLine && !x.CL_AdValoremTariff.IsEmpty))
				{
					yield return child;
				}
			}
		}

		public CusEntryLine FirstVLine
		{
			get
			{
				CusEntryLine result = null;
				if (IsSetXLine || IsVParentLine)
				{
					result = (from child in ChildLines
							  orderby child.CL_Calc_EntryNumber
							  select child).FirstOrDefault();
				}
				return result;
			}
		}

		public ZBool IsSetXLine => RandomLine.IsSetXLine;

		public ZBool IsSetVLine => RandomLine.IsSetVLine;

		IEnumerable<IXVVLine> IXVVLine.ChildVLines => ChildVLines;

		IXVVLine IXVVLine.FirstVLine => FirstVLine;
	}
}
