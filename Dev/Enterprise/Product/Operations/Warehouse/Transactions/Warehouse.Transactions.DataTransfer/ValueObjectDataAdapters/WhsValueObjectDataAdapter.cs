using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsValueObjectDataAdapter<TDocket, TValueObject> : ValueObjectDataAdapter<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsValueObjectDataAdapter()
			: this(EventsWithSourceType.Empty)
		{
		}

		protected WhsValueObjectDataAdapter(EventsWithSourceType triggeredByEvents)
		{
			TriggeredByEvents = triggeredByEvents;
		}

		protected readonly EventsWithSourceType TriggeredByEvents;
	}
}
