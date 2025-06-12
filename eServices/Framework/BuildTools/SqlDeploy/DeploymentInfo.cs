using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.SqlServer.Dac;

namespace eServices.BuildTools.SqlDeploy
{
	public class DeploymentInfo
	{
		public string DatabaseName { get; set; }
		public string MasterConfigID { get; set; }
		public string DacpacFile { get; set; }
		public List<string> Prerequisites { get; set; } = new List<string>();
		public List<string> PostDeploymentScripts { get; set; } = new List<string>();
		public string ConnectionString { get; set; } = "Data Source=localhost;Initial Catalog=master;Integrated Security=True;Encrypt=False;";
		public string ContentPath { get; set; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		public DacDeployOptions DeployOptions { get; set; } = new DacDeployOptions();
		public IDictionary<string, string> SqlCommandVariableValues
		{
			get { return DeployOptions.SqlCommandVariableValues; }
			set { foreach (var item in value) DeployOptions.SqlCommandVariableValues[item.Key] = item.Value; }
		}
		[JsonIgnore]
		public List<DeploymentInfo> PrerequisiteInfos { get; set; } = new List<DeploymentInfo>();
		[JsonIgnore]
		public Action<string> Messages { get; set; } = Console.WriteLine;
	}
}
