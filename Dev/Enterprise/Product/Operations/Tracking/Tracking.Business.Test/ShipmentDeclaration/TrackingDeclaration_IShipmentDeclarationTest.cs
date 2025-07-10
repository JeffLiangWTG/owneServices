using System;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingDeclaration_IShipmentDeclarationTest : IShipmentDeclarationTest
	{
		public void TestEmptyLists_IShipmentDeclaration()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			AssertEquals(0, declaration.ChargesApply_List.Count);
			AssertEquals(0, declaration.ReleaseType_List.Count);
			AssertEquals(0, declaration.OnBoard_List.Count);
			AssertEquals(0, declaration.PaymentTerm_List.Count);
		}

		#region TestPersistentBizOPK

		public override void TestPersistentBizOPK()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			AssertEquals(declaration.Declaration.PK, ((IShipmentDeclaration)declaration).PersistentBizOPK);
		}

		#endregion

		#region TestLoadPortCode

		public override void TestCurrentLoadPort()
		{
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();

			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_RL_NKPortOfLoading = port.RL_Code;

			AssertEquals(port.RL_Code, declaration.CurrentLoadPort);
		}

		#endregion

		#region TestDischargePortCode

		public override void TestCurrentDischargePort()
		{
			RefUNLOCO port = Factory.NewWithValidTestData<RefUNLOCO>();

			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_RL_NKPortOfArrival = port.RL_Code;

			AssertEquals(port.RL_Code, declaration.CurrentDischargePort);
		}

		#endregion

		#region TestNumber

		public override void TestNumber()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_DeclarationReference = "001";

			AssertEquals("001", declaration.Number);
		}

		#endregion

		#region TestHouseBill

		public override void TestHouseBill()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_HouseBill = "001";

			AssertEquals("001", declaration.HouseBill);
		}

		#endregion

		#region TestMasterBill

		public override void TestMasterBill()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_MasterBill = "001";

			AssertEquals("001", declaration.MasterBill);
		}

		#endregion

		#region TestOriginPortCode

		public override void TestOriginPortCode()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_RL_NKOrigin = "AUSYD";
			AssertEquals("AUSYD", declaration.OriginPortCode);
		}

		#endregion

		#region TestDestinationPortCode

		public override void TestDestinationPortCode()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_RL_NKFinalDestination = "AUSYD";
			AssertEquals("AUSYD", declaration.DestinationPortCode);
		}

		#endregion

		#region TestConsignorProperties

		public override void TestConsignorProperties()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_OH_Supplier = TestOrg.PK;
			WebAddressFormatter formatter = new WebAddressFormatter(declaration.Declaration.Consignor.Addresses[0]);

			AssertEquals("SHould be one address", 1, TestOrg.Addresses.Count);
			AssertEquals("Company Name", TestOrg.OH_FullName, declaration.ConsignorName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName(TestOrg.OH_FullName), declaration.ConsignorFullAddress);
			AssertEquals("Address1", TestOrg.Addresses[0].OA_Address1, declaration.ConsignorAddress);
			AssertEquals("City", TestOrg.Addresses[0].OA_City, declaration.ConsignorCity);
			AssertEquals("State", TestOrg.Addresses[0].OA_State, declaration.ConsignorState);
			AssertEquals("Post Code", TestOrg.Addresses[0].OA_PostCode, declaration.ConsignorPostCode);
		}

		#endregion

		#region TestConsigneeProperties

		public override void TestConsigneeProperties()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_OH_Importer = TestOrg.PK;
			WebAddressFormatter formatter = new WebAddressFormatter(declaration.Declaration.Consignee.Addresses[0]);

			AssertEquals("SHould be one address", 1, TestOrg.Addresses.Count);
			AssertEquals("Company Name", TestOrg.OH_FullName, declaration.ConsigneeName);
			AssertEquals("Full Address", formatter.FormattedAddressWithCompanyName(TestOrg.OH_FullName), declaration.ConsigneeFullAddress);
			AssertEquals("Address1", TestOrg.Addresses[0].OA_Address1, declaration.ConsigneeAddress);
			AssertEquals("City", TestOrg.Addresses[0].OA_City, declaration.ConsigneeCity);
			AssertEquals("State", TestOrg.Addresses[0].OA_State, declaration.ConsigneeState);
			AssertEquals("Post Code", TestOrg.Addresses[0].OA_PostCode, declaration.ConsigneePostCode);
		}

		#endregion

		#region TestETA

		public override void TestETA()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();

			DateTime testDate = new DateTime(2009, 01, 01);
			declaration.Declaration.JE_DateAtFinalDestination = testDate;

			AssertEquals(testDate, declaration.ETA);
		}

		#endregion

		#region TestBookingReference

		public override void TestBookingReference()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			AssertEquals(ZString.Empty, declaration.BookingReference);
		}

		#endregion

		#region TestOwnerReference

		public override void TestOwnerReference()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_OwnerRef = "001";

			AssertEquals("001", declaration.OwnerReference);
		}

		#endregion

		#region TestTransportMode

		public void TestTransportMode()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals(Core.Constants.TransportModes.Air, declaration.TransportMode);
		}

		#endregion

		#region TestPacksWithUnits

		public override void TestPacksWithUnits()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_TotalNoOfPacks = 5;
			declaration.Declaration.JE_TotalNoOfPacksPackType = "PLT";
			AssertEquals("5 PLT", declaration.PacksWithUnits);
		}

		#endregion

		#region TestVolumeWithUnits

		public override void TestVolumeWithUnits()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_TotalVolume = 5;
			declaration.Declaration.JE_TotalVolumeUnit = "M3";
			AssertEquals("5 M3", declaration.VolumeWithUnits);
		}

		#endregion

		#region TestWeightWithUnits

		public override void TestWeightWithUnits()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_TotalWeight = 5;
			declaration.Declaration.JE_TotalWeightUnit = "KG";
			AssertEquals("5 KG", declaration.WeightWithUnits);
		}

		#endregion

		#region TestGoodsProperties

		public override void TestGoodsProperties()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_GoodsDescription = "Drugs";
			AssertEquals("Drugs", declaration.GoodsDescription);
			AssertEquals(ZDecimal.Zero, declaration.GoodsValue);
			AssertEquals(ZString.Empty, declaration.GoodsValueCurrency);
		}

		#endregion

		#region TestDocsAndCartageProperties

		public override void TestDocsAndCartageProperties()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			AssertNotNull("DocsAndCartage", declaration.Declaration.DocsAndCartage);

			ZDateTime testDate = new ZDateTime(2009, 01, 19);

			declaration.Declaration.DocsAndCartage.JP_EstimatedPickup = testDate;
			declaration.Declaration.DocsAndCartage.JP_PickupRequiredBy = testDate.AddDays(1);
			declaration.Declaration.DocsAndCartage.JP_EstimatedDelivery = testDate.AddDays(2);
			declaration.Declaration.DocsAndCartage.JP_DeliveryRequiredBy = testDate.AddDays(3);
			declaration.Declaration.DocsAndCartage.JP_DeliveryCartageCompleted = testDate.AddDays(4);
			declaration.Declaration.DocsAndCartage.JP_PickupCartageCompleted = testDate.AddDays(5);

			AssertEquals("EstimatedPickupDate", testDate, declaration.EstimatedPickupDate);
			AssertEquals("PickupDateRequiredBy", testDate.AddDays(1), declaration.PickupDateRequiredBy);
			AssertEquals("EstimatedDeliveryDate", testDate.AddDays(2), declaration.EstimatedDeliveryDate);
			AssertEquals("DeliveryDateRequiredBy", testDate.AddDays(3), declaration.DeliveryDateRequiredBy);
			AssertEquals("DeliveryDate", testDate.AddDays(4), declaration.DeliveryDate);
			AssertEquals("ActualPickupDate", testDate.AddDays(5), declaration.ActualPickupDate);
		}

		#endregion

		#region TestServiceLevelCode

		public override void TestServiceLevelCode()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_RS_NKServiceLevel = "D2D";
			AssertEquals("D2D", declaration.ServiceLevelCode);
		}

		#endregion

		#region TestDeliveredLegProperties

		public override void TestDeliveredLegProperties()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();

			AssertEquals("ReceivedDate", ZDateTime.Empty, declaration.ReceivedDate);
			AssertEquals("ReceivedBy", ZString.Empty, declaration.ReceivedBy);
			AssertEquals("PiecesReceived", ZInt.Zero, declaration.PiecesReceived);
		}

		#endregion

		#region TestBookedOnline

		public override void TestBookedOnline()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			Assert("Declaration can't be booked online", !declaration.BookedOnline);
		}

		#endregion

		#region TestTop3Containers

		public override void TestTop3Containers()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();

			AssertEquals("Should be no containers", ZString.Empty, declaration.Top3Containers);

			BaseCusContainer cusContainer1 = declaration.Declaration.CusContainers.AddNew();
			cusContainer1.CO_ContainerNumber = "1";

			BaseCusContainer cusContainer2 = declaration.Declaration.CusContainers.AddNew();
			cusContainer2.CO_ContainerNumber = "2";

			BaseCusContainer cusContainer3 = declaration.Declaration.CusContainers.AddNew();
			cusContainer3.CO_ContainerNumber = "";

			string delimeter = string.Format(", {0}", System.Environment.NewLine);

			AssertTop3Containers("Should be 2 containers", declaration.Top3Containers, new string[] { "1", "2" });

			BaseCusContainer cusContainer4 = declaration.Declaration.CusContainers.AddNew();
			cusContainer4.CO_ContainerNumber = "2";

			BaseCusContainer cusContainer5 = declaration.Declaration.CusContainers.AddNew();
			cusContainer5.CO_ContainerNumber = "3";

			AssertTop3Containers("Should be 3 containers", declaration.Top3Containers, new string[] { "1", "2", "3" });

			BaseCusContainer cusContainer6 = declaration.Declaration.CusContainers.AddNew();
			cusContainer6.CO_ContainerNumber = "4";

			AssertTop3Containers("Should be 3 containers and dots", declaration.Top3Containers, new string[] { "1", "2", "3", "..." });
		}

		void AssertTop3Containers(string message, string top3Containers, string[] containerNames)
		{
			Assert(message, top3Containers.Split(new char[] { ',' }).Length == containerNames.Length);

			foreach (string containerName in containerNames)
			{
				Assert("Should contain container " + containerName, top3Containers.Contains(containerName));
			}
		}

		#endregion

		#region TestOrderReference

		public override void TestOrderReference()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.DocsAndCartage.JP_OrderItemsAsString = "test";
			AssertEquals("test", declaration.OrderReference);
		}

		#endregion

		#region TestForwarders

		public override void TestForwarders()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			declaration.Declaration.JE_OH_Forwarder = org.PK;
			declaration.Declaration.JE_MessageType = "EXP";

			AssertEquals("SendingForwarder", org.PK, declaration.SendingForwarderPK);
			AssertEquals("ReceivingForwarder", ZGuid.Empty, declaration.ReceivingForwarderPK);

			declaration.Declaration.JE_MessageType = "IMP";

			AssertEquals("SendingForwarder", ZGuid.Empty, declaration.SendingForwarderPK);
			AssertEquals("ReceivingForwarder", org.PK, declaration.ReceivingForwarderPK);
		}

		#endregion

		#region TestLoadingMeters

		public override void TestLoadingMeters()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			AssertEquals("LoadingMeters", Decimal.Zero, declaration.LoadingMeters);
		}

		#endregion

		#region TestReleaseStatusDesc

		public void TestReleaseStatusDesc()
		{
			var releaseStatus = CRLReleaseStatusList.Codes.REL;
			var declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_EntryStatus = releaseStatus;
			AssertEquals("ReleaseStatusDesc", declaration.ReleaseStatusDesc, "Released From Embargo");

			var usDeclaration = GetNewCountrySpecificTrackingDeclaration("US");
			if (usDeclaration.Declaration is JobDeclaration)
			{
				((JobDeclaration)usDeclaration.Declaration).ReleaseStatus = releaseStatus;
				AssertEquals("ReleaseStatusDesc (US Only)", usDeclaration.ReleaseStatusDesc, CRLReleaseStatusList.Descriptions.REL);
			}
		}

		#endregion

		#region TestFormattedEntryNumber

		public void TestFormattedEntryNumber()
		{
			var declarationNbr = "123456";
			var declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_MessageType = WebPartyType.Importer;

			var cusEntryHeader = declaration.Declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.EntryNumber = declarationNbr;

			AssertEquals("FormattedEntryNumber", declaration.FormattedEntryNumber, declarationNbr);

			declaration = GetNewCountrySpecificTrackingDeclaration("US");
			declaration.Declaration.JE_MessageType = WebPartyType.Importer;
			if (declaration.Declaration is JobDeclaration)
			{
				var usDeclaration = (JobDeclaration)declaration.Declaration;
				usDeclaration.US_EntryFilerCode = "XJ5";
				usDeclaration.DecEntryNumber = declarationNbr;
				AssertEquals("FormattedEntryNumber (US Only)", declaration.FormattedEntryNumber, Customs.US.Business.CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(usDeclaration.US_EntryFilerCode, usDeclaration.DecEntryNumber));
			}
		}

		#endregion

		#region TestTEUCount

		public void TestTEUCount()
		{
			var declaration = GetNewTrackingDeclaration();

			var container = declaration.Declaration.CusContainers.AddNew();
			container.CO_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			container.Container.RC_TEU = 5;

			container = declaration.Declaration.CusContainers.AddNew();
			container.CO_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			container.Container.RC_TEU = 10;

			AssertEquals(15m, declaration.TEUCount);
		}

		#endregion

		#region TestTop3JobNotes

		[HttpContextEnabledTest]
		public void TestTop3JobNotes()
		{
			var helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			var declaration = new TrackingDeclaration(Factory.New<BaseJobDeclaration>(), helper.TestSiteUser);

			declaration.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.AgentNotes.Description, "NotJobNotes");
			AssertEquals(string.Empty, declaration.Top3JobNotes);

			declaration.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, "1");
			AssertEquals("1", declaration.Top3JobNotes);

			declaration.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, $"2A{System.Environment.NewLine}2B");
			AssertEquals($"1{System.Environment.NewLine}2A 2B", declaration.Top3JobNotes);

			declaration.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, "3");
			AssertEquals($"1{System.Environment.NewLine}2A 2B{System.Environment.NewLine}3", declaration.Top3JobNotes);

			declaration.Declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description, "4");
			AssertEquals($"1{System.Environment.NewLine}2A 2B{System.Environment.NewLine}3{System.Environment.NewLine}...", declaration.Top3JobNotes);
		}

		#endregion

		[SetGlobalsIsWeb]
		public void TestFirstAndLastLegDates()
		{
			var declaration = GetNewTrackingDeclaration();

			var now = ZDateTime.Now;
			var etd = now.AddDays(1);
			var atd = now.AddDays(2);
			var eta = now.AddDays(4);
			var ata = now.AddDays(5);

			var firstLeg = declaration.TransportsIncludingRelated.AddNew();
			firstLeg.JW_TransportMode = "AIR";
			firstLeg.JW_RL_NKLoadPort = "USORD";
			firstLeg.JW_RL_NKDiscPort = "USLAX";
			firstLeg.JW_LegOrder = 1;
			firstLeg.JW_ETD = etd;
			firstLeg.JW_ATD = atd;
			var lastLeg = declaration.TransportsIncludingRelated.AddNew();
			lastLeg.JW_TransportMode = "AIR";
			lastLeg.JW_RL_NKLoadPort = "USLAX";
			lastLeg.JW_RL_NKDiscPort = "AUSYD";
			lastLeg.JW_LegOrder = 2;
			lastLeg.JW_ETA = eta;
			lastLeg.JW_ATA = ata;
			AssertEquals("Precondition", firstLeg, declaration.TransportsIncludingRelated.FirstLeg);
			AssertEquals("Precondition", lastLeg, declaration.TransportsIncludingRelated.LastLeg);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, false);

			AssertEquals(etd, declaration.FirstLegLoadETD);
			AssertEquals(atd, declaration.FirstLegLoadATD);
			AssertEquals(eta, declaration.LastLegDischargeETA);
			AssertEquals(ata, declaration.LastLegDischargeATA);

			SuppressionForTest.CacheObjectClear();
			SuppressionTest.SetSuppressingFields(WebDataRegistry.Instance.SuppressFlightDetailsForExport, true);

			AssertEquals(Suppression.SuppressedDate, declaration.FirstLegLoadETD);
			AssertEquals(Suppression.SuppressedDate, declaration.FirstLegLoadATD);
			AssertEquals(Suppression.SuppressedDate, declaration.LastLegDischargeETA);
			AssertEquals(Suppression.SuppressedDate, declaration.LastLegDischargeATA);
		}

		#region Implementation

		protected override IShipmentDeclaration GetNewBizOForChargesTest()
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_DeclarationReference = "001";
			return declaration;
		}

		protected override CodeDescriptionBoolRegistryItem RegistryItem
		{
			get { return WebDataRegistry.Instance.SuppressFlightDetailsForExport; }
		}

		protected override IShipmentDeclaration GetNewBizOForSuppressedFieldsTest(ZDateTime date, ZString vessel, ZString voyageFlight, ZString transportMode)
		{
			TrackingDeclaration declaration = GetNewTrackingDeclaration();
			declaration.Declaration.JE_DateAtFinalDestination = date;
			declaration.Declaration.JE_DateAtOrigin = date;
			declaration.Declaration.JE_TransportMode = transportMode;
			declaration.Declaration.JE_VesselName = vessel;
			declaration.Declaration.JE_VoyageFlightNo = voyageFlight;
			Factory.Save();

			return declaration;
		}

		TrackingDeclaration GetNewTrackingDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			return new TrackingDeclaration(declaration, TestSiteUser);
		}

		TrackingDeclaration GetNewCountrySpecificTrackingDeclaration(string countryCode)
		{
			BaseJobDeclaration declaration = null;
			switch (countryCode)
			{
				case "US":
					declaration = Factory.New<JobDeclaration>();
					break;
				default:
					declaration = Factory.New<BaseJobDeclaration>();
					break;
			}
			return new TrackingDeclaration(declaration, TestSiteUser);
		}

		#endregion
	}
}
