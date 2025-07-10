using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbBookingConsignmentFetchStrategy : DtbTransportFetchStrategy<DtbConsignmentInstruction>
	{
		public DtbBookingConsignmentFetchStrategy(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			ZQuery query = new ZQuery(JobHeaderSchema.JH_ParentID, Consignment.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			Factory.AddFetchHint(typeof(JobHeader), query);

			// the route planner starts from confirmations --> instructions --> consignment --> booking (for the booking ID)
			Factory.AddFetchHint(typeof(DtbConsignmentConsolidation), Consignment.KM_KB_Booking);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (Consignment.Instructions != null)
			{
				foreach (var instruction in Consignment.Instructions)
				{
					Factory.AddFetchHint(DtbBookingConfirmationSchema.KK_KN_BookingInstruction, instruction.PK);
					Factory.AddFetchHint(DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instruction.PK);
					Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, instruction.PK);
				}
			}
		}

		DtbBookingConsignment Consignment
		{
			get { return (DtbBookingConsignment)BusinessObject; }
		}
	}
}
