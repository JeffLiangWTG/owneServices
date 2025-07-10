using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.TW.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.TW.Business.XmlSerializers")]
	public class CusGoodsLocationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CusGoodsLocationCollection() : base(new BusinessObjectFactory() { NameForDebugging = "CusGoodsLocationCollection_Ctor" }) { }

		public CusGoodsLocationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		public new CusGoodsLocation this[int i] => (CusGoodsLocation)Elements[i];

		public new CusGoodsLocation AddNew() => (CusGoodsLocation)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new CusGoodsLocation(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new CusGoodsLocationCollection(fallbackLevel, factory);
	}
}
