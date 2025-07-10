using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	[XmlRoot(ElementName = "OrderManagerRequestMappings")]
	public class OrderManagerRequestMappingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public OrderManagerRequestMappingCollection()
		{
		}

		public OrderManagerRequestMappingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public new OrderManagerRequestMapping this[int index]
		{
			get { return (OrderManagerRequestMapping)Elements[index]; }
		}

		public new OrderManagerRequestMapping AddNew()
		{
			return (OrderManagerRequestMapping)base.AddNew();
		}

		public void AddDefaultValues()
		{
			foreach (ICodeDescription request in new OrderManagerRequestMappingList())
			{
				Add(new OrderManagerRequestMapping
				{
					Request = request.Code,
				});
			}
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OrderManagerRequestMappingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrderManagerRequestMapping(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
