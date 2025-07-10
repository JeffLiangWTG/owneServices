using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.DMS.Response_1p30;
using CargoWise.Customs.NL.MessageDefinitions.XML.Control_1p15;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business;

public static class DMSResponseMessageHelper
{
	public static IDMSIncomingDataProvider CreateDMSIncomingDataProvider(ZString messageText)
	{
		IDMSIncomingDataProvider result = null;
		try
		{
			var metaData = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<MetaData>(messageText);
			result = new DMSIncomingDataProvider(metaData);
		}
		catch (Exception ex)
		{
			ErrorReporter.ReportOnce($"Failed to deserialize incoming NL DMS xml: \n{messageText} \nException: \n{ex} ");
		}
		return result;
	}

	public static IControlIncomingDataProvider CreateControlIncomingDataProvider(ZString messageText)
	{
		IControlIncomingDataProvider result = null;
		try
		{
			var metaData = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.Deserialize<XmlControl>(messageText);
			result = new ControlIncomingDataProvider(metaData);
		}
		catch (Exception ex)
		{
			ErrorReporter.ReportOnce($"Failed to deserialize incoming NL Control xml: \n{messageText} \nException: \n{ex} ");
		}
		return result;
	}

	public static ZString GetMessageEntryStatus(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		switch (dataProvider.WCOTypeCode)
		{
			case WCoTypeCodes.AmendmentAccepted:
			case WCoTypeCodes.ExportAmendmentAccepted:
				return EntryStatus.AmendmentAccepted;
			case WCoTypeCodes.InvalidationConfirmation:
				return EntryStatus.Cancelled;
			case WCoTypeCodes.DeclarationAcceptance:
				return EntryStatus.AdvanceDeclarationReceived;
			case WCoTypeCodes.Acceptance:
				return EntryStatus.Accepted;
			case WCoTypeCodes.ExportAcceptance:
				return EntryStatusNew.MRNAllocated;
			case WCoTypeCodes.Release:
				return GetReleaseMessageStatuses(dataProvider).EntryStatus;
			case WCoTypeCodes.ExportRelease:
				return GetReleaseMessageStatusesForExport(dataProvider).EntryStatus;
			case WCoTypeCodes.SupplementReminder:
				return EntryStatus.SupplementReminder;
			case WCoTypeCodes.ExportSupplementReminder:
				if (entryHeader?.EntryInstruction?.CEI_SubStyle.In(new ZString[] { EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF }) ?? false)
				{
					return EntryStatusNew.ProvisionalRelease;
				}
				return EntryStatus.SupplementReminder;
			case WCoTypeCodes.ReminderForInformation:
				return ZString.Empty;
			case WCoTypeCodes.NoRelease:
				return EntryStatus.NoRelease_NRE;
			case WCoTypeCodes.Rejection:
			case WCoTypeCodes.ExportRejection:
				return GetRejectionMessageEntryStatus(entryHeader, dataProvider);
			case WCoTypeCodes.ControlNotification:
			case WCoTypeCodes.ExportControlNotification:
				return GetControlNotificationMessageStatuses(dataProvider).EntryStatus;
			case WCoTypeCodes.Cancellation:
				return EntryStatus.ExportCancellation;
			case WCoTypeCodes.CancellationReply:
				if (IsStatusEntryStatusNewForCancellationReply(entryHeader))
				{
					return NLConstants.EntryStatusNew.NoRelease;
				}
				return EntryStatus.CancellationReply;
			case WCoTypeCodes.ExitReminder:
				return IsSubstyleEorForXorY(entryHeader) ? entryHeader.CH_EntryStatus : EntryStatus.NoExitInformationRecievedYet;
			case WCoTypeCodes.Ext:
				return ZString.Empty;
			case WCoTypeCodes.ReceiveMessage:
				return GetReceiveMessageEntryStatus(entryHeader);
			case WCoTypeCodes.PreliminaryDeclarationAccepted:
				return EntryStatus.AdvanceDeclarationReceived;
			case WCoTypeCodes.RequestForInformation:
				return GetRequestForInformationMessageEntryStatus(dataProvider);
			case WCoTypeCodes.RequestForInformationReminder:
				if (entryHeader?.CH_PhaseStatus.EqualsIgnoringCase(CustomsEntryPhaseStatusList.Codes.CRE) ?? false)
				{
					return ZString.Empty;
				}
				return EntryStatus.ExportReminder_NoExitInformationReceived;
			default:
				return ZString.Empty;
		}
	}

