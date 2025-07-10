namespace Enterprise.MasterFiles.Business.Rating;

class CarrierChargeCodeBizoValidation(CarrierChargeCodeBizo bizo) : MappedChargeCodeBizoValidation(bizo)
{
	CarrierChargeCodeBizo Parent { get; } = bizo;

	protected override void ValidateAllCore()
	{
		base.ValidateAllCore();
		ValidateCarrier();
	}

	void ValidateCarrier() => ZValidationInternals.Validate(Parent.UCC_CarrierInfo, () =>
	{
		if (string.IsNullOrEmpty(Parent.UCC_Carrier))
		{
			return;
		}

		if (!AccChargeCodeUniversalCodeMappingLookups.TryGetCarrierFromScac(Parent.Factory, Parent.UCC_Carrier, out var _))
		{
			Parent.UCC_CarrierInfo.AddError(Res.GetString("14741d30-5b86-4879-8bf4-a36726429929", "Foreign Carrier \"{0}\" is not mapped to CargoWise Organization.", Parent.UCC_Carrier));
		}
	});
}
