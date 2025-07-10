using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	[TestedType(typeof(ShipmentPortMessaging))]
	sealed class ShipmentPortMessagingTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoad()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals(null, ShipmentPortMessaging.Load(shipment));

			var portMessaging = Factory.New<ShipmentPortMessaging>();
			portMessaging.JSM_JS_Shipment = shipment.PK;

			AssertEquals(portMessaging.PK, ShipmentPortMessaging.Load(shipment).PK);
		}

		public void TestLoadOrCreate()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			var portMessaging1 = ShipmentPortMessaging.LoadOrCreate(shipment1);
			AssertEquals(shipment1.PK, portMessaging1.JSM_JS_Shipment);

			var portMessaging11 = ShipmentPortMessaging.LoadOrCreate(shipment1);
			AssertEquals(portMessaging1, portMessaging11);

			var portMessaging2 = ShipmentPortMessaging.LoadOrCreate(shipment2);
			AssertEquals(shipment2.PK, portMessaging2.JSM_JS_Shipment);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			shipment1 = anotherFactory.Load<ForwardingShipment>(shipment1.PK);
			shipment2 = anotherFactory.Load<ForwardingShipment>(shipment2.PK);

			var portMessagingReloaded1 = ShipmentPortMessaging.LoadOrCreate(shipment1);
			AssertEquals(portMessaging1.PK, portMessagingReloaded1.PK);

			var portMessagingReloaded2 = ShipmentPortMessaging.LoadOrCreate(shipment2);
			AssertEquals(portMessaging2.PK, portMessagingReloaded2.PK);
		}

		public void TestAdditionalDakosyValidationIsIncluded()
		{
			var portMessaging = Factory.New<ShipmentPortMessaging>();
			AssertEquals(false, portMessaging.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			var portMessaging1 = ShipmentPortMessaging.LoadOrCreate(shipment1);
			AssertEquals(false, portMessaging1.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Sea;
			var portMessaging2 = ShipmentPortMessaging.LoadOrCreate(shipment2);
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			var consol = shipment2.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			consol.JK_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			consol.JK_RL_NKLoadPort = "DEFRA";
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			consol.JK_RL_NKDischargePort = "DEFRA";
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			var transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			transport.JW_RL_NKLoadPort = "AUMEL";
			AssertEquals(false, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));

			transport.JW_RL_NKDiscPort = "DEHAM";
			AssertEquals(true, portMessaging2.Validation.ContainsPiggybackedValidation(typeof(ShipmentPortMessagingForDakosyValidation)));
		}

		[TestDate(2023, 3, 1)]
		public void TestPropertiesReadOnlyState()
		{
			var portMessaging = Factory.New<ShipmentPortMessaging>();

			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				portMessaging.JSM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
				Assert(!portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(!portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(!portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_Annex30AType = Annex30ATypeList.Codes.AlreadyCompleted;
				Assert(!portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.ExitSummaryDeclaration;
				Assert(!portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.OtherExemptions;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(!portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.EmergencyConcept;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(!portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
				Assert(!portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_MovementReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_MovementReferenceNumberCompleteInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.AE1ExportDeclaration;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_MovementReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_MovementReferenceNumberCompleteInfo.ReadOnly);
			}

			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 6, 6)))
			{
				portMessaging.JSM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(!portMessaging.JSM_MovementReferenceNumberInfo.ReadOnly);
				Assert(!portMessaging.JSM_MovementReferenceNumberCompleteInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
				Assert(!portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(!portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(!portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_Annex30AType = Annex30ATypeList.Codes.AlreadyCompleted;
				Assert(!portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.ExitSummaryDeclaration;
				Assert(!portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.OtherExemptions;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(!portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.EmergencyConcept;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(!portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
				Assert(!portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_MovementReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_MovementReferenceNumberCompleteInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);

				portMessaging.JSM_EntryType = EntryTypeList.Codes.AE1ExportDeclaration;
				Assert(portMessaging.JSM_ATBNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_ExemptionReasonInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30ATypeInfo.ReadOnly);
				Assert(portMessaging.JSM_Annex30AFailureProcessInfo.ReadOnly);
				Assert(portMessaging.JSM_ExportDeclarationReferenceInfo.ReadOnly);
				Assert(!portMessaging.JSM_MovementReferenceNumberInfo.ReadOnly);
				Assert(!portMessaging.JSM_MovementReferenceNumberCompleteInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberInfo.ReadOnly);
				Assert(portMessaging.JSM_LocalReferenceNumberCompleteInfo.ReadOnly);
			}
		}

		[TestDate(2023, 3, 1)]
		public void TestResetReadOnlyPropertiesDependingOnEntryType()
		{
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				var portMessaging = Factory.New<ShipmentPortMessaging>();
				portMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
				portMessaging.JSM_ATBNumber = "123";
				portMessaging.JSM_ExemptionReason = "1";
				portMessaging.JSM_Annex30AType = "A";
				portMessaging.JSM_Annex30AFailureProcess = true;
				portMessaging.JSM_MovementReferenceNumber = "TEST1";
				portMessaging.JSM_MovementReferenceNumberComplete = true;
				portMessaging.JSM_LocalReferenceNumber = "TEST2";
				portMessaging.JSM_LocalReferenceNumberComplete = true;

				portMessaging.JSM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
				CombineAssertions("Property values have been cleared", () =>
				{
					AssertEquals("", portMessaging.JSM_ATBNumber);
					AssertEquals("", portMessaging.JSM_ExemptionReason);
					AssertEquals("", portMessaging.JSM_Annex30AType);
					AssertEquals(false, portMessaging.JSM_Annex30AFailureProcess);
					AssertEquals(string.Empty, portMessaging.JSM_LocalReferenceNumber);
					AssertEquals(false, portMessaging.JSM_LocalReferenceNumberComplete);
				});

				portMessaging.JSM_EntryType = EntryTypeList.Codes.AE1ExportDeclaration;
				CombineAssertions("Property values have been cleared", () =>
				{
					AssertEquals("", portMessaging.JSM_ATBNumber);
					AssertEquals("", portMessaging.JSM_ExemptionReason);
					AssertEquals("", portMessaging.JSM_Annex30AType);
					AssertEquals(false, portMessaging.JSM_Annex30AFailureProcess);
					AssertEquals(string.Empty, portMessaging.JSM_MovementReferenceNumber);
					AssertEquals(false, portMessaging.JSM_MovementReferenceNumberComplete);
				});

				portMessaging.JSM_MovementReferenceNumber = "TEST2";
				portMessaging.JSM_MovementReferenceNumberComplete = true;
				portMessaging.JSM_LocalReferenceNumber = "TEST2";
				portMessaging.JSM_LocalReferenceNumberComplete = true;

				portMessaging.JSM_EntryType = EntryTypeList.Codes.ExitSummaryDeclaration;
				CombineAssertions("Property values have been cleared", () =>
				{
					AssertEquals(string.Empty, portMessaging.JSM_LocalReferenceNumber);
					AssertEquals(false, portMessaging.JSM_LocalReferenceNumberComplete);
				});

				portMessaging.JSM_EntryType = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;
				CombineAssertions("Property values have been cleared", () =>
				{
					AssertEquals(string.Empty, portMessaging.JSM_MovementReferenceNumber);
					AssertEquals(false, portMessaging.JSM_MovementReferenceNumberComplete);
					AssertEquals(string.Empty, portMessaging.JSM_LocalReferenceNumber);
					AssertEquals(false, portMessaging.JSM_LocalReferenceNumberComplete);
				});

				portMessaging.JSM_LocalReferenceNumber = "TEST3";
				portMessaging.JSM_LocalReferenceNumberComplete = true;

				portMessaging.JSM_EntryType = EntryTypeList.Codes.OtherExemptions;
				CombineAssertions("Property values have been cleared", () =>
				{
					AssertEquals(string.Empty, portMessaging.JSM_LocalReferenceNumber);
					AssertEquals(false, portMessaging.JSM_LocalReferenceNumberComplete);
				});

				portMessaging.JSM_LocalReferenceNumber = "TEST3";
				portMessaging.JSM_LocalReferenceNumberComplete = true;

				portMessaging.JSM_EntryType = EntryTypeList.Codes.EmergencyConcept;
				CombineAssertions("Property values have been cleared", () =>
				{
					AssertEquals(string.Empty, portMessaging.JSM_LocalReferenceNumber);
					AssertEquals(false, portMessaging.JSM_LocalReferenceNumberComplete);
				});
			}
		}

		public void TestExemptionReasonResetOnEntryTypeSet()
		{
			var portMessaging = Factory.New<ShipmentPortMessaging>();
			portMessaging.JSM_ExemptionReason = "X";
			portMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			AssertEquals(ZString.Empty, portMessaging.JSM_ExemptionReason);

			portMessaging.JSM_ExemptionReason = "X";
			portMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			AssertEquals("X", portMessaging.JSM_ExemptionReason);
		}

		[TestDate(2023, 3, 1)]
		public void TestDefaultLocalReferenceNumberDependingOnEntryType()
		{
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				string sqlText = @"
DECLARE @JsPk UNIQUEIDENTIFIER = NEWID();

INSERT dbo.JobShipment (JS_PK, JS_ShipmentType, JS_RL_NKOrigin, JS_IsForwardRegistered, JS_UniqueConsignRef, JS_IsBooking, JS_IsCancelled, JS_SystemCreateTimeUtc) VALUES
	(@JsPk, 'STD', 'DEHAM', 1, 'S00001001', 1, 0, '2023-01-01');

INSERT dbo.CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_EntryType, CE_EntryNum, CE_RN_NKCountryCode, CE_IsValid, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
	(NEWID(), 'JobShipment', @JsPk, 'MRN', 'LRN1', 'DE', 1, '2023-01-01', 'E', '2023-01-01', 'E')";

				TestConnection.ExecuteNonQuery(sqlText);

				var shipment = Factory.Load<ForwardingShipment>(new ZQuery(JobShipmentSchema.JS_UniqueConsignRef, "S00001001")).First();
				var num = Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, shipment.PK)).First();
				shipment.CusEntryNumbers.Add(num);

				var portMessaging = Factory.New<ShipmentPortMessaging>();
				portMessaging.JSM_JS_Shipment = shipment.PK;
				portMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
				AssertEquals(string.Empty, portMessaging.JSM_LocalReferenceNumber);

				num.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
				portMessaging.JSM_EntryType = EntryTypeList.Codes.AE1ExportDeclaration;
				AssertEquals("LRN1", portMessaging.JSM_LocalReferenceNumber);
			}
		}
	}
}
