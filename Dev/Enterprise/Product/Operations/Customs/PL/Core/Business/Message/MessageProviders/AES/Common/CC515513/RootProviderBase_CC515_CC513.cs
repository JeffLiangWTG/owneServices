using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public abstract class RootProviderBase_CC515_CC513<TExportOperation> : AESBaseProviderWithPartiesBase
	where TExportOperation : class, IExportOperation
{
	protected RootProviderBase_CC515_CC513(BaseMessageSendingObject sendingObject)
		: base(sendingObject)
	{
	}

	public IReadOnlyCollection<IAuthorisationNumber> AuthorisationNumbers => authorisationNumbers ??= EntryInstruction.CusAuthorizationUsages
		.GroupBy(x => new { x.AGC_Code, x.AGC_Number })
		.Select((x, i) => new AESAuthorisationNumberProvider(x.First(), i + 1))
		.ToArray<IAuthorisationNumber>();
	IReadOnlyCollection<IAuthorisationNumber> authorisationNumbers;

	public string CustomsOfficeOfPresentationReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfPresentationReferenceNumber, () => Declaration.CustomsOfficeOfPresentationReferenceNumber());
	CachedValue<string> customsOfficeOfPresentationReferenceNumber;

	public string CustomsOfficeOfSupervisingReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfSupervisingReferenceNumber, () =>
		Declaration.CustomsOfficesForBinding.Cast<EuOfficeCode>().FirstOrDefault(o => o.CY_Code == EuOfficeCodesTypes.Codes.SupervisingCustomsOffice && !o.CY_Data.IsEmpty)?.CY_Data
		?? string.Empty);
	CachedValue<string> customsOfficeOfSupervisingReferenceNumber;

	public string CustomsOfficeOfExportReferenceNumber => Declaration.JE_CustomsOffice;

	public string CustomsOfficeOfExitDeclaredReferenceNumber => Declaration.JE_OfficeOfEntryExit;

	public ICurrencyExchange CurrencyExchange => CachedValueHelper.GetValue(ref currencyExchange, GetCurrencyExchange);
	CachedValue<ICurrencyExchange> currencyExchange;

	public string DeferredPayment => null; // future use

	public IGoodsShipment GoodsShipment => CachedValueHelper.GetValue(ref goodsShipment, GetGoodsShipment);
	CachedValue<IGoodsShipment> goodsShipment;

	public TExportOperation ExportOperation => CachedValueHelper.GetValue(ref exportOperation, CreateExportOperationCore);
	CachedValue<TExportOperation> exportOperation;

	protected abstract TExportOperation CreateExportOperationCore();

	protected ICurrencyExchange GetCurrencyExchange() => AesRuleHelper.CheckRuleR0089E(EntryInstruction)
		? null
		: new AESCurrencyExchangeProvider(EntryInstruction);

	protected IGoodsShipment GetGoodsShipment() => new GoodsShipmentProvider_CC515_CC513(EntryHeader);

	protected override IExporter GetExporterCore(OrgAddress orgAddress) => ExporterProvider_CC515_CC513.NewOrNull(orgAddress, EntryInstruction.IsAESTransitionPeriod());

	protected override IAESDeclarantWithIdentificationNumbers GetDeclarantCore(OrgAddress orgAddress) => DeclarantWithIdentificationNumbersProvider_CC515_CC513.NewOrNull(orgAddress, Declaration, EntryInstruction.IsAESTransitionPeriod());
}
