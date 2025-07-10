using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(LiquidationGroupNotification))]
	sealed class LiquidationGroupNotificationTest : RegistryBusinessObjectTemplateTestCase<LiquidationGroupNotification>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override LiquidationGroupNotification GetBusinessObjectToClone()
		{
			return new LiquidationGroupNotification();
		}

		protected override LiquidationGroupNotification GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
