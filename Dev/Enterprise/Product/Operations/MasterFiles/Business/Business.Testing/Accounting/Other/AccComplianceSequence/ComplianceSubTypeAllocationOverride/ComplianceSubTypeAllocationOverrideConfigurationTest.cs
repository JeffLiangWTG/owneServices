using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceSubTypeAllocationOverrideConfiguration))]
	class ComplianceSubTypeAllocationOverrideConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestConstructor()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			AssertEquals(CountryCodes.Brazil, item.Country);
			AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, item.AllocationMethod);
			AssertEquals(ZGuid.Empty, item.BranchPK);

			var defaultConstructorItem = new ComplianceSubTypeAllocationOverrideConfiguration();
			AssertEquals(ZString.Empty, defaultConstructorItem.Country);
			AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, defaultConstructorItem.AllocationMethod);
			AssertEquals(ZGuid.Empty, defaultConstructorItem.BranchPK);
		}

		public void TestDefaultAllocationMethod_IsBasedOnRegistry()
		{
			var fallbackLevel = NewFallbackLevel();

			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
			{
				var item = new ComplianceSubTypeAllocationOverrideConfiguration(fallbackLevel, Factory, CountryCodes._TemplateCountryName_);
				AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print, item.AllocationMethod);
			}

			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				var item = new ComplianceSubTypeAllocationOverrideConfiguration(fallbackLevel, Factory, CountryCodes._TemplateCountryName_);
				AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, item.AllocationMethod);
			}
		}

		public void TestBoundProperties_Brazil()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			AssertEquals(BrazilComplianceInfo.ComplianceSubTypeCodes.CNS, item.SubType);
			AssertEquals(BrazilComplianceInfo.ComplianceSubTypeDescriptions.CNS, item.SubTypeDescription);
			AssertEquals(BrazilComplianceInfo.ComplianceSubTypeLocalDescriptions.CNS, item.SubTypeDocumentTitle);
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, item.AllocationMethod);
			item.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.FirstActiveBranch.PK, item.BranchPK);

			item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			AssertEquals(BrazilComplianceInfo.ComplianceSubTypeCodes.NFE, item.SubType);
			AssertEquals(BrazilComplianceInfo.ComplianceSubTypeDescriptions.NFE, item.SubTypeDescription);
			AssertEquals(BrazilComplianceInfo.ComplianceSubTypeLocalDescriptions.NFE, item.SubTypeDocumentTitle);
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, item.AllocationMethod);
			item.BranchPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, item.BranchPK);
		}

		public void TestBoundProperties_India()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.India);
			item.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			AssertEquals(IndiaComplianceInfo.ComplianceSubTypeCodes.TXI, item.SubType);
			AssertEquals(IndiaComplianceInfo.ComplianceSubTypeDescriptions.TXI, item.SubTypeDescription);
			AssertEquals(IndiaComplianceInfo.ComplianceSubTypeLocalDescriptions.TXI, item.SubTypeDocumentTitle);
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual, item.AllocationMethod);
			item.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.FirstActiveBranch.PK, item.BranchPK);

			item.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.BSD;
			AssertEquals(IndiaComplianceInfo.ComplianceSubTypeCodes.BSD, item.SubType);
			AssertEquals(IndiaComplianceInfo.ComplianceSubTypeDescriptions.BSD, item.SubTypeDescription);
			AssertEquals(IndiaComplianceInfo.ComplianceSubTypeLocalDescriptions.BSD, item.SubTypeDocumentTitle);
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			AssertEquals(AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post, item.AllocationMethod);
			item.BranchPK = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, item.BranchPK);
		}

		public void TestCountryIsReadonly()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes._TemplateCountryName_);
			Assert(item.CountryInfo.ReadOnly);
		}

		public void TestCompany()
		{
			var currentCompanyFallback = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(currentCompanyFallback, Factory, CountryCodes._TemplateCountryName_);
			AssertEquals(GlbCompany.CurrentCompany.PK, item.Company.PK);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var otherCompanyFallback = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var otherCompanyItem = new ComplianceSubTypeAllocationOverrideConfiguration(otherCompanyFallback, Factory, CountryCodes._TemplateCountryName_);
			AssertEquals(company.PK, otherCompanyItem.Company.PK);

			var nullCompanyItem = new ComplianceSubTypeAllocationOverrideConfiguration();
			AssertNull(nullCompanyItem.Company);
		}

		#region Validation Tests

		public void TestSubTypeIsRequired()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			AssertNoError(item.SubTypeInfo, "Please enter a Compliance Sub Type.");

			item.SubType = ZString.Empty;
			AssertHasError(item.SubTypeInfo, "Please enter a Compliance Sub Type.");
		}

		public void TestSubTypeMustBeInLookup()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			AssertNoError(item.SubTypeInfo, "Enter a valid Sub-Type.");

			item.SubType = "123";
			AssertHasError(item.SubTypeInfo, "Enter a valid Sub-Type.");
		}

		public void TestSubTypeDrawsFromOtherSubTypeBooks()
		{
			var fallbackLevel = NewFallbackLevel();
			var dependencyCollection = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.Value;
			// Regular dependency XNC > XND
			var dependencyItem = dependencyCollection.AddNew();
			dependencyItem.Country = CountryCodes.Brazil;
			dependencyItem.ChildSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
			dependencyItem.ParentSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;

			// Dependency chain NFS > NFE > CNS
			dependencyItem = dependencyCollection.AddNew();
			dependencyItem.Country = CountryCodes.Brazil;
			dependencyItem.ChildSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
			dependencyItem.ParentSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			dependencyItem = dependencyCollection.AddNew();
			dependencyItem.Country = CountryCodes.Brazil;
			dependencyItem.ChildSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
			dependencyItem.ParentSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;

			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeDependencyConfiguration.SetTemporaryValue(
					fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty,
					dependencyCollection))
			{
				var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(fallbackLevel, Factory, CountryCodes.Brazil);
				var item = collection.AddNew();
				item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNE;
				AssertNoError(item.SubTypeInfo, "This Sub Type draws numbers from a book that belongs to another Sub Type. Please select the Sub Type that owns that book to setup rules for all Sub Types associated with that book.");
				AssertNoWarning(item.SubTypeInfo, "You are setting a rule for a Sub Type that shares its book with other sub types, be aware that the rule will apply to all Sub Types that draw numbers from that book.");

				item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
				AssertNoError(item.SubTypeInfo, "This Sub Type draws numbers from a book that belongs to another Sub Type. Please select the Sub Type that owns that book to setup rules for all Sub Types associated with that book.");
				AssertHasWarning(item.SubTypeInfo, "You are setting a rule for a Sub Type that shares its book with other sub types, be aware that the rule will apply to all Sub Types that draw numbers from that book.");

				item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XNC;
				AssertHasError(item.SubTypeInfo, "This Sub Type draws numbers from a book that belongs to another Sub Type. Please select the Sub Type that owns that book to setup rules for all Sub Types associated with that book.");
				AssertNoWarning(item.SubTypeInfo, "You are setting a rule for a Sub Type that shares its book with other sub types, be aware that the rule will apply to all Sub Types that draw numbers from that book.");

				item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.XND;
				AssertNoError(item.SubTypeInfo, "This Sub Type draws numbers from a book that belongs to another Sub Type. Please select the Sub Type that owns that book to setup rules for all Sub Types associated with that book.");
				AssertHasWarning(item.SubTypeInfo, "You are setting a rule for a Sub Type that shares its book with other sub types, be aware that the rule will apply to all Sub Types that draw numbers from that book.");

				item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFE;
				AssertHasError(item.SubTypeInfo, "This Sub Type draws numbers from a book that belongs to another Sub Type. Please select the Sub Type that owns that book to setup rules for all Sub Types associated with that book.");
				AssertNoWarning(item.SubTypeInfo, "You are setting a rule for a Sub Type that shares its book with other sub types, be aware that the rule will apply to all Sub Types that draw numbers from that book.");

				item.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				AssertHasError(item.SubTypeInfo, "This Sub Type draws numbers from a book that belongs to another Sub Type. Please select the Sub Type that owns that book to setup rules for all Sub Types associated with that book.");
				AssertNoWarning(item.SubTypeInfo, "You are setting a rule for a Sub Type that shares its book with other sub types, be aware that the rule will apply to all Sub Types that draw numbers from that book.");
			}
		}

		public void TestAllocationMethodIsRequired()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			AssertNoError(item.AllocationMethodInfo, "Please enter an Allocation Method.");

			item.AllocationMethod = ZString.Empty;
			AssertHasError(item.AllocationMethodInfo, "Please enter an Allocation Method.");
		}

		public void TestAllocationMethodMustBeInLookup()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			AssertNoError(item.AllocationMethodInfo, "Enter a valid Allocation Method.");

			item.AllocationMethod = "123";
			AssertHasError(item.AllocationMethodInfo, "Enter a valid Allocation Method.");
		}

		public void TestAllocationMethodMustPassCountryComplianceValidation()
		{
			using (ObjectFactory.Substitute(CreateDefaultProviderMockForErrorMessage(string.Empty)))
			{
				var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes._TemplateCountryName_);
				item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
				AssertNoError(item.AllocationMethodInfo, "Some validation error about Allocation Method.");
			}

			using (ObjectFactory.Substitute(CreateDefaultProviderMockForErrorMessage("Some validation error about Allocation Method.")))
			{
				var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes._TemplateCountryName_);
				item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
				AssertHasError(item.AllocationMethodInfo, "Some validation error about Allocation Method.");
			}
		}

		public void TestAllocationMethodCountryComplianceValidation_GVTIsNotAllowedForUndefinedCountries()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes._TemplateCountryName_);
			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			AssertNoError(item.AllocationMethodInfo, "'GVT' is not valid for country/region 'AI'.");

			item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate;
			AssertHasError(item.AllocationMethodInfo, "'GVT' is not valid for country/region 'AI'.");
		}

		public void TestSameAllocationMethodAsCompanyRegistryShowsWarning()
		{
			var fallbackLevel = NewFallbackLevel();
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(
					fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes._TemplateCountryName_);
				item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
				AssertNoWarning(item.AllocationMethodInfo, "You are selecting the same method as the Company level rule. This registry should be used to set different methods. Please select a different value.");

				item.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
				AssertHasWarning(item.AllocationMethodInfo, "You are selecting the same method as the Company level rule. This registry should be used to set different methods. Please select a different value.");
			}
		}

		public void TestAllocationMethodIsNotValidatedUntilFallbackLevelIsSet()
		{
			var fallbackLevel = NewFallbackLevel();
			using (ObjectFactory.Substitute(CreateDefaultProviderMockForErrorMessageWhenEInvoicingIsDisabled("EInvoicing is Disabled. Validation Error.")))
			{
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(
						fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty,
						true))
				{
					var deserialisedItem = new ComplianceSubTypeAllocationOverrideConfiguration();
					deserialisedItem.Country = CountryCodes._TemplateCountryName_;
					deserialisedItem.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
					AssertNull("During deserialiation, the CurrentFallbackLevel is not set until after all properties are deserialised", deserialisedItem.CurrentFallbackLevel);
					AssertNoError("Before CurrentFallbackLevel is set, validation of AllocationMethod against eInvoicing registry is skipped", deserialisedItem.AllocationMethodInfo, "EInvoicing is Disabled. Validation Error.");

					deserialisedItem.CurrentFallbackLevel = fallbackLevel;
					deserialisedItem.ValidateAllocationMethod();
					AssertNoError("After CurrentFallbackLevel is set, validation of AllocationMethod against eInvoicing registry is run", deserialisedItem.AllocationMethodInfo, "EInvoicing is Disabled. Validation Error.");
				}

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(
						fallbackLevel.CompanyPK(false), Guid.Empty, Guid.Empty,
						false))
				{
					var deserialisedItem = new ComplianceSubTypeAllocationOverrideConfiguration();
					deserialisedItem.Country = CountryCodes._TemplateCountryName_;
					deserialisedItem.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
					AssertNull("During deserialiation, the CurrentFallbackLevel is not set until after all properties are deserialised", deserialisedItem.CurrentFallbackLevel);
					AssertNoError("Before CurrentFallbackLevel is set, validation of AllocationMethod against eInvoicing registry is skipped", deserialisedItem.AllocationMethodInfo, "EInvoicing is Disabled. Validation Error.");

					deserialisedItem.CurrentFallbackLevel = fallbackLevel;
					deserialisedItem.ValidateAllocationMethod();
					AssertHasError("After CurrentFallbackLevel is set, validation of AllocationMethod against eInvoicing registry is run", deserialisedItem.AllocationMethodInfo, "EInvoicing is Disabled. Validation Error.");
				}
			}
		}

		public void TestBranchPKIsOptional()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			item.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			Assert(!item.BranchPKInfo.HasNotifications());

			item.BranchPK = ZGuid.Empty;
			Assert(!item.BranchPKInfo.HasNotifications());
		}

		public void TestBranchPKMustBeInList()
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, CountryCodes.Brazil);
			item.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			AssertNoError(item.BranchPKInfo, "Enter a valid Branch.");

			item.BranchPK = ZGuid.NewZGuid();
			AssertHasError(item.BranchPKInfo, "Enter a valid Branch.");
		}

		public void TestBranchPKIsNotValidatedUntilFallbackLevelIsSet()
		{
			var fallbackLevel = NewFallbackLevel();

			var deserialisedItem = new ComplianceSubTypeAllocationOverrideConfiguration();
			deserialisedItem.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			AssertNull("During deserialiation, the CurrentFallbackLevel is not set until after all properties are deserialised", deserialisedItem.CurrentFallbackLevel);
			AssertNoError("Before CurrentFallbackLevel is set, validation of BranchPK is skipped", deserialisedItem.BranchPKInfo, "Enter a valid Branch.");

			deserialisedItem.CurrentFallbackLevel = fallbackLevel;
			deserialisedItem.ValidateAllocationMethod();
			AssertNoError("After CurrentFallbackLevel is set, validation of BranchPK is run", deserialisedItem.BranchPKInfo, "Enter a valid Branch.");

			deserialisedItem.BranchPK = ZGuid.NewZGuid();
			AssertHasError("After CurrentFallbackLevel is set, validation of BranchPK is run", deserialisedItem.BranchPKInfo, "Enter a valid Branch.");
		}

		public void TestDuplicateRuleValidation()
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(NewFallbackLevel(), Factory, CountryCodes.India);
			var item1 = collection.AddNew();
			item1.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			item1.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			item1.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			var item2 = collection.AddNew();
			item2.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			item2.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;

			item2.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			AssertHasError(item2.BranchPKInfo, "Identical override configuration detected. Duplicate rules are not permitted.");

			item2.BranchPK = ZGuid.Empty;
			AssertNoError(item2.BranchPKInfo, "Identical override configuration detected. Duplicate rules are not permitted.");
			item2.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			AssertHasError(item2.BranchPKInfo, "Identical override configuration detected. Duplicate rules are not permitted.");

			item2.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
			AssertNoError(item2.SubTypeInfo, "Identical override configuration detected. Duplicate rules are not permitted.");
			item2.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			AssertHasError(item2.SubTypeInfo, "Identical override configuration detected. Duplicate rules are not permitted.");

			item2.Country = CountryCodes._TemplateCountryName_;
			AssertNoError(item2.CountryInfo, "Identical override configuration detected. Duplicate rules are not permitted.");
			item2.Country = CountryCodes.India;
			AssertHasError(item2.CountryInfo, "Identical override configuration detected. Duplicate rules are not permitted.");

			// Different AllocationMethod alone will trigger the Conflicting Rule Validation, see below.
		}

		public void TestConflictingRuleValidation()
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(NewFallbackLevel(), Factory, CountryCodes.India);
			var item1 = collection.AddNew();
			item1.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual;
			item1.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			item1.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			var item2 = collection.AddNew();
			item2.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			item2.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;

			item2.AllocationMethod = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			AssertHasError(item2.AllocationMethodInfo, "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted.");

			item2.BranchPK = ZGuid.Empty;
			AssertNoError(item2.BranchPKInfo, "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted.");
			item2.BranchPK = GlbCompany.CurrentCompany.FirstActiveBranch.PK;
			AssertHasError(item2.BranchPKInfo, "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted.");

			item2.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXC;
			AssertNoError(item2.SubTypeInfo, "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted.");
			item2.SubType = IndiaComplianceInfo.ComplianceSubTypeCodes.TXI;
			AssertHasError(item2.SubTypeInfo, "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted.");

			item2.Country = CountryCodes._TemplateCountryName_;
			AssertNoError(item2.CountryInfo, "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted.");
			item2.Country = CountryCodes.India;
			AssertHasError(item2.CountryInfo, "Conflicting override configuration detected. Identical rules for same Sub-Type and Branch, but different Allocation Method are not permitted.");
		}

		#endregion

		#region Implementation

		ComplianceSubTypeAllocationOverrideConfiguration GetNewObjectToTest(
			string subType = null,
			string allocation = null,
			string countryCode = null
		)
		{
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(NewFallbackLevel(), Factory, countryCode ?? CountryCodes.Brazil);
			item.SubType = subType ?? BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
			item.AllocationMethod = allocation ?? AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
			return item;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
			=> GetNewObjectToTest();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
			=> new ComplianceSubTypeAllocationOverrideConfiguration();

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected new ComplianceSubTypeAllocationOverrideConfiguration BizObj
			=> (ComplianceSubTypeAllocationOverrideConfiguration)base.BizObj;

		ICountryComplianceFactory CreateDefaultProviderMockForErrorMessage(string errorMessageToReturn)
		{
			var mock = new Mock<IComplianceRegistryDefaultProvider>();
			mock.Setup(x => x.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(It.IsAny<string>(), It.IsAny<bool>())).Returns(errorMessageToReturn);

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			mockComplianceFactory.Setup((x) => x.GetIComplianceRegistryDefaultProvider(It.IsAny<ZString>())).Returns(mock.Object);
			return mockComplianceFactory.Object;
		}

		ICountryComplianceFactory CreateDefaultProviderMockForErrorMessageWhenEInvoicingIsDisabled(string errorMessageToReturn)
		{
			var mock = new Mock<IComplianceRegistryDefaultProvider>();
			mock.Setup(x => x.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(It.IsAny<string>(), It.Is(true, EqualityComparer<bool>.Default))).Returns(string.Empty);
			mock.Setup(x => x.ValidateComplianceDocumentNumberAllocationOverride_ReceivablesRegistry(It.IsAny<string>(), It.Is(false, EqualityComparer<bool>.Default))).Returns(errorMessageToReturn);

			var mockComplianceFactory = new Mock<ICountryComplianceFactory>();
			mockComplianceFactory.Setup((x) => x.GetIComplianceRegistryDefaultProvider(It.IsAny<ZString>())).Returns(mock.Object);
			return mockComplianceFactory.Object;
		}

		#endregion
	}
}
