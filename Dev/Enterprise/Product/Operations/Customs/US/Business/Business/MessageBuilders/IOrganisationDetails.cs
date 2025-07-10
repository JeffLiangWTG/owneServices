using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IOrganisationDetails : IAddressDetails
	{
		ZString UserFriendlyPath { get; }
		ZString MatchedCustomsRegoNumber { get; }
	}

	public interface ISimplifiedEntryOrganisationDetails : IAddressDetails
	{
		ZString EntityCode { get; set; }
		ZString EntityIdentifierQualifier { get; }
		ZString EntityIdentifier { get; }

		IEnumerable<(ZString IdentifierType, ZString Identifier)> GlobalBusinessIdentifiers { get; }
	}
}
