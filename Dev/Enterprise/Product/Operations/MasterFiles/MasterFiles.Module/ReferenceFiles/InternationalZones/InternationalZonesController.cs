using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class InternationalZonesController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.InternationalZone; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.InternationalZone; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefZoneHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefZoneHeaderForm((RefZoneHeader)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ZoneDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ZoneModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ZoneNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ZoneView; }
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!sourceEntity.CanDelete)
			{
				Globals.Message.ShowError(sourceEntity.ReasonForNotAbleToDelete);
				return null;
			}
			return base.ShowDeleteForm(sourceEntity);
		}
	}
}
