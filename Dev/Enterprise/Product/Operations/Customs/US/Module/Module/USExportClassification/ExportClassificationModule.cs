using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class ExportClassificationModule : Customs.Module.ExportClassificationModule
	{
		protected override IFilterControl GetNewFilterControl() => new USExportClassificationFilterControl(GridCollection, (USExportClassificationFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ExportClassificationCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USExportClassificationFilterBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ExportBroker;

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USExportClassification;
	}
}
