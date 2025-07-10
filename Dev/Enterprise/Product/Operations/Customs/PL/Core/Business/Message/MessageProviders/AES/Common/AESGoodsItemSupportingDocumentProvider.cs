using System;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class AESGoodsItemSupportingDocumentProvider : ISupportingDocument
{
	public AESGoodsItemSupportingDocumentProvider(CusSupportingInfo cusSupportingInfo, Func<ZDecimal> getAmount, Func<ZDecimal> getQuantity)
	{
		this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
		this.getAmount = getAmount ?? (() => cusSupportingInfo.CSI_Value);
		this.getQuantity = getQuantity ?? (() => cusSupportingInfo.CSI_Quantity);
	}
	readonly CusSupportingInfo cusSupportingInfo;
	readonly Func<ZDecimal> getAmount;
	readonly Func<ZDecimal> getQuantity;

	public int? DocumentLineItemNumber => CachedValueHelper.GetValue(ref documentLineItemNumber, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_ItemNumber));
	CachedValue<int?> documentLineItemNumber;

	public string IssuingAuthorityName => CachedValueHelper.GetValue(ref issuerAuthorityName, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_AdditionalDescription));
	CachedValue<string> issuerAuthorityName;

	public DateTime? ValidityDateValue => CachedValueHelper.GetValue(ref validityDate, () => MessageProviderHelper.ReturnNullIfInvalid(cusSupportingInfo.CSI_DateOfExpiry));
	CachedValue<DateTime?> validityDate;

	public string MeasurementUnitAndQualifier => CachedValueHelper.GetValue(ref measurementUnitAndQualifier, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_UnitOfQuantity));
	CachedValue<string> measurementUnitAndQualifier;

	public decimal? QuantityValue => CachedValueHelper.GetValue(ref quantity, () => MessageProviderHelper.ReturnNullIfEmpty(getQuantity()));
	CachedValue<decimal?> quantity;

	public string Currency => CachedValueHelper.GetValue(ref currency, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_RX_NKCurrency));
	CachedValue<string> currency;

	public decimal? AmountValue => CachedValueHelper.GetValue(ref amount, () => MessageProviderHelper.ReturnNullIfEmpty(getAmount()));
	CachedValue<decimal?> amount;

	public string Type => CachedValueHelper.GetValue(ref type, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_Code));
	CachedValue<string> type;

	public string Description => CachedValueHelper.GetValue(ref description, GetDescriptionCore);
	protected virtual string GetDescriptionCore() => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_ReferenceNumber);
	CachedValue<string> description;
}
