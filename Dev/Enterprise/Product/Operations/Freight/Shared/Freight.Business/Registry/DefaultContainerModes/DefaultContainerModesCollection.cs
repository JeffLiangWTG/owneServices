using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class DefaultContainerModesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DefaultContainerModesCollection()
		{
		}

		public DefaultContainerModesCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new DefaultContainerModes this[int x]
		{
			get { return (DefaultContainerModes)Elements[x]; }
		}

		public new DefaultContainerModes AddNew()
		{
			return (DefaultContainerModes)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultContainerModesCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultContainerModes(CurrentFactory);
		}

		public bool IsDuplicateItem(DefaultContainerModes itemToCheck)
		{
			bool result = false;

			foreach (DefaultContainerModes item in this)
			{
				if (item != itemToCheck && item.TransportMode == itemToCheck.TransportMode)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public ZString FindDefaultContainerMode(string transportMode)
		{
			ZString defaultContainerMode = "";

			foreach (DefaultContainerModes registryItem in this)
			{
				if (registryItem.TransportMode == transportMode)
				{
					defaultContainerMode = registryItem.ContainerMode;
					break;
				}
			}

			return defaultContainerMode;
		}
	}
}
