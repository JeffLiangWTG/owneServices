using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	class SerializedWorkflowMacroDataStoreTest : TestCaseWithFactory
	{
		class CacheParentForTest : IHaveMacrosForSyntaxNodeCache
		{
			public CacheParentForTest(Guid identifier, string[] macros)
			{
				Identifier = identifier;
				Macros = macros;
			}

			readonly string[] Macros = Array.Empty<string>();

			public Guid Identifier { get; set; }

			public string ParentTable => "CacheParentForTest";

			public string[] GetAllMacrosForSyntaxNodeCache()
			{
				return Macros;
			}
		}

		bool SyntaxNodeEquals(SyntaxNode expected, SyntaxNode actual)
		{
			if (expected == null && actual == null)
			{
				return true;
			}
			if (expected == null || actual == null)
			{
				return false;
			}

			return expected.ToDebugString() == actual.ToDebugString();
		}

		Dictionary<string, SyntaxNode> MacrosToSyntaxNodesDict(string[] macros)
		{
			var syntaxNodes = macros.Select(c => c.CreateExpression().AST).ToArray();
			return macros.Zip(syntaxNodes, (macro, syntaxNode) => new { macro, syntaxNode }).ToDictionary(x => x.macro, x => x.syntaxNode);
		}

		void AssertArrayEqualsIgnoringOrder(SyntaxNode[] expected, SyntaxNode[] actual)
		{
			AssertEquals(expected.Length, actual.Length);

			CombineAssertions(() =>
			{
				foreach (var expectedNode in expected)
				{
					AssertNotNull($"{expectedNode.ToDebugString()}", actual.FirstOrDefault(actualNode => SyntaxNodeEquals(expectedNode, actualNode)));
				}
			}
			);
		}

		public void TestReadWrite()
		{
			string[] macros = ["Macro1", "Macro2"];
			var cacheParent = new CacheParentForTest(Guid.NewGuid(), macros);
			var expectedSyntaxNodesDict = MacrosToSyntaxNodesDict(macros);
			var dataStore = new SerializedWorkflowMacroDataStore(Factory);

			dataStore.Write(cacheParent, expectedSyntaxNodesDict);

			Factory.Save();

			var resultSyntaxNodesDict = dataStore.Read(cacheParent);

			AssertNotNull(resultSyntaxNodesDict);
			AssertArrayEqualsIgnoringOrder(expectedSyntaxNodesDict.Values.ToArray(), resultSyntaxNodesDict.Values.ToArray());
		}

		public void TestOverWrite()
		{
			string[] initialMacros = ["Macro1", "Macro2"];
			var cacheParent = new CacheParentForTest(Guid.NewGuid(), initialMacros);
			var initialSyntaxNodesDict = MacrosToSyntaxNodesDict(initialMacros);
			var dataStore = new SerializedWorkflowMacroDataStore(Factory);

			dataStore.Write(cacheParent, initialSyntaxNodesDict);
			Factory.Save();

			string[] updatedMacros = ["UpdatedMacro1", "UpdatedMacro2"];
			var updatedSyntaxNodesDict = MacrosToSyntaxNodesDict(updatedMacros);
			dataStore.Write(cacheParent, updatedSyntaxNodesDict);
			Factory.Save();

			var resultSyntaxNodesDict = dataStore.Read(cacheParent);
			AssertNotNull(resultSyntaxNodesDict);
			AssertArrayEqualsIgnoringOrder(updatedSyntaxNodesDict.Values.ToArray(), resultSyntaxNodesDict.Values.ToArray());
		}

		public void TestDeleteOutdatedNote()
		{
			var cacheParent = new CacheParentForTest(Guid.NewGuid(), null);

			var note = SerializedWorkflowMacroNote.New(Factory, cacheParent.Identifier, cacheParent.ParentTable);

			var jsonString = @"
			{
				""Version"": ""0.5"",
				""Value"": []
			}";

			note.ST_NoteData = System.Text.Encoding.UTF8.GetBytes(jsonString);

			Factory.Save();

			var dataStore = new SerializedWorkflowMacroDataStore(Factory);
			AssertNull(dataStore.Read(cacheParent));

			Factory.Save();

			var noteQuery = new ZQuery(StmNoteSchema.ST_Description, SerializedWorkflowMacroNote.Description).AddToFilter(StmNoteSchema.ST_ParentID, cacheParent.Identifier);
			AssertNull(Factory.LoadTop1<HiddenStmNote>(noteQuery));
		}

		public void TestDelete()
		{
			string[] macros = ["Macro1", "Macro2"];
			var cacheParent = new CacheParentForTest(Guid.NewGuid(), ["Macro1", "Macro2"]);
			var dataStore = new SerializedWorkflowMacroDataStore(Factory);

			dataStore.Write(cacheParent, MacrosToSyntaxNodesDict(macros));
			Factory.Save();

			var noteQuery = new ZQuery(StmNoteSchema.ST_Description, SerializedWorkflowMacroNote.Description).AddToFilter(StmNoteSchema.ST_ParentID, cacheParent.Identifier);
			var note = Factory.LoadTop1<HiddenStmNote>(noteQuery);
			AssertNotNull(note);

			dataStore.Delete(cacheParent);
			Factory.Save();

			AssertNull(Factory.LoadTop1<HiddenStmNote>(noteQuery));
		}
	}
}
