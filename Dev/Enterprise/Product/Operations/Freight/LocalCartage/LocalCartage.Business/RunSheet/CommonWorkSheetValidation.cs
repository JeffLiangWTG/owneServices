using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public class CommonWorkSheetValidation : JobCartageRunSheetValidation
	{
		public CommonWorkSheetValidation(CommonWorkSheet parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateErrorStatus();
		}

		public void ValidateErrorStatus()
		{
			ValidateCalculatedProperty(RunSheet.ErrorStatusInfo);
		}

		protected virtual void CheckErrorStatus()
		{
			MandatoryValidation.CheckEntered(RunSheet.ErrorStatusInfo);
			ListValidation.ErrorIfInvalidCode(RunSheet.ErrorStatusInfo);
		}

		protected override void CheckEY_StartTime()
		{
			base.CheckEY_StartTime();
			MandatoryValidation.CheckEntered(RunSheet.EY_StartTimeInfo);
			CheckStartEndTimeDontMatch(RunSheet.EY_StartTimeInfo);
			CheckTruckAndDriverDoesntOverrlapOtherRunSheets(RunSheet.EY_StartTimeInfo);
		}

		protected override void CheckEY_EndTime()
		{
			base.CheckEY_EndTime();
			MandatoryValidation.CheckEntered(RunSheet.EY_EndTimeInfo);
			CheckStartEndTimeDontMatch(RunSheet.EY_EndTimeInfo);
			CheckTruckAndDriverDoesntOverrlapOtherRunSheets(RunSheet.EY_EndTimeInfo);
		}

		protected override void CheckEY_RQ_Truck()
		{
			base.CheckEY_RQ_Truck();
			CheckTruckOrDriverDoesntOverrlapOtherRunSheets(RunSheet.EY_RQ_TruckInfo, DriverTruckCheck.CheckTruck);
			CheckTruckWeightVolumeCapacity();
		}

		protected override void CheckEY_GS_NKTruckDriver()
		{
			base.CheckEY_GS_NKTruckDriver();
			CheckTruckOrDriverDoesntOverrlapOtherRunSheets(RunSheet.EY_GS_NKTruckDriverInfo, DriverTruckCheck.CheckDriver);
		}

		void CheckTruckWeightVolumeCapacity()
		{
			ZString warning = CapacityChecker.CheckTruckCapacity();
			if (!warning.IsEmpty)
			{
				RunSheet.EY_RQ_TruckInfo.AddWarning(warning);
			}
		}

		WorkSheetTruckCapacityChecker CapacityChecker
		{
			get
			{
				if (fCapacityChecker == null)
				{
					fCapacityChecker = new WorkSheetTruckCapacityChecker(RunSheet);
				}

				return fCapacityChecker;
			}
		}
		WorkSheetTruckCapacityChecker fCapacityChecker;

		void CheckStartEndTimeDontMatch(ZPropertyInfo info)
		{
			if (!info.HasErrors() && RunSheet.EY_StartTime == RunSheet.EY_EndTime)
			{
				info.AddError(Res.GetString("93c4a5e9-afa7-42a1-a78f-1f139559d629", "Start and End Time cannot be the same"));
			}
		}

		void CheckTruckAndDriverDoesntOverrlapOtherRunSheets(ZPropertyInfo info)
		{
			CheckTruckOrDriverDoesntOverrlapOtherRunSheets(info, DriverTruckCheck.CheckBoth);
		}

		internal void CheckTruckOrDriverDoesntOverrlapOtherRunSheets(ZPropertyInfo info, DriverTruckCheck check)
		{
			var checkTruck = check == DriverTruckCheck.CheckTruck || check == DriverTruckCheck.CheckBoth && !RunSheet.EY_RQ_Truck.IsEmpty;
			var checkDriver = check == DriverTruckCheck.CheckDriver || check == DriverTruckCheck.CheckBoth && !RunSheet.EY_GS_NKTruckDriver.IsEmpty;

			if (!info.HasErrors()
				&& RunSheet.EY_StartTime.IsValid
				&& RunSheet.EY_EndTime.IsValid
				&& ((checkTruck && !RunSheet.EY_RQ_Truck.IsEmpty) || (checkDriver && !RunSheet.EY_GS_NKTruckDriver.IsEmpty)))
			{
				var runSheetsQuery = new ZQuery(JobCartageRunSheetSchema.PK, SQLComparisonOperator.NotEqual, RunSheet.PK);
				runSheetsQuery.AddToFilter(JobCartageRunSheetSchema.EY_StartTime, SQLComparisonOperator.LessThan, RunSheet.EY_EndTime);
				runSheetsQuery.AddToFilter(JobCartageRunSheetSchema.EY_EndTime, SQLComparisonOperator.GreaterThan, RunSheet.EY_StartTime);

				var truckAndDriverQuery = new ZQuery();
				if (checkTruck)
				{
					truckAndDriverQuery.AddToFilter(JoinCondition.Or, JobCartageRunSheetSchema.EY_RQ_Truck, RunSheet.EY_RQ_Truck);
				}

				if (checkDriver)
				{
					truckAndDriverQuery.AddToFilter(JoinCondition.Or, JobCartageRunSheetSchema.EY_GS_NKTruckDriver, RunSheet.EY_GS_NKTruckDriver);
				}
				runSheetsQuery.AddToFilter(truckAndDriverQuery);

				var similarRunSheet = RunSheet.Factory.LoadTop1<CommonWorkSheet>(runSheetsQuery);
				if (similarRunSheet != null)
				{
					var builder = new ZStringBuilder();
					builder.AppendIfNotEmpty(Res.GetString("a1f486bb-135f-4284-8c12-96445a53f0ff", "Run Sheet '{0} - {1} to {2}'", RunSheet.EY_RunSheetNumber, RunSheet.EY_StartTime.ToLongTimeString(), RunSheet.EY_EndTime.ToLongTimeString()));
					builder.AppendIfNotEmpty(Res.GetString("85ac7716-96ca-409a-ae61-4b9e4b2842ef", "overlaps"));
					builder.AppendIfNotEmpty(Res.GetString("a1f486bb-135f-4284-8c12-96445a53f0ff", "Run Sheet '{0} - {1} to {2}'", similarRunSheet.EY_RunSheetNumber, similarRunSheet.EY_StartTime.ToLongTimeString(), similarRunSheet.EY_EndTime.ToLongTimeString()));
					builder.AppendIfNotEmpty(Res.GetString("ba7abdb0-0cb8-465e-a699-07c60d49ec7a", "with same:"));

					if (similarRunSheet.EY_GS_NKTruckDriver == RunSheet.EY_GS_NKTruckDriver && RunSheet.TruckDriver != null)
					{
						builder.Append(Res.GetString("8623a9b4-7e72-413d-b7c4-8856928b0f4c", "Driver '{0}'", RunSheet.TruckDriver.GS_FullName));
					}

					if (similarRunSheet.EY_RQ_Truck == RunSheet.EY_RQ_Truck && RunSheet.Truck != null)
					{
						builder.Append(Res.GetString("56ab0d71-3c26-45fa-ad98-9d7200a4b438", "Vehicle '{0}'", RunSheet.Truck.RQ_DescriptionMultilingual));
					}

					info.AddError(builder.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		internal enum DriverTruckCheck
		{
			CheckDriver,
			CheckTruck,
			CheckBoth
		}

		CommonWorkSheet RunSheet
		{
			get { return (CommonWorkSheet)Parent; }
		}
	}
}
