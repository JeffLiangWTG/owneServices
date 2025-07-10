using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Registry.Test
{
	[TestedType(typeof(LandTransportRegistry))]
	sealed class LandTransportRegistryTest : RegistryItemSetTestCaseWithFactory<LandTransportRegistry>
	{
		public void TestFailureReasons()
		{
			TestGenericRegistryItem
			(
				ItemSet.TransportFailureReasons,
				"TransportFailureReasons",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
				"Failure Reasons",
				"Reasons why Run Sheet Instruction could not be completed.",
				RegistryStorageFlags.System
			);

			var defaultList = (CodeDescriptionPairList)(ObjectFactory.Get<IMasterFilesListProvider>()).FailureReasons();
			AssertContainsExactElementsInAnyOrder("Ensure we get failure reasons from Master files.", defaultList, ItemSet.TransportFailureReasons.Value);
		}

		public void TestLandTransportConsignmentAdditionalReferenceNumbers()
		{
			var internals = ItemSet.LandTransportConsignmentAdditionalReferenceNumbers;

			TestGenericRegistryItem(internals,
			"ConsignmentAdditionalReferenceNumbers",
			RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
			"Consignment Additional Reference Types",
			"This list defines the additional reference number types available under the Additional References tab.",
			RegistryStorageFlags.System,
			RegistryOptions.Default);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"ETB|External Transport Booking Number|Y",
					"TRF|Transport Reference Number|Y",
					"CIN|Commercial Invoice Number|Y",
					"UCR|External (3rd Party) Unique Consignment Reference|N",
					"CLR|Customer Reference Number|Y",
					"ORD|Order Number|N",
					"HSB|House Bill|Y",
					"MAB|Master Bill|N",
					"BPR|Booking Party Reference|Y",
				},
				internals.DefaultValue
				.Cast<TransportReferenceNumberType>()
				.Where((number) => number.CodeInfo.ReadOnly && !((ICanDelete)number).CanDelete)
				.Select((number) => String.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
				.ToArray());
		}

		public void TestLandTransportRunSheetAdditionalReferenceNumbers()
		{
			var internals = ItemSet.LandTransportRunSheetAdditionalReferenceNumbers;

			TestGenericRegistryItem(internals,
				"RunSheetAdditionalReferenceNumbers",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
				"Run Sheet Additional Reference Types",
				"This list defines the additional reference number types available for Run Sheets.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"MAN|Manifest|Y",
					"SPA|Sub-contractor payment advice|Y",
					"TPT|Transport company reference|Y",
				},
				internals.DefaultValue
					.Cast<TransportReferenceNumberType>()
					.Select((number) => String.Format("{0}|{1}|{2}", number.Code, number.Description, number.IsUnique))
					.ToArray());

			Assert("All values should be deletable", internals.DefaultValue.Cast<TransportReferenceNumberType>().All((number) => ((ICanDelete)number).CanDelete));
		}

		public void TestTransportConsignmentNumberFormat()
		{
			AssertEquals(RegistryStorageFlags.System, (ItemSet.TransportConsignmentNumberFormat.Storage & RegistryStorageFlags.System));
			AssertEquals(RegistryStorageFlags.Company, (ItemSet.TransportConsignmentNumberFormat.Storage & RegistryStorageFlags.Company));

			var customisations = new BillOfLadingNumberCustomisationsByServiceLevel();

			customisations.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix = true;
			customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail = "4";

			ItemSet.TransportConsignmentNumberFormat.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisations);

			var value = ItemSet.TransportConsignmentNumberFormat.Value;

			AssertEquals("Value.RemoveSPrefix", true, value.BillOfLadingNumberCustomisations["ALL"].RemoveFountainPrefix);
			AssertEquals("Value.TrimNumberToLength", "4", customisations.BillOfLadingNumberCustomisations["ALL"].UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.SequenceNumber].Detail);
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Standard, (value.Categories & NumberCustomisationElementCategories.Standard));
			AssertEquals("Value.Cetegories", NumberCustomisationElementCategories.Domestic, (value.Categories & NumberCustomisationElementCategories.Domestic));
			AssertEquals("Value.AllowNonAlphanumericCharacters", true, value.AllowNonAlphanumericCharacters);
			AssertEquals("Value.EnableMacroInsertion", false, value.EnableMacroInsertion);
		}

		public void TestTransportConsignmentNumberFormatIsAddedToRegistry()
		{
			AssertVisible(ItemSet.TransportConsignmentNumberFormat);
		}

		public void TestTransportConsignmentNumberFormat_Default()
		{
			var defaultCustomisationByServiceLevel = ItemSet.TransportConsignmentNumberFormat.DefaultValue;
			var defaultCustomisation = defaultCustomisationByServiceLevel.BillOfLadingNumberCustomisations["ALL"];

			AssertEquals("UniversalOfficeCode: Include", true, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode].Include);
			AssertEquals("UniversalOfficeCode: Order", (ZByte)30, defaultCustomisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode].Order);
		}

		public void TestDefaultJobLoadingFixedDuration()
		{
			TestRegistryItem(ItemSet.DefaultJobLoadingFixedDuration,
				"DefaultJobLoadingFixedDuration",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Optimization,
				"Default Job Loading Fixed Duration",
				"The default fixed number of minutes taken to complete a job (loading or unloading) at a customer location.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				15);
		}

		public void TestLandTransportOptimizationUrl()
		{
			TestStringRegistryItem(ItemSet.LandTransportOptimizationUrl,
				"LandTransportOptimizationUrl",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Optimization,
				"Land Transport Optimization URL",
				"This URL is for Land Transport Optimization.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport,
				"https://rope.wisegrid.net/",
				CharacterCase.Normal);
		}

		public void TestLandTransportMobilityProofOfDeliveryRequired()
		{
			TestRegistryItem(ItemSet.LandTransportProofOfDeliveryRequired,
				"ProofOfDeliveryRequired",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
				"Proof of Delivery Required",
				@"This registry sets whether a driver using the Land Transport Mobility App is forced to capture a Proof of Delivery when completing a Delivery Instruction within the App.

The available options are:

None(default): POD capture is not required.
Signature: An electronic signature is required to complete the delivery.
Photo: A Photo of POD paperwork is required to complete the delivery.
Signature Or Photo: Either an electronic signature OR a Photo of the POD paperwork is required to complete the delivery.
Signature and Photo: Both an electronic signature AND a Photo of the POD paperwork is required to complete the delivery

Note: Drivers retain the option to capture non mandatory POD signatures and POD photos.",
				RegistryStorageFlags.System,
				new ProofOfDeliveryRequiredTypes(),
				ProofOfDeliveryRequiredTypes.Codes.None);
		}

		public void TestLandTransportMobilityProofOfPickupSignatureRequired()
		{
			TestRegistryItem(ItemSet.LandTransportProofOfPickupSignatureRequired,
				"ProofOfPickupSignatureRequired",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
				"Proof of Pickup Signature Required",
				"This registry sets whether a driver using the Land Transport Mobility App is forced to capture a Proof of Pickup when completing a Pickup Instruction within the App.",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				expectedDefaultValue: false);
		}

		public void TestLandTransportMobilityDocumentTypes()
		{
			TestGenericRegistryItem(ItemSet.MobilityDocumentTypes,
				"MobilityDocumentTypes",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
				"Mobility Document Types",
				@"This list defines the eDoc Document Types that are accessible via the Mobility App.

Only PDF files can be sent to the device. No other file types (such as Word or Excel) will be sent to the device.",
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}

		public void TestLandTransportMobilityDocumentTypes_Default()
		{
			var internals = ItemSet.MobilityDocumentTypes;

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"DOR",
				},
				internals.DefaultValue
				.Cast<MobilityDocumentType>()
				.Select((d) => string.Format("{0}", d.DocType.RT_DocType))
				.ToArray());
		}

		public void TestLandTransportMobility_ProofOfPickupDeclaration()
		{
			TestStringRegistryItem(ItemSet.LandTransportProofOfPickupDeclaration,
				"ProofOfPickupDeclaration",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
				"Proof of Pickup Declaration",
				@"This text will appear on the driver's device screen when customers sign the Proof of Pickup and will appear on the Proof of Pickup Images saved in e-Docs.

An entry in this registry can be used for obtaining the consent of the signer as recipient, if required for collecting and processing personal data under applicable privacy laws. To include a hyperlink as part of the declaration, add the clickable text link and URL using the following format:

<a href=""URL"">Text</a>

e.g. <a href=""MyPrivacyPolicy.com"">Privacy Policy</a>",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				TextEditorType.Memo,
				RegistryOptions.Default,
				(NoResString)string.Empty,
				CharacterCase.Normal);
		}

		public void TestLandTransportMobility_ProofOfDeliveryDeclaration()
		{
			TestStringRegistryItem(ItemSet.LandTransportProofOfDeliveryDeclaration,
				"ProofOfDeliveryDeclaration",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
				"Proof of Delivery Declaration",
				@"This text will appear on the driver's device screen when customers sign the Proof of Delivery and will appear on the Proof of Delivery Images saved in e-Docs.

An entry in this registry can be used for obtaining the consent of the signer as recipient, if required for collecting and processing personal data under applicable privacy laws. To include a hyperlink as part of the declaration, add the clickable text link and URL using the following format:

<a href=""URL"">Text</a>

e.g. <a href=""MyPrivacyPolicy.com"">Privacy Policy</a>",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				TextEditorType.Memo,
				RegistryOptions.Default,
				(NoResString)string.Empty,
				CharacterCase.Normal);
		}

		public void TestProofOfDeliveryPickupStorageLocation()
		{
			TestRegistryItem
			(
				ItemSet.ProofOfDeliveryPickupStorageLocation,
				"ProofOfDeliveryPickupStorageLocation",
				"Transport/Land & Port Transport/Land Transport/Mobility",
				"Proof of Delivery/Pickup Storage Location",
				"Proof of Delivery and Proof of Pickup confirmations for single customer instructions will be stored as eDocs on the entity selected here. Changing this setting will not move existing documents.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				new ProofOfDeliveryPickupDestinations(),
				"LTC"
			);
		}

		public void TestDisableDtbConsignmentActionPickedUpDeliveredLogSubscriber()
		{
			TestRegistryItem(ItemSet.DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber, "DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber", "Transport/Land & Port Transport/Land Transport", "Disable Consignment Action Picked Up/Delivered Log Subscriber", "When disabled, service task will not process consignment action's picked up or delivered log events by Log Subscriber.", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, false);
		}

		public void TestShowDetailedErrorMessages()
		{
			TestRegistryItem
			(
				ItemSet.ShowDetailedErrorMessages,
				"ShowDetailedErrorMessages",
				"Transport/Land & Port Transport/Land Transport",
				"Show Detailed Error Messages",
				"When enabled, it provides additional details for the error messages, including steps to take.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForCargoWise,
				false
			);
		}

		public void TestEnableLandTransportBetaFeatures()
		{
			TestRegistryItem(
				ItemSet.EnableLandTransportBetaFeatures,
				"EnableLandTransportBetaFeatures",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
				"Enable Beta Features Menu",
				"Enable the Land Transport beta features menu in the LTP portal.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				expectedDefaultValue: false
			);
		}

		public void TestRunSheetMaxTimeInterval()
		{
			TestRegistryItem(ItemSet.RunSheetMaxTimeInterval,
				"RunSheetMaxTimeInterval",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Optimization,
				"Run Sheet Max Time Interval",
				"This sets the maximum end date-time when calculating the array of opening and closing time for the optimizer, calculated by adding registry hours onto the run sheet’s planned start time to calculate the address’s maximum end date. When optimizing a Run sheet, the optimizer will evaluate an address’s opening hours as needing to be completed between the run sheet planned start date-time and this maximum end date-time.",
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				RegistryOptions.Default,
				168,
				1,
				999
			   );
		}

		public void TestLandTransportConsignmentMandatoryFields()
		{
			var item = ItemSet.LandTransportConsignmentMandatoryFields;
			AssertEquals("Name", "LandTransportConsignmentMandatoryFields", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport, item.Category);
			AssertEquals("Caption", "Consignment Mandatory Fields", item.Caption);
			AssertEquals("Hint", "Specifies mandatory fields in Consignment.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System | RegistryStorageFlags.Company, item.Storage);

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("LandTransportConsignmentMandatoryFields", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.LandTransportConsignmentMandatoryFields;
				AssertEquals("Precondition: EnableLandTransport is set to false", false, TransportRegistry.Instance.EnableLandTransport.Value);
				AssertEquals("Option is hidden by default when EnableLandTransport is set to false", RegistryOptions.IsHidden, item.Options);
			}

			using (TransportRegistry.Instance.EnableLandTransport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ItemSet.RemoveItemFromCacheIfOlderThan("LandTransportConsignmentMandatoryFields", TimeSpan.FromMilliseconds(-1));

				item = ItemSet.LandTransportConsignmentMandatoryFields;
				AssertEquals("Precondition: EnableLandTransport is set to true", true, TransportRegistry.Instance.EnableLandTransport.Value);
				AssertEquals("Option is set by default when EnableLandTransport is set to true", RegistryOptions.Default, item.Options);
			}

			var defaultValue = item.DefaultValue;

			AssertEquals("DefaultValue.Count", 6, defaultValue.Count);
			AssertEquals("DefaultValue Allow New", false, defaultValue.AllowNew);

			CombineAssertions(() =>
			{
				AssertCodeDescriptionBool(defaultValue[0], "ClientRequestedBillingParty", "Bill to Party", false);
				AssertCodeDescriptionBool(defaultValue[1], "BookingParty", "Booking Party", false);
				AssertCodeDescriptionBool(defaultValue[2], "LTC_GB_Branch", "Controlling Branch", false);
				AssertCodeDescriptionBool(defaultValue[3], "LTC_RS_NKServiceLevel", "Service Level", false);
				AssertCodeDescriptionBool(defaultValue[4], "LTC_ConnoteNumber", "Consignment Note", false);
				AssertCodeDescriptionBool(defaultValue[5], "LTC_Incoterm", "Incoterm", false);
			});

			void AssertCodeDescriptionBool(CodeDescriptionBool value, ZString code, ZString description, bool boolValue)
			{
				AssertEquals($"{code} Code", code, value.Code);
				AssertEquals($"{code} Description", description, value.Description);
				AssertEquals($"{code} Bool", boolValue, value.Bool);
			}
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return nameof(ItemSet.DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber);
				yield return nameof(ItemSet.LandTransportConsignmentMandatoryFields);
			}
		}
	}
}
