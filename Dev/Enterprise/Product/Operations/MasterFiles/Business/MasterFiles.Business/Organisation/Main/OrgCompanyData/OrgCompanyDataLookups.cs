using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCompanyDataLookups : AutoOrgCompanyDataLookups
	{
		public OrgCompanyDataLookups(AutoOrgCompanyData parent)
			: base(parent)
		{
		}

		AutoOrgCompanyData ParentCompanyData
		{
			get { return (AutoOrgCompanyData)Parent; }
		}

		#region AP Bank Accounts

		public override AccBankAccountCollection APDefaultBankAccounts
		{
			get
			{
				if (fBankAccounts == null)
				{
					GlbBranch branch = (GlbBranch)Factory.Load(typeof(GlbBranch), GlbBranch.CurrentBranch.PK);
					fBankAccounts = new AccBankAccountCollection(Factory, branch);
				}
				return fBankAccounts;
			}
		}
		AccBankAccountCollection fBankAccounts;

		#endregion

		#region AP Payment Terms

		public CodeDescriptionPairList OB_APPaymentTerms_List
		{
			get
			{
				if (ob_APPaymentTerms_List_cachedValue == null)
				{
					ob_APPaymentTerms_List_cachedValue = new CachedProperty<CodeDescriptionPairList>(Factory, delegate
						{
							CodeDescriptionPairList invoiceTermsList = new CodeDescriptionPairList(OB_APPaymentTerms_ListWithoutDefaultValue);
							string settlementGroupTerm = string.Empty;
							if (ParentCompanyData.Header != null && ParentCompanyData.Validation.CanDefaultAPTermBeSet)
							{
								string apTermAsString = ParentCompanyData.Header.APSettlementGroup.CompanyData.GetAPTermWithoutFallback().ToString();
								if (!string.IsNullOrEmpty(apTermAsString))
								{
									settlementGroupTerm = string.Format(" ({0})", apTermAsString);
								}
							}
							invoiceTermsList.Insert(0, new CodeDescriptionPair(DefaultInvoiceTerm.Code, DefaultInvoiceTerm.Description + settlementGroupTerm));
							return invoiceTermsList;
						});
				}
				return ob_APPaymentTerms_List_cachedValue.Value;
			}
		}
		CachedProperty<CodeDescriptionPairList> ob_APPaymentTerms_List_cachedValue;

		public CodeDescriptionPairList OB_APPaymentTerms_ListWithoutDefaultValue
		{
			get { return OB_APPaymentTerms_ListWithoutDefaultValue_cachedValue ?? (OB_APPaymentTerms_ListWithoutDefaultValue_cachedValue = new APInvoiceTermsList()); }
		}
		CodeDescriptionPairList OB_APPaymentTerms_ListWithoutDefaultValue_cachedValue;

		public static readonly CodeDescriptionPair DefaultInvoiceTerm = new CodeDescriptionPair("DEF", ResString.GetMultilingualString("522f8fec-0b74-40b4-bfe6-249b82e98de4", "Default from AP Settlement Group"));

		#endregion

		#region AP Categories

		ReadOnlyCodeDescriptionPairList fOB_APCategory_List;
		public ReadOnlyCodeDescriptionPairList OB_APCategory_List
		{
			get
			{
				if (fOB_APCategory_List == null)
				{
					fOB_APCategory_List = Env.Registry.PayablesCategoryList;
				}
				return fOB_APCategory_List;
			}
		}

		#endregion

		#region AP Charge Codes

		public override AccChargeCodeCollection APDefaultChargeCodes
		{
			get { return new AccChargeCodeCollection(Factory, new ZQuery(), GlbCompany.CurrentCompany.PK.ToGuid()); }
		}

		#endregion

		#region AR Consolidated Account Categories

		ReadOnlyCodeDescriptionPairList fOB_ARConsolidatedAccountingCategory_List;
		public ReadOnlyCodeDescriptionPairList OB_ARConsolidatedAccountingCategory_List => fOB_ARConsolidatedAccountingCategory_List ?? (fOB_ARConsolidatedAccountingCategory_List = AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.ToReadOnlyCodeDescriptionPairList());

		#endregion

		#region AR Categories

		ReadOnlyCodeDescriptionPairList fOB_ARCategory_List;
		public ReadOnlyCodeDescriptionPairList OB_ARCategory_List
		{
			get
			{
				if (fOB_ARCategory_List == null)
				{
					fOB_ARCategory_List = Env.Registry.ReceivablesCategoryList;
				}
				return fOB_ARCategory_List;
			}
		}

		#endregion

		#region AR Credit Ratings

		ReadOnlyCodeDescriptionPairList fOB_ARCreditRating_List;
		public ReadOnlyCodeDescriptionPairList OB_ARCreditRating_List
		{
			get
			{
				if (fOB_ARCreditRating_List == null)
				{
					fOB_ARCreditRating_List = Env.Registry.ARCreditRatingList;
				}
				return fOB_ARCreditRating_List;
			}
		}

		#endregion

		#region ARPayToAccounts

		public override AccBankAccountCollection ARPayToAccounts
		{
			get { return new AccBankAccountCollection(Factory, GlbCompany.CurrentCompany); }
		}
		#endregion

		#region Company Tariffs

		public CodeDescriptionPairList GetCompanyTransportModes(ZString tariffType)
		{
			return CachedOrgCodeLists.GetCompanyTransportModes(Factory, ((OrgCompanyData)Parent).OB_GC, tariffType);
		}

		public CodeDescriptionPairList GetApplicableDirections(ZString tariffType)
		{
			return CachedOrgCodeLists.GetApplicableDirections(Factory, ((OrgCompanyData)Parent).OB_GC, tariffType);
		}

		public CodeDescriptionPairList CompanyTariffLevels
		{
			get { return CachedOrgCodeLists.CompanyTariffLevels_List(Factory, ((OrgCompanyData)Parent).OB_GC); }
		}

		OrgCodeLists CachedOrgCodeLists
		{
			get { return cachedOrgCodeLists ?? (cachedOrgCodeLists = new OrgCodeLists()); }
		}

		OrgCodeLists cachedOrgCodeLists;

		#endregion

		#region Rate Security Groups

		public ReadOnlyCodeDescriptionPairList OB_RateSecurityGroup_List
		{
			get
			{
				if (ob_RateSecurityGroup_List == null)
				{
					ob_RateSecurityGroup_List = new ReadOnlyCodeDescriptionPairList(OrganisationsDataRegistry.Instance.RatesSecurity.Value);
				}

				return ob_RateSecurityGroup_List;
			}
		}
		ReadOnlyCodeDescriptionPairList ob_RateSecurityGroup_List;

		#endregion

		#region Warehouse Rating Periods

		public CodeDescriptionPairList WarehouseRatingPeriods
		{
			get
			{
				if (fWarehouseRatingPeriods == null)
				{
					fWarehouseRatingPeriods = new CodeDescriptionPairList();
					CodeDescriptionPairList baseList = new CodeDescriptionPairList(OLookUpEditType.StorageCalculationPeriod);
					fWarehouseRatingPeriods.AddPair(Core.Constants.StorageCalculationPeriods.Default, Res.GetString("dfc08eb1-4562-4460-bd45-87e5b26b5552", "Default from Registry -") + " " + baseList.GetDescriptionFromCode(Env.Registry.Rating.StorageCalculationPeriod));
					fWarehouseRatingPeriods.AddRange(baseList);
				}

				return fWarehouseRatingPeriods;
			}
		}

		CodeDescriptionPairList fWarehouseRatingPeriods;

		#endregion

		#region Warehouse Storage Calculation Methods

		public const string WarehouseStorageMax = "MAX";
		public const string WarehouseStoragePeak = "PEK";
		public const string WarehouseStorageClosingBalance = "CLO";
		public const string WarehouseSplitPeriodBilling = "SPL";

		public CodeDescriptionPairList WarehouseStorageCalculationMethods
		{
			get
			{
				if (fWarehouseStorageCalculationMethods == null)
				{
					fWarehouseStorageCalculationMethods = new CodeDescriptionPairList();
					fWarehouseStorageCalculationMethods.AddPair(WarehouseStorageMax, Res.GetString("775b13f8-050c-4846-9230-5c05651adbf2", "Balance Brought Forward + Receive Movements (Part There Of Method)"));
					fWarehouseStorageCalculationMethods.AddPair(WarehouseStoragePeak, Res.GetString("4f1546cf-19ca-4195-b12f-1e989c799d53", "Peak Number of Units - Receipts and Releases"));
					fWarehouseStorageCalculationMethods.AddPair(WarehouseStorageClosingBalance, Res.GetString("a87b4403-5660-4798-b7e5-c285d5bc344c", "Closing Balance"));
					fWarehouseStorageCalculationMethods.AddPair(WarehouseSplitPeriodBilling, Res.GetString("484ce87b-3670-48d3-b26d-f075fca0694a", "Split Period Billing"));
				}

				return fWarehouseStorageCalculationMethods;
			}
		}

		CodeDescriptionPairList fWarehouseStorageCalculationMethods;

		#endregion

		#region Yard Storage Calculation Methods

		public const string YardStorageMethodStandard = "STD";
		public const string YardStorageMethodYardOut = "OUT";

		public CodeDescriptionPairList YardStorageCalculationMethods
		{
			get
			{
				if (yardStorageCalculationMethods == null)
				{
					yardStorageCalculationMethods = new CodeDescriptionPairList();
					yardStorageCalculationMethods.AddPair(YardStorageMethodStandard, Res.GetString("9bd99c7c-b51b-46ac-a912-888a9899c296", "Standard"));
					yardStorageCalculationMethods.AddPair(YardStorageMethodYardOut, Res.GetString("8df35c06-abe4-4805-9c68-00aa90574197", "Yard Out"));
				}

				return yardStorageCalculationMethods;
			}
		}

		CodeDescriptionPairList yardStorageCalculationMethods;

		#endregion

		#region Buyers Consol Invoicing Styles

		public CodeDescriptionPairList BuyersConsolInvoicingStyles
		{
			get
			{
				if (fBuyersConsolInvoicingStyles == null)
				{
					fBuyersConsolInvoicingStyles = new BuyersConsolInvoicingStyleList(true);
				}
				return fBuyersConsolInvoicingStyles;
			}
		}

		CodeDescriptionPairList fBuyersConsolInvoicingStyles;

		public class BuyersConsolInvoicingStyleList : CodeDescriptionPairList
		{
			public BuyersConsolInvoicingStyleList(bool includeDefault)
				: base()
			{
				if (includeDefault)
				{
					string defaultFromReg = new BuyersConsolInvoicingStyleList(false).GetDescriptionFromCode(OrganisationsDataRegistry.Instance.BuyersConsolInvoicingStyle.Value);
					AddPair(Default, Res.GetString("231dd06c-68a8-4583-a25e-462703c1e4b5", "Default from Registry - {0}", defaultFromReg));
				}
				AddRange(new CodeDescriptionPairList(OLookUpEditType.ConsolInvoicingStyles));
			}

			public const string Default = "DEF";
		}

		#endregion

		#region Shippers Consol Invoicing Styles

		public CodeDescriptionPairList ShippersConsolInvoicingStyles
		{
			get
			{
				if (fShippersConsolInvoicingStyles == null)
				{
					fShippersConsolInvoicingStyles = new ShippersConsolInvoicingStyleList(true);
				}
				return fBuyersConsolInvoicingStyles;
			}
		}

		CodeDescriptionPairList fShippersConsolInvoicingStyles;

		public class ShippersConsolInvoicingStyleList : CodeDescriptionPairList
		{
			public ShippersConsolInvoicingStyleList(bool includeDefault)
				: base()
			{
				if (includeDefault)
				{
					var defaultFromRegistry = new ShippersConsolInvoicingStyleList(false).GetDescriptionFromCode(OrganisationsDataRegistry.Instance.ShippersConsolInvoicingStyle.Value);
					AddPair(Default, Res.GetString("68336b6a-e730-4bc6-81cf-e02b9909d887", "Default from Registry - {0}", defaultFromRegistry));
				}
				AddRange(new CodeDescriptionPairList(OLookUpEditType.ConsolInvoicingStyles));
			}

			public const string Default = "DEF";
		}

		#endregion

		#region Credit Card Details

		public ReadOnlyCodeDescriptionPairList OB_ARCreditCardType_List
		{
			get
			{
				if (fOB_ARCreditCardType_List == null)
				{
					fOB_ARCreditCardType_List = new UntranslatableCodeDescriptionPairList((NoResString)"Credit card names are in English only");
					((CodeDescriptionPairList)fOB_ARCreditCardType_List).AddRange(new CreditCardTypeList());
				}

				return fOB_ARCreditCardType_List;
			}
		}
		ReadOnlyCodeDescriptionPairList fOB_ARCreditCardType_List;

		public ReadOnlyCodeDescriptionPairList OB_ARCreditCardExpire_Month_List
		{
			get
			{
				if (fOB_ARCreditCardExpire_Month_List == null)
				{
					fOB_ARCreditCardExpire_Month_List = new CodeDescriptionPairList();
					fOB_ARCreditCardExpire_Month_List.AddPair("__");
					fOB_ARCreditCardExpire_Month_List.AddPair("01");
					fOB_ARCreditCardExpire_Month_List.AddPair("02");
					fOB_ARCreditCardExpire_Month_List.AddPair("03");
					fOB_ARCreditCardExpire_Month_List.AddPair("04");
					fOB_ARCreditCardExpire_Month_List.AddPair("05");
					fOB_ARCreditCardExpire_Month_List.AddPair("06");
					fOB_ARCreditCardExpire_Month_List.AddPair("07");
					fOB_ARCreditCardExpire_Month_List.AddPair("08");
					fOB_ARCreditCardExpire_Month_List.AddPair("09");
					fOB_ARCreditCardExpire_Month_List.AddPair("10");
					fOB_ARCreditCardExpire_Month_List.AddPair("11");
					fOB_ARCreditCardExpire_Month_List.AddPair("12");
				}

				return fOB_ARCreditCardExpire_Month_List;
			}
		}
		CodeDescriptionPairList fOB_ARCreditCardExpire_Month_List;

		public ReadOnlyCodeDescriptionPairList OB_ARCreditCardExpire_Year_List
		{
			get
			{
				if (fOB_ARCreditCardExpire_Year_List == null)
				{
					fOB_ARCreditCardExpire_Year_List = new CodeDescriptionPairList();
					fOB_ARCreditCardExpire_Year_List.AddPair("__");
					for (int i = ZDateTime.Now.Year - 2000; i < ZDateTime.Now.Year - 2000 + 20; i++)
					{
						fOB_ARCreditCardExpire_Year_List.AddPair(i.ToString().Length != 2 ? "0" + i : i.ToString());
					}
				}

				return fOB_ARCreditCardExpire_Year_List;
			}
		}
		CodeDescriptionPairList fOB_ARCreditCardExpire_Year_List;

		#endregion

		#region Transaction Creation Constraint

		public CodeDescriptionPairList OB_APTransactionCreationRestrictionList => cached_OB_APTransactionCreationRestrictionList ?? (cached_OB_APTransactionCreationRestrictionList = new TransactionCreationRestrictionList());
		CodeDescriptionPairList cached_OB_APTransactionCreationRestrictionList;

		public CodeDescriptionPairList OB_ARTransactionCreationRestrictionList => cached_OB_ARTransactionCreationRestrictionList ?? (cached_OB_ARTransactionCreationRestrictionList = new TransactionCreationRestrictionList());
		CodeDescriptionPairList cached_OB_ARTransactionCreationRestrictionList;

		#endregion

		#region Tax Configuration Template

		public override AccOrgTaxConfigurationTemplateCollection ARTaxTemplates
		{
			get { return new OrgCompanyDataTaxConfigurationTemplateCollection(Factory, true); }
		}

		public override AccOrgTaxConfigurationTemplateCollection APTaxTemplates
		{
			get { return new OrgCompanyDataTaxConfigurationTemplateCollection(Factory, false); }
		}

		#endregion

		#region Goods Ownership List

		public CodeDescriptionPairList OB_GoodsOwnership_List
		{
			get
			{
				if (cached_OB_GoodsOwnership_List == null)
				{
					cached_OB_GoodsOwnership_List = new CodeDescriptionPairList();
					cached_OB_GoodsOwnership_List.AddPair(GoodsOwnership.NeverOwner, ResString.GetMultilingualString("02B32332-D3AE-4AC3-B383-543C35B43510", "Never Owner"));
				}

				return cached_OB_GoodsOwnership_List;
			}
		}
		CodeDescriptionPairList cached_OB_GoodsOwnership_List;

		public static class GoodsOwnership
		{
			public const string NeverOwner = "NVR";
		}

		#endregion

		public CodeDescriptionPairList OB_CusPaidByList => Factory.GetCachedValue<PaidByCodeList>();

		public CodeDescriptionPairList OB_VATConfigList
		{
			get { return Factory.GetCachedValue<CodeDescriptionPairList>("OrgCompanyDataLookups.OB_VATConfig", delegate { return new AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes(); }); }
		}

		public CodeDescriptionPairList OB_ARCreateVATComplianceDocumentOnPostingList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("OrgCompanyDataLookups.OB_ARCreateVATComplianceDocumentOnPosting", delegate
			{
				var arCreateVATComplianceDocumentOnPostingList = new AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes();
				arCreateVATComplianceDocumentOnPostingList.RemoveCode(Constants.OrganisationCreateComplianceDocumentOnPostingTypes.PerComplianceDocumentNumber);
				return arCreateVATComplianceDocumentOnPostingList;
			});
			}
		}

		public CodeDescriptionPairList OB_APCreateVATComplianceDocumentOnPostingList
		{
			get
			{
				return Factory.GetCachedValue<CodeDescriptionPairList>("OrgCompanyDataLookups.OB_APCreateVATComplianceDocumentOnPosting", delegate
				{
					return new AccountingMasterFilesConstants.OrganisationCreateComplianceDocumentOnPostingTypes();
				});
			}
		}
	}
}
