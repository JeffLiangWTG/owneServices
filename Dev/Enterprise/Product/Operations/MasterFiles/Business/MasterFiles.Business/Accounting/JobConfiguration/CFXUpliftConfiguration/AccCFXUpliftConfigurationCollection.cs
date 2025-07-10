using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccCFXUpliftConfigurationCollection : BusinessObjectCollection<AccCFXUpliftConfiguration>
	{
		readonly ZGuid companyPk;
		readonly ZGuid branchPk;
		readonly ZGuid orgHeaderPk;
		readonly AccCFXConfigurationLevelEnum level;
		const string ZCodeAll = "ALL";

		public AccCFXUpliftConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPk) : base(factory, GetZQuery(companyPk))
		{
			this.companyPk = companyPk;
			level = AccCFXConfigurationLevelEnum.Company;
		}

		public AccCFXUpliftConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPk, ZGuid branchPk) : base(factory, GetZQuery(companyPk, branchPk))
		{
			this.companyPk = companyPk;
			this.branchPk = branchPk;
			level = AccCFXConfigurationLevelEnum.Branch;
		}

		public AccCFXUpliftConfigurationCollection(BusinessObjectFactory factory, ZGuid companyPk, ZGuid branchPk, ZGuid orgHeaderPk) : base(factory, GetZQuery(companyPk, branchPk, orgHeaderPk))
		{
			this.companyPk = companyPk;
			this.branchPk = branchPk;
			this.orgHeaderPk = orgHeaderPk;
			level = AccCFXConfigurationLevelEnum.Organisation;
		}

		internal AccCFXConfigurationLevelEnum Level => level;

		public override bool ReadOnly => IsEditingForbidden() || base.ReadOnly;

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Exception message")]
		bool IsEditingForbidden()
		{
			if (level == AccCFXConfigurationLevelEnum.Company)
			{
				return !Env.Security.CompaniesModifyCurrencyCFXUplift.IsAllowed;
			}

			if (level == AccCFXConfigurationLevelEnum.Branch)
			{
				return !Env.Security.BranchModifyCurrencyCFXUplift.IsAllowed;
			}

			if (level == AccCFXConfigurationLevelEnum.Organisation)
			{
				return !Env.Security.OrgReceivablesModifyCurrencyUplift.IsAllowed;
			}

			throw new InvalidOperationException($"Unsupported level {level}");
		}

		public AccCFXUpliftConfiguration GetRecord(ZString jobType, ZString serviceDirection, ZString transportMode, ZString? origin = null, ZString? destination = null, ZString? currencyCode = null, ZDate? date = null)
		{
			var levelArray = new ZString[] { OrgHeaderSchema.Constants.Prefix, GlbBranchSchema.Constants.Prefix, string.Empty };
			var jobTypeList = new[] { jobType, (ZString)ZCodeAll };
			var originList = new[] { origin ?? ZString.Empty, ZString.Empty };
			var destinationList = new[] { destination ?? ZString.Empty, ZString.Empty };
			var serviceDirectionList = new[] { serviceDirection, (ZString)ZCodeAll };
			var transportModeList = new[] { transportMode, (ZString)ZCodeAll };
			var currencyCodeList = new[] { currencyCode ?? ZString.Empty, ZString.Empty };

			return Where(c => jobTypeList.Contains(c.JCF_JobType) &&
					originList.Contains(c.JCF_RN_NKOriginCountry) &&
					destinationList.Contains(c.JCF_RN_NKDestinationCountry) &&
					serviceDirectionList.Contains(c.JCF_ServiceDirection) &&
					transportModeList.Contains(c.JCF_TransportMode) &&
					currencyCodeList.Contains(c.JCF_RX_NKCurrency) &&
					(c.JCF_StartDate.IsEmpty || (date != null && c.JCF_StartDate <= date && date <= c.JCF_ExpiryDate)))
				.OrderBy(c => Array.IndexOf(levelArray, c.JCF_ParentTableCode))
				.ThenBy(c => Array.IndexOf(jobTypeList, c.JCF_JobType))
				.ThenBy(c => Array.IndexOf(originList, c.JCF_RN_NKOriginCountry))
				.ThenBy(c => Array.IndexOf(destinationList, c.JCF_RN_NKDestinationCountry))
				.ThenBy(c => Array.IndexOf(serviceDirectionList, c.JCF_ServiceDirection))
				.ThenBy(c => Array.IndexOf(transportModeList, c.JCF_TransportMode))
				.ThenBy(c => Array.IndexOf(currencyCodeList, c.JCF_RX_NKCurrency))
				.ThenByDescending(c => c.JCF_StartDate)
				.FirstOrDefault();
		}

		public void SetUplifts(ZString jobType, ZString serviceDirection, ZString transportMode, ZDecimal? percentage = null, ZDecimal? minimum = null)
		{
			SetUplifts(jobType, serviceDirection, transportMode, ZString.Empty, percentage, minimum);
		}

		public void SetUplifts(ZString jobType, ZString serviceDirection, ZString transportMode, ZString currencyCode, ZDecimal? percentage = null, ZDecimal? minimum = null)
		{
			var result = this.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(
						c => c.Level == level &&
						c.JCF_JobType == jobType &&
						c.JCF_RN_NKOriginCountry == ZString.Empty &&
						c.JCF_RN_NKDestinationCountry == ZString.Empty &&
						c.JCF_ServiceDirection == serviceDirection &&
						c.JCF_TransportMode == transportMode &&
						c.JCF_RX_NKCurrency == currencyCode &&
						c.JCF_StartDate == ZDate.Empty);

			if (result == null)
			{
				result = AddNew();

				result.JCF_JobType = jobType;
				result.JCF_ServiceDirection = serviceDirection;
				result.JCF_TransportMode = transportMode;
				result.JCF_RX_NKCurrency = currencyCode;
			}

			if (percentage != null)
			{
				result.JCF_CFXPercentage = percentage.Value;
			}

			if (minimum != null)
			{
				result.JCF_CFXMinimum = minimum.Value;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccCFXUpliftConfiguration)child;
			config.JCF_GC = companyPk;
			config.JCF_ParentTableCode = level.ToTablePrefix();

			if (level == AccCFXConfigurationLevelEnum.Branch)
			{
				config.JCF_ParentID = branchPk;
			}
			else if (level == AccCFXConfigurationLevelEnum.Organisation)
			{
				config.JCF_ParentID = orgHeaderPk;
			}
		}

		static ZQuery GetZQuery(ZGuid companyPk)
		{
			var query = new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_GC, companyPk);
			query.AddToFilter(GetFilterByParent(string.Empty, null));
			return query;
		}

		static ZQuery GetZQuery(ZGuid companyPk, ZGuid branchPk)
		{
			var query = new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_GC, companyPk);
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GetFilterByParent(string.Empty, null), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(GlbBranchSchema.Constants.Prefix, branchPk), JoinCondition.Or);
			query.AddToFilter(subQuery);
			return query;
		}

		static ZQuery GetZQuery(ZGuid companyPk, ZGuid branchPk, ZGuid orgHeaderPk)
		{
			var query = new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_GC, companyPk);
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GetFilterByParent(string.Empty, null), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(GlbBranchSchema.Constants.Prefix, branchPk), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(OrgHeaderSchema.Constants.Prefix, orgHeaderPk), JoinCondition.Or);
			query.AddToFilter(subQuery);
			return query;
		}

		static ZQuery GetFilterByParent(string parentTableCode, ZGuid? parentPk)
		{
			return new ZQuery(
					new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_ParentTableCode, parentTableCode),
					new ZQuery(AccCFXUpliftConfigurationViewSchema.JCF_ParentID, parentPk));
		}
	}
}
