using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAllocationOverrideConfigurationRegistryItem))]
	sealed class ComplianceSubTypeAllocationOverrideConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<ComplianceSubTypeAllocationOverrideConfigurationCollection>
	{
		protected override StronglyTypedRegistryItem<ComplianceSubTypeAllocationOverrideConfigurationCollection, ComplianceSubTypeAllocationOverrideConfigurationCollection> GetNewRegistryItem()
		{
			return new ComplianceSubTypeAllocationOverrideConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		[ExpectNoExceptions]
		public void TestDefaultValuesDependOnFallbackCountry()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData<GlbCompany>();
			factory.Save();

			AssertNotEquals("Precondition: We need new company different of current company", Env.CurrentCompany, company.PK);

			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IComplianceSubTypeAllocationOverrideConfigurationProvider>()
				.Setup(x => x.GetDefaults(It.IsAny<ComplianceSubTypeAllocationOverrideConfigurationCollection>()));
			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			AssertComplianceSubTypeAllocationOverrideConfigurationCollection(true);
			AssertComplianceSubTypeAllocationOverrideConfigurationCollection(false);

			void AssertComplianceSubTypeAllocationOverrideConfigurationCollection(bool enableEInvoicingFunctionality)
			{
				mockICountryComplianceFactory.Invocations.Clear();
				mockICountryComplianceInfoBase.Invocations.Clear();
				var expectedCalls = enableEInvoicingFunctionality ? Times.Once() : Times.Never();

				using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, enableEInvoicingFunctionality))
				{
					var collection = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

					mockICountryComplianceFactory.Verify(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()), expectedCalls);
					mockICountryComplianceFactory.Verify(x => x.GetICountryComplianceInfoBase(company.Country.Code), expectedCalls);

					mockICountryComplianceInfoBase.As<IComplianceSubTypeAllocationOverrideConfigurationProvider>().Verify(x => x.GetDefaults(It.IsAny<ComplianceSubTypeAllocationOverrideConfigurationCollection>()), expectedCalls);
					mockICountryComplianceInfoBase.As<IComplianceSubTypeAllocationOverrideConfigurationProvider>().Verify(x => x.GetDefaults(collection), expectedCalls);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestDefaultValuesDoesNotFailOnEmptyFallbackCountry()
		{
			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IComplianceSubTypeAllocationOverrideConfigurationProvider>()
				.Setup(x => x.GetDefaults(It.IsAny<ComplianceSubTypeAllocationOverrideConfigurationCollection>()));
			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var collection = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

				mockICountryComplianceFactory.Verify(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()), Times.Never);
				mockICountryComplianceInfoBase.As<IComplianceSubTypeAllocationOverrideConfigurationProvider>().Verify(x => x.GetDefaults(It.IsAny<ComplianceSubTypeAllocationOverrideConfigurationCollection>()), Times.Never);
			}
		}

		public void TestGetDefaultValue_Default()
		{
			var factory = new BusinessObjectFactory();

			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IComplianceSubTypeAllocationOverrideConfigurationProvider>()
				.Setup(x => x.GetDefaults(It.IsAny<ComplianceSubTypeAllocationOverrideConfigurationCollection>()))
				.Callback((ComplianceSubTypeAllocationOverrideConfigurationCollection collection) =>
				{
					var configuration = collection.AddNew();
					configuration.Country = CountryCodes.Uruguay;
					configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD;
					configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
				});
			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(CountryCodes.Uruguay))
				.Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Uruguay))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var defaultRegistryValue = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.Value;
				var actualSubTypes = defaultRegistryValue.Cast<ComplianceSubTypeAllocationOverrideConfiguration>().Select(x => (string)x.SubType).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] { UruguayComplianceInfo.ComplianceSubTypeCodes.TCD }, actualSubTypes);
			}
		}

		public void TestGetDefaultValue_Overridden()
		{
			var factory = new BusinessObjectFactory();
			var allocationOverride = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, factory, CountryCodes.Uruguay);
			var newItem = allocationOverride.AddNew();
			newItem.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.YKT;
			newItem.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;

			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IComplianceSubTypeAllocationOverrideConfigurationProvider>()
				.Setup(x => x.GetDefaults(It.IsAny<ComplianceSubTypeAllocationOverrideConfigurationCollection>()))
				.Callback((ComplianceSubTypeAllocationOverrideConfigurationCollection collection) =>
				{
					var configuration = collection.AddNew();
					configuration.Country = CountryCodes.Uruguay;
					configuration.SubType = UruguayComplianceInfo.ComplianceSubTypeCodes.TCD;
					configuration.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
				});
			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(CountryCodes.Uruguay))
				.Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Uruguay))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, allocationOverride);

				var overridenRegistryValue = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationOverride_Receivables.Value;
				var actualOverridenSubTypes = overridenRegistryValue.Cast<ComplianceSubTypeAllocationOverrideConfiguration>().Select(x => (string)x.SubType).ToArray();

				AssertContainsExactElementsInAnyOrder(new[] { UruguayComplianceInfo.ComplianceSubTypeCodes.YKT }, actualOverridenSubTypes);
			}
		}
	}
}
