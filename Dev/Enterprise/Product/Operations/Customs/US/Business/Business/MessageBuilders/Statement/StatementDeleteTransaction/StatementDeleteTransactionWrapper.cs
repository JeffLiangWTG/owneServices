using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class StatementDeleteTransactionWrapper : IStatementDeleteTransaction
	{
		public StatementDeleteTransactionWrapper(IStatementDeleteTransaction entity)
		{
			this.entity = entity;
		}

		readonly IStatementDeleteTransaction entity;

		public ZString ProcessingOfficeCode
		{
			get { return entity.PreparerOfficeCode; }
		}

		public ZString ProcessingPort
		{
			get { return entity.ProcessingPort; }
		}

		public ZString PortOfEntry
		{
			get { return entity.PortOfEntry; }
		}

		public ZString EntryFilerCode
		{
			get { return entity.EntryFilerCode; }
		}

		public ZString EntryNumber
		{
			get { return entity.EntryNumber; }
		}

		public GlbBranch Branch
		{
			get { return entity.Branch; }
		}

		public BusinessObjectFactory Factory
		{
			get { return entity.Factory; }
		}

		public bool ShouldPopulatePreparerSite
		{
			get { return entity.ShouldPopulatePreparerSite; }
		}

		public ZString PreparerPort
		{
			get { return entity.PreparerPort; }
		}

		public ZString PreparerOfficeCode
		{
			get { return ProcessingOfficeCode; }
		}

		public void AddMessages(MQEDIMessage message)
		{
			entity.AddMessages(message);
		}

		public bool IsACE
		{
			get { return entity.IsACE; }
		}

		public ZDateTime ReleaseDate
		{
			get { return entity.ReleaseDate; }
		}

		public bool ShouldGenerateACEStatementMessage
		{
			get { return entity.ShouldGenerateACEStatementMessage; }
		}

		public bool IsStatementUpdateMessagePending
		{
			get { return entity.IsStatementUpdateMessagePending; }
		}

		public Guid RegistryCompanyPK
		{
			get { return entity.RegistryCompanyPK; }
		}

		//*********************************************************
		//These properties should be set externally
		//Should not return a current entry's value as the message is about changing these values
		public ZString PaymentType { get; set; }
		public ZDateTime PreliminaryStatementPrintDate { get; set; }
		public ZString PeriodicStatementMonth { get; set; }
		public ZString ClientBranchDesignation { get; set; }
		//*********************************************************
	}
}
