using System;
using CargoWise.Application;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class ShipmentConsumerType : BaseShipmentConsumerType
	{
		public ShipmentConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.JobShipment; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>(); }
		}

		public override bool OverseasAgentApplicable
		{
			get { return true; }
		}

		public override bool ShouldDisplayClientContractNumber(IJobInvoicingPlugIn host)
		{
			var result = true;
			if (host.InvoicingSupporter is IJobInvoicingSupporterWithChargeableFactorSource hostWithChargeableFactorSource)
			{
				result = result && (hostWithChargeableFactorSource.ChargeableFactorSource != ChargeableFactorSource.TransportBooking);
			}

			result = result && IsSupportInvoicingPlugIn(host);

			return result;
		}
	}
}
