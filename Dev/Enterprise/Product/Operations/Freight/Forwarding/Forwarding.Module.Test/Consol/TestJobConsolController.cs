using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(JobConsolController))]
	public class TestJobConsolController : ZControllerBasherTest
	{
		public void TestControllerIsSettingIsRootOnOpenedConsols()
		{
			var controller = new JobConsolController();

			using (var form = controller.ShowNewForm() as ConsolForm)
			{
				var newConsol = form.BusinessEntity as ForwardingConsol;
				Assert(newConsol.IsRoot);
			}

			var savedConsol = (ForwardingConsol)GetBusinessObjectThatIsInTheDatabase();
			using (var form = controller.ShowEditForm(savedConsol) as ConsolForm)
			{
				savedConsol = form.BusinessEntity as ForwardingConsol;
				Assert(savedConsol.IsRoot);
			}
		}

		public void TestConsolTypeAccessCheckpoint()
		{
			Env.Security.MaintainConsolEdit.IsAllowed = true;
			ForwardingConsol consol = (ForwardingConsol)GetBusinessObjectThatIsInTheDatabase();
			JobConsolController controller = new JobConsolController();

			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			Env.Security.MaintainConsolTypeAgent.IsAllowed = true;
			IZForm form = controller.ShowEditForm(consol);
			AssertEquals("Agent consol security rights enabled, form should be browsable", form.DisplayMode, ODisplayMode.Browse);
			DisposeForm(form);

			Env.Security.MaintainConsolTypeAgent.IsAllowed = false;
			form = controller.ShowEditForm(consol);
			AssertEquals("Agent consol security rights disabled, form should be ReadOnly", form.DisplayMode, ODisplayMode.ReadOnly);
			DisposeForm(form);

			consol.JK_AgentType = "ZZX";
			form = controller.ShowEditForm(consol);
			AssertEquals("Invalid consol type provided, form should be browsable", form.DisplayMode, ODisplayMode.Browse);
			DisposeForm(form);
		}

		public void TestJobConsolControllerChildEditableState()
		{
			var consolController = ZControllerFactory.Create(GetControllerID());
			AssertEquals(ChildEditableServiceStates.Consol, ChildEditableService.GetStateDirectly(consolController.Factory));
		}

		#region Template Records

		public void TestGetLoadedBusinessEntityInLocalFactory_NormalRecord()
		{
			var consol = (ForwardingConsol)GetBusinessObjectThatIsInTheDatabase();
			consol.JK_MasterBillNum = "ABC";
			Factory.Save();

			var controller = new JobConsolControllerForTest();
			var localBizO = controller.GetLoadedBusinessEntityInLocalFactory_Exposed(consol) as ForwardingConsol;

			AssertNotNull(localBizO);

			CombineAssertions(() =>
			{
				AssertEquals("ABC", localBizO.JK_MasterBillNum);
				AssertNotEquals("Should be in different factory", consol.Factory._Instance, localBizO.Factory._Instance);
				AssertNotEquals("Should not be in template factory", typeof(TemplateRecordBusinessObjectFactory), localBizO.Factory.GetType());
				AssertEquals("Should have same PK", consol.PK, localBizO.PK);
				AssertNull((localBizO as ITemplateRecordProvider)?.TemplateRecord);
			});
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_TemplateRecordProvider()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;

			factory.TemplateRecordProvider = consol;
			factory.TemplateRecordProvider.IsTemplateRecord = true;
			factory.TemplateRecordProvider.TemplateRecord = templateRecord;

			consol.JK_MasterBillNum = "ABC";
			consol.Factory.Save();

			var controller = new JobConsolControllerForTest();
			var localBizO = controller.GetLoadedBusinessEntityInLocalFactory_Exposed(consol) as ForwardingConsol;

			CombineAssertions(() =>
			{
				AssertEquals("ABC", localBizO.JK_MasterBillNum);
				AssertNotEquals("Should be in different factory", consol.Factory._Instance, localBizO.Factory._Instance);
				AssertEquals("Should be in template factory", typeof(TemplateRecordBusinessObjectFactory), localBizO.Factory.GetType());
				AssertNotEquals("Template records do not keep PKs", consol.PK, localBizO.PK);
				AssertEquals(templateRecord.PK, localBizO.TemplateRecord.PK);
			});
		}

		public void TestGetLoadedBusinessEntityInLocalFactory_TemplateRecord()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;

			factory.TemplateRecordProvider = consol;
			factory.TemplateRecordProvider.IsTemplateRecord = true;
			factory.TemplateRecordProvider.TemplateRecord = templateRecord;

			consol.JK_MasterBillNum = "ABC";
			consol.Factory.Save();

			var controller = new JobConsolControllerForTest();
			var localBizO = controller.GetLoadedBusinessEntityInLocalFactory_Exposed(consol.TemplateRecord) as ForwardingConsol;

			CombineAssertions(() =>
			{
				AssertEquals("ABC", localBizO.JK_MasterBillNum);
				AssertNotEquals("Should be in different factory", consol.Factory._Instance, localBizO.Factory._Instance);
				AssertEquals("Should be in template factory", typeof(TemplateRecordBusinessObjectFactory), localBizO.Factory.GetType());
				AssertNotEquals("Template records do not keep PKs", consol.PK, localBizO.PK);
				AssertEquals(templateRecord.PK, localBizO.TemplateRecord.PK);
			});
		}

		#endregion

		#region Show Consol Form with (un)restricted shipments

		public void TestShowViewForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToTrue()
		{
			var controller = new JobConsolController();
			AssertShowConsolForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRights(true, controller.ShowViewForm);
		}

		public void TestShowEditForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToTrue()
		{
			var controller = new JobConsolController();
			AssertShowConsolForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRights(true, controller.ShowEditForm);
		}

		public void TestShowDeleteForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToTrue()
		{
			var controller = new JobConsolController();
			AssertShowConsolForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRights(true, controller.ShowDeleteForm);
		}

		public void TestShowViewForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToFalse()
		{
			var controller = new JobConsolController();
			AssertShowConsolForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRights(false, controller.ShowViewForm);
		}

		public void TestShowEditForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToFalse()
		{
			var controller = new JobConsolController();
			AssertShowConsolForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRights(false, controller.ShowEditForm);
		}

		public void TestShowDeleteForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRightsSetToFalse()
		{
			var controller = new JobConsolController();
			AssertShowConsolForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRights(false, controller.ShowDeleteForm);
		}

		void AssertShowConsolForm_ConsolAllowAccessRegardlessOfShipmentsOSMGRights(bool consolAllowAccessRegardlessOfShipmentsOSMGRights, Func<ForwardingConsol, IZForm> formAction)
		{
			FreightDataRegistry.Instance.ConsolAllowAccessRegardlessOfShipmentsOSMGRights.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, consolAllowAccessRegardlessOfShipmentsOSMGRights);

			var crmSecurityProvider = new JobShipmentCRMSecurityProvider();
			crmSecurityProvider.CRMSecurity.IgnoreOSMG.IsAllowed = true;
			crmSecurityProvider.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;
			crmSecurityProvider.CRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

			var testData = ConsolRelatedShipmentsOSMGSecurityCheckpointTest.GetTestData(Factory);

			foreach (var consol in testData.AllConsols)
			{
				using (var form = formAction(consol))
				{
					AssertNotNull("Should have shown a form.", form);
					AssertEquals("Should not have shown an error", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}

			crmSecurityProvider.CRMSecurity.IgnoreOSMG.IsAllowed = false;
			crmSecurityProvider.CRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;
			crmSecurityProvider.CRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			if (consolAllowAccessRegardlessOfShipmentsOSMGRights)
			{
				foreach (var consol in testData.AllConsols)
				{
					using (var form = formAction(consol))
					{
						AssertNotNull("Should have shown a form.", form);
						AssertEquals("Should not have shown an error", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
			else
			{
				foreach (var consol in testData.ConsolsWithAccessDenied)
				{
					using (var form = formAction(consol))
					{
						AssertNull("Should not have shown a form.", form);
						AssertEquals("Should have shown an error", @"Error You do not have the appropriate security rights to run this function.

At least one of the shipments on this consolidation has its access restricted.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:
Operate -> Forwarding -> Shipments -> Search and View Records Assigned to Other Login Staff
Operate -> Forwarding -> Shipments -> Permit Unconditional access regardless of Task assignment
Operate -> Forwarding -> Shipments -> Permit Unconditional access regardless of Org. Security Groups", UnitTestUserNotification.Instance.LastMessage.ToString());
						UnitTestUserNotification.Instance.ClearMessages();
					}
				}
				foreach (var consol in testData.ConsolsWithAccessGranted)
				{
					using (var form = formAction(consol))
					{
						AssertNotNull("Should have shown a form.", form);
						AssertEquals("Should not have shown an error", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
					}
				}
			}
		}

		#endregion

		protected override BusinessObject GetBusinessObjectWithoutValidationErrors()
		{
			var consol = base.GetBusinessObjectWithoutValidationErrors() as ForwardingConsol;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.BreakBulk;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "123";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			return consol;
		}

		protected override IEnumerable<ControllerID> NonCustomsPlugInsToExcludeFromTest
			=> new ControllerID[] { ControllerIDs.ComplexPickup, ControllerIDs.ComplexDelivery }
			.Union(base.NonCustomsPlugInsToExcludeFromTest);

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobConsol;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			return consol;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		void DisposeForm(IZForm form)
		{
			ZForm zForm = form as ZForm;
			zForm.Close();
		}

		class JobConsolControllerForTest : JobConsolController
		{
			public IBusiness GetLoadedBusinessEntityInLocalFactory_Exposed(IBusiness sourceEntity) => base.GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}

		#endregion
	}
}
