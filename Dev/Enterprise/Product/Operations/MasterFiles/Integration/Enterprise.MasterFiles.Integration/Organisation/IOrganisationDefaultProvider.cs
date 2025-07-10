using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IOrganisationDefaultProvider
	{
		bool ShouldSetValuesFromConditionalDefaults { get; set; }
		OrganisationTypes OrganisationType { get; set; }
		ZString OrganisationSubType { get; set; }
		ZString DocAddressType { get; set; }
		List<IOrgFieldDefault> ConditionalDefaults { get; }
	}
}
