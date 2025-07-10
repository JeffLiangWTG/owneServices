using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using EUBusiness = Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business;

public class DeclarationWrapper : IDeclaration
{
	public DeclarationWrapper(JobDeclarationMessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		entryHeader = Argument.NotNull(messageSendingObject.Header, nameof(messageSendingObject.Header));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, nameof(entryHeader.EntryInstruction));
	}
	readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly JobDeclarationMessageSendingObject messageSendingObject;
	readonly CusEntryInstruction entryInstruction;

	public IReadOnlyCollection<IGoodsShipment> GoodsShipments => goodsShipments ??= new List<IGoodsShipment> { new GoodsShipmentWrapper(entryHeader, messageSendingObject) };
	IReadOnlyCollection<IGoodsShipment> goodsShipments;

	public string FunctionalReferenceId => entryHeader.CH_BGMReference;

	public string Id => entryHeader.EntryNumber;

	public string IssueDateTime => ZDateTime.Now.ToString("yyyyMMddHHmmssZ");

	public string DeclarationOffice => declaration.JE_CustomsOffice;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ??= new List<AdditionalInformationWrapper> { new AdditionalInformationWrapper(messageSendingObject.ReasonForInvalidation) };
	IReadOnlyCollection<IAdditionalInformation> additionalInformation;

	public IParty Agent
	{
		get
		{
			IParty result = null;
			if (declaration.JE_DeclarantType == EUBusiness.RepresentationTypeList.Codes._2Direct)
			{
				if (declaration.Representative?.Header is OrgHeader representative)
				{
					result = new AgentDirectWrapper(representative);
				}
			}
			else if (declaration.JE_DeclarantType == EUBusiness.RepresentationTypeList.Codes._3Indirect)
			{
				result = new AgentIndirectWrapper();
			}
			return result;
		}
	}

	public IParty Declarant
	{
		get
		{
			IParty result = null;
			if (declaration.Declarant?.Header is OrgHeader declarantHeader)
			{
				if (declaration.JE_DeclarantType.EqualsIgnoringCase(EUBusiness.RepresentationTypeList.Codes._3Indirect))
				{
					result = PartyWithStaffWrapper.New(declarantHeader, GlbStaff.CurrentUser);
				}
				else
				{
					result = PartyWrapper.New(declarantHeader);
				}
			}
			return result;
		}
	}

	public string LanguageCode => GlbStaff.CurrentUser.GS_WorkingLanguage.EqualsIgnoringCase(Core.SharedConstants.Languages.Dutch) ? Core.Constants.CountryCodes.Netherlands : Core.SharedConstants.Languages.English;

	public string TypeCode => declaration.JE_EntryStyle + entryInstruction.CEI_SubStyle;

	public IReadOnlyCollection<IAuthorisation> Authorisations => authorisations ??= entryInstruction.CusAuthorizationUsages.Select((x, index) => new AuthorisationWrapper(x, index + 1)).ToArray();
	IReadOnlyCollection<IAuthorisation> authorisations;

	public IParty Importer => PartyWrapper.New(declaration.Importer);

	public IReadOnlyCollection<IObligationGuarantee> ObligationGuarantees => obligationGuarantees ??= declaration.Guarantees.Select((x, index) => new ObligationGuaranteeWrapper((EUBusiness.Declaration.GuaranteeForDeclaration)x, index + 1)).ToArray();
	IReadOnlyCollection<IObligationGuarantee> obligationGuarantees;

	public string Payer => declaration.PaymentPartyEORINumber;

	public string SupervisingOffice => declaration.CustomsOffices.Cast<EuOfficeCode>().FirstOrDefault(x => x.CY_Code.EqualsIgnoringCase(EuOfficeCodesTypes.Codes.AuthorityControlCode) && !x.CY_Data.IsEmpty)?.CY_Data ?? string.Empty;

	public string Surety => declaration.ControllingCustomer?.GetIdentificationNumber();

	public decimal CustomsValuationValue => entryHeader.InvoiceHeaders.Sum(
				x => x.Charges.Cast<Customs.Business.BaseInvoiceCharge>().Where(y => y.J7_ChargeType.EqualsIgnoringCase(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight)).Sum(z => z.J7_Amount));

	public string CustomsValuationCurrency => entryHeader.InvoiceHeaders.SelectMany(x => x.Charges).Cast<Customs.Business.BaseInvoiceCharge>()
				.FirstOrDefault(x => x.J7_ChargeType.EqualsIgnoringCase(Customs.Common.CustomsChargeTypeList.Codes.OverseasFreight) && !x.J7_RX_NKCurrency.IsEmpty)
				?.J7_RX_NKCurrency ?? string.Empty;

	public IParty Carrier => GetExporterDocAddressOrgWrapper();

	public string ExitOffice => declaration.OfficeOfExit;

	public string PresentationDateTime => declaration.ZG_PresentationStartDate.IsEmpty ? null : declaration.ZG_PresentationStartDate.ToString("yyyyMMddHHmmssZ");

	public string SecurityCode => declaration.ZG_TypeOfSecurity;

	public string SpecificCircumstancesCode => declaration.ZG_SpecificCircumstanceIndicator;

	public IParty Exporter => GetExporterDocAddressOrgWrapper();

	IParty GetExporterDocAddressOrgWrapper() => PartyWrapper.New(declaration.ExporterDocAddress.Organisation);
}
