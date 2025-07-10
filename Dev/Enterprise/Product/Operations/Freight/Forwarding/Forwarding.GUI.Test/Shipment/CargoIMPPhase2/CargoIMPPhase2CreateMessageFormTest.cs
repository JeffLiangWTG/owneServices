using System;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(CargoIMPPhase2CreateMessageForm))]
	public class CargoIMPPhase2CreateMessageFormTest : ZFormBasherTest
	{
		public void TestOutputTextBoxIsReadOnly()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (CargoIMPPhase2CreateMessageFormForTest form = new CargoIMPPhase2CreateMessageFormForTest(CargoIMPPhase2MessageManager.New(shipment), CargoIMPPhase2MessageTypes.RouteMapInformation))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("OutputTextbox.ReadOnly", true, form.OutputTextbox.ReadOnly);
			}
		}

		public void TestINotificationSubscriber_Notify()
		{
			ForwardingShipment consol = Factory.NewWithValidTestData<ForwardingShipment>();
			using (CargoIMPPhase2CreateMessageFormForTest form = new CargoIMPPhase2CreateMessageFormForTest(CargoIMPPhase2MessageManager.New(consol), CargoIMPPhase2MessageTypes.RouteMapInformation))
			{
				form.Show();
				Application.DoEvents();
				form.Notify(new ErrorNotification(ErrorType.Error, "MyError"));
				form.Notify(new InfoNotification(null));
				form.Notify(new InfoNotification("MyInfo"));
				form.Notify(new NewlineNotification());
				AssertEquals("Error: MyError\r\nMyInfo\r\n\r\n", form.OutputTextbox.Text);
			}
		}

		public void TestCreateMessage()
		{
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2TraxonSenderPIMAAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PIMA");
			ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_HouseBill = "HOUSE BILL";
			shipment.JS_OuterPacks = 10;
			Factory.Save();

			using (CargoIMPPhase2CreateMessageFormForTest form = new CargoIMPPhase2CreateMessageFormForTest(CargoIMPPhase2MessageManager.New(shipment), CargoIMPPhase2MessageTypes.RouteMapInformation))
			{
				form.Show();

				form.CreateMessage();
				Assert("Should not Create when not all is OK. output text box will say unsuccessful", form.OutputTextbox.Text.IndexOf("unsuccessful") != -1);

				shipment.JS_ActualWeight = 1300.345M;
				form.CreateMessage();
				Assert("Should Create succesfully when all is OK. output text box will say complete", form.OutputTextbox.Text.IndexOf("complete") != -1);
			}
		}

		protected override bool AllowFormSizeFixed => true;

		protected override Form GetFormToBashCore()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			return new CargoIMPPhase2CreateMessageForm(CargoIMPPhase2MessageManager.New(shipment), CargoIMPPhase2MessageTypes.RouteMapInformation);
		}

		class CargoIMPPhase2CreateMessageFormForTest : CargoIMPPhase2CreateMessageForm
		{
			public CargoIMPPhase2CreateMessageFormForTest(CargoIMPPhase2MessageManager messageManager, CargoIMPPhase2MessageTypes messageType)
				: base(messageManager, messageType)
			{
			}

			public new ZTextBox OutputTextbox
			{
				get { return base.OutputTextbox; }
			}

			protected override void QueryUserCore(IQueryUserEventArgs e)
			{
				if (e is QueryUserMsgBoxEventArgs)
				{
					((QueryUserMsgBoxEventArgs)e).Response = false;
				}
			}

			public new void CreateMessage()
			{
				base.CreateMessage();
			}
		}
	}
}
