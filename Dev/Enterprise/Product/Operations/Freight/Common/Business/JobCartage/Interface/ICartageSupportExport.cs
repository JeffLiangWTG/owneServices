using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Common.Business
{
	public interface ICartageParentExtra
	{
		JobDocAddress ConsigneeDocumentaryAddress { get; }
		JobDocAddress ConsignorDocumentaryAddress { get; }

		ZString CustomAttrib1 { get; }
		ZString CustomAttrib2 { get; }
		ZDateTime CustomDate1 { get; }
		ZDateTime CustomDate2 { get; }
		ZDecimal CustomDecimal1 { get; }
		ZDecimal CustomDecimal2 { get; }
		ZBool CustomFlag1 { get; }
		ZBool CustomFlag2 { get; }
	}
}
