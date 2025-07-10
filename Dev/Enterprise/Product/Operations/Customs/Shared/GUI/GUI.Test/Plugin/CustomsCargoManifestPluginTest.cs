using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GUI.PlugIn.Testing
{
	sealed class CustomsCargoManifestPluginTest : TestCaseWithFactory
	{
		public void TestLoadZPlugIn()
		{
			using (TestCustomsCargoManifestPlugin testPlugIn = new TestCustomsCargoManifestPlugin(Factory.New<ForwardingConsol>()))
			{
				AssertNotNull(testPlugIn.UserControl);
				Menu brokerageMenu = testPlugIn.TopLevelMenu;
				AssertNotNull(brokerageMenu);
			}
		}

		public void TestDontCreateMainMenuWhenNoMenuItems()
		{
			CommonShipment shipment = Factory.New<ForwardingShipment>();
			using (TestCustomsCargoManifestPluginWithNoMenu plugIn = new TestCustomsCargoManifestPluginWithNoMenu(shipment))
			{
				AssertEquals("Should be no top level menu if there are no menu items added", null, plugIn.TopLevelMenu);
			}
		}

		class TestCustomsCargoManifestPlugin : CustomsCargoManifestPlugin
		{
			public TestCustomsCargoManifestPlugin(IManifestProvider manifestProvider) : base(manifestProvider)
			{
			}

			protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.ImportManifest;

			protected override CustomsManifestStatus GetCustomsManifestStatus(IManifestProvider manifestProvider) => new MockCustomsManifestStatus(manifestProvider);

			protected override void ChangeTheVisibilityCore()
			{
			}

			protected override string NameCore => "";

			sealed class MockCustomsManifestStatus : CustomsManifestStatus
			{
				public MockCustomsManifestStatus(IManifestProvider manifestProvider) : base(manifestProvider)
				{
				}

				public override ZString E2_CustomsEntryNumber => "";

				public override ZString E2_CustomsEntryNumberHumanReadableName => "Test Number";

				protected override string MessageApplicationCode => "";

				protected override ManifestStatus GetMessageStatus(EDIMessage message) => ManifestStatus.NotSent;

				public override IManifestMessageBuilder NewCreateOrReplaceMessageBuilder() => null;

				protected override IManifestMessageBuilder[] NewMessageBuilders(MessageSubTypes messageType) => Array.Empty<IManifestMessageBuilder>();
			}
		}

		sealed class TestCustomsCargoManifestPluginWithNoMenu : TestCustomsCargoManifestPlugin
		{
			public TestCustomsCargoManifestPluginWithNoMenu(IManifestProvider manifestProvider) : base(manifestProvider)
			{
			}

			protected override void SetupTopLevelMenu()
			{
			}
		}
	}
}
