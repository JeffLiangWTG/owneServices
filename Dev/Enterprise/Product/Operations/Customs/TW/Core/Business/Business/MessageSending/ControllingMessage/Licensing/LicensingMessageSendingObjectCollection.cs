using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageSendingObjectCollection : ControllingMessageSendingObjectCollection
	{
		public LicensingMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new LicensingMessageSendingObject this[int index]
		{
			get { return (LicensingMessageSendingObject)Elements[index]; }
		}

		public new LicensingMessageSendingObject AddNew()
		{
			return (LicensingMessageSendingObject)base.AddNew();
		}
	}
}
