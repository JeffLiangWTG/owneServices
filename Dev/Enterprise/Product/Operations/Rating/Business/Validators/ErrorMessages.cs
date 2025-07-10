using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public static class ErrorMessages
	{
		#region Date Validation

		public static string ExpiryBeforeStartDate
		{
			get { return Res.GetString("cdaacf94-9185-4c42-b89e-156ff6403772", "Expiry date cannot be before the start date."); }
		}
		public static string StartDateInPast
		{
			get { return Res.GetString("61428fe5-5132-4a35-95f0-7688d5aae2b9", "You have entered a start date which is prior to today. Please confirm this is correct."); }
		}
		public static string StartDateAfterExpiryDate
		{
			get { return Res.GetString("f2b61046-bca4-462a-ae7b-5d649842ba52", "You cannot have a start date that is after the expiry date."); }
		}
		public static string OverlappingDatesOnRateEntry
		{
			get { return Res.GetString("5aba96e5-1b73-44f6-837f-35732a3988a4", "You have entered rates with an overlapping dates. Please enter rates without overlapping dates so the system can autorate using the correct rate."); }
		}
		public static string NewEndDateError
		{
			get { return Res.GetString("02eb55d6-0c0e-497d-b897-6815122a3cb6", "Rate End Date must be later than today."); }
		}

		public static string FollowUpDateOutsideQuotationPeriod => Res.GetString("d9ea1883-25ee-4f58-8549-ca9eaf2d9c93", "The Follow Up Date is not within the Quotation Period.");

		#endregion

		#region Quotation Documents

		public static string RateAttachmentSequenceDuplicateError
		{
			get { return Res.GetString("ee9a00c1-cdaa-4d34-8d1b-c06a18134a78", "A Quotation Document with this sequence number already exists. Please specify another sequence number."); }
		}
		public static string IsCoverSheetSequenceError1stPart
		{
			get { return Res.GetString("b8d05e09-b845-4a79-8530-4e2843e8c741", "The sequence for a cover sheet must be less than") + " "; }
		}
		public static string IsCoverSheetSequenceError2ndPart
		{
			get { return " " + Res.GetString("c78fa60e-ed29-41a8-b969-f37a9d48ae3d", "because a cover sheet must have a sequence less than a quote template."); }
		}
		public static string IsQuoteTemplateSequenceError1stPart
		{
			get { return Res.GetString("b3c91aa1-1a46-4401-affa-14cae773a8d9", "The sequence for a quote template must be between") + " "; }
		}
		public static string And
		{
			get { return " " + Res.GetString("458d3091-3419-484c-8e80-7e74d7e352d8", "and") + " "; }
		}
		public static string IsTrailingPageSequenceError1stPart
		{
			get { return Res.GetString("2135e61b-6370-43e6-8b6f-89e09cb5ce15", "The sequence for a trailing page must be greater than") + " "; }
		}
		public static string IsTrailingPageSequenceError2stPart
		{
			get { return " " + Res.GetString("d344da03-eacc-4f63-99f4-63156079bbbd", "because a trailing page must have a sequence greater than a quote template"); }
		}
		public static string RateAttachmentTypeNotSelected
		{
			get { return Res.GetString("478ab693-a478-4f3d-8047-af90e4dc29a1", "Please select a Quotation Document type before entering a sequence."); }
		}
		public static string InvalidSequenceNumber
		{
			get { return Res.GetString("70fed770-035f-468f-a874-6dbc2949e16c", "The sequence number must be higher than 0."); }
		}

		#endregion

		#region Calculator Validation

		#region Weight Breaks

		public static string PacksWeightUnitFactorRequiresWeightUnit
			=> Res.GetString("76DD1BBC-0FBC-433C-B79C-144DED36214F", "Packs weight requires weight unit.");

		public static string PacksWeightUnitFactorOnlySupportCMBCalculator
			=> Res.GetString("C7A31F69-6DE5-42BB-9348-D5D4F47384A0", "Packs weight only supports CMB calculator.");

		public static string CalculatorIsNotSupportedForProductLine
			=> Res.GetString("F895FC03-9AF4-40C3-B843-30B620A72D12", "Calculator is not supported for product line.");

		public static string PacksWeightUnitFactorOnlySupportNonCumulativeBreak
			=> Res.GetString("50729C8E-F21B-437D-AD32-7B504C2C9E8D", "Packs weight only supports non cumulative break.");

		public static string PackageLineUnitFactorSupportedCalculator
			=> Res.GetString("3F259829-9D0C-455F-A20B-EFAFD080886B", "Package line only supports FLT, UNT, CMB, CTG and CTZ calculators.");

		public static string LoadedPackagesOnlyFactorSupportedCalculator
			=> Res.GetString("1bda772b-f271-4d66-8a71-8a3b9ce99750", "Loaded Packages Only Unit Factor only supports WPK Calculator and PK Units.");

		public static string PacksWeightNonConvertiblePackType(string packType)
			=> Res.GetString("6A7FD9A1-5660-4320-B1D6-2C1BD34E6A28", "Cannot convert pack type '{0}' for packs weight break search.", packType);

		public static string WeightBreakRequired
		{
			get { return Res.GetString("d94e7201-4b7d-4a1c-a897-fa8e9b95e2f4", "A weight break is required if the Rate operator is '-' or '+'."); }
		}
		public static string WeightBreakNotAllowed
		{
			get { return Res.GetString("e88ca552-7e65-47ce-92ff-6651b55e7d4c", "A weight break is only allowed if the Rate operator is '-' or '+'."); }
		}
		public static string WeightBreakIsTheSameAsAnotherPlusRate
		{
			get { return Res.GetString("43a7548b-4039-46b9-9398-5c9c40be4e59", "This weight break is the same as another '+' Rate. Please enter each '+' weight break only once."); }
		}
		public static string WeightBreakLessThanMinusBreak
		{
			get { return Res.GetString("c63fd3a5-f074-4640-b725-fa3aeafca818", "This weight break is less than the '-' break. Please enter a weight break greater than the '-' break, or change the '-' break."); }
		}
		public static string MinChargeableAmountIsNotAllowedIfMinRateSpecified
		{
			get { return Res.GetString("e6279f93-8f66-422a-9d30-3489b3053b51", "Minimum chargeable is not allowed when minimum rate is specified. Either min chargeable or min rate can be specified."); }
		}
		public static string LowestBreakActsAsPlusAndMinus(string signForWarningMessage)
		{
			return Res.GetString("d4dbf602-51d8-47f9-b1ba-e6ddc7b6725d", "This weight break will cover all values between zero and the next '+' weight break. If this is not intentional please enter equal '{0}' weight break", signForWarningMessage);
		}

		public static string CalculationHasDuplicateBreaksWithDifferentValues(decimal duplicateBreak)
			=> Res.GetString("BD390C4D-E4E8-48C9-A64C-6DEF49B07F56", "calculator has duplicate breaks of {0} with different values", duplicateBreak);

		#endregion

		#region Rate Operator

		public static string BASNotAllowed
		{
			get { return Res.GetString("d24a10d3-6215-485b-a7ce-e05ef4418294", "A {0} ('BAS') cannot be entered on this calculator.", RatingDataRegistry.Instance.BaseRateText.Value); }
		}
		public static string UNTNotAllowed
		{
			get { return Res.GetString("65fb24c1-7df8-4ed4-af63-ccabf9c5f579", "A Per Unit rate ('UNT') cannot be entered on this calculator."); }
		}
		public static string MAXNotAllowed
		{
			get { return Res.GetString("4b1746a5-ec89-45e4-90be-c7ae487a0e3d", "A Maximum rate ('MAX') cannot be entered on this calculator."); }
		}
		public static string DuplicateMINNotAllowed
		{
			get { return Res.GetString("628ab86f-b34a-43c4-9dc0-2b244be90ffc", "More than 1 Minimum ('MIN') rate is not allowed."); }
		}
		public static string DuplicateMINorMAXorBASorUNTorMinusNotAllowed
		{
			get { return Res.GetString("BB6D0203-EDB8-451E-8BAA-18AADD90C27A", "More than 1 Per Unit, Base, Maximum, Minimum or Minus rate is not allowed."); }
		}
		public static string UNTIncompatibleWithSlidingItems
		{
			get { return Res.GetString("440D5249-C85C-444F-9A1A-556797092E06", "Rates for ‘UNT’ are incompatible with ‘-’ or ‘+’ rates. Either specify a per unit rate, or specify a rate for each individual weight break."); }
		}
		public static string MoreLinesRequired
		{
			get { return Res.GetString("29c55c2a-cc64-4cc3-919c-8415d85c09e7", "You must enter at least one '+' line to signify the last weight break."); }
		}
		public static string InvalidRateOperator
		{
			get { return Res.GetString("5779f164-4a5a-4caa-a525-53ba08cf756b", "You have entered an invalid code."); }
		}
		public static string MoreThanTwoUNTNotAllowed
		{
			get { return Res.GetString("f0cdd02f-9033-43de-b665-c269158a2e73", "More than 2 Per Unit ('UNT') rates are not allowed on this calculator."); }
		}
		public static string SameUNTNotAllowed
		{
			get { return Res.GetString("5dd0ba8d-b363-4b1c-a9b5-21331766e162", "This calculator requires only a single weight and single volume Per Unit ('UNT') rate. You cannot specify more Per Unit rates."); }
		}

		#endregion

		#region Conversion Factors

		public static string ConversionFactorMustBeEntered
		{
			get { return Res.GetString("64b27bdd-f63d-4c50-b07c-0090593ca635", "You must enter a Volumetric Conversion Factor for freight."); }
		}

		#endregion

		#region Equipment Types

		public static string EquipmentTypeDuplicate
		{
			get { return Res.GetString("74f9829c-9a38-4a0f-8d08-46d88d64565c", "You have already entered this charge with this equipment type. Please choose another equipment type or charge code."); }
		}

		#endregion

		public static string CostBasedCalculatorNotAllowed
		{
			get { return Res.GetString("bb71b5ce-eec9-4baf-9d86-780fb874764e", "The Cost Based Calculator cannot be used inside a Standard Costing. It should only be used to signify a sell rate based on a Buy rate, or a specific Cost based on a Generic Cost."); }
		}

		public static string CostBasedCalculatorWarning
		{
			get { return Res.GetString("34d4e29c-be2c-49b3-baf8-a6d0ead7fbb6", "Using the Cost Based Calculator within a Costing should only be used to specify a cost based on the Standard (i.e. TACT etc) costings in the system. If no standard costing is found, errors will be shown during AutoRating."); }
		}

		public static string CompanyTariffBasedCalculatorNotAllowed
		{
			get { return Res.GetString("8ad87c8e-fe4e-444d-ac39-09566a4a71db", "The Company Tariff Based Calculator cannot be used inside a Costing or Company Tariff Level 1. It should only be used within a Client Rate or Quotation to base a sell rate on your Company Tariff."); }
		}

		public static string CompanyTariffBasedCalculatorAppliesToBaseCompanyTariff
		{
			get { return Res.GetString("efe15665-ba2a-4544-b312-8d735c8d8e7c", "Please note that the Company Tariff Based Calculator applies to the Base Company Tariff regardless if the Organization is linked to a Company Tariff Level other than 1."); }
		}

		public static string EqualisationCalculatorNotAllowed
		{
			get { return Res.GetString("eb37f06d-9115-4593-9047-9087244f074b", "The Volume Equalization Discount Calculator can only be used within a Costing for Air, Origin or Destination charges"); }
		}

		public static string ExcludeCompanyTariffsCalculatorNotAllowed
		{
			get { return Res.GetString("4d4ce1a3-f02a-4738-b204-623a3ff6f804", "The Exclude from Company Tariffs Calculator cannot be used here. It should only be used within a Client Rate."); }
		}

		public static string CalculatorsSupportedOnFCLRateEntryWithEmptyContainer
		{
			get { return Res.GetString("9a393054-d80a-4074-b322-513d91c65fcc", "FCL Rates entry with blank Container/ULD can only use FLT or PER type of calculators."); }
		}

		public static string FeesAndChargesChargeCodeDoesNotBelongToOrganization
		{
			get { return Res.GetString("0838e097-07e2-4e73-b8ba-c48bcd8d3cbd", "Fees and charges charge code does not belong to client organization."); }
		}

		public static string CartageCalculatorNotAllowedOnSeaOrAll
		{
			get { return Res.GetString("4116d596-2fc0-4b23-9d3d-fd845427bc24", "The Transport Calculator can not be used to specify rates for general transport mode (i.e. SEA, ALL). Please choose a more specific Transport Mode."); }
		}

		public static string AgencyChargeOnFreightEntry
		{
			get { return Res.GetString("212cea5c-914f-424b-9705-2328886b5f41", "You cannot enter an agency charge on a Freight charge. Agency charges should be entered as Origin or Destination charges."); }
		}

		public static string NoteCalculatorOnCosting
		{
			get { return Res.GetString("7916b4b7-bd9f-46d8-82a5-279d4c4ceb04", "The Note Calculator cannot be used on a Costing."); }
		}

		public static string ProfitShareRebateCalculatorNotAllowedOnCosting
		{
			get { return Res.GetString("6513bf45-8766-44bd-bbca-706ccf3fc8aa", "The Profit Share / Rebate Calculator cannot be used on a Costing."); }
		}

		public static string FRTCalculatorOnFreightCharge
		{
			get { return Res.GetString("31fa700f-54f1-4231-bec4-57509a694b58", "The Freight Inclusive Calculator cannot be used to calculate the Freight charge code defined Maintain > System > Registry > AutoRating > Charge Codes > Freight > Freight Charge Code"); }
		}

		public static string FRTCalculatorRecursiveReference
		{
			get { return Res.GetString("4f3b2c3e-1ebf-4322-823a-2556431a49c6", "Recursive setup for Freight Inclusive Calculator is not allowed."); }
		}

		public static string CartageZoneChargeOnFreightEntry
		{
			get { return Res.GetString("01f10e7e-9fe2-4042-9368-4f56f48bc3da", "You cannot enter a Port Transport charge based on zones on a freight entry. Port Transport charges should be Origin or Destination charges."); }
		}

		public static string NonCartageCalcOnCartageCharge
		{
			get { return Res.GetString("5a8c653a-ab28-4b61-a34c-335cf9348758", "You should generally use the Transport calculator on Transport charge codes, as this allows you to specify conversion factors and equipment."); }
		}

		public static string WarehouseLocationTypeCalcOnNonWarehouseStorageCharge
		{
			get { return Res.GetString("b5cc4ec9-4a26-4a21-babf-d52c1aeea5ba", "You can only use the Warehouse Location Type calculator on charges that are in the Warehouse Storage charge code group."); }
		}

		public static string ChargeCodeRequiredForPercentageCalculator
		{
			get { return Res.GetString("e3a7e7a8-dc84-425b-ba47-83e401a4d742", "You must select a charge code for the percentage calculator."); }
		}

		public static string LocalChargeCodeIsRequired
		{
			get { return Res.GetString("02600532-4ee3-40fe-ac2b-237f1e05e197", "This charge code cannot be used as a Local Charge Code is required."); }
		}

		public static string GlobalChargeCodeIsRequired
		{
			get { return Res.GetString("14d1e61b-eaaa-47f1-8d1c-d44d422a1820", "This charge code cannot be used as a Global Charge Code is required."); }
		}

		public static string SameChargeCodes
		{
			get { return Res.GetString("89e4c1df-c295-41df-9452-90daab114f55", "The main charge code and percent charge code cannot be the same."); }
		}

		public static string DoubledChargeCodes
		{
			get { return Res.GetString("a9390469-9a2a-412f-9d9f-7fe6cff9e9a5", "The same charge code should not be included more than once."); }
		}

		public static string DoubledChargeTypes
		{
			get { return Res.GetString("fdfbcbd7-3ae5-4319-8ad9-ed8f7d80e2a5", "The same charge type should not be included more than once."); }
		}

		public static string AtLeastOneChargeType
		{
			get { return Res.GetString("9e5dd99c-4770-4276-a5c1-69410efc7a1e", "Please specify at least one charge code or charge type for this percentage calculation to be based on."); }
		}

		public static MultilingualString InvalidGlobalChargeCode
		{
			get { return ResString.GetMultilingualString("a5ca7522-3709-44cd-a255-e155b2f69c2d", "Enter a valid Global Charge Code."); }
		}

		public static MultilingualString InvalidChargeCode
		{
			get { return ResString.GetMultilingualString("1369b0f2-c7e6-4e51-aac1-5eba12805672", "Enter a valid Charge Code."); }
		}

		public static MultilingualString InapplicableProductNumber
		{
			get { return ResString.GetMultilingualString("b308bcac-cfb7-4ea5-bcfa-4e6d0679f0b5", "Enter a valid selection"); }
		}

		public static string MinimumCostMarkupNotMetPart1
		{
			get { return Res.GetString("d68cd660-44ce-41e5-b5bc-7f1bb0bf0a2d", "The minimum cost markup"); }
		}

		public static string MinimumCostMarkupNotMetPart2
		{
			get { return Res.GetString("3d60c017-feee-4603-a064-b59ecaff0cf9", "has not been met."); }
		}

		public static string GetMinimumCostMarkupNotMet(ZDecimal minimumMarkupPercentage)
		{
			return Res.GetString("01c28065-918b-46c4-8321-cadf200a5940", "{0} of {1}% {2}", MinimumCostMarkupNotMetPart1, minimumMarkupPercentage, MinimumCostMarkupNotMetPart2);
		}

		public static string MinimumCostMarkupNotMet2
		{
			get { return Res.GetString("962d2827-e342-4694-a98d-fb780a9a35d5", "{0} {1}", MinimumCostMarkupNotMetPart1, MinimumCostMarkupNotMetPart2); }
		}

		public static string CalculationOrderShouldBeSetWhenPercentageAndFixedChangeAreSet
		{
			get { return Res.GetString("A005ECB3-B6D4-4479-BC0D-5E7F2FF46A95", "Calculation Order should be set when both Percentage and Fixed Change are set."); }
		}

		public static string MessageTypeOrSubTypeDuplicate
		{
			get { return Res.GetString("e3c254c6-3c36-47fa-b2d6-f3887f3077c4", "You have already entered this charge with this message type or style. Please choose another message type, style or charge code."); }
		}

		#endregion

		#region Overlapping Rate Entries Found

		public static string OverlappingRatesCreatedByBulkRateUpdater => Res.GetString("7BEC3973-442C-4747-9B7D-40479B970515", @"Cannot save as there are duplicates / overlapping rates. The cause of this error may be due to selecting the ""Apply Changes for Specified Dates Only"" checkbox. Please verify.
When the ""Apply Changes for Specified Dates Only"" checkbox is selected, if an existing Rate falls partially outside of this date range then a new Rate is created and the start/end date of the existing Rate is changed to avoid overlapping.
However this may inadvertently create overlaps with other pre-existing rates. To avoid this, please that your existing rates would not conflict with the ""Apply Changes for Specified Dates Only"" options.");

		public static string OverlappingRatesCreatedByConcurrency => Res.GetString("2f5925f3-5575-4327-a655-60388da44b71", @"Cannot save as there are duplicates / overlapping Rate Entries. Another user has made changes to the Rate Entries that are preventing your changes from being saved.
Duplicate might be hidden due to current filters set up.
Please close and reopen this form in order to continue. Your changes may be lost.");

		public static string OverlappingRatesButPassesInDatabase => Res.GetString("f360bd85-46fa-4a6a-a9da-e19b9ec8117e", @"Cannot save as there are duplicates / overlapping Rate Entries.
Duplicate might be hidden due to current filters set up.
Rating cannot calculate the most applicable rate when there are two identical Rate Entries with overlapping start and end dates. To avoid this scenario, please avoid creating duplicate rates with overlapping date ranges.");

		#endregion

		#region Other Validation

		public static string ViaError
		{
			get { return Res.GetString("8599788a-9f66-4fa8-a83d-a0f533ada647", "Via port cannot be the same as the origin or destination ports."); }
		}

		public static string RatePriceIsZero
		{
			get { return Res.GetString("70594ea7-e772-43ca-bd5a-70faadc1e00d", "You have entered a price of $0.00. Please ensure this is correct."); }
		}
		public static string PercentIsZero
		{
			get { return Res.GetString("c24f014e-383d-42e2-81c2-368de4ead994", "You have entered a Percent of 0. Please ensure this is correct."); }
		}
		public static string FrequencyGreaterThanZero
		{
			get { return Res.GetString("207876ce-a01d-4d35-83fc-e4c27de899bb", "Frequency cannot be less than 0."); }
		}
		public static string NoFrequency
		{
			get { return Res.GetString("8699b261-561e-4018-8876-3a14ed634e80", "Since you have specified the frequency unit, you need to also specify the frequency."); }
		}
		public static string InvalidFrequencyUnit
		{
			get { return Res.GetString("b1825448-0274-4b4e-b3ee-90e50ea93a5c", "Enter a valid frequency unit."); }
		}
		public static string NoFrequencyUnit
		{
			get { return Res.GetString("e07bb67d-af0a-4a7c-a0c6-3ddb09f0cbcb", "Please specify what frequency unit your frequency represents."); }
		}
		public static string InvalidTransitTime
		{
			get { return Res.GetString("af3dee4e-8e58-47a9-ab9b-55357332dbcc", "Please enter a valid transit time from the available list."); }
		}

		public static string EntryHasNoLines
		{
			get { return Res.GetString("d9a6e222-730b-4e48-87bf-087c33cc2e7c", "This rate entry has been saved without rate lines. Please save a rate line against this rate entry or delete it."); }
		}

		public static string RateForThisClientAlreadyExists
		{
			get { return Res.GetString("242f390a-1b93-47b6-b1e9-2941e4013b60", "This client already has a Client Rate created in the system. Each client can only have a single Client Rate. Please modify the existing Client Rate."); }
		}

		public static string CostForThisSupplierAlreadyExists
		{
			get { return Res.GetString("e6c89e4c-51c8-41fa-ad19-33f03927505d", "This supplier already has a Cost created in the system. Each supplier can only have a single Costing. Please modify the existing Costing."); }
		}

		public static string GlobalRateForThisClientAlreadyExists
		{
			get { return Res.GetString("d160b4f7-ff10-4639-99c4-12aae5245621", "This client already has a Global Client Rate created in the system. Each client can only have a single Global Client Rate. Please modify the existing Global Client Rate."); }
		}

		public static string GlobalCostForThisSupplierAlreadyExists
		{
			get { return Res.GetString("d4e6426d-c6db-4a85-ae41-5294d9d97a5f", "This supplier already has a Global Cost created in the system. Each supplier can only have a single Global Costing. Please modify the existing Global Costing."); }
		}

		public static string StandardCostAlreadyExists
		{
			get { return Res.GetString("97b4964a-7a49-409e-a8f3-f4501bd2c53c", "You can only have a single Standard Costing published Locally in this Company. All other costings must have a Service Provider specified."); }
		}

		public static string StandardGlobalCostAlreadyExists
		{
			get { return Res.GetString("fd41a68d-7f14-448c-b4b4-ed1bb4e06934", "You can only have a single Standard Costing published Globally. All other costings must have a Service Provider specified."); }
		}

		public static ResourceString InvalidClientRateHeader
		{
			get { return ResString.GetMultilingualString("e6d3c3c3-20e5-4602-9b78-d9d9d6492177", "The client must be flagged as either: Consignor, Consignee, Transport Client, Warehouse or Sales."); }
		}

		public static ResourceString InvalidOverseasAgentRateHeader
		{
			get { return ResString.GetMultilingualString("4fe81f65-2544-44e3-8f12-f2ecac17d1ac", "The client must be flagged as either: Forwarder/Agent or Controlling Agent."); }
		}

		public static ResourceString InvalidCostingHeader
		{
			get { return ResString.GetMultilingualString("5b7dfb30-63ef-4e57-b4ae-c22e18d95df9", "The service provider must be creditor."); }
		}

		public static string FirstSignatoryIsInactive
		{
			get { return Res.GetString("f0d34f7e-d29d-11e4-845f-902b34dc814a", "The First Signatory defaults from the Sales Representative for the nominated Local Client. Delete or Replace the inactive record from Organizations>Details>Staff Assignments>Role>Sales Representative"); }
		}

		public static string InvalidFreightChargeCodeErrorMessage
		{
			get { return Res.GetString("549350a7-fb3c-4e8c-b61c-de0e8cb0961d", "Auto-rating could not find the 'Freight Charge Code' for this company. Either set a valid 'Freight Charge Code' in the Registry or revert the Freight Autorating Mode on this Job to Standard."); }
		}

		public static string CompanyTariffMustNotHaveOrganisation
		{
			get { return ResString.GetMultilingualString("1cd7f39d-b966-4324-adb8-882a6340749b", "Company Tariff must not have organization."); }
		}

		public static string InvalidPaymentTerm
		{
			get { return Res.GetString("bbc9e320-8bfe-4d12-b2dc-c70bd11397b9", "Please enter a valid payment term from the available list."); }
		}

		#endregion

		#region Intercompany Tariff

		public static ResourceString OrgProxyIsMandatoryForIntercompanyTariff =>
			ResString.GetMultilingualString("F54671BF-D2D9-454B-8F23-448DC98ED733", "The Service Provider must be an Organization Proxy.");

		public static ResourceString IntercompanyTariffForThisSupplierAlreadyExists =>
			ResString.GetMultilingualString("548DC9FF-807C-4DCF-8F39-983CEF833AB0", "This service provider already has an Intercompany Tariff created in the system. Each service provider can only have a single Intercompany Tariff. Please modify the existing Intercompany Tariff.");

		public static ResourceString IntercompanyTariffMustBeGlobal =>
			ResString.GetMultilingualString("041A208D-D055-464E-A4DB-A8141443A721", "global company for an intercompany tariff");

		public static ResourceString IntercompanyTariffRatesBulkUpdateCheck =>
			ResString.GetMultilingualString("ee807940-2e9a-4e17-9f69-71f46355003e", "Rates of Intercompany Tariffs cannot be updated in bulk together with rates of other Tariffs & Rates modules as Global Charge Codes are used for Intercompany Tariffs.");

		public static ResourceString ObsoleteGatewayAgentTypesErrorMessage =>
			ResString.GetMultilingualString("0fe5f84d-adbb-4240-b022-c1121ce788ca", "Enter a valid Gateway Agent Type. FSG and SSG are obsoleted");

		public static ResourceString OnlyApplicableForIntercompanyTariffErrorMessage =>
			ResString.GetMultilingualString("23B271DC-3EA4-412D-8A21-923AC1E516AD", "Gateway Service Level is only applicable for Intercompany Tariffs.");

		#endregion
	}
}
