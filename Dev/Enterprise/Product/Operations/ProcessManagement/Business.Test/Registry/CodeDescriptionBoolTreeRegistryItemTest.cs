using System.Collections.Generic;
using System.IO;
using System.Text;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Business.Test
{
	[TestedType(typeof(CodeDescriptionBoolTreeRegistryItem))]
	public class CodeDescriptionBoolTreeRegistryItemTest : StronglyTypedRegistryItemTestCase<CodeDescriptionBoolTreeNodeCollection, CodeDescriptionBoolTreeNodeCollection>
	{
		public void TestMaxDepthSerialization()
		{
			for (int depth = 2; depth <= 8; ++depth)
			{
				var defaultValue = new CodeDescriptionBoolTreeNodeCollection(true, 3, depth);
				var dataType = new CodeDescriptionBoolTreeRegistryDataType(defaultValue);
				AssertEquals(depth.ToString(), depth, dataType.Deserialise(dataType.Serialise(defaultValue)).MaxDepth);
			}
		}

		public void TestDescriptionSerialization()
		{
			for (int depth = 2; depth <= 8; ++depth)
			{
				var allDescriptions = new List<MultilingualString>(depth - 1);
				for (int i = 0; i < depth - 1; ++i)
				{
					allDescriptions.Add((NoResString)i.ToString());
				}

				var codeLists = new List<CodeDescriptionPairList>(depth);
				for (int i = 0; i < depth; ++i)
				{
					var codeList = new CodeDescriptionPairList();
					codeList.AddPair("Code " + i, "Desc " + i);
					codeLists.Add(codeList);
				}

				var defaultValue = new CodeDescriptionBoolTreeNodeCollection(true, 3, depth, allDescriptions.ToArray(), codeLists.ToArray());
				defaultValue.AddSystemChildren(null);
				defaultValue.AddSystemChildren(defaultValue.Add("TY1", (NoResString)"TY1 Desc"));
				var dataType = new CodeDescriptionBoolTreeRegistryDataType(defaultValue);
				var deserialized = dataType.Deserialise(dataType.Serialise(defaultValue));
				for (int i = 0; i < deserialized.Count; ++i)
				{
					var node = deserialized[i];
					if (node.SystemDefined)
					{
						AssertEquals(allDescriptions[deserialized.GetDepth(node) - 1], node.Description);
					}
					else
					{
						AssertEquals("TY1", node.Code);
						AssertEquals("TY1 Desc", node.Description);
					}
					AssertContainsExactElementsInAnyOrder(codeLists[deserialized.GetDepth(node) - 1], node.CodeList);
				}
			}
		}

		protected override StronglyTypedRegistryItem<CodeDescriptionBoolTreeNodeCollection, CodeDescriptionBoolTreeNodeCollection> GetNewRegistryItem()
		{
			var defaultValue = new CodeDescriptionBoolTreeNodeCollection(true, 3, 8);

			return new CodeDescriptionBoolTreeRegistryItem(
				"CodeDescriptionBoolTreeRegistryItemTest",
				(NoResString)"Category",
				(NoResString)"Caption",
				(NoResString)"Hint",
				RegistryStorageFlags.System,
				null,
				defaultValue);
		}
	}

	[TestedType(typeof(CodeDescriptionBoolTreeRegistryDataType))]
	public class CodeDescriptionBoolTreeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CodeDescriptionBoolTreeRegistryDataType>
	{
		protected override bool HasEditor
		{
			get { return false; }
		}

		protected override CodeDescriptionBoolTreeRegistryDataType GetNewDataType()
		{
			return new CodeDescriptionBoolTreeRegistryDataType(new CodeDescriptionBoolTreeNodeCollection());
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var list1 = new CodeDescriptionBoolTreeNodeCollection(true, 3, 5);
			var list2 = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			var node1 = list2.Add("AAA", (NoResString)"AAA Desc");
			list2.Add("BBB", (NoResString)"BBB Desc", node1);

			var serializer = ZXmlSerializer.New(list1.GetType());

			StringBuilder xml1 = new StringBuilder();
			using (StringWriter stream = new StringWriter(xml1))
			{
				serializer.Serialize(stream, list1);
			}

			StringBuilder xml2 = new StringBuilder();
			using (StringWriter stream = new StringWriter(xml2))
			{
				serializer.Serialize(stream, list2);
			}

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(list1, Encoding.Unicode.GetBytes(xml1.ToString())),
				new ValidSampleAndBinaryValueInDB(list2, Encoding.Unicode.GetBytes(xml2.ToString()))
			};
		}
	}
}
