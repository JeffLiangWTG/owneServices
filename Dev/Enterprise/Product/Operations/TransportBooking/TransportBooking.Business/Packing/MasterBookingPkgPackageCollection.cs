using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.Business
{
	public class MasterBookingPkgPackageCollection : PkgPackageCollection
	{
		public MasterBookingPkgPackageCollection(PkgPackageJob master, bool topLevelOnly = true) : base(master.Factory)
		{
			AddFilterForSubPackages(master, topLevelOnly);
		}

		void AddFilterForSubPackages(PkgPackageJob master, bool topLevelOnly = true)
		{
			var packageQuery = new ZDBOnlyQuery(typeof(PkgPackage));

			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.PK);

			var masterBookingConsolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingSchema.KM_KB_Booking);
			masterBookingConsolidationSubQuery.AddToFilter(DtbBookingConsolidationSchema.PK, master.KJ_ParentID);

			var subBookingConsolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingConsolidation), DtbBookingConsolidationSchema.PK);
			foreach (var subConsolPKToExclude in ((DtbBookingConsolidationPkgPackageJob)master).SubConsolidationPKsToExclude)
			{
				subBookingConsolidationSubQuery.AddToFilter(DtbBookingConsolidationSchema.PK, SQLComparisonOperator.NotEqual, subConsolPKToExclude);
			}

			packageJobSubQuery.AddSubQuery(PkgPackageJobSchema.KJ_ParentID, subBookingConsolidationSubQuery, JoinCondition.And);
			packageQuery.AddSubQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobSubQuery, JoinCondition.And);

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(DtbBooking), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			bookingSubQuery.AddSubQuery(masterBookingConsolidationSubQuery, JoinCondition.And);

			bookingSubQuery.AddToFilter(JoinCondition.Or, DtbBookingSchema.PK, ((DtbBookingConsolidationPkgPackageJob)master).SubBookingPKsToInclude);

			var bookingInstructionSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingInstruction), DtbBookingInstructionPkgDivotSchema.KD_KN_BookingInstruction);
			bookingInstructionSubQuery.AddSubQuery(bookingSubQuery, JoinCondition.And);

			var packageDivotSubQuery = new ZDBOnlySubQuery(typeof(DtbBookingInstructionPkgDivot), DtbBookingInstructionPkgDivotSchema.KD_KP_Package);
			packageDivotSubQuery.AddSubQuery(bookingInstructionSubQuery, JoinCondition.And);

			if (topLevelOnly)
			{
				packageQuery.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, SQLComparisonOperator.Equal, null);
			}

			packageQuery.AddSubQuery(packageDivotSubQuery, JoinCondition.And);
			AdditionalFilter.AddToFilter(packageQuery);
		}

		protected override void OnLoadedIntoCollectionCore(PkgPackage loadedObject)
		{
			loadedObject.SetReadOnlyIncludingChildren(true);
			base.OnLoadedIntoCollectionCore(loadedObject);
		}
	}
}
