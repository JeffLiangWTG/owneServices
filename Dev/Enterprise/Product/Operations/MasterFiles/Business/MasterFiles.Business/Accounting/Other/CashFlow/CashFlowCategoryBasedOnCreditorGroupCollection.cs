using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class CashFlowCategoryBasedOnCreditorGroupCollection : CashFlowCategoryBasedOnOrgGroupCollection<CashFlowCategoryBasedOnCreditorGroup>
	{
		public CashFlowCategoryBasedOnCreditorGroupCollection()
		{ }

		public CashFlowCategoryBasedOnCreditorGroupCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CashFlowCategoryBasedOnCreditorGroupCollection();
		}
	}
}
