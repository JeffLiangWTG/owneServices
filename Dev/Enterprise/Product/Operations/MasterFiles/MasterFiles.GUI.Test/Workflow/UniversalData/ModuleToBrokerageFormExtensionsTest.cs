using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.GUI.FormExtensions;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ModuleToBrokerageFormExtensionsTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCreateRelatedJobsMenuItem()
		{
			using (var form = new DummyModuleToBrokerageForm())
			{
				var relatedJobsMenuItem = form.CreateRelatedJobsMainMenuItem<DummyModuleToBrokerageForm, DummyWithWorkflow>();
				AssertEquals("Related Jobs", relatedJobsMenuItem.Text);
				AssertEquals(1, relatedJobsMenuItem.MenuItems.Count);

				var shipmentsMenuItem = relatedJobsMenuItem.MenuItems[0];
				AssertEquals("Objects", shipmentsMenuItem.Text);
				AssertEquals(true, shipmentsMenuItem.Enabled);
			}
		}

		public void TestCreateRelatedJobsMainMenuItem()
		{
			using (var form = new DummyModuleToBrokerageForm())
			{
				var relatedJobsMenuItem = form.CreateRelatedJobsMainMenuItem<DummyModuleToBrokerageForm, DummyWithWorkflow>();
				AssertEquals("Related Jobs", relatedJobsMenuItem.Text);
				AssertEquals(1, relatedJobsMenuItem.MenuItems.Count);

				var menuItem = relatedJobsMenuItem.MenuItems[0];
				AssertEquals("Objects", menuItem.Text);

				relatedJobsMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(false, menuItem.Enabled);

				var dummy = Factory.New<DummyWithWorkflow>();
				dummy.Z0_Description = "LOADME";
				Factory.Save();

				form.Entity = dummy;
				relatedJobsMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(true, menuItem.Enabled);
				AssertEquals(1, menuItem.MenuItems.Count);

				var createMenuItem = menuItem.MenuItems[0];
				AssertEquals("Create Dummy", createMenuItem.Text.Trim());

				form.HasChanges = true;
				createMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Dummy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please save", UnitTestUserNotification.Instance.LastMessage.Text);

				form.HasChanges = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.EventType = Events.DataImportFailureCode;
				createMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Dummy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(@"Failure!
Failed import!", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.EventType = Events.DataImportCode;
				AssertEquals("Precondition: no Related Jobs", 0, dummy.RelatedJobs.Count);
				createMenuItem.PerformClick();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Was importer and job added to Related Jobs", 1, dummy.RelatedJobs.Count);

				using (var lastFormShown = ModuleToModuleFormExtensions.LastFormShownForTesting)
				{
					ModuleToModuleFormExtensions.LastFormShownForTesting = null;
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.EventType = Events.DataExportCode;
				createMenuItem.PerformClick();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Dummy", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Was Exported", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				relatedJobsMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(true, menuItem.Enabled);
				AssertEquals(1, menuItem.MenuItems.Count);

				var viewMenuItem = menuItem.MenuItems[0];
				AssertEquals("View Dummy", viewMenuItem.Text.Trim());

				viewMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				using (var lastFormShown = ModuleToModuleFormExtensions.LastFormShownForTesting)
				{
					ModuleToModuleFormExtensions.LastFormShownForTesting = null;
				}
			}
		}

		class DummyModuleToBrokerageForm : ZForm, IModuleToModuleForm<DummyWithWorkflow>
		{
			public DummyModuleToBrokerageForm()
			{
			}

			public DummyModuleToBrokerageForm(DummyWithWorkflow entity)
				: base(entity)
			{
			}

			public bool HasChanges { get; set; }

			public DummyWithWorkflow Entity { get; set; }

			public new DummyWithWorkflow BusinessEntity
			{
				get { return Entity; }
			}

			public ZString ErrorMessage { get { return "Please save"; } }
			public ResourceString TopLevelMenuItemCaption
			{
				get { return ResString.GetMultilingualString("CAADD205-A9E0-41DB-AABE-55AE2E33D18F", "Related Jobs"); }
			}

			public ResourceString MainMenuItemCaption
			{
				get { return ResString.GetMultilingualString("CAADD205-A9E0-41DB-AABE-55AE2E33D18F", "Objects"); }
			}

			public ZString RelatedEntityName { get { return "Dummy"; } }
			public ZString RelatedEntityDescription { get { return ZString.Empty; } }
			public ResourceString WasExportedNotificationMessage
			{
				get { return ResString.GetMultilingualString("C0ED6B19-849A-4A13-BC02-2B8B2C0D212B", "Was Exported"); }
			}

			ControllerID IModuleToModuleForm<DummyWithWorkflow>.ControllerID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			IModuleToModuleSender IModuleToModuleForm<DummyWithWorkflow>.GetModuleToModuleSender()
			{
				var sender = new DummyModuleToModuleSender();
				sender.EventType = EventType;
				return sender;
			}

			internal string EventType { get; set; }
		}

		class DummyWithWorkflow : Business.Testing.DummyWithWorkflow, IModuleToModule, ISupportRelatedJobs
		{
			public DummyWithWorkflow(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			IOrgHeader IModuleToModule.RecipientOrganisation { get { return GlbCompany.CurrentCompany.OrgProxy; } }
			BusinessObject IModuleToModule.GetRelatedObject()
			{
				return RelatedJobs.Count > 0 ? (BusinessObject)RelatedJobs[0] : null;
			}

			bool IModuleToModule.CanExportData(out ZString errorMessage)
			{
				errorMessage = ZString.Empty;
				return true;
			}

			void IModuleToModule.AddToRelatedJobs(BusinessObject loadedJob)
			{
				RelatedJobs.Add(loadedJob);
			}

			public RelatedJobCollection RelatedJobs
			{
				get { return relatedJobs ?? (relatedJobs = new RelatedJobCollection(Factory)); }
			}
			RelatedJobCollection relatedJobs;
		}

		class DummyModuleToModuleSender : DataTransfer.Universal.Testing.ModuleToModuleSenderTest.DummyModuleToModuleSender,
			IModuleToModuleSender
		{
			internal string EventType { get; set; }

			#region IModuleToModuleSender Members

			public PublishToUniversalResult CreateJob<T>(T businessEntity)
				where T : BusinessObject, IWorkflowProvider, IModuleToModule
			{
				SetUpEventIfNeeded(EventType, "LOADME");
				return CreateEntityFromParent(businessEntity as DummyWithWorkflow);
			}

			#endregion
		}
	}
}
