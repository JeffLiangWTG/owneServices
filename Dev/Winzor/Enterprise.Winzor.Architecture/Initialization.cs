using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Common.MemoryManagement;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Winzor.Telemetry;
using CargoWiseNext.Infrastructure.Installations;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.Startup;
using Enterprise.Startup.Tasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Microsoft.Extensions.Logging;
using WinzorFramework;

namespace Enterprise.Winzor.Architecture;

public static partial class Initialization
{
#if DEBUG
	public static void ConfigureCargoWise(WinzorDispatcher dispatcher, IWinzorCargoWiseLoginHandler loginHandler, UserMonitorRegistry userMonitorRegistry)
	{
		var applicationArguments = new ApplicationArguments(Array.Empty<string>());
		CommandLineArguments.UsedToLaunchApplication = applicationArguments;
		StartConfigureCargoWise(dispatcher, applicationArguments, loginHandler, userMonitorRegistry, null);
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
		initializationTask.GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
	}
#endif

	public static void StartConfigureCargoWise(
		WinzorDispatcher dispatcher,
		ApplicationArguments arguments,
		IWinzorCargoWiseLoginHandler loginHandler,
		UserMonitorRegistry userMonitorRegistry,
		Guid? appServerCorrelationId,
		ActivitySource activitySource = null,
		ILogger logger = null)
	{
		using var activity = activitySource?.StartActivity($"{nameof(StartConfigureCargoWise)}");
		if (Initialized)
		{
			return;
		}

		Initialized = true;

		FillDatabaseInfoIfNeeded(arguments);
		if (!DatIsTesting)
		{
			userMonitorRegistry.RefreshMonitorConfiguration(arguments.ServerName, arguments.DatabaseName);
		}
		initializationTask = dispatcher.InvokeAsync(activitySource.WithTracing(() =>
		{
			SplashFormInstance = new SplashForm();

			using (dispatcher.WithContext(initializationWinzorDispatcherContext = new InitializationWinzorDispatcherContext()))
			{
				Globals.IsWinzor = true;
				Globals.ClientIdentifier = System.Environment.GetEnvironmentVariable("CargoWiseOptions:ClientIdentifier");
				ExceptionReporter.UIHooks = new ExceptionReporterUIHooks();
				ApplicationDispatcher.Current = SynchronizationContext.Current;
				dispatcher.RegisterDisposeAction(() => ApplicationDispatcher.Current = null);
				Application.ThreadExit += Application_ThreadExit;
				NotificationHandler.Instance = new ZGUINotificationHandler();

				var tasks = new object[] {
					new ProductBrandingCommandLineDeterminerTask(UpdateBranding),
					() => { exceptionHandler = new EnterStartupErrorHandler(); },
					new StartupInitEnableMemoryManager(),
					() => Initialiser.InitialiseWinForms(appServerCorrelationId),
					new SetupDbConnection(),
					new ProductBrandingRegistryDeterminerTask(UpdateBranding),
					new StartupCheckClientDll(),
					new WinzorDbUpgraderDirector(arguments),
					new RemoveOldUpgradePackagesTask(),
					new TempFileCleanupTask(),
					new ResourceStringsUpdaterTask(),
					new Action(ConfigureUrlHandlers),
					new WinzorCargoWiseLoginTask(loginHandler),
					() => { exceptionHandler = new EndStartupErrorHandler(); },
					new WinzorStartupOpenMainFormTask(),
					new ServiceManagerCommunicationMonitoringTask(),
					new DbConnectionCleanerTask()
				};
				for (int i = 0; i < tasks.Length; i++)
				{
					if (tasks[i] is IApplicationStartupTask task)
					{
						ExecuteStartupTask(arguments, task, (i + 1) * 100 / tasks.Length, activitySource, logger);
					}
					else if (tasks[i] is Action action)
					{
						action();
					}
				}

				mainFormInstanceInitializedTaskCompletionSource.SetResult();
			}

			initializationWinzorDispatcherContext = null;
		}, $"{nameof(Initialization)}.InitializationTask"));
	}

	internal static void ExecuteStartupTask(CommandLineArguments arguments, IApplicationStartupTask applicationStartupTask, int progressAfterCompleted = 100, ActivitySource activitySource = null, ILogger logger = null)
	{
		using var activity = activitySource?.StartActivity($"{nameof(ExecuteStartupTask)}({applicationStartupTask.GetType().Name})");

		if (!applicationStartupTask.ShouldExecute(arguments))
		{
			return;
		}

		try
		{
			SplashFormInstance.UpdateText(applicationStartupTask.TaskDescription);

			if (!applicationStartupTask.Execute(arguments))
			{
				TaskFailure(applicationStartupTask);
			}

			SplashFormInstance.UpdateProgress(progressAfterCompleted);
		}
		catch (Exception ex) when (ex is not TaskCanceledException)
		{
			logger?.LogWarning(ex, (NoResString)"Exception is thrown in ExecuteStartupTask. Start to handle it.");

			try
			{
				if (exceptionHandler == null || !exceptionHandler.HandleException(ex, arguments))
				{
					throw;
				}
			}
			catch
			{
				ExceptionReporter.Instance.HandleUnhandledException(ex);
				TaskFailure(applicationStartupTask, ex);
			}
		}

		return;

		static void TaskFailure(IApplicationStartupTask task, Exception exception = null)
		{
			var failMessage = $"Failed: {task.TaskDescription}";
			SplashFormInstance.UpdateText(failMessage);
			Globals.Message.ShowError(FormattableString.Invariant($"{BrandingFactory.Instance.ProductName} failed to start."));
			var tce = new TaskCanceledException(failMessage, exception);
			mainFormInstanceInitializedTaskCompletionSource.SetException(tce);
			throw tce;
		}
	}

