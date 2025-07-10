using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class AMSBillEDIMessageCollection : BusinessObjectCollection<AMSEDIMessage>
	{
		public AMSBillEDIMessageCollection(BusinessObjectFactory factory, string billPKAsString, ZGuid moveHeaderPK)
			: base(factory, GetAdditionalFilter(billPKAsString, moveHeaderPK))
		{
		}

		public AMSBillEDIMessageCollection(BusinessObjectFactory factory, string billPKAsString, IEnumerable<ZGuid> moveHeaderPKs)
			: base(factory, GetAdditionalFilter(billPKAsString, moveHeaderPKs))
		{
		}

		static ZQuery GetAdditionalFilter(string billPKAsString, ZGuid moveHeaderPK)
		{
			var query = GetCommonAdditionalFilter(billPKAsString);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, moveHeaderPK);
			return query;
		}

		static ZQuery GetAdditionalFilter(string billPKAsString, IEnumerable<ZGuid> moveHeaderPKs)
		{
			var query = GetCommonAdditionalFilter(billPKAsString);
			query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, moveHeaderPKs);
			return query;
		}

		static ZQuery GetCommonAdditionalFilter(string billPKAsString)
		{
			var query = AMSEDIMessage.AMSFilter;
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.Equal, billPKAsString);
			query.AddToFilter(EDIMessageSchema.EM_LinkTable, SQLComparisonOperator.Equal, CusInBondMoveHeader.Schema.TableName);
			return query;
		}
	}
}
