using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class NewsAndAnnouncementModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.NewsAndAnnouncement);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new NewsAndAnnouncementFilterControl(GridCollection, (NewsAndAnnouncementFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new NewsAnnouncementCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NewsAndAnnouncementFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.NewsAnnouncements; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.NewsAndAnnouncement; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}
	}
}
