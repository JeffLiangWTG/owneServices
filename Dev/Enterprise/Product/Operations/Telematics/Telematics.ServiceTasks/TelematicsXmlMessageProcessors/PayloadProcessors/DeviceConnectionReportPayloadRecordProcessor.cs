using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class DeviceConnectionReportPayloadRecordProcessor : IPayloadRecordProcessor<DeviceHeartbeatPayloadRecord>
	{
		public DeviceConnectionReportPayloadRecordProcessor(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			validEquipmentConfigurations = new Connections(new EquipmentConfigurationEqualityComparer());
		}

		public int Process(BusinessObjectFactory factory, GlbDevice device, DeviceHeartbeatPayloadRecord record)
		{
			var hardwareIds = record?.DeviceHeartbeats
				.Select(heartbeat => heartbeat.DeviceId.ToHexString())
				.ToList();

			if (!TryGetRelatedTelSubEquipment(factory, hardwareIds, out var equipmentList, record.DateTimeOffset))
			{
				logger.Log(LogType.Warning, $"Could not retrieve equipment from db for attached devices [{string.Join(", ", hardwareIds)}]");
				return 0;
			}

			if (!TryGetCombinationCode(equipmentList, validEquipmentConfigurations, out var combinationCode, out var failedCombination))
			{
				logger.Log(LogType.Error, $"Device connection report [{failedCombination}] could not map to a valid combination code");
				return 0;
			}

			var glbCombinationReport = factory.New<GlbDeviceCombinationReport>();
			glbCombinationReport.GDC_CombinationCode = combinationCode;
			glbCombinationReport.GDC_MeasurementTimeUtc = record.DateTimeOffset.UtcDateTime;
			glbCombinationReport.GDC_V3_Device = device.PK;
			return 1;
		}

		static bool TryGetRelatedTelSubEquipment(BusinessObjectFactory factory, IList<string> hardwareIds, out IOrderedEnumerable<TelSubEquipment> equipmentList, DateTimeOffset time)
		{
			if (hardwareIds.Count == 0)
			{
				equipmentList = null;
				return false;
			}

			var zQuery = new ZDBOnlyQuery(typeof(TelSubEquipment));
			zQuery.AddToFilter(TelSubEquipmentSchema.TSE_Id, hardwareIds);

			var subQuery = new ZDBOnlySubQuery(typeof(TelEdge), TelEdgeSchema.TE_EntityIdFrom);
			subQuery.AddToFilter(new ZQuery(TelEdgeSchema.TE_RelationshipType, TelEdgeRelationshipTypes.Codes.HW));

			var timeQuery = new ZQuery(TelEdgeSchema.TE_StartTime, SQLComparisonOperator.LessThanOrEqualTo, time);
			timeQuery.AddToFilter(new ZQuery(new ZQuery(TelEdgeSchema.TE_EndTime, null), JoinCondition.Or, new ZQuery(TelEdgeSchema.TE_EndTime, SQLComparisonOperator.GreaterThan, time)));
			subQuery.AddToFilter(timeQuery);
			zQuery.AddSubQuery(subQuery, JoinCondition.And);

			equipmentList = factory.Load<TelSubEquipment>(zQuery)
				.OrderBy(equipment => hardwareIds.IndexOf(equipment.TSE_Id));

			return equipmentList.Count() == hardwareIds.Count;
		}

		static bool TryGetCombinationCode(
			IOrderedEnumerable<TelSubEquipment> equipmentList,
			IDictionary<IEnumerable<string>, IDictionary<int, string>> validConfigs,
			out string combinationCode,
			out string failedCombination)
		{
			combinationCode = string.Empty;
			failedCombination = string.Empty;
			var configurationList = new List<(string id, string vehicleType, string axleConfig)>();
			foreach (var equipment in equipmentList)
			{
				var doc = new XmlDocument();
				doc.LoadXml(equipment.TSE_Configuration.ToString());
				configurationList.Add((equipment.TSE_Id, doc["Configuration"]["vehicleType"].InnerText.ToLower(), doc["Configuration"]["axleConfig"].InnerText));
			}

			if (!validConfigs.TryGetValue(configurationList.Select(conf => conf.vehicleType).ToList(), out var vehicleTypeCode))
			{
				failedCombination = string.Join(", ", configurationList.Select(configuration => $"{configuration.vehicleType}: {configuration.id}"));
				return false;
			}

			for (var i = 0; i < configurationList.Count; i++)
			{
				if (vehicleTypeCode.TryGetValue(i, out var code))
				{
					combinationCode += code;
				}
				combinationCode += configurationList.ElementAt(i).axleConfig;
			}
			return true;
		}

		class EquipmentConfigurationEqualityComparer : IEqualityComparer<IEnumerable<string>>
		{
			public bool Equals(IEnumerable<string> x, IEnumerable<string> y)
			{
				return x.SequenceEqual(y, StringComparer.InvariantCultureIgnoreCase);
			}

			public int GetHashCode(IEnumerable<string> obj)
			{
				return string.Join("", obj).GetHashCode();
			}
		}

		class Connections : Dictionary<IEnumerable<string>, IDictionary<int, string>>
		{
			#region SuppressResourceStringsCheckRegion
			public Connections(IEqualityComparer<IEnumerable<string>> comparer) : base(comparer)
			{
				Add(new List<string>() { "rigid truck" }, new Dictionary<int, string> { { 0, "R" } });
				Add(new List<string>() { "light vehicle" }, new Dictionary<int, string> { { 0, "L" } });
				Add(new List<string>() { "prime mover" }, new Dictionary<int, string> { { 0, "A" } });
				Add(new List<string>() { "prime mover", "b trailer" }, new Dictionary<int, string> { { 0, "A" } });
				Add(new List<string>() { "prime mover", "a trailer", "b trailer" }, new Dictionary<int, string> { { 0, "B" } });
				Add(new List<string>() { "prime mover", "a trailer", "a trailer", "b trailer" }, new Dictionary<int, string> { { 0, "B" } });
				Add(new List<string>() { "prime mover", "b trailer", "dolly", "b trailer" }, new Dictionary<int, string> { { 0, "A" }, { 2, "T" } });
				Add(new List<string>() { "prime mover", "b trailer", "dolly", "b trailer", "dolly", "b trailer" }, new Dictionary<int, string> { { 0, "A" }, { 2, "T" }, { 4, "T" } });
				Add(new List<string>() { "prime mover", "b trailer", "dolly", "a trailer", "b trailer" }, new Dictionary<int, string> { { 0, "B" }, { 2, "T" }, { 3, "B" } });
				Add(new List<string>() { "prime mover", "a trailer", "b trailer", "dolly", "a trailer", "b trailer" }, new Dictionary<int, string> { { 0, "B" }, { 3, "T" }, { 4, "B" } });
			}
			#endregion
		}

		readonly ILogger logger;
		readonly IDictionary<IEnumerable<string>, IDictionary<int, string>> validEquipmentConfigurations;
	}
}
