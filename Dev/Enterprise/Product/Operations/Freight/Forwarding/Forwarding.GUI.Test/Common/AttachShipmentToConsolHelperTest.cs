using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing.Common
{
	public class AttachShipmentToConsolHelperTest : TestCaseWithFactory
	{
		public void TestCheckAttaching_EstimatedDeliveryDateBeforeConsolETAShouldShowPopupMessage()
		{
			(var consol, var shipment) = SetUpDashboard();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			var shipments = new List<CargoWise.EntityFramework.BusinessObject>() { shipment };
			AttachShipmentToConsolHelper.CheckAttaching(shipments, consol);

			var question = @"None There's inconsistency between ETD/ETA of Shipments and Consols you are trying to link.
See details below:
Shipment shipment1 has estimated arrival date before Consol consol1 ETA.

How would you like to proceed?

Press [Yes] to attach and update the Shipments dates to match the Consols dates.
Press [No] to attach Shipments but do not update Shipments dates.
Press [Cancel] to cancel operation.";
			AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestCheckAttaching_EstimatedDeliveryDateAndShipmentETABeforeConsolETAShouldShowPopupMessageWithYesNo()
		{
			(var consol, var shipment) = SetUpDashboard();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var shipments = new List<CargoWise.EntityFramework.BusinessObject>() { shipment };
			AttachShipmentToConsolHelper.CheckAttaching(shipments, consol);

			var question = @"None The Estimated Delivery Date of below shipment(s) you are trying to link is before Consol's ETA.
Shipment shipment1

If [No] is selected, shipment will not be attached as the Estimated Delivery Date will not be met.
If [Yes] is selected, shipment will be attached with a warning on the Estimated Delivery Date field.";
			AssertMultilineASCIIEquals(question, UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		(ForwardingConsol consol, ForwardingShipment shipment) SetUpDashboard()
		{
			var today = ZDateTime.Today;

			var consol = CreateConsol("consol1");
			consol.Transports.MostInterestingTransport.JW_ETA = today;

			var shipment = CreateShipment("shipment1");
			shipment.DocsAndCartage.JP_EstimatedDelivery = today.AddDays(-5);
			shipment.JS_E_ARV = consol.JK_JX_JB_E_ARV.AddDays(-1);

			var dashboard = new ConsolDashboard(Factory);
			dashboard.Consols.AddRange(new[] { consol });

			return (consol, shipment);
		}

		ForwardingShipment CreateShipment(string uniqueConsignRef = null)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.ULD;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			shipment.ConsignorPK = Consignor.PK;
			shipment.ConsigneePK = Consignee.PK;

			shipment.JS_UniqueConsignRef = uniqueConsignRef;

			shipment.RunPreSaveValidation();

			AssertEquals("Prerequisite: shipment should have no errors; please adjust setup if this test fails",
				"",
				shipment.GetErrors().ToUniqueMessageListString());

			return shipment;
		}

		ForwardingConsol CreateConsol(string uniqueConsignRef = null)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.ULD;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			consol.Transports[0].JW_VoyageFlight = "QF512";
			consol.Transports[0].JW_ETD = ZDateTime.Today;

			consol.JK_UniqueConsignRef = uniqueConsignRef;
			consol.JK_MasterBillNum = uniqueConsignRef;

			consol.RunPreSaveValidation();

			AssertEquals("Prerequisite: consol should have no errors; please adjust setup if this test fails",
				"",
				consol.GetErrors().ToUniqueMessageListString());

			return consol;
		}

		OrgHeader Consignor
		{
			get
			{
				if (consignor == null)
				{
					consignor = Factory.NewWithValidTestData<OrgHeader>();
					consignor.OH_FullName = "CONSIGNOR";
					consignor.MainAddress.OA_Address1 = "Consignor Address";
					consignor.OH_IsConsignor = true;
				}

				return consignor;
			}
		}
		OrgHeader consignor;

		OrgHeader Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = Factory.NewWithValidTestData<OrgHeader>();
					consignee.OH_FullName = "CONSIGNEE";
					consignee.MainAddress.OA_Address1 = "Consignee Address";
					consignee.OH_IsConsignee = true;
				}

				return consignee;
			}
		}
		OrgHeader consignee;
	}
}
