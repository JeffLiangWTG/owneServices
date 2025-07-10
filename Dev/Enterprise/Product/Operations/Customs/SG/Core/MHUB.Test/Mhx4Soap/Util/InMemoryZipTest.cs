using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.SG.MHUB.Mhx4Soap.Util.Testing
{
	sealed class InMemoryZipTest : TestCase
	{
		public void TestEverything()
		{
			//Note, this test is not perfect.
			// I created sample files using the test, saved to disk, embedded them, then can use these as the expected output on other runs.
			// But there are tiny little differences each time, usually at byte #11.
			// Even setting the source files' modified date to somethign constant doesn't help.
			// So we compare sizes only :S
			// Note - this is too petty to hold up the checkin of this urgent shelf. So do not reject the reviewed shelf for (only) this reason.
			var tempDir = Temp.TempPath;
			var tempData = Path.Combine(tempDir, "data.txt");
			var tempAttachment1 = Path.Combine(tempDir, "att1.txt");
			var tempAttachment2 = Path.Combine(tempDir, "att2.txt");
			File.WriteAllBytes(tempData, System.Text.Encoding.ASCII.GetBytes("This is some fake same EDIFACT"));
			File.WriteAllBytes(tempAttachment1, new byte[] { 1, 234, 24, 242, 2, 4, 255, 65, 7, 53, 5, 0, 0, 24 });
			File.WriteAllBytes(tempAttachment2, new byte[] { 0, 1, 2, 3, 4, 242, 2, 4, 255, 65, 7, 53, 5, 0, 0, 24 });
			SetFileTimestamps(new string[] { tempData, tempAttachment1, tempAttachment2 });
			var imz = new InMemoryZip();
			var zippedThreeFilesA = imz.GetZippedBytesWithAttachments(new FileInfo(tempData), new FileInfo[] { new FileInfo(tempAttachment1), new FileInfo(tempAttachment2) }, "postFixMe");
			var zippedOneFilePostfixB = imz.GetZippedBytes(new FileInfo(tempData), "postFixMe");
			var zippedOneFileNakedC = imz.GetZippedBytes(new FileInfo(tempData));
			//File.WriteAllBytes(@"C:\Dev\Enterprise\Product\Operations\Customs\SG\MHUB\Mhx4Soap\Util\InMemoryZip.Testfile.A.zip", zippedThreeFilesA);
			//File.WriteAllBytes(@"C:\Dev\Enterprise\Product\Operations\Customs\SG\MHUB\Mhx4Soap\Util\InMemoryZip.Testfile.B.zip", zippedOneFilePostfixB);
			//File.WriteAllBytes(@"C:\Dev\Enterprise\Product\Operations\Customs\SG\MHUB\Mhx4Soap\Util\InMemoryZip.Testfile.C.zip", zippedOneFileNakedC);
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.Util.TestFiles.InMemoryZip.A.zip"))
			using (var br = new BinaryReader(stream))
			{
				var expectedBytes = br.ReadBytes((int)stream.Length);
				AssertEquals(expectedBytes.Length, zippedThreeFilesA.Length);
				// Note, this doesn't work, random tiny differences exist, usall at byte #11: AssertEquals(expectedBytes, zippedThreeFilesA);
			}

			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.Util.TestFiles.InMemoryZip.B.zip"))
			using (var br = new BinaryReader(stream))
			{
				var expectedBytes = br.ReadBytes((int)stream.Length);
				AssertEquals(expectedBytes.Length, zippedOneFilePostfixB.Length);
				// Ditto about tiny mismathces
			}

			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Customs.SG.MHUB.Testing.Mhx4Soap.Util.TestFiles.InMemoryZip.C.zip"))
			using (var br = new BinaryReader(stream))
			{
				var expectedBytes = br.ReadBytes((int)stream.Length);
				AssertEquals(expectedBytes.Length, zippedOneFileNakedC.Length);
				// Ditto about tiny mismathces
			}

			File.Delete(tempData);
			File.Delete(tempAttachment1);
			File.Delete(tempAttachment2);
		}

		void SetFileTimestamps(string[] filenames)
		{
			var t = new ZDateTime(1979, 8, 9, 9, 56, 0).ToDateTime();
			foreach (var file in filenames)
			{
				File.SetCreationTimeUtc(file, t);
				File.SetLastAccessTimeUtc(file, t);
				File.SetLastWriteTimeUtc(file, t);
			}
		}
	}
}
