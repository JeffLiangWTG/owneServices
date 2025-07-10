using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Excel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccOrgTaxConfigurationTemplateLinkedOrganisationCollection : OrgHeaderCollection, IHaveZQueryForZGridExcelExport
	{
		public AccOrgTaxConfigurationTemplateLinkedOrganisationCollection(AccOrgTaxConfigurationTemplate accOrgTaxConfigurationTemplate) : base(accOrgTaxConfigurationTemplate.Factory)
		{
			Parent = accOrgTaxConfigurationTemplate;
		}

		AccOrgTaxConfigurationTemplate Parent { get; }

		ZQuery IHaveZQueryForZGridExcelExport.Query => CachedQueryForExcelExport ?? ZQuery.NoResultQuery;

		ZQuery CachedQueryForExcelExport;

		protected override bool RunPreSaveValidationCore() => true;

		public override void Load() => Load(new ZQuery());

		public override void Load(ZQuery filter)
		{
			var mergedFilter = filter.DeepClone();
			mergedFilter.AddToFilter(AdditionalFilter);
			CachedQueryForExcelExport = mergedFilter;
			base.Load(mergedFilter);
		}

		public void ClearResult() => base.Load(ZQuery.NoResultQuery);

		protected override ZQuery CreateAdditionalFilter()
		{
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);

			if (Parent.OCT_IsReceivable)
			{
				subQuery.AddToFilter(OrgCompanyDataSchema.OB_OCT_ARTaxTemplate, Parent.PK);
			}

			if (Parent.OCT_IsPayable)
			{
				subQuery.AddToFilter(OrgCompanyDataSchema.OB_OCT_APTaxTemplate, Parent.PK);
			}

			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			query.AddSubQuery(subQuery, JoinCondition.And);

			var mainQuery = base.CreateAdditionalFilter();
			mainQuery.AddToFilter(query);
			return mainQuery;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var orgHeader = bizOAdded as OrgHeader;
			orgHeader.ClearRowNotifications();

			if (!IsLoading)
			{
				var forienKeyProp = GetForienKeyProperty(orgHeader);
				if (forienKeyProp != null)
				{
					if (!ValidateAttachedTemplate(orgHeader.CompanyData.PK, (ZGuid)forienKeyProp.OriginalValue, Parent.PK, out var msg))
					{
						orgHeader.AddRowWarning(msg);
					}

					forienKeyProp.Value = Parent.PK;
				}
			}
		}

		bool ValidateAttachedTemplate(ZGuid orgCompanyDataPK, ZGuid currentTemplatePK, ZGuid settingTemplatePK, out string warnningMsg)
		{
			var warnningMsgs = new List<string>();

			if (currentTemplatePK != settingTemplatePK
				&& currentTemplatePK.IsValid
				&& Factory.Load<AccOrgTaxConfigurationTemplate>(currentTemplatePK) is AccOrgTaxConfigurationTemplate template
				&& template != null)
			{
				warnningMsgs.Add(Res.GetString("01B2D201-BB5E-4A86-82C0-8B4E2AA3510D", "Current Tax Configuration Template is {0}", template.OCT_Code));
			}

			var subQueryETC = new ZDBOnlySubQuery(typeof(AccTaxConfiguration), AccTaxConfigurationSchema.PK);
			subQueryETC.AddToFilter(AccTaxConfigurationSchema.ETC_Ledger, Parent.Ledger);

			var query = new ZDBOnlyQuery(typeof(AccOrgTaxConfiguration));
			query.AddToFilter(AccOrgTaxConfigurationSchema.OTC_OB, orgCompanyDataPK);
			query.AddToFilter(AccOrgTaxConfigurationSchema.OTC_OCT, SQLComparisonOperator.Equal, null);
			query.AddSubQuery(AccOrgTaxConfigurationSchema.OTC_ETC, subQueryETC, JoinCondition.And);
			if (Factory.Exists(typeof(AccOrgTaxConfiguration), query))
			{
				warnningMsgs.Add(Res.GetString("61440C50-AFF4-4E62-8844-F3EE3BEB9327", "This organization has one or more existing Tax Configurations."));
			}

			if (currentTemplatePK == settingTemplatePK)
			{
				warnningMsgs.Add(Res.GetString("7644E317-6604-436D-8EC5-FBE740C86A1B", "This Organization was already attached, detaching it will remove linking."));
			}

			warnningMsg = string.Join(System.Environment.NewLine, warnningMsgs);
			return warnningMsgs.Count == 0;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (!IsLoading)
			{
				var orgHeader = bizO as OrgHeader;
				orgHeader.ClearRowNotifications();
				var forienKeyProp = GetForienKeyProperty(orgHeader);
				if (forienKeyProp != null)
				{
					forienKeyProp.Value = forienKeyProp.HasChanges
						? forienKeyProp.OriginalValue
						: ZGuid.Empty;
				}
			}
		}

		public bool CheckOrgLinkedHasChanged(OrgHeader orgHeader)
			=> !orgHeader.CompanyData.IsInDatabase || (GetForienKeyProperty(orgHeader)?.HasChanges ?? false);

		ZPropertyInfo GetForienKeyProperty(OrgHeader orgHeader)
		{
			if (Parent.OCT_IsReceivable)
			{
				return orgHeader.CompanyData.OB_OCT_ARTaxTemplateInfo;
			}

			if (Parent.OCT_IsPayable)
			{
				return orgHeader.CompanyData.OB_OCT_APTaxTemplateInfo;
			}

			return null;
		}

		public int GetTotalRowsCount() => Factory.GetDatabaseCount(typeof(OrgHeader), AdditionalFilter);
	}
}
