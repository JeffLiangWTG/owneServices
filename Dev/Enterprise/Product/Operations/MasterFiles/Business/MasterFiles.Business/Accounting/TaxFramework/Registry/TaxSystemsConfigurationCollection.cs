using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxSystemsConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new TaxSystemsConfiguration this[int x]
		{
			get { return (TaxSystemsConfiguration)base[x]; }
		}

		public new TaxSystemsConfiguration AddNew()
		{
			return (TaxSystemsConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxSystemsConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TaxSystemsConfiguration();
		}

		#region GetTaxSystem

#if DEBUG
		public void ClearCachedTaxSystemsByCode_ForTestOnly(BusinessObjectFactory factoryForCaching) => factoryForCaching.ClearCachedValue<Dictionary<ZString, TaxSystemsConfiguration>>("TaxSystemsByCode");
#endif

		#endregion
	}
}
