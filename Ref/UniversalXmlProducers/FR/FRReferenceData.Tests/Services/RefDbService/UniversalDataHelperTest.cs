using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.FRReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.FRReferenceData.Tests.Services
{
	[TestFixture]
	public class UniversalDataHelperTest
	{
		public void TestSendEmail() //Developer test only. Do not add [Test] attribute. You don't want to spam my mailbox, do you ?
		{
			ApplicationConfig.Instance.EmailRecipients = "bernard.navarro@wisetechglobal.com";
			Assert.DoesNotThrow(() => UniversalDataHelper.SendEmail("Blabla", "Blablabla"));
		}

		[Test]
		public void TestCheckRateIsValid()
		{
			var rate = 1.234m;
			Assert.IsTrue(UniversalDataHelper.CheckRateIsValid(rate));

			rate = 0m;
			Assert.IsFalse(UniversalDataHelper.CheckRateIsValid(rate));
		}

		[Test]
		public void TestCheckDatesAreValid()
		{
			var startDate = DateTime.Today;
			var endDate = DateTime.Today.AddDays(1);
			Assert.AreEqual(true, UniversalDataHelper.CheckDatesAreValid(startDate, endDate));

			endDate = DateTime.Today;
			Assert.AreEqual(true, UniversalDataHelper.CheckDatesAreValid(startDate, endDate));

			endDate = DateTime.Today.AddDays(-1);
			Assert.AreEqual(false, UniversalDataHelper.CheckDatesAreValid(startDate, endDate));
		}

		[Test]
		public void TestCheckDatesIncludeToday()
		{
			var startDate = DateTime.Today.AddDays(-1);
			var endDate = DateTime.Today.AddDays(4);
			var code = new RefCusCodeList{
				ZZD_StartDate = startDate,
				ZZD_EndDate = endDate,
			};
			Assert.AreEqual(true, UniversalDataHelper.CheckDatesIncludeToday(startDate, endDate));
			Assert.AreEqual(true, UniversalDataHelper.CheckDatesIncludeToday(code));

			startDate = DateTime.Today.AddDays(2);
			code.ZZD_StartDate = startDate;
			Assert.AreEqual(false, UniversalDataHelper.CheckDatesIncludeToday(startDate, endDate));
			Assert.AreEqual(false, UniversalDataHelper.CheckDatesIncludeToday(code));;

			startDate = DateTime.Today.AddDays(-3);
			endDate = DateTime.Today.AddDays(-1);
			code.ZZD_StartDate = startDate;
			code.ZZD_EndDate = endDate;
			Assert.AreEqual(false, UniversalDataHelper.CheckDatesIncludeToday(startDate, endDate));
			Assert.AreEqual(false, UniversalDataHelper.CheckDatesIncludeToday(code));
		}

		[Test]
		public void TestFilterOutputListByDate()
		{
			var codeList = new List<RefCusCodeList>();

			AddCode(codeList, "CODE1", 1995, 1996, "a");
			AddCode(codeList, "CODE1", 1996, 1997, "b");

			AddCode(codeList, "CODE2", 1995, 1996, "c");
			AddCode(codeList, "CODE2", 1996, 2040, "d");

			AddCode(codeList, "CODE3", 1995, 1996, "c");
			AddCode(codeList, "CODE3", 1996, 2050, "e");
			AddCode(codeList, "CODE3", 1996, 2040, "d");

			AddCode(codeList, "CODE4", 1996, 2040, "d");
			AddCode(codeList, "CODE4", 2040, 2050, "e");

			AddCode(codeList, "CODE5", 1996, 2000, "d");
			AddCode(codeList, "CODE5", 2040, 2050, "e");

			AddCode(codeList, "CODE6", 2040, 2050, "d");
			AddCode(codeList, "CODE6", 2050, 2060, "e");

			codeList = UniversalDataHelper.FilterOutputListByDate(codeList);

			Assert.AreEqual(6, codeList.Count);
			AssertCodeAndDescription(codeList, 0, "CODE1", "b");
			AssertCodeAndDescription(codeList, 1, "CODE2", "d");
			AssertCodeAndDescription(codeList, 2, "CODE3", "e");
			AssertCodeAndDescription(codeList, 3, "CODE4", "d");
			AssertCodeAndDescription(codeList, 4, "CODE5", "e");
			AssertCodeAndDescription(codeList, 5, "CODE6", "e");

			void AddCode(List<RefCusCodeList> list, string code, int startYear, int endYear, string description)
			{
				list.Add(new RefCusCodeList
				{
					ZZD_Code = code,
					ZZD_Description = description,
					ZZD_StartDate = new DateTime(startYear, 10, 10),
					ZZD_EndDate = new DateTime(endYear, 10, 10)
				});
			}

			void AssertCodeAndDescription(List<RefCusCodeList> list, int index, string code, string description)
			{
				Assert.AreEqual(code, list[index].ZZD_Code);
				Assert.AreEqual(description, list[index].ZZD_Description);
			}
		}

		[Test]
		public void TestGetTagValue()
		{
			var node = CreateNode("tag1", "value1");
			Assert.AreEqual("value1", UniversalDataHelper.GetTagValue(node, "tag1"));
			Assert.AreEqual("", UniversalDataHelper.GetTagValue(node, "tag2"));
		}

		[Test]
		public void TestGetStartDateFromTag()
		{
			Assert.AreEqual(new DateTime(2022, 02, 23), UniversalDataHelper.GetStartDateFromTag(CreateNode("date", "23/02/2022"), "date"));
			Assert.AreEqual(UniversalDataHelper.MinimumDateTime, UniversalDataHelper.GetStartDateFromTag(CreateNode("date", "02/23/2022"), "date"), "Wrong format");
			Assert.AreEqual(UniversalDataHelper.MinimumDateTime, UniversalDataHelper.GetStartDateFromTag(CreateNode("date", "01/01/1800"), "date"), "Overflow");
			Assert.AreEqual(UniversalDataHelper.MinimumDateTime, UniversalDataHelper.GetStartDateFromTag(CreateNode("date", "31/12/2099"), "date"), "Overflow");
		}

		[Test]
		public void TestGetStartDateFromTag_WithDefaultDate()
		{
			Assert.AreEqual(new DateTime(2022, 02, 23), UniversalDataHelper.GetStartDateFromTag(CreateNode("date", "23/02/2022"), "date", new DateTime(2030, 12, 31)));
			Assert.AreEqual(new DateTime(2030, 12, 31), UniversalDataHelper.GetStartDateFromTag(CreateNode("date", "02/23/2022"), "date", new DateTime(2030, 12, 31)), "Wrong format and fallback to default date.");
		}

		[Test]
		public void TestGetEndDateFromTag()
		{
			Assert.AreEqual(new DateTime(2022, 02, 23), UniversalDataHelper.GetEndDateFromTag(CreateNode("date", "23/02/2022"), "date"));
			Assert.AreEqual(UniversalDataHelper.MaximumDateTime, UniversalDataHelper.GetEndDateFromTag(CreateNode("date", "02/23/2022"), "date"), "Wrong format");
			Assert.AreEqual(UniversalDataHelper.MaximumDateTime, UniversalDataHelper.GetEndDateFromTag(CreateNode("date", "01/01/1800"), "date"), "Overflow");
			Assert.AreEqual(UniversalDataHelper.MaximumDateTime, UniversalDataHelper.GetEndDateFromTag(CreateNode("date", "31/12/2099"), "date"), "Overflow");
		}

		[Test]
		public void TestGetEndDateFromTag_WithDefaultDate()
		{
			Assert.AreEqual(new DateTime(2022, 02, 23), UniversalDataHelper.GetEndDateFromTag(CreateNode("date", "23/02/2022"), "date", new DateTime(2030, 12, 31)));
			Assert.AreEqual(new DateTime(2030, 12, 31), UniversalDataHelper.GetEndDateFromTag(CreateNode("date", "02/23/2022"), "date", new DateTime(2030, 12, 31)), "Wrong format and fallback to default date.");
		}

		[Test]
		public void TestGetDateFromTag()
		{
			Assert.AreEqual(new DateTime(2022, 02, 23), UniversalDataHelper.GetDateFromTag(CreateNode("date", "23/02/2022"), "date", new DateTime(9999, 12, 31)));
			Assert.AreEqual(new DateTime(9999, 12, 31), UniversalDataHelper.GetDateFromTag(CreateNode("date", "02/23/2022"), "date", new DateTime(9999, 12, 31)));
		}

		XmlNode CreateNode(string tagName, string value)
		{
			var node = new XmlDocument();
			node.LoadXml($"<{tagName}>{value}</{tagName}>");
			return node;
		}
	}
}
