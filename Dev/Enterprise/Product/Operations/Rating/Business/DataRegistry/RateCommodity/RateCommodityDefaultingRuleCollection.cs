using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class RateCommodityDefaultingRuleCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new RateCommodityDefaultingRule this[int i] => (RateCommodityDefaultingRule)Elements[i];
		public new RateCommodityDefaultingRule AddNew() => (RateCommodityDefaultingRule)base.AddNew();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new RateCommodityDefaultingRuleCollection();
		protected override BusinessObject CreateNonPersistentBusinessObject() => new RateCommodityDefaultingRule();
	}
}
