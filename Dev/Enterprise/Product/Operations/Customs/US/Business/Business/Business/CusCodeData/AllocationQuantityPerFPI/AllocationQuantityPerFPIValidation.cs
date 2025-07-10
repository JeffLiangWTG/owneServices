using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AllocationQuantityPerFPIValidation : Customs.Business.CusCodeDataValidation
	{
		public AllocationQuantityPerFPIValidation(AllocationQuantityPerFPI bizObj)
			: base(bizObj)
		{
		}

		new AllocationQuantityPerFPI Parent => (AllocationQuantityPerFPI)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateManufacturerOrgPK();
			ValidateUS_OA_ManufacturerAddress();
			ValidateUS_AllocationQuantity();
		}

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo, Parent.Lookups.ForeignProducerIdentifiers);

			if (!Parent.US_OA_ManufacturerAddress.IsEmpty || !Parent.US_AllocationQuantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_CodeInfo, "FPI");
			}

			ValidateManufacturerOrgPK();
			ValidateUS_AllocationQuantity();
		}

		public void ValidateManufacturerOrgPK()
		{
			ValidateCalculatedProperty(Parent.ManufacturerOrgPKInfo);
		}

		protected void CheckManufacturerOrgPK()
		{
			if (!Parent.US_AllocationQuantity.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ManufacturerOrgPKInfo);
			}
		}

		public void ValidateUS_OA_ManufacturerAddress()
		{
			ValidateCalculatedProperty(Parent.US_OA_ManufacturerAddressInfo);
		}

		protected void CheckUS_OA_ManufacturerAddress()
		{
			TypeValidation.CheckValidGuid(Parent.US_OA_ManufacturerAddressInfo);

			ValidateUS_AllocationQuantity();
			ValidateCY_Code();
		}

		public void ValidateUS_AllocationQuantity()
		{
			ValidateCalculatedProperty(Parent.US_AllocationQuantityInfo);
		}

		protected void CheckUS_AllocationQuantity()
		{
			TypeValidation.CheckValidDecimal(Parent.US_AllocationQuantityInfo, 12, 4);

			if (!Parent.US_OA_ManufacturerAddress.IsEmpty || !Parent.US_ForeignProducerIdentifier.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_AllocationQuantityInfo);
			}

			ValidateManufacturerOrgPK();
			ValidateCY_Code();
		}
	}
}
