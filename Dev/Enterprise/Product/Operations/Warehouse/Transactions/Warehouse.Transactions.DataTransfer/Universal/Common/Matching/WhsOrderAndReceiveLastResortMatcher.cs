using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsOrderAndReceiveLastResortMatcher<TDocket> : WhsDocketLastResortMatcher<TDocket, WhsOrderAndReceiveReferences>
		where TDocket : WhsDocket
	{
		protected WhsOrderAndReceiveLastResortMatcher(BusinessObjectFactory factory, WhsOrderAndReceiveReferences referencesParent, IXmlImportLogger logger)
			: base(factory, referencesParent, logger)
		{
		}

		protected override void BuildMatchingQueryAndMatchDelegates(WhsOrderAndReceiveReferences referencesParent)
		{
			var matchDelegates = new List<MatchDelegate>();
			var query = new ZQuery();

			var externalReference = referencesParent.ExternalReference;
			if (!externalReference.IsEmpty)
			{
				query.AddToFilter(WhsDocketSchema.WD_ExternalReference, externalReference);
				matchDelegates.Add(whsDocket => GetMatchCount(whsDocket.WD_ExternalReference, externalReference));

				var externalReferenceSplit = referencesParent.ExternalReferenceSplit;
				query.AddToFilter(WhsDocketSchema.WD_ExternalReferenceSplit, externalReferenceSplit);
				matchDelegates.Add(whsDocket => GetMatchCount(whsDocket.WD_ExternalReferenceSplit, externalReferenceSplit));
			}

			var clientReference = referencesParent.ClientReference;
			if (!clientReference.IsEmpty)
			{
				query.AddToFilter(WhsDocketSchema.WD_CustomerReference, clientReference);
				matchDelegates.Add(whsDocket => GetMatchCount(whsDocket.WD_CustomerReference, referencesParent.ClientReference));
			}

			var clientOrganization = referencesParent.ClientOrganization;
			if (!clientOrganization.IsEmpty && (!externalReference.IsEmpty || !clientReference.IsEmpty))
			{
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, clientOrganization);
				matchDelegates.Add(whsDocket => GetMatchCount(whsDocket.WD_OH_Client, clientOrganization));
			}

			BuildFullMatchDelegate(matchDelegates, query);
		}

		protected override void BuildFallbackMatchDelegates(WhsOrderAndReceiveReferences referencesParent)
		{
			AddFallbackMatch(referencesParent.TransportReference, whsDocket => GetMatchCount(whsDocket.WD_TransportReference, referencesParent.TransportReference));
			AddAdditionalReferencesFallback(referencesParent);
		}
	}
}
