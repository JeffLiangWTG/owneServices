using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class PreviousDocumentExtendedProvider : PreviousDocumentProvider, IPreviousDocumentExtended
{
	public PreviousDocumentExtendedProvider(CusSupportingInfo document) : base(document)
	{
	}

	public int? GoodsItemNumber => document.CSI_ItemNumber == ZInt.Zero ? null : (int?)document.CSI_ItemNumber;

	public string TypeOfPackages => document.CSI_UnitOfQuantity2;

	public int? NumberOfPackages => string.IsNullOrEmpty(TypeOfPackages) ? null : (int?)document.CSI_Quantity2.ToZInt();

	public string MeasurementUnitAndQualifier => document.CSI_UnitOfQuantity;

	public decimal? Quantity => string.IsNullOrEmpty(MeasurementUnitAndQualifier) ? null : (decimal?)document.CSI_Quantity;
}
