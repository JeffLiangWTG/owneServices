using System.Collections.Generic;

namespace Enterprise.Tracking.Business
{
	public class EventReferenceHelper
	{
		#region Constructors

		public EventReferenceHelper(TrackingSiteUser siteUser)
		{
			this.siteUser = siteUser;
		}

		#endregion

		#region Methods

		public string GetEventReferences(WebPartyTypeOrgPairCollection webParties)
		{
			var result = new List<string>();
			if (SiteUser != null && SiteUser.LoggedInOrganisation != null)
			{
				foreach (var webPartyKey in webParties.WebPartyTypes)
				{
					foreach (var webPartyOrg in webParties[webPartyKey])
					{
						if (webPartyOrg.PK == SiteUser.LoggedInOrganisation.PK)
						{
							result.Add(webPartyKey);
						}
					}
				}
			}
			return string.Join(",", result);
		}

		#endregion

		#region Implementation

		protected TrackingSiteUser SiteUser
		{
			get { return siteUser; }
		}

		readonly TrackingSiteUser siteUser;

		#endregion
	}
}
