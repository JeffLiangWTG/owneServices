using System.Linq;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationViewStmNumsCollection : ViewStmNumsCollection<OrganisationViewStmNums>
	{
		public OrganisationViewStmNumsCollection(OrgHeader header) : base(header, OrganisationViewStmNums.Schema.SN_NamePrefix)
		{
		}

		protected OrgHeader Header => Owner as OrgHeader;

		public INumberFountainProxy TryGetNumberFountainByZoneID(string zoneID, string fountainTypeCode)
		{
			var matchedStmNum = this.FirstOrDefault(viewStmNum => (viewStmNum.SN_Type == fountainTypeCode) && viewStmNum.SN_ZoneIDPrefix == zoneID);
			return matchedStmNum?.TryGetNumberFountain();
		}

		public CodeDescriptionPairList GetFTZPrefixList(string fountainTypeFtzOrFtw, int maximumLength)
		{
			var result = new CodeDescriptionPairList();
			var matchedStmNums = this.Find(viewStmNum => viewStmNum.SN_Type == fountainTypeFtzOrFtw);
			if (matchedStmNums != null)
			{
				foreach (var stmNum in matchedStmNums.Where(x => x.SN_ZoneIDPrefix.Length <= maximumLength))
				{
					result.AddPair(stmNum.SN_ZoneIDPrefix, stmNum.SN_ClientPrefix);
				}
			}
			return result;
		}
	}
}
