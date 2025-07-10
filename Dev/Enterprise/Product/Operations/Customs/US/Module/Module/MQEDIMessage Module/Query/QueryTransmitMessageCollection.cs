using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Module
{
	/// <summary>
	/// Query messages that are not attached to any jobs therefore, cannot be viewed anywhere in the system
	/// </summary>
	public class QueryTransmitMessageCollection : NonDependentEDIMessageCollection
	{
		public QueryTransmitMessageCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new MQEDIMessage AddNew() => (MQEDIMessage)base.AddNew();

		public new MQEDIMessage this[int index] => (MQEDIMessage)base[index];

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.USCustomsImport);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Enterprise.Messaging.Business.EDIMessage.Direction.Transmit);

			var messageTypeFilter = new ZQuery();

			var queryMessageTypes = ApplicationIdentifierCodeList.GetApplicationCodesForQuery();

			if (queryMessageTypes.Length == 0)
			{
				result.IsNoResultQuery = true;
			}
			else
			{
				var queryMessageTypesCodes = queryMessageTypes.Where(x => x.Code != ApplicationIdentifierCodeList.Codes.UserStatistics).Select(x => x.Code);
				messageTypeFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageType, queryMessageTypesCodes);
				result.AddToFilter(messageTypeFilter);
			}

			var additionalQuery = new ZQuery(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, Enterprise.Messaging.Business.EDIMessage.Direction.Receive), JoinCondition.And, new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.UserStatistics));
			result.AddToFilter(additionalQuery, JoinCondition.Or);
			return result;
		}
	}
}
