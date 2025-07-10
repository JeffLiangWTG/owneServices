
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Shared.Module
{
	/// <summary>
	/// Module Controller for RefPacks.
	/// </summary>
	public class RefPacksModule : ZFilterGridModule
	{
		public RefPacksModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.RefPacks; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.RefPacks);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new RefPacksFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusRefPacksCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new RefPacksFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Broker; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.RefPacks; }
		}
	}
}
