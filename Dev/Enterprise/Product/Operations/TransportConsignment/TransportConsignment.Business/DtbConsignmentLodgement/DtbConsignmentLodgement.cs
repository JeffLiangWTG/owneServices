using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLodgement : AutoDtbConsignmentLodgement
	{
		public DtbConsignmentLodgement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LCL_Status = DtbConsignmentLodgementStatuses.Codes.Success;
			LCL_MovementType = DtbConsignmentLodgementMovementTypes.Codes.Outbound;
			LCL_Type = DtbConsignmentLodgementTypes.Codes.Booking;
		}

		#endregion
	}
}
