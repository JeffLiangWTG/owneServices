using CargoWise.eHub.BizTalkAdapters.WinScp.Common;
using NUnit.Framework;

namespace CargoWise.eHub.BizTalkAdapters.Tests.WinScp.Common
{
	public class WinScpLocationTests
	{
		[Test, Property("DAT:CapabilityRequirements", "BIZTALK2020")]
		[TestCase("ftp://servername", "ftp://servername", "ftp", null, null, "servername", null, null, null)]
		[TestCase("ftp://servername:99", "ftp://servername:99", "ftp", null, null, "servername", 99, null, null)]
		[TestCase("ftp://servername\\folder\\", "ftp://servername/folder", "ftp", null, null, "servername", null, "folder", null)]
		[TestCase("ftp://servername/.*", "ftp://servername/.*", "ftp", null, null, "servername", null, null, ".*")]
		[TestCase("ftp://servername:99/folder/*", "ftp://servername:99/folder/*", "ftp", null, null, "servername", 99, "folder", "*")]
		[TestCase("sftp://username@servername:99/folder/file*.*", "sftp://username@servername:99/folder/file*.*", "sftp", "username", null, "servername", 99, "folder", "file*.*")]
		[TestCase("sftp://username:password@servername:99/folder/file*.*", "sftp://username:password@servername:99/folder/file*.*", "sftp", "username", "password", "servername", 99, "folder", "file*.*")]
		[TestCase("sftpwinscp://abc%40site:pass%60word@servername", "sftpwinscp://abc%40site:pass%60word@servername", "sftpwinscp", "abc@site", "pass`word", "servername", null, null, null)]
		[TestCase("abc%40site:pass%60word@servername:8080", "abc%40site:pass%60word@servername:8080", null, "abc@site", "pass`word", "servername", 8080, null, null)]
		[TestCase("sftpwinscp://username:password@servername:99/folder1/folder2/???.??????.????????????", "sftpwinscp://username:password@servername:99/folder1/folder2/???.??????.????????????", "sftpwinscp", "username", "password", "servername", 99, "folder1/folder2", "???.??????.????????????")]
		public void WinScpLocation_ParseUri(string uri, string parsedUri, string scheme, string username, string password, string server, int? port, string folder, string filemask)
		{
			var result = new WinScpLocation(uri);
			Assert.That(result.SourceUri, Is.EqualTo(uri));
			Assert.That(result.GetUri(true), Is.EqualTo(parsedUri));
			Assert.That(result.Scheme, Is.EqualTo(scheme));
			Assert.That(result.UserName, Is.EqualTo(username));
			Assert.That(result.Password, Is.EqualTo(password));
			Assert.That(result.Server, Is.EqualTo(server));
			Assert.That(result.Port, Is.EqualTo(port));
			Assert.That(result.Folder, Is.EqualTo(folder));
			Assert.That(result.FileName, Is.EqualTo(filemask));
		}
	}
}
