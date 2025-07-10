using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentInvoicingSupporter : JobInvoicingSupporter
	{
		public DtbConsignmentInvoicingSupporter(DtbConsignment consignment)
			: base(consignment)
		{
			Consignment = consignment;
		}

		protected readonly DtbConsignment Consignment;

		#region Packages

		public override ZString ContainerMode
		{
			get { return Constants.ContainerModes.LCL; }
		}

		public override ZDecimal ActualVolume
		{
			get { return Consignment.IsLooseOnly ? Consignment.GetTotalLoosePackageVolume().Amount : Consignment.GetTotalContainerisedVolume().Amount; }
		}

		public override ZString ActualVolumeUnit
		{
			get { return Consignment.IsLooseOnly ? Consignment.GetTotalLoosePackageVolume().Unit : Consignment.GetTotalContainerisedVolume().Unit; }
		}

		public override ZDecimal ActualWeight
		{
			get { return Consignment.IsLooseOnly ? Consignment.GetTotalLoosePackageWeight().Amount : Consignment.GetTotalContainerisedWeight().Amount; }
		}

		public override ZString ActualWeightUnit
		{
			get { return Consignment.IsLooseOnly ? Consignment.GetTotalLoosePackageWeight().Unit : Consignment.GetTotalContainerisedWeight().Unit; }
		}

		public override int ContainerCount
		{
			get { return 0; }
		}

		public override ZDecimal TEUCount
		{
			get { return 0m; }
		}

		#endregion

		#region ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.TransportConsignment; }
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.DtbConsignmentJobAuditBilling;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.DtbConsignmentJobInvoicing;
		}

		#endregion

		#region Consol Costing

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
		{
			return !Consignment.IsCancelled;
		}

		#endregion

		#region Other

		public override ZString ServiceLevel => Consignment.LTC_RS_NKServiceLevel;

		public override ZString OperationalJobRef
		{
			get { return Consignment.LTC_JobID; }
		}

		#region ConsolType

		public override ZString ConsolType
		{
			get { return Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol; }
		}

		#endregion

		#region CreateAccountingJobOnSavingOfOperationsJob

		public override bool CreateAccountingJobOnSavingOfOperationsJob
		{
			get { return !Consignment.IsInDatabaseIncludingChildren; }
		}

		#endregion

		#region OperationsBranch

		public override GlbBranch OperationsBranch
		{
			get { return GlbBranch.CurrentBranch; }
		}

		#endregion

		#region TransportMode

		public override ZString TransportMode
		{
			get { return Constants.TransportModes.Road; }
		}

		#endregion

		#region Consignor

		public override OrgHeader Consignor
		{
			get { return null; }
		}

		#endregion

		#region	OverriddenDefaultLocalClient

		public override OrgHeader OverriddenDefaultLocalClient
		{
			get { return null; }
		}

		#endregion

		#endregion

		public override ZDateTime GetOperationsSignificantDateByDirection(string significantDateCode, string direction)
		{
			return GetOperationsSignificantDate(significantDateCode);
		}

		public override ZDateTime GetOperationsSignificantDate(string significantDateCode)
		{
			ZDateTime result = ZDateTime.Empty;
			switch (significantDateCode)
			{
				case JobDateTypes.Codes.PickupDate:

					var revPickupEvent = Events.PickedUp;
					var pickupEventLog = Consignment.Logs.MostRecentLogByEventTime(revPickupEvent, new ZQuery(StmALogSchema.SL_IsEstimate, false))
													?? Consignment.Logs.MostRecentLogByEventTime(revPickupEvent, new ZQuery(StmALogSchema.SL_IsEstimate, true));
					if (pickupEventLog != null)
					{
						result = pickupEventLog.SL_EventTime;
					}
					break;
				case JobDateTypes.Codes.DeliveryDate:

					var revDeliveryEvent = Events.Delivered;
					var deliveredEventLog = Consignment.Logs.MostRecentLogByEventTime(revDeliveryEvent, new ZQuery(StmALogSchema.SL_IsEstimate, false))
													?? Consignment.Logs.MostRecentLogByEventTime(revDeliveryEvent, new ZQuery(StmALogSchema.SL_IsEstimate, true));
					if (deliveredEventLog != null)
					{
						result = deliveredEventLog.SL_EventTime;
					}
					break;
			}

			return result;
		}
	}
}
