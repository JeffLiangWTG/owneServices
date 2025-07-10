using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

public abstract class AESBaseProvider
{
	protected AESBaseProvider(BaseMessageSendingObject sendingObject)
	{
		SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
		EntryHeader = Argument.NotNull(SendingObject.Header, $"{nameof(SendingObject)}.{nameof(BaseMessageSendingObject.Header)}");
		Declaration = Argument.NotNull(EntryHeader.Declaration, $"{nameof(EntryHeader)}.{nameof(CusEntryHeader.Declaration)}");
		EntryInstruction = Argument.NotNull(EntryHeader.EntryInstruction, $"{nameof(EntryHeader)}.{nameof(CusEntryHeader.EntryInstruction)}");
	}
	protected CusEntryHeader EntryHeader { get; }
	protected JobDeclaration Declaration { get; }
	protected CusEntryInstruction EntryInstruction { get; }
	protected BaseMessageSendingObject SendingObject { get; }

	public string MessageSender => CachedValueHelper.GetValue(ref messageSender, GetMessageSender);
	CachedValue<string> messageSender;

	public string MessageRecipient => Constants.AESMessageProvidersConstants.MessageRecipient;

	public DateTime PreparationDateAndTime => CachedValueHelper.GetValue(ref preparationDateAndTime, () => ZDateTime.UtcNow.ToDateTime());
	CachedValue<DateTime> preparationDateAndTime;

	public string MessageIdentification => EDIMessage.PLMessageNumberPlaceHolder;

	public abstract string MessageType { get; }

	public string CorrelationIdentifier => CachedValueHelper.GetValue(ref correlationIdentifier, GetCorrelationIdentifier);
	CachedValue<string> correlationIdentifier;

	public string OperatorEmail => CachedValueHelper.GetValue(ref operatorEmail, GetOperatorEmail);
	CachedValue<string> operatorEmail;

	public string OfficeIdentifier => CachedValueHelper.GetValue(ref officeIdentifier, GetOfficeIdentifier);
	CachedValue<string> officeIdentifier;

	protected virtual string GetCorrelationIdentifier() => null;

	string GetOperatorEmail()
	{
		var glbStaff = GlbStaff.CurrentUser;
		var result = GlbStaffWrapper.Get(glbStaff)
			.GetGlbExternalPassword<CommunicationChannel>(PasswordTypesList.Codes.PLC, GlbCompany.CurrentCompany.PK)
			?.GP_MailBoxID ?? ZString.Empty;

		if (result.IsEmpty)
		{
			result = PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value;
		}

		return MessageProviderHelper.ReturnNullIfEmpty(result);
	}

	string GetMessageSender()
	{
		var result = string.Empty;
		if (!GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
		{
			result = GlbBranch.CurrentBranch.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;
		}
		if (string.IsNullOrEmpty(result)
			&& !GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
		{
			result = GlbCompany.CurrentCompany.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;
		}
		return result;
	}

	string GetOfficeIdentifier()
	{
		var result = string.Empty;
		if (!GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsEmpty)
		{
			result = GlbBranch.CurrentBranch.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.EDISiteID, CountryCodes.Poland) ?? ZString.Empty;
		}
		if (string.IsNullOrEmpty(result)
			&& !GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
		{
			result = GlbCompany.CurrentCompany.OrgProxy?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.EDISiteID, CountryCodes.Poland) ?? ZString.Empty;
		}
		bool theLengthIsValid = !string.IsNullOrEmpty(result) && result.Length == OrgCusCodesLength.EidAcceptedLength;
		return theLengthIsValid ? result : string.Empty;
	}
}
