using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Tracking.Business
{
	public class WebPartyTypeOrgPairCollection
	{
		public void Add(string webPartyType, JobDocAddress docAddress)
		{
			if (docAddress != null)
			{
				Add(webPartyType, docAddress.Organisation);
			}
		}

		public void Add(string webPartyType, OrgHeader org)
		{
			if (org != null)
			{
				if (Data.ContainsKey(webPartyType))
				{
					if (!Data[webPartyType].Contains(org))
					{
						Data[webPartyType].Add(org);
					}
				}
				else
				{
					Data.Add(webPartyType, new List<OrgHeader> { org });
				}
			}
		}

		public List<OrgHeader> this[string webPartyType]
		{
			get
			{
				if (Data.ContainsKey(webPartyType))
				{
					return Data[webPartyType];
				}
				return new List<OrgHeader>();
			}
		}

		public Dictionary<string, List<OrgHeader>>.KeyCollection WebPartyTypes
		{
			get { return Data.Keys; }
		}

		protected Dictionary<string, List<OrgHeader>> Data
		{
			get { return data ?? (data = new Dictionary<string, List<OrgHeader>>()); }
		}

		Dictionary<string, List<OrgHeader>> data;
	}
}
