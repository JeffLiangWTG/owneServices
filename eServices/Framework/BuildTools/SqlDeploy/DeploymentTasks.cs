using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace eServices.BuildTools.SqlDeploy
{
	public static class DeploymentTasks
	{
		public static Dictionary<string, DeploymentInfo> GetDefaultDeploymentInfos(string contentPath = null, Action<string> messages = null)
			=> DeserializeDeploymentInfos(Encoding.UTF8.GetString(Resources.Deployments), contentPath, messages);

		public static Dictionary<string, DeploymentInfo> DeserializeDeploymentInfos(string json, string contentPath = null, Action<string> messages = null)
		{
			var deployments = JsonSerializer.Deserialize<Dictionary<string, DeploymentInfo>>(json, jsonOptions);

			foreach (var deployment in deployments.Values)
			{
				if (!(contentPath is null)) deployment.ContentPath = contentPath;
				if (!(messages is null)) deployment.Messages = messages;
				deployment.Prerequisites.ForEach(p =>
				{
					if (deployments.TryGetValue(p, out var prerequisite))
					{
						deployment.PrerequisiteInfos.Add(prerequisite);
					}
				});
			}

			return deployments;
		}

		public static void DropDatabase(DeploymentInfo deploymentInfo, bool dropPrerequisites = true)
		{
			try
			{
				if (dropPrerequisites)
					deploymentInfo.PrerequisiteInfos.ForEach(p => DropDatabase(p, true));

				if (!string.IsNullOrEmpty(deploymentInfo.MasterConfigID))
					return;

				using (var connection = new SqlConnection(deploymentInfo.ConnectionString))
				using (var command = connection.CreateCommand())
				{
					deploymentInfo.Messages($"Dropping database: {deploymentInfo.DatabaseName}");
					connection.Open();
					command.CommandText =
						"DECLARE @sql nvarchar(2000) = 'IF EXISTS(SELECT * FROM sys.databases WHERE QUOTENAME(name) = ''' + QUOTENAME(@database)  + ''') " +
						"BEGIN ALTER DATABASE ' + QUOTENAME(@database) + ' SET SINGLE_USER WITH ROLLBACK IMMEDIATE " +
						"DROP DATABASE ' + QUOTENAME(@database) + ' END' " +
						"EXEC(@sql)";
					command.Parameters.AddWithValue("@database", deploymentInfo.DatabaseName);
					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				deploymentInfo.Messages($"Error dropping database '{deploymentInfo.DatabaseName}': {ex.Message}");
				throw;
			}
		}

		public static void DeployDatabase(DeploymentInfo deploymentInfo)
		{
			string deploymentID;
			string databaseName;
			string timestampProperty = "eServices.eHubDatabase.Deployment.DacpacTimestamp";
			if (!string.IsNullOrEmpty(deploymentInfo.DatabaseName) && !string.IsNullOrEmpty(deploymentInfo.MasterConfigID))
			{
				throw new InvalidDataException("Either 'DatabaseName' or 'MasterConfigID' must be specified.");
			}
			else if (!string.IsNullOrEmpty(deploymentInfo.DatabaseName))
			{
				deploymentID = deploymentInfo.DatabaseName;
				databaseName = deploymentInfo.DatabaseName;
			}
			else
			{
				deploymentID = deploymentInfo.MasterConfigID;
				databaseName = "master";
				timestampProperty += $".{deploymentInfo.MasterConfigID}";
			}

			try
			{
				deploymentInfo.PrerequisiteInfos.ForEach(p => DeployDatabase(p));

				deploymentInfo.Messages($"Starting deployment: {deploymentID}");

				var dacpacFile = new FileInfo(Path.Combine(deploymentInfo.ContentPath, deploymentInfo.DacpacFile));
				var dacpacTimestamp = dacpacFile.LastWriteTimeUtc.ToString("s");
				using (var connection = new SqlConnection(deploymentInfo.ConnectionString))
				using (var command = connection.CreateCommand())
				{
					connection.Open();
					command.CommandText =
					"DECLARE @sql nvarchar(2000) = 'IF DB_ID(''' + REPLACE(@database,'''''','''''''')  + ''') IS NOT NULL " +
					$"SELECT value FROM ' + QUOTENAME(@database) + '.sys.fn_listextendedproperty(''{timestampProperty}'', default, default, default, default, default, default)' " +
					"EXEC(@sql)";
					command.Parameters.AddWithValue("@database", databaseName);
					var deployedDacpacTimestamp = command.ExecuteScalar() as string;
					if (dacpacTimestamp == deployedDacpacTimestamp)
					{
						deploymentInfo.Messages($"Skipping deployment of '{deploymentID}' because database timestamp matches dacpac: {dacpacTimestamp}");
						return;
					}

					var dacpac = DacPackage.Load(dacpacFile.FullName);
					var service = new DacServices(deploymentInfo.ConnectionString);
					service.Message += (sender, args) => deploymentInfo.Messages(args.Message.Message);
					service.Deploy(dacpac, databaseName, true, deploymentInfo.DeployOptions);

					foreach (var script in deploymentInfo.PostDeploymentScripts)
					{
						var scriptFile = new FileInfo(Path.Combine(deploymentInfo.ContentPath, script));
						deploymentInfo.Messages($"Executing post-deployment script: {scriptFile.Name}");
						command.CommandText = scriptFile.OpenText().ReadToEnd();
						command.ExecuteNonQuery();
					}

					command.CommandText =
						$"DECLARE @sql nvarchar(2000) = 'EXEC ' + QUOTENAME(@database) + '.sys." +
						((databaseName is "master" && !string.IsNullOrEmpty(deployedDacpacTimestamp))
							? "sp_updateextendedproperty"
							: "sp_addextendedproperty") +
						$" @name = ''{timestampProperty}'', @value = ''' + @dacpacTimestamp + '''' " +
						"EXEC(@sql)";
					command.Parameters.AddWithValue("@dacpacTimestamp", dacpacTimestamp);
					command.ExecuteNonQuery();

					deploymentInfo.Messages($"Successfully deployed '{deploymentID}' with dacpac timestamp: {dacpacTimestamp}");
				}
			}
			catch (Exception ex)
			{
				deploymentInfo.Messages($"Error deploying '{deploymentID}': {ex.Message}");
				throw;
			}
		}

		private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
		{
			Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
		};
	}
}
