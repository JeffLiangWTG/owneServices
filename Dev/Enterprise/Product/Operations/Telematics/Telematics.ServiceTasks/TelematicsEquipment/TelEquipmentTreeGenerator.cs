using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using Microsoft.XmlDiffPatch;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	class TelEquipmentTreeGenerator : IDbTreeGenerator
	{
		public TelEquipmentTreeGenerator(BusinessObjectFactory businessObjectFactory, IJsonPlanner jsonEquipmentPlanner, IDbTreePlanner dbEquipmentPlanner, ILogger logger)
			: this(jsonEquipmentPlanner, dbEquipmentPlanner, logger)
		{
			this.businessObjectFactory = businessObjectFactory ?? throw new ArgumentNullException(nameof(businessObjectFactory));
		}

		internal TelEquipmentTreeGenerator(IJsonPlanner jsonEquipmentPlanner, IDbTreePlanner dbEquipmentPlanner, ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.jsonEquipmentPlanner = jsonEquipmentPlanner ?? throw new ArgumentNullException(nameof(jsonEquipmentPlanner));
			this.dbEquipmentPlanner = dbEquipmentPlanner ?? throw new ArgumentNullException(nameof(dbEquipmentPlanner));
			xmlDiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder | XmlDiffOptions.IgnoreWhitespace) { Algorithm = XmlDiffAlgorithm.Fast };
		}

		public void GenerateTree(string hardwareId, DateTimeOffset time, string vehicleInfoJson)
		{
			GenerateTree(businessObjectFactory, hardwareId, time, vehicleInfoJson);
		}

		public void GenerateTree(BusinessObjectFactory factory, string hardwareId, DateTimeOffset time, string vehicleInfoJson)
		{
			if (!dbEquipmentPlanner.CanAddEntry(hardwareId, time))
			{
				logger.Log(LogType.Information, $"Device: {hardwareId}, sent configuration with invalid timestamp");
				return;
			}
			if (!dbEquipmentPlanner.TryFlattenTree(factory, hardwareId, TelEdgeEntityTableCodes.Codes.TSE, time, out var edgesFromDb))
			{
				logger.Log(LogType.Error, $"Device with hardwareId: {hardwareId} has invalid database entries");
				return;
			}
			var edgesFromConfig = jsonEquipmentPlanner.FlattenJson(hardwareId, vehicleInfoJson);
			var vehicleKey = GetKey(hardwareId, "RQ");
			edgesFromConfig.Remove(vehicleKey);

			var equipmentChanges = ProcessEdgesForModifiedEquipment(factory, edgesFromDb, edgesFromConfig, vehicleKey, time);

			GenerateLeftoverConfigEdges(factory, edgesFromConfig, equipmentChanges.EquipmentReferences, equipmentChanges.NewEdges, time);
			ReplaceNodeReferences(equipmentChanges.NodesToReplace.Distinct(), equipmentChanges.NewEdges.Values);
			CloseEdges(factory, equipmentChanges.EdgesToClose.Distinct(), time);
		}

		TelEquipmentChanges ProcessEdgesForModifiedEquipment(BusinessObjectFactory factory, IList<TelEdgeEquipmentTreeNode> edgesFromDb, EquipmentFlatJson edgesFromConfig, TelSubEquipmentKey vehicleReference, DateTimeOffset time)
		{
			var equipmentChanges = new TelEquipmentChanges();

			foreach (TelEdgeEquipmentTreeNode dbEdge in edgesFromDb)
			{
				var childKey = GetKey(dbEdge.TET_ChildId, dbEdge.TET_ChildType);
				if (edgesFromConfig.TryGetValue(childKey, out var fromToPair))
				{
					ProcessExistingChildEquipmentChanges(factory, equipmentChanges, dbEdge, fromToPair, childKey, time);

					if (dbEdge.TET_ParentId == fromToPair.from.id &&
						dbEdge.TET_ParentType == fromToPair.from.type)
					{
						if (dbEdge.TET_ParentId == vehicleReference.id)
						{
							ProcessExistingVehicleEquipmentChanges(factory, equipmentChanges, dbEdge, fromToPair, childKey, time);
						}
						else
						{
							ProcessExistingParentEquipmentChanges(factory, equipmentChanges, dbEdge, fromToPair, childKey, time);
						}
					}
					edgesFromConfig.Remove(childKey);
				}
				else
				{
					equipmentChanges.EdgesToClose.Add(dbEdge.TET_TelEdgePK);
				}
			}
			return equipmentChanges;
		}

		void ProcessExistingVehicleEquipmentChanges(BusinessObjectFactory factory, TelEquipmentChanges equipmentChanges, TelEdgeEquipmentTreeNode dbEdge, (TelSubEquipmentData from, TelSubEquipmentData to) fromToPair, TelSubEquipmentKey childKey, DateTimeOffset time)
		{
			var vehicleKey = GetKey(dbEdge.TET_ParentId, dbEdge.TET_ParentType);
			if (ConfigsAreEqual(dbEdge.TET_ParentConfig, fromToPair.from.configuration))
			{
				equipmentChanges.EquipmentReferences[GetKey(dbEdge.TET_ParentId, dbEdge.TET_ParentType)] = dbEdge.TET_ParentPK;
			}
			else
			{
				if (!equipmentChanges.EquipmentReferences.TryGetValue(vehicleKey, out var subEquipmentPk))
				{
					var subEquipment = GenerateSubEquipment(factory, fromToPair.from);
					subEquipmentPk = subEquipment.PK;
					equipmentChanges.EquipmentReferences[vehicleKey] = subEquipmentPk;
					equipmentChanges.NodesToReplace.Add((dbEdge.TET_ParentPK, subEquipmentPk));
				}
				var endTime = dbEdge.TET_StartTime < time && dbEdge.TET_EndTime > time ? dbEdge.TET_EndTime : null;
				if (!equipmentChanges.NewEdges.TryGetValue(childKey, out var edge))
				{
					equipmentChanges.NewEdges[childKey] = CreateEdge(factory, subEquipmentPk, dbEdge.TET_ChildPK, time, endTime);
				}
				equipmentChanges.EdgesToClose.Add(dbEdge.TET_TelEdgePK);
			}
		}

		void ProcessExistingParentEquipmentChanges(BusinessObjectFactory factory, TelEquipmentChanges equipmentChanges, TelEdgeEquipmentTreeNode dbEdge, (TelSubEquipmentData from, TelSubEquipmentData to) fromToPair, TelSubEquipmentKey childKey, DateTimeOffset time)
		{
			if (ConfigsAreEqual(dbEdge.TET_ParentConfig, fromToPair.from.configuration))
			{
				equipmentChanges.EquipmentReferences[GetKey(dbEdge.TET_ParentId, dbEdge.TET_ParentType)] = dbEdge.TET_ParentPK;
			}
			else if (!equipmentChanges.NewEdges.ContainsKey(childKey))
			{
				var childPk = equipmentChanges.EquipmentReferences[childKey];
				var parentPk = equipmentChanges.EquipmentReferences[GetKey(fromToPair.from.id, fromToPair.from.type)];
				var endTime = dbEdge.TET_StartTime < time && dbEdge.TET_EndTime > time ? dbEdge.TET_EndTime : null;
				equipmentChanges.NewEdges[childKey] = CreateEdge(factory, parentPk, childPk, time, endTime);
				equipmentChanges.EdgesToClose.Add(dbEdge.TET_TelEdgePK);
			}
		}

		void ProcessExistingChildEquipmentChanges(BusinessObjectFactory factory, TelEquipmentChanges equipmentChanges, TelEdgeEquipmentTreeNode dbEdge, (TelSubEquipmentData from, TelSubEquipmentData to) fromToPair, TelSubEquipmentKey childKey, DateTimeOffset time)
		{
			if (ConfigsAreEqual(dbEdge.TET_ChildConfig, fromToPair.to.configuration))
			{
				equipmentChanges.EquipmentReferences[childKey] = dbEdge.TET_ChildPK;
			}
			else
			{
				var subEquipment = GenerateSubEquipment(factory, fromToPair.to);
				equipmentChanges.EquipmentReferences[childKey] = subEquipment.PK;
				equipmentChanges.NodesToReplace.Add((dbEdge.TET_ChildPK, subEquipment.PK));
				var endTime = dbEdge.TET_StartTime < time && dbEdge.TET_EndTime > time ? dbEdge.TET_EndTime : null;
				equipmentChanges.NewEdges[childKey] = CreateEdge(factory, dbEdge.TET_ParentPK, subEquipment.PK, time, endTime);
				equipmentChanges.EdgesToClose.Add(dbEdge.TET_TelEdgePK);
			}
		}

		void GenerateLeftoverConfigEdges(BusinessObjectFactory factory, EquipmentFlatJson edgesFromConfig, IDictionary<TelSubEquipmentKey, ZGuid> equipmentReferences, IDictionary<TelSubEquipmentKey, TelEdge> newEdges, DateTimeOffset time)
		{
			foreach (var edge in edgesFromConfig)
			{
				ZGuid parentPk;
				if (!equipmentReferences.TryGetValue(GetKey(edge.Value.from.id, edge.Value.from.type), out var fromPk))
				{
					var parentEquipment = GenerateSubEquipment(factory, edge.Value.from);
					var parentKey = GetKey(parentEquipment.TSE_Id, parentEquipment.TSE_Type);
					equipmentReferences[parentKey] = parentEquipment.PK;
					parentPk = parentEquipment.PK;
				}
				else
				{
					parentPk = fromPk;
				}
				var childEquipment = GenerateSubEquipment(factory, edge.Value.to);
				var childKey = GetKey(childEquipment.TSE_Id, childEquipment.TSE_Type);
				equipmentReferences[childKey] = childEquipment.PK;
				newEdges[childKey] = CreateEdge(factory, parentPk, childEquipment.PK, time);
			}
		}

		static void ReplaceNodeReferences(IEnumerable<(ZGuid oldPk, ZGuid newPk)> nodesToReplace, IEnumerable<TelEdge> newEdges)
		{
			foreach (var node in nodesToReplace)
			{
				foreach (var edge in newEdges)
				{
					edge.TE_EntityIdTo = edge.TE_EntityIdTo == node.oldPk ? node.newPk : edge.TE_EntityIdTo;
					edge.TE_EntityIdFrom = edge.TE_EntityIdFrom == node.oldPk ? node.newPk : edge.TE_EntityIdFrom;
				}
			}
		}

		static TelSubEquipmentKey GetKey(string id, string type)
		{
			return new TelSubEquipmentKey()
			{
				id = id,
				type = type,
			};
		}

		static XmlReader GetXmlReader(string xml)
		{
			return XmlReader.Create(new StringReader(xml));
		}

		bool ConfigsAreEqual(string config1, string config2)
		{
			return config1 == config2 || xmlDiff.Compare(GetXmlReader(config1), GetXmlReader(config2));
		}

		TelSubEquipment GenerateSubEquipment(BusinessObjectFactory factory, TelSubEquipmentData data)
		{
			var subEquipment = factory.New<TelSubEquipment>();
			subEquipment.TSE_Id = data.id;
			subEquipment.TSE_Type = data.type;
			subEquipment.TSE_Configuration = data.configuration;
			return subEquipment;
		}

		static TelEdge CreateEdge(BusinessObjectFactory factory, ZGuid parent, ZGuid child, DateTimeOffset startTime, DateTimeOffset? endTime = null)
		{
			var newEdge = factory.New<TelEdge>();
			newEdge.TE_StartTime = startTime;
			newEdge.TE_EntityTableCodeFrom = TelEdgeEntityTableCodes.Codes.TSE;
			newEdge.TE_EntityTableCodeTo = TelEdgeEntityTableCodes.Codes.TSE;
			newEdge.TE_EntityIdTo = child;
			newEdge.TE_EntityIdFrom = parent;
			newEdge.TE_RelationshipType = TelEdgeRelationshipTypes.Codes.HW;
			if (endTime != null)
			{
				newEdge.TE_EndTime = (DateTimeOffset)endTime;
			}
			return newEdge;
		}

		static void CloseEdges(BusinessObjectFactory factory, IEnumerable<ZGuid> nodesToClose, DateTimeOffset closingTime)
		{
			var query = new ZQuery(TelEdgeSchema.PK, nodesToClose);
			var edges = factory.Load<TelEdge>(query);
			foreach (var edge in edges)
			{
				edge.TE_EndTime = closingTime;
			}
		}

		class TelEquipmentChanges
		{
			public TelEquipmentChanges()
			{
				EdgesToClose = new List<ZGuid>();
				NodesToReplace = new List<(ZGuid oldPk, ZGuid newPk)>();
				NewEdges = new Dictionary<TelSubEquipmentKey, TelEdge>();
				EquipmentReferences = new Dictionary<TelSubEquipmentKey, ZGuid>();
			}
			public IList<ZGuid> EdgesToClose { get; }
			public IList<(ZGuid oldPk, ZGuid newPk)> NodesToReplace { get; }
			public IDictionary<TelSubEquipmentKey, TelEdge> NewEdges { get; }
			public IDictionary<TelSubEquipmentKey, ZGuid> EquipmentReferences { get; }
		}

		readonly ILogger logger;
		readonly IJsonPlanner jsonEquipmentPlanner;
		readonly IDbTreePlanner dbEquipmentPlanner;
		readonly XmlDiff xmlDiff;
		readonly BusinessObjectFactory businessObjectFactory;
	}
}
