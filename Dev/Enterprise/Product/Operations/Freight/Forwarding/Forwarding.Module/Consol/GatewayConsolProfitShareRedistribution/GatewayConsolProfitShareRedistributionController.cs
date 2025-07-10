using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module
{
	/// <summary>
	/// Module to search for Gateway Consol Profit Share Redistribution Batches or create a new one.
	/// Ideally it should extend ZSingletonController but doing so does not give us the power to control SecurityCheckpoint CheckPointForView.
	/// </summary>
	public class GatewayConsolProfitShareRedistributionController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GatewayProfitShareRedistributionForm((ForwardingProfitShareRedistribution)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.GatewayConsolProfitShareRedistribution;

		public override ModuleIdentifier ModuleID => ModuleIDs.GatewayConsolProfitShareRedistribution;

		public override Type TypeOfTopLevelBusinessObject => typeof(ForwardingProfitShareRedistribution);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GatewayConsolProfitShareRedistributionNew;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GatewayConsolProfitShareRedistributionView;

		/// <summary>
		/// There is no edit function. Let's restrict it.
		/// </summary>
		protected override SecurityCheckpoint CheckPointForEdit => new DeniedSecurityCheckpoint();

		/// <summary>
		/// There is no delete function. Let's restrict it.
		/// </summary>
		protected override SecurityCheckpoint CheckPointForDelete => new DeniedSecurityCheckpoint();
	}
}
