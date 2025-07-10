using System;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceNumberSequenceConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ComplianceNumberSequenceConfigurationCollection()
			: this(new BusinessObjectFactory() { NameForDebugging = "ComplianceNumberSequenceConfigurationCollection_Ctor" })
		{
		}

		public ComplianceNumberSequenceConfigurationCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ComplianceNumberSequenceConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ComplianceNumberSequenceConfiguration this[int x]
		{
			get { return (ComplianceNumberSequenceConfiguration)base[x]; }
		}

		public new ComplianceNumberSequenceConfiguration AddNew()
		{
			return (ComplianceNumberSequenceConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceNumberSequenceConfigurationCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceNumberSequenceConfiguration(CurrentFallbackLevel, CurrentFactory);
		}

		protected override bool AllowNewCore => !HasDefaultMandatoryComplianceNumberSequenceConfiguration;

		bool HasDefaultMandatoryComplianceNumberSequenceConfiguration
		{
			get
			{
				if (!hasDefaultMandatoryComplianceNumberSequenceConfiguration.HasValue)
				{
					var countryCode = AccountingMasterFilesUtils.GetCountryCodeFromRegistryFallBackLevel(CurrentFactory, CurrentFallbackLevel);
					var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(countryCode);
					hasDefaultMandatoryComplianceNumberSequenceConfiguration = complianceInfo != null
						&& !string.IsNullOrEmpty(complianceInfo.DefaultMandatoryComplianceNumberSequenceConfigurationCode.Code);
				}

				return hasDefaultMandatoryComplianceNumberSequenceConfiguration.HasValue && hasDefaultMandatoryComplianceNumberSequenceConfiguration.Value;
			}
		}
		bool? hasDefaultMandatoryComplianceNumberSequenceConfiguration;

		public ComplianceNumberSequenceConfiguration GetComplianceNumberSequenceConfigurationByCode(ZString code)
		{
			return this.Cast<ComplianceNumberSequenceConfiguration>().First(x => x.Code == code);
		}

		public void AddMandatoryComplianceNumberSequenceConfiguration(BusinessObjectFactory factory, Guid companyPK)
		{
			var countryCode = AccountingMasterFilesUtils.GetCountryCodeFromCompanyPK(factory, companyPK);
			var complianceInfo = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceNumberSequenceConfigurationProvider(countryCode);
			if (complianceInfo != null)
			{
				var mandatoryConfigs = complianceInfo.GetMandatoryComplianceNumberSequenceConfiguration();
				var existingConfigWithMandatoryCode = this.Cast<ComplianceNumberSequenceConfiguration>().FirstOrDefault(existingConfig => mandatoryConfigs.Select(x => x.Code).Contains(existingConfig.Code));
				this.Remove(existingConfigWithMandatoryCode);
				this.AddRange(mandatoryConfigs);
			}
		}
	}
}
