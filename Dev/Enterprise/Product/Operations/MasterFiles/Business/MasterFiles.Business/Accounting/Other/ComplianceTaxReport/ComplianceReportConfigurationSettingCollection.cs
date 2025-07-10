using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceReportConfigurationSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceReportConfigurationSettingCollection()
			: base()
		{
		}

		public ComplianceReportConfigurationSettingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ComplianceReportConfigurationSettingCollection(BusinessObjectFactory factory, ZString countryCode)
			: this(factory)
		{
			this.countryCode = countryCode;
		}

		public ComplianceReportConfigurationSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ComplianceReportConfigurationSetting this[int x]
		{
			get { return (ComplianceReportConfigurationSetting)base[x]; }
		}

		public new ComplianceReportConfigurationSetting AddNew()
		{
			return (ComplianceReportConfigurationSetting)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportConfigurationSettingCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceReportConfigurationSetting(CurrentFallbackLevel, CurrentFactory);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((ComplianceReportConfigurationSetting)child).Country = CountryCode;
		}

		internal ZString CountryCode
		{
			get { return countryCode; }
			set
			{
				countryCode = value;
				foreach (ComplianceReportConfigurationSetting item in this)
				{
					item.Country = countryCode;
				}
			}
		}
		ZString countryCode;

		internal ComplianceReportConfiguration ParentConfiguration
		{
			get;
			set;
		}
	}
}
