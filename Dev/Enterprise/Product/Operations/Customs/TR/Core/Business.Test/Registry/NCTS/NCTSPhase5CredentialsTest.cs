using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(NCTSPhase5Credentials))]
	sealed class NCTSPhase5CredentialsSettingsTest : RegistryBusinessObjectTemplateTestCase<NCTSPhase5Credentials>
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

		protected override NCTSPhase5Credentials GetBusinessObjectToClone()
		{
			var result = new NCTSPhase5Credentials();
			result.BasicAuthUsername = "NCTSTraderUser";
			result.BasicAuthPassword = "3vAT.2G98Ar!";
			result.FirmID = "WiseTech";
			result.RequestUserID = "11111111108";
			result.RequestPassword = "12345678";

			return result;
		}

		protected override NCTSPhase5Credentials GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		#endregion

		#region Default Values

		public void TestSetCustomDefaultValues()
		{
			var setting = new NCTSPhase5Credentials();

			CombineAssertions("Default Values", () =>
			{
				AssertEquals("BasicAuthUsername", "NCTSTraderUser", setting.BasicAuthUsername);
				AssertEquals("BasicAuthPassword", "3vAT.2G98Ar!", setting.BasicAuthPassword);
				AssertEquals("FirmID", "WiseTech", setting.FirmID);
				AssertEquals("RequestUserID", ZString.Empty, setting.RequestUserID);
				AssertEquals("RequestPassword", ZString.Empty, setting.RequestPassword);
			});
		}

		#endregion
	}
}
