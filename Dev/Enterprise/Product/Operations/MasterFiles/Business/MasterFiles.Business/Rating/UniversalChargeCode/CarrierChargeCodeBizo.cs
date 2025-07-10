using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Rating;

[CodeProperty(Schema.UCC_ForeignCode)]
[DescriptionProperty(Schema.UCC_ForeignName)]
public class CarrierChargeCodeBizo(BusinessObjectFactory factory) : MappedChargeCodeBizo(factory)
{
	protected override MappedChargeCodeBizoValidation Validation => new CarrierChargeCodeBizoValidation(this);
}
