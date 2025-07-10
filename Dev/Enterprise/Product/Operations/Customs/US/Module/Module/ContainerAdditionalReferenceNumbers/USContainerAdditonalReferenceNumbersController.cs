using CargoWise.EntityFramework;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.US.Module
{
	public class USContainerAdditonalReferenceNumbersController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity) => new ContainersForm(businessEntity as ForwardingContainer);

		public override ControllerID ID => ControllerIDs.Customs.US.ContainerAdditionalReferenceNumbers;

		public override ModuleIdentifier ModuleID => ModuleIDs.Containers;

		public override System.Type TypeOfTopLevelBusinessObject => typeof(ForwardingContainer);

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new USContainerAdditionalNumbersPlugIn(businessEntity);

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => Factory.New<ForwardingContainer>();
	}
}
