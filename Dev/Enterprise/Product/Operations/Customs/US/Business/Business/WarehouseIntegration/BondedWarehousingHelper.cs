using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class BondedWarehousingHelper : Customs.Business.BondedWarehousingHelper
	{
		public BondedWarehousingHelper(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected bool IsWarehouseEntryType
		{
			get
			{
				if (isWarehouseEntryTypeCached == null)
				{
					isWarehouseEntryTypeCached = new CachedProperty<bool>(declaration.Factory, () => declaration.IsWarehouseEntryType);
				}
				return isWarehouseEntryTypeCached.Value;
			}
		}
		CachedProperty<bool> isWarehouseEntryTypeCached;

		protected bool IsExWarehouseEntryType
		{
			get
			{
				if (isExWarehouseEntryTypeCached == null)
				{
					isExWarehouseEntryTypeCached = new CachedProperty<bool>(declaration.Factory, () => declaration.IsExWarehouseEntryType);
				}
				return isExWarehouseEntryTypeCached.Value;
			}
		}
		CachedProperty<bool> isExWarehouseEntryTypeCached;

		protected bool IsConsumptionFTZ
		{
			get
			{
				if (isConsumptionFTZCached == null)
				{
					isConsumptionFTZCached = new CachedProperty<bool>(declaration.Factory, () => declaration.IsENSFormalImportAndConsumptionFTZ);
				}
				return isConsumptionFTZCached.Value;
			}
		}
		CachedProperty<bool> isConsumptionFTZCached;

		protected override bool IsMarkedForBondedWarehousingCore(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			return (IsExWarehouseEntryType || IsConsumptionFTZ) ? ((JobComInvoiceLine)invoiceLine).JI_PartNo_CanBeSetByCustomer : invoiceLine.IsGoingIntoBondedWarehouse;
		}

		protected override bool HasBondedWarehouseEntryDetailsCore(Customs.Business.BaseJobComInvoiceLine invoiceLine, bool isInward, bool isOutward, bool isChangeOfOwnership)
		{
			var usInvoiceLine = (JobComInvoiceLine)invoiceLine;
			var result = false;
			if (declaration.IsImportByExternalBroker || IsExWarehouseEntryType)
			{
				result = !usInvoiceLine.US_WHSEntryLineNo.IsEmpty;
			}
			else if (IsWarehouseEntryType)
			{
				result = true;
			}
			else if (IsConsumptionFTZ)
			{
				result = !usInvoiceLine.US_WHSEntryLineNo.IsEmpty && !usInvoiceLine.US_WHSEntryNumber.IsEmpty;
			}
			return result;
		}

		public void UpdateWarehouseWithdrawal()
		{
			var enteredQty = declaration.US_QtyBeingWithdrawn;
			var calculatedQty = declaration.InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_InvoiceQuantity);
			if (enteredQty != calculatedQty)
			{
				declaration.US_QtyBeingWithdrawn = calculatedQty;
				declaration.US_IsFinalWHS = declaration.US_QtyInWHAfterWithdrawal.IsEmpty;
			}
		}

		protected new JobDeclaration declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}
	}
}
