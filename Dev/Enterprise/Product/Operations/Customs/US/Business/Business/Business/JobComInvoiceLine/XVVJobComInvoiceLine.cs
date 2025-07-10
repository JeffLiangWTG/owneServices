using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public partial class JobComInvoiceLine : IXVVLine
	{
		public ZBool IsSetXLine
		{
			get
			{
				if (isSetXLineCached == null)
				{
					isSetXLineCached = new CachedProperty<ZBool>(Factory, ImportHelper.GetIsSetXLine);
				}
				return isSetXLineCached.Value;
			}
		}
		CachedProperty<ZBool> isSetXLineCached;

		public ZBool IsSetVLine
		{
			get
			{
				if (isSetVLineCached == null)
				{
					isSetVLineCached = new CachedProperty<ZBool>(Factory, ImportHelper.GetIsSetVLine);
				}
				return isSetVLineCached.Value;
			}
		}
		CachedProperty<ZBool> isSetVLineCached;

		public ZBool IsVParentLine
		{
			get
			{
				if (isVParentLineCached == null)
				{
					isVParentLineCached = new CachedProperty<ZBool>(Factory, ImportHelper.GetIsVParentLine);
				}
				return isVParentLineCached.Value;
			}
		}
		CachedProperty<ZBool> isVParentLineCached;

		public ZBool IsVChildLine
		{
			get
			{
				if (isVChildLineCached == null)
				{
					isVChildLineCached = new CachedProperty<ZBool>(Factory, ImportHelper.GetIsVChildLine);
				}
				return isVChildLineCached.Value;
			}
		}
		CachedProperty<ZBool> isVChildLineCached;

		public JobComInvoiceLine VParentLine => IsVChildLine ? ParentTariffLine : null;

		public IEnumerable<JobComInvoiceLine> ChildVLines => ImportHelper.GetChildVLines().Cast<JobComInvoiceLine>();

		public JobComInvoiceLine FirstVLine
		{
			get
			{
				JobComInvoiceLine result = null;
				if (IsSetXLine || IsVParentLine)
				{
					result = (from child in ChildLines
							  orderby child.JI_LineNo
							  select child).FirstOrDefault();
				}
				return result;
			}
		}

		IEnumerable<IXVVLine> IXVVLine.ChildVLines => ChildVLines;

		IXVVLine IXVVLine.FirstVLine => FirstVLine;
	}
}
