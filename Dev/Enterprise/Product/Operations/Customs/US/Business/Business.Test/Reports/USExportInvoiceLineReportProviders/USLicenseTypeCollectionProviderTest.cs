using System;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Reports.Testing
{
	[TestedType(typeof(USLicenseTypeCollectionProvider))]
	sealed class USLicenseTypeCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(USAESLicenseCodeCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.Universal.ZZRefCusCodeList;

		protected override int ExpectedMaxLength => AutoUSAddInfo.Schema.US_LicenseTypeMaxLength;

		protected override void SetUp()
		{
			base.SetUp();
			var licenseType = Factory.New<ZZRefCusCodeListCombined>();
			licenseType.ZZD_Code = "C30";
			licenseType.ZZD_Description = "DESC";
			licenseType.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode;
			licenseType.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
		}
	}
}
