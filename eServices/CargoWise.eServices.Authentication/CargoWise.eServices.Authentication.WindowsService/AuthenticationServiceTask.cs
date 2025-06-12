using System;
using System.Threading;
using Common.Logging;

namespace CargoWise.eServices.Authentication.WindowsService
{
	public class AuthenticationServiceTask : IDisposable
	{
		public AuthenticationServiceTask(AuthenticationSettings authSettings)
		{
			this.authSettings = authSettings;
			timer = new Timer(new TimerCallback(ScheduledCallback));
		}

		void ScheduleService()
		{
			timer.Change(TimeSpan.FromSeconds(authSettings.INTERVAL), Timeout.InfiniteTimeSpan);
		}

		private void ScheduledCallback(Object o)
		{
			Do();
			ScheduleService();
		}

		public virtual void Start()
		{
			timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
		}

		public virtual void Do()
		{
			CopyFromediProdToAuthanticationDB();
		}

		private void CopyFromediProdToAuthanticationDB()
		{
			Logger.Info("Task starts...");
			DatabaseHelper.TransferData(authSettings.EDIPROD_DB_CONNECTION_STRING, authSettings.AUTH_DB_CONNECTION_STRING);
		}

		public void Dispose()
		{
			timer.Dispose();
		}

		#region DatabaseHelper

		public virtual IDatabaseHelper DatabaseHelper
		{
			get { return databaseHelper ?? (databaseHelper = new Databasehelper()); }
		}

		IDatabaseHelper databaseHelper;

		#endregion

		readonly Timer timer;
		AuthenticationSettings authSettings;
		static readonly ILog Logger = LogManager.GetLogger(typeof(AuthenticationServiceTask));
	}
}
