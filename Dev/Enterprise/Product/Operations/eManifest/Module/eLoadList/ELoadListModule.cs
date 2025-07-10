
namespace Enterprise.eManifest.Module
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.eManifest.Business;
	using Enterprise.eManifest.GUI;
	using Enterprise.Environment;
	using Enterprise.Licensing;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;

	public class ELoadListModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ELoadList; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return null; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.MaintainELoadList; }
		}

		public override ZBool HasActions
		{
			get { return false; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ELoadListCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ELoadListFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ELoadListFilterControl(GridCollection, FilterBusinessObject);
		}
	}
}
