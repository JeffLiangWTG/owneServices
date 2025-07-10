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
	class EDIMessagePurposeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Messaging.EDIMessagePurpose;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Messaging.EDIMessagePurpose);

		protected override IFilterControl GetNewFilterControl() => new EDIMessagePurposeFilterControl(GridCollection, (EDIMessagePurposeFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new EDIMessagePurposeCollection(Factory, new ZQuery());

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EDIMessagePurposeFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EDIMessagePurpose;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;
	}
}
