using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.Transhipment.Module
{
	/// <summary>
	/// Module for CusInBondHeaders
	/// </summary>
	public class CusInBondHeaderModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.TW.Transhipment;

		public override bool AllowNew => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => true;

		protected override IFilterControl GetNewFilterControl() => new CusInBondHeaderFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusInBondHeaderCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusInBondHeaderFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.TW.Transhipment);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.TWTranshipment;

		public override bool SupportsWorkflow => false;
	}
}
