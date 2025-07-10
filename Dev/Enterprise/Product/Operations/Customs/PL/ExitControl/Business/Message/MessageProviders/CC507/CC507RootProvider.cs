using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC507RootProvider(CusExitReport exitReport) : ICC507CRoot
{
	readonly CusExitReport cusExitReport = Argument.NotNull(exitReport, nameof(exitReport));
	readonly JobDeclaration jobDeclaration = (JobDeclaration)exitReport.Header?.Declaration;

	public string MessageType => Constants.MessageType.ExitControl.CC507C;

	public ICC507CExportOperation ExportOperation => exportOperation ??= new CC507CExportOperationProvider(cusExitReport);
	ICC507CExportOperation exportOperation;

	public IReadOnlyCollection<IAuthorisationNumber> AuthorisationNumbers => authorisationNumbers ??= Array.Empty<IAuthorisationNumber>(); // todo in the future
	IReadOnlyCollection<IAuthorisationNumber> authorisationNumbers;

	public string CustomsOfficeOfExitActualReferenceNumber => cusExitReport.CER_OfficeOfExit;

	public ICC507CGoodsShipment GoodsShipment => goodsShipment ??= new CC507CGoodsShipmentProvider(cusExitReport);
	ICC507CGoodsShipment goodsShipment;

	public IExporter Exporter => null;

	public IAESDeclarantWithIdentificationNumbers Declarant => null;

	public IAESRepresentative Representative => null;

	public string MessageSender => CachedValueHelper.GetValue(ref messageSender,
		() => IsAESDeclaration
			? Constants.AESMessageProvidersConstants.MessageRecipient
			: GetMessageSender());
	CachedValue<string> messageSender;

	public string MessageRecipient => CachedValueHelper.GetValue(ref messageRecipient,
		() => IsAESDeclaration
			? Constants.AESMessageProvidersConstants.MessageRecipient
			: GetMessageSender());
	CachedValue<string> messageRecipient;

	public DateTime PreparationDateAndTime => CachedValueHelper.GetValue(ref preparationDateAndTime, () => ZDateTime.UtcNow.ToDateTime());
	CachedValue<DateTime> preparationDateAndTime;

	public string OperatorEmail => CachedValueHelper.GetValue(ref operatorEmail, GetOperatorEmail);
	CachedValue<string> operatorEmail;

	public string OfficeIdentifier => null;

	public string MessageIdentification => EDIMessage.PLMessageNumberPlaceHolder;

	public string CorrelationIdentifier => null;

	bool IsAESDeclaration => jobDeclaration is not null;

	static string GetMessageSender()
	{
		string result = null;
		if (!GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
		{
			result = GlbBranch.CurrentBranch.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
		}
		if (string.IsNullOrEmpty(result)
		  && !GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
		{
			result = GlbCompany.CurrentCompany.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
		}
		return result ?? string.Empty;
	}

	string GetOperatorEmail()
	{
		var glbStaff = GlbStaff.CurrentUser;
		var result = PL.Business.GlbStaffWrapper.Get(glbStaff)
			.GetGlbExternalPassword<CommunicationChannel>(PasswordTypesList.Codes.PLC, GlbCompany.CurrentCompany.PK)
			?.GP_MailBoxID ?? ZString.Empty;

		if (result.IsEmpty)
		{
			result = PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value;
		}

		return MessageProviderHelper.ReturnNullIfEmpty(result);
	}
}
