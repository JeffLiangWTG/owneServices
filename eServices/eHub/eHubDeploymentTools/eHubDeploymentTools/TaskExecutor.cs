using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace eHubDeploymentTools
{
	class TaskExecutor
	{
		const string BiztalkBuildPath = @"MSBuildScripts\CargoWise.eHub.Biztalk.build";
		const string BuildCSSPath = @"CmdScripts\BuildCSS.bat";
		const string PublishCSSDatabasePath = @"CmdScripts\PublishCSSDatabase.bat";

		const string LogFilePath = @"C:\eHubDeployment.log";
		const string startWorkspaceData = @"------------ ---------------------------- ---------- --------------------------";

		public static string Execute(TaskTypes.TaskType taskType, List<Property> propertyList)
		{
			switch (taskType)
			{
				case TaskTypes.TaskType.GetLatest: GetLatest(propertyList); break;
				case TaskTypes.TaskType.CleanBiztalkApplications: CleanBiztalkApplications(propertyList); break;
				case TaskTypes.TaskType.CreateEHubTransactionDB: CreateeHubTransactionDB(propertyList); break;
				case TaskTypes.TaskType.DeployBtsSolutionDev: DeployBtsSolutionDev(propertyList); break;
				case TaskTypes.TaskType.BuildContainerTrackingSystem: BuildContainerTrackingSystem(propertyList); break;
				case TaskTypes.TaskType.PublishCSSDatabase: PublishCSSDatabase(propertyList); break;
			}

			return "";
		}

		public static string[] GetWorkspaces(string tsfPath)
		{
			try
			{
				string batchFile = Path.GetTempFileName().Replace(".tmp", ".bat");
				using (var writer = new StreamWriter(batchFile))
				{
					writer.WriteLine(String.Format("\"{0}\" workspaces", tsfPath));
				}
				string output = ExecuteCmdBatchSilent(batchFile, null);
				File.Delete(batchFile);
				return ParseWorkspaceResult(output);
			}
			catch
			{
				return null;
			}
		}

		static string[] ParseWorkspaceResult(string resultString)
		{
			var workspaces = new List<string>();
			int startPos = resultString.IndexOf(startWorkspaceData) + startWorkspaceData.Length;
			string data = resultString.Substring(startPos);
			var lines = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < lines.Length; i++)
			{
				string[] columns = lines[i].Split(new string[]{"   "}, StringSplitOptions.RemoveEmptyEntries);
				if (columns.Length > 0)
				{
					workspaces.Add(columns[0]);
				}
			}
			return workspaces.ToArray<string>();
		}

		#region DeployBtsSolutionDev Command

		static void DeployBtsSolutionDev(List<Property> propertyList)
		{
			string msBuildPath = GetPropertyValue(propertyList, "DeployBtsSolutionDev:FileSelector:MSBuild"); ;
			string gacUtilFilePath = GetPropertyValue(propertyList, "DeployBtsSolutionDev:FileSelector:GacUtilFilePath");
			string bizTalkMachineName = GetPropertyValue(propertyList, "DeployBtsSolutionDev:TextSelector:BizTalkMachineName"); ;
			string eHubBinFolder = GetPropertyValue(propertyList, "DeployBtsSolutionDev:DirectorySelector:eHubBinFolder"); ;
			string biztalkInstallationFolder = GetPropertyValue(propertyList, "DeployBtsSolutionDev:DirectorySelector:BiztalkInstallationFolder"); ;
			string bindingsFolder = GetPropertyValue(propertyList, "DeployBtsSolutionDev:DirectorySelector:BindingsFolder"); ;


			if (String.IsNullOrEmpty(msBuildPath) || String.IsNullOrEmpty(gacUtilFilePath) || String.IsNullOrEmpty(bizTalkMachineName) || String.IsNullOrEmpty(eHubBinFolder) || String.IsNullOrEmpty(biztalkInstallationFolder) || String.IsNullOrEmpty(bindingsFolder)) throw new Exception("One of 6 property is empty.");
			ExecuteDeployBtsSolutionDevMSBuildCommand(msBuildPath, gacUtilFilePath, bizTalkMachineName, eHubBinFolder, biztalkInstallationFolder, bindingsFolder);
		}

		static void ExecuteDeployBtsSolutionDevMSBuildCommand(string msBuildPath, string gacUtilFilePath, string bizTalkMachineName, string eHubBinFolder, string biztalkInstallationFolder, string bindingsFolder)
		{
			string targetName = "/t:DeployBtsSolutionDev";
			string argumentString = String.Format("/property:GacUtilFilePath=\"{0}\";BizTalkMachineName=\"{1}\";BinPath=\"{2}\";BtsInstallLocation=\"{3}\";DevBindingBasePath=\"{4}\"", gacUtilFilePath, bizTalkMachineName, eHubBinFolder, biztalkInstallationFolder, bindingsFolder);
			RunMSBuildBatch(msBuildPath, targetName, argumentString);
		}

		#endregion

		#region CleanBiztalkApplications Command

		static void CreateeHubTransactionDB(List<Property> propertyList)
		{
			string connectionString = GetPropertyValue(propertyList, "CreateeHubTransactionDB:TextSelector:ConnectionString");
			string eHubDBName = GetPropertyValue(propertyList, "CreateeHubTransactionDB:TextSelector:eHubDBName"); ;
			string sqlScriptFile = GetPropertyValue(propertyList, "CreateeHubTransactionDB:FileSelector:SqlScriptFile"); ;
			string msBuildPath = GetPropertyValue(propertyList, "CreateeHubTransactionDB:FileSelector:MSBuild"); ;

			if (String.IsNullOrEmpty(connectionString) || String.IsNullOrEmpty(eHubDBName) || String.IsNullOrEmpty(sqlScriptFile) || String.IsNullOrEmpty(msBuildPath)) throw new Exception("One of 4 property is empty.");
			ExecuteCleanBiztalkApplicationsMSBuildCommand(connectionString, eHubDBName, sqlScriptFile, msBuildPath);
		}

		static void ExecuteCleanBiztalkApplicationsMSBuildCommand(string connectionString, string eHubDBName, string sqlScriptFile, string msBuildPath)
		{
			string targetName = "/t:CreateeHubTransactionDB";
			string argumentString = String.Format("/property:ConnectionString=\"{0}\";eHubDBName=\"{1}\";eHubDBSqlScriptFile=\"{2}\"", connectionString, eHubDBName, sqlScriptFile);
			RunMSBuildBatch(msBuildPath, targetName, argumentString);
		}

		#endregion

		#region CleanBiztalkApplications Command

		static void CleanBiztalkApplications(List<Property> propertyList)
		{
			string bizTalkMachineName = GetPropertyValue(propertyList, "CleanBiztalkApplications:TextSelector:BizTalkMachineName");
			string gacUtilFilePath = GetPropertyValue(propertyList, "CleanBiztalkApplications:FileSelector:GacUtilFilePath"); ;
			string msBuildPath = GetPropertyValue(propertyList, "CleanBiztalkApplications:FileSelector:MSBuild"); ;

			if (String.IsNullOrEmpty(bizTalkMachineName) || String.IsNullOrEmpty(gacUtilFilePath) || String.IsNullOrEmpty(msBuildPath)) throw new Exception("One of 3 property is empty.");
			ExecuteCleanBiztalkApplicationMSBuildCommand(bizTalkMachineName, gacUtilFilePath, msBuildPath);
		}

		static void ExecuteCleanBiztalkApplicationMSBuildCommand(string bizTalkMachineNamer, string gacUtilFilePath, string msBuildPath)
		{
			string targetName = "/t:Clean";
			string argumentString = String.Format("/property:BizTalkMachineName=\"{0}\";GacUtilFilePath=\"{1}\"", bizTalkMachineNamer, gacUtilFilePath);
			RunMSBuildBatch(msBuildPath, targetName, argumentString);
		}

		#endregion

		#region GetLatest Command

		static void GetLatest(List<Property> propertyList)
		{
			string tfPath = GetPropertyValue(propertyList, "GetLatest:FileSelector:TF.exe Path");
			string tfsFolder = GetPropertyValue(propertyList, "GetLatest:TextSelector:TFS Folder"); ;
			string shareFolder = GetPropertyValue(propertyList, "GetLatest:DirectorySelector:Debug Bin Share Folder"); ;
			string localFolder = GetPropertyValue(propertyList, "GetLatest:DirectorySelector:Debug Bin Local Folder"); ;

			if (String.IsNullOrEmpty(tfPath) || String.IsNullOrEmpty(tfsFolder) || String.IsNullOrEmpty(shareFolder) || String.IsNullOrEmpty(localFolder)) throw new Exception("One of 4 property is empty.");

			ExecuteTFCommand(tfPath, tfsFolder, localFolder);
			ExecuteCopyBinCommand(shareFolder, localFolder);
		} 

		static void ExecuteTFCommand(string tfsPath, string tfsFolder, string workingDirectory)
		{
			string batchFile = Path.GetTempFileName().Replace(".tmp", ".bat");
			using (var writer = new StreamWriter(batchFile))
			{
				writer.WriteLine(String.Format(String.Format("\"{0}\" get {1} /recursive", tfsPath, tfsFolder)));
				writer.WriteLine(String.Format("echo Get Latest Version Taken"));
				writer.WriteLine(String.Format("pause"));
			}
			ExecuteCmdBatch(batchFile, workingDirectory);
			File.Delete(batchFile);
		}

		static void ExecuteCopyBinCommand(string sourceFolder, string destFolder)
		{
			string batchFile = Path.GetTempFileName().Replace(".tmp", ".bat");
			using (var writer = new StreamWriter(batchFile))
			{
				writer.WriteLine(String.Format(String.Format("DEL /F /S /Q {0}  ", destFolder)));
				writer.WriteLine(String.Format(String.Format("XCOPY {0} {1} /S /E /Y ", sourceFolder, destFolder)));
				writer.WriteLine(String.Format("echo Latest Binaries copied"));
				writer.WriteLine(String.Format("pause"));
			}
			ExecuteCmdBatch(batchFile, null);
			File.Delete(batchFile);
		}

		#endregion

		#region BuildContainerTrackingSystem

		static void BuildContainerTrackingSystem(List<Property> propertyList)
		{
			var parameterNames = new []
			{
				"BuildContainerTrackingSystem:DirectorySelector:ContainerSubscriptionService",
				"BuildContainerTrackingSystem:FileSelector:MsBuildExe",
			    "BuildContainerTrackingSystem:TextSelector:Configuration"
			};

			var parameters = new NameValueCollection();
			foreach (var parameterName in parameterNames)
			{
				string value = GetPropertyValue(propertyList, parameterName);
				if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(string.Format("Parameter {0} is empty", parameterName));
				parameters.Add(parameterName, value);	
			}

			ExecuteCmdBatch(BuildCSSPath, null, parameters);
		}

		#endregion

		#region PublishCSSDatabase

		static void PublishCSSDatabase(List<Property> propertyList)
		{
			var parameterNames = new[]
			{
				"PublishCSSDatabase:DirectorySelector:ContainerSubscriptionService",
				"PublishCSSDatabase:FileSelector:SqlPackageExe",
			    "PublishCSSDatabase:TextSelector:DatabaseServerName"
			};

			var parameters = new NameValueCollection();
			foreach (var parameterName in parameterNames)
			{
				string value = GetPropertyValue(propertyList, parameterName);
				if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(string.Format("Parameter {0} is empty", parameterName));
				parameters.Add(parameterName, value);
			}

			ExecuteCmdBatch(PublishCSSDatabasePath, null, parameters);
		}

		#endregion
	

		#region General Command

		static void RunMSBuildBatch(string msBuildPath, string targetName, string argumentString)
		{
			string batchFile = Path.GetTempFileName().Replace(".tmp", ".bat");
			using (var writer = new StreamWriter(batchFile))
			{
				string buildFile = Path.Combine(Directory.GetCurrentDirectory(), BiztalkBuildPath);
				if (!File.Exists(buildFile)) throw new Exception(String.Format("File {0} doesn't exists.", BiztalkBuildPath));

				writer.WriteLine(String.Format(String.Format(@"{0} /l:FileLogger,Microsoft.Build.Engine;verbosity=diagnostic;logfile={3} {1} {4} {2}", msBuildPath, buildFile, argumentString, LogFilePath, targetName)));
				writer.WriteLine(String.Format("echo eHub Biztalk Cleanned"));
				writer.WriteLine(String.Format("pause"));
			}
			ExecuteCmdBatch(batchFile, null);
			File.Delete(batchFile);
			Process.Start("notepad", LogFilePath);
		}

		static void ExecuteCmdBatch(string batchFile, string workingDirectory, NameValueCollection replaceCollection = null)
		{
			if (replaceCollection != null) batchFile = ReplaceParametersInBatchFile(batchFile, replaceCollection);

			using (Process proc = new Process())
			{
				proc.StartInfo.FileName = batchFile;
                proc.StartInfo.Verb = "runas";
                proc.StartInfo.UseShellExecute = true;
                if (!String.IsNullOrEmpty(workingDirectory)) proc.StartInfo.WorkingDirectory = workingDirectory;
				proc.Start();
				proc.WaitForExit();
				proc.Close();
			}
		}

		static string ExecuteCmdBatchSilent(string batchFile, string workingDirectory, NameValueCollection replaceCollection = null)
		{
			if (replaceCollection != null) batchFile = ReplaceParametersInBatchFile(batchFile, replaceCollection);
			string output = "";

			using (var proc = new Process())
			{
				proc.StartInfo.FileName = batchFile;
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.RedirectStandardOutput = true;
				proc.StartInfo.CreateNoWindow = true;
				if (!String.IsNullOrEmpty(workingDirectory)) proc.StartInfo.WorkingDirectory = workingDirectory;
				proc.Start();
				output = proc.StandardOutput.ReadToEnd();
				proc.WaitForExit();
			}

			return output;
		}

		static string ReplaceParametersInBatchFile(string inputFilePath, NameValueCollection replaceCollection)
		{
			string tempFilePath = Path.GetTempFileName();

			using (var writer = new StreamWriter(tempFilePath))
			using (var reader = new StreamReader(inputFilePath))
			{
				string text;
				while ((text = reader.ReadLine()) != null)
				{
					writer.WriteLine(ReplaceParameters(text, replaceCollection));
				}
			}

			string newFileName = tempFilePath.Replace(".tmp", ".bat");
			File.Move(tempFilePath, newFileName);
			return newFileName; 
		}

		static string ReplaceParameters(string text, NameValueCollection replaceCollection)
		{
			for(int i = 0; i < replaceCollection.Count; i++)
			{
				text = text.Replace(string.Format("[[{0}]]", replaceCollection.Keys[i]), replaceCollection[i]);
			}

			return text;
		}

		static string GetPropertyValue(List<Property> propertyList, string name)
		{
			for (int i = 0; i < propertyList.Count; i++)
			{
				if (propertyList[i].Key == name) return propertyList[i].Value;
			}

			return null;
		}

		#endregion

		#region Obsolete

		static public void CopyFolder(string sourceFolder, string destFolder)
		{
			if (!Directory.Exists(destFolder))
				Directory.CreateDirectory(destFolder);
			string[] files = Directory.GetFiles(sourceFolder);
			foreach (string file in files)
			{
				string name = Path.GetFileName(file);
				string dest = Path.Combine(destFolder, name);
				File.Copy(file, dest);
			}
			string[] folders = Directory.GetDirectories(sourceFolder);
			foreach (string folder in folders)
			{
				string name = Path.GetFileName(folder);
				string dest = Path.Combine(destFolder, name);
				CopyFolder(folder, dest);
			}
		}

		static public void DeleteAllFilesFromFolder(string folderPath)
		{
			if (!Directory.Exists(folderPath)) return;

			string[] files = Directory.GetFiles(folderPath);
			foreach (string file in files)
			{
				File.Delete(file);
			}

			string[] directories = Directory.GetDirectories(folderPath);
			foreach (string directory in directories)
			{
				Directory.Delete(directory, true);
			}
		}

		static string CopyBinFolder(string shareFolder, string localFolder)
		{
			var sbLog = new StringBuilder();

			try
			{
				DeleteAllFilesFromFolder(localFolder);
				sbLog.AppendLine(String.Format("{0} deleted successfully.", localFolder));
			}
			catch (Exception ex)
			{
				sbLog.AppendLine(String.Format("Error during deleted {0}: {1}", localFolder, ex.Message));
			}

			try
			{
				CopyFolder(shareFolder, localFolder);
				sbLog.AppendLine(String.Format("Folder copy successfully: from {0} to {1}.", shareFolder, localFolder));
			}
			catch (Exception ex)
			{
				sbLog.AppendLine(String.Format("Error during folder copy: from {0} to {1}: {2}", shareFolder, localFolder, ex.Message));
			}

			return sbLog.ToString();
		}

		#endregion

	}
}
