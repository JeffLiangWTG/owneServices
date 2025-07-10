using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccEInvoicingTemplateFileViewCollection : BusinessObjectCollection<AccEInvoicingTemplateFileView>
	{
		readonly ZGuid CompanyPk;
		readonly ZGuid BranchPk;
		readonly ZGuid OrgHeaderPk;
		readonly AccEInvoicingTemplateFileLevelEnum level;

		public AccEInvoicingTemplateFileViewCollection(BusinessObjectFactory factory, ZGuid companyPk) : base(factory, GetZQuery(companyPk))
		{
			CompanyPk = companyPk;
			level = AccEInvoicingTemplateFileLevelEnum.Company;
		}

		public AccEInvoicingTemplateFileViewCollection(BusinessObjectFactory factory, ZGuid companyPk, ZGuid branchPk) : base(factory, GetZQuery(companyPk, branchPk))
		{
			CompanyPk = companyPk;
			BranchPk = branchPk;
			level = AccEInvoicingTemplateFileLevelEnum.Branch;
		}

		public AccEInvoicingTemplateFileViewCollection(BusinessObjectFactory factory, ZGuid companyPk, ZGuid branchPk, ZGuid orgHeaderPk) : base(factory, GetZQuery(companyPk, branchPk, orgHeaderPk))
		{
			CompanyPk = companyPk;
			BranchPk = branchPk;
			OrgHeaderPk = orgHeaderPk;
			level = AccEInvoicingTemplateFileLevelEnum.Organisation;
		}

		public AccEInvoicingTemplateFileLevelEnum Level => level;

		static ZQuery GetZQuery(ZGuid companyPk)
		{
			var query = new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_GC, companyPk);
			query.AddToFilter(GetFilterByParent(string.Empty, null));
			return query;
		}

		static ZQuery GetZQuery(ZGuid companyPk, ZGuid branchPk)
		{
			var query = new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_GC, companyPk);
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GetFilterByParent(string.Empty, null), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(GlbBranchSchema.Constants.Prefix, branchPk), JoinCondition.Or);
			query.AddToFilter(subQuery);
			return query;
		}

		static ZQuery GetZQuery(ZGuid companyPk, ZGuid branchPk, ZGuid orgHeaderPk)
		{
			var query = new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_GC, companyPk);
			var subQuery = new ZQuery();
			subQuery.AddToFilter(GetFilterByParent(string.Empty, null), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(GlbBranchSchema.Constants.Prefix, branchPk), JoinCondition.Or);
			subQuery.AddToFilter(GetFilterByParent(OrgHeaderSchema.Constants.Prefix, orgHeaderPk), JoinCondition.Or);
			query.AddToFilter(subQuery);
			return query;
		}

		public AccEInvoicingTemplateFileView GetRecord(ZString jobType, ZString serviceDirection, ZString transportMode, ZString templateCode)
		{
			var levelArray = new ZString[] { OrgHeaderSchema.Constants.Prefix, GlbBranchSchema.Constants.Prefix, string.Empty };
			var jobTypeList = new[] { jobType };
			var serviceDirectionList = new[] { serviceDirection };
			var transportModeList = new[] { transportMode };
			var currencyCodeList = new[] { templateCode };

			return Where(c => jobTypeList.Contains(c.ETF_JobType) &&
							serviceDirectionList.Contains(c.ETF_ServiceDirection) &&
							transportModeList.Contains(c.ETF_TransportMode) &&
							currencyCodeList.Contains(c.ETF_TemplateCode))
					.OrderBy(c => Array.IndexOf(levelArray, c.ETF_ParentTableCode))
					.ThenBy(c => Array.IndexOf(jobTypeList, c.ETF_JobType))
					.ThenBy(c => Array.IndexOf(serviceDirectionList, c.ETF_ServiceDirection))
					.ThenBy(c => Array.IndexOf(transportModeList, c.ETF_TransportMode))
					.ThenBy(c => Array.IndexOf(currencyCodeList, c.ETF_TemplateCode))
					.FirstOrDefault();
		}

		public void SetTemplateFile(ZString jobType, ZString serviceDirection, ZString transportMode, ZString templateCode)
		{
			var result = this.Cast<AccEInvoicingTemplateFileView>().FirstOrDefault(
						c => c.Level == level &&
						c.ETF_JobType == jobType &&
						c.ETF_ServiceDirection == serviceDirection &&
						c.ETF_TransportMode == transportMode);

			if (result == null)
			{
				result = AddNew();

				result.ETF_JobType = jobType;
				result.ETF_ServiceDirection = serviceDirection;
				result.ETF_TransportMode = transportMode;
				result.ETF_TemplateCode = templateCode;
			}

			if (templateCode != ZString.Empty)
			{
				result.ETF_TemplateCode = templateCode;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccEInvoicingTemplateFileView)child;
			config.ETF_GC = CompanyPk;
			config.ETF_ParentTableCode = AccEInvoicingTemplateFileView.ToTablePrefix(level);

			if (level == AccEInvoicingTemplateFileLevelEnum.Branch)
			{
				config.ETF_ParentID = BranchPk;
			}
			else if (level == AccEInvoicingTemplateFileLevelEnum.Organisation)
			{
				config.ETF_ParentID = OrgHeaderPk;
			}
		}

		static ZQuery GetFilterByParent(string parentTableCode, ZGuid? parentPk)
		{
			return new ZQuery(
					new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_ParentTableCode, parentTableCode),
					new ZQuery(AccEInvoicingTemplateFileViewSchema.ETF_ParentID, parentPk));
		}
	}
}
