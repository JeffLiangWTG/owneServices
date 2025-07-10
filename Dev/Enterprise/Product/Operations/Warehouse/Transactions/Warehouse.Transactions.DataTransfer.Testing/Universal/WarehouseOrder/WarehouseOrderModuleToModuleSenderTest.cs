using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class WarehouseOrderModuleToModuleSenderTest : WhsTestCaseWithFactory
	{
		#region TestCreateForwardingShipmentFromOrder

		public void TestCreateForwardingShipmentFromOrder()
		{
			var sender = new WarehouseOrderModuleToModuleSender();

			var publishResult1 = sender.CreateForwardingShipmentFromOrder(null);
			AssertNull(publishResult1.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult1.ResultType);
			AssertEquals(@"Failed to create Shipment:
Null Entity", publishResult1.ErrorMessage);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			var publishResult2 = sender.CreateForwardingShipmentFromOrder(order);
			AssertNull(publishResult2.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult2.ResultType);
			AssertEquals(@"Failed to create Shipment:
Cannot create a Shipment as no Freight Forwarder is specified.", publishResult2.ErrorMessage);

			order.ConsigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			order.WD_OH_Forwarder = Factory.NewWithValidTestData<OrgHeader>().PK;
			order.Forwarder.OH_Code = "SAM";

			var publishResult3 = sender.CreateForwardingShipmentFromOrder(order);
			AssertNull(publishResult3.FindJobIfExists());
			AssertEquals(UniversalResult.HadErrors, publishResult3.ResultType);
			AssertMultilineASCIIEquals("errorMessage", @"
Failed to create Shipment:
No EDI Communications settings were found on the Recipient Organization [SAM]. Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.
			".Trim(), publishResult3.ErrorMessage);

			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				var communicationMode = order.Forwarder.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "Universal";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "WOU"; //Warehouse Outwards

				var publishResult4 = sender.CreateForwardingShipmentFromOrder(order);
				AssertNull(publishResult4.FindJobIfExists());
				AssertEquals(UniversalResult.External, publishResult4.ResultType);
				AssertMultilineASCIIEquals("errorMessage", @"".Trim(), publishResult4.ErrorMessage);

				order.WD_OH_Forwarder = GlbCompany.CurrentCompany.OrgProxy.PK;

				order.Factory.Save();

				var publishResult5 = sender.CreateForwardingShipmentFromOrder(order);
				var shipment = publishResult5.FindJobIfExists();
				AssertNotNull(shipment);
				AssertEquals(UniversalResult.Internal, publishResult5.ResultType);
				AssertEquals("", publishResult5.ErrorMessage);
				AssertEquals("S00001000", shipment[JobShipmentSchema.JS_UniqueConsignRef]);
				AssertEquals(1, order.RelatedJobs.Count);
			}
		}

		public void TestCreateForwardingShipmentFromOrderCopyAddressValidationDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "Test Company";
			order.ConsigneeDocAddress.E2_Address1 = "Test Address";
			order.ConsigneeDocAddress.E2_City = "Sydney";
			order.ConsigneeDocAddress.E2_State = "NSW";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";
			order.ConsigneeDocAddress.ValidationStatus = "VAD";
			order.ConsigneeDocAddress.AddressMap = "TestAddressMap";
			order.ConsigneeDocAddress.GeoLocation = ZGeography.CreatePoint(1.0, 1.0);

			order.WD_OH_Forwarder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var publishResult = new WarehouseOrderModuleToModuleSender().CreateForwardingShipmentFromOrder(order);
			var shipment = (IForwardingShipment)publishResult.FindJobIfExists();
			var jobDocAddress = Factory.Load<JobDocAddress>(shipment.ConsigneeDocumentaryAddress.PK);
			AssertEquals("VAD", jobDocAddress.E2_ValidationStatus);
			AssertEquals("TestAddressMap", jobDocAddress.E2_AddressMap);
			AssertEquals(ZGeography.CreatePoint(1.0, 1.0), jobDocAddress.E2_GeoLocation);
		}

		[ExpectNoExceptions]
		public void TestCreateForwardingShipmentFromOrderHandlesZSaveException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "Test Company";
			order.ConsigneeDocAddress.E2_Address1 = "Test Address";
			order.ConsigneeDocAddress.E2_City = "Sydney";
			order.ConsigneeDocAddress.E2_State = "NSW";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";
			order.ConsigneeDocAddress.ValidationStatus = "VAD";
			order.ConsigneeDocAddress.AddressMap = "TestAddressMap";
			order.ConsigneeDocAddress.GeoLocation = ZGeography.CreatePoint(1.0, 1.0);

			order.WD_OH_Forwarder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var sender = new WarehouseOrderModuleToModuleSender
			{
				OnSaveActionForTest = bizo => new SaveInTransactionActionForThrowingException(bizo)
			};
			var publishResult = sender.CreateForwardingShipmentFromOrder(order.RelatedBackOrder);
			var shipment = (IForwardingShipment)publishResult.FindJobIfExists();
		}

		public void TestCreateForwardingShipmentFromOrderWhenAddressOverrideIsTrue()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";

			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "Test Company";
			order.ConsigneeDocAddress.E2_Address1 = "Test Address";
			order.ConsigneeDocAddress.E2_City = "Sydney";
			order.ConsigneeDocAddress.E2_State = "NSW";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";
			order.ConsigneeDocAddress.ValidationStatus = AddressValidationStatus.Verified;
			order.ConsigneeDocAddress.AddressMap = "TestAddressMap";
			order.ConsigneeDocAddress.GeoLocation = ZGeography.CreatePoint(1.0, 1.0);

			order.WD_OH_Forwarder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var publishResult = new WarehouseOrderModuleToModuleSender().CreateForwardingShipmentFromOrder(order);
			var shipment = (IForwardingShipment)publishResult.FindJobIfExists();

			AssertEquals(true, shipment.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals(AddressValidationStatus.Verified, shipment.ConsigneeDocumentaryAddress.E2_ValidationStatus);
		}

		public void TestCreateForwardingShipmentFromOrderWhenAddressOverrideIsFalse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUMEL";
			var address = org.Addresses[0];
			address.OA_ValidationStatus = AddressValidationStatus.Verified;
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			order.ConsigneeDocAddress.E2_AddressOverride = false;
			order.ConsigneeDocAddress.E2_OA_Address = org.Addresses[0].PK;
			order.ConsigneeDocAddress.AddressMap = "TestAddressMap";
			order.ConsigneeDocAddress.GeoLocation = ZGeography.CreatePoint(1.0, 1.0);

			order.WD_OH_Forwarder = GlbCompany.CurrentCompany.OrgProxy.PK;

			var publishResult = new WarehouseOrderModuleToModuleSender().CreateForwardingShipmentFromOrder(order);
			var shipment = (IForwardingShipment)publishResult.FindJobIfExists();

			AssertEquals(false, shipment.ConsigneeDocumentaryAddress.E2_AddressOverride);
			AssertEquals(AddressValidationStatus.Verified, shipment.ConsigneeDocumentaryAddress.E2_ValidationStatus);
		}

		#endregion
	}

	sealed class SaveInTransactionActionForThrowingException : SaveInTransactionActionWithFactory
	{
		public SaveInTransactionActionForThrowingException(BusinessObject bizObj)
			: base(bizObj.Factory)
		{
			this.bizObj = bizObj;
		}

		readonly BusinessObject bizObj;

		protected override IChangedTableNames SaveInTransaction()
		{
			var row = ((INeedRow)bizObj).Row;
			var dataConcurrencyException = new ZDataConcurrencyException(new InvalidOperationException(), row, Db.Connection);
			var saveConcurrencyException = new ZSaveConcurrencyException(dataConcurrencyException, bizObj.Factory);
			var dataException = new ZDataException(saveConcurrencyException, row, Db.Connection);

			throw new ZSaveException(dataException, bizObj.Factory);
		}
	}
}
