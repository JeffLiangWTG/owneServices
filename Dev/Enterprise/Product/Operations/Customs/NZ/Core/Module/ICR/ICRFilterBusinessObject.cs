using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Module
{
	public class ICRFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			var filter = filters.AddNumberFilter("Consol ID", GetConsolIDQuery);
			filter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.ICRFilterBusinessObject|ConsolID", "Consol ID");
			filter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (GetConsolIDQuery)
			filters.AddNumberFilter("MAWB/Bill of lading", GetMasterBillNoQuery)
				.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.ICRFilterBusinessObject|MAWBBillOfLading", "MAWB/Bill of lading");

			ModuleTextFilter entryTypeFilter = filters.AddTextFilter("Entry Type", CusEntryNumSchema.CE_EntryType);
			entryTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Enterprise.Customs.NZ.Module.ICRFilterBusinessObject|EntryType", "Entry Type");
			entryTypeFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			entryTypeFilter.Property = CusEntryNumberTypeList.Codes.ICRNumber;

			return filters;
		}

		#region Filters

		ZQuery GetConsolIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			//TODO: Needs to work with multiple values.
			ZQuery query = new ZQuery();

			ZString consolID = value;
			if (consolID.Length < 9 && !consolID.Contains('C'))
			{
				consolID = "C" + consolID.PadLeft(8, '0');
			}

			var consol = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, consolID);
			AddCountry(query);

			ZGuid consolPK = consol == null ? ZGuid.Empty : consol.PK;
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, consolPK);

			return query;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		ZQuery GetMasterBillNoQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery query = new ZQuery();

			ZString masterBillNumber = value;
			masterBillNumber = masterBillNumber.Replace("-", "");

			BusinessObject[] consols = Factory.Load(typeof(ForwardingConsol), new ZQuery().AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_MasterBillNum, SQLComparisonOperator.Equal, masterBillNumber));
			if (consols.Length == 0)
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				foreach (BusinessObject consol in consols)
				{
					ZQuery oneConsolQuery = new ZQuery(CusEntryNumSchema.CE_ParentID, consol.PK);
					AddCountry(oneConsolQuery);
					query.AddToFilter(oneConsolQuery, JoinCondition.Or);
				}
			}

			return query;
		}

		void AddCountry(ZQuery query)
		{
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		#endregion
	}
}
