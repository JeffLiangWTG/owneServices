using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class CFSContainerFormVisibilityTest : BaseFreightTest
	{
		public void TestNewRaiseEventLogForm()
		{
			var bO = Factory.New<CFSContainer>();
			var @event = AutoEvents.DeliveryOrderHandedOver;

			Factory.Save();

			using (var containerForm = new CFSContainerFormTestClass(bO))
			using (var eventAddForm = containerForm.NewRaiseEventLogForm(@event))
			{
				var log = (BaseStmALog)eventAddForm.BusinessEntity;
				AssertEquals("Correct event type created", @event.Code, log.SL_SE_NKEvent);
			}
		}

		public void TestGuiFactoryServices()
		{
			CFSContainer cfsContainer = Factory.New<CFSContainer>();

			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);

			using (new CFSContainerForm(cfsContainer))
			{
				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		public void TestCaptionOnCreateNewContainerRegistrationButton()
		{
			CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;

			using (var cFSContainerForm = new CFSContainerFormTestClass(CFSContainer))
			{
				AssertEquals("Create Storage From CFS", cFSContainerForm.CreateNewContainerRegistrationButtonText);

				CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
				AssertEquals("Create CFS From Storage", cFSContainerForm.CreateNewContainerRegistrationButtonText);

				CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
				AssertEquals("Create Storage From CFS", cFSContainerForm.CreateNewContainerRegistrationButtonText);
			}
		}

		public void TestCreateNewContainerRegistrationButton_Click()
		{
			using (CFSContainerForm form = new CFSContainerForm(CFSContainer))
			{
				CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
				form.Show();
				form.CreateNewContainerRegistrationButton.PerformClick();
				AssertEquals("Error Please save the form before you create a new container registration.", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Factory.Save();
				form.CreateNewContainerRegistrationButton.PerformClick();
				AssertEquals("Error You must enter an arrival time before you create a new container registration.", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				CFSContainer.JC_ArrivalTime = ZDateTime.Now;
				Factory.Save();
				form.CreateNewContainerRegistrationButton.PerformClick();

				AssertNull("There should be no errors", UnitTestUserNotification.Instance.LastMessage.Text);
				Application.OpenForms.OfType<CFSContainerForm>().Single(cfsForm => cfsForm.ControllerID == ControllerIDs.PackContainerRegistration).Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestContainerModeReadOnlynessDoesNotStackOverflowInGui()
		{
			CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.CFS;
			using (CFSContainerForm cFSContainerForm = new CFSContainerForm(CFSContainer))
			{
				cFSContainerForm.Show();
				CFSContainer.JC_TransportMode = Constants.TransportModes.Air;
				CFSContainer.JC_ContainerMode = Constants.ContainerModes.ULD;
				Assert("Should not be read only", !CFSContainer.JC_RCInfo.ReadOnly);
				CFSContainer.JC_ContainerMode = Constants.ContainerModes.AIR;
				Assert("Should be read only", CFSContainer.JC_RCInfo.ReadOnly);
			}
		}

		public void TestPluginsAdded()
		{
			using (CFSContainerForm form = new CFSContainerForm(CFSContainer))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestJobInvoicingPlugin()
		{
			using (CFSContainerForm form = new CFSContainerForm(CFSContainer))
			{
				AssertEquals(false, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).Enabled);
			}

			CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
			using (CFSContainerForm form = new CFSContainerForm(CFSContainer))
			{
				AssertEquals(true, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing).Enabled);
			}
		}

		public void TestRegistrationDetailsTabPage()
		{
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(CFSContainer))
			{
				AssertEquals(DockStyle.Fill, form.RegistrationDetails.Dock);
				AssertNotNull(form.RegistrationDetails.Parent);
			}
		}

		public void TestFormCaption()
		{
			using (CFSContainerForm form = new CFSContainerForm(CFSContainer))
			{
				AssertEquals("Container Registration", form.FormCaption);
			}
		}

		public void TestShowWarningMessage()
		{
			using (CFSContainerForm form = new CFSContainerForm(CFSContainer))
			{
				CFSContainer.ContinueWithChanging = true;
				CFSContainer.WarningMessage = "Test Warning";
				AssertEquals("Question Test Warning", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new CFSContainerForm(Factory.New<CFSContainer>()));
		}

		#region Setup

		#region CFSContainer

		CFSContainer CFSContainer
		{
			get
			{
				if (fCFSContainer == null)
				{
					fCFSContainer = Factory.New<CFSContainer>();
				}
				return fCFSContainer;
			}
		}
		CFSContainer fCFSContainer;

		#endregion

		#region CFS Container Form Test Class

		public class CFSContainerFormTestClass : CFSContainerForm
		{
			public CFSContainerFormTestClass(CFSContainer container)
				: base(container)
			{
			}

			public new RegistrationDetails RegistrationDetails
			{
				get { return base.RegistrationDetails; }
			}

			public new ZTemplateTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}

			public new ZStmALogAddForm NewRaiseEventLogForm(Event @event)
			{
				return base.NewRaiseEventLogForm(@event);
			}

			public string CreateNewContainerRegistrationButtonText
			{
				get { return CreateNewContainerRegistrationButton.GetExtension<ILabelCaptionRenderer>().Caption; }
			}
		}

		#endregion

		#endregion
	}
}
