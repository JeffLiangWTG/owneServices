using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.US.eManifest.Business.eManifestDocumentWrapper;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class eManifestDocumentWrapperTest : TestCaseWithFactory
	{
		[TestDate(2012, 05, 14, 16, 21, 0)]
		public void TestTripDetails()
		{
			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;

			AssertEquals("TripReference", "AAGC110098", wrapper2.TripReference);
			AssertEquals("ConveyanceLicensePlate", "EQUIPMENT\r\nUS AB32FS", wrapper2.ConveyanceLicensePlateAndDescription);
			AssertEquals("TrailerPlates", "AB32ZY, DOC123", wrapper2.TrailerPlates);
			AssertEquals("OtherEquipmentIds", "1234567890, 0789456123", wrapper2.OtherEquipmentIds);
			AssertEquals("ManifestSubmitted", new ZDateTime(2012, 05, 14, 16, 21, 0), wrapper2.ManifestSubmitted);

			AssertEquals("HazardousMaterials", "NO", wrapper1.HazardousMaterials);
			AssertEquals("HazardousMaterials", "YES", wrapper2.HazardousMaterials);

			AssertEquals("ForeignPortOfLanding", "01535 - TORONTO, ONT, CA.", wrapper1.ForeignPortOfLanding);
			AssertEquals("ForeignPortOfLanding", "VARIOUS", wrapper2.ForeignPortOfLanding);
			foreach (var shipment in trip.Shipments)
			{
				shipment.B0_PortOfLadingKCode = "01535";
			}

			AssertEquals("ForeignPortOfLanding", "01535 - TORONTO, ONT, CA.", wrapper2.ForeignPortOfLanding);

			AssertEquals("EstimatedDateOfArrival", new ZDateTime(2012, 05, 14), wrapper2.EstimatedDateOfArrival);
			AssertEquals("FirstExpectedPortOfArrival", "0901 - BUFFALO-NIAGARA FALLS", wrapper2.FirstExpectedPortOfArrival);

			AssertEquals("PersonInCharge.FullName", "CHRIS AOMAD", wrapper2.PersonInCharge.FullName);
			AssertEquals("PersonInCharge.DateOfBirth", new ZDate(1935, 09, 19), wrapper2.PersonInCharge.DateOfBirth);

			AssertEquals("Drivers.Count", 2, wrapper2.CrewMembers.Count);

			AssertEquals("Driver1.FullName", "Brian Michael Richardson", wrapper2.CrewMembers[0].FullName);
			AssertEquals("Driver1.DateOfBirth", new ZDate(1946, 12, 24), wrapper2.CrewMembers[0].DateOfBirth);

			AssertEquals("Driver2.FullName", "Henry Francis Williams", wrapper2.CrewMembers[1].FullName);
			AssertEquals("Driver2.DateOfBirth", new ZDate(1952, 07, 12), wrapper2.CrewMembers[1].DateOfBirth);
		}

		public void TestConfigDetails()
		{
			CreateRefSysConfigTypeAndRefSysConfig("CBP7533CN", "1651-0001");
			CreateRefSysConfigTypeAndRefSysConfig("CBP7533ED", "01-31-2021");
			CreateRefSysConfigTypeAndRefSysConfig("CBP7533RD", "(08/20)");

			AssertEquals("OMB Control No", "1651-0001", wrapper1.ControlNumber);
			AssertEquals("Expiration Date", "01-31-2021", wrapper1.ExpirationDate);
			AssertEquals("Revision Date", "(08/20)", wrapper1.RevisionDate);
		}

		void CreateRefSysConfigTypeAndRefSysConfig(ZString configCode, ZString stringValue)
		{
			var configType = Factory.New<RefSysConfigType>();
			configType.ZRT_ConfigCode = configCode;
			configType.ZRT_Description = "This is a config type for testing.";
			configType.ZRT_LongDescription = "This is a config type for testing.";

			var config = Factory.New<RefSysConfig>();
			config.ZRC_ZRT_NKConfigCode = configType.ZRT_ConfigCode;
			config.ZRC_StringValue = stringValue;
			config.ZRC_StartDate = ZDateTime.UtcNow.AddDays(-1);
			config.ZRC_EndDate = ZDateTime.UtcNow.AddDays(1);

			Factory.Save();
		}

		public void TestShipments()
		{
			AssertEquals("Shipments.Count", 3, wrapper2.Shipments.Count);

			var shipment = wrapper2.Shipments[0];
			AssertEquals("ShipmentType", ShipmentTypes.Descriptions.BRASS, shipment.ShipmentType);
			AssertEquals("ShipmentControlNumber", "AAGC", shipment.ShipmentControlNumber);
			AssertEquals("Packages", ZString.Empty, shipment.Packages);
			AssertEquals("CargoGrossWeight", ZString.Empty, shipment.CargoGrossWeight);
			AssertEquals("DescriptionOfCargo", ZString.Empty, shipment.DescriptionOfCargo);
			AssertEquals("InbondNumber", ZString.Empty, shipment.InbondNumber);
			AssertEquals("Equipment", "AB32FS", shipment.Equipment);
			AssertEquals("Consignee", ZString.Empty, shipment.Consignee);
			AssertEquals("ForeignPortOfLanding", ZString.Empty, shipment.ForeignPortOfLanding);

			shipment = wrapper2.Shipments[1];
			AssertNotNull(shipment);
			AssertEquals("ShipmentType", ShipmentTypes.Descriptions.PAPS, shipment.ShipmentType);
			AssertEquals("ShipmentControlNumber", "AAGC123456789012", shipment.ShipmentControlNumber);
			AssertEquals("Packages", "500 PCE", shipment.Packages);
			AssertEquals("CargoGrossWeight", "200 L", shipment.CargoGrossWeight);
			AssertEquals("DescriptionOfCargo", "Computers", shipment.DescriptionOfCargo);
			AssertEquals("InbondNumber", ZString.Empty, shipment.InbondNumber);
			AssertEquals("Equipment", ZString.Empty, shipment.Equipment);
			AssertEquals("Consignee", PartyTypes.Descriptions.Consignee, shipment.Consignee);
			AssertEquals("ForeignPortOfLanding", "01535 - TORONTO, ONT, CA.", shipment.ForeignPortOfLanding);

			shipment = wrapper2.Shipments[2];
			AssertEquals("ShipmentType", "IE - Immediate exportation", shipment.ShipmentType);
			AssertEquals("ShipmentControlNumber", "AAGC123456789013", shipment.ShipmentControlNumber);
			AssertEquals("Packages", "500 PCE", shipment.Packages);
			AssertEquals("CargoGrossWeight", "2000 K", shipment.CargoGrossWeight);
			AssertEquals("DescriptionOfCargo", "FRENCH DARK CHOCOLATE", shipment.DescriptionOfCargo);
			AssertEquals("InbondNumber", "VVV34567890", shipment.InbondNumber);
			AssertEquals("Equipment", "AB32FS, AB32ZY", shipment.Equipment);
			AssertEquals("Consignee", PartyTypes.Descriptions.Consignee, shipment.Consignee);
			AssertEquals("ForeignPortOfLanding", "76231 - CABINDA; TAKULA, ANGOLA", shipment.ForeignPortOfLanding);

			trip.BH_ReleaseStatus = TripEntryStatusList.Codes.AcceptedComplete;
			AssertEquals("Shipments.Count", 0, wrapper2.Shipments.Count);

			foreach (var shipment1 in trip.Shipments)
			{
				shipment1.B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Linked;
			}

			AssertEquals("Shipments.Count", 3, wrapper2.Shipments.Count);

			trip.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[] {
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, "HVL"),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, "job123") });
			AssertEquals(0, wrapper2.Shipments.Count);
		}

		public void TestIVisualizerNoteSupporterMembers()
		{
			var supporter = wrapper2 as IVisualizerNoteSupporter;
			AssertNotNull("eManifestDocumentWrapper should implement IVisualizerNoteSupporter", supporter);
			AssertEquals("supporter.PK", trip.PK, supporter.PK);
			AssertEquals("supporter.TableCode", CusInBondHeaderSchema.Constants.Prefix, supporter.TableCode);
			AssertEquals("supporter.ChildBusinessObjectPK", ZGuid.Empty, supporter.ChildBusinessObjectPK);
		}

		public void TestQRCodeText()
		{
			AssertEquals("V01 AAGC123456                20120514                          ", wrapper1.QRCodeText);
			AssertEquals("V01 AAGC110098                20120514 US TXAB32FS  MXCHHAB32ZY ", wrapper2.QRCodeText);
		}

		public void TestSourceIdentifierProvider()
		{
			var trip = Factory.NewWithValidTestData<Trip>();
			var wrapper = new eManifestDocumentWrapper(trip);
			var provider = wrapper as ISourceIdentifierProvider;

			AssertNotNull(provider);
			AssertEquals(trip.PK, provider.SourceIdentifier);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "01535", "TORONTO, ONT, CA.", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "76231", "CABINDA; TAKULA, ANGOLA", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0901", "BUFFALO-NIAGARA FALLS", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();
			base.SetUp();
			#region Trip 1

			trip = Factory.New<Trip>();
			trip.BH_JobReference = "MAN0000001";
			trip.BH_CarrierSCAC = "AAGC";
			trip.BH_PortUnladingDCode = "0901";
			trip.BH_ETA = new ZDateTime(2012, 05, 14);
			trip.BH_VoyageNumber = "123456";

			wrapper1 = new eManifestDocumentWrapper(trip);

			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
			shipment.B0_MasterBillNumber = "123456789012";
			shipment.B0_PortOfLadingKCode = "01535";
			shipment.B0_ManifestQty = 500;
			shipment.B0_ManifestUQ = Core.Constants.PkgUnit.Piece;
			shipment.B0_BoardedQuantity = 100;
			shipment.B0_Weight = 200;
			shipment.B0_WeightUQ = Core.Constants.Weight.Pounds;
			shipment.B0_DescriptionOfCargo = "Computers";

			var party = shipment.Consignee;
			party.E2_AddressOverride = true;
			party.E2_CompanyName = PartyTypes.Descriptions.Consignee;

			#endregion

			#region Trip 2

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_FullName = PartyTypes.Descriptions.Carrier;
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "AAGC", Core.Constants.CountryCodes.UnitedStates);

			trip = Factory.New<Trip>();
			trip.BH_JobReference = "MAN0001001";
			trip.BH_CarrierSCAC = "AAGC";
			trip.BH_PortUnladingDCode = "0901";
			trip.BH_ETA = new ZDateTime(2012, 05, 14);
			trip.BH_VoyageNumber = "110098";

			wrapper2 = new eManifestDocumentWrapper(trip);

			#region Conveyance / Equipment

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(ConveyanceTypes.Codes.BoxTruck, Enterprise.Core.Constants.CountryCodes.UnitedStates);

			var refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			refEquipment.RQ_Registration = "AB32FS";
			refEquipment.RQ_Description = "EQUIPMENT";
			refEquipment.RQ_RN_NKRegistrationCountry = Core.Constants.CountryCodes.UnitedStates;
			refEquipment.RQ_RegState = USStateList.Codes.Texas;
			trip.Conveyance.BJ_RQ_Equipment = refEquipment.PK;

			refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_IsVehicle = true;
			refEquipment.RQ_Registration = "AB32ZY";
			refEquipment.RQ_RN_NKRegistrationCountry = Core.Constants.CountryCodes.Mexico;
			refEquipment.RQ_RegState = MexicoState3CharsList.Codes.Chihuahua;
			trip.Equipment.AddNew().BJ_RQ_Equipment = refEquipment.PK;

			refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_IsVehicle = true;
			refEquipment.RQ_Registration = "DOC123";
			trip.Equipment.AddNew().BJ_RQ_Equipment = refEquipment.PK;

			refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_IsVehicle = false;
			refEquipment.RQ_Registration = "1234567890";
			trip.Equipment.AddNew().BJ_RQ_Equipment = refEquipment.PK;

			refEquipment = Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_IsVehicle = false;
			refEquipment.RQ_Registration = "0789456123";
			trip.Equipment.AddNew().BJ_RQ_Equipment = refEquipment.PK;

			#endregion

			#region Crew

			var crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.ResponsibleParty;
			crew.CP_FullName = "CHRIS AOMAD";
			crew.CP_DateOfBirth = new ZDate(1935, 09, 19);

			crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.CrewMember;
			crew.CP_FullName = "Brian Michael Richardson";
			crew.CP_DateOfBirth = new ZDate(1946, 12, 24);

			crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.CrewMember;
			crew.CP_FullName = "Henry Francis Williams";
			crew.CP_DateOfBirth = new ZDate(1952, 07, 12);

			crew = trip.CrewMembers.AddNew();
			crew.CP_Type = CrewTypes.Codes.Passenger;
			crew.CP_FullName = "Marianne Jane Martin,";

			#endregion

			#region Shipments

			shipment = trip.Shipments.AddNew();
			shipment.Commodities.DeleteAll();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			shipment.Commodities.AddNew().BY_Description = "DESC 1";
			shipment.Commodities.AddNew().BY_Description = "DESC 2";

			shipment = trip.Shipments.AddNew();
			shipment.Commodities.DeleteAll();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.SplitShipment;
			shipment.B0_MasterBillNumber = "123456789012";

			shipment = trip.Shipments.AddNew();
			shipment.Commodities.DeleteAll();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.Inbond;
			shipment.B0_MasterBillNumber = "123456789013";
			shipment.B0_PortOfLadingKCode = "76231";
			shipment.B0_ManifestQty = 500;
			shipment.B0_ManifestUQ = Core.Constants.PkgUnit.Piece;
			shipment.B0_Weight = 2;
			shipment.B0_WeightUQ = Core.Constants.Weight.Tonnes;

			party = shipment.Consignee;
			party.E2_AddressOverride = true;
			party.E2_CompanyName = PartyTypes.Descriptions.Consignee;

			var inbond = shipment.InBond;
			inbond.BM_InBondEntryType = InbondTypes.Codes.ImmediateExportation;
			inbond.InBondNumber = "VVV34567890";

			var commodity1 = shipment.Commodities[0];
			commodity1.BY_Description = "FRENCH DARK CHOCOLATE";
			commodity1.BY_BJ_Equipment = ((Equipment)commodity1.Lookups.Equipment[0]).PK;
			commodity1.UNDGs.AddNew();

			var commodity2 = shipment.Commodities.AddNew();
			commodity2.BY_Description = "FRENCH DARK CHOCOLATE";
			commodity2.BY_BJ_Equipment = ((Equipment)commodity2.Lookups.Equipment[1]).PK;

			#endregion

			#endregion
		}

		eManifestDocumentWrapper wrapper1;
		eManifestDocumentWrapper wrapper2;
		Trip trip;
	}

	[TestedType(typeof(BusinessObjectCollectionWrapper<DummyNonPersistentBusinessObject>))]
	sealed class BusinessObjectCollectionWrapperTest : NonPersistentBusinessObjectCollectionTestCase<BusinessObjectCollectionWrapper<DummyNonPersistentBusinessObject>>
	{
		protected override BusinessObjectCollectionWrapper<DummyNonPersistentBusinessObject> GetCollectionToTest()
		{
			var collection = new List<DummyNonPersistentBusinessObject>();
			return new BusinessObjectCollectionWrapper<DummyNonPersistentBusinessObject>(collection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DummyNonPersistentBusinessObject();
	}
}
