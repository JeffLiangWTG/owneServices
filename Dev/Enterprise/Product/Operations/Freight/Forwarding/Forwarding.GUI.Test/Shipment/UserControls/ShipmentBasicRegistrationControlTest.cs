using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentBasicRegistrationControlTest : ShipmentControlTest<ShipmentBasicRegistrationControl>
	{
		protected override string[] ExpectedPanelNames
		{
			get
			{
				return new string[]
				{
					CustomizablePanelNames.LeftTopPanel,
					CustomizablePanelNames.LeftBottomPanel,
					CustomizablePanelNames.MiddleTopPanel,
					CustomizablePanelNames.MiddleBottomPanel,
					CustomizablePanelNames.RightTopPanel,
					CustomizablePanelNames.RightBottomPanel
				};
			}
		}

		[RequiresSTA]
		public void TestAccountingJob_AquireMutexOnTabChange_WhenMutexAlreadyAquired()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "ZUBIN";
			var mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);

			try
			{
				mutex.Lock();

				using (var form = new ShipmentFormWithBasicRegistration(shipment))
				{
					form.Show();

					AssertEquals("Automated creation of Job Header.", form.ShipmentBasicRegistrationControl.JobHandler.InitializationMessage);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}
		}

		[RequiresSTA]
		public void TestAccountingJob_MutexReleased()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(shipment.PK);

			using (var form = new ShipmentFormWithBasicRegistration(shipment))
			{
				form.Show();
			}

			AssertEquals("Mutex released on dispose of the user control", false, mutex.IsLocked);
		}

		public void TestDetailsSplitContainerNotFixed()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			using (var form = new ShipmentFormWithBasicRegistration(shipment))
			{
				var detailsSplitContainer = (KSplitContainer)form.ShipmentBasicRegistrationControl.Controls.Find("splitContainer4", true).FirstOrDefault();
				Assert("DetailsSplitContainer must not be fixed.", detailsSplitContainer != null && !detailsSplitContainer.IsSplitterFixed);
			}
		}

		[RequiresSTA]
		public void TestCustomFieldsControlHints()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			using (FreightDataRegistry.Instance.ShipmentCustomText1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("String1", "String1 hint")))
			using (FreightDataRegistry.Instance.ShipmentCustomText2.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("String2", "String2 hint")))
			using (FreightDataRegistry.Instance.ShipmentCustomDate1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date1", "Date1 hint")))
			using (FreightDataRegistry.Instance.ShipmentCustomDate2.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date2", "Date2 hint")))
			using (FreightDataRegistry.Instance.ShipmentCustomDecimalNo1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal1", "Decimal1 hint")))
			using (FreightDataRegistry.Instance.ShipmentCustomDecimalNo2.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal2", "Decimal2 hint")))
			using (FreightDataRegistry.Instance.ShipmentCustomFlag1.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Flag1", "Flag1 hint")))
			using (FreightDataRegistry.Instance.ShipmentCustomFlag2.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Flag2", "Flag2 hint")))
			using (var form = new ShipmentFormWithBasicRegistration(shipment))
			{
				form.Show();

				var shipmentCustomFields = form.ShipmentBasicRegistrationControl.Controls.Find("ShipmentCustomFields", true).FirstOrDefault();
				AssertNotNull(shipmentCustomFields);

				var rowLayoutPanel = shipmentCustomFields.Controls.Find("rowLayoutPanel", true).FirstOrDefault();
				AssertNotNull(rowLayoutPanel);

				var dict = new Dictionary<string, string>()
				{
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMATTRIB1__prop__ZString", "String1 hint" },
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMATTRIB2__prop__ZString", "String2 hint" },
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMDATE1__prop__ZDateTime", "Date1 hint" },
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMDATE2__prop__ZDateTime", "Date2 hint" },
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMDECIMAL1__prop__ZDecimal", "Decimal1 hint" },
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMDECIMAL2__prop__ZDecimal", "Decimal2 hint" },
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMFLAG1__prop__ZBool", "Flag1 hint" },
					{ "__DOCSANDCARTAGE`-JP`=CUSTOMFLAG2__prop__ZBool", "Flag2 hint" },
				};

				foreach (Control dynamicControl in rowLayoutPanel.Controls)
				{
					var bindingMember = dynamicControl.GetBindingMember();

					Assert(dict.ContainsKey(bindingMember));
					var expectedHint = dict[bindingMember];

					var resCaptionedControl = dynamicControl as IResCaptionedControl;
					AssertNotNull(resCaptionedControl?.CaptionResourceString);
					AssertEquals(expectedHint, resCaptionedControl.CaptionResourceString.Caption);
				}
			}
		}
	}
}
