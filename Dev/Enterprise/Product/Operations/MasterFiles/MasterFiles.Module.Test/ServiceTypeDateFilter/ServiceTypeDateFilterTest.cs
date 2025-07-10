using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ServiceTypeDateFilter))]
	sealed class ServiceTypeDateFilterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);
		}

		public void TestProperties()
		{
			var dateBookedFilter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);
			var dateCompletedFilter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), false);

			CombineAssertions(() =>
			{
				AssertEquals("Service Type / Date Booked", dateBookedFilter.Description);
				AssertEquals("Service Type / Date Booked", dateBookedFilter.MultilingualDescription);
				AssertEquals("Service Type / Date Completed", dateCompletedFilter.Description);
				AssertEquals("Service Type / Date Completed", dateCompletedFilter.MultilingualDescription);

				AssertOtherProperties(dateBookedFilter);
				AssertOtherProperties(dateCompletedFilter);
			});

			void AssertOtherProperties(ServiceTypeDateFilter filter)
			{
				var jobService = Factory.New<JobService>();
				AssertEquals(FilterCategories.ModesAndTypes, filter.Category);
				AssertEquals(filter.JobServiceType_List, jobService.Lookups.JobServiceType_List);
				AssertType<ServiceTypeDateFilterValidation>(filter.Validation);
			}
		}

		public void TestClear()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);
			filter.JobServiceType = "kkk";
			filter.Property1 = ZDateTime.BrettsBirthday;
			filter.Property2 = ZDateTime.BrettsBirthday.AddHours(1);
			filter.Clear();

			CombineAssertions(() =>
			{
				AssertEquals(ZDateTime.Empty, filter.Property1);
				AssertEquals(ZDateTime.Empty, filter.Property2);
				AssertEquals(string.Empty, filter.JobServiceType);
				AssertEquals(true, filter.IsEmpty);
			});
		}

		public void TestSerialisation()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);
			filter.JobServiceType = "FUM";
			filter.Property1 = ZDateTime.BrettsBirthday;
			filter.Property2 = ZDateTime.BrettsBirthday.AddHours(1);

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;

				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("serialisation", SampleXml, writer.ToString());
			}
		}

		public void TestGetServiceTypeDateFilterControl()
		{
			var filterStrip = new ZFilterStrip();
			var dataSource = new ZBindingSource();
			var control = ServiceTypeDateFilter.GetServiceTypeDateFilterControl(filterStrip, dataSource);

			AssertType<ServiceTypeDateFilterControl>(control);
			control.Dispose();
			filterStrip.Dispose();
			dataSource.Dispose();
		}

		public void TestDeserilisation()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);
			using (StringReader reader = new StringReader(SampleXml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter"); // because we follow a broken pattern for reading xml.
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement(); // because we follow a broken pattern for reading xml.
			}

			CombineAssertions(delegate
			{
				AssertEquals(ZDateTime.BrettsBirthday, filter.Property1);
				AssertEquals(ZDateTime.BrettsBirthday.AddHours(1), filter.Property2);
				AssertEquals("FUM", filter.JobServiceType);
			});
		}

		const string SampleXml = @"<Filter>
  <SearchProperty />
  <Property1>1971-09-18 00:00:00.000</Property1>
  <Property2>1971-09-18 01:00:00.000</Property2>
  <FilterOption>Past</FilterOption>
  <PropertyDecimal1>0.00</PropertyDecimal1>
  <PropertyDecimal2>0.00</PropertyDecimal2>
  <JobServiceType>FUM</JobServiceType>
</Filter>";

		public void TestGetQuery_EmptyQuery()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);
			AssertEquals(true, filter.Query.IsEmpty);

			filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), false);
			AssertEquals(true, filter.Query.IsEmpty);
		}

		public void TestGetQuery_HasNoDate()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);

			CombineAssertions(() =>
			{
				filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

				AssertEquals(@"JC_PK IN 
(
	SELECT ES_ParentID FROM dbo.JobService WHERE ES_Booked is NULL
)
", filter.Query.LiteralTextSqlFormatted);
			});
		}

		public void TestGetQuery_HasDate()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);

			CombineAssertions(() =>
			{
				filter.PropertySearch = ModuleDateFilter.HasDateEntered;

				AssertEquals(@"JC_PK IN 
(
	SELECT ES_ParentID FROM dbo.JobService WHERE ES_Booked is not NULL
)
", filter.Query.LiteralTextSqlFormatted);
			});
		}

		public void TestGetQuery_HasDateInRange()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);

			CombineAssertions(() =>
			{
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
				filter.Property1 = ZDateTime.BrettsBirthday;
				filter.Property2 = ZDateTime.BrettsBirthday.AddDays(1);

				AssertEquals(@"JC_PK IN 
(
	SELECT ES_ParentID FROM dbo.JobService WHERE ES_Booked >= '1971-09-18 00:00:00.000' 
	AND
	ES_Booked <= '1971-09-19 00:00:00.000'
)
", filter.Query.LiteralTextSqlFormatted);
			});
		}

		public void TestGetQuery_HasJobServiceType()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), true);
			CombineAssertions(() =>
			{
				filter.JobServiceType = "FUM";

				AssertEquals(@"JC_PK IN 
(
	SELECT ES_ParentID FROM dbo.JobService WHERE ES_ServiceCode = 'FUM'
)
", filter.Query.LiteralTextSqlFormatted);
			});
		}

		public void TestGetQuery_HasJobServiceType_HasDateInRange()
		{
			var filter = new ServiceTypeDateFilter(new FilterStripBusinessObjectForTest(), typeof(CommonContainer), false);

			CombineAssertions(() =>
			{
				filter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
				filter.Property1 = ZDateTime.BrettsBirthday;
				filter.Property2 = ZDateTime.BrettsBirthday.AddDays(1);
				filter.JobServiceType = "FUM";

				AssertEquals(@"JC_PK IN 
(
	SELECT ES_ParentID FROM dbo.JobService WHERE 
	(
		ES_Completed >= '1971-09-18 00:00:00.000' 
		AND
		ES_Completed <= '1971-09-19 00:00:00.000'
	)
	AND
	ES_ServiceCode = 'FUM'
)
", filter.Query.LiteralTextSqlFormatted);
			});
		}
	}
}
