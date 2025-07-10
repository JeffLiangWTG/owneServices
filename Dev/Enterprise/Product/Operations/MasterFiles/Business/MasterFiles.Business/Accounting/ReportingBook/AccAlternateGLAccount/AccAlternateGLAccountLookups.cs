using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateGLAccountLookups : AutoAccAlternateGLAccountLookups
	{
		public AccAlternateGLAccountLookups(AutoAccAlternateGLAccount parent) : base(parent)
		{
		}

		public override AccAlternateChartCollection AlternateCharts
		{
			get
			{
				var query = new ZQuery(AccAlternateChartSchema.AAC_GC_Company, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, null);
				return new AccAlternateChartCollection(Factory, query);
			}
		}

		public CodeDescriptionPairList AccountTypeList => BaseLookUps.AccountTypeList;

		public CodeDescriptionPairList BSHPAndLAccountTypeList => BaseLookUps.BSHPAndLAccountTypeList;

		public CodeDescriptionPairList WithoutParentAccountTypeList => BaseLookUps.WithoutParentAccountTypeList;

		public CodeDescriptionPairList DebitCreditList => BaseLookUps.DebitCreditList;

		public CodeDescriptionPairList ReportSectionList
		{
			get
			{
				var reportSectionList = BaseLookUps.ReportSectionList;
				if (Parent is AutoAccAlternateGLAccount alternateGLAccount && alternateGLAccount.AlternateChart != null && alternateGLAccount.AlternateChart.AAC_BalanceSheetStyle == AccAlternateChartLookups.BalanceSheetStyleCode.EAL)
				{
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Assets, AccGLHeader.Constants.SectionTypes.Descriptions.Assets);
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, AccGLHeader.Constants.SectionTypes.Descriptions.Liabilities);
				}
				else
				{
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Liabilities, BaseAlternateGLAccountLookups.Constants.ELASectionTypes.Descriptions.Liabilities);
					reportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Assets, BaseAlternateGLAccountLookups.Constants.ELASectionTypes.Descriptions.Assets);
				}

				return reportSectionList;
			}
		}

		public ReadOnlyCodeDescriptionPairList StatisticalUnitsList => BaseLookUps.StatisticalUnitsList;

		public CodeDescriptionPairList CashFlowTypeList => BaseLookUps.CashFlowTypeList;

		public override AccAlternateGLAccountCollection AlternateNums => BaseLookUps.AlternateNums;

		public override AccAlternateGLAccountCollection ConsolidationNums => BaseLookUps.ConsolidationNums;

		public override AccAlternateGLAccountCollection HeaderDependsOnTotals => BaseLookUps.HeaderDependsOnTotals;

		public override AccAlternateGLAccountCollection PercentNums => BaseLookUps.PercentNums;

		BaseAlternateGLAccountLookups BaseLookUps => baseLookUps ?? (baseLookUps = new BaseAlternateGLAccountLookups(Parent.Factory));

		BaseAlternateGLAccountLookups baseLookUps;
	}
}
