using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class BranchDistrictPortCollection : RegistryBusinessObjectCollectionTemplate
	{
		public BranchDistrictPortCollection()
		{
		}

		public BranchDistrictPortCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new BranchDistrictPort this[int i]
		{
			get { return (BranchDistrictPort)Elements[i]; }
		}

		public new BranchDistrictPort AddNew()
		{
			return (BranchDistrictPort)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BranchDistrictPortCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BranchDistrictPort(CurrentFallbackLevel, CurrentFactory, this);
		}
	}
}
