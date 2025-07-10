using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	[XmlRoot("UCMPTypes")]
	public sealed class UCMEDIMessageTestTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public UCMEDIMessageTestTypeCollection()
		{
		}

		public UCMEDIMessageTestTypeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new UCMEDIMessageTestType this[int i]
		{
			get { return (UCMEDIMessageTestType)Elements[i]; }
		}

		public new UCMEDIMessageTestType AddNew()
		{
			return (UCMEDIMessageTestType)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UCMEDIMessageTestTypeCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new UCMEDIMessageTestType(CurrentFactory);
		}
	}
}
