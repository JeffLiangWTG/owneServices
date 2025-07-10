using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.VersionInfo.Testing
{
	sealed class PackageVersionInfoTest : TestCase
	{
		public void TestVersionInfoWithPackageName()
		{
			var info = new PackageVersionInfo("paCKage20041231_135611_1_2_3_4.edp");

			ZDateTime newDate = new ZDateTime(2004, 12, 31, 13, 56, 00);

			AssertNotNull("VersionInfo", info);
			Assert("Is Valid", info.IsValid);
			AssertEquals("ExeVersionDate", newDate, info.ExeVersionDate);
			AssertEquals("MajorVersion", 1, info.MajorVersion);
			AssertEquals("MinorVersion", 2, info.MinorVersion);
			AssertEquals("Release", 3, info.Release);
			AssertEquals("Patch", 4, info.Patch);
			AssertEquals("Package File Name", "Package20041231_135600_1_2_3_4.edp", info.PackageFileName);

			var info1 = new PackageVersionInfo("paCKage20041231_135611_1_2_3_4.edpbak");
			AssertNotNull("VersionInfo", info1);
			AssertEquals("Invalid", false, info1.IsValid);
			// No properties should be assigned because file name does not match regular expression
			AssertEquals("ExeVersionDate should be not assigned", ZDateTime.Empty, info1.ExeVersionDate);
			AssertEquals("MajorVersion should be not assigned", 0, info1.MajorVersion);
			AssertEquals("MinorVersion should be not assigned", 0, info1.MinorVersion);
			AssertEquals("Release should be not assigned", 0, info1.Release);
			AssertEquals("Patch should be not assigned", 0, info1.Patch);
			AssertEquals("Package File Name is Invalid", "<Invalid>", info1.PackageFileName);

			var info2 = new PackageVersionInfo("paCKage20041231_135611_1_2_3_4A.edp");
			AssertNotNull("VersionInfo", info2);
			AssertEquals("Invalid", false, info2.IsValid);
			// No properties should be assigned because file name does not match regular expression
			AssertEquals("ExeVersionDate should be not assigned", ZDateTime.Empty, info2.ExeVersionDate);
			AssertEquals("MajorVersion should be not assigned", 0, info2.MajorVersion);
			AssertEquals("MinorVersion should be not assigned", 0, info2.MinorVersion);
			AssertEquals("Release should be not assigned", 0, info2.Release);
			AssertEquals("Patch should be not assigned", 0, info2.Patch);
			AssertEquals("Package File Name is Invalid", "<Invalid>", info2.PackageFileName);

			var info3 = new PackageVersionInfo("paCKage20041231_335611_1_2_3_4.edp");
			AssertNotNull("VersionInfo", info3);
			AssertEquals("Invalid", false, info3.IsValid);
			// ExeVersion data should be Invalid because Time part is incorrect
			AssertEquals("ExeVersionDate should be Invalid", ZDateTime.Invalid, info3.ExeVersionDate);
			AssertEquals("MajorVersion", 1, info3.MajorVersion);
			AssertEquals("MinorVersion", 2, info3.MinorVersion);
			AssertEquals("Release", 3, info3.Release);
			AssertEquals("Patch", 4, info3.Patch);
			AssertEquals("Package File Name is Invalid", "<Invalid>", info3.PackageFileName);
		}

		public void TestVersionInfoWithIndividualParameters()
		{
			ZDateTime newDate = new ZDateTime(2004, 12, 31, 13, 56, 11);

			var info = new PackageVersionInfo(newDate, 1, 2, 3, 4);

			AssertNotNull("VersionInfo", info);
			Assert("Is Valid", info.IsValid);
			AssertEquals("ExeVersionDate", newDate, info.ExeVersionDate);
			AssertEquals("MajorVersion", 1, info.MajorVersion);
			AssertEquals("MinorVersion", 2, info.MinorVersion);
			AssertEquals("Release", 3, info.Release);
			AssertEquals("Patch", 4, info.Patch);
			AssertEquals("Package File Name", "Package20041231_135600_1_2_3_4.edp", info.PackageFileName);
		}
	}
}
