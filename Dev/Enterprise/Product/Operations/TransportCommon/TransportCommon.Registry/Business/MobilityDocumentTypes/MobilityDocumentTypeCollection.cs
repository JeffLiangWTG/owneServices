using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	[XmlRoot(ElementName = "MobilityDocumentTypeCollection")]
	public class MobilityDocumentTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public MobilityDocumentTypeCollection()
			: base() { }

		public MobilityDocumentTypeCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public MobilityDocumentTypeCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public MobilityDocumentTypeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new MobilityDocumentType this[int index]
		{
			get { return (MobilityDocumentType)Elements[index]; }
		}

		public new MobilityDocumentType AddNew()
		{
			return (MobilityDocumentType)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MobilityDocumentType(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new MobilityDocumentTypeCollection(fallbackLevel, factory);
		}
	}
}
