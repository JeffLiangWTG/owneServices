using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ReferenceNumberFilter))]
	sealed class ReferenceNumberFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_Code = "MYX";
			company.GC_RN_NKCountryCode = "MY";

			GlbBranch branch = company.Branches.AddNew();
			branch.GB_Code = "XXX";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				AssertEquals("Property", "", Filter.Property);
				AssertEquals("DefaultCountry", "MY", Filter.DefaultCountry);
				AssertEquals("Country", "MY", Filter.Country);
				AssertEquals("Type", "", Filter.Type);
			}
		}

		public void TestClear()
		{
			Filter.DefaultCountry = "NZ";

			Filter.Country = "AU";
			Filter.Type = "XXX";
			Filter.Property = "MAGIC";

			Filter.Clear();

			AssertEquals("Property", "", Filter.Property);
			AssertEquals("DefaultCountry", "NZ", Filter.DefaultCountry);
			AssertEquals("Country", "NZ", Filter.Country);
			AssertEquals("Type", "", Filter.Type);
		}

		public void TestIsEmpty()
		{
			Filter.Property = "";
			Filter.Country = "";
			Filter.Type = "";

			AssertEquals("Empty as all properties are empty", true, Filter.IsEmpty);

			Filter.Property = "MAGIC";
			AssertEquals("Not empty as 'Property' is not empty", false, Filter.IsEmpty);

			Filter.Property = "";
			Filter.Type = "COC";
			AssertEquals("Not empty as 'Type' is not empty", false, Filter.IsEmpty);

			// Country should be ignored for the IsEmpty check.
			// This is so we can default the country to the current login country without the default values
			// hiding anything. Also, filtering on country without either number or type is meaningless.
			Filter.Type = "";
			Filter.Country = "AU";
			AssertEquals("Empty *EVEN THOUGH* country is set", true, Filter.IsEmpty);
		}

		public void TestTypes()
		{
			CustomsReferenceNumberTypeCollection list = new CustomsReferenceNumberTypeCollection();
			list.RemoveAndDeleteAll();
			list.Add(new CustomsReferenceNumberType(list) { Code = "AAA" });
			list.Add(new CustomsReferenceNumberType(list) { Code = "BBB" });
			list.Add(new CustomsReferenceNumberType(list) { Code = "CCC" });

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			Filter.Country = "";
			AssertMultilineASCIIEquals("", "AAA, BBB, CCC", filter.Types.CodesAsString);

			Filter.Country = "US";
			AssertMultilineASCIIEquals("US", "IT, RRN, AAA, BBB, CCC", Filter.Types.CodesAsString);
		}

		public void TestGetReferenceNumberTypes()
		{
			RatingDataRegistry.Instance.EnableSpotRatingBehaviourFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Filter.Country = "AU";
			AssertContainsExactElementsInAnyOrder(new[] { "COC", "AMS", "UBR", "CON", "CLC", "BKG", "RLB", "UCR", "CAR", "LCR", "ACA", "CCN", "PCN", "BAG", "COU", "NAC", "OAG", "TWR", "HIR", "CQN", "SPO", "ACI", "ISF", "JDR", "CLR", "CMR", "EOE", "EOI", "CTK" }, filter.Types.GetAllCodes());
		}

		public void TestTypesFromModule()
		{
			CustomsReferenceNumberTypeCollection list = new CustomsReferenceNumberTypeCollection();
			list.RemoveAndDeleteAll();
			list.Add(new CustomsReferenceNumberType(list) { Code = "AAA" });
			list.Add(new CustomsReferenceNumberType(list) { Code = "BBB" });
			list.Add(new CustomsReferenceNumberType(list) { Code = "CCC" });

			FreightDataRegistry.Instance.CustomsAdditionalReferenceNumbers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var listFromModule = new CodeDescriptionPairList();
			listFromModule.Add(new CodeDescriptionPair("I1", "Item 1"));
			listFromModule.Add(new CodeDescriptionPair("I2", "Item 2"));
			listFromModule.Add(new CodeDescriptionPair("I3", "Item 3"));

			var filter1 = new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory));
			var filter2 = new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory), listFromModule);

			AssertListsAreSame(list, filter1);
			AssertContainsExactElementsInAnyOrder("", listFromModule, filter2.Types);
		}

		void AssertListsAreSame(CustomsReferenceNumberTypeCollection list, ReferenceNumberFilter filter1)
		{
			AssertEquals("Reference type should be same.", list[0].Code, filter1.Types[0].Code);
			AssertEquals("Reference type should be same.", list[1].Code, filter1.Types[1].Code);
			AssertEquals("Reference type should be same.", list[2].Code, filter1.Types[2].Code);
		}

		public void TestSerialisation()
		{
			Filter.Country = "NZ";
			Filter.Type = "COC";
			Filter.Property = "MAGIC1";

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("serialisation", SampleXml, writer.ToString());
			}
		}

		public void TestDeserilisation()
		{
			using (StringReader reader = new StringReader(SampleXml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter"); // because we follow a broken pattern for reading xml.
				((IXmlSerializable)Filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement(); // because we follow a broken pattern for reading xml.
			}

			CombineAssertions(delegate
			{
				AssertEquals("Country", "NZ", Filter.Country);
				AssertEquals("TypeProperty", "COC", Filter.Type);
				AssertEquals("Property", "MAGIC1", Filter.Property);
			});
		}

		#region Implementation

		const string SampleXml =
			"<Filter>\r\n" +
			"  <Comparer>starts with</Comparer>\r\n" +
			"  <Property>MAGIC1</Property>\r\n" +
			"  <Country>NZ</Country>\r\n" +
			"  <Type>COC</Type>\r\n" +
			"</Filter>\r\n" +
			"";

		ReferenceNumberFilter Filter
		{
			get { return filter ?? (filter = new ReferenceNumberFilter("description", delegate { return new ZQuery(); }, new RefCountryCollection(Factory))); }
		}
		ReferenceNumberFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReferenceNumberFilter("description", delegate
			{ return new ZQuery(); }, new RefCountryCollection(Factory));
		}

		#endregion
	}
}
