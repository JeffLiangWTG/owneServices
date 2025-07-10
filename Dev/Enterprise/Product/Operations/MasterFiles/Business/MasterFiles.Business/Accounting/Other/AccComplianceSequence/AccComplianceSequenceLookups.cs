using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccComplianceSequenceLookups
//
//    This class should be used for overriding collections in AutoAccComplianceSequenceLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	public class AccComplianceSequenceLookups : AutoAccComplianceSequenceLookups
	{
		public AccComplianceSequenceLookups(AutoAccComplianceSequence parent)
			: base(parent)
		{
		}

		public BusinessObjectCollection Printers
		{
			get
			{
				if (printers == null)
				{
					ZQuery filter = new ZQuery(StmPrintQueueSchema.SQ_AllowPrinting, ZBool.True);
					BusinessObjectCollection printQueueCollection = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueueCollection>(), new object[] { Factory, filter });
					printQueueCollection.Sort(new SortInfo(StmPrintQueueSchema.SQ_DisplayName.Name, ListSortDirection.Ascending));
					printers = printQueueCollection;
				}
				return printers;
			}
		}
		BusinessObjectCollection printers;

		public StmMenuItemFilteredCollection ComplianceInvoiceDocumentMenus
		{
			get
			{
				if (complianceInvoiceDocumentMenus == null)
				{
					ZQuery menuItemFilter = new ZQuery(StmMenuItemSchema.SU_BusinessContext, new string[] { nameof(BusinessContext.ARInvoice), nameof(BusinessContext.APInvoice),
						nameof(BusinessContext.ARComplianceDocument) ,nameof(BusinessContext.APComplianceDocument) });
					complianceInvoiceDocumentMenus = new StmMenuItemFilteredCollection(Factory, menuItemFilter);
				}
				return complianceInvoiceDocumentMenus;
			}
		}
		StmMenuItemFilteredCollection complianceInvoiceDocumentMenus;

		public GlbBranchCollection PrintingBranches
		{
			get
			{
				if (printingBranches == null)
				{
					ZQuery branchFilter = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					printingBranches = new GlbBranchCollection(Factory, branchFilter);
				}
				return printingBranches;
			}
		}
		GlbBranchCollection printingBranches;

		public GlbDepartmentCollection PrintingDepartments
		{
			get
			{
				if (printingDepartments == null)
				{
					printingDepartments = new GlbDepartmentCollection(Factory);
				}
				return printingDepartments;
			}
		}
		GlbDepartmentCollection printingDepartments;

		public CodeDescriptionPairList XD_SequenceClass_List
		{
			get
			{
				return Factory.GetCachedValue("AccComplianceSequenceLookups.XD_SequenceClass_List",
					delegate
					{
						return SequenceClassListBaseOnCurrentCompanyCountryCode();
					}
				);
			}
		}

		public static CodeDescriptionPairList SequenceClassListBaseOnCurrentCompanyCountryCode()
		{
			return SequenceClassListBaseOnCountryCode(GlbCompany.CurrentCompany.Country.Code);
		}

		public static CodeDescriptionPairList SequenceClassListBaseOnCurrentCompanyCountryCodeInLocalLanguage()
		{
			return SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.Country.Code);
		}

		public static CodeDescriptionPairList SequenceClassListBaseOnCountryCode(ZString countryCode, ZString? ledger = null, ZString? transactionType = null, TransactionCreatingMode? transactionMode = null)
		{
			if (countryCode.IsEmpty)
			{
				return new CodeDescriptionPairList();
			}
			else
			{
				var subtypeCodeList = GetComplianceSubTypeCodeProvider(countryCode, ledger, transactionType, transactionMode)?.GetDescriptionPairList() as CodeDescriptionPairList;
				if (subtypeCodeList == null)
				{
					return new CodeDescriptionPairList();
				}
				return subtypeCodeList;
			}
		}

		public static CodeDescriptionPairList SequenceClassListBaseOnCountryCodeInLocalLanguage(ZString countryCode, ZString? ledger = null, ZString? transactionType = null, TransactionCreatingMode? transactionMode = null)
		{
			if (countryCode.IsEmpty)
			{
				return new CodeDescriptionPairList();
			}
			else
			{
				var subtypeCodeList = GetComplianceSubTypeCodeProvider(countryCode, ledger, transactionType, transactionMode)?.GetLocalDescriptionPairList() as CodeDescriptionPairList;
				if (subtypeCodeList == null)
				{
					return new CodeDescriptionPairList();
				}
				return subtypeCodeList;
			}
		}

		static ComplianceSubTypeList GetComplianceSubTypeCodeProvider(
			ZString countryCode,
			ZString? ledger = null,
			ZString? transactionType = null,
			TransactionCreatingMode? transactionMode = null)
		{
			var ledgerOfUse = GetLedgerOfUse(ledger);
			var transactionTypeOfUse = GetTransactionTypeOfUse(transactionType);
			var countryComplianceFactory = ObjectFactory.Get<ICountryComplianceFactoryIntegration>();
			var filteredSubtypeCodeList = countryComplianceFactory.GetIComplianceSubTypeCodeProvider(countryCode)?.GetComplianceSubTypes()?
				.Where(x => (!ledgerOfUse.HasValue || x.Ledger == ledgerOfUse || x.Ledger == LedgerOfUse.ALL)
					&& (!transactionTypeOfUse.HasValue || x.TransactionType.HasFlag(transactionTypeOfUse))
					&& (!transactionMode.HasValue || x.TransactionCreatingMode.HasFlag(transactionMode)));

			if (filteredSubtypeCodeList != null)
			{
				var complianceSubTypeList = new ComplianceSubTypeList();
				complianceSubTypeList.AddRange(filteredSubtypeCodeList);
				return complianceSubTypeList;
			}

			return null;
		}

		static LedgerOfUse? GetLedgerOfUse(ZString? ledger)
		{
			LedgerOfUse? ledgerOfUse = null;

			if (ledger.HasValue)
			{
				switch (ledger.Value)
				{
					case LedgerTypes.AccountsPayable:
					case LedgerTypes.IncompleteTransactions:
						ledgerOfUse = LedgerOfUse.AP;
						break;
					case LedgerTypes.AccountsReceivable:
						ledgerOfUse = LedgerOfUse.AR;
						break;
				}
			}

			return ledgerOfUse;
		}

		static TransactionTypeOfUse? GetTransactionTypeOfUse(ZString? transactionType)
		{
			TransactionTypeOfUse? transactionTypeOfUse = null;

			if (transactionType.HasValue)
			{
				switch (transactionType.Value)
				{
					case TransactionTypes.Invoice:
						transactionTypeOfUse = TransactionTypeOfUse.INV;
						break;
					case TransactionTypes.CreditNote:
						transactionTypeOfUse = TransactionTypeOfUse.CRD;
						break;
					case TransactionTypes.AdjustmentNote:
						transactionTypeOfUse = TransactionTypeOfUse.ADJ;
						break;
				}
			}

			return transactionTypeOfUse;
		}

		public CodeDescriptionPairList XD_RollupBehaviourType_List
		{
			get
			{
				return Factory.GetCachedValue("AccComplianceSequenceLookups.XD_RollupBehaviourType_List",
					delegate
					{
						return new CodeDescriptionPairList(OLookUpEditType.ComplianceRollupBehaviourType);
					}
				);
			}
		}

		public static CodeDescriptionPair ComplianceNumberFormatDefault { get { return new CodeDescriptionPair("DEF", ResString.GetMultilingualString("006eb2c7-b289-4c03-bff4-4047f275b35d", "Series Prefix + Sequence Number")); } }

		public CodeDescriptionPairList XD_ComplianceNumberFormat_List
		{
			get
			{
				return Factory.GetCachedValue("AccComplianceSequenceLookups.XD_ComplianceNumberFormat_List",
					delegate
					{
						var collection = AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

						var lookUpList = new CodeDescriptionPairList();

						lookUpList.Add(ComplianceNumberFormatDefault);
						foreach (ComplianceNumberSequenceConfiguration configuration in collection)
						{
							lookUpList.AddPair(configuration.Code, configuration.Description);
						}

						return lookUpList;
					}
				);
			}
		}

		public CodeDescriptionPairList XD_AllocationLevel_List
		{
			get
			{
				return Factory.GetCachedValue("AccComplianceSequenceLookups.XD_AllocationLevel_List",
					delegate
					{
						return new CodeDescriptionPairList(OLookUpEditType.ComplianceBookAllocationLevel);
					}
				);
			}
		}

		public static class ComplianceDocumentNumberAllocationSettingCodes
		{
			public const string Print = "PRN";
			public const string Post = "PST";
			public const string GovermentNumberAllocate = "GVT";
			public const string Manual = "MAN";
		}

		public static CodeDescriptionPairList ComplianceDocumentNumberAllocationSettingList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(ComplianceDocumentNumberAllocationSettingCodes.Print, ResString.GetMultilingualString("BCDA2D98-20CE-424C-B89C-6AD3D5F001B8", "Defer Compliance Numbering until Printing Compliance Document"));
				lookUpList.AddPair(ComplianceDocumentNumberAllocationSettingCodes.Post, ResString.GetMultilingualString("3DC944C9-E495-4C62-80DC-C1B74F83A289", "Assign Compliance Number when Posting Transaction"));
				lookUpList.AddPair(ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate, ResString.GetMultilingualString("B2B615AD-9111-4FA6-B9FA-7CA50F32EF4E", "Obtain Compliance Number from Government"));
				lookUpList.AddPair(ComplianceDocumentNumberAllocationSettingCodes.Manual, ResString.GetMultilingualString("6F3B2636-3105-4B8A-90D0-3634F81B9F3B", "Manually assign Compliance Number after Posting Transaction"));
				return lookUpList;
			}
		}

		public static CodeDescriptionPairList APComplianceDocumentNumberAllocationSettingList
		{
			get
			{
				CodeDescriptionPairList lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(ComplianceDocumentNumberAllocationSettingCodes.Print, ResString.GetMultilingualString("BCDA2D98-20CE-424C-B89C-6AD3D5F001B8", "Defer Compliance Numbering until Printing Compliance Document"));
				lookUpList.AddPair(ComplianceDocumentNumberAllocationSettingCodes.Post, ResString.GetMultilingualString("d20e2d01-c8d9-48f2-9a9a-070bc602a1cd", "Assign Compliance Number After Posting Transaction"));
				return lookUpList;
			}
		}
	}
}