	public static bool IsStatusEntryStatusNewForCancellationReply(CusEntryHeader entryHeader) => (entryHeader?.CH_Status ?? ZString.Empty).In(new ZString[] { NLConstants.StatusNew.SentToCustoms, NLConstants.StatusNew.Error, NLConstants.StatusNew.Invalid, NLConstants.StatusNew.Accepted }) &&
																								 (entryHeader?.CH_EntryStatus ?? ZString.Empty).In(new ZString[] { NLConstants.EntryStatusNew.RequestForInformation, NLConstants.EntryStatusNew.PreLodged, NLConstants.EntryStatusNew.MRNAllocated, NLConstants.EntryStatusNew.Released, NLConstants.EntryStatusNew.DocumentsControl, NLConstants.EntryStatusNew.PhysicalInspection, NLConstants.EntryStatusNew.ProvisionalRelease, NLConstants.EntryStatusNew.Received });

	public static (ZString EntryStatus, ZString Status) GetReleaseMessageStatuses(IDMSIncomingDataProvider dataProvider)
	{
		var statusNameCode = dataProvider.Statuses.FirstOrDefault()?.NameCode;
		switch (statusNameCode)
		{
			case StatusNameCodes.Released:
				return (EntryStatus.ReleasedAndTaxed, Status.CLE);
			case StatusNameCodes.NoRelease:
				return (EntryStatus.NoRelease, Status.Cancelled);
			case StatusNameCodes.ProvisionalRelease:
				return (EntryStatus.ProvisionalRelease, Status.ROG);
			default:
				return (ZString.Empty, ZString.Empty);
		}
	}

	public static (ZString EntryStatus, ZString Status) GetReleaseMessageStatusesForExport(IDMSIncomingDataProvider dataProvider)
	{
		var statusNameCode = dataProvider.Statuses.FirstOrDefault()?.NameCode;
		switch (statusNameCode)
		{
			case StatusNameCodes.Released:
				return (EntryStatusNew.Released, StatusNew.Accepted);
			case StatusNameCodes.NoRelease:
				return (EntryStatusNew.NoRelease, StatusNew.Accepted);
			case StatusNameCodes.ProvisionalRelease:
				return (EntryStatusNew.ProvisionalRelease, StatusNew.Accepted);
			default:
				return (ZString.Empty, ZString.Empty);
		}
	}

