using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace CargoWise.RefDbRepo.SpecialFileHandling
{
	public class PreUpgradeTransformationMapperSafe : MapperChangeRequest
	{
		public PreUpgradeTransformationMapperSafe(SpecialFileHandlerContext context) : base(context)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
		}

		protected override string MapperCSharpFileName => "PreUpgradeTransformationTasks.cs";

		protected override string ShelfCheckinMapperTextFileName => "ShelfCheckinPreUpgradeTransformationMapper.txt";

		protected override string ShelfCheckinMapperPath => @"/Service/SchemaManagement/UpgradeManagerRunner/" + ShelfCheckinMapperTextFileName;

		protected override string TransformationVersionPath => @"/Service/SchemaManagement/UpgradeManagerRunner/SafeSchemaVersion.cs";
	}

	public class PreUpgradeTransformationMapperStaging : MapperChangeRequest
	{
		public PreUpgradeTransformationMapperStaging(SpecialFileHandlerContext context) : base(context)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
		}

		protected override string MapperCSharpFileName => "PreUpgradeTransformationTasks.cs";

		protected override string ShelfCheckinMapperTextFileName => "ShelfCheckinPreUpgradeTransformationMapper.txt";

		protected override string ShelfCheckinMapperPath => @"/Staging/Schema/DbUpgrader/" + ShelfCheckinMapperTextFileName;

		protected override string TransformationVersionPath => @"/Staging/Schema/DbUpgrader/StagingSchemaVersion.cs";
	}

	public class DataTransformationMapperSafe : MapperChangeRequest
	{
		public DataTransformationMapperSafe(SpecialFileHandlerContext context) : base(context)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
		}

		protected override string MapperCSharpFileName => "DataTransformationTasks.cs";

		protected override string ShelfCheckinMapperTextFileName => "ShelfCheckinDataTransformationMapper.txt";

		protected override string ShelfCheckinMapperPath => @"/Service/SchemaManagement/UpgradeManagerRunner/" + ShelfCheckinMapperTextFileName;

		protected override string TransformationVersionPath => @"/Service/SchemaManagement/UpgradeManagerRunner/TransformationVersion.cs";
	}

	public class DataTransformationMapperStaging : MapperChangeRequest
	{
		public DataTransformationMapperStaging(SpecialFileHandlerContext context) : base(context)
		{
			_ = context ?? throw new ArgumentNullException(nameof(context));
		}

		protected override string MapperCSharpFileName => "DataTransformationTasks.cs";
		protected override string ShelfCheckinMapperTextFileName => "ShelfCheckinDataTransformationMapper.txt";

		protected override string ShelfCheckinMapperPath => @"/Staging/Schema/DbUpgrader/" + ShelfCheckinMapperTextFileName;

		protected override string TransformationVersionPath => @"/Staging/Schema/DbUpgrader/TransformationVersion.cs";
	}

	public abstract class MapperChangeRequest : DeltaFileHandler
	{
		protected MapperChangeRequest(SpecialFileHandlerContext context)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
		}
		protected readonly SpecialFileHandlerContext Context;

		protected abstract string ShelfCheckinMapperPath { get; }
		protected abstract string TransformationVersionPath { get; }
		protected abstract string MapperCSharpFileName { get; }
		protected abstract string ShelfCheckinMapperTextFileName { get; }

		protected virtual string VersionPattern
		{
			get
			{
				return @"(?<=public const int Version\s*=\s*)\d+(?=\s*;)";
			}
		}

		protected string MapperCSharpPath => ShelfCheckinMapperPath.Replace(ShelfCheckinMapperTextFileName, MapperCSharpFileName);

		protected override bool IsDeltaFile(string serverPath)
		{
			return !string.IsNullOrEmpty(serverPath) ? serverPath.EndsWith(ShelfCheckinMapperPath)
			: throw new ArgumentNullException(nameof(serverPath));
		}

		protected override bool IsMasterFile(string serverPath)
		{
			return !string.IsNullOrEmpty(serverPath) ? serverPath.EndsWith(TransformationVersionPath) || serverPath.EndsWith(MapperCSharpPath)
			: throw new ArgumentNullException(nameof(serverPath));
		}

		protected override bool ShouldUnshelveMaster(string serverPath) => true;

		public override bool ShouldUnshelveForDATCheckin(IPendingChange change)
		{
			_ = change ?? throw new ArgumentNullException(nameof(change));
			var result = base.ShouldUnshelveForDATCheckin(change);
			_ = change.ServerItem ?? throw new ArgumentNullException("change.ServerItem");
			return !change.ServerItem.EndsWith(TransformationVersionPath) && result;
		}

		protected override string[] UpdateForDATCheckinOnDeltaFile(IWorkspaceAccess workspace, IPendingChange change, string localTempFile)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));
			_ = change ?? throw new ArgumentNullException(nameof(change));

			int version;
			var updatedFiles = new string[2];
			updatedFiles[0] = BumpVersion(workspace, GetVersionFileFromMapperChange(change), out version);
			updatedFiles[1] = UpdateMapper(workspace, change, localTempFile, version);
			return updatedFiles;
		}

		protected virtual string UpdateMapper(IWorkspaceAccess workspace, IPendingChange change, string localTempFile, int version)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));
			_ = change ?? throw new ArgumentNullException(nameof(change));

			const string DATInsertTag = @"//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW";
			StringBuilder mappingStrings = new StringBuilder();
			mappingStrings.Append(DATInsertTag);

			using (TextReader streamReader = new StreamReader(localTempFile))
			{
				string line = string.Empty;
				do
				{
					line = line.Trim();
					if (!string.IsNullOrEmpty(line) && !line.StartsWith("//"))
					{
						AppendMappingLine(version, mappingStrings, line);
					}
					line = streamReader.ReadLine();
				}
				while (line != null);
			}

			string mapperFileServerItem = change.ServerItem.Replace(ShelfCheckinMapperTextFileName, MapperCSharpFileName);
			string mapperFileLocalItem = workspace.GetLocalItemForServerItem(mapperFileServerItem);
			workspace.GetLatest(new string[] { mapperFileServerItem }, TfsRecursionType.None, TfsGetOptions.None);
			workspace.PendEdit(mapperFileServerItem);

			_ = mapperFileLocalItem ?? throw new ArgumentNullException(nameof(mapperFileLocalItem));
			string mapperFileCurrentContents = File.ReadAllText(mapperFileLocalItem);
			if (!mapperFileCurrentContents.Contains(DATInsertTag))
			{
				throw new Exception(
					string.Format(
						"DAT cannot auto map transformation, because required string is missing in mapper file or its format is incorrect.\r\nMissing string: {0}\r\nMapper file: {1}",
						DATInsertTag,
						mapperFileLocalItem
						)
				);
			}

			string newMapperContent = mapperFileCurrentContents.Replace(DATInsertTag, mappingStrings.ToString());
			File.WriteAllText(mapperFileLocalItem, newMapperContent);

			return mapperFileLocalItem;
		}

		protected void AppendMappingLine(int version, StringBuilder mappingStrings, string shelfCheckInMapperLine)
		{
			_ = mappingStrings ?? throw new ArgumentNullException(nameof(mappingStrings));

			mappingStrings.AppendLine();
			mappingStrings.Append($@"				yield return new {shelfCheckInMapperLine}({version});");
		}

		protected string GetVersionFileFromMapperChange(IPendingChange change)
		{
			_ = Context.RepositoryKey ?? throw new ArgumentNullException("Context.RepositoryKey");
			return Context.RepositoryKey.GetServerPath(TransformationVersionPath);
		}

		protected virtual string BumpVersion(IWorkspaceAccess workspace, string versionFileServerItem, out int version)
		{
			_ = workspace ?? throw new ArgumentNullException(nameof(workspace));

			string versionFileLocalItem = workspace.GetLocalItemForServerItem(versionFileServerItem);

			workspace.GetLatest(new string[] { versionFileServerItem }, TfsRecursionType.None, TfsGetOptions.None);
			workspace.PendEdit(versionFileServerItem);

			string versionFileContents = File.ReadAllText(versionFileLocalItem);
			var match = Regex.Match(versionFileContents, VersionPattern);
			version = Convert.ToInt32(match.Value, CultureInfo.InvariantCulture);

			version++;
			versionFileContents = Regex.Replace(versionFileContents, VersionPattern, version.ToString());

			File.WriteAllText(versionFileLocalItem, versionFileContents);
			return versionFileLocalItem;
		}
	}
}
