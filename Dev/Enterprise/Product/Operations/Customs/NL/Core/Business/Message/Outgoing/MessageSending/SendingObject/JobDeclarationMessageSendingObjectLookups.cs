using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public class JobDeclarationMessageSendingObjectLookups : ZLookups
{
	public JobDeclarationMessageSendingObjectLookups(JobDeclarationMessageSendingObject messageSendingObject)
		: base(messageSendingObject)
	{
	}

	public new JobDeclarationMessageSendingObject Parent => (JobDeclarationMessageSendingObject)base.Parent;

	public JobDeclaration JobDeclaration => Parent.Declaration;

	public CodeDescriptionPairList MessageTypeList
	{
		get
		{
			var result = new CodeDescriptionPairList();

			var declaration = JobDeclaration;
			if (declaration.IsImport)
			{
				result = GetImportMessageTypeList();
			}
			else if (declaration.IsExport)
			{
				result = GetExportMessageTypeList();
			}
			return result;
		}
	}

	public CodeDescriptionPairList ExitTypeList => Factory.GetCachedValue<ExitTypeList>();

	public CustomsOfficeCodeCollection ExitCustomsOfficeList => CustomsOfficeCodeCollection.LocalCountryOnlyCustomsOfficesWithRequiredRoles(Factory, Core.Constants.CountryCodes.Netherlands, EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExitInland);

	public CodeDescriptionPairList SecurityTypeList => Factory.GetCachedValue<ExportSecurityTypeList>();

	public CodeDescriptionPairList GetImportMessageTypeList()
	{
		return new ImportSendMessageTypes();
	}

	public CodeDescriptionPairList GetExportMessageTypeList()
	{
		var entryHeader = Parent.Header;
		var entryHeaderStatus = entryHeader.CH_Status;
		var entryStatus = entryHeader.CH_EntryStatus;
		var phaseStatus = entryHeader.CH_PhaseStatus;
		var entryInstructionSubStyle = entryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
		var dmsFallbackIsActive = entryHeader.DMSFallbackIsActive;
		var messageHasBeenSentBeforeFallback = entryHeader.MessageHasBeenSentBeforeFallback;
		return Factory.GetCachedValue($"NL.JobDeclarationMessageSendingObjectLookups|GetExportMessageTypeList|{entryHeaderStatus}|{entryStatus}|{phaseStatus}|{entryInstructionSubStyle}|{dmsFallbackIsActive}|{messageHasBeenSentBeforeFallback}", () =>
		{
			CodeDescriptionPairList messageTypes;
			if (dmsFallbackIsActive)
			{
				messageTypes = new CodeDescriptionPairList();
				if (!messageHasBeenSentBeforeFallback)
				{
					messageTypes.AddPair(ExportSendMessageTypes.Codes.FBK, ExportSendMessageTypes.Descriptions.FBK);
				}
			}
			else
			{
				messageTypes = new ExportSendMessageTypes();
				messageTypes.RemoveCode(ExportSendMessageTypes.Codes.FBK);
				if (!ValidStatusCombinationsForDEC.Any(c => c.entryHeaderStatus == entryHeaderStatus && c.entryStatus == entryStatus && c.phaseStatus == phaseStatus))
				{
					messageTypes.RemoveCode(ExportSendMessageTypes.Codes.DEC);
				}
				if (!ValidStatusCombinationsForAMD.Any(c => c.entryHeaderStatus == entryHeaderStatus && c.phaseStatus == phaseStatus))
				{
					messageTypes.RemoveCode(ExportSendMessageTypes.Codes.AMD);
				}
				if (!ValidStatusCombinationsForEXT.Any(c => c.entryHeaderStatus == entryHeaderStatus && (c.entryStatus == ZString.Empty || c.entryStatus == entryStatus) && c.phaseStatus == phaseStatus))
				{
					messageTypes.RemoveCode(ExportSendMessageTypes.Codes.EXT);
				}
				if (!entryInstructionSubStyle.In(new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC }) || !ValidStatusCombinationsForPRE.Any(c => c.messageStatus == entryHeaderStatus && (c.entryStatus == ZString.Empty || c.entryStatus == entryStatus) && c.phaseStatus == phaseStatus))
				{
					messageTypes.RemoveCode(ExportSendMessageTypes.Codes.PRE);
				}
				if (!ValidStatusCombinationsForCAN.Any(c => c.entryHeaderStatus == entryHeaderStatus && (c.entryStatus.IsEmpty || c.entryStatus == entryStatus) && c.phaseStatus == phaseStatus))
				{
					messageTypes.RemoveCode(ExportSendMessageTypes.Codes.CAN);
				}
				if (!((entryHeaderStatus.IsEmpty || entryHeaderStatus == StatusNew.Error || entryHeaderStatus == StatusNew.Invalid) && phaseStatus == CustomsEntryPhaseStatusList.Codes.CRE))
				{
					messageTypes.RemoveCode(ExportSendMessageTypes.Codes.CRE);
				}
				if (!((entryInstructionSubStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE || entryInstructionSubStyle == EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF)
					&& (entryHeaderStatus.IsEmpty || entryHeaderStatus == StatusNew.Error || entryHeaderStatus == StatusNew.Invalid)
					&& phaseStatus == CustomsEntryPhaseStatusList.Codes.SUP))
				{
					messageTypes.RemoveCode(ExportSendMessageTypes.Codes.SUP);
				}
			}

			return messageTypes;
		});
	}

	List<(ZString entryHeaderStatus, ZString entryStatus, ZString phaseStatus)> ValidStatusCombinationsForDEC => validStatusCombinationsForDEC ??=
	[
		(ZString.Empty, ZString.Empty, ZString.Empty),
		(ZString.Empty, ZString.Empty, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Invalid, EntryStatusNew.DeclarationRejected, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Error, ZString.Empty, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Invalid, ZString.Empty, CustomsEntryPhaseStatusList.Codes._515),
	];

	List<(ZString entryHeaderStatus, ZString phaseStatus)> ValidStatusCombinationsForAMD => validStatusCombinationsForAMD ??=
	[
		(ZString.Empty, CustomsEntryPhaseStatusList.Codes._513),
		(StatusNew.Accepted, CustomsEntryPhaseStatusList.Codes._513),
		(StatusNew.Error, CustomsEntryPhaseStatusList.Codes._513),
		(StatusNew.Invalid, CustomsEntryPhaseStatusList.Codes._513)
	];

	List<(ZString entryHeaderStatus, ZString entryStatus, ZString phaseStatus)> ValidStatusCombinationsForEXT => validStatusCombinationsForEXT ??=
	[
		(StatusNew.Accepted, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.ReminderReceived, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.ReminderReceived, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Error, ZString.Empty, CustomsEntryPhaseStatusList.Codes._583),
		(StatusNew.Accepted, ZString.Empty, CustomsEntryPhaseStatusList.Codes._513),
		(StatusNew.Invalid, ZString.Empty, CustomsEntryPhaseStatusList.Codes._583),
		(StatusNew.Accepted, ZString.Empty, CustomsEntryPhaseStatusList.Codes._514),
		(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.CRE),
		(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.SUP),
	];

	List<(ZString messageStatus, ZString entryStatus, ZString phaseStatus)> ValidStatusCombinationsForPRE => validStatusCombinationsForPRE ??=
	[
		(StatusNew.Accepted, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes.REG),
		(StatusNew.Accepted, EntryStatusNew.PhysicalInspection, CustomsEntryPhaseStatusList.Codes.REG),
		(StatusNew.Accepted, EntryStatusNew.DocumentsControl, CustomsEntryPhaseStatusList.Codes.REG),
		(StatusNew.Error, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
		(StatusNew.Invalid, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
		(StatusNew.Accepted, ZString.Empty, CustomsEntryPhaseStatusList.Codes._513),
		(StatusNew.Accepted, ZString.Empty, CustomsEntryPhaseStatusList.Codes._514),
	];

	List<(ZString entryHeaderStatus, ZString entryStatus, ZString phaseStatus)> ValidStatusCombinationsForCAN => validStatusCombinationsForCAN ??=
	[
		(StatusNew.Accepted, EntryStatusNew.MRNAllocated, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes.REG),
		(StatusNew.SentToCustoms, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
		(StatusNew.Error, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
		(StatusNew.Invalid, EntryStatusNew.PreLodged, CustomsEntryPhaseStatusList.Codes._511),
		(StatusNew.Accepted, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.ReminderReceived, EntryStatusNew.RequestForInformation, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.ReminderReceived, EntryStatusNew.ProvisionalRelease, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.ReminderReceived, EntryStatusNew.Released, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Error, ZString.Empty, CustomsEntryPhaseStatusList.Codes._514),
		(StatusNew.Invalid, ZString.Empty, CustomsEntryPhaseStatusList.Codes._514),
		(StatusNew.Accepted, ZString.Empty, CustomsEntryPhaseStatusList.Codes._514),
		(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.CRE),
		(StatusNew.Accepted, EntryStatusNew.Amended, CustomsEntryPhaseStatusList.Codes._515),
		(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes.SUP),
		(StatusNew.Accepted, EntryStatusNew.Received, CustomsEntryPhaseStatusList.Codes._583),
		(StatusNew.Accepted, ZString.Empty, CustomsEntryPhaseStatusList.Codes._513),
	];

	List<(ZString, ZString, ZString)> validStatusCombinationsForDEC;
	List<(ZString, ZString)> validStatusCombinationsForAMD;
	List<(ZString, ZString, ZString)> validStatusCombinationsForEXT;
	List<(ZString, ZString, ZString)> validStatusCombinationsForCAN;
	List<(ZString, ZString, ZString)> validStatusCombinationsForPRE;
}
