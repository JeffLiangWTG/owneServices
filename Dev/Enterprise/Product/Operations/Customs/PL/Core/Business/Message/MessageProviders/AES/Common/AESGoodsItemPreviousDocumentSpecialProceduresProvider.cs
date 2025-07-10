using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsItemPreviousDocumentSpecialProceduresProvider : IPreviousDocumentSpecialProcedures
{
	public AESGoodsItemPreviousDocumentSpecialProceduresProvider(CusSupportingInfo cusSupportingInfo)
	{
		this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
	}
	readonly CusSupportingInfo cusSupportingInfo;

	public int GoodsItemNumber => CachedValueHelper.GetValue(ref goodsItemNumber, () => cusSupportingInfo.CSI_LineNo);
	CachedValue<int> goodsItemNumber;

	public string TypeOfPackages => CachedValueHelper.GetValue(ref typeOfPackages, () => IsPackingInfoAvailable() ? MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_PackType) : null);
	CachedValue<string> typeOfPackages;

	public int? NumberOfPackages => CachedValueHelper.GetValue(ref numberOfPackages, () =>
	{
		var quantity = cusSupportingInfo.CSI_PackQty;
		return IsPackingInfoAvailable() ? quantity : null;
	});
	CachedValue<int?> numberOfPackages;

	public string MeasurementUnitAndQualifier => CachedValueHelper.GetValue(ref measurementUnitAndQualifier, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_UnitOfQuantity));
	CachedValue<string> measurementUnitAndQualifier;

	public decimal? QuantityValue => CachedValueHelper.GetValue(ref quantity, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_Quantity));
	CachedValue<decimal?> quantity;

	public string Type => CachedValueHelper.GetValue(ref type, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_Code));
	CachedValue<string> type;

	public string Description => CachedValueHelper.GetValue(ref description, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_ReferenceNumber));
	CachedValue<string> description;

	public int? GoodsShipmentNumber => CachedValueHelper.GetValue(ref goodsShipmentNumber, () => !cusSupportingInfo.CSI_LineNo.IsEmpty
		? MessageProviderHelper.IntReturnNullIfEmpty(cusSupportingInfo.CSI_ReferenceNumber2)
		: null);
	CachedValue<int?> goodsShipmentNumber;

	bool IsPackingInfoAvailable() => !cusSupportingInfo.CSI_PackQty.IsEmpty
									&& cusSupportingInfo.CSI_PackQty > ZDecimal.Zero
									&& !cusSupportingInfo.CSI_PackType.IsEmpty;
}
