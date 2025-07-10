using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxDateDefaultingOptionCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new TaxDateDefaultingOption this[int i]
		{
			get { return (TaxDateDefaultingOption)Elements[i]; }
		}

		public new TaxDateDefaultingOption AddNew()
		{
			return (TaxDateDefaultingOption)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxDateDefaultingOptionCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TaxDateDefaultingOption();
		}
	}
}
