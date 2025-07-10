using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.Business
{
	public static class CartageQueryHelper
	{
		/// <summary>
		/// Returns a ZDBOnlyQuery with JobCartage being the Top Level Type
		/// </summary>
		/// <param name="organisationPK"></param>
		/// <returns></returns>
		public static ZQuery GetLocalClientQuery(ZGuid organisationPK)
		{
			ZDBOnlySubQuery addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			addressQuery.AddToFilter(OrgAddressSchema.OA_OH, organisationPK);

			ZDBOnlyQuery headerQuery = new ZDBOnlyQuery(typeof(JobHeader));
			headerQuery.AddSubQuery(addressQuery, JoinCondition.And);

			return GetJobHeaderQuery(headerQuery);
		}

		public static ZDBOnlyQuery GetJobHeaderQuery(ZQuery headerQuery)
		{
			ZDBOnlyQuery jobCartageDBOnlyQuery = new ZDBOnlyQuery(typeof(CommonCartage));

			ZDBOnlySubQuery jobCartageJobHeader = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobCartageJobHeader.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			jobCartageJobHeader.AddToFilter(headerQuery);

			jobCartageDBOnlyQuery.AddSubQuery(jobCartageJobHeader, JoinCondition.And);

			return jobCartageDBOnlyQuery;
		}

		public static ZDBOnlyQuery GetDocAddressOnlyQuery(ZGuid organisationPK, DocAddressType docAddressType, BusinessObjectFactory factory)
		{
			return GetDocAddressOnlyQuery(organisationPK, new DocAddressType[] { docAddressType }, factory);
		}

		public static ZDBOnlyQuery GetDocAddressOnlyQuery(ZGuid organisationPK, DocAddressType[] docAddressTypes, BusinessObjectFactory factory)
		{
			return GetDocAddressOnlyQuery(null, OrgAddressSchema.OA_OH, organisationPK, SQLComparisonOperator.Equal, docAddressTypes, factory);
		}

		public static ZDBOnlyQuery GetDocAddressOnlyQuery(SchemaColumn docAddressColumn, SchemaColumn orgAddressColumn, IZType value, SQLComparisonOperator comparisonOperator, DocAddressType docAddressType, BusinessObjectFactory factory)
		{
			return GetDocAddressOnlyQuery(docAddressColumn, orgAddressColumn, value, comparisonOperator, new DocAddressType[] { docAddressType }, factory);
		}

		public static ZDBOnlyQuery GetDocAddressOnlyQuery(SchemaColumn docAddressColumn, SchemaColumn orgAddressColumn, IZType value, SQLComparisonOperator comparisonOperator, DocAddressType[] docAddressTypes, BusinessObjectFactory factory)
		{
			List<ZString> typeCodes = new List<ZString>();
			foreach (DocAddressType docAddressType in docAddressTypes)
			{
				typeCodes.Add(DocAddressTypes.GetCode(factory, docAddressType));
			}

			ZDBOnlyQuery docAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			if (typeCodes.Count > 0)
			{
				docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, typeCodes);
			}

			if (orgAddressColumn != null)
			{
				ZDBOnlySubQuery orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressQuery.AddToFilter(orgAddressColumn, comparisonOperator, value);
				ZDBOnlyQuery docAddressWithOrgAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
				docAddressWithOrgAddressQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
				docAddressQuery.AddToFilter(docAddressWithOrgAddressQuery, JoinCondition.Or);
			}

			if (docAddressColumn != null)
			{
				ZDBOnlyQuery docAddressOverridenQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
				docAddressOverridenQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
				docAddressOverridenQuery.AddToFilter(docAddressColumn, comparisonOperator, value);
				docAddressQuery.AddToFilter(docAddressOverridenQuery, JoinCondition.Or);
			}

			return docAddressQuery;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Don't have access to the alternatives it wants")]
		public static ZQuery GetConsignmentIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			//TODO: Needs to work with multiple values.
			ZQuery result = new ZQuery();
			if (@operator == SQLComparisonOperator.Equal)
			{
				if (value.StartsWith("T", StringComparison.Ordinal) || value.StartsWith("S", StringComparison.Ordinal)
					|| value.StartsWith("L", StringComparison.Ordinal) || value.StartsWith("H", StringComparison.Ordinal)
					|| value.StartsWith("B", StringComparison.Ordinal))
				{
					ZString jobPrefix = value.Left(1);
					value = value.SubstringSafe(1);
					if (value.IsEmpty)
					{
						result.AddToFilter(JoinCondition.And, JobCartageSchema.JJ_ConsignmentID, SQLComparisonOperator.StartsWith, jobPrefix);
					}
					else if (value.Length <= 8 && value.IsNumbersOnlyOrEmpty)
					{
						value = value.PadLeft(8, '0');
						result.AddToFilter(JoinCondition.And, JobCartageSchema.JJ_ConsignmentID, SQLComparisonOperator.Equal, jobPrefix + value);
					}
				}
				else if (value.Length <= 8 && value.IsNumbersOnlyOrEmpty)
				{
					value = value.PadLeft(8, '0');
					IEnumerable[] jobNumbers = new IEnumerable[] { 'T' + value, 'S' + value, 'L' + value, 'H' + value, 'B' + value };
					result.AddToFilter(new ZQuery(JobCartageSchema.JJ_ConsignmentID, SQLComparisonOperator.Equal, jobNumbers));
				}
			}

			if (result.IsEmpty)
			{
				result.AddToFilter(JobCartageSchema.JJ_ConsignmentID, @operator, value);
			}
			return result;
		}

		public static ZQuery GetCartageParentJobTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var parentTypeSubQueryClause = value.EqualsIgnoringCase(CartageBindToLists.StandAloneCartageCode)
				? new ZQuery(JobCartageSchema.JJ_ParentID, null)
				: new ZQuery(ViewJobCartageParentsSchema.VCP_JobType, comparisonOperator, value);
			return GetCartageParentSubQuery(parentTypeSubQueryClause);
		}

		public static ZQuery GetCartageParentJobNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			var notIn = IsNotIn(@operator);
			@operator = NotInComparisonOperator(@operator);
			var result = new ZDBOnlyQuery(typeof(CommonCartage));
			result.IgnoreActiveFilter = true;

			var topSubQuery = new ZDBOnlySubQuery(typeof(CommonCartage), JobCartageSchema.PK, notIn);
			topSubQuery.AddToFilter(GetCartageParentSubQuery(new ZQuery().AddToFilter_PossiblyCommaSeparated(ViewJobCartageParentsSchema.VCP_JobNumber, @operator, value)), JoinCondition.And);

			result.AddSubQuery(topSubQuery, JoinCondition.And);

			return result;
		}

		static bool IsNotIn(SQLComparisonOperator comparisonOperator)
		{
			return
				comparisonOperator == SQLComparisonOperator.NotContains ||
				comparisonOperator == SQLComparisonOperator.NotEqual ||
				comparisonOperator == SQLComparisonOperator.DoesNotStartWith;
		}

		static SQLComparisonOperator NotInComparisonOperator(SQLComparisonOperator comparisonOperator)
		{
			if (IsNotIn(comparisonOperator))
			{
				if (comparisonOperator == SQLComparisonOperator.NotContains)
				{
					comparisonOperator = SQLComparisonOperator.Contains;
				}
				else if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					comparisonOperator = SQLComparisonOperator.Equal;
				}
				else if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith)
				{
					comparisonOperator = SQLComparisonOperator.StartsWith;
				}
			}
			return comparisonOperator;
		}

		public static ZQuery GetCartageParentSubQuery(ZQuery filter)
		{
			var cartageFilter = new ZDBOnlyQuery(typeof(CommonCartage));
			if (filter.Params.Any() || ColumnsThatMayExceedMaximumElementsForParameterisation.Any(c => filter.FilterPartsHashKey.Contains(c)))
			{
				var subFilter = new ZDBOnlySubQuery(typeof(AutoViewJobCartageParents), ViewJobCartageParentsSchema.PK);
				subFilter.AddToFilter(filter);
				cartageFilter.AddSubQuery(JobCartageSchema.JJ_ParentID, subFilter, JoinCondition.And);
			}
			else
			{
				cartageFilter.AddToFilter(filter, JoinCondition.And);
			}

			return cartageFilter;
		}

		static List<ZString> ColumnsThatMayExceedMaximumElementsForParameterisation
		{
			get
			{
				return new List<ZString> { AutoViewJobCartageParents.Schema.VCP_JobNumber };
			}
		}
	}
}
