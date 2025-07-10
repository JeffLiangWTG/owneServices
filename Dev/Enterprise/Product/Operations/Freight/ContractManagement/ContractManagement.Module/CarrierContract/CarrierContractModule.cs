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
	public class CarrierContractModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.CarrierContracts;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new ModuleGuiNotSupportedException("Findbox only");
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CarrierContractFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CarrierContractFilterStripControl();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CarrierContractCollection(Factory);
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new CarrierContractModuleDecisionProvider(this);
		}

		class CarrierContractModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public CarrierContractModuleDecisionProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}
	}
}
