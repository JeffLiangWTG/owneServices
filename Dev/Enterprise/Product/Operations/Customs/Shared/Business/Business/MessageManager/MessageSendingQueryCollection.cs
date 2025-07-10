using System.Collections;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public delegate void MessageSendingQueryDelegate(bool answer);

	public class MessageSendingQueryCollection : IEnumerable
	{
		public MessageSendingQueryCollection()
		{
			elements = new ArrayList();
		}

		public void Add(MessageSendingQuery query)
		{
			elements.Add(query);
		}

		#region Implementation

		readonly ArrayList elements;

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return elements.GetEnumerator();
		}

		#endregion
	}

	public struct MessageSendingQuery
	{
		public ZString Question;
		public ZString Caption;
		public MessageSendingQueryDelegate Delegate;
	}
}
