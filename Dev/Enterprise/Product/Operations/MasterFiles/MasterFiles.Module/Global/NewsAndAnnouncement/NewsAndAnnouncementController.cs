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
	public class NewsAndAnnouncementController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.NewsAndAnnouncement; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.NewsAndAnnouncement; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NewsAnnouncement); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new NewsAndAnnouncementForm((NewsAnnouncement)businessEntity);
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.NewsAnnouncementsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.NewsAnnouncementsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewsAnnouncementsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.NewsAnnouncementsView; }
		}

		#endregion
	}
}
