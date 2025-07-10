using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class CusOutturnHeaderDataObjectReaderTest : OutturnDataObjectReaderTestHelper<CusOutturnHeader, CusOutturn>
	{
		public void TestGetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotNull(outturnHeader);
			AssertEquals("Get existed outturnHeader PK", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("Get existed outturnHeader C6_SendersMessageReference", existOutturnHeader.C6_SendersMessageReference, outturnHeader.C6_SendersMessageReference);

			shipment.VoyageFlightNo = "1234";
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotNull(outturnHeader);
			AssertNotEquals("No existed outturnHeader and create new PK", existOutturnHeader.PK, outturnHeader.PK);
			AssertNotEquals("No existed outturnHeader and create new C6_SendersMessageReference", existOutturnHeader.C6_SendersMessageReference, outturnHeader.C6_SendersMessageReference);
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO()
		{
			var reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get exist OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			Assert("No Log error", !logger.HasErrors);

			var updatedOutturnHeader = CreateOuttrunHeader(refVessel, premise, "Q125", "O00000125");
			shipment.DataContext.DataTargetCollection.Single().Key = updatedOutturnHeader.C6_SendersMessageReference;
			shipment.VoyageFlightNo = "Q123";
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			reader.ReadIntoBusinessObject();

			Assert("Has Log error", logger.HasErrors);
			AssertContains("Stop import Log", "A Sea Cargo Outturn O00000123 already exists with the same Vessel '9832343', Voyage 'Q123' and Premise ID 'CP123'. Can't update Sea Cargo Outturn O00000125", logger.Logs);
			AssertEquals("Stop import and OutturnHeader not be updated", "Q125", updatedOutturnHeader.C6_VoyageNum);
		}

		public void TestImportingShipmentData()
		{
			var reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("Update existed OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
				AssertEquals("C6_VoyageNum", "Q123", outturnHeader.C6_VoyageNum);
				AssertEquals("C6_DateOfArrival", new ZDateTime(2019, 08, 07), outturnHeader.C6_DateOfArrival);
				AssertEquals("C6_MessageStatus", "", outturnHeader.C6_MessageStatus);
				AssertEquals("C6_ResponsiblePartyID", "RE123", outturnHeader.C6_ResponsiblePartyID);
				AssertEquals("C6_LloydsIMO", "9832343", outturnHeader.C6_LloydsIMO);
				AssertEquals("C6_VesselName", "VESSEL", outturnHeader.C6_VesselName);
				AssertEquals("C6_OutturningPremiseID", "CP123", outturnHeader.C6_OutturningPremiseID);
				AssertEquals("C6_OA_OutturningPremise", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);
			});

			shipment.VoyageFlightNo = "TEST0";
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotEquals("Add new OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
				AssertEquals("C6_VoyageNum", "TEST0", outturnHeader.C6_VoyageNum);
				AssertEquals("C6_DateOfArrival", new ZDateTime(2019, 08, 07), outturnHeader.C6_DateOfArrival);
				AssertEquals("C6_MessageStatus", "", outturnHeader.C6_MessageStatus);
				AssertEquals("C6_ResponsiblePartyID", "RE123", outturnHeader.C6_ResponsiblePartyID);
				AssertEquals("C6_LloydsIMO", "9832343", outturnHeader.C6_LloydsIMO);
				AssertEquals("C6_VesselName", "VESSEL", outturnHeader.C6_VesselName);
				AssertEquals("C6_OutturningPremiseID", "CP123", outturnHeader.C6_OutturningPremiseID);
				AssertEquals("C6_OA_OutturningPremise", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);
			});
		}

		public void TestImportingSubShipmentData()
		{
			var reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Outturn imported", outturnHeader.Outturns);
				AssertEquals("Outturn count", 1, outturnHeader.Outturns.Count);
				AssertEquals("Outturn C5_HouseBill", "HB123", outturnHeader.Outturns[0].C5_HouseBill);
			});
		}

		public void TestDeleteUnprocessedSubShipmentData()
		{
			CreateOuttrun("FCL", "CON123", "MB123", "HB123", existOutturnHeader);
			CreateOuttrun("FCL", "CON123", "MB123", "HB124", existOutturnHeader);
			CreateOuttrun("FCL", "CON123", "MB123", "HB125", existOutturnHeader);
			AssertEquals("Exist Outturn count", 3, existOutturnHeader.Outturns.Count);

			shipment.DataContext.DataTargetCollection.Single().Key = existOutturnHeader.C6_SendersMessageReference;
			Factory.SaveForTesting();

			var reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Update existed outturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("Updated Outturn count", 1, outturnHeader.Outturns.Count);
			AssertEquals("Updated Outturn C5_HouseBill", "HB123", outturnHeader.Outturns[0].C5_HouseBill);
		}

		public void TestImportingPrimiseAddressForNewOutturnHeader()
		{
			shipment.VoyageFlightNo = "TEST0";
			Factory.SaveForTesting();

			var reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("One premise address in xml", 1, shipment.OrganizationAddressCollection.Count);
			AssertEquals("outturnHeader.C6_OA_OutturningPremise: Set matched address Guid when matched address", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);

			shipment.VoyageFlightNo = "TEST1";
			shipment.SetOrganizationAddressCollection(() => null);
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			AssertNull("No premise address in xml", shipment.OrganizationAddressCollection);
			AssertEquals("outturnHeader.C6_OA_OutturningPremise: Set empty when no address", ZGuid.Empty, outturnHeader.C6_OA_OutturningPremise);

			shipment.VoyageFlightNo = "TEST2";
			var primiseAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = AddressTypes.DepotAddress,
				CompanyName = "ZZZ",
			};
			shipment.AddOrgAddress(primiseAddress);
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertNotEquals("Add new OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("One premise address in xml", 1, shipment.OrganizationAddressCollection.Count);
			AssertEquals("outturnHeader.C6_OA_OutturningPremise: Set empty when no matched address", ZGuid.Empty, outturnHeader.C6_OA_OutturningPremise);
		}

		public void TestImportingPrimiseAddressForExistOutturnHeader()
		{
			var reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get existed OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("outturnHeader.C6_OA_OutturningPremise: Set matched address Guid when matched address", premise.MainAddress.PK, outturnHeader.C6_OA_OutturningPremise);

			shipment.SetOrganizationAddressCollection(() => null);
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get existed OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			AssertNull("No premise address in xml", shipment.OrganizationAddressCollection);
			AssertEquals("outturnHeader.C6_OA_OutturningPremise: Not set and keep current value when XML has no premise address", existOutturnHeader.C6_OA_OutturningPremise, outturnHeader.C6_OA_OutturningPremise);

			var primiseAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = AddressTypes.ArrivalCFSAddress,
				CompanyName = "ZZZ",
			};
			shipment.AddOrgAddress(primiseAddress);
			Factory.SaveForTesting();

			reader = new CusOutturnHeaderDataObjectReader<CusOutturnHeader, CusOutturn>(shipment, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals("Get existed OutturnHeader", existOutturnHeader.PK, outturnHeader.PK);
			AssertEquals("One premise address in xml", 1, shipment.OrganizationAddressCollection.Count);
			AssertEquals("outturnHeader.C6_OA_OutturningPremise: Set empty when XML has premise address but not matched", ZGuid.Empty, outturnHeader.C6_OA_OutturningPremise);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.China);
			refVessel = CreateRefVesselForTest();
			premise = CreatePremiseAddressForTest();
			Factory.SaveForTesting();

			existOutturnHeader = CreateOuttrunHeader(refVessel, premise, "Q123", "O00000123");
			shipment = CreateTestOutturnHeaderShipment(refVessel, premise, "Q123", "CN");
			subShipment = CreateTestOutturnShipment("FCL", "CON123", "MB123", "HB123");
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment> { subShipment });
			Factory.SaveForTesting();
		}

		OrgHeader premise;
		RefVessel refVessel;
		CusOutturnHeader existOutturnHeader;
		Shipment shipment;
		Shipment subShipment;
	}
}
