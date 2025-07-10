using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business.Testing
{
	sealed class PackLinePortMessagingValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2023, 3, 1)]
		public void TestEntryType()
		{
			AssertEquals("Pre-condition: should default to empty", ZString.Empty, PortMessaging.JLM_EntryType);
			AssertNoErrors("Pre-condition: should have no errors when empty", PortMessaging.JLM_EntryTypeInfo);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertEntryTypeValidation(PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString);
		}

		public void TestEntryType_Ports()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_GoodsDescription = "shiba inu";
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertEntryTypeForPortsValidation(PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString, shipment, () => PortMessaging.Validation.ValidateJLM_EntryType());
		}

		public void TestEntryType_Consignor()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_GoodsDescription = "shiba inu";
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertEntryTypeForConsignorValidation(PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString, shipment, () => PortMessaging.Validation.ValidateJLM_EntryType());
		}

		public void TestEntryType_MarksAndNumbers()
		{
			var expectedMessage = "Marks & Numbers are required for Entry Type DOX - DUX Without MRN (Exit Summary Declaration).";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_MarksAndNumbers = "SHP0123";
			shipment.JS_GoodsDescription = "Goods";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_MarksAndNumbers = "PKG001";
			packline1.JL_HarmonisedCode = "AAA";

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_MarksAndNumbers = "PKG002";
			packline2.JL_HarmonisedCode = "BBB";

			var message = PackLinePortMessaging.LoadOrCreate(packline1);
			message.JLM_EntryType = EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN;

			AssertNoErrors("Should not have any errors as the shipment and all pack lines Marks are all not empty.", message.JLM_EntryTypeInfo);

			shipment.JS_MarksAndNumbers = string.Empty;
			message.Validation.ValidateJLM_EntryType();

			AssertNoErrors("Should not have any errors as all pack lines Marks are all not empty.", message.JLM_EntryTypeInfo);

			packline1.JL_MarksAndNumbers = string.Empty;
			message.Validation.ValidateJLM_EntryType();

			AssertHasError("Should has error as the shipment Marks are blank for DOX and at least one pack line has no Marks.",
				message.JLM_EntryTypeInfo,
				expectedMessage);

			message.JLM_EntryType = EntryTypeList.Codes.Message;
			AssertNoErrors("Should not have any errors as the message's entry type is not DOX.", message.JLM_EntryTypeInfo);
		}

		public void TestUNDGTechnicalName()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;
			var message = PackLinePortMessaging.LoadOrCreate(packline);
			var expectedError = "Dangerous Goods with special provision 274 requires a Technical Name";
			message.Validation.ValidateDGTechnicalName();
			AssertNoMessageError("Should not have any errors as there is no dangerous good on the shipment",
				message.DGTechnicalNameInfo,
				expectedError);

			var undg = packline.UNDGs.AddNew();
			var provision = Factory.New<UNDGCommonData>();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1001";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.LinkDefault(subs);
			undg.Substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			provision.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			provision.DC_Index = "274";
			undg.Substance.SpecialProvisions.Add(provision);
			message.Validation.ValidateDGTechnicalName();
			AssertHasMessageError("Should have error as the dangerous good doesn't have a technical name assigned to it.",
				message.DGTechnicalNameInfo,
				expectedError);

			undg.DI_TechnicalName = "Oat";
			message.Validation.ValidateDGTechnicalName();
			AssertNoMessageError("Should not have any errors as there is a technical name on the dangerous good",
				message.DGTechnicalNameInfo,
				expectedError);
		}

		public void TestEntryType_HarmonisedCodeAndGoodsDescription()
		{
			var entryTypes = new[]
			{
				EntryTypeList.Codes.Message,
				EntryTypeList.Codes.EUPortOfDestination,
				EntryTypeList.Codes.OtherExemptions,
				EntryTypeList.Codes.ExitSummaryDeclarationWithoutMRN
			};

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_MarksAndNumbers = "SHP0123";
			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packLine = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var message = PackLinePortMessaging.LoadOrCreate(packLine);

			foreach (var entryType in entryTypes)
			{
				var expectedError = string.Format("HS Code or Packing Goods Description is required for Entry Type {0} - {1}."
					, entryType
					, message.Lookups.EntryTypeList.GetDescriptionFromCode(entryType));

				shipment.JS_SZB = "123456";
				shipment.JS_GoodsDescription = string.Empty;
				shipment.DetailedGoodsDescriptionNoteText = string.Empty;

				packLine.JL_HarmonisedCode = "AAA";
				packLine.JL_Description = "PG1";
				packLine.JL_DetailedDescription = string.Empty;

				message.JLM_EntryType = entryType;
				message.Validation.ValidateJLM_EntryType();

				AssertNoError("Should not has the expected error as the Harmonised Code and the pack line’s Goods Description are all not empty",
					message.JLM_EntryTypeInfo, expectedError);

				packLine.JL_HarmonisedCode = string.Empty;
				packLine.JL_Description = string.Empty;
				message.Validation.ValidateJLM_EntryType();

				AssertHasError("Should has the expected error as the Harmonised Code and the pack line’s Goods Description are all empty",
					message.JLM_EntryTypeInfo, expectedError);

				packLine.JL_HarmonisedCode = "BBB";
				packLine.JL_Description = string.Empty;
				message.Validation.ValidateJLM_EntryType();

				AssertNoError("Should not has the expected error as the Harmonised Code is not empty",
					message.JLM_EntryTypeInfo, expectedError);

				shipment.DetailedGoodsDescriptionNoteText = "CCC";
				packLine.JL_HarmonisedCode = string.Empty;
				message.Validation.ValidateJLM_EntryType();

				AssertNoError("Should not has the expected error as the shipment’s Detailed Goods Description is not empty",
					message.JLM_EntryTypeInfo, expectedError);

				shipment.DetailedGoodsDescriptionNoteText = string.Empty;
				shipment.JS_GoodsDescription = "DDD";
				message.Validation.ValidateJLM_EntryType();

				AssertNoError("Should not has the expected error as the shipment’s Goods Description is not empty",
					message.JLM_EntryTypeInfo, expectedError);

				shipment.JS_GoodsDescription = string.Empty;
				packLine.JL_DetailedDescription = "EEE";
				message.Validation.ValidateJLM_EntryType();

				AssertNoError("Should not has the expected error as the pack line’s Detailed Description is not empty",
					message.JLM_EntryTypeInfo, expectedError);
			}
		}

		public void TestMovementReferenceNumber()
		{
			AssertNoErrors("Pre-condition: expected mrn to have no errors", PortMessaging.JLM_MovementReferenceNumberInfo);
			AssertEquals("Pre-condition: expected mrn to default to blank", ZString.Empty, PortMessaging.JLM_MovementReferenceNumber);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertMovementReferenceNumberValidation(
				PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JLM_MovementReferenceNumberInfo as ZPropertyInfoString,
				PortMessaging.JLM_Annex30ATypeInfo as ZPropertyInfoString);
		}

		public void TestMovementReferenceNumberFormat()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertMovementReferenceNumberFormatValidation(
				PortMessaging.JLM_MovementReferenceNumberInfo as ZPropertyInfoString);
		}

		public void TestExemptionReason()
		{
			AssertEquals("Pre-condition: should default to empty", ZString.Empty, PortMessaging.JLM_ExemptionReason);
			AssertNoErrors("Pre-condition: should have no errors when empty", PortMessaging.JLM_ExemptionReasonInfo);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertExemptionReasonValidation(
				PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JLM_ExemptionReasonInfo as ZPropertyInfoString);
		}

		public void TestExemptionReasonObsoleteCodes()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			ForwardingPackLine packline = shipment.OuterPackLines.AddNew();
			PortMessaging.JLM_JL_PackLine = packline.PK;
			PortMessaging.JLM_EntryType = EntryTypeList.Codes.Message;

			PortMessagingValidationHelperTest.AssertExemptionReasonObsoleteCodesValidation(
				PortMessaging.JLM_ExemptionReasonInfo as ZPropertyInfoString,
				() => PortMessaging.Validation.ValidateJLM_ExemptionReason(),
				shipment);
		}

		public void TestATBNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertATBNumberValidation(
				PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JLM_ATBNumberInfo as ZPropertyInfoString,
				shipment);
		}

		public void TestAnnex30AType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertAnnex30ATypeValidation(
				PortMessaging.JLM_Annex30ATypeInfo as ZPropertyInfoString);
		}

		public void TestExportDeclarationReference()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertExportDeclarationReferenceValidation(
				PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JLM_ExportDeclarationReferenceInfo as ZPropertyInfoString);
		}

		public void TestCustomsReleaseDate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			PortMessaging.JLM_JL_PackLine = packline.PK;

			PortMessagingValidationHelperTest.AssertCustomsReleaseDateValidation(
				PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString,
				PortMessaging.JLM_CustomsReleaseDateInfo as ZPropertyInfoDateTime,
				() => PortMessaging.Validation.ValidateJLM_CustomsReleaseDate(),
				shipment);
		}

		const string DataEnteredAtBothLevelsError = "Port messaging data may be entered either at Shipment level or at Pack Line level, not both.";

		[TestDate(2023, 3, 1)]
		public void TestDataCouldBeEitherOnShipmentOrPackline()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			var packline = shipment.OuterPackLines.AddNew();
			var packlinePortMessaging = PackLinePortMessaging.LoadOrCreate(packline);

			Action<ZPropertyInfoString, ZPropertyInfoString, ZString> assertDuplicatedDataEntryIsValidated = (portMessagingInfo, relaredPortMessagingInfo, validValue) =>
				{
					relaredPortMessagingInfo.Value = "";
					portMessagingInfo.Value = "";
					AssertNoError(portMessagingInfo, DataEnteredAtBothLevelsError);

					relaredPortMessagingInfo.Value = validValue;
					portMessagingInfo.Value = validValue;
					AssertHasError(portMessagingInfo, DataEnteredAtBothLevelsError);

					relaredPortMessagingInfo.Value = validValue;
					portMessagingInfo.Value = "";
					AssertNoError(portMessagingInfo, DataEnteredAtBothLevelsError);

					relaredPortMessagingInfo.Value = "";
					portMessagingInfo.Value = validValue;
					AssertNoError(portMessagingInfo, DataEnteredAtBothLevelsError);

					relaredPortMessagingInfo.Value = "";
					portMessagingInfo.Value = "";
					AssertNoError(portMessagingInfo, DataEnteredAtBothLevelsError);
				};

			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			assertDuplicatedDataEntryIsValidated(
				packlinePortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString,
				shipmentPortMessaging.JSM_EntryTypeInfo as ZPropertyInfoString,
				EntryTypeList.Codes.Message);

			packlinePortMessaging.JLM_EntryType = EntryTypeList.Codes.Message;

			assertDuplicatedDataEntryIsValidated(
				packlinePortMessaging.JLM_ATBNumberInfo as ZPropertyInfoString,
				shipmentPortMessaging.JSM_ATBNumberInfo as ZPropertyInfoString,
				"ATB123456789123454851");

			assertDuplicatedDataEntryIsValidated(
				packlinePortMessaging.JLM_MovementReferenceNumberInfo as ZPropertyInfoString,
				shipmentPortMessaging.JSM_MovementReferenceNumberInfo as ZPropertyInfoString,
				"ATB123456789123454851");

			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				packlinePortMessaging.JLM_EntryType = EntryTypeList.Codes.AE1ExportDeclaration;
				assertDuplicatedDataEntryIsValidated(
				packlinePortMessaging.JLM_LocalReferenceNumberInfo as ZPropertyInfoString,
				shipmentPortMessaging.JSM_LocalReferenceNumberInfo as ZPropertyInfoString,
				"ATB123456789123454851");
			}
		}

		public void TestDataCouldBeEitherOnShipmentOrPackline_ForwardingOfficeID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var shipmentPortMessaging = ShipmentPortMessaging.LoadOrCreate(shipment);

			var packline = shipment.OuterPackLines.AddNew();
			var packlinePortMessaging = PackLinePortMessaging.LoadOrCreate(packline);

			shipmentPortMessaging.JSM_ForwardingCustomsOfficeCode = "SCAMBURG";
			shipmentPortMessaging.JSM_EntryType = EntryTypeList.Codes.Message;
			packlinePortMessaging.JLM_EntryType = EntryTypeList.Codes.Message;

			AssertHasError(packlinePortMessaging.JLM_EntryTypeInfo, DataEnteredAtBothLevelsError);

			shipmentPortMessaging.JSM_EntryType = ZString.Empty;
			packlinePortMessaging.Validation.ValidateJLM_EntryType();
			AssertNoError(packlinePortMessaging.JLM_EntryTypeInfo, DataEnteredAtBothLevelsError);
		}

		public void TestEntryType_CheckSiblingPackLinePortMessagingsForCompatibleEntryTypes()
		{
			var entryTypeErrorMessage = "Only AES, AEM and DUX Entry Types can be submitted together. All pack lines should have an Entry Type or all must be blank.";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "DEHAM";

			var packline1 = shipment.OuterPackLines.AddNew();
			var packline2 = shipment.OuterPackLines.AddNew();
			var packline3 = shipment.OuterPackLines.AddNew();

			var portMessaging1 = PackLinePortMessaging.LoadOrCreate(packline1);
			var portMessaging2 = PackLinePortMessaging.LoadOrCreate(packline2);
			var portMessaging3 = PackLinePortMessaging.LoadOrCreate(packline3);

			var portMessagings = new[] { portMessaging1, portMessaging2, portMessaging3 };

			AssertNoEntryTypeErrors("Pre-condition: expected no errors on any packLine entry type", portMessagings);

			portMessaging1.JLM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
			AssertHasError("Expected error as packline1 has an entry type while the rest are blank", portMessaging1.JLM_EntryTypeInfo, entryTypeErrorMessage);

			portMessaging2.JLM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
			portMessaging3.JLM_EntryType = EntryTypeList.Codes.AESExportDeclaration;
			AssertNoEntryTypeErrors("Expect no errors as the entry type is the same for all packlines", portMessagings);

			portMessaging2.JLM_EntryType = EntryTypeList.Codes.Message;
			AssertHasError("Expected error as packline2 has an entry type different to the others", portMessaging2.JLM_EntryTypeInfo, entryTypeErrorMessage);

			portMessaging2.JLM_EntryType = EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities;
			AssertNoEntryTypeErrors("Expect no errors as all packlines have compatible entry types", portMessagings);

			portMessaging3.JLM_EntryType = EntryTypeList.Codes.ExitSummaryDeclaration;
			AssertNoEntryTypeErrors("Expect no errors as all packlines have compatible entry types", portMessagings);

			portMessaging1.JLM_EntryType = EntryTypeList.Codes.ConsolidatedContainer;
			AssertHasError("Expected error as packline1 has an entry type different to the others", portMessaging1.JLM_EntryTypeInfo, entryTypeErrorMessage);

			portMessaging1.JLM_EntryType = "YYY";
			AssertNoErrors("Expected no errors as packline2 and packline3 match, while packline1 is invalid so is excluded from compatibility check", portMessaging2.JLM_EntryTypeInfo);
			AssertNoErrors("Expected no errors as packline2 and packline3 match, while packline1 is invalid so is excluded from compatibility check", portMessaging3.JLM_EntryTypeInfo);

			portMessaging1.JLM_EntryType = EntryTypeList.Codes.AESExportDeclarationForMarketRegulationCommodities;
			AssertNoEntryTypeErrors("Expect no errors as all packline entry types are valid and compatible", portMessagings);
		}

		[TestDate(2023, 3, 1)]
		public void TestLocalReferenceNumber()
		{
			using (PortMessagingRegistry.Instance.EORIAndLRNEffectiveDate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1)))
			{
				AssertNoErrors("Pre-condition: expected lrn to have no errors", PortMessaging.JLM_LocalReferenceNumberInfo);
				AssertEquals("Pre-condition: expected lrn to default to blank", ZString.Empty, PortMessaging.JLM_LocalReferenceNumber);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Sea;
				var packline = shipment.OuterPackLines.AddNew();

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_RL_NKLoadPort = "DEHAM";

				PortMessaging.JLM_JL_PackLine = packline.PK;

				PortMessagingValidationHelperTest.AssertLocalReferenceNumberValidation(
					PortMessaging.JLM_EntryTypeInfo as ZPropertyInfoString,
					PortMessaging.JLM_LocalReferenceNumberInfo as ZPropertyInfoString);
			}
		}

		#region Implementation

		PackLinePortMessaging PortMessaging
		{
			get { return portMessaging ?? (portMessaging = Factory.New<PackLinePortMessaging>()); }
		}
		PackLinePortMessaging portMessaging;

		void AssertNoEntryTypeErrors(ZString errorMessage, PackLinePortMessaging[] portMessagings)
		{
			foreach (var portMessaging in portMessagings)
			{
				var oldEntryType = portMessaging.JLM_EntryType;
				portMessaging.JLM_EntryType = ZString.Empty;
				portMessaging.JLM_EntryType = oldEntryType;     //forces revalidation

				AssertNoErrors(errorMessage, portMessaging.JLM_EntryTypeInfo);
			}
		}

		#endregion
	}
}
