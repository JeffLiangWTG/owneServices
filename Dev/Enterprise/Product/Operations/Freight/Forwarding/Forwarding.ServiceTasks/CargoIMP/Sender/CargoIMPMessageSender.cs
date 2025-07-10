using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.ServiceTasks.CargoIMP
{
	public class CargoIMPMessageSender
	{
		#region New

		protected CargoIMPMessageSender(ILogger logger)
		{
			Logger = Argument.NotNull(logger, "logger");
		}

		protected readonly ILogger Logger;

		public static CargoIMPMessageSender New(ILogger logger)
		{
			CargoIMPMessageSender result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(logger);
			}
			else
			{
				result = new CargoIMPMessageSender(logger);
			}

			return result;
		}

		protected delegate CargoIMPMessageSender NewDelegate(ILogger logger);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Process

#if DEBUG

		public void Process()
		{
			Process(CancellationToken.None);
		}

#endif

		public void Process(CancellationToken token)
		{
			ProcessCore(token);
		}

		const int BatchSize = 50;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Factory Name")]
		protected virtual void ProcessCore(CancellationToken token)
		{
			if (GlbCompany.CurrentCompany == null)
			{
				Logger.Log(LogType.Error, "No logged in company found.");
			}
			else
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(EDIInterchangeSchema.EI_Status, EDIMessage.Status.Queued);
				filter.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				filter.AddToFilter(EDIInterchangeSchema.EI_IsActive, ZBool.True);
				filter.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCode);
				filter.AddToFilter(EDIInterchangeSchema.EI_GB, GlbCompany.CurrentCompany.Branches.Select(branch => branch.PK));

				filter.OrderBy = EDIInterchangeSchema.Constants.EI_InterchangeNum;
				filter.MaximumRows = BatchSize;

				const string factoryName = "CargoIMP Message Sender";

				BusinessObjectFactory factory = new BusinessObjectFactory() { NameForDebugging = factoryName };
				EDIInterchange[] interchanges = factory.Load<EDIInterchange>(filter);

				while (interchanges.Length > 0)
				{
					token.ThrowIfCancellationRequested();
					ProcessInterchanges(interchanges, factory);

					try
					{
						factory.Save();
					}
					catch (ZSaveException ex)
					{
						Logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "At least one interchange in this batch has failed. Each interchange in this batch will now be processed again individually.\r\n{0}", ex.ToString()));
						LogQueue.Clear();

						ZGuid[] interchangesPKs = interchanges.Select(i => i.PK).ToArray();
						ProcessInterchangesIndividually(interchangesPKs);
					}

					AddLogsFromProcessingInterchangesLogQueue();

					factory = new BusinessObjectFactory() { NameForDebugging = factoryName };
					interchanges = factory.Load<EDIInterchange>(filter);
				}
			}
		}

		protected virtual string ApplicationCode
		{
			get { return EDIInterchange.ApplicationCodes.CIM; }
		}

		protected virtual string GetRegistryServiceProvider(EDIInterchange interchange)
		{
			return ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.GetFallBackValueAtAllLevels(interchange.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}

		void ProcessInterchanges(IEnumerable<EDIInterchange> interchanges, BusinessObjectFactory factory)
		{
			foreach (EDIInterchange interchange in interchanges)
			{
				using (DisposableEnvironment.ForBranch(interchange.EI_GB.ToGuid()))
				{
					LogQueue.Add(new LogEntry(LogType.Information, Res.GetString("498F2672-E395-4D4C-9895-9F0D9F1A7FCA", "Switched to branch: {0}.", GlbBranch.CurrentBranch.HumanReadableNameForRegistry)));

					if (!GlbBranch.CurrentBranch.GB_IsActive)
					{
						UpdateInterchangeAndMessageStatus(interchange, EDIInterchange.Status.Failed, EDIMessage.Status.Failed);
						LogQueue.Add(new LogEntry(LogType.Error, Res.GetString("9E752BE8-FE93-4A3E-A9B9-F309CDF96A64", "Message with Interchange #{0} has an inactive branch and will be ignored and marked as failed.", interchange.EI_InterchangeNum)));
					}
					else
					{
						LogQueue.Add(new LogEntry(LogType.Information, Res.GetString("8f9f5f6b-21a4-4a6f-a3dc-cedeb52c3d43", "Attempting to process Interchange #{0}", interchange.EI_InterchangeNum)));

						try
						{
							SetTransmissionMethodBasedHeaderFooter(interchange);
							if (interchange.EI_Status.Equals(EDIInterchange.Status.eHubQueued))
							{
								LogQueue.Add(new LogEntry(LogType.Information, Res.GetString("958b9bb5-95bc-42e9-914c-5749a50e2dad", "Message with Interchange #{0} is queued for eHub.", interchange.EI_InterchangeNum)));
							}
							else if (interchange.EI_Status.Equals(EDIInterchange.Status.eAdaptorQueued))
							{
								if (eAdaptorRegistry.Instance.CanSendCargoImpMessagesThroughEAdaptor.Value)
								{
									LogQueue.Add(new LogEntry(LogType.Information, Res.GetString("e199ebcd-ea2c-440f-a83a-302c06fc9e43", "Message with Interchange #{0} is queued for eAdaptor.", interchange.EI_InterchangeNum)));
								}
								else
								{
									UpdateInterchangeAndMessageStatus(interchange, EDIInterchange.Status.Failed, EDIMessage.Status.Failed);
									LogQueue.Add(new LogEntry(LogType.Error, Res.GetString("7813f8b8-9618-47eb-8d85-da772ac8d181", "eAdaptor SDK license does not work with CargoIMP messages. The message with Interchange #{0} will be ignored and marked as failed.", interchange.EI_InterchangeNum)));
								}
							}
							else if (interchange.EI_Status != EDIInterchange.Status.Failed)
							{
								ProcessEDIInterchange(interchange, factory);
								SetMessagesAsSent(interchange);
							}
						}
						catch (CargoIMPApplicationException ex)
						{
							LogQueue.Add(new LogEntry(LogType.Error, Res.GetString("82d7b3b2-b098-4d28-9abd-3341c1ff891e", "Sending of Interchange #{0} has failed.\r\n{1}", interchange.EI_InterchangeNum, ex.Message)));
							SetMessagesAsFailed(interchange);
						}
					}
				}
			}
		}

		void ProcessInterchangesIndividually(ZGuid[] interchangesPKs)
		{
			foreach (ZGuid interchangePK in interchangesPKs)
			{
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				EDIInterchange interchange = newFactory.Load<EDIInterchange>(interchangePK);

				ProcessInterchanges(new EDIInterchange[] { interchange }, newFactory);

				try
				{
					newFactory.Save();
					AddLogsFromProcessingInterchangesLogQueue();
				}
				catch (ZSaveException ex)
				{
					BusinessObjectFactory factoryForSavingExceptions = new BusinessObjectFactory();
					EDIInterchange reloadedInterchange = factoryForSavingExceptions.Load<EDIInterchange>(interchange.PK);
					SetMessagesAsFailed(reloadedInterchange);
					factoryForSavingExceptions.Save();

					LogQueue.Clear();
					LogQueue.Add(new LogEntry(LogType.Error, ex.ToString()));
					LogQueue.Add(new LogEntry(LogType.Error, Res.GetString("18654863-001b-4dff-8925-955c75efa864", "Sending of Interchange #{0} has failed. This interchange and all messages attached to it have been failed.", interchange.EI_InterchangeNum)));
				}
			}
		}

		#region Processing Interchanges Log List

		void AddLogsFromProcessingInterchangesLogQueue()
		{
			if (LogQueue.Any())
			{
				foreach (LogEntry currentLog in LogQueue)
				{
					Logger.Log(currentLog.LogType, currentLog.Message);
				}

				LogQueue.Clear();
			}
		}

		readonly List<LogEntry> LogQueue = new List<LogEntry>();

		class LogEntry
		{
			public LogEntry(LogType logType, ZString message)
			{
				LogType = logType;
				Message = message;
			}

			public LogType LogType { get; set; }
			public ZString Message { get; set; }
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CargoIMP Mail Subject")]
		protected virtual void ProcessEDIInterchange(EDIInterchange interchange, BusinessObjectFactory factory)
		{
			EmailDef emailDef = new EmailDef();
			emailDef.AddRecipientForSystemCommunication(CargoIMPServiceProvider.CargoWiseCargoImpEmailAddress);
			emailDef.Subject = GetEmailSubject(interchange);
			emailDef.Body = emailDef.Subject + " From: " + interchange.Branch.GB_BranchName;

			byte[] attachmentData = Encoding.ASCII.GetBytes(interchange.EI_HeaderText + interchange.EI_BodyText + interchange.EI_FooterText);
			emailDef.Attachments.Add(new AttachmentDef(AttachmentFileName, attachmentData));

			Env.OutgoingMailManager.Create(factory, emailDef);
		}

		protected virtual string AttachmentFileName
		{
			get { return "FWBMessage Interchange.EI_InterchangeNum.txt"; } // CargoIMP Attachment File Name
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CargoIMP Mail Subject")]
		string GetEmailSubject(EDIInterchange interchange)
		{
			string result = "EAGLE FWB EDI - LIVE - ";
#if DEBUG
			result = "EAGLE FWB EDI - TEST - ";
#endif
			result += interchange.EI_From;

			if (!IsGoingViaCCN)
			{
				result += " - " + ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.Value;
			}

			return result;
		}

		protected virtual bool IsGoingViaCCN => CargoIMPServiceProvider.Get(ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.Value).IsGoingViaCCN;

		#endregion

		#region Status

		void SetMessagesAsSent(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Sent;
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessage.Status.Sent;
			}
		}

		void SetMessagesAsFailed(EDIInterchange interchange)
		{
			interchange.EI_Status = EDIInterchange.Status.Failed;
			foreach (EDIMessage message in interchange.ContainedMessages)
			{
				message.EM_Status = EDIMessage.Status.Failed;
			}
		}

		#endregion

		#region Transmission Method

		void SetTransmissionMethodBasedHeaderFooter(EDIInterchange interchange)
		{
			if (interchange.ContainedMessages.Count > 0)
			{
				var message = interchange.ContainedMessages[0];
				var linkedParent = interchange.ContainedMessages[0].EM_LinkedObject;

				var consol = linkedParent as ForwardingConsol;
				if (consol == null && linkedParent is IHVLVConsignment hvlvConsignment)
				{
					var shipment = linkedParent.Factory.Load<ForwardingShipment>(hvlvConsignment.HVC_JS_ManifestedOnShipment);
					consol = shipment?.LocalConsol;
				}

				if (consol != null)
				{
					if (consol.AWBHeader == null)
					{
						LogQueue.Add(new LogEntry(LogType.Error, Res.GetString("fd5a8e6c-abde-e0bb-48fb-a7a4e8ef60b3", "Sending of Interchange #{0} has failed.\r\n{1} Consol ({2}) does not contain an Air Waybill.", interchange.EI_InterchangeNum, consol.JK_TransportMode, consol.JK_UniqueConsignRef)));
						SetMessagesAsFailed(interchange);
						return;
					}
					else
					{
						var registryServiceProvider = GetRegistryServiceProvider(interchange);
						if (registryServiceProvider == Constants.AWB.CargoIMPServiceProviderConstants.HUB || registryServiceProvider == Constants.AWB.CargoIMPServiceProviderConstants.EDP)
						{
							var provider = CargoIMPServiceProvider.Get(registryServiceProvider);
							interchange.EI_HeaderText = provider.GetTransmissionMethodHeader(consol.AWBHeader, message, GetIMPTransmissionMethod(registryServiceProvider));
							interchange.EI_FooterText = provider.GetTransmissionMethodFooter(consol.AWBHeader, message, GetIMPTransmissionMethod(registryServiceProvider));

							if (registryServiceProvider == Constants.AWB.CargoIMPServiceProviderConstants.HUB)
							{
								ModifyInterchange(interchange, "eHubAirService", EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, EDIMessage.Status.Sent);
							}

							if (registryServiceProvider == Constants.AWB.CargoIMPServiceProviderConstants.EDP)
							{
								ModifyInterchange(interchange, "", EDIInterchange.Status.eAdaptorQueued, EDIInterchange.TransportType.eAdaptor, EDIMessage.Status.Sent);
							}
						}
						else
						{
							var eHubCommunicationsModes = consol.ShippingLine == null ? Array.Empty<EDICommunicationsMode>()
								: consol.ShippingLine.EDICommunicationsModes.FindByModuleAndFileFormat(JobInvoicingConsumerTypes.Consol.Code, interchange.ContainedMessages[0].EM_MessageType);

							var transmissionMethod = GetTransmissionMethod(interchange, consol, eHubCommunicationsModes);

							if (transmissionMethod.Equals(CargoIMPTransmissionMethod.eHub))
							{
								string eHubClientRecipient = eHubCommunicationsModes.Length > 0 ? eHubCommunicationsModes[0].EK_Destination : ZString.Empty;
								ModifyInterchange(interchange, eHubClientRecipient, EDIInterchange.Status.eHubQueued, EDIInterchange.TransportType.eHub, EDIMessage.Status.Sent);
							}

							if (transmissionMethod.Equals(CargoIMPTransmissionMethod.eAdaptor))
							{
								string eHubClientRecipient = eHubCommunicationsModes.Length > 0 ? eHubCommunicationsModes[0].EK_Destination : ZString.Empty;
								ModifyInterchange(interchange, eHubClientRecipient, EDIInterchange.Status.eAdaptorQueued, EDIInterchange.TransportType.eAdaptor, EDIMessage.Status.Sent);
							}

							CargoIMPServiceProvider provider = CargoIMPServiceProvider.Get(GetServiceProvider(interchange, eHubCommunicationsModes));
							interchange.EI_HeaderText = provider.GetTransmissionMethodHeader(consol.AWBHeader, message, transmissionMethod);
							interchange.EI_FooterText = provider.GetTransmissionMethodFooter(consol.AWBHeader, message, transmissionMethod);
						}
					}
				}

				var awbHeader = linkedParent as AWB.Business.ExportAWBHeader;
				if (awbHeader != null)
				{
					for (; awbHeader.ParentBill != null && awbHeader.ParentBill != awbHeader; awbHeader = awbHeader.ParentBill)
					{ }

					var provider = CargoIMPServiceProvider.Get(ForwardingConfigurationRegistry.Instance.CargoIMPServiceProvider.Value);
					var transmissionMethod = CargoIMPTransmissionMethod.SMTP;

					interchange.EI_HeaderText = provider.GetTransmissionMethodHeader(awbHeader, message, transmissionMethod);
					interchange.EI_FooterText = provider.GetTransmissionMethodFooter(awbHeader, message, transmissionMethod);
				}
			}
		}

		static void ModifyInterchange(EDIInterchange interchange, string eHubClientRecipient, string interchangeStatus, string interchangeTransportType, string messageStatus)
		{
			interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			interchange.EI_To = eHubClientRecipient;
			interchange.EI_SessionGUID = interchange.PK;
			interchange.EI_TransportType = interchangeTransportType;
			UpdateInterchangeAndMessageStatus(interchange, interchangeStatus, messageStatus);
		}

		static void UpdateInterchangeAndMessageStatus(EDIInterchange interchange, string interchangeStatus, string messageStatus)
		{
			interchange.EI_Status = interchangeStatus;
			Array.ForEach(interchange.ContainedMessages.ToArray<EDIMessage>(), (EDIMessage message) => { message.EM_Status = messageStatus; });
		}

		CargoIMPTransmissionMethod GetTransmissionMethod(EDIInterchange interchange, ForwardingConsol consol, EDICommunicationsMode[] modes)
		{
			if (modes.Length > 0)
			{
				if (modes[0].EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EHubService)
				{
					return CargoIMPTransmissionMethod.eHub;
				}

				if (modes[0].EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface)
				{
					return CargoIMPTransmissionMethod.eAdaptor;
				}
			}

			return GetTransmissionMethodCore(interchange, consol);
		}

		string GetServiceProvider(EDIInterchange interchange, EDICommunicationsMode[] modes)
		{
			if (modes.Length > 0)
			{
				return Core.Constants.AWB.CargoIMPServiceProviderConstants.IATA;
			}

			return GetRegistryServiceProvider(interchange);
		}

		protected virtual CargoIMPTransmissionMethod GetTransmissionMethodCore(EDIInterchange interchange, ForwardingConsol consol)
		{
			return CargoIMPTransmissionMethod.SMTP;
		}

		protected virtual CargoIMPTransmissionMethod GetIMPTransmissionMethod(string serviceProvider)
		{
			if (serviceProvider == Constants.AWB.CargoIMPServiceProviderConstants.HUB)
			{
				return CargoIMPTransmissionMethod.eHub;
			}

			if (serviceProvider == Constants.AWB.CargoIMPServiceProviderConstants.EDP)
			{
				return CargoIMPTransmissionMethod.eAdaptor;
			}

			return CargoIMPTransmissionMethod.SMTP;
		}

		#endregion
	}
}
