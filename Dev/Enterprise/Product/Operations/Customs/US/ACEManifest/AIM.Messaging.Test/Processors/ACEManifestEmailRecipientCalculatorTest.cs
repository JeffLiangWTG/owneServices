using System.Reflection;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public sealed class ACEManifestEmailRecipientCalculatorTest : TestCase
	{
		public void TestIsNotSystemCommunication()
		{
			AssertEquals("Should not be system communication email.", false, (bool)ReflectionGetNonPublicPropertyValue("IsSystemCommunication"));
		}

		public void TestIsCorrectMailManager()
		{
			AssertEquals("Should be Env.OutgoingMailManager", Env.OutgoingMailManager, ReflectionGetNonPublicPropertyValue("OutgoingMailManager"));
		}

		ACEManifestEmailRecipientCalculator calculator;

		object ReflectionGetNonPublicPropertyValue(string propertyName)
		{
			var propertyInfo = typeof(ACEManifestEmailRecipientCalculator).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);
			return propertyInfo.GetValue(calculator);
		}
		protected override void SetUp()
		{
			calculator = new ACEManifestEmailRecipientCalculator("DMY", ZGuid.Empty, null, ZGuid.Empty);
		}
	}
}
