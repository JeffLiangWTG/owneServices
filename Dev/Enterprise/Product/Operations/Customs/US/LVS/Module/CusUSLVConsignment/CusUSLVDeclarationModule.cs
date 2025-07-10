using CargoWise.EntityFramework;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVDeclarationModule : JobDeclarationModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USLowValueEntriesDeclaration;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USLVConsignment;

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<USConsignmentCombined>(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new USConsignmentCombinedFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CusUSLVConsignmentFilterControl(GridCollection, (USConsignmentCombinedFilterBusinessObject)FilterBusinessObject);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntriesDeclaration);

		#region Module Decision Provider

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new USConsignmentCombinedModuleDecisionProvider(this);

		protected class USConsignmentCombinedModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public USConsignmentCombinedModuleDecisionProvider(ZFilterModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false;
		}

		#endregion
	}
}
