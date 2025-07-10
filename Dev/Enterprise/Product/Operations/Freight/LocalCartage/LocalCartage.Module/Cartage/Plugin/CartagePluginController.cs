using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartagePluginController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TransportJobDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TransportJobEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TransportJobNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TransportJob; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("Not supported");
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			var cartageParent = businessEntity as ICartageParent;
			return cartageParent == null
				? throw new NotSupportedException("BusinessEntity does not implement ICartageParent")
				: (ZPlugIn)new CartagePlugin(cartageParent);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CartagePlugin; }
		}

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(factory, new CartageBehaviorStrategyProvider());
		}
	}
}
