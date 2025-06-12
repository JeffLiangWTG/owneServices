using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Common.Logging;
using IBM.WMQ;
using ServiceBroker.Common;
using ServiceBroker.Interface;
using CargoWise.eServices.USCustoms.Common;
using CargoWise.eServices.USCustoms.Integration;
using CargoWise.eServices.USCustoms.MQConfiguration;

namespace CargoWise.eServices.USCustoms.InboundService
{
	public partial class InboundService : ServiceBase
	{
		internal ManualResetEventSlim StopEvent { get; set; }
		internal virtual bool StopEventSet { get { return this.StopEvent.IsSet; } }
		internal Task Task { get; set; }
		internal virtual IInboundServiceConfiguration Config { get; set; }
		internal virtual ILog Logger { get; set; }
		internal virtual ILog ReceivedMessageLog { get; set; }
		internal virtual IMQMessageRetriever MessageRetriever { get; set; }
		const string INPUTPATH_OPT = "-inputpath";
		readonly string[] COMMAND_LINE_OPTIONS = new[] { INPUTPATH_OPT };
		public Func<bool, IMQConfiguration> GetInboundConfiguration;
		public InboundService(string serviceName)
		{
			InitializeComponent();
			base.ServiceName = serviceName;
			this.StopEvent = new ManualResetEventSlim();
		}

		protected override void OnStart(string[] args)
		{
			StartTask(args);
		}

		protected override void OnStop()
		{
			StopTask();
		}

		public void StartTask(string[] args)
		{
			try
			{
				this.Task = Task.Factory.StartNew(ServiceTask, TaskCreationOptions.LongRunning);
			}
			catch (Exception ex)
			{
				WriteStartupErrorLogEntry("Error starting service", ex);
				throw;
			}
		}

		internal void StopTask()
		{
			this.StopEvent.Set();
			try
			{
				this.Task.Wait(20000);
			}
			catch (AggregateException ex)
			{
				foreach (var innerEx in ex.InnerExceptions)
				{
					WriteStartupErrorLogEntry("Error running service task", innerEx);
				}
			}
		}

		internal void ServiceTask()
		{
			try
			{
				ResolveDependencies();
				this.Logger.Debug("Starting service '" + base.ServiceName + "'");

				do
				{
					try
					{
						this.Logger.Debug("Starting pull cycle");
						ProcessMessages();
					}
					catch (MQException mqException)
					{
						LogExceptionFromPullCycle(mqException);
						DisposeMQMessageRetriever();
						InitialiseMQMessageRetriever();
					}
					catch (Exception ex)
					{
						LogExceptionFromPullCycle(ex);
					}
				} while (!this.StopEvent.Wait(this.Config.PullIntervalInSecond * 1000));

				this.Logger.Debug("Stopping service '" + base.ServiceName + "'");
			}
			catch (Exception exception)
			{
				if (Logger != null)
				{
					Logger.Error(exception);
				}

				Task.Factory.StartNew(Stop);
				throw;
			}
		}

		internal virtual void ResolveDependencies()
		{
			this.Logger = LogManager.GetLogger(this.GetType());
			this.ReceivedMessageLog = LogManager.GetLogger("ReceivedMessageLog");
			this.Config = new InboundServiceConfiguration();

			if (this.Config.SendCopiesToTest && this.Config.IsProduction)
				this.Logger.Warn("SendCopiesToTest config option is invalid for production queues and will be ignored.");

			var cmdLineOpts = GetCommandLineOptions(Environment.GetCommandLineArgs().Skip(1).ToList());
			if (cmdLineOpts.Count > 0 && !Environment.UserInteractive)
			{
				this.Logger.Warn("Command line options are not allowed when running as a service. Options will be ignored.");
				cmdLineOpts.Clear();
			}

			if (cmdLineOpts.ContainsKey(INPUTPATH_OPT))
			{
				this.MessageRetriever = new FileSystemMessageRetriever(cmdLineOpts[INPUTPATH_OPT], this.Config, this.Logger);
			}
			else
			{
				//var inboundConfig = ConfigurationHelper<IMQConfiguration>.GetInboundConfiguration(this.Config.MessageType, this.Config.IsProduction);
				if (GetInboundConfiguration == null)
					throw new ApplicationException(string.Format("Message Type '{0}' for type '{1}' is not supported", this.Config.MessageType, typeof(IMQConfiguration).Name));
				var inboundConfig = GetInboundConfiguration(this.Config.IsProduction);
				InitialiseMQMessageRetriever();
				this.Logger.InfoFormat(string.Format("Initialised MQ message retriever. Host: {0}, Port {1}, QMgr {2}, QName {3}, Channel {4}.", inboundConfig.Hostname, inboundConfig.Port, inboundConfig.QueueManager, inboundConfig.QueueName, inboundConfig.Channel));
			}
		}

