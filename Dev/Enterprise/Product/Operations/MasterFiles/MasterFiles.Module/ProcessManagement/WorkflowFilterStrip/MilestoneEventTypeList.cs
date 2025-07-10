using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class MilestoneEventTypeList : UntranslatableCodeDescriptionPairList
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable reason")]
		public MilestoneEventTypeList(ZString jobType)
			: base("Descriptions defined in the database")
		{
			AddPair("");

			if (!jobType.IsEmpty)
			{
				AddMilestoneTemplateEvents(jobType);
				Add(new CategoryCodeDescriptionPair(ResString.GetMultilingualString("aba77c19-33ff-4ee6-87ea-8d4decd48b14", "Other Events"), ""));
			}

			AddRemainingEvents();
		}

		#region Implementation

		void AddMilestoneTemplateEvents(ZString jobType)
		{
			AddMilestoneTemplateEvents(jobType, Factory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is NameForDebugging.")]
		internal const string TemplateLoaderFactoryName = "Template Loader Factory";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Optimising Performance Issue")]
#if DEBUG
		internal
#endif
		void AddMilestoneTemplateEvents(ZString jobType, BusinessObjectFactory businessObjectFactory)
		{
			var sqlText = FormattableString.Invariant(
$@"
SELECT DISTINCT {ProcessTasksSchema.P9_SE_NKMilestoneEvent.Name} FROM {ProcessTasksSchema.Constants.SqlSchemaName}.{ProcessTasksSchema.Constants.TableName}
WHERE ({ProcessTasksSchema.P9_ParentTableCode.Name} = @p1 and ({ProcessTasksSchema.P9_Type.Name} = @p2 or {ProcessTasksSchema.P9_Type.Name} = @p3))
and ({ProcessTasksSchema.P9_ParentID.Name} IN (SELECT {ProcessTaskTemplateSchema.PK.Name} FROM {ProcessTaskTemplateSchema.Constants.SqlSchemaName}.{ProcessTaskTemplateSchema.Constants.TableName}
WHERE {ProcessTaskTemplateSchema.P0_ProcessType.Name} = @p4 AND {ProcessTaskTemplateSchema.P0_IsActive.Name} = @p5))
");

			var pairs = new List<CodeDescriptionPair>();

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameter("@p1", System.Data.SqlDbType.VarChar, ProcessTaskTemplateSchema.Constants.Prefix);
				command.AddParameter("@p2", System.Data.SqlDbType.VarChar, "MIL");
				command.AddParameter("@p3", System.Data.SqlDbType.VarChar, "TRG");
				command.AddParameter("@p4", System.Data.SqlDbType.VarChar, (string)jobType);
				command.AddParameter("@p5", System.Data.SqlDbType.Bit, true);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var code = (string)reader[ProcessTasksSchema.P9_SE_NKMilestoneEvent.Name];
						var description = (string)Events.GetEventDescriptionFromCode(code);
						pairs.Add(new CodeDescriptionPair(code, description));
					}
				}
			}

			AddPairsIfNotExist(pairs);
			SortByDescription();
			AddPair("");
		}

		void AddRemainingEvents()
		{
			var pairs = Events.All.Cast<Event>()
				.Select(type => new CodeDescriptionPair(type.Code, CustomizableEventList.ContainsCode(type.Code) ? CustomizableEventList[type.Code, System.StringComparison.OrdinalIgnoreCase].Description : type.Description))
				.OrderBy(t => t, new DescriptionComparer());
			AddPairsEvenIfThisWillCauseDuplicateEntries(pairs);
		}

		CodeDescriptionPairList customizableEventList;
		CodeDescriptionPairList CustomizableEventList
		{
			get
			{
				if (customizableEventList == null)
				{
					customizableEventList = new CodeDescriptionPairList();
					var query = new ZQuery(StmEventSchema.SE_IsCustomizable, true);
					query.AddToFilter(StmEventSchema.SE_IsActive, true);
					var customizableEvents = Factory.Load<StmEvent>(query);
					foreach (var item in customizableEvents)
					{
						customizableEventList.AddPair(item.SE_Code, item.SE_DescMultilingual);
					}
				}
				return customizableEventList;
			}
		}

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory() { RefreshEnabled = false, NameForDebugging = TemplateLoaderFactoryName };
				}
				return factory;
			}
		}

		#endregion
	}
}
