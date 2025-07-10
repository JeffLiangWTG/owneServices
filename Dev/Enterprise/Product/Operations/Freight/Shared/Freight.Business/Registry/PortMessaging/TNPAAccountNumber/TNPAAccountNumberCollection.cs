using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class TNPAAccountNumberCollection : RegistryBusinessObjectCollectionTemplate
	{
		public TNPAAccountNumberCollection()
			: base()
		{ }

		public TNPAAccountNumberCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new TNPAAccountNumber this[int i]
		{
			get { return (TNPAAccountNumber)Elements[i]; }
		}

		public new TNPAAccountNumber AddNew()
		{
			return (TNPAAccountNumber)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TNPAAccountNumberCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TNPAAccountNumber();
		}

		public bool IsDuplicateItem(TNPAAccountNumber itemToCheck)
		{
			return (itemToCheck != null)
				&& (this.OfType<TNPAAccountNumber>().Any((item) => item != itemToCheck
				&& item.Port == itemToCheck.Port));
		}
	}
}
