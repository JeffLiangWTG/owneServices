using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business
{
	public class TallyContainerCustomsLinkTest : TestCaseWithFactory
	{
		public void TestRefreshReceiptDateBindingOnSetOutturnLink()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			int valueChangedCount = 0;
			container.ReceiptDateInfo.ValueChanged += delegate(object sender, EventArgs e)
			{ valueChangedCount++; };
			((IOutturnLinkable)container).SetOutturnLink(null);
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);
			AssertEquals("ReceiptDateInfo.ValueChanged should have been hit twice due to refresh binding", 2, valueChangedCount);
		}

		[ExpectNoExceptions]
		public void TestNotifyShipmentsOnSetCustomsListener()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			PackUnpackShipment shipment1 = container.PackUnpackShipments.AddNew();
			PackUnpackShipment shipment2 = container.PackUnpackShipments.AddNew();

			mock.Setup(m => m.GetOutturnFor(shipment1)).Returns((IOutturn)null);
			mock.Setup(m => m.GetOutturnFor(shipment2)).Returns((IOutturn)null);
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);
			mock.VerifyAll();
		}

		public void TestSetCustomsListener()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			AssertEquals(mock.Object, container.link);
			((IOutturnLinkable)container).SetOutturnLink(null);
			AssertNull(container.link);
		}

		public void TestReceiptDate()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			mock.Setup(m => m.ReceiptDate).Returns(ZDateTime.BrettsBirthday);
			AssertEquals(ZDateTime.BrettsBirthday, container.ReceiptDate);
			mock.VerifyAll();

			mock.SetupProperty(m => m.ReceiptDate, ZDateTime.BrettsBirthday);
			container.ReceiptDate = ZDateTime.BrettsBirthday;
			mock.VerifyAll();

			((IOutturnLinkable)container).SetOutturnLink(null);
			AssertEquals(ZDateTime.Empty, container.ReceiptDate);
			mock.VerifyAll();
		}

		public void TestReceiptDateInfo()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			AssertNotNull("precondition", container.link);
			AssertNotNull(container.ReceiptDateInfo);
			AssertEquals(typeof(ZPropertyInfoDateTime), container.ReceiptDateInfo.GetType());
			AssertEquals(false, container.ReceiptDateInfo.ReadOnly);

			((IOutturnLinkable)container).SetOutturnLink(null);
			AssertEquals(true, container.ReceiptDateInfo.ReadOnly);
		}

		public void TestGetOutturnFor()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			PackUnpackShipment shipment = container.PackUnpackShipments.AddNew();
			var outturnMock = new Mock<IOutturn>();
			mock.Setup(m => m.GetOutturnFor(shipment)).Returns(outturnMock.Object);

			IOutturn outturnListener = ((IOutturnProvider)container).GetOutturnFor(shipment);
			mock.VerifyAll();

			AssertEquals(outturnMock.Object, outturnListener);

			((IOutturnLinkable)container).SetOutturnLink(null);
			AssertNull(((IOutturnProvider)container).GetOutturnFor(shipment));
			mock.VerifyAll();
		}

		public void TestSealNumber()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			AssertNotNull("precondition", container.link);
			mock.Setup(m => m.SetSealNumber(new ZString("foo")));
			container.JC_SealNum = "foo";
			mock.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestSealIntact()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);

			mock.Setup(m => m.SetSealIntact(ZBool.True));
			container.JC_IsSealOk = true;
			mock.VerifyAll();

			mock.Setup(m => m.SetSealIntact(ZBool.False));
			container.JC_IsSealOk = false;
			mock.VerifyAll();
		}

		[ExpectNoExceptions()]
		public void TestUpdateLink()
		{
			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);
			container.link = null;
			container.JC_SealNum = "foo";
			container.JC_IsSealOk = true;
			mock.Setup(m => m.SetSealNumber(new ZString("foo")));
			mock.Setup(m => m.SetSealIntact(ZBool.True));
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);
			mock.VerifyAll();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var container = Factory.New<TallyContainer>();
			var mock = new Mock<IOutturnLink>();
			((IOutturnLinkable)container).SetOutturnLink(mock.Object);
		}
		#endregion
	}
}
