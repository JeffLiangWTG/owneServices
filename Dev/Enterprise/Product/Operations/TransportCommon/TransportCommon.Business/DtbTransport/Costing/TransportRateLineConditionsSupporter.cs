using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportCommon.Business
{
	public abstract class TransportRateLineConditionsSupporter : RateLineConditionsSupporter
	{
		protected TransportRateLineConditionsSupporter(DtbTransport transport)
			: base(transport)
		{
		}

		DtbTransport transport;
		DtbTransport Transport
		{
			get { return transport ?? (transport = ObjectToWrap as DtbTransport); }
		}

		protected override OrgHeader GetExportBroker()
		{
			return null;
		}

		protected override OrgHeader GetImportBroker()
		{
			return null;
		}

		protected override OrgHeader GetSendingAgent()
		{
			return null;
		}

		protected override OrgHeader GetReceivingAgent()
		{
			return null;
		}

		protected override OrgHeader GetControllingAgent()
		{
			return null;
		}

		protected override OrgHeader GetDepartureCFS()
		{
			return null;
		}

		protected override OrgHeader GetArrivalCFS()
		{
			return null;
		}

		protected override bool GetHasDangerousGoods()
		{
			return Transport != null && Transport.KM_IsHazardous;
		}
	}
}
