using IWarehouseIntegrationSupporter = Enterprise.Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter;

namespace Enterprise.Customs.GUI
{
	public class BondedWarehouseOperationDeterminer
	{
		public BondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			this.supporter = supporter;
		}
		protected readonly IWarehouseIntegrationSupporter supporter;

		public bool CanCancelBondedWarehouseOutwardCheck()
		{
			return CanCancelBondedWarehouseOutwardCheckCore();
		}

		protected virtual bool CanCancelBondedWarehouseOutwardCheckCore()
		{
			return true;
		}

		public bool CanCancelBondedWarehouseChangeOfOwnershipCheck()
		{
			return CanCancelBondedWarehouseChangeOfOwnershipCheckCore();
		}

		protected virtual bool CanCancelBondedWarehouseChangeOfOwnershipCheckCore()
		{
			return true;
		}

		public bool CanCancelInventoryChangeOfRegimeCheck()
		{
			return CanCancelInventoryChangeOfRegimeCheckCore();
		}

		protected virtual bool CanCancelInventoryChangeOfRegimeCheckCore()
		{
			return true;
		}

		public bool CanCancelUpdateBondedWarehouseInwardCheck()
		{
			return CanCancelUpdateBondedWarehouseInwardCheckCore();
		}

		protected virtual bool CanCancelUpdateBondedWarehouseInwardCheckCore()
		{
			return true;
		}

		public bool CanUpdateBondedWarehouseOutwardCheck()
		{
			return CanUpdateBondedWarehouseOutwardCheckCore();
		}

		protected virtual bool CanUpdateBondedWarehouseOutwardCheckCore()
		{
			return true;
		}

		public bool CanUpdateBondedWarehouseChangeOfOwnershipCheck()
		{
			return CanUpdateBondedWarehouseChangeOfOwnershipCheckCore();
		}

		protected virtual bool CanUpdateBondedWarehouseChangeOfOwnershipCheckCore()
		{
			return true;
		}

		public bool CanUpdateInventoryChangeOfRegimeCheck()
		{
			return CanUpdateInventoryChangeOfRegimeCheckCore();
		}

		protected virtual bool CanUpdateInventoryChangeOfRegimeCheckCore()
		{
			return true;
		}

		public bool CanUpdateBondedWarehouseInwardCheck()
		{
			return CanUpdateBondedWarehouseInwardCheckCore();
		}

		protected virtual bool CanUpdateBondedWarehouseInwardCheckCore()
		{
			return true;
		}
	}
}
