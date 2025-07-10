using CargoWise.EntityFramework;
using Enterprise.ContractManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ContractManagement.Module
{
	public class AllocationRouteModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.ContractAllocationRoutes;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new ModuleGuiNotSupportedException("Findbox only");
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AllocationRouteFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AllocationRouteFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new RatingContractAllocationLineCollection(Factory);
		}
	}
}
