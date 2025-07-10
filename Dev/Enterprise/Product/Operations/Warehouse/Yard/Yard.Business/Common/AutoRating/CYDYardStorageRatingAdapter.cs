using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardStorageRatingAdapter : CYDYardUnitsRatingAdapter<PeriodicInvoicing, CYDPeriodicInvoicingYardUnits>, IRatingAdapterHumanReadableName
	{
		public CYDYardStorageRatingAdapter(PeriodicInvoicing periodicInvoicing, CYDPeriodicInvoicingYardUnits unitsForRating, CYDYardUnitState yardUnit) : base(periodicInvoicing, unitsForRating, yardUnit)
		{
			this.yardUnit = yardUnit;
		}

		readonly CYDYardUnitState yardUnit;

		#region Measures

		public override IRateableMeasureSet RateableMeasures
		{
			get
			{
				var rateableMeasures = (RateableMeasureSet)base.RateableMeasures;
				rateableMeasures.Time = StorageTimeInfo.TimeInfo;

				return rateableMeasures;
			}
		}

		CYDYardStorageTimeInfo StorageTimeInfo => storageTimeInfo ??= new CYDYardStorageTimeInfo(JobDatesProvider, yardUnit, Parent);

		CYDYardStorageTimeInfo storageTimeInfo;

		#endregion

		#region IRatingAdapterHumanReadableName Members

		ZString IRatingAdapterHumanReadableName.HumanReadableName => $"{yardUnit.YUS_UnitID} ({StorageTimeInfo.StartDate.ToShortDateString()} - {StorageTimeInfo.EndDate.ToShortDateString()})";

		#endregion

		public override void OnAutoRated(IEnumerable<IAutoRatedCharge> charges)
		{
			base.OnAutoRated(charges);

			if (charges.Any())
			{
				var storageLine = FindOrCreateStorageLine();
				var storageCharge = charges.FirstOrDefault(charge => charge.CalculatorType == CalculatorType.Time);
				SaveStorageLine(storageLine, storageCharge);
			}
		}

		CYDYardStorageLines FindOrCreateStorageLine()
		{
			var jobStorageFilter = new ZQuery(CYDYardStorageLinesSchema.YSL_ET_JobStorage, Parent.PK);
			var yardUnitfilter = new ZQuery(CYDYardStorageLinesSchema.YSL_YUS_YardUnit, yardUnit.PK);
			var storageLine = Parent.Factory.LoadTop1<CYDYardStorageLines>(new ZQuery(jobStorageFilter, yardUnitfilter));
			if (storageLine == null)
			{
				storageLine = Parent.Factory.New<CYDYardStorageLines>();
				storageLine.YSL_ET_JobStorage = Parent.PK;
				storageLine.YSL_YUS_YardUnit = yardUnit.PK;
			}
			return storageLine;
		}

		void SaveStorageLine(CYDYardStorageLines storageLine, IAutoRatedCharge charge)
		{
			storageLine.YSL_FreeDaysOpenBalance = (byte)StorageTimeInfo.FreeStorageDays;
			storageLine.YSL_FreeDaysCloseBalance = (byte)StorageTimeInfo.TimeInfo.RemainingFreeDays;
			storageLine.YSL_StorageDays = (short)StorageTimeInfo.TimeInfo.Span.TotalDays;
			if (charge != null && charge is AutoRateInfo autoRateInfo)
			{
				storageLine.YSL_ChargedDays = (short)autoRateInfo.Attributes.GetSingleValue<decimal>(JobChargeAttribTypeList.Codes.ItemsToRate);
				storageLine.YSL_ChargedAmount = autoRateInfo.Amount;
			}
		}
	}
}
