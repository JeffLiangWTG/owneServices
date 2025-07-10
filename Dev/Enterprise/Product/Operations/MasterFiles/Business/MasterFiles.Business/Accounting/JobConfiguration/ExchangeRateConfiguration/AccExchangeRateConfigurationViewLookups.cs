//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccExchangeRateConfigurationViewLookups
//
//    This class should be used for overriding collections in AutoAccExchangeRateConfigurationViewLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccExchangeRateConfigurationViewLookups : AutoAccExchangeRateConfigurationViewLookups
	{
		public AccExchangeRateConfigurationViewLookups(AutoAccExchangeRateConfigurationView parent) : base(parent)
		{
		}

		public IJobConfiguration ParentJobConfiguration => (IJobConfiguration)base.Parent;
		public new AccExchangeRateConfiguration Parent => (AccExchangeRateConfiguration)base.Parent;

		#region LedgerList

		public CodeDescriptionPairList LedgerList => GetLedgerList();

		CodeDescriptionPairList GetLedgerList()
		{
			var result = new CodeDescriptionPairList();
			if (Parent.JCE_ParentID.IsEmpty)
			{
				result.AddPair(ZString.Empty, Res.GetString("9da72eed-59c8-4a7c-8e6c-43dca232f820", "Both Accounts Receivable and Payable"));
				result.AddPair(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription);
				result.AddPair(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription);
			}
			else if (Parent.JCE_ParentTableCode == OrgDebtorGroupSchema.Constants.Prefix)
			{
				result.AddPair(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription);
			}
			else if (Parent.JCE_ParentTableCode == OrgCreditorGroupSchema.Constants.Prefix)
			{
				result.AddPair(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription);
			}
			else if (Parent.JCE_ParentTableCode == OrgHeaderSchema.Constants.Prefix)
			{
				result.AddPair(LedgerTypes.AccountsReceivable, AccPaymentApprovalLookups.AccountsReceivableDescription);
				result.AddPair(LedgerTypes.AccountsPayable, AccPaymentApprovalLookups.AccountsPayableDescription);
			}
			return result;
		}

		#endregion

		#region JobTypeList

		public CodeDescriptionPairList JobTypeList
		{
			get
			{
				var fJobTypes = JobConfigurationLookupsExtensions.GetJobTypeList();
				var njrPair = new CodeDescriptionPair(AccountingMasterFilesConstants.JobTypes.NonJobRelated, Res.GetString("D7E9FDC3-0492-431A-A4DA-C175932F0DB7", "Non-Job"));
				fJobTypes.InsertInSortOrder(njrPair);
				return fJobTypes;
			}
		}

		#endregion

		#region Direction List

		public CodeDescriptionPairList DirectionList => ParentJobConfiguration.GetDirectionList();

		#endregion

		#region Mode List

		public CodeDescriptionPairList TransportModeList => ParentJobConfiguration.GetTransportModeList();

		#endregion

		#region ExRateTypeList

		public CodeDescriptionPairList ExRateTypeList => GetExRateTypeList();

		CodeDescriptionPairList GetExRateTypeList()
		{
			var result = new CodeDescriptionPairList();

			var registryDataSystemLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value;
			var registryDataApplicable = new CodeDescriptionBoolCollection();

			switch (Parent.Level)
			{
				case AccExRateConfigurationLevelEnum.System:
					{
						registryDataApplicable.AddRange(registryDataSystemLevel);
						break;
					}
				case AccExRateConfigurationLevelEnum.Company:
				case AccExRateConfigurationLevelEnum.Creditor:
				case AccExRateConfigurationLevelEnum.CreditorGroup:
				case AccExRateConfigurationLevelEnum.Debtor:
				case AccExRateConfigurationLevelEnum.DebtorGroup:
					{
						var registryDataCompanyLevel = AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
						registryDataApplicable.AddRange(registryDataSystemLevel);
						registryDataApplicable.AddRange(registryDataCompanyLevel);
						break;
					}
				default:
					break;
			}

			foreach (var item in registryDataApplicable
									.Cast<CodeDescriptionBool>()
									.Where(item => item.Bool))
			{
				result.AddPair(item.Code, item.Description);
			}

			return result;
		}

		#endregion

		#region Preference List

		public CodeDescriptionPairList PreferenceList => ParentJobConfiguration.GetPreferenceList();

		#endregion

		#region Currency Type List

		public CodeDescriptionPairList CurrencyTypeList
		{
			get
			{
				if (currencyTypeList == null)
				{
					currencyTypeList = new CodeDescriptionPairList();
					currencyTypeList.AddPair(CurrencyTypeCodes.ALL, CurrencyTypeDescriptions.ALL);
					currencyTypeList.AddPair(CurrencyTypeCodes.CUR, CurrencyTypeDescriptions.CUR);
				}

				return currencyTypeList;
			}
		}
		CodeDescriptionPairList currencyTypeList;

		public static class CurrencyTypeCodes
		{
			public const string ALL = "ALL";
			public const string CUR = "CUR";
		}

		public static class CurrencyTypeDescriptions
		{
			public static string ALL => Res.GetString("335CEB3F-3647-484E-9CC0-B230A441F4AA", "Applies to all currencies");
			public static string CUR => Res.GetString("A80A0693-0F50-4651-842E-1463471EFC76", "Applies only to currencies listed");
		}

		#endregion

		public CodeDescriptionPairList InvoiceCurrencyTypeList => JobConfigurationLookupsExtensions.GetInvoiceCurrencyTypeList();
	}
}
