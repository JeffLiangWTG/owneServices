using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.CMDMessaging.Testing
{
	[TestedType(typeof(CMDShipmentUserControl))]
	sealed class CMDShipmentUserControlTest : BasherTest
	{
		public void TestCustomRowBackgroundColour()
		{
			using (CMDShipmentUserControl userControl = new CMDShipmentUserControl())
			{
				CMDEDIMessage message1 = Factory.New<CMDEDIMessage>();
				message1.EM_IsActive = true;
				message1.Reply = new CMDInbound("CMA/2\r\nA/N/N\r\nTesting123");
				CMDEDIMessage message2 = Factory.New<CMDEDIMessage>();
				message2.EM_IsActive = true;
				message2.Reply = new CMDInbound("FNA\r\nA/N/N\r\nTesting123");
				ZGrid grid = (ZGrid)userControl.GetType().GetField("messageGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(userControl);
				EventHandler<ColourDecidingEventArgs> colourDecidingEventHandler = (EventHandler<ColourDecidingEventArgs>)typeof(ZGrid).GetField("ColourDeciding", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(grid);
				ColourDecidingEventArgs args = new ColourDecidingEventArgs(message1);
				colourDecidingEventHandler(grid, args);
				AssertEquals("Not a failure reply. Should not be highlighted", Color.Empty, args.Colour);
				args = new ColourDecidingEventArgs(message2);
				colourDecidingEventHandler(grid, args);
				AssertEquals("Should be highlighted", Color.FromArgb(235, 155, 155), args.Colour);
			}
		}

		public void TestSetCustomEntryNumbersButtonClick()
		{
			using (CMDShipmentUserControl userControl = new CMDShipmentUserControl())
			{
				ForwardingShipment shipment = Factory.New<ForwardingShipment>();
				CMDShipmentWrapper shipmentWrapper = new CMDShipmentWrapper(shipment);
				userControl.SetDataBinding(shipmentWrapper, "");
				ZButton button = (ZButton)userControl.GetType().GetField("customsEntryNumbersButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(userControl);
				button.PerformClick();
				AssertEquals(typeof(CMDShipmentCusEntryNumberCollectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(shipmentWrapper, ((CMDShipmentCusEntryNumberCollectionForm)ZFormModaliser.LastFormShownDialogForTest).LastDataSourceForTest);
			}
		}

		public override Form GetFormToBash()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ZForm form = new ZForm(new CMDShipmentWrapper(shipment));
			form.CaptionRenderingEnabled = true;
			form.Controls.Add(new CMDShipmentUserControl());
			return form;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
