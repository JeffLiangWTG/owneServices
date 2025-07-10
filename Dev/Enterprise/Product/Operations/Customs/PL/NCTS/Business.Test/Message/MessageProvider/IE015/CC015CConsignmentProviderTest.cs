using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.PL.NCTS.Business.Testing.Message.MessageProvider.Common;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC015CConsignmentProviderTest : CC013015ConsignmentProviderTest<CC015CConsignmentProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: transportMeansProvider", () => new CC015CConsignmentProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: movementHeader.Header", () => new CC015CConsignmentProvider(Factory.New<NctsDepartureMovementHeader>(), null));
			AssertExceptionThrown<ArgumentNullException>("Null ICC015C", "Value cannot be null.\r\nParameter name: cc015cProvider", () => new CC015CConsignmentProvider(movementHeader, null));
		});
	}

	public void TestActiveBorderTransportMeans_ItemsLimit_InPhase5()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
			movementHeader.BM_TOLCarrierID = "A";
			AssertEquals($"Only main item", 1, GetProvider().ActiveBorderTransportMeans.Count);

			var additionalTransportMeans = movementHeader.AdditionalTransportAtBorderList.AddNew();
			additionalTransportMeans.TPM_IdentificationNumber = "B";
			AssertEquals($"Only main item (additional is not included)", 1, GetProvider().ActiveBorderTransportMeans.Count);
		});
	}

	public void TestActiveBorderTransportMeans_MultipleItems_OutsidePhase5()
	{
		TestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
		{
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			movementHeader.BM_TOLCarrierID = "A";
			AssertEquals($"Only main item", 1, GetProvider().ActiveBorderTransportMeans.Count);

			nctsHeader.MovementHeader.CustomsOffices.AddNew().CY_Code = "TRA";
			movementHeader.AdditionalTransportAtBorderList.AddNew().TPM_IdentificationNumber = "B";
			AssertEquals($"Main item and one additional item", 2, GetProvider().ActiveBorderTransportMeans.Count);

			for (int i = 0; i < 10; i++)
			{
				movementHeader.AdditionalTransportAtBorderList.AddNew().TPM_IdentificationNumber = "B";
			}
			AssertEquals($"ActiveBorderTransportMeans has a limit = 9", 9, GetProvider().ActiveBorderTransportMeans.Count);

			AssertEquals("The sequence numeration starts with 1", "1", GetProvider().ActiveBorderTransportMeans.First().SequenceNumber);
			AssertEquals("The sequence numeration ends with 9", "9", GetProvider().ActiveBorderTransportMeans.Last().SequenceNumber);
		});
	}

	public void TestActiveBorderTransportMeans_B1806_and_C0806_InPhase5()
	{
		TestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
		{
			foreach (var mode in AllMeansOfTransportExcept_5_PostalConsignment)
			{
				movementHeader.BM_ExportTransportMode = mode;
				AssertEquals($"BM_ExportTransportMode is {mode} and all fields are empty", 1, GetProvider().ActiveBorderTransportMeans.Count);
			}

			movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
			AssertEquals("BM_ExportTransportMode is 5 and all fields are empty", 0, GetProvider().ActiveBorderTransportMeans.Count);

			AssertActiveBorderTransportMeansCountForFields(1, "BM_ExportTransportMode is 5");
		});
	}

	public void TestActiveBorderTransportMeans_B1806_and_C0806_OutsidePhase5()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._5_PostalConsignment;
			AssertEquals("BM_ExportTransportMode is 5 and all fields are empty", 0, GetProvider().ActiveBorderTransportMeans.Count);

			AssertActiveBorderTransportMeansCountForFields(0, "BM_ExportTransportMode is 5");

			foreach (var mode in AllMeansOfTransportExcept_5_PostalConsignment)
			{
				movementHeader.BM_ExportTransportMode = mode;

				movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
				var description = $"BM_ExportTransportMode is {mode} and BM_AdditionalDeclarationType = A";
				AssertEquals(description, 0, GetProvider().ActiveBorderTransportMeans.Count);

				foreach (var typeOfSecurity in new[] { NctsTypeOfSecurityList.Codes.ENT, NctsTypeOfSecurityList.Codes.EXI, NctsTypeOfSecurityList.Codes.BTH })
				{
					movementHeader.BM_TypeOfSecurity = typeOfSecurity;
					AssertEquals($"{description} and BM_TypeOfSecurity = {typeOfSecurity}", 1, GetProvider().ActiveBorderTransportMeans.Count);
					movementHeader.BM_TypeOfSecurity = ZString.Empty;
				}

				AssertActiveBorderTransportMeansCountForFields(1, description);
			}
		});
	}

	void AssertActiveBorderTransportMeansCountForFields(int expected, string description)
	{
		movementHeader.BM_CustomsOfficeAtBorder = "A";
		AssertEquals($"{description} and BM_CustomsOfficeAtBorder not empty", expected, GetProvider().ActiveBorderTransportMeans.Count);

		movementHeader.BM_CustomsOfficeAtBorder = ZString.Empty;
		movementHeader.BM_ActiveBorderIdentificationType = "A";
		AssertEquals($"{description} and BM_ActiveBorderIdentificationType not empty", expected, GetProvider().ActiveBorderTransportMeans.Count);

		movementHeader.BM_ActiveBorderIdentificationType = ZString.Empty;
		movementHeader.BM_TOLCarrierID = "A";
		AssertEquals($"{description} and BM_TOLCarrierID not empty", expected, GetProvider().ActiveBorderTransportMeans.Count);

		movementHeader.BM_TOLCarrierID = ZString.Empty;
		movementHeader.BM_RN_NKTOLCarrierNationality = "A";
		AssertEquals($"{description} and BM_RN_NKTOLCarrierNationality not empty", expected, GetProvider().ActiveBorderTransportMeans.Count);

		movementHeader.BM_RN_NKTOLCarrierNationality = ZString.Empty;
		movementHeader.BM_ConveyanceNumber = "A";
		AssertEquals($"{description} and BM_ConveyanceNumber not empty", expected, GetProvider().ActiveBorderTransportMeans.Count);
		movementHeader.BM_ConveyanceNumber = ZString.Empty;
	}

	public void TestHouseConsignment()
	{
		nctsHeader.Bills.DeleteAll();
		nctsHeader.Bills.AddNew().SequenceNumber = 2;
		nctsHeader.Bills.AddNew().SequenceNumber = 1;

		CombineAssertions(() =>
		{
			AssertEquals("Should have 2 HouseConsignments", 2, GetProvider().HouseConsignment.Count);
			AssertEquals("Sequence number should start from 1", "1", GetProvider().HouseConsignment.First().SequenceNumber);
			AssertEquals("Sequence number for 2nd HouseConsignments should be 2", "2", GetProvider().HouseConsignment.Last().SequenceNumber);
		});
	}

	public void TestAdditionalSupplyChainActor()
	{
		nctsHeader.CusSupplyChainActors.AddNew();
		nctsHeader.CusSupplyChainActors.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Should have 2 HouseConsignments", 2, GetProvider().AdditionalSupplyChainActor.Count);
			AssertEquals("Sequence number should start from 1", 1, GetProvider().AdditionalSupplyChainActor.First().SequenceNumber);
			AssertEquals("Sequence number for 2nd HouseConsignments should be 2", 2, GetProvider().AdditionalSupplyChainActor.Last().SequenceNumber);
		});
	}

	protected override void SetTransportTypeAtDeparture(string value) => movementHeader.TransportTypeAtDeparture = value;

	protected override void SetTransportAtDeparture(string transport) => movementHeader.TransportAtDeparture = transport;

	protected override void SetTrailer1IDAtDeparture(string value) => movementHeader.Trailer1IDAtDeparture = value;

	protected override void SetTrailer2IDAtDeparture(string value) => movementHeader.Trailer2IDAtDeparture = value;

	protected override void SetVesselNameAtDeparture(string value) => movementHeader.VesselNameAtDeparture = value;

	ICC015C GetCC015CProvider() => new CC015CProvider(movementHeader, "CC015C");

	protected override CC015CConsignmentProvider GetProvider() => new CC015CConsignmentProvider(movementHeader, GetCC015CProvider());
}
