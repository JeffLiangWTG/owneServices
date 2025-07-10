using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface IItemLineDetails
{
	ZInt LineItem { get; }
	ZDecimal GrossWeight { get; }
	ZDecimal NettWeight { get; }
	ZString TariffCode { get; }
	ZString Procedure { get; }
	ZString UNDG { get; }
	ZString GoodsDescription { get; }
	ZString QtyUoM { get; }
	ZDecimal StatValue { get; }
	ZDecimal CustomsValue { get; }
	ZString PackageMarks { get; }
	IEnumerable<ICusSupportingDocument> AdditionalDocuments { get; }
}
