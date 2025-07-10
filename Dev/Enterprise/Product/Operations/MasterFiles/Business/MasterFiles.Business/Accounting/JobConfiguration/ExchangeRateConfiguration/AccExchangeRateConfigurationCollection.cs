using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccExchangeRateConfigurationCollection : BusinessObjectCollection<AccExchangeRateConfiguration>
	{
		public AccExchangeRateConfigurationCollection(BusinessObjectFactory factory, ZQuery additionalQuery) : base(factory, additionalQuery)
		{
			level = AccExRateConfigurationLevelEnum.None;
		}

		public AccExchangeRateConfigurationCollection(BusinessObjectFactory factory) : base(factory, GetZQuery())
		{
			level = AccExRateConfigurationLevelEnum.System;
		}

		public AccExchangeRateConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPK) : base(factory, GetZQuery(companyPK))
		{
			this.companyPK = companyPK;
			level = AccExRateConfigurationLevelEnum.Company;
		}

		public AccExchangeRateConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPK, ZString ledger, ZGuid orgGroupPK) : base(factory, GetZQuery(companyPK, ledger, orgGroupPK))
		{
			this.companyPK = companyPK;
			this.ledger = ledger;
			this.orgGroupPK = orgGroupPK;
			level = ledger == LedgerTypes.AccountsReceivable ? AccExRateConfigurationLevelEnum.DebtorGroup : AccExRateConfigurationLevelEnum.CreditorGroup;
		}

		public AccExchangeRateConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPK, ZString ledger, ZGuid orgGroupPK, ZGuid orgHeaderPK) : base(factory, GetZQuery(companyPK, ledger, orgGroupPK, orgHeaderPK))
		{
			this.companyPK = companyPK;
			this.ledger = ledger;
			this.orgGroupPK = orgGroupPK;
			this.orgHeaderPK = orgHeaderPK;
			level = ledger == LedgerTypes.AccountsReceivable ? AccExRateConfigurationLevelEnum.Debtor : AccExRateConfigurationLevelEnum.Creditor;
		}

		readonly ZGuid companyPK;
		readonly ZGuid orgGroupPK;
		readonly ZGuid orgHeaderPK;
		readonly ZString ledger;
		readonly AccExRateConfigurationLevelEnum level;

		public ZGuid CompanyPK => companyPK;
		public AccExRateConfigurationLevelEnum Level => level;

		public override bool ReadOnly => IsEditingForbidden() || base.ReadOnly;

		BusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (fReadOnlyFactory == null)
				{
					fReadOnlyFactory = new BusinessObjectFactory();
				}

				return fReadOnlyFactory;
			}
		}
		BusinessObjectFactory fReadOnlyFactory;

		public AccExchangeRateConfigurationWrapper GetRecord(ZString ledgerCode, ZString jobType, ZString serviceDirection, ZString transportMode, ZString invoiceCurrencyType, ZString currencyCode, IAccExchangeRateConfigurationRateConsumer rateConsumer)
		{
			var levelArray = Level.GetThisAndUpperLevels().ToArray();
			var ledgerArray = ledgerCode.IsEmpty ? new[] { ZString.Empty } : new[] { ledgerCode, ZString.Empty };
			var jobTypeList = new[] { jobType, (ZString)"ALL" };
			var serviceDirectionList = new[] { serviceDirection, (ZString)"ALL" };
			var transportModeList = new[] { transportMode, (ZString)"ALL" };
			var currencyTypeArray = new ZString[] { AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.CUR, AccExchangeRateConfigurationViewLookups.CurrencyTypeCodes.ALL };
			var invoiceCurrencyTypeList = invoiceCurrencyType.IsEmpty ? new[] { ZString.Empty } : new[] { invoiceCurrencyType, ZString.Empty };

			var result = Where(c => levelArray.Contains(c.Level) &&
				invoiceCurrencyTypeList.Contains(c.JCE_InvoiceCurrencyType) &&
				HasMatchedCurrency(c) &&
				jobTypeList.Contains(c.JCE_JobType) &&
				serviceDirectionList.Contains(c.JCE_ServiceDirection) &&
				transportModeList.Contains(c.JCE_TransportMode) &&
				ledgerArray.Contains(c.JCE_Ledger))
			.OrderBy(c => Array.IndexOf(levelArray, c.Level))
			.ThenBy(c => Array.IndexOf(invoiceCurrencyTypeList, c.JCE_InvoiceCurrencyType))
			.ThenBy(c => Array.IndexOf(currencyTypeArray, c.JCE_Calc_CurrencyType))
			.ThenBy(c => Array.IndexOf(jobTypeList, c.JCE_JobType))
			.ThenBy(c => Array.IndexOf(ledgerArray, c.JCE_Ledger))
			.ThenBy(c => Array.IndexOf(transportModeList, c.JCE_TransportMode))
			.ThenBy(c => Array.IndexOf(serviceDirectionList, c.JCE_ServiceDirection))
			.FirstOrDefault();

			if (result == null
				//&& jobType != AccountingMasterFilesConstants.JobTypes.NonJobRelated
				//&& ledgerCode != LedgerTypes.AccountsReceivable
				)
			{
				var queryForSystemRecord = new ZQuery();
				queryForSystemRecord.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_JobType, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All);
				queryForSystemRecord.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_TransportMode, JobConfigurationSelectorLookups.ModeAdditionalCodes.All);
				queryForSystemRecord.AddToFilter(AccExchangeRateConfigurationViewSchema.JCE_ServiceDirection, Constants.FreightShipmentDirection.Code.All);
				result = ReadOnlyFactory.LoadTop1<AccExchangeRateConfiguration>(queryForSystemRecord);
				if (result == null)
				{
					result = ReadOnlyFactory.New<AccExchangeRateConfiguration>();
					//result.JCE_ConfigType = JobConfiguration.TypeCodes.ExchangeRate;
					result.JCE_JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
					result.JCE_TransportMode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
					result.JCE_ServiceDirection = Constants.FreightShipmentDirection.Code.All;
					result.JCE_Preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate;
				}
			}

			return new AccExchangeRateConfigurationWrapper(result, result.GetCurrencyConfig(currencyCode, GetRateDate(result)));

			bool HasMatchedCurrency(AccExchangeRateConfiguration exchangeRateConfiguration)
			{
				return exchangeRateConfiguration.GetCurrencyConfig(currencyCode, GetRateDate(exchangeRateConfiguration)) != null;
			}

			ZDate GetRateDate(AccExchangeRateConfiguration exchangeRateConfiguration)
			{
				var rateDate = ZDateTime.Empty;

				if (rateConsumer != null)
				{
					rateDate = ObjectFactory.Get<IAccExchangeRateConfigurationsHelper>().GetExchangeRateDate(rateConsumer, exchangeRateConfiguration.JCE_Preference);
				}

				return (ZDate)rateDate;
			}
		}

		public void SetExRate(ZString jobType, ZString serviceDirection, ZString transportMode, string exRateType = Constants.ExchangeRateTypes.Code.BuyRate,
			string preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate, int offset = 0, bool prompt = false, string invoiceCurrencyType = "")
		{
			SetExRate(ZString.Empty, jobType, serviceDirection, transportMode, exRateType, preference, offset, prompt, invoiceCurrencyType);
		}

		public void SetExRate(ZString ledgerCode, ZString jobType, ZString serviceDirection, ZString transportMode, string exRateType = Constants.ExchangeRateTypes.Code.BuyRate,
			string preference = Constants.JobBillingExchangeRatePreference.Code.TodaysRate, int offset = 0, bool prompt = false, string invoiceCurrencyType = "")
		{
			var result = this.Cast<AccExchangeRateConfiguration>().FirstOrDefault(
						c => c.Level == level &&
						c.JCE_Ledger == ledgerCode &&
						c.JCE_JobType == jobType &&
						c.JCE_ServiceDirection == serviceDirection &&
						c.JCE_TransportMode == transportMode &&
						c.JCE_InvoiceCurrencyType == invoiceCurrencyType);

			if (result == null)
			{
				result = AddNew();

				result.JCE_Ledger = ledgerCode;
				result.JCE_JobType = jobType;
				result.JCE_ServiceDirection = serviceDirection;
				result.JCE_TransportMode = transportMode;
				result.JCE_InvoiceCurrencyType = invoiceCurrencyType;
			}

			result.JCE_Offset = offset;
			result.JCE_Preference = preference;
			result.JCE_Prompt = prompt;
			result.GetCurrencyConfig(ZString.Empty, ZDate.Empty).JCT_ExRateType = exRateType;
		}

		bool IsEditingForbidden()
		{
			if (level == AccExRateConfigurationLevelEnum.Debtor)
			{
				return !Env.Security.OrgReceivablesModifyExchangeRates.IsAllowed;
			}

			if (level == AccExRateConfigurationLevelEnum.Creditor)
			{
				return !Env.Security.OrgPayablesModifyExchangeRates.IsAllowed;
			}

			if (level == AccExRateConfigurationLevelEnum.Company)
			{
				return !Env.Security.CompaniesModifyJobBillingExchangeRates.IsAllowed;
			}

			if (level == AccExRateConfigurationLevelEnum.System)
			{
				return !Env.Security.JobExRateSysConfigs.IsAllowed || !Env.Security.JobExRateSysConfigsModify.IsAllowed;
			}

			return false;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccExchangeRateConfiguration)child;
			config.JCE_GC = companyPK;
			config.JCE_ParentTableCode = level.ToTablePrefix();

			switch (level)
			{
				case AccExRateConfigurationLevelEnum.DebtorGroup:
				case AccExRateConfigurationLevelEnum.CreditorGroup:
					config.JCE_ParentID = orgGroupPK;
					config.JCE_Ledger = ledger;
					break;
				case AccExRateConfigurationLevelEnum.Debtor:
				case AccExRateConfigurationLevelEnum.Creditor:
					config.JCE_ParentID = orgHeaderPK;
					config.JCE_Ledger = ledger;
					break;
			}
		}

		static ZQuery GetZQuery()
		{
			return new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_GC, DBNull.Value);
		}

		static ZQuery GetZQuery(ZGuid companyPK)
		{
			var subQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_GC, null);
			var companyLevelProvider = AccExchangeRateConfigurationsQueryProviderFactory.CreateCompanyLevelProvider(companyPK, subQuery);
			return companyLevelProvider.GetQuery();
		}

		static ZQuery GetZQuery(ZGuid companyPK, ZString ledger, ZGuid orgGroupPK)
		{
			return GetZQueryCore(companyPK, ledger, orgGroupPK, ZGuid.Empty);
		}

		static ZQuery GetZQuery(ZGuid companyPK, ZString ledger, ZGuid orgGroupPK, ZGuid orgHeaderPK)
		{
			return GetZQueryCore(companyPK, ledger, orgGroupPK, orgHeaderPK);
		}

		static ZQuery GetZQueryCore(ZGuid companyPK, ZString ledger, ZGuid orgGroupPK, ZGuid orgHeaderPK)
		{
			var result = new ZQuery();

			var companyLevelWithFallbackQuery = GetZQuery(companyPK);

			var systemConfigQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_GC, null);
			var organizationGroupLevelWithFallbackQuery = AccExchangeRateConfigurationsQueryProviderFactory
															.CreateOrganizationGroupLevelProvider(companyPK, ledger, orgGroupPK, systemConfigQuery)
															.GetQuery();

			result.AddToFilter(companyLevelWithFallbackQuery, JoinCondition.Or);
			result.AddToFilter(organizationGroupLevelWithFallbackQuery, JoinCondition.Or);

			if (!orgHeaderPK.IsEmpty)
			{
				var companyWithEmptyFallbackQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_GC, companyPK);
				companyWithEmptyFallbackQuery.AddToFilter(systemConfigQuery, JoinCondition.Or);

				var organizationLevelAdditionalQuery = new ZQuery(AccExchangeRateConfigurationViewSchema.JCE_Ledger, ledger);
				organizationLevelAdditionalQuery.AddToFilter(companyWithEmptyFallbackQuery);

				var organizationLevelWithFallbackQuery = AccExchangeRateConfigurationsQueryProviderFactory
															.CreateOrganizationLevelProvider(orgHeaderPK, organizationLevelAdditionalQuery)
															.GetQuery();

				result.AddToFilter(organizationLevelWithFallbackQuery, JoinCondition.Or);
			}

			if (!string.IsNullOrEmpty(ledger))
			{
				var oppositeLedger = ledger == LedgerTypes.AccountsReceivable ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
				result.AddToFilter(JoinCondition.And, AccExchangeRateConfigurationViewSchema.JCE_Ledger, SQLComparisonOperator.NotEqual, oppositeLedger);
			}

			return result;
		}
	}
}
