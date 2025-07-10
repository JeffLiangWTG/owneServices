using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MasterFiles.Module
{
	public class ExporterSchemeController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public ExporterSchemeController()
		{
		}

		public sealed override ControllerID ID
		{
			get { return ControllerIDs.ExporterScheme; }
		}

		public sealed override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(OrgHeader); }
		}

		protected sealed override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return GetPlugInCore(businessEntity);
		}

		protected virtual ZPlugIn GetPlugInCore(IBusiness businessEntity)
		{
			return new ExporterSchemePlugin((OrgHeader)businessEntity);
		}

		protected sealed override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This plugin does not support access outside the organisation form.");
		}

		public override ModuleIdentifier ModuleID
		{
			get { throw new ModuleGuiNotSupportedException("Not supported"); }
		}

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OrgConsignorModifyExporterScheme; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OrgConsignorModifyExporterScheme; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OrgConsignorModifyExporterScheme; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.Organisation; }
		}

		#endregion
	}
}
