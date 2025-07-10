using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Tests
{
	[TestFixture]
	class DownloadROSErrorListTest
	{
		[Test]
		public void EmptyUrl()
		{
			var downloader = new DownloadROSErrorList();
			var (errors, _) = DownloadROSErrorList.Download("");

			Assert.That(errors, Does.Contain("Unable to Download the IE ROS Error Codes List File from the following URL"));
			Assert.That(errors, Does.Contain("The URI is empty."));
		}

		[Test]
		public void InvalidUrl()
		{
			var downloader = new DownloadROSErrorList();
			var (errors, _) = DownloadROSErrorList.Download("InvalidURL");

			Assert.That(errors, Does.Contain("Unable to Download the IE ROS Error Codes List File from the following URL: InvalidURL"));
		}

		[Test]
		public void MultipleLineDescriptions()
		{
			var downloader = new DownloadROSErrorList();
			var (_, extractedErrorList) = DownloadROSErrorList.Download(errorListPath);

			Assert.Multiple(() =>
			{
				Assert.That(extractedErrorList, Has.Count.EqualTo(2282), "Correct Number of Codes");
				Assert.That(extractedErrorList.Single(x => x.Key == "ECLR019852").Value, Is.EqualTo(@"When CD CAP export is 'true'  and Declaration - 1st subdivision is ""CO"", then SAD COM control number must be completed as follows: -   Box 40a must be ""Z""   -  Box 40b must be ""CO""   - Box 40c must comprise exactly 12 numeric digits (N12): e.g. ""Z-CO-000000000000"" or ""Z-CO-123456789012"""), "Multi Line description is correct");
			});
		}

		[OneTimeSetUp]
		public void Setup()
		{
			assembly = Assembly.GetExecutingAssembly();
			errorListPath = Path.Combine(Path.GetDirectoryName(assembly.Location), @"ROSErrors\TestFiles\Input\error-spec1.txt");
		}
		Assembly assembly;
		string errorListPath;
	}
}
