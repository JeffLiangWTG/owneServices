using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class TransitTimeServiceLevelCombinationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.TransitTimeServiceLevelCombination; }
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new TransitTimeServiceLevelCombinationFilterControl(GridCollection, (TransitTimeServiceLevelCombinationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TransitTimeServiceLevelCombinationCollection(Factory, null, null, null, null, null);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new TransitTimeServiceLevelCombinationFilterBusinessObject();
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.TransitTimeServiceLevelCombination);
		}

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		public override Security.SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TransitTimeServiceLevelCombination; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new TransitTimeServiceLevelCombinationModuleDecisionProvider(this);
		}

		class TransitTimeServiceLevelCombinationModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public TransitTimeServiceLevelCombinationModuleDecisionProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}
	}
}
