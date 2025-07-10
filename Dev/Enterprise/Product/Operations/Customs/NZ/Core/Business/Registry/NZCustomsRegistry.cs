using System;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Customs.NZ.Business.ResString;

namespace Enterprise.Customs.NZ.Registry
{
	public sealed class NZCustomsDataRegistry : RegistryItemSet, Integration.Customs.NZ.INZCustomsDataRegistry
	{
		#region Construction

		public static NZCustomsDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new NZCustomsDataRegistry();
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static NZCustomsDataRegistry fInstance;

		NZCustomsDataRegistry()
		{
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_NewZealand { get { return CombineCategories(Customs_CountryOrRegion, (NoResString)"New Zealand"); } }
			public static MultilingualString Customs_NewZealand_DocumentationOptions { get { return CombineCategories(Customs_NewZealand, (NoResString)"Documentation Options"); } }
			public static MultilingualString Customs_NewZealand_BrokerDeferredCutoffDate { get { return CombineCategories(Customs_NewZealand, (NoResString)"Broker Deferred Cutoff Date"); } }
			public static MultilingualString Customs_NewZealand_AutoPrintonClearanceResponse { get { return CombineCategories(Customs_NewZealand, (NoResString)"AutoPrint on Clearance Response"); } }
			public static MultilingualString Customs_NewZealand_AutoPrintonClearanceResponse_CustomsCertificate { get { return CombineCategories(Customs_NewZealand_AutoPrintonClearanceResponse, (NoResString)"Customs Certificate"); } }
			public static MultilingualString Customs_NewZealand_AutoPrintonClearanceResponse_EntryPrint { get { return CombineCategories(Customs_NewZealand_AutoPrintonClearanceResponse, (NoResString)"Entry Print"); } }
			public static MultilingualString Customs_NewZealand_AutoPrintonClearanceResponse_DeliveryOrder { get { return CombineCategories(Customs_NewZealand_AutoPrintonClearanceResponse, (NoResString)"Delivery Order"); } }
			public static MultilingualString Customs_NewZealand_MAFeBACCa { get { return CombineCategories(Customs_NewZealand, (NoResString)"MPI eBACCa"); } }
			public static MultilingualString Customs_NewZealand_MessagingModeTesting { get { return CombineCategories(Customs_NewZealand, (NoResString)"Messaging Mode (Testing)"); } }
			public static MultilingualString Customs_NewZealand_ResponseMessageDelivery { get { return CombineCategories(Customs_NewZealand, (NoResString)"Response Message Delivery"); } }
			public static MultilingualString Customs_NewZealand_ResponseMessageDelivery_ImportDeclarations { get { return CombineCategories(Customs_NewZealand_ResponseMessageDelivery, (NoResString)"Import Declarations"); } }
			public static MultilingualString Customs_NewZealand_ResponseMessageDelivery_ExportDeclarations { get { return CombineCategories(Customs_NewZealand_ResponseMessageDelivery, (NoResString)"Export Declarations"); } }
			public static MultilingualString Customs_NewZealand_ResponseMessageDelivery_ImportWriteoff { get { return CombineCategories(Customs_NewZealand_ResponseMessageDelivery, (NoResString)"Import Write-off"); } }
			public static MultilingualString Customs_NewZealand_ResponseMessageDelivery_ExportWriteoff { get { return CombineCategories(Customs_NewZealand_ResponseMessageDelivery, (NoResString)"Export Write-off"); } }
			public static MultilingualString Customs_NewZealand_ResponseMessageDelivery_ExportOutwardReport { get { return CombineCategories(Customs_NewZealand_ResponseMessageDelivery, (NoResString)"Export Outward Report"); } }
			public static MultilingualString Customs_NewZealand_ResponseMessageDelivery_UnsolicitedResponses { get { return CombineCategories(Customs_NewZealand_ResponseMessageDelivery, (NoResString)"Unsolicited Responses"); } }
			public static MultilingualString Customs_NewZealand_TradeSingleWindow { get { return CombineCategories(Customs_NewZealand, (NoResString)"Trade Single Window"); } }
			public static MultilingualString Customs_NewZealand_Testing { get { return CombineCategories(Customs_NewZealand, ResString.GetMultilingualString("A724DC95-0813-468C-8D2A-FB0F4DBD9C09", "Testing")); } }
		}

		#endregion

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.NewZealand;
		}

		public StringArrayRegistryItem DefaultResendingRemarks
		{
			get
			{
				return GetItem("NZDefaultResendingRemarks", delegate
				{
					StringArrayRegistryItem result = new StringArrayRegistryItem(
						"NZDefaultResendingRemarks",
						Categories.Customs_NewZealand,
						(NoResString)"Default Resending Remarks",
						(NoResString)"Enter the default remarks for resending Declarations to Customs",
						RegistryStorageFlags.Company);
					result.DataType.MaximumLength = 250;
					return result;
				});
			}
		}

