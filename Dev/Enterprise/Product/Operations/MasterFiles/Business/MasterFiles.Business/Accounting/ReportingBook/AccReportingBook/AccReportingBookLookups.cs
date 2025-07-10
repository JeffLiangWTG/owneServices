using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccReportingBookLookups : AutoAccReportingBookLookups
	{
		public AccReportingBookLookups(AutoAccReportingBook parent) : base(parent)
		{
			Parent = parent as AccReportingBook;
		}

		new AccReportingBook Parent { get; }

		public override AccAlternateChartCollection AlternateCharts
		{
			get
			{
				var query = new ZQuery(AccAlternateChartSchema.AAC_GC_Company, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(JoinCondition.Or, AccAlternateChartSchema.AAC_GC_Company, null);
				return new AccAlternateChartCollection(Factory, query);
			}
		}

		#region ExRateTypes

		public CodeDescriptionPairList ExRateTypes => GetExchangeRateTypeList();

		public CodeDescriptionPairList GetExchangeRateTypeList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			var applicableRates = new CodeDescriptionPairList();
			applicableRates.AddRange(AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypes.Value);
			applicableRates.AddRange(Parent.ARB_IsGlobal ? new CodeDescriptionPairList() : AccountingMasterFilesRegistry.Instance.CurrencyExchangeRateTypesCompanySpecific.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			foreach (var item in applicableRates.Cast<CodeDescriptionBool>().Where(x => x.Bool))
			{
				switch (item.Code)
				{
					case Core.Constants.ExchangeRateTypes.Code.GlobalCreditControl:
						if (Env.Security.GCBExchangeRateUpdate.IsAllowed)
						{
							list.AddPair(item.Code, item.Description);
						}
						break;
					case Core.Constants.ExchangeRateTypes.Code.CustomsRate:
					case Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary:
					case Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate:
					case Core.Constants.ExchangeRateTypes.Code.IATARate:
						break;
					default:
						list.AddPair(item.Code, item.Description);
						break;
				}
			}

			return list;
		}

		#endregion

		#region PresentationCategoryList

		public CodeDescriptionPairList PresentationCategoryList => GetPresentationCategoryList();

		public CodeDescriptionPairList GetPresentationCategoryList()
		{
			return ObjectFactory.Get<IAccounting>().GLPresentationJournalCategoriesList(Parent.ARB_IsGlobal ? Guid.Empty : GlbCompany.CurrentCompany.PK.ToGuid()) as CodeDescriptionPairList;
		}

		#endregion
	}
}
