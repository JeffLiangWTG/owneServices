using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.Module
{
	public class ForwardingConsolToNZECIManifestSyncroniserController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ForwardingConsolToNZECIManifestSyncroniserController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.NZ.ECIWriteOffManifestingConsolSynchroniser; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ForwardingConsol); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new PlugIn((ForwardingConsol)businessEntity);
		}

		public class PlugIn : ZAlwaysLoadPlugIn
		{
			public PlugIn(ForwardingConsol consol)
				: base(consol)
			{
				consol.Syncroniser = new ForwardingConsolToNZECIManifestSyncroniser();
			}

			protected override ZBool HasUserControl
			{
				get { return ZBool.False; }
			}

			protected override LicenceCheckpoint LicenceCheckPoint
			{
				get { return Env.Licence.Core; }
			}

			public override string Name
			{
				get { return "Consol to ECI Manifest Syncroniser"; }
			}
		}
	}
}
