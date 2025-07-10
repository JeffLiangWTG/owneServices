//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDtbConsignmentRunSheetValidation
//
//    This class should be used for overriding validation in AutoDtbConsignmentRunSheetValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentRunSheetValidation : AutoDtbConsignmentRunSheetValidation
	{
		public DtbConsignmentRunSheetValidation(AutoDtbConsignmentRunSheet parent)
			: base(parent)
		{
		}

		#region KG_EndTime

		protected override void CheckKG_EndTime()
		{
			base.CheckKG_EndTime();

			MandatoryValidation.CheckEntered(Parent.KG_EndTimeInfo);
			if (!Parent.KG_EndTimeInfo.HasErrors())
			{
				TransportDateRangeValidation.ErrorOnEndIfBeforeStart((ZPropertyInfo<ZDateTimeOffset>)Parent.KG_StartTimeInfo, (ZPropertyInfo<ZDateTimeOffset>)Parent.KG_EndTimeInfo);
			}
		}

		#endregion

		#region KG_GS_NKTruckDriver

		protected override void CheckKG_GS_NKTruckDriver()
		{
			base.CheckKG_GS_NKTruckDriver();
			ListValidation.ErrorIfInvalidCode(Parent.KG_GS_NKTruckDriverInfo);

			if (!Parent.KG_GS_NKTruckDriverInfo.HasErrors() && Parent.TruckDriver != null && Parent.KG_StartTime.IsValid && Parent.KG_EndTime.IsValid)
			{
				CheckOverlap(DtbConsignmentRunSheetSchema.KG_GS_NKTruckDriver, Parent.KG_GS_NKTruckDriver, Parent.TruckDriver.GS_FullName, Parent.KG_GS_NKTruckDriverInfo);
			}
		}

		#endregion

		#region KG_OH_TransportCo

		protected override void CheckKG_OH_TransportCo()
		{
			base.CheckKG_OH_TransportCo();
			ListValidation.ErrorIfInvalidPK(Parent.KG_OH_TransportCoInfo);

			if (!Parent.KG_OH_TransportCoInfo.HasErrors() && Parent.TransportCo != null && Parent.KG_StartTime.IsValid && Parent.KG_EndTime.IsValid)
			{
				CheckTransportCompanyOverlap(Parent.KG_OH_TransportCoInfo);
			}
		}

		#endregion

		#region KG_RQ_Truck

		protected override void CheckKG_RQ_Truck()
		{
			base.CheckKG_RQ_Truck();
			ListValidation.ErrorIfInvalidPK(Parent.KG_RQ_TruckInfo);

			if (!Parent.KG_RQ_TruckInfo.HasErrors() && Parent.Truck != null && Parent.KG_StartTime.IsValid && Parent.KG_EndTime.IsValid)
			{
				CheckOverlap(DtbConsignmentRunSheetSchema.KG_RQ_Truck, Parent.KG_RQ_Truck, Parent.Truck.RQ_DescriptionMultilingual, Parent.KG_RQ_TruckInfo);
			}
		}

		#endregion

		#region KG_AdHocDriversName

		protected override void CheckKG_AdHocDriversName()
		{
			base.CheckKG_AdHocDriversName();

			if (!Parent.KG_AdHocDriversNameInfo.HasErrors() && Parent.TransportCo != null && Parent.KG_StartTime.IsValid && Parent.KG_EndTime.IsValid)
			{
				CheckTransportCompanyOverlap(Parent.KG_AdHocDriversNameInfo);
			}
		}

		#endregion

		#region KG_StartTime

		protected override void CheckKG_StartTime()
		{
			base.CheckKG_StartTime();

			MandatoryValidation.CheckEntered(Parent.KG_StartTimeInfo);
			if (!Parent.KG_StartTimeInfo.HasErrors())
			{
				TransportDateRangeValidation.ErrorOnStartIfAfterEnd((ZPropertyInfo<ZDateTimeOffset>)Parent.KG_StartTimeInfo, (ZPropertyInfo<ZDateTimeOffset>)Parent.KG_EndTimeInfo);
			}
		}

		#endregion

		#region CheckOverlap

		void CheckTransportCompanyOverlap(ZPropertyInfo info)
		{
			if (!Parent.KG_AdHocDriversName.IsEmpty)
			{
				var query = new ZQuery(DtbConsignmentRunSheetSchema.KG_OH_TransportCo, Parent.KG_OH_TransportCo);
				query.AddToFilter(DtbConsignmentRunSheetSchema.KG_AdHocDriversName, Parent.KG_AdHocDriversName);

				var transportCoName = Parent.TransportCo != null ? Parent.TransportCo.OH_FullName.ToString() : "";
				var driverName = Parent.KG_AdHocDriversName;
				var description = driverName.IsEmpty ? transportCoName : Invariant($"{transportCoName} ({driverName})"); // Company name and driver names are not translated.
				var additionalMessage = Res.GetString("d1db04e5-1140-490a-a7be-43274fb0cbac", "Multiple Transport Company Run Sheets may be booked for the same day if the Driver Name is not specified or unique.");

				CheckOverlap(query, description, info, additionalMessage);
			}
		}

		void CheckOverlap(SchemaColumn column, IZType value, ZString description, ZPropertyInfo info)
		{
			CheckOverlap(new ZQuery(column, value), description, info);
		}

		void CheckOverlap(ZQuery runSheetQuery, ZString description, ZPropertyInfo info, string additionalMessage = "")
		{
			var query = GetQueryForRunsheetsThatOverlap(runSheetQuery);
			var runsheets = Parent.Factory.Load<DtbConsignmentRunSheet>(query);

			string errorMessage = string.Join("\r\n", runsheets.Select(r => GetErrorMessageForTimeOverlap(r, description)));
			if (!string.IsNullOrEmpty(errorMessage))
			{
				info.AddError(string.IsNullOrEmpty(additionalMessage) ? errorMessage : errorMessage + "\r\n" + additionalMessage);
			}
		}

		string GetErrorMessageForTimeOverlap(DtbConsignmentRunSheet runSheet, ZString description)
		{
			var today = ZDateTime.Today;

			bool datesNotSame = runSheet.KG_StartTime.Date != today.Date || runSheet.KG_EndTime.Date != today.Date;
			var startTime = datesNotSame ? runSheet.KG_StartTime.ToLocalZDateTime().ToLongTimeString() : runSheet.KG_StartTime.ToLocalZDateTime().ToShortTimeString();
			var endTime = datesNotSame ? runSheet.KG_EndTime.ToLocalZDateTime().ToLongTimeString() : runSheet.KG_EndTime.ToLocalZDateTime().ToShortTimeString();

			return Res.GetString("064a1b94-fd27-4290-a8d1-93e438ef57a0",
				"{0} is already booked for Run Sheet {1} between {2} and {3}.", description, runSheet.KG_RunSheetNumber, startTime, endTime);
		}

		ZQuery GetQueryForRunsheetsThatOverlap(ZQuery runSheetQuery)
		{
			var overlapQuery = new ZQuery();
			overlapQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_StartTime, SQLComparisonOperator.LessThan, Parent.KG_EndTime);
			overlapQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_EndTime, SQLComparisonOperator.GreaterThan, Parent.KG_StartTime);

			runSheetQuery.AddToFilter(overlapQuery, JoinCondition.And);
			runSheetQuery.AddToFilter(DtbConsignmentRunSheetSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			runSheetQuery.OrderBy = DtbConsignmentRunSheetSchema.Constants.KG_RunSheetNumber;

			return runSheetQuery;
		}

		#endregion
	}
}
