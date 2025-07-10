using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class AutoRateDateCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new AutoRateDate this[int i]
		{
			get { return (AutoRateDate)Elements[i]; }
		}

		public new AutoRateDate AddNew()
		{
			return (AutoRateDate)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutoRateDateCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AutoRateDate();
		}
	}
}
