using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
	public class RefHarbourRateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.Universal.RefHarbourRate;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalHarbourRate;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) =>
			ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefHarbourRate);

		protected override IBusinessObjectCollection GetNewGridCollection() =>
			new RefHarbourRateCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new RefHarbourRateFilterStripControl(GridCollection,
			(RefHarbourRateFilterStripBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() =>
			new RefHarbourRateFilterStripBusinessObject();

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		public override bool AllowUniversalCopy => false;

		protected virtual bool AllowNewAndEditUserDefinedRecords => false;
	}
}
