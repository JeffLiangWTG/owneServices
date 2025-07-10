using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentInvoicingSupporter : DtbTransportInvoicingSupporter
	{
		public DtbBookingConsignmentInvoicingSupporter(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		#region ConsumerType

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.TransportBookingConsignment; }
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint GetAuditSecurityCore()
		{
			return Env.Security.None;
		}

		protected override SecurityCheckpoint GetJobInvoicingSecurityCore()
		{
			return Env.Security.None;
		}

		#endregion

		#region Consol Costing

		protected override bool IncludeInConsolCostingCore(bool includeRelatedShipments)
		{
			return !Transport.IsCancelled;
		}

		#endregion

		#region OperationsBranch

		protected override bool UseRegistryFallbackForBranch
		{
			get { return true; }
		}

		#endregion

		#region Other

		public override ZString ServiceLevel => Consignment.KM_RS_NKServiceLevel;

		public override ZString OperationalJobRef
		{
			get { return Transport.KM_JobID; }
		}

		public override OrgHeader GetDefaultCreditor(DefaultCreditorSetting defaultCreditorSetting)
		{
			return GlbCompany.CurrentCompany.OrgProxy;
		}

		#endregion

		#region Consignment

		DtbBookingConsignment Consignment
		{
			get { return (DtbBookingConsignment)Transport; }
		}

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
