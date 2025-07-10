using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.eTail.Business.Testing;

sealed class ESCustomsStatusStoreTest : TestCaseWithFactory
{
	public void TestGetAllRefCusCodeList()
	{
		HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "CLR", "CSTA - Cleared", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.Spain);
		HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "ERR", "CSTA - Error", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.Spain);
		HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "CAN", "CSTI - Cancelled", HVLVReleaseStatus.Held, RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.UnitedStates);
		HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "UNK", "CSTI - Unknown", HVLVReleaseStatus.None, RefCusCodeListTypes.Codes.CustomsStatusForInterface, CountryCodes.Spain);

		var expectedCodeList = new[]
		{
				("CLR", "CSTA - Cleared"),
				("ERR", "CSTA - Error"),
				("MUL", "Multiple"),
			};

		var customsStatusStore = new ESCustomsStatusStore(Factory);
		var importCodeList = customsStatusStore.GetAllRefCusCodeList(true);
		AssertCodeDescriptionPairList("Should include only CSTA codes of Spain and MUL", importCodeList, expectedCodeList);
	}

	public void TestGetReleaseStatus()
	{
		HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "PDA", "Pre-Declaration accepted", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.Spain);

		var customsStatusStore = new ESCustomsStatusStore(Factory);
		var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

		CombineAssertions(() =>
		{
			var releaseStatus = customsStatusStore.GetReleaseStatus("PDA", true, declaration);
			AssertEquals("Should use CSTA codes", HVLVReleaseStatus.Cleared, releaseStatus);

			releaseStatus = customsStatusStore.GetReleaseStatus("MUL", true, declaration);
			AssertEquals("Should return HLD for MUL", HVLVReleaseStatus.Held, releaseStatus);
		});
	}

	public void TestGetCustomStatusDescription()
	{
		HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "PDA", "Pre-Declaration accepted", HVLVReleaseStatus.Cleared, RefCusCodeListTypes.Codes.CustomsStatus, CountryCodes.Spain);

		var customsStatusStore = new ESCustomsStatusStore(Factory);
		var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

		CombineAssertions(() =>
		{
			var description = customsStatusStore.GetCustomStatusDescription("PDA", true, declaration);
			AssertEquals("Should use CSTA codes", "Pre-Declaration accepted", description);

			description = customsStatusStore.GetCustomStatusDescription("MUL", true, declaration);
			AssertEquals("Should return Multiple for MUL", "Multiple", description);
		});
	}
}
