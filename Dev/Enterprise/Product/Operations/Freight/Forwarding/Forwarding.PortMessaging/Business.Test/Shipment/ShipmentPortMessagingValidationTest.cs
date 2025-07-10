using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class ShipmentPortMessagingValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2023, 3, 1)]
		public void TestEntryType()
		{
			AssertEquals("Pre-condition: should default to empty", ZString.Empty, PortMessaging.JSM_EntryType);
			AssertNoErrors("Pre-condition: should have no errors when empty", PortMessaging.JSM_EntryTypeInfo);

			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessagingValidationHelperTest.AssertEntryTypeValidation(PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString);
		}

		public void TestEntryType_Ports()
		{
			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_GoodsDescription = "shiba inu";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			PortMessagingValidationHelperTest.AssertEntryTypeForPortsValidation(PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString, shipment, () => PortMessaging.Validation.ValidateJSM_EntryType());
		}

		public void TestEntryType_Consignor()
		{
			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_GoodsDescription = "shiba inu";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			PortMessagingValidationHelperTest.AssertEntryTypeForConsignorValidation(PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString, shipment, () => PortMessaging.Validation.ValidateJSM_EntryType());
		}

		public void TestEntryType_MarksAndNumbers()
		{
			var expectedMessage = "Marks & Numbers are required for Entry Type DOX - DUX Without MRN (Exit Summary Declaration).";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_MarksAndNumbers = "SHP0123";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var message = ShipmentPortMessaging.LoadOrCreate(shipment);
			message.JSM_EntryType = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;

			AssertNoErrors("Should not have any errors as the shipment Marks is not empty.", message.JSM_EntryTypeInfo);

			shipment.JS_MarksAndNumbers = string.Empty;
			message.Validation.ValidateJSM_EntryType();

			AssertHasError("Should has error as the shipment Marks is empty for DOX.", message.JSM_EntryTypeInfo, expectedMessage);

			message.JSM_EntryType = EntryTypeList.Codes.Message;
			AssertNoErrors("Should not have any errors as the message's entry type is not DOX.", message.JSM_EntryTypeInfo);
		}

		public void TestMovementReferenceNumber()
		{
			AssertNoErrors("Pre-condition: expected mrn to have no errors", PortMessaging.JSM_MovementReferenceNumberInfo);
			AssertEquals("Pre-condition: expected mrn to default to blank", ZString.Empty, PortMessaging.JSM_MovementReferenceNumber);

			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessagingValidationHelperTest.AssertMovementReferenceNumberValidation(
				PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JSM_MovementReferenceNumberInfo as ZPropertyInfoString,
				PortMessaging.JSM_Annex30ATypeInfo as ZPropertyInfoString);
		}

		public void TestMovementReferenceNumberFormat()
		{
			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessagingValidationHelperTest.AssertMovementReferenceNumberFormatValidation(
				PortMessaging.JSM_MovementReferenceNumberInfo as ZPropertyInfoString);
		}

		public void TestExemptionReason()
		{
			AssertEquals("Pre-condition: should default to empty", ZString.Empty, PortMessaging.JSM_ExemptionReason);
			AssertNoErrors("Pre-condition: should have no errors when empty", PortMessaging.JSM_ExemptionReasonInfo);

			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessagingValidationHelperTest.AssertExemptionReasonValidation(
				PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JSM_ExemptionReasonInfo as ZPropertyInfoString,
				PortMessaging.Shipment);
		}

		public void TestExemptionReasonObsoleteCodes()
		{
			PortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;

			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			PortMessagingValidationHelperTest.AssertExemptionReasonObsoleteCodesValidation(
				PortMessaging.JSM_ExemptionReasonInfo as ZPropertyInfoString,
				() => PortMessaging.Validation.ValidateJSM_ExemptionReason(),
				PortMessaging.Shipment);
		}

		public void TestATBNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JSM_JS_Shipment = shipment.PK;

			PortMessagingValidationHelperTest.AssertATBNumberValidation(
				PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JSM_ATBNumberInfo as ZPropertyInfoString,
				shipment);
		}

		public void TestAnnex30AType()
		{
			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessagingValidationHelperTest.AssertAnnex30ATypeValidation(
				PortMessaging.JSM_Annex30ATypeInfo as ZPropertyInfoString);
		}

		public void TestExportDeclarationReference()
		{
			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessagingValidationHelperTest.AssertExportDeclarationReferenceValidation(
				PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JSM_ExportDeclarationReferenceInfo as ZPropertyInfoString);
		}

		public void TestCustomsReleaseDate()
		{
			var shipment = PortMessaging.Shipment;
			AssertNotNull(shipment);
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessagingValidationHelperTest.AssertCustomsReleaseDateValidation(
				PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JSM_CustomsReleaseDateInfo as ZPropertyInfoDateTime,
				() => PortMessaging.Validation.ValidateJSM_CustomsReleaseDate(),
				PortMessaging.Shipment);
		}

		[TestDate(2023, 3, 1)]
		public void TestLocalReferenceNumber()
		{
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				AssertNoErrors("Pre-condition: expected lrn to have no errors", PortMessaging.JSM_LocalReferenceNumberInfo);
				AssertEquals("Pre-condition: expected lrn to default to blank", ZString.Empty, PortMessaging.JSM_LocalReferenceNumber);

				var shipment = PortMessaging.Shipment;
				AssertNotNull(shipment);
				shipment.JS_TransportMode = Constants.TransportModes.Sea;

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "DEHAM";

				PortMessagingValidationHelperTest.AssertLocalReferenceNumberValidation(
					PortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString,
					PortMessaging.JSM_LocalReferenceNumberInfo as ZPropertyInfoString);
			}
		}

		#region Implementation

		ShipmentPortMessaging PortMessaging
		{
			get
			{
				if (portMessaging == null)
				{
					portMessaging = Factory.New<ShipmentPortMessaging>();
					portMessaging.JSM_JS_Shipment = Factory.New<ForwardingShipment>().PK;
				}

				return portMessaging;
			}
		}
		ShipmentPortMessaging portMessaging;

		#endregion
	}
}
