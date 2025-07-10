using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class MessageHelperLookups : ZLookups
	{
		public MessageHelperLookups(TWMessage parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList GetGovernmentAgencyResponseCodeList(ZString agencyCode)
		{
			var cacheKey = $"TW.Business.MessageProcessors.MessageHelperLookups.GetGovernmentAgencyResponseCodeList.{agencyCode}";
			return Factory.GetCachedValue(cacheKey, () =>
			{
				switch (agencyCode)
				{
					case ControllingAgencyList.Codes.FT:
						return Factory.GetCachedValue<CPT_112_FT_ResponseCodeList>();
					case ControllingAgencyList.Codes.CI:
					case ControllingAgencyList.Codes._20:
					case ControllingAgencyList.Codes._2Q:
						return Factory.GetCachedValue<CPT_112_CI_20_2Q_ResponseCodeList>();
					case ControllingAgencyList.Codes.DN:
						return Factory.GetCachedValue<CPT_112_IF_DN_ResponseCodeList>();
					case ControllingAgencyList.Codes.CD:
					case ControllingAgencyList.Codes.IF:
					case ControllingAgencyList.Codes.DH:
						return Factory.GetCachedValue<CPT_112_CD_ResponseCodeList>();
					case ControllingAgencyList.Codes.VP:
						return Factory.GetCachedValue<CPT_112_VP_ResponseCodeList>();
					case ControllingAgencyList.Codes.AX:
					case ControllingAgencyList.Codes.AG:
						return Factory.GetCachedValue<CPT_112_AX_ResponseCodeList>();
					default:
						return new CodeDescriptionPairList();
				}
			});
		}

		public CodeDescriptionPairList GetTypeOfApplicationCodeList(ZString messageType)
		{
			var cacheKey = $"TW.Business.MessageProcessors.MessageHelperLookups.TypeOfApplicationCodeList.{messageType}";
			return Factory.GetCachedValue(cacheKey, () =>
			{
				CodeDescriptionPairList typeOfApplicationCodeList;
				switch (messageType)
				{
					case ControllingMessageTypeList.Codes.NX301_DN:
					case ControllingMessageTypeList.Codes.NX401:
					case ControllingMessageTypeList.Codes.NX603:
					case MessageTypeList.Codes._602:
						typeOfApplicationCodeList = new CPT_111_BusinessTypeList();
						break;
					case ControllingMessageTypeList.Codes.NX101:
						typeOfApplicationCodeList = TypeOfApplicationCode_FromGlobalCodes;
						break;
					case ControllingMessageTypeList.Codes.NX201_01:
					case ControllingMessageTypeList.Codes.NX201_07:
						typeOfApplicationCodeList = new NX902_TypeOfApplicationCodeList();
						break;
					default:
						typeOfApplicationCodeList = new CodeDescriptionPairList();
						break;
				}
				return typeOfApplicationCodeList;
			});
		}

		public CodeDescriptionPairList CPT_118_602_CodeOfErrorConditionOverrideCodeList => Factory.GetCachedValue<CPT_118_602_CodeOfErrorConditionOverrideCodeList>();

		public CodeDescriptionPairList AppointmentPeriodList => Factory.GetCachedValue<CPT_113_AppointmentPeriodList>();

		public CodeDescriptionPairList CPT_118_302_CodeofErrorConditionOverrideCodeList => Factory.GetCachedValue<CPT_118_302_CodeofErrorConditionOverrideCodeList>();

		public CodeDescriptionPairList CPT_111_201_ApplicationType => Factory.GetCachedValue<CPT_111_201_ApplicationType>();

		public CodeDescriptionPairList CPT_118_302_AX_CodeofErrorConditionOverrideCodeList => Factory.GetCachedValue<CPT_118_302_AX_CodeofErrorConditionOverrideCodeList>();

		public CodeDescriptionPairList CPT_119_902_NotificationCodeList => Factory.GetCachedValue<CPT_119_902_NotificationCodeList>();

		public CodeDescriptionPairList NX903_CodeOfErrorOrIrregularityNoticeCodeList => Factory.GetCachedValue<NX903_CodeOfErrorOrIrregularityNoticeCodeList>();

		public CodeDescriptionPairList TypeOfApplicationCode_FromGlobalCodes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TaiwanCertificateOfOriginType, ZDateTime.Today);

		public CodeDescriptionPairList CPT_118_NX102_ErrorConditionOverrideCodeList => Factory.GetCachedValue<CPT_118_NX102_ErrorConditionOverrideCodeList>();

		public CodeDescriptionPairList NX302_DNBusinessTypeList => Factory.GetCachedValue<NX302_DNBusinessTypeList>();

		public CodeDescriptionPairList CPT_118_302_DN_CodeofErrorConditionOverrideCodeList => Factory.GetCachedValue<CPT_118_302_DN_CodeofErrorConditionOverrideCodeList>();

		public CodeDescriptionPairList NX302_DNAuditResultCodeList => Factory.GetCachedValue<NX302_DNAuditResultCodeList>();

		public CodeDescriptionPairList CPT_025_ExtraConditionCodeList => Factory.GetCachedValue<CPT_025_ExtraCondition>();

		public CodeDescriptionPairList CPT_121_CodeOfSpecialConditionCodeList => Factory.GetCachedValue<CPT_121_CodeOfSpecialCondition>();

		public CodeDescriptionPairList CPT_120_202_ResultCodeList => Factory.GetCachedValue<CPT_120_202_Result>();

		public CodeDescriptionPairList NX202TypeOfApplicationCodeList => Factory.GetCachedValue<NX202TypeOfApplicationList>();

		public CodeDescriptionPairList ModeofCustomsClearanceCodeList => Factory.GetCachedValue<ModeofCustomsClearanceCodeList>();

		public CodeDescriptionPairList ProcessCodeList => Factory.GetCachedValue<ProcessCodeList>();

		public CodeDescriptionPairList TypeofReleaseNoteCodeList => Factory.GetCachedValue<TypeofReleaseNoteCodeList>();

		public CodeDescriptionPairList UnabletoHandleContainerCodeList => Factory.GetCachedValue<UnabletoHandleContainerCodeList>();

		public CodeDescriptionPairList ResponseCodeList => Factory.GetCachedValue("TWResponseCodeList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(YesNoList.Codes.No, CommonDescriptions.DoNotAccept);
			list.AddPair(YesNoList.Codes.Yes, CommonDescriptions.Accepted);
			return list;
		});

		public CodeDescriptionPairList N5108_ErrorCodeList => Factory.GetCachedValue<N5108_ErrorCode>();

		public CodeDescriptionPairList FunctionCodeList => Factory.GetCachedValue("TWFunctionCodeList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(MessageFunctionCode.Delete, CommonDescriptions.Deleted);
			list.AddPair(MessageFunctionCode.Add, CommonDescriptions.Added);
			return list;
		});

		public CodeDescriptionPairList NX302AuditResultCoded => Factory.GetCachedValue<NX302NX602AuditResultCodeList>();

		public CodeDescriptionPairList RejectionReasonList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RejectionReason);

		public CodeDescriptionPairList RequiredFormalitiesList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.RequiredFormalities);

		public CodeDescriptionPairList ICIRefCusCodeList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection);

		public CodeDescriptionPairList PROURefCusCodeList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit);

		public CodeDescriptionPairList TWCARefCusCodeList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWControllingAgency);

		public CodeDescriptionPairList EntryTypeList => Factory.GetCachedValue<EntryTypeList>();

		public CodeDescriptionPairList InBondTransportModeList => Factory.GetCachedValue<InBondTransportModeCodes>();

		public CodeDescriptionPairList TranshipmentExaminationNoteList => Factory.GetCachedValue<TranshipmentExaminationNoteList>();

		public CodeDescriptionPairList TransportContractTypeList => Factory.GetCachedValue<TransportContractTypeList>();

		public CodeDescriptionPairList N5108ResponseCodeList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Taiwan, Codes.CustomsManifestStatus, ZDateTime.Today);

		public CodeDescriptionPairList CPT_104_QuarantineResultCodeList => Factory.GetCachedValue<CPT_104_QuarantineResultCodeList>();

		public CodeDescriptionPairList CPT_120_402_ResultCodeList => Factory.GetCachedValue<CPT_120_402_ResultCodeList>();

		public CodeDescriptionPairList TransportCodeList => Factory.GetCachedValue<TransportCodeList>();

		public static class CommonDescriptions
		{
			public static string DoNotAccept => Res.GetString("{7645EC10-DCC4-4069-AF2C-D8417ADEE06A}", "不受理");

			public static string Accepted => Res.GetString("{78C5A8BB-A95A-4C08-A829-31EE67B63806}", "受理");

			public static string Deleted => Res.GetString("{028A83A6-6300-4832-987C-4C1937D4E9C5}", "刪除");

			public static string Added => Res.GetString("{6A459132-3F1B-4862-8BFE-F7117F2458DF}", "新增");
		}
	}
}
