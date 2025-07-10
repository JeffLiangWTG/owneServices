using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class DangerousGoodsManifestMessageDialog : ZChildForm
	{
		public DangerousGoodsManifestMessageDialog(DangerousGoodsManifestMessage message)
			: base(message)
		{
			InitializeComponent();

			warningText.Text = Res.GetString("de7f9642-b23d-11e4-aff1-902b34dc814a", "Please ensure all relevant bills have been correctly entered before attempting to send a Port Message as you may not be able to send an amendment.");

			var menuItemEdit = new ZMenuItem(ResString.GetMultilingualString("aaf055cd-fbba-464a-9858-b0501104cf82", "Edit"), EditIssueMenuItemClicked);
			issuesGrid.ContextMenu.MenuItems.Add(menuItemEdit);
			issuesGrid.ContextMenu.MenuItems.Add(1, new ZMenuItem("-"));
		}

		public PortMessageIssue CurrentIssue
		{
			get
			{
				CurrencyManager manager = issuesGrid.ListManager;
				return manager == null || manager.Position < 0 ? null : (PortMessageIssue)manager.GetCurrent();
			}
		}

		DangerousGoodsManifestMessage Message
		{
			get
			{
				return (DangerousGoodsManifestMessage)BusinessEntity;
			}
		}

		public static void ShowDialog(JobVoyage voyage)
		{
			using (var form = new DangerousGoodsManifestMessageDialog(new DangerousGoodsManifestMessage(voyage)))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
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
				Globals.Message.ShowInformation(Res.GetString("f10a16e7-8503-4f9d-8bbd-28d58a36e9c4", "Message successfully created and queued for delivery."), Res.GetString("bd5cd58e-1e3e-45cb-9ac4-1aa4b66f65f3", "Success"));
				Close();
			}
		}

		void ResetStatus()
		{
			logTextBox.Text = string.Empty;
		}

		bool TryValidateAndSend()
		{
			Message.RunPreSaveValidation();

			if (Message.HasErrors())
			{
				ShowErrorsDialog();
				return false;
			}
			UpdateStatus(Res.GetString("625fdd2f-c25b-4c80-a6e4-6d1f68373109", "Loading shipments..."));

			var billOfLadings = Message.GetRelatedBillOfLadings();
			if (!billOfLadings.Any())
			{
				UpdateStatus(ZString.Empty);

				var message = GetNoShipmentsErrorMessage();
				Globals.Message.ShowError(message, Res.GetString("24458c05-c3ce-449f-8bd0-a68430c944e3", "Error"));

				return false;
			}

			UpdateStatus(Res.GetString("2df81f52-e1e8-4ac0-bb3f-e2305e1b6a65", "Validating shipments..."));

			var portConfig = DangerousGoodsManifestRegistryHelper.RetrievePortConfiguration(Message.Port, Message.PrincipalPK);
			if (portConfig == null || !portConfig.Enabled)
			{
				Message.Issues.AddNew
					(
						ZGuid.Empty,
						ZString.Empty,
						Res.GetString("8C3CF257-12B2-49CB-AFB8-05DA3B978486", "Registry Error: Liner & Agency > Port Messaging > Dangerous Goods Manifest"),
						Res.GetString("9AFFC8A6-BD09-48BD-9A2D-072B4C6264DE", "Principal is not configured for sending Dangerous Goods Manifest message to {0}.", Message.Port)
					);
			}

			Validate(billOfLadings);
			if (Message.Issues.Any())
			{
				UpdateStatus(ZString.Empty);

				Globals.Message.ShowError(
					Res.GetString("9d39e1ce-a992-4ec7-beb8-e58fd4d2a2df", "Encountered {0} error(s) while attempting to collect the required information. Please correct the error(s) and try again.", Message.Issues.Count),
					Res.GetString("8b2edae6-b774-4ae2-8acb-2f428a04b25c", "Error"));

				return false;
			}

			UpdateStatus(Res.GetString("bf9a0d23-43a5-4b3a-bc92-ff133f1dfa7c", "Sending message..."));
			var logger = new NotificationsLogger(logTextBox);

			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Send Dangerous Goods Manifest Message Factory" };
				using (factory.AddDisposableService())
				{
					SendMessage(factory, billOfLadings, logger);
					factory.Save();
				}
				logger.Add(new InfoNotification((NoResString)"Delivery succeeded"));
			}, () => logger.AddWarning((NoResString)"Error during delivery. Retrying."));

			if (logger.HasFatalErrors)
			{
				Globals.Message.ShowError(
					Res.GetString("987c9e19-c478-4f01-a0dc-6f71a5cdace5", "An error occurred when sending the message. See output screen for more details."),
					Res.GetString("881251dc-1019-4932-b151-f29bd9be12e9", "Error"));

				return false;
			}

			return true;
		}

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

		string GetNoShipmentsErrorMessage()
		{
			switch (Message.Direction)
			{
				case Constants.PortDirection.Load:
					return Res.GetString("3dfc2d21-5132-4135-8b9d-48587ca7214d", "There are no shipments on this voyage loading in {0}.", Message.Port);

				case Constants.PortDirection.Discharge:
					return Res.GetString("a895beb4-bbae-4045-a305-ae0f2a6436f2", "There are no shipments on this voyage discharging in {0}.", Message.Port);

				case Constants.PortDirection.Transit:
					return Res.GetString("4694f25e-acd2-4a4e-bf71-7ce3d782f792", "There are no shipments on this voyage transiting in {0}.", Message.Port);

				default:
					return Res.GetString("bd86acbc-814d-4e83-932f-6f04a4098718", "The direction is not valid.");
			}
		}

		#endregion

		#region Validate

		void Validate(IEnumerable<BillOfLading> billOfLadings)
		{
			foreach (var shipment in billOfLadings)
			{
				DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(shipment.Factory);

				shipment.MarkAsNeedingValidationIncludingChildren();

				shipment.RunPreSaveValidation();

				if (shipment.HasMessageErrors)
				{
					Message.Issues.AddNew(shipment.PK, JobShipmentSchema.Constants.Prefix, Res.GetString("a4383e46-daad-483f-add7-c5acbd1984ff", "{0} has message errors.", shipment.JS_UniqueConsignRef), ExtractErrorDetail(shipment));
				}

				if (shipment.HasErrors)
				{
					Message.Issues.AddNew(shipment.PK, JobShipmentSchema.Constants.Prefix, Res.GetString("b6f34806-4524-436a-aa7d-2638b26b0b9d", "{0} has errors.", shipment.JS_UniqueConsignRef), ExtractErrorDetail(shipment));
				}
			}
		}

		static string ExtractErrorDetail(BusinessObject obj)
		{
			var notifications = new ZNotificationCollector(obj, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName);
			var errors = notifications.GetErrors();
			var messageErrors = notifications.GetMessageErrors();

			return errors.Concat(messageErrors).ToUniqueMessageListString().Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine);
		}

		#endregion

		#region Send Message

		void SendMessage(BusinessObjectFactory factory, IEnumerable<BillOfLading> billOfLadings, INotifications logger)
		{
			using (var exporter = new ManualDataExport(factory, billOfLadings, UniversalDataType.UniversalShipment, Message.CommunicationModes, UniversalXmlSchema.Version_2012_11_DO_NOT_USE, dataWriterGetter: GetDataWriterGetter()))
			{
				var eventType = GetEventType();

				exporter.EventCode = eventType.Code;
				exporter.EventReference = StmALog.GenerateEventReference("", GetEventParameters());
				exporter.RecipientType = GetRecipientType();

				exporter.SendData(logger);

				var voyageInMessagingFactory = factory.Load<JobVoyage>(Message.Voyage.PK);
				voyageInMessagingFactory.Logs.AddNew(eventType, exporter.EventReference);
			}
		}

		string GetRecipientType()
		{
			switch (Message.Direction)
			{
				case Constants.PortDirection.Load:
					return nameof(RecipientRoleType.PEM);

				case Constants.PortDirection.Discharge:
					return nameof(RecipientRoleType.PIM);

				case Constants.PortDirection.Transit:
					return nameof(RecipientRoleType.PTM);

				default:
					return string.Empty;
			}
		}

		Event GetEventType()
		{
			switch (Message.MessageType)
			{
				case DangerousGoodsManifestMessageTypeList.Codes.Original:
				case DangerousGoodsManifestMessageTypeList.Codes.Amendment:
					return AutoEvents.MessageSent;

				case DangerousGoodsManifestMessageTypeList.Codes.Withdrawal:
					return AutoEvents.MessageWithdrawCancelRequest;

				default:
					return null;
			}
		}

		KeyValuePair<string, string>[] GetEventParameters()
		{
			var parameters = new Dictionary<string, string>();
			parameters[Params.Location] = Message.Port;
			parameters[Params.MessageType] = GetDangerousGoodMessageType();
			parameters[Params.Department] = Params.Department.AsKeyFor((NoResString)"Terminal").Value;

			return parameters.ToArray();
		}

		ZString GetDangerousGoodMessageType()
		{
			switch (Message.Direction)
			{
				case Constants.PortDirection.Load:
					switch (Message.MessageType)
					{
						case DangerousGoodsManifestMessageTypeList.Codes.Original:
							return Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifest;
						case DangerousGoodsManifestMessageTypeList.Codes.Amendment:
							return Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifestReplacement;
						case DangerousGoodsManifestMessageTypeList.Codes.Withdrawal:
							return Constants.EventReferenceMessageTypes.DangerousGoodsLoadManifestCancellation;
						default:
							return ZString.Empty;
					}
				case Constants.PortDirection.Transit:
					switch (Message.MessageType)
					{
						case DangerousGoodsManifestMessageTypeList.Codes.Original:
							return Constants.EventReferenceMessageTypes.DangerousGoodsTransitManifest;
						case DangerousGoodsManifestMessageTypeList.Codes.Amendment:
							return Constants.EventReferenceMessageTypes.DangerousGoodsTransitManifestReplacement;
						case DangerousGoodsManifestMessageTypeList.Codes.Withdrawal:
							return Constants.EventReferenceMessageTypes.DangerousGoodsTransitManifestCancellation;
						default:
							return ZString.Empty;
					}
				case Constants.PortDirection.Discharge:
					switch (Message.MessageType)
					{
						case DangerousGoodsManifestMessageTypeList.Codes.Original:
							return Constants.EventReferenceMessageTypes.DangerousGoodsDischargeManifest;
						case DangerousGoodsManifestMessageTypeList.Codes.Amendment:
							return Constants.EventReferenceMessageTypes.DangerousGoodsDischargeManifestReplacement;
						case DangerousGoodsManifestMessageTypeList.Codes.Withdrawal:
							return Constants.EventReferenceMessageTypes.DangerousGoodsDischargeManifestCancellation;
						default:
							return ZString.Empty;
					}
				default:
					return ZString.Empty;
			}
		}

		Func<IDataWritingManager, ITopLevelDataObjectWriter> GetDataWriterGetter()
		{
			return dataWritingManager => new DangerousGoodsManifestDataObjectWriter(dataWritingManager, Message.GetSenderID(), Message.Port, Message.Direction, Message.MessageType);
		}

		#endregion

		#region Override

		public override string FormVerb => string.Empty;

		#endregion

		#region Nested Types

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
