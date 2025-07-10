using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class USCCarrierAndFIRMSModule : USCFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USCCarrierAndFIRMS;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IFilterControl GetNewFilterControl() => new USCCarrierAndFIRMSFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCCarrierAndFIRMSCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCCarrierAndFIRMSFilterStripBusinessObject();
	}
}