		public GuidRegistryItem ExportEntryFeeChargeCode
		{
			get
			{
				return GetItem("ExportEntryFeeChargeCode", delegate
					{
						var result = new GuidRegistryItem(
							"ExportEntryFeeChargeCode",
							RatingDataRegistry.Categories.AutoRating_ChargeCodes_Customs_NewZealand,
							(NoResString)"Export Entry Fee Charge Code",
							(NoResString)"This is the charge code that will be used when auto-rating an entry fee for export declarations. If this is not set, then system will not autorate entry fees for export declarations.",
							RegistryStorageFlags.Company);

						result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.AccChargeCode, RegistryFindBoxFilter.None);
						result.CountryFilterPKs = new Guid[] { Core.Constants.CountryGuids.NewZealand };
						return result;
					});
			}
		}

		public BooleanRegistryItem UpdateAttachedManifestedECIsWhenConsolDetailsChange
		{
			get
			{
				return GetItem("NZCustomsUpdateECIManifestsFromConsol", delegate
				{
					return new BooleanRegistryItem(
						"NZCustomsUpdateECIManifestsFromConsol",
						Categories.Customs_NewZealand,
						(NoResString)"Update Write-off Manifests attached to Consols",
						(NoResString)@"Do you want to automatically update attached Manifested Write-offs when Consol Details Change?

Please keep in mind that this will make saving Consols slower when there are Manifest Write-offs attached to Shipments on a Consol you are trying to Save.",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem PreferentialCountryGroupCodeDefaulting
		{
			get
			{
				return GetItem("PreferentialCountryGroupCodeDefaulting", delegate
				{
					return new BooleanRegistryItem(
						"PreferentialCountryGroupCodeDefaulting",
						Categories.Customs_NewZealand,
						(NoResString)"Preferential Country/Region Group Code Defaulting",
						(NoResString)"When set to true, the preferential country/region group code will default to the code that will have the lowest duty rate.",
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public IntRegistryItem MaxNumberOfECIManifestLinesAccepted
		{
			get
			{
				return GetItem("MaxNumberOfECIManifestLinesAccepted", delegate
				{
					return new IntRegistryItem(
						"MaxNumberOfECIManifestLinesAccepted",
						Categories.Customs_NewZealand,
						(NoResString)"Maximum number of entry lines accepted on ICR manifests",
						(NoResString)@"The Maximum number of entry lines accepted on Import ICR manifests. 
When creating an Air Cargo or Sea Cargo ICR using a Universal Shipment XML that exceeds the amount of Consignments permitted by this setting, the system will create multiple jobs up to this limit.",
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						9999,
						5,
						9999);
				});
			}
		}

		IRegistryItem Integration.Customs.NZ.INZCustomsDataRegistry.MaxNumberOfECIManifestLinesAccepted => MaxNumberOfECIManifestLinesAccepted;

		public IntRegistryItem MaxNumberOfECIManifestLinesAcceptedCRE
		{
			get
			{
				return GetItem("MaxNumberOfECIManifestLinesAcceptedCRE", delegate
				{
					return new IntRegistryItem(
						"MaxNumberOfECIManifestLinesAcceptedCRE",
						Categories.Customs_NewZealand,
						(NoResString)"Maximum number of entry lines accepted on CRE manifests",
						(NoResString)@"The Maximum number of entry lines accepted on Export CRE manifests. 
When creating an Air Cargo or Sea Cargo CRE using a Universal Shipment XML that exceeds the amount of Consignments permitted by this setting, the system will create multiple jobs up to this limit.",
						null,
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						6000,
						5,
						9999);
				});
			}
		}

		IRegistryItem Integration.Customs.NZ.INZCustomsDataRegistry.MaxNumberOfECIManifestLinesAcceptedCRE => MaxNumberOfECIManifestLinesAcceptedCRE;

		public BooleanRegistryItem HideDeclarantCodeOnCustomsDocumentation
		{
			get
			{
				return GetItem("NZCustomsHideDeclarantCode", delegate
				{
					return new BooleanRegistryItem(
						"NZCustomsHideDeclarantCode",
						Categories.Customs_NewZealand_DocumentationOptions,
						(NoResString)"Hide Declarant Code",
						(NoResString)"Do you want the Broker's Declarant Code to be hidden on Customs Documentation?",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem UseRefDatabaseData
		{
			get
			{
				return GetItem("UseRefDatabaseData", delegate
				{
					return new BooleanRegistryItem(
						"UseRefDatabaseData",
						Categories.Customs_NewZealand,
						(NoResString)"Use RefDatabase Data",
						(NoResString)"Setting this to True will force CW1 to use the reference data in CW-RefDatabase instead of RefDb_Trf_NZ.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#region Broker Deferred Cutoff Date

		public BooleanRegistryItem BrokerDeferredCutoffDateEnabled
		{
			get
			{
				return GetItem("Enabled", delegate
				{
					return new BooleanRegistryItem("Enabled",
						Categories.Customs_NewZealand_BrokerDeferredCutoffDate,
						(NoResString)"Enabled",
						(NoResString)"Enable the Broker Deferred Cutoff Date. From 27 October 2020, NZ Customs moved their deferred broker payments to the 20th each month.",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public DecimalRegistryItem BrokerDeferredCutoffDateMinimumDutyWarning
		{
			get
			{
				return GetItem("MimimumDutyForWarning", delegate
				{
					return new DecimalRegistryItem("MimimumDutyForWarning",
						Categories.Customs_NewZealand_BrokerDeferredCutoffDate,
						(NoResString)"Minimum Duty/Tax for Warning",
						(NoResString)"Minimum Duty/Tax for Warning",
						RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue, 0.00m);
				});
			}
		}

		public IntRegistryItem BrokerDeferredCutoffDateDaysBeforeWarning
		{
			get
			{
				return GetItem("DaysBeforeCutoffWarning", delegate
				{
					return new IntRegistryItem("DaysBeforeCutoffWarning",
						Categories.Customs_NewZealand_BrokerDeferredCutoffDate,
						(NoResString)"Days before Cutoff to start Warning",
						(NoResString)"Days before Cutoff to start Warning",
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						3);
				});
			}
		}

		#endregion

		#region Customs Certificate
		public GuidRegistryItem CustomsCertificatePrinter
		{
			get
			{
				return GetItem("NZCustomsCertificatePrinter", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NZCustomsCertificatePrinter",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_CustomsCertificate,
						(NoResString)"Printer",
						(NoResString)"The Printer you would like copies of the Customs Certificate Document to go to when a Delivery Order response is received.",
						RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue);
					return result;
				});
			}
		}

		public IntRegistryItem CustomsCertificateCopies
		{
			get
			{
				return GetItem("NZCustomsCertificateCopies", delegate
				{
					return new IntRegistryItem(
						"NZCustomsCertificateCopies",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_CustomsCertificate,
						(NoResString)"Copies",
						(NoResString)"How many copies of the Customs Certificate Document you would like to automatically print when a Delivery Order response is received.",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						1);
				});
			}
		}

		public BooleanRegistryItem CustomsCertificateCopyToEDocs
		{
			get
			{
				return GetItem("NZCustomsCertificateCopyToEDocs", delegate
				{
					return new BooleanRegistryItem(
						"NZCustomsCertificateCopyToEDocs",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_CustomsCertificate,
						(NoResString)"Copy to eDocs",
						(NoResString)"Would you like a copy of the Customs Certificate Document saved in eDocs when a Delivery Order response is received.",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Entry Print

		public GuidRegistryItem EntryPrintPrinter
		{
			get
			{
				return GetItem("NZEntryPrintPrinter", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NZEntryPrintPrinter",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_EntryPrint,
						(NoResString)"Printer",
						(NoResString)"The Printer you would like copies of the Entry Print Document to go to when a Delivery Order response is received.",
						RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue);
					return result;
				});
			}
		}

		public IntRegistryItem EntryPrintCopies
		{
			get
			{
				return GetItem("NZEntryPrintCopies", delegate
				{
					return new IntRegistryItem(
						"NZEntryPrintCopies",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_EntryPrint,
						(NoResString)"Copies",
						(NoResString)"How many copies of the Entry Print Document you would like to automatically print when a Delivery Order response is received.",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						1);
				});
			}
		}

		public BooleanRegistryItem EntryPrintCopyToEDocs
		{
			get
			{
				return GetItem("NZEntryPrintCopyToEDocs", delegate
				{
					return new BooleanRegistryItem(
						"NZEntryPrintCopyToEDocs",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_EntryPrint,
						(NoResString)"Copy to eDocs",
						(NoResString)"Would you like a copy of the Entry Print Document saved in eDocs when a Delivery Order response is received.",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region Delivery Order

		public GuidRegistryItem DeliveryOrderPrinter
		{
			get
			{
				return GetItem("NZDeliveryOrderPrinter", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NZDeliveryOrderPrinter",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_DeliveryOrder,
						(NoResString)"Printer",
						(NoResString)"The Printer you would like copies of the Delivery Order Document to go to when a Delivery Order response is received.",
						RegistryStorageFlags.BranchDepartment | RegistryStorageFlags.Branch,
						RegistryOptions.IsValueOptional | RegistryOptions.PreserveTestValue);

					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.StmPrintQueue);
					return result;
				});
			}
		}

		public IntRegistryItem DeliveryOrderCopies
		{
			get
			{
				return GetItem("NZDeliveryOrderCopies", delegate
				{
					return new IntRegistryItem(
						"NZDeliveryOrderCopies",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_DeliveryOrder,
						(NoResString)"Copies",
						(NoResString)"How many copies of the Delivery Order Document you would like to automatically print when a Delivery Order response is received.",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						1);
				});
			}
		}

		public BooleanRegistryItem DeliveryOrderCopyToEDocs
		{
			get
			{
				return GetItem("NZDeliveryOrderCopyToEDocs", delegate
				{
					return new BooleanRegistryItem(
						"NZDeliveryOrderCopyToEDocs",
						Categories.Customs_NewZealand_AutoPrintonClearanceResponse_DeliveryOrder,
						(NoResString)"Copy to eDocs",
						(NoResString)"Would you like a copy of the Delivery Order Document saved in eDocs when a Delivery Order response is received.",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		#endregion

		#region MPIeBACCa

		public DecimalRegistryItem DefaultOverseasInsurance
		{
			get
			{
				return GetItem("DefaultOverseasInsurance", delegate
				{
					DecimalRegistryItem result = new DecimalRegistryItem(
						"DefaultOverseasInsurance",
						Categories.Customs_NewZealand,
						(NoResString)"Default Insurance Percentage",
						(NoResString)"Default Insurance as Percentage of Invoice Line Price",
						RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new NumericRegistryEditorInfo(5);
					return result;
				});
			}
		}

		public BooleanRegistryItem MAFeBACCaTestMode
		{
			get
			{
				return GetItem("NZMAFeBACCaTestMode", delegate
				{
					return new BooleanRegistryItem(
						"NZMAFeBACCaTestMode",
						Categories.Customs_NewZealand_MAFeBACCa,
						(NoResString)"Set to Test Mode",
						(NoResString)"Should MPI eBACCa messages be sent to the Test System?",
						RegistryStorageFlags.Company, false);
				});
			}
		}

		public CodePairRegistryItem MAFeBACCaSendAcknowledgements
		{
			get
			{
				return GetItem("NZMAFeBACCaSendAcknowledgements", delegate
				{
					return new CodePairRegistryItem(
						"NZMAFeBACCaSendAcknowledgements",
						Categories.Customs_NewZealand_MAFeBACCa,
						(NoResString)"Send Message Acknowledgements",
						(NoResString)"Send message acknowledgements to staff member, nominated group or combination of both",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem MAFeBACCaSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("NZMAFeBACCaSendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NZMAFeBACCaSendAcknowledgementsToGroup",
						Categories.Customs_NewZealand_MAFeBACCa,
						(NoResString)"Send Message Acknowledgements To Group",
						(NoResString)"Send message acknowledgements to selected group",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem MAFeBACCaSendErrors
		{
			get
			{
				return GetItem("NZMAFeBACCaSendErrors", delegate
				{
					return new CodePairRegistryItem(
						"NZMAFeBACCaSendErrors",
						Categories.Customs_NewZealand_MAFeBACCa,
						(NoResString)"Send Message Errors",
						(NoResString)"Send message errors to staff member, nominated group or combination of both",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem MAFeBACCaSendErrorsToGroup
		{
			get
			{
				return GetItem("NZMAFeBACCaSendErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NZMAFeBACCaSendErrorsToGroup",
						Categories.Customs_NewZealand_MAFeBACCa,
						(NoResString)"Send Message Errors To Group",
						(NoResString)"Send message errors to selected group",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem MAFeBACCaSendImpediments
		{
			get
			{
				return GetItem("NZMAFeBACCaSendImpediments", delegate
				{
					return new CodePairRegistryItem(
						"NZMAFeBACCaSendImpediments",
						Categories.Customs_NewZealand_MAFeBACCa,
						(NoResString)"Send Message Impediments",
						(NoResString)"Send message Impediments to staff member, nominated group or combination of both",
						OLookUpEditType.EmailTo,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem MAFeBACCaSendImpedimentsToGroup
		{
			get
			{
				return GetItem("NZMAFeBACCaSendImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"NZMAFeBACCaSendImpedimentsToGroup",
						Categories.Customs_NewZealand_MAFeBACCa,
						(NoResString)"Send Message Impediments To Group",
						(NoResString)"Send message Impediments to selected group",
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Core.Constants.Groups.PostMastersGroupPK);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Registry items converted from deprecated Env.Registry classes

		public StringRegistryItem NZBrokerageID
		{
			get
			{
				return GetItem("NZBrokerageID", delegate
				{
					return new StringRegistryItem("NZBrokerageID"
						, Categories.Customs_NewZealand, (NoResString)"Brokerage ID"
						, (NoResString)"The customs brokerage license number."
						, new UniqueBrokerageIDDataType(CharacterCase.Upper)
						, RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue | RegistryOptions.IsValueMandatory);
				});
			}
		}

		public CodePairRegistryItem DefaultCustomsProcessingPort
		{
			get
			{
				return GetItem("DefaultCustomsProcessingPort", delegate
				{
					return new CodePairRegistryItem("DefaultCustomsProcessingPort"
						, Categories.Customs_NewZealand, (NoResString)"Default Customs Processing Port"
						, (NoResString)"Default Customs Processing Port filled in when creating a new Customs Declaration."
						, OLookUpEditType.NZProcessingPort
						, RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public IntRegistryItem LoadedEDITariffReferenceFilesNZDataVersion
		{
			get
			{
				return GetItem("LoadedEDITariffReferenceFilesNZDataVersion", delegate
				{
					return new IntRegistryItem("LoadedEDITariffReferenceFilesNZDataVersion",
						Categories.Customs_NewZealand,
						(NoResString)"NZ Tariff Data Version",
						(NoResString)"Currently Loaded NZ Tariff Data Version",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue);
				});
			}
		}

		#region Messaging Test Mode Items

		public BooleanRegistryItem ImportDeclarationsTestMode
		{
			get
			{
				return GetItem("NZImportDeclarationsTestMode", delegate
				{
					return new BooleanRegistryItem("NZImportDeclarationsTestMode"
						, Categories.Customs_NewZealand_MessagingModeTesting, (NoResString)"Import Declarations Test Mode"
						, (NoResString)"Should Import Declarations messages be sent to the test rather than production system?"
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, false);
				});
			}
		}

		public BooleanRegistryItem ExportDeclarationsTestMode
		{
			get
			{
				return GetItem("NZExportDeclarationsTestMode", delegate
				{
					return new BooleanRegistryItem("NZExportDeclarationsTestMode"
						, Categories.Customs_NewZealand_MessagingModeTesting, (NoResString)"Export Declarations Test Mode"
						, (NoResString)"Should Export Declarations messages be sent to the test rather than production system?"
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, false);
				});
			}
		}

		public BooleanRegistryItem ImportEciTestMode
		{
			get
			{
				return GetItem("NZImportEciTestMode", delegate
				{
					return new BooleanRegistryItem("NZImportEciTestMode"
						, Categories.Customs_NewZealand_MessagingModeTesting, (NoResString)"Import Write-off Test Mode"
						, (NoResString)"Should Import Write-off messages be sent to the test rather than production system?"
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, false);
				});
			}
		}

		public BooleanRegistryItem ExportEciTestMode
		{
			get
			{
				return GetItem("NZExportEciTestMode", delegate
				{
					return new BooleanRegistryItem("NZExportEciTestMode"
						, Categories.Customs_NewZealand_MessagingModeTesting, (NoResString)"Export Write-off Test Mode"
						, (NoResString)"Should Export Write-off messages be sent to the test rather than production system?"
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, false);
				});
			}
		}

		public BooleanRegistryItem ExportOrnTestMode
		{
			get
			{
				return GetItem("NZExportOrnTestMode", delegate
				{
					return new BooleanRegistryItem("NZExportOrnTestMode"
						, Categories.Customs_NewZealand_MessagingModeTesting, (NoResString)"Export Outward Report Test Mode"
						, (NoResString)"Should Export Write-off messages be sent to the test rather than production system?"
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, false);
				});
			}
		}

		public BooleanRegistryItem ConsolIPITestMode
		{
			get
			{
				return GetItem("NZConsolIPITestMode", delegate
				{
					return new BooleanRegistryItem("NZConsolIPITestMode"
						, Categories.Customs_NewZealand_MessagingModeTesting, (NoResString)"Consol IPI Message Test Mode"
						, (NoResString)"Should Consol IPI messages be sent to the test rather than production system?"
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, false);
				});
			}
		}

		#endregion

		#region Response Message Email Delivery Options

		#region Import Declarations

		public CodePairRegistryItem ImportDeclarationsSendErrors
		{
			get
			{
				return GetItem("NZImportDeclarationsSendErrors", delegate
				{
					return new CodePairRegistryItem("NZImportDeclarationsSendErrors"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportDeclarations, (NoResString)"Send Import Declarations Errors"
						, (NoResString)"This Registry item determines who the recipients are for Import Declarations errors sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ImportDeclarationsSendErrorsToGroup
		{
			get
			{
				return GetItem("NZImportDeclarationsSendErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZImportDeclarationsSendErrorsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportDeclarations, (NoResString)"Group To Send Import Declarations Errors To"
						, (NoResString)"This is the group that Import Declarations errors will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ImportDeclarationsSendAcknowledgements
		{
			get
			{
				return GetItem("NZImportDeclarationsSendAcknowledgements", delegate
				{
					return new CodePairRegistryItem("NZImportDeclarationsSendAcknowledgements"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportDeclarations, (NoResString)"Send Import Declarations Acknowledgements"
						, (NoResString)"This Registry item determines who the recipients are for Import Declarations acknowledgements sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ImportDeclarationsSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("NZImportDeclarationsSendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZImportDeclarationsSendAcknowledgementsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportDeclarations, (NoResString)"Group To Send Import Declarations Acknowledgements To"
						, (NoResString)"This is the group that Import Declarations acknowledgements will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ImportDeclarationsSendImpediments
		{
			get
			{
				return GetItem("NZImportDeclarationsSendImpediments", delegate
				{
					return new CodePairRegistryItem("NZImportDeclarationsSendImpediments"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportDeclarations, (NoResString)"Send Import Declarations Impediments"
						, (NoResString)"This Registry item determines who the recipients are for Import Declarations impediments sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ImportDeclarationsSendImpedimentsToGroup
		{
			get
			{
				return GetItem("NZImportDeclarationsSendImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZImportDeclarationsSendImpedimentsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportDeclarations, (NoResString)"Group To Send Import Declarations Impediments To"
						, (NoResString)"This is the group that Import Declarations impediments will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Export Declarations

		public CodePairRegistryItem ExportDeclarationsSendErrors
		{
			get
			{
				return GetItem("NZExportDeclarationsSendErrors", delegate
				{
					return new CodePairRegistryItem("NZExportDeclarationsSendErrors"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportDeclarations, (NoResString)"Send Export Declarations Errors"
						, (NoResString)"This Registry item determines who the recipients are for Export Declarations errors sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportDeclarationsSendErrorsToGroup
		{
			get
			{
				return GetItem("NZExportDeclarationsSendErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportDeclarationsSendErrorsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportDeclarations, (NoResString)"Group To Send Export Declarations Errors To"
						, (NoResString)"This is the group that Export Declarations errors will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportDeclarationsSendAcknowledgements
		{
			get
			{
				return GetItem("NZExportDeclarationsSendAcknowledgements", delegate
				{
					return new CodePairRegistryItem("NZExportDeclarationsSendAcknowledgements"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportDeclarations, (NoResString)"Send Export Declarations Acknowledgements"
						, (NoResString)"This Registry item determines who the recipients are for Export Declarations acknowledgements sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportDeclarationsSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("NZExportDeclarationsSendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportDeclarationsSendAcknowledgementsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportDeclarations, (NoResString)"Group To Send Export Declarations Acknowledgements To"
						, (NoResString)"This is the group that Export Declarations acknowledgements will be sent to by the Message Processor."
						, RegistryStorageFlags.Company, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportDeclarationsSendImpediments
		{
			get
			{
				return GetItem("NZExportDeclarationsSendImpediments", delegate
				{
					return new CodePairRegistryItem("NZExportDeclarationsSendImpediments"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportDeclarations, (NoResString)"Send Export Declarations Impediments"
						, (NoResString)"This Registry item determines who the recipients are for Export Declarations impediments sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportDeclarationsSendImpedimentsToGroup
		{
			get
			{
				return GetItem("NZExportDeclarationsSendImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportDeclarationsSendImpedimentsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportDeclarations, (NoResString)"Group To Send Export Declarations Impediments To"
						, (NoResString)"This is the group that Export Declarations impediments will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Import ECI

		public CodePairRegistryItem ImportEciSendErrors
		{
			get
			{
				return GetItem("NZImportEciSendErrors", delegate
				{
					return new CodePairRegistryItem("NZImportEciSendErrors"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportWriteoff, (NoResString)"Send Import Write-off Errors"
						, (NoResString)"This Registry item determines who the recipients are for Import Write-off errors sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ImportEciSendErrorsToGroup
		{
			get
			{
				return GetItem("NZImportEciSendErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZImportEciSendErrorsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportWriteoff, (NoResString)"Group To Send Import Write-off Errors To"
						, (NoResString)"This is the group that Import Write-off errors will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ImportEciSendAcknowledgements
		{
			get
			{
				return GetItem("NZImportEciSendAcknowledgements", delegate
				{
					return new CodePairRegistryItem("NZImportEciSendAcknowledgements"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportWriteoff, (NoResString)"Send Import Write-off Acknowledgements"
						, (NoResString)"This Registry item determines who the recipients are for Import Write-off acknowledgements sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ImportEciSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("NZImportEciSendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZImportEciSendAcknowledgementsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportWriteoff, (NoResString)"Group To Send Import Write-off Acknowledgements To"
						, (NoResString)"This is the group that Import Write-off acknowledgements will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ImportEciSendImpediments
		{
			get
			{
				return GetItem("NZImportEciSendImpediments", delegate
				{
					return new CodePairRegistryItem("NZImportEciSendImpediments"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportWriteoff, (NoResString)"Send Import Write-off Impediments"
						, (NoResString)"This Registry item determines who the recipients are for Import Write-off impediments sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ImportEciSendImpedimentsToGroup
		{
			get
			{
				return GetItem("NZImportEciSendImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZImportEciSendImpedimentsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ImportWriteoff, (NoResString)"Group To Send Import Write-off Impediments To"
						, (NoResString)"This is the group that Import Write-off impediments will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Export ECI

		public CodePairRegistryItem ExportEciSendErrors
		{
			get
			{
				return GetItem("NZExportEciSendErrors", delegate
				{
					return new CodePairRegistryItem("NZExportEciSendErrors"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportWriteoff, (NoResString)"Send Export Write-off Errors"
						, (NoResString)"This Registry item determines who the recipients are for Export Write-off errors sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportEciSendErrorsToGroup
		{
			get
			{
				return GetItem("NZExportEciSendErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportEciSendErrorsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportWriteoff, (NoResString)"Group To Send Export Write-off Errors To"
						, (NoResString)"This is the group that Export Write-off errors will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportEciSendAcknowledgements
		{
			get
			{
				return GetItem("NZExportEciSendAcknowledgements", delegate
				{
					return new CodePairRegistryItem("NZExportEciSendAcknowledgements"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportWriteoff, (NoResString)"Send Export Write-off Acknowledgements"
						, (NoResString)"This Registry item determines who the recipients are for Export Write-off acknowledgements sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportEciSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("NZExportEciSendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportEciSendAcknowledgementsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportWriteoff, (NoResString)"Group To Send Export Write-off Acknowledgements To"
						, (NoResString)"This is the group that Export Write-off acknowledgements will be sent to by the Message Processor."
						, RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportEciSendImpediments
		{
			get
			{
				return GetItem("NZExportEciSendImpediments", delegate
				{
					return new CodePairRegistryItem("NZExportEciSendImpediments"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportWriteoff, (NoResString)"Send Export Write-off Impediments"
						, (NoResString)"This Registry item determines who the recipients are for Export Write-off impediments sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportEciSendImpedimentsToGroup
		{
			get
			{
				return GetItem("NZExportEciSendImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportEciSendImpedimentsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportWriteoff, (NoResString)"Group To Send Export Write-off Impediments To"
						, (NoResString)"This is the group that Export Write-off impediments will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Export ORN

		public CodePairRegistryItem ExportOrnSendErrors
		{
			get
			{
				return GetItem("NZExportOrnSendErrors", delegate
				{
					return new CodePairRegistryItem("NZExportOrnSendErrors"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportOutwardReport, (NoResString)"Send Export Outward Report Errors"
						, (NoResString)"This Registry item determines who the recipients are for Export Outward Report errors sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportOrnSendErrorsToGroup
		{
			get
			{
				return GetItem("NZExportOrnSendErrorsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportOrnSendErrorsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportOutwardReport, (NoResString)"Group To Send Export Outward Report Errors To"
						, (NoResString)"This is the group that Export Outward Report errors will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportOrnSendAcknowledgements
		{
			get
			{
				return GetItem("NZExportOrnSendAcknowledgements", delegate
				{
					return new CodePairRegistryItem("NZExportOrnSendAcknowledgements"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportOutwardReport, (NoResString)"Send Export Outward Report Acknowledgements"
						, (NoResString)"This Registry item determines who the recipients are for Export Outward Report acknowledgements sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue,
						Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportOrnSendAcknowledgementsToGroup
		{
			get
			{
				return GetItem("NZExportOrnSendAcknowledgementsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportOrnSendAcknowledgementsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportOutwardReport, (NoResString)"Group To Send Export Outward Report Acknowledgements To"
						, (NoResString)"This is the group that Export Outward Report acknowledgements will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public CodePairRegistryItem ExportOrnSendImpediments
		{
			get
			{
				return GetItem("NZExportOrnSendImpediments", delegate
				{
					return new CodePairRegistryItem("NZExportOrnSendImpediments"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportOutwardReport, (NoResString)"Send Export Outward Report Impediments"
						, (NoResString)"This Registry item determines who the recipients are for Export Outward Report impediments sent by the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.StaffMemberAndNominatedGroup);
				});
			}
		}

		public GuidRegistryItem ExportOrnSendImpedimentsToGroup
		{
			get
			{
				return GetItem("NZExportOrnSendImpedimentsToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("NZExportOrnSendImpedimentsToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_ExportOutwardReport, (NoResString)"Group To Send Export Outward Report Impediments To"
						, (NoResString)"This is the group that Export Outward Report impediments will be sent to by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#region Unsolicited Responses

		public CodePairRegistryItem UnsolicitedDeliveryOrderResponses
		{
			get
			{
				return GetItem("UnsolicitedDeliveryOrderResponses", delegate
				{
					return new CodePairRegistryItem("UnsolicitedDeliveryOrderResponses"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_UnsolicitedResponses, (NoResString)"Send Unsolicited Delivery Order"
						, (NoResString)"This Registry item determines who the recipients are for unsolicited Delivery Orders received in the Message Processor."
						, OLookUpEditType.EmailTo
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue
						, Constants.EmailTo.NoEmails);
				});
			}
		}

		public GuidRegistryItem UnsolicitedDeliveryOrderResponsesSendToGroup
		{
			get
			{
				return GetItem("UnsolicitedDeliveryOrderResponsesSendToGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem("UnsolicitedDeliveryOrderResponsesSendToGroup"
						, Categories.Customs_NewZealand_ResponseMessageDelivery_UnsolicitedResponses, (NoResString)"Group To Send Unsolicited Delivery Orders To"
						, (NoResString)"This is the group that unsolicited Delivery Orders will be sent to when received by the Message Processor."
						, RegistryStorageFlags.Company
						, RegistryOptions.PreserveTestValue);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region INZCustomsDataRegistry Members

		IRegistryItem Integration.Customs.NZ.INZCustomsDataRegistry.UpdateAttachedManifestedECIsWhenConsolDetailsChange
		{
			get { return UpdateAttachedManifestedECIsWhenConsolDetailsChange; }
		}

		IRegistryItem Integration.Customs.NZ.INZCustomsDataRegistry.ExportEntryFeeChargeCode
		{
			get { return ExportEntryFeeChargeCode; }
		}

		#endregion

		#region Trade Single Window

		public IntRegistryItem MaxMessageAttachmentSize
		{
			get
			{
				return GetItem("MaxMessageAttachmentSize", delegate
				{
					return new IntRegistryItem(
						"MaxMessageAttachmentSize",
						Categories.Customs_NewZealand_TradeSingleWindow,
						(NoResString)"Attached documents maximum size",
						(NoResString)"The maximum file size (in bytes) for attached TSW documents.\r\n\r\nThe current maximum size limit allowed by TSW for any any single attachment sent to the NZ Customs System.",
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						10240000);
				});
			}
		}

		#endregion

		#region Manifests
		public BooleanRegistryItem EnableInwardCargoReportManifest
		{
			get
			{
				return GetItem("EnableInwardCargoReportManifest", delegate
				{
					return new BooleanRegistryItem(
						"EnableInwardCargoReportManifest",
						Categories.Customs_NewZealand,
						(NoResString)"Enable Inward Cargo Report Manifest",
						(NoResString)"Enable Inward Cargo Report Manifest.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers | RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		IRegistryItem Integration.Customs.NZ.INZCustomsDataRegistry.EnableInwardCargoReportManifest => EnableInwardCargoReportManifest;

		#endregion

		#region Enable NZ Service Task Check For Sending Message
		public BooleanRegistryItem EnableNZServiceTaskCheckForSendingMessage
		{
			get
			{
				return GetItem("EnableNZServiceTaskCheckForSendingMessage", delegate
				{
					return new BooleanRegistryItem(
						"EnableNZServiceTaskCheckForSendingMessage",
						Categories.Customs_NewZealand,
						(NoResString)"Enable NZ Service Task Check For Sending Message",
						(NoResString)"Enable NZ Service Task Check For Sending Message.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}
		#endregion

		#region NZ Testing / Development

		public BooleanRegistryItem EnableSeaICRFields
		{
			get
			{
				return GetItem("EnableSeaICRFields", delegate
				{
					return new BooleanRegistryItem(
						"EnableSeaICRFields",
						Categories.Customs_NewZealand_Testing,
						ResString.GetMultilingualString("F829E561-AEFE-4BBE-B991-75457F39B594", "Enable new Sea ICR/CRE fields"),
						ResString.GetMultilingualString("76C79D0E-2B0C-498E-99EF-C125FBEC34B2", "This will enable the functionality being created in WI00306302."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableUpdatedMessageSubmissions
		{
			get
			{
				return GetItem("EnableUpdatedMessageSubmissions", delegate
				{
					return new BooleanRegistryItem(
						"EnableUpdatedMessageSubmissions",
						Categories.Customs_NewZealand_Testing,
						ResString.GetMultilingualString("6F7803E1-62B0-44B8-9BF2-9B57272FA047", "Enable Updated Message Submissions"),
						ResString.GetMultilingualString("377201BF-7133-46FF-A8BD-CC22A539AEE1", "Enables the latest message submission features that include the ability to manually amend a message before sending."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						false);
				});
			}
		}

		#endregion
	}
}
