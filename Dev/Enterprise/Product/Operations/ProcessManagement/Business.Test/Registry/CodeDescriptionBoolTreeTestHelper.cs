using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ProcessManagement.Business.Test
{
	public static class CodeDescriptionBoolTreeTestHelper
	{
		const string ALL = CodeDescriptionBoolTreeNode.AllCode;

		public static CodeDescriptionBoolTreeNodeCollection CreateTestTree3()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			AddTree3(tree);
			return tree;
		}

		static void AddTree3(CodeDescriptionBoolTreeNodeCollection tree)
		{
			tree.AddSystemChildren();

			Add(tree, "1A");
			Add(tree, "1A", "2A");
			Add(tree, "1A", "2B");
			Add(tree, "1A", "2B", "3A");
			Add(tree, "1A", "2B", "3A", "4A");
		}

		public static CodeDescriptionBoolTreeNodeCollection CreateTestTree4()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			AddTree4(tree);
			return tree;
		}

		static void AddTree4(CodeDescriptionBoolTreeNodeCollection tree)
		{
			tree.AddSystemChildren();

			Add(tree, "1AA");
			Add(tree, "1AA", "2AA");
			Add(tree, "1AA", "2AB");
			Add(tree, "1AA", "2ZZ").Bool = false;
			Add(tree, "1AA", "2AA", "3AA");
			Add(tree, "1AA", "2AA", "3AA", "4AA");
			Add(tree, "1AA", "2AA", ALL, "4AX");
			Add(tree, "1AA", ALL, ALL, "4XX");
			Add(tree, ALL, ALL, ALL, "ANY");
			Add(tree, "1AA", ALL, "A11");

			Add(tree, "1BB");
			Add(tree, "1BB", "2B1");
			Add(tree, "1BB", "2B1", "3BB");

			Add(tree, ALL, "2CC");
			Add(tree, ALL, "2DD");

			Add(tree, ALL, "2CC", "C11");

			Add(tree, ALL, ALL, "3S1");
			Add(tree, ALL, ALL, "3S2");

			Add(tree, ALL, ALL, "3S1", "4S1");
			Add(tree, ALL, ALL, "3S1", "4S2");

			Add(tree, "1ZZ").Bool = false;
			Add(tree, "1ZZ", "2ZZ").Bool = false;
			Add(tree, "1ZZ", "2ZZ", "3ZZ").Bool = false;
			Add(tree, "1ZZ", "2ZZ", "3ZZ", "4ZZ").Bool = false;
		}

		public static CodeDescriptionBoolTreeNodeCollection CreateTestTree5()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 5);
			AddTree4(tree);
			Add(tree, ALL, ALL, ALL, ALL, "LOW");
			Add(tree, ALL, ALL, ALL, ALL, "MED");
			Add(tree, "1ZZ", "2ZZ", "3ZZ", "4ZZ", "5ZZ").Bool = false;

			return tree;
		}

		public static CodeDescriptionBoolTreeNode Add(CodeDescriptionBoolTreeNodeCollection tree, params string[] codes)
		{
			string[] parentCodes = new string[codes.Length - 1];
			Array.Copy(codes, parentCodes, parentCodes.Length);
			string code = codes[codes.Length - 1];
			CodeDescriptionBoolTreeNode parent = parentCodes.Length > 0 ? tree.Find(parentCodes) : null;
			var result = tree.Add(code, (NoResString)(code + " depth " + (parentCodes.Length + 1)), true, false, parent);
			tree.AddSystemChildren(result);
			return result;
		}
	}
}
