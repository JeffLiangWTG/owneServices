using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class RelatedShipmentsControlTest : TestCaseWithFactory
	{
		public void TestMaintainShipmentAllowDetachingSubShipmentsSecurity()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var shipment = Factory.New<ForwardingShipment>();
			masterShipment.CoLoadShipments.Add(shipment);

			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;

			try
			{
				using (MockShipmentForm frm = new MockShipmentForm(shipment))
				{
					frm.Show();

					shipment.JS_JS_ColoadMasterShipmentForBinding = ZGuid.Empty;

					AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments),
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;
			}
		}

		public void TestMaintainShipmentAllowMovingSubShipmentsSecurity()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var subShipment = Factory.New<ForwardingShipment>();
			masterShipment.CoLoadShipments.Add(subShipment);

			Factory.Save();

			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = true;

			using (var control = new RelatedShipmentsControl())
			using (MockShipmentForm form = new MockShipmentForm(masterShipment))
			{
				control.SetDataBinding(masterShipment, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				control.ShipmentModuleButtonGrid.InnerGrid.SelectSingleElement(subShipment);

				var moduleGridMenuItems = control.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems;
				MenuItem subShipmentAllocMenuItem = moduleGridMenuItems.FindByText("Move sub shipments");
				subShipmentAllocMenuItem.PerformClick();

				AssertNullOrEmpty("Precondition: should not have security message",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed = false;

			using (var control = new RelatedShipmentsControl())
			using (MockShipmentForm form = new MockShipmentForm(masterShipment))
			{
				control.SetDataBinding(masterShipment, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var moduleGridMenuItems = control.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems;
				MenuItem subShipmentAllocMenuItem = moduleGridMenuItems.FindByText("Move sub shipments");
				subShipmentAllocMenuItem.PerformClick();

				AssertEquals("Security message should pop up if user does not have rights to detach subshipment",
					Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments),
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestControlBinding()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var masterShipment = Factory.New<ForwardingShipment>();
			shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(shipment))
			using (var control = new RelatedShipmentsControl())
			{
				control.SetDataBinding(shipment, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var moduleGrid = control.ShipmentModuleButtonGrid;

				AssertEquals("Lookups+CoLoadShipment_List", moduleGrid.BindToFindBoxList);
				AssertEquals("make sure bindingToList is added", 1, moduleGrid.DataBindings.Count);
				Assert("make sure no RelatedPropertyManager is created for Lookups", !moduleGrid.BindingContext.Contains(shipment, "Lookups"));
			}
		}

		public void TestShowPackingMenuItemExists()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (MockShipmentForm form = new MockShipmentForm(shipment))
			{
				AssertNotNull(form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing"));
			}
		}

		public void TestShowPackingDetailForms()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var subShipment1 = shipment.CoLoadShipments.AddNew();
			subShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			subShipment1.CoLoadShipments.AddNew();

			var subShipment2 = shipment.CoLoadShipments.AddNew();
			subShipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			using (MockShipmentForm form = new MockShipmentForm(shipment))
			{
				form.Show();

				form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Position = 0;

				form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();

				AssertEquals("You cannot edit pack lines of master shipments.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ListManager.Position = 1;

				form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Packing").PerformClick();

				AssertEquals(typeof(ShipmentPackingDetailForm), ZFormModaliser.ActiveForm.GetType());
			}
		}

		[RequiresSTA]
		public void TestMasterChanged()
		{
			var oldMaster = Factory.New<ForwardingShipment>();
			var subShipment = oldMaster.CoLoadShipments.AddNew();

			var newMaster = Factory.New<ForwardingShipment>();
			Factory.Save();

			using (var form = new MockShipmentForm(subShipment))
			{
				form.Show();

				var helper = new Mock<IShipmentVsConsolGUIMessageHelper>(MockBehavior.Strict);

				using (ShipmentVsConsolGUIMessageHelper.OverrideHelperInstance(helper.Object))
				{
					helper
						.Setup(m => m.OnShipmentMasterChanged(
							FreightShipmentVsConsolMessageHelper.Instance,
							subShipment, It.IsAny<CommonConsol>(),
							It.Is<MasterChangedEventArgs>(x =>
								x.OldMasterPK == oldMaster.PK && x.NewMasterPK == newMaster.PK)))
						.Callback(() => Assert(true));
					subShipment.JS_JS_ColoadMasterShipment = newMaster.PK;
				}
			}
		}

		public void TestCreateShipmentJobHeaders()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.AssemblyMaster;

			var shipment1 = masterShipment.CoLoadShipments.AddNew();
			shipment1.JS_UniqueConsignRef = "IGOR1";
			ZGlobalMutex mutex = JobHeader.GetMutex_ForTestOnly(shipment1.PK);

			var shipment2 = masterShipment.CoLoadShipments.AddNew();
			shipment2.JS_UniqueConsignRef = "IGOR2";
			new JobHeader.Loader(shipment2).TryCreate();

			var shipment3 = masterShipment.CoLoadShipments.AddNew();
			shipment3.JS_UniqueConsignRef = "IGOR3";
			shipment3.SetReadOnlyIncludingChildren(true);

			var shipment4 = masterShipment.CoLoadShipments.AddNew();
			shipment4.JS_UniqueConsignRef = "IGOR4";

			var shipment5 = masterShipment.CoLoadShipments.AddNew();
			shipment5.JS_UniqueConsignRef = "IGOR5";

			var shipment6 = masterShipment.CoLoadShipments.AddNew();
			shipment6.JS_UniqueConsignRef = "IGOR6";

			var shipment7 = masterShipment.CoLoadShipments.AddNew();
			shipment7.JS_UniqueConsignRef = "ALEX7";
			var job7 = new JobHeader.Loader(shipment7).TryCreate();
			job7.IsManuallyCreated = true;

			var shipment8 = masterShipment.CoLoadShipments.AddNew();
			shipment8.JS_UniqueConsignRef = "ALEX8";
			var job8 = new JobHeader.Loader(shipment8).TryCreate();

			var shipment9 = masterShipment.CoLoadShipments.AddNew();
			shipment9.JS_UniqueConsignRef = "ALEX9";
			var job9 = new JobHeader.Loader(shipment9).TryCreate();

			try
			{
				mutex.Lock();
				using (MockShipmentForm form = new MockShipmentForm(masterShipment))
				{
					form.Show();

					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record").PerformClick();

					AssertEquals("Please select shipments to create Job Invoicing Record.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.UnSelectAll();
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(1);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record").PerformClick();
					AssertEquals(@"Existing unsaved Job Invoicing Record has been marked as manually created for the following shipment(s):
- Shipment IGOR2
", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.UnSelectAll();
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(0);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(1);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(2);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(3);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(4);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(6);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(7);
					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.Select(8);

					form.RelatedShipmentsControl.ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record").PerformClick();

					AssertEquals(@"Job Invoicing Record has been created for the following shipment(s):
- Shipment IGOR4
- Shipment IGOR5

Existing unsaved Job Invoicing Record has been marked as manually created for the following shipment(s):
- Shipment ALEX8
- Shipment ALEX9

For other shipment(s) it can't be done due to the following errors:
You have created the job IGOR1 on another form, but haven't saved it yet.
Please close or save other forms that use job IGOR1 to continue.
Shipment IGOR3 is read only
", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertNull("shipment1 job header", shipment1.ShipmentJobHeader);
					AssertNotNull("shipment2 job header", shipment2.ShipmentJobHeader);
					Assert("IsManuallyCreated", shipment2.ShipmentJobHeader.IsManuallyCreated);
					AssertNull("shipment3 job header", shipment3.ShipmentJobHeader);
					AssertNotNull("shipment4 job header", shipment4.ShipmentJobHeader);
					Assert("IsManuallyCreated", !shipment4.ShipmentJobHeader.IsManuallyCreated);
					AssertNotNull("shipment5 job header", shipment5.ShipmentJobHeader);
					Assert("IsManuallyCreated", !shipment5.ShipmentJobHeader.IsManuallyCreated);
					AssertNull("shipment6 job header", shipment6.ShipmentJobHeader);
					Assert("IsManuallyCreated", job8.IsManuallyCreated);
					Assert("IsManuallyCreated", job9.IsManuallyCreated);
				}
			}
			finally
			{
				if (mutex.HasLock)
				{
					mutex.Unlock();
				}
			}

			mutex = JobHeader.GetMutex_ForTestOnly(shipment4.PK);
			AssertNotNull("Job Header Mutex", mutex);
			AssertEquals(false, mutex.IsLocked);

			mutex = JobHeader.GetMutex_ForTestOnly(shipment5.PK);
			AssertNotNull("Job Header Mutex", mutex);
			AssertEquals(false, mutex.IsLocked);
		}

		#region Implementation

		class MockShipmentForm : ZForm
		{
			public MockShipmentForm(ForwardingShipment businessEntity)
				: base(businessEntity)
			{
				InitializeComponent();
			}

			public RelatedShipmentsControl RelatedShipmentsControl
			{
				get;
				private set;
			}

			new void InitializeComponent()
			{
				this.BindingSource.DataSourceType = typeof(ForwardingShipment);
				RelatedShipmentsControl = new RelatedShipmentsControl();
				Controls.Add(RelatedShipmentsControl);
				this.BindingSource.SetBindingMember(this.RelatedShipmentsControl, ".");
			}
		}

		#endregion
	}
}
