using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class UpdateableMilestoneEventsHelper
	{
		#region Constructors

		public UpdateableMilestoneEventsHelper(OrgContactWebUser siteUser)
		{
			this.siteUser = siteUser;
		}

		#endregion

		#region Methods

		public static List<string> GetUpdateableMilestoneEvents(MilestoneEventUpdatesCollection eventUpdatesSettings, WebPartyTypeOrgPairCollection webParties, ZGuid loggedInOrganisationPK)
		{
			var result = new List<string>();
			foreach (var webPartyKey in webParties.WebPartyTypes)
			{
				foreach (var webPartyOrg in webParties[webPartyKey])
				{
					if (webPartyOrg.PK == loggedInOrganisationPK)
					{
						result.AddRange(eventUpdatesSettings.GetUpdateableEventCodes(webPartyKey));
					}
				}
			}
			return result;
		}

		public List<string> GetUpdateableMilestoneEvents(MilestoneEventUpdatesCollection eventUpdatesSettings, WebPartyTypeOrgPairCollection webParties)
		{
			if (SiteUser != null && SiteUser.LoggedInOrganisation != null)
			{
				return GetUpdateableMilestoneEvents(eventUpdatesSettings, webParties, SiteUser.LoggedInOrganisation.PK);
			}
			return new List<string>();
		}

		#endregion

		#region Implementation

		protected OrgContactWebUser SiteUser
		{
			get { return siteUser; }
		}

		readonly OrgContactWebUser siteUser;

		#endregion
	}
}
