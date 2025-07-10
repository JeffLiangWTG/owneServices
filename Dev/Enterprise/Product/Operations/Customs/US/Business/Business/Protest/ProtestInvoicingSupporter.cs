using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Protest
{
	public class ProtestInvoicingSupporter : JobInvoicingSupporter, IServiceDirection
	{
		public ProtestInvoicingSupporter(Protest parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		protected readonly Protest Parent;

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return ((IJobInvoicingPlugIn)Parent.Declaration).InvoicingSupporter.AuditSecurity;
		}

		public override ZString ConsolType
		{
			get { return Core.Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Brokerage; }
		}

		protected override SecurityCheckpoint GetEditSecurityCheckpointCore()
		{
			return ((IJobInvoicingPlugIn)Parent.Declaration).InvoicingSupporter.EditSecurityCheckpoint;
		}

		public override bool EditSecurityLock
		{
			get { return ((IJobInvoicingPlugIn)Parent.Declaration).InvoicingSupporter.EditSecurityLock; }
		}

		public override ZString EditSecurityMessage
		{
			get { return ((IJobInvoicingPlugIn)Parent.Declaration).InvoicingSupporter.EditSecurityMessage; }
		}

		public override bool IsImport
		{
			get { return true; }
		}

		public override OrgHeader Consignee
		{
			get { return (Parent.Protestant == null) ? null : Parent.Protestant.Organisation; }
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return ((IJobInvoicingPlugIn)Parent.Declaration).InvoicingSupporter.JobInvoicingSecurity;
		}

		public override ZGuid OverriddenDepartmentPK
		{
			get { return ((IJobInvoicingPlugIn)Parent.Declaration).InvoicingSupporter.OverriddenDepartmentPK; }
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			var result = ZDateTime.Empty;

			if (significantDateCode == AccountingMasterFilesConstants.SignificantDateCodes.CustomsClearanceDate)
			{
				result = GetCustomsClearanceDate();
			}

			return result;
		}

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return GetOperationsSignificantDate(significantDateCode);
		}

		public override ZDateTime GetCustomsClearanceDate()
		{
			var result = ZDateTime.Empty;
			var mostRecentClearedLog = Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared);

			if (mostRecentClearedLog != null)
			{
				result = mostRecentClearedLog.SL_EventTime;
			}

			return result;
		}

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Parent.IsInDatabaseIncludingChildren; }
		}

		public ZString ServiceDirection
		{
			get
			{
				return Parent.Declaration.JE_MessageType;
			}
		}
	}
}
