using System;

namespace CargoWise.RefDbRepo.RemoteDbManager
{
	public abstract class SqlObject
	{
		protected SqlObject(string objectName)
		{
			ObjectName = objectName;
		}

		public string ObjectName { get; }
		protected abstract string ResourceSubfolder { get; }
		protected abstract string ObjectType { get; }

		public string GetCreateSqlScriptByVersion(int version = 0) => GetCreateSqlScriptByVersionCore(version);

		string GetCreateSqlScriptByVersionCore(int version)
		{
			var objectNameWithVersion = ObjectType == "TR" ? ObjectName : FormattableString.Invariant($"{ObjectName}_V{version}");
			var sqlScript = SharedDbSchemaChange.GetSqlScriptFromZippedFileUsingSubfolderAndObjectname(ResourceSubfolder, objectNameWithVersion);
			return SharedDbSchemaChange.GetCreateSqlObjectIfNotExistsScript(sqlScript, ObjectType, objectNameWithVersion);
		}
	}

	public class View : SqlObject
	{
		public View(string viewName) : base(viewName) { }

		const string Subfolder = "View";
		protected override string ResourceSubfolder => Subfolder;
		protected override string ObjectType => "V";
	}

	public class StoredProcedure : SqlObject
	{
		public StoredProcedure(string storedProcedureName) : base(storedProcedureName) { }

		protected override string ResourceSubfolder => "StoredProcedure";
		protected override string ObjectType => "P";
	}

	public class SqlFunction : SqlObject
	{
		public SqlFunction(string functionName) : base(functionName) { }

		const string Subfolder = "Function";
		protected override string ResourceSubfolder => Subfolder;
		protected override string ObjectType => "IF";
	}

	public class Trigger : SqlObject
	{
		public Trigger(string triggerName) : base(triggerName) { }

		const string Subfolder = "Trigger";
		protected override string ResourceSubfolder => Subfolder;
		protected override string ObjectType => "TR";
	}
}
