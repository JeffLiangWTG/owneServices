using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using RegReleaseType = Enterprise.Registry.Business.ReleaseType;
using RegReleaseTypes = Enterprise.Registry.Business.ReleaseTypes;

namespace Enterprise.Freight.Agency.Business
{
	public sealed class AgencyRegistry : RegistryItemSet
	{
		#region Instance

		public static AgencyRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new AgencyRegistry();
				}
				return instance;
			}
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : LinerAgencyDataRegistry.Categories
		{
			public static MultilingualString Notification_LinerAgency { get { return CombineCategories(Notification, ResString.GetMultilingualString("db9dcbb5-9c6a-4874-b146-377c0789552b", "Liner & Agency")); } }
			public static MultilingualString Notification_LinerAgency_ContainerManagementMessaging { get { return CombineCategories(Notification_LinerAgency, ResString.GetMultilingualString("624dae45-72e7-4ba4-a6e6-3c1dd6c4e3bc", "Container Management Messaging")); } }
			public static MultilingualString Notification_LinerAgency_PeriodicContainerMovementExporter { get { return CombineCategories(Notification_LinerAgency, ResString.GetMultilingualString("d8705643-cb5a-4780-847f-395a7bba3276", "Periodic Container Movement Exporter")); } }
			public static MultilingualString LinerAgency_BillsOfLading { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("11d6c77c-f129-4ebf-8a98-5459f22f8a42", "Bills of Lading")); } }
			public static MultilingualString LinerAgency_BillofLading_BillClause { get { return CombineCategories(LinerAgency_BillsOfLading, ResString.GetMultilingualString("06cd5a44-e73e-4782-88ef-2b94cffe7395", "Bill Clause")); } }
			public static MultilingualString LinerAgency_CarrierContractNumbers { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("6b1ee4e0-daeb-40a7-9a57-d5864af3f57f", "Carrier Contract Numbers")); } }
			public static MultilingualString LinerAgency_ContainerManager { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("7d5ee7b4-4eed-435a-abb4-b6af0ee0f34e", "Container Manager")); } }
			public static MultilingualString LinerAgency_DefaultUnits { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("6d93b39a-072a-4da8-b1c2-ea695057bca7", "Default Units")); } }
			public static MultilingualString LinerAgency_EIDOMessaging { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("bb9c5e1e-d197-40e0-ba0b-c49b137570b5", "E-IDO Messaging")); } }
			public static MultilingualString LinerAgency_ElectronicBookingMessaging { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("826ee8dc-dd74-40b9-9641-9c1745f962a7", "Electronic Booking Messaging")); } }
			public static MultilingualString LinerAgency_PortAuthority { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("9913eae6-1a8c-45a1-81fc-897cf9eeaf96", "Port Authority")); } }
			public static MultilingualString LinerAgency_PortMessaging { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("10b6ff93-d0a6-41a2-985e-ae4df82a1c7c", "Port Messaging")); } }
			public static MultilingualString LinerAgency_PortMessaging_DangerousGoodsManifest { get { return CombineCategories(LinerAgency_PortMessaging, ResString.GetMultilingualString("9410CE47-A447-4184-99E8-FAAADA975E0C", "Dangerous Goods Manifest")); } }
			public static MultilingualString LinerAgency_PortMessaging_ImportReleaseOrder { get { return CombineCategories(LinerAgency_PortMessaging, ResString.GetMultilingualString("b9f0fb6b-7587-45f5-af64-fae66821dcad", "Import Release Order")); } }
			public static MultilingualString LinerAgency_PortMessaging_LoadAndDischargeManifest { get { return CombineCategories(LinerAgency_PortMessaging, ResString.GetMultilingualString("4c54d8f9-6ab4-4033-8d68-c15f81c73df8", "Load and Discharge Manifest")); } }
			public static MultilingualString LinerAgency_PortMessaging_ContainerExportPreAdvice { get { return CombineCategories(LinerAgency_PortMessaging, ResString.GetMultilingualString("234c57d1-3003-4ec0-8262-cf3b6dd2768a", "Container Export Pre-Advice")); } }
			public static MultilingualString LinerAgency_PortMessaging_ContainerManagementMessaging { get { return CombineCategories(LinerAgency_PortMessaging, ResString.GetMultilingualString("d65ec0d8-4bc2-4a59-9961-255436f8fa3e", "Container Management Messaging")); } }
			public static MultilingualString LinerAgency_PrincipalAgencySettings { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("f78c61e7-0094-47a4-bb36-92808b742872", "Principal\\Agency Settings")); } }
			public static MultilingualString LinerAgency_SundryCharges { get { return CombineCategories(LinerAgency, ResString.GetMultilingualString("fc9bd80a-fb25-4422-bc8d-0769f9300807", "Sundry Charges")); } }
		}

		#endregion

		#region Notifications / Conatiner Management Messaging

		public GuidRegistryItem CMMAcknowledgementEmailGroup
		{
			get
			{
				return GetItem("CODECOAcknowledgementEmailGroup", delegate
				{
					return new GuidRegistryItem(
						"CODECOAcknowledgementEmailGroup",
						Categories.Notification_LinerAgency_ContainerManagementMessaging,
						ResString.GetMultilingualString("a696994f-d603-43ad-85d0-3b462b7c35a6", "Group To Send Acknowledgement Emails To"),
						ResString.GetMultilingualString("324147e0-39e4-42bb-98a2-07893117e2bd", "Override the Registry if you would like to suppress Acknowledgement email notifications."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.IsValueOptional,
						Constants.Groups.AllPK);
				});
			}
		}

		public GuidRegistryItem CMMDiscrepanciesEmailGroup
		{
			get
			{
				return GetItem("CODECODiscrepanciesEmailGroup", delegate
				{
					return new GuidRegistryItem(
						"CODECODiscrepanciesEmailGroup",
						Categories.Notification_LinerAgency_ContainerManagementMessaging,
						ResString.GetMultilingualString("dbf2a1e7-5a06-439c-8719-8838d5a333cd", "Group To Send Discrepancies Emails To"),
						ResString.GetMultilingualString("d4668522-48b5-42e4-8b3e-e93bd3e882c2", "Override the Registry if you would like to suppress Discrepancies email notifications."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.IsValueOptional,
						Constants.Groups.AllPK);
				});
			}
		}

		public GuidRegistryItem CMMErrorEmailGroup
		{
			get
			{
				return GetItem("CODECOErrorEmailGroup", delegate
				{
					return new GuidRegistryItem(
						"CODECOErrorEmailGroup",
						Categories.Notification_LinerAgency_ContainerManagementMessaging,
						ResString.GetMultilingualString("1be42d62-4951-4b3d-be6e-5496a07a6894", "Group To Send Error Emails To"),
						ResString.GetMultilingualString("3f7cb975-8c87-487c-98cc-6e19fb3cd3d8", "Override the Registry if you would like to suppress Error email notifications."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.IsValueOptional,
						Constants.Groups.AllPK);
				});
			}
		}

		#endregion

		#region Notifications / Periodic Container Movement Exporter

		public GuidRegistryItem CMMeHubNotificationGroup
		{
			get
			{
				return GetItem("AgencyCMMeHubNotificationGroup", delegate
				{
					return new GuidRegistryItem(
						"AgencyCMMeHubNotificationGroup",
						Categories.Notification_LinerAgency_PeriodicContainerMovementExporter,
						ResString.GetMultilingualString("66c06378-084a-441e-b609-1359857d6b85", "Export Error Notification Group"),
						ResString.GetMultilingualString("62ddd258-1cda-422d-99df-1ac312872da0", "Members of this notification group will receive emails if errors are encountered while trying to export container movements to eHub."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.IsValueOptional,
						Constants.Groups.AllPK);
				});
			}
		}

		#endregion

		#region Liner & Agency

		public DecimalRegistryItem AllowInvoiceAmendmentsDays
		{
			get
			{
				return GetItem("AllowInvoiceAmendmentsDays", delegate
				{
					return new DecimalRegistryItem(
						"AllowInvoiceAmendmentsDays",
						Categories.LinerAgency,
						ResString.GetMultilingualString("190ab9c8-d6cc-4b6f-8357-2c5ad9984440", "Allow Invoice Amendments Days"),
						ResString.GetMultilingualString("fcb311e1-b9d9-485d-838a-902aacf2c769", "Specify a number of days from last discharge port ATA for Imports, and last load port ATD for Exports, when unrestricted amendments to the Bill of Lading > Billing are allowed."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0m, 0, 100);
				});
			}
		}

		public BillCustomisationRegistryItem BookingNumberCustomisation
		{
			get
			{
				return GetItem("BookingNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.FountainPrefix = "V";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("02011926-9e93-4ed0-9bde-0c58f4626399", "Booking");
					dataType.SequenceNumberName = ResString.GetMultilingualString("02011926-9e93-4ed0-9bde-0c58f4626399", "Booking");
					dataType.MaxLength = JobShipmentSchema.JS_CFSReference.MaxLength;
					dataType.Categories |= NumberCustomisationElementCategories.LinerAgency;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = false;

					return new BillCustomisationRegistryItem(
						"BookingNumberCustomisation",
						Categories.LinerAgency,
						ResString.GetMultilingualString("9635eea5-418e-4ff5-b4b8-c8f1293efb85", "Booking Number Customization"),
						ResString.GetMultilingualString("dff58a93-2059-4764-b408-dd0b50fe43fc", "Override this value to customize how bookings are formatted."),
						RegistryStorageFlags.All,
						dataType,
						OceanBillShipmentNumberCustomisation
						);
				});
			}
		}

		public ContainerTranshipmentIndicatorRegistryItem ContainerTranshipmentIndicator
		{
			get
			{
				return GetItem("ContainerTranshipmentIndicator", delegate
				{
					return new ContainerTranshipmentIndicatorRegistryItem(
						"ContainerTranshipmentIndicator",
						Categories.LinerAgency,
						ResString.GetMultilingualString("bbc9a498-e48e-4605-b4db-20853dd15d2f", "Container Transhipment Indicator"), ResString.GetMultilingualString("bc847a56-60d6-4536-abfc-36c026005214", "Enter the characters against each container category and shipment category."),
						RegistryStorageFlags.All
						);
				});
			}
		}

		public BillCustomisationRegistryItem OceanBillNumberCustomisation
		{
			get
			{
				return GetItem("OceanBillNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.FountainPrefix = "V";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("c02ad167-2811-47ab-a9ac-66da3ac7516a", "Ocean Bill");
					dataType.SequenceNumberName = ResString.GetMultilingualString("3F8AC636-3122-44A9-90EB-184C9934FDEF", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_HouseBill.MaxLength;
					dataType.Categories |= NumberCustomisationElementCategories.LinerAgency;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = false;

					return new BillCustomisationRegistryItem(
						"OceanBillNumberCustomisation",
						Categories.LinerAgency,
						ResString.GetMultilingualString("8ab5d1a8-f486-475a-9610-ddd28510cd46", "Bill Of Lading Number Customization"),
						ResString.GetMultilingualString("459e7187-211d-48e1-a5ef-bdabc10c488f", "Override this value to customize how ocean bills are formatted."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue,
						dataType,
						OceanBillShipmentNumberCustomisation
						);
				});
			}
		}

		public BillCustomisationRegistryItem OceanBillShipmentNumberCustomisation
		{
			get
			{
				return GetItem("OceanBillShipmentNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.FountainPrefix = null;
					dataType.GeneratedNumberName = ResString.GetMultilingualString("50fe52a1-1130-46c9-8d43-0186bd03182e", "Shipment Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("3F8AC636-3122-44A9-90EB-184C9934FDEF", "Shipment");
					dataType.MaxLength = JobShipmentSchema.JS_UniqueConsignRef.MaxLength;
					dataType.Categories |= NumberCustomisationElementCategories.LinerAgency;

					return new BillCustomisationRegistryItem(
						"OceanBillShipmentNumberCustomisation",
						Categories.LinerAgency,
						ResString.GetMultilingualString("195e0d66-c75e-4512-b09c-bd5a23882e74", "Shipment Number Customization"),
						ResString.GetMultilingualString("cbc3cb2a-d85e-465e-898b-d3eb4a26342f", "Override this value to customize how shipment numbers are formatted"),
						RegistryStorageFlags.All,
						dataType
						);
				});
			}
		}

		public CodePairRegistryItem ReleaseTypeDefault
		{
			get
			{
				return GetItem("ReleaseTypeDefault", delegate
				{
					return new CodePairRegistryItem(
						"ReleaseTypeDefault",
						Categories.LinerAgency,
						ResString.GetMultilingualString("039df2f1-c9e9-4eae-a272-9758cb11ddea", "Release Type Default"),
						ResString.GetMultilingualString("3641fe00-3d3e-47a5-b8b8-4178fc5cde45", "Default Release Type for Agency"),
						ReleaseTypes,
						RegistryStorageFlags.System,
						""
					);
				});
			}
		}

		public ReleaseTypesRegistryItem ReleaseTypes
		{
			get
			{
				return GetItem("LinerAgencyReleaseTypes", delegate
				{
					RegReleaseTypes defaultValues = new RegReleaseTypes();
					defaultValues.OriginalsNumber = 3;
					defaultValues.CopiesNumber = 3;

					RegReleaseType defaultValue = defaultValues.Types.AddNew();
					defaultValue.Code = "OBR";
					defaultValue.Description = ResString.GetMultilingualString("7bf4fdb4-2f3b-4531-a07e-ce669715db4d", "Ocean bill required at destination");
					defaultValue.OriginalsNumber = 3;
					defaultValue.CopiesNumber = 3;

					defaultValue = defaultValues.Types.AddNew();
					defaultValue.Code = "OBO";
					defaultValue.Description = ResString.GetMultilingualString("c26b26a2-9133-4269-8661-17d680943824", "Ocean bill surrendered at origin");
					defaultValue.OriginalsNumber = 3;
					defaultValue.CopiesNumber = 3;

					defaultValue = defaultValues.Types.AddNew();
					defaultValue.Code = "SWB";
					defaultValue.Description = ResString.GetMultilingualString("53dd5ef0-b720-4708-b716-12d8d905a625", "Sea Waybill");
					defaultValue.OriginalsNumber = 0;
					defaultValue.CopiesNumber = 3;

					defaultValue = defaultValues.Types.AddNew();
					defaultValue.Code = "EBL";
					defaultValue.Description = ResString.GetMultilingualString("0232ebcd-b85e-477e-a258-2580223d74e3", "Express Bill of Lading");
					defaultValue.OriginalsNumber = 0;
					defaultValue.CopiesNumber = 1;

					return new ReleaseTypesRegistryItem(
						"LinerAgencyReleaseTypes",
						Categories.LinerAgency,
						ResString.GetMultilingualString("b8cae68a-98b0-4f5e-9a95-713938bf1d36", "Release Types"),
						ResString.GetMultilingualString("b5ea8a62-193f-45d7-8ba3-52f81edb9431", "This list defines the different Release Type options available in Liner & Agency. You can add your own items to this list but cannot change code and description of the system defined items."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						defaultValues);
				});
			}
		}

		public GuidRegistryItem ServiceLevelDefault
		{
			get
			{
				return GetItem("AgencyServiceLevelDefault", delegate
					{
						return new GuidRegistryItem(
							"AgencyServiceLevelDefault",
							Categories.LinerAgency,
							ResString.GetMultilingualString("baefab62-9cc2-4a46-a599-2625486fc0cc", "Service Level Default"),
							ResString.GetMultilingualString("538cee4b-e4f3-4cb5-a62e-b09451aec437", "Override the Registry if you would like to set the default Service Level."),
							new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.RefServiceLevel),
							RegistryStorageFlags.System | RegistryStorageFlags.SystemDepartment | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.BranchDepartment,
							RegistryOptions.IsValueOptional,
							ServiceLevelDefaultValue
							);
					});
			}
		}

		Guid ServiceLevelDefaultValue
		{
			get
			{
				RefServiceLevel std = RegistryFactory.Instance.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, "STD");
				return std == null ? Guid.Empty : std.PK.ToGuid();
			}
		}

		public BooleanRegistryItem UpdateShipmentDatesFromSailing
		{
			get
			{
				return GetItem("AgencyUpdateShipmentDatesFromSailing", delegate
				{
					return new BooleanRegistryItem(
						"AgencyUpdateShipmentDatesFromSailing",
						Categories.LinerAgency,
						ResString.GetMultilingualString("e4b956ee-fc68-4625-9687-0c11104c8a8a", "Update Shipment Dates From Sailing"),
						ResString.GetMultilingualString("1d772a70-000c-49cb-ba30-326d4091dbfb", "If enabled, 'Shipped on Board' and 'Issued' dates on the shipments will be defaulted from the sailings ATD. If the date is updated on the sailing schedule then the user will be asked if the defaulting should take place."),
						RegistryStorageFlags.System,
						false
						);
				});
			}
		}

		public AllowSendingBookingConfirmationCollectionRegistryItem AllowSendingBookingConfirmationEDIAfterATD
		{
			get
			{
				return GetItem("AllowSendingBookingConfirmationEDIAfterATD", delegate
				{
					return new AllowSendingBookingConfirmationCollectionRegistryItem(
						"AllowSendingBookingConfirmationEDIAfterATD",
						Categories.LinerAgency_ElectronicBookingMessaging,
						ResString.GetMultilingualString("bd5f8df3-fbc5-47f6-8bf8-b385bb714fa3", "Send Booking Confirmation after ATD"),
						ResString.GetMultilingualString("80d68eee-4e92-4396-b64b-ab15e73cb3c3", "If enabled, an updated Booking Confirmation will be automatically sent to the Booking Party after the ATD (Actual Time of Departure) is added to the load port."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						Instance.ElectronicBookingAndShippingInstructions.Value ? RegistryOptions.Default : RegistryOptions.IsHidden,
						AllowSendingBookingConfirmationCollection.NewWithDefaultValues());
				});
			}
		}

		#endregion

		#region Generate Booking & Bill of Lading Numbers

		public BooleanRegistryItem AlwaysGenerateBookingNumbers
		{
			get
			{
				return GetItem("AlwaysGenerateBookingNumbers", delegate
				{
					return new BooleanRegistryItem("AlwaysGenerateBookingNumbers",
						Categories.LinerAgency,
						ResString.GetMultilingualString("3ae1d770-1a6a-4c30-ba32-fc2d2e620234", "Generate Booking Numbers"),
						ResString.GetMultilingualString("3cb2f910-4bc5-4871-af6b-5d4262154830", "If enabled, system will always generate Booking Numbers regardless of the direction of the job. If disabled, the Booking Numbers will only be generated for the export or domestic jobs."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		public BooleanRegistryItem AlwaysGenerateBillOfLadingNumbers
		{
			get
			{
				return GetItem("AlwaysGenerateBillOfLadingNumbers", delegate
				{
					return new BooleanRegistryItem("AlwaysGenerateBillOfLadingNumbers",
						Categories.LinerAgency,
						ResString.GetMultilingualString("c7c397c3-e1c0-4a9e-a4cf-e919072e2f3e", "Generate Bill Of Lading Numbers"),
						ResString.GetMultilingualString("a7e63b5f-6b57-4dba-b143-11a0bc784b20", "If enabled, system will always generate Bill Of Lading Numbers regardless of the direction of the job. If disabled, the Bill Of Lading Numbers will only be generated for the export or domestic jobs."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Liner & Agency / Bills of Lading

		public BooleanRegistryItem UseNewFormBuilderBillOfLading
		{
			get
			{
				return GetItem("UseNewFormBuilderBillOfLading", delegate
				{
					return new BooleanRegistryItem(
						"UseNewFormBuilderBillOfLading",
						Categories.LinerAgency_BillsOfLading,
						ResString.GetMultilingualString("6934AA00-BCED-4989-A975-3D6CEE7508EB", "Use new Form Builder Bill Of Lading"),
						ResString.GetMultilingualString("0D7D879B-6C94-4A3F-9692-54667FF7EBFF", "If turned on CW1 will use new Form builder Bill of Lading."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false
						);
				});
			}
		}

		public BooleanRegistryItem PrintSignature
		{
			get
			{
				return GetItem("PrintSignature", delegate
				{
					return new BooleanRegistryItem(
						"PrintSignature",
						Categories.LinerAgency_BillsOfLading,
						ResString.GetMultilingualString("4D48EEF4-0160-4807-BB7E-6D229F7749C4", "Print Signature"),
						ResString.GetMultilingualString("A88FF1A9-FDBA-48BD-9E08-34A565B95D3D", "Use this registry to allow staff signature to be printed on Bill of Lading documents where a dedicated signature box is provided."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false
						);
				});
			}
		}

		public BooleanRegistryItem ShowPacklineDetailsOnBillsOfLading
		{
			get
			{
				return GetItem("ShowPacklineDetailsOnBillsOfLading", delegate
				{
					return new BooleanRegistryItem(
						"ShowPacklineDetailsOnBillsOfLading",
						Categories.LinerAgency_BillsOfLading,
						ResString.GetMultilingualString("29902A68-C27E-4E5A-90A1-42A6AAC43BC2", "Show Packline Details on Bills of Lading"),
						ResString.GetMultilingualString("F8A4664D-CD02-4662-A07F-E5AB1A347D85", "Enable this option to show the breakdown of Outer Packline details for each container on Bill of Ladings."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						false
						);
				});
			}
		}

		public BillOfLadingImageCollectionRegistryItem BillOfLadingTermsAndConditionsImages
		{
			get
			{
				return GetItem("BillOfLadingTermsAndConditionsImages", delegate
				{
					return new BillOfLadingImageCollectionRegistryItem(
						"BillOfLadingTermsAndConditionsImages",
						Categories.LinerAgency_BillsOfLading,
						ResString.GetMultilingualString("37113E07-AAA4-45EE-A977-1A431CF55D7A", "Bill Of Lading Terms & Conditions"),
						ResString.GetMultilingualString("0E7A04FC-A308-41CD-B171-150E9413B50B", "If enabled, the terms and conditions will be printed on the Bill Of Lading for the nominated principal."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
			}
		}

		public BillOfLadingImageCollectionRegistryItem BillOfLadingLogosImages
		{
			get
			{
				return GetItem("BillOfLadingLogosImages", delegate
				{
					return new BillOfLadingImageCollectionRegistryItem(
						"BillOfLadingLogosImages",
						Categories.LinerAgency_BillsOfLading,
						ResString.GetMultilingualString("EAED5CDE-C752-4DCD-BC1D-56AE2B2EB253", "Bill Of Lading Logos"),
						ResString.GetMultilingualString("8936912B-AA49-4BE2-8AB7-65A0AAABBCA3", "If enabled, the logo will be printed on the Bill Of Lading for the nominated principal."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default);
				});
			}
		}

		public CodePairRegistryItem AgencyOBLChargesDefaultDisplay
		{
			get
			{
				return GetItem("AgencyOBLChargesDisplay", delegate
				{
					return new CodePairRegistryItem(
						"AgencyOBLChargesDisplay",
						Categories.LinerAgency_BillsOfLading,
						ResString.GetMultilingualString("25ddbd06-b8fe-4c42-8c0a-a44b8e661212", "OBL Charges Default"),
						ResString.GetMultilingualString("35e5ed85-13c8-477f-a97a-ff5d72d74bc0", "Use this registry to default what is displayed in the charges section of the Bill Of Lading."),
						new CodeDescriptionPairListProvider(() => new OBLChargesDisplayMode()),
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DocumentsDataRegistry.HBLChargesDisplayTypes.CollectCharges);
				});
			}
		}

		public StringRegistryItem BillOfLadingClause(OrgHeader principal)
		{
			if (principal == null)
			{
				throw new ArgumentNullException(nameof(principal));
			}

			var key = "AgencyBillOfLadingClause-" + principal.PK.ToString(); // hard-coded constant

			return GetItem(key, delegate
			{
				return new StringRegistryItem(
					key,
					Categories.LinerAgency_BillofLading_BillClause,
					(NoResString)principal.OH_FullNameTruncated,
					ResString.GetMultilingualString("fcfc4cc1-340b-4de2-abd5-ec3df47ac8a7", "The Bill Clause will be printed on the body of the Bill Of Lading."),
					new StringRegistryDataType(0, 500),
					new TextRegistryEditorInfo(TextEditorType.Memo),
					RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					RegistryOptions.Default,
					"");
			});
		}

		#endregion

		#region Liner & Agency / Carrier Contract Numbers

		public BooleanRegistryItem PopulateContractNumbersFromRevenueRates
		{
			get
			{
				return GetItem("AgencyPopulateContractNumbersFromRevenueRates", delegate
				{
					return new BooleanRegistryItem(
						"AgencyPopulateContractNumbersFromRevenueRates",
						Categories.LinerAgency_CarrierContractNumbers,
						ResString.GetMultilingualString("da13ac55-af6b-4033-ab88-d71354d9105b", "Populate contract numbers from revenue rates."),
						ResString.GetMultilingualString("00443272-c4ec-43cb-869c-42133e8bec4b", "When enabled, the carrier contract numbers will be added to bills of lading and bookings during revenue rating."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false
						);
				});
			}
		}

		public BooleanRegistryItem ReplaceExistingContractNumbersWithNewNumbers
		{
			get
			{
				return GetItem("AgencyReplaceExistingContractNumbersWithNewNumbers", delegate
				{
					return new BooleanRegistryItem(
						"AgencyReplaceExistingContractNumbersWithNewNumbers",
						Categories.LinerAgency_CarrierContractNumbers,
						ResString.GetMultilingualString("4ac57104-f0b1-4164-a2ac-5237b89d6ca5", "Replace existing contract numbers with new numbers."),
						ResString.GetMultilingualString("fad9b7b2-1383-49c2-91cd-e30040ab49d8", "When this registry is set to Yes, the previously entered/auto-populated contract numbers will be deleted and new contract numbers will be populated."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false
						);
				});
			}
		}

		#endregion

		#region Liner & Agency / Container Detention

		public DetentionAdviceDeliveryRegistryItem DetentionAdviceBehaviour
		{
			get
			{
				return GetItem("DetentionAdviceBehaviour", delegate
				{
					return new DetentionAdviceDeliveryRegistryItem(
						"DetentionAdviceBehaviour",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("8a5090ac-d269-4d05-b47e-8512ace1e75c", "Detention Advice Delivery Behavior"),
						ResString.GetMultilingualString("d6437624-48c8-4124-a74e-44d204b862fa", "This registry option defines how to deliver detention advices."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		public IntRegistryItem DetentionAdviceWarningDays
		{
			get
			{
				return GetItem("DetentionAdviceWarningDays", delegate
				{
					return new IntRegistryItem(
						"DetentionAdviceWarningDays",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("4faed2d1-5128-4627-b1b9-25a8f694d8ea", "Detention Advice Warning Days"),
						null,
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						7, 0, 99);
				});
			}
		}

		public BooleanRegistryItem UpdateEmptyReturnByWhenAvailabilityDatesChange
		{
			get
			{
				return GetItem("UpdateEmptyReturnByWhenAvailabilityDatesChange", delegate
				{
					return new BooleanRegistryItem(
						"UpdateEmptyReturnByWhenAvailabilityDatesChange",
						Categories.LinerAgency_ContainerDetention,
						ResString.GetMultilingualString("03c62313-2e74-4aa9-84ac-91daa4f57fe7", "Recalculated Empty Return By Date"),
						ResString.GetMultilingualString("425c2141-903c-4665-bec9-c92048d243ca", "If enabled, ‘Empty Return By’ date on Bills of Lading Containers will be recalculated automatically when the corresponding Availability Date on Sailing Schedule changes. If there are detention days already calculated with no detention invoices attached, then the movements will also have their detention days recalculated. Any manually given free days to the bill of lading container will be overridden."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / Container Management Messaging

		public BooleanRegistryItem CMMDefaultVoyageFromMovement
		{
			get
			{
				return GetItem("CMMDefaultVoyageFromMovement", delegate
				{
					return new BooleanRegistryItem(
						"CMMDefaultVoyageFromMovement",
						Categories.LinerAgency_PortMessaging_ContainerManagementMessaging,
						ResString.GetMultilingualString("e236cbfe-e574-42de-a4f4-7a8af23b451c", "Default Voyage From Movement"),
						ResString.GetMultilingualString("7d2244ce-3a11-49d7-8aab-e88a36f77c30", "If enabled and the container event message processor is unable to identify the correct Vessel/Voyage, the processor will resort to using the vessel/voyage of an earlier movement considered to be related."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public DateTimeRegistryItem CMMeHubCutOff
		{
			get
			{
				return GetItem("AgencyCMMeHubCutOff", delegate
				{
					return new DateTimeRegistryItem(
						"AgencyCMMeHubCutOff",
						Categories.LinerAgency_PortMessaging_ContainerManagementMessaging,
						ResString.GetMultilingualString("4887ffde-29b7-4e02-9c92-3511448745de", "Auto Movement Export Cut Off"),
						ResString.GetMultilingualString("39126b1e-8544-44d1-9dc7-e286efe2643a", "If entered, then movements that were added before this date will not generate messages. This date should be entered in UTC."),
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						DateTime.MinValue,
						true);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public DateTimeRegistryItem CMMeHubHighWaterMark
		{
			get
			{
				return GetItem("CMMeHubHighWaterMark", delegate
				{
					return new DateTimeRegistryItem(
						"CMMeHubHighWaterMark",
						Categories.LinerAgency_PortMessaging_ContainerManagementMessaging,
						(NoResString)"Auto Movement Export High-Water Mark",
						(NoResString)"Auto Movement Export High-Water Mark",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.NotCached,
						DateTime.MinValue,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowDirectCODECOCOARRICMMMessaging
		{
			get
			{
				return GetItem("AllowDirectCODECOCOARRICMMMessaging", delegate
				{
					return new BooleanRegistryItem(
						"AllowDirectCODECOCOARRICMMMessaging",
						Categories.LinerAgency_PortMessaging_ContainerManagementMessaging,
						(NoResString)"Allow Direct CODECO/COARRI (developer only)",
						(NoResString)"Allow Direct CODECO/COARRI usage for Container Management Messaging.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		public CMMFlagsRegistryItem CMMUpdateBookings
		{
			get
			{
				return GetItem("CMMUpdateBookings", delegate
				{
					CMMFlags defaultValue = new CMMFlags()
					{
						WharfGateIn = true,
						Load = true,
					};

					return new CMMFlagsRegistryItem(
						"CMMUpdateBookings",
						Categories.LinerAgency_PortMessaging_ContainerManagementMessaging,
						ResString.GetMultilingualString("20c57a0c-e0fe-47e6-bade-43b4b7ba1b38", "Update Bookings"),
						ResString.GetMultilingualString("3a1a00ba-587f-4dd2-8ccd-5aa61eb76f69", "Which container event message should update related bookings."),
						RegistryStorageFlags.System,
						defaultValue);
				});
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		public BooleanRegistryItem CMMCreateMissingContainers
		{
			get
			{
				return GetItem("CMMCreateMissingContainers", delegate
				{
					return new BooleanRegistryItem(
						"CMMCreateMissingContainers",
						Categories.LinerAgency_PortMessaging_ContainerManagementMessaging,
						ResString.GetMultilingualString("fb92c237-74d0-4d9c-9033-8faa808f682a", "Create Missing Containers"),
						ResString.GetMultilingualString("8736b0f0-661d-4a4a-9b15-f6dbe9d7074f", "By default, if a message is received referring to the movement of a container not already in the Container Management module, {0} will ignore the movement and report it as a discrepancy. Set this to true if you would rather {0} to add a new container to the Container Management module.", "CargoWise"),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Liner & Agency / Container Manager

		public BooleanRegistryItem AllowNonStandardContainerNumbersInContainerManager
		{
			get
			{
				return GetItem("AgencyCMAllowNonStandardContainerNumbers", delegate
				{
					return new BooleanRegistryItem(
						"AgencyCMAllowNonStandardContainerNumbers",
						Categories.LinerAgency_ContainerManager,
						ResString.GetMultilingualString("d49b016d-5c66-4438-b562-c5e4f4a80147", "Allow Non-Standard Container Numbers"),
						ResString.GetMultilingualString("9ac21c52-bfaa-426d-9f25-a4dd1539fd4b", "When enabled, validation checking on container stock records will be relaxed to allow non-standard container numbers."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false
						);
				});
			}
		}

		public IntRegistryItem MovementArchiveDays
		{
			get
			{
				return GetItem("MovementArchiveDays", delegate
				{
					return new IntRegistryItem(
						"MovementArchiveDays",
						Categories.LinerAgency_ContainerManager,
						ResString.GetMultilingualString("263f3a4c-3695-49ee-ac2f-ade0f531a89e", "Movement Archive Days"),
						ResString.GetMultilingualString("add9f86a-d58f-45a9-84e6-028d697e12ea", "Movements on the movements tab more than this many days old will be hidden by default. A value of 0 indicates that all movements should be shown by default."),
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						60
						);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerCleanCodes
		{
			get
			{
				return GetItem("ContainerCleanCodes", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ContainerCleanCodes",
						Categories.LinerAgency_ContainerManager,
						ResString.GetMultilingualString("1018c0b0-8f8d-4dee-b0b0-f8ee319eade3", "Container Clean Codes"),
						ResString.GetMultilingualString("1018c0b0-8f8d-4dee-b0b0-f8ee319eade3", "Container Clean Codes"),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						new DefaultContainerCleanList()
						);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerDamageCodes
		{
			get
			{
				return GetItem("ContainerDamageCodes", delegate
				{
					return new CodeDescriptionPairListRegistryItem(
						"ContainerDamageCodes",
						Categories.LinerAgency_ContainerManager,
						ResString.GetMultilingualString("d3f50839-6b69-4038-bf63-fd69ee456153", "Container Damage Codes"),
						ResString.GetMultilingualString("d3f50839-6b69-4038-bf63-fd69ee456153", "Container Damage Codes"),
						3,
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						new DefaultContainerDamageList()
						);
				});
			}
		}

		public CodeDescriptionPairListRegistryItem ContainerServiceCodes
		{
			get
			{
				return GetItem("ContainerServiceCodes", delegate
				{
					CodeDescriptionPairListRegistryItem item = new CodeDescriptionPairListRegistryItem(
						"ContainerServiceCodes",
						Categories.LinerAgency_ContainerManager,
						ResString.GetMultilingualString("9a5f1071-c478-4f3f-b1a9-ecac70227894", "Container Service Codes"),
						ResString.GetMultilingualString("dd0db36f-f07c-41cb-97f1-1fa0fd78f1f9", "Container Service Codes"),
						3,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new CodeDescriptionPairList()
						);
					((CodeDescriptionPairListRegistryDataType)item.DataType).AllowEmptyCodes = false;
					return item;
				});
			}
		}

		#endregion

		#region Liner & Agency / Default Units

		public BooleanRegistryItem ConvertToDefaultUnitsOnConfirmation
		{
			get
			{
				return GetItem("AgencyConvertToDefaultUnitsOnConfirmation", delegate
				{
					return new BooleanRegistryItem(
						"AgencyConvertToDefaultUnitsOnConfirmation",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("bd3fc6b2-1fb3-40b5-888b-ea945126b7b4", "Convert Weight and Volume Units on Booking Confirmation"),
						ResString.GetMultilingualString("7a837fbe-4006-47a5-b81e-8edc07b3d0c6", "If enabled, the weight and volume units will be converted to the default units for a bill of lading when a booking is confirmed."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						false);
				});
			}
		}

		public CodePairRegistryItem DefaultBookingWeightUnit
		{
			get
			{
				return GetItem("AgencyDefaultBookingWeightUnit", delegate
				{
					return new CodePairRegistryItem(new RegistryItemImplWithDefaultDelegate<string>(
						"AgencyDefaultBookingWeightUnit",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("6fd8ed17-730f-485d-a3e2-7fd6d07cd12a", "Default Weight Unit for Bookings"),
						ResString.GetMultilingualString("ccb30cf7-f85d-4f41-81b5-6b1a02a42df1", "The default unit of weight to use for Liner & Agency bookings."),
						new CodePairRegistryDataType(OLookUpEditType.Weight, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Weight.Kilograms));
				});
			}
		}

		public CodePairRegistryItem DefaultBookingVolumeUnit
		{
			get
			{
				return GetItem("AgencyDefaultBookingVolumeUnit", delegate
				{
					return new CodePairRegistryItem(new RegistryItemImplWithDefaultDelegate<string>(
						"AgencyDefaultBookingVolumeUnit",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("4ad84acf-220f-4509-89d1-c03a3d41fc12", "Default Volume Unit for Bookings"),
						ResString.GetMultilingualString("96168179-6704-46c0-a3b4-45881de66a2b", "The default unit of volume to use for Liner & Agency bookings."),
						new CodePairRegistryDataType(OLookUpEditType.Volume, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Volume.CubicMetres));
				});
			}
		}

		public CodePairRegistryItem DefaultBookingDimensionUnit
		{
			get
			{
				return GetItem("AgencyDefaultBookingDimensionUnit", delegate
				{
					return new CodePairRegistryItem(new RegistryItemImplWithDefaultDelegate<string>(
						"AgencyDefaultBookingDimensionUnit",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("863c4d6a-4fa1-4c8c-8bb9-63a36e202b47", "Default Dimension Unit for Bookings"),
						ResString.GetMultilingualString("7b027d31-924c-42bf-bb57-5471b3252d2d", "The default unit of dimension to use for Liner & Agency bookings."),
						new CodePairRegistryDataType(OLookUpEditType.Length, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Length.Metres));
				});
			}
		}

		public CodePairRegistryItem DefaultBillWeightUnit
		{
			get
			{
				return GetItem("AgencyDefaultWeightUnit", delegate
				{
					return new CodePairRegistryItem(new RegistryItemImplWithDefaultDelegate<string>(
						"AgencyDefaultWeightUnit",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("d6fb8dda-b4a9-4cf2-b0bc-c57d13c38afe", "Default Weight Unit for Bills of Lading"),
						ResString.GetMultilingualString("2dc883db-96c5-40d3-aed1-623ff58a459d", "The default unit of weight to use for Liner & Agency bills of lading."),
						new CodePairRegistryDataType(OLookUpEditType.Weight, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Weight.Kilograms));
				});
			}
		}

		public CodePairRegistryItem DefaultBillVolumeUnit
		{
			get
			{
				return GetItem("AgencyDefaultVolumeUnit", delegate
				{
					return new CodePairRegistryItem(new RegistryItemImplWithDefaultDelegate<string>(
						"AgencyDefaultVolumeUnit",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("49d1d8c0-2acd-47ef-92a9-3c859fc353a3", "Default Volume Unit for Bills of Lading"),
						ResString.GetMultilingualString("a157f3e6-793a-46e7-81ab-f47e40077fad", "The default unit of volume to use for Liner & Agency bills of lading."),
						new CodePairRegistryDataType(OLookUpEditType.Volume, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Volume.CubicMetres));
				});
			}
		}

		public CodePairRegistryItem DefaultBillDimensionUnit
		{
			get
			{
				return GetItem("AgencyDefaultDimensionUnit", delegate
				{
					return new CodePairRegistryItem(new RegistryItemImplWithDefaultDelegate<string>(
						"AgencyDefaultDimensionUnit",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("66ae32de-ffa5-4843-9662-1c57b8839a85", "Default Dimension Unit for Bills of Lading"),
						ResString.GetMultilingualString("6186a6ae-618a-48f5-87d7-a63a0426eb04", "The default unit of dimension to use for Liner & Agency bills of lading."),
						new CodePairRegistryDataType(OLookUpEditType.Length, false, true),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default,
						(c, b, d) => Constants.Length.Metres));
				});
			}
		}

		public DefaultNumberOfDecimalsRegistryItem DefaultNumberOfDecimalPlaces
		{
			get
			{
				return GetItem("DefaultNumberOfDecimalPlacesForShipping", delegate
				{
					return new DefaultNumberOfDecimalsRegistryItem(
						"DefaultNumberOfDecimalPlacesForShipping",
						Categories.LinerAgency_DefaultUnits,
						ResString.GetMultilingualString("7983B5E6-7D60-49B5-900D-F87A11A2F8BB", "Default Number of Decimal Places"),
						ResString.GetMultilingualString("773A3E00-2E9C-4729-B786-6FA30730142B", @"This registry item allows you to configure default number of decimal places for different units of measure (and different rounding rule when data is calculated or imported electronically) per specific transport mode.

If not configured, 3 decimal places are used for weight, volume and length units of measure by default."),
						RegistryStorageFlags.System,
						new DefaultNumberOfDecimalsCollection(Module.Shipping));
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / E-IDO Messaging

		public EIDOMessagingRegistryItem EIDOMessagingDetails
		{
			get
			{
				return GetItem("EIDOMessagingDetails", delegate
				{
					return new EIDOMessagingRegistryItem(
						"EIDOMessagingDetails",
						Categories.LinerAgency_EIDOMessaging,
						ResString.GetMultilingualString("a2204d29-8d1f-45d9-8e2a-3ab4aed511dd", "Enable"),
						null);
				});
			}
		}

		#region SuppressResourceStringsCheckRegion

		public StringRegistryItem EIDOCCEmailAddresses
		{
			get
			{
				return GetItem("EIDOCCEmailAddresses", delegate
				{
					return new StringRegistryItem(
						"EIDOCCEmailAddresses",
						Categories.LinerAgency_EIDOMessaging,
						(NoResString)"CC Email Addresses (developer only).",
						(NoResString)"A list of email address to CC. One per line.",
						new StringRegistryDataType(0, 1024),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						"");
				});
			}
		}

		#endregion

		public GuidRegistryItem EIDOAcknowledgementEmailGroup
		{
			get
			{
				return GetItem("EIDOAcknowledgementEmailGroup", delegate
				{
					return new GuidRegistryItem(
						"EIDOAcknowledgementEmailGroup",
						Categories.LinerAgency_EIDOMessaging,
						ResString.GetMultilingualString("408dfcd4-3f65-48d5-9873-60f1ace4a3cb", "Acknowledgement Email Group"),
						ResString.GetMultilingualString("e0d9e735-78df-44a3-9d0e-46e829e3dbdd", "Send E-IDO Acknowledgements to Group."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Constants.Groups.AllPK);
				});
			}
		}

		public CodePairRegistryItem EIDOAcknowledgementEmailMode
		{
			get
			{
				return GetItem("EIDOAcknowledgementEmailMode", delegate
				{
					return new CodePairRegistryItem(
						"EIDOAcknowledgementEmailMode",
						Categories.LinerAgency_EIDOMessaging,
						ResString.GetMultilingualString("12cf5781-b9ec-4f78-a5e7-0b333f595cda", "Acknowledgement Email Mode"),
						null,
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.System,
						Constants.EmailTo.NominatedGroup);
				});
			}
		}

		public GuidRegistryItem EIDOErrorEmailGroup
		{
			get
			{
				return GetItem("EIDOErrorEmailGroup", delegate
				{
					return new GuidRegistryItem(

						"EIDOErrorEmailGroup",
						Categories.LinerAgency_EIDOMessaging,
						ResString.GetMultilingualString("23ded75f-085e-4f70-8178-c958c8490587", "Error Email Group"),
						ResString.GetMultilingualString("5684feb5-c2e3-4939-9e2b-7b9f1a8986ff", "Send E-IDO Errors to Group."),
						new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						Constants.Groups.AllPK);
				});
			}
		}

		public CodePairRegistryItem EIDOErrorEmailMode
		{
			get
			{
				return GetItem("EIDOErrorEmailMode", delegate
				{
					return new CodePairRegistryItem(
						"EIDOErrorEmailMode",
						Categories.LinerAgency_EIDOMessaging,
						ResString.GetMultilingualString("4084d189-5fbe-4b41-b964-73cb6bcf5632", "Error Email Mode"),
						null,
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.System,
						Constants.EmailTo.NominatedGroup);
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / Port Authority

		public PortAuthorityPortCollectionRegistryItem PortAuthorityPorts
		{
			get
			{
				return GetItem("PortAuthorityPorts", delegate
				{
					return new PortAuthorityPortCollectionRegistryItem(
						"PortAuthorityPorts",
						Categories.LinerAgency_PortAuthority,
						ResString.GetMultilingualString("03a0b6f4-6bcd-4073-8747-2e33c70fc7c5", "Ports"),
						ResString.GetMultilingualString("e3776e47-a11d-4fd8-a878-2a537e8deb46", "This registry option controls port specific details that usually remain the same for everyone."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						PortAuthorityPortCollection.NewWithDefaultValues());
				});
			}
		}

		public PortAuthoritySettingsRegistryItem PortAuthoritySettings
		{
			get
			{
				return GetItem("PortAuthority", delegate
				{
					return new PortAuthoritySettingsRegistryItem(
						"PortAuthority",
						Categories.LinerAgency_PortAuthority,
						ResString.GetMultilingualString("7200d256-56cf-46b5-93c6-7f4ec3509538", "Settings"),
						ResString.GetMultilingualString("f1e2629d-d9ba-4c43-bfe7-fe60e8e5db98", "This option allows you to enable port authority messaging for specific ports.\r\nIf the desired port is not in this list then you will need to add it to:\r\nLiner & Agency->Port Authority->Ports"),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / Dangerous Goods Notification

		public DangerousGoodsManifestPortCollectionRegistryItem DangerousGoodsManifestPorts
		{
			get
			{
				return GetItem("DangerousGoodsManifestPorts", delegate
				{
					var result = new DangerousGoodsManifestPortCollectionRegistryItem(
						"DangerousGoodsManifestPorts",
						Categories.LinerAgency_PortMessaging_DangerousGoodsManifest,
						ResString.GetMultilingualString("0312add3-c9ff-41ca-a250-003a92341752", "Ports"),
						ResString.GetMultilingualString("aa3d1e4b-ca54-4578-8c42-fa30fc5dd5bd", "This registry allows you to enable Dangerous Goods Manifest messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
					result.CountryFilterPKs = DangerousGoodsManifestCountryList.EnabledCountries;
					return result;
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / Container Export Pre-Advice/Ports

		public PortMessagingPortRegistryItem ContainerExportPreAdvicePorts
		{
			get
			{
				return GetItem("ContainerExportPreAdvicePorts", delegate
				{
					var result = new PortMessagingPortRegistryItem(
						"ContainerExportPreAdvicePorts",
						Categories.LinerAgency_PortMessaging_ContainerExportPreAdvice,
						ResString.GetMultilingualString("06e84ce7-b272-4eae-a73c-f93191a25b69", "Ports"),
						ResString.GetMultilingualString("3d679a41-4617-4a22-bcd2-fc44eff7caf2", "This registry allows you to enable Container Export Pre-Advice messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						PortMessagingPortCollection.NewWithDefaultValues(PortMessagingPortCountryList.EnabledPorts));

					result.CountryFilterPKs = PortMessagingPortCountryList.EnabledCountries;

					return result;
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / Load and Discharge Manifest/Ports

		public PortManifestPortCollectionRegistryItem LoadAndDischargeManifestPorts
		{
			get
			{
				return GetItem("LoadAndDischargeManifestPorts", delegate
				{
					var result = new PortManifestPortCollectionRegistryItem(
						"LoadAndDischargeManifestPorts",
						Categories.LinerAgency_PortMessaging_LoadAndDischargeManifest,
						ResString.GetMultilingualString("19915e39-6f4e-4f23-8135-a07d1d946976", "Ports"),
						ResString.GetMultilingualString("f18a75a9-1996-400b-b322-9cae1ba6f00f", "This registry allows you to enable Load and Discharge Manifest messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
					result.CountryFilterPKs = PortManifestCountryList.EnabledCountries;
					return result;
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / Import Release Order/Ports

		public PortMessagingPortRegistryItem ImportReleaseOrderPorts
		{
			get
			{
				return GetItem("ImportReleaseOrderPorts", delegate
				{
					var result = new PortMessagingPortRegistryItem(
						"ImportReleaseOrderPorts",
						Categories.LinerAgency_PortMessaging_ImportReleaseOrder,
						ResString.GetMultilingualString("6583b2b1-5e4c-4590-9ed4-1e1c3bd2b4ba", "Ports"),
						ResString.GetMultilingualString("8db62c41-68dc-44f0-a5c2-0b2b7fb1cc08", "This registry allows you to enable Import Release Order messaging for specific ports. If your desired port is not in this list, contact WiseTech by creating an incident."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						PortMessagingPortCollection.NewWithDefaultValues(PortMessagingPortCountryList.EnabledPorts));

					result.CountryFilterPKs = PortMessagingPortCountryList.EnabledCountries;

					return result;
				});
			}
		}

		#endregion

		#region Liner & Agency / Port Messaging / Shipping Ports Messaging eHub ID

		public ShippingPortsMessagingEHubIDCollectionRegistryItem ShippingPortsMessagingEHubID
		{
			get
			{
				return GetItem("ShippingPortsMessagingEHubID", delegate
				{
					return new ShippingPortsMessagingEHubIDCollectionRegistryItem(
						"ShippingPortsMessagingEHubID",
						Categories.LinerAgency_PortMessaging,
						(NoResString)"Shipping Ports Messaging eHub ID", // Hidden Registry Item
						(NoResString)"This is the eHub ID used for sending interchange to Shipping Ports.", // Hidden Registry Item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		#endregion

		#region Liner & Agency / Principal\Agency Settings

		public BooleanRegistryItem AllowInterCountryDetentionJobCreation
		{
			get
			{
				return GetItem("AgencyAllowInterCountryDetentionJobCreation", delegate
				{
					return new BooleanRegistryItem("AgencyAllowInterCountryDetentionJobCreation",
						Categories.LinerAgency_PrincipalAgencySettings,
						ResString.GetMultilingualString("43720b15-4794-4e08-9fb3-5b73c13aa06a", "Allow creation of detention jobs for foreign countries/regions"),
						ResString.GetMultilingualString("1543bc06-d144-4d0c-a8e6-aec36ed8b29a", "Allow users to create container detention jobs for containers in other countries/regions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem DefaultCreditorFromPrincipal
		{
			get
			{
				return GetItem("AgencyDefaultCreditorToPrincipal", delegate
				{
					return new BooleanRegistryItem("AgencyDefaultCreditorToPrincipal",
						Categories.LinerAgency_PrincipalAgencySettings,
						ResString.GetMultilingualString("43de4123-4fca-47bb-99f6-f472fa4a34e2", "Default Creditor From Principal"),
						ResString.GetMultilingualString("0eafec67-cf12-436e-83be-20706c1ed076", "If this is enabled, the principal charges in the Booking, Bill of Lading and Container Detention modules will be defaulted from the principal. Whilst this is correct for agencies, if you are a principal you probably want to disable this option."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem PostAllShipmentRevenueCharges
		{
			get
			{
				return GetItem("AgencyPostAllShipmentRevenueCharges", delegate
				{
					return new BooleanRegistryItem("AgencyPostAllShipmentRevenueCharges",
						Categories.LinerAgency_PrincipalAgencySettings,
						ResString.GetMultilingualString("22002eb9-f2d5-4a90-a592-f4063739a2f0", "Booking/Bill of Lading Revenue Charges Posting"),
						ResString.GetMultilingualString("850b4ae4-d12a-4c7c-b15a-2ffef446010e", "If you are a Principal (Shipping Line) organization, you may want to post all revenue charges on Bookings and Bills of Lading (prepaid & collect charges in sending and receiving locations). If you are an Agency organization, you would most likely only want to post charges that are to be paid locally.\r\n\r\nPost all revenue charges?"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem PostAllShipmentCostCharges
		{
			get
			{
				return GetItem("AgencyPostAllShipmentCostCharges", delegate
				{
					return new BooleanRegistryItem("AgencyPostAllShipmentCostCharges",
						Categories.LinerAgency_PrincipalAgencySettings,
						ResString.GetMultilingualString("bd7c249d-ab4a-4643-a4cf-e90d671aa0db", "Booking/Bill of Lading Cost Charges Posting"),
						ResString.GetMultilingualString("0c57a6d3-468f-4a8a-a705-578fc1b5fd8b", "If you are a Principal (Shipping Line) organization, you may want to post all cost charges on Bookings and Bills of Lading (prepaid & collect charges in sending and receiving locations). If you are an Agency organization, you would most likely only want to post charges that are to be paid locally.\r\n\r\nPost all cost charges?"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Liner & Agency / Sundry Charges

		public BillCustomisationRegistryItem SundryJobNumberCustomisation
		{
			get
			{
				return GetItem("SundryJobNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.GeneratedNumberName = ResString.GetMultilingualString("ad12d44d-edc5-4f54-a5f7-422b7c46b62a", "Job Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("3F8AC636-3122-44A9-90EB-184C9934FDEF", "Shipment");
					dataType.MaxLength = 20;
					dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.SundryCharges;
					dataType.PrefixLength = 2;

					return new BillCustomisationRegistryItem(
						"SundryJobNumberCustomisation",
						Categories.LinerAgency_SundryCharges,
						ResString.GetMultilingualString("ac209b06-83c1-4e00-b93b-c1d2165f0425", "Job Number Customization"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						dataType);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem SundryChargeTypes
		{
			get
			{
				return GetItem("SundryChargeTypes", delegate
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList();
					list.AddPair("PSN", ResString.GetMultilingualString("66a7c777-05cc-4ffc-ab7c-19fd57f0ad63", "Principal Sundries"));
					list.DefaultCode = "PSN";

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"SundryChargeTypes",
						Categories.LinerAgency_SundryCharges,
						ResString.GetMultilingualString("8b7573b8-ee54-4d55-948c-ac3de8d00ac5", "Sundry Charge Types"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						list,
						false,
						true,
						3);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem SundryChargeModes
		{
			get
			{
				return GetItem("SundryChargeModes", delegate
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList();
					list.AddPair("STD", ResString.GetMultilingualString("59b3570c-589f-46c5-8b99-027c8816e35f", "Standard"));
					list.DefaultCode = "STD";

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"SundryChargeModes",
						Categories.LinerAgency_SundryCharges,
						ResString.GetMultilingualString("9f557ee0-edfe-4943-90a7-f807c37b7236", "Sundry Charge Modes"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						list,
						false,
						true,
						3);
				});
			}
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem SundryChargeActivities
		{
			get
			{
				return GetItem("SundryChargeActivities", delegate
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList();
					list.AddPair("STD", ResString.GetMultilingualString("59b3570c-589f-46c5-8b99-027c8816e35f", "Standard"));
					list.DefaultCode = "STD";

					return new CodeDescriptionPairListWithDefaultCodeRegistryItem(
						"SundryChargeActivities",
						Categories.LinerAgency_SundryCharges,
						ResString.GetMultilingualString("5f554d6e-469a-4b7c-8b82-dcf96f7b899b", "Sundry Charge Activities"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						list,
						false,
						true,
						3);
				});
			}
		}

		#endregion

		#region electronic Booking & Shipping Instructions

		public BooleanRegistryItem ElectronicBookingAndShippingInstructions
		{
			get
			{
				return GetItem("ElectronicBookingAndShippingInstructions", delegate
				{
					return new BooleanRegistryItem("ElectronicBookingAndShippingInstructions",
						Categories.LinerAgency,
						ResString.GetMultilingualString("53bd7903-8389-486c-b932-2921f762ac0e", "Enable electronic Booking and Shipping Instructions"),
						ResString.GetMultilingualString("ff61c1b2-e85f-48af-9da2-0d69b8df1ace", "If enabled, Electronic Booking Requests and Shipping Instructions from other CargoWise Systems can be received and processed in Liner & Agency Module (Bookings/Bill of Lading)."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Implementation

		[ThreadStatic]
		static AgencyRegistry instance;

		protected override IEnumerable<IRegistryItem> GetItemsNotAccessedUsingProperties()
		{
			List<IRegistryItem> result = new List<IRegistryItem>();
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ShipsAgencyPrincipalCollection principals = new ShipsAgencyPrincipalCollection(factory);
			principals.Load();

			foreach (OrgHeader principal in principals)
			{
				result.Add(BillOfLadingClause(principal));
			}

			return result;
		}

		#endregion
	}
}
