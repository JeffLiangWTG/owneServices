using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCVisaTariffModule : USCFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.TariffRequiringVisa;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IFilterControl GetNewFilterControl() => new USCVisaTariffFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCVisaTariffNonDependentCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCVisaTariffFilterStripBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
