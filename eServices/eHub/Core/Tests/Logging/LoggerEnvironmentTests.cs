using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.eHub.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Core.Tests.Logging
{
	[TestClass]
	public class LoggerEnvironmentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void LoggerEnvironment_ValidateIsBizTalkHost()
		{
			var result = string.Empty;
			result = LoggerEnvironment.ValidateHostAndGetName(GetCommandLineArguments("\"C:\\Program Files (x86)\\Microsoft BizTalk Server 2013 R2\\BTSNTSvc.exe\" -group \"BizTalk Group\" -name \"BizTalkServerApplication\" -btsapp \"{F4DD2A7D-C38D-46BE-94B5-89ABA20C6BC6}\""));
			Assert.AreEqual("BizTalkServerApplication", result);
			result = LoggerEnvironment.ValidateHostAndGetName(GetCommandLineArguments("\"C:\\Program Files (x86)\\Microsoft BizTalk Server 2013 R2\\BTSNTSvc64.exe\" -group \"BizTalk Group\" -name \"TrackingHost\" -btsapp \"{DBAD6598-7C82-47EF-9D9E-52278A41F658}\""));
			Assert.AreEqual("TrackingHost", result);
			result = LoggerEnvironment.ValidateHostAndGetName(GetCommandLineArguments("C:\\Windows\\SysWOW64\\inetsrv\\w3wp.exe -ap \"BizTalk HTTP - AS2\" -v \"v4.0\" -l \"webengine4.dll\" -a \\\\.\\pipe\\iisipm68fb1948-ea71-4715-a147-30c1a2347553 -h \"C:\\inetpub\\temp\\apppools\\BizTalk HTTP - AS2\\BizTalk HTTP - AS2.config\" -w \"\" -m 0 -t 20 -ta 0"));
			Assert.AreEqual("BizTalkHTTPAS2", result);
			AssertException<InvalidOperationException>(() =>
				LoggerEnvironment.ValidateHostAndGetName(GetCommandLineArguments("\"C:\\Program Files (x86)\\Microsoft BizTalk Server 2013 R2\\BTSNTSvc64.exe\" \"BizTalk Group\" \"TrackingHost\" \"{DBAD6598-7C82-47EF-9D9E-52278A41F658}\"")));
			AssertException<InvalidOperationException>(() =>
				LoggerEnvironment.ValidateHostAndGetName(GetCommandLineArguments("\"C:\\Program Files (x86)\\Microsoft BizTalk Server 2013 R2\\BTSNTSvc64.RENAMED.exe\" -group \"BizTalk Group\" -name \"TrackingHost\" -btsapp \"{DBAD6598-7C82-47EF-9D9E-52278A41F658}\"")));
		}

		string[] GetCommandLineArguments(string commandLine)
		{
			return Regex
				.Matches(commandLine, @"(\""(?<arg>.+?)\"")|(?<arg>\S+)",
					RegexOptions.CultureInvariant | RegexOptions.Singleline).Cast<Match>()
				.Select(m => m.Groups["arg"].Value).ToArray();
		}

		void AssertException<T>(Action action) where T : Exception
		{
			try
			{
				action();
				Assert.Fail("Expected exception not thrown");
			}
			catch (Exception e)
			{
				Assert.IsInstanceOfType(e, typeof(T));
			}
		}
	}
}
