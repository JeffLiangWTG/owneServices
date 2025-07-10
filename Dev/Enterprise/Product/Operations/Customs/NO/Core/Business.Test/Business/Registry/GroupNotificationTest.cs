using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Registry.Testing;

[TestedType(typeof(GroupNotification))]
sealed class GroupNotificationTest : RegistryBusinessObjectTemplateTestCase<GroupNotification>
{
	public void TestValidateSendMode()
	{
		AssertEquals("NOE, ESM, ENG, ESG", BizObj.SendModeList.CodesAsString);
	}

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override GroupNotification GetBusinessObjectToClone() => new();

	protected override GroupNotification GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
}
