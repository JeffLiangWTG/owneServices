using System;
using System.IO;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace CargoWise.RefDbRepo.SpecialFileHandling
{
	public abstract class SchemaVersionChangeRequest : VersionChangeRequest
	{
		const string SqlFileExt = ".sql";

		protected abstract string SchemaTablePath { get; }
		protected abstract string SchemaVersionFileServerPath { get; }

		public SchemaVersionChangeRequest(SpecialFileHandlerContext context) => this.context = context ?? throw new ArgumentNullException(nameof(context));
		readonly SpecialFileHandlerContext context;

		protected override bool TriggersVersionChange(string serverPath)
		{
			if (!string.IsNullOrEmpty(serverPath) && !string.IsNullOrEmpty(SchemaTablePath))
			{
				return serverPath.Contains(SchemaTablePath) && serverPath.EndsWith(SqlFileExt, StringComparison.OrdinalIgnoreCase);
			}

			return false;
		}

		protected override string GetVersionFileServerPath(string serverPath)
		{
			return context.RepositoryKey.GetServerPath(SchemaVersionFileServerPath);
		}

		protected override void BumpVersion(IWorkspaceAccess workspace, string versionFileServerPath)
		{
			var localFile = workspace.GetLocalItemForServerItem(versionFileServerPath);
			if (string.IsNullOrEmpty(localFile))
			{
				return;
			}

			var versionFileContents = File.ReadAllText(localFile);

			var currentVersion = Convert.ToInt32(Regex.Match(versionFileContents, VersionPattern).Value);

			var newVersion = currentVersion + 1;

			versionFileContents = Regex.Replace(versionFileContents, VersionPattern, newVersion.ToString());

			File.WriteAllText(localFile, versionFileContents);
		}

		const string VersionPattern = @"(?<=Version\s*=\s*)[0-9]+(?=\s*;)";
	}
}
