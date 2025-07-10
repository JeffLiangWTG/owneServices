using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class SADDocumentWatermarkCollection : RegistryBusinessObjectCollectionTemplate
	{
		public SADDocumentWatermarkCollection()
		{
		}

		public SADDocumentWatermarkCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public new SADDocumentWatermark this[int i]
		{
			get { return (SADDocumentWatermark)Elements[i]; }
		}

		public new SADDocumentWatermark AddNew()
		{
			return (SADDocumentWatermark)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SADDocumentWatermark(CurrentFallbackLevel, CurrentFactory, this);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SADDocumentWatermarkCollection(fallbackLevel, factory);
		}
	}
}
