using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Recruiter.Business
{
	[XmlSerializerAssembly("Enterprise.Recruiter.Business.XmlSerializers")]
	public class ReferringPartyConfigurationCollection : RegistryBusinessObjectCollection
	{
		public ReferringPartyConfigurationCollection()
		{
		}

		public ReferringPartyConfigurationCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new ReferringPartyConfiguration AddNew()
		{
			return (ReferringPartyConfiguration)base.AddNew();
		}

		public new ReferringPartyConfiguration this[int i]
		{
			get { return (ReferringPartyConfiguration)base[i]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReferringPartyConfiguration();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = GetNewCollection();
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		protected virtual ReferringPartyConfigurationCollection GetNewCollection()
		{
			return new ReferringPartyConfigurationCollection(CurrentFallbackLevel);
		}
	}
}
