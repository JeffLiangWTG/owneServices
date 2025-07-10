using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(ShipmentPackingDetailForm))]
	public class ShipmentPackingDetailFormTest : ZFormBasherTest
	{
		#region Tests

		public void TestCreateInstance()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var form = new ShipmentPackingDetailForm(shipment))
			{
				form.Show();

				AssertEquals(shipment, form.DataSource);
			}
		}

		public void TestShipmentOuterPacksCollectionIsINotificationProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertNotNull(shipment.OuterPackLines);
		}

		[RequiresSTA]
		public void TestValidateOuterPacksBeforeClose()
		{
			var shipment = Factory.New<ForwardingShipment>();

			ForwardingPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 5;

			ForwardingPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 5;

			shipment.OuterPackLines.RunPreSaveValidation();

			AssertEquals(false, (shipment.OuterPackLines as INotificationProvider).HasNotifications(NotificationType.Error));

			using (var form = new ShipmentPackingDetailFormForTest(shipment))
			{
				form.Show();
				form.MatchShipmentTotalsCheckBox.Checked = false;

				packLine1.JL_F3_NKPackType = "XXX";

				FormClosedEventHandler failWhenClosed = (s, e) => Assertion.Fail("Form should not close");

				form.FormClosed += failWhenClosed;

				form.Close();

				AssertEquals("Please fix errors before closing this dialog.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, (shipment.OuterPackLines as INotificationProvider).HasNotifications(NotificationType.Error));

				form.FormClosed -= failWhenClosed;

				packLine1.JL_F3_NKPackType = "BOX";

				form.Close();

				AssertEquals(false, (shipment.OuterPackLines as INotificationProvider).HasNotifications(NotificationType.Error));
			}
		}

		public void TestValidateOnlyPackLineProperties()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "ZZZ";
			shipment.Validation.ValidateJS_RL_NKOrigin();

			ForwardingPackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 5;

			ForwardingPackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 5;

			shipment.OuterPackLines.RunPreSaveValidation();

			AssertEquals(true, shipment.HasErrors);
			AssertEquals(false, (shipment.OuterPackLines as INotificationProvider).HasNotifications(NotificationType.Error));

			bool formWasClosed = false;

			using (var form = new ShipmentPackingDetailFormForTest(shipment))
			{
				form.Show();
				form.MatchShipmentTotalsCheckBox.Checked = false;
				form.FormClosed += (s, e) => formWasClosed = true;

				form.Close();
			}

			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			AssertEquals(true, formWasClosed);
		}

		public void TestMatchShipmentTotals()
		{
			var shipment = Factory.New<ForwardingShipment>();
			bool matchTotalsOriginalValue = shipment.MatchShipmentTotalsOnPackLinesDetailForm;

			try
			{
				using (var form = new ShipmentPackingDetailFormForTest(shipment))
				{
					form.Show();
					form.MatchShipmentTotalsCheckBox.Checked = true;
				}

				using (var form = new ShipmentPackingDetailFormForTest(shipment))
				{
					form.Show();
					AssertEquals(true, form.MatchShipmentTotalsCheckBox.Checked);
					form.MatchShipmentTotalsCheckBox.Checked = false;
				}

				using (var form = new ShipmentPackingDetailFormForTest(shipment))
				{
					form.Show();
					AssertEquals(false, form.MatchShipmentTotalsCheckBox.Checked);
				}
			}
			finally
			{
				shipment.MatchShipmentTotalsOnPackLinesDetailForm = matchTotalsOriginalValue;
			}
		}

		public void TestDisallowEditReadOnlyShipment()
		{
			using (Form parentForm = new Form())
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.ReadOnly = true;

				ShipmentPackingDetailFormForTest.Show(shipment, parentForm);
				AssertEquals("You're not allowed to edit packages of a read-only shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				shipment.ReadOnly = false;

				ShipmentPackingDetailFormForTest.Show(shipment, parentForm);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		[RequiresSTA]
		public void TestDisallowEditReadOnlyDueToPhaseReadOnly()
		{
			using (Form parentForm = new Form())
			{
				var shipment = Factory.New<ForwardingShipment>();

				PhaseSecurityTestHelper.Shipment.SetupRestricted("XXX");
				shipment.JS_Phase = "XXX";
				Factory.Save();

				Assert(shipment.IsReadOnlyDueToPhase);

				ShipmentPackingDetailFormForTest.Show(shipment, parentForm);
				AssertEquals("You're not allowed to edit packages of a read-only shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				shipment.JS_Phase = "ABC";
				Factory.Save();

				ShipmentPackingDetailFormForTest.Show(shipment, parentForm);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				var security = PhaseSecurityTestHelper.Shipment.SetupAllowed("XYZ");
				var rule = security.Phases[0].Rules[0];

				var dependant1 = rule.Dependants.AddNew();
				dependant1.Name = "OuterPackLines";
				dependant1.IsReadOnly = true;

				ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

				shipment.JS_Phase = "XYZ";
				Factory.Save();

				ShipmentPackingDetailFormForTest.Show(shipment, parentForm);
				AssertEquals("You're not allowed to edit packages of a read-only shipment.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestPackLinesReadOnly()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			bool matchTotalsOriginalValue = shipment.MatchShipmentTotalsOnPackLinesDetailForm;
			try
			{
				var subShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				subShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
				shipment.CoLoadShipments.Add(subShipment);

				var packLine1 = subShipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 2;

				var packLine2 = subShipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 5;

				Factory.Save();

				AssertEquals("Precondition", 2, subShipment.OuterPackLines.Count);
				AssertEquals("Precondition", 2, shipment.OuterPackLines.Count);
				AssertContainsExactElementsInAnyOrder(subShipment.OuterPackLines, shipment.OuterPackLines);
				AssertEquals("Precondition", false, packLine1.ReadOnly);
				AssertEquals("Precondition", false, packLine2.ReadOnly);

				using (Form parentForm = new Form())
				{
					subShipment.MatchShipmentTotalsOnPackLinesDetailForm = true;

					ShipmentPackingDetailFormForTest.Show(subShipment, parentForm);
					var packingDetailsForm = FindShipmentPackingDetailForm();

					AssertNotNull(packingDetailsForm);
					AssertEquals(false, shipment.OuterPackLines.ReadOnly);
					AssertEquals(false, subShipment.OuterPackLines.ReadOnly);
					AssertEquals(false, packLine1.ReadOnly);
					AssertEquals(false, packLine2.ReadOnly);

					packingDetailsForm.Close();

					AssertEquals(false, shipment.OuterPackLines.ReadOnly);
					AssertEquals(false, subShipment.OuterPackLines.ReadOnly);
					AssertEquals(false, packLine1.ReadOnly);
					AssertEquals(false, packLine2.ReadOnly);
				}
			}
			finally
			{
				shipment.MatchShipmentTotalsOnPackLinesDetailForm = matchTotalsOriginalValue;
			}
		}

		public void TestAddingPackLine()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			bool matchTotalsOriginalValue = shipment.MatchShipmentTotalsOnPackLinesDetailForm;
			try
			{
				Factory.Save();

				var subShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				subShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

				var packLine1 = subShipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 2;

				var packLine2 = subShipment.OuterPackLines.AddNew();
				packLine2.JL_PackageCount = 5;

				shipment.CoLoadShipments.Add(subShipment);

				AssertEquals("Precondition", 2, subShipment.OuterPackLines.Count);
				AssertEquals("Precondition", 2, shipment.OuterPackLines.Count);
				AssertContainsExactElementsInAnyOrder(subShipment.OuterPackLines, shipment.OuterPackLines);
				AssertEquals("Precondition", false, packLine1.ReadOnly);
				AssertEquals("Precondition", false, packLine2.ReadOnly);

				using (var parentForm = new Form())
				{
					subShipment.MatchShipmentTotalsOnPackLinesDetailForm = false;

					ShipmentPackingDetailFormForTest.Show(subShipment, parentForm);
					var packingDetailsForm = FindShipmentPackingDetailForm();

					AssertNotNull(packingDetailsForm);
					AssertEquals(false, shipment.OuterPackLines.ReadOnly);
					AssertEquals(false, subShipment.OuterPackLines.ReadOnly);
					AssertEquals(false, packLine1.ReadOnly && packLine2.ReadOnly);

					var packLine3 = subShipment.OuterPackLines.AddNew();

					packingDetailsForm.Close();

					AssertEquals(3, subShipment.OuterPackLines.Count);
					AssertEquals(3, shipment.OuterPackLines.Count);
					AssertContainsExactElementsInAnyOrder(subShipment.OuterPackLines, shipment.OuterPackLines);

					AssertEquals(false, shipment.OuterPackLines.ReadOnly);
					AssertEquals(false, subShipment.OuterPackLines.ReadOnly);
					AssertEquals(false, packLine1.ReadOnly);
					AssertEquals(false, packLine2.ReadOnly);
					AssertEquals(false, packLine3.ReadOnly);
				}
			}
			finally
			{
				shipment.MatchShipmentTotalsOnPackLinesDetailForm = matchTotalsOriginalValue;
			}
		}

		public void TestCheckTotalsDifferSyncPackages()
		{
			AssertCheckTotalsDiffer(true);
		}

		public void TestCheckTotalsDifferDoNotSyncPackages()
		{
			AssertCheckTotalsDiffer(false);
		}

		void AssertCheckTotalsDiffer(bool syncPackages)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			bool matchTotalsOriginalValue = shipment.MatchShipmentTotalsOnPackLinesDetailForm;
			try
			{
				Factory.Save();

				var subShipment = Factory.NewWithValidTestData<ForwardingShipment>();
				subShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
				subShipment.JS_OuterPacks = 2;

				ForwardingPackLine packLine1 = subShipment.OuterPackLines[0];

				shipment.CoLoadShipments.Add(subShipment);

				AssertEquals("Precondition", 2, shipment.JS_OuterPacks);

				using (Form parentForm = new Form())
				{
					subShipment.MatchShipmentTotalsOnPackLinesDetailForm = true;

					ShipmentPackingDetailFormForTest.Show(subShipment, parentForm);
					var packingDetailsForm = FindShipmentPackingDetailForm();

					packLine1.JL_PackageCount = 3;

					var packLine2 = subShipment.OuterPackLines.AddNew();
					packLine2.JL_PackageCount = 1;

					UnitTestUserNotification.Instance.AddAnswer(syncPackages ? DialogResult.Yes : DialogResult.No);
					packingDetailsForm.Close();

					AssertEquals(2, subShipment.OuterPackLines.Count);
					AssertEquals(2, shipment.OuterPackLines.Count);
					AssertContainsExactElementsInAnyOrder(subShipment.OuterPackLines, shipment.OuterPackLines);
					AssertEquals(syncPackages ? 4 : 2, shipment.JS_OuterPacks);
				}
			}
			finally
			{
				shipment.MatchShipmentTotalsOnPackLinesDetailForm = matchTotalsOriginalValue;
			}
		}

		public void TestAddingPacklineToRadOnlyCollectionDoesNotMakeItReadOnly()
		{
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			shipment1.CoLoadShipments.AddNew();
			shipment1.OuterPackLines.SetReadOnlyIncludingChildren(true);

			AssertEquals("Precondition", true, shipment1.OuterPackLines.ReadOnly);

			bool matchTotalsOriginalValue = shipment1.MatchShipmentTotalsOnPackLinesDetailForm;
			try
			{
				var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

				using (Form parentForm = new Form())
				{
					shipment2.MatchShipmentTotalsOnPackLinesDetailForm = false;

					ShipmentPackingDetailFormForTest.Show(shipment2, parentForm);
					var packingDetailsForm = FindShipmentPackingDetailForm();

					var newPackLine = shipment2.OuterPackLines.AddNew();

					shipment1.OuterPackLines.Add(newPackLine);

					AssertEquals(false, newPackLine.ReadOnly);

					packingDetailsForm.Close();

					AssertEquals(true, newPackLine.ReadOnly);
				}
			}
			finally
			{
				shipment1.MatchShipmentTotalsOnPackLinesDetailForm = matchTotalsOriginalValue;
			}
		}

		#endregion

		#region Implementation

		class ShipmentPackingDetailFormForTest : ShipmentPackingDetailForm
		{
			public ShipmentPackingDetailFormForTest(ForwardingShipment shipment)
				: base(shipment)
			{
			}

			public ZCheckBox MatchShipmentTotalsCheckBox
			{
				get { return matchShipmentTotalCheckBox; }
			}

			public ZButton OKButton
			{
				get { return okButton; }
			}

			public ZGrid Grid
			{
				get { return packLinesContol.Grid; }
			}
		}

		protected override Form GetFormToBashCore()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			return new ShipmentPackingDetailForm(shipment);
		}

		ShipmentPackingDetailForm FindShipmentPackingDetailForm()
		{
			return (from form in Application.OpenForms.Cast<Form>()
					where form.Name == "ShipmentPackingDetailForm"
					select form as ShipmentPackingDetailForm).FirstOrDefault();
		}

		#endregion
	}
}
