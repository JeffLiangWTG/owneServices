using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.PL.Module;

public class TemporaryStorageModule : EU.TemporaryStorage.Module.TemporaryStorageModule
{
	protected override IBusinessObjectCollection GetNewGridCollection() => new EU.Business.CusTempStorage.CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch);

	protected override IFilterControl GetNewFilterControl() => new TemporaryStorageFilterControl(GridCollection, (TemporaryStorageFilterStripBusinessObject)FilterBusinessObject);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new TemporaryStorageFilterStripBusinessObject();

	protected override ZController GetNewController(BusinessObject selectedBusinessObject)
	{
		return ZControllerFactory.Create(ControllerIDs.Customs.TemporaryStorage);
	}
}
