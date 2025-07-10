using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class HouseBillsNumberValidationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public HouseBillsNumberValidationCollection()
			: base() { }

		public HouseBillsNumberValidationCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public new HouseBillsNumberValidation this[int index]
		{
			get { return (HouseBillsNumberValidation)Elements[index]; }
		}

		public new HouseBillsNumberValidation AddNew()
		{
			return (HouseBillsNumberValidation)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HouseBillsNumberValidationCollection(factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HouseBillsNumberValidation(CurrentFactory);
		}

		#endregion
	}
}
