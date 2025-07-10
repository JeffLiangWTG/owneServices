using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public abstract class XmlSerializableNonPersistentBusinessObject : NonPersistentBusinessObject
	{
		protected XmlSerializableNonPersistentBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected XmlSerializableNonPersistentBusinessObject()
		{
		}
	}
}