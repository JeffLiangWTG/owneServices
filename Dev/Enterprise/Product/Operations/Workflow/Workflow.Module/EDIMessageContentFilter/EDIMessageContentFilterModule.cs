using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Workflow.Module
{
	class EDIMessageContentFilterModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Messaging.EDIMessageContentFilter;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Messaging.EDIMessageContentFilter);

		protected override IFilterControl GetNewFilterControl() => new EDIMessageContentFilterFilterControl(GridCollection, (EDIMessageContentFilterFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new EDIMessageContentFilterCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EDIMessageContentFilterFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EDIMessageContentFilter;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;
	}
}
