using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class UpdateQuartzJobGroupsTransformation : DataTransformation, IDataTransformationTask
	{
		public UpdateQuartzJobGroupsTransformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var localDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var configurationXmlFiles = Directory.GetFiles(Path.Combine(localDirectory, @"..\net8.0\Configuration"), "*.xml");
			var groupDictionary = new Dictionary<string, List<string>>();
			foreach (var xmlFile in configurationXmlFiles)
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(xmlFile);

				var jobNodes = xmlDoc.SelectNodes("//*[local-name()='job-scheduling-data']/*[local-name()='schedule']/*[local-name()='job']");
				foreach (XmlNode node in jobNodes)
				{
					var jobName = node.SelectSingleNode("*[local-name()='name']").InnerText;
					var jobGroupNode = node.SelectSingleNode("*[local-name()='group']");
					var jobGroup = jobGroupNode == null ? "DEFAULT" : jobGroupNode.InnerText;
					if (!groupDictionary.ContainsKey(jobGroup))
					{
						groupDictionary[jobGroup] = new List<string>();
					}
					groupDictionary[jobGroup].Add(jobName);
				}
			}

			var sql = new StringBuilder();
			sql.AppendLine(@"IF EXISTS (SELECT 1 FROM QRTZ_JOB_DETAILS WHERE JOB_GROUP='DEFAULT')");
			sql.AppendLine(@"BEGIN
ALTER TABLE QRTZ_TRIGGERS NOCHECK CONSTRAINT FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS;");

			foreach (var jobGroup in groupDictionary.Keys)
			{
				if (groupDictionary[jobGroup].Count == 0)
				{
					continue;
				}
				var jobs = string.Join(", ", groupDictionary[jobGroup].Select(x => $"'{x}'"));
				var subSql = $@"UPDATE QRTZ_JOB_DETAILS SET JOB_GROUP='{jobGroup}' WHERE JOB_NAME IN ({jobs});
UPDATE QRTZ_TRIGGERS SET JOB_GROUP='{jobGroup}' WHERE JOB_NAME IN ({jobs});";
				sql.AppendLine(subSql);
			}

			sql.AppendLine(@"ALTER TABLE QRTZ_TRIGGERS CHECK CONSTRAINT FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS;
END");
			DbHelper.ExecuteNonQuery(trans, sql.ToString());
		}
	}
}
