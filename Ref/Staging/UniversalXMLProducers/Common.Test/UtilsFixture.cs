using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	[TestFixture]
	public class UtilsFixture
	{
		[Test]
		public void TestExportToXml()
		{
			var refExchangeRateList = new List<RefExchangeRateElement>();
			refExchangeRateList.Add(new RefExchangeRateElement()
			{
				ZZN_ExRateType = "CUS",
				ZZN_StartDate = new DateTime(2017, 02, 06),
				ZZN_EndDate = new DateTime(2017, 02, 06),
				ZZN_RX_NKExCurrency = "USD",
				ZZN_Rate = 1.1533M,
				ZZN_RN_NKCountry = "ZA"
			});

			refExchangeRateList.Add(new RefExchangeRateElement()
			{
				ZZN_ExRateType = "CUS",
				ZZN_StartDate = new DateTime(2017, 03, 03),
				ZZN_EndDate = new DateTime(2017, 03, 03),
				ZZN_RX_NKExCurrency = "JPY",
				ZZN_Rate = 129.03M,
				ZZN_RN_NKCountry = "ZA"
			});

			var publicationTime = new DateTime(2017, 01, 01);
			var targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"testexchangerate_20180102.xml");
			Utils.ExportToXMLFile("Test Exchange Rates", publicationTime, refExchangeRateList, targetPath);
			var xmlDoc = new XmlDocument();
			xmlDoc.Load(targetPath);

			var expected = @"
	<UniversalReferenceData>
	<PublicationTime>" + $"{publicationTime:s}" + @"</PublicationTime>
	<UpdateType>FULL</UpdateType>
	<DataSource>Test Exchange Rates</DataSource>
	<Schema>
		<EntityType Name=""RefExchangeRateZZ"" Data=""true"">
		  <Key>
			<PropertyRef Name=""ZZN_ExRateType"" />
			<PropertyRef Name=""ZZN_RX_NKExCurrency"" />
			<PropertyRef Name=""ZZN_RN_NKCountry"" />
			<PropertyRef Name=""ZZN_StartDate"" />
		  </Key>
		  <Property Name=""ZZN_ExRateType"" Type=""String"" MaxLength=""3"" FixedLength=""false"" Unicode=""false"" />
		  <Property Name=""ZZN_StartDate"" Type=""DateTime"" Precision=""0"" />
		  <Property Name=""ZZN_EndDate"" Type=""DateTime"" Precision=""0"" />
		  <Property Name=""ZZN_Rate"" Type=""Decimal"" Precision=""18"" Scale=""9"" />
		  <Property Name=""ZZN_RX_NKExCurrency"" Type=""String"" MaxLength=""3"" FixedLength=""true"" Unicode=""false"" />
		  <Property Name=""ZZN_RN_NKCountry"" Type=""String"" MaxLength=""2"" FixedLength=""true"" Unicode=""false"" />
		</EntityType>
	</Schema>
	<RefExchangeRateZZ>
		<ZZN_ExRateType>CUS</ZZN_ExRateType>
		<ZZN_StartDate>2017-02-06T00:00:00</ZZN_StartDate>
		<ZZN_EndDate>2017-02-06T00:00:00</ZZN_EndDate>
		<ZZN_Rate>1.1533</ZZN_Rate>
		<ZZN_RX_NKExCurrency>USD</ZZN_RX_NKExCurrency>
		<ZZN_RN_NKCountry>ZA</ZZN_RN_NKCountry>
	</RefExchangeRateZZ>
	<RefExchangeRateZZ>
		<ZZN_ExRateType>CUS</ZZN_ExRateType>
		<ZZN_StartDate>2017-03-03T00:00:00</ZZN_StartDate>
		<ZZN_EndDate>2017-03-03T00:00:00</ZZN_EndDate>
		<ZZN_Rate>129.0300</ZZN_Rate>
		<ZZN_RX_NKExCurrency>JPY</ZZN_RX_NKExCurrency>
		<ZZN_RN_NKCountry>ZA</ZZN_RN_NKCountry>
	</RefExchangeRateZZ>
</UniversalReferenceData>";
			var xml = new XmlDocument();
			xml.InnerXml = expected;

			Assert.True(xmlDoc.InnerXml.Contains(xml.InnerXml));
		}

		[Test]
		public void TestFormatCurrency()
		{
			Assert.AreEqual(3.1412, Utils.FormatCurrency(3.14124M));
			Assert.AreEqual(3.1413, Utils.FormatCurrency(3.14125M));
			Assert.AreEqual(3.1413, Utils.FormatCurrency(3.14126M));
		}

		[Test]
		public void TestGetLastDayInMonthDateString()
		{
			var dateString = Utils.GetLastDayInMonthDateString(2017, 2);
			Assert.AreEqual("20170228", dateString);

			dateString = Utils.GetLastDayInMonthDateString(2020, 2);
			Assert.AreEqual("20200229", dateString);
		}

		[TestCase("6110000000", "6110")]
		public void TestRemoveTrailingZeros(string input, string expected)
		{
			var actual = Utils.RemoveTrailingZeros(input);
			Assert.AreEqual(expected, actual);
		}
	}
}
