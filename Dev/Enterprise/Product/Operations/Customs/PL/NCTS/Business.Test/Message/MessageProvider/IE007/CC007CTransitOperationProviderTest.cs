using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC007CTransitOperationProviderTest : Customs.Business.Testing.DataProviderTestCase<CC007CTransitOperationProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsArrivalMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new CC007CTransitOperationProvider(null));
			var movementHeaderWithoutNctsHeader = Factory.New<NctsArrivalMovementHeader>();
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: movementHeader.Header", () => new CC007CTransitOperationProvider(movementHeaderWithoutNctsHeader));
			AssertNoExceptionThrown("NctsArrivalMovementHeader is not null", () => new CC007CTransitOperationProvider(movementHeader));
		});
	}

	public void TestMRN()
	{
		CombineAssertions(() =>
		{
			nctsHeader.ArrivalMrnFromUser = string.Empty;
			AssertEquals("MRN is empty", string.Empty, GetProvider().MRN);

			nctsHeader.ArrivalMrnFromUser = "123";
			AssertEquals("MRN equals 123", "123", GetProvider().MRN);
		});
	}

	[TestDate(TestYear, TestMonth, TestDay)]
	public void TestArrivalNotificationDateAndTime()
	{
		CombineAssertions(() =>
		{
			var testDateTime = DateTime.Now.AddDays(3);
			movementHeader.BM_ArrivalDate = testDateTime;
			AssertEquals("ArrivalNotificationDateAndTime value when BM_ArrivalDate is not empty", testDateTime, GetProvider().ArrivalNotificationDateAndTime);

			movementHeader.BM_ArrivalDate = ZDateTime.Empty;
			var expectedDateTime = new DateTime(TestYear, TestMonth, TestDay);
			AssertEquals("ArrivalNotificationDateAndTime has value if BM_ArrivalDate is empty", expectedDateTime, GetProvider().ArrivalNotificationDateAndTime);
		});
	}

	public void TestSimplifiedProcedure()
	{
		CombineAssertions(() =>
		{
			movementHeader.AuthorizationNumber = ZString.Empty;
			AssertEquals("SimplifiedProcedure is false - no authorisation", NCTSIndicator.NO, GetProvider().SimplifiedProcedure);

			movementHeader.AuthorizationNumber = "123";
			AssertEquals("SimplifiedProcedure is true - Authorisation is present", NCTSIndicator.YES, GetProvider().SimplifiedProcedure);
		});
	}

	public void TestIncidentFlag()
	{
		CombineAssertions(() =>
		{
			nctsHeader.BH_ExportFlag = string.Empty;
			AssertEquals("BH_ExportFlag is empty", NCTSIndicator.NO, GetProvider().IncidentFlag);

			nctsHeader.BH_ExportFlag = YesNoList.Codes.Yes;
			AssertEquals("BH_ExportFlag equals Y", NCTSIndicator.YES, GetProvider().IncidentFlag);

			nctsHeader.BH_ExportFlag = YesNoList.Codes.No;
			AssertEquals("BH_ExportFlag equals N", NCTSIndicator.NO, GetProvider().IncidentFlag);
		});
	}

	protected override CC007CTransitOperationProvider GetProvider()
	{
		return new CC007CTransitOperationProvider(movementHeader);
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		movementHeader = nctsHeader.ArrivalMovementHeader;
	}

	const int TestYear = 2020;
	const int TestMonth = 10;
	const int TestDay = 30;

	NctsHeader nctsHeader;
	NctsArrivalMovementHeader movementHeader;
}
