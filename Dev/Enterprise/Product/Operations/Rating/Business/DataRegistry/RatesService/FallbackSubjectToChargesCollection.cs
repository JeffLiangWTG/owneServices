using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.Business
{
	[XmlSerializerAssembly("Enterprise.Rating.Business.XmlSerializers")]
	public class FallbackSubjectToChargesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new FallbackSubjectToCharges this[int i] => (FallbackSubjectToCharges)Elements[i];
		public new FallbackSubjectToCharges AddNew() => (FallbackSubjectToCharges)base.AddNew();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new FallbackSubjectToChargesCollection();
		protected override BusinessObject CreateNonPersistentBusinessObject() => new FallbackSubjectToCharges();
	}
}
