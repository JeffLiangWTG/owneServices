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
	public class EDIMessageDeliveryContextModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Messaging.EDIMessageDeliveryContext;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Messaging.EDIMessageDeliveryContext);

		protected override IFilterControl GetNewFilterControl() => new EDIMessageDeliveryContextFilterControl(GridCollection, (EDIMessageDeliveryContextFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new EDIMessageDeliveryContextSelectorCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EDIMessageDeliveryContextFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EDIMessageDeliveryContext;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Workflow;
	}
}
