using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		public DtbConsignmentRateLineConditionsSupporter(DtbConsignment consignment) : base(consignment)
		{
		}

		#region Related Objects

		DtbConsignment Consignment
		{
			get { return consignment ?? (consignment = ObjectToWrap as DtbConsignment); }
		}

		DtbConsignment consignment;

		#endregion

		#region GetExportBroker

		protected override OrgHeader GetExportBroker()
		{
			return null;
		}

		#endregion

		#region GetImportBroker

		protected override OrgHeader GetImportBroker()
		{
			return null;
		}

		#endregion

		#region GetSendingAgent

		protected override OrgHeader GetSendingAgent()
		{
			return null;
		}

		#endregion

		#region GetReceivingAgent

		protected override OrgHeader GetReceivingAgent()
		{
			return null;
		}

		#endregion

		#region GetControllingAgent

		protected override OrgHeader GetControllingAgent()
		{
			return null;
		}

		#endregion

		#region GetDepartureCFS

		protected override OrgHeader GetDepartureCFS()
		{
			return null;
		}

		#endregion

		#region GetArrivalCFS

		protected override OrgHeader GetArrivalCFS()
		{
			return null;
		}

		#endregion

		#region GetHasDangerousGoods

		protected override bool GetHasDangerousGoods()
		{
			return Consignment != null && Consignment.LTC_IsHazardous;
		}

		#endregion
	}
}
