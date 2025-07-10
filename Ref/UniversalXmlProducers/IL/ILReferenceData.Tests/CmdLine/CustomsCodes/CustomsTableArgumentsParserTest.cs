using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.CmdLine;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine
{
	[TestFixture]
	sealed class CustomsTableArgumentsParserTest
	{
		[Test]
		public void TryParse_ValidArguments_ReturnsTrueAndSetsTableName()
		{
			bool result = CustomsTableArgumentsParser.TryParse(new string[] { "arg0", "myTable" }, out ISYSTBL_NG_9000_MSG_SystemTableRequest[] systemTableRequests1, out string message1);
			AssertArgumentsParsing(result, systemTableRequests1, message1, new string[] { "myTable" });

			result = CustomsTableArgumentsParser.TryParse(new string[] { "arg0", "myTable1,myTable2" }, out ISYSTBL_NG_9000_MSG_SystemTableRequest[] systemTableRequests2, out string message2);
			AssertArgumentsParsing(result, systemTableRequests2, message2, new string[] { "myTable1", "myTable2" });

			result = CustomsTableArgumentsParser.TryParse(new string[] { "arg0", "myTable1 , myTable2" }, out ISYSTBL_NG_9000_MSG_SystemTableRequest[] systemTableRequests3, out string message3);
			AssertArgumentsParsing(result, systemTableRequests3, message3, new string[] { "myTable1", "myTable2" });
		}

		[Test]
		public void TryParse_InvalidArguments_ReturnsFalseAndNull()
		{
			bool result = CustomsTableArgumentsParser.TryParse(new string[] { "arg0", "" }, out ISYSTBL_NG_9000_MSG_SystemTableRequest[] systemTableRequests, out string message);

			Assert.IsFalse(result);
			Assert.IsNull(systemTableRequests);
			Assert.AreEqual("Bad Command Line Arguments", message);
		}

		[SetUp]
		public void SetUp()
		{
			var mock = new Mock<IDateTimeProvider>();
			mock.Setup(i => i.Now).Returns(new DateTime(2024, 07, 16));
			DateTimeUtil.DateTimeProvider = mock.Object;
		}

		[TearDown]
		public void TearDown()
		{
			DateTimeUtil.DateTimeProvider = null;
		}

		static void AssertArgumentsParsing(bool result, ISYSTBL_NG_9000_MSG_SystemTableRequest[] systemTableRequests, string message, string[] expectedTableNames)
		{
			Assert.IsTrue(result);
			Assert.IsNotNull(systemTableRequests);
			Assert.AreEqual(expectedTableNames.Length, systemTableRequests.Length);

			for (int i = 0; i < expectedTableNames.Length; i++)
			{
				var systemTableRequest = systemTableRequests[i];
				Assert.AreEqual(expectedTableNames[i], systemTableRequest.TableName);
				var requestContentHeader = systemTableRequest.RequestContentHeader;
				Assert.IsNotNull(requestContentHeader);
				Assert.AreEqual(new DateTime(2024, 07, 16), requestContentHeader.TransmitionDateTime);
				Assert.IsNotNull(systemTableRequest.SelectOptions);
				Assert.That(systemTableRequest.SelectOptions.GetAsDataTable, Is.True);
				Assert.That(systemTableRequest.SelectOptions.PageNumber, Is.EqualTo(1));
				Assert.That(systemTableRequest.SelectOptions.PageSize, Is.EqualTo(999));
			}

			Assert.IsNull(message);
		}
	}
}
