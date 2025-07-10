using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class SameChargeCodeDifferentProviderCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new SameChargeCodeDifferentProvider this[int i] => (SameChargeCodeDifferentProvider)Elements[i];
		public new SameChargeCodeDifferentProvider AddNew() => (SameChargeCodeDifferentProvider)base.AddNew();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new SameChargeCodeDifferentProviderCollection();
		protected override BusinessObject CreateNonPersistentBusinessObject() => new SameChargeCodeDifferentProvider();

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			foreach (var item in Elements.Cast<SameChargeCodeDifferentProvider>())
			{
				item.Validate();
			}
		}
	}
}
