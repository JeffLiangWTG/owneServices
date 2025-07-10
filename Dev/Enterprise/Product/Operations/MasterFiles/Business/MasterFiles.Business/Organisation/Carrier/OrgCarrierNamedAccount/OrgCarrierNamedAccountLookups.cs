//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCarrierNamedAccountLookups
//
//    This class should be used for overriding collections in AutoOrgCarrierNamedAccountLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Rating;
using Enterprise.ZArchitecture.Core;
using WiseRates.Api.Client;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierNamedAccountLookups : AutoOrgCarrierNamedAccountLookups
	{
		public OrgCarrierNamedAccountLookups(AutoOrgCarrierNamedAccount parent) : base(parent)
		{
		}

		#region Foreign Names

		public UntranslatableCodeDescriptionPairList ForeignNames
		{
			get
			{
				var namedAccounts = new UntranslatableCodeDescriptionPairList((NoResString)"Named account values cannot be translated");
				var namedAccountsList = GetCachedNamedAccountsList(Factory);
				foreach (var account in namedAccountsList)
				{
					namedAccounts.AddPair(account);
				}
				return namedAccounts;
			}
		}

		public const string NamedAccountsCacheKey = "UrsNamedAccountsList";

		public static HashSet<string> GetCachedNamedAccountsList(BusinessObjectFactory factory)
			=> factory.GetCachedValue(NamedAccountsCacheKey, GetNamedAccountsList);

		public static HashSet<string> GetNamedAccountsList()
		{
			var requestID = WiseRatesClient.GenerateTraceID();
			var clientFactory = ObjectFactory.Get<IWiseRatesClientFactory>();

			var (client, _) = clientFactory.TryCreate(requestID);
			if (client != null)
			{
				return client.GetNamedAccounts(requestID);
			}
			return new HashSet<string>();
		}

		#endregion

		public OrganisationsFindBoxCollection ConsigneeOrConsignorOrControllingCustomerCollection => new ConsigneeOrConsignorOrControllingCustomerCollection(Factory);
	}
}
