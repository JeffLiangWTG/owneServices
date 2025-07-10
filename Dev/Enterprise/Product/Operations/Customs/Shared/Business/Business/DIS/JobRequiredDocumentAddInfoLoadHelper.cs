using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class JobRequiredDocumentAddInfoLoadHelper
	{
		public static ZQuery GetJobRequiredDocumentAddInfoFilter(ZGuid docsAndCartageParentPK, IEnumerable<ZGuid> holderOrgHeadersPks, ZGuid companyPK, ZString applicationCode, ZString govAgencyIDCode)
		{
			var query = new ZDBOnlyQuery(typeof(JobRequiredDocumentAddInfo));
			query.AddToFilter(GetJobRequiredDocumentAddInfoFilterForCusPermitHeader(holderOrgHeadersPks, companyPK, applicationCode, govAgencyIDCode));
			if (!docsAndCartageParentPK.IsEmpty)
			{
				query.AddToFilter(GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(docsAndCartageParentPK, companyPK, applicationCode, govAgencyIDCode), JoinCondition.Or);
			}
			return query;
		}

		public static ZDBOnlyQuery GetJobRequiredDocumentAddInfoFilterForCusPermitHeader(IEnumerable<ZGuid> holderOrgHeadersPks, ZGuid companyPK, ZString applicationCode, ZString govAgencyIDCode)
		{
			var result = GetBaseJobRequiredDocumentAddInfoFilter(companyPK, applicationCode, govAgencyIDCode, "");
			var requiredDocumentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
			requiredDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, CusPermitHeaderSchema.Constants.Prefix);
			var permitQuery = new ZDBOnlySubQuery(typeof(BaseCusPermitHeader), JobRequiredDocumentSchema.EQ_ParentID);
			permitQuery.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, holderOrgHeadersPks);
			requiredDocumentQuery.AddSubQuery(permitQuery, JoinCondition.And);
			result.AddSubQuery(requiredDocumentQuery, JoinCondition.And);
			return result;
		}

		public static ZDBOnlyQuery GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(ZGuid docsAndCartageParentPK, ZGuid companyPK, ZString applicationCode, ZString govAgencyIDCode)
		{
			var result = GetBaseJobRequiredDocumentAddInfoFilter(companyPK, applicationCode, govAgencyIDCode, "");
			var requiredDocumentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
			requiredDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, JobDocsAndCartageSchema.Constants.Prefix);
			var docsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobRequiredDocumentSchema.EQ_ParentID);
			docsAndCartageQuery.AddToFilter(JobDocsAndCartageSchema.JP_ParentID, docsAndCartageParentPK);
			requiredDocumentQuery.AddSubQuery(docsAndCartageQuery, JoinCondition.And);
			result.AddSubQuery(requiredDocumentQuery, JoinCondition.And);
			return result;
		}

		public static ZDBOnlyQuery GetJobRequiredDocumentAddInfoFilterForJobDocsAndCartage(ZGuid docsAndCartageParentPK, ZGuid companyPK, ZString applicationCode, ZString govAgencyIDCode, ZString referenceNumber)
		{
			var result = GetBaseJobRequiredDocumentAddInfoFilter(companyPK, applicationCode, govAgencyIDCode, referenceNumber);
			var requiredDocumentQuery = new ZDBOnlySubQuery(typeof(JobRequiredDocument), JobRequiredDocumentAddInfoSchema.EX_EQ_RequiredDocument);
			requiredDocumentQuery.AddToFilter(JobRequiredDocumentSchema.EQ_ParentTableCode, JobDocsAndCartageSchema.Constants.Prefix);
			var docsAndCartageQuery = new ZDBOnlySubQuery(typeof(JobDocsAndCartage), JobRequiredDocumentSchema.EQ_ParentID);
			docsAndCartageQuery.AddToFilter(JobDocsAndCartageSchema.JP_ParentID, docsAndCartageParentPK);
			requiredDocumentQuery.AddSubQuery(docsAndCartageQuery, JoinCondition.And);
			result.AddSubQuery(requiredDocumentQuery, JoinCondition.And);
			return result;
		}

		static ZDBOnlyQuery GetBaseJobRequiredDocumentAddInfoFilter(ZGuid companyPK, ZString applicationCode, ZString govAgencyIDCode, ZString referenceNumber)
		{
			var result = new ZDBOnlyQuery(typeof(JobRequiredDocumentAddInfo));
			result.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_GC_Company, companyPK);
			result.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_ApplicationCode, applicationCode);
			if (!govAgencyIDCode.IsEmpty)
			{
				result.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_AddInfo, SQLComparisonOperator.Like, ZString.Format("%<PGA>{0}</PGA>%", govAgencyIDCode));
			}
			if (!referenceNumber.IsEmpty)
			{
				result.AddToFilter(JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber, referenceNumber);
			}
			return result;
		}
	}
}
