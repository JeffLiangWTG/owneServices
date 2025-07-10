using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Module
{
	public class PackingController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Packing; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Packing; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(PkgPackageJob); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new PackingForm((PkgPackageJob)businessEntity);
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return PkgPackageJob.LoadPackageJob(factory, sourceEntityPK);
		}

		#region Security CheckPoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.PackingView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.PackingEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		#endregion
	}
}