	public static bool IsSubstyleEorForXorY(CusEntryHeader entryHeader) => (entryHeader?.EntryInstruction?.CEI_SubStyle ?? ZString.Empty).In(new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF });

	public static (ZString EntryStatus, ZString Status) GetControlNotificationMessageStatuses(IDMSIncomingDataProvider dataProvider)
	{
		var entryStatus = ZString.Empty;
		var status = ZString.Empty;
		var statusQualifier = dataProvider.Controls.FirstOrDefault()?.TypeCode;
		if (!string.IsNullOrEmpty(statusQualifier))
		{
			entryStatus = PrefixStatusWith3(statusQualifier);
			if (statusQualifier == ControlTypeList.Codes.DocumentsControl)
			{
				status = MessageStatuses.DOC;
			}
			else
			{
				status = MessageStatuses.FYC;
			}
		}
		return (entryStatus, status);
	}

	public static (ZString EntryStatus, ZString Status) GetControlNotificationMessageStatusesForExport(IDMSIncomingDataProvider dataProvider)
	{
		var entryStatus = ZString.Empty;
		var statusQualifier = dataProvider.Controls.FirstOrDefault()?.TypeCode;
		if (!string.IsNullOrEmpty(statusQualifier))
		{
			if (statusQualifier == ControlTypes.DocumentsControl)
			{
				entryStatus = EntryStatusNew.DocumentsControl;
			}
			else
			{
				entryStatus = EntryStatusNew.PhysicalInspection;
			}
		}
		return (entryStatus, StatusNew.Accepted);
	}

	public static ZString GetFormattedShortDateString(DateTime? date) => date == null ? string.Empty : date.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

	public static ZString GetFormattedLocalLongTimeString(DateTime? dateTime) => dateTime == null ? string.Empty : dateTime.Value.ToLocalTime().ToString("dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture);

	public static ZString GetRefNumbersForRequestedDocument(CusEntryHeader entryHeader, string typeCode)
	{
		var refNumList = ZString.Empty;

		if (entryHeader != null && entryHeader.EntryInstruction is CusEntryInstruction entryInstruction)
		{
			foreach (var supportingDocument in entryInstruction.SupportingDocuments)
			{
				if (supportingDocument.CSI_Type.Equals(typeCode) && AllowedSupportingInfoTypes().Contains(supportingDocument.CSI_Type))
				{
					refNumList = refNumList + ", " + supportingDocument.CSI_ReferenceNumber;
				}
			}

			foreach (var additionalDocument in entryInstruction.AdditionalInfos)
			{
				if (additionalDocument.CSI_SubType.Equals(typeCode) && AllowedSupportingInfoTypes().Contains(additionalDocument.CSI_SubType))
				{
					refNumList = refNumList + ", " + additionalDocument.CSI_ReferenceNumber;
				}
			}

			refNumList = refNumList.TrimStart(',');
			refNumList = refNumList.TrimStart();
		}

		return refNumList;
	}

	static ZString[] AllowedSupportingInfoTypes() => new ZString[] { AdditionalInfoSubTypeList.Codes.TransportDocument, AdditionalInfoSubTypeList.Codes.AdditionalReference, AdditionalInfoSubTypeList.Codes.AdditionalInformation };

	static ZString GetRejectionMessageEntryStatus(CusEntryHeader entryHeader, IDMSIncomingDataProvider dataProvider)
	{
		var subStyle = entryHeader.EntryInstruction?.CEI_SubStyle ?? ZString.Empty;
		switch (dataProvider.BusinessRejectionTypeCode)
		{
			case RejectionStatus.Rejection_413:
				return EntryStatus.Rejection_413;
			case RejectionStatus.Rejection_414:
				return EntryStatus.Rejection_414;
			case RejectionStatus.Rejection_415:
				switch (subStyle)
				{
					case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE:
					case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF:
						return EntryStatus.Rejection_415_for_SubStyle_XY;
					default:
						return EntryStatus.Rejection_415_not_SubStyle_XY;
				}
			case RejectionStatus.Rejection_432:
				return EntryStatus.Rejection_432;
			case RejectionStatus.Rejection_CRI:
				return EntryStatus.Rejection_CRI;
			case RejectionStatus.Rejection_513:
				return EntryStatus.ExportRejection_513;
			case RejectionStatus.Rejection_514:
				return EntryStatus.ExportRejection_514;
			case RejectionStatus.Rejection_515:
				switch (subStyle)
				{
					case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE:
					case EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF:
						return EntryStatus.ExportRejection_515_for_SubStyle_XY;
					default:
						return EntryStatus.ExportRejection_515_not_SubStyle_XY;
				}
			case RejectionStatus.Rejection_511:
				return EntryStatus.ExportRejection_511;
			case RejectionStatus.Rejection_CRE:
				return EntryStatus.ExportRejection_CRE;
			case RejectionStatus.Rejection_583:
				return EntryStatus.ExportRejection_583;
			default:
				return ZString.Empty;
		}
	}

	static ZString GetReceiveMessageEntryStatus(CusEntryHeader entryHeader)
	{
		switch (entryHeader.CH_EntryStatus)
		{
			case EntryStatus.AdvanceDeclarationSent:
				return EntryStatus.AdvanceDeclarationReceived;
			case EntryStatus.InformationSentToCustoms:
				return EntryStatus.InformationReceivedByCustoms;
			case EntryStatus.SupplementSent:
				return EntryStatus.SupplementReceivedByCustoms;
			case EntryStatus.InvalidationRequestSent:
				return EntryStatus.InvalidationRequestReceivedByCustoms;
			case EntryStatus.AmendmentRequestSent:
				return EntryStatus.AmendmentRequestReceivedByCustoms;
			case EntryStatus.ExitInformationDetailsSent:
				return EntryStatus.ExitInformationDetailsReceived;
			default:
				return ZString.Empty;
		}
	}

	static ZString GetRequestForInformationMessageEntryStatus(IDMSIncomingDataProvider dataProvider)
	{
		var result = ZString.Empty;
		var statusQualifier = dataProvider.ControlResults.FirstOrDefault()?.Controls.FirstOrDefault()?.TypeCode;
		if (!string.IsNullOrEmpty(statusQualifier))
		{
			result = PrefixStatusWith3(statusQualifier);
		}
		return result;
	}

	static ZString PrefixStatusWith3(ZString statusQualifier) => "3" + statusQualifier;
	public static ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => linkedObject is CusEntryHeader entry ? entry.RegistryBranchPK : ZGuid.Empty;
}
