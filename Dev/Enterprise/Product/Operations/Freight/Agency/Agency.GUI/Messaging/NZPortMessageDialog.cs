using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using MessageTypes = Enterprise.Core.Constants.EventReferenceMessageTypes;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class NZPortMessageDialog : ZChildForm
	{
		public NZPortMessageDialog(NZPortMessage message)
			: base(message)
		{
			InitializeComponent();

			warningText.Text = Res.GetString("de7f9642-b23d-11e4-aff1-902b34dc814a", "Please ensure all relevant bills have been correctly entered before attempting to send a Port Message as you may not be able to send an amendment.");

			var menuItemEdit = new ZMenuItem(ResString.GetMultilingualString("b2e751d2-b27b-11e4-9997-902b34dc814a", "Edit"), EditIssueMenuItemClicked);
			issuesGrid.ContextMenu.MenuItems.Add(menuItemEdit);
			issuesGrid.ContextMenu.MenuItems.Add(1, new ZMenuItem("-"));
		}

		public static void ShowDialog(JobVoyage voyage)
		{
			using (var form = new NZPortMessageDialog(new NZPortMessage(voyage)))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		#region Implementations

		IEnumerable<NonPersistentEDICommunicationMode> CommunicationModes
		{
			get
			{
				if (communicationModes == null)
				{
					communicationModes = new[]
					{
						new NonPersistentEDICommunicationMode
						{
							EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
							EK_Destination = EHubID
						}
					};
				}

				return communicationModes;
			}
		}
		IEnumerable<NonPersistentEDICommunicationMode> communicationModes;

		bool IsEHubIDSet
		{
			get
			{
				return !string.IsNullOrEmpty(EHubID);
			}
		}

		ZString EHubID
		{
			get
			{
				return ShippingPortsMessagingEHubIDHelper.GetEHubID(Message.Port);
			}
		}

		NZPortMessage Message
		{
			get
			{
				return (NZPortMessage)BusinessEntity;
			}
		}

		ZString PortCountryCode => Message.Port.SubstringSafe(0, 2);

		void UpdateStatus(string newStatus)
		{
			if (string.IsNullOrEmpty(newStatus))
			{
				issuesGrid.Visible = true;
				logTextBox.Visible = false;
			}
			else
			{
				issuesGrid.Visible = false;
				logTextBox.Visible = true;

				logTextBox.AppendText(newStatus);
				logTextBox.AppendText(System.Environment.NewLine);
			}
		}

		void ResetStatus()
		{
			logTextBox.Text = string.Empty;
		}

		#region Issues Managing

		void IssuesGridDoubleClick(object sender, MouseEventArgs e)
		{
			var rowNumber = issuesGrid.HitTest(e.X, e.Y).Row;

			if (rowNumber > -1)
			{
				var issue = (PortMessageIssue)issuesGrid.ListManager.List[rowNumber];
				OpenIssue(issue);
			}
		}

		void IssuesGridKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				e.Handled = true;

				var issue = issuesGrid.ListManager.GetCurrent() as PortMessageIssue;
				if (issue != null)
				{
					OpenIssue(issue);
				}
			}
		}

		void EditIssueMenuItemClicked(object sedner, EventArgs e)
		{
			var issue = issuesGrid.ListManager.GetCurrent() as PortMessageIssue;
			if (issue != null)
			{
				OpenIssue(issue);
			}
		}

		void OpenIssue(PortMessageIssue issue)
		{
			if (!issue.TargetPK.IsEmpty)
			{
				var controller = GetController(issue);

				if (controller != null)
				{
					var factory = new BusinessObjectFactory();
					controller.SetFormsModalTo(this);
					controller.ShowEditForm(factory.Load(controller.TypeOfTopLevelBusinessObject, issue.TargetPK));
				}
			}
		}

		ZController GetController(PortMessageIssue issue)
		{
			switch (issue.TargetCode)
			{
				case JobShipmentSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.AgencyBillOfLading);

				case OrgHeaderSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.Organisation);

				case GlbBranchSchema.Constants.Prefix:
					return ZControllerFactory.Create(ControllerIDs.GlbBranch);

				default:
					return null;
			}
		}

		#endregion

		#region Message Sending

		void SendButtonClicked(object sender, EventArgs e)
		{
			ResetStatus();
			Message.Issues.RemoveAndDeleteAll();

			if (TryValidateAndSend())
			{
				Globals.Message.ShowInformation(Res.GetString("b1101490-b23d-11e4-9e17-902b34dc814a", "Message successfully created and queued for delivery."), Res.GetString("3f5e2ffa-b0ba-11e4-be34-902b34dc814a", "Success"));
				Close();
			}
		}

		bool TryValidateAndSend()
		{
			Message.RunPreSaveValidation();

			if (Message.HasErrors())
			{
				ShowErrorsDialog();

				return false;
			}

			UpdateStatus(Res.GetString("10aa86c0-b64d-11e4-aa95-902b34dc814a", "Loading shipments..."));
			var shipments = Message.GetRelatedShipments();
			if (!shipments.Any())
			{
				UpdateStatus(ZString.Empty);

				var message = Message.Direction == Constants.PortDirection.Load
						? Res.GetString("a47c4d02-b641-11e4-9385-902b34dc814a", "There are no shipments on this voyage loading in {0}.", Message.Port)
						: Res.GetString("a9da01d0-b641-11e4-819a-902b34dc814a", "There are no shipments on this voyage discharging in {0}.", Message.Port);

				Globals.Message.ShowError(message, Res.GetString("55913eee-06fa-4dba-9d2e-9c016821bf37", "Error"));

				return false;
			}

			UpdateStatus(Res.GetString("1755f39c-b64d-11e4-85f7-902b34dc814a", "Validating shipments..."));
			var portConfig = PortManifestRegistryHelper.RetrievePortConfiguration(Message.Port, Message.PrincipalPK);
			if (portConfig == null || !portConfig.Enabled)
			{
				Message.Issues.AddNew
					(
						ZGuid.Empty,
						ZString.Empty,
						Res.GetString("305ae6c3-8da6-4801-bc45-3147eb8e55fc", "Registry Error: Liner & Agency > Port Messaging > Load & Discharge Manifest"),
						Res.GetString("2b711bed-494e-42ac-b567-109f17b65f1a", "Principal is not configured for sending Load & Discharge Manifest message to {0}.", Message.Port)
					);
			}

			Validate(shipments);

			if (Message.Issues.Any())
			{
				UpdateStatus(ZString.Empty);

				Globals.Message.ShowError(
					Res.GetString("f729c4e4-6d8b-4a52-a096-c4905798cd92", "Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", Message.Issues.Count),
					Res.GetString("55913eee-06fa-4dba-9d2e-9c016821bf37", "Error"));

				return false;
			}

			UpdateStatus(Res.GetString("264914ba-b64d-11e4-bee7-902b34dc814a", "Sending message..."));
			var logger = new NotificationsLogger(logTextBox);

			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Send Load & Manifest Port Message Factory" };
				using (factory.AddDisposableService())
				{
					SendMessage(factory, shipments, logger);
					factory.Save();
				}
				logger.Add(new InfoNotification((NoResString)"Delivery succeeded")); // Service task logs aren't res strings
			}, () => logger.AddWarning((NoResString)"Error during delivery. Retrying.")); // Service task logs aren't res strings

			if (logger.HasFatalErrors)
			{
				Globals.Message.ShowError(
					Res.GetString("66e7026c-b64c-11e4-8f7f-902b34dc814a", "An error occurred when sending the message. See output screen for more details."),
					Res.GetString("55913eee-06fa-4dba-9d2e-9c016821bf37", "Error"));

				return false;
			}
			return true;
		}

		void Validate(IEnumerable<BillOfLading> shipments)
		{
			if (!IsEHubIDSet)
			{
				Message.Issues.AddNew(ZGuid.Empty, ZString.Empty, Res.GetString("d89d77d2-b23d-11e4-8a16-902b34dc814a", "The Port Message cannot be sent as eHub ID is not set."), ZString.Empty);
			}

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			if (orgProxy != null)
			{
				if (orgProxy.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.CarrierPrincipalCode, PortCountryCode).IsEmpty)
				{
					Message.Issues.AddNew(orgProxy.PK, OrgHeaderSchema.Constants.Prefix, Res.GetString("f2103ecc-785f-4dee-936a-65565bb54a35", "The org. proxy does not have a CAR code for current country entered."), ZString.Empty);
				}
			}
			else
			{
				Message.Issues.AddNew(GlbBranch.CurrentBranch.PK, GlbBranchSchema.Constants.Prefix, Res.GetString("cafc6b76-b23d-11e4-bd69-902b34dc814a", "The current branch does not have an org. proxy."), ZString.Empty);
			}

			if (PortCountryCode == Core.Constants.CountryCodes.Spain)
			{
				var cto = Message.Factory.Load<OrgHeader>(Message.CTO);
				if (cto != null && cto.CustomsCodes.GetCustomsRegNo(OrgCusCode.SpainCodeTypes.NIF, PortCountryCode).IsEmpty)
				{
					Message.Issues.AddNew(cto.PK, OrgHeaderSchema.Constants.Prefix, Res.GetString("d0d2f737-3694-4b88-89dc-2e18f975c457", "The CTO does not have a NIF code for Spain."), ZString.Empty);
				}
			}

			foreach (var shipment in shipments)
			{
				NZPortMessageValidationStrategy.RegisterForFactory(shipment.Factory);
				shipment.MarkAsNeedingValidation();
				shipment.RealContainers.MarkAsNeedingValidation();
				shipment.RunPreSaveValidation();

				if (shipment.HasMessageErrors)
				{
					Message.Issues.AddNew(shipment.PK, JobShipmentSchema.Constants.Prefix, Res.GetString("b874a246-b23d-11e4-a268-902b34dc814a", "{0} has message errors.", shipment.JS_UniqueConsignRef), ExtractErrorDetail(shipment));
				}

				if (shipment.HasErrors)
				{
					Message.Issues.AddNew(shipment.PK, JobShipmentSchema.Constants.Prefix, Res.GetString("be4fc2fe-b23d-11e4-8336-902b34dc814a", "{0} has errors.", shipment.JS_UniqueConsignRef), ExtractErrorDetail(shipment));
				}

				if (PortCountryCode == Core.Constants.CountryCodes.Spain)
				{
					var principal = shipment.Principal;
					if (principal != null && principal.CustomsCodes.GetCustomsRegNo(OrgCusCode.SpainCodeTypes.NIF, PortCountryCode).IsEmpty)
					{
						Message.Issues.AddNew(principal.PK, OrgHeaderSchema.Constants.Prefix, Res.GetString("577f4e92-95ea-4161-94e1-18406e0c0a09", "{0}'s Principal does not have a NIF code for Spain.", shipment.JS_UniqueConsignRef), ZString.Empty);
					}

					if (Message.Direction == Constants.PortDirection.Load)
					{
						var consignor = shipment.Consignor;
						if (consignor != null && consignor.CustomsCodes.GetCustomsRegNo(OrgCusCode.SpainCodeTypes.NIF, PortCountryCode).IsEmpty)
						{
							Message.Issues.AddNew(consignor.PK, OrgHeaderSchema.Constants.Prefix, Res.GetString("1df1301a-ea22-4cff-b0d4-d51f549f4d99", "{0}'s Consignor does not have a NIF code for Spain.", shipment.JS_UniqueConsignRef), ZString.Empty);
						}
					}
					else
					{
						var consignee = shipment.Consignee;
						if (consignee != null && consignee.CustomsCodes.GetCustomsRegNo(OrgCusCode.SpainCodeTypes.NIF, PortCountryCode).IsEmpty)
						{
							Message.Issues.AddNew(consignee.PK, OrgHeaderSchema.Constants.Prefix, Res.GetString("9edf47d1-abe8-4810-bd54-bd85fbaa9b58", "{0}'s Consignee does not have a NIF code for Spain.", shipment.JS_UniqueConsignRef), ZString.Empty);
						}
					}
				}
			}
		}

		void SendMessage(BusinessObjectFactory factory, IEnumerable<BillOfLading> shipments, INotifications logger)
		{
			using (var exporter = new ManualDataExport(factory, shipments, UniversalDataType.UniversalShipment, CommunicationModes, UniversalXmlSchema.Version_2012_11_DO_NOT_USE, dataWriterGetter: dataWritingManager => new PortManifestDataObjectWriter(dataWritingManager, Message.GetSenderID(), Message.Port, Message.Direction, Message.MessageType)))
			{
				exporter.EventCode = GetEventType().Code;
				exporter.EventReference = StmALog.GenerateEventReference("", GetEventParameters());
				exporter.RecipientType = Message.Direction == Constants.PortDirection.Load ? nameof(RecipientRoleType.PEM) : nameof(RecipientRoleType.PIM);

				exporter.SendData(logger);
				Message.Voyage.Logs.AddNew(GetEventType(), exporter.EventReference);
			}
		}

		Event GetEventType()
		{
			switch (Message.MessageType)
			{
				case PortMessageTypeList.Codes.Original:
				case PortMessageTypeList.Codes.Replace:
					return Enterprise.ZArchitecture.Business.Events.MessageSent;

				case PortMessageTypeList.Codes.Cancellation:
					return Enterprise.ZArchitecture.Business.Events.MessageWithdrawCancelRequest;

				default:
					return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It is an event reference parameter")]
		KeyValuePair<string, string>[] GetEventParameters()
		{
			var parameters = new Dictionary<string, string>();

			switch (Message.MessageType)
			{
				case PortMessageTypeList.Codes.Original:

					parameters[Params.MessageType] = Message.Direction == Constants.PortDirection.Load ? MessageTypes.LoadManifest : MessageTypes.DischargeManifest;
					parameters[Params.Department] = (NoResString)"Terminal";
					parameters[Params.Location] = Message.Port;

					break;

				case PortMessageTypeList.Codes.Replace:

					parameters[Params.MessageType] = Message.Direction == Constants.PortDirection.Load ? MessageTypes.LoadManifestReplacement : MessageTypes.DischargeManifestReplacement;
					parameters[Params.Department] = "Terminal";
					parameters[Params.Location] = Message.Port;

					break;

				case PortMessageTypeList.Codes.Cancellation:

					parameters[Params.MessageType] = Message.Direction == Constants.PortDirection.Load ? MessageTypes.LoadManifestCancellation : MessageTypes.DischargeManifestCancellation;
					parameters[Params.Department] = "Terminal";
					parameters[Params.Location] = Message.Port;

					break;

				default:
					break;
			}

			return parameters.ToArray();
		}

		static string ExtractErrorDetail(BusinessObject obj)
		{
			var notifications = new ZNotificationCollector(obj, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
			var errors = notifications.GetErrors();
			var messageErrors = notifications.GetMessageErrors();

			return errors.Concat(messageErrors).ToUniqueMessageListString().Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
		}

		#endregion

		#endregion

		#region Types

		protected class NotificationsLogger : INotifications
		{
			public NotificationsLogger(ZTextBox logTextBox)
			{
				this.logTextBox = logTextBox;
			}

			public bool HasFatalErrors
			{
				get;
				private set;
			}

			public void Add(INotification notification)
			{
				logTextBox.AppendText(notification.Message);
				logTextBox.AppendText(System.Environment.NewLine);

				if (notification.Type.IsFatal)
				{
					HasFatalErrors = true;
				}
			}

			readonly ZTextBox logTextBox;
		}

		#endregion
	}
}



