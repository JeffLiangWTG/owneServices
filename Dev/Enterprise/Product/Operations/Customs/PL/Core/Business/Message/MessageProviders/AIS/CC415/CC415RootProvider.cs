using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AIS;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AIS.Outgoing;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CC415RootProvider(BaseMessageSendingObject sendingObject) : ICC415CRoot
{
	protected readonly BaseMessageSendingObject SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	protected readonly CusEntryHeader EntryHeader = Argument.NotNull(sendingObject.Header, $"{nameof(SendingObject)}.{nameof(BaseMessageSendingObject.Header)}");
	protected readonly JobDeclaration Declaration = Argument.NotNull(sendingObject.Header.Declaration, $"{nameof(EntryHeader)}.{nameof(CusEntryHeader.Declaration)}");
	protected readonly CusEntryInstruction EntryInstruction = Argument.NotNull(sendingObject.Header.EntryInstruction, $"{nameof(EntryHeader)}.{nameof(CusEntryHeader.EntryInstruction)}");

	public string OfficeIdentifier => null;

	public string LRN => SendingObject.LocalReferenceNumber;

	public string DeclarationType => Declaration.JE_EntryStyle;

	public string AdditionalDeclarationType => EntryInstruction.CEI_SubStyle;

	public string EmailOfTemporaryStorageFacilityOperator => null;

	public string EMailOfWarehouseAuthorisationHolder => null;

	public string InternalCurrencyUnit => null;

	public decimal? TotalAmountInvoiced => null;

	public decimal? ExchangeRate => null;

	public IReadOnlyCollection<IDeferredPayment> DeferredPayments => Array.Empty<IDeferredPayment>();

	public IReadOnlyCollection<IAuthorisation> Authorisations => Array.Empty<IAuthorisation>();

	public IApplicationAndAuthorisationForSpecialProcedures ApplicationAndAuthorisationForSpecialProcedures => null;

	public IPerson Importer => null;

	public IDeclarant Declarant => null;

	public IRepresentative Representative => null;

	public string CustomsOfficeOfPresentation => CachedValueHelper.GetValue(ref customsOfficeOfPresentation, () => Declaration.CustomsOfficeOfPresentationReferenceNumber());
	CachedValue<string> customsOfficeOfPresentation;

	public string SupervisingCustomsOffice => CachedValueHelper.GetValue(ref supervisingCustomsOffice, () =>
		Declaration.CustomsOfficesForBinding.Cast<EuOfficeCode>().FirstOrDefault(o => o.CY_Code == EuOfficeCodesTypes.Codes.SupervisingCustomsOffice && !o.CY_Data.IsEmpty)?.CY_Data
		?? string.Empty);
	CachedValue<string> supervisingCustomsOffice;

	public IReadOnlyCollection<IGuarantee> Guarantees => Array.Empty<IGuarantee>();

	public IReadOnlyCollection<IGoodsShipment> GoodsShipments => Array.Empty<IGoodsShipment>();

	public IReadOnlyCollection<string> OperatorsEmails => Array.Empty<string>();

	public string PersonPayingCustomsDutyIdentificationNumber => null;

	public string PersonProvidingGuaranteeIdentificationNumber => null;

	public string CustomsOfficeOfDeclarationReferenceNumber => Declaration.JE_CustomsOffice;
}
