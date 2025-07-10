using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class GuaranteesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.Guarantees;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Guarantees;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.Guarantees);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GuaranteesFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GuaranteesFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusGuaranteeHeaderCollection(Factory);

		public override bool AllowUniversalCopy => false;
	}
}
