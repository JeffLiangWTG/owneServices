using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	class UtilsTest
	{
		[Test]
		public void GetDateTimeFromString()
		{
			var (success, date) = Utils.GetDateTimeFromString("2020-12-01", "yyyy-MM-dd");
			Assert.That(success, Is.True);
			Assert.That(date, Is.EqualTo(new DateTime(2020, 12, 01)));
		}

		[Test]
		public void GetDateTimeFromString_EmptyDateTimeString()
		{
			var (success, date) = Utils.GetDateTimeFromString(string.Empty, "yyyy-MM-dd");
			Assert.That(success, Is.False);
			Assert.That(date, Is.EqualTo(Constants.MinimumDateTime));
		}

		[Test]
		public void GetDateTimeFromString_NullDateTimeString()
		{
			var (success, date) = Utils.GetDateTimeFromString(null, "yyyy-MM-dd");
			Assert.That(success, Is.False);
			Assert.That(date, Is.EqualTo(Constants.MinimumDateTime));
		}

		[Test]
		public void GetDateTimeFromString_EmptyDateTimeFormatString()
		{
			var (success, date) = Utils.GetDateTimeFromString("2020-12-01", string.Empty);
			Assert.That(success, Is.False);
			Assert.That(date, Is.EqualTo(Constants.MinimumDateTime));
		}

		[Test]
		public void GetDateTimeFromString_NullDateTimeFormatString()
		{
			var (success, date) = Utils.GetDateTimeFromString("2020-12-01", null);
			Assert.That(success, Is.False);
			Assert.That(date, Is.EqualTo(Constants.MinimumDateTime));
		}

		[Test]
		public void GetDateTimeFromString_NotMatching()
		{
			var (success, date) = Utils.GetDateTimeFromString("2020-12-01", "yyyy.MM.dd");
			Assert.That(success, Is.False);
			Assert.That(date, Is.EqualTo(DateTime.MinValue));
		}

		[Test]
		public void GetDateTimeFromString_ExceedsMaxValue()
		{
			var (success, date) = Utils.GetDateTimeFromString("2080-12-01", "yyyy-MM-dd");
			Assert.That(success, Is.True);
			Assert.That(date, Is.EqualTo(Constants.MaximumDateTime));
		}

		[Test]
		public void GetXmlFromZipFileAndPublicationTime()	
		{
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"ICS2\MethodOfPayment\TestFiles\Input\RD_ICS2_TransportChargesMethodOfPayment.zip");

			var (sourceXml, publicationTime) = Utils.GetXmlFromZipFileAndPublicationTime(filePath, ".xml");

			Assert.That(sourceXml, Is.Not.Null);
			Assert.That(publicationTime, Is.EqualTo(new DateTime(2022, 06, 10, 00, 44, 58)));
		}

		[Test]
		public void FindXElementByAttribute()
		{
			var document = XDocument.Parse(@"<?xml version='1.0' encoding='utf - 8'?>
											  <RDEntityList>
												<RDEntity name='AAAAA'>												 
												</RDEntity>
												<RDEntity name='BBBBB'>												 
												</RDEntity>
											  </RDEntityList>");

			var entityElement = Utils.FindXElementByAttribute(document, "RDEntity", "name", "AAAAA");
			Assert.That(entityElement.ToString(), Is.EqualTo("<RDEntity name=\"AAAAA\"></RDEntity>"));

			entityElement = Utils.FindXElementByAttribute(document, "RDEntity", "name", "BBBBB");
			Assert.That(entityElement.ToString(), Is.EqualTo("<RDEntity name=\"BBBBB\"></RDEntity>"));

			entityElement = Utils.FindXElementByAttribute(document, "RDEntity", "name", "CCCCC");
			Assert.That(entityElement, Is.Null);
		}

		[Test]
		public void FindXElementValueByAttribute()
		{
			var element = XElement.Parse(@"<RDEntityList>
												<RDEntity name='AAAAA'>11</RDEntity>
												<RDEntity name='BBBBB'>22</RDEntity>
											</RDEntityList>");

			var result = Utils.FindXElementValueByAttribute(element, "RDEntity", "name", "AAAAA");
			Assert.That(result, Is.EqualTo("11"));

			result = Utils.FindXElementValueByAttribute(element, "RDEntity", "name", "BBBBB");
			Assert.That(result, Is.EqualTo("22"));

			result = Utils.FindXElementValueByAttribute(element, "RDEntity", "name", "CCCCC");
			Assert.That(result, Is.Null);
		}

		[Test]
		public void FindXElementValueFirstOne()
		{
			var element = XElement.Parse(@"<RDEntityList>
												<RDEntity>11</RDEntity>
												<RDEntity>22</RDEntity>
											  </RDEntityList>");

			var result = Utils.FindXElementValueFirstOne(element, "RDEntity");
			Assert.That(result, Is.EqualTo("11"));
		}
	}
}
