using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business;

public class ProductInformationDeliveryContactValidation(ProductInformationDeliveryContact parent) : DocDeliveryContactValidation(parent)
{
	#region Supplier
	public void ValidateSupplier()
	{
		ValidateCalculatedProperty(parent.SupplierInfo);
	}

	protected virtual void CheckSupplier()
	{
		MandatoryValidation.CheckEntered(parent.SupplierInfo);
		ListValidation.ErrorIfInvalidCode(parent.SupplierInfo);
	}
	#endregion

	#region Implementation
	public override Type AutoValidationType => typeof(ProductInformationDeliveryContactValidation);

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateSupplier();
	}
	#endregion
}
