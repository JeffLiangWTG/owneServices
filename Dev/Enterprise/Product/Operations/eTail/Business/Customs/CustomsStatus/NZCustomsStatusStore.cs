using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.NZ;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;
using AmalgamatedStatusList = Enterprise.Customs.Common.NZ.AmalgamatedStatusList;

namespace Enterprise.eTail.Business
{
	class NZCustomsStatusStore : DefaultCustomsStatusStore
	{
		public NZCustomsStatusStore(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override IEnumerable<CustomsStatusInfo> GetCustomsStatusListForCodeType(string codeType)
		{
			return codeType == RefCusCodeListTypes.Codes.CustomsStatusForInterface
				? FormalEntryStatusList
				: base.GetCustomsStatusListForCodeType(RefCusCodeListTypes.Codes.CustomsStatus);
		}

		protected override bool HasFormalDeclaration(BaseJobDeclaration standaloneDeclaration)
		{
			return base.HasFormalDeclaration(standaloneDeclaration) &&
				   standaloneDeclaration.JE_MessageSubType != NZJobMessageTypeList.Codes.WriteOff;
		}

		protected override ZString CountryCode => CountryCodes.NewZealand;

		internal static CustomsStatusInfo[] FormalEntryStatusList => new[]
		{
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.AdjustmentAcceptedFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.AdjustmentAcceptedFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.AgencyResponsePending, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.AgencyResponsePending, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.CreditAdvice, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.CreditAdvice, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.DeliveryOnPaymentFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.DeliveryOnPaymentFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.DeliveryOrderReceivedFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.DeliveryOrderReceivedFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.DOSentToRecipient, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.DOSentToRecipient, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.Cancelled, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.Cancelled, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.EntryCleared, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.EntryCleared, HVLVReleaseStatus.Cleared),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.InError, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.InError, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.EntryRejectedFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.EntryRejectedFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.RestoredFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.RestoredFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.InspectionsAuditRequirementsFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.InspectionsAuditRequirementsFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.ManualEntryCannotBeSentToCustomsFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.ManualEntryCannotBeSentToCustomsFormalOnly, HVLVReleaseStatus.None),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.NotSentToCustoms, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.NotSentToCustoms, HVLVReleaseStatus.None),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.QueuedForSendingFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.QueuedForSendingFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.ResponseReceivedFormalOnly, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.ResponseReceivedFormalOnly, HVLVReleaseStatus.Held),
			new CustomsStatusInfo(AmalgamatedStatusList.Codes.SentToCustoms, RefCusCodeListTypes.Codes.CustomsStatusForInterface, AmalgamatedStatusList.Descriptions.SentToCustoms, HVLVReleaseStatus.Held),
		};
	}
}
