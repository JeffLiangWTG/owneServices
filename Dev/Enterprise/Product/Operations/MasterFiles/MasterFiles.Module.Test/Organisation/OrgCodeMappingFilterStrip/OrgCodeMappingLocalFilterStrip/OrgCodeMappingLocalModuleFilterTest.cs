using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgCodeMappingLocalModuleFilter))]
	sealed class OrgCodeMappingLocalModuleFilterTest : ModuleTextFilterTest
	{
		#region Empty

		public void TestIsEmpty()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingLocalFilter = (OrgCodeMappingLocalModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("AAA");
			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				AssertEquals("Default is empty", true, orgCodeMappingLocalFilter.IsEmpty);

				orgCodeMappingLocalFilter.LocalCode = ZGuid.BrettsGuid;
				AssertEquals("Local Code Set - Not Empty", false, orgCodeMappingLocalFilter.IsEmpty);

				orgCodeMappingLocalFilter.LocalCode = ZGuid.Empty;
				orgCodeMappingLocalFilter.Context = "AAA";
				AssertEquals("Context Set - Not Empty", false, orgCodeMappingLocalFilter.IsEmpty);

				orgCodeMappingLocalFilter.Context = ZString.Empty;
				orgCodeMappingLocalFilter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				AssertEquals("Relationship Type Set - Not Empty", false, orgCodeMappingLocalFilter.IsEmpty);

				orgCodeMappingLocalFilter.RelationshipType = ZString.Empty;
				AssertEquals("Back to Empty", true, orgCodeMappingLocalFilter.IsEmpty);
			}
		}

		#endregion

		#region Clear

		public void TestClear()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingForeignFilter = (OrgCodeMappingLocalModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();

			validCodes.AddPair("AAA");
			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				orgCodeMappingForeignFilter.LocalCode = ZGuid.BrettsGuid;
				orgCodeMappingForeignFilter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				orgCodeMappingForeignFilter.Context = "AAA";

				CombineAssertions("Preconditions:", () =>
				{
					AssertEquals("Local Code", ZGuid.BrettsGuid, orgCodeMappingForeignFilter.LocalCode);
					AssertEquals("Relationship Type", Constants.OrgPatternMatchOverrideRelationships.Organisation, orgCodeMappingForeignFilter.RelationshipType);
					AssertEquals("Context", "AAA", orgCodeMappingForeignFilter.Context);
				});

				orgCodeMappingForeignFilter.Clear();

				CombineAssertions("All fields should be empty", () =>
				{
					AssertEquals("Local Code", ZGuid.Empty, orgCodeMappingForeignFilter.LocalCode);
					AssertEquals("Relationship Type", ZString.Empty, orgCodeMappingForeignFilter.RelationshipType);
					AssertEquals("Context", ZString.Empty, orgCodeMappingForeignFilter.Context);
				});
			}
		}

		#endregion

		#region Serialization

		public void TestSerialization()
		{
			#region ExpectedXML

			var expectedXML = $@"<Filter>
  <Comparer>starts with</Comparer>
  <Property />
  <RelationshipType>ORG</RelationshipType>
  <Context>ABC</Context>
  <LocalCode>{ZGuid.BrettsGuid.ToString()}</LocalCode>
</Filter>";

			#endregion

			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingForeignFilter = (OrgCodeMappingLocalModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("ABC");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				orgCodeMappingForeignFilter.LocalCode = ZGuid.BrettsGuid;
				orgCodeMappingForeignFilter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				orgCodeMappingForeignFilter.Context = "ABC";

				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)orgCodeMappingForeignFilter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals(expectedXML, writer.ToString());
			}
		}

		public void TestDeserialization()
		{
			#region TestXML

			var testXML = $@"<Filter>
  <Comparer>starts with</Comparer>
  <Property />
  <RelationshipType>ORG</RelationshipType>
  <Context>QQQ</Context>
  <LocalCode>{ZGuid.BrettsGuid.ToString()}</LocalCode>
</Filter>";

			#endregion

			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingForeignFilter = (OrgCodeMappingLocalModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("QQQ");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			using (var reader = new StringReader(testXML))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)orgCodeMappingForeignFilter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			CombineAssertions("Filter properties should all be deserialized", () =>
			{
				AssertEquals("Comparer", SQLComparisonOperator.StartsWith, orgCodeMappingForeignFilter.SqlComparisonOperator);
				AssertEquals("Local Code", ZGuid.BrettsGuid, orgCodeMappingForeignFilter.LocalCode);
				AssertEquals("Relationship Type", "ORG", orgCodeMappingForeignFilter.RelationshipType);
				AssertEquals("Context", "QQQ", orgCodeMappingForeignFilter.Context);
			});
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedDescription => "Code Mapping (Local Code)";

		protected override ModuleTextFilter GetNewModuleFilter() => new OrgCodeMappingLocalModuleFilter("Code Mapping (Local Code)");

		#endregion
	}
}
