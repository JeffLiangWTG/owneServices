using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Workflow.Business
{
	class SerializedWorkflowMacroNote : HiddenStmNote
	{
		/// <summary>
		/// Don't use this constructor directly, use <see cref="Load(BusinessObjectFactory, Guid)"/> instead.
		/// </summary>
		public SerializedWorkflowMacroNote(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		readonly Version CurrentVersion = new(1, 0);
		internal (string, string)[] MacroSerializedSyntaxNodes;
		internal static string Description => (NoResString)"Serialized Macro SyntaxNode";

		internal static SerializedWorkflowMacroNote Load(BusinessObjectFactory factory, Guid identifier)
		{
			var noteQuery = new ZQuery(StmNoteSchema.ST_ParentID, identifier);
			noteQuery.AddToFilter(StmNoteSchema.ST_Description, Description);
			var note = factory.LoadTop1<SerializedWorkflowMacroNote>(noteQuery);
			return note?.LoadValues();
		}

		SerializedWorkflowMacroNote LoadValues()
		{
			var stream = new MemoryStream(ST_NoteData);
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			using (var jsonReader = new JsonTextReader(reader))
			{
				var jsonObj = JObject.Load(jsonReader);

				var version = (string)jsonObj["Version"];
				var value = (JArray)jsonObj["Value"];

				if (new Version(version).Equals(CurrentVersion))
				{
					MacroSerializedSyntaxNodes = value.Select(j => ((string)j["Macro"], (string)j["Value"])).ToArray();
					return this;
				}

				Delete();
				return null;
			}
		}

		internal static SerializedWorkflowMacroNote New(BusinessObjectFactory factory, Guid parentId, string noteTable)
		{
			var note = factory.New<SerializedWorkflowMacroNote>();
			note.ST_ParentID = parentId;
			note.ST_Table = noteTable;
			return note;
		}

		internal void WriteToNoteData(JArray serializedSyntaxNodes)
		{
			var jsonObj = new JObject
			{
				{ (NoResString)"Version", CurrentVersion.ToString() },
				{ (NoResString)"Value", serializedSyntaxNodes }
			};

			using (var ms = new MemoryStream())
			using (var writer = new StreamWriter(ms, Encoding.UTF8))
			using (var jsonWriter = new JsonTextWriter(writer))
			{
				var serializer = new JsonSerializer();
				serializer.Serialize(jsonWriter, jsonObj);
				jsonWriter.Flush();
				ST_NoteData = ms.ToArray();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ST_Description = Description;
		}
	}
}
