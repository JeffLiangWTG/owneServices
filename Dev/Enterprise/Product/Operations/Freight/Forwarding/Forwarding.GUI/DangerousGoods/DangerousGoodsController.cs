using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.GUI.DangerousGoods
{
	public class DangerousGoodsController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("This controller does not have a GUI.");

		public override ControllerID ID => ControllerIDs.DangerousGoodsPlugin;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => throw new ModuleGuiNotSupportedException("Not supported");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var businessEntityTypeToGlowEndPointMapping = new Dictionary<string, string>()
			{
				{ typeof(ForwardingShipment).FullName, "Goto/ManageDangerousGoodsForShipments" }
			};

			var urlGenerator = new UrlGenerator(businessEntityTypeToGlowEndPointMapping);
			var plugin = new DangerousGoodsPlugin(businessEntity, urlGenerator);
			return plugin;
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
	}
}
