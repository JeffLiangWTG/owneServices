using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccCashAdvanceDefaultingConfigurationCollection : BusinessObjectCollection<AccCashAdvanceDefaultingConfiguration>
	{
		readonly ZGuid CompanyPk;
		readonly ZGuid BranchPk;
		readonly ZGuid OrgHeaderPk;
		readonly ZString Ledger;
		internal readonly AccCashAdvanceDefaultingLevel Level;
		const string ZCodeAll = "ALL";

		#region Constructors

		public AccCashAdvanceDefaultingConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPk) : base(factory, GetZQuery(companyPk))
		{
			CompanyPk = companyPk;
			Level = AccCashAdvanceDefaultingLevel.Company;
		}

		public AccCashAdvanceDefaultingConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPk, ZGuid branchPk) : base(factory, GetZQuery(companyPk, branchPk))
		{
			CompanyPk = companyPk;
			BranchPk = branchPk;
			Level = AccCashAdvanceDefaultingLevel.Branch;
		}

		public AccCashAdvanceDefaultingConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPk, ZGuid branchPk, ZGuid orgHeaderPk, ZString ledger) : base(factory, GetZQuery(companyPk, branchPk, orgHeaderPk, ledger))
		{
			if (ledger != LedgerTypes.AccountsReceivable && ledger != LedgerTypes.AccountsPayable)
			{
				throw new ArgumentException("Advance Payment Defaulting can be configured for Receivables and Payables Ledgers only.", nameof(ledger));
			}

			CompanyPk = companyPk;
			BranchPk = branchPk;
			OrgHeaderPk = orgHeaderPk;
			Ledger = ledger;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				Level = AccCashAdvanceDefaultingLevel.Debtor;
			}
			else
			{
				Level = AccCashAdvanceDefaultingLevel.Creditor;
			}
		}

		#endregion

		#region Overrides

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccCashAdvanceDefaultingConfiguration)child;
			config.CAC_GC = CompanyPk;
			config.CAC_ParentTableCode = Level.ToTablePrefix();
			config.CAC_Ledger = LedgerTypes.AccountsReceivable;
			switch (Level)
			{
				case AccCashAdvanceDefaultingLevel.Branch:
					config.CAC_ParentId = BranchPk;
					break;
				case AccCashAdvanceDefaultingLevel.Debtor:
				case AccCashAdvanceDefaultingLevel.Creditor:
					config.CAC_ParentId = OrgHeaderPk;
					config.CAC_Ledger = Ledger;
					break;
				case AccCashAdvanceDefaultingLevel.Company:
				default:
					break;
			}
			config.CAC_DefaultingOptionInfo.ValueChanged += CAC_DefaultingOptionInfo_ValueChanged;
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (e.ItemAdded)
			{
				((AccCashAdvanceDefaultingConfiguration)e.BizObject).CAC_DefaultingOptionInfo.ValueChanged += CAC_DefaultingOptionInfo_ValueChanged;
			}
			else if (e.ItemRemoved)
			{
				((AccCashAdvanceDefaultingConfiguration)e.BizObject).CAC_DefaultingOptionInfo.ValueChanged -= CAC_DefaultingOptionInfo_ValueChanged;
			}
		}

		#endregion

		#region Event Handlers

		void CAC_DefaultingOptionInfo_ValueChanged(object sender, EventArgs e)
		{
			if (OnChildDefaultingOptionChanged != null && sender is AccCashAdvanceDefaultingConfiguration config)
			{
				var shouldHide = config.CAC_DefaultingOption != CashAdvanceDefaultingOption.SelectedChargeCodesAndGroups;
				OnChildDefaultingOptionChanged.Invoke(config, new DefaultingOptionChangedEventArgs(config.PK, shouldHide));
			}
		}

		public event EventHandler<DefaultingOptionChangedEventArgs> OnChildDefaultingOptionChanged;

		public class DefaultingOptionChangedEventArgs : EventArgs
		{
			public DefaultingOptionChangedEventArgs(ZGuid configPk, ZBool shouldHide)
			{
				ConfigPk = configPk;
				ShouldHide = shouldHide;
			}

			public ZGuid ConfigPk { get; }

			public ZBool ShouldHide { get; }
		}

		#endregion

		#region Queries

		static ZQuery GetZQuery(ZGuid companyPk)
		{
			var query = new ZQuery(AccCashAdvanceDefaultingConfigurationViewSchema.CAC_GC, companyPk);
			query.AddToFilter(GetFilterByParent(string.Empty, null));
			return query;
		}

		static ZQuery GetZQuery(ZGuid companyPk, ZGuid branchPk)
		{
			var query = new ZQuery(AccCashAdvanceDefaultingConfigurationViewSchema.CAC_GC, companyPk);
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GetFilterByParent(string.Empty, null), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(GlbBranchSchema.Constants.Prefix, branchPk), JoinCondition.Or);
			query.AddToFilter(subQuery);
			return query;
		}

		static ZQuery GetZQuery(ZGuid companyPk, ZGuid branchPk, ZGuid orgHeaderPk, ZString ledger)
		{
			var query = new ZQuery(AccCashAdvanceDefaultingConfigurationViewSchema.CAC_GC, companyPk);
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GetFilterByParent(string.Empty, null), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(GlbBranchSchema.Constants.Prefix, branchPk), JoinCondition.Or);

			var subQueryForOrganization = GetFilterByParent(OrgHeaderSchema.Constants.Prefix, orgHeaderPk);
			subQueryForOrganization.AddToFilter(AccCashAdvanceDefaultingConfigurationViewSchema.CAC_Ledger, ledger);
			subQuery.AddToFilter(subQueryForOrganization, JoinCondition.Or);

			query.AddToFilter(subQuery);
			return query;
		}

		static ZQuery GetFilterByParent(string parentTableCode, ZGuid? parentPk)
		{
			return new ZQuery(
					new ZQuery(AccCashAdvanceDefaultingConfigurationViewSchema.CAC_ParentTableCode, parentTableCode),
					new ZQuery(AccCashAdvanceDefaultingConfigurationViewSchema.CAC_ParentId, parentPk));
		}

		#endregion

		#region Find Best Matching Config

		public AccCashAdvanceDefaultingConfiguration FindBestMatchingConfiguration(ZString ledgerCode, ZString jobType, ZString serviceDirection, ZString transportMode)
		{
			var levelArray = new ZString[] { OrgHeaderSchema.Constants.Prefix, GlbBranchSchema.Constants.Prefix, string.Empty };
			var ledgerArray = ledgerCode.IsEmpty ? new[] { ZString.Empty } : new[] { ledgerCode, ZString.Empty };
			var jobTypeList = new[] { jobType, (ZString)ZCodeAll };
			var serviceDirectionList = new[] { serviceDirection, (ZString)ZCodeAll };
			var transportModeList = new[] { transportMode, (ZString)ZCodeAll };

			return Where(c => jobTypeList.Contains(c.CAC_JobType) &&
							serviceDirectionList.Contains(c.CAC_ServiceDirection) &&
							transportModeList.Contains(c.CAC_TransportMode) &&
							ledgerArray.Contains(c.CAC_Ledger))
					.OrderBy(c => Array.IndexOf(levelArray, c.CAC_ParentTableCode))
					.ThenBy(c => Array.IndexOf(ledgerArray, c.CAC_Ledger))
					.ThenBy(c => Array.IndexOf(jobTypeList, c.CAC_JobType))
					.ThenBy(c => Array.IndexOf(serviceDirectionList, c.CAC_ServiceDirection))
					.ThenBy(c => Array.IndexOf(transportModeList, c.CAC_TransportMode))
					.FirstOrDefault();
		}

		#endregion
	}
}
