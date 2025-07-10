using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCQuotaModule : USCFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.Quota;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IFilterControl GetNewFilterControl() => new USCQuotaFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCQuotaCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCQuotaFilterStripBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
