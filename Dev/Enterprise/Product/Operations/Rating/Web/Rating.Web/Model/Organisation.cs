using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Model
{
	/// <summary>
	/// This class describes the information of each Organisation (Service Provider / Carrier) applicable to the rates.
	/// It can be used for filtering rates when included in the RateQuery class for sending request to any of the Rates API endpoints.
	/// It is also included in the Rate class to specify the Service Provider applicable to the rates responded by the Rates API.
	/// To specify an Organization, providing only one attribute is enough.
	/// </summary>
	public class Organisation
	{
		/// <summary>
		/// Organisation Code of the Service Provider / Carrier applicable to the rate.
		/// Value Reference: CargoWise > Maintain > Master Data > Organisation > Codes.
		/// </summary>
		public string CWCode { get; set; }

		/// <summary>
		/// SCAC Code of the Service Provider / Carrier applicable to the rate.
		/// Value Reference: CargoWise > Maintain > Reference Files > Shipping Lines > SCA Code.
		/// </summary>
		public string SCAC { get; set; }

		/// <summary>
		/// C1C Code of the Service Provider / Carrier applicable to the rate.
		/// Value Reference: CargoWise > Maintain > Reference Files > Shipping Lines > C1C Code.
		/// </summary>
		public string C1CCode { get; set; }

		/// <summary>
		/// Two Characters IATA Code of the Service Provider / Carrier applicable to the rate.
		/// Value Reference: CargoWise > Maintain > Reference Files > Airlines > Two Character Code.
		/// </summary>
		public string IATACode { get; set; }
	}

	internal static class OrganisationExtensions
	{
		public static OrgHeader ResolveOrganisation(this Organisation provider, BusinessObjectFactory factory)
		{
			if (!string.IsNullOrEmpty(provider?.CWCode))
			{
				return OrgHeader.LoadFromCode(factory, provider.CWCode);
			}

			if (!string.IsNullOrEmpty(provider?.IATACode))
			{
				return ResolveCW1OrganizationByIATA(factory, provider.IATACode).FirstOrDefault();
			}

			if (!string.IsNullOrEmpty(provider?.SCAC))
			{
				return ResolveCW1OrganizationBySCAC(factory, provider.SCAC).FirstOrDefault();
			}

			if (!string.IsNullOrEmpty(provider?.C1CCode))
			{
				return ResolveCW1OrganizationByC1C(factory, provider.C1CCode).FirstOrDefault();
			}

			return null;
		}

		public static OrgHeader[] ResolveOrganisations(this IEnumerable<Organisation> organisations, BusinessObjectFactory factory)
		{
			if (organisations.IsNullOrEmpty())
			{
				return Array.Empty<OrgHeader>();
			}

			var result = new HashSet<OrgHeader>();

			if (organisations.Any(p => !string.IsNullOrEmpty(p.CWCode)))
			{
				var cwCodes = organisations.Where(p => !string.IsNullOrEmpty(p.CWCode)).Select(p => p.CWCode).ToArray();

				var query = new ZDBOnlyQuery(typeof(OrgHeader));
				query.AddToFilter(OrgHeaderSchema.OH_Code, cwCodes);

				result.UnionWith(factory.Load<OrgHeader>(query));
			}

			var iataCodes = organisations.Where(p => !string.IsNullOrEmpty(p.IATACode)).Select(p => p.IATACode).ToArray();
			result.UnionWith(ResolveCW1OrganizationByIATA(factory, iataCodes));

			var scacCodes = organisations.Where(p => !string.IsNullOrEmpty(p.SCAC)).Select(p => p.SCAC).ToArray();
			result.UnionWith(ResolveCW1OrganizationBySCAC(factory, scacCodes));

			var c1cCodes = organisations.Where(p => !string.IsNullOrEmpty(p.C1CCode)).Select(p => p.C1CCode).ToArray();
			result.UnionWith(ResolveCW1OrganizationByC1C(factory, c1cCodes));

			return result.ToArray();
		}

		static OrgHeader[] ResolveCW1OrganizationByIATA(BusinessObjectFactory factory, params string[] iataCodes)
		{
			iataCodes = iataCodes?.Where(x => !string.IsNullOrEmpty(x)).ToArray();

			if (iataCodes.IsNullOrEmpty())
			{
				return Array.Empty<OrgHeader>();
			}

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			var refAirLineSubQuery = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
			refAirLineSubQuery.AddToFilter(RefAirlineSchema.RM_TwoCharacterCode, iataCodes);
			subQuery.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, refAirLineSubQuery, JoinCondition.And);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return factory.Load<OrgHeader>(query);
		}

		static OrgHeader[] ResolveCW1OrganizationBySCAC(BusinessObjectFactory factory, params string[] scacCodes)
		{
			scacCodes = scacCodes?.Where(x => !string.IsNullOrEmpty(x)).ToArray();

			if (scacCodes.IsNullOrEmpty())
			{
				return Array.Empty<OrgHeader>();
			}

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK);
			subQuery.AddToFilter(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, scacCodes);
			query.AddSubQuery(OrgHeaderSchema.OH_RSL_ShippingLine, subQuery, JoinCondition.And);

			return factory.Load<OrgHeader>(query);
		}

		static OrgHeader[] ResolveCW1OrganizationByC1C(BusinessObjectFactory factory, params string[] c1cCodes)
		{
			c1cCodes = c1cCodes?.Where(x => !string.IsNullOrEmpty(x)).ToArray();

			if (c1cCodes.IsNullOrEmpty())
			{
				return Array.Empty<OrgHeader>();
			}

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(RefShippingLine), RefShippingLineSchema.PK);
			subQuery.AddToFilter(RefShippingLineSchema.RSL_CargoWiseOneCode, c1cCodes);
			query.AddSubQuery(OrgHeaderSchema.OH_RSL_ShippingLine, subQuery, JoinCondition.And);

			return factory.Load<OrgHeader>(query);
		}
	}
}
