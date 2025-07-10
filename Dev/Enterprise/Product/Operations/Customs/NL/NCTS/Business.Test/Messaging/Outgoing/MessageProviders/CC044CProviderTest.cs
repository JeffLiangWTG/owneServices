using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CProvider))]
sealed class CC044CProviderTest : MessageHeaderProviderAbstractTest<CC044CProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CProvider(null));

	public void TestMRN()
	{
		nctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "MRN";
		AssertEquals("MRN", Provider.MRN);
	}

	public void TestOtherThingsToReport()
	{
		nctsHeader.ArrivalMovementHeader.OtherThingsToReport = "OtherThingsToReport";
		AssertEquals("OtherThingsToReport", Provider.OtherThingsToReport);
	}

	public void TestCustomsOfficeOfDestination()
	{
		nctsHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "CusOffID";
		AssertEquals("CustomsOfficeOfDestination", "CusOffID", Provider.CustomsOfficeOfDestination);
	}

	public void TestTraderIdentificationNumber()
	{
		NCTSTestHelper.CreateJobDocAddressForTest(Factory, "IMD", nctsHeader.DestinationTrader, countryCode: "NL", traderTin: "152425232B01", suffix: "");
		CreateProvider();
		AssertEquals("TraderIdentificationNumber", "NL152425232B01", Provider.TraderIdentificationNumber);
	}

	public void TestConform()
	{
		CombineAssertions(() =>
		{
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			AssertEquals(true, provider.Conform);
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
			AssertEquals(false, provider.Conform);
		});
	}

	public void TestUnloadingCompletion()
	{
		CombineAssertions(() =>
		{
			nctsHeader.ArrivalMovementHeader.BM_UnloadingCompleted = true;
			AssertEquals(true, provider.UnloadingCompletion);
			nctsHeader.ArrivalMovementHeader.BM_UnloadingCompleted = false;
			AssertEquals(false, provider.UnloadingCompletion);
		});
	}

	public void TestStateOfSeals()
	{
		CombineAssertions(() =>
		{
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			var containerOnHeader = nctsHeader.ArrivalHeaderContainers.AddNew();
			containerOnHeader.Seals.AddNew();
			containerOnHeader.BC_UnloadedState = "MIS";
			CreateProvider();
			AssertEquals("BM_StateOfSealsBoolean: true, Seals on Header: true, Seals on Incident: false", "1", provider.StateOfSeals);

			containerOnHeader.Seals.RemoveAll();
			var containerOnIncident = nctsHeader.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
			containerOnIncident.Seals.AddNew();
			CreateProvider();
			AssertEquals("BM_StateOfSealsBoolean: true, Seals on Header: false, Seals on Incident: true", "1", provider.StateOfSeals);

			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			CreateProvider();
			AssertEquals("BM_StateOfSealsBoolean: false, Seals on Header: false, Seals on Incident: true", "0", provider.StateOfSeals);

			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			containerOnIncident.Seals.RemoveAll();
			CreateProvider();
			AssertEquals("BM_StateOfSealsBoolean: true, Seals on Header: false, Seals on Incident: false", null, provider.StateOfSeals);

			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
			CreateProvider();
			AssertEquals("BM_StateOfSealsBoolean: false, Seals on Header: false, Seals on Incident: false", null, provider.StateOfSeals);
		});
	}

	public void TestUnloadingdate()
	{
		CombineAssertions(() =>
		{
			var date = new DateTime(1994, 2, 1);
			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = date;
			AssertEquals(date, Provider.Unloadingdate);

			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = ZDateTimeOffset.Empty;
			AssertEquals("Unloadingdate should be MinValue from Empty", DateTime.MinValue, Provider.Unloadingdate);

			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(DateTime.MinValue);
			AssertEquals("Unloadingdate should be MinValue from MinValue", DateTime.MinValue, Provider.Unloadingdate);
		});
	}

	public void TestUnloadingRemark()
	{
		nctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks = "UnloadingRemark";
		AssertEquals("UnloadingRemark", provider.UnloadingRemark);
	}

	public void TestConsignment()
	{
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
		var transportInfo = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
		transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
		var house = nctsHeader.Bills.AddNew();
		house.UnloadedStatus = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
		var container = nctsHeader.ArrivalHeaderContainers.AddNew();
		container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
		var seal = container.Seals.AddNew();
		seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;

		CombineAssertions(() =>
		{
			var provider = new CC044CProvider(nctsHeader);
			AssertNull("No consignment has to be passed when all unloaded states are equal to DEC and BM_NoChangesToReport = true", provider.Consignment);

			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
			provider = new CC044CProvider(nctsHeader);
			AssertNull("BM_NoChangesToReport = false, house = DEC", provider.Consignment);

			house.UnloadedStatus = NctsUnloadedStateListForHouseConsignment.Codes.DIF;
			provider = new CC044CProvider(nctsHeader);
			AssertNotNull("BM_NoChangesToReport = false, house = DIF", provider.Consignment);

			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			provider = new CC044CProvider(nctsHeader);
			AssertNull("BM_NoChangesToReport = true, house = DIF", provider.Consignment);

			house.UnloadedStatus = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
			transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
			provider = new CC044CProvider(nctsHeader);
			AssertNotNull("ArrivalTransportInfo.TPM_TransportState = DIF", provider.Consignment);

			transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			container.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			provider = new CC044CProvider(nctsHeader);
			AssertNotNull("ArrivalHeaderContainers.BC_UnloadedState = DIF", provider.Consignment);

			container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
			provider = new CC044CProvider(nctsHeader);
			AssertNotNull("ArrivalHeaderContainers.Seals.BK_UnloadingState = DIF", provider.Consignment);

			seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;
			provider = new CC044CProvider(nctsHeader);
			AssertNull("ArrivalHeaderContainers.Seals.BK_UnloadingState = DAM", provider.Consignment);
		});
	}

	protected override string MessageType => "CC044C";

	protected override string MovementType => NctsMovementType.Codes.Arrival;
}
