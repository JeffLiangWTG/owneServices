using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgWebURLDependentCollection : DependentBusinessObjectCollection<OrgWebURL, OrgHeader>
	{
		public OrgWebURLDependentCollection(OrgHeader organisation)
			: base(organisation)
		{
		}

		#region AddNew

		public OrgWebURL AddNew(string urlType)
		{
			OrgWebURL result = AddNew();
			result.PU_Type = urlType;
			return result;
		}

		#endregion

		#region FindByUrlType

		public OrgWebURL[] FindByUrlType(string urlType)
		{
			return (OrgWebURL[])Find(new ZQuery(OrgWebURLSchema.PU_Type, urlType));
		}

		#endregion

		#region Main url

		public OrgWebURL MainURL
		{
			get
			{
				if (mainURLCache == null)
				{
					mainURLCache = new CachedProperty<OrgWebURL>(Factory, GetOrCreateMainURL);
				}
				return mainURLCache.Value;
			}
		}
		CachedProperty<OrgWebURL> mainURLCache;

		OrgWebURL GetOrCreateMainURL()
		{
			OrgWebURL url = GetMainUrl();
			if (url == null && !((IBusinessObjectCollectionInternals)this).MastersAreDeleted)
			{
				url = AddNew();
				SetMainProperties(url);
			}
			return url;
		}

		OrgWebURL GetMainUrl()
		{
			return this.Cast<OrgWebURL>().FirstOrDefault(url => (!url.IsDeleted && url.PU_IsPrimary));
		}

		#endregion
		#region SetDefaultsForNewChild

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Count == 0)
			{
				OrgWebURL element = (child as OrgWebURL);
				SetMainProperties(element);
			}
		}

		void SetMainProperties(OrgWebURL element)
		{
			if (element != null)
			{
				element.SuspendSettingMarkParentAsNeedingValidation();
				element.PU_IsPrimary = true;
				element.PU_Description = (NoResString)"Main Website";
				element.PU_Type = OrgWebUrlList.Codes.MainWebsite;
				element.MainDefaultAdded = true;
				element.ResumeSettingMarkParentAsNeedingValidation();
			}
		}

		#endregion
	}
}
