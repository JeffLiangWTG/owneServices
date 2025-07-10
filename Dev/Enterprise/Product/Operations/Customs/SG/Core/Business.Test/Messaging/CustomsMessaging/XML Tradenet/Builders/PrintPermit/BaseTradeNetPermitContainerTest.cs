using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.Testing
{
	sealed class BaseTradeNetPermitContainerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContainerSequenceNumber1()
		{
			AssertEquals("ContainerSequenceNumber1", "   01", Container.ContainerSequenceNumber1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContainerSequenceNumber2()
		{
			AssertEquals("ContainerSequenceNumber2", "", Container.ContainerSequenceNumber2);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContainerIdentifier1()
		{
			AssertEquals("ContainerIdentifier1", "BFV5462       LCL 20 001 NA", Container.ContainerIdentifier1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContainerIdentifier2()
		{
			AssertEquals("ContainerIdentifier2", "", Container.ContainerIdentifier2);
		}

		BaseTradeNetPermitContainer Container
		{
			get
			{
				if (container == null)
				{
					var message = Factory.New<SGXmlEDIMessage>();
					message.EM_MessageText = System.IO.File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\XML\TNPPMT.XML");
					Factory.Save();
					container = new BaseTradeNetPermitContainer(message.TradenetResponse.OutboundMessage.TranshipmentMovementPermit.Declaration.Cargo.TransportEquipment.FirstOrDefault());
				}

				return container;
			}
		}

		BaseTradeNetPermitContainer container;
	}
}
