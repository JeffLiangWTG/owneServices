using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CPackagingProvider))]
sealed class CC044CPackagingProviderTest : Customs.Business.Testing.DataProviderTestCase<CC044CPackagingProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CPackagingProvider(null));

	public void TestTypeOfPackages() => CombineAssertions(() =>
	{
		package.B5_UnitType = "KG";
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		provider = new CC044CPackagingProvider(package);
		AssertEquals("KG", GetProvider().TypeOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		provider = new CC044CPackagingProvider(package);
		AssertNullOrEmpty(GetProvider().TypeOfPackages);
	});

	public void TestNumberOfPackages() => CombineAssertions(() =>
	{
		package.B5_UnitCount = 5;
		package.B5_UnitType = "VG";
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		provider = new CC044CPackagingProvider(package);
		AssertNull("UnloadedState MIS", GetProvider().NumberOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		provider = new CC044CPackagingProvider(package);
		AssertEquals("UnloadedState NEW", 5, GetProvider().NumberOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		package.B5_UnitCount = 0;
		package.B5_UnitType = "NE";
		provider = new CC044CPackagingProvider(package);
		AssertNull("UnloadedState MIS - bulk", GetProvider().NumberOfPackages);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		provider = new CC044CPackagingProvider(package);
		AssertNull("UnloadedState NEW - bulk", GetProvider().NumberOfPackages);
	});

	public void TestShippingMarks() => CombineAssertions(() =>
	{
		package.B5_MarksAndNumbers = "marks";
		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		provider = new CC044CPackagingProvider(package);
		AssertEquals("marks", GetProvider().ShippingMarks);

		package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		provider = new CC044CPackagingProvider(package);
		AssertNullOrEmpty(GetProvider().ShippingMarks);
	});

	protected override CC044CPackagingProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

		package = Factory.New<NctsPackage>();
		package.B5_UnitType = "NE";

		provider = new CC044CPackagingProvider(package);
	}
	NctsPackage package;
	CC044CPackagingProvider provider;
}
