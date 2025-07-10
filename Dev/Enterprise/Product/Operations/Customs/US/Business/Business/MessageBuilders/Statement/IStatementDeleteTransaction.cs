using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IStatementDeleteTransaction
	{
		ZString ProcessingPort { get; }
		bool ShouldPopulatePreparerSite { get; }
		bool IsACE { get; }
		ZString PreparerPort { get; }
		ZString PreparerOfficeCode { get; }
		ZString PortOfEntry { get; }
		ZString EntryFilerCode { get; }
		ZString EntryNumber { get; }
		ZString PaymentType { get; }
		ZDateTime PreliminaryStatementPrintDate { get; }
		ZDateTime ReleaseDate { get; }
		ZString ClientBranchDesignation { get; }
		ZString PeriodicStatementMonth { get; }
		BusinessObjectFactory Factory { get; }
		GlbBranch Branch { get; }
		bool ShouldGenerateACEStatementMessage { get; }
		bool IsStatementUpdateMessagePending { get; }
		Guid RegistryCompanyPK { get; }

		void AddMessages(MQEDIMessage message);
	}
}