		internal virtual void InitialiseMQMessageRetriever()
		{
			//MessageRetriever = new MQMessageRetriever(ConfigurationHelper<IMQConfiguration>.GetInboundConfiguration(this.Config.MessageType, this.Config.IsProduction), new ServiceLogger(this.Logger), new ServiceLogger(this.ReceivedMessageLog), this.Config.MessageType);
			if (GetInboundConfiguration == null)
				throw new ApplicationException(string.Format("Message Type '{0}' for type '{1}' is not supported", this.Config.MessageType, typeof(IMQConfiguration).Name));
			var inboundConfig = GetInboundConfiguration(this.Config.IsProduction);
			MessageRetriever = new MQMessageRetriever(inboundConfig, this.Logger, this.ReceivedMessageLog, this.Config.MessageType);
		}

		void DisposeMQMessageRetriever()
		{
			if (MessageRetriever == null) return;

			MessageRetriever.Dispose();
			MessageRetriever = null;
			this.Logger.DebugFormat("Disposed MQ message retriever.");
		}

		internal virtual void ProcessMessages()
		{
			using (var connection = GetConnection())
			{
				connection.Open();
				try
				{
					while (!this.StopEventSet)
					{
						this.MessageRetriever.BeginTransaction();
						this.Logger.Debug("Began a new MQ transaction");
						using (var messageStream = RetrieveMQMessage(connection))
						{
							if (messageStream == null || this.StopEventSet)
							{
								this.Logger.Debug("No message retrieved from MQ");
								this.MessageRetriever.RollbackTransaction();
								this.Logger.Debug("Rollbacked MQ transaction");
								break;
							}

							this.Logger.Debug("Retrieved message from MQ");

							try
							{
								using (var transaction = connection.BeginTransaction())
								{
									this.Logger.Debug("Began a new eHub transaction");
									var message = new Message(ServiceBrokerConstants.eHubInboxServiceConstants.InboundCustomsMessageType, messageStream);

									var initialAndToService = SendMessageToEHub(message, connection, transaction);
									this.Logger.Debug(string.Format("Sent message from {0} to {1}", initialAndToService.Item1, initialAndToService.Item2));
									transaction.Commit();
									this.Logger.Debug("Committed eHub transaction");
								}
								this.MessageRetriever.CommitTransaction();
								this.Logger.Debug("Committed MQ transaction");
								this.Logger.Info("Processed message successfully");
							}
							catch (Exception ex)
							{
								this.Logger.Error("Failed to process message. ", ex);
								throw;
							}

							if (this.Config.SendCopiesToTest)
							{
								try
								{
									using (var transaction = connection.BeginTransaction())
									{
										this.Logger.Debug("Began a new eHub transaction to send message copy to test");
										messageStream.Position = 0;
										var message = new Message(ServiceBrokerConstants.eHubInboxServiceConstants.InboundCustomsMessageType, messageStream);
										SendCopyMessageToTest(message, connection, transaction);
										this.Logger.Debug("Sent message copy to test");
										transaction.Commit();
										this.Logger.Debug("Committed eHub transaction");
									}
								}
								catch (Exception ex)
								{
									this.Logger.Warn("Failed to send message copy to test.", ex);
								}
							}
						}
					}
					this.Logger.Debug(this.StopEventSet ? "Message retrieval stopped." : "No new messages.");
				}
				catch (Exception ex)
				{
					this.MessageRetriever.RollbackTransaction();
					this.Logger.Debug("Rollbacked MQ transaction");
					this.Logger.ErrorFormat("MessageType: {0}, IsProduction: {1}.", ex, this.Config.MessageType, this.Config.IsProduction);
					if (!this.StopEventSet)
						SendErrorToEHub(connection, ex.ToString());
				}
			}
		}

