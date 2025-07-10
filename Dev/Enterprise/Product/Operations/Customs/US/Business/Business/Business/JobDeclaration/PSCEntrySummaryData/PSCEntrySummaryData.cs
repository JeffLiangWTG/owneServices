using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;

namespace Enterprise.Customs.US.Business
{
	public class PSCEntrySummaryData
	{
		public PSCEntrySummaryData(MQEDIMessage latest7501Accepted, MQEDIMessage latestSTUAccepted)
		{
			this.message = latest7501Accepted;
			this.latestSTUAccepted = latestSTUAccepted;
			PopulateHeaderDataFromMessage();
		}
		readonly MQEDIMessage message;
		readonly MQEDIMessage latestSTUAccepted;

		public bool IsValid
		{
			get { return message != null; }
		}

		public ZString EntryType
		{
			get;
			private set;
		}

		public ZString ReconType
		{
			get;
			private set;
		}

		public ZBool FTARecon
		{
			get;
			private set;
		}

		public ZString IORNumber
		{
			get;
			private set;
		}

		public ZString EntryPort
		{
			get;
			private set;
		}

		public ZString PaymentType
		{
			get;
			private set;
		}

		public ZDate PSD
		{
			get;
			private set;
		}

		public ZString PSCMonth
		{
			get;
			private set;
		}

		public ZString ClientBranchDesig
		{
			get;
			private set;
		}

		public ZString GoodsLocation
		{
			get;
			private set;
		}

		public ZBool Consolidated
		{
			get;
			private set;
		}

		public ZBool LiveEntry
		{
			get;
			private set;
		}

		void PopulateHeaderDataFromMessage()
		{
			if (message != null)
			{
				var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
				if (ens10 != null)
				{
					EntryType = ens10.EntryTypeCode;
					ReconType = ens10.ReconciliationIssueCode;
					FTARecon = ens10.TradeAgreementReconciliationIndicator == "Y";
					EntryPort = ens10.DistrictPortOfEntry;
					PaymentType = ens10.PaymentTypeCode;
					PSD = ens10.PreliminaryStatementPrintDate;
					PSCMonth = ens10.PeriodicStatementMonth;
					ClientBranchDesig = ens10.StatementClientBranchIdentifier;
					Consolidated = ens10.ConsolidatedSummaryIndicator == "Y";
					LiveEntry = ens10.LiveEntryIndicator == "Y";
				}

				var ens11 = message.MessageBlock.MessageBlocks.OfType<AENS11>().FirstOrDefault();
				if (ens11 != null)
				{
					IORNumber = ens11.ImporterOfRecordNumber;
				}

				var ens20 = message.MessageBlock.MessageBlocks.OfType<AENS20>().FirstOrDefault();
				if (ens20 != null)
				{
					GoodsLocation = ens20.LocationOfGoodsCode;
				}
			}

			if (latestSTUAccepted != null)
			{
				var hBlock = latestSTUAccepted.MessageBlock.MessageBlocks.OfType<IStatementUpdateInputHBlock>().FirstOrDefault();

				if (hBlock != null)
				{
					PaymentType = hBlock.PaymentTypeIndicator;
					PSD = hBlock.PreliminaryStatementPrintDate;
					PSCMonth = hBlock.PeriodicStatementMonth;
					ClientBranchDesig = hBlock.ClientBranchDesignation;
				}
			}
		}
	}
}
