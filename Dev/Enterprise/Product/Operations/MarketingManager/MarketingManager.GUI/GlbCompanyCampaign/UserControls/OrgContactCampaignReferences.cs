using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.GUI
{
	public class OrgContactCampaignReferences : NonPersistentBusinessObject
	{
		public OrgContactCampaignReferences(OrgContact contact)
		{
			Argument.NotNull(contact, "contact");

			Contact = contact;
		}

		public readonly OrgContact Contact;

		public GlbCompanyCampaignItem CampaignItem;
		public HashSet<IRelatableActivity> CampaignChildren = new HashSet<IRelatableActivity>();

		#region Properties

		public ZString ContactName
		{
			get { return Contact.OC_ContactName; }
		}

		public ZBool HasPostRelations
		{
			get
			{
				return
					(CampaignItem != null && CampaignItem.RelatedChildActivityPivotCollection.Activities.Any()) ||
					(CampaignChildren.Count > 0);
			}
		}

		#endregion
	}
}
