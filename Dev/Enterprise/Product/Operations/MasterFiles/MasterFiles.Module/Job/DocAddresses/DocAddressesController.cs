using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	/// <summary>
	/// Module Controller for DocAddresses.
	/// </summary>
	public class DocAddressesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public DocAddressesController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.DocAddresses; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { throw new ModuleGuiNotSupportedException("I'm for plug in only baby!"); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.MasterFiles.Module.Res.GetData("PlugInTabPage|DocAddresses", "Addresses", "The Addresses tab."); } }

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new DocAddressesPlugIn(businessEntity);
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("I'm for plug in only baby!");
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		#region Security Checkpoints

		// TODO: Check this is correct security

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.Organisation; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.Organisation; }
		}

		#endregion
	}
}
