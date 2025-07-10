using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Rating;

[CodeProperty(Schema.UCC_Code)]
[DescriptionProperty(Schema.UCC_Description)]
public class UniversalChargeCodeBizo(BusinessObjectFactory factory) : MappedChargeCodeBizo(factory)
{
}
