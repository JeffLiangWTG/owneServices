using System;
using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(AllocationMethodDefaultHeader))]
	sealed class AllocationMethodDefaultHeaderTest : RegistryBusinessObjectTemplateTestCase<AllocationMethodDefaultHeader>
	{
		public void TestSerialisation()
		{
			const string ExpectedXML =
				"<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
				"<AllocationMethodDefaultHeader>\r\n" +
				"  <DefaultAllocationMethod>IGR</DefaultAllocationMethod>\r\n" +
				"  <Rules>\r\n" +
				"    <Rule>\r\n" +
				"      <Country>AU</Country>\r\n" +
				"      <AllocationMethod>ORI</AllocationMethod>\r\n" +
				"    </Rule>\r\n" +
				"    <Rule>\r\n" +
				"      <Country>NZ</Country>\r\n" +
				"      <AllocationMethod>COU</AllocationMethod>\r\n" +
				"    </Rule>\r\n" +
				"  </Rules>\r\n" +
				"</AllocationMethodDefaultHeader>" +
				"";

			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(AllocationMethodDefaultHeader));

			using (StringWriter writer = new StringWriter())
			{
				AllocationMethodDefaultHeader header = new AllocationMethodDefaultHeader(Factory);
				header.DefaultAllocationMethod = AllocationMethodList.Codes.Ignore;

				AllocationMethodDefaultRule rule1 = header.Rules.AddNew();
				rule1.AllocationMethod = AllocationMethodList.Codes.Origin;
				rule1.CountryCode = "AU";

				AllocationMethodDefaultRule rule2 = header.Rules.AddNew();
				rule2.AllocationMethod = AllocationMethodList.Codes.Country;
				rule2.CountryCode = "NZ";

				serialiser.Serialize(writer, header);
				writer.Flush();

				AssertMultilineASCIIEquals("", ExpectedXML, writer.ToString());
			}

			using (StringReader reader = new StringReader(ExpectedXML))
			{
				AllocationMethodDefaultHeader header = (AllocationMethodDefaultHeader)serialiser.Deserialize(reader);

				AssertEquals(AllocationMethodList.Codes.Ignore, header.DefaultAllocationMethod);
				AssertContainsExactElementsInAnyOrder("Rules",
					new string[]
					{
						"AU - ORI",
						"NZ - COU"
					},
					Array.ConvertAll(header.Rules.ToArray<AllocationMethodDefaultRule>(), (r) => string.Format("{0} - {1}", r.CountryCode, r.AllocationMethod))
				);
			}
		}

		public void TestValidateDefaultAllocationMethod()
		{
			Header.DefaultAllocationMethod = "XXX";
			AssertHasError(Header.DefaultAllocationMethodInfo, "Enter a valid Default Allocation Method.");

			Header.DefaultAllocationMethod = AllocationMethodList.Codes.Origin;
			AssertNoNotifications(Header.DefaultAllocationMethodInfo);

			Header.DefaultAllocationMethod = "";
			AssertHasError(Header.DefaultAllocationMethodInfo, "Please enter a Default Allocation Method.");
		}

		public void TestGetAllocationMethod()
		{
			Header.DefaultAllocationMethod = AllocationMethodList.Codes.Ignore;

			AllocationMethodDefaultRule rule1 = Header.Rules.AddNew();
			rule1.CountryCode = "AU";
			rule1.AllocationMethod = AllocationMethodList.Codes.Country;

			AllocationMethodDefaultRule rule2 = Header.Rules.AddNew();
			rule2.CountryCode = "NZ";
			rule2.AllocationMethod = AllocationMethodList.Codes.Origin;

			AssertEquals(AllocationMethodList.Codes.Country, Header.GetAllocationMethod("AU"));
			AssertEquals(AllocationMethodList.Codes.Origin, Header.GetAllocationMethod("NZ"));
			AssertEquals(AllocationMethodList.Codes.Ignore, Header.GetAllocationMethod("SG"));
		}

		public void TestSetDefaultValues()
		{
			AllocationMethodDefaultHeader header = new AllocationMethodDefaultHeader();
			AssertEquals(AllocationMethodList.Codes.NotSet, header.DefaultAllocationMethod);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override AllocationMethodDefaultHeader GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override AllocationMethodDefaultHeader GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		AllocationMethodDefaultHeader NewPopulatedBusinessObject()
		{
			AllocationMethodDefaultHeader result = new AllocationMethodDefaultHeader(Factory);
			result.DefaultAllocationMethod = AllocationMethodList.Codes.Ignore;

			AllocationMethodDefaultRule rule = result.Rules.AddNew();
			rule.CountryCode = "AU";
			rule.AllocationMethod = AllocationMethodList.Codes.Sailing;

			return result;
		}

		AllocationMethodDefaultHeader Header
		{
			get { return header ?? (header = new AllocationMethodDefaultHeader(Factory)); }
		}
		AllocationMethodDefaultHeader header;

		#endregion
	}
}
