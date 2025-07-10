using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Freight.PortHubs.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.PortHubs.Module
{
	public class PortHubSelectionController : ZPopupController
	{
		public override ControllerID ID => ControllerIDs.PortHubSelection;

		public override ModuleIdentifier ModuleID => ModuleIDs.PortHubSelection;

		public override Type TypeOfTopLevelBusinessObject => typeof(PortHubSelection);

		protected override IZForm GetForm(IBusiness businessEntity) => new PortHubSelectionForm(new PortHubSelectionCollectionWrapper(Factory));

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.PortDepotSelectionView;

		protected override ODisplayMode GetDisplayModeForNew()
		{
			return ODisplayMode.NewSaved;
		}
	}
}
