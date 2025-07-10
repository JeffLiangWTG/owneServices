using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json.Linq;

namespace Enterprise.Workflow.Business
{
	class SerializedWorkflowMacroDataStore
	{
		public SerializedWorkflowMacroDataStore(BusinessObjectFactory factory)
		{
			Factory = factory;
			syntaxNodeSerializationHelper = new SyntaxNodeSerializationHelper();
		}

		readonly BusinessObjectFactory Factory;

		readonly SyntaxNodeSerializationHelper syntaxNodeSerializationHelper;

		public IDictionary<string, SyntaxNode> Read(IHaveMacrosForSyntaxNodeCache cacheParent)
		{
			var note = SerializedWorkflowMacroNote.Load(Factory, cacheParent.Identifier);

			if (note == null)
			{
				return null;
			}

			var result = new ConcurrentDictionary<string, SyntaxNode>();

			Parallel.ForEach(note.MacroSerializedSyntaxNodes, tuple =>
			{
				result[tuple.Item1] = syntaxNodeSerializationHelper.Deserialize(tuple.Item2);
			});

			return result;
		}

		public void Write(IHaveMacrosForSyntaxNodeCache cacheParent, Dictionary<string, SyntaxNode> syntaxNodes)
		{
			var note = SerializedWorkflowMacroNote.Load(Factory, cacheParent.Identifier) ?? SerializedWorkflowMacroNote.New(Factory, cacheParent.Identifier, cacheParent.ParentTable);

			var serializedSyntaxNodes = new JArray();
			foreach (var node in syntaxNodes)
			{
				var serializedNode = new JObject
				{
					{ (NoResString)"Macro", node.Key },
					{ (NoResString)"Value", syntaxNodeSerializationHelper.Serialize(node.Value) }
				};
				serializedSyntaxNodes.Add(serializedNode);
			}

			note.WriteToNoteData(serializedSyntaxNodes);
		}

		public void Delete(IHaveMacrosForSyntaxNodeCache cacheParent)
		{
			var note = SerializedWorkflowMacroNote.Load(Factory, cacheParent.Identifier);
			note?.Delete();
		}
	}
}
