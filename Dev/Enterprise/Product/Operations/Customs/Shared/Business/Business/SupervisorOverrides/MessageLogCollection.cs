using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class MessageLogCollection : NonPersistentBusinessObjectCollection<MessageLog>
	{
		public MessageLogCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MessageLog(ZString.Empty, ZString.Empty);
		}
	}
}
