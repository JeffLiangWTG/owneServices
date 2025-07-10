using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(DateLocationFilter))]
	sealed class DateLocationFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperty3Validation()
		{
			string errorText = "ERROR!!!";
			Filter.Property3Validation = null;
			Filter.Property3 = ZString.Empty;

			Filter.Validation.ValidateProperty3();
			AssertNoError(Filter.Property3Info, errorText);

			Filter.Property3Validation = (ZPropertyInfo info) =>
			{
				if (info.Value.IsEmpty)
				{
					info.AddError(errorText);
				}
			};

			Filter.Validation.ValidateProperty3();
			AssertHasError(Filter.Property3Info, errorText);
		}

		public void TestDefaultValues()
		{
			AssertEquals("Property1", ZDateTime.Empty, Filter.Property1);
			AssertEquals("Property2", ZDateTime.Empty, Filter.Property2);
			AssertEquals("Property3", "", Filter.Property3);
		}

		public void TestClear()
		{
			ZString searchValue = "Today";
			ZDateTime dateValue1 = new ZDateTime(2012, 05, 05);
			ZDateTime dateValue2 = new ZDateTime(2012, 05, 06);
			ZString location = "AUSYD";

			Filter.PropertySearch = searchValue;
			Filter.Property1 = dateValue1;
			Filter.Property2 = dateValue2;
			Filter.Property3 = location;

			AssertEquals("Precondition", searchValue, Filter.PropertySearch);
			AssertEquals("Precondition", dateValue1, Filter.Property1);
			AssertEquals("Precondition", dateValue2, Filter.Property2);
			AssertEquals("Precondition", location, Filter.Property3);

			Filter.Clear();
			AssertEquals(ZString.Empty, Filter.PropertySearch);
			AssertEquals(ZDateTime.Empty, Filter.Property1);
			AssertEquals(ZDateTime.Empty, Filter.Property2);
			AssertEquals(ZString.Empty, Filter.Property3);
		}

		public void TestIsEmpty()
		{
			Filter.Property1 = ZDateTime.Today;
			Filter.Property2 = ZDateTime.Today;

			Filter.PropertySearch = ZString.Empty;
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "Tomorrow";
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = "crap data";
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			AssertEquals(false, Filter.IsEmpty);

			Filter.Property1 = ZDateTime.Empty;
			Filter.Property2 = ZDateTime.Empty;
			AssertEquals(true, Filter.IsEmpty);

			Filter.PropertySearch = "Tomorrow";
			AssertEquals(false, Filter.IsEmpty);
		}

		public void TestSerialisation()
		{
			Filter.PropertySearch = "Tomorrow";
			Filter.Property1 = new ZDateTime(2008, 1, 1, 10, 0, 0);
			Filter.Property2 = new ZDateTime(2008, 2, 2, 10, 0, 0);
			Filter.Property3 = "AUSYD";

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("DateLocationFilter Serialisation", SampleXml, writer.ToString());
			}
		}

		public void TestSerialisationOffsetRange()
		{
			Filter.PropertySearch = ModuleDateFilter.SpecifiedHourOffsetRange;
			Filter.Property1 = new ZDateTime(2020, 1, 1, 10, 0, 0);
			Filter.Property2 = new ZDateTime(2020, 2, 1, 10, 0, 0);
			Filter.FilterOption = DateOffsetRangeFilterOptions.Codes.Future;
			Filter.PropertyDecimal1 = new ZDecimal(2.00);
			Filter.PropertyDecimal2 = new ZDecimal(10.00);
			Filter.Property3 = "AUSYD";

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("DateLocationFilterWithOffsetRange Serialisation", SampleXml2, writer.ToString());
			}
		}

		public void TestDeserialisation()
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

			CombineAssertions(() =>
			{
				AssertEquals("PropertySearch", "Tomorrow", Filter.PropertySearch);
				AssertEquals("Property1", new ZDateTime(2008, 01, 01, 10, 0, 0), Filter.Property1);
				AssertEquals("Property2", new ZDateTime(2008, 02, 02, 10, 0, 0), Filter.Property2);
				AssertEquals("Property3", "AUSYD", Filter.Property3);
			});
		}

		public void TestDeserialisationPreviousFilters()
		{
			using (StringReader reader = new StringReader(SampleXmlPrevious))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)Filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			CombineAssertions(() =>
			{
				AssertEquals("PropertySearch", "Tomorrow", Filter.PropertySearch);
				AssertEquals("Property1", new ZDateTime(2008, 01, 01, 10, 0, 0), Filter.Property1);
				AssertEquals("Property2", new ZDateTime(2008, 02, 02, 10, 0, 0), Filter.Property2);
				AssertEquals("Property3", "AUSYD", Filter.Property3);
			});
		}

		public void TestDeserialisationOffsetRange()
		{
			using (var reader = new StringReader(SampleXml2))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)Filter).ReadXml(xmlReader);
			}
			CombineAssertions(() =>
			{
				AssertEquals("PropertySearch", ModuleDateFilter.SpecifiedHourOffsetRange, Filter.PropertySearch);
				AssertEquals("Property1", new ZDateTime(2020, 01, 01, 10, 0, 0), Filter.Property1);
				AssertEquals("Property2", new ZDateTime(2020, 02, 01, 10, 0, 0), Filter.Property2);
				AssertEquals("PropertyDecimal1", new ZDecimal(2.00), Filter.PropertyDecimal1);
				AssertEquals("PropertyDecimal2", new ZDecimal(10.00), Filter.PropertyDecimal2);
				AssertEquals("FilterOption", DateOffsetRangeFilterOptions.Codes.Future, Filter.FilterOption);
				AssertEquals("Property3", "AUSYD", Filter.Property3);
			});
		}

		public void TestLocationCanFilterOn2LetterCountryCode()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Transport transport = consol.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUMEL";

			Factory.Save();

			Filter.Property3 = "AUMEL";

			MainFormConsolCollection collection = new MainFormConsolCollection(Factory);
			collection.Load(Filter.Query);
			AssertCollectionContains(consol, collection);

			Filter.Property3 = "US";

			collection = new MainFormConsolCollection(Factory);
			collection.Load(Filter.Query);
			AssertCollectionNotContains(consol, collection);

			Filter.Property3 = "AU";

			collection = new MainFormConsolCollection(Factory);
			collection.Load(Filter.Query);
			AssertCollectionContains(consol, collection);
		}

		public void TestGetQueryReturnsCorrectConsolQuery()
		{
			var consol1 = Factory.NewWithValidTestData<CommonConsol>();
			var transport1 = consol1.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUMEL";

			var consol2 = Factory.NewWithValidTestData<CommonConsol>();
			var transport2 = consol2.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";

			Factory.Save();

			Filter.Property3 = "AUMEL";

			var consolCollection = new MainFormConsolCollection(Factory);
			consolCollection.Load(Filter.Query);
			AssertEquals("Consol filter should return the consol", 1, consolCollection.Count);
			AssertCollectionContains(consol1, consolCollection);
			AssertCollectionNotContains(consol2, consolCollection);
		}

		public void TestGetQueryForShipmentQuery()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUMEL";
			var transport2 = shipment.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "USBOS";

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			var consolShipment = consol.Shipments.AddNew();
			var consolShipmentTransport = consolShipment.Transports.AddNew();
			consolShipmentTransport.JW_RL_NKLoadPort = "AUMEL";

			Factory.Save();

			var shipmentFilter = new DateLocationFilter("description", SailingFilterBuilder.Dates.ETD, Locations, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Shipment, Factory);
			shipmentFilter.Property3 = "AUMEL";

			var shipmentCollection = new ShipmentCollection(Factory);
			shipmentCollection.Load(shipmentFilter.Query);
			AssertEquals("Shipment Filter should return both shipments", 2, shipmentCollection.Count);
			AssertCollectionContains("Should contain the consol shipment as related consol has a load port in AUMEL", consolShipment, shipmentCollection);
			AssertCollectionContains("Should contain the shipment as it has a transport loading in AUMEL", shipment, shipmentCollection);

			shipmentFilter.Property3 = "USBOS";

			shipmentCollection.Load(shipmentFilter.Query);
			AssertEquals("Shipment Filter should only return the shipment", 1, shipmentCollection.Count);
			AssertCollectionNotContains(consolShipment, shipmentCollection);
			AssertCollectionContains(shipment, shipmentCollection);
		}

		#region Implementation

		const string SampleXml =
			"<Filter>\r\n" +
			"  <SearchProperty>Tomorrow</SearchProperty>\r\n" +
			"  <Property1>2008-01-01 10:00:00.000</Property1>\r\n" +
			"  <Property2>2008-02-02 10:00:00.000</Property2>\r\n" +
			"  <FilterOption>Past</FilterOption>\r\n" +
			"  <PropertyDecimal1>0.00</PropertyDecimal1>\r\n" +
			"  <PropertyDecimal2>0.00</PropertyDecimal2>\r\n" +
			"  <Property3>AUSYD</Property3>\r\n" +
			"</Filter>\r\n" +
			"";

		const string SampleXml2 =
			"<Filter>\r\n" +
			"  <SearchProperty>Offset range</SearchProperty>\r\n" +
			"  <Property1>2020-01-01 10:00:00.000</Property1>\r\n" +
			"  <Property2>2020-02-01 10:00:00.000</Property2>\r\n" +
			"  <FilterOption>Future</FilterOption>\r\n" +
			"  <PropertyDecimal1>2.00</PropertyDecimal1>\r\n" +
			"  <PropertyDecimal2>10.00</PropertyDecimal2>\r\n" +
			"  <Property3>AUSYD</Property3>\r\n" +
			"</Filter>\r\n" +
			"";

		const string SampleXmlPrevious =
			 "<Filter>\r\n" +
			 "  <SearchProperty>Tomorrow</SearchProperty>\r\n" +
			 "  <Property1>2008-01-01 10:00:00.000</Property1>\r\n" +
			 "  <Property2>2008-02-02 10:00:00.000</Property2>\r\n" +
			 "  <Property3>AUSYD</Property3>\r\n" +
			 "</Filter>\r\n" +
			 "";

		DateLocationFilter Filter
		{
			get { return filter ?? (filter = (DateLocationFilter)GetNewBusinessObject()); }
		}
		DateLocationFilter filter;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DateLocationFilter("description", SailingFilterBuilder.Dates.ETD, Locations, DateLocationFilter.LocationTypes.Load, DateLocationFilter.TargetFilterTypes.Consol, Factory);
		}

		IBusinessObjectCollection Locations
		{
			get { return locations ?? (locations = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<ILocationCollection>(), new object[] { Factory })); }
		}
		IBusinessObjectCollection locations;

		#endregion
	}
}
