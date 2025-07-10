using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class OnlineApplicationDocTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public OnlineApplicationDocTypeCollection()
			: base() { }

		public OnlineApplicationDocTypeCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public OnlineApplicationDocTypeCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public OnlineApplicationDocTypeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		public new OnlineApplicationDocType this[int index]
		{
			get { return (OnlineApplicationDocType)Elements[index]; }
		}

		public new OnlineApplicationDocType AddNew()
		{
			return (OnlineApplicationDocType)base.AddNew();
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OnlineApplicationDocTypeCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OnlineApplicationDocType(CurrentFallbackLevel, CurrentFactory);
		}

		#endregion
	}
}
