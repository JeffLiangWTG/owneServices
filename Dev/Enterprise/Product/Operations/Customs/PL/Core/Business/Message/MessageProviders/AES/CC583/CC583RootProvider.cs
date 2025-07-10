using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

sealed class CC583RootProvider : AESBaseProvider, ICC583CRoot
{
	readonly BaseMessageSendingObjectParent sendingObjectParent;

	public CC583RootProvider(BaseMessageSendingObject sendingObject, BaseMessageSendingObjectParent sendingObjectParent) : base(sendingObject)
	{
		this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
	}

	public ICC583CExportOperation ExportOperation => exportOperation ??= new CC583ExportOperationProvider(SendingObject, sendingObjectParent);
	ICC583CExportOperation exportOperation;

	public ICC583CExitCarrier ExitCarrier => CachedValueHelper.GetValue(ref exitCarrier, () =>
		CheckRuleC0681() ? null : new CC583ExitCarrierProvider(sendingObjectParent.ParentDeclaration));
	CachedValue<ICC583CExitCarrier> exitCarrier;

	public string CustomsOfficeOfExportReferenceNumber => sendingObjectParent.CustomsOffice;

	public string CustomsOfficeOfExitActualReferenceNumber => sendingObjectParent.OfficeOfExitActual;

	public IAESDeclarantWithIdentificationNumbers Declarant => CachedValueHelper.GetValue(ref declarant, () =>
		AESDeclarantWithIdentificationNumbersProvider.NewOrNull(Declaration.DeclarantAddress, Declaration)
	);
	CachedValue<IAESDeclarantWithIdentificationNumbers> declarant;

	public IAESRepresentative Representative => CachedValueHelper.GetValue(ref representative, () =>
		Declaration.JE_DeclarantType == PLRepresentationTypeList.Codes._4Direct
			? AESRepresentativeProvider.NewOrNull(Declaration.Representative?.Header)
			: null);
	CachedValue<IAESRepresentative> representative;

	public IReadOnlyCollection<IAlternativeEvidence> AlternativeEvidences =>
		alternativeEvidences ??= sendingObjectParent.AlternativeEvidences.Cast<AlternativeEvidence>().Select((x, i) => new AlternativeEvidenceProvider(x, i + 1)).ToList();
	IReadOnlyCollection<IAlternativeEvidence> alternativeEvidences;

	public override string MessageType => Constants.MessageType.AES.CC583C;

	protected override string GetCorrelationIdentifier() => SendingObject.ResponseMessage.IsEmpty ? (string)null : SendingObject.ResponseMessage;

	readonly string[] ruleC0681EnquiryInformationCodes = ["1", "2"];

	bool CheckRuleC0681() => ruleC0681EnquiryInformationCodes.Contains(ExportOperation.EnquiryInformationCode);
}
