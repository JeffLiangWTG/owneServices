using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public enum RelatedJobMatcherKeys { Consignee, Consignor, BillOfLading, ITN, BookingRefNumber }

	public class RelatedBrokerageJobMatcher
	{
		public RelatedBrokerageJobMatcher() { }

		public static RelatedBrokerageJobMatcher New()
		{
			var types = ObjectFactory.Get<Hashtable>("RelatedBrokerageJobMatchers");
			var objectHandle = (ObjectHandle)types[GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString()];
			return objectHandle != null ? (RelatedBrokerageJobMatcher)objectHandle.GetObject() : new RelatedBrokerageJobMatcher();
		}

		public BusinessObject GetMatchingJob(Dictionary<RelatedJobMatcherKeys, ZString> matchingCriteria)
		{
			return GetMatchingJobCore(matchingCriteria);
		}

		protected virtual BusinessObject GetMatchingJobCore(Dictionary<RelatedJobMatcherKeys, ZString> matchingCriteria)
		{
			BusinessObject result = null;
			if (matchingCriteria.ContainsKey(RelatedJobMatcherKeys.Consignee) && ZGuid.IsGuid(matchingCriteria[RelatedJobMatcherKeys.Consignee]) &&
				matchingCriteria.ContainsKey(RelatedJobMatcherKeys.Consignor) && ZGuid.IsGuid(matchingCriteria[RelatedJobMatcherKeys.Consignor]) &&
				matchingCriteria.ContainsKey(RelatedJobMatcherKeys.BillOfLading))
			{
				var consigneePK = new ZGuid(matchingCriteria[RelatedJobMatcherKeys.Consignee]);
				var query = new ZQuery(JobDeclarationSchema.JE_OH_Importer, consigneePK);
				var consignorPK = new ZGuid(matchingCriteria[RelatedJobMatcherKeys.Consignor]);
				query.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, consignorPK);
				query.AddToFilter(JobDeclarationSchema.JE_MasterBill, matchingCriteria[RelatedJobMatcherKeys.BillOfLading]);

				var jobs = new BusinessObjectFactory().Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(query);
				if (jobs.Length == 1)
				{
					result = (BusinessObject)jobs[0];
				}
			}
			return result;
		}
	}
}
