using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Rating;

public class MappedChargeCodeBizoValidation(MappedChargeCodeBizo bizo) : ZValidation(bizo)
{
	public override void ValidateAll() => ValidateAllCore();

	protected virtual void ValidateAllCore() { }

	public override Type AutoValidationType => typeof(MappedChargeCodeBizoValidation);

	protected IValidationInternals ZValidationInternals => this;
}
