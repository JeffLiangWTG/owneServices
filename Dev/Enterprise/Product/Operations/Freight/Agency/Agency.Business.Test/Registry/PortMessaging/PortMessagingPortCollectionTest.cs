using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(PortMessagingPortCollection))]
	internal class PortMessagingPortCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PortMessagingPortCollection>
	{
		public void TestNewWithDefaultValues()
		{
			using (RawDataRegistry.Instance.SystemEnterpriseCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "UPS"))
			{
				var portMessagingPortCollection = PortMessagingPortCollection.NewWithDefaultValues(PortMessagingPortCountryList.EnabledPorts).OfType<PortMessagingPort>();

				AssertContainsExactElementsInAnyOrder(PortMessagingPortCountryList.EnabledPorts, portMessagingPortCollection.Select(x => x.Port));
				portMessagingPortCollection.ForEach(portMessagingPort => CombineAssertions(() =>
				{
					AssertEquals("UPS", portMessagingPort.SenderID);
					AssertEquals(false, portMessagingPort.Enabled);
				}));
			}
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		protected override PortMessagingPortCollection GetCollectionToTest()
		{
			return new PortMessagingPortCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PortMessagingPort();
		}

		#endregion
	}
}
