using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	/// <summary>
	/// Adding new mock helper methods to this class? please follow the existing guidelines and structure.
	/// Refer: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/9188/Mock-Helpers
	/// </summary>
	public static class TestMockObjectCreator
	{
		public static Mock<IGlobalAccountingCountryFactory> CreateAndRegisterIGlobalAccountingCountryFactory(IAccountingCountryFactory countryFactory = null)
		{
			var globalFactoryMock = new Mock<IGlobalAccountingCountryFactory>();
			globalFactoryMock.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(() => countryFactory);

			ObjectFactory.Substitute(globalFactoryMock.Object);

			return globalFactoryMock;
		}

		public static Mock<TInterface> CreateIAccountingCountryFactoryImplementingInterface<TInterface>() where TInterface : class
		{
			var countryFactoryMock = new Mock<IAccountingCountryFactory>().As<TInterface>();

			return countryFactoryMock;
		}

		public static Mock<IComplianceInfoElectronicInvoicing> CreateAndRegisterIComplianceInfoElectronicInvoicing()
		{
			var complianceInfoEInvoicingMock = new Mock<IComplianceInfoElectronicInvoicing>();
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();
			countryComplianceFactoryMock.Setup(x => x.GetIComplianceInfoElectronicInvoicing(It.IsAny<ZString>())).Returns(complianceInfoEInvoicingMock.Object);

			ObjectFactory.Substitute(countryComplianceFactoryMock.Object);

			return complianceInfoEInvoicingMock;
		}

		public static Mock<ITaxFrameworkConfigurationHelper> CreateAndRegisterITaxFrameworkConfigurationHelper()
		{
			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			var mockITaxFrameworkConfigurationHelper = new Mock<ITaxFrameworkConfigurationHelper>();
			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetTaxFrameworkConfigurationHelper()).Returns(mockITaxFrameworkConfigurationHelper.Object);
			ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object);

			return mockITaxFrameworkConfigurationHelper;
		}

		public static Mock<ITaxFrameworkConfigurationHelper> WithGetTaxAuthorities(this Mock<ITaxFrameworkConfigurationHelper> mockITaxFrameworkConfigurationHelper, ZString countryCode, ZString? taxAuthorityType, CodeDescriptionPairList expectedList)
		{
			mockITaxFrameworkConfigurationHelper.Setup(x => x.GetTaxAuthorities(countryCode, taxAuthorityType)).Returns(expectedList);

			return mockITaxFrameworkConfigurationHelper;
		}

		public static Mock<ITaxFrameworkConfigurationHelper> WithGetTaxSystems(this Mock<ITaxFrameworkConfigurationHelper> mockITaxFrameworkConfigurationHelper, ZString countryCode, ZString taxSystemRegistrationLevel, CodeDescriptionPairList expectedList)
		{
			mockITaxFrameworkConfigurationHelper.Setup(x => x.GetTaxSystems(countryCode, taxSystemRegistrationLevel)).Returns(expectedList);
			return mockITaxFrameworkConfigurationHelper;
		}

		public static Mock<ITaxFrameworkConfigurationHelper> WithGetTaxSystems(this Mock<ITaxFrameworkConfigurationHelper> mockITaxFrameworkConfigurationHelper, TaxSystemsConfiguration taxConfig)
		{
			return mockITaxFrameworkConfigurationHelper.WithGetTaxSystems(taxConfig.Country,taxConfig.RegistrationLevel,new CodeDescriptionPairList { new CodeDescriptionPair(taxConfig.Code.ToString(), taxConfig.Name.ToString()) });
		}

		public static Mock<ITaxFrameworkConfigurationHelper> WithGetTaxSystem(this Mock<ITaxFrameworkConfigurationHelper> mockITaxFrameworkConfigurationHelper, BusinessObjectFactory factory, TaxSystemsConfiguration taxConfig)
		{
			mockITaxFrameworkConfigurationHelper.Setup(x => x.GetTaxSystem(taxConfig.Code, factory)).Returns(taxConfig);
			return mockITaxFrameworkConfigurationHelper;
		}

		public static Mock<ITaxFrameworkConfigurationHelper> WithGetCompanyTaxConfigurations(this Mock<ITaxFrameworkConfigurationHelper> mockITaxFrameworkConfigurationHelper,
			AccTaxConfigurationCollection taxConfigList, BusinessObjectFactory factory, GlbCompany company , ZQuery additionalFilter)
		{
			mockITaxFrameworkConfigurationHelper.Setup(x =>
					x.GetCompanyTaxConfigurations(factory,company, additionalFilter))
				.Returns(taxConfigList);
			return mockITaxFrameworkConfigurationHelper;
		}

		/// <summary>
		/// Applies the given country factory to all countries via IGlobalAccountingCountryFactory.
		/// </summary>
		public static Mock<IGlobalAccountingCountryFactory> ToGlobalFactory(this Mock<IAccountingCountryFactory> mockFactory)
		{
			var globalFactory = new Mock<IGlobalAccountingCountryFactory>();
			globalFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockFactory.Object);
			return globalFactory;
		}

		/// <summary>
		/// Adds a null result for any IInstanceProvider type.
		/// Note that production code NEVER returns a null; avoid this method.
		/// </summary>
		public static Mock<IAccountingCountryFactory> WithNullResultFromInstanceProvider_Dangerous<TInstanceInterface>(this Mock<IAccountingCountryFactory> mockFactory)
			where TInstanceInterface : class
		{
			mockFactory.As<IInstanceProvider<TInstanceInterface>>().Setup(x => x.Get()).Returns(() => null);
			return mockFactory;
		}

		public static void SetupSupportComplianceSubType(string country)
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();

			var mockComplianceSubTypeRulesWithMultipleRuleSetProvider = new Mock<IComplianceSubTypeRulesWithMultipleRuleSetProvider>();
			mockComplianceSubTypeRulesWithMultipleRuleSetProvider
				.Setup(x => x.SetComplianceSubTypeAttributionRuleConfigurations(It.IsAny<ComplianceSubTypeAttributionRuleConfigurationCollection>(), It.IsAny<string>()))
				.Callback((ComplianceSubTypeAttributionRuleConfigurationCollection collection, string rule) =>
				{
					var configuration = collection.AddNew();
					configuration.Country = country;
				});

			mockICountryComplianceFactory.Setup(x => x.GetIComplianceSubTypeRulesWithMultipleRuleSetProvider(country)).Returns(mockComplianceSubTypeRulesWithMultipleRuleSetProvider.Object);

			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);
		}

		public static Mock<IAccountingCountryComplianceGlobalFactory> CreateAndRegisterIAccountingCountryComplianceGlobalFactory()
		{
			var mockIAccountingCountryComplianceGlobalFactory = new Mock<IAccountingCountryComplianceGlobalFactory>();
			ObjectFactory.Substitute(mockIAccountingCountryComplianceGlobalFactory.Object);
			return mockIAccountingCountryComplianceGlobalFactory;
		}

		public static Mock<TInterface> SetupFeatureInterface<TInterface>(this Mock<IAccountingCountryComplianceGlobalFactory> mockFactory) where TInterface : class
		{
			var mockFeatureInterface = new Mock<TInterface>();
			mockFactory.Setup(x => x.GetFeatureInterface<TInterface>(It.IsAny<ZString>())).Returns(mockFeatureInterface.Object);
			return mockFeatureInterface;
		}
	}
}
