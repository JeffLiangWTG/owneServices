using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbCapabilityController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CapabilityDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CapabilityEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CapabilityNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Capability; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbCapabilityForm((GlbCapability)businessEntity);
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.GlbCapability; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbCapability); }
		}

		public override ModuleIdentifier ModuleID
		{
			get
			{
				return ModuleIDs.GlbCapability;
			}
		}
	}
}
