using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Module for USCFIRMS
	/// </summary>
	class USCFIRMSModule : USCFilterGridModule
	{
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.FIRMS;

		protected override IFilterControl GetNewFilterControl() => new USCFIRMSFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCFIRMSCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCFIRMSFilterStripBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;
	}
}
