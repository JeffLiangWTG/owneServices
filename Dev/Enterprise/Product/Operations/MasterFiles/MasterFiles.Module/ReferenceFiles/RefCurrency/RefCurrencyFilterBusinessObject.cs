using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefCurrencyFilterBusinessObject : FilterStripBusinessObject
	{
		public RefCurrencyFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Code", RefCurrencySchema.RX_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefCurrencyFilter|Code", "Code");
			filters.AddFiltersForTranslatableText("Description", RefCurrencySchema.RX_Desc, typeof(RefCurrency), ResString.GetMultilingualString("MasterFiles|RefCurrencyFilter|Description", "Description"));

			var filter = filters.AddTextFilter("CFX Calculations Status", GetCFXCalculationsStatusQuery, CFXCalculationsStatusList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.DefaultProperty = "ALL";
			filter.MultilingualDescription = ResString.GetMultilingualString("79d13810-55c9-4054-847d-51fc66398f19", "CFX Calculations Status");
		}

		#endregion

		#endregion

		#region Query

		#region GetCFXCalculationsStatusQuery

		ZQuery GetCFXCalculationsStatusQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RefCurrency));

			if (!value.IsEmpty && value != "ALL")
			{
				var excludedCurrencies = AccountingMasterFilesRegistry.Instance.ExcludeCurrencyFromCFXCalculation.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (excludedCurrencies.Count > 0)
				{
					var sqlText = string.Join(", ", excludedCurrencies.Cast<CodeDescriptionBool>().Select(x => x.Code.Replace(x.Code, "'" + x.Code + "'")).ToArray());

					if (value == "EXC")
					{
						query.AddFilterAndZSQLParameterCollection(RefCurrencySchema.RX_Code.Name + " in ( " + sqlText + " )", new ZSqlParameterCollection());
					}
					else if (value == "INC")
					{
						query.AddFilterAndZSQLParameterCollection(RefCurrencySchema.RX_Code.Name + " not in ( " + sqlText + " )", new ZSqlParameterCollection());
					}
				}
				else if (value == "EXC")
				{
					query.IsNoResultQuery = true;
				}
			}
			return query;
		}

		#endregion

		#endregion

		#region Lookups

		#region CFX Calculations Status List

		CodeDescriptionPairList CFXCalculationsStatusList
		{
			get
			{
				if (cFXCalculationsStatusList == null)
				{
					cFXCalculationsStatusList = new CodeDescriptionPairList();

					cFXCalculationsStatusList.AddPair("ALL", Res.GetString("1a5472e0-59ee-479c-99f4-64501e9887fc", "Display all currencies"));
					cFXCalculationsStatusList.AddPair("EXC", Res.GetString("9d5e97e7-c52f-4a80-b137-47974fa9f2b9", "Display currencies excluded from CFX calculations"));
					cFXCalculationsStatusList.AddPair("INC", Res.GetString("9067b3d4-8f30-4aeb-90d7-d119f7707ac8", "Display currencies included in CFX calculations"));
				}

				return cFXCalculationsStatusList;
			}
		}
		CodeDescriptionPairList cFXCalculationsStatusList;

		#endregion

		#endregion

	}
}
