using System;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Registry.Business.MobilityDocumentTypes;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Integration;
using Enterprise.TransportConsignment.Registry.Lists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Registry
{
	public sealed class LandTransportRegistry : RegistryItemSet, ILandTransportRegistry
	{
		public TransportReferenceNumberTypesRegistryItem LandTransportConsignmentAdditionalReferenceNumbers
		{
			get
			{
				return GetItem("ConsignmentAdditionalReferenceNumbers", () =>
				{
					var defaultValue = new TransportReferenceNumberTypeCollection();
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.TransportReference, TransportCommonAdditionalReferenceTypes.Descriptions.TransportReference);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.CommercialInvoiceNumber, TransportCommonAdditionalReferenceTypes.Descriptions.CommercialInvoiceNumber);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.ExternalUniqueConsignmentReference, TransportCommonAdditionalReferenceTypes.Descriptions.ExternalUniqueConsignmentReference, isUnique: false);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.ClientReferenceNumber, TransportCommonAdditionalReferenceTypes.Descriptions.ClientReferenceNumber);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.OrderNumber, TransportCommonAdditionalReferenceTypes.Descriptions.OrderNumber, isUnique: false);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.HouseBill, TransportCommonAdditionalReferenceTypes.Descriptions.HouseBill);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.MasterBill, TransportCommonAdditionalReferenceTypes.Descriptions.MasterBill, isUnique: false);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.BookingPartyReference, TransportCommonAdditionalReferenceTypes.Descriptions.BookingPartyReference);
					defaultValue.AddSystemDefined(TransportCommonAdditionalReferenceTypes.Codes.ExternalTransportBookingNumber, TransportCommonAdditionalReferenceTypes.Descriptions.ExternalTransportBookingNumber);

					return new TransportReferenceNumberTypesRegistryItem(
						"ConsignmentAdditionalReferenceNumbers",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
						ResString.GetMultilingualString("21e1516e-1f4f-4916-a2d8-5671da6565e9", "Consignment Additional Reference Types"),
						ResString.GetMultilingualString("45aadcc2-e484-4cff-bf23-2ccdb0bc97fc", "This list defines the additional reference number types available under the Additional References tab."),
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		public TransportReferenceNumberTypesRegistryItem LandTransportRunSheetAdditionalReferenceNumbers
		{
			get
			{
				return GetItem("RunSheetAdditionalReferenceNumbers", () =>
				{
					var defaultValue = new TransportReferenceNumberTypeCollection();
					defaultValue.Add(RunSheetAdditionalReferenceTypes.Codes.Manifest, RunSheetAdditionalReferenceTypes.Descriptions.Manifest);
					defaultValue.Add(RunSheetAdditionalReferenceTypes.Codes.SubContractorPaymentAdvice, RunSheetAdditionalReferenceTypes.Descriptions.SubContractorPaymentAdvice);
					defaultValue.Add(RunSheetAdditionalReferenceTypes.Codes.TransportCompanyReference, RunSheetAdditionalReferenceTypes.Descriptions.TransportCompanyReference);

					return new TransportReferenceNumberTypesRegistryItem(
						"RunSheetAdditionalReferenceNumbers",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
						ResString.GetMultilingualString("2941f3e3-45f1-4caf-8347-19ba6c42dbb1", "Run Sheet Additional Reference Types"),
						ResString.GetMultilingualString("a3e2063a-0f21-4bac-b430-87e105aca8de", "This list defines the additional reference number types available for Run Sheets."),
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		public CodePairRegistryItem LandTransportProofOfDeliveryRequired
		{
			get
			{
				return GetItem("ProofOfDeliveryRequired", () => new CodePairRegistryItem(
						"ProofOfDeliveryRequired",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
						ResString.GetMultilingualString("553427eb-225b-4f1b-b86c-4ca28e07bf7a", "Proof of Delivery Required"),
						ResString.GetMultilingualString("baf410d0-929c-48df-a84b-9d6df0a14394", @"This registry sets whether a driver using the Land Transport Mobility App is forced to capture a Proof of Delivery when completing a Delivery Instruction within the App.

The available options are:

None(default): POD capture is not required.
Signature: An electronic signature is required to complete the delivery.
Photo: A Photo of POD paperwork is required to complete the delivery.
Signature Or Photo: Either an electronic signature OR a Photo of the POD paperwork is required to complete the delivery.
Signature and Photo: Both an electronic signature AND a Photo of the POD paperwork is required to complete the delivery

Note: Drivers retain the option to capture non mandatory POD signatures and POD photos."),
						new CodeDescriptionPairListProvider(() => new ProofOfDeliveryRequiredTypes()),
						RegistryStorageFlags.System,
						ProofOfDeliveryRequiredTypes.Codes.None));
			}
		}

		public BooleanRegistryItem LandTransportProofOfPickupSignatureRequired
		{
			get
			{
				return GetItem("ProofOfPickupSignatureRequired", () => new BooleanRegistryItem(
					"ProofOfPickupSignatureRequired",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
					ResString.GetMultilingualString("58904b69-2dec-452d-884a-c2ba136b24dd", "Proof of Pickup Signature Required"),
					ResString.GetMultilingualString("7f941c69-23e7-4926-91be-cf08695f6bff", "This registry sets whether a driver using the Land Transport Mobility App is forced to capture a Proof of Pickup when completing a Pickup Instruction within the App."),
					RegistryStorageFlags.System,
					false));
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem TransportFailureReasons
		{
			get
			{
				return GetItem("TransportFailureReasons", delegate
				{
					var defaultList = (CodeDescriptionPairList)ObjectFactory.Get<IMasterFilesListProvider>().FailureReasons();
					defaultList.DefaultCode = defaultList[0].Code; // we don't use the default, this is just to get rid of the error

					var item = new CodeDescriptionPairListWithDefaultCodeRegistryItem
						(
							"TransportFailureReasons",
							RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
							ResString.GetMultilingualString("7751838A-AC0A-4C0D-AA79-F6D7664628BD", "Failure Reasons"),
							ResString.GetMultilingualString("625F064D-A089-4130-84F9-9C29EBF71596", "Reasons why Run Sheet Instruction could not be completed."),
							RegistryStorageFlags.System,
							defaultList,
							false
						);

					return item;
				});
			}
		}

		public BillCustomisationByServiceLevelRegistryItem TransportConsignmentNumberFormat
		{
			get
			{
				return GetItem("TransportConsignmentNumberFormat", delegate
				{
					var dataType = new BillCustomisationByServiceLevelRegistryDataType(DefaultConsignmentNumberCustomisation);
					dataType.FountainPrefix = "CN";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("50fe52a1-1ad0-46c9-8d43-0186bd03182e", "Transport Consignment");
					dataType.SequenceNumberName = ResString.GetMultilingualString("AFCE8165-C28F-4492-955E-0DB394F87887", "Shipment");
					dataType.MaxLength = 20;
					dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.Domestic;
					dataType.AllowNonAlphanumericCharacters = true;

					return new BillCustomisationByServiceLevelRegistryItem(
						"TransportConsignmentNumberFormat",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
						ResString.GetMultilingualString("195e0d66-ad5e-4512-b09c-bd5a23882e74", "Transport Consignment Number Format"),
						ResString.GetMultilingualString("cbc3cb2a-dfde-465e-898b-d3eb4a26342f", "Override this value to customize how Transport Consignment numbers are formatted"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						dataType);
				});
			}
		}

		BillOfLadingNumberCustomisationsByServiceLevel DefaultConsignmentNumberCustomisation
		{
			get
			{
				var customisationByServiceLevel = new BillOfLadingNumberCustomisationsByServiceLevel();
				var customisation = customisationByServiceLevel.BillOfLadingNumberCustomisations["ALL"];

				var universalOfficeCode = customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.UniversalOfficeCode];
				universalOfficeCode.Include = true;
				universalOfficeCode.Order = 30;

				return customisationByServiceLevel;
			}
		}

		public IntRegistryItem DefaultJobLoadingFixedDuration
		{
			get
			{
				return GetItem("DefaultJobLoadingFixedDuration", delegate
				{
					return new IntRegistryItem(
						"DefaultJobLoadingFixedDuration",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Optimization,
						(NoResString)"Default Job Loading Fixed Duration",
						(NoResString)"The default fixed number of minutes taken to complete a job (loading or unloading) at a customer location.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						15);
				});
			}
		}

		IRegistryItem ILandTransportRegistry.DefaultJobLoadingFixedDuration => DefaultJobLoadingFixedDuration;

		public StringRegistryItem LandTransportOptimizationUrl
		{
			get
			{
				return GetItem("LandTransportOptimizationUrl", delegate
				{
					return new StringRegistryItem(
						"LandTransportOptimizationUrl",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Optimization,
						(NoResString)"Land Transport Optimization URL",
						(NoResString)"This URL is for Land Transport Optimization.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						"https://rope.wisegrid.net/");
				});
			}
		}

		public IntRegistryItem RunSheetMaxTimeInterval
		{
			get
			{
				return GetItem("RunSheetMaxTimeInterval", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"RunSheetMaxTimeInterval",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Optimization,
						ResString.GetMultilingualString("df43e7f8-b731-441d-89da-338bc5d9acb3", "Run Sheet Max Time Interval"),
						ResString.GetMultilingualString("f2f51b2f-042f-4a0c-a5bd-9c0fca5b7d87", "This sets the maximum end date-time when calculating the array of opening and closing time for the optimizer, calculated by adding registry hours onto the run sheet’s planned start time to calculate the address’s maximum end date. When optimizing a Run sheet, the optimizer will evaluate an address’s opening hours as needing to be completed between the run sheet planned start date-time and this maximum end date-time."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						168,
						1,
						999
					);
					result.EditorInfo = new NumericRegistryEditorInfo(0);
					return result;
				});
			}
		}

		public static class ConsignmentMandatoryFieldOptions
		{
			public const string BillToParty = "ClientRequestedBillingParty";
			public const string BookingParty = "BookingParty";
			public const string ControllingBranch = DtbConsignmentSchema.Constants.LTC_GB_Branch;
			public const string ServiceLevel = DtbConsignmentSchema.Constants.LTC_RS_NKServiceLevel;
			public const string ConsignmentNote = DtbConsignmentSchema.Constants.LTC_ConnoteNumber;
			public const string Incoterm = DtbConsignmentSchema.Constants.LTC_Incoterm;
		}

		public CodeDescriptionBoolDisallowNewRegistryItem LandTransportConsignmentMandatoryFields
		{
			get
			{
				return GetItem("LandTransportConsignmentMandatoryFields",
					delegate
					{
						var defaultValue = new CodeDescriptionBoolDisallowNewCollection();
						defaultValue.Add(50, ConsignmentMandatoryFieldOptions.BillToParty, ResString.GetMultilingualString("3d536914-b956-4475-9c2c-771f73221360", "Bill to Party"), false);
						defaultValue.Add(50, ConsignmentMandatoryFieldOptions.BookingParty, ResString.GetMultilingualString("e8c70ec7-b4f7-42e8-a32a-3be938c7a65e", "Booking Party"), false);
						defaultValue.Add(50, ConsignmentMandatoryFieldOptions.ControllingBranch, ResString.GetMultilingualString("b85bcd3b-eed4-43fe-8397-709bd24f8fd4", "Controlling Branch"), false);
						defaultValue.Add(50, ConsignmentMandatoryFieldOptions.ServiceLevel, ResString.GetMultilingualString("507df3dc-7951-4e64-bb7a-7829a2c0ba25", "Service Level"), false);
						defaultValue.Add(50, ConsignmentMandatoryFieldOptions.ConsignmentNote, ResString.GetMultilingualString("db16c464-cdda-4ee2-8506-16ec2c6f0edb", "Consignment Note"), false);
						defaultValue.Add(50, ConsignmentMandatoryFieldOptions.Incoterm, ResString.GetMultilingualString("280d603d-3b35-40d2-9e35-4e73416a5dbf", "Incoterm"), false);

						return new CodeDescriptionBoolDisallowNewRegistryItem(
							"LandTransportConsignmentMandatoryFields",
							RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
							ResString.GetMultilingualString("038d7b0c-ba21-4f3a-b974-582e0a4c9175", "Consignment Mandatory Fields"),
							ResString.GetMultilingualString("6ccecece-a1ca-407e-b998-c536ea7e4c8e", "Specifies mandatory fields in Consignment."),
							RegistryStorageFlags.System | RegistryStorageFlags.Company,
							TransportRegistry.Instance.EnableLandTransport.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
							new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("c630f9dd-d363-4673-a621-ed551c9a786a", "Is Mandatory"), true, true),
							defaultValue);
					});
			}
		}

		public MultilingualStringRegistryItem LandTransportProofOfPickupDeclaration
		{
			get
			{
				return GetItem("ProofOfPickupDeclaration", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"ProofOfPickupDeclaration",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
						ResString.GetMultilingualString("3e6097a6-cb75-4c04-bca1-30a3a7aa2a44", "Proof of Pickup Declaration"),
						ResString.GetMultilingualString("f7c69c90-e941-4fd7-979b-658dae8a4d47", @"This text will appear on the driver's device screen when customers sign the Proof of Pickup and will appear on the Proof of Pickup Images saved in e-Docs.

An entry in this registry can be used for obtaining the consent of the signer as recipient, if required for collecting and processing personal data under applicable privacy laws. To include a hyperlink as part of the declaration, add the clickable text link and URL using the following format:

<a href=""URL"">Text</a>

e.g. <a href=""MyPrivacyPolicy.com"">Privacy Policy</a>"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						(NoResString)string.Empty);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem LandTransportProofOfDeliveryDeclaration
		{
			get
			{
				return GetItem("ProofOfDeliveryDeclaration", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"ProofOfDeliveryDeclaration",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
						ResString.GetMultilingualString("a4d9acf6-457d-4b6a-8588-2712d478ea25", "Proof of Delivery Declaration"),
						ResString.GetMultilingualString("2ca820c4-0855-4ef2-a8e6-a86e29a91be2", @"This text will appear on the driver's device screen when customers sign the Proof of Delivery and will appear on the Proof of Delivery Images saved in e-Docs.

An entry in this registry can be used for obtaining the consent of the signer as recipient, if required for collecting and processing personal data under applicable privacy laws. To include a hyperlink as part of the declaration, add the clickable text link and URL using the following format:

<a href=""URL"">Text</a>

e.g. <a href=""MyPrivacyPolicy.com"">Privacy Policy</a>"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						(NoResString)string.Empty);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public BooleanRegistryItem DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber
		{
			get
			{
				return GetItem("DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber", delegate
				{
					return new BooleanRegistryItem(
					"DisableDtbConsignmentActionPickedUpDeliveredLogSubscriber",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
					ResString.GetMultilingualString("670b348b-e2be-43b0-a278-855b31aa190e", "Disable Consignment Action Picked Up/Delivered Log Subscriber"),
					ResString.GetMultilingualString("55a4e888-c921-4738-98d9-953ab67623a4", "When disabled, service task will not process consignment action's picked up or delivered log events by Log Subscriber."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false);
				});
			}
		}

		public CodePairRegistryItem ProofOfDeliveryPickupStorageLocation => GetItem("ProofOfDeliveryPickupStorageLocation", () =>
		{
			return new CodePairRegistryItem(
				"ProofOfDeliveryPickupStorageLocation",
				RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
				ResString.GetMultilingualString("ea6fc716-fa87-4873-b289-58eab1f904f2", "Proof of Delivery/Pickup Storage Location"),
				ResString.GetMultilingualString("5523fa47-51a5-42cf-ab11-a5e54417edcc", "Proof of Delivery and Proof of Pickup confirmations for single customer instructions will be stored as eDocs on the entity selected here. Changing this setting will not move existing documents."),
				new CodeDescriptionPairListProvider(() => new ProofOfDeliveryPickupDestinations()),
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				RegistryOptions.Default,
				defaultValue: ProofOfDeliveryPickupDestinations.Codes.Consignment
			);
		});

		public BooleanRegistryItem ShowDetailedErrorMessages
		{
			get
			{
				return GetItem("ShowDetailedErrorMessages", delegate
				{
					return new BooleanRegistryItem(
					"ShowDetailedErrorMessages",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
					(NoResString)"Show Detailed Error Messages",
					(NoResString)"When enabled, it provides additional details for the error messages, including steps to take.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForCargoWise,
					false);
				});
			}
		}

		public MobilityDocumentTypesRegistryItem MobilityDocumentTypes
		{
			get
			{
				return GetItem("MobilityDocumentTypes", delegate
				{
					var defaultValue = new MobilityDocumentTypeCollection();
					var newType = new MobilityDocumentType();
					newType.RT_DocType = "DOR";
					defaultValue.Add(newType);

					return new MobilityDocumentTypesRegistryItem(
						"MobilityDocumentTypes",
						RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport_Mobility,
						defaultValue);
				});
			}
		}

		public BooleanRegistryItem EnableLandTransportBetaFeatures
		{
			get
			{
				return GetItem("EnableLandTransportBetaFeatures", () => new BooleanRegistryItem(
					"EnableLandTransportBetaFeatures",
					RawDataRegistry.Categories.Transport_LandAndPortTransport_LandTransport,
					(NoResString)"Enable Beta Features Menu",
					(NoResString)"Enable the Land Transport beta features menu in the LTP portal.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public static LandTransportRegistry Instance => instance ??= new LandTransportRegistry();

		[ThreadStatic]
		static LandTransportRegistry instance;

		public override bool IsForProductivityWise => false;
	}
}
