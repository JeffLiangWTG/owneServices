using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class BaseAlternateGLAccountLookups
	{
		public static class Constants
		{
			public static class ELASectionTypes
			{
				public static class Descriptions
				{
					public static MultilingualString Liabilities
					{
						get { return ResString.GetMultilingualString("5793F49A-09E7-4F07-A8B5-2AC1375286DA", "(5) Liabilities"); }
					}
					public static MultilingualString Assets
					{
						get { return ResString.GetMultilingualString("69DE0F9A-7421-4835-99EA-E8FB7751028D", "(6) Assets"); }
					}
				}
			}
		}

		public BaseAlternateGLAccountLookups(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public CodeDescriptionPairList AccountTypeList
		{
			get
			{
				var fAccountTypeList = new CodeDescriptionPairList();
				fAccountTypeList.AddPair(Core.Constants.AccountType.Note, Res.GetString("B4FEE4A6-E04B-4EA6-880E-9F32C416268C", "Note"));
				fAccountTypeList.AddRange(BSHPAndLAccountTypeList);
				fAccountTypeList.AddRange(WithoutParentAccountTypeList);

				return fAccountTypeList;
			}
		}

		public CodeDescriptionPairList BSHPAndLAccountTypeList
		{
			get
			{
				var fBSHPAndLAccountTypeList = new CodeDescriptionPairList();
				fBSHPAndLAccountTypeList.AddPair(Core.Constants.AccountType.BalanceSheetAccount, Res.GetString("91466119-1C79-46F2-AECF-63E081CB4C11", "Balance Sheet"));
				fBSHPAndLAccountTypeList.AddPair(Core.Constants.AccountType.ProfitAndLossAccount, Res.GetString("48AED45D-0C23-455D-96A2-6AD7C37C1354", "Profit & Loss"));

				return fBSHPAndLAccountTypeList;
			}
		}

		public CodeDescriptionPairList WithoutParentAccountTypeList => AccountingMasterFilesConstants.WithoutParentAccountTypeList;

		public CodeDescriptionPairList DebitCreditList => new CodeDescriptionPairList(OLookUpEditType.DebitCredit);

		public CodeDescriptionPairList ReportSectionList
		{
			get
			{
				var fReportSectionList = new CodeDescriptionPairList();
				fReportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, AccGLHeader.Constants.SectionTypes.Descriptions.TradingStatement);
				fReportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.Overheads, AccGLHeader.Constants.SectionTypes.Descriptions.Overheads);
				fReportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.ProfitAndLossAppropriation, AccGLHeader.Constants.SectionTypes.Descriptions.ProfitAndLossAppropriation);
				fReportSectionList.AddPair(AccGLHeader.Constants.SectionTypes.Codes.OwnersEquity, AccGLHeader.Constants.SectionTypes.Descriptions.OwnersEquity);

				return fReportSectionList;
			}
		}

		public ReadOnlyCodeDescriptionPairList StatisticalUnitsList => ObjectFactory.Get<IAccounting>().Registry?.NoteGLAccountsStatisticalUnitsofMeasurement(GlbCompany.CurrentCompany.PK.ToGuid()) as ReadOnlyCodeDescriptionPairList;

		public CodeDescriptionPairList CashFlowTypeList
		{
			get
			{
				var fCashFlowTypeList = new CodeDescriptionPairList();
				foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
				{
					fCashFlowTypeList.AddPair(cashFlowActivity.Code, cashFlowActivity.Description);
				}

				return fCashFlowTypeList;
			}
		}

		public AccAlternateGLAccountCollection AlternateNums
		{
			get { return new AccAlternateGLAccountCollection(Factory, new ZQuery(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Alternate)); }
		}

		public AccAlternateGLAccountCollection ConsolidationNums
		{
			get { return new AccAlternateGLAccountCollection(Factory, new ZQuery(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Consolidation)); }
		}

		public AccAlternateGLAccountCollection HeaderDependsOnTotals
		{
			get { return new AccAlternateGLAccountCollection(Factory, new ZQuery(AccAlternateGLAccountSchema.AGA_AccountType, Core.Constants.AccountType.Total)); }
		}

		public AccAlternateGLAccountCollection PercentNums
		{
			get { return new AccAlternateGLAccountCollection(Factory, new ZQuery(AccAlternateGLAccountSchema.AGA_AccountType, new List<string>() { Core.Constants.AccountType.Consolidation, Core.Constants.AccountType.Total })); }
		}

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		BusinessObjectFactory factory;
	}
}
