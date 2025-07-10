using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageProcessors;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module
{
	public class BorderLineReleaseMessageModule : MQEDIMessageModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.US.BorderLineReleaseMessage;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.BorderLineReleaseMessage;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.BorderLineReleaseMessage);

		protected override IBusinessObjectCollection GetNewGridCollection() => new BorderLineReleaseMessageCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new BorderLineReleaseMessageFilterStripBusinessObject();

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.Add(new ZMenuItem(CreateNewDeclarationMenuName, new EventHandler(OnCreateNewDeclaration_Click)));
			result.Add(new ZMenuItem(AddToExistingShipmentMenuName, new EventHandler(OnAddToExistingShipment_Click)));
			result.Add(new ZMenuItem(OpenLinkedDeclarationMenuName, new EventHandler(OnOpenLinkedDeclaration_Click)));
			return result.ToArray();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			var columnsToHide = new string[]
			{
				LineReleaseMQEDIMessage.Schema.EM_SendOrReceiveHumanReadable,
				LineReleaseMQEDIMessage.Schema.EM_ApplicationCode,
				LineReleaseMQEDIMessage.Schema.EM_MessageNum,
				LineReleaseMQEDIMessage.Schema.EM_MessageType,
				LineReleaseMQEDIMessage.Schema.EM_MessageSubType,
				LineReleaseMQEDIMessage.Schema.EM_MessageSubTypeDescription,
				LineReleaseMQEDIMessage.Schema.EM_User,
				LineReleaseMQEDIMessage.Schema.EM_InterchangeSender,
				LineReleaseMQEDIMessage.Schema.EM_InterchangeReceiver,
				LineReleaseMQEDIMessage.Schema.EM_SendingUser,
				LineReleaseMQEDIMessage.Schema.EM_SystemCreateUser,
				LineReleaseMQEDIMessage.Schema.EM_DateTimeInterchangeSent,
				LineReleaseMQEDIMessage.Schema.EM_InterchangeNumber,
				LineReleaseMQEDIMessage.Schema.EM_InterchangeStatus,
				LineReleaseMQEDIMessage.Schema.EM_SystemLastEditTimeUtc,
				LineReleaseMQEDIMessage.Schema.EM_SystemLastEditUser,
				LineReleaseMQEDIMessage.Schema.EM_Status
			};
			var control = new BorderLineReleaseMessageFilterControl((BorderLineReleaseMessageCollection)GridCollection, (BorderLineReleaseMessageFilterStripBusinessObject)FilterBusinessObject, columnsToHide);
			control.FilteredGrid.SetColumnCaption(LineReleaseMQEDIMessage.Schema.EM_ApplicationReference, "Entry Number");
			return control;
		}

		void OnAddToExistingShipment_Click(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowWarning(SelectAtLeastOneMessage);
			}
			else
			{
				foreach (LineReleaseMQEDIMessage selectedMessage in Grid.SelectedElements)
				{
					try
					{
						var puller = new LineReleaseDeclarationFromShipmentPuller(new LineReleaseDeclarationCreator(selectedMessage));
						using (var pullerDialog = new Customs.Module.DeclarationFromShipmentPullerDialog(puller))
						{
#if DEBUG
							if (SetupPullerDialogForTestingEvent != null)
							{
								SetupPullerDialogForTestingEvent(pullerDialog);
							}
#endif

							if (ParentModalForm == null)
							{
								ZFormModaliser.ShowDialogWithoutDispose(pullerDialog);
							}
							else
							{
								ZFormModaliser.Show(pullerDialog, ParentModalForm);
							}
							if (pullerDialog.CreateClicked)
							{
								var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
								controller.ShowEditForm(puller);
							}
						}
					}
					catch (InvalidMessageFormatException ex)
					{
						Globals.Message.ShowError("Cannot create a new declaration as the message is invalid.\r\n" + ex.Message, "Invalid Format");
					}
					catch (InvalidOperationException ex)
					{
						Globals.Message.ShowError("Cannot create a new declaration as there is an error.\r\n" + ex.Message, "Setup Error");
					}
				}
			}
		}

		void OnCreateNewDeclaration_Click(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowWarning(SelectAtLeastOneMessage);
			}
			else
			{
				foreach (LineReleaseMQEDIMessage selectedMessage in Grid.SelectedElements)
				{
					try
					{
						var declaration = new LineReleaseDeclarationCreator(selectedMessage).CreateANewDeclaration();
						if (declaration != null)
						{
							var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
							controller.ShowFormForNewEntity(declaration);
#if DEBUG
							LastCreatedDeclarationPKForTesting = declaration.PK;
#endif
						}
					}
					catch (InvalidMessageFormatException ex)
					{
						Globals.Message.ShowError("Cannot create a new declaration as the message is invalid.\r\n" + ex.Message, "Invalid Format");
					}
					catch (InvalidOperationException ex)
					{
						Globals.Message.ShowError("Cannot create a new declaration as there is an error.\r\n" + ex.Message, "Setup Error");
					}
				}
			}
		}

		void OnOpenLinkedDeclaration_Click(object sender, EventArgs e)
		{
			if (Grid.SelectedElements.Length == 0)
			{
				Globals.Message.ShowWarning(SelectAtLeastOneMessage);
			}
			else
			{
				foreach (LineReleaseMQEDIMessage selectedMessage in Grid.SelectedElements)
				{
					try
					{
						var declarationPK = selectedMessage.EM_LinkUniqueID;

						if (!declarationPK.IsEmpty)
						{
							var declaration = Factory.Load<JobDeclaration>(declarationPK);
							var shipment = declaration.Shipment;
							if (shipment != null)
							{
								var shipmentController = (JobDeclarationShipmentController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
								shipmentController.ShowEditForm(shipment);
							}
							else
							{
								var controller = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
								controller.ShowEditForm(declaration);
							}
						}
						else
						{
							Globals.Message.ShowError("No declaration linked to this message.\r\n");
						}
					}
					catch (InvalidMessageFormatException ex)
					{
						Globals.Message.ShowError("Cannot open declaration as the message is invalid.\r\n" + ex.Message, "Invalid Format");
					}
					catch (InvalidOperationException ex)
					{
						Globals.Message.ShowError("Cannot open declaration as there is an error.\r\n" + ex.Message, "Setup Error");
					}
				}
			}
		}

#if DEBUG
		internal delegate void SetupPullerDialogEventHandler(Customs.Module.DeclarationFromShipmentPullerDialog pullerDialog);
		internal event SetupPullerDialogEventHandler SetupPullerDialogForTestingEvent;
		internal ZGuid LastCreatedDeclarationPKForTesting;
#endif

		internal const string CreateNewDeclarationMenuName = "Create A New Declaration";
		internal const string AddToExistingShipmentMenuName = "Add To An Existing Shipment";
		internal const string OpenLinkedDeclarationMenuName = "Open Linked Declaration";
	}
}
