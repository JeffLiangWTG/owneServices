using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.VersionInfo.Testing
{
	sealed class PackageDownloadInfoTest : TestCase
	{
		public void TestConstructorWithValidXmlData()
		{
			string xmlData =
				"<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine +
				"<UpgradeDownload>" + System.Environment.NewLine +
				"  <ExeVersionDate>31-DEC-04 13:56</ExeVersionDate>" + System.Environment.NewLine +
				"  <MajorVersion>1</MajorVersion>" + System.Environment.NewLine +
				"  <MinorVersion>2</MinorVersion>" + System.Environment.NewLine +
				"  <Release>3</Release>" + System.Environment.NewLine +
				"  <Patch>4</Patch>" + System.Environment.NewLine +
				"  <Comment>1.2.3.4</Comment>" + System.Environment.NewLine +
				"  <ForceDownload>Y</ForceDownload>" + System.Environment.NewLine +
				"  <PackageURL>http://www.cargowise.com/ftpmirror/Banner.jpg</PackageURL>" + System.Environment.NewLine +
				"</UpgradeDownload>";

			var info = new PackageDownloadInfo(xmlData);
			AssertNotNull("VersionInfo", info);
			Assert("Is Valid", info.VersionInfo.IsValid);
			AssertEquals("ExeVersionDate", new ZDateTime(2004, 12, 31, 13, 56, 0), info.ExeVersionDate);
			AssertEquals("MajorVersion", 1, info.MajorVersion);
			AssertEquals("MinorVersion", 2, info.MinorVersion);
			AssertEquals("Release", 3, info.Release);
			AssertEquals("Patch", 4, info.Patch);
			AssertEquals("Comment", "1.2.3.4", info.Comment);
			AssertEquals("Package File Name", "Package20041231_135600_1_2_3_4.edp", info.VersionInfo.PackageFileName);
			AssertEquals("Frorcedownload", ZBool.True, info.ForceDownload);
			AssertEquals("PackageURL", "http://www.cargowise.com/ftpmirror/Banner.jpg", info.PackageURL);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructorWithInvalidXmlData()
		{
			var info = new PackageDownloadInfo("!!!!!!");
		}

		public void TestExceptionMessage()
		{
			try
			{
				var info = new PackageDownloadInfo("Invalid Data!");
			}
			catch (ArgumentException ex)
			{
				AssertEquals("Exception Message", "XmlData is not valid!\n\nData at the root level is invalid. Line 1, position 1.\n\nXmlData:\n\nInvalid Data!", ex.Message);
			}
		}
	}
}
