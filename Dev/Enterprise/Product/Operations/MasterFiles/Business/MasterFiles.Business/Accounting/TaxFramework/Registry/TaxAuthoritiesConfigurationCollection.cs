using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxAuthoritiesConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new TaxAuthoritiesConfiguration this[int x]
		{
			get { return (TaxAuthoritiesConfiguration)base[x]; }
		}

		public new TaxAuthoritiesConfiguration AddNew()
		{
			return (TaxAuthoritiesConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxAuthoritiesConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TaxAuthoritiesConfiguration();
		}

		public CodeDescriptionPairList GetTaxAuthorities(ZString countryCode)
		{
			var types = new CodeDescriptionPairList();
			var taxAuthoritiesConfigurations = this.Cast<TaxAuthoritiesConfiguration>().Where(x => x.Country == countryCode);
			foreach (var taxAuthorityConfiguration in taxAuthoritiesConfigurations)
			{
				types.AddPair(taxAuthorityConfiguration.Code, taxAuthorityConfiguration.Name);
			}
			return types;
		}
	}
}
