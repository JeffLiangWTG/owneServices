using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.eHub.Adapter;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Staging.DataPublishingProcessor
{
	public class MessageFactory : IMessageFactory
	{
		readonly IeHubAdapter _adapter;

		public MessageFactory(IeHubAdapter eHubAdapter)
		{
			_adapter = eHubAdapter;
		}

		Dictionary<(string dataSetName, string tableName, short dataSetId), DateTime> DataSetsToSend
		{
			get
			{
				if (_dataSetsToSend == null)
				{
					_dataSetsToSend = new Dictionary<(string dataSetName, string tableName, short dataSetId), DateTime>();
				}
				return _dataSetsToSend;
			}
		}
		Dictionary<(string dataSetName, string tableName, short dataSetId), DateTime> _dataSetsToSend;

		public void AddBatchToMessage(IDbCommand cmd, IEnumerable<short> dataSetIds)
		{
			if (dataSetIds == null || !dataSetIds.Any())
			{
				return;
			}

			var dataSetIdArr = dataSetIds.ToArray();
			var parameters = new string[dataSetIdArr.Length];
			for (var i = 0; i < dataSetIdArr.Length; i++)
			{
				parameters[i] = $"@RDS_DataSetId{i}";
				var parameter = new SqlParameter
				{
					ParameterName = $"@RDS_DataSetId{i}",
					Value = dataSetIdArr[i]
				};
				cmd.Parameters.Add(parameter);
			}
#pragma warning disable CA2100
			cmd.CommandText = $@"SELECT RDS_DataSetName, RDS_DataSetId, RDS_TableName, RDS_LastUpdatedUTC
FROM RefDataSetInformation
WHERE RDS_DataSetId IN ({string.Join(", ", parameters)})
AND RDS_IsPush = 1"; //The method is only called under IsPush circumstances, but we can add it here to make sure it is always IsPush.
#pragma warning restore CA2100
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var dataSetName = reader.GetString(0);
					var dataSetId = reader.GetInt16(1);
					var tableName = reader.GetString(2);
					if (!DataSetsToSend.ContainsKey((dataSetName, tableName, dataSetId)))
					{
						DataSetsToSend.Add((dataSetName, tableName, dataSetId), reader.GetDateTime(3));
					}
				}
			}
		}

		public void SendMessage(IDbCommand cmd, int messageBatchSize)
		{
			var subscribers = GetSubscribers(cmd).ToList();
			if (subscribers == null)
			{
				return;
			}

			var messagesCount = 0;
			var subCount = subscribers.Count;
			foreach (var subs in subscribers)
			{
#pragma warning disable CA2000 // Dispose objects before losing scope
				var gmdMessage = new EHubMessageBuilder(GetMessage(subs.Value), subs.Key).Build();
#pragma warning restore CA2000 // Dispose objects before losing scope
				if (gmdMessage == null)
				{
					continue;
				}
				_adapter.Outbox.AddMessage(gmdMessage);
				messagesCount++;

				if (messagesCount == messageBatchSize || messagesCount == subCount)
				{
					subCount -= messagesCount;
					messagesCount = 0;
					_adapter.SendMessages();
				}
			}
		}

		public Dictionary<string, List<short>> GetSubscribers(IDbCommand cmd)
		{
			var result = new Dictionary<string, List<short>>();
			if (HasMessagesToSend())
			{
				var dataSetIdArr = DataSetsToSend.Select(x => x.Key.dataSetId).ToArray();
				var parameters = new string[dataSetIdArr.Length];
				for (var i = 0; i < dataSetIdArr.Length; i++)
				{
					parameters[i] = $"@DPS_DataSetId{i}";
					var parameter = new SqlParameter
					{
						ParameterName = $"@DPS_DataSetId{i}",
						Value = dataSetIdArr[i]
					};
					cmd.Parameters.Add(parameter);
				}
#pragma warning disable CA2100
				cmd.CommandText = $"SELECT DPS_ClientRecipientId, DPS_DataSetId FROM DataPushSubscription WHERE DPS_DataSetId IN ({string.Join(", ", parameters)})";
#pragma warning restore CA2100
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var recipient = reader.GetString(0);
						var dataSetId = reader.GetInt16(1);
						if (!result.ContainsKey(recipient))
						{
							result.Add(recipient, new List<short>());
						}
						result[recipient].Add(dataSetId);
					}
				}
			}
			return result;
		}

		public bool HasMessagesToSend()
		{
			return DataSetsToSend.Count > 0;
		}

		string GetMessage(List<short> dataSetIds)
		{
			var dataSetsMessage = string.Empty;
			foreach (var dataSet in DataSetsToSend.Where(x => dataSetIds.Contains(x.Key.dataSetId)))
			{
				dataSetsMessage += MessageHelper.GetDataSetLayout(dataSet.Key.dataSetName, dataSet.Key.tableName, dataSet.Value);
			}
			return !string.IsNullOrEmpty(dataSetsMessage) ? string.Format(CultureInfo.InvariantCulture, MessageHelper.GetMessageLayout, dataSetsMessage) : string.Empty;
		}
	}

	public static class MessageHelper
	{
		public static string GetMessageLayout => @"<RefDbRepoMessage>
	<DataSets>{0}
	</DataSets>
</RefDbRepoMessage>";

		public static string GetDataSetLayout(string dataSetName, string tableName, DateTime lastUpdatedUtc)
		{
			return $@"
		<DataSet>
			<DataSetId>{dataSetName}</DataSetId>
			<TimeStamp>{lastUpdatedUtc:s}</TimeStamp>
			<TableName>{tableName}</TableName>
		</DataSet>";
		}
	}
}
