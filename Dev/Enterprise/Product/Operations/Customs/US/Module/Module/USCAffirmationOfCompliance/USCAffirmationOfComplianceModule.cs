using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Module for USCAffirmationOfCompliance.
	/// </summary>
	public class USCAffirmationOfComplianceModule : USCFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.AffirmationOfCompliance;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override IFilterControl GetNewFilterControl() => new USCAffirmationOfComplianceFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new USCAffirmationOfComplianceCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USCAffirmationOfComplianceFilterStripBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
