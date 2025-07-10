using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching
{
	public class OrganisationName : StringWithLanguage
	{
		public OrganisationName(string value, string languageCode)
			: base(value, languageCode)
		{
		}

		public ZGuid OrgAddressPK { get; set; }
	}
}
