using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class MessagesToShowCollection : ActiveBusinessObjectCollection<BaseEDIMessage>
	{
		public enum MessagesStatus { ActiveOnly, InactiveOnly }

		public MessagesToShowCollection(BusinessObjectFactory factory, IReadOnlyList<ZGuid> pKs, MessagesStatus messageStatus, ZQuery additionalFilter)
			: base(factory, GetMessagesToShow(pKs, messageStatus, additionalFilter))
		{
		}

		static ZQuery GetMessagesToShow(IReadOnlyList<ZGuid> pKs, MessagesStatus messageStatus, ZQuery additionalFilter)
		{
			var result = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, pKs);

			result.AddToFilter(GetApplicationCodeQuery(), JoinCondition.And);
			result.AddToFilter(additionalFilter);

			var sqlOperator = messageStatus == MessagesStatus.ActiveOnly ? SQLComparisonOperator.NotEqual : SQLComparisonOperator.Equal;
			result.AddToFilter(EDIMessageSchema.EM_Status, sqlOperator, EDIMessage.Status.Discarded);

			return result;
		}

		static ZQuery GetApplicationCodeQuery()
		{
			var result = new ZQuery(EDIMessageSchema.EM_ApplicationCode, Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USeBond);
			result.AddToFilter(CBPEDIMessage.AllCBPFilter, JoinCondition.Or);

			return result;
		}

		protected override bool AllowNew => false;
	}
}
