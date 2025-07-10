using System.Globalization;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.GUI
{
	public class BondedWarehouseOperationDeterminer : Customs.GUI.BondedWarehouseOperationDeterminer
	{
		public BondedWarehouseOperationDeterminer(JobDeclaration supporter)
			: base(supporter)
		{
		}

		protected new JobDeclaration supporter
		{
			get { return (JobDeclaration)base.supporter; }
		}

		#region Inventory Management Data

		bool IsEntrySummaryWaitingForResponse
		{
			get
			{
				var entry = supporter.ActiveEntryHeaders.EntrySummaryEntry;
				return entry != null && entry.IsWaitingForResponse;
			}
		}

		#endregion

		protected override bool CanCancelBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanCancelBondedWarehouseOutwardCheckCore();
			if (IsEntrySummaryWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(string.Format(CultureInfo.InvariantCulture, "Cannot cancel {0} stock release while waiting for a response.", supporter.TermNameForBondedWarehouse));
			}
			return result;
		}

		protected override bool CanCancelUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanCancelUpdateBondedWarehouseInwardCheckCore();

			if (IsEntrySummaryWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(string.Format(CultureInfo.InvariantCulture, "Cannot cancel {0} stock levels update while waiting for a response.", supporter.TermNameForBondedWarehouse));
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseInwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseInwardCheckCore();
			if (IsEntrySummaryWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(string.Format(CultureInfo.InvariantCulture, "Cannot update {0} stock levels while waiting for a response.", supporter.TermNameForBondedWarehouse));
			}
			return result;
		}

		protected override bool CanUpdateBondedWarehouseOutwardCheckCore()
		{
			var result = base.CanUpdateBondedWarehouseOutwardCheckCore();
			if (IsEntrySummaryWaitingForResponse)
			{
				result = false;
				Globals.Message.ShowError(string.Format(CultureInfo.InvariantCulture, "Cannot update {0} stock release while waiting for a response.", supporter.TermNameForBondedWarehouse));
			}
			return result;
		}
	}
}
