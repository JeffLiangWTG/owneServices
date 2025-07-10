using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public interface ICusSupportingDocument
{
	ZString Type { get; }
	ZString Reference { get; }
	ZDecimal? Quantity { get; }
	ZDecimal? Value { get; }
}
