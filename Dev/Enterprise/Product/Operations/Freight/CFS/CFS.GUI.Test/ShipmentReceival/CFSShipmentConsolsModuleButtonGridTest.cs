using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	internal sealed class CFSShipmentConsolsModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestNewButton_Click()
		{
			var master =
				FreightTestHelper.GetShipment<CFSShipment>("S00001002", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var shipment =
				FreightTestHelper.GetShipment<CFSShipment>("S00001001", Constants.ShipmentTypes.CoLoadMaster, Factory);

			Factory.Save();

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var newButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>()
					.First();

				var helper = new Mock<IShipmentVsConsolMessageHelper>();

				using (CFSShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
				{
					string message = "NOT ALLOWED!";

					// NOT Allowed
					helper.Setup(m => m.IsAllowedToAddNewConsol(out message, It.IsAny<CommonShipment>()))
						.Returns(false);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					newButton.PerformClick();
					AssertEquals("Error NOT ALLOWED!", UnitTestUserNotification.Instance.LastMessage.ToString());

					// Allowed
					message = "";
					helper.Setup(m => m.IsAllowedToAddNewConsol(out message, It.IsAny<CommonShipment>())).Returns(true);
					helper.Setup(m => m.IsAllowedToAddNewShipment(out message, It.IsAny<CommonConsol>()))
						.Returns(false); // Last shown form is Consol Form that set up GridShipments.AllowAddNew
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					newButton.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

					string questionIsSubShipment =
						@"Question The shipment S00001001 is a sub-shipment of the master/lead shipment S00001002. A new load list will be attached to this shipment only, without attaching to its master.

If you would like to attach a new load list to this shipment, its master/lead and all sub-shipments of its master/lead, you need to attach a new load list to the master/lead shipment S00001002 instead.";

					message = "";
					// Allowed; asked about master - CANCEL
					helper.Setup(m => m.IsAllowedToAddNewConsol(out message, It.IsAny<CommonShipment>())).Returns(true);

					shipment.JS_JS_ColoadMasterShipment = master.PK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					newButton.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals(questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());
					}

					message = "";
					// Allowed; asked about master - OK
					helper.Setup(m => m.IsAllowedToAddNewConsol(out message, It.IsAny<CommonShipment>())).Returns(true);
					helper.Setup(m => m.IsAllowedToAddNewShipment(out message, null)).Returns(false); // Last shown form is Consol Form that set up GridShipments.AllowAddNew

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					newButton.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals(questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestAttachButton_Click()
		{
			var master = FreightTestHelper.GetShipment<CFSShipment>("S00001002", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var shipment = FreightTestHelper.GetShipment<CFSShipment>("S00001001", Constants.ShipmentTypes.CoLoadMaster, Factory);

			Factory.Save();

			var mockRepository = new MockRepository(MockBehavior.Strict);
			var mockIShipmentVsConsolMessageHelper = mockRepository.Create<IShipmentVsConsolMessageHelper>();

			var attachRequest = new ShipmentConsolAttachRequest(null, null);
			mockIShipmentVsConsolMessageHelper.SetupSequence(x => x.IsAllowedToAttachConsol(It.IsAny<CommonShipment>(), It.IsAny<CommonConsol>()))
			.Returns(new ShipmentConsolAttachRequest(() => "ERROR?", null))
			.Returns(attachRequest)
			.Returns(attachRequest)
			.Returns(attachRequest);

			string questionIsSubShipment = @"Question The shipment S00001001 is a sub-shipment of the master/lead shipment S00001002. A new load list will be attached to this shipment only, without attaching to its master.

If you would like to attach a new load list to this shipment, its master/lead and all sub-shipments of its master/lead, you need to attach a new load list to the master/lead shipment S00001002 instead.";

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				using (CFSShipmentVsConsolMessageHelper.OverrideHelperInstance(mockIShipmentVsConsolMessageHelper.Object))
				{
					// Has errors
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Error ERROR?", UnitTestUserNotification.Instance.LastMessage.ToString());

					// No master
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.Attacher.LastShownAttachPopupForTesting)
					{
						AssertEquals("Should be NO Errors", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
					}

					// Has master - Cancel
					shipment.JS_JS_ColoadMasterShipment = master.PK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
					button.PerformClick();
					AssertEquals("Should be IsSubShipment Question", questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());

					// Has master - OK
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					button.PerformClick();
					using (form.Grid.Attacher.LastShownAttachPopupForTesting)
					{
						AssertEquals("Should be IsSubShipment Question", questionIsSubShipment, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestAttacher()
		{
			ZDateTime today = ZDateTime.Today;

			var consol = FreightTestHelper.GetConsol<CFSLoadListConsol>("C00001001", Factory);

			var shipment = FreightTestHelper.GetShipment<CFSShipment>("S00001001", Constants.ShipmentTypes.CoLoadMaster, Factory);
			shipment.Consols.Add(consol);

			var sUB = FreightTestHelper.GetShipment("SUB", shipment, Constants.ShipmentTypes.StandardHouse, Factory);

			//past cut off date
			var pCD = FreightTestHelper.GetConsol<CFSLoadListConsol>("PCD", Factory);
			pCD.JK_ConsolCutOffDateLocal = today.AddDays(-1);

			//direct with shipment
			var dIR = FreightTestHelper.GetConsol<CFSLoadListConsol>("DIR", Factory);
			dIR.JK_AgentType = Constants.AgentType.Direct;
			dIR.Shipments.AddNew();

			//direct with NO shipment
			var dIR_EMPTY = FreightTestHelper.GetConsol<CFSLoadListConsol>("DIR_EMPTY", Factory);
			dIR_EMPTY.JK_AgentType = Constants.AgentType.Direct;

			//standard consol
			var sTD = FreightTestHelper.GetConsol<CFSLoadListConsol>("STD", Factory);

			Factory.Save();

			string commonMessage = "Question Of the load lists you are trying to attach to the shipment S00001001, there are load lists that cannot be attached for the following reasons:";

			string errorPastCutOffDate = string.Format(
				"Cannot attach the load list PCD to the shipment S00001001 as the load list Cut Off Date has passed.\r\n\r\n{0}",
				Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolAttachDetachShipmentAfterCutOffDate));

			string errorIsDirectConsol = "Cannot attach the Direct load list DIR to the shipment S00001001 because the load list already has another shipment attached.";

			string errorShipmentCanHaveDirectConsol = "Cannot attach the Direct load list DIR_EMPTY to the shipment S00001001 because the shipment is not a Standard House shipment, High Volume Low Value shipment or Assembly Master shipment with no master shipment.";

			string questionToSkip = string.Format(
				"{0}\r\n{1}\r\n{2}\r\n{3}",
				commonMessage,
				errorIsDirectConsol,
				errorShipmentCanHaveDirectConsol,
				errorPastCutOffDate);

			Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = false;
			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { pCD, dIR, dIR_EMPTY, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals("Consols should NOT be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has NOT changed", selected, pCD, dIR, dIR_EMPTY, sTD);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);

				consol.Shipments.RemoveAll();
				FreightTestHelper.AssertConsolCollection("shipment should have NO consols", shipment.Consols);
				FreightTestHelper.AssertShipmentCollection("consol should have NO shipments", consol.Shipments);
				FreightTestHelper.AssertShipmentCollection("shipment should have sub-shipment", shipment.CoLoadShipments, sUB);
				selected = new List<BusinessObject>() { pCD, dIR, dIR_EMPTY, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);

				shipment.CoLoadShipments.RemoveAll();
				FreightTestHelper.AssertShipmentCollection(shipment.CoLoadShipments);
				questionToSkip = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", commonMessage, errorIsDirectConsol, errorShipmentCanHaveDirectConsol, errorPastCutOffDate);
				selected = new List<BusinessObject>() { pCD, dIR, dIR_EMPTY, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);

				Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed = true;
				questionToSkip = string.Format("{0}\r\n{1}\r\n{2}\r\n{3}", commonMessage, errorIsDirectConsol, errorShipmentCanHaveDirectConsol, errorPastCutOffDate);
				selected = new List<BusinessObject>() { pCD, dIR, dIR_EMPTY, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, sTD);

				Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
				questionToSkip = string.Format("{0}\r\n{1}\r\n{2}", commonMessage, errorIsDirectConsol, errorShipmentCanHaveDirectConsol);
				selected = new List<BusinessObject>() { pCD, dIR, dIR_EMPTY, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Consols should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip consols", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertConsolCollection("Selected list has changed", selected, pCD, sTD);
			}
		}

		public void TestDetachButton_Click()
		{
			var shipment = Factory.New<CFSShipment>();
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var consol3 = shipment.Consols.AddNew();

			Factory.Save();

			using (var form = new MockShipmentForm(shipment))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				form.Grid.InnerGrid.Select(0);
				form.Grid.InnerGrid.Select(1);

				var selectedConsols = new[] { consol1, consol2 };
				var mockRepository = new MockRepository(MockBehavior.Strict);
				var mockIShipmentVsConsolGUIMessageHelper = mockRepository.Create<IShipmentVsConsolGUIMessageHelper>();

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(mockIShipmentVsConsolGUIMessageHelper.Object))
				{
					mockIShipmentVsConsolGUIMessageHelper.SetupSequence
					(
						x => x.IsAllowedToDetachConsols
						(
							It.Is<IShipmentVsConsolMessageHelper>(h => h == CFSShipmentVsConsolMessageHelper.Instance),
							It.Is<CommonShipment>(s => s == shipment),
							It.Is<IEnumerable<CommonConsol>>(c => VerifyList(c, selectedConsols))
						)
					)
					.Returns(false)
					.Returns(true);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					detachButton.PerformClick();
					Assert("Base detach dialog was not shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					detachButton.PerformClick();
					Assert("Base detach dialog was shown", UnitTestUserNotification.Instance.LastMessage.Text.StartsWith(form.Grid.DetachMessage.Caption));
				}
			}
		}

		static bool VerifyList(IEnumerable<CommonConsol> expectedList, CFSLoadListConsol[] actualList)
		{
			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
			return true;
		}

		#region Implementation

		class MockShipmentForm : ZForm
		{
			public MockShipmentForm(CFSShipment businessEntity)
				: base(businessEntity)
			{
			}

			public CFSShipmentConsolsModuleButtonGridForTest Grid;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new CFSShipmentConsolsModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";

				Grid.BindToFindBoxList = "Lookups+Consols_List";
				Grid.BindToGridList = "Consols";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				Controls.Add(Grid);
			}
		}

		class CFSShipmentConsolsModuleButtonGridForTest : CFSShipmentConsolsModuleButtonGrid
		{
			public CFSShipmentConsolsModuleButtonGridAttacherForTest Attacher { get; set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return Attacher ?? (Attacher = new CFSShipmentConsolsModuleButtonGridAttacherForTest(this.ParentShipment, destinationCollection, findBoxList, moduleID));
			}
		}

		class CFSShipmentConsolsModuleButtonGridAttacherForTest : CFSShipmentConsolsModuleButtonGrid.CFSShipmentConsolsModuleButtonGridAttacher
		{
			public CFSShipmentConsolsModuleButtonGridAttacherForTest(CFSShipment shipment, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(shipment, destinationCollection, findBoxList, moduleID)
			{
			}

			public bool CheckSelected(List<BusinessObject> selected)
			{
				return CheckAttaching(selected);
			}
		}

		#endregion //Implementation

		[TestedType(typeof(CFSShipmentConsolsModuleButtonGrid))]
		class CFSShipmentConsolsModuleButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
		{
		}
	}
}
