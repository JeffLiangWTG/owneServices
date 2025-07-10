using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class CashFlowCategoryBasedOnDebtorGroupCollection : CashFlowCategoryBasedOnOrgGroupCollection<CashFlowCategoryBasedOnDebtorGroup>
	{
		public CashFlowCategoryBasedOnDebtorGroupCollection()
		{ }

		public CashFlowCategoryBasedOnDebtorGroupCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CashFlowCategoryBasedOnDebtorGroupCollection();
		}
	}
}
