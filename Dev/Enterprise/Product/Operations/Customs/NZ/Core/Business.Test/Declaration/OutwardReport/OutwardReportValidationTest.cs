using CargoWise.Types;
using Enterprise.Customs.NZ.Business.MessageBuilders.OutwardReport;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Declaration.OutwardReport.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Common.NZ;

	public class OutwardReportValidationTest : TestCaseWithFactory
	{
		public void TestNoErrorForValidConsol()
		{
			SetUpValidAIRConsol();
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("This is a valid consol", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckClearanceNumberIfCancelationOrReplacement()
		{
			SetUpValidAIRConsol();
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("For replacement, Clearance number for Consol is necessary", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Replacement));
		}

		public void TestCancelationOrReplacementRecognisesInvalidNo()
		{
			SetUpValidAIRConsol();

			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			entryNumber.CE_EntryNum = ZString.Empty;
			entryNumber.CE_ParentID = Consol.PK;
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Rejected;

			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("For replacement, a valid Clearance number for Consol is necessary", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Replacement));

			entryNumber.CE_EntryNum = "47842342";
			entryNumber.CE_EntryStatus = OutwardReportStatusList.Codes.Cleared;
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("For replacement, Clearance number for Consol is necessary", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Replacement));
		}

		public void TestCheckValidBrokerageID()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Broker ID is necessary in Registry", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckValidShippingLine()
		{
			SetUpValidAIRConsol();
			Consol.SetDefaultShippingLineAddress(ZGuid.Empty);
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Shipping Line is empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckTransportMode()
		{
			SetUpValidAIRConsol();
			Consol.JK_TransportMode = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			Assert("Consol doesnt have transport mode entered", validator.GetErrorCount(MessageBuilder.MessageTypes.Original) > 0);
		}

		public void TestCheckFlightNumberForAir()
		{
			SetUpValidAIRConsol();
			Transport.JW_VoyageFlight = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			Assert("Consol doesnt have flight no entered", validator.GetErrorCount(MessageBuilder.MessageTypes.Original) > 0);
		}

		public void TestCheckVoyageNumerForSea()
		{
			SetUpValidSEAConsol();
			Transport.JW_VoyageFlight = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			Assert("Consol doesnt have voyage no entered", validator.GetErrorCount(MessageBuilder.MessageTypes.Original) > 0);
		}

		public void TestCheckMasterBill()
		{
			SetUpValidAIRConsol();
			Consol.JK_MasterBillNum = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Master bill number may not be empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));

			Consol.JK_MasterBillNum = "123456";
			Factory.Save();
			AssertEquals("Master bill number is now there", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckVesselForAir()
		{
			SetUpValidAIRConsol();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;

			Transport.Sailing.Voyage.JV_OH_Line = carrier.PK;
			Transport.JW_Vessel = ZString.Empty;

			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Consol is air and empty vessel is not invalid", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckLoadPortOfConsol()
		{
			SetUpValidAIRConsol();

			Consol.JK_RL_NKLoadPort = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Port of loading may not be empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));

			Consol.JK_RL_NKLoadPort = "AUSYD";
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Port of loading may not be an overseas port", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckDischargePortOfConsol()
		{
			SetUpValidAIRConsol();

			Consol.JK_RL_NKDischargePort = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Port of discharge may not be empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));

			Consol.JK_RL_NKDischargePort = "NZAKL";
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Port of loading may not be a NZ port", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckShipmentsCount()
		{
			SetUpValidAIRConsol();
			Consol.Shipments.Remove(shipment);
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("There should be at least one shipment", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckShipmentsHouseBill()
		{
			SetUpValidAIRConsol();
			shipment.JS_HouseBill = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("House Bill may not be empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckShipmentsClearanceNumber()
		{
			SetUpValidAIRConsol();
			shipment.CustomsEntryNumber = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Shipment's clearance no may not be empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckSubShipmentsClearanceNumber()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = ZString.Empty;
			CommonShipment subshipment = shipment.CoLoadShipments.AddNew();
			subshipment.CustomsEntryNumberType = "CUS";
			subshipment.CustomsEntryNumber = "C123";
			subshipment.JS_HouseBill = "S121212";
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Shipment's clearance empty, but its subshipment has one", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckCustomsEntryNumberWhenMasterHasOneWhileTwoSubShipmentsDontHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "123456";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "";

			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Shipment's clearance not empty, but its subshipment has none", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckCustomsEntryNumberWhenMasterHasOneWhileOneOfSubShipmentsDoesntHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "123456";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "1234";

			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Shipment's clearance not empty, but one subshipment has none", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void CheckCustomsEntryNumberWhenMasterDoesntHaveAndOneOfSubShipmentDoesHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "1234";

			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Shipment's clearance not empty, but one subshipment has none", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestCheckSubShipmentsClearanceNumber2()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = ZString.Empty;
			CommonShipment subshipment = shipment.CoLoadShipments.AddNew();
			subshipment.CustomsEntryNumber = ZString.Empty;
			subshipment.JS_HouseBill = "S121212";
			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("Shipment's clearance may not be empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestValidETD()
		{
			SetUpValidAIRConsol();

			Transport.JW_ETD = ZDateTime.Empty;

			validator = new OutwardReportValidation(ManifestStatus);
			AssertEquals("ETD may not be empty", 1, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		public void TestContainerNumber()
		{
			SetUpValidSEAConsol();
			container.JC_ContainerNum = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			Assert("Container number may not be empty", validator.GetErrorCount(MessageBuilder.MessageTypes.Original) > 0);
		}

		public void TestContainerMode()
		{
			SetUpValidSEAConsol();
			container.JC_ContainerMode = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			Assert("Container mode may not be empty", validator.GetErrorCount(MessageBuilder.MessageTypes.Original) > 0);
		}

		public void TestCheckErrorsAfterSaving()
		{
			SetUpValidAIRConsol();
			shipment.JS_HouseBill = ZString.Empty;
			validator = new OutwardReportValidation(ManifestStatus);
			Assert("House bill number may not be empty", validator.GetErrorCount(MessageBuilder.MessageTypes.Original) == 1);

			shipment.JS_HouseBill = "123456";
			Factory.Save();
			AssertEquals("House bill number is now there", 0, validator.GetErrorCount(MessageBuilder.MessageTypes.Original));
		}

		#region Implementation

		internal ForwardingConsol Consol;
		internal Transport Transport;
		internal OutwardReportManifestStatus ManifestStatus;
		OutwardReportValidation validator;
		CommonShipment shipment;
		CommonContainer container;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345678A");
			Consol = Factory.New<ForwardingConsol>();
			Transport = Consol.Transports[0];

			ManifestStatus = new OutwardReportManifestStatus(Consol);
		}

		protected void SetUpShipmentsAndCommonData()
		{
			shipment = Consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1212";
			shipment.CustomsEntryNumberType = "ENT";
			shipment.CustomsEntryNumber = "EntryNumber";
			shipment.JS_UniqueConsignRef = "SA0001000";

			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(shippingLine);
			Consol.JK_RL_NKLoadPort = "NZAKL";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			Transport.JW_ETD = new ZDateTime(2005, 1, 1);
		}

		protected void SetUpValidAIRConsol()
		{
			SetUpShipmentsAndCommonData();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			Transport.JW_VoyageFlight = "QF115";
		}

		protected void SetUpValidSEAConsol()
		{
			SetUpShipmentsAndCommonData();
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport.JW_VoyageFlight = "12345678";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_Code = "TESTVESL";
			Transport.JW_Vessel = vessel.RV_Code;

			container = Consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_ContainerNum = "CRUX12345";
		}

		#endregion
	}
}
