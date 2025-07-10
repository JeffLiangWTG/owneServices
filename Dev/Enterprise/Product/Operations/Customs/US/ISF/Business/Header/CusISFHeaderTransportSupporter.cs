using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Security;

namespace Enterprise.Customs.US.ISF.Business
{
	public sealed class CusISFHeaderTransportSupporter : TransportSupporter<CusISFHeader>
	{
		public CusISFHeaderTransportSupporter(CusISFHeader parent)
			: base(parent) { }

		public override ZString BillOfLading
		{
			get { return Parent.BF_OceanBill.IsEmpty ? Parent.BF_HouseBill : Parent.BF_OceanBill; }
		}
		public override ZString ConsignmentRef
		{
			get { return Parent.BF_JobReference; }
		}
		public override ZString ContainerMode
		{
			get { return ZString.Empty; }
		}
		public override ZString Description
		{
			get { return Parent.BF_JobReference; }
		}
		public override ZGuid ShippingLine
		{
			get { return ZGuid.Empty; }
			set { }
		}
		public override ZString TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		public override void SetConsignmentRefIfNotSet()
		{
			base.SetConsignmentRefIfNotSet();
			Parent.PopulateJobReferenceIfNeeded();
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceCustoms; }
		}
	}
}
