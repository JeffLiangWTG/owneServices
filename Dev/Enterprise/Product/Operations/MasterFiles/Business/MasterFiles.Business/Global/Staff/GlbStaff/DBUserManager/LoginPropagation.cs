using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Common.ErrorManagement;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.ZArchitecture.AlwaysOnHelper;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	static class LoginPropagation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Should be constant as this configures RetryHandler, identificator of connection problem for logging")]
		internal static Task PropagateLogin(ILogger logger, List<DbUserManager.StaffLoginInfo> infos, Action<AdminConnection, DbUserManager.StaffLoginInfo> loginAction)
		{
			var replicas = AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(Db.DatabaseName, useCache: true);
			if (replicas.Length == 0)
			{
				return null;
			}

			using (var newConnection = Db.NewAdminConnection(Db.DatabaseName))
			{
				foreach (var info in infos)
				{
					info.HashedPassword = newConnection.GetSqlLoginHashedPassword(info.LoginName);
					if (string.IsNullOrWhiteSpace(info.Sid))
					{
						info.Sid = newConnection.GetSqlLoginSid(info.LoginName);
					}
				}
			}

			var replicationResult = new ReplicationResult();

			var task = Task.Run(() =>
			{
				Parallel.ForEach(replicas, (replicaServerName) =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						using (var newAdminConnection = Db.NewAdminConnection(replicaServerName, Db.SqlMasterDb))
						{
							foreach (var info in infos)
							{
								try
								{
									// 4 attempts in total will be made with 30s, 1m and 2m intervals in between accordingly, subject to adjustment in future
									var retryHandler = new RetryHandler("30s;1m;2m");
#if DEBUG
									if (Globals.IsTest)
									{
										retryHandler = new RetryHandler(""); // For test we'll just try once
									}
#endif
									retryHandler.Invoke(() => loginAction(newAdminConnection, info));
								}
								catch (EmailHasNoFromAddressException ex)
								{
									logger?.Log(LogType.Error, "Couldn't send email: " + ex.Message); // If Logger is null, this exception has to be swallowed.
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									replicationResult.NewNotification(Tuple.Create(info.LoginName, replicaServerName, ex.ToString()));
								}
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						replicationResult.NewNotification(Tuple.Create("Connection problem", replicaServerName, ex.ToString()));
					}
				});

				using (Db.DisposableActionForDbConnection())
				{
					if (replicationResult.NotificationsList.Count != 0)
					{
						replicationResult.SendLoginNotPropagatedNotification();
					}
				}
			});

			return task;
		}
	}
}
