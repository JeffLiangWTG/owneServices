using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class AutoRateDateByChargeGroupCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new AutoRateDateByChargeGroup this[int i]
		{
			get { return (AutoRateDateByChargeGroup)Elements[i]; }
		}

		public new AutoRateDateByChargeGroup AddNew()
		{
			return (AutoRateDateByChargeGroup)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoRateDateByChargeGroupCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AutoRateDateByChargeGroup();
		}
	}
}
