using System;
using System.IO;
using CargoWise.eServices.Billing.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WiseCloudRefFileValidator.Tests
{
	[TestFixture]
	public class TestValidator
	{
		[Test]
		public void TestCsvValidation()
		{
			var file = TestHelper.PathToFileCreatedFromEmbeddedResource(typeof(TestValidator).Assembly, "Sample.csv");
			try
			{
				using (var sw = new StringWriter())
				{
					Console.SetOut(sw);
					Program.Main(new[] { file });
					Assert.That(sw.ToString(), Is.EqualTo(@"ERROR: Reference file has incorrect customer code or IP format: ""3.3.3.3"",""Customer-cccCCC-01""
ERROR: Reference file has incorrect customer code or IP format: 2.2.3.3.,Customer-eeeEEE-01
ERROR: Reference file has incorrect customer code or IP format: 11.11.11.11,Customer-lllLLLlll-01
ERROR: Reference file has incorrect customer code or IP format: 12.12.12.12,CUSTOMER-mmmMMM-01
ERROR: Reference file has incorrect customer code or IP format: 13.13.13.13,Customer-nnnNNN-1
ERROR: Please replace subnet in 50.50.50.50/60 with IP range. Customer code: Customer-qqqQQQ-01
INFO: Ambiguous records found: Customers bbbBBB, dddDDD share same IP 2.2.2.2
ERROR: Reference file has invalid lines. Check log for which lines are invalid. System.FormatException: One of the identified items was in an invalid format.
WARNING: Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'
"));
				}
			}
			finally
			{
				File.Delete(file);
			}
		}
	}
}
