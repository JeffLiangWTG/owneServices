using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ModuleRegistration;
using Enterprise.Customs.ZA.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(Customs.Module.EntryHeaderController))]
	sealed class EntryHeaderControllerTest : Customs.Module.Testing.EntryHeaderControllerTest
	{
		[ExpectNoExceptions()]
		public void TestNoExceptionWhenOpenByUserWithoutOSMGSecurity()
		{
			var controller = new EntryHeaderController();
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			Env.Security.CustomsDeclarationEnquiryCRMSecurity.IgnoreOSMG.IsAllowed = true;
			using (var form = controller.ShowEditForm(entry) as ZForm)
			{
				form.Show();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			Env.Security.CustomsDeclarationEnquiryCRMSecurity.IgnoreOSMG.IsAllowed = false;
			using (var form = controller.ShowEditForm(entry) as ZForm)
			{
				AssertNull(form);
				AssertContains("Permit Unconditional access regardless of Org. Security Groups", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessages();
			Env.Security.CustomsDeclarationEnquiryCRMSecurity.IgnoreOSMG.IsAllowed = true;
			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;
			using (var form = controller.ShowEditForm(entry) as ZForm)
			{
				form.Show();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();
			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = false;
			using (var form = controller.ShowEditForm(entry) as ZForm)
			{
				AssertNull(form);
				AssertContains("Permit Unconditional access regardless of Org. Security Groups", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenedFormWithoutCustomsBrokerageUserControl()
		{
			var shipmentController = (JobDeclarationShipmentController)ZControllerFactory.Create(CustomsControllerIDs.JobDeclarationPluggedIntoShipment);
			using (var form = shipmentController.ShowNewForm())
			{
				form.Show();
				var shipment = ((ForwardingShipment)form.BusinessEntityForPersistingForm);
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
				shipment.Factory.Save();
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var controller = new EntryHeaderController();
				AssertNoExceptionThrown(() => controller.ShowEditForm(entry));
			}
		}

		public override Type ControllerToBashType => typeof(EntryHeaderController);

		protected override Type ExpectedFormType => typeof(Customs.GUI.BaseJobDeclarationForm);

		protected override Customs.Business.CusEntryHeader GetNewEntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.CustomsEntryHeaders.AddNew();
		}
	}
}
