using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using Enterprise.Customs.FR.TransportSvc.Messages;
using Enterprise.Customs.FR.TransportSvc.TransportService.Framework;
using Enterprise.Customs.FR.TransportSvc.Utilities;

namespace Enterprise.Customs.FR.TransportSvc.TransportService
{
	/// <summary>
	/// The actual implementation of the windows service goes here...
	/// </summary>
	[WindowsService(
		"WTG.TransportService",
		DisplayName = "WTG.TransportService",
		Description = "The description of the WTG.TransportService service.",
		EventLogSource = "WTG.TransportService",
		StartMode = ServiceStartMode.Manual)]
	public class ServiceImplementation : IWindowsService
	{
		public void Dispose()
		{
			loopTimer.Dispose();
			startTimer.Dispose();
		}

		public void OnStart(string[] args)
		{
			logger.AddNotification(Notification.MessageType.Information, Notification.Events.ServiceStart, verboseModeOnly: false);
			nbLoginToeHubAttemptsLeft = MaxFailedLoginToeHubAttempts;
			var timerInterval = 1000;
			_ = int.TryParse(ApplicationConfig.Instance.TimerInterval, out timerInterval);
			startTimer.Elapsed += new ElapsedEventHandler(StartTimer_Tick);
			startTimer.Interval = timerInterval;
			startTimer.Enabled = true;
			startTimer.AutoReset = false;
		}

		public void OnStop()
		{
			logger.AddNotification("Stop attempt...", Notification.MessageType.Information, Notification.Events.ServiceStop, verboseModeOnly: false);
			while (isJobInProgress)
			{
				System.Threading.Thread.Sleep(150);
			}
			logger.AddNotification("Service has stopped.", Notification.MessageType.Information, Notification.Events.ServiceStop, verboseModeOnly: false);

			nbLoginToeHubAttemptsLeft = MaxFailedLoginToeHubAttempts;

			if (loopTimer != null)
			{
				loopTimer.Stop();
				loopTimer.Dispose();
			}
		}

		public void OnPause()
		{
			logger.AddNotification("Pause attempt...", Notification.MessageType.Information, Notification.Events.ServicePause, verboseModeOnly: false);
			while (isJobInProgress)
			{
				System.Threading.Thread.Sleep(1000);
			}

			nbLoginToeHubAttemptsLeft = MaxFailedLoginToeHubAttempts;

			if (loopTimer != null)
			{
				loopTimer.Stop();
				loopTimer.Dispose();
			}
			logger.AddNotification("Service paused.", Notification.MessageType.Information, Notification.Events.ServicePause, verboseModeOnly: false);
		}

		public void OnResume()
		{
			//Let's pause the service silently beore resuming the service
			while (isJobInProgress)
			{
				System.Threading.Thread.Sleep(1000);
			}

			nbLoginToeHubAttemptsLeft = MaxFailedLoginToeHubAttempts;

			if (loopTimer != null)
			{
				loopTimer.Stop();
				loopTimer.Dispose();
			}

			//Resume the service
			StartLoopTimer();
			logger.AddNotification("Service resumed.", Notification.MessageType.Information, Notification.Events.ServiceResume, verboseModeOnly: false);
		}

		public void OnShutdown()
		{
			logger.AddNotification("Service shutdown.", Notification.MessageType.Information, Notification.Events.ServiceShutdown, verboseModeOnly: false);
		}

		public void OnCustomCommand(int command)
		{
		}

		bool ManageDirectory(string directory)
		{
			if (!string.IsNullOrEmpty(directory))
			{
				if (!Directory.Exists(directory))
				{
					try
					{
						Directory.CreateDirectory(directory);
						logger.AddNotification("Directory " + directory + " created.", Notification.MessageType.Information, Notification.Events.ServiceStart, verboseModeOnly: true);
					}
					catch
					{
						logger.AddNotification("Directory " + directory + " could not be created.", Notification.MessageType.Error, Notification.Events.PathError, verboseModeOnly: false);
						return false;
					}
				}
			}
			else
			{
				logger.AddNotification("One of the directory settings is blank or missing. Check the app directory settings.", Notification.MessageType.Information, Notification.Events.PathError, verboseModeOnly: false);
			}
			return true;
		}

		void StartTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				var isCorrectSendParams = true;
				var isCorrectFtpParams = true;
				var iseHubReachable = false;

				//The service is running, make sure it doesn't launch another service thread
				startTimer.Stop();
				startTimer.Dispose();

				#region Check FTP parameters

				var ftpHelper = new FtpHelper();
				isCorrectFtpParams = FtpHelper.CheckFTPSettings(logger);
				ftpHelper.Dispose();

				#endregion

				#region Check connection to eHub

				//Try multiple time before considering the connection to eHub has failed
				while (nbLoginToeHubAttemptsLeft > 0 && !iseHubReachable)
				{
					if (!eAdaptorSampleWebClient.Ping(ApplicationConfig.Instance.EHubAddress, ApplicationConfig.Instance.EHubProductionLogin, ApplicationConfig.Instance.EHubProductionPassword))
					{
						logger.AddNotification("Error : could not connect to eHub using poduction credentials.", Notification.MessageType.Error, Notification.Events.PingError, verboseModeOnly: false);
						nbLoginToeHubAttemptsLeft -= 1;
						break;
					}
					else
					{
						logger.AddNotification("Connection to eHub established.", Notification.MessageType.Information, Notification.Events.ServiceStart, verboseModeOnly: false);
						iseHubReachable = true;
						nbLoginToeHubAttemptsLeft = MaxFailedLoginToeHubAttempts;
					}
				}

