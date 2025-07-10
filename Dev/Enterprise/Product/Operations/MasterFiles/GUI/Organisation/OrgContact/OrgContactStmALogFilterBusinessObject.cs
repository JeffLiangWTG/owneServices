using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using ResString = Enterprise.MasterFiles.GUI.ResString;

namespace Enterprise.MasterFiles.Module
{
	public class OrgContactStmALogFilterBusinessObject : ZStmALogFilterBusinessObject
	{
		public OrgContactStmALogFilterBusinessObject()
		{
			LogsToShow = LogsToShow.All;
		}

		protected override bool ShowBaseFiltersWithoutMaster => true;
		ModuleDateFilter postedTimeFilter;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = base.GetModuleFiltersCore();
			var tableFilter = result.AddTextFilter("Contact Table", GetTableQuery);
			tableFilter.MultilingualDescription = ResString.GetMultilingualString("3B5D8484-118F-462F-B1C5-1881CABC96F7", "Contact Table");
			tableFilter.Property = OrgContactSchema.Constants.TableName;
			tableFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			postedTimeFilter = result[Schema.PostedTime] as ModuleDateFilter;
			if (postedTimeFilter != null)
			{
				postedTimeFilter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
				postedTimeFilter.Property1Validation += info =>
				{
					if (ShouldValidatePostedTime)
					{
						MandatoryValidation.CheckEntered(info);
					}
				};

				postedTimeFilter.Property2Validation += info =>
				{
					if (ShouldValidatePostedTime)
					{
						MandatoryValidation.CheckEntered(info);
					}
				};

				postedTimeFilter.PropertySearchInfo.AdditionalValidation += delegate()
				{ MandatoryValidation.CheckEntered(postedTimeFilter.PropertySearchInfo); };
			}

			return result;
		}

		bool ShouldValidatePostedTime
		{
			get
			{
				return (postedTimeFilter.IsPropertySearchValid
					&& (postedTimeFilter.IsPropertySearchUsingSpecifiedDateRange
						|| postedTimeFilter.IsPropertySearchUsingSpecifiedDateTimeRange
						|| postedTimeFilter.IsPropertySearchUsingSpecifiedDayOffsetRange));
			}
		}

		ZQuery GetTableQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(StmALogSchema.SL_Table, SQLComparisonOperator.Equal, value);
		}
	}
}