		internal virtual Stream RetrieveMQMessage(SqlConnection connection)
		{
			do
			{
				try
				{
					return this.MessageRetriever.Retrieve();
				}
				catch (Exception ex)
				{
					if (this.StopEventSet)
						throw;
					var fmqEx = ex as FailedMQMessageException;
					var mqEx = ex as MQException;
					if (mqEx != null)
					{
						this.Logger.WarnFormat("MQException ReasonCode: {0}, Reason: {1}, CompCode: {2}, CompletionCode: {3}. Message: {4}", mqEx.ReasonCode, mqEx.Reason, mqEx.CompCode, mqEx.CompletionCode, mqEx.Message);
						if (this.Config.PoisonMessageMQErrorCode.Contains(mqEx.ReasonCode.ToString()))
						{
							this.Logger.WarnFormat("Error code was in PoisonMessageMQErrorCode configuration list. Error details  Code {0} - {1}.", mqEx.ReasonCode, mqEx.Message);
							this.MessageRetriever.RollbackTransaction();
							this.Logger.Warn("Rollbacked MQ transaction");
							this.MessageRetriever.BeginTransaction();
							this.Logger.Warn("Began a new MQ transaction");
							try
							{
								this.MessageRetriever.RetrieveSafe(); // throws FailedMQMessageException if successfully retrieves message(s).
								this.Logger.Warn("Failed to retrieve message from MQ using RetrieveSafe()");
								throw;
							}
							catch (FailedMQMessageException rsEx)
							{
								this.Logger.Warn("Retrieved message from MQ using RetrieveSafe()");
								fmqEx = rsEx as FailedMQMessageException;
							}
						}
					}
					if (fmqEx != null)
					{
						this.Logger.WarnFormat("Error during message pulling from MQ. {0} messages will be moved to dead letter queue.", fmqEx, fmqEx.MQMessageNumber);
						try
						{
							GetMQDeadMessageSender().Send(fmqEx.MQMessage, this.Config.IsProduction);
							this.Logger.Warn("Message(s) moved to dead letter queue.");
							continue;
						}
						catch (Exception dmEx)
						{
							this.Logger.WarnFormat("Unable send {0} to dead letter queue. Message contents follow.", dmEx, fmqEx.MQMessageNumber);
							try
							{
								fmqEx.MQMessage.ToList().ForEach(m => this.Logger.Warn(GetMQMessageText(m)));
							}
							catch (Exception saveEx)
							{
								this.Logger.Error("Unable to save message contents. Messages will be removed.", saveEx);
								throw;
							}
							return null;
						}
						finally
						{
							this.MessageRetriever.CommitTransaction();
							this.Logger.Warn("Committed MQ transaction");
							this.MessageRetriever.BeginTransaction();
							this.Logger.Warn("Began a new MQ transaction");
							SendErrorToEHub(connection, ex.ToString());
						}
					}
					throw;
				}
			} while (!this.StopEventSet);

			return null;
		}

		internal virtual string GetMQMessageText(MQMessage message)
		{
			using (var ms = new MemoryStream())
			{
				int bufferSize = 32000;
				for (int i = 0; i < message.MessageLength; i += bufferSize)
				{
					int count = Math.Min(bufferSize, message.MessageLength - i);
					ms.Write(message.ReadBytes(count), 0, count);
				}
				ms.Position = 0;
				using (var sr = new StreamReader(ms))
				{
					return sr.ReadToEnd();
				}
			}
		}

		internal virtual void SendErrorToEHub(SqlConnection connection, string errorDescription)
		{
			try
			{
				using (var messageStream = new MemoryStream())
				using (var transaction = connection.BeginTransaction())
				{
					var writer = XmlWriter.Create(messageStream, new XmlWriterSettings() { OmitXmlDeclaration = true });
					writer.WriteStartElement(Constants.MessageAttributes.ErrorNotification);
					writer.WriteAttributeString(Constants.MessageAttributes.ErrorDescription, errorDescription);
					writer.WriteEndElement();
					writer.Flush();
					messageStream.Position = 0;
					var message = new Message(ServiceBrokerConstants.eHubInboxErrorServiceConstants.InboundCustomsErrorMessageType, messageStream);
					SendErrorMessageToEHub(message, connection, transaction);
					transaction.Commit();
				}
			}
			catch (Exception ex)
			{
				this.Logger.Error("Error sending error to eHub.", ex);
			}
		}

