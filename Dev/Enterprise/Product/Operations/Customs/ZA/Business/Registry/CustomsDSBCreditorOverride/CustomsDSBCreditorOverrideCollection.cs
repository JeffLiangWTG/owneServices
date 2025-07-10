using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class CustomsDSBCreditorOverrideCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CustomsDSBCreditorOverrideCollection()
			: base()
		{
		}

		public CustomsDSBCreditorOverrideCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new CustomsDSBCreditorOverride this[int i]
		{
			get { return (CustomsDSBCreditorOverride)Elements[i]; }
		}

		public new CustomsDSBCreditorOverride AddNew()
		{
			return (CustomsDSBCreditorOverride)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomsDSBCreditorOverrideCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomsDSBCreditorOverride(CurrentFallbackLevel, CurrentFactory, this);
		}

		public ZGuid GetCreditorFor(ZString districtOfficeCode)
		{
			foreach (CustomsDSBCreditorOverride mapping in this)
			{
				if (mapping.DistrictOfficeCode == districtOfficeCode)
				{
					return mapping.CreditorPK;
				}
			}
			return ZGuid.Empty;
		}
	}
}
