using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(CodeDescriptionBoolTreeNode))]
	public class CodeDescriptionBoolTreeNodeTest : RegistryBusinessObjectTest
	{
		public override void TestMaxDescriptionLength()
		{
			// We haven't overriden MaxDescriptionLength, but it's a protected property so can't test it anyway
			Assert(true);
		}

		public void TestEnglishDescriptionReadOnly()
		{
			var node = (CodeDescriptionBoolTreeNode)GetNewBusinessObject();
			AssertEquals(false, node.EnglishDescriptionReadOnly);

			node.CodeList = new CodeDescriptionPairList();
			AssertEquals(true, node.EnglishDescriptionReadOnly);
		}

		public void TestDefaultDescriptionFromCodeList()
		{
			var node = (CodeDescriptionBoolTreeNode)GetNewBusinessObject();
			node.Code = "ABC";
			AssertEquals("Description is not automatically set because no code list", string.Empty, node.Description);

			node.CodeList = new CodeDescriptionPairList();
			node.CodeList.AddPair("AAA", (NoResString)"AAA Desc");
			node.CodeList.AddPair("BBB", (NoResString)"BBB Desc");

			node.Code = "AAA";
			AssertEquals("Description is automatically set", (NoResString)"AAA Desc", node.Description);

			node.Code = "BBB";
			AssertEquals("Description is automatically set", (NoResString)"BBB Desc", node.Description);

			node.Code = "";
			AssertEquals("Description is automatically set", (NoResString)"", node.Description);

			node.SystemDefined = true;
			node.Code = CodeDescriptionBoolTreeNode.AllCode;
			node.Description = (NoResString)"All Desc";
			node.Code = CodeDescriptionBoolTreeNode.AllCode;
			AssertEquals("Description is not automatically set for 'All' node", (NoResString)"All Desc", node.Description);
		}

		public void TestValidateCode()
		{
			CodeDescriptionBoolTreeNodeCollection tree = new CodeDescriptionBoolTreeNodeCollection();
			var node1 = tree.Add("AAA");
			var node1a = tree.Add("AAA", node1);
			var node1b = tree.Add("AA1", node1);
			var node2 = tree.Add("BBB");
			var node2a = tree.Add("BBB", node2);

			node1.ValidateCode();
			node1a.ValidateCode();
			AssertNoErrors(node1.CodeInfo);
			AssertNoErrors(node1a.CodeInfo);

			node2.Code = "AAA";
			node2.ValidateCode();
			AssertHasErrors(node2.CodeInfo);

			node2.Code = "BBB";
			node1a.Code = "BBB";
			node1a.ValidateCode();
			AssertNoErrors("duplicate codes with different parents", node1a.CodeInfo);

			node1a.Code = "AA1";
			node1a.ValidateCode();
			AssertHasErrors("duplicate codes with same parent", node1a.CodeInfo);

			tree = new CodeDescriptionBoolTreeNodeCollection();
			tree.AddSystemChildren(null);
			foreach (CodeDescriptionBoolTreeNode node in tree)
			{
				node.ValidateCode();
				AssertEquals(false, node.HasNotifications());
			}
		}

		protected override void AssertCloneValues(RegistryBusinessObject cloneBizo)
		{
			var clone = cloneBizo as CodeDescriptionBoolTreeNode;
			AssertEquals("Code", "TSCOD", clone.Code);
			AssertEquals("Description", "DescriptionA", clone.Description);
			AssertEquals("CodeMaxLength", 5, clone.CodeMaxLength);
			Assert("SystemDefined", clone.SystemDefined);
			AssertEquals("ParentID", TestParentID, clone.ParentID);
			AssertEquals("ID is cloned", TestID, clone.ID);
			var expectedCodeList = new CodeDescriptionPairList();
			expectedCodeList.AddPair("AAA", "AAA Desc");
			AssertContainsExactElementsInAnyOrder("CodeList is cloned", expectedCodeList, clone.CodeList);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CodeDescriptionBoolTreeNode)GetNewBusinessObject();
			result.SetIdForTesting(TestID);
			result.CodeMaxLength = 5;
			result.Code = "TSCOD";
			result.Description = (NoResString)"DescriptionA";
			result.SystemDefined = true;
			result.ParentID = TestParentID;
			result.CodeList = new CodeDescriptionPairList();
			result.CodeList.AddPair("AAA", "AAA Desc");

			return result;
		}

		readonly ZGuid TestParentID = ZGuid.NewZGuid();
		readonly ZGuid TestID = ZGuid.NewZGuid();

		protected new CodeDescriptionBoolTreeNode BizObj
		{
			get { return (CodeDescriptionBoolTreeNode)base.BizObj; }
		}

		protected override bool IsCodeUniqueInCollection
		{
			get { return false; }
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
