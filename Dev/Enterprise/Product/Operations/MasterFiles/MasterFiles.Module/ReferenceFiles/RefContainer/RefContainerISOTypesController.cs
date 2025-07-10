using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefContainerISOTypesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefContainerISOTypes; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefContainerISOTypes; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ContainerISOType); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ContainerISOType();
		}
	}
}
