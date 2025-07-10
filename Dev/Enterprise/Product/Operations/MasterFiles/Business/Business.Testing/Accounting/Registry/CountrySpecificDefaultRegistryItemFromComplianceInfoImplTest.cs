using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Compliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CountrySpecificDefaultRegistryItemFromComplianceInfoImplTest : TestCaseWithFactory
	{
		public void TestGetDefaultValue()
		{
			var twCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany1.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;
			var uzCompany = Factory.NewWithValidTestData<GlbCompany>();
			uzCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Uzbekistan;
			var isCompany = Factory.NewWithValidTestData<GlbCompany>();
			isCompany.GC_RN_NKCountryCode = Constants.CountryCodes.Iceland;
			Factory.Save();

			var twCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			twCompany2.GC_RN_NKCountryCode = Constants.CountryCodes.Taiwan;

			using (InitaliseComplianceFactory())
			{
				var regItemImpl = CreateObjectForTest((complianceInfo) => complianceInfo?.GetComplianceVersionNo() ?? "NoCountryVersionNo");

				AssertEquals("default", regItemImpl.GetDefaultValue(Guid.NewGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("TaiwanVersionNo", regItemImpl.GetDefaultValue(twCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("default", regItemImpl.GetDefaultValue(twCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("UzbekistanVersionNo", regItemImpl.GetDefaultValue(uzCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
				AssertEquals("NoCountryVersionNo", regItemImpl.GetDefaultValue(isCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				Factory.Save();
				AssertEquals("TaiwanVersionNo", regItemImpl.GetDefaultValue(twCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		#region Implementation

		static CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<string> CreateObjectForTest(Func<ICountryComplianceInfo, string> defaultValueGetter)
			=> new CountrySpecificDefaultRegistryItemFromComplianceInfoImpl<string>("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new StringRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default, defaultValueGetter, "default");

		static IDisposable InitaliseComplianceFactory()
		{
			var alMockInfo = new Mock<ICountryComplianceInfo>();
			alMockInfo.Setup(x => x.GetComplianceVersionNo()).Returns("AlbaniaVersionNo");
			var twMockInfo = new Mock<ICountryComplianceInfo>();
			twMockInfo.Setup(x => x.GetComplianceVersionNo()).Returns("TaiwanVersionNo");
			var uzMockInfo = new Mock<ICountryComplianceInfo>();
			uzMockInfo.Setup(x => x.GetComplianceVersionNo()).Returns("UzbekistanVersionNo");

			var mockComplianceFactory = new Mock<ICountryComplianceFactoryIntegration>();
			mockComplianceFactory.Setup((x) => x.GetICountryComplianceInfo(It.Is<ZString>(c => c == Constants.CountryCodes.Albania))).Returns(alMockInfo.Object);
			mockComplianceFactory.Setup((x) => x.GetICountryComplianceInfo(It.Is<ZString>(c => c == Constants.CountryCodes.Taiwan))).Returns(twMockInfo.Object);
			mockComplianceFactory.Setup((x) => x.GetICountryComplianceInfo(It.Is<ZString>(c => c == Constants.CountryCodes.Uzbekistan))).Returns(uzMockInfo.Object);

			return ObjectFactory.Substitute(mockComplianceFactory.Object);
		}

		#endregion
	}
}
