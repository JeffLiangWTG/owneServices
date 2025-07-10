using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TW.Business.XmlSerializers")]
	public class CusBrokerageBoxNumberCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CusBrokerageBoxNumberCollection() : base(new BusinessObjectFactory() { NameForDebugging = "CusBrokerageBoxNumberCollection_Ctor" }) { }

		public CusBrokerageBoxNumberCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		public new CusBrokerageBoxNumber this[int i] => (CusBrokerageBoxNumber)Elements[i];

		public new CusBrokerageBoxNumber AddNew() => (CusBrokerageBoxNumber)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CusBrokerageBoxNumber(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CusBrokerageBoxNumberCollection(fallbackLevel, factory);
	}
}
