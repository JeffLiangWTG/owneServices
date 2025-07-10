using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ReferenceNumberDataAdapterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			Xsd.ReferenceNumberCollection numbers = Deserialise(GetPopulatedReferenceNumbersXml());

			NotificationBuffer buffer = new NotificationBuffer();
			CusEntryNumAdditionalReferenceCollection result = new CusEntryNumAdditionalReferenceCollection(Factory.New<DummyBusinessObject>());

			ReferenceNumberDataAdapter.ImportReferenceNumbers(result, numbers, new ValueObjectImportContext(Factory, buffer));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("Should have no errors", "", buffer.AsString);
				AssertEquals("result.Count", 3, result.Count);
			});

			CombineAssertions(delegate
			{
				AssertEquals("result[0].CE_RN_NKCountryCode", "AU", result[0].CE_RN_NKCountryCode);
				AssertEquals("result[0].CE_EntryType", "COC", result[0].CE_EntryType);
				AssertEquals("result[0].CE_EntryNum", "MAGIC", result[0].CE_EntryNum);

				AssertEquals("result[1].CE_RN_NKCountryCode", "US", result[1].CE_RN_NKCountryCode);
				AssertEquals("result[1].CE_EntryType", "BOB", result[1].CE_EntryType);
				AssertEquals("result[1].CE_EntryNum", "5341", result[1].CE_EntryNum);

				AssertEquals("result[2].CE_RN_NKCountryCode", "", result[2].CE_RN_NKCountryCode);
				AssertEquals("result[2].CE_EntryType", "DOD", result[2].CE_EntryType);
				AssertEquals("result[2].CE_EntryNum", "999", result[2].CE_EntryNum);
			});
		}

		public void TestImportForIAdditionalReferenceNumberTypeProvider()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			IAdditionalReferenceNumberTypeProvider numberTypeProvider = shipment;
			Assert("Precondition", numberTypeProvider.GetAdditionalReferenceNumberTypeList(CusEntryNumber.Categories.AdditionalReferenceNumber, GlbCompany.CurrentCompany.GC_RN_NKCountryCode).Count > 0);

			string otherAgentReference = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.OtherAgentReference;

			shipment.Numbers.AddNewIfNotExist(otherAgentReference, "hello");
			shipment.Numbers.AddNewIfNotExist(otherAgentReference, "cruel");

			Xsd.ReferenceNumberCollection numbers = new Xsd.ReferenceNumberCollection();
			var refNumber = numbers.AddNew();
			refNumber.Type = otherAgentReference;
			refNumber.Number = "world";

			ReferenceNumberDataAdapter.ImportReferenceNumbers(shipment.Numbers, numbers, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "hello", "cruel", "world" }, shipment.Numbers.GetAllReferenceNumbersByType(otherAgentReference));
		}

		public void TestExport()
		{
			CusEntryNumAdditionalReferenceCollection numbers = new CusEntryNumAdditionalReferenceCollection(Factory.New<DummyBusinessObject>());

			CusEntryNumber num1 = numbers.AddNew();
			num1.CE_RN_NKCountryCode = "AU";
			num1.CE_EntryType = "COC";
			num1.CE_EntryNum = "MAGIC";

			CusEntryNumber num2 = numbers.AddNew();
			num2.CE_RN_NKCountryCode = "US";
			num2.CE_EntryType = "BOB";
			num2.CE_EntryNum = "5341";

			CusEntryNumber num3 = numbers.AddNew();
			num3.CE_RN_NKCountryCode = "";
			num3.CE_EntryType = "DOD";
			num3.CE_EntryNum = "999";

			Xsd.ReferenceNumberCollection valueObject = new Xsd.ReferenceNumberCollection();
			NotificationBuffer buffer = new NotificationBuffer();

			ReferenceNumberDataAdapter.ExportReferenceNumbers(numbers, valueObject, new ValueObjectExportContext(buffer));

			string result = Serialise(valueObject);

			AssertMultilineASCIIEquals("Should have no errors", "", buffer.AsString);
			AssertXMLEquals("xml", GetPopulatedReferenceNumbersXml(), result);
		}

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string GetPopulatedReferenceNumbersXml()
		{
			return resourceRetriever.Value.GetString("Enterprise.Freight.DataTransfer.Test.Freight.Testing.PopulatedReferenceNumbers.xml");
		}

		static string Serialise(Xsd.ReferenceNumberCollection valueObject)
		{
			string result;

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.Indentation = 2;
				xmlWriter.IndentChar = ' ';

				XmlSerializer serialiser = NewSerialiser();
				serialiser.Serialize(xmlWriter, valueObject);
				result = writer.ToString();
			}

			return result;
		}

		static Xsd.ReferenceNumberCollection Deserialise(string xml)
		{
			Xsd.ReferenceNumberCollection result;

			using (StringReader reader = new StringReader(xml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				XmlSerializer serialiser = NewSerialiser();
				result = (Xsd.ReferenceNumberCollection)serialiser.Deserialize(xmlReader);
			}

			return result;
		}

		static XmlSerializer NewSerialiser()
		{
			return new XmlSerializer(typeof(Xsd.ReferenceNumberCollection));
		}

		#endregion
	}
}
