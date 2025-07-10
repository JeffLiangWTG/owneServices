using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public abstract class CashFlowCategoryBasedOnOrgGroupCollection<T> : RegistryBusinessObjectCollectionTemplate where T : CashFlowCategoryBasedOnOrgGroup, new()
	{
		public CashFlowCategoryBasedOnOrgGroupCollection()
		{ }

		public CashFlowCategoryBasedOnOrgGroupCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		public new T this[int x]
		{
			get { return (T)base[x]; }
		}

		public new T AddNew()
		{
			return (T)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new T();
		}
	}
}