	static void FillDatabaseInfoIfNeeded(ApplicationArguments arguments)
	{
		var datIsTesting = DatIsTesting;

#if DEBUG // LocalDBConnection is only build on DEBUG mode
		if (string.IsNullOrEmpty(arguments.ServerName))
		{
			Dat.Implementation.LocalDBConnection.DatIsTesting = datIsTesting;
			arguments.ServerName = Dat.Implementation.LocalDBConnection.GetServerName(); // pick up sql server instance in the same way with CW1 Enterprise.Dat.Adapter
		}
#endif

		if (string.IsNullOrEmpty(arguments.DatabaseName))
		{
			arguments.DatabaseName = datIsTesting ? "OdysseyDat" : (NoResString)"Odyssey";
		}
	}

	static void UpdateBranding()
	{
	}

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks")]
	public static async Task OnInitialRenderAsync(IWinzorDispatcherContext context, ActivitySource activitySource)
	{
		using var activity = activitySource?.StartActivity($"{nameof(Initialization)}.{nameof(OnInitialRenderAsync)}");
		if (initializationWinzorDispatcherContext != null)
		{
			await initializationWinzorDispatcherContext.SetWorkingDispatcherContextAsync(context);
			await initializationTask;
		}
	}

	static void ConfigureUrlHandlers()
	{
		EnterpriseUrlHandlerService.RegisterUrlHandler(ReportUrlHandler.Instance);
		EnterpriseUrlHandlerService.RegisterUrlHandler(ShowReportUrlHandler.Instance);
		EnterpriseUrlHandlerService.RegisterUrlHandler(CustomizeDocumentsUrlHandler.Instance);
	}

	static void UnconfigureUrlHandlers()
	{
		EnterpriseUrlHandlerService.UnregisterUrlHandler(ReportUrlHandler.Instance);
		EnterpriseUrlHandlerService.UnregisterUrlHandler(ShowReportUrlHandler.Instance);
		EnterpriseUrlHandlerService.UnregisterUrlHandler(CustomizeDocumentsUrlHandler.Instance);
	}

	public static Form MainFormInstance => StartupOpenMainFormTask.MainFormInstance;
	public static SplashForm SplashFormInstance { get; private set; }

	public static void ResetSplashFormInstanceForTesting()
	{
		SplashFormInstance = new SplashForm();
	}

	static void Application_ThreadExit(object sender, EventArgs e)
	{
		try
		{
			if (ApplicationDispatcher.MainThread == Thread.CurrentThread)
			{
				using (Db.DisposableActionForDbConnection())
				{
					Env.LoginController.Logout();
				}
			}
		}
		catch (Exception)
		{
			// By this stage the exception handler has been disconnected, so all exceptions should be eaten
			// An example where this happens is during system upgrade if the user attempts to close the app
		}
	}

	static Task initializationTask;

	static InitializationWinzorDispatcherContext initializationWinzorDispatcherContext;

	static TaskCompletionSource mainFormInstanceInitializedTaskCompletionSource = new TaskCompletionSource();
	public static Task MainFormInstanceInitializedTask => mainFormInstanceInitializedTaskCompletionSource.Task;

	[ThreadStatic]
	internal static IApplicationStartupTaskExceptionHandler exceptionHandler;

	public static bool Initialized { get; private set; }

	public static bool DatIsTesting => bool.TryParse(System.Environment.GetEnvironmentVariable("DAT_IS_TESTING"), out var datIsTesting) && datIsTesting;

	internal static IWinzorDispatcherContext Context => initializationWinzorDispatcherContext;

	public static void UnconfigureCargoWise()
	{
		Initialized = false;
		MemoryManager.Disable();
		UnconfigureUrlHandlers();
		StartupOpenMainFormTask.MainFormInstance?.Dispose();
		StartupOpenMainFormTask.MainFormInstance = null;
		mainFormInstanceInitializedTaskCompletionSource = new TaskCompletionSource();
		exceptionHandler = null;
	}

	internal class EnterStartupErrorHandler : IApplicationStartupTaskExceptionHandler
	{
		public bool HandleException(Exception e, CommandLineArguments arguments)
		{
			var handled = false;

			if (e is DatabaseUpgradeException)
			{
				DbEnv.Instance.ConnectionGuiPlugin.HandleDatabaseUpgradeException((DatabaseUpgradeException)e);
				handled = true;
			}
			else if (e is SqlException sqlException)
			{
				switch (new DbErrorMatch(sqlException).ExceptionType)
				{
					case DbErrorType.ModuleBeingExecutedIsNotTrusted:
						using (var conn = Db.NewAdminConnection())
						{
							DataUtils.SetTrustworthyOn(conn, Db.DatabaseName);
						}
						handled = true;
						break;

					case DbErrorType.CannotExecuteAsDatabasePrincipal:
						using (var conn = Db.NewAdminConnection())
						{
							DataUtils.AlterDbAuthorisation(conn, Db.DatabaseName, sqlException);
						}
						handled = true;
						break;

					default:
						break;
				}
			}

			return handled;
		}
	}

	class EndStartupErrorHandler : IApplicationStartupTaskExceptionHandler
	{
		public bool HandleException(Exception e, CommandLineArguments arguments)
		{
			return ExceptionReporter.Instance.TryHandleWithoutReporting(e);
		}
	}
}
