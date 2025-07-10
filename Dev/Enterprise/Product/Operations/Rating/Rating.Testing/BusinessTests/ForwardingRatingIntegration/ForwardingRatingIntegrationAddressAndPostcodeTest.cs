using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.RatingTests.Testing.GUI
{
	public sealed class ForwardingRatingIntegrationAddressAndPostcodeTest : BaseRatingIntegrationTest
	{
		#region Pickup and Consignor organisations

		void SetupSameChargeCodeEntriesWithOrganisationsOnConsignorFields()
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupOrg = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 500);
			entryWithPickupOrg.TI_OH_Consignor = NewClient.PK;

			var entryWithConsignorOrg = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 600);
			entryWithConsignorOrg.TI_OH_Consignor = Consignor.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN
		/// - rate entries setup with organisations overrides having the same charge codes
		/// - Pickup address is not overridden
		/// WHEN autorating shipment
		/// THEN
		/// - Pickup address is used for matching rate entry and the rate has the highest priority among the same charge code rates
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorOrganisations_GivenSameChargeCodes_PickupAddressIsNotOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 500 } };
			AutorateAndAssert("Charge with more specific parameter should take priority", expectedCharges, shipment, Consignor, autorateCosts: false);
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x3)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1\treason:\toverridden by ODOC-FLT-Client Rate CONSIGNOR1 by Consignor comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN
		/// - rate entries setup with organisations overrides having the same charge codes
		/// - Pickup address is overridden
		/// WHEN autorating shipment
		/// THEN
		/// - Pickup address is not used for matching rate entry
		/// - Consignor address is used and its matched rate has higher priority than the blank one
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorOrganisations_GivenSameChargeCodes_PickupAddressIsOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;

			var expectedCharges = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 600 } };
			AutorateAndAssert("charge with more specific parameter should take priority", expectedCharges, shipment, Consignor, autorateCosts: false);

			var message = "The log doesn't say clearly that the entry with Pickup Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1\treason:\toverridden by ODOC-FLT-Client Rate CONSIGNOR1 by Consignor comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		/// <summary>
		/// GIVEN
		/// - rate entries setup with organisations overrides having the same charge codes
		/// - Pickup address is blank
		/// WHEN autorating shipment
		/// THEN
		/// - Consignor address is used and its matched rate has higher priority than the blank one
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorOrganisations_GivenSameChargeCodes_PickupAddressIsBlank()
		{
			SetupSameChargeCodeEntriesWithOrganisationsOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;

			var expectedCharges = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 600 } };
			AutorateAndAssert("charge with more specific parameter should take priority", expectedCharges, shipment, Consignor, autorateCosts: false);
			var message = "The log doesn't say clearly that the entry with Pickup Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1\treason:\toverridden by ODOC-FLT-Client Rate CONSIGNOR1 by Consignor comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		void SetupDifferentChargeCodeEntriesWithOrganisationsOnConsignorFields()
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OTHC", 500);
			entryWithPickupPostcode.TI_OH_Consignor = NewClient.PK;

			var entryWithConsignorPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 600);
			entryWithConsignorPostcode.TI_OH_Consignor = Consignor.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OCART", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN
		/// - rates with different charge codes
		/// - Pickup address is not overridden
		/// WHEN autorating shipment
		/// THEN
		/// - all rates' organisation overrides matching shipment's pickup organisation and consignor should apply
		/// - plus the rate with blank organisation override.
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorOrganisations_GivenDifferentChargeCodes_PickupAddressIsNotOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "OTHC", JR_OSSellAmt = 500 },
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "OCART", JR_OSSellAmt = 700 }
			};
			AutorateAndAssert("All charges should be created", expectedCharges, shipment, Consignee, autorateCosts: false);
		}

		/// <summary>
		/// GIVEN
		/// - rates with different charge codes
		/// - Pickup address is overridden
		/// WHEN autorating shipment
		/// THEN
		/// - rate matching shipment's consignor should apply
		/// - plus the rate with blank organisation override.
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorOrganisations_GivenDifferentChargeCodes_PickupAddressIsOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "OCART", JR_OSSellAmt = 700 }
			};
			AutorateAndAssert("Only charges with blank and consignor address should be created", expectedCharges, shipment, Consignee, autorateCosts: false);

			var message = "The log doesn't say clearly that the entry with Pickup Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		/// <summary>
		/// GIVEN
		/// - rates with different charge codes
		/// - Pickup address is blank
		/// WHEN autorating shipment
		/// THEN
		/// - rate matching shipment's consignor should apply
		/// - plus the rate with blank organisation override.
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorOrganisations_GivenDifferentChargeCodes_PickupAddressIsBlank()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "OCART", JR_OSSellAmt = 700 }
			};
			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

			var message = "The log doesn't say clearly that the entry with Pickup Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		#endregion

		#region Delivery and Consignee organisations

		void SetupSameChargeCodeEntriesWithOrganisationsOnConsigneeFields()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryOrg = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 500);
			entryWithDeliveryOrg.TI_OH_Consignee = NewClient.PK;

			var entryWithConsigneeOrg = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 600);
			entryWithConsigneeOrg.TI_OH_Consignee = Consignee.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN
		/// - rate entries setup with organisations overrides having the same charge codes
		/// - Delivery address is not overridden
		/// WHEN autorating shipment
		/// THEN
		/// - Delivery address is used for matching rate entry and the rate has the highest priority among the same charge code rates
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeOrganisations_GivenSameChargeCodes_DeliveryAddressIsNotOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 500 } };
			AutorateAndAssert("Charge with more specific parameter should take priority", expectedCharges, shipment, Consignee, autorateCosts: false);
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1 (x3)",
				"Information: RateLine Filtered DDOC-FLT-Client Rate CONSIGNEE1\treason:\toverridden by DDOC-FLT-Client Rate CONSIGNEE1 by Consignee comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN
		/// - rate entries setup with organisations overrides having the same charge codes
		/// - Delivery address is overridden
		/// WHEN autorating shipment
		/// THEN
		/// - Delivery address is not used for matching rate entry
		/// - Consignee address is used and its matched rate has higher priority than the blank one
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeOrganisations_GivenSameChargeCodes_DeliveryAddressIsOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;

			var expectedCharges = new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 600 } };
			AutorateAndAssert("charge with more specific parameter should take priority", expectedCharges, shipment, Consignee, autorateCosts: false);

			var message = "The log doesn't say clearly that the entry with Delivery Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1 (x2)",
				"Information: RateLine Filtered DDOC-FLT-Client Rate CONSIGNEE1\treason:\toverridden by DDOC-FLT-Client Rate CONSIGNEE1 by Consignee comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		/// <summary>
		/// GIVEN
		/// - rate entries setup with organisations overrides having the same charge codes
		/// - Delivery address is blank
		/// WHEN autorating shipment
		/// THEN
		/// - Consignee address is used and its matched rate has higher priority than the blank one
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeOrganisations_GivenSameChargeCodes_DeliveryAddressIsBlank()
		{
			SetupSameChargeCodeEntriesWithOrganisationsOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;

			var expectedCharges = new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 600 } };
			AutorateAndAssert("charge with more specific parameter should take priority", expectedCharges, shipment, Consignee, autorateCosts: false);
			var message = "The log doesn't say clearly that the entry with Delivery Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1 (x2)",
				"Information: RateLine Filtered DDOC-FLT-Client Rate CONSIGNEE1\treason:\toverridden by DDOC-FLT-Client Rate CONSIGNEE1 by Consignee comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		void SetupDifferentChargeCodeEntriesWithOrganisationsOnConsigneeFields()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DTHC", 500);
			entryWithDeliveryPostcode.TI_OH_Consignee = NewClient.PK;

			var entryWithConsigneePostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 600);
			entryWithConsigneePostcode.TI_OH_Consignee = Consignee.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DCART", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN
		/// - rates with different charge codes
		/// - Delivery address is not overridden
		/// WHEN autorating shipment
		/// THEN
		/// - all rates' organisation overrides matching shipment's delivery organisation and consignee should apply
		/// - plus the rate with blank organisation override.
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeOrganisations_GivenDifferentChargeCodes_DeliveryAddressIsNotOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DTHC", JR_OSSellAmt = 500 },
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "DCART", JR_OSSellAmt = 700 }
			};
			AutorateAndAssert("All charges should be created", expectedCharges, shipment, Consignee, autorateCosts: false);
		}

		/// <summary>
		/// GIVEN
		/// - rates with different charge codes
		/// - Delivery address is overridden
		/// WHEN autorating shipment
		/// THEN
		/// - rate matching shipment's consignee should apply
		/// - plus the rate with blank organisation override.
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeOrganisations_GivenDifferentChargeCodes_DeliveryAddressIsOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = NewClient.MainAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "DCART", JR_OSSellAmt = 700 }
			};
			AutorateAndAssert("Only charges with blank and consignee address should be created", expectedCharges, shipment, Consignee, autorateCosts: false);

			var message = "The log doesn't say clearly that the entry with Delivery Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		/// <summary>
		/// GIVEN
		/// - rates with different charge codes
		/// - Delivery address is blank
		/// WHEN autorating shipment
		/// THEN
		/// - rate matching shipment's consignee should apply
		/// - plus the rate with blank organisation override.
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeOrganisations_GivenDifferentChargeCodes_DeliveryAddressIsBlank()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "DCART", JR_OSSellAmt = 700 }
			};
			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

			var message = "The log doesn't say clearly that the entry with Delivery Org is filtered but it should have lines";
			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, message, expectedLogLines);
		}

		#endregion

		#region Pickup and Consignor addresses

		void SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields()
		{
			Consignor.MainAddress.Address1 = "ConsignorOffice";
			var pickupAddress = NewClient.MainAddress;
			pickupAddress.Address1 = "PickupPlace";

			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 500);
			entryWithPickupAddress.TI_OH_Consignor = NewClient.PK;
			entryWithPickupAddress.TI_OA_CartagePickupAddressOverride = pickupAddress.PK;

			var entryWithConsignorAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 600);
			entryWithConsignorAddress.TI_OH_Consignor = Consignor.PK;
			entryWithConsignorAddress.TI_OA_CartagePickupAddressOverride = Consignor.MainAddress.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN pickup address is not overridden, same charge code found
		/// WHEN autorating shipment
		/// THEN the address should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorAddresses_GivenSameChargeCode_PickupAddressIsNotOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = NewClient.MainAddress.PK;
			// precondition
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 500 } };
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Pickup/Consignor Address didn't match job ConsignorOffice,PickupPlace.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1\treason:\toverridden by ODOC-FLT-Client Rate CONSIGNOR1 by Consignor comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN pickup address is overridden, same charge code found
		/// WHEN autorating shipment
		/// THEN the address should not be used to match rate entries but fallback to consignor address
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorAddresses_GivenSameChargeCode_PickupAddressIsOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = NewClient.MainAddress.PK;
			// precondition
			shipment.ConsignorPickupAddress.E2_AddressOverride = true;

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 600 } };
			AutorateAndAssert("Rate matches consignor should have priority", expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1\treason:\toverridden by ODOC-FLT-Client Rate CONSIGNOR1 by Consignor comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN pickup address is leaving empty, same charge code found
		/// WHEN autorating shipment
		/// THEN the consignor address should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorAddresses_GivenSameChargeCode_PickupAddressIsBlank()
		{
			SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: consignor address", Consignor.MainAddress.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 600 } };
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1\treason:\toverridden by ODOC-FLT-Client Rate CONSIGNOR1 by Consignor comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		void SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields()
		{
			Consignor.MainAddress.Address1 = "ConsignorOffice";
			var pickupAddress = NewClient.MainAddress;
			pickupAddress.Address1 = "PickupPlace";

			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 500);
			entryWithPickupAddress.TI_OH_Consignor = NewClient.PK;
			entryWithPickupAddress.TI_OA_CartagePickupAddressOverride = pickupAddress.PK;

			var entryWithConsignorAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OCART", 600);
			entryWithConsignorAddress.TI_OH_Consignor = Consignor.PK;
			entryWithConsignorAddress.TI_OA_CartagePickupAddressOverride = Consignor.MainAddress.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OTHC", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN pickup address is not overridden, different charge codes found
		/// WHEN autorating shipment
		/// THEN the address should be used to match rate entry, the blank address rate is also accepted
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorAddresses_GivenDifferentChargeCodes_PickupAddressIsNotOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = NewClient.MainAddress.PK;
			// precondition
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			AssertEquals("Precondition: consignor address", Consignor.MainAddress.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 500 },
				new AssertionCharge { ChargeCode = "OTHC", JR_OSSellAmt = 700 },
			};
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Pickup/Consignor Address didn't match job ConsignorOffice,PickupPlace.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Found OTHC-FLT-Client Rate CONSIGNOR1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN pickup address is overridden, different charge codes found
		/// WHEN autorating shipment
		/// THEN the address consignor should be fallen back and used to match rate entry, the blank address rate is also accepted
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorAddresses_GivenDifferentChargeCodes_PickupAddressIsOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			AssertEquals("Precondition: consignor address", Consignor.MainAddress.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "OCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "OTHC", JR_OSSellAmt = 700 },
			};
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
				"Information: RateLine Found OCART-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Found OTHC-FLT-Client Rate CONSIGNOR1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN pickup address is blank, different charge codes found
		/// WHEN autorating shipment
		/// THEN the address consignor should be fallen back and used to match rate entry, the blank address rate is also accepted
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorAddresses_GivenDifferentChargeCodes_PickupAddressIsBlank()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsignorFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsignorPickupAddress.E2_AddressOverride = false;
			AssertEquals("Precondition: consignor address", Consignor.MainAddress.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "OCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "OTHC", JR_OSSellAmt = 700 },
			};
			AutorateAndAssert(expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: Consignor didn't match job CONSIGNOR1.",
				"Information: RateLine Found OCART-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Found OTHC-FLT-Client Rate CONSIGNOR1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		#endregion

		#region Delivery and Consignee addresses

		void SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields()
		{
			Consignee.MainAddress.Address1 = "ConsigneeOffice";
			var deliveryAddress = NewClient.MainAddress;
			deliveryAddress.Address1 = "DeliveryPlace";

			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 500);
			entryWithDeliveryAddress.TI_OH_Consignee = NewClient.PK;
			entryWithDeliveryAddress.TI_OA_CartageDeliveryAddressOverride = deliveryAddress.PK;

			var entryWithConsigneeAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 600);
			entryWithConsigneeAddress.TI_OH_Consignee = Consignee.PK;
			entryWithConsigneeAddress.TI_OA_CartageDeliveryAddressOverride = Consignee.MainAddress.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN delivery address is not overridden, same charge code found
		/// WHEN autorating shipment
		/// THEN the address should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeAddresses_GivenSameChargeCode_DeliveryAddressIsNotOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = NewClient.MainAddress.PK;
			// precondition
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 500 } };
			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Delivery/Consignee Address didn't match job ConsigneeOffice,DeliveryPlace.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1 (x2)",
				"Information: RateLine Filtered DDOC-FLT-Client Rate CONSIGNEE1\treason:\toverridden by DDOC-FLT-Client Rate CONSIGNEE1 by Consignee comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN delivery address is overridden, same charge code found
		/// WHEN autorating shipment
		/// THEN the address should not be used to match rate entries but fallback to consignee address
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeAddresses_GivenSameChargeCode_DeliveryAddressIsOverridden()
		{
			SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = NewClient.MainAddress.PK;
			// precondition
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = true;

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 600 } };
			AutorateAndAssert("Rate matches consignee should have priority", expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1 (x2)",
				"Information: RateLine Filtered DDOC-FLT-Client Rate CONSIGNEE1\treason:\toverridden by DDOC-FLT-Client Rate CONSIGNEE1 by Consignee comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN delivery address is leaving empty, same charge code found
		/// WHEN autorating shipment
		/// THEN the consignee address should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeAddresses_GivenSameChargeCode_DeliveryAddressIsBlank()
		{
			SetupSameChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: consignee address", Consignee.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[] { new AssertionCharge { JR_OSSellAmt = 600 } };
			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1 (x2)",
				"Information: RateLine Filtered DDOC-FLT-Client Rate CONSIGNEE1\treason:\toverridden by DDOC-FLT-Client Rate CONSIGNEE1 by Consignee comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		void SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields()
		{
			Consignee.MainAddress.Address1 = "ConsigneeOffice";
			var deliveryAddress = NewClient.MainAddress;
			deliveryAddress.Address1 = "DeliveryPlace";

			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 500);
			entryWithDeliveryAddress.TI_OH_Consignee = NewClient.PK;
			entryWithDeliveryAddress.TI_OA_CartageDeliveryAddressOverride = deliveryAddress.PK;

			var entryWithConsigneeAddress = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DCART", 600);
			entryWithConsigneeAddress.TI_OH_Consignee = Consignee.PK;
			entryWithConsigneeAddress.TI_OA_CartageDeliveryAddressOverride = Consignee.MainAddress.PK;

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DTHC", 700);

			Factory.Save();
		}

		/// <summary>
		/// GIVEN delivery address is not overridden, different charge codes found
		/// WHEN autorating shipment
		/// THEN the address should be used to match rate entry, the blank address rate is also accepted
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeAddresses_GivenDifferentChargeCodes_DeliveryAddressIsNotOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = NewClient.MainAddress.PK;
			// precondition
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			AssertEquals("Precondition: consignee address", Consignee.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 500 },
				new AssertionCharge { ChargeCode = "DTHC", JR_OSSellAmt = 700 },
			};
			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Delivery/Consignee Address didn't match job ConsigneeOffice,DeliveryPlace.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1",
				"Information: RateLine Found DTHC-FLT-Client Rate CONSIGNEE1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN delivery address is overridden, different charge codes found
		/// WHEN autorating shipment
		/// THEN the address consignee should be fallen back and used to match rate entry, the blank address rate is also accepted
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeAddresses_GivenDifferentChargeCodes_DeliveryAddressIsOverridden()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			AssertEquals("Precondition: consignee address", Consignee.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "DTHC", JR_OSSellAmt = 700 },
			};
			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
				"Information: RateLine Found DCART-FLT-Client Rate CONSIGNEE1",
				"Information: RateLine Found DTHC-FLT-Client Rate CONSIGNEE1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN delivery address is blank, different charge codes found
		/// WHEN autorating shipment
		/// THEN the address consignee should be fallen back and used to match rate entry, the blank address rate is also accepted
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneeAddresses_GivenDifferentChargeCodes_DeliveryAddressIsBlank()
		{
			SetupDifferentChargeCodeEntriesWithOrganisationsAndAddressesOnConsigneeFields();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;
			AssertEquals("Precondition: consignee address", Consignee.MainAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "DTHC", JR_OSSellAmt = 700 },
			};
			AutorateAndAssert(expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: Consignee didn't match job CONSIGNEE1.",
				"Information: RateLine Found DCART-FLT-Client Rate CONSIGNEE1",
				"Information: RateLine Found DTHC-FLT-Client Rate CONSIGNEE1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		#endregion

		#region Pickup and Consignor postcodes

		/// <summary>
		/// GIVEN pickup address is not overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the pickup postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsNotBlank_PickupAddressIsNotOverridden()
			=> TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsNotBlank(isAddressOverridden: false);

		/// <summary>
		/// GIVEN pickup address is overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the pickup postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsNotBlank_PickupAddressIsOverridden()
			=> TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsNotBlank(isAddressOverridden: true);

		void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsNotBlank(bool isAddressOverridden)
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 500);
			entryWithPickupPostcode.TI_CartagePickupAddressPostCode = "2015";

			var entryWithConsignorPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OCART", 600);
			entryWithConsignorPostcode.TI_CartagePickupAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OTHC", 700);

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);

			var consignorAddress = Consignor.MainAddress;
			consignorAddress.OA_PostCode = "2016";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorAddress.PK;

			var pickupAddress = NewClient.MainAddress;
			pickupAddress.OA_PostCode = "2015";
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = isAddressOverridden;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 500 },
				new AssertionCharge { ChargeCode = "OTHC", JR_OSSellAmt = 700 },
			};
			var message = "Whether the pickup address is overridden or not, we don't fallback and use its postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: From Postcode didn't match job 2016,2015.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Found OTHC-FLT-Client Rate CONSIGNOR1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN pickup address is not overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the pickup postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsBlank_PickupAddressIsNotOverridden()
			=> TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsBlank(isAddressOverridden: false);

		/// <summary>
		/// GIVEN pickup address is overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the pickup postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsBlank_PickupAddressIsOverridden()
			=> TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsBlank(isAddressOverridden: true);

		void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPostcodeIsBlank(bool isAddressOverridden)
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 500);
			entryWithPickupPostcode.TI_CartagePickupAddressPostCode = "2015";

			var entryWithConsignorPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OCART", 600);
			entryWithConsignorPostcode.TI_CartagePickupAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OTHC", 700);

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);

			var consignorAddress = Consignor.MainAddress;
			consignorAddress.OA_PostCode = "2016";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorAddress.PK;

			var pickupAddress = NewClient.MainAddress;
			pickupAddress.OA_PostCode = ZString.Empty;
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsignorPickupAddress.E2_AddressOverride = isAddressOverridden;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "OCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "OTHC", JR_OSSellAmt = 700 },
			};
			var message = "Whether the pickup address is overridden or not, when it is blank, we fallback to consignor documentary postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: From Postcode didn't match job 2015,2016.",
				"Information: RateLine Found OCART-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Found OTHC-FLT-Client Rate CONSIGNOR1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN pickup address is blank and different charge codes
		/// WHEN autorating shipment
		/// THEN the consignor postcode should be used to match rate entry. The rate with blank postcode should come through.
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPickupAddressIsBlank_DifferentChargeCodes()
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 500);
			entryWithPickupPostcode.TI_CartagePickupAddressPostCode = "2015";

			var entryWithConsignorPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OCART", 600);
			entryWithConsignorPostcode.TI_CartagePickupAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "OTHC", 700);

			var consignorAddress = Consignor.MainAddress;
			consignorAddress.OA_PostCode = "2016";

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: consignor address with postcode", consignorAddress.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "OCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "OTHC", JR_OSSellAmt = 700 },
			};
			var message = "When the pickup address is not set, we fallback to consignor address and use its postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: From Postcode didn't match job 2015,2016.",
				"Information: RateLine Found OCART-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Found OTHC-FLT-Client Rate CONSIGNOR1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN pickup address is blank and same charge codes
		/// WHEN autorating shipment
		/// THEN rate with consignor postcode should have higher priority than the rate with blank one
		/// </summary>
		public void TestAutorateShipmentWithPickupAndConsignorPostcode_GivenPickupAddressIsBlank_SameChargeCodes()
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var entryWithPickupPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 500);
			entryWithPickupPostcode.TI_CartagePickupAddressPostCode = "2015";

			var entryWithConsignorPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 600);
			entryWithConsignorPostcode.TI_CartagePickupAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "USLAX", "ODOC", 700);

			var consignorAddress = Consignor.MainAddress;
			consignorAddress.OA_PostCode = "2016";

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsignorPickupAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: consignor address with postcode", consignorAddress.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 600 },
			};
			var message = "When the pickup address is not set, we fallback to consignor address and use its postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNOR1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNOR1 reason: From Postcode didn't match job 2015,2016.",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1\treason:\toverridden by ODOC-FLT-Client Rate CONSIGNOR1 by TI_CartagePickupAddressPostCode comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		#endregion

		#region Delivery and Consignee postcodes

		/// <summary>
		/// GIVEN delivery address is not overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the delivery postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsNotBlank_DeliveryAddressIsNotOverridden()
			=> TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsNotBlank(isAddressOverridden: false);

		/// <summary>
		/// GIVEN delivery address is overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the delivery postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsNotBlank_DeliveryAddressIsOverridden()
			=> TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsNotBlank(isAddressOverridden: true);

		void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsNotBlank(bool isAddressOverridden)
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 500);
			entryWithDeliveryPostcode.TI_CartageDeliveryAddressPostCode = "2015";

			var entryWithConsigneePostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DCART", 600);
			entryWithConsigneePostcode.TI_CartageDeliveryAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DTHC", 700);

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);

			var consigneeAddress = Consignee.MainAddress;
			consigneeAddress.OA_PostCode = "2016";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeAddress.PK;

			var deliveryAddress = NewClient.MainAddress;
			deliveryAddress.OA_PostCode = "2015";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = isAddressOverridden;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 500 },
				new AssertionCharge { ChargeCode = "DTHC", JR_OSSellAmt = 700 },
			};
			var message = "Whether the delivery address is overridden or not, we don't fallback and use its postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 2016,2015.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1",
				"Information: RateLine Found DTHC-FLT-Client Rate CONSIGNEE1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN delivery address is not overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the delivery postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsBlank_DeliveryAddressIsNotOverridden()
			=> TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsBlank(isAddressOverridden: false);

		/// <summary>
		/// GIVEN delivery address is overridden and its postcode is not blank
		/// WHEN autorating shipment
		/// THEN the delivery postcode should be used to match rate entry
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsBlank_DeliveryAddressIsOverridden()
			=> TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsBlank(isAddressOverridden: true);

		void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenPostcodeIsBlank(bool isAddressOverridden)
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 500);
			entryWithDeliveryPostcode.TI_CartageDeliveryAddressPostCode = "2015";

			var entryWithConsigneePostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DCART", 600);
			entryWithConsigneePostcode.TI_CartageDeliveryAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DTHC", 700);

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);

			var consigneeAddress = Consignee.MainAddress;
			consigneeAddress.OA_PostCode = "2016";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeAddress.PK;

			var deliveryAddress = NewClient.MainAddress;
			deliveryAddress.OA_PostCode = ZString.Empty;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = isAddressOverridden;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "DTHC", JR_OSSellAmt = 700 },
			};
			var message = "Whether the delivery address is overridden or not, when it is blank, we fallback to consignee documentary postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 2015,2016.",
				"Information: RateLine Found DCART-FLT-Client Rate CONSIGNEE1",
				"Information: RateLine Found DTHC-FLT-Client Rate CONSIGNEE1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN delivery address is blank and different charge codes
		/// WHEN autorating shipment
		/// THEN the consignee postcode should be used to match rate entry. The rate with blank postcode should come through.
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenDeliveryAddressIsBlank_DifferentChargeCodes()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 500);
			entryWithDeliveryPostcode.TI_CartageDeliveryAddressPostCode = "2015";

			var entryWithConsigneePostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DCART", 600);
			entryWithConsigneePostcode.TI_CartageDeliveryAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DTHC", 700);

			var consigneeAddress = Consignee.MainAddress;
			consigneeAddress.OA_PostCode = "2016";

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: consignee address with postcode", consigneeAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DCART", JR_OSSellAmt = 600 },
				new AssertionCharge { ChargeCode = "DTHC", JR_OSSellAmt = 700 },
			};
			var message = "When the delivery address is not set, we fallback to consignee address and use its postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 2015,2016.",
				"Information: RateLine Found DCART-FLT-Client Rate CONSIGNEE1",
				"Information: RateLine Found DTHC-FLT-Client Rate CONSIGNEE1",
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		/// <summary>
		/// GIVEN delivery address is blank and same charge codes
		/// WHEN autorating shipment
		/// THEN rate with consignee postcode should have higher priority than the rate with blank one
		/// </summary>
		public void TestAutorateShipmentWithDeliveryAndConsigneePostcode_GivenDeliveryAddressIsBlank_SameChargeCodes()
		{
			var clientRate = Helper.NewClientRate(Consignee);

			var entryWithDeliveryPostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 500);
			entryWithDeliveryPostcode.TI_CartageDeliveryAddressPostCode = "2015";

			var entryWithConsigneePostcode = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 600);
			entryWithConsigneePostcode.TI_CartageDeliveryAddressPostCode = "2016";

			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "USLAX", "DDOC", 700);

			var consigneeAddress = Consignee.MainAddress;
			consigneeAddress.OA_PostCode = "2016";

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignee.PK, Consignee.PK, "AUSYD", "USLAX", 100);
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			AssertEquals("Precondition: consignee address with postcode", consigneeAddress.PK, shipment.ConsigneeDocumentaryAddress.E2_OA_Address);

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 600 },
			};
			var message = "When the delivery address is not set, we fallback to consignee address and use its postcode to match rate entries";
			AutorateAndAssert(message, expectedCharges, shipment, Consignee, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RatingHeader Found Client Rate CONSIGNEE1 Entries: 3",
				"Information: RateEntry Filtered Client Rate CONSIGNEE1 reason: To Postcode didn't match job 2015,2016.",
				"Information: RateLine Found DDOC-FLT-Client Rate CONSIGNEE1 (x2)",
				"Information: RateLine Filtered DDOC-FLT-Client Rate CONSIGNEE1\treason:\toverridden by DDOC-FLT-Client Rate CONSIGNEE1 by TI_CartageDeliveryAddressPostCode comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should contain log lines", expectedLogLines);
		}

		#endregion

		#region CTZ Calculator with City And Postcode

		/// <summary>
		/// GIVEN pickup postcode is not empty
		/// WHEN autorating shipment
		/// THEN the postcode should be used to match the zone items
		/// ELSE
		/// GIVEN pickup postcode is empty
		/// WHEN autorating shipment
		/// THEN the consignor documentary postcode should be used to match the zone items
		/// </summary>
		public void TestCTZCalculatorWithPickupAndConsignorPostcode()
		{
			// with the below setup, ODOC charge will be created in job with FDA department. Let's set its department. to ALL so the job doesn't have error.
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");

			var postcodeSydney = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var postcodeMelbourne = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "3000"));
			var cityBrisbane = Factory.LoadTop1<RefCityTown>(new ZQuery(RefCityTownSchema.R9_InternationalName, "Brisbane"));

			var zoneSet = Helper.CreateRateTransportZoneSet(Consignor, Constants.CountryCodes.Australia, Helper.GetCityTown("Sydney", "NSW"));
			var sydZone = zoneSet.CreateRateTransportZoneForTest("Syd Zone");
			sydZone.CreateRateTransportZoneItemForTest(postcodeSydney);
			var melZone = zoneSet.CreateRateTransportZoneForTest("Mel Zone");
			melZone.CreateRateTransportZoneItemForTest(postcodeMelbourne);
			var bneZone = zoneSet.CreateRateTransportZoneForTest("Bne Zone");
			var zoneItem = bneZone.CreateRateTransportZoneItemForTest(cityBrisbane);
			zoneItem.TQ_IsExcludingPostCode = true;

			var clientRate = Helper.NewClientRate(Consignor);
			var orgEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Constants.RateMode.AIR, "AUSYD", "AUMEL");
			var orgLine = orgEntry.AddRateLine("ODOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var originCalculator = orgLine.GetCalculator<CartageZoneDistanceCalculator>();
			originCalculator.EquipmentType = Constants.EquipmentNeeded.Any;
			originCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 20m, sydZone.PK);
			originCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30m, melZone.PK);
			originCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 40m, bneZone.PK);

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUMEL", 100);

			var pickupAddress = NewClient.MainAddress;
			pickupAddress.OA_PostCode = "2000";
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;

			var consignorAddress = Consignor.MainAddress;
			consignorAddress.OA_PostCode = "3000";
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignorAddress.PK;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 2000 },
			};
			var message = "Given pickup postcode is not empty, the postcode should match the postcode zone item";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);
			AssertAutoratingAuditLogContains(shipment, @"Information: Matched 'Syd Zone' for RateLine ODOC-CTZ-KG-Client Rate CONSIGNOR1
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Syd Zone' transport zone matched by Consignor Pickup/Delivery Address fallback");

			pickupAddress.OA_PostCode = ZString.Empty;
			pickupAddress.OA_City = "Brisbane";

			expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 4000 },
			};
			message = "Given pickup postcode is empty, the pick up city should match the CITY ONLY zone item";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);
			AssertAutoratingAuditLogContains(shipment, @"Information: Matched 'Bne Zone' for RateLine ODOC-CTZ-KG-Client Rate CONSIGNOR1
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Bne Zone' transport zone matched by Consignor Pickup/Delivery Address fallback");

			pickupAddress.OA_PostCode = ZString.Empty;
			pickupAddress.OA_City = ZString.Empty;

			expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 3000 },
			};
			message = "Given both pickup postcode and city are empty, should fallback to use the document address postcode";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);
			AssertAutoratingAuditLogContains(shipment, @"Information: Matched 'Mel Zone' for RateLine ODOC-CTZ-KG-Client Rate CONSIGNOR1
	- Pickup Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignor Pickup Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Mel Zone' transport zone matched by Consignor Documentary Address fallback");
		}

		/// <summary>
		/// GIVEN
		/// - CTZ calculator having different zone items
		/// - Addresses are not overridden
		/// WHEN autorating shipment
		/// THEN the postcodes from the organisations in the shipment should match the zone items accordingly
		/// </summary>
		public void TestCTZCalculatorWithDeliveryAndConsigneeAddress_GivenAddressesAreNotOverridden_ShouldUseDeliveryPostCodeFallbackToConsigneePostCode()
			=> TestCTZCalculatorWithDeliveryAndConsigneeAddress(false);

		/// <summary>
		/// GIVEN
		/// - CTZ calculator having different zone items
		/// - Addresses are overridden
		/// WHEN autorating shipment
		/// THEN the postcodes from the organisations in the shipment should match the zone items accordingly
		/// </summary>
		public void TestCTZCalculatorWithDeliveryAndConsigneeAddress_GivenAddressesAreOverridden_ShouldUseDeliveryPostCodeFallbackToConsigneePostCode()
			=> TestCTZCalculatorWithDeliveryAndConsigneeAddress(true);

		void TestCTZCalculatorWithDeliveryAndConsigneeAddress(bool isAddressOverridden)
		{
			// with the below setup, DDOC charge will be created in job with FDA department. Let's set its dep. to ALL so the job doesn't have error.
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");

			var sydPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "2000"));
			var melPostCode = Factory.LoadTop1<RefPostCode>(new ZQuery(RefPostCodeSchema.RK_CityTownPostCode, "3000"));

			var zoneSet = Helper.CreateRateTransportZoneSet(Consignor, "AU", cityTown: null);
			var sydZone = zoneSet.CreateRateTransportZoneForTest("Syd Zone");
			sydZone.CreateRateTransportZoneItemForTest(sydPostCode);
			var melZone = zoneSet.CreateRateTransportZoneForTest("Mel Zone");
			melZone.CreateRateTransportZoneItemForTest(melPostCode);

			var clientRate = Helper.NewClientRate(Consignor);
			var dstEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.DST, Constants.RateMode.AIR, "AUSYD", "AUMEL");
			var dstLine = dstEntry.AddRateLine("DDOC", CartageZoneDistanceCalculator.Code, QuantityUnit.KG);
			var destinationCalculator = dstLine.GetCalculator<CartageZoneDistanceCalculator>();
			destinationCalculator.EquipmentType = Constants.EquipmentNeeded.Any;
			destinationCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 20, sydZone.PK);
			destinationCalculator.AddRateLineItemWithZone(Calculator.Items.Operator.UNT, 0, 30, melZone.PK);

			Factory.Save();

			var shipment = CreateForwardingShipment(Constants.TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "AUMEL", 100);

			var deliveryAddress = NewClient.MainAddress;
			deliveryAddress.OA_PostCode = "2000";
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = isAddressOverridden;

			var consigneeAddress = Consignee.MainAddress;
			consigneeAddress.OA_PostCode = "3000";
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consigneeAddress.PK;
			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = isAddressOverridden;

			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 2000 },
			};
			var message = "Whether the delivery address is overridden or not, it should be used to match transport zone set in CTZ calculator";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);
			AssertAutoratingAuditLogContains(shipment, @"Information: Matched 'Syd Zone' for RateLine DDOC-CTZ-KG-Client Rate CONSIGNOR1
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Syd Zone' transport zone matched by Consignee Pickup/Delivery Address fallback");

			// clear delivery address
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = ZGuid.Empty;
			shipment.ConsigneeDeliveryAddress.E2_AddressOverride = false;

			shipment.ConsigneeDocumentaryAddress.E2_AddressOverride = isAddressOverridden;

			expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 3000 },
			};
			message = "Given delivery address is empty, whether the consignee address is overridden or not, it should be used to match transport zone set in CTZ calculator";
			AutorateAndAssert(message, expectedCharges, shipment, Consignor, autorateCosts: false);
			AssertAutoratingAuditLogContains(shipment, @"Information: Matched 'Mel Zone' for RateLine DDOC-CTZ-KG-Client Rate CONSIGNOR1
	- Delivery Distance: empty
	- Distance Calculation Service: empty (registry disabled)
	- Lat/Long Postcode Distance: empty (Consignee Delivery Address to CTO/Wharf)
	- No transport zone could be matched by distance (0)
	- 'Mel Zone' transport zone matched by Consignee Documentary Address fallback");
		}

		#endregion
	}
}
