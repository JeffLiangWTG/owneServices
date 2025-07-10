using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsItemPreviousDocumentProvider : IPreviousDocument
{
	public AESGoodsItemPreviousDocumentProvider(CusSupportingInfo cusSupportingInfo, ZString procedureCode)
	{
		this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
		this.procedureCode = procedureCode;
	}
	readonly CusSupportingInfo cusSupportingInfo;
	readonly ZString procedureCode;

	public int? GoodsItemNumber => CachedValueHelper.GetValue(ref goodsItemNumber, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_LineNo));
	CachedValue<int?> goodsItemNumber;

	public string TypeOfPackages => CachedValueHelper.GetValue(ref typeOfPackages, GetTypeOfPackages);
	CachedValue<string> typeOfPackages;

	public int? NumberOfPackages => CachedValueHelper.GetValue(ref numberOfPackages, GetNumberOfPackages);
	CachedValue<int?> numberOfPackages;

	public string MeasurementUnitAndQualifier => CachedValueHelper.GetValue(ref measurementUnitAndQualifier, GetMeasurementUnitAndQualifier);
	CachedValue<string> measurementUnitAndQualifier;

	public decimal? QuantityValue => CachedValueHelper.GetValue(ref quantity, GetQuantity);
	CachedValue<decimal?> quantity;

	public string Type => CachedValueHelper.GetValue(ref type, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_Code));
	CachedValue<string> type;

	public string Description => CachedValueHelper.GetValue(ref description, GetDescriptionCore);
	protected virtual string GetDescriptionCore() => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_ReferenceNumber);
	CachedValue<string> description;

	int? GetNumberOfPackages()
	{
		var result = (int?)null;

		var quantity = cusSupportingInfo.CSI_PackQty;
		if (CheckRuleG0133()
			&& IsPackingInfoAvailable())
		{
			result = quantity;
		}

		return result;
	}

	string GetTypeOfPackages() => CheckRuleG0133() && IsPackingInfoAvailable() ? MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_PackType) : null;

	decimal? GetQuantity() => CheckRuleG0133() ? MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_Quantity) : null;

	string GetMeasurementUnitAndQualifier() => CheckRuleG0133() ? MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_UnitOfQuantity) : null;

	bool CheckRuleG0133() => procedureCode == Constants.ProcedureCodes._31;

	bool IsPackingInfoAvailable() => !cusSupportingInfo.CSI_PackQty.IsEmpty
									&& cusSupportingInfo.CSI_PackQty > ZDecimal.Zero
									&& !cusSupportingInfo.CSI_PackType.IsEmpty;
}
