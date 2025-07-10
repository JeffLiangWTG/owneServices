using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	interface IXVVLine
	{
		ZBool IsSetXLine { get; }
		ZBool IsSetVLine { get; }
		ZBool IsVParentLine { get; }
		ZBool IsVChildLine { get; }
		IEnumerable<IXVVLine> ChildVLines { get; }
		IXVVLine FirstVLine { get; }
	}
}
