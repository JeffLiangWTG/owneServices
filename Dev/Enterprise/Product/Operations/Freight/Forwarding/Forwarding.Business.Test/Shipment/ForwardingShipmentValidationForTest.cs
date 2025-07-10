using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentValidationForTest : ForwardingShipmentValidation
	{
		public ForwardingShipmentValidationForTest(ForwardingShipment parent) : base(parent)
		{
		}

		internal bool ShouldValidateFKToCancelledRecordForTest(ZPropertyInfo info) => base.ShouldValidateFKToCancelledRecord(info);
	}
}
