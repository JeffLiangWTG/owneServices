using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.ZA.Business
{
	public class EDIMessageCollection : Messaging.Business.EDIMessageCollection
	{
		public EDIMessageCollection(BusinessObject master)
			: base(master)
		{
		}

		public new ZAMessage this[int index] => (ZAMessage)base[index];

		public new ZAMessage AddNew() => (ZAMessage)base.AddNew();

		public IEnumerable<ZAMessage> GetMatchingMessages(ZString messageType, ZString direction, Predicate<ZAMessage> additionalCondition)
		{
			var result = GetMatchingMessages(ApplicationCodes.SouthAfricanCustoms, new[] { messageType }, direction);
			return result.OfType<ZAMessage>().Where(x => additionalCondition == null || additionalCondition(x));
		}

		public ZAMessage[] GetIncomingMessages(params ZString[] messageTypes)
			=> GetMatchingMessages(ApplicationCodes.SouthAfricanCustoms, messageTypes, Direction.Receive).Cast<ZAMessage>().ToArray();
	}
}
