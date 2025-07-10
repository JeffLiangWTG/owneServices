using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CountrySpecificDefaultValueRegistryItemImplTest : TestCaseWithFactory
	{
		public void TestRegistryItemImpl_WhenPassedDelegateIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CountrySpecificDefaultValueRegistryItemImpl<bool>("Name", (NoResString)"Category", (NoResString)"Hint", RegistryDataTypes.BoolType, RegistryOptions.Default, Factory, null));
		}

		public void TestRegistryItemImpl_WhenPassedFactoryIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CountrySpecificDefaultValueRegistryItemImpl<bool>("Name", (NoResString)"Category", (NoResString)"Hint", RegistryDataTypes.BoolType, RegistryOptions.Default, null, new FakeCountrySpecificRegistryDescriptor_Bool((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) => true)));
		}

		public void TestGetDefaultValue_ValueComesFromPassedDelegateAndRespectsRegistryValueType()
		{
			var fakeCompany = new FakeCompany(countryCode: "Any");
			ObjectFactory.Substitute(fakeCompany.CreateMockForICompanyProvider().Object);

			var stringRegItemImpl = CreateObjectForTest(RegistryDataTypes.StringType, new FakeCountrySpecificRegistryDescriptor_String((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) => "stringValue"));
			AssertEquals("stringValue", stringRegItemImpl.GetDefaultValue(fakeCompany.PK, Guid.Empty, Guid.Empty));

			var boolRegItemImpl = CreateObjectForTest(RegistryDataTypes.BoolType, new FakeCountrySpecificRegistryDescriptor_Bool((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) => true));
			AssertEquals(true, boolRegItemImpl.GetDefaultValue(fakeCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestGetDefaultValue_DefaultFallBackValue()
		{
			var fakeCompany = new FakeCompany(countryCode: "Any");
			SetupRequiredMocksForTest(new[] { fakeCompany });

			var stringRegItemImpl = CreateObjectForTest(RegistryDataTypes.StringType, new FakeCountrySpecificRegistryDescriptor_String((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) => "valueFromDelegate"));

			AssertEquals(string.Empty, stringRegItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty));
			AssertEquals(string.Empty, stringRegItemImpl.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("valueFromDelegate", stringRegItemImpl.GetDefaultValue(fakeCompany.PK, Guid.Empty, Guid.Empty));
		}

		[ExpectNoExceptions]
		public void TestGetDefaultValue_CorrectCountryCodeIsPassed()
		{
			var uzCompany = new FakeCompany(countryCode: Constants.CountryCodes.Uzbekistan);
			var auCompany = new FakeCompany(countryCode: Constants.CountryCodes.Australia);

			var (mockICountrySpecificRegistryDefaultValuesProvider, _) = SetupRequiredMocksForTest(new[] { uzCompany, auCompany });

			var regItemImpl = CreateObjectForTest(RegistryDataTypes.StringType, new FakeCountrySpecificRegistryDescriptor_String((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) => "valueDoesNotMatter"));

			regItemImpl.GetDefaultValue(auCompany.PK, Guid.Empty, Guid.Empty);
			mockICountrySpecificRegistryDefaultValuesProvider.Verify(x => x.Get("AU"));

			regItemImpl.GetDefaultValue(uzCompany.PK, Guid.Empty, Guid.Empty);
			mockICountrySpecificRegistryDefaultValuesProvider.Verify(x => x.Get("UZ"));
		}

		[ExpectNoExceptions]
		public void TestGetDefaultValue_WhenInvalidCompanyPK_DefaultValuesProviderNotInvoked()
		{
			var uzCompany = new FakeCompany(countryCode: Constants.CountryCodes.Uzbekistan);
			var (mockICountrySpecificRegistryDefaultValuesProvider, _) = SetupRequiredMocksForTest(new[] { uzCompany });

			var regItemImpl = CreateObjectForTest(RegistryDataTypes.StringType, new FakeCountrySpecificRegistryDescriptor_String((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) => "valueDoesNotMatter"));

			regItemImpl.GetDefaultValue(Guid.Empty, Guid.Empty, Guid.Empty);
			mockICountrySpecificRegistryDefaultValuesProvider.Verify(x => x.Get(It.IsAny<ZString>()), Times.Never);

			regItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty);
			mockICountrySpecificRegistryDefaultValuesProvider.Verify(x => x.Get(It.IsAny<ZString>()), Times.Never);

			regItemImpl.GetDefaultValue(uzCompany.PK, Guid.Empty, Guid.Empty);
			mockICountrySpecificRegistryDefaultValuesProvider.Verify(x => x.Get(It.IsAny<ZString>()));
		}

		public void TestGetDefaultValue_ArgumentForDefaultValueGetterIsCorrect()
		{
			var uzCompany = new FakeCompany(countryCode: Constants.CountryCodes.Uzbekistan);
			var (mockICountrySpecificRegistryDefaultValuesProvider, mockIDefaultValuesForCountrySpecificRegistryItems) = SetupRequiredMocksForTest(new[] { uzCompany });

			var regItemImpl = CreateObjectForTest(RegistryDataTypes.StringType, new FakeCountrySpecificRegistryDescriptor_String((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) =>
			{
				AssertEquals(defaultValuesForCountrySpecificRegistryItems, mockIDefaultValuesForCountrySpecificRegistryItems.Object);
				return "delegateWasCalled";
			}));

			AssertEquals("Postcondition", "delegateWasCalled", regItemImpl.GetDefaultValue(uzCompany.PK, Guid.Empty, Guid.Empty));
		}

		public void TestGetDefaultValue_UsesConstructorFactory()
		{
			var regItemImpl = CreateObjectForTest(RegistryDataTypes.StringType, new FakeCountrySpecificRegistryDescriptor_String((NoResString)"Caption", (defaultValuesForCountrySpecificRegistryItems) => "valueDoesNotMatter"));

			var loadCountBefore = Factory.DatabaseLoadCount;
			regItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty);
			var loadCountAfter = Factory.DatabaseLoadCount;
			var loadCount = loadCountAfter - loadCountBefore;
			AssertEquals("The constructor factory is used for loading business objects", 1, loadCount);
		}

		#region Implementation

		CountrySpecificDefaultValueRegistryItemImpl<T> CreateObjectForTest<T>(IRegistryDataType dataType, CountrySpecificDefaultRegistryDescriptor<T> countrySpecificDefaultRegistryDescriptor)
			=> new CountrySpecificDefaultValueRegistryItemImpl<T>("TestRegistry", (NoResString)"Category", (NoResString)"Hint", dataType, RegistryOptions.Default, Factory, countrySpecificDefaultRegistryDescriptor);

		(Mock<ICountrySpecificRegistryDefaultValuesProvider>, Mock<IDefaultValuesForCountrySpecificRegistryItems>) SetupRequiredMocksForTest(IEnumerable<ICompany> companies = null)
		{
			var mockICountrySpecificRegistryDefaultValuesProvider = new Mock<ICountrySpecificRegistryDefaultValuesProvider>();
			ObjectFactory.Substitute(mockICountrySpecificRegistryDefaultValuesProvider.Object);

			var mockIDefaultValuesForCountrySpecificRegistryItems = new Mock<IDefaultValuesForCountrySpecificRegistryItems>();
			mockICountrySpecificRegistryDefaultValuesProvider.Setup(x => x.Get(It.IsAny<ZString>())).Returns(mockIDefaultValuesForCountrySpecificRegistryItems.Object);

			var mockICompanyProvider = new Mock<ICompanyProvider>();
			foreach (var company in companies ?? Enumerable.Empty<ICompany>())
			{
				company.RegisterWithMock(mockICompanyProvider);
			}
			ObjectFactory.Substitute(mockICompanyProvider.Object);

			return (mockICountrySpecificRegistryDefaultValuesProvider, mockIDefaultValuesForCountrySpecificRegistryItems);
		}

		#endregion
	}
}
