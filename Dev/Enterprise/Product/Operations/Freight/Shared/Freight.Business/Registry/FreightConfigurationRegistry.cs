using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public sealed class FreightConfigurationRegistry : RegistryItemSet, IFreightConfigurationRegistry
	{
		#region Construction

		public static FreightConfigurationRegistry Instance
		{
			get { return instance ?? (instance = new FreightConfigurationRegistry()); }
		}

		[ThreadStatic]
		static FreightConfigurationRegistry instance;

		FreightConfigurationRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : FreightDataRegistry.Categories
		{
			public static MultilingualString Freight_Consolidations_CarrierContractNumbers => CombineCategories(Freight_Consolidations, ResString.GetMultilingualString("319c3485-5602-44aa-b178-eb81708aad19", "Carrier Contract Numbers"));
			public static MultilingualString Freight_Shipment_ClientContractNumber => CombineCategories(Freight_Shipment, ResString.GetMultilingualString("61499d76-43ef-4ef0-86a5-443450a3f52a", "Client Contract Number"));
			public static MultilingualString Schedules_SailingSchedule_Allocations => CombineCategories(Schedules_SailingSchedule, ResString.GetMultilingualString("876de77e-fab6-49fc-9d1c-ade5058c8499", "Allocations"));
		}

		#endregion

		public BillCustomisationRegistryItem ConsolNumberCustomisation
		{
			get
			{
				return GetItem("ConsolNumberCustomisation", delegate
				{
					BillCustomisationRegistryDataType dataType = new BillCustomisationRegistryDataType();
					dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.Consol;
					dataType.GeneratedNumberName = ResString.GetMultilingualString("e459264e-d9d6-48a2-a4de-1ae979ea7675", "Consol Number");
					dataType.SequenceNumberName = ResString.GetMultilingualString("B342E71B-C8C6-499E-9BE2-44D872F3C134", "Consol");
					dataType.MaxLength = JobConsolSchema.JK_UniqueConsignRef.MaxLength;

					return new BillCustomisationRegistryItem(
						"ConsolNumberCustomisation",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("e459264e-d9d6-48a2-a4de-1ae979ea7675", "Consol Number"),
						ResString.GetMultilingualString("dadbe29c-db2d-45d8-a589-388cc23ad98c", "Override this value to customize how consolidations are formatted for ALL Transport Modes"),
						RegistryStorageFlags.All,
						dataType
						);
				});
			}
		}

		IRegistryItem IFreightConfigurationRegistry.ConsolNumberCustomisation => ConsolNumberCustomisation;

		public BillCustomisationRegistryItem RoadConsolMasterBillNumber
		{
			get
			{
				return GetItem("RoadConsolMasterBillNumber", delegate
				{
					var dataType = new BillCustomisationRegistryDataType();
					dataType.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.Consol;
					dataType.FountainPrefix = "C";
					dataType.GeneratedNumberName = ResString.GetMultilingualString("5F92B1D1-7A7E-434E-A88F-808DEFD50F8F", "Master Bill");
					dataType.SequenceNumberName = ResString.GetMultilingualString("B342E71B-C8C6-499E-9BE2-44D872F3C134", "Consol");
					dataType.MaxLength = JobConsolSchema.JK_MasterBillNum.MaxLength;
					dataType.AllowNonAlphanumericCharacters = true;
					dataType.EnableMacroInsertion = false;

					return new BillCustomisationRegistryItem(
						"RoadConsolMasterBillNumber",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("76F50098-D78A-4311-A007-84508ACFB9F4", "Road Consol Master Bill Number"),
						ResString.GetMultilingualString("69447C62-3F36-44FB-92E8-4E51E87F9F34", "Override this value to customize how Master Bills are formatted for Transport Mode ROAD."),
						RegistryStorageFlags.All,
						dataType
						);
				});
			}
		}

		#region AWB Issue Date

		public CodePairRegistryItem AWBIssueDate
		{
			get
			{
				return GetItem("AWBIssueDate", delegate
				{
					var listProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(AWBIssueDateIsTodaysDate, ResString.GetMultilingualString("642d02e1-abdd-4fcf-802d-4ec037a587b0", "Today's Date"));
						list.AddPair(AWBIssueDateIsCutOffDate, ResString.GetMultilingualString("51ce0406-e3e5-4d59-8c4c-8d3c6244dc9c", "Consol Main Transport Cut Off Date"));
						return list;
					});

					return new CodePairRegistryItem(
						"AWBIssueDate",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("a06b2cce-c806-44e6-b7a4-9f45e778531c", "AWB Issue Date"),
						ResString.GetMultilingualString("e84a13ab-3484-4849-b6a7-eb3892fe47fb", "Specify which date should be used for AWB issue date."),
						listProvider,
						RegistryStorageFlags.Company,
						AWBIssueDateIsTodaysDate);
				});
			}
		}

		public readonly ZString AWBIssueDateIsTodaysDate = "TYE";
		public readonly ZString AWBIssueDateIsCutOffDate = "CTF";

		public BooleanRegistryItem DefaultShipmentIssueDateFromHAWB
		{
			get
			{
				return GetItem("DefaultShipmentIssueDateFromHAWB", delegate
				{
					return new BooleanRegistryItem(
						"DefaultShipmentIssueDateFromHAWB",
						Categories.Freight_AWB_HAWB,
						ResString.GetMultilingualString("7045747f-2106-4d86-9796-0aa9291177b8", "Default Shipment Issue Date from HAWB Executed Date "),
						ResString.GetMultilingualString("396d3889-06db-4f00-aa7f-2a85a2d5ecfa", "Specify whether the Shipment Issue Date, if blank, should defaults from the Executed Date on the HAWB once it has been printed. This setting only applies to Export Air Shipments."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		public BooleanRegistryItem ConsolCutOffDate
		{
			get
			{
				return GetItem("ConsolCutOffDate", delegate
				{
					return new BooleanRegistryItem(
						"ConsolCutOffDate",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("a5658504-2c59-467d-9215-0b04a5179a28", "Consol Cut Off Mandatory"),
						ResString.GetMultilingualString("2cc1632b-df7d-454a-bee3-bb0773c11f6c", "Specify whether the Consol cut off date for the first routing leg should be mandatory. This setting only applies to Export Consols."),
						RegistryStorageFlags.BranchDepartment,
						false);
				});
			}
		}

		public BooleanRegistryItem AutoPackFreightContainers
		{
			get
			{
				return GetItem("AutoPackFreightContainers", delegate
				{
					return new BooleanRegistryItem(
						"AutoPackFreightContainers",
						Categories.Freight,
						ResString.GetMultilingualString("fca8afa8-625e-45be-853e-ba85efa41ef8", "Auto-pack Containers"),
						ResString.GetMultilingualString("60f0172c-f03b-4ff7-be00-68965f935189", "Select No if you do not want your shipments to be automatically packed to Containers on a Consol"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany
		{
			get
			{
				return GetItem("AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany",
					() => new BooleanRegistryItem(
					"AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany",
					Categories.Freight,
					ResString.GetMultilingualString("93b06732-13af-4b3a-83c1-df748d3dfdfd", "Allow inter-company hyperlinks to be opened in receiving company"),
					ResString.GetMultilingualString("9d71f70e-70aa-4687-ab65-2e845033b0b6", "Allow inter-company Hyperlinks to be opened and create an operational job in the logged-in company"),
					RegistryStorageFlags.Company | RegistryStorageFlags.CompanyDepartment | RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
					false
				));
			}
		}

		public BooleanRegistryItem MandatoryIncoTerm
		{
			get
			{
				return GetItem("MandatoryIncoTerm", delegate
				{
					return new BooleanRegistryItem(
						"MandatoryIncoTerm",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("881c0781-95e5-91a8-4e84-05d2261c9445", "Incoterm Mandatory"),
						ResString.GetMultilingualString("c6ecdf6c-aea3-f0bc-415e-b0f28af56738", "Specify whether the shipment Incoterm should be a mandatory field. This setting only applies to Export Shipments."),
						RegistryStorageFlags.BranchDepartment,
						false);
				});
			}
		}

		public BooleanRegistryItem MandatoryImportBroker
		{
			get
			{
				return GetItem("MandatoryImportBroker", delegate
				{
					return new BooleanRegistryItem(
						"MandatoryImportBroker",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("7953a153-7e4a-4b23-9d2e-e94534529500", "Import Broker Mandatory"),
						ResString.GetMultilingualString("c3890105-e1a6-4ae0-96f3-317c34992561", "Specify whether the shipment Import Broker should be a mandatory field. This setting only applies to Import Shipments."),
						RegistryStorageFlags.BranchDepartment,
						false);
				});
			}
		}

		public DelayAlertDeliveryRegistryItem AutoDeliverDelayAlertDocuments
		{
			get
			{
				return GetItem("AutoDeliverDelayAlertDocuments", delegate
				{
					return new DelayAlertDeliveryRegistryItem(
						"AutoDeliverDelayAlertDocuments",
						Categories.Freight_Notifications,
						ResString.GetMultilingualString("4c3495a7-120f-4ab0-8c99-0e84e5fc159f", "Delay alert auto-send"),
						ResString.GetMultilingualString("0653a637-22b6-4e19-98f7-68ea30a50649", "When the sailing schedule changes, this registry determines which jobs should have a delay alert delivered."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public BooleanRegistryItem AutoCreateLooseConfirmations
		{
			get
			{
				return GetItem("AutoCreateLooseConfirmations", delegate
				{
					return new BooleanRegistryItem(
						"AutoCreateLooseConfirmations",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("e401877e-9b38-4105-b42e-58dacd049083", "Automatically Create Confirmations for Loose Cargo"),
						ResString.GetMultilingualString("97ac5313-46a4-4624-90e4-56408bc491f5", "Setting the registry to 'No' will disable automatic creation of Confirmations for every loose cargo shipment. With the registry set to 'No', the users will still be able to create Confirmations by manually clicking on Pickup/Delivery > Confirmations tab."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#region Freight/Contracts Registry Items

		public BooleanRegistryItem EnableCarrierContractAllocations
		{
			get
			{
				return GetItem("EnableCarrierContractAllocationsCWNext", delegate
				{
					return new BooleanRegistryItem(
						"EnableCarrierContractAllocationsCWNext",
						Categories.Freight_Contracts,
						(NoResString)"Enable Carrier Contract Allocations",
						(NoResString)@"Set this registry to 'Yes' to enable Allocation Routes under Carrier Contract & Allocations module for production of clients who have subscribed to these functions.
Set this registry to 'Yes' to enable Allocation Routes under Carrier Contract & Allocations module for trial or testing in non-Production environment.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableCarrierContractTariffsAndRates
		{
			get
			{
				return GetItem("EnableCarrierContractTariffsAndRatesCWNext", delegate
				{
					return new BooleanRegistryItem(
						"EnableCarrierContractTariffsAndRatesCWNext",
						Categories.Freight_Contracts,
						(NoResString)"Enable Carrier Contract Tariffs And Rates",
						(NoResString)@"Set this registry to 'Yes' to enable Tariffs & Rates configuration under Carrier Contract & Allocations module for clients who have subscribed to these functions.
Set this registry to 'Yes' to enable Tariffs & Rates configuration under Carrier Contract & Allocations module for trial or testing in non-Production environment.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableContainerWeightLimitSupportOnAllocationRoutes
		{
			get
			{
				return GetItem("EnableContainerWeightLimitSupportOnAllocationRoutes", delegate
				{
					return new BooleanRegistryItem(
						"EnableContainerWeightLimitSupportOnAllocationRoutes",
						Categories.Freight_Contracts,
						(NoResString)"Enable Container Weight Limit Support On Allocation Routes",
						(NoResString)@"If set to ‘Yes’, Container Weight Limit Support will be activated for Allocation Routes. Note: This is temporary and will be removed in WI00881108.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableFreightSpotRateOnAllocationRoutes
		{
			get
			{
				return GetItem("EnableFreightSpotRateOnAllocationRoutes", delegate
				{
					return new BooleanRegistryItem(
						"EnableFreightSpotRateOnAllocationRoutes",
						Categories.Freight_Contracts,
						(NoResString)"Enable Freight Spot Rate On Allocation Routes",
						(NoResString)@"If set to 'Yes', Freight Spot Rate will be enabled for Allocation Routes",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableSubAllocationForAllocationRoutes
		{
			get
			{
				return GetItem("EnableSubAllocationForAllocationRoutes", delegate
				{
					return new BooleanRegistryItem(
						"EnableSubAllocationForAllocationRoutes",
						Categories.Freight_Contracts,
						(NoResString)"Enable Sub Allocation For Allocation Routes",
						(NoResString)@"If set to 'Yes', Sub Allocations will be enabled for Allocation Routes",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableBlankQuantityForSubAllocations
		{
			get
			{
				return GetItem("EnableBlankQuantityForSubAllocations", delegate
				{
					return new BooleanRegistryItem(
						"EnableBlankQuantityForSubAllocations",
						Categories.Freight_Contracts,
						(NoResString)"Enable Blank Quantity For Sub Allocation Routes",
						(NoResString)@"If set to 'Yes', Blank Quantity will be enabled for Sub Allocation Routes",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes
		{
			get
			{
				return GetItem("EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes", delegate
				{
					return new BooleanRegistryItem(
						"EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes",
						Categories.Freight_Contracts,
						(NoResString)"Enable Place of Receipt and Place of Delivery Support On Allocation Routes",
						(NoResString)@"Set this registry to 'Yes' to enable Place of Receipt and Place of Delivery on Allocation Routes. Note: This is temporary and will be removed in WI00894799.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnablePrioritySupportOnAllocationRoutes
		{
			get
			{
				return GetItem("EnablePrioritySupportOnAllocationRoutes", delegate
				{
					return new BooleanRegistryItem(
						"EnablePrioritySupportOnAllocationRoutes",
						Categories.Freight_Contracts,
						(NoResString)"Enable Priority Support On Allocation Routes",
						(NoResString)@"If set to 'Yes', Priority will be enabled for Allocation Routes",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableDemandQuantityOnAllocationRoutes
		{
			get
			{
				return GetItem("EnableDemandQuantityOnAllocationRoutes", delegate
				{
					return new BooleanRegistryItem(
						"EnableDemandQuantityOnAllocationRoutes",
						Categories.Freight_Contracts,
						(NoResString)"Enable Demand Quantity On Allocation Routes",
						(NoResString)@"If set to 'Yes', Demand Quantity will be enabled for Allocation Routes",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableAllocatedEventGenerationOnConsolidations
		{
			get
			{
				return GetItem("EnableAllocatedEventGenerationOnConsolidations", delegate
				{
					return new BooleanRegistryItem(
							"EnableAllocatedEventGenerationOnConsolidations",
							Categories.Freight_Contracts,
							(NoResString)"Enable Allocated Event Generation on Consolidations",
							(NoResString)@"If set to 'Yes', Allocated Event Generation will be enabled for Consolidations",
							RegistryStorageFlags.System,
							RegistryOptions.IsOnlyForSupport,
							false);
				});
			}
		}

		#region AMG Related Parties

		public IntRegistryItem MaximumHierarchyDepthOfAMGRelatedParties
		{
			get
			{
				return GetItem("MaximumHierarchyDepthOfAMGRelatedParties", delegate
				{
					return new IntRegistryItem(
						"MaximumHierarchyDepthOfAMGRelatedParties",
						Categories.Freight_Organizations,
						(NoResString)"Maximum Hierarchy Depth of AMG Related Parties",
						(NoResString)@"Maximum allowable hierarchy depth for configuring 'AMG - Allocations Management Group' type Related Parties between organizations'.
Note: Setting this registry value above 5 may lead to performance issues when searching for carrier contracts with allocations assigned to non-exclusive customers.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						5, 2, int.MaxValue);
				});
			}
		}

		public BooleanRegistryItem EnableRelatedOrganizationForCustomerSpecificAllocations
		{
			get
			{
				return GetItem("EnableRelatedOrganizationForCustomerSpecificAllocations", delegate
				{
					return new BooleanRegistryItem(
						"EnableRelatedOrganizationForCustomerSpecificAllocations",
						Categories.Freight_Contracts,
						(NoResString)"Enable Related Organization for Customer Specific Allocations",
						(NoResString)@"Set this registry to 'Yes' to enable Related Organization for Customer Specific Allocations.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#endregion

		#region Consol / Carrier Contract Numbers

		public BooleanRegistryItem PopulateForwardingConsolContractNumbersIfBlankDuringAutorating
		{
			get
			{
				return GetItem("PopulateForwardingConsolContractNumbersIfBlankDuringAutorating", delegate
				{
					return new BooleanRegistryItem(
						"PopulateForwardingConsolContractNumbersIfBlankDuringAutorating",
						Categories.Freight_Consolidations_CarrierContractNumbers,
						ResString.GetMultilingualString("5a7f087f-f694-43be-b4f0-c5ed249f0c08", "Auto populate carrier contract numbers if blank during Autorating"),
						ResString.GetMultilingualString("5F9D9204-4A75-400A-B5AB-DADD61F487BE", "This registry setting controls whether Contract Number/s of the rates applied to the job during Autorating to be populated to the Consol Reference Number with type 'Carrier Contract Number' if no Carrier Contract Number has been added before. Override the registry if Carrier Contract Number/s to be populated to Consols during Autorating Costs."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public BooleanRegistryItem IgnoreAndReplaceCarrierContractNumbersDuringAutorating
		{
			get
			{
				return GetItem("FreightIgnoreAndReplaceContractNumbersDuringAutorating", delegate
				{
					return new BooleanRegistryItem(
						"FreightIgnoreAndReplaceContractNumbersDuringAutorating",
						Categories.Freight_Consolidations_CarrierContractNumbers,
						ResString.GetMultilingualString("f6fb550f-1eb9-4910-bdff-a52c7518a53b", "Ignore and replace Consol Contract Number/s during Autorating"),
						ResString.GetMultilingualString("313aad63-3771-44c7-84cb-47f0384a0fed", "This registry setting controls whether the Contract Number/s of the rates applied to the job during Autorating Costs will populate to Consol > Reference Number with the type 'Carrier Contract Number' regardless if Carrier Contract Number/s were previously registered on a Consol or not. If this registry is set to “Yes”, any previously entered or auto-populated Contract Numbers will not be used as rate differentiators, but instead they will be deleted and new Contract Numbers if found from matching costs, will be populated to the Consol. Do you want to override the default value?"),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region Shipment / Client Contract Numbers

		public BooleanRegistryItem PopulateClientContractNumbersIfBlankDuringAutorating
		{
			get
			{
				return GetItem("PopulateClientContractNumbersIfBlankDuringAutorating", () => new BooleanRegistryItem(
					"PopulateClientContractNumbersIfBlankDuringAutorating",
					Categories.Freight_Shipment_ClientContractNumber,
					ResString.GetMultilingualString("ad752560-512f-44aa-830c-bb332bfc3085", "Auto populate Client Contract Numbers if blank during Autorating"),
					ResString.GetMultilingualString("62f9a095-7dce-45f1-934e-50d950a124ff", "This registry setting controls whether Client Contract Number/s of the rates applied to the job during Autorating are to be populated to the Reference Number grid with type 'CLC - Client Contract Number', if no Client Contract Number has been added previously. Override the registry to 'Yes' if Client Contract Number/s are to be populated to Shipments during Autorating Revenue."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false));
			}
		}

		public BooleanRegistryItem IgnoreAndReplaceClientContractNumbersDuringAutorating
		{
			get
			{
				return GetItem("FreightIgnoreAndReplaceClientContractNumbersDuringAutorating", () => new BooleanRegistryItem(
					"FreightIgnoreAndReplaceClientContractNumbersDuringAutorating",
					Categories.Freight_Shipment_ClientContractNumber,
					ResString.GetMultilingualString("ff68f900-dc91-4dd2-8544-ce7095f8af3b", "Ignore and replace Client Contract Number(s) during Autorating"),
					ResString.GetMultilingualString("43d0151b-7628-48c7-becb-4d8d4b480f2b", "This registry setting controls whether the Client Contract Number/s of the rates applied to the job during Autorating Revenue will populate to Shipment Reference Number with the type 'Client Contract Number' regardless if Client Contract Number/s were previously registered on a Shipment or not. If this registry is set to “Yes”, any previously entered or auto-populated Client Contract Numbers will not be used as rate differentiators, but instead they will be deleted and new Client Contract Numbers if found from matching revenue, will be populated to the Shipment. Do you want to override the default value?"),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					false));
			}
		}

		public BillCustomisationRegistryItem ClientContractNumberFormat
		{
			get
			{
				return GetItem("ClientContractNumberFormat",
					() => new BillCustomisationRegistryItem(
						"ClientContractNumberFormat",
						Categories.Freight_Shipment_ClientContractNumber,
						ResString.GetMultilingualString("cb50d421-ed53-4613-9772-f63e5fe62595", "Client Contract Number Format"),
						ResString.GetMultilingualString("f4710221-fe14-4cc9-b606-4268534263dc", "Override this value to customize how Client Contract numbers are formatted"),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						new ClientContractNumberRegistryDataType())
				);
			}
		}

		public BooleanRegistryItem EnableClientContractNumber
		{
			get
			{
				return GetItem("EnableClientContractNumber", delegate
				{
					return new BooleanRegistryItem("EnableClientContractNumber",
						Categories.Freight_Shipment,
						(NoResString)"Enable Client Contract Number",
						(NoResString)"This will enable Client Contract Number for Shipments and Bookings grids.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Schedules

		#region Sailing Schedule

		public CodePairRegistryItem VoyageRecyclingPeriod
		{
			get
			{
				return GetItem("VoyageRecyclingPeriod", delegate
				{
					return new CodePairRegistryItem(
						"VoyageRecyclingPeriod",
						Categories.Schedules_SailingSchedule,
						ResString.GetMultilingualString("ec97f5e0-be06-492f-ba80-4d3c6084f85d", "Voyage Recycling Period"),
						ResString.GetMultilingualString("3ceb6444-2230-44db-a4ae-14dcb53a197d", "Use this registry to specify system default value for voyage recycling period. Vessel-Voyage-Carrier combinations will automatically be flagged as 'Archived' after a period of time specified here.\r\nThe default value is \"None\", i.e. no automatic archiving.\r\nDefault voyage recycling period can also be specified at a carrier organization."),
						new CodeDescriptionPairListProvider(() => new VoyageRecyclingPeriodList()),
						RegistryStorageFlags.System,
						VoyageRecyclingPeriodList.Codes.None);
				});
			}
		}

		#region Allocations

		public AllocationMethodDefaultRegistryItem DefaultAllocationMethods
		{
			get
			{
				return GetItem("SchedulesDefaultAllocationMethods", delegate
				{
					return new AllocationMethodDefaultRegistryItem(
						"SchedulesDefaultAllocationMethods",
						Categories.Schedules_SailingSchedule_Allocations,
						ResString.GetMultilingualString("a1f836d8-5199-4c0d-ad89-397a7cf3adc9", "Allocation Method Defaulting"),
						ResString.GetMultilingualString("3707eca7-988b-495e-ac9c-96abab476c84", "This registry option allows you to specify the default allocation method to use for each country/region."));
				});
			}
		}

		public DecimalRegistryItem DefaultOverAllocationPercent
		{
			get
			{
				return GetItem("DefaultOverAllocationPercent", delegate
				{
					return new DecimalRegistryItem(
						"DefaultOverAllocationPercent",
						Categories.Schedules_SailingSchedule_Allocations,
						ResString.GetMultilingualString("1db2f08b-8a16-4b66-b12f-16ae3f214bb3", "Default Over Allocation Percent"),
						ResString.GetMultilingualString("218efa14-874c-4314-b920-cdb40bd8eae9", "The default percentage by which an allocation may be exceeded.\r\nIf set to 0%, allocations may not be exceeded.\r\nIf set to 10%, allocations may exceed allocations by up to 10%.\r\n\r\nWarning: no checking will be done to ensure existing bookings remain within there respective allocations after reducing this value"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0m, 0, 100);
				});
			}
		}

		public BooleanRegistryItem UseGlobalAllocations
		{
			get
			{
				return GetItem("UseGlobalAllocations", delegate
				{
					return new BooleanRegistryItem(
						"UseGlobalAllocations",
						Categories.Schedules_SailingSchedule_Allocations,
						ResString.GetMultilingualString("cb116890-8504-475b-8fc1-2b16a9b74a7c", "Use Global Allocations"),
						ResString.GetMultilingualString("c2ecb5ac-951c-487f-9b1d-8cad93cf7b56", "If enabled, then allocations for all countries/regions will be visible and enforced, not just those for the country/region of the current logged in company."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion // Allocations

		public BooleanRegistryItem GlowGSS
		{
			get
			{
				var name = nameof(GlowGSS);
				return GetItem(name, delegate
				{
					return new BooleanRegistryItem(
						name,
						Categories.Schedules_SailingSchedule,
						(NoResString)"GLOW GSS",
						(NoResString)"Override this registry to enable the new web-based global sailing schedules.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion // Sailing Schedule

		#endregion // Schedules

		#region Default Number of Decimal Places

		public DefaultNumberOfDecimalsRegistryItem DefaultNumberOfDecimalPlaces
		{
			get
			{
				return GetItem("DefaultNumberOfDecimalPlacesForFreight", delegate
				{
					return new DefaultNumberOfDecimalsRegistryItem(
						"DefaultNumberOfDecimalPlacesForFreight",
						Categories.Freight_Shipment_Packages,
						ResString.GetMultilingualString("2A63DAA1-83C6-4D6F-A45A-0E2CBD43D2A3", "Default Number of Decimal Places"),
						ResString.GetMultilingualString("3AD14C58-DD07-413D-B6F2-BF4277FA8180", @"This registry item allows you to configure default number of decimal places for different units of measure (and different rounding rule when data is calculated or imported electronically) per specific transport mode.

If not configured, 3 decimal places are used for weight, volume and length units of measure by default."),
						RegistryStorageFlags.System,
						new DefaultNumberOfDecimalsCollection(Module.Freight));
				});
			}
		}

		public BooleanRegistryItem UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume
		{
			get
			{
				return GetItem("UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume", delegate
				{
					return new BooleanRegistryItem(
						"UseFreightNumberOfDecimalPlacesForAWBWeightAndVolume",
						Categories.Freight_AWB,
						ResString.GetMultilingualString("a4e51d96-f512-4480-9fb4-ffa1bfb993e8", "Number of Decimal Places for Weight and Volume"),
						ResString.GetMultilingualString("a5815589-271e-4943-b572-c21279c98d92", @"Set the value to Yes if you want to synchronize number of decimal places for weight and volume on MAWB and HAWB with the number of decimal places specified in the registry Freight > Shipment > Packages > Default Number of Decimal Places for transport mode Air.

If the registry is off, the weight and volume will be rounded for AWB to 1 decimal place for weight and 2 decimal places for volume."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion // Default Number of Decimal Places

		#region Default Container Modes

		public DefaultContainerModesRegistryItem DefaultContainerModes
		{
			get
			{
				return GetItem("DefaultContainerModes", delegate
				{
					return new DefaultContainerModesRegistryItem(
						"DefaultContainerModes",
						Categories.Freight_Shipment,
						ResString.GetMultilingualString("704065af-c560-403c-8eb6-255b2231a826", "Default Container Modes"),
						ResString.GetMultilingualString("9d91ffcf-ec3f-4227-b630-ecc15ea11401", "Configure the default container mode for each transport mode for Shipment and Export Bookings."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#region Send Contact/Company Id in SI Bill Clauses

		public BooleanRegistryItem SendContactCompanyIdInSIBillClauses
		{
			get
			{
				return GetItem("SendContactCompanyIdInSIBillClauses", delegate
				{
					return new BooleanRegistryItem(
						"SendContactCompanyIdInSIBillClauses",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						ResString.GetMultilingualString("9a49ac8b-761c-44d9-98c7-c3c09e988d99", "Send Contact/Company Id in SI Bill Clauses"),
						ResString.GetMultilingualString("21a056a7-b2a7-42da-838f-6d97452ca456", @"Override this setting if you would like to include Contact details and Government Company ID Registration details of shipper, consignee and main notify party to ‘Other BL Clauses’ section of the Shipping Instruction message to carriers.

Please note, these details are already available within Shipper, Consignee and Notify Sections but might not be printed by some of the carriers on Bills of Lading.  Once this registry is enabled, some carriers may print details in both places."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		#region SI Default Forwarder for Non-Direct Consols

		public BooleanRegistryItem SIDefaultForwarderForNonDirectConsols
		{
			get
			{
				return GetItem("SIDefaultForwarderForNonDirectConsols", delegate
				{
					return new BooleanRegistryItem(
						"SIDefaultForwarderForNonDirectConsols",
						Categories.Freight_Consolidations_OceanCarrierMessaging,
						ResString.GetMultilingualString("E5D130FC-2459-45A2-839D-0EFC6F97B789", "SI Default Forwarder for Non-Direct Consols"),
						ResString.GetMultilingualString("AD1F20D6-EBEB-4B8B-BDB2-0B59E7403F45", @"Carrier Shipping Instruction messages include optional Forwarder organization. Forwarder is defaulted from Consol > Sending Agent field. Override this registry to leave Forwarder organization blank for a non-direct Consolidation.

Note. Forwarder organization can still be overridden per a carrier messaging form prior to sending."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true
					);
				});
			}
		}

		#endregion

		#region Is Consol Dangerous Goods Validation Enabled

		public BooleanRegistryItem IsConsolDangerousGoodsValidationEnabled
		{
			get
			{
				return GetItem("IsConsolDangerousGoodsValidationEnabled", () =>
				{
					return new BooleanRegistryItem(
						"IsConsolDangerousGoodsValidationEnabled",
						Categories.Freight_Consolidations,
						ResString.GetMultilingualString("63af09e0-dad6-90a5-4a10-2c16e632bc40", "Validation of Dangerous Goods"),
						ResString.GetMultilingualString("76db0ade-04e2-e18c-4b87-7e7af7602d7c", @"Override this value if you would like to enable validation of Shipments with dangerous goods against Consolidation dangerous goods settings.

If this registry is set to No, you will be able to attach shipments with dangerous goods to a Consol regardless of Consol hazardous settings.

If this registry is set to Yes, each shipment with dangerous goods will be validated against hazardous settings specified on Consolidation."),
						RegistryStorageFlags.System | RegistryStorageFlags.BranchDepartment,
						false);
				});
			}
		}

		#endregion

		#region EnableTransitWarehouseIntegration

		public BooleanRegistryItem EnableTransitWarehouseIntegration
		{
			get
			{
				return GetItem("DisplayUniquePacklineID", delegate
				{
					return new BooleanRegistryItem(
						"DisplayUniquePacklineID",
						RawDataRegistry.Categories.Freight_PackLine,
						ResString.GetMultilingualString("71fda86a-e053-47b4-9b8a-fcb0f027f173", "Enable Transit Warehouse Integration"),
						ResString.GetMultilingualString("d4ed9b9c-04a5-4225-9554-5d2986e5d0c7", "Enable the integration between Forwarding and Transit Warehouse"),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region EnableTWPackageLinking

		public BooleanRegistryItem EnableTWPackageLinking
		{
			get
			{
				return GetItem("EnableTWPackageLinking", delegate
				{
					return new BooleanRegistryItem(
						"EnableTWPackageLinking",
						RawDataRegistry.Categories.Freight_PackLine,
						ResString.GetMultilingualString("76f0c940-bd1a-4a0c-a2d2-ad0e07fc8f7b", "Enable TW Package Linking"),
						ResString.GetMultilingualString("88d9a419-c66a-4a1a-9bdc-2a65a356a1f2", "Enable TW Package Linking"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region EnableOverpacksAndSealsProjectFeatures

		public BooleanRegistryItem EnableOverpacksAndSealsProjectFeatures
		{
			get
			{
				return GetItem("EnableOverpacksAndSealsProjectFeatures", delegate
				{
					return new BooleanRegistryItem(
						"EnableOverpacksAndSealsProjectFeatures",
						RawDataRegistry.Categories.Freight,
						(NoResString)"Enable Overpacks & Seals project features",
						(NoResString)"Enable all features developed in the overpacks and seals project. This Registry must be deleted upon release of these features to the whole client base.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region EnableDangerousGoodsPortal

		public BooleanRegistryItem EnableDangerousGoodsPortal
		{
			get
			{
				return GetItem("EnableDangerousGoodsPortal", delegate
				{
					return new BooleanRegistryItem(
						"EnableDangerousGoodsPortal",
						RawDataRegistry.Categories.Freight,
						(NoResString)"Enable Dangerous Goods Portal",
						(NoResString)"If set to ‘Yes’, Dangerous Goods Portal will be activated",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region EnableOverpacksGroupingToNewPackLines

		public BooleanRegistryItem EnableOverpacksGroupingToNewPackLines
		{
			get
			{
				return GetItem("EnableOverpacksGroupingToNewPackLines", delegate
				{
					return new BooleanRegistryItem("EnableOverpacksGroupingToNewPackLines",
						RawDataRegistry.Categories.Freight_PackLine,
						(NoResString)"Enable Overpacks Grouping To New Pack Lines",
						(NoResString)"Enable this setting in order to allow the grouping of overpacks sent from TWH integration onto new pack lines. Original pack lines containing the inner packages will be removed from the Shipment in this case. \r\n\r\nNote that Integration with Transit Warehouse must be enabled (Registry > Freight > Pack Line > Enable Transit Warehouse Integration).",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion
	}
}
