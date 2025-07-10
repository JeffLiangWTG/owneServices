using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public DtbBookingFetchStrategy(DtbBooking booking)
			: base(booking)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			Factory.AddFetchHint(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, Booking.PK);
			Factory.AddFetchHint(typeof(DtbBookingConsolidation), Booking.KM_KB_Booking); // need booking consolidation for loading package job
			Factory.AddFetchHint(DtbBookingConsolidationSchema.KB_ParentID, Booking.PK); // Consignment - shallow for readonly
			Factory.AddFetchHint(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID, Booking.KM_KB_Booking);

			var requireClient = false;
			foreach (var column in columns)
			{
				if (column.ColumnName.Equals("LocalClient"))
				{
					requireClient = true;
					break;
				}
			}

			var requireAddress = false;
			foreach (var column in columns)
			{
				if (column.ColumnName.Contains("Address+E2_OA_Address") ||
					column.ColumnName.Contains("Address+E2_CompanyName") ||
					column.ColumnName.Contains("Address+E2_City") ||
					column.ColumnName.Contains("Address+E2_Postcode") ||
					column.ColumnName.Contains("Address+E2_State") ||
					column.ColumnName.Contains("Address+OrganisationNameOrPK"))
				{
					requireAddress = true;
					break;
				}
			}

			if (requireClient)
			{
				ZQuery query = new ZQuery(JobHeaderSchema.JH_ParentID, Booking.PK);
				query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				Factory.AddFetchHint(typeof(JobHeader), query);
			}

			if (requireAddress)
			{
				Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, Booking.PK);
			}

			if (Booking.Instructions != null)
			{
				foreach (var instruction in Booking.Instructions)
				{
					Factory.AddFetchHint(typeof(DtbBookingConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction, instruction.PK);
					Factory.AddFetchHint(typeof(DtbBookingInstructionPkgDivot), DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction, instruction.PK);
					Factory.AddFetchHint(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, instruction.PK);
				}
			}
		}

		DtbBooking Booking
		{
			get { return (DtbBooking)BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(DtbBookingInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, BusinessObject.PK);
		}
	}
}
