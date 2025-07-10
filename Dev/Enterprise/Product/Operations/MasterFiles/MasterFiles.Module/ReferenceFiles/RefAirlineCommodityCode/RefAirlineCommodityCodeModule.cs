using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefAirlineCommodityCodeModule : ZFilterGridModule
	{
		public RefAirlineCommodityCodeModule()
		{
		}

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		public override bool AllowView => false;

		public override ModuleIdentifier ID => ModuleIDs.RefAirlineCommodityCode;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Commodity;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefAirlineCommodityCode);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefAirlineCommodityCodeFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefAirlineCommodityCodeFilterControl(GridCollection, (RefAirlineCommodityCodeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefAirlineCommodityCodeCollection(Factory);
		}
	}
}
