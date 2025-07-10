using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;

	public class ICRValidationTest : TestCaseWithFactory
	{
		public void TestNoErrorForValidConsol()
		{
			SetUpValidAIRConsol();
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("This is a valid consol", 0, validator.ErrorCount);
		}

		public void TestConsolBillValidation()
		{
			SetUpValidSEAConsol();
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("This is a valid consol", 0, validator.ErrorCount);

			consol.JK_MasterBillNum = "OceanBillThatIsTooLongForMPI";
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertNoErrors("Ocean Bill Number can now be full 35 characters for NZ MPI", consol.JK_MasterBillNumInfo);
		}

		public void TestCheckClearanceNumberIfCancelationOrReplacement()
		{
			SetUpValidAIRConsol();
			validator = new ICRValidation(consol, TSWTransactionTypes.Replace, manifestStatus);
			AssertEquals("For replacement, Clearance number for Consol is necessary", 1, validator.ErrorCount);
		}

		public void TestCheckValidBrokerageID()
		{
			SetUpValidAIRConsol();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("Broker ID is necessary in Registry", 1, validator.ErrorCount);
		}

		public void TestCheckValidShippingLine()
		{
			SetUpValidAIRConsol();
			consol.SetDefaultShippingLineAddress(ZGuid.Empty);
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("Shipping Line is empty", 1, validator.ErrorCount);
		}

		public void TestCheckConsolTransportMode()
		{
			SetUpValidAIRConsol();
			consol.JK_TransportMode = ZString.Empty;
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			Assert("Consol doesn't have transport mode entered", validator.ErrorCount > 0);

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
		}

		public void TestCheckDeclarationTransportMode()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = ZString.Empty;
			validator = new ICRValidation(declaration, TSWTransactionTypes.Original, manifestStatus);
			Assert("Declaration doesn't have transport mode entered", validator.ErrorCount > 0);
		}

		public void TestCheckFlightNumberForAir()
		{
			SetUpValidAIRConsol();
			transport.JW_VoyageFlight = ZString.Empty;
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			Assert("Consol doesn't have flight no entered", validator.ErrorCount > 0);
		}

		public void TestCheckVoyageNumerForSea()
		{
			SetUpValidSEAConsol();
			transport.JW_VoyageFlight = ZString.Empty;
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			Assert("Consol doesn't have voyage no entered", validator.ErrorCount > 0);
		}

		public void TestCheckVesselForSea()
		{
			SetUpValidSEAConsol();

			transport.JW_Vessel = ZString.Empty;
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			Assert("Consol doesn't have vessel no entered", validator.ErrorCount > 0);

			transport.JW_Vessel = "Rubbish";
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			Assert("Consol has vessel no entered", validator.ErrorCount == 0);
		}

		public void TestCheckSubShipmentsClearanceNumber()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = ZString.Empty;
			CommonShipment subshipment = shipment.CoLoadShipments.AddNew();
			subshipment.CustomsEntryNumber = "C123";
			subshipment.JS_HouseBill = "S121212";
			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance empty, but its subshipment has one", 0, validator.ErrorCount);
		}

		public void TestCheckCustomsEntryNumberWhenMasterHasOneWhileTwoSubShipmentsDontHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "123456";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "";

			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance not empty, but its subshipment has none", 0, validator.ErrorCount);
		}

		public void TestCheckCustomsEntryNumberWhenMasterHasOneWhileOneOfSubShipmentsDoesntHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "123456";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "1234";

			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance not empty, but one subshipment has none", 0, validator.ErrorCount);
		}

		public void CheckCustomsEntryNumberWhenMasterDoesntHaveAndOneOfSubShipmentDoesHave()
		{
			SetUpValidAIRConsol();

			shipment.CustomsEntryNumber = "";
			CommonShipment subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.CustomsEntryNumber = "";
			CommonShipment subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.CustomsEntryNumber = "1234";

			validator = new ICRValidation(consol, TSWTransactionTypes.Original, manifestStatus);
			AssertEquals("Shipment's clearance not empty, but one subshipment has none", 1, validator.ErrorCount);
		}

		#region Implementation

		ForwardingConsol consol;
		Transport transport;
		ICRManifestStatus manifestStatus;
		ICRValidation validator;
		CommonShipment shipment;
		CommonContainer container;
		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "12345678A");
			consol = Factory.New<ForwardingConsol>();
			transport = consol.Transports[0];
			manifestStatus = new ICRManifestStatus(consol);
		}

		protected void SetUpShipmentsAndCommonData()
		{
			shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "Shipment1212";
			shipment.CustomsEntryNumberType = "ENT";
			shipment.CustomsEntryNumber = "EntryNumber";
			shipment.JS_UniqueConsignRef = "SA0001000";

			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			consol.SetDefaultShippingLineAddress(shippingLine);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			transport.JW_ETD = new ZDateTime(2013, 03, 25);
		}

		protected void SetUpValidAIRConsol()
		{
			SetUpShipmentsAndCommonData();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "QF23";
		}

		protected void SetUpValidSEAConsol()
		{
			SetUpShipmentsAndCommonData();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			transport.JW_VoyageFlight = "12345678";
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			vessel.RV_Code = "TESTVESL";
			transport.JW_Vessel = vessel.RV_Code;

			container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container.JC_ContainerNum = "CRUX12345";
		}

		#endregion
	}
}
