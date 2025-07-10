using System;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Reports.Testing
{
	[TestedType(typeof(USRegionDistrictPortCollectionProvider))]
	sealed class USRegionDistrictPortCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(ZZRefCusCodeListCombinedCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.Customs.US.RegionDistrictPort;

		protected override int ExpectedMaxLength => 4;

		protected override void SetUp()
		{
			base.SetUp();
			var port = Factory.New<ZZRefCusCodeListCombined>();
			port.ZZD_Code = "1001";
			port.ZZD_Description = "DUMMY PORT";
			port.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			port.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.UnitedStates;
		}
	}
}
