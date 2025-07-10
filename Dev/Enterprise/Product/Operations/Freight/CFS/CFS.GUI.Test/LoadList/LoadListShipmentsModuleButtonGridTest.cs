using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
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
	internal sealed class LoadListShipmentsModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestNewButton_Click()
		{
			var consol = FreightTestHelper.GetConsol<CFSLoadListConsol>("consol", Factory);
			Factory.Save();
			var mockRepository = new MockRepository(MockBehavior.Strict);
			var mockIShipmentVsConsolMessageHelper = mockRepository.Create<IShipmentVsConsolMessageHelper>();

			string message = "message";
			mockIShipmentVsConsolMessageHelper.SetupSequence(x => x.IsAllowedToAddNewShipment(out message, It.IsAny<CommonConsol>()))
				.Returns(false)
				.Returns(true);

			string errorMessage = "Error message";
			string noneMessage = "None ";

			using (var form = new MockConsolForm(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true).OfType<ZToolStripButton>().First();

				using (CFSShipmentVsConsolMessageHelper.OverrideHelperInstance(mockIShipmentVsConsolMessageHelper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should be Error", errorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.LastShownZForm)
					{
						AssertEquals("Should be NO Errors", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestAttachButton_Click()
		{
			var consol = FreightTestHelper.GetConsol<CFSLoadListConsol>("consol", Factory);
			Factory.Save();
			var mockRepository = new MockRepository(MockBehavior.Strict);
			var mockIShipmentVsConsolMessageHelper = mockRepository.Create<IShipmentVsConsolMessageHelper>();

			mockIShipmentVsConsolMessageHelper.SetupSequence(x => x.IsAllowedToAttachShipment(It.IsAny<CommonConsol>(), It.IsAny<CommonShipment>()))
				.Returns(new ShipmentConsolAttachRequest(null, null))
				.Returns(new ShipmentConsolAttachRequest(() => "ERROR?", null));

			string errorMessage = "Error ERROR?";
			string noneMessage = "None ";

			using (var form = new MockConsolForm(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();

				using (CFSShipmentVsConsolMessageHelper.OverrideHelperInstance(mockIShipmentVsConsolMessageHelper.Object))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					AssertEquals("Should be No Error", noneMessage, UnitTestUserNotification.Instance.LastMessage.ToString());

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					button.PerformClick();
					using (form.Grid.Attacher.LastShownAttachPopupForTesting)
					{
						AssertEquals("Should have Errors", errorMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		public void TestAttacher()
		{
			var consol = FreightTestHelper.GetConsol<CFSLoadListConsol>("H00001001", Factory);
			Factory.Save();

			var mAS_1 = FreightTestHelper.GetShipment<CFSShipment>("MAS_1", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var sUB_1 = FreightTestHelper.GetShipment("SUB_1", mAS_1, Constants.ShipmentTypes.StandardHouse, Factory);

			var mAS_2 = FreightTestHelper.GetShipment<CFSShipment>("MAS_2", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var sUB_2 = FreightTestHelper.GetShipment("SUB_2", mAS_2, Constants.ShipmentTypes.StandardHouse, Factory);

			var dIR = FreightTestHelper.GetShipment<CFSShipment>("DIR", Constants.ShipmentTypes.StandardHouse, Factory);
			var dirConsol = dIR.Consols.AddNew();
			dirConsol.JK_AgentType = Constants.AgentType.Direct;

			var sTD = FreightTestHelper.GetShipment<CFSShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);

			string questionToSkip = @"Question Of the shipments you are trying to attach to the load list H00001001, there are shipments that cannot be attached for the following reasons:
The shipment DIR is already attached to at least one other Direct Load list. A shipment on a Direct Load list can only be attached to Direct Load lists (with the exception of consolidations for pre-carriage or on-forwarding Road/Rail Agent Consol(s) or Air/Sea Domestic Consol(s)).";

			string questionToSkipSubShipments = @"Question The shipments that you are trying to attach are sub-shipments:
SUB_2 is a sub-shipment of master/lead MAS_2

Only these shipments, without their masters, will be attached to H00001001 load list.

If you would like to attach these shipments, their masters/leads and all sub-shipments of their masters/leads to this load list, you need to attach the master/lead shipments to this load list instead.

Press [Yes] if you would like to continue.
Press [No] if you would like to skip this shipments and apply all other selected shipments.
Press [Cancel] to cancel operation.";

			using (var form = new MockConsolForm(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var button = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				button.PerformClick();

				var selected = new List<BusinessObject>() { mAS_1, sUB_1, sUB_2, dIR, sTD };
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals("Shipments should NOT be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip shipments", questionToSkip, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should NOT be changed", selected, mAS_1, sUB_1, sUB_2, dIR, sTD);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertEquals("Shipments should NOT be added", false, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip sub-shipments", questionToSkipSubShipments, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should NOT be changed", selected, mAS_1, sUB_1, sUB_2, dIR, sTD);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertEquals("Shipments should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip sub-shipments", questionToSkipSubShipments, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should be changed", selected, mAS_1, sTD);

				selected = new List<BusinessObject>() { mAS_1, sUB_1, sUB_2, dIR, sTD };
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertEquals("Shipments should be added", true, form.Grid.Attacher.CheckSelected(selected));
				AssertMultilineASCIIEquals("Should be ask to skip sub-shipments", questionToSkipSubShipments, UnitTestUserNotification.Instance.LastMessage.ToString());
				FreightTestHelper.AssertShipmentCollection("selected shipments should be changed", selected, mAS_1, sUB_2, sTD);
			}
		}

		public void TestDetachButton_Click()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			Factory.Save();

			using (var form = new MockConsolForm(consol))
			{
				form.Show();
				var toolStrip = form.Grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var detachButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				form.Grid.InnerGrid.Select(0);
				form.Grid.InnerGrid.Select(1);

				var selectedShipments = new[] { shipment1, shipment2 };

				var mockRepository = new MockRepository(MockBehavior.Strict);
				var mockIShipmentVsConsolGUIMessageHelper = mockRepository.Create<IShipmentVsConsolGUIMessageHelper>();

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(mockIShipmentVsConsolGUIMessageHelper.Object))
				{
					mockIShipmentVsConsolGUIMessageHelper.SetupSequence
					(
						x => x.IsAllowedToDetachShipments
						(
							It.Is<IShipmentVsConsolMessageHelper>(h => h == CFSShipmentVsConsolMessageHelper.Instance),
							It.Is<CommonConsol>(c => c == consol),
							It.Is<IEnumerable<CommonShipment>>(s => VerifyList(s, selectedShipments))
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

		static bool VerifyList(IEnumerable<CommonShipment> expectedList, CFSShipment[] actualList)
		{
			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
			return true;
		}

		public void TestMasterChanged()
		{
			var oldMaster = Factory.New<CFSShipment>();
			var subShipment = Factory.New<CFSShipment>();
			subShipment.JS_JS_ColoadMasterShipment = oldMaster.PK;

			var newMaster = Factory.New<CFSShipment>();

			var consol = Factory.New<CFSLoadListConsol>();
			consol.Shipments.AddRange(oldMaster, newMaster);

			Factory.Save();

			using (var form = new MockConsolForm(consol))
			{
				form.Show();

				var helper = new Mock<IShipmentVsConsolGUIMessageHelper>();

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
				{
					helper
						.Setup(m => m.OnShipmentMasterChanged(CFSShipmentVsConsolMessageHelper.Instance, subShipment,
							consol,
							It.Is<MasterChangedEventArgs>(p =>
								p.OldMasterPK == oldMaster.PK && p.NewMasterPK == newMaster.PK)))
						.Callback(() => Assert(true));
					subShipment.JS_JS_ColoadMasterShipment = newMaster.PK;
				}
			}
		}

		public void TestDetachingMasterDoesNotThrowAnException()
		{
			var master = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			master.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			master.JS_IsCFSRegistered = true;

			var sub = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			sub.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			sub.JS_JS_ColoadMasterShipment = master.PK;
			sub.JS_IsCFSRegistered = true;

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol.Shipments.Add(master);
			consol.JK_IsCFS = true;

			Factory.Save();

			ReleaseFactory();

			var loadList = Factory.Load<CFSLoadListConsol>(consol.PK);

			using (var form = new MockConsolForm(loadList))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var subCFS = loadList.Shipments
				.Cast<CFSShipment>()
				.First(shp => shp.PK == sub.PK);

				AssertNoExceptionThrown(() => subCFS.JS_JS_ColoadMasterShipment = ZGuid.Empty);
			}
		}

		#region Implementation

		class MockConsolForm : ZForm
		{
			public MockConsolForm(CFSLoadListConsol businessEntity)
				: base(businessEntity)
			{
			}

			public LoadListShipmentsModuleButtonGridForTest Grid;

			public ZGridWithoutColumnStylesSerialisation InnerGrid
			{
				get { return Grid.InnerGrid; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Grid = new LoadListShipmentsModuleButtonGridForTest();

				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
				zTextBoxColumnStyleInfo2.ColumnName = "JS_UniqueConsignRef";

				Grid.BindToGridList = "Shipments";
				Grid.BindToFindBoxList = "Shipments_List";
				Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
				Controls.Add(Grid);
			}
		}

		class LoadListShipmentsModuleButtonGridForTest : LoadListShipmentsModuleButtonGrid
		{
			public LoadListShipmentsModuleButtonGridAttacherForTest Attacher { get; set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return Attacher ?? (Attacher = new LoadListShipmentsModuleButtonGridAttacherForTest(this.ParentConsol, destinationCollection, findBoxList, moduleID));
			}
		}

		class LoadListShipmentsModuleButtonGridAttacherForTest : LoadListShipmentsModuleButtonGrid.LoadListShipmentsModuleButtonGridAttacher
		{
			public LoadListShipmentsModuleButtonGridAttacherForTest(CFSLoadListConsol consol, IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
				: base(consol, destinationCollection, findBoxList, moduleID)
			{
			}

			public bool CheckSelected(List<BusinessObject> selected)
			{
				return CheckAttaching(selected);
			}
		}

		#endregion // Implementation

		[TestedType(typeof(LoadListShipmentsModuleButtonGrid))]
		class LoadListShipmentsModuleButtonGridModuleButtonGridTest : ZArchitecture.GUI.Testing.ZModuleButtonGridTestBase
		{
		}
	}
}
