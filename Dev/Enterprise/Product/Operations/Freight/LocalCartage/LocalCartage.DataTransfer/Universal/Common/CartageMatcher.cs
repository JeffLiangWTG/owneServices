using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal
{
	class CartageMatcher : CombinationKeyMatcher<CommonCartage, CartageReferences>
	{
		public CartageMatcher(BusinessObjectFactory factory, CartageReferences references, IXmlImportLogger logger)
			: base(factory, references, logger) { }

		protected override bool CheckLatestParent(CommonCartage cartage, CommonCartage cartageToCompare)
		{
			return cartage.Logs.AddedLog.SL_EventTime > cartageToCompare.Logs.AddedLog.SL_EventTime;
		}

		protected override void BuildMatchingQueryAndMatchDelegates(CartageReferences refParent)
		{
			AddConnoteNumberMatch(refParent);
			AddClientReferenceMatch(refParent);
			AddWaybillNumberMatch(refParent);
			AddQuoteNumberMatch(refParent);
		}

		void AddClientReferenceMatch(CartageReferences refParent)
		{
			if (!refParent.ClientOrderNumber.IsEmpty)
			{
				var matchDelegates = new List<MatchDelegate>();
				var cartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
				var clientReferenceQuery = GetClientReferenceQuery(refParent.ClientOrderNumber);
				cartageQuery.AddToFilter(clientReferenceQuery, JoinCondition.And);

				matchDelegates.Add(c => GetMatchCount(c.ClientReference, refParent.ClientOrderNumber));

				if (!refParent.ClientOrganization.IsEmpty)
				{
					var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
					addressQuery.AddToFilter(OrgAddressSchema.OA_OH, refParent.ClientOrganization);

					var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
					jobHeaderQuery.AddSubQuery(addressQuery, JoinCondition.And);

					cartageQuery.AddSubQuery(jobHeaderQuery, JoinCondition.And);
					matchDelegates.Add(c => GetMatchCount(c.LocalClientPK, refParent.ClientOrganization));
				}

				AddCombinedPossibleMatch(matchDelegates, cartageQuery);
			}
		}

		static ZDBOnlyQuery GetClientReferenceQuery(ZString clientOrderNumber)
		{
			var additionalRefCartageQuery = GetAdditionalReferenceCartageQuery(clientOrderNumber);
			var orderRefCartageQuery = GetNoAdditionalReferenceQuery(clientOrderNumber);

			var cartageRefQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			cartageRefQuery.AddToFilter(additionalRefCartageQuery, JoinCondition.Or);
			cartageRefQuery.AddToFilter(orderRefCartageQuery, JoinCondition.Or);

			return cartageRefQuery;
		}

		static ZDBOnlyQuery GetAdditionalReferenceCartageQuery(ZString clientOrderNumber)
		{
			var cartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			var additionalRefSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			additionalRefSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, TransportAdditionalReferenceTypes.Codes.TransportReference);
			additionalRefSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, clientOrderNumber);
			cartageQuery.AddSubQuery(additionalRefSubQuery, JoinCondition.And);
			return cartageQuery;
		}

		static ZDBOnlyQuery GetNoAdditionalReferenceQuery(ZString clientOrderNumber)
		{
			var cartageQuery = new ZDBOnlyQuery(typeof(CommonCartage));
			var noAdditionalRefSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, true);
			noAdditionalRefSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, TransportAdditionalReferenceTypes.Codes.TransportReference);
			cartageQuery.AddSubQuery(noAdditionalRefSubQuery, JoinCondition.And);
			cartageQuery.AddToFilter(JobCartageSchema.JJ_OrderReferenceNumber, clientOrderNumber);
			return cartageQuery;
		}

		void AddCombinedPossibleMatch(List<MatchDelegate> matchDelegates, ZQuery query)
		{
			if (matchDelegates.Count > 0)
			{
				MatchDelegate fullMatch = cartage => { return matchDelegates.TrueForAll(m => m(cartage) == 1) ? 1 : 0; };
				AddPossibleMatch(query, fullMatch);
			}
		}

		void AddConnoteNumberMatch(CartageReferences refParent)
		{
			AddPossibleMatch(JobCartageSchema.JJ_ConsignmentID, refParent.ConnoteNumber, cartage => GetMatchCount(cartage.JJ_ConsignmentID, refParent.ConnoteNumber));
		}

		void AddWaybillNumberMatch(CartageReferences refParent)
		{
			AddFallbackMatch(refParent.WaybillNumber, cartage => GetMatchCount(cartage.JJ_WaybillNumber, refParent.WaybillNumber));
		}

		void AddQuoteNumberMatch(CartageReferences refParent)
		{
			AddFallbackMatch(refParent.QuoteNumber, cartage => GetMatchCount(cartage.JJ_QuoteNumber, refParent.QuoteNumber));
		}

		protected override void BuildFallbackMatchDelegates(CartageReferences referencesParent)
		{
			AddFallbackMatch(referencesParent.References, cartage => GetMatchCount(cartage.AdditionalReferenceNumbers, CusEntryNumSchema.CE_EntryType, CusEntryNumSchema.CE_EntryNum, referencesParent.References));
		}
	}
}
