using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultOrgTimetableSettingsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new DefaultOrgTimetableSettings this[int x]
		{
			get { return (DefaultOrgTimetableSettings)Elements[x]; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DefaultOrgTimetableSettings(CurrentFallbackLevel, CurrentFactory);
		}

		public DefaultOrgTimetableSettingsCollection()
			: base()
		{
		}

		public DefaultOrgTimetableSettingsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DefaultOrgTimetableSettingsCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultOrgTimetableSettingsCollection(fallbackLevel, factory);
		}

		public new DefaultOrgTimetableSettings AddNew()
		{
			var newItem = (DefaultOrgTimetableSettings)base.AddNew();
			return newItem;
		}

		public DefaultOrgTimetableCollection DefaultOrgTimetablesForCountry(ZString countryCode)
		{
			return this.Cast<DefaultOrgTimetableSettings>().SingleOrDefault(x => x.CountryCode == (DefaultOrgTimetablesExistForCountry(countryCode) ? countryCode : ZString.Empty))?.Timetables;
		}

		bool DefaultOrgTimetablesExistForCountry(ZString countryCode)
		{
			return this.Cast<DefaultOrgTimetableSettings>().Any(x => x.CountryCode == countryCode);
		}
	}
}
