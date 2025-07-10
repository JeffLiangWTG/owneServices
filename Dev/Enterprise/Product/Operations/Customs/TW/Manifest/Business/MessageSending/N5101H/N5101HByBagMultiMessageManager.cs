using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HByBagMultiMessageManager : N5101HMultiMessageManager
	{
		public N5101HByBagMultiMessageManager(MessageSendingObjectParent n5101HWrapper) : base(n5101HWrapper)
		{
		}

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			var groupedByAction = N5101HWrapper.SendingObjectsCollection.Cast<MessageSendingObject>().Where(x => x.ShouldSend).GroupBy(x => x.Action).ToDictionary(x => x.Key, y => y.ToArray());

			foreach (var pair in groupedByAction)
			{
				var groupedByBag = pair.Value.GroupBy(x => x.Bill.BagNumber).ToDictionary(x => x.Key, y => y.ToArray());
				foreach (var group in groupedByBag)
				{
					var collection = new MessageSendingObjectCollection(Factory);
					collection.AddRange(group.Value);
					var singleManager = new N5101HMessageManager(new N5101HByBagMessageSendingObject(ManifestHeader, collection, N5101HWrapper));
					result.Add(singleManager);
				}
			}
			return result.ToArray();
		}
	}
}
