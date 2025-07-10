using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class DetailsEntryControlTest : TestCaseWithFactory
	{
		public void TestUseDeniedPartyScreeningStatusDropEdit()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ZForm(shipment))
				{
					using (var control = new DetailsEntryControlForTest())
					{
						form.Controls.Add(control);
						form.Show();

						AssertEquals(false, control.ScreeningStatusForTest.Visible);
					}
				}
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (var form = new ZForm(shipment))
				{
					using (var control = new DetailsEntryControlForTest())
					{
						form.Controls.Add(control);
						form.Show();

						AssertEquals(true, control.ScreeningStatusForTest.Visible);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestChangeCommissionedShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";

			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				using (var control = new DetailsEntryControlForCommissionsTest())
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_RL_NKOrigin = "UAIEV";
					AssertEquals(shipment.JS_RL_NKOriginInfo, control.LastConfirmedInfo);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					shipment.JS_RL_NKDestination = "USLAX";
					AssertEquals(shipment.JS_RL_NKDestinationInfo, control.LastConfirmedInfo);
				}
			}
		}

		[RequiresSTA]
		public void TestPiecesDetailButton_WhenHVLShipment_IsReadOnly()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("HVL Shipment type should make button readonly", true, control.PiecesDetailButtonForTest.ReadOnly);
			}
		}

		public void TestPiecesDetailButton_WhenNotHVLShipment_IsNotReadOnly()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;

			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Non-HVL Shipment type should NOT make button readonly", false, control.PiecesDetailButtonForTest.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestPiecesDetailButton_WhenChangeShipmentType_TogglesReadOnly()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;

			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("HVL Shipment type should make button readonly", true, control.PiecesDetailButtonForTest.ReadOnly);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
				AssertEquals("Change to non-HVL Shipment type should NOT make button readonly", false, control.PiecesDetailButtonForTest.ReadOnly);

				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
				AssertEquals("Change back to HVL Shipment type should make button readonly again", true, control.PiecesDetailButtonForTest.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestHighRiskShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";

			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("Inspection Type is shown", control.Controls.Find("AviationSecurity", true)[0].Visible);
				Assert("High risk is not applicable", !control.Controls.Find("IsHighRisk", true)[0].Visible);
				Assert("Additional Inspection type is not applicable", !control.Controls.Find("AdditionalInspectionType", true)[0].Visible);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("Inspection Type is shown", control.Controls.Find("AviationSecurity", true)[0].Visible);
				Assert("High risk is shown", control.Controls.Find("IsHighRisk", true)[0].Visible);
				Assert("Additional Inspection type is shown", control.Controls.Find("AdditionalInspectionType", true)[0].Visible);
			}
		}

		public void TestDestinationGoodsValueAndExchangeRateUserControls()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";

			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("Destination Goods Value user control is shown", control.Controls.Find("DestinationGoodsValueCalcFindBox", true).Any());
				Assert("Destination Exchange Rate user control is shown", control.Controls.Find("DestinationExchangeRateCalcEdit", true).Any());
			}
		}

		#region CustomEntriesLinkLabel

		[RequiresSTA]
		public void TestCustomEntriesLinkLabel()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "AUBNE";
			CusEntryNumber shipmentCusEntryNumber = CreateCusEntryNumber(shipment, "MRN", "11111");

			BusinessObject declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
			declaration[JobDeclarationSchema.Constants.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.Constants.JE_GB] = GlbBranch.CurrentBranch.PK;
			declaration[JobDeclarationSchema.Constants.JE_MessageType] = "EXP";
			CusEntryNumber declarationCusEntryNumber = CreateCusEntryNumber(declaration, "COC", "22222");
			Factory.Save();

			var factory = new BusinessObjectFactory();
			shipment = factory.Load<ForwardingShipment>(shipment.PK);
			shipmentCusEntryNumber = factory.Load<CusEntryNumber>(shipmentCusEntryNumber.PK);
			declarationCusEntryNumber = factory.Load<CusEntryNumber>(declarationCusEntryNumber.PK);

			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("Custom Entries link is shown", control.CustomEntriesLinkLabelForTest.Visible);

				bool customsEntryNumbersFormShown = false;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) => customsEntryNumbersFormShown = popupForm is CustomsEntryNumbersForm);
				control.CustomEntriesLinkLabelForTest.OnLinkClicked_Exposed(null);
				AssertEquals("expected CustomsEntryNumbersForm shown", true, customsEntryNumbersFormShown);

				ForwardingShipmentCusEntryNumberProxyCollection cusEntryNumberProxyCollection = (ForwardingShipmentCusEntryNumberProxyCollection)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertNotNull(cusEntryNumberProxyCollection);

				AssertContainsExactElementsInAnyOrder(new[] { shipmentCusEntryNumber, declarationCusEntryNumber },
					cusEntryNumberProxyCollection.Cast<ForwardingShipmentCusEntryNumberProxy>().Select((proxy) => proxy.CusEntryNumber));

				AssertEquals("expected shipment custom entry number proxy not read only", false,
					cusEntryNumberProxyCollection.Cast<ForwardingShipmentCusEntryNumberProxy>().FirstOrDefault((proxy) => proxy.CusEntryNumber == shipmentCusEntryNumber).ReadOnly);
				AssertEquals("expected declaration custom entry number proxy read only", true,
					cusEntryNumberProxyCollection.Cast<ForwardingShipmentCusEntryNumberProxy>().FirstOrDefault((proxy) => proxy.CusEntryNumber == declarationCusEntryNumber).ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestPopupFormRemovingEntryNumberUpdatesControl()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			CusEntryNumber cusEntryNumber1 = CreateCusEntryNumber(shipment, "CAN", "111111");
			CusEntryNumber cusEntryNumber2 = CreateCusEntryNumber(shipment, "CCN", "222222");

			using (var form = new ZForm(shipment))
			using (var control = new DetailsEntryControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((popupForm) =>
				{
					CustomsEntryNumbersForm customsEntryNumbersForm = (CustomsEntryNumbersForm)popupForm;
					customsEntryNumbersForm.FormClosing += (s, e) =>
					{
						ForwardingShipmentCusEntryNumberProxyCollection cusEntryNumberProxyCollection = (ForwardingShipmentCusEntryNumberProxyCollection)customsEntryNumbersForm.DataSource;
						((IBusinessObjectCollection)cusEntryNumberProxyCollection).Delete(cusEntryNumberProxyCollection[0]);
					};
				});
				control.CustomEntriesLinkLabelForTest.OnLinkClicked_Exposed(null);

				AssertEquals("expected customs entry number removed", 1, shipment.CusEntryNumbers.Count);
			}
		}

		#endregion

		#region Implementation

		class DetailsEntryControlForTest : DetailsEntryControl
		{
			public DeniedPartyScreeningStatusDropEdit ScreeningStatusForTest => ScreeningStatus;

			public ZButton PiecesDetailButtonForTest => PiecesDetailButton;

			public ZLinkLabel CustomEntriesLinkLabelForTest => CustomEntriesLinkLabel;
		}

		class DetailsEntryControlForCommissionsTest : DetailsEntryControl
		{
			public ZPropertyInfo LastConfirmedInfo { get; set; }
			protected override void ConfirmReversal(ZPropertyInfo info)
			{
				LastConfirmedInfo = info;
				base.ConfirmReversal(info);
			}
		}

		CusEntryNumber CreateCusEntryNumber(BusinessObject parent, ZString numberType, ZString value)
		{
			CusEntryNumber number = Factory.New<CusEntryNumber>();
			number.CE_Category = "CUS";
			number.CE_EntryNum = value;
			number.CE_EntryType = numberType;
			number.CE_ParentID = parent.PK;
			number.CE_ParentTable = parent.TableName;
			number.CE_RN_NKCountryCode = "AU";
			return number;
		}

		#endregion
	}
}
