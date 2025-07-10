using CargoWise.Common;

namespace Enterprise.Customs.Business
{
	public class BondedWarehousingHelper
	{
		public BondedWarehousingHelper(BaseJobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}

		public bool HasBondedWarehouseEntryDetails(BaseJobComInvoiceLine invoiceLine, bool isInward, bool isOutward, bool isChangeOfOwnership)
		{
			return HasBondedWarehouseEntryDetailsCore(invoiceLine, isInward, isOutward, isChangeOfOwnership);
		}

		protected virtual bool HasBondedWarehouseEntryDetailsCore(BaseJobComInvoiceLine invoiceLine, bool isInward, bool isOutward, bool isChangeOfOwnership)
		{
			return (isInward && invoiceLine.CusEntryLine?.Header != null) || (isOutward && !invoiceLine.JI_PreviousEntryNumber.IsEmpty) ||
					(isChangeOfOwnership && invoiceLine.CusEntryLine?.Header != null && !invoiceLine.JI_PreviousEntryNumber.IsEmpty) || (invoiceLine.JI_PreviousEntryNumber.IsEmpty && invoiceLine.ComponentInventoryCollection.Count > 0);
		}

		public bool IsMarkedForBondedWarehousing(BaseJobComInvoiceLine invoiceLine)
		{
			return IsMarkedForBondedWarehousingCore(invoiceLine);
		}

		protected virtual bool IsMarkedForBondedWarehousingCore(BaseJobComInvoiceLine invoiceLine)
		{
			var cusProcedure = invoiceLine.CusProcedure;
			return cusProcedure != null && (invoiceLine.IsIntoRegime(cusProcedure) || invoiceLine.IsOutOfRegime(cusProcedure));
		}

		protected readonly BaseJobDeclaration declaration;

		public static class Constants
		{
			public const string CustomsThirdQuantity = "CustomsThirdQuantity";
			public const string CustomsThirdQuantityUnit = "CustomsThirdQuantityUnit";

			public static class AreaTypes
			{
				public const string InwardProcessing = "IPR";
				public const string Bonded = "BON";
			}
		}
	}
}

