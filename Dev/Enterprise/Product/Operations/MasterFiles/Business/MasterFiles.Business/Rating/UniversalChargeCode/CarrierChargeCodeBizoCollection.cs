using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Rating;

[ModuleID(ModuleId.CarrierChargeCode)]
public class CarrierChargeCodeBizoCollection(BusinessObjectFactory factory) : NonPersistentBusinessObjectCollection<CarrierChargeCodeBizo>(factory)
{
	protected override BusinessObject CreateNonPersistentBusinessObject() => new CarrierChargeCodeBizo(Factory);
	protected override bool AllowNewCore => false;
	protected override bool AllowRemoveCore => false;
}
