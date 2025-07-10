using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class CourtesyNoticesOfLiquidationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.CourtesyNoticesOfLiquidation;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CourtesyNoticesOfLiquidationMessages;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.CourtesyNoticesOfLiquidation);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusLiquidationCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CourtesyNoticesOfLiquidationFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CourtesyNoticesOfLiquidationFilterControl((CusLiquidationCollection)GridCollection, (CourtesyNoticesOfLiquidationFilterStripBusinessObject)FilterBusinessObject);

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
	}
}
