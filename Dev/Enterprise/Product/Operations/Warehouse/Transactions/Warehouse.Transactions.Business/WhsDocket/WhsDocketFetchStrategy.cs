using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsDocketFetchStrategy(WhsDocket docket)
			: base(docket)
		{
		}

		protected WhsDocket Docket => (WhsDocket)BusinessObject;

		//This is tested in the FilterControlTests
		protected sealed override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			AddBaseFetchHintsForView(GetFetchForViewStrategyFlags(columns));

			foreach (var column in columns)
			{
				AddDocketSpecificFetchHintsForView(column.ColumnName);
			}
		}

		void AddBaseFetchHintsForView(FetchStrategyFlag fetchStrategyFlags)
		{
			if (fetchStrategyFlags.HasFlag(FetchStrategyFlag.RequireClient))
			{
				AddFetchHintForClient();
			}

			if (fetchStrategyFlags.HasFlag(FetchStrategyFlag.RequireWarehouse))
			{
				AddFetchHintForWarehouse();
			}

			if (fetchStrategyFlags.HasFlag(FetchStrategyFlag.RequireContainer))
			{
				AddFetchHintForContainer();
			}

			if (fetchStrategyFlags.HasFlag(FetchStrategyFlag.RequireJobDocAddress))
			{
				AddFetchHintForJobDocAddress();
			}

			if (fetchStrategyFlags.HasFlag(FetchStrategyFlag.RequireLines))
			{
				AddFetchHintForLines();
			}

			if (fetchStrategyFlags.HasFlag(FetchStrategyFlag.RequireWorkflowItems))
			{
				AddFetchHintForWorkFlowItems();
			}

			if (fetchStrategyFlags.HasFlag(FetchStrategyFlag.RequireOrgSupplierPart))
			{
				AddFetchHintForOrgSupplierPart();
			}
		}

		FetchStrategyFlag GetFetchForViewStrategyFlags(TableColumn[] columns)
		{
			var fetchStrategyFlags = FetchStrategyFlag.None;

			foreach (var column in columns)
			{
				switch (column.ColumnName)
				{
					case WhsDocketSchema.Constants.WD_OH_Client:
					case nameof(WhsDocket.ClientName):
						fetchStrategyFlags |= FetchStrategyFlag.RequireClient;
						break;
					case WhsDocketSchema.Constants.WD_WW_Whs:
						fetchStrategyFlags |= FetchStrategyFlag.RequireWarehouse;
						break;
					case nameof(WhsDocket.ContainerType):
					case nameof(WhsDocket.ContainerID):
						fetchStrategyFlags |= FetchStrategyFlag.RequireContainer;
						break;
					case WhsDocket.Schema.WD_TotalUnitsFromLines:
						fetchStrategyFlags |= FetchStrategyFlag.RequireLines;
						break;
					default:
						if (column.ColumnName.Contains(nameof(WhsDocket.WorkflowItems)))  // table column name constants are not multilingual
						{
							fetchStrategyFlags |= FetchStrategyFlag.RequireWorkflowItems;
						}
						break;
				}

				fetchStrategyFlags |= GetDocketSpecificFetchForViewStrategyFlag(column.ColumnName);
			}

			return fetchStrategyFlags;
		}

		protected virtual FetchStrategyFlag GetDocketSpecificFetchForViewStrategyFlag(string columnName)
		{
			return FetchStrategyFlag.None;
		}

		protected virtual void AddDocketSpecificFetchHintsForView(string columnName)
		{
		}

		void AddFetchHintForWorkFlowItems()
		{
			Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, Docket.PK);
		}

		void AddFetchHintForLines()
		{
			AddDocketLineFetchHint();
		}

		void AddDocketLineFetchHint()
		{
			Factory.AddFetchHint(WhsDocketLineSchema.WE_WD, Docket.PK);
		}

		void AddFetchHintForContainer()
		{
			Factory.AddFetchHint(typeof(WhsDocketContainer), WhsDocketContainerSchema.WC_WD, Docket.PK);
		}

		void AddFetchHintForOrgSupplierPart()
		{
			var partQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_OP);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WD, Docket.PK);
			partQuery.AddSubQuery(OrgSupplierPartSchema.PK, docketLineSubQuery, JoinCondition.And);
			Factory.AddFetchHint(typeof(OrgSupplierPart), partQuery);
		}

		void AddFetchHintForWarehouse()
		{
			Factory.AddFetchHint(WhsWarehouseSchema.PK, Docket.WD_WW_Whs);
		}

		void AddFetchHintForClient()
		{
			Factory.AddFetchHint(OrgHeaderSchema.PK, Docket.WD_OH_Client);
		}

		void AddFetchHintForJobDocAddress()
		{
			Factory.AddFetchHint(typeof(JobDocAddress), FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, Docket.PK));
		}

		protected ZQuery GoodsHandlingInstructionDocketQuery()
		{
			return GoodsHandlingInstructionQueryCore(WhsDocketSchema.Constants.TableName, Docket.PK);
		}

		protected ZQuery GoodsHandlingInstructionOrgHeaderQuery()
		{
			return GoodsHandlingInstructionQueryCore(OrgHeaderSchema.Constants.TableName, Docket.WD_OH_Client);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Query filter")]
		ZQuery GoodsHandlingInstructionQueryCore(ZString parentTableName, ZGuid parentPk)
		{
			var noteTypeQuery = new ZQuery(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.PUB));
			noteTypeQuery.AddToFilter(JoinCondition.Or, StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.PRV));
			noteTypeQuery.AddToFilter(JoinCondition.Or, StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.INT));
			noteTypeQuery.AddToFilter(JoinCondition.Or, StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.AGV));
			noteTypeQuery.AddToFilter(StmNoteSchema.ST_Table, parentTableName);

			var parentQuery = new ZQuery(StmNoteSchema.ST_ParentID, parentPk);
			parentQuery.AddToFilter(noteTypeQuery);

			var noteQuery = new ZQuery(StmNoteSchema.ST_Description, "Goods Handling Instructions"); // Note description for db query.
			noteQuery.AddToFilter(parentQuery);

			return noteQuery;
		}
	}
}
