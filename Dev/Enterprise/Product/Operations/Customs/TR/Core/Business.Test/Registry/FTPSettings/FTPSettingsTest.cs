using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(FTPSettings))]
	sealed class FTPSettingsTest : RegistryBusinessObjectTemplateTestCase<FTPSettings>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override FTPSettings GetBusinessObjectToClone()
		{
			var result = new FTPSettings();
			result.FTPAddress = ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet;
			result.Port = 21;
			result.Inbox = "inbox";
			result.Outbox = "outbox";

			return result;
		}

		protected override FTPSettings GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion

		#region Default Values

		public void TestSetCustomDefaultValues()
		{
			var setting = new FTPSettings();

			CombineAssertions("Default Values", () =>
			{
				AssertEquals(ExportUnionFTPAddressList.Codes.FtpistanbulEbirlikNet, setting.FTPAddress);
				AssertEquals(21, setting.Port);
				AssertEquals("inbox", setting.Inbox);
				AssertEquals("outbox", setting.Outbox);
			});
		}

		#endregion
	}
}