				#endregion

				#region Check directories

				isCorrectSendParams = ManageDirectory(Path.Combine(ApplicationConfig.Instance.WorkingDirectory));
				if (!isCorrectSendParams)
				{
					logger.AddNotification("Check app path settings.", Notification.MessageType.Error, Notification.Events.PathError, verboseModeOnly: false);
				}

				#endregion

				#region Run the main working thread

				if (!isCorrectFtpParams || !isCorrectSendParams || !iseHubReachable)
				{
					logger.AddNotification("One or more error prevents the service to start normally.", Notification.MessageType.Error, Notification.Events.GlobalConfigurationError, verboseModeOnly: false);
					OnStop();
				}
				else
				{
					StartLoopTimer();
				}

				#endregion
			}
			catch (CargoWise.eHub.Adapter.eHubAdapterException ex)
			{
				if (ex.Message.Contains("failed ping request"))
				{
					logger.AddNotification("The eHub URL was not reachable. Check either its availability or update configuration 'eHubAddress' value.", Notification.MessageType.Error, Notification.Events.EHubConfigurationError, verboseModeOnly: false);
				}
				else
				{
					logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.EHubConfigurationError, verboseModeOnly: false);
				}
				OnStop();
			}
			catch (Exception ex)
			{
				logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.ServiceStart, verboseModeOnly: false);
				OnStop();
			}
		}

		void StartLoopTimer()
		{
			var timerInterval = 2000;
			_ = int.TryParse(ApplicationConfig.Instance.TimerInterval, out timerInterval);

			loopTimer = new Timer();
			loopTimer.Elapsed += new ElapsedEventHandler(LoopTimer_Tick);
			loopTimer.Start();
			loopTimer.Interval = timerInterval;
			loopTimer.Enabled = true;
			loopTimer.AutoReset = true;
		}

		void LoopTimer_Tick(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (isJobInProgress)
			{
				return;
			}
			else
			{
				ExecuteJob();
			}
		}

		/// <summary>
		/// Main thread
		/// </summary>
		void ExecuteJob()
		{
			if (isJobInProgress)
			{
				return;
			}
			else
			{
				isJobInProgress = true;
			}

			try
			{
				var now = DateTime.Now;

				//Notifies the service is still working
				if ((DateTime.Now - dtInfoServiceWorking).TotalSeconds > nbSecondBetweenInfoServiceWorking)
				{
					var message = isProductionDisabled ? "Production messages will be skipped. - " : string.Empty;
					message += isTestDisabled ? "Test messages will be skipped." : string.Empty;
					logger.AddNotification(message, Notification.MessageType.Information, Notification.Events.ServiceWorkingGetInfo, verboseModeOnly: false);
					dtInfoServiceWorking = now;
				}

				//Receive the messages waiting in the eHub queue
				if (shouldProcesseHubMessages)
				{
					//The following will occur in case eHub is not reachable anymore
					try
					{
						using (var eHubMessagesProcessor = new eHubMessagesProcessor(logger))
						{
							if (!eHubMessagesProcessor.ProcessIncomingCW1Messages())
							{
								//this occurs in case of a login failure to eHub, when eHub is not reachable.
								isJobInProgress = false;
								nbLoginToeHubAttemptsLeft -= 1;
								if (nbLoginToeHubAttemptsLeft == 0)
								{
									OnStop();
									return;
								}
							}
							else
							{
								//ehub was reachable last attempt, let's reset the attempts countdown
								nbLoginToeHubAttemptsLeft = MaxFailedLoginToeHubAttempts;
							}
						}
					}
					catch (Exception ex)
					{
						logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.UnkownError, verboseModeOnly: false);
					}
				}

				//Process the messages waiting on platform FTP
				if (shouldProcessPlatformMessages)
				{
					using (var platformResponsesProcessor = new PlatformResponsesProcessor(logger))
					{
						platformResponsesProcessor.ProcessResponses();
					}
				}
			}
			catch (CargoWise.eHub.Adapter.LoginException)
			{
				logger.AddNotification("Login to eHub failed. Check the eHub login or 'Password' values.", Notification.MessageType.Error, Notification.Events.EHubConfigurationError, verboseModeOnly: false);
			}
			catch (Exception ex)
			{
				logger.AddNotification(ex.Message, Notification.MessageType.Error, Notification.Events.UnkownError, verboseModeOnly: false);
			}
			finally
			{
				isJobInProgress = false;
			}
		}

		static bool isJobInProgress;
		readonly Timer startTimer = new Timer();
		Timer loopTimer;
		DateTime dtInfoServiceWorking;
		readonly int nbSecondBetweenInfoServiceWorking = 30;
		const int MaxFailedLoginToeHubAttempts = 10;
		int nbLoginToeHubAttemptsLeft;
		readonly bool shouldProcesseHubMessages = ApplicationConfig.Instance.RunningMode.Contains("Messages");
		readonly bool shouldProcessPlatformMessages = ApplicationConfig.Instance.RunningMode.Contains("Responses");
		readonly bool isProductionDisabled = ApplicationConfig.Instance.EHubProductionEnabled == "0";
		readonly bool isTestDisabled = ApplicationConfig.Instance.EHubTestEnabled == "0";
		Logger logger = new Logger();
	}
}
