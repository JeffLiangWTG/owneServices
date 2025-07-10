using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Module;

public class UniversalChargeCodeModule() : MappedChargeCodeModule<UniversalChargeCodeBizo>
{
	public override ModuleIdentifier ID => ModuleIDs.UniversalChargeCode;

	protected override FilterBusinessObject GetNewFilterBusinessObject() =>
		new UniversalChargeCodeFilterBusinessObject();

	protected override IFilterControl GetNewFilterControl() =>
		new UniversalChargeCodeFilterControl(GridCollection, (UniversalChargeCodeFilterBusinessObject)FilterBusinessObject);

	protected override IBusinessObjectCollection GetNewGridCollection() =>
		new UniversalChargeCodeBizoCollection(Factory);
}
