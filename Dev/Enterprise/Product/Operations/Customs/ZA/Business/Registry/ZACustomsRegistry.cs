using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Integration.Customs.ZA;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	public sealed class ZACustomsRegistry : RegistryItemSet, IZACustomsRegistry
	{
		#region Construction

		public static ZACustomsRegistry Instance => instance ?? (instance = new ZACustomsRegistry());

		[ThreadStatic]
		static ZACustomsRegistry instance;

		ZACustomsRegistry()
		{
		}

		protected override void SetDefaultsForNewItem(IRegistryItem item)
		{
			base.SetDefaultsForNewItem(item);
			if (!item.CountryFilterPKs.Any())
			{
				item.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.SouthAfrica;
			}
		}

		#endregion

		public override bool IsForProductivityWise => false;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Customs_SouthAfrica_TestingDevelopment => CombineCategories(Customs_SouthAfrica, ZA.Business.ResString.GetMultilingualString("8F3203D0-EE24-4438-930D-44689B0CB588", "Testing and Development"));
			public static MultilingualString Customs_SouthAfrica_AutomaticSplitEntriesByBondAmount => CombineCategories(Customs_SouthAfrica, ZA.Business.ResString.GetMultilingualString("2E13D356-6185-49BC-8634-D5FBB04129EE", "Automatic Split of Entries by Bond Amount"));
		}

		#endregion

		public BooleanRegistryItem CusCarHabSendPciMarks
		{
			get
			{
				return GetItem("CusCarHabSendPciMarks", delegate
				{
					return new BooleanRegistryItem(
						"CusCarHabSendPciMarks",
						Registry.Business.RatingDataRegistry.Categories.Customs_SouthAfrica,
						(NoResString)"Send package marks in PCI segment of HAB CUSCAR?",
						(NoResString)"Send marks, or suppress them as per specs? Dev only.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		public FinancialAccountNumberPortMapRegistryItem FinancialAccountNumberPortMaps
		{
			get
			{
				return GetItem("ZAFinancialAccountNumberPortMaps", delegate
				{
					return new FinancialAccountNumberPortMapRegistryItem(
						"ZAFinancialAccountNumberPortMaps",
						Registry.Business.RatingDataRegistry.Categories.Customs_SouthAfrica,
						(NoResString)"Financial Account Number Mapping",
						(NoResString)"Enter the organizations that will have EDI profiles that will be used for submitting entries to Customs. Financial Account Numbers (FANs) must be entered for all organizations, Creditor Codes must be entered for all Customs offices where the account is to be paid by the agent. If the account is to be paid by the importer then the Importer Pays box must be checked and the Creditor Code cannot be entered.",
						RegistryStorageFlags.Company);
				});
			}
		}

		public IRegistryItem WarehouseOperatorTransactionsModuleEnabled
		{
			get
			{
				return GetItem("WarehouseOperatorTransactionsModuleEnabled", () => new BooleanRegistryItem("WarehouseOperatorTransactionsModuleEnabled",
					Categories.Customs_SouthAfrica,
					ZA.Business.ResString.GetMultilingualString("6C71DE8C-62AF-4014-8272-6BDB917ED12A", "Warehouse Operator Transactions Module enabled"),
					ZA.Business.ResString.GetMultilingualString("C6514EC6-FF2F-4CF1-8945-25877FED0D06", "Override the Default value to enable the Warehouse Operator Transactions Module."),
					RegistryStorageFlags.Company,
					false)
				);
			}
		}

		public CPCAcquitByDateRegistryItem CPCAcquitByDate
		{
			get
			{
				return GetItem("ZACPCAcquitByDate", delegate
				{
					return new CPCAcquitByDateRegistryItem(
						"ZACPCAcquitByDate",
						Registry.Business.RatingDataRegistry.Categories.Customs_SouthAfrica,
						(NoResString)"CPC Acquit By Date",
						(NoResString)@"This Registry setting is used to define the default Acquit By Date for all types of ZA Brokerage Jobs 

This date will output to the Acquit by Date: field on the Entries > Messages > Entry Details sub tab of ZA Brokerage jobs

The Acquit By Date: field remains editable in the Brokerage job so that default dates can be overwritten when necessary. Eg Extension of deadlines granted to bonded goods in Customs Warehouses",
						RegistryStorageFlags.Company);
				});
			}
		}

		public IntRegistryItem MessageSendingInterval
		{
			get
			{
				return GetItem("ZAMessageSendingInterval", delegate
				{
					return new IntRegistryItem(
						"ZAMessageSendingInterval",
						Categories.Customs_SouthAfrica,
						(NoResString)"Message Sending Interval",
						(NoResString)@"The minimum time (in Minutes) between sending two messages for a single Entry.
If '0' is specified, then there will be no check between message sending.",
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						5, ZInt.Zero, int.MaxValue);
				});
			}
		}

		//legacy - to be removed after no more interfaced customers
		public CustomsDSBCreditorOverrideRegistryItem DSBCreditors
		{
			get
			{
				return GetItem("DSBCreditorsOverrideZA", delegate
				{
					return new CustomsDSBCreditorOverrideRegistryItem(
						"DSBCreditorsOverrideZA",
						Registry.Business.RatingDataRegistry.Categories.AutoRating_ChargeCodes_Customs_SouthAfrica,
						(NoResString)"Customs Disbursement Creditor Mapping",
						(NoResString)"For Interfaced Customers only: Mappings between district office code and customs disbursement creditor. If no override exists for a particular district office code, then AutoRating > Charge Codes > Customs > Disbursement Creditor is used.");
				});
			}
		}

		public GuidRegistryItem CustomsOfficeCode
		{
			get
			{
				return GetItem("ZACustomsOfficeCode", delegate
				{
					var result = new GuidRegistryItem(
						(NoResString)"ZACustomsOfficeCode",
						Categories.Customs_SouthAfrica,
						(NoResString)"Customs Office Code",
						(NoResString)"The default Customs Office Code for a branch",
						new ZACustomsOfficeDataType(),
						RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.ZACustomsOffice);
					return result;
				});
			}
		}

		public GuidRegistryItem FallbackNotificationGroup
		{
			get
			{
				return GetItem("ZAFallbackNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						(NoResString)"ZAFallbackNotificationGroup",
						Categories.Customs_SouthAfrica,
						ZA.Business.ResString.GetMultilingualString("6e6370bf-4d17-422a-8acc-7ed3d1495132", "Fallback Notification Group"),
						ZA.Business.ResString.GetMultilingualString("84175fe3-df1e-4610-8918-7696d3bd5bbf", "The group of users who will be receiving notifications about South Africa Responses."),
						RegistryStorageFlags.Branch | RegistryStorageFlags.BranchDepartment,
						RegistryConstants.GroupPKs.Notification);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public AutomaticDeferredSelectionRegistryItem AutomaticDeferredSelection
		{
			get
			{
				return GetItem("ZAAutomaticDeferredSelection", delegate
				{
					return new AutomaticDeferredSelectionRegistryItem(
						"ZAAutomaticDeferredSelection",
						Registry.Business.RatingDataRegistry.Categories.Customs_SouthAfrica,
						(NoResString)"Automatic Deferred Selection",
						(NoResString)"Automatic Deferred Selection",
						RegistryStorageFlags.Company);
				});
			}
		}

		public BillCustomisationRegistryItem ZAOutturnGateInOutJobNumberCustomization
		{
			get
			{
				return GetItem(
					"ZAOutturnGateInOutJobNumberCustomization", () => new BillCustomisationRegistryItem("ZAOutturnGateInOutJobNumberCustomization",
						Categories.Customs_SouthAfrica,
						ZA.Business.ResString.GetMultilingualString("7E52D422-2F30-4A5E-BFAE-605F6F224BF7", "Outturn & Gate In/Out Job number Customization"),
						ZA.Business.ResString.GetMultilingualString("EAC36087-CF0B-462A-B636-B5064B8CB402", "Override this value to customize how Outturn & Gate In/Out Job Numbers are formatted"),
						RegistryStorageFlags.All,
						new ZAOutturnGateInOutJobNumberCustomisationRegistryDataType()));
			}
		}

		public BooleanRegistryItem DutyFreeGoodsIntoBondedWarehouse
		{
			get
			{
				return GetItem("ZADutyFreeGoodsIntoBondedWarehouse", () => new BooleanRegistryItem("ZADutyFreeGoodsIntoBondedWarehouse",
					Categories.Customs_SouthAfrica,
					ZA.Business.ResString.GetMultilingualString("d06d73b2-03ba-4512-b3bd-309624fcb5e1", "Duty Free Goods into Bonded Warehouse"),
					ZA.Business.ResString.GetMultilingualString("32c7ce4e-fc40-4cd8-9069-c72df6f16c9e", "The default is that a Warning will be displayed if a Duty Free Tariff is captured on an invoice line when doing an 'Into Warehouse' entry on a Brokerage Job.\nSelect the Override to change the Warning to an Error message."),
					RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
					false
				));
			}
		}

		public SADDocumentWatermarkRegistryItem SADDocumentPackWatermarks
		{
			get
			{
				return GetItem("ZASADDocumentWatermarks", delegate
				{
					return new SADDocumentWatermarkRegistryItem(
						"ZASADDocumentWatermarks",
						Registry.Business.RatingDataRegistry.Categories.Customs_SouthAfrica,
						(NoResString)"SAD Document Pack Watermark",
						(NoResString)"Activate this Registry Setting to print a Watermark on the SAD500 Document for the status codes selected in the Status Code Grid.\r\nIf the Text Box is blank, the Status Code Description will be used as the Watermark text",
						RegistryStorageFlags.Branch);
				});
			}
		}

		public BooleanRegistryItem AllowAutomaticSplitEntriesByBondAmount
		{
			get
			{
				return GetItem("ZAAllowAutomaticSplitEntriesByBondAmount", () => new BooleanRegistryItem("ZAAllowAutomaticSplitEntriesByBondAmount",
					Categories.Customs_SouthAfrica_AutomaticSplitEntriesByBondAmount,
					ZA.Business.ResString.GetMultilingualString("7E1D491F-18D2-404E-8DEE-0BF25F90194E", "Allow automatic split of entries by bond amount"),
					ZA.Business.ResString.GetMultilingualString("564D4755-33EF-4E9A-B870-9FBE930B58CA", "On merging this will enable the automatic splitting of entries based on the Bond Holder's bond amount."),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					false)
				);
			}
		}

		public IntRegistryItem PercentageOfBondAmountUsedBeforeSplit
		{
			get
			{
				return GetItem("ZAPercentageOfBondAmountUsedBeforeSplit", () => new IntRegistryItem("ZAPercentageOfBondAmountUsedBeforeSplit",
					Categories.Customs_SouthAfrica_AutomaticSplitEntriesByBondAmount,
					ZA.Business.ResString.GetMultilingualString("82F63A1F-3288-44A2-9094-E481BC033E37", "% of bond amount to be used"),
					ZA.Business.ResString.GetMultilingualString("52C2F9D4-E7B0-4D5E-9AB3-F5265507C51D", "This value will determine what % of the bond amount will be used before we split an entry into a new one."),
					new NumericRegistryEditorInfo(0),
					RegistryStorageFlags.Company,
					RegistryOptions.Default,
					90,
					50,
					100)
				);
			}
		}

		public IntRegistryItem ExbondMaxNumberEntryLines => GetItem("ExbondMaxNumberEntryLines", () => new IntRegistryItem("ExbondMaxNumberEntryLines",
			RatingDataRegistry.Categories.Customs_SouthAfrica,
			ZA.Business.ResString.GetMultilingualString("E9B34B2A-0860-4BF5-9114-6FE2E3615E7E", "Ex-bond Max Number of Entry Lines"),
			ZA.Business.ResString.GetMultilingualString("392945DE-FCB1-4983-A9D9-5ADE96F5802B", "Override the Default to set the 'Max Entry Lines' allowed per 'Ex-Bond Entry/Entry Instruction'. When the value is '0', then the current maximum of 9 999 lines will apply."),
			RegistryStorageFlags.Company,
			RegistryOptions.Default,
			0, 0, 9999));

		public IntRegistryItem ExbondMaxNumberJobInvoiceLines => GetItem("ExbondMaxNumberJobInvoiceLines", () => new IntRegistryItem("ExbondMaxNumberJobInvoiceLines",
			RatingDataRegistry.Categories.Customs_SouthAfrica,
			ZA.Business.ResString.GetMultilingualString("C0662983-9C01-4523-A71B-F429995581C7", "Ex-bond Max Number of Job Invoice Lines"),
			ZA.Business.ResString.GetMultilingualString("7336843E-2CA1-4683-BD53-FC910B349F11", "Override the Default to set the 'Max Invoice Lines' allowed per 'Ex-Bond' job. When the value is '0', then no maximum will apply."),
			RegistryStorageFlags.Company,
			RegistryOptions.Default,
			0, 0, int.MaxValue));

		public IntRegistryItem MaxNumberLinesPerHomeConsumptionFile => GetItem("MaxNumberLinesPerHomeConsumptionFile", () => new IntRegistryItem("MaxNumberLinesPerHomeConsumptionFile",
			RatingDataRegistry.Categories.Customs_SouthAfrica,
			ZA.Business.ResString.GetMultilingualString("321B1969-241F-46AC-9DC1-15E366383309", "Max No of Lines for WOT Home Consumption file"),
			ZA.Business.ResString.GetMultilingualString("D0E7345A-3A5C-4E8D-9A1E-114B0BE705A5", "Override the Default to set the 'Max number of Lines' allowed per WOT Home Consumption file. When the value is '0', then no maximum will apply."),
			RegistryStorageFlags.Company,
			RegistryOptions.Default,
			0, 0, 9999));

		public IntRegistryItem WOTReceiptsMatchingPeriod => GetItem("WOTReceiptsMatchingPeriod", () => new IntRegistryItem("WOTReceiptsMatchingPeriod",
			RatingDataRegistry.Categories.Customs_SouthAfrica,
			ZA.Business.ResString.GetMultilingualString("9040C2B4-564D-428A-86F0-BF776A7B5961", "Number of days to look back for matching receipts"),
			ZA.Business.ResString.GetMultilingualString("BDE8067D-CBC9-4FAF-80CE-E1B81F14C506", "Override the Default to set the 'Number of days' to look back for matching receipts. When the value is '0', there will be no restriction."),
			RegistryStorageFlags.Company,
			RegistryOptions.Default,
			120, 0, 730));

		public BooleanRegistryItem AllowReversalOfCusWarehouseBatch => GetItem("AllowReversalOfCusWarehouseBatch", () => new BooleanRegistryItem("AllowReversalOfCusWarehouseBatch",
			Categories.Customs_SouthAfrica,
			ZA.Business.ResString.GetMultilingualString("B6948CF7-F8A3-4406-8290-485AC74481F7", "Allow Reversal of Cus Warehouse Batch"),
			ZA.Business.ResString.GetMultilingualString("F0D2429E-1625-4812-B640-4F01C4E32175", "Allow reversal of Cus Warehouse Batch due to being added in error."),
			RegistryStorageFlags.Company,
			false
		));

		#region DefaultBranchFroManifestSubmission
		public CodePairRegistryItem DefaultBranchForManifestSubmission
		{
			get
			{
				return GetItem("DefaultBranchForManifestSubmission", delegate
				{
					var result = new CodePairRegistryItem(
						"DefaultBranchForManifestSubmission",
						Categories.Customs_SouthAfrica,
						ZA.Business.ResString.GetMultilingualString("0C4D8CCA-5B04-48D4-9872-E9B39992182D", "Default Branch for Manifest Submission"),
						ZA.Business.ResString.GetMultilingualString("FD2376D1-B1AD-4746-916D-9E780536596E", "Insert the ZA Branch Code that has a ZA Customs Manifest EDI Profile.\r\nSetting this registry item will allow ZA Manifest Messages to be submitted to ZA Customs from a Non ZA Company using the ZA Customs Manifest EDI Profile of the selected branch."),
						new CodeDescriptionPairListProvider(() => ZaBranchCodesForRegistry()),
						RegistryStorageFlags.Company);

					result.CountryFilterPKs = ActiveCompaniesCountriesExcludingSouthAfrica;
					return result;
				});
			}
		}

		internal static CodeDescriptionPairList ZaBranchCodesForRegistry(BusinessObjectFactory factory = null)
		{
			var result = new CodeDescriptionPairList();

			var companies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.SouthAfrica, factory);
			var branches = companies.SelectMany(x => x.Branches);
			foreach (var branch in branches)
			{
				var cusCodes = GetActiveAgentOrgCusCodes(branch);
				if (cusCodes.Any())
				{
					var agentCode = cusCodes.FirstOrDefault().OK_CustomsRegNo;

					var code = branch.GB_Code;
					var description = branch.GB_BranchName + "(" + agentCode + ", " + branch.Company.GC_Code + ")";
					result.AddPairIfNotExist(code, description);
				}
			}

			return result;
		}

		public static OrgCusCode[] GetActiveAgentOrgCusCodes(GlbBranch branch)
		{
			OrgCusCode[] result = Array.Empty<OrgCusCode>();
			var proxy = branch.OrgProxy;
			if (proxy != null)
			{
				result = proxy.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.CodeTypes.AgentCode, Core.Constants.CountryCodes.SouthAfrica);
			}
			return result;
		}
		#endregion

		public IEnumerable<Guid> ActiveCompaniesCountriesExcludingSouthAfrica
		{
			get
			{
				if (activeCompaniesCountriesExcludingSouthAfrica == null)
				{
					activeCompaniesCountriesExcludingSouthAfrica = CustomsDataRegistry.Instance.GetActiveCompaniesCountriesExcluding(Core.Constants.CountryCodes.SouthAfrica);
				}
				return activeCompaniesCountriesExcludingSouthAfrica;
			}
		}
		IEnumerable<Guid> activeCompaniesCountriesExcludingSouthAfrica;

		#region IZACustomsRegistry

		ICodeDescriptionPairList IZACustomsRegistry.FANList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				foreach (var map in FinancialAccountNumberPortMaps.Value.Cast<FinancialAccountNumberPortMap>().Where(map => !map.ImporterPays && !map.Cash))
				{
					var customsOffice = map.CustomsOfficeCode;
					var codeDescription = map.CustomsOfficeCodeList.GetDescriptionFromCode(customsOffice);
					if (codeDescription != null)
					{
						customsOffice = codeDescription;
					}

					list.AddPair(map.FinancialAccountNumber, customsOffice);
				}

				return list;
			}
		}

		(ZGuid CompanyPK, ZGuid OrgPK, ZString FAN)[] IZACustomsRegistry.GetFANsForOrgs(ZGuid[] orgPKs)
		{
			var companyOrgFans = new List<(ZGuid, ZGuid, ZString)>();

			foreach (var company in GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.SouthAfrica))
			{
				var mappings = FinancialAccountNumberPortMaps.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				companyOrgFans.AddRange(mappings.Cast<FinancialAccountNumberPortMap>()
												.Where(m => orgPKs.Contains(m.OrganizationPK))
												.Select(m => (company.PK, m.OrganizationPK, m.FinancialAccountNumber)));
			}

			return companyOrgFans.ToArray();
		}

		bool IZACustomsRegistry.IsWarehouseOperatorTransactionsModuleEnabled => ((BooleanRegistryItem)WarehouseOperatorTransactionsModuleEnabled).Value;

		#endregion
	}
}
