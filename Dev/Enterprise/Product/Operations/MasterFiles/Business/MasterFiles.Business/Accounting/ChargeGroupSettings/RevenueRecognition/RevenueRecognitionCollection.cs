using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class RevenueRecognitionCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new RevenueRecognition this[int i]
		{
			get { return (RevenueRecognition)Elements[i]; }
		}

		public new RevenueRecognition AddNew()
		{
			return (RevenueRecognition)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RevenueRecognitionCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RevenueRecognition();
		}
	}
}
