using CargoWise.Definitions.Customs;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class CusOutturnHeaderDataObjectReaderForTransitWarehouseTest : OutturnDataObjectReaderTestHelper<CusOutturnHeader, CusOutturn>
	{
		public void TestImportingData_MissingAllOutturnHeaderMatchingDetails()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() =>
				new DataObjectList<Container>
				{
					new Container() { ContainerNumber = "CTN1" }
				}
			);

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Vessel Lloyds, Premise ID and Voyage Flight Number",
@"UXML received could not be used to match with any Sea Cargo Outturn because of below errors. Correct them and try again.
Vessel Lloyds is not found.
Premise ID is not found.
Voyage Flight Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MissingContainer()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment container collection is empty",
@"No Container found from the UXML received.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_ContainerNumberIsEmpty()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() =>
			new DataObjectList<Container>
			{
				new Container() { ContainerNumber = ZString.Empty },
			});

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("No container number in the container collection",
@"No Container number found from the UXML received.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MultipleContainers()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() =>
			new DataObjectList<Container>
			{
				new Container() { ContainerNumber = "CTN1" },
				new Container() { ContainerNumber = "CTN2" }
			}
		);
			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment container collection contains more than one element",
@"Multiple Containers found from the UXML received. Container Number(s): CTN1, CTN2.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MissingPremiseIDAndVoyageFlightNumber()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, "CTN1", "Lloyds", ZString.Empty, ZString.Empty);

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Premise ID and Voyage Flight Number",
@"UXML received could not be used to match with any Sea Cargo Outturn because of below errors. Correct them and try again.
Premise ID is not found.
Voyage Flight Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MissingVesselLloydsAndPremiseID()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, "CTN1", ZString.Empty, "Voyage", ZString.Empty);

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Vessel Lloyds and Premise ID",
@"UXML received could not be used to match with any Sea Cargo Outturn because of below errors. Correct them and try again.
Vessel Lloyds is not found.
Premise ID is not found.
", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MissingVesselLloydsAndVoyageFlightNumber()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, "CTN1", ZString.Empty, ZString.Empty, "PremiseID");

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Shipment doesn't have Vessel Lloyds and Voyage Flight Number",
@"UXML received could not be used to match with any Sea Cargo Outturn because of below errors. Correct them and try again.
Vessel Lloyds is not found.
Voyage Flight Number is not found.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_NotMatchingSeaCargoOutturn()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, "CTN1", "Lloyds", "Voyage", "PremiseID");

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("No matching sea cargo outturn header found",
@"No matching Sea Cargo Outturn found for Vessel Lloyds/IMO: Lloyds, Premise ID: PremiseID and Voyage Flight Number: Voyage.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MatchingMultipleSeaCargoOutturn()
		{
			var outturnHeader1 = Factory.New<CusOutturnHeader>();
			outturnHeader1.C6_LloydsIMO = "Lloyds";
			outturnHeader1.C6_VoyageNum = "Voyage";
			outturnHeader1.C6_OutturningPremiseID = "PremiseID";

			var outturnHeader2 = Factory.New<CusOutturnHeader>();
			outturnHeader2.C6_LloydsIMO = "Lloyds";
			outturnHeader2.C6_VoyageNum = "Voyage";
			outturnHeader2.C6_OutturningPremiseID = "PremiseID";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, "CTN1", "Lloyds", "Voyage", "PremiseID");

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			AssertExceptionThrown<DataObjectReadFailureException>("Matched to multiple Sea Cargo Outturn Headers",
@"Multiple matching Sea Cargo Outturn found for Vessel Lloyds/IMO: Lloyds, Premise ID: PremiseID and Voyage Flight Number: Voyage.", () => reader.ReadIntoBusinessObject());
		}

		public void TestImportingData_MatchingSeaCargoOutturn()
		{
			var outturnHeader1 = Factory.New<CusOutturnHeader>();
			outturnHeader1.C6_LloydsIMO = "Lloyds";
			outturnHeader1.C6_VoyageNum = "Voyage";
			outturnHeader1.C6_OutturningPremiseID = "PremiseID";
			CreateOutturn("FCL", "CTN1", "MAB1", "", outturnHeader1);

			var outturnHeader2 = Factory.New<CusOutturnHeader>();
			outturnHeader2.C6_LloydsIMO = "Lloyds2";
			outturnHeader2.C6_VoyageNum = "Voyage2";
			outturnHeader2.C6_OutturningPremiseID = "PremiseID2";

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, "CTN1", "Lloyds", "Voyage", "PremiseID");

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			var matchingSeaCargoOutturn = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				Assert(logger.Logs.Contains("Information - Matching Sea Cargo Outturn found for Vessel Lloyds/IMO: Lloyds, Premise ID: PremiseID and Voyage Flight Number: Voyage."));
				AssertEquals(outturnHeader1.PK, matchingSeaCargoOutturn.PK);
			});
		}

		public void TestCusOutturnHeaderDataObjectReaderForTransitWarehouse_HasCalculatedProperties()
		{
			var outturnHeader1 = Factory.New<CusOutturnHeader>();
			outturnHeader1.C6_LloydsIMO = "Lloyds";
			outturnHeader1.C6_VoyageNum = "Voyage";
			outturnHeader1.C6_OutturningPremiseID = "PremiseID";
			CreateOutturn("FCL", "CTN1", "MAB1", "", outturnHeader1);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, "CTN1", "Lloyds", "Voyage", "PremiseID");

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("hasCalculatedPremiseID should be false.", reader.hasCalculatedPremiseID, false);
				AssertEquals("hasCalculatedLloyds should be false.", reader.hasCalculatedLloyds, false);
				AssertEquals("hasCalculatedVoyageFlightNo should be false.", reader.hasCalculatedVoyageFlightNo, false);
			});

			var matchingSeaCargoOutturn = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("hasCalculatedPremiseID should be true.", reader.hasCalculatedPremiseID, true);
				AssertEquals("hasCalculatedLloyds should be true", reader.hasCalculatedLloyds, true);
				AssertEquals("hasCalculatedVoyageFlightNo should be true", reader.hasCalculatedVoyageFlightNo, true);
			});
		}

		public void TestPremiseID()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, ZString.Empty, ZString.Empty, ZString.Empty, "PremiseID");

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);

			AssertEquals("PremiseID", reader.PremiseID);
		}

		public void TestLloydsIMO()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, ZString.Empty, "LloydsIMO", ZString.Empty, ZString.Empty);

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);

			AssertEquals("LloydsIMO", reader.LloydsIMO);
		}

		public void TestVoyageFlightNo()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			SetupValidShipmentDataForCusOutturnHeader(shipment, ZString.Empty, ZString.Empty, "VoyageFlightNo", ZString.Empty);

			var reader = new CusOutturnHeaderDataObjectReaderForTransitWarehouse(shipment, logger, Factory);

			AssertEquals("VoyageFlightNo", reader.VoyageFlightNo);
		}

		void SetupValidShipmentDataForCusOutturnHeader(Shipment shipment, ZString containerNumber, ZString lloydsNo, ZString voyageNo, ZString premiseID)
		{
			if (!containerNumber.IsEmpty)
			{
				shipment.SetContainerCollection(() =>
					new DataObjectList<Container>
					{
						new Container() { ContainerNumber = containerNumber }
					}
				);
			}

			if (!lloydsNo.IsEmpty)
			{
				shipment.LloydsIMO = lloydsNo;
			}

			if (!voyageNo.IsEmpty)
			{
				shipment.VoyageFlightNo = voyageNo;
			}

			if (!premiseID.IsEmpty)
			{
				shipment.SetAdditionalReferenceCollection(() =>
				new DataObjectList<AdditionalReference>
				{
					new AdditionalReference
					{
						Type = new EntryType
						{
							Code = CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID,
							Description = CustomsAdditionalReferenceTypes.EntryType.Descriptions.ControlledPremiseID
						},
						ContextInformation = GlbCompany.CurrentCompany.Country.Code,
						ReferenceNumber = premiseID
					}
				});
			}
		}
	}
}
