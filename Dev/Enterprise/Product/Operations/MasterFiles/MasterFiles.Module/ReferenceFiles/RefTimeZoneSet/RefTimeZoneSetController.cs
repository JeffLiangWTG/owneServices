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
	public class RefTimeZoneSetController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public RefTimeZoneSetController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.RefTimeZoneSet; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.RefTimeZoneSet; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(RefTimeZoneSet); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new RefTimeZoneSetForm((RefTimeZoneSet)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.TimeZoneSetDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.TimeZoneSetEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.TimeZoneSetNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.TimeZoneSetView; }
		}
	}
}
