using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Freight.Forwarding.Module
{
	public class UniversalDataCarrierMessagingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new UniversalDataCarrierMessagingPlugIn((ForwardingConsol)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.UniversalDataCarrierMessaging; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This is a plugin only");
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.MaintainConsol; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.MaintainConsolEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.MaintainConsolNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.MaintainConsolDelete; }
		}
	}
}
