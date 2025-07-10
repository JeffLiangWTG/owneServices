using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public abstract class MessageSendingObjectLookups(BusinessObject parent) : ZLookups(parent)
{
	public CodeDescriptionPairList CustomsOfficeList
	{
		get
		{
			var declarant = Parent.Header?.Declaration?.DeclarantAddress?.Header?.PK ?? ZGuid.Empty;
			var authHeaderType = CustomsAuthorizationHeaderType;

			return Factory.GetCachedValue($"NO.MessageSendingObjectLookups.CustomsOfficeList-{authHeaderType}-{declarant}", () =>
			{
				var customsOfficeList = new CodeDescriptionPairList();
				var authHeader = Customs.Business.CusAuthorisationHeader.Loader.GetAuthorisation(
					Parent.Header?.Factory,
					Core.Constants.CountryCodes.Norway,
					authHeaderType,
					ZDateTime.Today,
					declarant);
				var authRule = authHeader?.CusAuthorisationRules.FirstOrDefault(x => x.CPR_RuleCode == CusAuthorisationRuleTypeList.Codes.MainCustomsOffice);
				authRule?.LinkedCusAuthorisationRules.ForEach(lr => customsOfficeList.AddPair(lr.CPR_ValueFrom, lr.CPR_Description));
				customsOfficeList.DefaultCode = authRule?.CPR_ValueFrom;
				return customsOfficeList;
			});
		}
	}

	public new MessageSendingObject Parent => (MessageSendingObject)base.Parent;

	public CodeDescriptionPairList MessageTypeList
	{
		get
		{
			var isFirstMessageToSend = Parent.IsFirstMessageToSend;
			var subStyle = Parent.Header?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
			var procedurePrefix = Parent.Procedure.SubstringSafe(0, 2);
			var entryStatus = Parent.EntryStatus;

			return Factory.GetCachedValue($"NO.MessageSendingObjectLookups.MessageTypeList-{isFirstMessageToSend}-{subStyle}-{procedurePrefix}-{entryStatus}", () =>
			{
				if (IsDefaultMessageTypeRE(subStyle))
				{
					return GetPairList_RE();
				}
				if (IsDefaultMessageTypeEN(entryStatus))
				{
					return GetPairList_EN();
				}
				if (IsDefaultMessageTypeFO(subStyle))
				{
					return GetPairList_FO();
				}
				if (IsDefaultMessageTypeKO(entryStatus, subStyle))
				{
					return GetPairList_KO();
				}
				if (IsDefaultMessageTypeMA(isFirstMessageToSend, subStyle, procedurePrefix))
				{
					return GetPairList_MA();
				}
				if (IsDefaultMessageTypeFU(isFirstMessageToSend, subStyle, procedurePrefix))
				{
					return GetPairList_FU();
				}

				return [];
			});
		}
	}

	// TODO: These constants are to be removed in a future WI, once they are added as valid values in the GUI
	const string SimplifiedDeclarationSubType = "S";
	const string RecalculationDeclarationSubType = "REC";

	bool IsDefaultMessageTypeFU(bool isFirstMessageToSend, ZString subStyle, ZString procedurePrefix) => isFirstMessageToSend && !IsSubStylePreliminaryOrSimplified(subStyle) && IsProcedureOrdinary(procedurePrefix) && !IsChangeNotification(subStyle);
	bool IsDefaultMessageTypeMA(bool isFirstMessageToSend, ZString subStyle, ZString procedurePrefix) => isFirstMessageToSend && !IsSubStylePreliminaryOrSimplified(subStyle) && !IsProcedureOrdinary(procedurePrefix) && !IsChangeNotification(subStyle);
	bool IsDefaultMessageTypeKO(ZString entryStatus, ZString subStyle) => entryStatus == UniversalReferenceConstants.CusEntryStatus.MEC && !IsSubStylePreliminary(subStyle) && !IsChangeNotification(subStyle);
	bool IsDefaultMessageTypeFO(ZString subStyle) => IsSubStylePreliminary(subStyle);
	bool IsDefaultMessageTypeEN(ZString entryStatus) => entryStatus == UniversalReferenceConstants.CusEntryStatus.UAR;
	bool IsDefaultMessageTypeRE(ZString subStyle) => IsChangeNotification(subStyle);
	bool IsSubStylePreliminaryOrSimplified(string subStyle) => IsSubStylePreliminary(subStyle) || IsSubStyleSimplified(subStyle);
	bool IsSubStylePreliminary(string subStyle) => subStyle == ImportDeclarationSubTypes.Codes.P;
	bool IsSubStyleSimplified(string subStyle) => subStyle == SimplifiedDeclarationSubType;
	bool IsChangeNotification(string subStyle) => subStyle == RecalculationDeclarationSubType;
	bool IsProcedureOrdinary(string procedurePrefix) => OrdinaryProcedurePrefixes.Contains(procedurePrefix);
	static HashSet<string> OrdinaryProcedurePrefixes => ["10", "11", "40", "41"];

	CodeDescriptionPairList GetPairList_FU()
	{
		var messageTypeList = new CodeDescriptionPairList();

		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.CompleteOrdinaryDeclaration, MessageSendingMessageTypes.Descriptions.CompleteOrdinaryDeclaration);
		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.ManualDeclaration, MessageSendingMessageTypes.Descriptions.ManualDeclaration);

		return messageTypeList;
	}

	CodeDescriptionPairList GetPairList_MA()
	{
		var messageTypeList = new CodeDescriptionPairList();

		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.ManualDeclaration, MessageSendingMessageTypes.Descriptions.ManualDeclaration);

		return messageTypeList;
	}

	CodeDescriptionPairList GetPairList_KO()
	{
		var messageTypeList = new CodeDescriptionPairList();

		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.Correction, MessageSendingMessageTypes.Descriptions.Correction);

		return messageTypeList;
	}

	CodeDescriptionPairList GetPairList_FO()
	{
		var messageTypeList = new CodeDescriptionPairList();

		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.PreliminaryDeclaration, MessageSendingMessageTypes.Descriptions.PreliminaryDeclaration);

		return messageTypeList;
	}

	CodeDescriptionPairList GetPairList_EN()
	{
		var messageTypeList = new CodeDescriptionPairList();

		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.FinalDeclaration, MessageSendingMessageTypes.Descriptions.FinalDeclaration);

		return messageTypeList;
	}

	CodeDescriptionPairList GetPairList_RE()
	{
		var messageTypeList = new CodeDescriptionPairList();

		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.RefundDeclaration, MessageSendingMessageTypes.Descriptions.RefundDeclaration);
		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.PostDeclaration, MessageSendingMessageTypes.Descriptions.PostDeclaration);
		messageTypeList.AddPair(MessageSendingMessageTypes.Codes.StatisticalRecalculatedDeclaration, MessageSendingMessageTypes.Descriptions.StatisticalRecalculatedDeclaration);

		return messageTypeList;
	}

	protected abstract ZString CustomsAuthorizationHeaderType { get; }
}
