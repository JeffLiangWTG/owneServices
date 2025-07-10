using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class CommunitySystemCodesOfForwarderAndAgentCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CommunitySystemCodesOfForwarderAndAgentCollection()
			: base()
		{ }

		public CommunitySystemCodesOfForwarderAndAgentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new CommunitySystemCodesOfForwarderAndAgent this[int i]
		{
			get { return (CommunitySystemCodesOfForwarderAndAgent)Elements[i]; }
		}

		public new CommunitySystemCodesOfForwarderAndAgent AddNew()
		{
			return (CommunitySystemCodesOfForwarderAndAgent)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CommunitySystemCodesOfForwarderAndAgentCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommunitySystemCodesOfForwarderAndAgent();
		}

		public bool IsDuplicateItem(CommunitySystemCodesOfForwarderAndAgent itemToCheck)
		{
			return (itemToCheck != null)
				&& (this.OfType<CommunitySystemCodesOfForwarderAndAgent>().Any((item) => item != itemToCheck
				&& item.Port == itemToCheck.Port));
		}
	}
}
