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
	public class RefCommodityCodeModule : ZFilterGridModule
	{
		public RefCommodityCodeModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.RefCommodityCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.RefCommodityCode);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefCommodityCodeFilterControl(GridCollection, (RefCommodityCodeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RefCommodityCodeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefCommodityCodeFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Commodity; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
