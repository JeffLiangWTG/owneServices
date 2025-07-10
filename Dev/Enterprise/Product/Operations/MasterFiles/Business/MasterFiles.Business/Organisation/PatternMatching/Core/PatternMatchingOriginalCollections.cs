using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public class PatternMatchingOriginalCollections : IMatchingOrganisationCollections
	{
		public IEnumerable<IMatchingAddress> Addresses { get; set; }
		public IEnumerable<IMatchingCusCode> CustomsCodes { get; set; }
		public IEnumerable<OrganisationName> OrganisationNamesExceptBrands { get; set; }
		public IEnumerable<OrganisationName> OrganisationNamesFromBrands { get; set; }

		internal void SetDefaultValues()
		{
			OrganisationNamesExceptBrands = new List<OrganisationName>();
			OrganisationNamesFromBrands = new List<OrganisationName>();
			Addresses = new List<IMatchingAddress>();
			CustomsCodes = new List<IMatchingCusCode>();
		}
	}
}
