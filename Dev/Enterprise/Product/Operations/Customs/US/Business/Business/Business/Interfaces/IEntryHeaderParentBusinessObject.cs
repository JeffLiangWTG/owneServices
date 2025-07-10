using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IEntryHeaderParentBusinessObject : IMessageAttachee
	{
		BusinessObject LinkedObject { get; }
		CusEntryHeader EntrySummaryEntry { get; }
		CusEntryHeader CargoReleaseEntry { get; }
		ISimplifiedMessageLinkedObject SimplifiedEntry { get; }
		ZString ReferenceNumber { get; }
		Guid RegistryCompanyPK { get; }
		Guid RegistryBranchPK { get; }
		GlbStaff CusAgent { get; }
		OrgHeader Importer { get; }
		DispositionDataCollection DispositionCodes { get; }
		IDisposable ReleaseStatusChangingSuspender { get; }
		ZString ReleaseStatus { get; set; }
		ZDateTime ReleaseDateTime { get; set; }
		bool ShouldUpdateDeclarationWithCargoReleaseResults { get; }

		ErrorsRecordCollection ENSStatusNotifications { get; }

		void UpdateQuotaStatus(ZString quotaStatus);
		void MarkAIIRequested();
		void UpdateMessageLinkedParentBOAfterReleased(ASESSO10Base blockSO10, IEnumerable<ASESSO40Base> blockSO40List, IEnumerable<ASESSO50Base> blockSO50List);
		void AddOrUpdateCusDisposition(Dictionary<ZString, IPGADispositionProvider> pgaEntryStatusMapping, List<IPGADispositionProvider> pgaLineStatusList);
		void LogPGAEntryAndLineStatus(Dictionary<ZString, ZString> previousPGAEntryStatus, List<IPGADispositionProvider> dispositionProviders, ZString messageType);

		Bill GetFirstBillHasSameNumber(ZString billNumber);
		ZString GetUnableToDeactivateStatementLineRemarkIfNecessary();
		Dictionary<ZString, ZString> GetPGAEntryStatus();
		OGADispositionData AddOGADispositionData(IPGADispositionProvider dispositionProvider, ZString source);
	}
}
