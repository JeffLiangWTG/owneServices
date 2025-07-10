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
	public class RefDomesticCartageZoneController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefDomesticCartageZone; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefDomesticCartageZone; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefDomesticCartageZone); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefDomesticCartageZoneForm((RefDomesticCartageZone)businessEntity);
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("");
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.RefDomesticCartageZoneModify; }
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
	}
}
