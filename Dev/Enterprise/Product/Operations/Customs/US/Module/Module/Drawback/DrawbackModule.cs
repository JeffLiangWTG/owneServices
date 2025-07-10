using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	class DrawbackModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => JobDeclarationZControllerDecider.GetZController(selectedBusinessObject as JobDeclaration) ?? ZControllerFactory.Create(ControllerIDs.Customs.US.Drawback);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new DrawbackFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new DrawbackFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.Drawback;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Drawback;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USDrawback;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode;
	}
}
