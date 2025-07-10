using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Moq;

namespace Enterprise.Freight.CFS.Business
{
	public class CustomsLinkingTest : TestCaseWithFactory
	{
		public void TestWhenLinkedAndDataGetsChangedOnLoadList()
		{
			var outturnLinkMock = new Mock<IOutturnLink>();
			var outturnMock = new Mock<IOutturn>();

			ZGuid containerGuid = GetNewTallyJobPK();
			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();

				CFSContainer container = factory2.Load<CFSContainer>(containerGuid);
				CFSLoadListConsol consol = container.Consol;
				AssertNotNull(consol);
				AssertEquals(1, consol.Shipments.Count);
				CFSShipment shipment = consol.Shipments[0];
				AssertEquals(1, container.PackUnpackShipments.Count);
				AssertEquals(shipment, container.PackUnpackShipments[0]);

				AssertEquals(1, container.PackLines.Count);
				CFSPackLine line = container.PackLines[0];
				AssertEquals(shipment, line.Shipment);

				container.JC_SealNum = "eckyeckyecky";
				container.JC_IsSealOk = true;

				line.JL_Outturn = 48;
				line.JL_Damaged = 1;
				line.JL_Pillaged = 1;
				line.JL_F3_NKPackType = "FOO";

				outturnMock.VerifyAll();

				factory2.Save();
			}

			{
				BusinessObjectFactory factory2 = new BusinessObjectFactory();

				TallyContainer container = factory2.Load<TallyContainer>(containerGuid);
				AssertEquals(1, container.PackLines.Count);
				PackLine line = container.PackLines[0];

				CommonShipment shipment = line.Shipment;

				outturnLinkMock.Setup(m => m.SetSealNumber(new ZString("eckyeckyecky")));
				outturnLinkMock.Setup(m => m.SetSealIntact(ZBool.True));
				outturnMock.Setup(m => m.SetPackagesOutturned((ZInt)48));
				outturnMock.Setup(m => m.SetPillaged(ZBool.True));
				outturnMock.Setup(m => m.SetDamaged(ZBool.True));
				outturnMock.Setup(m => m.SetPackageType(new ZString("FOO")));
				outturnLinkMock.Setup(m => m.GetOutturnFor((PackUnpackShipment)shipment)).Returns(outturnMock.Object);
				((IOutturnLinkable)container).SetOutturnLink(outturnLinkMock.Object);
			}
		}

		ZGuid GetNewTallyJobPK()
		{
			var outturnLinkMock = new Mock<IOutturnLink>();
			var outturnMock = new Mock<IOutturn>();

			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			TallyContainer container = Factory.New<TallyContainer>();
			consol.Containers.Add(container);

			PackUnpackShipment shipment = container.PackUnpackShipments.AddNew();
			container.JC_ContainerNum = "AAAA1111113";
			shipment.JS_HouseBill = "HOUSE999";
			consol.JK_MasterBillNum = "OBL99944";

			AssertEquals(1, container.PackLines.Count);
			TallyPackLine line = container.PackLines[0];
			AssertEquals(shipment, line.Shipment);

			outturnMock.Setup(m => m.IsDeleted).Returns(ZBool.False);

			outturnLinkMock.Setup(m => m.GetOutturnFor(shipment)).Returns(outturnMock.Object);

			outturnLinkMock.Setup(m => m.SetSealNumber(ZString.Empty));
			outturnLinkMock.Setup(m => m.SetSealIntact(ZBool.True));
			outturnMock.Setup(m => m.SetPackagesOutturned((ZInt)0));
			outturnMock.Setup(m => m.SetPillaged(ZBool.False));
			outturnMock.Setup(m => m.SetDamaged(ZBool.False));
			outturnMock.Setup(m => m.SetPackageType(new ZString("PKG")));
			((IOutturnLinkable)container).SetOutturnLink(outturnLinkMock.Object);

			line.JL_PackageCount = 50;

			outturnMock.Setup(m => m.SetPackagesOutturned((ZInt)10));
			outturnMock.Setup(m => m.SetPillaged(ZBool.False));
			outturnMock.Setup(m => m.SetDamaged(ZBool.False));
			outturnMock.Setup(m => m.SetPackageType(new ZString("PKG")));
			line.JL_Outturn = 10;

			Factory.Save();

			outturnLinkMock.VerifyAll();
			outturnMock.VerifyAll();

			return container.PK;
		}
	}
}
