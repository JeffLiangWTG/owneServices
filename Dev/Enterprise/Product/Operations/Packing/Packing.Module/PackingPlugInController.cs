using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Packing.Module
{
	public class PackingPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity) => throw new NotSupportedException();

		public override ControllerID ID => ControllerIDs.PackingPlugIn;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");

		public override Type TypeOfTopLevelBusinessObject => typeof(PkgPackageJob);

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK) => PkgPackageJob.LoadPackageJob(factory, sourceEntityPK);

		public override ResourceStringData PluginTabPageCaption => Res.GetData("PlugInTabPage|Packing", "Packing");

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new PackingPlugIn(businessEntity);

		#region Security CheckPoints

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		#endregion
	}
}