		internal virtual Tuple<string, string> SendMessageToEHub(Message message, SqlConnection connection, SqlTransaction transaction)
		{
			var isProd = Config.IsProduction;
			var initiatorService = new eHubInboxService(connection, transaction, isProd);
			var toServiceName = isProd ? ServiceBrokerConstants.InboundMessageProcessingServiceConstants.ServiceName : ServiceBrokerConstants.InboundMessageProcessingServiceTestConstants.ServiceName;

			var conversation = new DialogueManager().GetClientDialogue(ServiceBrokerConstants.USCustoms.USCustomsClientId,
				initiatorService,
				toServiceName,
				ServiceBrokerConstants.InboundMessageProcessingServiceConstants.ContractName,
				connection, transaction);

			conversation.Send(message, connection, transaction);

			var initiatorServiceName = conversation.Service.Name;

			return new Tuple<string, string>(initiatorServiceName, toServiceName);
		}

		internal virtual void SendErrorMessageToEHub(Message message, SqlConnection connection, SqlTransaction transaction)
		{
			var isProd = Config.IsProduction;
			var initiatorService = new eHubInboxErrorService(connection, transaction, isProd);
			var toServiceName = isProd ? ServiceBrokerConstants.ErrorProcessingServiceConstants.ServiceName : ServiceBrokerConstants.ErrorProcessingServiceTestConstants.ServiceName;

			var conversation = new DialogueManager().GetClientDialogue(ServiceBrokerConstants.USCustoms.USCustomsClientId,
				initiatorService,
				toServiceName,
				ServiceBrokerConstants.ErrorProcessingServiceConstants.ContractName,
				connection, transaction);

			conversation.Send(message, connection, transaction);
		}

		internal virtual void SendCopyMessageToTest(Message message, SqlConnection connection, SqlTransaction transaction)
		{
			new DialogueManager().GetDialogue(new eHubInboxService(connection, transaction, true),
				ServiceBrokerConstants.InboundMessageProcessingServiceConstants.ServiceName,
				this.Config.TestBrokerInstanceID,
				ServiceBrokerConstants.InboundMessageProcessingServiceConstants.ContractName,
				connection, transaction)
			.Send(message, connection, transaction);
		}

		internal virtual IMQDeadMessageSender GetMQDeadMessageSender()
		{
			return new MQDeadMessageSender();
		}

		internal virtual SqlConnection GetConnection()
		{
			return new SqlConnection(this.Config.EHubConnectionString);
		}

		internal Dictionary<string, string> GetCommandLineOptions(List<string> args)
		{
			var options = new Dictionary<string, string>();
			try
			{
				while (args.Count > 0 && args[0].StartsWith("-"))
				{
					if (COMMAND_LINE_OPTIONS.Contains(args[0]))
					{
						if (args.Count < 2 || args[1].StartsWith("-"))
							throw new ArgumentException(String.Format("Missing value for command line option '{0}'.", args[0]));
						options.Add(args[0], args[1]);
						args.RemoveRange(0, 2);
					}
					else
					{
						throw new ArgumentException(String.Format("Invalid command line option '{0}'.", args[0]));
					}
				}
			}
			catch (Exception ex)
			{
				this.Logger.ErrorFormat(ex.Message + " Options will be ignored.\r\nCommand Line: " + Environment.CommandLine);
				options.Clear();
			}
			return options;
		}

		internal virtual void WriteStartupErrorLogEntry(string message, Exception ex = null)
		{
			if (this.Logger != null)
			{
				LogErrorWithExSafe(message, ex);
			}
			else
			{
				if (ex != null) message += Environment.NewLine + ex.ToString();
				EventLog.WriteEntry(message, EventLogEntryType.Error);
			}
		}

		void LogExceptionFromPullCycle(Exception exception)
		{
			LogErrorWithExSafe("Exception happened in current pull cycle. Waiting for the next pull cycle.", exception);
		}

		void LogErrorWithExSafe(string message, Exception ex)
		{
			if (ex == null)
			{
				this.Logger.Error(message);
			}
			else
			{
				message += Environment.NewLine + ex.ToString(); //Combine exception into message. Sometimes "void ILog.Error(object message, Exception exception)" does not log exception type and stack trace.
				this.Logger.Error(message, ex); //Store ex object in the logging event to enable usage of %exception% pattern layout.
			}
		}
	}
}
