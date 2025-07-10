using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceReportConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceReportConfigurationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ComplianceReportConfigurationCollection()
			: this(new BusinessObjectFactory() { NameForDebugging = "ComplianceReportConfigurationCollection_Ctor" })
		{
		}

		public ComplianceReportConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ComplianceReportConfiguration this[int x]
		{
			get { return (ComplianceReportConfiguration)base[x]; }
		}

		public new ComplianceReportConfiguration AddNew()
		{
			return (ComplianceReportConfiguration)base.AddNew();
		}

		public CodeDescriptionPairList GetReportTypeList()
		{
			return GetReportTypeList(null, null);
		}

		public CodeDescriptionPairList GetReportTypeList(string countryCode, string descriptionFormat)
		{
			var result = new CodeDescriptionPairList();

			result.AddRange(this.Cast<ComplianceReportConfiguration>().
				Where(x => string.IsNullOrEmpty(countryCode) || x.Country == countryCode.ToUpper()).
				Select(x =>
					new CodeDescriptionPair(x.ReportCode.ToString(), string.Format((string.IsNullOrEmpty(descriptionFormat) ? "{0}" : descriptionFormat), x.ReportTitle))).ToArray());

			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceReportConfigurationCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceReportConfiguration(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
