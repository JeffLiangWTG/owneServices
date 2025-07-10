using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;

namespace Enterprise.Customs.Business
{
	public class BaseJobComInvoiceHeaderTransportSupporter : TransportSupporter<BaseJobComInvoiceHeader>
	{
		public BaseJobComInvoiceHeaderTransportSupporter(BaseJobComInvoiceHeader parent)
			: base(parent) { }

		public override ZString BillOfLading
		{
			get { return ZString.Empty; }
		}

		public override ZString ConsignmentRef
		{
			get { return Parent.JZ_InvoiceNumber; }
		}

		public override ZString ContainerMode
		{
			get { return ZString.Empty; }
		}

		public override ZString Description
		{
			get { return Parent.JZ_InvoiceNumber; }
		}

		public override ZGuid ShippingLine
		{
			get { return ZGuid.Empty; }
			set { }
		}

		public override ZString TransportMode
		{
			get { return ZString.Empty; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}
