using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class ImportClassificationModule : Customs.Module.ImportClassificationModule
	{
		public ImportClassificationModule()
		{
			AddImportFromCSVDataMenuItem(new CreateFormHandler(CreateImportFromCSVForm));
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USImportClassification;

		protected override IFilterControl GetNewFilterControl() => new USImportClassificationFilterControl(GridCollection, (USImportClassificationFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ImportClassificationCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USImportClassificationFilterBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		KForm CreateImportFromCSVForm() => new ImportClassificationsFromCSVForm();
	}
}
