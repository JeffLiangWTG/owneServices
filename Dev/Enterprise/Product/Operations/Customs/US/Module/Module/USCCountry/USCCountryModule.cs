using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCCountryModule : USCFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.Country;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IFilterControl GetNewFilterControl() => new USCCountryFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCCountryCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCCountryFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.USCCountry);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override bool ShowRecentItemsCore() => false;
	}
}
