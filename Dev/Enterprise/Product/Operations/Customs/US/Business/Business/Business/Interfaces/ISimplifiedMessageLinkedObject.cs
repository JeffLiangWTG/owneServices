using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business
{
	public interface ISimplifiedMessageLinkedObject : IMessageAttachee, IControllerIDProvider
	{
		BusinessObject LinkedObject { get; }
		IEntryHeaderParentBusinessObject ParentBusinessObject { get; }
		ZBool IsCargoReleaseBeingCertified { get; }
		bool IsFormalEntry { get; }
		bool IsBorderCargoRelease { get; }
		bool IsCargoRelease { get; }
		bool IsACECargoRelease { get; }
		bool IsLVSCargoRelease { get; }
		bool UseCodeIsHVL { get; }
		ZString CargoReleaseCertifiedStatus { get; set; }
		ZString EntryFilerCode { get; }

		void ClearCRLCertStatusFromAllHeaders();
		void DeactiveStatementLineIfRequired(ZString[] clearDeletedStatus, ZString messageStatus);
		void AutoSendEntrySummaryQueryIfEligible();
		void UpdateACEFDALineInfoIfRequired(OGADispositionData processingOGADispositionData);
		void UpdateFWSLineInfoIfRequired(OGADispositionData processingOGADispositionData);
		KeyValuePair<string, string>[] GetMSCEventParameters();

		ZString GetMSCEventReferenceForCargoReleaseResponse(CBPEDIMessage message);
	}
}
