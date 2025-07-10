using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Rating;

[ModuleID(ModuleId.UniversalChargeCode)]
public class UniversalChargeCodeBizoCollection(BusinessObjectFactory factory) : NonPersistentBusinessObjectCollection<UniversalChargeCodeBizo>(factory)
{
	protected override BusinessObject CreateNonPersistentBusinessObject() => new UniversalChargeCodeBizo(Factory);
	protected override bool AllowNewCore => false;
	protected override bool AllowRemoveCore => false;
}
