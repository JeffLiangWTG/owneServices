using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.ContainerYard.Business
{
	public class GateBookingProcessTaskCollection : ProcessTaskCollection
	{
		public GateBookingProcessTaskCollection(GateBooking container)
		: base(container)
		{
		}

		public new GateBooking Parent => (GateBooking)base.Parent;

		public new GateBookingProcessTask this[int index] => (GateBookingProcessTask)Elements[index];

		public new GateBookingProcessTask AddNew() => (GateBookingProcessTask)base.AddNew();

		public override ProcessTaskCollection CreateNewCollection() => new GateBookingProcessTaskCollection(Parent);

		public override bool IsCondition1Met(ZString conditionCode)
		{
			var gateBookingDetail = Parent.GateBookingDetails.FirstOrDefault();
			if (gateBookingDetail is null)
			{
				return false;
			}

			switch (conditionCode)
			{
				case GateBookingWorkflowCondition1CodeList.Codes.Cargo:
					return !gateBookingDetail.GTD_IsContainer;

				case GateBookingWorkflowCondition1CodeList.Codes.FullContainer:
					return gateBookingDetail.GTD_IsContainer
						&& (!gateBookingDetail.YardUnit?.GTY_IsEmpty ?? false);

				case GateBookingWorkflowCondition1CodeList.Codes.EmptyContainer:
					return gateBookingDetail.GTD_IsContainer
						&& (gateBookingDetail.YardUnit?.GTY_IsEmpty ?? false);

				default:
					return false;
			}
		}
	}
}
