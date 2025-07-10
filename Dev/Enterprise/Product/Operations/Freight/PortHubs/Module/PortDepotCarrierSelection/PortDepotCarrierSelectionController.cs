using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.PortHubs.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.PortHubs.Module
{
	public class PortDepotCarrierSelectionController : ZController
	{
		public override ControllerID ID => ControllerIDs.PortDepotCarrierSelection;

		public override ModuleIdentifier ModuleID => ModuleIDs.PortDepotCarrierSelection;

		public override Type TypeOfTopLevelBusinessObject => typeof(Business.PortHubSelection);

		protected override IZForm GetForm(IBusiness businessEntity) => new PortDepotCarrierSelectionDetailsForm((Business.PortHubSelection)businessEntity);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		#region Security

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.PortDepotSelectionView;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.PortDepotSelectionView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.PortDepotSelectionModify;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.PortDepotSelectionModify;

		#endregion
	}
}
