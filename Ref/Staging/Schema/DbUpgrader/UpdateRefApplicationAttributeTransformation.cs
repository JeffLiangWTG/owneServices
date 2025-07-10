using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class UpdateRefApplicationAttributeTransformation: DataTransformation, IDataTransformationTask
	{
		public UpdateRefApplicationAttributeTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var emptyJobGroupCount = (int)DbHelper.ExecuteScalar(trans, "SELECT COUNT(1) FROM dbo.RefApplicationAttribute WHERE RAA_JobGroup = ''");
			if (emptyJobGroupCount == 0)
			{
				return;
			}

			var programPathAndGroupDictionary = GetProgramPathAndGroupFromXml();
			var sql = new StringBuilder();
			var getAppAttributeSql = "SELECT RAA_PK, RAA_ConfigFilePath FROM dbo.RefApplicationAttribute WHERE RAA_JobGroup = ''";
			using (var cmd = DbHelper.CreateCommand(trans, getAppAttributeSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = reader.GetGuid(0);
					var configFilePath = reader.GetString(1).Replace(".exe.config", "").Replace(".config.json", "");
					if (programPathAndGroupDictionary.ContainsKey(configFilePath))
					{
						sql.AppendLine(CultureInfo.InvariantCulture, $"UPDATE dbo.RefApplicationAttribute SET RAA_JobGroup = '{programPathAndGroupDictionary[configFilePath]}' WHERE RAA_PK = '{pk}'");
					}
				}
			}

			if (!string.IsNullOrEmpty(sql.ToString()))
			{
				DbHelper.ExecuteNonQuery(trans, sql.ToString());
			}
		}

		static Dictionary<string, string> GetProgramPathAndGroupFromXml()
		{
			var localDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var configurationXmlFiles = Directory.GetFiles(Path.Combine(localDirectory, @"..\net8.0\Configuration"), "*.xml");
			var programPathAndGroupDictionary = new Dictionary<string, string>();
			foreach (var xmlFile in configurationXmlFiles)
			{
				var xmlContent = File.ReadAllText(xmlFile);
				var startIndex = xmlContent.IndexOf("<schedule>", StringComparison.OrdinalIgnoreCase);
				var endIndex = xmlContent.IndexOf("</schedule>", StringComparison.OrdinalIgnoreCase);
				xmlContent = xmlContent.Substring(startIndex, endIndex - startIndex + 11);
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xmlContent);

				var jobNodes = xmlDoc.SelectNodes("/schedule/job");
				foreach (XmlNode node in jobNodes)
				{
					var jobGroupNode = node.SelectSingleNode("group");
					if (jobGroupNode == null)
					{
						continue;
					}
					var jobGroup = jobGroupNode.InnerText;
					var entryNodes = node.SelectNodes("job-data-map/entry");
					if (entryNodes == null)
					{
						continue;
					}
					foreach (XmlNode entryNode in entryNodes)
					{
						var key = entryNode.SelectSingleNode("key")?.InnerText;
						if (key != null && key == "ProgramExePath")
						{
							var programPath = entryNode.SelectSingleNode("value").InnerText;
							programPath = programPath.Substring(programPath.LastIndexOf('\\') + 1).Replace(".exe", "");
							if (!programPathAndGroupDictionary.ContainsKey(programPath))
							{
								programPathAndGroupDictionary[programPath] = jobGroup;
							}
							continue;
						}
					}
				}
			}

			return programPathAndGroupDictionary;
		}
	}
}
