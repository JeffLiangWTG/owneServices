using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public abstract class ConsolidatedDeclarationModule : ZFilterGridModule
	{
		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.ConsolidatedDeclarationWorkflowDescriptorCode;

		public sealed override ModuleIdentifier ID => ModuleIDs.Customs.ConsolidatedDeclaration;

		public sealed override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsConsolidatedDeclaration;

		protected sealed override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.ConsolidatedEntry };

		protected sealed override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.ConsolidatedDeclaration);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ConsolidatedDeclarationFilterBusinessObject();

		protected sealed override IFilterControl GetNewFilterControl() => new ConsolidatedDeclarationFilterStripControl(this, GridCollection, (ConsolidatedDeclarationFilterBusinessObject)FilterBusinessObject);
	}
}
