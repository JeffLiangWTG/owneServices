using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgCodeMappingForeignModuleFilter))]
	sealed class OrgCodeMappingForeignModuleFilterTest : OrgCodeMappingBaseModuleFilterTest
	{
		#region Empty

		public override void TestIsEmpty()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingForeignFilter = (OrgCodeMappingForeignModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("AAA");
			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				AssertEquals("IsEmpty", true, orgCodeMappingForeignFilter.IsEmpty);

				orgCodeMappingForeignFilter.ForeignCode = "XXX";
				AssertEquals("IsEmpty", false, orgCodeMappingForeignFilter.IsEmpty);

				orgCodeMappingForeignFilter.ForeignCode = ZString.Empty;
				orgCodeMappingForeignFilter.Context = "AAA";
				AssertEquals("IsEmpty", false, orgCodeMappingForeignFilter.IsEmpty);

				orgCodeMappingForeignFilter.Context = ZString.Empty;
				orgCodeMappingForeignFilter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				AssertEquals("IsEmpty", false, orgCodeMappingForeignFilter.IsEmpty);

				orgCodeMappingForeignFilter.RelationshipType = ZString.Empty;
				AssertEquals("IsEmpty", true, orgCodeMappingForeignFilter.IsEmpty);
			}
		}

		#endregion

		#region Clear

		public override void TestClear()
		{
			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingForeignFilter = (OrgCodeMappingForeignModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();

			validCodes.AddPair("AAA");
			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			{
				orgCodeMappingForeignFilter.ForeignCode = "XYZ";
				orgCodeMappingForeignFilter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				orgCodeMappingForeignFilter.Context = "AAA";

				CombineAssertions("Preconditions:", () =>
				{
					AssertEquals("Foreign Code", "XYZ", orgCodeMappingForeignFilter.ForeignCode);
					AssertEquals("Relationship Type", Constants.OrgPatternMatchOverrideRelationships.Organisation, orgCodeMappingForeignFilter.RelationshipType);
					AssertEquals("Context", "AAA", orgCodeMappingForeignFilter.Context);
				});

				orgCodeMappingForeignFilter.Clear();

				CombineAssertions("All fields should be empty", () =>
				{
					AssertEquals("Foreign Code", ZString.Empty, orgCodeMappingForeignFilter.ForeignCode);
					AssertEquals("Relationship Type", ZString.Empty, orgCodeMappingForeignFilter.RelationshipType);
					AssertEquals("Context", ZString.Empty, orgCodeMappingForeignFilter.Context);
				});
			}
		}

		#endregion

		#region Serialization

		public override void TestSerialization()
		{
			#region ExpectedXML

			const string ExpectedXML = @"<Filter>
  <Comparer>starts with</Comparer>
  <Property />
  <RelationshipType>ORG</RelationshipType>
  <Context>ABC</Context>
  <ForeignCode>DEF</ForeignCode>
</Filter>";

			#endregion

			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingForeignFilter = (OrgCodeMappingForeignModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("ABC");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				orgCodeMappingForeignFilter.ForeignCode = "DEF";
				orgCodeMappingForeignFilter.RelationshipType = Constants.OrgPatternMatchOverrideRelationships.Organisation;
				orgCodeMappingForeignFilter.Context = "ABC";

				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)orgCodeMappingForeignFilter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals("", ExpectedXML, writer.ToString());
			}
		}

		public override void TestDeserialization()
		{
			#region TestXML

			const string TestXML = @"<Filter>
  <Comparer>starts with</Comparer>
  <Property />
  <RelationshipType>ORG</RelationshipType>
  <Context>QQQ</Context>
  <ForeignCode>123</ForeignCode>
</Filter>";

			#endregion

			var filterStripBizO = new OrganisationFilterBusinessObject();
			var orgCodeMappingForeignFilter = (OrgCodeMappingForeignModuleFilter)filterStripBizO[ExpectedDescription];
			var validCodes = new CodeDescriptionPairList();
			validCodes.AddPair("QQQ");

			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validCodes))
			using (var reader = new StringReader(TestXML))
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
				AssertEquals("Foreign Code", "123", orgCodeMappingForeignFilter.ForeignCode);
				AssertEquals("Relationship Type", "ORG", orgCodeMappingForeignFilter.RelationshipType);
				AssertEquals("Context", "QQQ", orgCodeMappingForeignFilter.Context);
			});
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedDescription => "Code Mapping (Foreign Code)";

		protected override ModuleTextFilter GetNewModuleFilter() => new OrgCodeMappingForeignModuleFilter(ExpectedDescription);

		#endregion
	}
}
