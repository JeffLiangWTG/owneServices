using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.GUI.Testing
{
	[TestedType(typeof(PreAdviceExportToForwardingJobForm))]
	public class PreAdviceExportToForwardingJobFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var preAdvice = Factory.New<JobShipmentPreplanning>();
			return new PreAdviceExportToForwardingJobForm(preAdvice, JobShipmentPreplanning.OrderShipmentCreationMode.ShipmentPerBuyerSupplier, new JobShipmentPreplanningTest.TestNotificationSubscriber());
		}

		public void TestExportWithAIRConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "MASTER";

			Factory.Save();

			var preplanning1 = Factory.New<JobShipmentPreplanning>();
			preplanning1.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			preplanning1.EF_MasterBill = "MASTER";

			Factory.Save();

			using (var form = new PreAdviceExportToForwardingJobForm(preplanning1, JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, new NotificationBuffer()))
			{
				form.Show();
				SimulateToClickOKButton(form);
				AssertEquals("An air consol has been created for this Master Bill. Same Master Bill can be used only for one air consol. This form will now close.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestExportWithSeaConsol_ConsolAndShipmentsCheck()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			Factory.Save();

			var preplanning1 = Factory.New<JobShipmentPreplanning>();
			preplanning1.BuyerPK = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();

			var order1 = preplanning1.Orders.AddNew();
			order1.JD_OrderNumber = "ORDER1";
			order1.BuyerPK = buyer.PK;
			order1.SupplierPK = supplier1.PK;
			order1.JD_RL_NKGoodsAvailableAt = "AUSYD";
			order1.JD_RL_NKGoodsDeliveredTo = "USLAX";
			order1.JD_TransportMode = "SEA";
			order1.JD_IncoTerm = "FCA";
			order1.JD_RX_NKOrderCurrency = Factory.LoadTop1<RefCurrency>(new ZQuery()).RX_Code;
			order1.JD_RN_NKCountryOfSupply = "NZ";
			order1.JD_RS_NKServiceLevel_NI = "AAA";

			Factory.Save();

			using (var form = new PreAdviceExportToForwardingJobForm(preplanning1, JobShipmentPreplanning.OrderShipmentCreationMode.SingleShipment, new NotificationBuffer()))
			{
				form.Show();
				SimulateToClickOKButton(form);

				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(ConsolForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void SimulateToClickOKButton(ZChildForm form)
		{
			if (form != null)
			{
				var mi = form.GetType().GetMethod("OKButton_Click", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Static);
				AssertNotNull(mi);

				mi.Invoke(form, new object[] { null, null });
			}
		}
	}
}
