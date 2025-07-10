using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class RevenueRecognitionByChargeGroupCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new RevenueRecognitionByChargeGroup this[int i]
		{
			get { return (RevenueRecognitionByChargeGroup)Elements[i]; }
		}

		public new RevenueRecognitionByChargeGroup AddNew()
		{
			return (RevenueRecognitionByChargeGroup)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RevenueRecognitionByChargeGroupCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RevenueRecognitionByChargeGroup();
		}
	}
}
