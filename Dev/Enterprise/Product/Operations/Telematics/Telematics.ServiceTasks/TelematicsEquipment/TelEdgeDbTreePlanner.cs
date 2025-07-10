using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	class TelEdgeDbTreePlanner : IDbTreePlanner
	{
		internal TelEdgeDbTreePlanner()
		{
		}

		internal TelEdgeDbTreePlanner(BusinessObjectFactory businessObjectFactory)
		{
			this.businessObjectFactory = businessObjectFactory ?? throw new ArgumentNullException(nameof(businessObjectFactory));
		}

		public bool TryFlattenTree(string hardwareId, string equipmentType, DateTimeOffset time, out IList<TelEdgeEquipmentTreeNode> dbTree)
		{
			return TryFlattenTree(businessObjectFactory, hardwareId, equipmentType, time, out dbTree);
		}

		public bool TryFlattenTree(BusinessObjectFactory factory, string hardwareId, string equipmentType, DateTimeOffset time, out IList<TelEdgeEquipmentTreeNode> dbTree)
		{
			var @params = new ZSqlParameterCollection();
			@params.Add("@hardwareId", hardwareId, TelSubEquipmentSchema.TSE_Id);
			@params.Add((NoResString)"@time", time, TelEdgeSchema.TE_StartTime);
			@params.Add("@relationshipType", TelEdgeRelationshipTypes.Codes.HW, TelEdgeSchema.TE_RelationshipType);
			if (!DbTreeIsValid(factory, hardwareId, time, out var rootNodes))
			{
				CloseInvalidEdges(factory, rootNodes);
				dbTree = null;
				return false;
			}

			var equipment = rootNodes.SingleOrDefault();
			if (equipment == default)
			{
				dbTree = new List<TelEdgeEquipmentTreeNode>();
				return true;
			}

			@params.Add("@equipmentPk", equipment.PK, TelSubEquipmentSchema.PK);
			var hardwareTree = new DynamicBusinessObjectCollection<TelEdgeEquipmentTreeNode>(factory);
			hardwareTree.Load($@"
			WITH equipmentTree AS(
				SELECT {AutoTelEdge.Schema.PK},
					{AutoTelEdge.Schema.TE_EntityTableCodeFrom},
					{AutoTelEdge.Schema.TE_EntityIdFrom},
					{AutoTelEdge.Schema.TE_EntityTableCodeTo},
					{AutoTelEdge.Schema.TE_EntityIdTo},
					{AutoTelEdge.Schema.TE_StartTime},
					{AutoTelEdge.Schema.TE_EndTime}
				FROM dbo.TelEdgeGrowTree('{equipmentType}', @equipmentPk, @time)
			),
			configTree AS(
				SELECT 
					TET_TelEdgePK = {AutoTelEdge.Schema.PK},
					TET_ParentTableCode = {AutoTelEdge.Schema.TE_EntityTableCodeFrom},
					TET_ParentPK = {AutoTelEdge.Schema.TE_EntityIdFrom},
					TET_ChildTableCode = {AutoTelEdge.Schema.TE_EntityTableCodeTo},
					TET_ChildPK = {AutoTelEdge.Schema.TE_EntityIdTo},
					TET_StartTime = {AutoTelEdge.Schema.TE_StartTime},
					TET_EndTime = {AutoTelEdge.Schema.TE_EndTime},
					TET_ParentConfig = parent.{AutoTelSubEquipment.Schema.TSE_Configuration},
					TET_ChildConfig = child.{AutoTelSubEquipment.Schema.TSE_Configuration},
					TET_ChildId = child.{AutoTelSubEquipment.Schema.TSE_Id},
					TET_ParentId = parent.{AutoTelSubEquipment.Schema.TSE_Id},
					TET_ChildType = child.{AutoTelSubEquipment.Schema.TSE_Type},
					TET_ParentType = parent.{AutoTelSubEquipment.Schema.TSE_Type}
				FROM equipmentTree
				LEFT JOIN [dbo].[{AutoTelSubEquipment.Schema.TableName}] child ON equipmentTree.{AutoTelEdge.Schema.TE_EntityIdTo} = child.{AutoTelSubEquipment.Schema.PK}
				LEFT JOIN [dbo].[{AutoTelSubEquipment.Schema.TableName}] parent ON equipmentTree.{AutoTelEdge.Schema.TE_EntityIdFrom} = parent.{AutoTelSubEquipment.Schema.PK}
			) SELECT TET_TelEdgePK,
				TET_ParentTableCode,
				TET_ParentPK,
				TET_ChildTableCode,
				TET_ChildPK,
				TET_StartTime,
				TET_EndTime,
				TET_ParentConfig,
				TET_ChildConfig,
				TET_ChildId,
				TET_ParentId,
				TET_ChildType,
				TET_ParentType
			FROM configTree
			", @params);

			dbTree = hardwareTree.ToList();
			return true;
		}

		static bool DbTreeIsValid(BusinessObjectFactory factory, string hardwareId, DateTimeOffset time, out IEnumerable<TelSubEquipment> rootNodes)
		{
			var query = new ZDBOnlyQuery(typeof(TelSubEquipment));
			query.AddToFilter(TelSubEquipmentSchema.TSE_Id, hardwareId);

			var subQuery = new ZDBOnlySubQuery(typeof(TelEdge), TelEdgeSchema.TE_EntityIdFrom);
			subQuery.AddToFilter(TelEdgeSchema.TE_RelationshipType, TelEdgeRelationshipTypes.Codes.HW);

			var timeQuery = new ZQuery(TelEdgeSchema.TE_StartTime, SQLComparisonOperator.LessThanOrEqualTo, time);
			timeQuery.AddToFilter(
				new ZQuery(
					new ZQuery(TelEdgeSchema.TE_EndTime, null),
					JoinCondition.Or,
					new ZQuery(TelEdgeSchema.TE_EndTime, SQLComparisonOperator.GreaterThan, time)));
			subQuery.AddToFilter(timeQuery);

			query.AddSubQuery(subQuery, JoinCondition.And);

			var equipmentList = factory.Load<TelSubEquipment>(query)
				.Distinct()
				.ToList();

			rootNodes = equipmentList;
			return rootNodes.Count() <= 1;
		}

		public bool CanAddEntry(string hardwareId, DateTimeOffset time)
		{
			return 1 == Db.Connection.ExecuteScalar<int>($@"
			WITH edgeCount AS(
				SELECT 
					{AutoTelSubEquipment.Schema.PK},
					{AutoTelEdge.Schema.TE_StartTime}
				FROM dbo.{AutoTelSubEquipment.Schema.TableName}
				INNER JOIN {TelEdgeSchema.Constants.SqlSchemaName}.{TelEdgeSchema.Constants.TableName} edge ON {AutoTelSubEquipment.Schema.PK} = {AutoTelEdge.Schema.TE_EntityIdFrom}
				AND {AutoTelSubEquipment.Schema.TSE_Id}=@hardwareId
				AND {AutoTelEdge.Schema.TE_RelationshipType} = @relationshipType
			),
			result AS(
				SELECT
					COUNT(*) as total,
					sum(case when {AutoTelEdge.Schema.TE_StartTime} < @time then 1 else 0 end) as olderCount
				FROM edgeCount
			) SELECT
				IIF(total = 0 OR olderCount != 0, 1, 0)
			FROM result
			", command =>
			{
				command.AddParameterBasedOnDbColumn("@hardwareId", hardwareId, TelSubEquipmentSchema.TSE_Id);
				command.AddParameterBasedOnDbColumn((NoResString)"@time", time, TelEdgeSchema.TE_StartTime);
				command.AddParameterBasedOnDbColumn("@relationshipType", TelEdgeRelationshipTypes.Codes.HW, TelEdgeSchema.TE_RelationshipType);
			});
		}

		public IDictionary<string, SimpleSubEquipment> GetSubEquipmentFromTree(BusinessObjectFactory factory, string deviceId, DateTimeOffset dateTimeOffset, string equipmentType)
		{
			if (TryFlattenTree(factory, deviceId, TelEdgeEntityTableCodes.Codes.TSE, dateTimeOffset, out var equipmentTree))
			{
				var equipmentPks = equipmentTree
					.Where(edge => edge.TET_ChildType == equipmentType)
					.Select(node => new SimpleSubEquipment(node.TET_ChildPK, node.TET_ChildId, node.TET_ChildType))
					.ToDictionary(node => node.Id);
				return equipmentPks;
			}

			return new Dictionary<string, SimpleSubEquipment>();
		}

		void CloseInvalidEdges(BusinessObjectFactory factory, IEnumerable<TelSubEquipment> rootNodes)
		{
			var edges = factory.Load<TelEdge>(
				new ZQuery(
					new ZQuery(TelEdgeSchema.TE_EntityIdFrom, rootNodes.Select(nodes => nodes.PK)),
					JoinCondition.And,
					new ZQuery(TelEdgeSchema.TE_EndTime, SQLComparisonOperator.Equal, null))
				{
					OrderBy = $"{TelEdgeSchema.Constants.TE_StartTime} DESC",
				});

			foreach (var edge in edges)
			{
				if (edge.TE_EntityIdFrom == edges[0].TE_EntityIdFrom)
				{
					continue;
				}

				edge.TE_EndTime = edges[0].TE_StartTime;
			}
		}

		readonly BusinessObjectFactory businessObjectFactory;
	}
}
